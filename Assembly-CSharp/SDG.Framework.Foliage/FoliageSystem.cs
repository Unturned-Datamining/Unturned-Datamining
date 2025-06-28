using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Landscapes;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Framework.Foliage;

public class FoliageSystem : DevkitHierarchyItemBase
{
    public static float TILE_SIZE;

    public static int TILE_SIZE_INT;

    public static int SPLATMAP_RESOLUTION_PER_TILE;

    protected static Dictionary<FoliageCoord, FoliageTile> previousFrameRelevantTiles;

    protected static Dictionary<FoliageCoord, FoliageTile> relevantTiles;

    protected static Dictionary<FoliageCoord, FoliageTile> tiles;

    /// <summary>
    /// Implementation of tile data storage.
    /// </summary>
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

    /// <summary>
    /// Nelson 2025-04-22: instanced foliage rendering is a decent chunk of CPU time. In retrospect this seems like
    /// an obvious optimization: Graphics.DrawMeshInstanced accepts up to 1023 instances per call. Each tile
    /// groups instances in lists of up to 1023, but often isn't that high. Now, we collect instances until we
    /// hit the 1023 limit. This is particularly useful for sparse variants like colored flowers.
    /// With a consistent camera transform ("/copycameratransform") on an upcoming map remaster I went from between
    /// 0.72-0.8 ms on my PC to 0.55-0.6 ms!
    /// </summary>
    private static Dictionary<FoliageInstancingBatchConfig, FoliageInstancingBatchData> batches;

    private static Stack<Matrix4x4[]> activeMatrixLists;

    private static Stack<Matrix4x4[]> matrixListPool;

    /// <summary>
    /// 2022-04-26: drawTiles previously looped over a square [-N, +N] from the upper-left to the bottom-right,
    /// and each tile checked radial distance. We can improve over this by pre-computing the radial offsets and
    /// starting from the center to improve responsiveness. N is [1, 5]
    /// </summary>
    private static readonly FoliageCoord[][] DRAW_OFFSETS;

    private const int MAX_MATRICES_PER_BATCH = 1023;

    /// <summary>
    /// Version number associated with this particular system instance.
    /// </summary>
    protected uint version;

    private static bool shouldDrawWithoutInstancing;

    /// <summary>
    /// 2022-04-26: this used to be environment layer, but "scope focus foliage" can draw outside that render distance
    /// so we now use the sky layer which is visible up to the far clip plane.
    /// </summary>
    private const int foliageRenderLayer = 18;

    public static FoliageSystem instance { get; private set; }

    public static List<IFoliageSurface> surfaces { get; private set; }

    public static int bakeQueueProgress => bakeQueueTotal - bakeQueue.Count;

    public static int bakeQueueTotal { get; private set; }

    /// <summary>
    /// Settings configured when starting the bake.
    /// </summary>
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
            LevelHierarchy.AssignInstanceIdAndMarkDirty(new GameObject().AddComponent<FoliageSystem>());
            if (VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().additiveVolumes.Count < 1)
            {
                UnturnedLog.info("Adding default additive foliage volume to level");
                GameObject obj = new GameObject();
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.localScale = new Vector3((int)Level.size, Landscape.TILE_HEIGHT, (int)Level.size);
                FoliageVolume foliageVolume = obj.AddComponent<FoliageVolume>();
                LevelHierarchy.AssignInstanceIdAndMarkDirty(foliageVolume);
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
            if (!surface.IsValidFoliageSurface)
            {
                continue;
            }
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
        FoliageSystem.preBake?.Invoke();
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
        FoliageSystem.globalBake?.Invoke();
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
        FoliageSystem.localBake?.Invoke(bakeLocalPosition);
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
        FoliageSystem.preBakeTile?.Invoke(bakeSettings, tile);
        if (!bakeSettings.bakeApplyScale)
        {
            foreach (IFoliageSurface item in list)
            {
                if (item.IsValidFoliageSurface)
                {
                    item.bakeFoliageSurface(bakeSettings, tile);
                }
            }
        }
        FoliageSystem.postBakeTile?.Invoke(bakeSettings, tile);
    }

