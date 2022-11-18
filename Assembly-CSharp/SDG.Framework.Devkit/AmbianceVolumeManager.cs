using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class AmbianceVolumeManager : VolumeManager<AmbianceVolume, AmbianceVolumeManager>
{
    public AmbianceVolumeManager()
    {
        base.FriendlyName = "Ambiance";
        SetDebugColor(new Color32(0, 127, 127, byte.MaxValue));
    }
}
