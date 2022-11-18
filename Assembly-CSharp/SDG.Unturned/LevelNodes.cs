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

    internal static bool hasLegacyVolumesForConversion;

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

    public static List<Node> nodes => _nodes;

    public static byte[] hash { get; private set; }

    internal static void AutoConvertLegacyVolumes()
    {
        UnturnedLog.info("Auto converting legacy volumes");
        foreach (Node node in nodes)
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
            }
            else if (node is SafezoneNode safezoneNode)
            {
                GameObject gameObject5 = new GameObject();
                Transform transform5 = gameObject5.transform;
                transform5.rotation = Quaternion.identity;
                SafezoneVolume safezoneVolume = gameObject5.AddComponent<SafezoneVolume>();
                safezoneVolume.noWeapons = safezoneNode.noWeapons;
                safezoneVolume.noBuildables = safezoneNode.noBuildables;
                LevelHierarchy.initItem(safezoneVolume);
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
        foreach (Node node in nodes)
        {
            if (node is AirdropNode airdropNode)
            {
                GameObject gameObject = new GameObject();
                Transform transform = gameObject.transform;
                transform.position = airdropNode.point;
                transform.rotation = Quaternion.identity;
                gameObject.AddComponent<AirdropDevkitNode>().id = airdropNode.id;
            }
            else if (node is LocationNode locationNode)
            {
                GameObject gameObject2 = new GameObject();
                Transform transform2 = gameObject2.transform;
                transform2.position = locationNode.point;
                transform2.rotation = Quaternion.identity;
                gameObject2.AddComponent<LocationDevkitNode>().locationName = locationNode.name;
            }
        }
    }

    internal static Node FindLocationNode(string id)
    {
        foreach (Node node in nodes)
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
            nodes.Add(new LocationNode(point));
            break;
        case ENodeType.SAFEZONE:
            nodes.Add(new SafezoneNode(point));
            break;
        case ENodeType.PURCHASE:
            nodes.Add(new PurchaseNode(point));
            break;
        case ENodeType.ARENA:
            nodes.Add(new ArenaNode(point));
            break;
        case ENodeType.DEADZONE:
            nodes.Add(new DeadzoneNode(point));
            break;
        case ENodeType.AIRDROP:
            nodes.Add(new AirdropNode(point));
            break;
        case ENodeType.EFFECT:
            nodes.Add(new EffectNode(point));
            break;
        }
        return nodes[nodes.Count - 1].model;
    }

    public static bool isPointInsideSafezone(Vector3 point, out SafezoneNode outSafezoneNode)
    {
        SafezoneVolume firstOverlappingVolume = VolumeManager<SafezoneVolume, SafezoneVolumeManager>.Get().GetFirstOverlappingVolume(point);
        outSafezoneNode = firstOverlappingVolume?.backwardsCompatibilityNode;
        return firstOverlappingVolume != null;
    }

    public static void removeNode(Transform select)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].model == select)
            {
                nodes[i].remove();
                nodes.RemoveAt(i);
                break;
            }
        }
    }

    public static Node getNode(Transform select)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].model == select)
            {
                return nodes[i];
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
                for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
                {
                    Vector3 vector = river.readSingleVector3();
                    switch (river.readByte())
                    {
                    case 0:
                    {
                        flag2 = true;
                        string newName = river.readString();
                        nodes.Add(new LocationNode(vector, newName));
                        break;
                    }
                    case 1:
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
                        nodes.Add(new SafezoneNode(vector, newRadius2, newHeight, newNoWeapons, newNoBuildables));
                        break;
                    }
                    case 2:
                    {
                        flag = true;
                        float newRadius4 = river.readSingle();
                        ushort newID2 = river.readUInt16();
                        uint newCost = river.readUInt32();
                        nodes.Add(new PurchaseNode(vector, newRadius4, newID2, newCost));
                        break;
                    }
                    case 3:
                    {
                        flag = true;
                        float num4 = river.readSingle();
                        if (b < 6)
                        {
                            num4 *= 0.5f;
                        }
                        nodes.Add(new ArenaNode(vector, num4));
                        break;
                    }
                    case 4:
                    {
                        flag = true;
                        float newRadius3 = river.readSingle();
                        EDeadzoneType newDeadzoneType = EDeadzoneType.DefaultRadiation;
                        if (b > 6)
                        {
                            newDeadzoneType = (EDeadzoneType)river.readByte();
                        }
                        nodes.Add(new DeadzoneNode(vector, newRadius3, newDeadzoneType));
                        break;
                    }
                    case 5:
                    {
                        flag2 = true;
                        ushort num3 = river.readUInt16();
                        if (SpawnTableTool.resolve(num3) == 0 && (bool)Assets.shouldLoadAnyAssets && Regions.tryGetCoordinate(vector, out var x, out var y))
                        {
                            Assets.reportError(Level.info.name + " airdrop references invalid spawn table " + num3 + " at (" + x + ", " + y + ")!");
                        }
                        nodes.Add(new AirdropNode(vector, num3));
                        break;
                    }
                    case 6:
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
                        nodes.Add(new EffectNode(vector, (ENodeShape)newShape, newRadius, newBounds, newID, newNoWater, newNoLighting));
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
        for (ushort num = 0; num < nodes.Count; num = (ushort)(num + 1))
        {
            if (nodes[num].type != 0 || ((LocationNode)nodes[num]).name.Length > 0)
            {
                b = (byte)(b + 1);
            }
        }
        river.writeByte(b);
        for (byte b2 = 0; b2 < nodes.Count; b2 = (byte)(b2 + 1))
        {
            if (nodes[b2].type != 0 || ((LocationNode)nodes[b2]).name.Length > 0)
            {
                river.writeSingleVector3(nodes[b2].point);
                river.writeByte((byte)nodes[b2].type);
                if (nodes[b2].type == ENodeType.LOCATION)
                {
                    river.writeString(((LocationNode)nodes[b2]).name);
                }
                else if (nodes[b2].type == ENodeType.SAFEZONE)
                {
                    river.writeSingle(((SafezoneNode)nodes[b2]).radius);
                    river.writeBoolean(((SafezoneNode)nodes[b2]).isHeight);
                    river.writeBoolean(((SafezoneNode)nodes[b2]).noWeapons);
                    river.writeBoolean(((SafezoneNode)nodes[b2]).noBuildables);
                }
                else if (nodes[b2].type == ENodeType.PURCHASE)
                {
                    river.writeSingle(((PurchaseNode)nodes[b2]).radius);
                    river.writeUInt16(((PurchaseNode)nodes[b2]).id);
                    river.writeUInt32(((PurchaseNode)nodes[b2]).cost);
                }
                else if (nodes[b2].type == ENodeType.ARENA)
                {
                    river.writeSingle(((ArenaNode)nodes[b2]).radius);
                }
                else if (nodes[b2].type == ENodeType.DEADZONE)
                {
                    river.writeSingle(((DeadzoneNode)nodes[b2]).radius);
                    river.writeByte((byte)((DeadzoneNode)nodes[b2]).DeadzoneType);
                }
                else if (nodes[b2].type == ENodeType.AIRDROP)
                {
                    river.writeUInt16(((AirdropNode)nodes[b2]).id);
                }
                else if (nodes[b2].type == ENodeType.EFFECT)
                {
                    river.writeByte((byte)((EffectNode)nodes[b2]).shape);
                    river.writeSingle(((EffectNode)nodes[b2]).radius);
                    river.writeSingleVector3(((EffectNode)nodes[b2]).bounds);
                    river.writeUInt16(((EffectNode)nodes[b2]).id);
                    river.writeBoolean(((EffectNode)nodes[b2]).noWater);
                    river.writeBoolean(((EffectNode)nodes[b2]).noLighting);
                }
            }
        }
        river.closeRiver();
    }
}
