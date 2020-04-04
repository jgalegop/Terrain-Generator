using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private int _mapWidth = 2;
    [SerializeField]
    private int _mapHeight = 2;
    [SerializeField]
    private float _noiseScale = 1;

    public bool AutoUpdate = true;

    [SerializeField]
    private MapDisplay mapDisplay = null;


    public enum DrawMode { NoiseMap, RegionColorMap };
    [SerializeField]
    private DrawMode _drawMode = DrawMode.NoiseMap;
    [SerializeField]
    private TerrainType[] _regions = null;


    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(_mapWidth, _mapHeight, _noiseScale);

        Color[] colormap = SetColorMap(noiseMap);

        // Display
        if (_drawMode == DrawMode.NoiseMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }   
        else if (_drawMode == DrawMode.RegionColorMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColormap(colormap, _mapWidth, _mapHeight));
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
                foreach (TerrainType region in _regions)
                {
                    if (currentHeight <= region.height)
                    {
                        colormap[y * _mapWidth + x] = region.color;
                        break;
                    }
                }
            }
        }
        return colormap;
    }
}


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
