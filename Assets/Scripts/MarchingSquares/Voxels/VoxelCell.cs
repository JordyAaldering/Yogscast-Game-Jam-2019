using System;
using UnityEngine;

namespace MarchingSquares.Voxels
{
    [Serializable]
    public class VoxelCell
    {
        [NonSerialized] public Voxel a, b, c, d;
        [NonSerialized] public int i;

        public float sharpFeatureLimit, parallelLimit;

        public Vector2 AverageNESW => (a.XEdgePoint + a.YEdgePoint + b.YEdgePoint + c.XEdgePoint) * 0.25f;

        public VoxelPoint VoxelSW => GetSharpFeature(a.XEdgePoint, a.xNormal, a.YEdgePoint, a.yNormal);
        public VoxelPoint VoxelSE => GetSharpFeature(a.XEdgePoint, a.xNormal, b.YEdgePoint, b.yNormal);
        public VoxelPoint VoxelNW => GetSharpFeature(a.YEdgePoint, a.yNormal, c.XEdgePoint, c.xNormal);
        public VoxelPoint VoxelNE => GetSharpFeature(c.XEdgePoint, c.xNormal, b.YEdgePoint, b.yNormal);
        public VoxelPoint VoxelNS => GetSharpFeature(a.XEdgePoint, a.xNormal, c.XEdgePoint, c.xNormal);
        public VoxelPoint VoxelEW => GetSharpFeature(a.YEdgePoint, a.yNormal, b.YEdgePoint, b.yNormal);

        public VoxelPoint VoxelNEW
        {
            get
            {
                VoxelPoint f = VoxelPoint.Average(VoxelEW, VoxelNE, VoxelNW);
                if (!f.exists)
                {
                    f.position = (a.YEdgePoint + b.YEdgePoint + c.XEdgePoint) / 3f;
                    f.exists = true;
                }

                return f;
            }
        }

        public VoxelPoint VoxelNSE
        {
            get
            {
                VoxelPoint f = VoxelPoint.Average(VoxelNS, VoxelSE, VoxelNE);
                if (!f.exists)
                {
                    f.position = (a.XEdgePoint + b.YEdgePoint + c.XEdgePoint) / 3f;
                    f.exists = true;
                }

                return f;
            }
        }

        public VoxelPoint VoxelNsw
        {
            get
            {
                VoxelPoint f = VoxelPoint.Average(VoxelNS, VoxelNW, VoxelSW);
                if (!f.exists)
                {
                    f.position = (a.XEdgePoint + a.YEdgePoint + c.XEdgePoint) / 3f;
                    f.exists = true;
                }

                return f;
            }
        }

        public VoxelPoint VoxelSew
        {
            get
            {
                VoxelPoint f = VoxelPoint.Average(VoxelEW, VoxelSE, VoxelSW);
                if (!f.exists)
                {
                    f.position = (a.XEdgePoint + a.YEdgePoint + b.YEdgePoint) / 3f;
                    f.exists = true;
                }

                return f;
            }
        }

        private VoxelPoint CheckedVoxelSw
        {
            get
            {
                Vector2 n2 = a.state < b.state == a.state < c.state ? a.yNormal : -a.yNormal;
                return GetSharpFeature(a.XEdgePoint, a.xNormal, a.YEdgePoint, n2);
            }
        }

        private VoxelPoint CheckedVoxelSe
        {
            get
            {
                Vector2 n2 = b.state < a.state == b.state < c.state ? b.yNormal : -b.yNormal;
                return GetSharpFeature(a.XEdgePoint, a.xNormal, b.YEdgePoint, n2);
            }
        }

        private VoxelPoint CheckedVoxelNw
        {
            get
            {
                Vector2 n2 = c.state < a.state == c.state < d.state ? c.xNormal : -c.xNormal;
                return GetSharpFeature(a.YEdgePoint, a.yNormal, c.XEdgePoint, n2);
            }
        }

        private VoxelPoint CheckedVoxelNe
        {
            get
            {
                Vector2 n2 = d.state < b.state == d.state < c.state ? b.yNormal : -b.yNormal;
                return GetSharpFeature(c.XEdgePoint, c.xNormal, b.YEdgePoint, n2);
            }
        }

        private VoxelPoint CheckedVoxelNs
        {
            get
            {
                Vector2 n2 = a.state < b.state == c.state < d.state ? c.xNormal : -c.xNormal;
                return GetSharpFeature(a.XEdgePoint, a.xNormal, c.XEdgePoint, n2);
            }
        }

