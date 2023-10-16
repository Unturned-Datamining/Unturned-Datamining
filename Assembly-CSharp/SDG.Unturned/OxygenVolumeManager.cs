using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class OxygenVolumeManager : VolumeManager<OxygenVolume, OxygenVolumeManager>
{
    internal List<OxygenVolume> breathableVolumes;

    internal List<OxygenVolume> nonBreathableVolumes;

    public bool IsPositionInsideBreathableVolume(Vector3 position, out float maxAlpha)
    {
        bool result = false;
        maxAlpha = 0f;
        foreach (OxygenVolume breathableVolume in breathableVolumes)
        {
            if (breathableVolume.IsPositionInsideVolumeWithAlpha(position, out var alpha))
            {
                result = true;
                maxAlpha = Mathf.Max(maxAlpha, alpha);
                if (maxAlpha > 0.9999f)
                {
                    maxAlpha = 1f;
                    break;
                }
            }
        }
        return result;
    }

    public bool IsPositionInsideNonBreathableVolume(Vector3 position, out float maxAlpha)
    {
        bool result = false;
        maxAlpha = 0f;
        foreach (OxygenVolume nonBreathableVolume in nonBreathableVolumes)
        {
            if (nonBreathableVolume.IsPositionInsideVolumeWithAlpha(position, out var alpha))
            {
                result = true;
                maxAlpha = Mathf.Max(maxAlpha, alpha);
                if (maxAlpha > 0.9999f)
                {
                    maxAlpha = 1f;
                    break;
                }
            }
        }
        return result;
    }

    public override void AddVolume(OxygenVolume volume)
    {
        base.AddVolume(volume);
        if (volume.isBreathable)
        {
            breathableVolumes.Add(volume);
        }
        else
        {
            nonBreathableVolumes.Add(volume);
        }
    }

    public override void RemoveVolume(OxygenVolume volume)
    {
        base.RemoveVolume(volume);
        if (volume.isBreathable)
        {
            breathableVolumes.RemoveFast(volume);
        }
        else
        {
            nonBreathableVolumes.RemoveFast(volume);
        }
    }

    public OxygenVolumeManager()
    {
        base.FriendlyName = "Oxygen";
        SetDebugColor(new Color32(110, 100, 110, byte.MaxValue));
        supportsFalloff = true;
        breathableVolumes = new List<OxygenVolume>();
        nonBreathableVolumes = new List<OxygenVolume>();
    }
}
