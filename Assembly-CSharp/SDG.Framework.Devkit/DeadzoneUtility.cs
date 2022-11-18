using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DeadzoneUtility
{
    [Obsolete]
    public static bool isPointInsideVolume(Vector3 point, out DeadzoneVolume volume)
    {
        volume = VolumeManager<DeadzoneVolume, DeadzoneVolumeManager>.Get().GetMostDangerousOverlappingVolume(point);
        return volume != null;
    }

    [Obsolete]
    public static bool isPointInsideVolume(DeadzoneVolume volume, Vector3 point)
    {
        return volume.IsPositionInsideVolume(point);
    }
}
