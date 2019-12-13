﻿using System;
using UnityEngine;

namespace MarchingSquares.Voxels
{
    [Serializable]
    public class Voxel
    {
        public Vector2 position;
        public int state;

        public float xEdge, yEdge;
        public Vector2 xNormal, yNormal;

        public bool Filled => state > 0f;

        public Vector2 XEdgePoint => new Vector2(xEdge, position.y);
        public Vector2 YEdgePoint => new Vector2(position.x, yEdge);

        public Voxel(int x, int y, float size)
        {
            position.x = (x + 0.5f) * size;
            position.y = (y + 0.5f) * size;

            xEdge = float.MinValue;
            yEdge = float.MinValue;
        }

        public Voxel() { }

        public void BecomeXDummyOf(Voxel voxel, float offset)
        {
            state = voxel.state;
            position = voxel.position;
            position.x += offset;
            
            xEdge = voxel.xEdge + offset;
            yEdge = voxel.yEdge;
            
            yNormal = voxel.yNormal;
        }

        public void BecomeYDummyOf(Voxel voxel, float offset)
        {
            state = voxel.state;
            position = voxel.position;
            position.y += offset;
            
            xEdge = voxel.xEdge;
            yEdge = voxel.yEdge + offset;
            
            xNormal = voxel.xNormal;
        }

        public void BecomeTDummyOf(Voxel voxel, float offset)
        {
            state = voxel.state;
            
            position = voxel.position;
            position.x += offset;
            position.y += offset;
            
            xEdge = voxel.xEdge + offset;
            yEdge = voxel.yEdge + offset;
        }
    }
}
