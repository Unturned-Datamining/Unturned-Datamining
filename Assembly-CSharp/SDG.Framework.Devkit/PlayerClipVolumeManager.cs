using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class PlayerClipVolumeManager : VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>
{
    public PlayerClipVolumeManager()
    {
        base.FriendlyName = "Player Clip";
        SetDebugColor(new Color32(63, 0, 0, byte.MaxValue));
    }
}
