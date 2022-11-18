using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class UndergroundWhitelistVolumeManager : VolumeManager<UndergroundWhitelistVolume, UndergroundWhitelistVolumeManager>
{
    public UndergroundWhitelistVolumeManager()
    {
        base.FriendlyName = "Underground Whitelist";
        SetDebugColor(new Color32(63, 63, 63, byte.MaxValue));
    }
}
