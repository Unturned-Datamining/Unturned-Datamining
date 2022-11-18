using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class KillVolumeManager : VolumeManager<KillVolume, KillVolumeManager>
{
    public KillVolumeManager()
    {
        base.FriendlyName = "Kill";
        SetDebugColor(new Color32(220, 100, 20, byte.MaxValue));
    }
}
