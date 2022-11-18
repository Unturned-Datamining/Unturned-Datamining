using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class PlayerClipVolumeUtility
{
    [Obsolete]
    public static bool isPointInsideVolume(Vector3 point)
    {
        return VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>.Get().IsPositionInsideAnyVolume(point);
    }
}
