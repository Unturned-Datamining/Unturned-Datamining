using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class NPCRewardVolumeManager : VolumeManager<NPCRewardVolume, NPCRewardVolumeManager>
{
    public NPCRewardVolumeManager()
    {
        base.FriendlyName = "NPC Reward";
        SetDebugColor(new Color32(220, 220, 20, byte.MaxValue));
    }
}
