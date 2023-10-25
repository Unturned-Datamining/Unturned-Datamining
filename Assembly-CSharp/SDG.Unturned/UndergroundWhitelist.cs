using System;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

[Obsolete("Renamed to UndergroundAllowlist")]
public static class UndergroundWhitelist
{
    public static bool isPointInsideVolume(Vector3 worldspacePosition)
    {
        return VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>.Get().IsPositionInsideAnyVolume(worldspacePosition);
    }

    /// <summary>
    /// If level is using underground whitelist then conditionally clamp world-space position.
    /// </summary>
    [Obsolete("Renamed to UndergroundAllowlist.AdjustPosition")]
    public static bool adjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
    {
        return UndergroundAllowlist.AdjustPosition(ref worldspacePosition, offset, threshold);
    }
}