    private static void bakePost()
    {
        if (LevelHierarchy.instance != null)
        {
            LevelHierarchy.instance.isDirty = true;
        }
        FoliageSystem.postBake?.Invoke();
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

    private static void drawTiles(Vector3 position, int drawDistance, Camera camera, Plane[] frustumPlanes)
    {
        batches.Clear();
        Stack<Matrix4x4[]> stack = activeMatrixLists;
        activeMatrixLists = matrixListPool;
        matrixListPool = stack;
        FoliageCoord foliageCoord = new FoliageCoord(position);
        FoliageCoord[] array = DRAW_OFFSETS[drawDistance];
        for (int i = 0; i < array.Length; i++)
        {
            FoliageCoord foliageCoord2 = array[i];
            FoliageCoord foliageCoord3 = new FoliageCoord(foliageCoord.x + foliageCoord2.x, foliageCoord.y + foliageCoord2.y);
            if (relevantTiles.ContainsKey(foliageCoord3))
            {
                continue;
            }
            FoliageTile tile = getTile(foliageCoord3);
            if (tile != null)
            {
                relevantTiles.Add(foliageCoord3, tile);
                if (!tile.isRelevantToViewer)
                {
                    tile.isRelevantToViewer = true;
                    storage?.TileBecameRelevantToViewer(tile);
                }
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, tile.worldBounds))
                {
                    int sqrDistance = foliageCoord2.x * foliageCoord2.x + foliageCoord2.y * foliageCoord2.y;
                    drawTile(tile, sqrDistance, camera);
                }
            }
        }
        foreach (KeyValuePair<FoliageInstancingBatchConfig, FoliageInstancingBatchData> batch in batches)
        {
            FoliageInstancingBatchData value = batch.Value;
            if (value.count >= 1)
            {
                FoliageInstancingBatchConfig config = batch.Key;
                DrawInstances(in config, value.list, value.count, camera);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void drawTile(FoliageTile tile, int sqrDistance, Camera camera)
    {
        foreach (FoliageInstanceList value2 in tile.instances.Values)
        {
            int count = value2.matrices.Count;
            if (count < 1 || !value2.isLoadedAndRenderable || (value2.sqrDrawDistance != -1 && sqrDistance > value2.sqrDrawDistance))
            {
                continue;
            }
            if (count > 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    List<Matrix4x4> list = value2.matrices[i];
                    int matrixCount = Mathf.RoundToInt((float)list.Count * FoliageSettings._instanceDensity);
                    DrawInstances(in value2.batchConfig, list.GetInternalArray(), matrixCount, camera);
                }
            }
            List<Matrix4x4> list2 = value2.matrices[count - 1];
            int num = Mathf.RoundToInt((float)list2.Count * FoliageSettings._instanceDensity);
            if (num < 1)
            {
                continue;
            }
            if (!batches.TryGetValue(value2.batchConfig, out var value))
            {
                if (!matrixListPool.TryPop(out var result))
                {
                    result = new Matrix4x4[1023];
                }
                activeMatrixLists.Push(result);
                FoliageInstancingBatchData foliageInstancingBatchData = default(FoliageInstancingBatchData);
                foliageInstancingBatchData.list = result;
                foliageInstancingBatchData.count = 0;
                value = foliageInstancingBatchData;
            }
            int num2 = 1023 - value.count;
            if (num < num2)
            {
                FastMatrixCopy(list2, 0, value.list, value.count, num);
                value.count += num;
            }
            else
            {
                FastMatrixCopy(list2, 0, value.list, value.count, num2);
                DrawInstances(in value2.batchConfig, value.list, 1023, camera);
                value.count = num - num2;
                if (value.count > 0)
                {
                    FastMatrixCopy(list2, num2, value.list, 0, value.count);
                }
            }
            batches[value2.batchConfig] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe static void FastMatrixCopy(List<Matrix4x4> source, int sourceIndex, Matrix4x4[] destination, int destinationIndex, int count)
    {
        fixed (Matrix4x4* ptr = source.GetInternalArray())
        {
            fixed (Matrix4x4* ptr2 = destination)
            {
                Matrix4x4* source2 = ptr + sourceIndex;
                Matrix4x4* destination2 = ptr2 + destinationIndex;
                long destinationSizeInBytes = (1023 - destinationIndex) * sizeof(Matrix4x4);
                long sourceBytesToCopy = count * sizeof(Matrix4x4);
                Buffer.MemoryCopy(source2, destination2, destinationSizeInBytes, sourceBytesToCopy);
            }
        }
    }

    /// <param name="matrixCount">Must be within [0, MAX_MATRICES_PER_BATCH] range.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DrawInstances(in FoliageInstancingBatchConfig config, Matrix4x4[] matrices, int matrixCount, Camera camera)
    {
        if (shouldDrawWithoutInstancing)
        {
            for (int i = 0; i < matrixCount; i++)
            {
                Graphics.DrawMesh(config.mesh, matrices[i], config.material, 18, camera, 0, null, config.castShadows, receiveShadows: true);
            }
        }
        else
        {
            ShadowCastingMode castShadows = (config.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
            Graphics.DrawMeshInstanced(config.mesh, 0, config.material, matrices, matrixCount, null, castShadows, receiveShadows: true, 18, camera);
        }
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
        if (Dedicator.IsDedicatedServer)
        {
            shutdownStorage();
            return;
        }
        for (int i = 0; i < num; i++)
        {
            reader.readArrayIndex(i);
            FoliageTile foliageTile = new FoliageTile(FoliageCoord.ZERO);
            foliageTile.read(reader);
            tiles.Add(foliageTile.coord, foliageTile);
        }
        if (Level.isEditor)
        {
            storage.EditorLoadAllTiles(tiles.Values);
            if (version < 2)
            {
                LevelHierarchy.instance.isDirty = true;
            }
        }
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

    /// <summary>
    /// Automatically placing foliage onto tiles in editor.
    /// </summary>
    protected void tickBakeQueue()
    {
        KeyValuePair<FoliageTile, List<IFoliageSurface>> keyValuePair = bakeQueue.Dequeue();
        bake(keyValuePair.Key, keyValuePair.Value);
        if (bakeQueue.Count == 0)
        {
            bakeEnd();
        }
    }

    protected void Update()
    {
        if (MainCamera.instance == null || this != instance)
        {
            return;
        }
        relevantTiles.Clear();
        if (FoliageSettings.enabled && !(hiddenByHeightEditor | hiddenByMaterialEditor))
        {
            shouldDrawWithoutInstancing = FoliageSettings.forceInstancingOff || !SystemInfo.supportsInstancing;
            GeometryUtility.CalculateFrustumPlanes(MainCamera.instance, mainCameraFrustumPlanes);
            drawTiles(MainCamera.RenderingPosition, FoliageSettings.drawDistance, null, mainCameraFrustumPlanes);
            if (FoliageSettings.drawFocus && isFocused && focusCamera != null)
            {
                Plane[] frustumPlanes;
                if (MainCamera.instance == focusCamera)
                {
                    frustumPlanes = mainCameraFrustumPlanes;
                }
                else
                {
                    GeometryUtility.CalculateFrustumPlanes(focusCamera, focusCameraFrustumPlanes);
                    frustumPlanes = focusCameraFrustumPlanes;
                }
                drawTiles(focusPosition, FoliageSettings.drawFocusDistance, focusCamera, frustumPlanes);
            }
        }
        foreach (KeyValuePair<FoliageCoord, FoliageTile> previousFrameRelevantTile in previousFrameRelevantTiles)
        {
            if (!relevantTiles.ContainsKey(previousFrameRelevantTile.Key) && previousFrameRelevantTile.Value != null && previousFrameRelevantTile.Value.isRelevantToViewer)
            {
                previousFrameRelevantTile.Value.isRelevantToViewer = false;
                storage?.TileNoLongerRelevantToViewer(previousFrameRelevantTile.Value);
            }
        }
        Dictionary<FoliageCoord, FoliageTile> dictionary = previousFrameRelevantTiles;
        previousFrameRelevantTiles = relevantTiles;
        relevantTiles = dictionary;
        if (Level.isEditor && bakeQueue.Count > 0)
        {
            tickBakeQueue();
        }
        if (storage != null)
        {
            storage.Update();
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
            previousFrameRelevantTiles.Clear();
            relevantTiles.Clear();
            bakeQueue.Clear();
            batches.Clear();
            activeMatrixLists.Clear();
            matrixListPool.Clear();
            shutdownStorage();
            clearAndReleaseTiles();
        }
    }

    protected void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            previousFrameRelevantTiles.Clear();
            relevantTiles.Clear();
            bakeQueue.Clear();
            batches.Clear();
            activeMatrixLists.Clear();
            matrixListPool.Clear();
            shutdownStorage();
            clearAndReleaseTiles();
        }
    }

    static FoliageSystem()
    {
        TILE_SIZE = 32f;
        TILE_SIZE_INT = 32;
        SPLATMAP_RESOLUTION_PER_TILE = 8;
        previousFrameRelevantTiles = new Dictionary<FoliageCoord, FoliageTile>();
        relevantTiles = new Dictionary<FoliageCoord, FoliageTile>();
        tiles = new Dictionary<FoliageCoord, FoliageTile>();
        storage = null;
        bakeQueue = new Queue<KeyValuePair<FoliageTile, List<IFoliageSurface>>>();
        mainCameraFrustumPlanes = new Plane[6];
        focusCameraFrustumPlanes = new Plane[6];
        batches = new Dictionary<FoliageInstancingBatchConfig, FoliageInstancingBatchData>();
        activeMatrixLists = new Stack<Matrix4x4[]>();
        matrixListPool = new Stack<Matrix4x4[]>();
        DRAW_OFFSETS = new FoliageCoord[6][]
        {
            new FoliageCoord[0],
            new FoliageCoord[9]
            {
                new FoliageCoord(0, 0),
                new FoliageCoord(1, 0),
                new FoliageCoord(1, 1),
                new FoliageCoord(0, 1),
                new FoliageCoord(-1, 1),
                new FoliageCoord(-1, 0),
                new FoliageCoord(-1, -1),
                new FoliageCoord(0, -1),
                new FoliageCoord(1, -1)
            },
            new FoliageCoord[21]
            {
                new FoliageCoord(0, 0),
                new FoliageCoord(1, 0),
                new FoliageCoord(1, 1),
                new FoliageCoord(0, 1),
                new FoliageCoord(-1, 1),
                new FoliageCoord(-1, 0),
                new FoliageCoord(-1, -1),
                new FoliageCoord(0, -1),
                new FoliageCoord(1, -1),
                new FoliageCoord(2, -1),
                new FoliageCoord(2, 0),
                new FoliageCoord(2, 1),
                new FoliageCoord(1, 2),
                new FoliageCoord(0, 2),
                new FoliageCoord(-1, 2),
                new FoliageCoord(-2, 1),
                new FoliageCoord(-2, 0),
                new FoliageCoord(-2, -1),
                new FoliageCoord(-1, -2),
                new FoliageCoord(0, -2),
                new FoliageCoord(1, -2)
            },
            new FoliageCoord[45]
            {
                new FoliageCoord(0, 0),
                new FoliageCoord(1, 0),
                new FoliageCoord(1, 1),
                new FoliageCoord(0, 1),
                new FoliageCoord(-1, 1),
                new FoliageCoord(-1, 0),
                new FoliageCoord(-1, -1),
                new FoliageCoord(0, -1),
                new FoliageCoord(1, -1),
                new FoliageCoord(2, -1),
                new FoliageCoord(2, 0),
                new FoliageCoord(2, 1),
                new FoliageCoord(2, 2),
                new FoliageCoord(1, 2),
                new FoliageCoord(0, 2),
                new FoliageCoord(-1, 2),
                new FoliageCoord(-2, 2),
                new FoliageCoord(-2, 1),
                new FoliageCoord(-2, 0),
                new FoliageCoord(-2, -1),
                new FoliageCoord(-2, -2),
                new FoliageCoord(-1, -2),
                new FoliageCoord(0, -2),
                new FoliageCoord(1, -2),
                new FoliageCoord(2, -2),
                new FoliageCoord(3, -2),
                new FoliageCoord(3, -1),
                new FoliageCoord(3, 0),
                new FoliageCoord(3, 1),
                new FoliageCoord(3, 2),
                new FoliageCoord(2, 3),
                new FoliageCoord(1, 3),
                new FoliageCoord(0, 3),
                new FoliageCoord(-1, 3),
                new FoliageCoord(-2, 3),
                new FoliageCoord(-3, 2),
                new FoliageCoord(-3, 1),
                new FoliageCoord(-3, 0),
                new FoliageCoord(-3, -1),
                new FoliageCoord(-3, -2),
                new FoliageCoord(-2, -3),
                new FoliageCoord(-1, -3),
                new FoliageCoord(0, -3),
                new FoliageCoord(1, -3),
                new FoliageCoord(2, -3)
            },
            new FoliageCoord[69]
            {
                new FoliageCoord(0, 0),
                new FoliageCoord(1, 0),
                new FoliageCoord(1, 1),
                new FoliageCoord(0, 1),
                new FoliageCoord(-1, 1),
                new FoliageCoord(-1, 0),
                new FoliageCoord(-1, -1),
                new FoliageCoord(0, -1),
                new FoliageCoord(1, -1),
                new FoliageCoord(2, -1),
                new FoliageCoord(2, 0),
                new FoliageCoord(2, 1),
                new FoliageCoord(2, 2),
                new FoliageCoord(1, 2),
                new FoliageCoord(0, 2),
                new FoliageCoord(-1, 2),
                new FoliageCoord(-2, 2),
                new FoliageCoord(-2, 1),
                new FoliageCoord(-2, 0),
                new FoliageCoord(-2, -1),
                new FoliageCoord(-2, -2),
                new FoliageCoord(-1, -2),
                new FoliageCoord(0, -2),
                new FoliageCoord(1, -2),
                new FoliageCoord(2, -2),
                new FoliageCoord(3, -2),
                new FoliageCoord(3, -1),
                new FoliageCoord(3, 0),
                new FoliageCoord(3, 1),
                new FoliageCoord(3, 2),
                new FoliageCoord(3, 3),
                new FoliageCoord(2, 3),
                new FoliageCoord(1, 3),
                new FoliageCoord(0, 3),
                new FoliageCoord(-1, 3),
                new FoliageCoord(-2, 3),
                new FoliageCoord(-3, 3),
                new FoliageCoord(-3, 2),
                new FoliageCoord(-3, 1),
                new FoliageCoord(-3, 0),
                new FoliageCoord(-3, -1),
                new FoliageCoord(-3, -2),
                new FoliageCoord(-3, -3),
                new FoliageCoord(-2, -3),
                new FoliageCoord(-1, -3),
                new FoliageCoord(0, -3),
                new FoliageCoord(1, -3),
                new FoliageCoord(2, -3),
                new FoliageCoord(3, -3),
                new FoliageCoord(4, -2),
                new FoliageCoord(4, -1),
                new FoliageCoord(4, 0),
                new FoliageCoord(4, 1),
                new FoliageCoord(4, 2),
                new FoliageCoord(2, 4),
                new FoliageCoord(1, 4),
                new FoliageCoord(0, 4),
                new FoliageCoord(-1, 4),
                new FoliageCoord(-2, 4),
                new FoliageCoord(-4, 2),
                new FoliageCoord(-4, 1),
                new FoliageCoord(-4, 0),
                new FoliageCoord(-4, -1),
                new FoliageCoord(-4, -2),
                new FoliageCoord(-2, -4),
                new FoliageCoord(-1, -4),
                new FoliageCoord(0, -4),
                new FoliageCoord(1, -4),
                new FoliageCoord(2, -4)
            },
            new FoliageCoord[89]
            {
                new FoliageCoord(0, 0),
                new FoliageCoord(1, 0),
                new FoliageCoord(1, 1),
                new FoliageCoord(0, 1),
                new FoliageCoord(-1, 1),
                new FoliageCoord(-1, 0),
                new FoliageCoord(-1, -1),
                new FoliageCoord(0, -1),
                new FoliageCoord(1, -1),
                new FoliageCoord(2, -1),
                new FoliageCoord(2, 0),
                new FoliageCoord(2, 1),
                new FoliageCoord(2, 2),
                new FoliageCoord(1, 2),
                new FoliageCoord(0, 2),
                new FoliageCoord(-1, 2),
                new FoliageCoord(-2, 2),
                new FoliageCoord(-2, 1),
                new FoliageCoord(-2, 0),
                new FoliageCoord(-2, -1),
                new FoliageCoord(-2, -2),
                new FoliageCoord(-1, -2),
                new FoliageCoord(0, -2),
                new FoliageCoord(1, -2),
                new FoliageCoord(2, -2),
                new FoliageCoord(3, -2),
                new FoliageCoord(3, -1),
                new FoliageCoord(3, 0),
                new FoliageCoord(3, 1),
                new FoliageCoord(3, 2),
                new FoliageCoord(3, 3),
                new FoliageCoord(2, 3),
                new FoliageCoord(1, 3),
                new FoliageCoord(0, 3),
                new FoliageCoord(-1, 3),
                new FoliageCoord(-2, 3),
                new FoliageCoord(-3, 3),
                new FoliageCoord(-3, 2),
                new FoliageCoord(-3, 1),
                new FoliageCoord(-3, 0),
                new FoliageCoord(-3, -1),
                new FoliageCoord(-3, -2),
                new FoliageCoord(-3, -3),
                new FoliageCoord(-2, -3),
                new FoliageCoord(-1, -3),
                new FoliageCoord(0, -3),
                new FoliageCoord(1, -3),
                new FoliageCoord(2, -3),
                new FoliageCoord(3, -3),
                new FoliageCoord(4, -3),
                new FoliageCoord(4, -2),
                new FoliageCoord(4, -1),
                new FoliageCoord(4, 0),
                new FoliageCoord(4, 1),
                new FoliageCoord(4, 2),
                new FoliageCoord(4, 3),
                new FoliageCoord(3, 4),
                new FoliageCoord(2, 4),
                new FoliageCoord(1, 4),
                new FoliageCoord(0, 4),
                new FoliageCoord(-1, 4),
                new FoliageCoord(-2, 4),
                new FoliageCoord(-3, 4),
                new FoliageCoord(-4, 3),
                new FoliageCoord(-4, 2),
                new FoliageCoord(-4, 1),
                new FoliageCoord(-4, 0),
                new FoliageCoord(-4, -1),
                new FoliageCoord(-4, -2),
                new FoliageCoord(-4, -3),
                new FoliageCoord(-3, -4),
                new FoliageCoord(-2, -4),
                new FoliageCoord(-1, -4),
                new FoliageCoord(0, -4),
                new FoliageCoord(1, -4),
                new FoliageCoord(2, -4),
                new FoliageCoord(3, -4),
                new FoliageCoord(5, -1),
                new FoliageCoord(5, 0),
                new FoliageCoord(5, 1),
                new FoliageCoord(1, 5),
                new FoliageCoord(0, 5),
                new FoliageCoord(-1, 5),
                new FoliageCoord(5, 1),
                new FoliageCoord(5, 0),
                new FoliageCoord(5, -1),
                new FoliageCoord(-1, -5),
                new FoliageCoord(0, -5),
                new FoliageCoord(1, -5)
            }
        };
        surfaces = new List<IFoliageSurface>();
    }
}
