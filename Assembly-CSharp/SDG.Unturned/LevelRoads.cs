using System;
using System.Collections.Generic;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned;

public class LevelRoads
{
    public static readonly byte SAVEDATA_ROADS_VERSION = 2;

    public static readonly byte SAVEDATA_PATHS_VERSION = 5;

    private static Transform _models;

    private static RoadMaterial[] _materials;

    private static List<Road> roads;

    private static bool isListeningForLandscape;

    [Obsolete("Was the parent of all roads in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Roads";
                _models.parent = Level.level;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelRoads.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static RoadMaterial[] materials => _materials;

    public static void setEnabled(bool isEnabled)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            roads[i].setEnabled(isEnabled);
        }
    }

    public static Transform addRoad(Vector3 point)
    {
        roads.Add(new Road(EditorRoads.selected, 0));
        return roads[roads.Count - 1].addVertex(0, point);
    }

    [Obsolete]
    public static void removeRoad(Transform select)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            for (int j = 0; j < roads[i].paths.Count; j++)
            {
                if (roads[i].paths[j].vertex == select)
                {
                    roads[i].remove();
                    roads.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public static void removeRoad(Road road)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i] == road)
            {
                roads[i].remove();
                roads.RemoveAt(i);
                break;
            }
        }
    }

    public static RoadMaterial getRoadMaterial(Transform road)
    {
        if (road == null || road.parent == null)
        {
            return null;
        }
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i].road == road || roads[i].road == road.parent)
            {
                return materials[roads[i].material];
            }
        }
        return null;
    }

    public static Road getRoad(int index)
    {
        if (index < 0 || index >= roads.Count)
        {
            return null;
        }
        return roads[index];
    }

    public static int getRoadIndex(Road road)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i] == road)
            {
                return i;
            }
        }
        return -1;
    }

    public static Road getRoad(Transform target, out int vertexIndex, out int tangentIndex)
    {
        vertexIndex = -1;
        tangentIndex = -1;
        for (int i = 0; i < roads.Count; i++)
        {
            Road road = roads[i];
            for (int j = 0; j < road.paths.Count; j++)
            {
                RoadPath roadPath = road.paths[j];
                if (roadPath.vertex == target)
                {
                    vertexIndex = j;
                    return road;
                }
                if (roadPath.tangents[0] == target)
                {
                    vertexIndex = j;
                    tangentIndex = 0;
                    return road;
                }
                if (roadPath.tangents[1] == target)
                {
                    vertexIndex = j;
                    tangentIndex = 1;
                    return road;
                }
            }
        }
        return null;
    }

    public static void bakeRoads()
    {
        for (int i = 0; i < roads.Count; i++)
        {
            roads[i].updatePoints();
        }
        buildMeshes();
    }

    public static void load()
    {
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Roads.unity3d", useCloud: false, usePath: false))
        {
            try
            {
                Bundle bundle = Bundles.getBundle(Level.info.path + "/Environment/Roads.unity3d", prependRoot: false);
                Texture2D[] array = bundle.loadAll<Texture2D>();
                bundle.unload();
                _materials = new RoadMaterial[array.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = new RoadMaterial(array[i]);
                }
            }
            catch (Exception e)
            {
                UnturnedLog.error("Failed to load level Roads bundle! Most likely needs to be re-built from Unity.");
                UnturnedLog.exception(e);
                _materials = new RoadMaterial[0];
            }
        }
        else
        {
            _materials = new RoadMaterial[0];
        }
        roads = new List<Road>();
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Roads.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Environment/Roads.dat", usePath: false);
            byte b = river.readByte();
            if (b > 0)
            {
                byte b2 = river.readByte();
                byte b3 = 0;
                while (b3 < b2 && b3 < materials.Length)
                {
                    materials[b3].width = river.readSingle();
                    materials[b3].height = river.readSingle();
                    materials[b3].depth = river.readSingle();
                    if (b > 1)
                    {
                        materials[b3].offset = river.readSingle();
                    }
                    materials[b3].isConcrete = river.readBoolean();
                    b3 = (byte)(b3 + 1);
                }
            }
            river.closeRiver();
        }
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Paths.dat", useCloud: false, usePath: false))
        {
            River river2 = new River(Level.info.path + "/Environment/Paths.dat", usePath: false);
            byte b4 = river2.readByte();
            if (b4 > 1)
            {
                ushort num = river2.readUInt16();
                for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
                {
                    ushort num3 = river2.readUInt16();
                    byte newMaterial = river2.readByte();
                    bool newLoop = b4 > 2 && river2.readBoolean();
                    List<RoadJoint> list = new List<RoadJoint>();
                    for (ushort num4 = 0; num4 < num3; num4 = (ushort)(num4 + 1))
                    {
                        Vector3 vertex = river2.readSingleVector3();
                        Vector3[] array2 = new Vector3[2];
                        if (b4 > 2)
                        {
                            array2[0] = river2.readSingleVector3();
                            array2[1] = river2.readSingleVector3();
                        }
                        ERoadMode mode = (ERoadMode)((b4 <= 2) ? 2 : river2.readByte());
                        float offset = ((b4 <= 4) ? 0f : river2.readSingle());
                        bool ignoreTerrain = b4 > 3 && river2.readBoolean();
                        RoadJoint item = new RoadJoint(vertex, array2, mode, offset, ignoreTerrain);
                        list.Add(item);
                    }
                    if (b4 < 3)
                    {
                        for (ushort num5 = 0; num5 < num3; num5 = (ushort)(num5 + 1))
                        {
                            RoadJoint roadJoint = list[num5];
                            if (num5 == 0)
                            {
                                roadJoint.setTangent(0, (roadJoint.vertex - list[num5 + 1].vertex).normalized * 2.5f);
                                roadJoint.setTangent(1, (list[num5 + 1].vertex - roadJoint.vertex).normalized * 2.5f);
                            }
                            else if (num5 == num3 - 1)
                            {
                                roadJoint.setTangent(0, (list[num5 - 1].vertex - roadJoint.vertex).normalized * 2.5f);
                                roadJoint.setTangent(1, (roadJoint.vertex - list[num5 - 1].vertex).normalized * 2.5f);
                            }
                            else
                            {
                                roadJoint.setTangent(0, (list[num5 - 1].vertex - roadJoint.vertex).normalized * 2.5f);
                                roadJoint.setTangent(1, (list[num5 + 1].vertex - roadJoint.vertex).normalized * 2.5f);
                            }
                        }
                    }
                    roads.Add(new Road(newMaterial, num2, newLoop, list));
                }
            }
            else if (b4 > 0)
            {
                byte b5 = river2.readByte();
                for (byte b6 = 0; b6 < b5; b6 = (byte)(b6 + 1))
                {
                    byte b7 = river2.readByte();
                    byte newMaterial2 = river2.readByte();
                    List<RoadJoint> list2 = new List<RoadJoint>();
                    for (byte b8 = 0; b8 < b7; b8 = (byte)(b8 + 1))
                    {
                        Vector3 vertex2 = river2.readSingleVector3();
                        Vector3[] tangents = new Vector3[2];
                        ERoadMode mode2 = ERoadMode.FREE;
                        RoadJoint item2 = new RoadJoint(vertex2, tangents, mode2, 0f, ignoreTerrain: false);
                        list2.Add(item2);
                    }
                    for (byte b9 = 0; b9 < b7; b9 = (byte)(b9 + 1))
                    {
                        RoadJoint roadJoint2 = list2[b9];
                        if (b9 == 0)
                        {
                            roadJoint2.setTangent(0, (roadJoint2.vertex - list2[b9 + 1].vertex).normalized * 2.5f);
                            roadJoint2.setTangent(1, (list2[b9 + 1].vertex - roadJoint2.vertex).normalized * 2.5f);
                        }
                        else if (b9 == b7 - 1)
                        {
                            roadJoint2.setTangent(0, (list2[b9 - 1].vertex - roadJoint2.vertex).normalized * 2.5f);
                            roadJoint2.setTangent(1, (roadJoint2.vertex - list2[b9 - 1].vertex).normalized * 2.5f);
                        }
                        else
                        {
                            roadJoint2.setTangent(0, (list2[b9 - 1].vertex - roadJoint2.vertex).normalized * 2.5f);
                            roadJoint2.setTangent(1, (list2[b9 + 1].vertex - roadJoint2.vertex).normalized * 2.5f);
                        }
                    }
                    roads.Add(new Road(newMaterial2, b6, newLoop: false, list2));
                }
            }
            river2.closeRiver();
        }
        if (!isListeningForLandscape)
        {
            isListeningForLandscape = true;
            Landscape.loaded += handleLandscapeLoaded;
        }
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Environment/Roads.dat", usePath: false);
        river.writeByte(SAVEDATA_ROADS_VERSION);
        river.writeByte((byte)materials.Length);
        for (byte b = 0; b < materials.Length; b = (byte)(b + 1))
        {
            river.writeSingle(materials[b].width);
            river.writeSingle(materials[b].height);
            river.writeSingle(materials[b].depth);
            river.writeSingle(materials[b].offset);
            river.writeBoolean(materials[b].isConcrete);
        }
        river.closeRiver();
        river = new River(Level.info.path + "/Environment/Paths.dat", usePath: false);
        river.writeByte(SAVEDATA_PATHS_VERSION);
        ushort num = 0;
        for (ushort num2 = 0; num2 < roads.Count; num2 = (ushort)(num2 + 1))
        {
            if (roads[num2].joints.Count > 1)
            {
                num = (ushort)(num + 1);
            }
        }
        river.writeUInt16(num);
        for (ushort num3 = 0; num3 < roads.Count; num3 = (ushort)(num3 + 1))
        {
            List<RoadJoint> joints = roads[num3].joints;
            if (joints.Count > 1)
            {
                river.writeUInt16((ushort)joints.Count);
                river.writeByte(roads[num3].material);
                river.writeBoolean(roads[num3].isLoop);
                for (ushort num4 = 0; num4 < joints.Count; num4 = (ushort)(num4 + 1))
                {
                    RoadJoint roadJoint = joints[num4];
                    river.writeSingleVector3(roadJoint.vertex);
                    river.writeSingleVector3(roadJoint.getTangent(0));
                    river.writeSingleVector3(roadJoint.getTangent(1));
                    river.writeByte((byte)roadJoint.mode);
                    river.writeSingle(roadJoint.offset);
                    river.writeBoolean(roadJoint.ignoreTerrain);
                }
            }
        }
        river.closeRiver();
    }

    private static void buildMeshes()
    {
        for (int i = 0; i < roads.Count; i++)
        {
            roads[i].buildMesh();
        }
    }

    private static void handleLandscapeLoaded()
    {
        if (Level.isEditor)
        {
            bakeRoads();
        }
        else
        {
            buildMeshes();
        }
    }
}
