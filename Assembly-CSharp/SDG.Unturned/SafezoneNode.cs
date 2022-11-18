using UnityEngine;

namespace SDG.Unturned;

public class SafezoneNode : Node
{
    public static readonly float MIN_SIZE = 32f;

    public static readonly float MAX_SIZE = 1024f;

    internal float _normalizedRadius;

    public bool isHeight;

    public bool noWeapons;

    public bool noBuildables;

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

    public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
    {
        return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
    }

    public static float CalculateNormalizedRadiusFromRadius(float radius)
    {
        return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2f);
    }

    public SafezoneNode(Vector3 newPoint)
        : this(newPoint, 0f, newHeight: false, newNoWeapons: true, newNoBuildables: true)
    {
    }

    public SafezoneNode(Vector3 newPoint, float newRadius, bool newHeight, bool newNoWeapons, bool newNoBuildables)
    {
        _point = newPoint;
        _normalizedRadius = newRadius;
        isHeight = newHeight;
        noWeapons = newNoWeapons;
        noBuildables = newNoBuildables;
        _type = ENodeType.SAFEZONE;
    }
}
