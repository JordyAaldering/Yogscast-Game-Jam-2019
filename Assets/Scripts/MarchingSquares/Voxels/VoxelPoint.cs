using UnityEngine;

namespace MarchingSquares.Voxels
{
    public struct VoxelPoint
    {
        public Vector2 position;
        public bool exists;

        public static VoxelPoint Average(VoxelPoint a, VoxelPoint b, VoxelPoint c)
        {
            VoxelPoint average;
            average.position = Vector2.zero;
            float features = 0f;
            
            if (a.exists)
            {
                average.position += a.position;
                features += 1f;
            }

            if (b.exists)
            {
                average.position += b.position;
                features += 1f;
            }

            if (c.exists)
            {
                average.position += c.position;
                features += 1f;
            }

            if (features > 0f)
            {
                average.position /= features;
                average.exists = true;
            }
            else
            {
                average.exists = false;
            }

            return average;
        }

        public static VoxelPoint Average(VoxelPoint a, VoxelPoint b, VoxelPoint c, VoxelPoint d)
        {
            VoxelPoint average;
            average.position = Vector2.zero;
            var features = 0f;
            
            if (a.exists)
            {
                average.position += a.position;
                features += 1f;
            }

            if (b.exists)
            {
                average.position += b.position;
                features += 1f;
            }

            if (c.exists)
            {
                average.position += c.position;
                features += 1f;
            }

            if (d.exists)
            {
                average.position += d.position;
                features += 1f;
            }

            if (features > 0f)
            {
                average.position /= features;
                average.exists = true;
            }
            else
            {
                average.exists = false;
            }

            return average;
        }
    }
}
