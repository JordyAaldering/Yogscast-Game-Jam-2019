using UnityEngine;
using System;

namespace MarchingSquares
{
	[Serializable]
	public class Voxel
	{
		public bool state;
		public Vector2 position;
		public float xEdge, yEdge;

		public Voxel(bool state, Vector2Int pos, float size)
		{
			this.state = state;
			
			position.x = (pos.x + 0.5f) * size;
			position.y = (pos.y + 0.5f) * size;

			xEdge = position.x + size * 0.5f;
			yEdge = position.y + size * 0.5f;
		}

		public Voxel() { }

		public void BecomeXDummyOf(Voxel voxel, float offset)
		{
			state = voxel.state;
			
			position = voxel.position;
			position.x += offset;
			
			xEdge = voxel.xEdge + offset;
			yEdge = voxel.yEdge;
		}

		public void BecomeYDummyOf(Voxel voxel, float offset)
		{
			state = voxel.state;
			
			position = voxel.position;
			position.y += offset;
			
			xEdge = voxel.xEdge;
			yEdge = voxel.yEdge + offset;
		}

		public void BecomeXYDummyOf(Voxel voxel, float offset)
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
