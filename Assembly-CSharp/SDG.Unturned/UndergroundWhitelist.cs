using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public static class UndergroundWhitelist
{
    public static bool isPointInsideVolume(Vector3 worldspacePosition)
    {
        return VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>.Get().IsPositionInsideAnyVolume(worldspacePosition);
    }

    public static bool adjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
    {
        if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
        {
            return false;
        }
        if (isPointInsideVolume(worldspacePosition))
        {
            return false;
        }
        float height = LevelGround.getHeight(worldspacePosition);
        if (worldspacePosition.y < height - threshold)
        {
            worldspacePosition.y = height + offset;
            return true;
        }
        return false;
    }
}
