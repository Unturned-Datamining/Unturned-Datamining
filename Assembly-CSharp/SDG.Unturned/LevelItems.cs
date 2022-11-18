using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelItems
{
    public static readonly byte SAVEDATA_VERSION = 4;

    private static Transform _models;

    private static List<ItemTable> _tables;

    private static List<ItemSpawnpoint>[,] _spawns;

    [Obsolete("Was the parent of all items in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Items";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
            }
            return _models;
        }
    }

    public static List<ItemTable> tables => _tables;

    public static List<ItemSpawnpoint>[,] spawns => _spawns;

    public static void setEnabled(bool isEnabled)
    {
        if (spawns == null)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                for (int i = 0; i < spawns[b, b2].Count; i++)
                {
                    spawns[b, b2][i].setEnabled(isEnabled);
                }
            }
        }
    }

    public static void addTable(string name)
    {
        if (tables.Count != 255)
        {
            tables.Add(new ItemTable(name));
        }
    }

    public static void removeTable()
    {
        tables.RemoveAt(EditorSpawns.selectedItem);
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                List<ItemSpawnpoint> list = new List<ItemSpawnpoint>();
                for (int i = 0; i < spawns[b, b2].Count; i++)
                {
                    ItemSpawnpoint itemSpawnpoint = spawns[b, b2][i];
                    if (itemSpawnpoint.type == EditorSpawns.selectedItem)
                    {
                        UnityEngine.Object.Destroy(itemSpawnpoint.node.gameObject);
                        continue;
                    }
                    if (itemSpawnpoint.type > EditorSpawns.selectedItem)
                    {
                        itemSpawnpoint.type--;
                    }
                    list.Add(itemSpawnpoint);
                }
                _spawns[b, b2] = list;
            }
        }
        for (int j = 0; j < LevelZombies.tables.Count; j++)
        {
            ZombieTable zombieTable = LevelZombies.tables[j];
            if (zombieTable.lootIndex > EditorSpawns.selectedItem)
            {
                zombieTable.lootIndex--;
            }
        }
        EditorSpawns.selectedItem = 0;
        if (EditorSpawns.selectedItem < tables.Count)
        {
            EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedItem].color;
        }
    }

    public static void addSpawn(Vector3 point)
    {
        if (Regions.tryGetCoordinate(point, out var x, out var y) && EditorSpawns.selectedItem < tables.Count)
        {
            spawns[x, y].Add(new ItemSpawnpoint(EditorSpawns.selectedItem, point));
        }
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                List<ItemSpawnpoint> list = new List<ItemSpawnpoint>();
                for (int i = 0; i < spawns[b, b2].Count; i++)
                {
                    ItemSpawnpoint itemSpawnpoint = spawns[b, b2][i];
                    if ((itemSpawnpoint.point - point).sqrMagnitude < radius)
                    {
                        UnityEngine.Object.Destroy(itemSpawnpoint.node.gameObject);
                    }
                    else
                    {
                        list.Add(itemSpawnpoint);
                    }
                }
                _spawns[b, b2] = list;
            }
        }
    }

    public static ushort getItem(ItemSpawnpoint spawn)
    {
        return getItem(spawn.type);
    }

    public static ushort getItem(byte type)
    {
        return tables[type].getItem();
    }

    public static void load()
    {
        if (!Level.isEditor && !Provider.isServer)
        {
            return;
        }
        _tables = new List<ItemTable>();
        _spawns = new List<ItemSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        if (ReadWrite.fileExists(Level.info.path + "/Spawns/Items.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Spawns/Items.dat", useCloud: false, usePath: false, 0);
            byte b = block.readByte();
            if (b > 1 && b < 3)
            {
                block.readSteamID();
            }
            byte b2 = block.readByte();
            for (byte b3 = 0; b3 < b2; b3 = (byte)(b3 + 1))
            {
                Color newColor = block.readColor();
                string text = block.readString();
                ushort num;
                if (b > 3)
                {
                    num = block.readUInt16();
                    if (num != 0 && SpawnTableTool.resolve(num) == 0 && (bool)Assets.shouldLoadAnyAssets)
                    {
                        Assets.reportError(Level.info.name + " item table \"" + text + "\" references invalid spawn table " + num + "!");
                    }
                }
                else
                {
                    num = 0;
                }
                List<ItemTier> list = new List<ItemTier>();
                byte b4 = block.readByte();
                for (byte b5 = 0; b5 < b4; b5 = (byte)(b5 + 1))
                {
                    string newName = block.readString();
                    float newChance = block.readSingle();
                    List<ItemSpawn> list2 = new List<ItemSpawn>();
                    byte b6 = block.readByte();
                    for (byte b7 = 0; b7 < b6; b7 = (byte)(b7 + 1))
                    {
                        ushort num2 = block.readUInt16();
                        if (Assets.find(EAssetType.ITEM, num2) is ItemAsset itemAsset && !itemAsset.isPro)
                        {
                            list2.Add(new ItemSpawn(num2));
                        }
                    }
                    if (list2.Count > 0)
                    {
                        list.Add(new ItemTier(list2, newName, newChance));
                    }
                }
                tables.Add(new ItemTable(list, newColor, text, num));
                if (!Level.isEditor)
                {
                    tables[b3].buildTable();
                }
            }
        }
        for (byte b8 = 0; b8 < Regions.WORLD_SIZE; b8 = (byte)(b8 + 1))
        {
            for (byte b9 = 0; b9 < Regions.WORLD_SIZE; b9 = (byte)(b9 + 1))
            {
                spawns[b8, b9] = new List<ItemSpawnpoint>();
            }
        }
        if (ReadWrite.fileExists(Level.info.path + "/Spawns/Jars.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Spawns/Jars.dat", usePath: false);
            if (river.readByte() > 0)
            {
                for (byte b10 = 0; b10 < Regions.WORLD_SIZE; b10 = (byte)(b10 + 1))
                {
                    for (byte b11 = 0; b11 < Regions.WORLD_SIZE; b11 = (byte)(b11 + 1))
                    {
                        ushort num3 = river.readUInt16();
                        for (ushort num4 = 0; num4 < num3; num4 = (ushort)(num4 + 1))
                        {
                            byte newType = river.readByte();
                            Vector3 newPoint = river.readSingleVector3();
                            spawns[b10, b11].Add(new ItemSpawnpoint(newType, newPoint));
                        }
                    }
                }
            }
            river.closeRiver();
            return;
        }
        for (byte b12 = 0; b12 < Regions.WORLD_SIZE; b12 = (byte)(b12 + 1))
        {
            for (byte b13 = 0; b13 < Regions.WORLD_SIZE; b13 = (byte)(b13 + 1))
            {
                spawns[b12, b13] = new List<ItemSpawnpoint>();
                if (ReadWrite.fileExists(Level.info.path + "/Spawns/Items_" + b12 + "_" + b13 + ".dat", useCloud: false, usePath: false))
                {
                    River river2 = new River(Level.info.path + "/Spawns/Items_" + b12 + "_" + b13 + ".dat", usePath: false);
                    if (river2.readByte() > 0)
                    {
                        ushort num5 = river2.readUInt16();
                        for (ushort num6 = 0; num6 < num5; num6 = (ushort)(num6 + 1))
                        {
                            byte newType2 = river2.readByte();
                            Vector3 newPoint2 = river2.readSingleVector3();
                            spawns[b12, b13].Add(new ItemSpawnpoint(newType2, newPoint2));
                        }
                    }
                    river2.closeRiver();
                }
            }
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeByte((byte)tables.Count);
        for (byte b = 0; b < tables.Count; b = (byte)(b + 1))
        {
            ItemTable itemTable = tables[b];
            block.writeColor(itemTable.color);
            block.writeString(itemTable.name);
            block.writeUInt16(itemTable.tableID);
            block.write((byte)itemTable.tiers.Count);
            for (byte b2 = 0; b2 < itemTable.tiers.Count; b2 = (byte)(b2 + 1))
            {
                ItemTier itemTier = itemTable.tiers[b2];
                block.writeString(itemTier.name);
                block.writeSingle(itemTier.chance);
                block.writeByte((byte)itemTier.table.Count);
                for (byte b3 = 0; b3 < itemTier.table.Count; b3 = (byte)(b3 + 1))
                {
                    ItemSpawn itemSpawn = itemTier.table[b3];
                    block.writeUInt16(itemSpawn.item);
                }
            }
        }
        ReadWrite.writeBlock(Level.info.path + "/Spawns/Items.dat", useCloud: false, usePath: false, block);
        River river = new River(Level.info.path + "/Spawns/Jars.dat", usePath: false);
        river.writeByte(SAVEDATA_VERSION);
        for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4 = (byte)(b4 + 1))
        {
            for (byte b5 = 0; b5 < Regions.WORLD_SIZE; b5 = (byte)(b5 + 1))
            {
                List<ItemSpawnpoint> list = spawns[b4, b5];
                river.writeUInt16((ushort)list.Count);
                for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
                {
                    ItemSpawnpoint itemSpawnpoint = list[num];
                    river.writeByte(itemSpawnpoint.type);
                    river.writeSingleVector3(itemSpawnpoint.point);
                }
            }
        }
        river.closeRiver();
    }
}
