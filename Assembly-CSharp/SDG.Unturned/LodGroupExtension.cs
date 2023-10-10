using UnityEngine;

namespace SDG.Unturned;

public static class LodGroupExtension
{
    public static float GetCullingScreenSize(this LODGroup lodGroup)
    {
        return lodGroup.GetLODs()[^1].screenRelativeTransitionHeight;
    }

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

    public static void DisableCulling(this LODGroup lodGroup)
    {
        lodGroup.ClampCulling(0f);
    }
}
