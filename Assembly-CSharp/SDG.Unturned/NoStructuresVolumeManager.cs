using UnityEngine;

namespace SDG.Unturned;

public class NoStructuresVolumeManager : VolumeManager<NoStructuresVolume, NoStructuresVolumeManager>
{
    public NoStructuresVolumeManager()
    {
        base.FriendlyName = "No Structures";
        SetDebugColor(new Color32(150, 125, 175, byte.MaxValue));
        benefitsFromStaticVolumes = true;
    }
}
