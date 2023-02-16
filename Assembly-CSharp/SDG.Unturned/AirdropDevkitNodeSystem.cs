using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned;

public class AirdropDevkitNodeSystem : TempNodeSystemBase
{
    private static AirdropDevkitNodeSystem instance;

    private List<AirdropDevkitNode> allNodes;

    private CustomSampler gizmoUpdateSampler;

    public static AirdropDevkitNodeSystem Get()
    {
        return instance;
    }

    public IReadOnlyList<AirdropDevkitNode> GetAllNodes()
    {
        return allNodes;
    }

    internal override Type GetComponentType()
    {
        return typeof(AirdropDevkitNode);
    }

    internal override IEnumerable<GameObject> EnumerateGameObjects()
    {
        foreach (AirdropDevkitNode allNode in allNodes)
        {
            yield return allNode.gameObject;
        }
    }

    internal void AddNode(AirdropDevkitNode node)
    {
        allNodes.Add(node);
    }

    internal void RemoveNode(AirdropDevkitNode node)
    {
        allNodes.RemoveFast(node);
    }

    internal AirdropDevkitNodeSystem()
    {
        instance = this;
        allNodes = new List<AirdropDevkitNode>();
        gizmoUpdateSampler = CustomSampler.Create("AirdropDevkitNodeSystem.UpdateGizmos");
        TimeUtility.updated += OnUpdateGizmos;
    }

    private void OnUpdateGizmos()
    {
        if (!SpawnpointSystemV2.Get().IsVisible || !Level.isEditor)
        {
            return;
        }
        foreach (AirdropDevkitNode allNode in allNodes)
        {
            Color color = (allNode.isSelected ? Color.yellow : Color.red);
            RuntimeGizmos.Get().Arrow(allNode.transform.position + allNode.transform.up * 32f, -allNode.transform.up, 32f, color);
        }
    }
}
