using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelPlayers
{
    public static readonly byte SAVEDATA_VERSION = 4;

    private static Transform _models;

    private static List<PlayerSpawnpoint> _spawns;

    [Obsolete("Was the parent of all players in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Players";
                _models.parent = Level.spawns;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelPlayers.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<PlayerSpawnpoint> spawns => _spawns;

    public static void setEnabled(bool isEnabled)
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            spawns[i].setEnabled(isEnabled);
        }
    }

    public static bool checkCanBuild(Vector3 point)
    {
        float num = 256f;
        if (Level.info != null && Level.info.configData != null)
        {
            float prevent_Building_Near_Spawnpoint_Radius = Level.info.configData.Prevent_Building_Near_Spawnpoint_Radius;
            num = prevent_Building_Near_Spawnpoint_Radius * prevent_Building_Near_Spawnpoint_Radius;
        }
        for (int i = 0; i < spawns.Count; i++)
        {
            if ((spawns[i].point - point).sqrMagnitude < num)
            {
                return false;
            }
        }
        return true;
    }

    public static void addSpawn(Vector3 point, float angle, bool isAlt)
    {
        spawns.Add(new PlayerSpawnpoint(point, angle, isAlt));
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        List<PlayerSpawnpoint> list = new List<PlayerSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            PlayerSpawnpoint playerSpawnpoint = spawns[i];
            if ((playerSpawnpoint.point - point).sqrMagnitude < radius)
            {
                UnityEngine.Object.Destroy(playerSpawnpoint.node.gameObject);
            }
            else
            {
                list.Add(playerSpawnpoint);
            }
        }
        _spawns = list;
    }

    public static List<PlayerSpawnpoint> getRegSpawns()
    {
        List<PlayerSpawnpoint> list = new List<PlayerSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            PlayerSpawnpoint playerSpawnpoint = spawns[i];
            if (!playerSpawnpoint.isAlt)
            {
                list.Add(playerSpawnpoint);
            }
        }
        return list;
    }

    public static List<PlayerSpawnpoint> getAltSpawns()
    {
        List<PlayerSpawnpoint> list = new List<PlayerSpawnpoint>();
        for (int i = 0; i < spawns.Count; i++)
        {
            PlayerSpawnpoint playerSpawnpoint = spawns[i];
            if (playerSpawnpoint.isAlt)
            {
                list.Add(playerSpawnpoint);
            }
        }
        return list;
    }

    public static PlayerSpawnpoint getSpawn(bool isAlt)
    {
        List<PlayerSpawnpoint> list = (isAlt ? getAltSpawns() : getRegSpawns());
        if (list.Count == 0)
        {
            return new PlayerSpawnpoint(new Vector3(0f, 256f, 0f), 0f, isAlt);
        }
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static void load()
    {
        _spawns = new List<PlayerSpawnpoint>();
        if (!ReadWrite.fileExists(Level.info.path + "/Spawns/Players.dat", useCloud: false, usePath: false))
        {
            return;
        }
        River river = new River(Level.info.path + "/Spawns/Players.dat", usePath: false);
        byte b = river.readByte();
        if (b > 1 && b < 3)
        {
            river.readSteamID();
        }
        int num = 0;
        int num2 = 0;
        byte b2 = river.readByte();
        for (int i = 0; i < b2; i++)
        {
            Vector3 point = river.readSingleVector3();
            float angle = river.readByte() * 2;
            bool flag = false;
            if (b > 3)
            {
                flag = river.readBoolean();
            }
            if (flag)
            {
                num2++;
            }
            else
            {
                num++;
            }
            addSpawn(point, angle, flag);
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Spawns/Players.dat", usePath: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeByte((byte)spawns.Count);
        for (int i = 0; i < spawns.Count; i++)
        {
            PlayerSpawnpoint playerSpawnpoint = spawns[i];
            river.writeSingleVector3(playerSpawnpoint.point);
            river.writeByte(MeasurementTool.angleToByte(playerSpawnpoint.angle));
            river.writeBoolean(playerSpawnpoint.isAlt);
        }
        river.closeRiver();
    }
}
