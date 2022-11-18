using UnityEngine;

namespace SDG.Unturned;

public class HordePurchaseVolumeManager : VolumeManager<HordePurchaseVolume, HordePurchaseVolumeManager>
{
    public HordePurchaseVolumeManager()
    {
        base.FriendlyName = "Horde Purchase";
        SetDebugColor(new Color32(20, 50, 20, byte.MaxValue));
    }
}
