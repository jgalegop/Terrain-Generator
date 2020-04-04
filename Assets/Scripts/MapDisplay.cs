using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer _textureRender = null;

    [SerializeField]
    private MeshFilter _meshFilter = null;
    [SerializeField]
    private MeshRenderer _meshRenderer = null;

    public void DrawTexture(Texture2D texture)
    {
        _textureRender.sharedMaterial.mainTexture = texture;
        _textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshGenerator.MeshData meshData, Texture2D texture)
    {
        _meshFilter.sharedMesh = meshData.CreateMesh();
        _meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
