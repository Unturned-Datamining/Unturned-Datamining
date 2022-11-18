using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class AmbianceUtility
{
    [Obsolete]
    public static bool isPointInsideVolume(Vector3 point, out AmbianceVolume volume)
    {
        volume = VolumeManager<AmbianceVolume, AmbianceVolumeManager>.Get().GetFirstOverlappingVolume(point);
        return volume != null;
    }

    [Obsolete]
    public static bool isPointInsideVolume(AmbianceVolume volume, Vector3 point)
    {
        return volume.IsPositionInsideVolume(point);
    }
}
