#pragma warning disable 0649
using MarchingSquares.Stencils;
using MarchingSquares.Voxels;
using UnityEngine;

namespace MarchingSquares.Grid
{
    [SelectionBase]
    public class VoxelGrid : MonoBehaviour
    {
        [SerializeField] private int resolution = 8;
        
        [SerializeField] private GameObject voxelPrefab;
        [SerializeField] private VoxelGridSurface surfacePrefab;
        [SerializeField] private VoxelGridWall wallPrefab;
        
        private readonly VoxelCell cell = new VoxelCell();
        
        private float voxelSize, gridSize;
        private Voxel[] voxels;
        
        private Material[] voxelMaterials;
        public VoxelMaterials[] materials;
        private VoxelRenderer[] renderers;
        
        private Voxel dummyX = new Voxel(), dummyY = new Voxel(), dummyT = new Voxel();
        public VoxelGrid neighborX, neighborY, neighborT;

        public void Initialize(int resolution, float size, float maxFeatureAngle, float maxParallelAngle)
        {
            this.resolution = resolution;
            gridSize = size;
            voxelSize = size / resolution;
            
            cell.sharpFeatureLimit = Mathf.Cos(maxFeatureAngle * Mathf.Deg2Rad);
            cell.parallelLimit = Mathf.Cos(maxParallelAngle * Mathf.Deg2Rad);
            
            voxels = new Voxel[resolution * resolution];
            voxelMaterials = new Material[voxels.Length];

            for (int i = 0, y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y);
            }

            CreateRenderers();
            Refresh();
        }

        private void CreateRenderers()
        {
            renderers = new VoxelRenderer[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                VoxelGridSurface surface = Instantiate(surfacePrefab, transform, true);
                surface.transform.localPosition = Vector3.zero;
                surface.Initialize(resolution, materials[i].surfaceMaterial);

                VoxelGridWall wall = Instantiate(wallPrefab, transform, true);
                wall.transform.localPosition = Vector3.zero;
                wall.Initialize(resolution, materials[i].wallMaterial);

                renderers[i + 1] = new VoxelRenderer(surface, wall);
            }
        }

