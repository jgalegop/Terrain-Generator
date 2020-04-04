using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer _textureRender = null;

    public void DrawTexture(Texture2D texture)
    {
        _textureRender.sharedMaterial.mainTexture = texture;
        _textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
