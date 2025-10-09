using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class NPCOverlapVolumeManager : VolumeManager<NPCOverlapVolume, NPCOverlapVolumeManager>
{
    internal Dictionary<string, List<NPCOverlapVolume>> idToVolumes = new Dictionary<string, List<NPCOverlapVolume>>();

    public int CountPlayersInVolume(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return 0;
        }
        if (!idToVolumes.TryGetValue(id, out var value))
        {
            return 0;
        }
        int num = 0;
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client.player == null)
            {
                continue;
            }
            Vector3 position = client.player.transform.position;
            foreach (NPCOverlapVolume item in value)
            {
                if (item != null && item.IsPositionInsideVolume(position))
                {
                    num++;
                    break;
                }
            }
        }
        return num;
    }

    public override void AddVolume(NPCOverlapVolume volume)
    {
        base.AddVolume(volume);
        AddVolumeToIdDictionary(volume);
    }

    public override void RemoveVolume(NPCOverlapVolume volume)
    {
        RemoveVolumeFromIdDictionary(volume);
        base.RemoveVolume(volume);
    }

    internal void AddVolumeToIdDictionary(NPCOverlapVolume volume)
    {
        if (!string.IsNullOrEmpty(volume.id))
        {
            if (!idToVolumes.TryGetValue(volume.id, out var value))
            {
                value = new List<NPCOverlapVolume>();
                idToVolumes.Add(volume.id, value);
            }
            value.Add(volume);
        }
    }

    internal void RemoveVolumeFromIdDictionary(NPCOverlapVolume volume)
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
        foreach (NPCOverlapVolume allVolume in allVolumes)
        {
            if (allVolume.isSelected)
            {
                runtimeGizmos.Label(allVolume.transform.position, allVolume.id);
            }
        }
    }

    public NPCOverlapVolumeManager()
    {
        base.FriendlyName = "NPC Overlap";
        SetDebugColor(new Color32(130, 20, 200, byte.MaxValue));
    }
}
