﻿using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map parameters")]
    [SerializeField]
    private int _mapWidth = 2;
    [SerializeField]
    private int _mapHeight = 2;

    [SerializeField]
    private int _seed = 0;
    [SerializeField]
    private Vector2 _offset = Vector2.zero;

    [SerializeField]
    private float _noiseScale = 1;

    [SerializeField]
    private int _octaves = 1;
    [SerializeField] [Range(0, 1)]
    private float _persistance = 0.5f;
    [SerializeField]
    private float _lacunarity = 2f;

    [Header("Map generator display")]
    public bool AutoUpdate = true;

    [SerializeField]
    private MapDisplay _mapDisplay = null;

    [SerializeField]
    private bool _generateObstacles = false;


    public enum DrawMode { NoiseMap, RegionColorMap, Mesh, Mesh3D };
    [SerializeField]
    private DrawMode _drawMode = DrawMode.NoiseMap;
    [SerializeField]
    private TerrainType[] _regions = null;

    private void Awake()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(_mapWidth, _mapHeight, _seed, _noiseScale, _octaves, _persistance, _lacunarity, _offset);

        Color[] colormap = SetColorMap(noiseMap);

        // Display
        if (_drawMode == DrawMode.NoiseMap)
        {
            _mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }   
        else if (_drawMode == DrawMode.RegionColorMap)
        {
            _mapDisplay.DrawTexture(TextureGenerator.TextureFromColormap(colormap, _mapWidth, _mapHeight));
        }
        else if (_drawMode == DrawMode.Mesh)
        {
            var meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, _regions[0].height);
            _mapDisplay.DrawMesh(meshData, TextureGenerator.TextureFromColormap(colormap, _mapWidth, _mapHeight));
            if (_generateObstacles)
                ObstacleGenerator.GenerateObstacleMesh(meshData);
        }
        else if (_drawMode == DrawMode.Mesh3D)
        {
            _mapDisplay.DrawMesh3D(MeshGenerator3D.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColormap(colormap, _mapWidth, _mapHeight));
        }
        
    }

    private Color[] SetColorMap(float[,] noiseMap)
    {
        Color[] colormap = new Color[_mapWidth * _mapHeight];
        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];

                float lowerHeight = 0;
                for (int i = 0; i < _regions.Length; i++)
                {
                    TerrainType region = _regions[i];

                    if (currentHeight <= region.height)
                    {
                        if (i > 0)
                            lowerHeight = _regions[i - 1].height;

                        float t = Mathf.InverseLerp(lowerHeight, region.height, currentHeight);
                        Color regionColor = Color.Lerp(region.startColor, region.endColor, t);
                        colormap[y * _mapWidth + x] = regionColor;
                        break;
                    }
                }
            }
        }
        return colormap;
    }

    private void OnValidate()
    {
        if (_mapHeight < 1)
            _mapHeight = 1;
        if (_mapWidth < 1)
            _mapWidth = 1;
        if (_lacunarity < 1)
            _lacunarity = 1;
        if (_octaves < 0)
            _octaves = 0;
    }
}


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color startColor;
    public Color endColor;
}
