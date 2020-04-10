﻿using UnityEngine;
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
        Vector3[] tileNormals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        Vector2Int[] nswe = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int[][] nsweSideVertIndex = { new int[] { 3, 2 }, new int[] { 0, 1 }, new int[] { 2, 0 }, new int[] { 1, 3 } };
        Vector3[] nsweNormals = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };


        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();



        float uFraction = 1f / (float)tilesWidth;
        float vFraction = 1f / (float)tilesHeight;



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
                int neIndex, swIndex, seIndex;

                List<Vector2> tileUvs = new List<Vector2>();

                Vector2 uvnw = new Vector2((float)x * uFraction, (float)y * vFraction);
                Vector2 uvne = new Vector2((float)(x + 1) * uFraction, (float)y * vFraction);
                Vector2 uvsw = new Vector2((float)x * uFraction, (float)(y + 1) * vFraction);
                Vector2 uvse = new Vector2((float)(x + 1) * uFraction, (float)(y + 1) * vFraction);

                bool isEdgeTile = x == 0 || y == 0 || x == tilesWidth - 1 || y == tilesHeight - 1;
                
                if (true)
                {
                    if (vertices.Contains(nw))
                    {
                        //Debug.Log("contains nw");
                        nwIndex = vertices.IndexOf(nw);
                        if (nwIndex < 0)
                        {
                            // This can be erased once we get everything working
                            Debug.LogWarning("NW index at x = " + x + ", y = " + y + " has not been found. You are in trouble.");
                            tileVertices.Add(nw);
                            vertexIndex++;
                            indicesCountCurrentIter++;
                        }
                    }
                    else
                    {
                        tileVertices.Add(nw);
                        tileUvs.Add(uvnw);
                        vertexIndex++;
                        indicesCountCurrentIter++;
                    }

                    if (vertices.Contains(ne))
                    {
                        //Debug.Log("contains ne");
                        neIndex = vertices.IndexOf(ne);
                        if (neIndex < 0)
                        {
                            // This can be erased once we get everything working
                            Debug.LogWarning("NE index at x = " + x + ", y = " + y + " has not been found. You are in trouble.");
                            tileVertices.Add(ne);
                            vertexIndex++;
                            indicesCountCurrentIter++;
                        }
                    }
                    else
                    {
                        tileVertices.Add(ne);
                        tileUvs.Add(uvne);
                        neIndex = vertexIndex;
                        vertexIndex++;
                        indicesCountCurrentIter++;
                    }

                    if (vertices.Contains(sw))
                    {
                        //Debug.Log("contains sw");
                        swIndex = vertices.IndexOf(sw, vertexIndex - indicesCountLastIter - indicesCountCurrentIter);
                        if (swIndex < 0)
                        {
                            Debug.LogWarning("SW index at x = " + x + ", y = " + y + " has not been found. You are in trouble.");
                            tileVertices.Add(sw);
                            vertexIndex++;
                            indicesCountCurrentIter++;
                        }
                    }
                    else
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
                }
                else
                {
                    tileVertices.AddRange(new Vector3[] { nw, ne, sw, se });
                    tileUvs.AddRange(new Vector2[] { uvnw, uvne, uvsw, uvse});
                    indicesCountCurrentIter += 4;
                }

                vertices.AddRange(tileVertices);
                normals.AddRange(NormalsUpArray(indicesCountCurrentIter));
                uvs.AddRange(tileUvs);

                int[] triangleIndices = { nwIndex, neIndex, swIndex, neIndex, seIndex, swIndex };
                triangles.AddRange(triangleIndices);

                
                //bool isEdgeTile = x == 0 || y == 0 || x == tilesWidth - 1 || y == tilesHeight - 1;

                /*
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

                            if (i == 0)
                                uvs.AddRange(new Vector2[] { uvnw, uvnw, uvne, uvne });
                            else if (i == 1)
                                uvs.AddRange(new Vector2[] { uvse, uvse, uvsw, uvsw });
                            else if (i == 2)
                                uvs.AddRange(new Vector2[] { uvsw, uvsw, uvnw, uvnw });
                            else
                                uvs.AddRange(new Vector2[] { uvne, uvne, uvse, uvse });

                            //uvs.AddRange(new Vector2[] { uv, uv, uv, uv });

                            triangleIndices = new int[]{ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3, vertexIndex + 2 };
                            triangles.AddRange(triangleIndices);

                            Vector3[] sideNormals = new Vector3[] { nsweNormals[i], nsweNormals[i], nsweNormals[i], nsweNormals[i] };
                            normals.AddRange(sideNormals);
                        }
                    }
                }
                
                */
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


    // POSSIBLY DELETE
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
