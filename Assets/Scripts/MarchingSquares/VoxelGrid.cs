#pragma warning disable 0649
using System.Collections.Generic;
using System.Linq;
using MarchingSquares.Stencils;
using Noise;
using UnityEngine;

namespace MarchingSquares
{
    [SelectionBase]
    public class VoxelGrid : MonoBehaviour
    {
        private int resolution;
        private float voxelSize, gridSize;
        private Voxel[] voxels;

        private Mesh mesh;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();

        private Voxel dummyX = new Voxel(), dummyY = new Voxel(), dummyT = new Voxel();
        [HideInInspector] public VoxelGrid neighborX, neighborY, neighborT;

        private int[] rowCacheMax, rowCacheMin;
        private int edgeCacheMin, edgeCacheMax;

        public void Initialize(int resolution, float size, float xOffset, float[] noiseMap)
        {
            this.resolution = resolution;
            gridSize = size;
            voxelSize = size / resolution;

            voxels = new Voxel[resolution * resolution];
            for (int i = 0, y = 0; y < resolution; y++)
            {
                float height = xOffset + (float) y / resolution;
                for (int x = 0; x < resolution; x++, i++)
                {
                    bool solid = noiseMap[x] > height;
                    voxels[i] = new Voxel(solid, x, y, voxelSize);
                }
            }

            mesh = new Mesh {name = "Voxel Grid Mesh"};
            GetComponent<MeshFilter>().mesh = mesh;

            int cacheSize = resolution * 2 + 1;
            rowCacheMax = new int[cacheSize];
            rowCacheMin = new int[cacheSize];
            
            Refresh();
        }

        public void Refresh()
        {
            Triangulate();
            GetComponent<EdgeCollider2D>().points = vertices.Select(v => new Vector2(v.x, v.y)).ToArray();
        }

