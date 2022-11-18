using UnityEngine;

namespace SDG.Unturned;

public class ArenaNode : Node
{
    public static readonly float MIN_SIZE = 128f;

    public static readonly float MAX_SIZE = 8192f;

    internal float _normalizedRadius;

    public float radius
    {
        get
        {
            if (Level.isEditor)
            {
                return _normalizedRadius;
            }
            return CalculateRadiusFromNormalizedRadius(_normalizedRadius);
        }
        set
        {
            _normalizedRadius = value;
        }
    }

    public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
    {
        return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
    }

    public static float CalculateNormalizedRadiusFromRadius(float radius)
    {
        return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2f);
    }

    public ArenaNode(Vector3 newPoint)
        : this(newPoint, 0f)
    {
    }

    public ArenaNode(Vector3 newPoint, float newRadius)
    {
        _point = newPoint;
        _normalizedRadius = newRadius;
        _type = ENodeType.ARENA;
    }
}
