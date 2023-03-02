using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Water;

public class WaterUtility
{
    [Obsolete]
    public static bool isPointInsideVolume(Vector3 point)
    {
        return VolumeManager<WaterVolume, WaterVolumeManager>.Get().IsPositionInsideAnyVolume(point);
    }

    [Obsolete]
    public static bool isPointInsideVolume(Vector3 point, out WaterVolume volume)
    {
        volume = VolumeManager<WaterVolume, WaterVolumeManager>.Get().GetFirstOverlappingVolume(point);
        return volume != null;
    }

    public static float getWaterSurfaceElevation(WaterVolume volume, Vector3 point)
    {
        point.y += 1024f;
        Ray ray = new Ray(point, new Vector3(0f, -1f, 0f));
        if (volume.volumeCollider.Raycast(ray, out var hitInfo, 2048f))
        {
            return hitInfo.point.y;
        }
        return 0f;
    }

    [Obsolete]
    public static bool isPointInsideVolume(WaterVolume volume, Vector3 point)
    {
        return volume.IsPositionInsideVolume(point);
    }

    public static bool isPointUnderwater(Vector3 point)
    {
        return VolumeManager<WaterVolume, WaterVolumeManager>.Get().IsPositionInsideAnyVolume(point);
    }

    public static bool isPointUnderwater(Vector3 point, out WaterVolume volume)
    {
        volume = VolumeManager<WaterVolume, WaterVolumeManager>.Get().GetFirstOverlappingVolume(point);
        return volume != null;
    }

    public static float getWaterSurfaceElevation(Vector3 point)
    {
        bool flag = false;
        float num = -1024f;
        foreach (WaterVolume item in VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes())
        {
            if (item.IsPositionInsideVolume(point))
            {
                return getWaterSurfaceElevation(item, point);
            }
            Ray ray = new Ray(point, new Vector3(0f, -1f, 0f));
            if (item.volumeCollider.Raycast(ray, out var hitInfo, 2048f) && hitInfo.point.y > num)
            {
                num = hitInfo.point.y;
                flag = true;
            }
        }
        if (flag)
        {
            return num;
        }
        return -1024f;
    }

    public static void getUnderwaterInfo(Vector3 point, out bool isUnderwater, out float surfaceElevation)
    {
        WaterVolume firstOverlappingVolume = VolumeManager<WaterVolume, WaterVolumeManager>.Get().GetFirstOverlappingVolume(point);
        if (firstOverlappingVolume != null)
        {
            isUnderwater = true;
            surfaceElevation = getWaterSurfaceElevation(firstOverlappingVolume, point);
        }
        else
        {
            isUnderwater = false;
            surfaceElevation = -1024f;
        }
    }
}
