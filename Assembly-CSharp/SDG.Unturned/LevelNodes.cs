using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class LevelNodes
{
    private const byte SAVEDATA_VERSION_CONVERTED_NODE_VOLUMES = 8;

    private const byte SAVEDATA_VERSION_FINISHED_CONVERTING_ALL_NODES = 9;

    private const byte SAVEDATA_VERSION_NEWEST = 9;

    public static readonly byte SAVEDATA_VERSION = 9;

    private static Transform _models;

    private static List<Node> _nodes;

    /// <summary>
    /// If true then level should convert old node types to volumes.
    /// </summary>
    internal static bool hasLegacyVolumesForConversion;

    /// <summary>
    /// If true then level should convert old non-volumes types to devkit objects.
    /// </summary>
    internal static bool hasLegacyNodesForConversion;

    [Obsolete("Was the parent of all editor nodes in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Nodes";
                _models.parent = Level.level;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelNodes.models which has been deprecated.");
            }
            return _models;
        }
    }

    [Obsolete("All legacy node types have been converted to subclasses of IDevkitHierarchyItem")]
    public static List<Node> nodes => _nodes;

    /// <summary>
    /// Hash of nodes file.
    /// Prevents using the level editor to make noLight nodes visible.
    /// </summary>
    public static byte[] hash { get; private set; }

    internal static void AutoConvertLegacyVolumes()
    {
        UnturnedLog.info("Auto converting legacy volumes");
        foreach (Node node in _nodes)
        {
            if (node is ArenaNode arenaNode)
            {
                GameObject gameObject = new GameObject();
                Transform transform = gameObject.transform;
                transform.position = arenaNode.point;
                transform.rotation = Quaternion.identity;
                float sphereRadius = ArenaNode.CalculateRadiusFromNormalizedRadius(arenaNode._normalizedRadius);
                ArenaCompactorVolume arenaCompactorVolume = gameObject.AddComponent<ArenaCompactorVolume>();
                arenaCompactorVolume.Shape = ELevelVolumeShape.Sphere;
                arenaCompactorVolume.SetSphereRadius(sphereRadius);
                LevelHierarchy.AssignInstanceIdAndMarkDirty(arenaCompactorVolume);
            }
            else if (node is DeadzoneNode deadzoneNode)
            {
                GameObject gameObject2 = new GameObject();
                Transform transform2 = gameObject2.transform;
                transform2.position = deadzoneNode.point;
                transform2.rotation = Quaternion.identity;
                float sphereRadius2 = DeadzoneNode.CalculateRadiusFromNormalizedRadius(deadzoneNode._normalizedRadius);
                DeadzoneVolume deadzoneVolume = gameObject2.AddComponent<DeadzoneVolume>();
                deadzoneVolume.DeadzoneType = deadzoneNode.DeadzoneType;
                deadzoneVolume.Shape = ELevelVolumeShape.Sphere;
                deadzoneVolume.SetSphereRadius(sphereRadius2);
                LevelHierarchy.AssignInstanceIdAndMarkDirty(deadzoneVolume);
            }
            else if (node is EffectNode effectNode)
            {
                GameObject gameObject3 = new GameObject();
                Transform transform3 = gameObject3.transform;
                transform3.position = effectNode.point;
                transform3.rotation = Quaternion.identity;
                AmbianceVolume ambianceVolume = gameObject3.AddComponent<AmbianceVolume>();
                ambianceVolume.id = effectNode.id;
                ambianceVolume.noLighting = effectNode.noLighting;
                ambianceVolume.noWater = effectNode.noWater;
                LevelHierarchy.AssignInstanceIdAndMarkDirty(ambianceVolume);
                if (effectNode.shape == ENodeShape.BOX)
                {
                    transform3.localScale = effectNode.bounds * 2f;
                    continue;
                }
                float sphereRadius3 = EffectNode.CalculateRadiusFromNormalizedRadius(effectNode._normalizedRadius);
                ambianceVolume.SetSphereRadius(sphereRadius3);
                ambianceVolume.Shape = ELevelVolumeShape.Sphere;
            }
            else if (node is PurchaseNode purchaseNode)
            {
                GameObject gameObject4 = new GameObject();
                Transform transform4 = gameObject4.transform;
                transform4.position = purchaseNode.point;
                transform4.rotation = Quaternion.identity;
                float sphereRadius4 = PurchaseNode.CalculateRadiusFromNormalizedRadius(purchaseNode._normalizedRadius);
                HordePurchaseVolume hordePurchaseVolume = gameObject4.AddComponent<HordePurchaseVolume>();
                hordePurchaseVolume.Shape = ELevelVolumeShape.Sphere;
                hordePurchaseVolume.SetSphereRadius(sphereRadius4);
                LevelHierarchy.AssignInstanceIdAndMarkDirty(hordePurchaseVolume);
            }
            else if (node is SafezoneNode safezoneNode)
            {
                GameObject gameObject5 = new GameObject();
                Transform transform5 = gameObject5.transform;
                transform5.rotation = Quaternion.identity;
                SafezoneVolume safezoneVolume = gameObject5.AddComponent<SafezoneVolume>();
                safezoneVolume.noWeapons = safezoneNode.noWeapons;
                safezoneVolume.noBuildables = safezoneNode.noBuildables;
                LevelHierarchy.AssignInstanceIdAndMarkDirty(safezoneVolume);
                if (safezoneNode.isHeight)
                {
                    transform5.position = node.point + new Vector3(0f, 1000f, 0f);
                    transform5.localScale = new Vector3(10000f, 2000f, 10000f);
                    continue;
                }
                transform5.position = node.point;
                float sphereRadius5 = SafezoneNode.CalculateRadiusFromNormalizedRadius(safezoneNode._normalizedRadius);
                safezoneVolume.SetSphereRadius(sphereRadius5);
                safezoneVolume.Shape = ELevelVolumeShape.Sphere;
            }
        }
    }

    internal static void AutoConvertLegacyNodes()
    {
        UnturnedLog.info("Auto converting legacy nodes");
        foreach (Node node in _nodes)
        {
            if (node is AirdropNode airdropNode)
            {
                GameObject gameObject = new GameObject();
                Transform transform = gameObject.transform;
                transform.position = airdropNode.point;
                transform.rotation = Quaternion.identity;
                AirdropDevkitNode airdropDevkitNode = gameObject.AddComponent<AirdropDevkitNode>();
                airdropDevkitNode.id = airdropNode.id;
                LevelHierarchy.AssignInstanceIdAndMarkDirty(airdropDevkitNode);
            }
            else if (node is LocationNode locationNode)
            {
                GameObject gameObject2 = new GameObject();
                Transform transform2 = gameObject2.transform;
                transform2.position = locationNode.point;
                transform2.rotation = Quaternion.identity;
                LocationDevkitNode locationDevkitNode = gameObject2.AddComponent<LocationDevkitNode>();
                locationDevkitNode.locationName = locationNode.name;
                locationDevkitNode.isVisibleOnMap = true;
                LevelHierarchy.AssignInstanceIdAndMarkDirty(locationDevkitNode);
            }
        }
    }

    internal static Node FindLocationNode(string id)
    {
        foreach (Node node in _nodes)
        {
            if (node.type == ENodeType.LOCATION && string.Equals(((LocationNode)node).name, id, StringComparison.InvariantCultureIgnoreCase))
            {
                return node;
            }
        }
        return null;
    }

    public static Transform addNode(Vector3 point, ENodeType type)
    {
        switch (type)
        {
        case ENodeType.LOCATION:
            _nodes.Add(new LocationNode(point));
            break;
        case ENodeType.SAFEZONE:
            _nodes.Add(new SafezoneNode(point));
            break;
        case ENodeType.PURCHASE:
            _nodes.Add(new PurchaseNode(point));
            break;
        case ENodeType.ARENA:
            _nodes.Add(new ArenaNode(point));
            break;
        case ENodeType.DEADZONE:
            _nodes.Add(new DeadzoneNode(point));
            break;
        case ENodeType.AIRDROP:
            _nodes.Add(new AirdropNode(point));
            break;
        case ENodeType.EFFECT:
            _nodes.Add(new EffectNode(point));
            break;
        }
        return _nodes[_nodes.Count - 1].model;
    }

    public static bool isPointInsideSafezone(Vector3 point, out SafezoneNode outSafezoneNode)
    {
        SafezoneVolume firstOverlappingVolume = VolumeManager<SafezoneVolume, SafezoneVolumeManager>.Get().GetFirstOverlappingVolume(point);
        outSafezoneNode = firstOverlappingVolume?.backwardsCompatibilityNode;
        return firstOverlappingVolume != null;
    }

    public static void removeNode(Transform select)
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].model == select)
            {
                _nodes[i].remove();
                _nodes.RemoveAt(i);
                break;
            }
        }
    }

    public static Node getNode(Transform select)
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].model == select)
            {
                return _nodes[i];
            }
        }
        return null;
    }

    public static void load()
    {
        _nodes = new List<Node>();
        hasLegacyVolumesForConversion = false;
        hasLegacyNodesForConversion = false;
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Nodes.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Environment/Nodes.dat", usePath: false);
            byte b = river.readByte();
            if (b > 0)
            {
                bool flag = false;
                bool flag2 = false;
                ushort num = river.readByte();
                for (ushort num2 = 0; num2 < num; num2++)
                {
                    Vector3 vector = river.readSingleVector3();
                    switch ((ENodeType)river.readByte())
                    {
                    case ENodeType.LOCATION:
                    {
                        flag2 = true;
                        string newName = river.readString();
                        _nodes.Add(new LocationNode(vector, newName));
                        break;
                    }
                    case ENodeType.SAFEZONE:
                    {
                        flag = true;
                        float newRadius2 = river.readSingle();
                        bool newHeight = false;
                        if (b > 1)
                        {
                            newHeight = river.readBoolean();
                        }
                        bool newNoWeapons = true;
                        if (b > 4)
                        {
                            newNoWeapons = river.readBoolean();
                        }
                        bool newNoBuildables = true;
                        if (b > 4)
                        {
                            newNoBuildables = river.readBoolean();
                        }
                        _nodes.Add(new SafezoneNode(vector, newRadius2, newHeight, newNoWeapons, newNoBuildables));
                        break;
                    }
                    case ENodeType.PURCHASE:
                    {
                        flag = true;
                        float newRadius4 = river.readSingle();
                        ushort newID2 = river.readUInt16();
                        uint newCost = river.readUInt32();
                        _nodes.Add(new PurchaseNode(vector, newRadius4, newID2, newCost));
                        break;
                    }
                    case ENodeType.ARENA:
                    {
                        flag = true;
                        float num4 = river.readSingle();
                        if (b < 6)
                        {
                            num4 *= 0.5f;
                        }
                        _nodes.Add(new ArenaNode(vector, num4));
                        break;
                    }
                    case ENodeType.DEADZONE:
                    {
                        flag = true;
                        float newRadius3 = river.readSingle();
                        EDeadzoneType newDeadzoneType = EDeadzoneType.DefaultRadiation;
                        if (b > 6)
                        {
                            newDeadzoneType = (EDeadzoneType)river.readByte();
                        }
                        _nodes.Add(new DeadzoneNode(vector, newRadius3, newDeadzoneType));
                        break;
                    }
                    case ENodeType.AIRDROP:
                    {
                        flag2 = true;
                        ushort num3 = river.readUInt16();
                        if (SpawnTableTool.ResolveLegacyId(num3, EAssetType.ITEM, OnGetTestAirdropSpawnTableErrorContext) == 0 && (bool)Assets.shouldLoadAnyAssets && Regions.tryGetCoordinate(vector, out var x, out var y))
                        {
                            Assets.reportError(Level.info.name + " airdrop references invalid spawn table " + num3 + " at (" + x + ", " + y + ")!");
                        }
                        _nodes.Add(new AirdropNode(vector, num3));
                        break;
                    }
                    case ENodeType.EFFECT:
                    {
                        flag = true;
                        byte newShape = 0;
                        if (b > 2)
                        {
                            newShape = river.readByte();
                        }
                        float newRadius = river.readSingle();
                        Vector3 newBounds = Vector3.one;
                        if (b > 2)
                        {
                            newBounds = river.readSingleVector3();
                        }
                        ushort newID = river.readUInt16();
                        bool newNoWater = river.readBoolean();
                        bool newNoLighting = false;
                        if (b > 3)
                        {
                            newNoLighting = river.readBoolean();
                        }
                        _nodes.Add(new EffectNode(vector, (ENodeShape)newShape, newRadius, newBounds, newID, newNoWater, newNoLighting));
                        break;
                    }
                    }
                }
                hasLegacyVolumesForConversion = flag && b < 8;
                hasLegacyNodesForConversion = flag2 && b < 9;
            }
            hash = river.getHash();
            river.closeRiver();
        }
        else
        {
            hash = new byte[20];
        }
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Environment/Nodes.dat", usePath: false);
        river.writeByte(9);
        byte b = 0;
        for (ushort num = 0; num < _nodes.Count; num++)
        {
            if (_nodes[num].type != 0 || ((LocationNode)_nodes[num]).name.Length > 0)
            {
                b++;
            }
        }
        river.writeByte(b);
        for (byte b2 = 0; b2 < _nodes.Count; b2++)
        {
            if (_nodes[b2].type != 0 || ((LocationNode)_nodes[b2]).name.Length > 0)
            {
                river.writeSingleVector3(_nodes[b2].point);
                river.writeByte((byte)_nodes[b2].type);
                if (_nodes[b2].type == ENodeType.LOCATION)
                {
                    river.writeString(((LocationNode)_nodes[b2]).name);
                }
                else if (_nodes[b2].type == ENodeType.SAFEZONE)
                {
                    river.writeSingle(((SafezoneNode)_nodes[b2]).radius);
                    river.writeBoolean(((SafezoneNode)_nodes[b2]).isHeight);
                    river.writeBoolean(((SafezoneNode)_nodes[b2]).noWeapons);
                    river.writeBoolean(((SafezoneNode)_nodes[b2]).noBuildables);
                }
                else if (_nodes[b2].type == ENodeType.PURCHASE)
                {
                    river.writeSingle(((PurchaseNode)_nodes[b2]).radius);
                    river.writeUInt16(((PurchaseNode)_nodes[b2]).id);
                    river.writeUInt32(((PurchaseNode)_nodes[b2]).cost);
                }
                else if (_nodes[b2].type == ENodeType.ARENA)
                {
                    river.writeSingle(((ArenaNode)_nodes[b2]).radius);
                }
                else if (_nodes[b2].type == ENodeType.DEADZONE)
                {
                    river.writeSingle(((DeadzoneNode)_nodes[b2]).radius);
                    river.writeByte((byte)((DeadzoneNode)_nodes[b2]).DeadzoneType);
                }
                else if (_nodes[b2].type == ENodeType.AIRDROP)
                {
                    river.writeUInt16(((AirdropNode)_nodes[b2]).id);
                }
                else if (_nodes[b2].type == ENodeType.EFFECT)
                {
                    river.writeByte((byte)((EffectNode)_nodes[b2]).shape);
                    river.writeSingle(((EffectNode)_nodes[b2]).radius);
                    river.writeSingleVector3(((EffectNode)_nodes[b2]).bounds);
                    river.writeUInt16(((EffectNode)_nodes[b2]).id);
                    river.writeBoolean(((EffectNode)_nodes[b2]).noWater);
                    river.writeBoolean(((EffectNode)_nodes[b2]).noLighting);
                }
            }
        }
        river.closeRiver();
    }

    private static string OnGetTestAirdropSpawnTableErrorContext()
    {
        return "level nodes airdrop test";
    }
}
