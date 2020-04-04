using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColormap(Color[] colormap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colormap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colormap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colormap[y * width + x] = Color.Lerp(Color.blue, Color.red, heightMap[x, y]);
            }
        }

        return TextureFromColormap(colormap, width, height);
    }
}
