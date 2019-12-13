#pragma warning disable 0649
using MarchingSquares.Grid;
using MarchingSquares.Stencils;
using UnityEngine;

namespace MarchingSquares
{
    public class VoxelMap : MonoBehaviour
    {
        [SerializeField] private float size = 2f;
        [SerializeField] private int chunkResolution = 2;
        [SerializeField] private int voxelResolution = 8;
        
        [SerializeField] private float maxFeatureAngle = 135f;
        [SerializeField] private float maxParallelAngle = 8f;
        
        [SerializeField] private VoxelGrid voxelGridPrefab;
        private VoxelGrid[] chunks;

        [SerializeField] private Transform[] stencilVisualizations;
        [SerializeField] private bool snapToGrid;
        
        private static readonly VoxelStencil[] stencils = {new VoxelStencil(), new VoxelStencilCircle()};
        private static readonly string[] fillTypeNames = {"X", "A", "B", "C", "D"};
        private static readonly string[] radiusNames = {"0", "1", "2", "3", "4", "5"};
        private static readonly string[] stencilNames = {"Square", "Circle"};
        
        private float chunkSize, voxelSize, halfSize;
        private int fillTypeIndex = 1, radiusIndex, stencilIndex;
        
        private Camera cam;
        
        private void Awake()
        {
            chunkSize = size / chunkResolution;
            voxelSize = chunkSize / voxelResolution;
            halfSize = size * 0.5f;

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
            chunk.Initialize(voxelResolution, chunkSize, maxFeatureAngle, maxParallelAngle);
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
            Transform visualization = stencilVisualizations[stencilIndex];
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo) &&
                hitInfo.collider.gameObject == gameObject)
            {
                Vector2 center = transform.InverseTransformPoint(hitInfo.point);
                center.x += halfSize;
                center.y += halfSize;
                
                if (snapToGrid)
                {
                    center.x = ((int) (center.x / voxelSize) + 0.5f) * voxelSize;
                    center.y = ((int) (center.y / voxelSize) + 0.5f) * voxelSize;
                }

                if (Input.GetMouseButton(0))
                {
                    EditVoxels(center);
                }

                center.x -= halfSize;
                center.y -= halfSize;
                visualization.localPosition = center;
                visualization.localScale = Vector3.one * ((radiusIndex + 0.5f) * voxelSize * 2f);
                visualization.gameObject.SetActive(true);
            }
            else
            {
                visualization.gameObject.SetActive(false);
            }
        }

        private void EditVoxels(Vector2 center)
        {
            VoxelStencil activeStencil = stencils[stencilIndex];
            activeStencil.Initialize(fillTypeIndex, (radiusIndex + 0.5f) * voxelSize);
            activeStencil.SetCenter(center.x, center.y);

            int xStart = (int) ((activeStencil.XStart - voxelSize) / chunkSize);
            if (xStart < 0) xStart = 0;

            int xEnd = (int) ((activeStencil.XEnd + voxelSize) / chunkSize);
            if (xEnd >= chunkResolution) xEnd = chunkResolution - 1;

            int yStart = (int) ((activeStencil.YStart - voxelSize) / chunkSize);
            if (yStart < 0) yStart = 0;

            int yEnd = (int) ((activeStencil.YEnd + voxelSize) / chunkSize);
            if (yEnd >= chunkResolution) yEnd = chunkResolution - 1;

            for (int y = yEnd; y >= yStart; y--)
            {
                int i = y * chunkResolution + xEnd;
                for (int x = xEnd; x >= xStart; x--, i--)
                {
                    activeStencil.SetCenter(center.x - x * chunkSize, center.y - y * chunkSize);
                    chunks[i].Apply(activeStencil);
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
            GUILayout.Label("Fill Type");
            fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 5);
            GUILayout.Label("Radius");
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
            GUILayout.Label("Stencil");
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
            GUILayout.EndArea();
        }
    }
}
