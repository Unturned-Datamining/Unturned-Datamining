using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class TeleporterExitVolumeManager : VolumeManager<TeleporterExitVolume, TeleporterExitVolumeManager>
{
    internal Dictionary<string, List<TeleporterExitVolume>> idToVolumes = new Dictionary<string, List<TeleporterExitVolume>>();

    public TeleporterExitVolume FindExitVolume(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        if (!idToVolumes.TryGetValue(id, out var value))
        {
            return null;
        }
        return value.RandomOrDefault();
    }

    public override void AddVolume(TeleporterExitVolume volume)
    {
        base.AddVolume(volume);
        AddVolumeToIdDictionary(volume);
    }

    public override void RemoveVolume(TeleporterExitVolume volume)
    {
        RemoveVolumeFromIdDictionary(volume);
        base.RemoveVolume(volume);
    }

    internal void AddVolumeToIdDictionary(TeleporterExitVolume volume)
    {
        if (!string.IsNullOrEmpty(volume.id))
        {
            if (!idToVolumes.TryGetValue(volume.id, out var value))
            {
                value = new List<TeleporterExitVolume>();
                idToVolumes.Add(volume.id, value);
            }
            value.Add(volume);
        }
    }

    internal void RemoveVolumeFromIdDictionary(TeleporterExitVolume volume)
    {
        if (!string.IsNullOrEmpty(volume.id) && idToVolumes.TryGetValue(volume.id, out var value))
        {
            value.RemoveFast(volume);
            if (value.Count < 1)
            {
                idToVolumes.Remove(volume.id);
            }
        }
    }

    protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
    {
        base.OnUpdateGizmos(runtimeGizmos);
        foreach (TeleporterExitVolume allVolume in allVolumes)
        {
            Color color = (allVolume.isSelected ? Color.yellow : debugColor);
            runtimeGizmos.Arrow(allVolume.transform.position, allVolume.transform.forward, 1f, color);
        }
    }

    public TeleporterExitVolumeManager()
    {
        base.FriendlyName = "Teleporter Exit";
        SetDebugColor(new Color32(0, 0, byte.MaxValue, byte.MaxValue));
    }
}
