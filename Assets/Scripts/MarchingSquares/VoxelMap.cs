#pragma warning disable 0649
using MarchingSquares.Stencils;
using UnityEngine;

namespace MarchingSquares
{
    public class VoxelMap : MonoBehaviour
    {
        [SerializeField] private float size = 2f;
        [SerializeField] private int voxelResolution = 8;
        [SerializeField] private int chunkResolution = 2;
        
        [SerializeField] private VoxelGrid voxelGridPrefab;
        
        private float chunkSize, voxelSize,halfSize;
        private VoxelGrid[] chunks;
        
        private static readonly string[] fillTypeNames = {"Filled", "Empty"};
        private static readonly string[] radiusNames = {"0", "1", "2", "3", "4", "5"};
        private static readonly string[] stencilNames = {"Square", "Circle"};
        private readonly VoxelStencil[] stencils = {new VoxelStencil(), new VoxelStencilCircle()};
        private int fillTypeIndex, radiusIndex, stencilIndex;
        
        private Camera cam;
        
        private void Awake()
        {
            halfSize = size * 0.5f;
            chunkSize = size / chunkResolution;
            voxelSize = chunkSize / voxelResolution;
            
            chunks = new VoxelGrid[chunkResolution * chunkResolution];
            for (int i = 0, y = 0; y < chunkResolution; y++)
            for (int x = 0; x < chunkResolution; x++, i++)
            {
                CreateChunk(i, x, y);
            }
            
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(size, size);
            
            cam = Camera.main;
        }

        private void CreateChunk(int i, int x, int y)
        {
            VoxelGrid chunk = Instantiate(voxelGridPrefab, transform, true);
            chunk.Initialize(voxelResolution, chunkSize);
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

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo))
                {
                    if (hitInfo.collider.gameObject == gameObject)
                    {
                        EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                    }
                }
            }
        }

        private void EditVoxels(Vector3 point)
        {
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
            activeStencil.Initialize(fillTypeIndex == 0, radiusIndex);

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
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
            GUILayout.Label("Stencil");
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
            GUILayout.EndArea();
        }
    }
}