using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelZombies
{
    public const byte SAVEDATA_TABLE_VERSION_OLDER = 9;

    public const byte SAVEDATA_TABLE_VERSION_ADDED_UNIQUE_ID = 10;

    private const byte SAVEDATA_TABLE_VERSION_NEWEST = 10;

    public static readonly byte SAVEDATA_TABLE_VERSION = 10;

    public static readonly byte SAVEDATA_SPAWN_VERSION = 1;

    private static Transform _models;

    public static List<ZombieTable> tables;

    private static List<ZombieSpawnpoint>[] _zombies;

    private static List<ZombieSpawnpoint>[,] _spawns;

    private static int nextTableUniqueId;

    [Obsolete("Was the parent of all zombies in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Zombies";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelZombies.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<ZombieSpawnpoint>[] zombies => _zombies;

    public static List<ZombieSpawnpoint>[,] spawns => _spawns;

    internal static int GenerateTableUniqueId()
    {
        int result = nextTableUniqueId;
        nextTableUniqueId++;
        return result;
    }

    public static void setEnabled(bool isEnabled)
    {
        if (spawns == null)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
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
            tables.Add(new ZombieTable(name));
        }
    }

    public static void removeTable()
    {
        tables.RemoveAt(EditorSpawns.selectedZombie);
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                List<ZombieSpawnpoint> list = new List<ZombieSpawnpoint>();
                for (int i = 0; i < spawns[b, b2].Count; i++)
                {
                    ZombieSpawnpoint zombieSpawnpoint = spawns[b, b2][i];
                    if (zombieSpawnpoint.type == EditorSpawns.selectedZombie)
                    {
                        UnityEngine.Object.Destroy(zombieSpawnpoint.node.gameObject);
                        continue;
                    }
                    if (zombieSpawnpoint.type > EditorSpawns.selectedZombie)
                    {
                        zombieSpawnpoint.type--;
                    }
                    list.Add(zombieSpawnpoint);
                }
                _spawns[b, b2] = list;
            }
        }
        EditorSpawns.selectedZombie = 0;
        if (EditorSpawns.selectedZombie < tables.Count)
        {
            EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedZombie].color;
        }
    }

    public static void addSpawn(Vector3 point)
    {
        if (Regions.tryGetCoordinate(point, out var x, out var y) && EditorSpawns.selectedZombie < tables.Count)
        {
            spawns[x, y].Add(new ZombieSpawnpoint(EditorSpawns.selectedZombie, point));
        }
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                List<ZombieSpawnpoint> list = new List<ZombieSpawnpoint>();
                for (int i = 0; i < spawns[b, b2].Count; i++)
                {
                    ZombieSpawnpoint zombieSpawnpoint = spawns[b, b2][i];
                    if ((zombieSpawnpoint.point - point).sqrMagnitude < radius)
                    {
                        UnityEngine.Object.Destroy(zombieSpawnpoint.node.gameObject);
                    }
                    else
                    {
                        list.Add(zombieSpawnpoint);
                    }
                }
                _spawns[b, b2] = list;
            }
        }
    }

    /// <returns>-1 if table was not found.</returns>
    public static int FindTableIndexByUniqueId(int uniqueId)
    {
        if (tables != null && uniqueId > 0)
        {
            for (int i = 0; i < tables.Count; i++)
            {
                ZombieTable zombieTable = tables[i];
                if (zombieTable != null && zombieTable.tableUniqueId == uniqueId)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static void load()
    {
        tables = new List<ZombieTable>();
        nextTableUniqueId = 1;
        if (ReadWrite.fileExists(Level.info.path + "/Spawns/Zombies.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Spawns/Zombies.dat", useCloud: false, usePath: false, 0);
            byte b = block.readByte();
            if (b > 3 && b < 5)
            {
                block.readSteamID();
            }
            if (b >= 10)
            {
                nextTableUniqueId = block.readInt32();
            }
            if (b > 2)
            {
                byte b2 = block.readByte();
                for (byte b3 = 0; b3 < b2; b3++)
                {
                    int newTableUniqueId = ((b < 10) ? GenerateTableUniqueId() : block.readInt32());
                    Color newColor = block.readColor();
                    string newName = block.readString();
                    bool flag = block.readBoolean();
                    ushort newHealth = block.readUInt16();
                    byte newDamage = block.readByte();
                    byte newLootIndex = block.readByte();
                    ushort newLootID = (ushort)((b > 6) ? block.readUInt16() : 0);
                    uint newXP = ((b > 7) ? block.readUInt32() : ((!flag) ? 3u : 40u));
                    float newRegen = 10f;
                    if (b > 5)
                    {
                        newRegen = block.readSingle();
                    }
                    string newDifficultyGUID = string.Empty;
                    if (b > 8)
                    {
                        newDifficultyGUID = block.readString();
                    }
                    ZombieSlot[] array = new ZombieSlot[4];
                    byte b4 = block.readByte();
                    for (byte b5 = 0; b5 < b4; b5++)
                    {
                        List<ZombieCloth> list = new List<ZombieCloth>();
                        float newChance = block.readSingle();
                        byte b6 = block.readByte();
                        for (byte b7 = 0; b7 < b6; b7++)
                        {
                            ushort num = block.readUInt16();
                            if (Assets.find(EAssetType.ITEM, num) is ItemAsset)
                            {
                                list.Add(new ZombieCloth(num));
                            }
                        }
                        array[b5] = new ZombieSlot(newChance, list);
                    }
                    tables.Add(new ZombieTable(array, newColor, newName, flag, newHealth, newDamage, newLootIndex, newLootID, newXP, newRegen, newDifficultyGUID, newTableUniqueId));
                }
            }
            else
            {
                byte b8 = block.readByte();
                for (byte b9 = 0; b9 < b8; b9++)
                {
                    int newTableUniqueId2 = GenerateTableUniqueId();
                    Color newColor2 = block.readColor();
                    string newName2 = block.readString();
                    byte newLootIndex2 = block.readByte();
                    ZombieSlot[] array2 = new ZombieSlot[4];
                    byte b10 = block.readByte();
                    for (byte b11 = 0; b11 < b10; b11++)
                    {
                        List<ZombieCloth> list2 = new List<ZombieCloth>();
                        float newChance2 = block.readSingle();
                        byte b12 = block.readByte();
                        for (byte b13 = 0; b13 < b12; b13++)
                        {
                            ushort num2 = block.readUInt16();
                            if (Assets.find(EAssetType.ITEM, num2) is ItemAsset)
                            {
                                list2.Add(new ZombieCloth(num2));
                            }
                        }
                        array2[b11] = new ZombieSlot(newChance2, list2);
                    }
                    tables.Add(new ZombieTable(array2, newColor2, newName2, newMega: false, 100, 15, newLootIndex2, 0, 5u, 10f, string.Empty, newTableUniqueId2));
                }
            }
        }
        _spawns = new List<ZombieSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b14 = 0; b14 < Regions.WORLD_SIZE; b14++)
        {
            for (byte b15 = 0; b15 < Regions.WORLD_SIZE; b15++)
            {
                spawns[b14, b15] = new List<ZombieSpawnpoint>();
            }
        }
        if (Level.isEditor)
        {
            if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals.dat", useCloud: false, usePath: false))
            {
                River river = new River(Level.info.path + "/Spawns/Animals.dat", usePath: false);
                if (river.readByte() > 0)
                {
                    for (byte b16 = 0; b16 < Regions.WORLD_SIZE; b16++)
                    {
                        for (byte b17 = 0; b17 < Regions.WORLD_SIZE; b17++)
                        {
                            ushort num3 = river.readUInt16();
                            for (ushort num4 = 0; num4 < num3; num4++)
                            {
                                byte newType = river.readByte();
                                Vector3 newPoint = river.readSingleVector3();
                                spawns[b16, b17].Add(new ZombieSpawnpoint(newType, newPoint));
                            }
                        }
                    }
                }
                river.closeRiver();
                return;
            }
            for (byte b18 = 0; b18 < Regions.WORLD_SIZE; b18++)
            {
                for (byte b19 = 0; b19 < Regions.WORLD_SIZE; b19++)
                {
                    spawns[b18, b19] = new List<ZombieSpawnpoint>();
                    if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals_" + b18 + "_" + b19 + ".dat", useCloud: false, usePath: false))
                    {
                        River river2 = new River(Level.info.path + "/Spawns/Animals_" + b18 + "_" + b19 + ".dat", usePath: false);
                        if (river2.readByte() > 0)
                        {
                            ushort num5 = river2.readUInt16();
                            for (ushort num6 = 0; num6 < num5; num6++)
                            {
                                byte newType2 = river2.readByte();
                                Vector3 newPoint2 = river2.readSingleVector3();
                                spawns[b18, b19].Add(new ZombieSpawnpoint(newType2, newPoint2));
                            }
                            river2.closeRiver();
                        }
                    }
                }
            }
        }
        else
        {
            if (!Provider.isServer)
            {
                return;
            }
            _zombies = new List<ZombieSpawnpoint>[LevelNavigation.bounds.Count];
            for (int i = 0; i < zombies.Length; i++)
            {
                zombies[i] = new List<ZombieSpawnpoint>();
            }
            if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals.dat", useCloud: false, usePath: false))
            {
                River river3 = new River(Level.info.path + "/Spawns/Animals.dat", usePath: false);
                if (river3.readByte() > 0)
                {
                    for (byte b20 = 0; b20 < Regions.WORLD_SIZE; b20++)
                    {
                        for (byte b21 = 0; b21 < Regions.WORLD_SIZE; b21++)
                        {
                            ushort num7 = river3.readUInt16();
                            for (ushort num8 = 0; num8 < num7; num8++)
                            {
                                byte newType3 = river3.readByte();
                                Vector3 vector = river3.readSingleVector3();
                                if (LevelNavigation.tryGetBounds(vector, out var bound) && LevelNavigation.checkNavigation(vector))
                                {
                                    zombies[bound].Add(new ZombieSpawnpoint(newType3, vector));
                                }
                            }
                        }
                    }
                }
                river3.closeRiver();
                return;
            }
            for (byte b22 = 0; b22 < Regions.WORLD_SIZE; b22++)
            {
                for (byte b23 = 0; b23 < Regions.WORLD_SIZE; b23++)
                {
                    if (ReadWrite.fileExists(Level.info.path + "/Spawns/Animals_" + b22 + "_" + b23 + ".dat", useCloud: false, usePath: false))
                    {
                        River river4 = new River(Level.info.path + "/Spawns/Animals_" + b22 + "_" + b23 + ".dat", usePath: false);
                        if (river4.readByte() > 0)
                        {
                            ushort num9 = river4.readUInt16();
                            for (ushort num10 = 0; num10 < num9; num10++)
                            {
                                byte newType4 = river4.readByte();
                                Vector3 vector2 = river4.readSingleVector3();
                                if (LevelNavigation.tryGetBounds(vector2, out var bound2) && LevelNavigation.checkNavigation(vector2))
                                {
                                    zombies[bound2].Add(new ZombieSpawnpoint(newType4, vector2));
                                }
                            }
                            river4.closeRiver();
                        }
                    }
                }
            }
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_TABLE_VERSION);
        block.writeInt32(nextTableUniqueId);
        block.writeByte((byte)tables.Count);
        for (byte b = 0; b < tables.Count; b++)
        {
            ZombieTable zombieTable = tables[b];
            block.writeInt32(zombieTable.tableUniqueId);
            block.writeColor(zombieTable.color);
            block.writeString(zombieTable.name);
            block.writeBoolean(zombieTable.isMega);
            block.writeUInt16(zombieTable.health);
            block.writeByte(zombieTable.damage);
            block.writeByte(zombieTable.lootIndex);
            block.writeUInt16(zombieTable.lootID);
            block.writeUInt32(zombieTable.xp);
            block.writeSingle(zombieTable.regen);
            block.writeString(zombieTable.difficultyGUID);
            block.write((byte)zombieTable.slots.Length);
            for (byte b2 = 0; b2 < zombieTable.slots.Length; b2++)
            {
                ZombieSlot zombieSlot = zombieTable.slots[b2];
                block.writeSingle(zombieSlot.chance);
                block.writeByte((byte)zombieSlot.table.Count);
                for (byte b3 = 0; b3 < zombieSlot.table.Count; b3++)
                {
                    ZombieCloth zombieCloth = zombieSlot.table[b3];
                    block.writeUInt16(zombieCloth.item);
                }
            }
        }
        ReadWrite.writeBlock(Level.info.path + "/Spawns/Zombies.dat", useCloud: false, usePath: false, block);
        River river = new River(Level.info.path + "/Spawns/Animals.dat", usePath: false);
        river.writeByte(SAVEDATA_SPAWN_VERSION);
        for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4++)
        {
            for (byte b5 = 0; b5 < Regions.WORLD_SIZE; b5++)
            {
                List<ZombieSpawnpoint> list = spawns[b4, b5];
                river.writeUInt16((ushort)list.Count);
                for (ushort num = 0; num < list.Count; num++)
                {
                    ZombieSpawnpoint zombieSpawnpoint = list[num];
                    river.writeByte(zombieSpawnpoint.type);
                    river.writeSingleVector3(zombieSpawnpoint.point);
                }
            }
        }
        river.closeRiver();
    }
}
