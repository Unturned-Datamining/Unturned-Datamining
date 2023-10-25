using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Overrides breathability for example in a deep cave with no oxygen, or near a deep sea plant that provides oxygen.
/// </summary>
public class OxygenVolumeManager : VolumeManager<OxygenVolume, OxygenVolumeManager>
{
    internal List<OxygenVolume> breathableVolumes;

    internal List<OxygenVolume> nonBreathableVolumes;

    /// <summary>
    /// Find highest alpha breathable volume overlapping position.
    /// </summary>
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

    /// <summary>
    /// Find highest alpha non-breathable volume overlapping position.
    /// </summary>
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
