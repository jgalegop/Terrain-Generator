using System.Collections;
using System.Collections.Generic;
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

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(_mapWidth, _mapHeight, _noiseScale);

        // Display
        mapDisplay.DrawNoiseMap(noiseMap); 
    }
}