        private VoxelPoint CheckedVoxelEw
        {
            get
            {
                Vector2 n2 = a.state < c.state == b.state < d.state ? b.yNormal : -b.yNormal;
                return GetSharpFeature(a.YEdgePoint, a.yNormal, b.YEdgePoint, n2);
            }
        }

        public bool HasConnectionAD(VoxelPoint fA, VoxelPoint fD)
        {
            bool flip = a.state < b.state == a.state < c.state;
            if (IsParallel(a.xNormal, a.yNormal, flip) || IsParallel(c.xNormal, b.yNormal, flip))
                return true;

            if (fA.exists)
            {
                if (fD.exists)
                {
                    if (IsBelowLine(fA.position, b.YEdgePoint, fD.position))
                    {
                        if (IsBelowLine(fA.position, fD.position, c.XEdgePoint) || 
                            IsBelowLine(fD.position, fA.position, a.XEdgePoint))
                            return true;
                    }
                    else if (IsBelowLine(fA.position, fD.position, c.XEdgePoint) && 
                             IsBelowLine(fD.position, a.YEdgePoint, fA.position))
                    {
                        return true;
                    }

                    return false;
                }

                return IsBelowLine(fA.position, b.YEdgePoint, c.XEdgePoint);
            }

            return fD.exists && IsBelowLine(fD.position, a.YEdgePoint, a.XEdgePoint);
        }

        public bool HasConnectionBC(VoxelPoint fB, VoxelPoint fC)
        {
            bool flip = b.state < a.state == b.state < d.state;
            if (IsParallel(a.xNormal, b.yNormal, flip) || IsParallel(c.xNormal, a.yNormal, flip))
                return true;

            if (fB.exists)
            {
                if (fC.exists)
                {
                    if (IsBelowLine(fC.position, a.XEdgePoint, fB.position))
                    {
                        if (IsBelowLine(fC.position, fB.position, b.YEdgePoint) ||
                            IsBelowLine(fB.position, fC.position, a.YEdgePoint))
                            return true;
                    }
                    else if (IsBelowLine(fC.position, fB.position, b.YEdgePoint) &&
                             IsBelowLine(fB.position, c.XEdgePoint, fC.position))
                    {
                        return true;
                    }

                    return false;
                }

                return IsBelowLine(fB.position, c.XEdgePoint, a.YEdgePoint);
            }

            return fC.exists && IsBelowLine(fC.position, a.XEdgePoint, b.YEdgePoint);
        }

        public bool IsInsideABD(Vector2 point)
        {
            return IsBelowLine(point, a.position, d.position);
        }

        public bool IsInsideACD(Vector2 point)
        {
            return IsBelowLine(point, d.position, a.position);
        }

        public bool IsInsideABC(Vector2 point)
        {
            return IsBelowLine(point, c.position, b.position);
        }

        public bool IsInsideBCD(Vector2 point)
        {
            return IsBelowLine(point, b.position, c.position);
        }

        private static bool IsBelowLine(Vector2 p, Vector2 start, Vector2 end)
        {
            var determinant = (end.x - start.x) * (p.y - start.y) - (end.y - start.y) * (p.x - start.x);
            return determinant < 0f;
        }

        private VoxelPoint GetSharpFeature(Vector2 p1, Vector2 n1, Vector2 p2, Vector2 n2)
        {
            VoxelPoint point;
            if (IsSharpFeature(n1, n2))
            {
                point.position = GetIntersection(p1, n1, p2, n2);
                point.exists = IsInsideCell(point.position);
            }
            else
            {
                point.position = Vector2.zero;
                point.exists = false;
            }

            return point;
        }

        private bool IsSharpFeature(Vector2 n1, Vector2 n2)
        {
            float dot = Vector2.Dot(n1, -n2);
            return dot >= sharpFeatureLimit && dot < 0.9999f;
        }

        private bool IsParallel(Vector2 n1, Vector2 n2, bool flip)
        {
            return Vector2.Dot(n1, flip ? -n2 : n2) > parallelLimit;
        }

        private static Vector2 GetIntersection(Vector2 p1, Vector2 n1, Vector2 p2, Vector2 n2)
        {
            Vector2 d2 = new Vector2(-n2.y, n2.x);
            float u2 = -Vector2.Dot(n1, p2 - p1) / Vector2.Dot(n1, d2);
            return p2 + d2 * u2;
        }

        private bool IsInsideCell(Vector2 point)
        {
            return point.x > a.position.x && point.y > a.position.y && 
                   point.x < d.position.x && point.y < d.position.y;
        }
    }
}
