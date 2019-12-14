#pragma warning disable 0649
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
        [SerializeField, Range(0f, 2f)] private float worldHeight = 0.75f;

        [SerializeField] private TextureData textureData;
        [SerializeField] private PerlinSettings heightSettings;
        [SerializeField] private PerlinSettings lodeSettings;
        
        [SerializeField] private VoxelGrid voxelGridPrefab;
        
        private float chunkSize = 1f, voxelSize, halfSize;
        private VoxelGrid[] chunks;
        
        private static readonly string[] fillTypeNames = {"Empty", "Filled"};
        private static readonly string[] radiusNames = {"1", "2", "3", "4", "5"};
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
            chunk.Initialize(voxelResolution, chunkSize, offset, heightMap, textureData.GenerateColorMap(heightMap, lodeMap, offset, voxelResolution * chunkResolution));
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
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
            GUILayout.Label("Fill Type");
            fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
            GUILayout.Label("Radius");
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 5);
            GUILayout.Label("Stencil");
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
            GUILayout.EndArea();
        }
    }
}