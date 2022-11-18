using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageVolumeManager : VolumeManager<FoliageVolume, FoliageVolumeManager>
{
    internal List<FoliageVolume> additiveVolumes;

    internal List<FoliageVolume> subtractiveVolumes;

    public bool IsTileBakeable(FoliageTile tile)
    {
        if (additiveVolumes.Count > 0)
        {
            Vector3 center = tile.worldBounds.center;
            for (int i = 0; i < additiveVolumes.Count; i++)
            {
                if (additiveVolumes[i].IsPositionInsideVolume(center))
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    public bool IsPositionBakeable(Vector3 point, bool instancedMeshes, bool resources, bool objects)
    {
        bool flag;
        if (additiveVolumes.Count > 0)
        {
            flag = false;
            for (int i = 0; i < additiveVolumes.Count; i++)
            {
                FoliageVolume foliageVolume = additiveVolumes[i];
                if ((!instancedMeshes || foliageVolume.instancedMeshes) && (!resources || foliageVolume.resources) && (!objects || foliageVolume.objects) && foliageVolume.IsPositionInsideVolume(point))
                {
                    flag = true;
                    break;
                }
            }
        }
        else
        {
            flag = true;
        }
        if (!flag)
        {
            return false;
        }
        for (int j = 0; j < subtractiveVolumes.Count; j++)
        {
            FoliageVolume foliageVolume2 = subtractiveVolumes[j];
            if ((!instancedMeshes || foliageVolume2.instancedMeshes) && (!resources || foliageVolume2.resources) && (!objects || foliageVolume2.objects) && foliageVolume2.IsPositionInsideVolume(point))
            {
                return false;
            }
        }
        return true;
    }

    public override void AddVolume(FoliageVolume volume)
    {
        base.AddVolume(volume);
        if (volume.mode == FoliageVolume.EFoliageVolumeMode.ADDITIVE)
        {
            additiveVolumes.Add(volume);
        }
        else
        {
            subtractiveVolumes.Add(volume);
        }
    }

    public override void RemoveVolume(FoliageVolume volume)
    {
        base.RemoveVolume(volume);
        if (volume.mode == FoliageVolume.EFoliageVolumeMode.ADDITIVE)
        {
            additiveVolumes.RemoveFast(volume);
        }
        else
        {
            subtractiveVolumes.RemoveFast(volume);
        }
    }

    public FoliageVolumeManager()
    {
        base.FriendlyName = "Foliage";
        additiveVolumes = new List<FoliageVolume>();
        subtractiveVolumes = new List<FoliageVolume>();
        SetDebugColor(new Color32(44, 114, 34, byte.MaxValue));
    }
}
