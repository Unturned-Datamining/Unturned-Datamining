using UnityEngine;

namespace SDG.Unturned;

public class RoadMaterial
{
    private static Shader _shader;

    private Material _material;

    public float width;

    public float height;

    public float depth;

    public float offset;

    public bool isConcrete;

    public static Shader shader
    {
        get
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Standard/Diffuse");
                if (_shader == null)
                {
                    UnturnedLog.error("Road Standard/Diffuse shader is missing!");
                }
            }
            return _shader;
        }
    }

    public Material material => _material;

    public RoadMaterial(Texture2D texture)
    {
        _material = new Material(shader);
        material.name = "Road";
        material.mainTexture = texture;
        width = 4f;
        height = 1f;
        depth = 0.5f;
        offset = 0f;
        isConcrete = true;
    }
}