        private void Triangulate()
        {
            vertices.Clear();
            triangles.Clear();
            mesh.Clear();

            FillFirstRowCache();
            TriangulateCellRows();
            
            if (neighborY != null)
            {
                TriangulateGapRow();
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
        }

        private void FillFirstRowCache()
        {
            CacheFirstCorner(voxels[0]);
            
            int i = 0;
            for (; i < resolution - 1; i++)
            {
                CacheNextEdgeAndCorner(i * 2, voxels[i], voxels[i + 1]);
            }

            if (neighborX != null)
            {
                dummyX.BecomeXDummyOf(neighborX.voxels[0], gridSize);
                CacheNextEdgeAndCorner(i * 2, voxels[i], dummyX);
            }
        }

        private void CacheFirstCorner(Voxel voxel)
        {
            if (voxel.state)
            {
                rowCacheMax[0] = vertices.Count;
                vertices.Add(voxel.position);
            }
        }

        private void CacheNextEdgeAndCorner(int i, Voxel xMin, Voxel xMax)
        {
            if (xMin.state != xMax.state)
            {
                rowCacheMax[i + 1] = vertices.Count;
                vertices.Add(new Vector3(xMin.xEdge, xMin.position.y));
            }

            if (xMax.state)
            {
                rowCacheMax[i + 2] = vertices.Count;
                vertices.Add(xMax.position);
            }
        }

        private void CacheNextMiddleEdge(Voxel yMin, Voxel yMax)
        {
            edgeCacheMin = edgeCacheMax;
            if (yMin.state != yMax.state)
            {
                edgeCacheMax = vertices.Count;
                vertices.Add(new Vector3(yMin.position.x, yMin.yEdge));
            }
        }

        private void TriangulateCellRows()
        {
            int cells = resolution - 1;
            for (int i = 0, y = 0; y < cells; y++, i++)
            {
                SwapRowCaches();
                CacheFirstCorner(voxels[i + resolution]);
                CacheNextMiddleEdge(voxels[i], voxels[i + resolution]);

                for (int x = 0; x < cells; x++, i++)
                {
                    Voxel a = voxels[i];
                    Voxel b = voxels[i + 1];
                    Voxel c = voxels[i + resolution];
                    Voxel d = voxels[i + resolution + 1];
                    
                    int cacheIndex = x * 2;
                    CacheNextEdgeAndCorner(cacheIndex, c, d);
                    CacheNextMiddleEdge(b, d);
                    TriangulateCell(cacheIndex, a, b, c, d);
                }

                if (neighborX != null)
                {
                    TriangulateGapCell(i);
                }
            }
        }

        private void SwapRowCaches()
        {
            int[] rowSwap = rowCacheMin;
            rowCacheMin = rowCacheMax;
            rowCacheMax = rowSwap;
        }

        private void TriangulateGapCell(int i)
        {
            Voxel dummySwap = dummyT;
            dummySwap.BecomeXDummyOf(neighborX.voxels[i + 1], gridSize);
            dummyT = dummyX;
            dummyX = dummySwap;
            
            int cacheIndex = (resolution - 1) * 2;
            CacheNextEdgeAndCorner(cacheIndex, voxels[i + resolution], dummyX);
            CacheNextMiddleEdge(dummyT, dummyX);
            TriangulateCell(cacheIndex, voxels[i], dummyT, voxels[i + resolution], dummyX);
        }

        private void TriangulateGapRow()
        {
            dummyY.BecomeYDummyOf(neighborY.voxels[0], gridSize);
            
            int cells = resolution - 1;
            int offset = cells * resolution;
            SwapRowCaches();
            CacheFirstCorner(dummyY);
            CacheNextMiddleEdge(voxels[cells * resolution], dummyY);

            for (int x = 0; x < cells; x++)
            {
                Voxel dummySwap = dummyT;
                dummySwap.BecomeYDummyOf(neighborY.voxels[x + 1], gridSize);
                dummyT = dummyY;
                dummyY = dummySwap;
                
                int cacheIndex = x * 2;
                CacheNextEdgeAndCorner(cacheIndex, dummyT, dummyY);
                CacheNextMiddleEdge(voxels[x + offset + 1], dummyY);
                TriangulateCell(cacheIndex, voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY);
            }

            if (neighborX != null)
            {
                dummyT.BecomeXYDummyOf(neighborT.voxels[0], gridSize);
                
                int cacheIndex = cells * 2;
                CacheNextEdgeAndCorner(cacheIndex, dummyY, dummyT);
                CacheNextMiddleEdge(dummyX, dummyT);
                TriangulateCell(cacheIndex, voxels[voxels.Length - 1], dummyX, dummyY, dummyT);
            }
        }

        private void TriangulateCell(int i, Voxel a, Voxel b, Voxel c, Voxel d)
        {
            int cellType = 0;
            if (a.state)
                cellType |= 1;
            if (b.state)
                cellType |= 2;
            if (c.state)
                cellType |= 4;
            if (d.state)
                cellType |= 8;
            
            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(rowCacheMin[i], edgeCacheMin, rowCacheMin[i + 1]);
                    break;
                case 2:
                    AddTriangle(rowCacheMin[i + 2], rowCacheMin[i + 1], edgeCacheMax);
                    break;
                case 3:
                    AddQuad(rowCacheMin[i], edgeCacheMin, edgeCacheMax, rowCacheMin[i + 2]);
                    break;
                case 4:
                    AddTriangle(rowCacheMax[i], rowCacheMax[i + 1], edgeCacheMin);
                    break;
                case 5:
                    AddQuad(rowCacheMin[i], rowCacheMax[i], rowCacheMax[i + 1], rowCacheMin[i + 1]);
                    break;
                case 6:
                    AddTriangle(rowCacheMin[i + 2], rowCacheMin[i + 1], edgeCacheMax);
                    AddTriangle(rowCacheMax[i], rowCacheMax[i + 1], edgeCacheMin);
                    break;
                case 7:
                    AddPentagram(rowCacheMin[i], rowCacheMax[i], rowCacheMax[i + 1], edgeCacheMax, rowCacheMin[i + 2]);
                    break;
                case 8:
                    AddTriangle(rowCacheMax[i + 2], edgeCacheMax, rowCacheMax[i + 1]);
                    break;
                case 9:
                    AddTriangle(rowCacheMin[i], edgeCacheMin, rowCacheMin[i + 1]);
                    AddTriangle(rowCacheMax[i + 2], edgeCacheMax, rowCacheMax[i + 1]);
                    break;
                case 10:
                    AddQuad(rowCacheMin[i + 1], rowCacheMax[i + 1], rowCacheMax[i + 2], rowCacheMin[i + 2]);
                    break;
                case 11:
                    AddPentagram(rowCacheMin[i + 2], rowCacheMin[i], edgeCacheMin, rowCacheMax[i + 1],
                        rowCacheMax[i + 2]);
                    break;
                case 12:
                    AddQuad(edgeCacheMin, rowCacheMax[i], rowCacheMax[i + 2], edgeCacheMax);
                    break;
                case 13:
                    AddPentagram(rowCacheMax[i], rowCacheMax[i + 2], edgeCacheMax, rowCacheMin[i + 1], rowCacheMin[i]);
                    break;
                case 14:
                    AddPentagram(rowCacheMax[i + 2], rowCacheMin[i + 2], rowCacheMin[i + 1], edgeCacheMin,
                        rowCacheMax[i]);
                    break;
                case 15:
                    AddQuad(rowCacheMin[i], rowCacheMax[i], rowCacheMax[i + 2], rowCacheMin[i + 2]);
                    break;
            }
        }

        private void AddTriangle(int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private void AddQuad(int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private void AddPentagram(int a, int b, int c, int d, int e)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
            
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(e);
        }

        public void Apply(VoxelStencil stencil)
        {
            int xStart = stencil.XStart;
            if (xStart < 0) xStart = 0;

            int xEnd = stencil.XEnd;
            if (xEnd >= resolution) xEnd = resolution - 1;

            int yStart = stencil.YStart;
            if (yStart < 0) yStart = 0;

            int yEnd = stencil.YEnd;
            if (yEnd >= resolution) yEnd = resolution - 1;

            for (int y = yStart; y <= yEnd; y++)
            {
                int i = y * resolution + xStart;
                for (int x = xStart; x <= xEnd; x++, i++)
                {
                    voxels[i].state = stencil.Apply(x, y, voxels[i].state);
                }
            }

            Refresh();
        }
    }
}
