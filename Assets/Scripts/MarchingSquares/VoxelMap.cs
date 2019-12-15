#pragma warning disable 0649
using System;
using System.Linq;
using MarchingSquares.Stencils;
using MarchingSquares.Texturing;
using Noise;
using UnityEngine;

namespace MarchingSquares
{
    public class VoxelMap : MonoBehaviour
    {
        [SerializeField] private int chunkResolution = 2;
        [SerializeField] private int voxelResolution = 8;
        [SerializeField] private float worldHeight = 10f;

        [SerializeField] private TextureData textureData;
        [SerializeField] private PerlinSettings heightSettings;
        [SerializeField] private PerlinSettings lodeSettings;
        
        [SerializeField] private VoxelGrid voxelGridPrefab;

        [SerializeField] private Font textFont;

        public Action OnVoxelEdit = delegate { };
        
        private float chunkSize = 1f, voxelSize, halfSize;
        private VoxelGrid[] chunks;
        
        private static readonly string[] fillTypeNames = {"Mine", "Build"};
        private static readonly string[] radiusNames = {"1", "2", "3", "4", "5", "6", "7", "8"};
        private static readonly string[] stencilNames = {"Square", "Circle"};
        private readonly VoxelStencil[] stencils = {new VoxelStencil(), new VoxelStencilCircle()};
        private int fillTypeIndex, radiusIndex, stencilIndex;
        
        private Camera cam;
        
        private void Awake()
        {
            halfSize = chunkResolution * 0.5f;
            voxelSize = 1f / voxelResolution;
            
            chunks = new VoxelGrid[chunkResolution * chunkResolution];
            for (int i = 0, y = 0; y < chunkResolution; y++)
            for (int x = 0; x < chunkResolution; x++, i++)
            {
                CreateChunk(i, x, y);
            }

            foreach (VoxelGrid chunk in chunks)
            {
                chunk.Refresh();
            }
            
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(chunkResolution, chunkResolution);
            
            cam = Camera.main;
        }

        private void CreateChunk(int i, int x, int y)
        {
            float[] heightMap = Perlin.GenerateNoiseMap2D(voxelResolution, heightSettings, x * voxelResolution);
            float[,] lodeMap = Perlin.GenerateNoiseMap3D(voxelResolution, lodeSettings, new Vector2(x * voxelResolution, y * voxelResolution));
            
            float offset = y - worldHeight * voxelResolution;
            
            VoxelGrid chunk = Instantiate(voxelGridPrefab, transform, true);
            chunk.Initialize(voxelResolution, chunkSize, offset, heightMap, lodeMap, textureData.GenerateColorMap(heightMap, lodeMap, offset, voxelResolution * chunkResolution));
            chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
            
            chunks[i] = chunk;
            if (x > 0)
            {
                chunks[i - 1].neighborX = chunk;
            }
            if (y > 0)
            {
                chunks[i - chunkResolution].neighborY = chunk;
                
                if (x > 0)
                {
                    chunks[i - chunkResolution - 1].neighborT = chunk;
                }
            }

            OnVoxelEdit();
        }

        public void EditVoxels(Vector3 point)
        {
            if (fillTypeIndex == 1)
            {
                if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo) && 
                    hitInfo.collider.gameObject == gameObject)
                {
                    point = transform.InverseTransformPoint(hitInfo.point);
                }
            }
            
            int centerX = (int) ((point.x + halfSize) / voxelSize);
            int centerY = (int) ((point.y + halfSize) / voxelSize);

            int xStart = (centerX - radiusIndex - 1) / voxelResolution;
            if (xStart < 0) xStart = 0;

            int xEnd = (centerX + radiusIndex) / voxelResolution;
            if (xEnd >= chunkResolution) xEnd = chunkResolution - 1;

            int yStart = (centerY - radiusIndex - 1) / voxelResolution;
            if (yStart < 0) yStart = 0;

            int yEnd = (centerY + radiusIndex) / voxelResolution;
            if (yEnd >= chunkResolution) yEnd = chunkResolution - 1;

            VoxelStencil activeStencil = stencils[stencilIndex];
            activeStencil.Initialize(fillTypeIndex != 0, radiusIndex + 1);

            int voxelYOffset = yEnd * voxelResolution;
            for (int y = yEnd; y >= yStart; y--)
            {
                int i = y * chunkResolution + xEnd;
                int voxelXOffset = xEnd * voxelResolution;
                for (int x = xEnd; x >= xStart; x--, i--)
                {
                    activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
                    chunks[i].Apply(activeStencil);
                    voxelXOffset -= voxelResolution;
                }

                voxelYOffset -= voxelResolution;
            }

            OnVoxelEdit();
        }

        public int MaxRadius { get; set; } = 2;
        public int MaxStencils { get; set; } = 1;
        
        private void OnGUI()
        {
            GUI.skin.font = textFont;
            const int labelSpace = -7;
            const int itemSpace = -5;
            
            GUILayout.BeginArea(new Rect(4f, 4f, 150f, 450f));
            GUILayout.Label("Fill Type");
            GUILayout.Space(labelSpace);
            fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
            GUILayout.Space(itemSpace);
            GUILayout.Label("Mine Radius");
            GUILayout.Space(labelSpace);
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames.Take(MaxRadius).ToArray(), MaxRadius);
            GUILayout.Space(itemSpace);
            GUILayout.Label("Mine Shape");
            GUILayout.Space(labelSpace);
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames.Take(MaxStencils).ToArray(), MaxStencils);
            GUILayout.EndArea();
        }
    }
}