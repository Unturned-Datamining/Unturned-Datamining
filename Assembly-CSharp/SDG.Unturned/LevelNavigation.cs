using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SDG.Unturned;

public class LevelNavigation
{
    public static readonly Vector3 BOUNDS_SIZE = new Vector3(64f, 64f, 64f);

    public static readonly byte SAVEDATA_BOUNDS_VERSION = 1;

    public static readonly byte SAVEDATA_FLAGS_VERSION = 4;

    internal const byte SAVEDATA_VERSION_FLAG_DATA_ADDED_HYPER_AGRO = 4;

    internal const byte SAVEDATA_VERSION_FLAG_DATA_ADDED_MAX_BOSS_COUNT = 5;

    internal const byte SAVEDATA_VERSION_FLAG_DATA_NEWEST = 5;

    public static readonly byte SAVEDATA_FLAG_DATA_VERSION = 5;

    public static readonly byte SAVEDATA_NAVIGATION_VERSION = 1;

    private static Transform _models;

    private static List<Flag> flags;

    private static List<Bounds> _bounds;

    /// <summary>
    /// Nelson 2026-03-04: the "bounds" list contains navmesh baking bounds + "BOUNDS_SIZE" expansion.
    /// Several parts of the code were looping through ASPFP active graphs and checking whether a
    /// point is within those bounds, so we'll now duplicate some bounds data for that purpose.
    /// </summary>
    private static List<Bounds> nonExpandedNavmeshBounds;

