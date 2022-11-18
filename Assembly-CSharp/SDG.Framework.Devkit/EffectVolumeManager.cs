using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class EffectVolumeManager : VolumeManager<EffectVolume, EffectVolumeManager>
{
    public EffectVolumeManager()
    {
        base.FriendlyName = "Effect";
        SetDebugColor(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
    }
}
