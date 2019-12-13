﻿using System.Collections.Generic;
using MarchingSquares.Voxels;
using UnityEngine;

namespace MarchingSquares.Grid
{
    public class VoxelGridWall : MonoBehaviour
    {
        public float bottom, top;

        private Mesh mesh;
        private List<int> triangles;
        private List<Vector3> vertices;
        private List<Vector3> normals;

        private int[] xEdgesMin, xEdgesMax;
        private int yEdgeMin, yEdgeMax;

        public void Initialize(int resolution, Material material)
        {
            GetComponent<MeshRenderer>().material = material;
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            
            mesh.name = "Voxel Grid Wall Mesh";
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            triangles = new List<int>();
            
            xEdgesMin = new int[resolution];
            xEdgesMax = new int[resolution];
        }

        public void Clear()
        {
            vertices.Clear();
            normals.Clear();
            triangles.Clear();
            mesh.Clear();
        }

        public void Apply()
        {
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
        }

        public void CacheXEdge(int i, Voxel voxel)
        {
            xEdgesMax[i] = vertices.Count;
            Vector3 v = voxel.XEdgePoint;
            
            v.z = bottom;
            vertices.Add(v);
            
            v.z = top;
            vertices.Add(v);
            
            Vector3 n = voxel.xNormal;
            normals.Add(n);
            normals.Add(n);
        }

        public void CacheYEdge(Voxel voxel)
        {
            yEdgeMax = vertices.Count;
            Vector3 v = voxel.YEdgePoint;
            v.z = bottom;
            vertices.Add(v);
            
            v.z = top;
            vertices.Add(v);
            
            Vector3 n = voxel.yNormal;
            normals.Add(n);
            normals.Add(n);
        }

        public void PrepareCacheForNextCell()
        {
            yEdgeMin = yEdgeMax;
        }

        public void PrepareCacheForNextRow()
        {
            int[] swap = xEdgesMin;
            xEdgesMin = xEdgesMax;
            xEdgesMax = swap;
        }

        public void AddABAC(int i)
        {
            AddSection(xEdgesMin[i], yEdgeMin);
        }

        public void AddABAC(int i, Vector2 extraVertex)
        {
            AddSection(xEdgesMin[i], yEdgeMin, extraVertex);
        }

        public void AddABBD(int i)
        {
            AddSection(xEdgesMin[i], yEdgeMax);
        }

        public void AddABCD(int i)
        {
            AddSection(xEdgesMin[i], xEdgesMax[i]);
        }

        public void AddACAB(int i)
        {
            AddSection(yEdgeMin, xEdgesMin[i]);
        }

        public void AddACBD(int i)
        {
            AddSection(yEdgeMin, yEdgeMax);
        }

        public void AddACCD(int i)
        {
            AddSection(yEdgeMin, xEdgesMax[i]);
        }

        public void AddACCD(int i, Vector2 extraVertex)
        {
            AddSection(yEdgeMin, xEdgesMax[i], extraVertex);
        }

        public void AddBDAB(int i)
        {
            AddSection(yEdgeMax, xEdgesMin[i]);
        }

        public void AddBDAB(int i, Vector2 extraVertex)
        {
            AddSection(yEdgeMax, xEdgesMin[i], extraVertex);
        }

        public void AddBDAC(int i)
        {
            AddSection(yEdgeMax, yEdgeMin);
        }

        public void AddBDCD(int i)
        {
            AddSection(yEdgeMax, xEdgesMax[i]);
        }

        public void AddCDAB(int i)
        {
            AddSection(xEdgesMax[i], xEdgesMin[i]);
        }

        public void AddCDAC(int i)
        {
            AddSection(xEdgesMax[i], yEdgeMin);
        }

        public void AddCDBD(int i)
        {
            AddSection(xEdgesMax[i], yEdgeMax);
        }

        public void AddCDBD(int i, Vector2 extraVertex)
        {
            AddSection(xEdgesMax[i], yEdgeMax, extraVertex);
        }

        public void AddFromAB(int i, Vector2 extraVertex)
        {
            AddHalfSection(xEdgesMin[i], extraVertex);
        }

        public void AddToAB(int i, Vector2 extraVertex)
        {
            AddHalfSection(extraVertex, xEdgesMin[i]);
        }

        public void AddFromAC(int i, Vector2 extraVertex)
        {
            AddHalfSection(yEdgeMin, extraVertex);
        }

        public void AddToAC(int i, Vector2 extraVertex)
        {
            AddHalfSection(extraVertex, yEdgeMin);
        }

        public void AddFromBD(int i, Vector2 extraVertex)
        {
            AddHalfSection(yEdgeMax, extraVertex);
        }

        public void AddToBD(int i, Vector2 extraVertex)
        {
            AddHalfSection(extraVertex, yEdgeMax);
        }

        public void AddFromCD(int i, Vector2 extraVertex)
        {
            AddHalfSection(xEdgesMax[i], extraVertex);
        }

        public void AddToCD(int i, Vector2 extraVertex)
        {
            AddHalfSection(extraVertex, xEdgesMax[i]);
        }

        private void AddSection(int a, int b)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(b + 1);
            
            triangles.Add(a);
            triangles.Add(b + 1);
            triangles.Add(a + 1);
        }

        private void AddSection(int a, int b, Vector3 extraPoint)
        {
            AddSection(a, AddPoint(extraPoint, a));
            AddSection(AddPoint(extraPoint, b), b);
        }

        private void AddHalfSection(int a, Vector3 extraPoint)
        {
            AddSection(a, AddPoint(extraPoint, a));
        }

        private void AddHalfSection(Vector3 extraPoint, int a)
        {
            AddSection(AddPoint(extraPoint, a), a);
        }

        private int AddPoint(Vector3 extraPoint, int normalIndex)
        {
            int p = vertices.Count;
            extraPoint.z = bottom;
            vertices.Add(extraPoint);
            
            extraPoint.z = top;
            vertices.Add(extraPoint);
            
            Vector3 n = normals[normalIndex];
            normals.Add(n);
            normals.Add(n);
            
            return p;
        }
    }
}