        private void CreateVoxel(int i, int x, int y)
        {
            GameObject o = Instantiate(voxelPrefab, transform, true);
            o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, -0.01f);
            o.transform.localScale = 0.1f * voxelSize * Vector3.one;
            voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
            voxels[i] = new Voxel(x, y, voxelSize);
        }

        private void Refresh()
        {
            SetVoxelColors();
            Triangulate();
        }

        private void Triangulate()
        {
            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].Clear();
            }

            FillFirstRowCache();
            TriangulateCellRows();
            
            if (neighborY != null)
                TriangulateGapRow();

            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].Apply();
            }
        }

        private void FillFirstRowCache()
        {
            CacheFirstCorner(voxels[0]);
            int i = 0;
            for (; i < resolution - 1; i++)
            {
                CacheNextEdgeAndCorner(i, voxels[i], voxels[i + 1]);
            }

            if (neighborX != null)
            {
                dummyX.BecomeXDummyOf(neighborX.voxels[0], gridSize);
                CacheNextEdgeAndCorner(i, voxels[i], dummyX);
            }
        }

        private void CacheFirstCorner(Voxel voxel)
        {
            if (voxel.Filled) renderers[voxel.state].CacheFirstCorner(voxel);
        }

        private void CacheNextEdgeAndCorner(int i, Voxel xMin, Voxel xMax)
        {
            if (xMin.state != xMax.state)
            {
                if (xMin.Filled)
                {
                    if (xMax.Filled)
                    {
                        renderers[xMin.state].CacheXEdge(i, xMin);
                        renderers[xMax.state].CacheXEdge(i, xMin);
                    }
                    else
                    {
                        renderers[xMin.state].CacheXEdgeWithWall(i, xMin);
                    }
                }
                else
                {
                    renderers[xMax.state].CacheXEdgeWithWall(i, xMin);
                }
            }

            if (xMax.Filled)
                renderers[xMax.state].CacheNextCorner(i, xMax);
        }

        private void CacheNextMiddleEdge(Voxel yMin, Voxel yMax)
        {
            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].PrepareCacheForNextCell();
            }

            if (yMin.state != yMax.state)
            {
                if (yMin.Filled)
                {
                    if (yMax.Filled)
                    {
                        renderers[yMin.state].CacheYEdge(yMin);
                        renderers[yMax.state].CacheYEdge(yMin);
                    }
                    else
                    {
                        renderers[yMin.state].CacheYEdgeWithWall(yMin);
                    }
                }
                else
                {
                    renderers[yMax.state].CacheYEdgeWithWall(yMin);
                }
            }
        }

        private void SwapRowCaches()
        {
            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].PrepareCacheForNextRow();
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
                    
                    CacheNextEdgeAndCorner(x, c, d);
                    CacheNextMiddleEdge(b, d);
                    TriangulateCell(x, a, b, c, d);
                }

                if (neighborX != null)
                    TriangulateGapCell(i);
            }
        }

        private void TriangulateGapCell(int i)
        {
            Voxel dummySwap = dummyT;
            dummySwap.BecomeXDummyOf(neighborX.voxels[i + 1], gridSize);
            dummyT = dummyX;
            dummyX = dummySwap;
            
            int cacheIndex = resolution - 1;
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
                
                CacheNextEdgeAndCorner(x, dummyT, dummyY);
                CacheNextMiddleEdge(voxels[x + offset + 1], dummyY);
                TriangulateCell(x, voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY);
            }

            if (neighborX != null)
            {
                dummyT.BecomeTDummyOf(neighborT.voxels[0], gridSize);
                CacheNextEdgeAndCorner(cells, dummyY, dummyT);
                CacheNextMiddleEdge(dummyX, dummyT);
                TriangulateCell(cells, voxels[voxels.Length - 1], dummyX, dummyY, dummyT);
            }
        }

        private void TriangulateCell(int i, Voxel a, Voxel b, Voxel c, Voxel d)
        {
            cell.i = i;
            cell.a = a;
            cell.b = b;
            cell.c = c;
            cell.d = d;

            if (a.state == b.state)
            {
                if (a.state == c.state)
                {
                    if (a.state == d.state)
                        Triangulate0000();
                    else
                        Triangulate0001();
                }
                else
                {
                    if (a.state == d.state)
                        Triangulate0010();
                    else if (c.state == d.state)
                        Triangulate0011();
                    else
                        Triangulate0012();
                }
            }
            else
            {
                if (a.state == c.state)
                {
                    if (a.state == d.state)
                        Triangulate0100();
                    else if (b.state == d.state)
                        Triangulate0101();
                    else
                        Triangulate0102();
                }
                else if (b.state == c.state)
                {
                    if (a.state == d.state)
                        Triangulate0110();
                    else if (b.state == d.state)
                        Triangulate0111();
                    else
                        Triangulate0112();
                }
                else
                {
                    if (a.state == d.state)
                        Triangulate0120();
                    else if (b.state == d.state)
                        Triangulate0121();
                    else if (c.state == d.state)
                        Triangulate0122();
                    else
                        Triangulate0123();
                }
            }
        }

        private void Triangulate0000()
        {
            FillABCD();
        }

        private void Triangulate0001()
        {
            VoxelPoint f = cell.VoxelNE;
            FillABC(f);
            FillD(f);
        }

        private void Triangulate0010()
        {
            VoxelPoint f = cell.VoxelNW;
            FillABD(f);
            FillC(f);
        }

        private void Triangulate0100()
        {
            VoxelPoint f = cell.VoxelSE;
            FillACD(f);
            FillB(f);
        }

        private void Triangulate0111()
        {
            VoxelPoint f = cell.VoxelSW;
            FillA(f);
            FillBCD(f);
        }

        private void Triangulate0011()
        {
            VoxelPoint f = cell.VoxelEW;
            FillAB(f);
            FillCD(f);
        }

        private void Triangulate0101()
        {
            VoxelPoint f = cell.VoxelNS;
            FillAC(f);
            FillBD(f);
        }

        private void Triangulate0012()
        {
            VoxelPoint f = cell.VoxelNEW;
            FillAB(f);
            FillC(f);
            FillD(f);
        }

        private void Triangulate0102()
        {
            VoxelPoint f = cell.VoxelNSE;
            FillAC(f);
            FillB(f);
            FillD(f);
        }

        private void Triangulate0121()
        {
            VoxelPoint f = cell.VoxelNsw;
            FillA(f);
            FillBD(f);
            FillC(f);
        }

        private void Triangulate0122()
        {
            VoxelPoint f = cell.VoxelSew;
            FillA(f);
            FillB(f);
            FillCD(f);
        }

        private void Triangulate0110()
        {
            VoxelPoint fA = cell.VoxelSW;
            VoxelPoint fB = cell.VoxelSE;
            VoxelPoint fC = cell.VoxelNW;
            VoxelPoint fD = cell.VoxelNE;

            if (cell.HasConnectionAD(fA, fD))
            {
                fB.exists &= cell.IsInsideABD(fB.position);
                fC.exists &= cell.IsInsideACD(fC.position);
                FillADToB(fB);
                FillADToC(fC);
                FillB(fB);
                FillC(fC);
            }
            else if (cell.HasConnectionBC(fB, fC))
            {
                fA.exists &= cell.IsInsideABC(fA.position);
                fD.exists &= cell.IsInsideBCD(fD.position);
                FillA(fA);
                FillD(fD);
                FillBCToA(fA);
                FillBCToD(fD);
            }
            else if (cell.a.Filled && cell.b.Filled)
            {
                FillJoinedCorners(fA, fB, fC, fD);
            }
            else
            {
                FillA(fA);
                FillB(fB);
                FillC(fC);
                FillD(fD);
            }
        }

        private void Triangulate0112()
        {
            VoxelPoint fA = cell.VoxelSW;
            VoxelPoint fB = cell.VoxelSE;
            VoxelPoint fC = cell.VoxelNW;
            VoxelPoint fD = cell.VoxelNE;

            if (cell.HasConnectionBC(fB, fC))
            {
                fA.exists &= cell.IsInsideABC(fA.position);
                fD.exists &= cell.IsInsideBCD(fD.position);
                FillA(fA);
                FillD(fD);
                FillBCToA(fA);
                FillBCToD(fD);
            }
            else if (cell.b.Filled || cell.HasConnectionAD(fA, fD))
            {
                FillJoinedCorners(fA, fB, fC, fD);
            }
            else
            {
                FillA(fA);
                FillD(fD);
            }
        }

        private void Triangulate0120()
        {
            VoxelPoint fA = cell.VoxelSW;
            VoxelPoint fB = cell.VoxelSE;
            VoxelPoint fC = cell.VoxelNW;
            VoxelPoint fD = cell.VoxelNE;

            if (cell.HasConnectionAD(fA, fD))
            {
                fB.exists &= cell.IsInsideABD(fB.position);
                fC.exists &= cell.IsInsideACD(fC.position);
                FillADToB(fB);
                FillADToC(fC);
                FillB(fB);
                FillC(fC);
            }
            else if (cell.a.Filled || cell.HasConnectionBC(fB, fC))
            {
                FillJoinedCorners(fA, fB, fC, fD);
            }
            else
            {
                FillB(fB);
                FillC(fC);
            }
        }

        private void Triangulate0123()
        {
            FillJoinedCorners(cell.VoxelSW, cell.VoxelSE, cell.VoxelNW, cell.VoxelNE);
        }

        private void FillA(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillA(cell, f);
        }

        private void FillB(VoxelPoint f)
        {
            if (cell.b.Filled)
                renderers[cell.b.state].FillB(cell, f);
        }

        private void FillC(VoxelPoint f)
        {
            if (cell.c.Filled)
                renderers[cell.c.state].FillC(cell, f);
        }

        private void FillD(VoxelPoint f)
        {
            if (cell.d.Filled)
                renderers[cell.d.state].FillD(cell, f);
        }

        private void FillABC(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillABC(cell, f);
        }

        private void FillABD(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillABD(cell, f);
        }

        private void FillACD(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillACD(cell, f);
        }

        private void FillBCD(VoxelPoint f)
        {
            if (cell.b.Filled)
                renderers[cell.b.state].FillBCD(cell, f);
        }

        private void FillAB(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillAB(cell, f);
        }

        private void FillAC(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillAC(cell, f);
        }

        private void FillBD(VoxelPoint f)
        {
            if (cell.b.Filled)
                renderers[cell.b.state].FillBD(cell, f);
        }

        private void FillCD(VoxelPoint f)
        {
            if (cell.c.Filled)
                renderers[cell.c.state].FillCD(cell, f);
        }

        private void FillADToB(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillADToB(cell, f);
        }

        private void FillADToC(VoxelPoint f)
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillADToC(cell, f);
        }

        private void FillBCToA(VoxelPoint f)
        {
            if (cell.b.Filled)
                renderers[cell.b.state].FillBCToA(cell, f);
        }

        private void FillBCToD(VoxelPoint f)
        {
            if (cell.b.Filled)
                renderers[cell.b.state].FillBCToD(cell, f);
        }

        private void FillABCD()
        {
            if (cell.a.Filled)
                renderers[cell.a.state].FillABCD(cell);
        }

        private void FillJoinedCorners(VoxelPoint fA, VoxelPoint fB, VoxelPoint fC, VoxelPoint fD)
        {
            VoxelPoint point = VoxelPoint.Average(fA, fB, fC, fD);
            if (!point.exists)
            {
                point.position = cell.AverageNESW;
                point.exists = true;
            }

            FillA(point);
            FillB(point);
            FillC(point);
            FillD(point);
        }

        private void SetVoxelColors()
        {
            for (var i = 0; i < voxels.Length; i++)
            {
                voxelMaterials[i].color = voxels[i].Filled ? Color.black : Color.white;
            }
        }

        public void Apply(VoxelStencil stencil)
        {
            int xStart = (int) (stencil.XStart / voxelSize);
            if (xStart < 0) xStart = 0;

            int xEnd = (int) (stencil.XEnd / voxelSize);
            if (xEnd >= resolution) xEnd = resolution - 1;

            int yStart = (int) (stencil.YStart / voxelSize);
            if (yStart < 0) yStart = 0;

            int yEnd = (int) (stencil.YEnd / voxelSize);
            if (yEnd >= resolution) yEnd = resolution - 1;

            for (int y = yStart; y <= yEnd; y++)
            {
                int i = y * resolution + xStart;
                for (int x = xStart; x <= xEnd; x++, i++)
                {
                    stencil.Apply(voxels[i]);
                }
            }

            SetCrossings(stencil, xStart, xEnd, yStart, yEnd);
            Refresh();
        }

        private void SetCrossings(VoxelStencil stencil, int xStart, int xEnd, int yStart, int yEnd)
        {
            bool crossHorizontalGap = false;
            bool includeLastVerticalRow = false;
            bool crossVerticalGap = false;

            if (xStart > 0)
            {
                xStart -= 1;
            }

            if (xEnd == resolution - 1)
            {
                xEnd -= 1;
                crossHorizontalGap = neighborX != null;
            }

            if (yStart > 0)
            {
                yStart -= 1;
            }

            if (yEnd == resolution - 1)
            {
                yEnd -= 1;
                includeLastVerticalRow = true;
                crossVerticalGap = neighborY != null;
            }

            Voxel a, b;
            for (var y = yStart; y <= yEnd; y++)
            {
                int i = y * resolution + xStart;
                b = voxels[i];
                for (var x = xStart; x <= xEnd; x++, i++)
                {
                    a = b;
                    b = voxels[i + 1];
                    stencil.SetHorizontalCrossing(a, b);
                    stencil.SetVerticalCrossing(a, voxels[i + resolution]);
                }

                stencil.SetVerticalCrossing(b, voxels[i + resolution]);
                if (crossHorizontalGap)
                {
                    dummyX.BecomeXDummyOf(
                        neighborX.voxels[y * resolution], gridSize);
                    stencil.SetHorizontalCrossing(b, dummyX);
                }
            }

            if (includeLastVerticalRow)
            {
                int i = voxels.Length - resolution + xStart;
                b = voxels[i];
                for (var x = xStart; x <= xEnd; x++, i++)
                {
                    a = b;
                    b = voxels[i + 1];
                    stencil.SetHorizontalCrossing(a, b);
                    if (crossVerticalGap)
                    {
                        dummyY.BecomeYDummyOf(neighborY.voxels[x], gridSize);
                        stencil.SetVerticalCrossing(a, dummyY);
                    }
                }

                if (crossVerticalGap)
                {
                    dummyY.BecomeYDummyOf(neighborY.voxels[xEnd + 1], gridSize);
                    stencil.SetVerticalCrossing(b, dummyY);
                }

                if (crossHorizontalGap)
                {
                    dummyX.BecomeXDummyOf(
                        neighborX.voxels[voxels.Length - resolution], gridSize);
                    stencil.SetHorizontalCrossing(b, dummyX);
                }
            }
        }
    }
}
