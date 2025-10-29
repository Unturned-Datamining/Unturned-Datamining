using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class LevelObjects : MonoBehaviour
{
    /// <summary>
    /// Should objects that failed to load due to missing assets be saved?
    /// If true, a placeholder transform is created and used to save.
    /// If false, objects without assets are zeroed during save. (old default)  
    /// </summary>
    public static CommandLineFlag preserveMissingAssets = new CommandLineFlag(defaultValue: true, "-NoPreserveMissingObjects");

    private const byte SAVEDATA_VERSION_BEFORE_NAMED_VERSIONS = 10;

    private const byte SAVEDATA_VERSION_ADDED_MATERIAL_OVERRIDES = 11;

    private const byte SAVEDATA_VERSION_ADDED_PER_OBJECT_CULLING_OVERRIDES = 12;

    private const byte SAVEDATA_VERSION_NEWEST = 12;

    public static readonly byte SAVEDATA_VERSION = 12;

    public static readonly byte OBJECT_REGIONS = 3;

    private static uint availableInstanceID;

    private static IReun[] reun;

    public static int step;

    private static int frame;

    private static Transform _models;

    private static List<LevelObject>[,] _objects;

    private static List<LevelBuildableObject>[,] _buildables;

    internal static Dictionary<uint, LevelObject> instanceIdToObject;

    private static int _total;

    private static bool[,] _regions;

    private static RegionIncrementalVisibilityTracker regionTracker;

    private static RegionIncrementalVisibilityTracker skyboxRegionTracker;

    private static Dictionary<Vector2Int, RegionVisibilityData> regionTrackerData = new Dictionary<Vector2Int, RegionVisibilityData>();

    private static bool isHierarchyReady;

    public static RegionActivated onRegionActivated;

    [Obsolete("Was the parent of all objects in the past, but now empty for TransformHierarchy performance.")]
    public static Transform models
    {
        get
        {
            if (_models == null)
            {
                _models = new GameObject().transform;
                _models.name = "Objects";
                _models.parent = Level.level;
                _models.tag = "Logic";
                _models.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing LevelObjects.models which has been deprecated.");
            }
            return _models;
        }
    }

    public static List<LevelObject>[,] objects => _objects;

    public static List<LevelBuildableObject>[,] buildables => _buildables;

    public static int total => _total;

    public static bool[,] regions => _regions;

    public static float RegularObjectMaxDistance { get; set; }

    public static float SkyboxObjectMaxDistance { get; set; }

    /// <summary>
    /// Hash of Objects.dat
    /// </summary>
    public static byte[] hash { get; private set; }

    public static bool shouldInstantlyLoad { get; internal set; }

    private static uint generateUniqueInstanceID()
    {
        return availableInstanceID++;
    }

    public static bool IsRegionUpdating(Vector2Int coord)
    {
        return regionTracker.IsRegionUpdating(coord);
    }

    public static void undo()
    {
        while (frame <= reun.Length - 1)
        {
            if (reun[frame] != null)
            {
                reun[frame].undo();
            }
            if (frame < reun.Length - 1 && reun[frame + 1] != null)
            {
                frame++;
                if (reun[frame].step != step)
                {
                    step--;
                    break;
                }
                continue;
            }
            break;
        }
    }

    public static void redo()
    {
        while (frame >= 0)
        {
            if (reun[frame] != null)
            {
                reun[frame].redo();
            }
            if (frame > 0 && reun[frame - 1] != null)
            {
                frame--;
                if (reun[frame].step != step)
                {
                    step++;
                    break;
                }
                continue;
            }
            break;
        }
    }

    public static Transform register(IReun newReun)
    {
        if (frame > 0)
        {
            reun = new IReun[reun.Length];
            frame = 0;
        }
        for (int num = reun.Length - 1; num > 0; num--)
        {
            reun[num] = reun[num - 1];
        }
        reun[0] = newReun;
        return reun[0].redo();
    }

    public static void transformObject(Transform select, Vector3 toPosition, Quaternion toRotation, Vector3 toScale, Vector3 fromPosition, Quaternion fromRotation, Vector3 fromScale)
    {
        if (Regions.tryGetCoordinate(fromPosition, out var x, out var y))
        {
            if (Regions.tryGetCoordinate(toPosition, out var x2, out var y2))
            {
                LevelObject levelObject = null;
                int index = -1;
                for (int i = 0; i < objects[x, y].Count; i++)
                {
                    if (objects[x, y][i].transform == select)
                    {
                        levelObject = objects[x, y][i];
                        index = i;
                        break;
                    }
                }
                if (levelObject != null)
                {
                    if (x != x2 || y != y2)
                    {
                        objects[x, y].RemoveAt(index);
                        objects[x2, y2].Add(levelObject);
                    }
                    if (levelObject.transform != null)
                    {
                        levelObject.transform.position = toPosition;
                        levelObject.transform.rotation = toRotation;
                        levelObject.transform.localScale = toScale;
                    }
                    if (levelObject.ownedCullingVolume != null)
                    {
                        levelObject.ownedCullingVolume.OnLevelObjectMoved();
                    }
                    if (levelObject.skybox != null)
                    {
                        levelObject.skybox.position = toPosition;
                        levelObject.skybox.rotation = toRotation;
                        levelObject.skybox.localScale = toScale;
                    }
                    return;
                }
                LevelBuildableObject levelBuildableObject = null;
                int index2 = -1;
                for (int j = 0; j < buildables[x, y].Count; j++)
                {
                    if (buildables[x, y][j].transform == select)
                    {
                        levelBuildableObject = buildables[x, y][j];
                        index2 = j;
                        break;
                    }
                }
                if (levelBuildableObject != null)
                {
                    if (x != x2 || y != y2)
                    {
                        buildables[x, y].RemoveAt(index2);
                        buildables[x2, y2].Add(levelBuildableObject);
                    }
                    if (levelBuildableObject.transform != null)
                    {
                        levelBuildableObject.transform.position = toPosition;
                        levelBuildableObject.transform.rotation = toRotation;
                    }
                }
                else
                {
                    select.position = fromPosition;
                    select.rotation = fromRotation;
                    select.localScale = fromScale;
                }
            }
            else
            {
                select.position = fromPosition;
                select.rotation = fromRotation;
                select.localScale = fromScale;
            }
        }
        else
        {
            select.position = fromPosition;
            select.rotation = fromRotation;
            select.localScale = fromScale;
        }
    }

    public static void registerTransformObject(Transform select, Vector3 toPosition, Quaternion toRotation, Vector3 toScale, Vector3 fromPosition, Quaternion fromRotation, Vector3 fromScale)
    {
        register(new ReunObjectTransform(step, select, fromPosition, fromRotation, fromScale, toPosition, toRotation, toScale));
    }

    [Obsolete]
    public static DevkitHierarchyWorldObject addDevkitObject(Guid GUID, Vector3 position, Quaternion rotation, Vector3 scale, ELevelObjectPlacementOrigin placementOrigin)
    {
        addObject(position, rotation, scale, 0, GUID, placementOrigin);
        return null;
    }

    [Obsolete]
    public static void registerDevkitObject(LevelObject levelObject, out byte x, out byte y)
    {
        if (Regions.tryGetCoordinate(levelObject.transform.position, out x, out y))
        {
            objects[x, y].Add(levelObject);
            if (regions[x, y])
            {
                levelObject.SetIsActiveInRegion(isActive: true);
            }
            else
            {
                levelObject.SetIsActiveInRegion(isActive: false);
            }
        }
        else
        {
            levelObject.SetIsActiveInRegion(isActive: true);
        }
    }

    [Obsolete]
    public static void moveDevkitObject(LevelObject levelObject, byte old_x, byte old_y, byte new_x, byte new_y)
    {
        if (Regions.checkSafe(old_x, old_y))
        {
            objects[old_x, old_y].Remove(levelObject);
        }
        objects[new_x, new_y].Add(levelObject);
    }

    [Obsolete]
    public static void unregisterDevkitObject(LevelObject levelObject, byte x, byte y)
    {
        if (Regions.checkSafe(x, y))
        {
            objects[x, y].Remove(levelObject);
        }
    }

    [Obsolete]
    public static Transform addObject(Vector3 position, Quaternion rotation, Vector3 scale, ushort id, string name, Guid GUID, ELevelObjectPlacementOrigin placementOrigin)
    {
        return addObject(position, rotation, scale, id, GUID, placementOrigin);
    }

    internal static Transform addObject(Vector3 position, Quaternion rotation, Vector3 scale, ushort id, Guid GUID, ELevelObjectPlacementOrigin placementOrigin)
    {
        if (Regions.tryGetCoordinate(position, out var x, out var y))
        {
            LevelObject levelObject = new LevelObject(position, rotation, scale, id, GUID, placementOrigin, generateUniqueInstanceID(), AssetReference<MaterialPaletteAsset>.invalid, -1, NetId.INVALID, isOwnedCullingVolumeAllowed: true);
            levelObject.SetIsActiveInRegion(isActive: true);
            objects[x, y].Add(levelObject);
            _total++;
            return levelObject.transform;
        }
        return null;
    }

    public static Transform addBuildable(Vector3 position, Quaternion rotation, ushort id)
    {
        if (Regions.tryGetCoordinate(position, out var x, out var y))
        {
            LevelBuildableObject levelBuildableObject = new LevelBuildableObject(position, rotation, id);
            levelBuildableObject.enable();
            buildables[x, y].Add(levelBuildableObject);
            _total++;
            return levelBuildableObject.transform;
        }
        return null;
    }

    public static Transform registerAddObject(Vector3 position, Quaternion rotation, Vector3 scale, ObjectAsset objectAsset, ItemAsset itemAsset)
    {
        return register(new ReunObjectAdd(step, objectAsset, itemAsset, position, rotation, scale));
    }

    public static void removeObject(Transform select)
    {
        if (select == null || !Regions.tryGetCoordinate(select.position, out var x, out var y))
        {
            return;
        }
        for (int i = 0; i < objects[x, y].Count; i++)
        {
            if (objects[x, y][i].transform == select)
            {
                objects[x, y][i].destroy();
                objects[x, y].RemoveAt(i);
                _total--;
                break;
            }
        }
    }

    public static void removeBuildable(Transform select)
    {
        if (select == null || !Regions.tryGetCoordinate(select.position, out var x, out var y))
        {
            return;
        }
        for (int i = 0; i < buildables[x, y].Count; i++)
        {
            if (buildables[x, y][i].transform == select)
            {
                buildables[x, y][i].destroy();
                buildables[x, y].RemoveAt(i);
                _total--;
                break;
            }
        }
    }

    public static void registerRemoveObject(Transform select)
    {
        if (select == null || !Regions.tryGetCoordinate(select.position, out var x, out var y))
        {
            return;
        }
        if (select.CompareTag("Barricade") || select.CompareTag("Structure"))
        {
            for (int i = 0; i < buildables[x, y].Count; i++)
            {
                if (buildables[x, y][i].transform == select)
                {
                    register(new ReunObjectRemove(step, select, null, buildables[x, y][i].asset, select.position, select.rotation, select.localScale));
                    break;
                }
            }
            return;
        }
        for (int j = 0; j < objects[x, y].Count; j++)
        {
            if (objects[x, y][j].transform == select)
            {
                register(new ReunObjectRemove(step, select, objects[x, y][j].asset, null, select.position, select.rotation, select.localScale));
                break;
            }
        }
    }

    public static ObjectAsset getAsset(Transform select)
    {
        if (select != null)
        {
            select = select.root;
        }
        if (select != null && Regions.tryGetCoordinate(select.position, out var x, out var y))
        {
            for (int i = 0; i < objects[x, y].Count; i++)
            {
                if (objects[x, y][i].transform == select)
                {
                    return objects[x, y][i].asset;
                }
            }
        }
        return null;
    }

    public static void getAssetEditor(Transform select, out ObjectAsset objectAsset, out ItemAsset itemAsset)
    {
        objectAsset = null;
        itemAsset = null;
        if (select == null || !Regions.tryGetCoordinate(select.position, out var x, out var y))
        {
            return;
        }
        if (select.CompareTag("Barricade") || select.CompareTag("Structure"))
        {
            for (int i = 0; i < buildables[x, y].Count; i++)
            {
                if (buildables[x, y][i].transform == select)
                {
                    itemAsset = buildables[x, y][i].asset;
                    break;
                }
            }
            return;
        }
        for (int j = 0; j < objects[x, y].Count; j++)
        {
            if (objects[x, y][j].transform == select)
            {
                objectAsset = objects[x, y][j].asset;
                break;
            }
        }
    }

    internal static LevelObject FindLevelObject(GameObject rootGameObject)
    {
        if (rootGameObject == null)
        {
            return null;
        }
        Transform transform = rootGameObject.transform;
        if (Regions.tryGetCoordinate(transform.position, out var x, out var y))
        {
            for (int i = 0; i < objects[x, y].Count; i++)
            {
                if (objects[x, y][i].transform == transform)
                {
                    return objects[x, y][i];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Note: refers to per-LevelObject unique ID, not Unity object instance ID.
    /// </summary>
    public static LevelObject FindLevelObjectByInstanceId(uint instanceId)
    {
        if (instanceIdToObject == null)
        {
            return null;
        }
        instanceIdToObject.TryGetValue(instanceId, out var value);
        return value;
    }

    public static void load()
    {
        _objects = new List<LevelObject>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        _buildables = new List<LevelBuildableObject>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        instanceIdToObject = new Dictionary<uint, LevelObject>();
        _total = 0;
        _regions = new bool[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        shouldInstantlyLoad = true;
        isHierarchyReady = false;
        regionTracker = new RegionIncrementalVisibilityTracker();
        skyboxRegionTracker = new RegionIncrementalVisibilityTracker();
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                objects[b, b2] = new List<LevelObject>();
                buildables[b, b2] = new List<LevelBuildableObject>();
            }
        }
        hash = new byte[20];
        if (ReadWrite.fileExists(Level.info.path + "/Level/Objects.dat", useCloud: false, usePath: false))
        {
            River river = new River(Level.info.path + "/Level/Objects.dat", usePath: false);
            byte b3 = river.readByte();
            LegacyObjectRedirectorMap legacyObjectRedirectorMap = null;
            if (Level.shouldUseHolidayRedirects)
            {
                legacyObjectRedirectorMap = new LegacyObjectRedirectorMap();
            }
            bool flag = Level.isEditor && EditorAssetRedirector.HasRedirects;
            LevelBatching levelBatching = (Level.shouldUseLevelBatching ? LevelBatching.Get() : null);
            if (b3 > 0)
            {
                if (b3 > 1 && b3 < 3)
                {
                    river.readSteamID();
                }
                if (b3 > 8)
                {
                    availableInstanceID = river.readUInt32();
                }
                else
                {
                    availableInstanceID = 1u;
                }
                for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4++)
                {
                    for (byte b5 = 0; b5 < Regions.WORLD_SIZE; b5++)
                    {
                        ushort num = river.readUInt16();
                        for (ushort num2 = 0; num2 < num; num2++)
                        {
                            Vector3 vector = river.readSingleVector3();
                            Quaternion roundedIfNearlyAxisAligned = river.readSingleQuaternion().GetRoundedIfNearlyAxisAligned();
                            Vector3 newScale = ((b3 <= 3) ? Vector3.one : river.readSingleVector3().GetRoundedIfNearlyEqualToOne());
                            ushort num3 = river.readUInt16();
                            if (b3 > 5 && b3 < 10)
                            {
                                river.readString();
                            }
                            Guid guid = Guid.Empty;
                            if (b3 > 7)
                            {
                                guid = river.readGUID();
                            }
                            ELevelObjectPlacementOrigin newPlacementOrigin = ELevelObjectPlacementOrigin.MANUAL;
                            if (b3 > 6)
                            {
                                newPlacementOrigin = (ELevelObjectPlacementOrigin)river.readByte();
                            }
                            uint newInstanceID = ((b3 <= 8) ? generateUniqueInstanceID() : river.readUInt32());
                            if (legacyObjectRedirectorMap != null)
                            {
                                ObjectAsset objectAsset = legacyObjectRedirectorMap.redirect(guid);
                                if (objectAsset == null)
                                {
                                    num3 = 0;
                                    guid = Guid.Empty;
                                }
                                else
                                {
                                    num3 = objectAsset.id;
                                    guid = objectAsset.GUID;
                                }
                            }
                            else if (flag)
                            {
                                ObjectAsset objectAsset2 = EditorAssetRedirector.Redirect<ObjectAsset>(guid);
                                if (objectAsset2 != null)
                                {
                                    num3 = objectAsset2.id;
                                    guid = objectAsset2.GUID;
                                }
                            }
                            AssetReference<MaterialPaletteAsset> customMaterialOverride;
                            int materialIndexOverride;
                            if (b3 >= 11)
                            {
                                customMaterialOverride = new AssetReference<MaterialPaletteAsset>(river.readGUID());
                                materialIndexOverride = river.readInt32();
                            }
                            else
                            {
                                customMaterialOverride = default(AssetReference<MaterialPaletteAsset>);
                                materialIndexOverride = -1;
                            }
                            bool isOwnedCullingVolumeAllowed = b3 < 12 || river.readBoolean();
                            if (guid != Guid.Empty || num3 != 0)
                            {
                                NetId regularObjectNetId = LevelNetIdRegistry.GetRegularObjectNetId(b4, b5, num2);
                                LevelObject levelObject = new LevelObject(vector, roundedIfNearlyAxisAligned, newScale, num3, guid, newPlacementOrigin, newInstanceID, customMaterialOverride, materialIndexOverride, regularObjectNetId, isOwnedCullingVolumeAllowed);
                                if (levelObject.asset == null && (bool)Assets.shouldLoadAnyAssets)
                                {
                                    UnturnedLog.error("Object with no asset in region {0}, {1}: {2} {3}", b4, b5, num3, guid);
                                }
                                byte b6 = b4;
                                byte b7 = b5;
                                if (Level.isEditor)
                                {
                                    if (Regions.tryGetCoordinate(vector, out var x, out var y))
                                    {
                                        if (x != b4 || y != b5)
                                        {
                                            UnturnedLog.error(num3 + " should be in " + x + ", " + y + " but was in " + b4 + ", " + b5 + "!");
                                            b6 = x;
                                            b7 = y;
                                        }
                                    }
                                    else
                                    {
                                        UnturnedLog.warn("Object '{0}' ({1}) is outside the map bounds. Position: {2}", levelObject.asset?.name, num3, vector);
                                    }
                                }
                                objects[b6, b7].Add(levelObject);
                                levelBatching?.AddLevelObject(levelObject);
                                _total++;
                            }
                        }
                    }
                }
            }
            hash = river.getHash();
            river.closeRiver();
        }
        else
        {
            for (byte b8 = 0; b8 < Regions.WORLD_SIZE; b8++)
            {
                for (byte b9 = 0; b9 < Regions.WORLD_SIZE; b9++)
                {
                    if (ReadWrite.fileExists(Level.info.path + "/Objects/Objects_" + b8 + "_" + b9 + ".dat", useCloud: false, usePath: false))
                    {
                        River river2 = new River(Level.info.path + "/Objects/Objects_" + b8 + "_" + b9 + ".dat", usePath: false);
                        if (river2.readByte() > 0)
                        {
                            ushort num4 = river2.readUInt16();
                            for (ushort num5 = 0; num5 < num4; num5++)
                            {
                                Vector3 position = river2.readSingleVector3();
                                Quaternion rotation = river2.readSingleQuaternion();
                                ushort num6 = river2.readUInt16();
                                Guid empty = Guid.Empty;
                                ELevelObjectPlacementOrigin placementOrigin = ELevelObjectPlacementOrigin.MANUAL;
                                if (num6 != 0)
                                {
                                    addObject(position, rotation, Vector3.one, num6, empty, placementOrigin);
                                }
                            }
                        }
                        river2.closeRiver();
                    }
                }
            }
        }
        if ((Provider.isServer || Level.isEditor) && ReadWrite.fileExists(Level.info.path + "/Level/Buildables.dat", useCloud: false, usePath: false))
        {
            River river3 = new River(Level.info.path + "/Level/Buildables.dat", usePath: false);
            river3.readByte();
            for (byte b10 = 0; b10 < Regions.WORLD_SIZE; b10++)
            {
                for (byte b11 = 0; b11 < Regions.WORLD_SIZE; b11++)
                {
                    ushort num7 = river3.readUInt16();
                    for (ushort num8 = 0; num8 < num7; num8++)
                    {
                        Vector3 vector2 = river3.readSingleVector3();
                        Quaternion newRotation = river3.readSingleQuaternion();
                        ushort num9 = river3.readUInt16();
                        if (num9 != 0)
                        {
                            LevelBuildableObject levelBuildableObject = new LevelBuildableObject(vector2, newRotation, num9);
                            if (levelBuildableObject.asset == null)
                            {
                                UnturnedLog.warn($"Missing asset for default buildable object ID {num9} in region ({b10}, {b11})");
                            }
                            else if (!(levelBuildableObject.asset is ItemBarricadeAsset) && !(levelBuildableObject.asset is ItemStructureAsset))
                            {
                                UnturnedLog.warn($"Default buildable object ID {num9} in region ({b10}, {b11}) loaded as {levelBuildableObject.asset.name} (this is probably an ID conflict)");
                            }
                            if (Level.isEditor)
                            {
                                if (Regions.tryGetCoordinate(vector2, out var x2, out var y2))
                                {
                                    if (x2 != b10 || y2 != b11)
                                    {
                                        UnturnedLog.error(num9 + " should be in " + x2 + ", " + y2 + " but was in " + b10 + ", " + b11 + "!");
                                        b10 = x2;
                                        b11 = y2;
                                    }
                                }
                                else
                                {
                                    UnturnedLog.warn("Buildable {0} is outside the map bounds. Position: {1}", num9, vector2);
                                }
                            }
                            buildables[b10, b11].Add(levelBuildableObject);
                            _total++;
                        }
                    }
                }
            }
            river3.closeRiver();
        }
        if (Level.isEditor)
        {
            reun = new IReun[256];
            step = 0;
            frame = 0;
        }
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Level/Objects.dat", usePath: false);
        river.writeByte(12);
        river.writeUInt32(availableInstanceID);
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                List<LevelObject> list = objects[b, b2];
                river.writeUInt16((ushort)list.Count);
                for (ushort num = 0; num < list.Count; num++)
                {
                    LevelObject levelObject = list[num];
                    Transform placeholderTransform = levelObject.transform;
                    if (placeholderTransform == null)
                    {
                        placeholderTransform = levelObject.placeholderTransform;
                    }
                    if (levelObject != null && placeholderTransform != null && (levelObject.GUID != Guid.Empty || levelObject.id != 0))
                    {
                        Vector3 position = placeholderTransform.position;
                        if (Regions.clampPositionIntoBounds(ref position))
                        {
                            UnturnedLog.warn("Object '{0}' ({1}) was clamped into map bounds. Position: {2}", levelObject.asset?.name, levelObject.id, position);
                        }
                        river.writeSingleVector3(position);
                        river.writeSingleQuaternion(placeholderTransform.rotation);
                        river.writeSingleVector3(placeholderTransform.localScale);
                        river.writeUInt16(levelObject.id);
                        river.writeGUID(levelObject.GUID);
                        river.writeByte((byte)levelObject.placementOrigin);
                        river.writeUInt32(levelObject.instanceID);
                        river.writeGUID(levelObject.customMaterialOverride.GUID);
                        river.writeInt32(levelObject.materialIndexOverride);
                        river.writeBoolean(levelObject.isOwnedCullingVolumeAllowed);
                    }
                    else
                    {
                        river.writeSingleVector3(Vector3.zero);
                        river.writeSingleQuaternion(Quaternion.identity);
                        river.writeSingleVector3(Vector3.one);
                        river.writeUInt16(0);
                        river.writeGUID(Guid.Empty);
                        river.writeByte(0);
                        river.writeUInt32(0u);
                        river.writeGUID(Guid.Empty);
                        river.writeInt32(-1);
                        river.writeBoolean(value: true);
                        UnturnedLog.error("Found invalid object at " + b + ", " + b2 + " with model: " + levelObject.transform?.ToString() + " and ID: " + levelObject.id);
                    }
                }
            }
        }
        river.closeRiver();
        River river2 = new River(Level.info.path + "/Level/Buildables.dat", usePath: false);
        river2.writeByte(SAVEDATA_VERSION);
        for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3++)
        {
            for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4++)
            {
                List<LevelBuildableObject> list2 = buildables[b3, b4];
                river2.writeUInt16((ushort)list2.Count);
                for (ushort num2 = 0; num2 < list2.Count; num2++)
                {
                    LevelBuildableObject levelBuildableObject = list2[num2];
                    if (levelBuildableObject != null && levelBuildableObject.transform != null && levelBuildableObject.id != 0)
                    {
                        river2.writeSingleVector3(levelBuildableObject.transform.position);
                        river2.writeSingleQuaternion(levelBuildableObject.transform.rotation);
                        river2.writeUInt16(levelBuildableObject.id);
                    }
                    else
                    {
                        river2.writeSingleVector3(Vector3.zero);
                        river2.writeSingleQuaternion(Quaternion.identity);
                        river2.writeUInt16(0);
                        UnturnedLog.error("Found invalid object at " + b3 + ", " + b4 + " with model: " + levelBuildableObject.transform?.ToString() + " and ID: " + levelBuildableObject.id);
                    }
                }
            }
        }
        river2.closeRiver();
    }

    private static void onRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y)
    {
        bool canIncrementIndex = true;
        onRegionUpdated(null, old_x, old_y, new_x, new_y, 0, ref canIncrementIndex);
    }

    private static void onPlayerTeleported(Player player, Vector3 position)
    {
        shouldInstantlyLoad = true;
        LevelRoads.shouldInstantlyLoad = true;
    }

    private static void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
    {
        if (step != 0)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                if (regions[b, b2] && !Regions.checkArea(b, b2, new_x, new_y, OBJECT_REGIONS))
                {
                    regions[b, b2] = false;
                    if (Level.isEditor)
                    {
                        List<LevelBuildableObject> list = buildables[b, b2];
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].disable();
                        }
                    }
                }
            }
        }
        if (Regions.checkSafe(new_x, new_y))
        {
            for (int j = new_x - OBJECT_REGIONS; j <= new_x + OBJECT_REGIONS; j++)
            {
                for (int k = new_y - OBJECT_REGIONS; k <= new_y + OBJECT_REGIONS; k++)
                {
                    if (!Regions.checkSafe((byte)j, (byte)k) || regions[j, k])
                    {
                        continue;
                    }
                    regions[j, k] = true;
                    if (Level.isEditor)
                    {
                        List<LevelBuildableObject> list2 = buildables[j, k];
                        for (int l = 0; l < list2.Count; l++)
                        {
                            list2[l].enable();
                        }
                    }
                }
            }
        }
        if (Level.isLoadingArea && Player.LocalPlayer != null && Provider.isServer)
        {
            Player.LocalPlayer.adjustStanceOrTeleportIfStuck();
        }
        Level.isLoadingArea = false;
    }

    private static void onPlayerCreated(Player player)
    {
        if (player.channel.IsLocalPlayer)
        {
            Player localPlayer = Player.LocalPlayer;
            localPlayer.onPlayerTeleported = (PlayerTeleported)Delegate.Combine(localPlayer.onPlayerTeleported, new PlayerTeleported(onPlayerTeleported));
            PlayerMovement movement = Player.LocalPlayer.movement;
            movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
        }
    }

    private static void handleEditorAreaRegistered(EditorArea area)
    {
        area.onRegionUpdated = (EditorRegionUpdated)Delegate.Combine(area.onRegionUpdated, new EditorRegionUpdated(onRegionUpdated));
    }

    private static void handleLevelHierarchyReady()
    {
        isHierarchyReady = true;
    }

    /// <summary>
    /// Called by navmesh baking to complete pending object changes that may affect which nav objects are enabled.
    /// </summary>
    internal static void ImmediatelySyncRegionalVisibility()
    {
        regionTrackerData.Clear();
        regionTracker.MaxDistance = RegularObjectMaxDistance;
        regionTracker.UpdateRegions(regionTrackerData);
        foreach (KeyValuePair<Vector2Int, RegionVisibilityData> regionTrackerDatum in regionTrackerData)
        {
            Vector2Int key = regionTrackerDatum.Key;
            if (!Regions.checkSafe(key.x, key.y))
            {
                continue;
            }
            RegionVisibilityData value = regionTrackerDatum.Value;
            foreach (LevelObject item in objects[key.x, key.y])
            {
                item.SetIsActiveInRegion(value.isInsideMask);
            }
            if (value.isInsideMask)
            {
                onRegionActivated?.Invoke((byte)key.x, (byte)key.y);
            }
        }
        regionTracker.FlushProgress();
        regionTrackerData.Clear();
        skyboxRegionTracker.MaxDistance = SkyboxObjectMaxDistance;
        skyboxRegionTracker.UpdateRegions(regionTrackerData);
        foreach (KeyValuePair<Vector2Int, RegionVisibilityData> regionTrackerDatum2 in regionTrackerData)
        {
            Vector2Int key2 = regionTrackerDatum2.Key;
            if (!Regions.checkSafe(key2.x, key2.y))
            {
                continue;
            }
            RegionVisibilityData value2 = regionTrackerDatum2.Value;
            foreach (LevelObject item2 in objects[key2.x, key2.y])
            {
                item2.SetIsSkyboxActiveInRegion(value2.isInsideMask);
            }
        }
        skyboxRegionTracker.FlushProgress();
    }

    /// <summary>
    /// Stagger regional visibility across multiple frames.
    /// </summary>
    private void tickRegionalVisibility()
    {
        Vector2Int coordinateVector2Int = Regions.GetCoordinateVector2Int(MainCamera.RenderingPosition);
        shouldInstantlyLoad |= (coordinateVector2Int - regionTracker.CameraCoord).sqrMagnitude >= 4;
        regionTracker.CameraCoord = coordinateVector2Int;
        skyboxRegionTracker.CameraCoord = coordinateVector2Int;
        if (shouldInstantlyLoad)
        {
            shouldInstantlyLoad = false;
            ImmediatelySyncRegionalVisibility();
            return;
        }
        regionTrackerData.Clear();
        regionTracker.MaxDistance = RegularObjectMaxDistance;
        regionTracker.UpdateRegions(regionTrackerData);
        foreach (KeyValuePair<Vector2Int, RegionVisibilityData> regionTrackerDatum in regionTrackerData)
        {
            Vector2Int key = regionTrackerDatum.Key;
            if (!Regions.checkSafe(key.x, key.y))
            {
                regionTracker.NotifyRegionFinishedUpdating(key);
                continue;
            }
            RegionVisibilityData value = regionTrackerDatum.Value;
            List<LevelObject> list = objects[key.x, key.y];
            if (value.progressIndex < list.Count)
            {
                list[value.progressIndex].SetIsActiveInRegion(value.isInsideMask);
                continue;
            }
            regionTracker.NotifyRegionFinishedUpdating(key);
            onRegionActivated?.Invoke((byte)key.x, (byte)key.y);
        }
        regionTrackerData.Clear();
        skyboxRegionTracker.MaxDistance = SkyboxObjectMaxDistance;
        skyboxRegionTracker.UpdateRegions(regionTrackerData);
        foreach (KeyValuePair<Vector2Int, RegionVisibilityData> regionTrackerDatum2 in regionTrackerData)
        {
            Vector2Int key2 = regionTrackerDatum2.Key;
            if (!Regions.checkSafe(key2.x, key2.y))
            {
                skyboxRegionTracker.NotifyRegionFinishedUpdating(key2);
                continue;
            }
            RegionVisibilityData value2 = regionTrackerDatum2.Value;
            List<LevelObject> list2 = objects[key2.x, key2.y];
            if (value2.progressIndex < list2.Count)
            {
                list2[value2.progressIndex].SetIsSkyboxActiveInRegion(value2.isInsideMask);
            }
            else
            {
                skyboxRegionTracker.NotifyRegionFinishedUpdating(key2);
            }
        }
    }

    private void Update()
    {
        if (Level.isLoaded && !Dedicator.IsDedicatedServer && regions != null && objects != null && isHierarchyReady && !(MainCamera.instance == null))
        {
            tickRegionalVisibility();
            LevelRoads.UpdateRegionalVisibility();
        }
    }

    public void Start()
    {
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
        EditorArea.registered += handleEditorAreaRegistered;
        LevelHierarchy.ready += handleLevelHierarchyReady;
    }
}
