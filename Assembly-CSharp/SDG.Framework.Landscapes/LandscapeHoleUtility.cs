using System;
using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public class LandscapeHoleUtility
{
    [Obsolete("New code should probably be using Landscape.IsPointInsideHole")]
    public static bool isPointInsideHoleVolume(Vector3 point)
    {
        LandscapeHoleVolume volume;
        return isPointInsideHoleVolume(point, out volume);
    }

    [Obsolete("New code should probably be using Landscape.IsPointInsideHole")]
    public static bool isPointInsideHoleVolume(Vector3 point, out LandscapeHoleVolume volume)
    {
        List<LandscapeHoleVolume> list = VolumeManager<LandscapeHoleVolume, LandscapeHoleVolumeManager>.Get().InternalGetAllVolumes();
        for (int i = 0; i < list.Count; i++)
        {
            volume = list[i];
            if (isPointInsideHoleVolume(volume, point))
            {
                return true;
            }
        }
        volume = null;
        return false;
    }

    [Obsolete]
    public static bool isPointInsideHoleVolume(LandscapeHoleVolume volume, Vector3 point)
    {
        return volume.IsPositionInsideVolume(point);
    }

    [Obsolete]
    public static bool doesRayIntersectHoleVolume(Ray ray, out RaycastHit hit, out LandscapeHoleVolume volume, float maxDistance)
    {
        return VolumeManager<LandscapeHoleVolume, LandscapeHoleVolumeManager>.Get().Raycast(ray, out hit, out volume, maxDistance);
    }

    [Obsolete]
    public static bool doesRayIntersectHoleVolume(LandscapeHoleVolume volume, Ray ray, out RaycastHit hit, float maxDistance)
    {
        return volume.volumeCollider.Raycast(ray, out hit, maxDistance);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static bool shouldRaycastIgnoreLandscape(Ray ray, float maxDistance)
    {
        if (doesRayIntersectHoleVolume(ray, out var _, out var volume, maxDistance) && Physics.Raycast(ray, out var hitInfo, maxDistance, 1048576) && isPointInsideHoleVolume(volume, hitInfo.point))
        {
            return true;
        }
        if (isPointInsideHoleVolume(ray.origin, out volume) && Physics.Raycast(ray, out var hitInfo2, maxDistance, 1048576) && isPointInsideHoleVolume(volume, hitInfo2.point))
        {
            return true;
        }
        return false;
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static bool shouldSpherecastIgnoreLandscape(Ray ray, float radius, float maxDistance)
    {
        ray.origin -= ray.direction * radius;
        maxDistance += radius * 2f;
        return shouldRaycastIgnoreLandscape(ray, maxDistance);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static void raycastIgnoreLandscapeIfNecessary(Ray ray, float maxDistance, ref int layerMask)
    {
        if (shouldRaycastIgnoreLandscape(ray, maxDistance))
        {
            layerMask &= -1048577;
        }
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static void spherecastIgnoreLandscapeIfNecessary(Ray ray, float radius, float maxDistance, ref int layerMask)
    {
        if (shouldSpherecastIgnoreLandscape(ray, radius, maxDistance))
        {
            layerMask &= -1048577;
        }
    }
}
