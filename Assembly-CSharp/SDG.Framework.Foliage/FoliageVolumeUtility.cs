using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageVolumeUtility
{
    [Obsolete]
    public static bool isTileBakeable(FoliageTile tile)
    {
        return VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsTileBakeable(tile);
    }

    [Obsolete]
    public static bool isPointValid(Vector3 point, bool instancedMeshes, bool resources, bool objects)
    {
        return VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsPositionBakeable(point, instancedMeshes, resources, objects);
    }

    [Obsolete]
    public static bool isPointInsideVolume(FoliageVolume volume, Vector3 point)
    {
        return volume.IsPositionInsideVolume(point);
    }
}
