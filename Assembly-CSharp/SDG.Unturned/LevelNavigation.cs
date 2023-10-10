using System;
using System.Collections.Generic;
using System.Globalization;
using Pathfinding;
using SDG.Framework.Landscapes;
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
            for (byte b = 0; b < bounds.Count; b = (byte)(b + 1))
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
        if (AstarPath.active != null)
        {
            for (byte b = 0; b < Mathf.Min(bounds.Count, AstarPath.active.graphs.Length); b = (byte)(b + 1))
            {
                if (AstarPath.active.graphs[b] != null && ((RecastGraph)AstarPath.active.graphs[b]).forcedBounds.ContainsXZ(point))
                {
                    nav = b;
                    return true;
                }
            }
        }
        return false;
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
        for (byte b = 0; b < bounds.Count; b = (byte)(b + 1))
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
        if (LevelNavigation.bounds == null)
        {
            return false;
        }
        for (byte b = 0; b < LevelNavigation.bounds.Count; b = (byte)(b + 1))
        {
            Bounds bounds = LevelNavigation.bounds[b];
            bounds.size -= BOUNDS_SIZE;
            if (bounds.ContainsXZ(point))
            {
                return true;
            }
        }
        return false;
    }

    public static bool checkNavigation(Vector3 point)
    {
        if (AstarPath.active == null)
        {
            return false;
        }
        for (byte b = 0; b < AstarPath.active.graphs.Length; b = (byte)(b + 1))
        {
            if (AstarPath.active.graphs[b] != null && ((RecastGraph)AstarPath.active.graphs[b]).forcedBounds.ContainsXZ(point))
            {
                return true;
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

    public static RecastGraph addGraph()
    {
        RecastGraph obj = (RecastGraph)AstarPath.active.astarData.AddGraph(typeof(RecastGraph));
        obj.cellSize = 0.1f;
        obj.cellHeight = 0.1f;
        obj.useTiles = true;
        obj.editorTileSize = 128;
        obj.minRegionSize = 64f;
        obj.walkableHeight = 2f;
        obj.walkableClimb = 0.75f;
        obj.characterRadius = 0.5f;
        obj.maxSlope = 75f;
        obj.maxEdgeLength = 16f;
        obj.contourMaxError = 2f;
        obj.terrainSampleSize = 1;
        obj.rasterizeTrees = false;
        obj.rasterizeMeshes = false;
        obj.rasterizeColliders = true;
        obj.colliderRasterizeDetail = 4f;
        obj.mask = RayMasks.BLOCK_NAVMESH;
        return obj;
    }

    public static void updateBounds()
    {
        _bounds = new List<Bounds>();
        for (int i = 0; i < AstarPath.active.graphs.Length; i++)
        {
            RecastGraph recastGraph = (RecastGraph)AstarPath.active.graphs[i];
            if (recastGraph != null)
            {
                bounds.Add(new Bounds(recastGraph.forcedBoundsCenter, recastGraph.forcedBoundsSize + BOUNDS_SIZE));
            }
            else
            {
                bounds.Add(new Bounds(new Vector3(20000f, 20000f, 20000f), Vector3.zero));
            }
        }
    }

    public static Transform addFlag(Vector3 point)
    {
        RecastGraph graph = null;
        Func<bool, bool> update = delegate
        {
            graph = addGraph();
            return true;
        };
        AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(update));
        AstarPath.active.FlushWorkItems();
        FlagData flagData = new FlagData("", 64);
        flags.Add(new Flag(point, graph, flagData));
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
                catch
                {
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
        flagData = new List<FlagData>();
        RecastGraph.UnturnedIsPointInsideTerrainHole = Landscape.IsPointInsideHole;
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Bounds.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Environment/Bounds.dat", usePath: false);
            if (river.readByte() > 0)
            {
                byte b = river.readByte();
                for (byte b2 = 0; b2 < b; b2 = (byte)(b2 + 1))
                {
                    Vector3 center = river.readSingleVector3();
                    Vector3 size = river.readSingleVector3();
                    bounds.Add(new Bounds(center, size));
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
                for (byte b5 = 0; b5 < b4; b5 = (byte)(b5 + 1))
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
        Func<bool, bool> update = delegate
        {
            if (Level.isEditor)
            {
                flags = new List<Flag>();
                UnityEngine.Object.Destroy(AstarPath.active.GetComponent<TileHandlerHelpers>());
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
                        for (byte b8 = 0; b8 < b7; b8 = (byte)(b8 + 1))
                        {
                            Vector3 newPoint = river3.readSingleVector3();
                            float num = river3.readSingle();
                            float num2 = river3.readSingle();
                            if (b6 < 4)
                            {
                                num *= 0.5f;
                                num2 *= 0.5f;
                            }
                            RecastGraph recastGraph = null;
                            if (ReadWrite.fileExists(Level.info.path + "/Environment/Navigation_" + b8.ToString(CultureInfo.InvariantCulture) + ".dat", useCloud: false, usePath: false))
                            {
                                River river4 = new River(Level.info.path + "/Environment/Navigation_" + b8.ToString(CultureInfo.InvariantCulture) + ".dat", usePath: false);
                                if (river4.readByte() > 0)
                                {
                                    recastGraph = buildGraph(river4);
                                }
                                river4.closeRiver();
                            }
                            if (recastGraph == null)
                            {
                                recastGraph = addGraph();
                            }
                            flags.Add(new Flag(newPoint, num, num2, recastGraph, flagData[b8]));
                        }
                    }
                    river3.closeRiver();
                }
                if (bounds.Count != AstarPath.active.graphs.Length)
                {
                    UnturnedLog.error("Navigation bounds count ({0}) does not match graph count ({1}) during editor load, fixing", bounds.Count, AstarPath.active.graphs.Length);
                    updateBounds();
                }
            }
            else if (Provider.isServer)
            {
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
                            buildGraph(river5);
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
                if (bounds.Count != AstarPath.active.graphs.Length)
                {
                    UnturnedLog.error("Navigation bounds count ({0}) does not match graph count ({1}) during server load", bounds.Count, AstarPath.active.graphs.Length);
                }
            }
            return true;
        };
        AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(update));
        AstarPath.active.FlushWorkItems();
    }

    public static void save()
    {
        if (bounds.Count != AstarPath.active.graphs.Length)
        {
            UnturnedLog.error("Navigation bounds count ({0}) does not match graph count ({1}) during save", bounds.Count, AstarPath.active.graphs.Length);
        }
        River river = new River(Level.info.path + "/Environment/Bounds.dat", usePath: false);
        river.writeByte(SAVEDATA_BOUNDS_VERSION);
        river.writeByte((byte)bounds.Count);
        for (byte b = 0; b < bounds.Count; b = (byte)(b + 1))
        {
            river.writeSingleVector3(bounds[b].center);
            river.writeSingleVector3(bounds[b].size);
        }
        river.closeRiver();
        River river2 = new River(Level.info.path + "/Environment/Flags_Data.dat", usePath: false);
        river2.writeByte(5);
        river2.writeByte((byte)flagData.Count);
        for (byte b2 = 0; b2 < flagData.Count; b2 = (byte)(b2 + 1))
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
        for (byte b3 = 0; b3 < flags.Count; b3 = (byte)(b3 + 1))
        {
            Flag flag = flags[b3];
            river3.writeSingleVector3(flag.point);
            river3.writeSingle(flag.width);
            river3.writeSingle(flag.height);
            if (flag.needsNavigationSave)
            {
                River river4 = new River(Level.info.path + "/Environment/Navigation_" + b3.ToString(CultureInfo.InvariantCulture) + ".dat", usePath: false);
                river4.writeByte(SAVEDATA_NAVIGATION_VERSION);
                RecastGraph graph = flag.graph;
                river4.writeSingleVector3(graph.forcedBoundsCenter);
                river4.writeSingleVector3(graph.forcedBoundsSize);
                river4.writeByte((byte)graph.tileXCount);
                river4.writeByte((byte)graph.tileZCount);
                RecastGraph.NavmeshTile[] tiles = graph.GetTiles();
                for (int i = 0; i < graph.tileZCount; i++)
                {
                    for (int j = 0; j < graph.tileXCount; j++)
                    {
                        RecastGraph.NavmeshTile navmeshTile = tiles[j + i * graph.tileXCount];
                        river4.writeUInt16((ushort)navmeshTile.tris.Length);
                        for (int k = 0; k < navmeshTile.tris.Length; k++)
                        {
                            river4.writeUInt16((ushort)navmeshTile.tris[k]);
                        }
                        river4.writeUInt16((ushort)navmeshTile.verts.Length);
                        for (int l = 0; l < navmeshTile.verts.Length; l++)
                        {
                            Int3 @int = navmeshTile.verts[l];
                            river4.writeInt32(@int.x);
                            river4.writeInt32(@int.y);
                            river4.writeInt32(@int.z);
                        }
                    }
                }
                river4.closeRiver();
                flag.needsNavigationSave = false;
            }
        }
        river3.closeRiver();
    }

    private static RecastGraph buildGraph(River river)
    {
        RecastGraph recastGraph = addGraph();
        int graphIndex = AstarPath.active.astarData.GetGraphIndex(recastGraph);
        TriangleMeshNode.SetNavmeshHolder(graphIndex, recastGraph);
        recastGraph.forcedBoundsCenter = river.readSingleVector3();
        recastGraph.forcedBoundsSize = river.readSingleVector3();
        recastGraph.tileXCount = river.readByte();
        recastGraph.tileZCount = river.readByte();
        RecastGraph.NavmeshTile[] array = new RecastGraph.NavmeshTile[recastGraph.tileXCount * recastGraph.tileZCount];
        recastGraph.SetTiles(array);
        for (int i = 0; i < recastGraph.tileZCount; i++)
        {
            for (int j = 0; j < recastGraph.tileXCount; j++)
            {
                RecastGraph.NavmeshTile navmeshTile = new RecastGraph.NavmeshTile();
                navmeshTile.x = j;
                navmeshTile.z = i;
                navmeshTile.w = 1;
                navmeshTile.d = 1;
                navmeshTile.bbTree = new BBTree(navmeshTile);
                int num = j + i * recastGraph.tileXCount;
                array[num] = navmeshTile;
                navmeshTile.tris = new int[river.readUInt16()];
                for (int k = 0; k < navmeshTile.tris.Length; k++)
                {
                    navmeshTile.tris[k] = river.readUInt16();
                }
                navmeshTile.verts = new Int3[river.readUInt16()];
                for (int l = 0; l < navmeshTile.verts.Length; l++)
                {
                    navmeshTile.verts[l] = new Int3(river.readInt32(), river.readInt32(), river.readInt32());
                }
                navmeshTile.nodes = new TriangleMeshNode[navmeshTile.tris.Length / 3];
                num <<= 12;
                for (int m = 0; m < navmeshTile.nodes.Length; m++)
                {
                    navmeshTile.nodes[m] = new TriangleMeshNode(AstarPath.active);
                    TriangleMeshNode triangleMeshNode = navmeshTile.nodes[m];
                    triangleMeshNode.GraphIndex = (uint)graphIndex;
                    triangleMeshNode.Penalty = 0u;
                    triangleMeshNode.Walkable = true;
                    triangleMeshNode.v0 = navmeshTile.tris[m * 3] | num;
                    triangleMeshNode.v1 = navmeshTile.tris[m * 3 + 1] | num;
                    triangleMeshNode.v2 = navmeshTile.tris[m * 3 + 2] | num;
                    triangleMeshNode.UpdatePositionFromVertices();
                    navmeshTile.bbTree.Insert(triangleMeshNode);
                }
                recastGraph.CreateNodeConnections(navmeshTile.nodes);
            }
        }
        for (int n = 0; n < recastGraph.tileZCount; n++)
        {
            for (int num2 = 0; num2 < recastGraph.tileXCount; num2++)
            {
                RecastGraph.NavmeshTile tile = array[num2 + n * recastGraph.tileXCount];
                recastGraph.ConnectTileWithNeighbours(tile);
            }
        }
        return recastGraph;
    }
}
