using UnityEngine;

namespace SDG.Unturned;

public class SafezoneVolumeManager : VolumeManager<SafezoneVolume, SafezoneVolumeManager>
{
    public SafezoneVolumeManager()
    {
        base.FriendlyName = "Safezone";
        SetDebugColor(new Color32(205, 145, 205, byte.MaxValue));
    }
}
