using UnityEngine;

namespace SDG.Unturned;

public class PurchaseNode : Node
{
    public static readonly float MIN_SIZE = 2f;

    public static readonly float MAX_SIZE = 16f;

    internal float _normalizedRadius;

    public ushort id;

    public uint cost;

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

    public PurchaseNode(Vector3 newPoint)
        : this(newPoint, 0f, 0, 0u)
    {
    }

    public PurchaseNode(Vector3 newPoint, float newRadius, ushort newID, uint newCost)
    {
        _point = newPoint;
        _normalizedRadius = newRadius;
        id = newID;
        cost = newCost;
        _type = ENodeType.PURCHASE;
    }
}
