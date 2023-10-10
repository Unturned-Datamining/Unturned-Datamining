using UnityEngine;

namespace SDG.Unturned;

public static class ColorEx
{
    public static readonly Color BlackZeroAlpha = new Color(0f, 0f, 0f, 0f);

    public static readonly Color WhiteZeroAlpha = new Color(1f, 1f, 1f, 0f);

    public static bool IsNearlyBlack(this Color color, float tolerance = 0.001f)
    {
        return (Mathf.Abs(color.r) < tolerance) & (Mathf.Abs(color.g) < tolerance) & (Mathf.Abs(color.b) < tolerance);
    }

    public static bool IsNearlyWhite(this Color color, float tolerance = 0.001f)
    {
        return (Mathf.Abs(color.r - 1f) < tolerance) & (Mathf.Abs(color.g - 1f) < tolerance) & (Mathf.Abs(color.b - 1f) < tolerance);
    }

    public static Color WithAlpha(this Color color, float alphaOverride)
    {
        return new Color(color.r, color.g, color.b, alphaOverride);
    }
}
