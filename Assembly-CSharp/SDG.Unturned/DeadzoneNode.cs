using UnityEngine;

namespace SDG.Unturned;

public class DeadzoneNode : Node, IDeadzoneNode
{
    public static readonly float MIN_SIZE = 32f;

    public static readonly float MAX_SIZE = 1024f;

    internal float _normalizedRadius;

    private EDeadzoneType _deadzoneType;

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

    public EDeadzoneType DeadzoneType
    {
        get
        {
            return _deadzoneType;
        }
        set
        {
            _deadzoneType = value;
        }
    }

    /// <summary>
    /// Nelson 2024-06-10: Added this property after nodes were converted to volumes. i.e., only old levels from
    /// before this property were added still have nodes, so it's expected that they won't deal damage over time.
    /// </summary>
    public float UnprotectedDamagePerSecond => 0f;

    /// <summary>
    /// Same description as <see cref="P:SDG.Unturned.DeadzoneNode.UnprotectedDamagePerSecond" />.
    /// </summary>
    public float ProtectedDamagePerSecond => 0f;

    /// <summary>
    /// Same description as <see cref="P:SDG.Unturned.DeadzoneNode.UnprotectedDamagePerSecond" />.
    /// </summary>
    public float UnprotectedRadiationPerSecond => 6.25f;

    /// <summary>
    /// Same description as <see cref="P:SDG.Unturned.DeadzoneNode.UnprotectedDamagePerSecond" />.
    /// </summary>
    public float MaskFilterDamagePerSecond => 0.4f;

    public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
    {
        return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
    }

    public static float CalculateNormalizedRadiusFromRadius(float radius)
    {
        return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2f);
    }

    public DeadzoneNode(Vector3 newPoint)
        : this(newPoint, 0f, EDeadzoneType.DefaultRadiation)
    {
    }

    public DeadzoneNode(Vector3 newPoint, float newRadius, EDeadzoneType newDeadzoneType)
    {
        _point = newPoint;
        _deadzoneType = newDeadzoneType;
        _normalizedRadius = newRadius;
        _type = ENodeType.DEADZONE;
    }
}
