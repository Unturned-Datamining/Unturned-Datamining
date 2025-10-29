using System;
using System.Collections.Generic;
using System.Linq;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class SpawnpointSystemV2 : TempNodeSystemBase
{
    private bool _isVisible;

    private static SpawnpointSystemV2 instance;

    internal List<Spawnpoint> allSpawnpoints;

    internal Dictionary<string, List<Spawnpoint>> idToSpawnpoints;

    public bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (_isVisible == value)
            {
                return;
            }
            _isVisible = value;
            ConvenientSavedata.get().write("Visibility_Spawnpoints", value);
            if (!Level.isEditor)
            {
                return;
            }
            foreach (AirdropDevkitNode allNode in AirdropDevkitNodeSystem.Get().GetAllNodes())
            {
                allNode.UpdateEditorVisibility();
            }
            foreach (LocationDevkitNode allNode2 in LocationDevkitNodeSystem.Get().GetAllNodes())
            {
                allNode2.UpdateEditorVisibility();
            }
            foreach (Spawnpoint allSpawnpoint in allSpawnpoints)
            {
                allSpawnpoint.UpdateEditorVisibility();
            }
        }
    }

    public static SpawnpointSystemV2 Get()
    {
        return instance;
    }

    public IReadOnlyList<Spawnpoint> GetAllSpawnpoints()
    {
        return allSpawnpoints;
    }

    public Spawnpoint FindFirstSpawnpoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        if (idToSpawnpoints.TryGetValue(id, out var value))
        {
            return value.FirstOrDefault();
        }
        return null;
    }

    internal override Type GetComponentType()
    {
        return typeof(Spawnpoint);
    }

    internal override IEnumerable<GameObject> EnumerateGameObjects()
    {
        foreach (Spawnpoint allSpawnpoint in allSpawnpoints)
        {
            yield return allSpawnpoint.gameObject;
        }
    }

    internal void AddSpawnpoint(Spawnpoint spawnpoint)
    {
        allSpawnpoints.Add(spawnpoint);
        AddSpawnpointToIdDictionary(spawnpoint);
    }

    internal void RemoveSpawnpoint(Spawnpoint spawnpoint)
    {
        RemoveSpawnpointFromIdDictionary(spawnpoint);
        allSpawnpoints.RemoveFast(spawnpoint);
    }

    internal void AddSpawnpointToIdDictionary(Spawnpoint spawnpoint)
    {
        string spawnpointID = spawnpoint.SpawnpointID;
        if (!string.IsNullOrEmpty(spawnpointID))
        {
            if (!idToSpawnpoints.TryGetValue(spawnpointID, out var value))
            {
                value = new List<Spawnpoint>();
                idToSpawnpoints.Add(spawnpointID, value);
            }
            value.Add(spawnpoint);
        }
    }

    internal void RemoveSpawnpointFromIdDictionary(Spawnpoint spawnpoint)
    {
        string spawnpointID = spawnpoint.SpawnpointID;
        if (!string.IsNullOrEmpty(spawnpointID) && idToSpawnpoints.TryGetValue(spawnpointID, out var value))
        {
            value.RemoveFast(spawnpoint);
            if (value.Count < 1)
            {
                idToSpawnpoints.Remove(spawnpointID);
            }
        }
    }

    internal SpawnpointSystemV2()
    {
        instance = this;
        allSpawnpoints = new List<Spawnpoint>();
        idToSpawnpoints = new Dictionary<string, List<Spawnpoint>>(StringComparer.InvariantCultureIgnoreCase);
        TimeUtility.updated += OnUpdateGizmos;
        if (ConvenientSavedata.get().read("Visibility_Nodes", out bool value))
        {
            _isVisible = value;
        }
        else
        {
            _isVisible = true;
        }
    }

    private void OnUpdateGizmos()
    {
        if (!_isVisible || !Level.isEditor)
        {
            return;
        }
        foreach (Spawnpoint allSpawnpoint in allSpawnpoints)
        {
            Color color = (allSpawnpoint.isSelected ? Color.yellow : Color.red);
            Matrix4x4 localToWorldMatrix = allSpawnpoint.transform.localToWorldMatrix;
            RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, 0f, 0f)), color);
            RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, -0.5f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0.5f, 0f)), color);
            RuntimeGizmos.Get().ArrowFromTo(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, -0.5f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 1f)), color);
        }
    }

    [Obsolete("Renamed to clarify behavior")]
    public Spawnpoint FindSpawnpoint(string id)
    {
        return FindFirstSpawnpoint(id);
    }
}
