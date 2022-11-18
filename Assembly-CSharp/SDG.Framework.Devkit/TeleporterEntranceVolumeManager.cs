using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class TeleporterEntranceVolumeManager : VolumeManager<TeleporterEntranceVolume, TeleporterEntranceVolumeManager>
{
    protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
    {
        base.OnUpdateGizmos(runtimeGizmos);
        foreach (TeleporterEntranceVolume allVolume in allVolumes)
        {
            Color color = (allVolume.isSelected ? Color.yellow : debugColor);
            runtimeGizmos.Arrow(allVolume.transform.position, allVolume.transform.forward, 1f, color);
            if (string.IsNullOrEmpty(allVolume.pairId) || !VolumeManager<TeleporterExitVolume, TeleporterExitVolumeManager>.Get().idToVolumes.TryGetValue(allVolume.pairId, out var value))
            {
                continue;
            }
            foreach (TeleporterExitVolume item in value)
            {
                runtimeGizmos.Line(allVolume.transform.position, item.transform.position, color);
            }
        }
    }

    public TeleporterEntranceVolumeManager()
    {
        base.FriendlyName = "Teleporter Entrance";
        SetDebugColor(new Color32(0, 0, byte.MaxValue, byte.MaxValue));
    }
}
