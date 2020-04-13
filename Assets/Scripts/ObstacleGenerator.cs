using System.Collections.Generic;
using UnityEngine;

public static class ObstacleGenerator
{
    public static void GenerateObstacleMesh(MeshGenerator.MeshData meshData)
    {
        bool[,] walkableBoundary = meshData.walkableBoundary;
        bool[,] waterTile = meshData.waterTile;

        int width = meshData.walkableBoundary.GetLength(0);
        int height = meshData.walkableBoundary.GetLength(1);

        float topLeftX = -0.5f * width;
        float topLeftZ = 0.5f * height;

        float colliderHeight = 2f;
        float epsilon = 0.00001f;

        Vector2Int[] nswe = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector3[] sideDirections = { Vector3.right, Vector3.left, Vector3.back, Vector3.forward };
        Vector3[] nsweNormals = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        int[][] nsweSideVertIndex = { new int[] { 3, 2 }, new int[] { 0, 1 }, new int[] { 2, 0 }, new int[] { 1, 3 } };

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (walkableBoundary[x,y])
                {
                    //Debug.Log("Boundary at y = " + y + ", x = " + x);

                    Vector3 nw = new Vector3(topLeftX + x, 1f, topLeftZ - y);
                    Vector3 ne = nw + Vector3.right;
                    Vector3 sw = nw - Vector3.forward;
                    Vector3 se = sw + Vector3.right;

                    var tileVertices = new List<Vector3> { nw, ne, sw, se };

                    for (int i = 0; i < nswe.Length; i++)
                    {
                        int neighbourX = x + nswe[i].x;
                        int neighbourY = y + nswe[i].y;
                        bool isNeighbourOutside = neighbourX < 0 || neighbourY < 0 || neighbourX >= width || neighbourY >= height;

                        bool isNeighbourWaterTile = false;
                        if (!isNeighbourOutside)
                            isNeighbourWaterTile = waterTile[neighbourX, neighbourY];

                        

                        if (isNeighbourOutside || (!waterTile[x, y] && isNeighbourWaterTile))
                        {
                            vertexIndex = vertices.Count;

                            int vertSideIndexA = nsweSideVertIndex[i][0];
                            int vertSideIndexB = nsweSideVertIndex[i][1];

                            Vector3 vertexA = tileVertices[vertSideIndexA] + (Vector3.up - sideDirections[i]) * epsilon;
                            Vector3 vertexAup = tileVertices[vertSideIndexA] + Vector3.up * colliderHeight - (Vector3.up + sideDirections[i]) * epsilon;
                            Vector3 vertexB = tileVertices[vertSideIndexB] + (Vector3.up + sideDirections[i]) * epsilon;
                            Vector3 vertexBup = tileVertices[vertSideIndexB] + Vector3.up * colliderHeight - (Vector3.up - sideDirections[i]) * epsilon;

                            var sideVertices = new List<Vector3>();

                            sideVertices.Add(vertexA);
                            sideVertices.Add(vertexAup);
                            sideVertices.Add(vertexB);
                            sideVertices.Add(vertexBup);

                            vertices.AddRange(sideVertices);

                            int[] triangleIndices = new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3, vertexIndex + 2 };
                            triangles.AddRange(triangleIndices);

                            normals.AddRange(NormalsArray(sideVertices.Count, nsweNormals[i]));
                        }
                    }
                }
            }
        }

        CreateMesh(vertices.ToArray(), triangles.ToArray(), normals.ToArray());
    }

    private static Vector3[] NormalsArray(int size, Vector3 normal)
    {
        var normals = new Vector3[size];
        for (int i = 0; i < size; i++)
        {
            normals[i] = normal;
        }
        return normals;
    }

    public static void CreateMesh(Vector3[] vertices, int[] triangles, Vector3[] normals)
    {
        GameObject holder = GameObject.Find("MeshObstacles");
        if (holder == null)
        {
            holder = new GameObject("MeshObstacles");
            holder.AddComponent<MeshFilter>();
            holder.AddComponent<MeshCollider>();
        }
        
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            normals = normals
        };

        holder.GetComponent<MeshFilter>().sharedMesh = mesh;
        holder.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