    [Obsolete("Was the parent of misc objects in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Navigation";
                _models.parent = Level.level;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelNavigation.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<Bounds> bounds => _bounds;

    public static List<FlagData> flagData { get; private set; }

    public static bool tryGetBounds(Vector3 point, out byte bound)
    {
        bound = byte.MaxValue;
        if (bounds != null)
        {
            for (byte b = 0; b < bounds.Count; b++)
            {
                if (bounds[b].ContainsXZ(point))
                {
                    bound = b;
                    return true;
                }
            }
        }
        return false;
    }

    public static bool tryGetNavigation(Vector3 point, out byte nav)
    {
        nav = byte.MaxValue;
        bool result = false;
        if (nonExpandedNavmeshBounds != null)
        {
            for (int i = 0; i < nonExpandedNavmeshBounds.Count; i++)
            {
                if (nonExpandedNavmeshBounds[i].ContainsXZ(point))
                {
                    nav = (byte)i;
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    public static bool checkSafe(byte bound)
    {
        if (bounds == null)
        {
            return false;
        }
        return bound < bounds.Count;
    }

    public static bool checkSafe(Vector3 point)
    {
        if (bounds == null)
        {
            return false;
        }
        for (byte b = 0; b < bounds.Count; b++)
        {
            if (bounds[b].ContainsXZ(point))
            {
                return true;
            }
        }
        return false;
    }

    public static bool checkSafeFakeNav(Vector3 point)
    {
        if (nonExpandedNavmeshBounds != null)
        {
            for (int i = 0; i < nonExpandedNavmeshBounds.Count; i++)
            {
                if (nonExpandedNavmeshBounds[i].ContainsXZ(point))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool checkNavigation(Vector3 point)
    {
        if (nonExpandedNavmeshBounds != null)
        {
            for (int i = 0; i < nonExpandedNavmeshBounds.Count; i++)
            {
                if (nonExpandedNavmeshBounds[i].ContainsXZ(point))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void setEnabled(bool isEnabled)
    {
        if (flags != null)
        {
            for (int i = 0; i < flags.Count; i++)
            {
                flags[i].setEnabled(isEnabled);
            }
        }
    }

    public static void updateBounds()
    {
        if (!Level.isEditor)
        {
            UnturnedLog.error("LevelNavigation.updateBounds should not be called from outside the level editor");
            return;
        }
        _bounds = new List<Bounds>(flags.Count);
        nonExpandedNavmeshBounds = new List<Bounds>(flags.Count);
        foreach (Flag flag in flags)
        {
            Bounds item = flag.CalculateBakingBounds();
            nonExpandedNavmeshBounds.Add(item);
            Bounds item2 = new Bounds(item.center, item.size + BOUNDS_SIZE);
            _bounds.Add(item2);
        }
    }

    public static Transform addFlag(Vector3 point)
    {
        IUnturnedNavmeshInterface newNavmesh = UnturnedPathfinding.Get().CreateNavmesh();
        FlagData flagData = new FlagData("", 64);
        flags.Add(new Flag(point, newNavmesh, flagData));
        LevelNavigation.flagData.Add(flagData);
        return flags[flags.Count - 1].model;
    }

    public static void removeFlag(Transform select)
    {
        for (int i = 0; i < flags.Count; i++)
        {
            if (flags[i].model == select)
            {
                for (int j = i + 1; j < flags.Count; j++)
                {
                    flags[j].needsNavigationSave = true;
                }
                try
                {
                    flags[i].remove();
                }
                catch (Exception e)
                {
                    UnturnedLog.exception(e, "Caught exception removing navmesh:");
                }
                flags.RemoveAt(i);
                flagData.RemoveAt(i);
                break;
            }
        }
        updateBounds();
    }

    public static Flag getFlag(Transform select)
    {
        for (int i = 0; i < flags.Count; i++)
        {
            if (flags[i].model == select)
            {
                return flags[i];
            }
        }
        return null;
    }

    public static void load()
    {
        _bounds = new List<Bounds>();
        nonExpandedNavmeshBounds = new List<Bounds>();
        flagData = new List<FlagData>();
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Bounds.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Environment/Bounds.dat", usePath: false);
            if (river.readByte() > 0)
            {
                byte b = river.readByte();
                for (byte b2 = 0; b2 < b; b2++)
                {
                    Vector3 center = river.readSingleVector3();
                    Vector3 vector = river.readSingleVector3();
                    bounds.Add(new Bounds(center, vector));
                    nonExpandedNavmeshBounds.Add(new Bounds(center, vector - BOUNDS_SIZE));
                }
            }
            river.closeRiver();
        }
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Flags_Data.dat", useCloud: false, usePath: false))
        {
            River river2 = new River(Level.info.path + "/Environment/Flags_Data.dat", usePath: false);
            byte b3 = river2.readByte();
            if (b3 > 0)
            {
                byte b4 = river2.readByte();
                for (byte b5 = 0; b5 < b4; b5++)
                {
                    string newDifficultyGUID = river2.readString();
                    byte newMaxZombies = 64;
                    if (b3 > 1)
                    {
                        newMaxZombies = river2.readByte();
                    }
                    bool newSpawnZombies = true;
                    if (b3 > 2)
                    {
                        newSpawnZombies = river2.readBoolean();
                    }
                    bool newHyperAgro = false;
                    if (b3 >= 4)
                    {
                        newHyperAgro = river2.readBoolean();
                    }
                    int maxBossZombies = -1;
                    if (b3 >= 5)
                    {
                        maxBossZombies = river2.readInt32();
                    }
                    flagData.Add(new FlagData(newDifficultyGUID, newMaxZombies, newSpawnZombies, newHyperAgro, maxBossZombies));
                }
            }
            river2.closeRiver();
        }
        if (flagData.Count < bounds.Count)
        {
            for (int i = flagData.Count; i < bounds.Count; i++)
            {
                flagData.Add(new FlagData("", 64));
            }
        }
        if (Level.isEditor)
        {
            flags = new List<Flag>();
            if (ReadWrite.fileExists(Level.info.path + "/Environment/Flags.dat", useCloud: false, usePath: false))
            {
                River river3 = new River(Level.info.path + "/Environment/Flags.dat", usePath: false);
                byte b6 = river3.readByte();
                if (b6 > 2)
                {
                    byte b7 = river3.readByte();
                    if (flagData.Count < b7)
                    {
                        UnturnedLog.error($"Navigation flag data count ({flagData.Count}) does not match flags count ({b7}) during editor load, fixing");
                        for (int j = flagData.Count; j < b7; j++)
                        {
                            flagData.Add(new FlagData("", 64));
                        }
                    }
                    for (byte b8 = 0; b8 < b7; b8++)
                    {
                        Vector3 newPoint = river3.readSingleVector3();
                        float num = river3.readSingle();
                        float num2 = river3.readSingle();
                        if (b6 < 4)
                        {
                            num *= 0.5f;
                            num2 *= 0.5f;
                        }
                        IUnturnedNavmeshInterface unturnedNavmeshInterface = UnturnedPathfinding.Get().CreateNavmesh();
                        if (ReadWrite.fileExists(Level.info.path + "/Environment/Navigation_" + b8.ToString(CultureInfo.InvariantCulture) + ".dat", useCloud: false, usePath: false))
                        {
                            River river4 = new River(Level.info.path + "/Environment/Navigation_" + b8.ToString(CultureInfo.InvariantCulture) + ".dat", usePath: false);
                            if (river4.readByte() > 0)
                            {
                                unturnedNavmeshInterface.Deserialize(river4);
                            }
                            river4.closeRiver();
                        }
                        flags.Add(new Flag(newPoint, num, num2, unturnedNavmeshInterface, flagData[b8]));
                    }
                }
                river3.closeRiver();
            }
            if (bounds.Count != flags.Count)
            {
                UnturnedLog.error("Navigation bounds count ({0}) does not match flags count ({1}) during editor load, fixing", bounds.Count, flags.Count);
                updateBounds();
            }
        }
        else
        {
            if (!Provider.isServer)
            {
                return;
            }
            int num3 = 0;
            int num4 = 0;
            while (num3 < 5)
            {
                string text = Level.info.path + "/Environment/Navigation_" + num4.ToString(CultureInfo.InvariantCulture) + ".dat";
                if (ReadWrite.fileExists(text, useCloud: false, usePath: false))
                {
                    River river5 = new River(text, usePath: false);
                    if (river5.readByte() > 0)
                    {
                        UnturnedPathfinding.Get().CreateNavmesh().Deserialize(river5);
                    }
                    river5.closeRiver();
                    num3 = 0;
                }
                else
                {
                    num3++;
                }
                num4++;
            }
        }
    }

    public static void save()
    {
        if (bounds.Count != flags.Count)
        {
            UnturnedLog.error("Navigation bounds count ({0}) does not match flags count ({1}) during save", bounds.Count, flags.Count);
            updateBounds();
        }
        River river = new River(Level.info.path + "/Environment/Bounds.dat", usePath: false);
        river.writeByte(SAVEDATA_BOUNDS_VERSION);
        river.writeByte((byte)bounds.Count);
        for (byte b = 0; b < bounds.Count; b++)
        {
            river.writeSingleVector3(bounds[b].center);
            river.writeSingleVector3(bounds[b].size);
        }
        river.closeRiver();
        River river2 = new River(Level.info.path + "/Environment/Flags_Data.dat", usePath: false);
        river2.writeByte(5);
        river2.writeByte((byte)flagData.Count);
        for (byte b2 = 0; b2 < flagData.Count; b2++)
        {
            river2.writeString(flagData[b2].difficultyGUID);
            river2.writeByte(flagData[b2].maxZombies);
            river2.writeBoolean(flagData[b2].spawnZombies);
            river2.writeBoolean(flagData[b2].hyperAgro);
            river2.writeInt32(flagData[b2].maxBossZombies);
        }
        river2.closeRiver();
        River river3 = new River(Level.info.path + "/Environment/Flags.dat", usePath: false);
        river3.writeByte(SAVEDATA_FLAGS_VERSION);
        int num = flags.Count;
        while (ReadWrite.fileExists(Level.info.path + "/Environment/Navigation_" + num.ToString(CultureInfo.InvariantCulture) + ".dat", useCloud: false, usePath: false))
        {
            ReadWrite.deleteFile(Level.info.path + "/Environment/Navigation_" + num.ToString(CultureInfo.InvariantCulture) + ".dat", useCloud: false, usePath: false);
            num++;
        }
        river3.writeByte((byte)flags.Count);
        for (byte b3 = 0; b3 < flags.Count; b3++)
        {
            Flag flag = flags[b3];
            river3.writeSingleVector3(flag.point);
            river3.writeSingle(flag.width);
            river3.writeSingle(flag.height);
            if (!flag.navmeshInterface.ContainsAnyBakedData)
            {
                UnturnedLog.warn($"Navmesh at {flag.point} has not been baked yet");
            }
            if (flag.needsNavigationSave)
            {
                River river4 = new River(Level.info.path + "/Environment/Navigation_" + b3.ToString(CultureInfo.InvariantCulture) + ".dat", usePath: false);
                river4.writeByte(SAVEDATA_NAVIGATION_VERSION);
                flag.navmeshInterface.Serialize(river4);
                river4.closeRiver();
                flag.needsNavigationSave = false;
            }
        }
        river3.closeRiver();
    }
}
