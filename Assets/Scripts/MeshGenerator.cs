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

        int indicesCountCurrentIter = 0;
        for (int y = 0; y < tilesHeight; y++)
        {
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
                int nwIndex = vertexIndex;
                int neIndex = vertexIndex + 1;
                int swIndex = vertexIndex + 2;
                int seIndex = vertexIndex + 3;

                if (y == 0 && false)
                {
                    if (vertices.Contains(nw))
                    {
                        Debug.Log("contains nw");
                        nwIndex = vertices.IndexOf(nw, vertexIndex - indicesCountLastIter);
                        if (nwIndex < 0)
                        {
                            Debug.LogWarning("NW index at x = " + x + ", y = " + y + " has not been found");
                            tileVertices.Add(nw);
                            vertexIndex++;
                            indicesCountCurrentIter++;
                        }
                    }
                    else
                    {
                        tileVertices.Add(nw);
                        vertexIndex++;
                        indicesCountCurrentIter++;
                    }

                    tileVertices.Add(ne);
                    neIndex = vertexIndex;
                    vertexIndex++;
                    indicesCountCurrentIter++;

                    if (vertices.Contains(sw))
                    {
                        Debug.Log("contains sw");
                        swIndex = vertices.IndexOf(sw, vertexIndex - indicesCountLastIter);
                        if (swIndex < 0)
                        {
                            Debug.LogWarning("SW index at x = " + x + ", y = " + y + " has not been found");
                            tileVertices.Add(sw);
                            vertexIndex++;
                            indicesCountCurrentIter++;
                        }
                    }
                    else
                    {
                        tileVertices.Add(sw);
                        vertexIndex++;
                        indicesCountCurrentIter++;
                    }

                    tileVertices.Add(se);
                    seIndex = vertexIndex;
                    indicesCountCurrentIter++;
                }
                else
                {
                    tileVertices.AddRange(new Vector3[] { nw, ne, sw, se });
                    indicesCountCurrentIter += 4;
                }

                vertices.AddRange(tileVertices);
                normals.AddRange(NormalsUpArray(indicesCountCurrentIter));

                Vector2 uv = new Vector2(x / (float)tilesWidth, y / (float)tilesHeight);
                uvs.AddRange(UvsArray(uv, indicesCountCurrentIter));

                int[] triangleIndices = { nwIndex, neIndex, swIndex, neIndex, seIndex, swIndex };
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
                            //Debug.Log("vertices count " + vertexIndex);
                            //Debug.Log("normals count " + normals.Count);
                            //Debug.Log("uvs count " + uvs.Count);

                            // vertices from the west may be already done (both the upper and lower ones)
                            // we need to find them and get their indices
                            // then, add necessary indices only 
                            // and create triangles

                            // select vertices according to in which direction we are right now

                            // if looking north, we need to add nw + down & ne + down
                            // but then we need to make triangles using the west vertices, so we need to find them
                            //if (i == 0)
                            // THINK OF COUNTING SEPARETEDLY indexCounter for tile-vertices and for side-vertices
                            // that way we can distinguish between the two types of indices when searching for them

                            int vertSideIndexA = nsweSideVertIndex[i][0];
                            int vertSideIndexB = nsweSideVertIndex[i][1];
                            vertices.Add(tileVertices[vertSideIndexA]);
                            vertices.Add(tileVertices[vertSideIndexA] + Vector3.down * depth);
                            vertices.Add(tileVertices[vertSideIndexB]);
                            vertices.Add(tileVertices[vertSideIndexB] + Vector3.down * depth);
                            indicesCountCurrentIter += 4;

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

        //Debug.Log(vertices.Count);
        //Debug.Log(normals.Count);

        return new MeshData(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), normals.ToArray());
    }


    private static Vector3[] NormalsUpArray(int size)
    {
        var normals = new Vector3[size];
        for (int i = 0; i < size; i++)
        {
            normals[i] = Vector3.up;
        }
        return normals;
    }

    private static Vector2[] UvsArray(Vector2 uv, int size)
    {
        var uvs = new Vector2[size];
        for (int i = 0; i < size; i++)
        {
            uvs[i] = uv;
        }
        return uvs;
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
            /*
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, true);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.Optimize();
            mesh.RecalculateNormals();
            */
            return mesh;
        }
    }
}
