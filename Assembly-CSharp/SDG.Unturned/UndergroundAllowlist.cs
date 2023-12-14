using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public static class UndergroundAllowlist
{
    /// <summary>
    /// If level is using underground allowlist then conditionally clamp world-space position.
    /// </summary>
    public static bool AdjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
    {
        if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
        {
            return false;
        }
        if (VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>.Get().IsPositionInsideAnyVolume(worldspacePosition))
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

    /// <summary>
    /// Used by animals and zombies to teleport to a spawnpoint if outside the map.
    /// </summary>
    public static bool IsPositionWithinValidHeight(Vector3 position, float threshold = 0.1f)
    {
        if (position.y < -1024f || position.y > 1024f)
        {
            return false;
        }
        if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
        {
            return true;
        }
        float height = LevelGround.getHeight(position);
        if (position.y > height - threshold)
        {
            return true;
        }
        return VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>.Get().IsPositionInsideAnyVolume(position);
    }

    /// <summary>
    /// Used by housing validation to check item isn't placed underground.
    /// </summary>
    public static bool IsPositionBuildable(Vector3 position)
    {
        if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
        {
            return true;
        }
        float height = LevelGround.getHeight(position);
        if (position.y > height)
        {
            return true;
        }
        return VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>.Get().IsPositionInsideAnyVolume(position);
    }
}
