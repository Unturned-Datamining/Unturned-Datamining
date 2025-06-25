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

    /// <summary>
    /// Original width field is misleadingly named. It represents half the width of the flat section of the road.
    /// </summary>
    public float HalfWidth
    {
        get
        {
            return width;
        }
        set
        {
            width = value;
        }
    }

    /// <summary>
    /// Original depth field is misleadingly named. It represents half the "up" size of the road.
    /// </summary>
    public float HalfVerticalSize
    {
        get
        {
            return depth;
        }
        set
        {
            depth = value;
        }
    }

    /// <summary>
    /// Distance along the terrain surface normal to move each road vertex.
    /// </summary>
    public float VerticalOffset
    {
        get
        {
            return offset;
        }
        set
        {
            offset = value;
        }
    }

    public RoadMaterial(Texture2D texture)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            _material = new Material(shader);
            material.name = "Road";
            material.mainTexture = texture;
        }
        width = 4f;
        height = 1f;
        depth = 0.5f;
        offset = 0f;
        isConcrete = true;
    }
}
