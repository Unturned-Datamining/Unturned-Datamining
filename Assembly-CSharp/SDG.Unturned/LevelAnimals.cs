using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelAnimals
{
    public static readonly byte SAVEDATA_VERSION = 3;

    private static Transform _models;

    private static List<AnimalTable> _tables;

    private static List<AnimalSpawnpoint> _spawns;

    [Obsolete("Was the parent of all animals in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Animals";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelAnimals.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<AnimalTable> tables => _tables;

    public static List<AnimalSpawnpoint> spawns => _spawns;

    public static void setEnabled(bool isEnabled)
    {
        if (spawns != null)
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                spawns[i].setEnabled(isEnabled);
            }
        }
    }

    public static void addTable(string name)
    {
        if (tables.Count != 255)
        {
            tables.Add(new AnimalTable(name));
        }
    }

    public static void removeTable()
    {
        tables.RemoveAt(EditorSpawns.selectedAnimal);
        List<AnimalSpawnpoint> list = new List<AnimalSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            AnimalSpawnpoint animalSpawnpoint = spawns[i];
            if (animalSpawnpoint.type == EditorSpawns.selectedAnimal)
            {
                UnityEngine.Object.Destroy(animalSpawnpoint.node.gameObject);
                continue;
            }
            if (animalSpawnpoint.type > EditorSpawns.selectedAnimal)
            {
                animalSpawnpoint.type--;
            }
            list.Add(animalSpawnpoint);
        }
        _spawns = list;
        EditorSpawns.selectedAnimal = 0;
        if (EditorSpawns.selectedAnimal < tables.Count)
        {
            EditorSpawns.animalSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedAnimal].color;
        }
    }

    public static void addSpawn(Vector3 point)
    {
        if (EditorSpawns.selectedAnimal < tables.Count)
        {
            spawns.Add(new AnimalSpawnpoint(EditorSpawns.selectedAnimal, point));
        }
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        List<AnimalSpawnpoint> list = new List<AnimalSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            AnimalSpawnpoint animalSpawnpoint = spawns[i];
            if ((animalSpawnpoint.point - point).sqrMagnitude < radius)
            {
                UnityEngine.Object.Destroy(animalSpawnpoint.node.gameObject);
            }
            else
            {
                list.Add(animalSpawnpoint);
            }
        }
        _spawns = list;
    }

    public static ushort getAnimal(AnimalSpawnpoint spawn)
    {
        return getAnimal(spawn.type);
    }

    public static ushort getAnimal(byte type)
    {
        return tables[type].getAnimal();
    }

    public static void load()
    {
        if (!Level.isEditor && !Provider.isServer)
        {
            return;
        }
        _tables = new List<AnimalTable>();
        _spawns = new List<AnimalSpawnpoint>();
        if (!ReadWrite.fileExists(Level.info.path + "/Spawns/Fauna.dat", useCloud: false, usePath: false))
        {
            return;
        }
        River river = new River(Level.info.path + "/Spawns/Fauna.dat", usePath: false);
        byte b = river.readByte();
        byte b2 = river.readByte();
        for (byte b3 = 0; b3 < b2; b3 = (byte)(b3 + 1))
        {
            Color newColor = river.readColor();
            string text = river.readString();
            ushort num;
            if (b > 2)
            {
                num = river.readUInt16();
                if (num != 0 && SpawnTableTool.resolve(num) == 0 && (bool)Assets.shouldLoadAnyAssets)
                {
                    Assets.reportError(Level.info.name + " animal table \"" + text + "\" references invalid spawn table " + num + "!");
                }
            }
            else
            {
                num = 0;
            }
            List<AnimalTier> list = new List<AnimalTier>();
            byte b4 = river.readByte();
            for (byte b5 = 0; b5 < b4; b5 = (byte)(b5 + 1))
            {
                string newName = river.readString();
                float newChance = river.readSingle();
                List<AnimalSpawn> list2 = new List<AnimalSpawn>();
                byte b6 = river.readByte();
                for (byte b7 = 0; b7 < b6; b7 = (byte)(b7 + 1))
                {
                    ushort newAnimal = river.readUInt16();
                    list2.Add(new AnimalSpawn(newAnimal));
                }
                list.Add(new AnimalTier(list2, newName, newChance));
            }
            tables.Add(new AnimalTable(list, newColor, text, num));
            if (!Level.isEditor)
            {
                tables[b3].buildTable();
            }
        }
        ushort num2 = river.readUInt16();
        for (int i = 0; i < num2; i++)
        {
            byte newType = river.readByte();
            Vector3 newPoint = river.readSingleVector3();
            spawns.Add(new AnimalSpawnpoint(newType, newPoint));
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Spawns/Fauna.dat", usePath: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeByte((byte)tables.Count);
        for (byte b = 0; b < tables.Count; b = (byte)(b + 1))
        {
            AnimalTable animalTable = tables[b];
            river.writeColor(animalTable.color);
            river.writeString(animalTable.name);
            river.writeUInt16(animalTable.tableID);
            river.writeByte((byte)animalTable.tiers.Count);
            for (byte b2 = 0; b2 < animalTable.tiers.Count; b2 = (byte)(b2 + 1))
            {
                AnimalTier animalTier = animalTable.tiers[b2];
                river.writeString(animalTier.name);
                river.writeSingle(animalTier.chance);
                river.writeByte((byte)animalTier.table.Count);
                for (byte b3 = 0; b3 < animalTier.table.Count; b3 = (byte)(b3 + 1))
                {
                    AnimalSpawn animalSpawn = animalTier.table[b3];
                    river.writeUInt16(animalSpawn.animal);
                }
            }
        }
        river.writeUInt16((ushort)spawns.Count);
        for (int i = 0; i < spawns.Count; i++)
        {
            AnimalSpawnpoint animalSpawnpoint = spawns[i];
            river.writeByte(animalSpawnpoint.type);
            river.writeSingleVector3(animalSpawnpoint.point);
        }
        river.closeRiver();
    }
}
