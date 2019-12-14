using System.Collections.Generic;
using UnityEngine;

public static class ColliderCreator
{
    public static void ApplyMesh(PolygonCollider2D polygonCollider, Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
        
        for (int i = 0; i < triangles.Length; i += 3)
        for (int e = 0; e < 3; e++)
        {
            int vert1 = triangles[i + e];
            int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
            string edge = $"{Mathf.Min(vert1, vert2)}:{Mathf.Max(vert1, vert2)}";

            if (edges.ContainsKey(edge))
            {
                edges.Remove(edge);
            }
            else
            {
                edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
            }
        }

        Dictionary<int, int> lookup = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> edge in edges.Values)
        {
            if (!lookup.ContainsKey(edge.Key))
                lookup.Add(edge.Key, edge.Value);
        }

        polygonCollider.pathCount = 0;

        int startVert = 0;
        int nextVert = startVert;
        int highestVert = startVert;
        List<Vector2> colliderPath = new List<Vector2>();
        
        while (true)
        {
            if (nextVert >= vertices.Length)
                break;
            
            colliderPath.Add(vertices[nextVert]);
            nextVert = lookup[nextVert];

            if (nextVert > highestVert)
                highestVert = nextVert;

            if (nextVert == startVert)
            {
                int pathCount = polygonCollider.pathCount;
                
                pathCount++;
                polygonCollider.pathCount = pathCount;
                polygonCollider.SetPath(pathCount - 1, colliderPath.ToArray());
                
                colliderPath.Clear();

                if (lookup.ContainsKey(highestVert + 1))
                {
                    startVert = highestVert + 1;
                    nextVert = startVert;
                    continue;
                }

                break;
            }
        }
    }
}
