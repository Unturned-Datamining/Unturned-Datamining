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

    [Obsolete("Renamed to UndergroundAllowlist.AdjustPosition")]
    public static bool adjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
    {
        return UndergroundAllowlist.AdjustPosition(ref worldspacePosition, offset, threshold);
    }
}
