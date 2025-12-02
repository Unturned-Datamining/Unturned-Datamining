using UnityEngine;

namespace SDG.Unturned;

public static class IArmorFalloffEx
{
    /// <summary>
    /// Should hitmarker be shown client-side for a given range?
    /// </summary>
    public static bool DoesArmorFalloffShowHitmarker(this IArmorFalloff instance, float distance)
    {
        if (!(instance.ArmorFalloffMaxRange < -0.5f) && !(instance.ArmorFalloffMultiplier > 1E-05f))
        {
            return distance < instance.ArmorFalloffMaxRange;
        }
        return true;
    }

    /// <summary>
    /// Amount to multiply damage by at a given range.
    /// </summary>
    public static float GetArmorFalloffMultiplier(this IArmorFalloff instance, float distance)
    {
        if (instance.ArmorFalloffMaxRange < -0.5f)
        {
            return 1f;
        }
        float t = Mathf.InverseLerp(instance.ArmorFalloffRange, instance.ArmorFalloffMaxRange, distance);
        return Mathf.Lerp(1f, instance.ArmorFalloffMultiplier, t);
    }

    public static void PopulateArmorFalloff(this IArmorFalloff instance, in PopulateAssetParameters p)
    {
        instance.ArmorFalloffMaxRange = p.data.ParseFloat("Armor_FalloffMaxRange", -1f);
        instance.ArmorFalloffRange = p.data.ParseFloat("Armor_FalloffRange", instance.ArmorFalloffMaxRange);
        instance.ArmorFalloffMultiplier = p.data.ParseFloat("Armor_FalloffMultiplier", 1f);
    }
}
