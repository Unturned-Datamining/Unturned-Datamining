using UnityEngine;

namespace SDG.Unturned;

public class ArenaCompactorVolumeManager : VolumeManager<ArenaCompactorVolume, ArenaCompactorVolumeManager>
{
    public ArenaCompactorVolumeManager()
    {
        base.FriendlyName = "Arena Mode Compactor";
        SetDebugColor(new Color32(20, 20, 20, byte.MaxValue));
    }
}
