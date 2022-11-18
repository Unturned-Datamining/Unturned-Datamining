using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class NavClipVolumeManager : VolumeManager<NavClipVolume, NavClipVolumeManager>
{
    public NavClipVolumeManager()
    {
        base.FriendlyName = "Navmesh Clip";
        SetDebugColor(new Color32(63, 63, 0, byte.MaxValue));
    }
}
