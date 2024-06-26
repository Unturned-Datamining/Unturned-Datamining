using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelVehicles
{
    public static readonly byte SAVEDATA_VERSION = 4;

    private static Transform _models;

    private static List<VehicleTable> _tables;

    private static List<VehicleSpawnpoint> _spawns;

    [Obsolete("Was the parent of all vehicles in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Vehicles";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelVehicles.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<VehicleTable> tables => _tables;

    public static List<VehicleSpawnpoint> spawns => _spawns;

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
            tables.Add(new VehicleTable(name));
        }
    }

    public static void removeTable()
    {
        tables.RemoveAt(EditorSpawns.selectedVehicle);
        List<VehicleSpawnpoint> list = new List<VehicleSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            VehicleSpawnpoint vehicleSpawnpoint = spawns[i];
            if (vehicleSpawnpoint.type == EditorSpawns.selectedVehicle)
            {
                UnityEngine.Object.Destroy(vehicleSpawnpoint.node.gameObject);
                continue;
            }
            if (vehicleSpawnpoint.type > EditorSpawns.selectedVehicle)
            {
                vehicleSpawnpoint.type--;
            }
            list.Add(vehicleSpawnpoint);
        }
        _spawns = list;
        EditorSpawns.selectedVehicle = 0;
        if (EditorSpawns.selectedVehicle < tables.Count)
        {
            EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = tables[EditorSpawns.selectedVehicle].color;
        }
    }

    public static void addSpawn(Vector3 point, float angle)
    {
        if (EditorSpawns.selectedVehicle < tables.Count)
        {
            spawns.Add(new VehicleSpawnpoint(EditorSpawns.selectedVehicle, point, angle));
        }
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        List<VehicleSpawnpoint> list = new List<VehicleSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            VehicleSpawnpoint vehicleSpawnpoint = spawns[i];
            if ((vehicleSpawnpoint.point - point).sqrMagnitude < radius)
            {
                UnityEngine.Object.Destroy(vehicleSpawnpoint.node.gameObject);
            }
            else
            {
                list.Add(vehicleSpawnpoint);
            }
        }
        _spawns = list;
    }

    /// <summary>
    /// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
    /// vehicle spawner requires to properly set paint color.
    /// </summary>
    public static Asset GetRandomAssetForSpawnpoint(VehicleSpawnpoint spawnpoint)
    {
        return tables[spawnpoint.type].GetRandomAsset();
    }

    public static void load()
    {
        if (!Level.isEditor && !Provider.isServer)
        {
            return;
        }
        _tables = new List<VehicleTable>();
        _spawns = new List<VehicleSpawnpoint>();
        if (!ReadWrite.fileExists(Level.info.path + "/Spawns/Vehicles.dat", useCloud: false, usePath: false))
        {
            return;
        }
        River river = new River(Level.info.path + "/Spawns/Vehicles.dat", usePath: false);
        byte b = river.readByte();
        if (b > 1 && b < 3)
        {
            river.readSteamID();
        }
        byte b2 = river.readByte();
        for (byte b3 = 0; b3 < b2; b3++)
        {
            Color newColor = river.readColor();
            string text = river.readString();
            ushort num = (ushort)((b > 3) ? river.readUInt16() : 0);
            List<VehicleTier> list = new List<VehicleTier>();
            byte b4 = river.readByte();
            for (byte b5 = 0; b5 < b4; b5++)
            {
                string newName = river.readString();
                float newChance = river.readSingle();
                List<VehicleSpawn> list2 = new List<VehicleSpawn>();
                byte b6 = river.readByte();
                for (byte b7 = 0; b7 < b6; b7++)
                {
                    ushort newVehicle = river.readUInt16();
                    list2.Add(new VehicleSpawn(newVehicle));
                }
                list.Add(new VehicleTier(list2, newName, newChance));
            }
            VehicleTable vehicleTable = new VehicleTable(list, newColor, text, num);
            tables.Add(vehicleTable);
            if (!Level.isEditor)
            {
                vehicleTable.buildTable();
            }
            if (vehicleTable.tableID != 0 && SpawnTableTool.ResolveLegacyId(num, EAssetType.VEHICLE, vehicleTable.OnGetSpawnTableValidationErrorContext) == 0 && (bool)Assets.shouldLoadAnyAssets)
            {
                Assets.reportError(Level.info.name + " vehicle table \"" + text + "\" references invalid spawn table " + num + "!");
            }
        }
        ushort num2 = river.readUInt16();
        for (int i = 0; i < num2; i++)
        {
            byte newType = river.readByte();
            Vector3 newPoint = river.readSingleVector3();
            float newAngle = river.readByte() * 2;
            spawns.Add(new VehicleSpawnpoint(newType, newPoint, newAngle));
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Spawns/Vehicles.dat", usePath: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeByte((byte)tables.Count);
        for (byte b = 0; b < tables.Count; b++)
        {
            VehicleTable vehicleTable = tables[b];
            river.writeColor(vehicleTable.color);
            river.writeString(vehicleTable.name);
            river.writeUInt16(vehicleTable.tableID);
            river.writeByte((byte)vehicleTable.tiers.Count);
            for (byte b2 = 0; b2 < vehicleTable.tiers.Count; b2++)
            {
                VehicleTier vehicleTier = vehicleTable.tiers[b2];
                river.writeString(vehicleTier.name);
                river.writeSingle(vehicleTier.chance);
                river.writeByte((byte)vehicleTier.table.Count);
                for (byte b3 = 0; b3 < vehicleTier.table.Count; b3++)
                {
                    VehicleSpawn vehicleSpawn = vehicleTier.table[b3];
                    river.writeUInt16(vehicleSpawn.vehicle);
                }
            }
        }
        river.writeUInt16((ushort)spawns.Count);
        for (int i = 0; i < spawns.Count; i++)
        {
            VehicleSpawnpoint vehicleSpawnpoint = spawns[i];
            river.writeByte(vehicleSpawnpoint.type);
            river.writeSingleVector3(vehicleSpawnpoint.point);
            river.writeByte(MeasurementTool.angleToByte(vehicleSpawnpoint.angle));
        }
        river.closeRiver();
    }

    [Obsolete("GetRandomAssetForSpawnpoint should be used instead because it properly supports guids in spawn assets.")]
    public static ushort getVehicle(VehicleSpawnpoint spawn)
    {
        return getVehicle(spawn.type);
    }

    [Obsolete("GetRandomAssetForSpawnpoint should be used instead because it properly supports guids in spawn assets.")]
    public static ushort getVehicle(byte type)
    {
        return tables[type].getVehicle();
    }
}
