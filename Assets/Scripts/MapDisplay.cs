using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer _textureRender = null;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colormap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colormap[y * width + x] = Color.Lerp(Color.blue, Color.red, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colormap);
        texture.Apply();

        _textureRender.sharedMaterial.mainTexture = texture;
        _textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
