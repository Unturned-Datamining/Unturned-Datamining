using UnityEngine;

namespace SDG.Unturned;

public static class LodGroupExtension
{
    /// <summary>
    /// Lod group will be culled when screen size is smaller than this value.
    /// </summary>
    public static float GetCullingScreenSize(this LODGroup lodGroup)
    {
        return lodGroup.GetLODs()[^1].screenRelativeTransitionHeight;
    }

    /// <summary>
    /// Clamp the culling screen percentage to be less than or equal to a maximum value.
    /// </summary>
    public static void ClampCulling(this LODGroup lodGroup, float max)
    {
        LOD[] lODs = lodGroup.GetLODs();
        int num = lODs.Length - 1;
        if (num <= lODs.Length && lODs[num].screenRelativeTransitionHeight > max)
        {
            lODs[num].screenRelativeTransitionHeight = max;
            lodGroup.SetLODs(lODs);
        }
    }

    /// <summary>
    /// Prevent the lowest LOD from being culled.
    /// </summary>
    public static void DisableCulling(this LODGroup lodGroup)
    {
        lodGroup.ClampCulling(0f);
    }
}
