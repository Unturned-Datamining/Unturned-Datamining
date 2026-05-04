using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DeadzoneVolumeManager : VolumeManager<DeadzoneVolume, DeadzoneVolumeManager>
{
    public DeadzoneVolume GetMostDangerousOverlappingVolume(Vector3 position)
    {
        List<DeadzoneVolume> overlapTestVolumes = GetOverlapTestVolumes(position);
        if (overlapTestVolumes == null)
        {
            return null;
        }
        DeadzoneVolume deadzoneVolume = null;
        foreach (DeadzoneVolume item in overlapTestVolumes)
        {
            if ((deadzoneVolume == null || item.DeadzoneType > deadzoneVolume.DeadzoneType) && item.IsPositionInsideVolume(position))
            {
                deadzoneVolume = item;
                if (deadzoneVolume.DeadzoneType == EDeadzoneType.FullSuitRadiation)
                {
                    break;
                }
            }
        }
        return deadzoneVolume;
    }

    /// <summary>
    /// Hacked to check horizontal distance.
    /// </summary>
    public bool IsNavmeshCenterInsideAnyVolume(Vector3 position)
    {
        List<DeadzoneVolume> overlapTestVolumes = GetOverlapTestVolumes(position);
        if (overlapTestVolumes == null)
        {
            return false;
        }
        foreach (DeadzoneVolume item in overlapTestVolumes)
        {
            Vector3 position2 = new Vector3(position.x, item.transform.position.y, position.z);
            if (item.IsPositionInsideVolume(position2))
            {
                return true;
            }
        }
        return false;
    }

    public DeadzoneVolumeManager()
    {
        base.FriendlyName = "Deadzone";
        SetDebugColor(new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
        benefitsFromStaticVolumes = true;
    }
}
