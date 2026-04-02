using UnityEngine;

namespace SDG.Unturned;

public class SafezoneNode : Node
{
    public static readonly float MIN_SIZE = 32f;

    public static readonly float MAX_SIZE = 1024f;

    internal float _normalizedRadius;

    public bool isHeight;

    public bool noWeapons;

    /// <summary>
    /// Please check CurrentlyAllowsBuilding.
    /// Bypassed by LevelAsset's ShouldAllowBuildingInSafezonesInSingleplayer option as well as
    /// Gameplay config's Bypass_Building_In_Safezones option.
    /// </summary>
    public bool noBuildables;

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

    public bool CurrentlyAllowsBuilding
    {
        get
        {
            if (noBuildables && !(Provider.modeConfigData?.Gameplay?.Bypass_Building_In_Safezones).GetValueOrDefault())
            {
                LevelAsset asset = Level.getAsset();
                if (asset != null && asset.ShouldAllowBuildingInSafezonesInSingleplayer && Provider.isServer)
                {
                    return !Dedicator.IsDedicatedServer;
                }
                return false;
            }
            return true;
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
