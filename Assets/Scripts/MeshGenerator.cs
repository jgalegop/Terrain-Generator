using UnityEngine;
using System.Collections.Generic;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap)
    {
        int tilesWidth = heightMap.GetLength(0);
        int tilesHeight = heightMap.GetLength(1);

        float topLeftX = -(tilesWidth - 1) / 2f;
        float topLeftZ = (tilesHeight - 1) / 2f;

        float waterThreshold = 0.4f;
        float defaultHeight = 1f;
        float waterDepth = 0.2f;

        // convenience
        Vector3[] tileNormals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        Vector2Int[] nswe = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int[][] nsweSideVertIndex = { new int[] { 3, 2 }, new int[] { 0, 1 }, new int[] { 2, 0 }, new int[] { 1, 3 } };
        Vector3[] nsweNormals = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };


        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        for (int y = 0; y < tilesHeight; y++)
        {
            for (int x = 0; x < tilesWidth; x++)
            {
                bool isWaterTile = heightMap[x, y] < waterThreshold;
                int vertexIndex = vertices.Count;

                float height = (isWaterTile) ? (defaultHeight - waterDepth) : defaultHeight;
                Vector3 nw = new Vector3(topLeftX + x, height, topLeftZ - y);
                Vector3 ne = nw + Vector3.right;
                Vector3 sw = nw - Vector3.forward;
                Vector3 se = sw + Vector3.right;
                Vector3[] tileVertices = { nw, ne, sw, se };
                vertices.AddRange(tileVertices);
                normals.AddRange(tileNormals);

                Vector2 uv = new Vector2(x / (float)tilesWidth, y / (float)tilesHeight);
                uvs.AddRange(new Vector2[] { uv, uv, uv, uv });

                int[] triangleIndices = { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3, vertexIndex + 2 };
                triangles.AddRange(triangleIndices);

                
                bool isEdgeTile = x == 0 || y == 0 || x == tilesWidth - 1 || y == tilesHeight - 1;
                if (!isWaterTile || isEdgeTile)
                {
                    for (int i = 0; i < nswe.Length; i++)
                    {
                        int neighbourX = x + nswe[i].x;
                        int neighbourY = y + nswe[i].y;
                        bool isNeighbourOutside = neighbourX < 0 || neighbourY < 0 || neighbourX >= tilesWidth || neighbourY >= tilesHeight;

                        bool isNeighbourWaterTile = false;
                        if (!isNeighbourOutside)
                            isNeighbourWaterTile = heightMap[neighbourX, neighbourY] < waterThreshold;

                        if (isNeighbourOutside || (!isWaterTile && isNeighbourWaterTile))
                        {
                            float depth = (isNeighbourOutside) ? height : waterDepth;
                            vertexIndex = vertices.Count;

                            int vertSideIndexA = nsweSideVertIndex[i][0];
                            int vertSideIndexB = nsweSideVertIndex[i][1];
                            vertices.Add(tileVertices[vertSideIndexA]);
                            vertices.Add(tileVertices[vertSideIndexA] + Vector3.down * depth);
                            vertices.Add(tileVertices[vertSideIndexB]);
                            vertices.Add(tileVertices[vertSideIndexB] + Vector3.down * depth);

                            uvs.AddRange(new Vector2[] { uv, uv, uv, uv });

                            triangleIndices = new int[]{ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3, vertexIndex + 2 };
                            triangles.AddRange(triangleIndices);

                            Vector3[] sideNormals = new Vector3[] { nsweNormals[i], nsweNormals[i], nsweNormals[i], nsweNormals[i] };
                            normals.AddRange(sideNormals);
                        }
                    }
                }
                
            }
        }

        return new MeshData(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), normals.ToArray());
    }



    public class MeshData
    {
        //public Vector3[,] tileCenters;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public Vector3[] normals;

        public MeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3[] normals)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
            this.normals = normals;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, true);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.Optimize();
            return mesh;
        }
    }
}
