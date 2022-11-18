using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DeadzoneVolumeManager : VolumeManager<DeadzoneVolume, DeadzoneVolumeManager>
{
    public DeadzoneVolume GetMostDangerousOverlappingVolume(Vector3 position)
    {
        DeadzoneVolume deadzoneVolume = null;
        foreach (DeadzoneVolume allVolume in allVolumes)
        {
            if ((deadzoneVolume == null || allVolume.DeadzoneType > deadzoneVolume.DeadzoneType) && allVolume.IsPositionInsideVolume(position))
            {
                deadzoneVolume = allVolume;
                if (deadzoneVolume.DeadzoneType == EDeadzoneType.FullSuitRadiation)
                {
                    return deadzoneVolume;
                }
            }
        }
        return deadzoneVolume;
    }

    public bool IsNavmeshCenterInsideAnyVolume(Vector3 position)
    {
        foreach (DeadzoneVolume allVolume in allVolumes)
        {
            Vector3 position2 = new Vector3(position.x, allVolume.transform.position.y, position.z);
            if (allVolume.IsPositionInsideVolume(position2))
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
    }
}
