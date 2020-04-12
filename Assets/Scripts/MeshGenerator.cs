using UnityEngine;
using System.Collections.Generic;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap)
    {
        int tilesWidth = heightMap.GetLength(0);
        int tilesHeight = heightMap.GetLength(1);

        float topLeftX = - 0.5f * tilesWidth;
        float topLeftZ = 0.5f * tilesHeight;

        float waterThreshold = 0.4f;
        float defaultHeight = 1f;
        float waterDepth = 0.2f;

        // convenience
        Vector2Int[] nswe = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int[][] nsweSideVertIndex = { new int[] { 3, 2 }, new int[] { 0, 1 }, new int[] { 2, 0 }, new int[] { 1, 3 } };
        Vector3[] nsweNormals = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3[] sideDirections = { Vector3.right, Vector3.left, Vector3.back, Vector3.forward };

        float epsilon = 0.00001f;

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        float uFraction = 1f / (float)tilesWidth;
        float vFraction = 1f / (float)tilesHeight;

        int indicesCountCurrentIter = 0;
        int indicesCountCurrentRow = 0;
        for (int y = 0; y < tilesHeight; y++)
        {
            int indicesCountLastRow = indicesCountCurrentRow;
            indicesCountCurrentRow = 0;

            for (int x = 0; x < tilesWidth; x++)
            {
                int indicesCountLastIter = indicesCountCurrentIter;
                indicesCountCurrentIter = 0;

                bool isWaterTile = heightMap[x, y] < waterThreshold;

                int vertexIndex = vertices.Count;

                float height = (isWaterTile) ? (defaultHeight - waterDepth) : defaultHeight;
                Vector3 nw = new Vector3(topLeftX + x, height, topLeftZ - y);
                Vector3 ne = nw + Vector3.right;
                Vector3 sw = nw - Vector3.forward;
                Vector3 se = sw + Vector3.right;

                // check that previous tile in x-line has west indices (only check x-lines)
                List<Vector3> tileVertices = new List<Vector3>();
                int nwIndex, neIndex, swIndex, seIndex;

                List<Vector2> tileUvs = new List<Vector2>();

                Vector2 uvnw = new Vector2((float)x * uFraction, (float)y * vFraction);
                Vector2 uvne = new Vector2((float)(x + 1) * uFraction, (float)y * vFraction);
                Vector2 uvsw = new Vector2((float)x * uFraction, (float)(y + 1) * vFraction);
                Vector2 uvse = new Vector2((float)(x + 1) * uFraction, (float)(y + 1) * vFraction);


                // List.IndexOf() is x2 - x4 faster than List.Contains()
                // In IndexOf() we estimate the starting index to improve speed (x5 - x10 faster that without it)
                nwIndex = vertices.IndexOf(nw, vertexIndex - indicesCountLastRow - indicesCountCurrentRow - indicesCountCurrentIter);
                if (nwIndex == -1)
                {
                    tileVertices.Add(nw);
                    tileUvs.Add(uvnw);
                    nwIndex = vertexIndex;
                    vertexIndex++;
                    indicesCountCurrentIter++;
                }

                neIndex = vertices.IndexOf(ne, vertexIndex - indicesCountLastRow - indicesCountCurrentRow - indicesCountCurrentIter);
                if (neIndex == -1)
                {
                    tileVertices.Add(ne);
                    tileUvs.Add(uvne);
                    neIndex = vertexIndex;
                    vertexIndex++;
                    indicesCountCurrentIter++;
                }

                swIndex = vertices.IndexOf(sw, vertexIndex - indicesCountLastIter - indicesCountCurrentIter);
                if (swIndex == -1)
                {
                    tileVertices.Add(sw);
                    tileUvs.Add(uvsw);
                    swIndex = vertexIndex;
                    vertexIndex++;
                    indicesCountCurrentIter++;
                }

                // se vertex corner should never be already in the list so we just add it
                tileVertices.Add(se);
                tileUvs.Add(uvse);
                seIndex = vertexIndex;
                indicesCountCurrentIter++;

                vertices.AddRange(tileVertices);
                normals.AddRange(NormalsArray(indicesCountCurrentIter, Vector3.up));
                uvs.AddRange(tileUvs);

                int[] triangleIndices = { nwIndex, neIndex, swIndex, neIndex, seIndex, swIndex };
                triangles.AddRange(triangleIndices);

                tileVertices = new List<Vector3> { nw, ne, sw, se };


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

                            Vector3 vertexA = tileVertices[vertSideIndexA] + (Vector3.down - sideDirections[i]) * epsilon;
                            Vector3 vertexAdown = tileVertices[vertSideIndexA] + Vector3.down * depth - (Vector3.down + sideDirections[i]) * epsilon;
                            Vector3 vertexB = tileVertices[vertSideIndexB] + (Vector3.down + sideDirections[i]) * epsilon;
                            Vector3 vertexBdown = tileVertices[vertSideIndexB] + Vector3.down * depth - (Vector3.down - sideDirections[i]) * epsilon;

                            var sideVertices = new List<Vector3>();
                            var sideUvs = new List<Vector2>();

                            sideVertices.Add(vertexA);
                            sideVertices.Add(vertexAdown);
                            sideVertices.Add(vertexB);
                            sideVertices.Add(vertexBdown);

                            sideUvs.Add(uvnw);
                            sideUvs.Add(uvsw);
                            sideUvs.Add(uvne);
                            sideUvs.Add(uvse);

                            indicesCountCurrentIter += 4;

                            vertices.AddRange(sideVertices);
                            uvs.AddRange(sideUvs);

                            triangleIndices = new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3, vertexIndex + 2 };
                            triangles.AddRange(triangleIndices);

                            normals.AddRange(NormalsArray(sideVertices.Count, nsweNormals[i]));
                        }
                    }
                }

                indicesCountCurrentRow += indicesCountCurrentIter;
            }
        }

        return new MeshData(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), normals.ToArray());
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
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
