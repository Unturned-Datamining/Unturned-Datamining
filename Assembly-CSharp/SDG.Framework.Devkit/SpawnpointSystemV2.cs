using System;
using System.Collections.Generic;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class SpawnpointSystemV2 : TempNodeSystemBase
{
    private bool _isVisible;

    private static SpawnpointSystemV2 instance;

    internal List<Spawnpoint> spawnpoints;

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
            foreach (Spawnpoint spawnpoint in spawnpoints)
            {
                spawnpoint.UpdateEditorVisibility();
            }
        }
    }

    public static SpawnpointSystemV2 Get()
    {
        return instance;
    }

    public IReadOnlyList<Spawnpoint> GetAllSpawnpoints()
    {
        return spawnpoints;
    }

    public Spawnpoint FindSpawnpoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        return spawnpoints.Find((Spawnpoint x) => x.id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    internal override Type GetComponentType()
    {
        return typeof(Spawnpoint);
    }

    internal override IEnumerable<GameObject> EnumerateGameObjects()
    {
        foreach (Spawnpoint spawnpoint in spawnpoints)
        {
            yield return spawnpoint.gameObject;
        }
    }

    internal void AddSpawnpoint(Spawnpoint spawnpoint)
    {
        spawnpoints.Add(spawnpoint);
    }

    internal void RemoveSpawnpoint(Spawnpoint spawnpoint)
    {
        spawnpoints.RemoveFast(spawnpoint);
    }

    internal SpawnpointSystemV2()
    {
        instance = this;
        spawnpoints = new List<Spawnpoint>();
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
        foreach (Spawnpoint spawnpoint in spawnpoints)
        {
            Color color = (spawnpoint.isSelected ? Color.yellow : Color.red);
            Matrix4x4 localToWorldMatrix = spawnpoint.transform.localToWorldMatrix;
            RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, 0f, 0f)), color);
            RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, -0.5f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0.5f, 0f)), color);
            RuntimeGizmos.Get().ArrowFromTo(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, -0.5f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 1f)), color);
        }
    }
}
