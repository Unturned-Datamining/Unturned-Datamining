using UnityEngine;

namespace SDG.Unturned;

public class EffectNode : Node, IAmbianceNode
{
    public static readonly float MIN_SIZE = 8f;

    public static readonly float MAX_SIZE = 256f;

    internal float _normalizedRadius;

    private Vector3 _bounds;

    private ENodeShape _shape;

    /// <summary>
    /// This value is confusing because in the level editor it is the normalized radius, but in-game it is the square radius.
    /// </summary>
    public float radius
    {
        get
        {
            if (Level.isEditor)
            {
                return _normalizedRadius;
            }
            return MathfEx.Square(CalculateRadiusFromNormalizedRadius(_normalizedRadius));
        }
        set
        {
            _normalizedRadius = value;
        }
    }

    public float editorRadius => MathfEx.Square(CalculateRadiusFromNormalizedRadius(_normalizedRadius));

    public Vector3 bounds
    {
        get
        {
            return _bounds;
        }
        set
        {
            _bounds = value;
        }
    }

    public ENodeShape shape
    {
        get
        {
            return _shape;
        }
        set
        {
            _shape = value;
        }
    }

    public ushort id { get; set; }

    public bool noWater { get; set; }

    public bool noLighting { get; set; }

    public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
    {
        return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
    }

    public static float CalculateNormalizedRadiusFromRadius(float radius)
    {
        return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2f);
    }

    public EffectAsset GetEffectAsset()
    {
        return Assets.find(EAssetType.EFFECT, id) as EffectAsset;
    }

    public EffectNode(Vector3 newPoint)
        : this(newPoint, ENodeShape.SPHERE, 0f, Vector3.one, 0, newNoWater: false, newNoLighting: false)
    {
    }

    public EffectNode(Vector3 newPoint, ENodeShape newShape, float newRadius, Vector3 newBounds, ushort newID, bool newNoWater, bool newNoLighting)
    {
        _point = newPoint;
        shape = newShape;
        _normalizedRadius = newRadius;
        bounds = newBounds;
        id = newID;
        noWater = newNoWater;
        noLighting = newNoLighting;
        _type = ENodeType.EFFECT;
    }
}
