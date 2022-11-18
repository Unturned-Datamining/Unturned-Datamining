using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

public class LocationDevkitNodeSystem : TempNodeSystemBase
{
    private static LocationDevkitNodeSystem instance;

    private List<LocationDevkitNode> allNodes;

    public static LocationDevkitNodeSystem Get()
    {
        return instance;
    }

    public IReadOnlyList<LocationDevkitNode> GetAllNodes()
    {
        return allNodes;
    }

    public LocationDevkitNode FindByName(string id)
    {
        foreach (LocationDevkitNode allNode in allNodes)
        {
            if (string.Equals(allNode.locationName, id, StringComparison.InvariantCultureIgnoreCase))
            {
                return allNode;
            }
        }
        return null;
    }

    internal override Type GetComponentType()
    {
        return typeof(LocationDevkitNode);
    }

    internal override IEnumerable<GameObject> EnumerateGameObjects()
    {
        foreach (LocationDevkitNode allNode in allNodes)
        {
            yield return allNode.gameObject;
        }
    }

    internal void AddNode(LocationDevkitNode node)
    {
        allNodes.Add(node);
    }

    internal void RemoveNode(LocationDevkitNode node)
    {
        allNodes.RemoveFast(node);
    }

    internal LocationDevkitNodeSystem()
    {
        instance = this;
        allNodes = new List<LocationDevkitNode>();
        TimeUtility.updated += OnUpdateGizmos;
    }

    private void OnUpdateGizmos()
    {
        if (!SpawnpointSystemV2.Get().IsVisible || !Level.isEditor)
        {
            return;
        }
        foreach (LocationDevkitNode allNode in allNodes)
        {
            Color color = (allNode.isSelected ? Color.yellow : Color.red);
            RuntimeGizmos.Get().Cube(allNode.transform.position, allNode.transform.rotation, 1.5f, color);
        }
    }
}
