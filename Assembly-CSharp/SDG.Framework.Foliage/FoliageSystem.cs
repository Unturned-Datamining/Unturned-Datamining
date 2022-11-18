using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Landscapes;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageSystem : DevkitHierarchyItemBase
{
    public static float TILE_SIZE;

    public static int TILE_SIZE_INT;

    public static int SPLATMAP_RESOLUTION_PER_TILE;

    protected static Dictionary<FoliageCoord, FoliageTile> prevTiles;

    protected static Dictionary<FoliageCoord, FoliageTile> activeTiles;

    protected static Dictionary<FoliageCoord, FoliageTile> tiles;

    protected static IFoliageStorage storage;

    protected static Queue<KeyValuePair<FoliageTile, List<IFoliageSurface>>> bakeQueue;

    protected static FoliageSystemPostBakeHandler bakeEnd;

    protected static Vector3 bakeLocalPosition;

    private static Plane[] mainCameraFrustumPlanes;

    private static Plane[] focusCameraFrustumPlanes;

    public static Vector3 focusPosition;

    public static bool isFocused;

    public static Camera focusCamera;

    public bool hiddenByHeightEditor;

    public bool hiddenByMaterialEditor;

    protected uint version;

    private const int foliageRenderLayer = 18;

    public static FoliageSystem instance { get; private set; }

    public static List<IFoliageSurface> surfaces { get; private set; }

    public static int bakeQueueProgress => bakeQueueTotal - bakeQueue.Count;

    public static int bakeQueueTotal { get; private set; }

    public static FoliageBakeSettings bakeSettings { get; private set; }

    public static event FoliageSystemPreBakeHandler preBake;

    public static event FoliageSystemPreBakeTileHandler preBakeTile;

    public static event FoliageSystemPostBakeTileHandler postBakeTile;

    public static event FoliageSystemGlobalBakeHandler globalBake;

    public static event FoliageSystemLocalBakeHandler localBake;

    public static event FoliageSystemPostBakeHandler postBake;

    public static void CreateInLevelIfMissing()
    {
        if (instance == null)
        {
            UnturnedLog.info("Adding default foliage system to level");
            LevelHierarchy.initItem(new GameObject().AddComponent<FoliageSystem>());
            if (VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().additiveVolumes.Count < 1)
            {
                UnturnedLog.info("Adding default additive foliage volume to level");
                GameObject obj = new GameObject();
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.localScale = new Vector3((int)Level.size, Landscape.TILE_HEIGHT, (int)Level.size);
                FoliageVolume foliageVolume = obj.AddComponent<FoliageVolume>();
                LevelHierarchy.initItem(foliageVolume);
                foliageVolume.mode = FoliageVolume.EFoliageVolumeMode.ADDITIVE;
            }
        }
    }

    public static void addSurface(IFoliageSurface surface)
    {
        surfaces.Add(surface);
    }

    public static void removeSurface(IFoliageSurface surface)
    {
        surfaces.Remove(surface);
    }

    [Obsolete]
    public static void addCut(IShapeVolume cut)
    {
    }

    internal static void AddCut(FoliageCut cut)
    {
        for (int i = cut.foliageBounds.min.x; i <= cut.foliageBounds.max.x; i++)
        {
            for (int j = cut.foliageBounds.min.y; j <= cut.foliageBounds.max.y; j++)
            {
                getOrAddTile(new FoliageCoord(i, j)).AddCut(cut);
            }
        }
    }

    internal static void RemoveCut(FoliageCut cut)
    {
        for (int i = cut.foliageBounds.min.x; i <= cut.foliageBounds.max.x; i++)
        {
            for (int j = cut.foliageBounds.min.y; j <= cut.foliageBounds.max.y; j++)
            {
                getOrAddTile(new FoliageCoord(i, j)).RemoveCut(cut);
            }
        }
    }

    private static Dictionary<FoliageTile, List<IFoliageSurface>> getTileSurfacePairs()
    {
        Dictionary<FoliageTile, List<IFoliageSurface>> dictionary = new Dictionary<FoliageTile, List<IFoliageSurface>>();
        foreach (KeyValuePair<FoliageCoord, FoliageTile> tile in tiles)
        {
            FoliageTile value = tile.Value;
            if (VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsTileBakeable(value))
            {
                dictionary.Add(value, new List<IFoliageSurface>());
            }
        }
        foreach (IFoliageSurface surface in surfaces)
        {
            FoliageBounds foliageSurfaceBounds = surface.getFoliageSurfaceBounds();
            for (int i = foliageSurfaceBounds.min.x; i <= foliageSurfaceBounds.max.x; i++)
            {
                for (int j = foliageSurfaceBounds.min.y; j <= foliageSurfaceBounds.max.y; j++)
                {
                    FoliageTile orAddTile = getOrAddTile(new FoliageCoord(i, j));
                    if (VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsTileBakeable(orAddTile))
                    {
                        if (!dictionary.TryGetValue(orAddTile, out var value2))
                        {
                            value2 = new List<IFoliageSurface>();
                            dictionary.Add(orAddTile, value2);
                        }
                        value2.Add(surface);
                    }
                }
            }
        }
        return dictionary;
    }

    private static void bakePre()
    {
        if (FoliageSystem.preBake != null)
        {
            FoliageSystem.preBake();
        }
        bakeQueue.Clear();
    }

    public static void bakeGlobal(FoliageBakeSettings bakeSettings)
    {
        CreateInLevelIfMissing();
        FoliageSystem.bakeSettings = bakeSettings;
        bakePre();
        bakeGlobalBegin();
    }

    private static void bakeGlobalBegin()
    {
        foreach (KeyValuePair<FoliageTile, List<IFoliageSurface>> tileSurfacePair in getTileSurfacePairs())
        {
            bakeQueue.Enqueue(tileSurfacePair);
        }
        bakeQueueTotal = bakeQueue.Count;
        bakeEnd = bakeGlobalEnd;
        bakeEnd();
    }

    private static void bakeGlobalEnd()
    {
        if (FoliageSystem.globalBake != null)
        {
            FoliageSystem.globalBake();
        }
        bakePost();
    }

    public static void bakeLocal(FoliageBakeSettings bakeSettings)
    {
        CreateInLevelIfMissing();
        FoliageSystem.bakeSettings = bakeSettings;
        bakePre();
        bakeLocalBegin();
    }

    private static void bakeLocalBegin()
    {
        bakeLocalPosition = MainCamera.instance.transform.position;
        int num = 6;
        int num2 = num * num;
        FoliageCoord foliageCoord = new FoliageCoord(bakeLocalPosition);
        Dictionary<FoliageTile, List<IFoliageSurface>> tileSurfacePairs = getTileSurfacePairs();
        for (int i = -num; i <= num; i++)
        {
            for (int j = -num; j <= num; j++)
            {
                if (i * i + j * j <= num2)
                {
                    FoliageTile tile = getTile(new FoliageCoord(foliageCoord.x + i, foliageCoord.y + j));
                    if (tile != null && tileSurfacePairs.TryGetValue(tile, out var value))
                    {
                        KeyValuePair<FoliageTile, List<IFoliageSurface>> item = new KeyValuePair<FoliageTile, List<IFoliageSurface>>(tile, value);
                        bakeQueue.Enqueue(item);
                    }
                }
            }
        }
        bakeQueueTotal = bakeQueue.Count;
        bakeEnd = bakeLocalEnd;
        bakeEnd();
    }

    private static void bakeLocalEnd()
    {
        if (FoliageSystem.localBake != null)
        {
            FoliageSystem.localBake(bakeLocalPosition);
        }
        bakePost();
    }

    public static void bakeCancel()
    {
        if (bakeQueue.Count != 0)
        {
            bakeQueue.Clear();
            bakeEnd();
        }
    }

    private static void bakePreTile(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
    {
        if (bakeSettings.bakeInstancesMeshes)
        {
            if (bakeSettings.bakeApplyScale)
            {
                foliageTile.applyScale();
            }
            else
            {
                foliageTile.clearGeneratedInstances();
            }
        }
    }

    private static void bake(FoliageTile tile, List<IFoliageSurface> list)
    {
        bakePreTile(bakeSettings, tile);
        if (FoliageSystem.preBakeTile != null)
        {
            FoliageSystem.preBakeTile(bakeSettings, tile);
        }
        if (!bakeSettings.bakeApplyScale)
        {
            foreach (IFoliageSurface item in list)
            {
                item.bakeFoliageSurface(bakeSettings, tile);
            }
        }
        if (FoliageSystem.postBakeTile != null)
        {
            FoliageSystem.postBakeTile(bakeSettings, tile);
        }
    }

    private static void bakePost()
    {
        if (LevelHierarchy.instance != null)
        {
            LevelHierarchy.instance.isDirty = true;
        }
        if (FoliageSystem.postBake != null)
        {
            FoliageSystem.postBake();
        }
    }

    public static void addInstance(AssetReference<FoliageInstancedMeshInfoAsset> assetReference, Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
    {
        FoliageTile orAddTile = getOrAddTile(position);
        Matrix4x4 newMatrix = Matrix4x4.TRS(position, rotation, scale);
        orAddTile.addInstance(new FoliageInstanceGroup(assetReference, newMatrix, clearWhenBaked));
    }

    protected static void shutdownStorage()
    {
        if (storage != null)
        {
            storage.Shutdown();
            storage = null;
        }
    }

    protected static void clearAndReleaseTiles()
    {
        foreach (KeyValuePair<FoliageCoord, FoliageTile> tile in tiles)
        {
            tile.Value.clearAndReleaseInstances();
        }
        tiles.Clear();
    }

    public static FoliageTile getOrAddTile(Vector3 worldPosition)
    {
        return getOrAddTile(new FoliageCoord(worldPosition));
    }

    public static FoliageTile getTile(Vector3 worldPosition)
    {
        return getTile(new FoliageCoord(worldPosition));
    }

    public static FoliageTile getOrAddTile(FoliageCoord tileCoord)
    {
        if (!tiles.TryGetValue(tileCoord, out var value))
        {
            value = new FoliageTile(tileCoord);
            tiles.Add(tileCoord, value);
        }
        return value;
    }

    public static FoliageTile getTile(FoliageCoord tileCoord)
    {
        tiles.TryGetValue(tileCoord, out var value);
        return value;
    }

    public override void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader.containsKey("Version"))
        {
            version = reader.readValue<uint>("Version");
        }
        else
        {
            version = 1u;
        }
        int num = reader.readArrayLength("Tiles");
        if (instance != this)
        {
            UnturnedLog.warn("Level contains multiple FoliageSystems. Ignoring {0} tile(s) with instance ID: {1}", num, instanceID);
            return;
        }
        if (version == 2)
        {
            storage = new FoliageStorageV2();
        }
        else
        {
            storage = new FoliageStorageV1();
        }
        storage.Initialize();
        shutdownStorage();
    }

    public override void write(IFormattedFileWriter writer)
    {
        if (storage == null || version < 2)
        {
            new FoliageStorageV2().EditorSaveAllTiles(tiles.Values);
            version = 2u;
        }
        else
        {
            storage.EditorSaveAllTiles(tiles.Values);
        }
        writer.beginObject();
        writer.writeValue("Version", version);
        writer.beginArray("Tiles");
        foreach (KeyValuePair<FoliageCoord, FoliageTile> tile in tiles)
        {
            FoliageTile value = tile.Value;
            writer.writeValue(value);
        }
        writer.endArray();
        writer.endObject();
    }

    protected void tickBakeQueue()
    {
        KeyValuePair<FoliageTile, List<IFoliageSurface>> keyValuePair = bakeQueue.Dequeue();
        bake(keyValuePair.Key, keyValuePair.Value);
        if (bakeQueue.Count == 0)
        {
            bakeEnd();
        }
    }

    protected void OnEnable()
    {
        LevelHierarchy.addItem(this);
    }

    protected void OnDisable()
    {
        LevelHierarchy.removeItem(this);
    }

    protected void Awake()
    {
        base.name = "Foliage_System";
        base.gameObject.layer = 20;
        if (instance == null)
        {
            instance = this;
            prevTiles.Clear();
            activeTiles.Clear();
            bakeQueue.Clear();
            shutdownStorage();
            clearAndReleaseTiles();
        }
    }

    protected void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            prevTiles.Clear();
            activeTiles.Clear();
            bakeQueue.Clear();
            shutdownStorage();
            clearAndReleaseTiles();
        }
    }

    static FoliageSystem()
    {
        TILE_SIZE = 32f;
        TILE_SIZE_INT = 32;
        SPLATMAP_RESOLUTION_PER_TILE = 8;
        prevTiles = new Dictionary<FoliageCoord, FoliageTile>();
        activeTiles = new Dictionary<FoliageCoord, FoliageTile>();
        tiles = new Dictionary<FoliageCoord, FoliageTile>();
        storage = null;
        bakeQueue = new Queue<KeyValuePair<FoliageTile, List<IFoliageSurface>>>();
        mainCameraFrustumPlanes = new Plane[6];
        focusCameraFrustumPlanes = new Plane[6];
        surfaces = new List<IFoliageSurface>();
    }
}
