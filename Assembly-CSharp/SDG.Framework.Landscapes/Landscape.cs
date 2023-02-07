using System.Collections;
using System.Collections.Generic;
using SDG.Framework.Debug;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Foliage;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public class Landscape : DevkitHierarchyItemBase
{
    public delegate void LandscapeReadHeightmapHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight);

    public delegate void LandscapeReadSplatmapHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights);

    public delegate float LandscapeWriteHeightmapHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight);

    public delegate void LandscapeWriteSplatmapHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights);

    public delegate bool LandscapeWriteHolesHandler(Vector3 worldPosition, bool currentlyVisible);

    public delegate void LandscapeGetHeightmapVerticesHandler(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition);

    public delegate void LandscapeGetSplatmapVerticesHandler(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition);

    public static readonly float TILE_SIZE = 1024f;

    public static readonly int TILE_SIZE_INT = 1024;

    public static readonly float TILE_HEIGHT = 2048f;

    public static readonly int TILE_HEIGHT_INT = 2048;

    public static readonly int HEIGHTMAP_RESOLUTION = 257;

    public static readonly int HEIGHTMAP_RESOLUTION_MINUS_ONE = 256;

    public static readonly float HEIGHTMAP_WORLD_UNIT = 4f;

    public static readonly float HALF_HEIGHTMAP_WORLD_UNIT = 2f;

    public static readonly int SPLATMAP_RESOLUTION = 256;

    public static readonly int SPLATMAP_RESOLUTION_MINUS_ONE = 255;

    public static readonly float SPLATMAP_WORLD_UNIT = 4f;

    public static readonly float HALF_SPLATMAP_WORLD_UNIT = 2f;

    public static readonly int BASEMAP_RESOLUTION = 128;

    public static readonly int SPLATMAP_COUNT = 2;

    public static readonly int SPLATMAP_CHANNELS = 4;

    public static readonly int SPLATMAP_LAYERS = SPLATMAP_COUNT * SPLATMAP_CHANNELS;

    public const int HOLES_RESOLUTION = 256;

    public const float HALF_DIAGONAL_SPLATMAP_WORLD_UNIT = 2.828427f;

    protected static readonly float[] SPLATMAP_LAYER_BUFFER = new float[SPLATMAP_LAYERS];

    protected static Dictionary<LandscapeCoord, LandscapeTile> tiles = new Dictionary<LandscapeCoord, LandscapeTile>();

    protected static Dictionary<LandscapeCoord, LandscapeHeightmapTransaction> heightmapTransactions = new Dictionary<LandscapeCoord, LandscapeHeightmapTransaction>();

    protected static Dictionary<LandscapeCoord, LandscapeSplatmapTransaction> splatmapTransactions = new Dictionary<LandscapeCoord, LandscapeSplatmapTransaction>();

    protected static Dictionary<LandscapeCoord, LandscapeHoleTransaction> holeTransactions = new Dictionary<LandscapeCoord, LandscapeHoleTransaction>();

    private static bool _disableHoleColliders;

    private static bool _highlightHoles;

    private bool shouldTriggerLandscapeLoaded = true;

    private bool hasConverted;

    public static Landscape instance { get; protected set; }

    public static bool DisableHoleColliders
    {
        get
        {
            return _disableHoleColliders;
        }
        set
        {
            if (_disableHoleColliders == value)
            {
                return;
            }
            _disableHoleColliders = value;
            foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
            {
                LandscapeTile value2 = tile.Value;
                if (value2.collider != null)
                {
                    value2.collider.terrainData = (_disableHoleColliders ? value2.dataWithoutHoles : value2.data);
                }
            }
        }
    }

    public static bool HighlightHoles
    {
        get
        {
            return _highlightHoles;
        }
        set
        {
            if (_highlightHoles != value)
            {
                _highlightHoles = value;
                Shader.SetGlobalFloat("_TerrainHighlightHoles", _highlightHoles ? 1f : 0f);
            }
        }
    }

    public static event LandscapeLoadedHandler loaded;

    public static void GetUniqueMaterials(List<LandscapeMaterialAsset> materials)
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            foreach (AssetReference<LandscapeMaterialAsset> material in tile.Value.materials)
            {
                LandscapeMaterialAsset landscapeMaterialAsset = material.Find();
                if (landscapeMaterialAsset != null && !materials.Contains(landscapeMaterialAsset))
                {
                    materials.Add(landscapeMaterialAsset);
                }
            }
        }
    }

    public static bool IsPointInsideHole(Vector3 worldPosition)
    {
        LandscapeCoord landscapeCoord = new LandscapeCoord(worldPosition);
        LandscapeTile tile = getTile(landscapeCoord);
        if (tile != null)
        {
            SplatmapCoord splatmapCoord = new SplatmapCoord(landscapeCoord, worldPosition);
            return !tile.holes[splatmapCoord.x, splatmapCoord.y];
        }
        return false;
    }

    public static bool getWorldHeight(Vector3 position, out float height)
    {
        LandscapeTile tile = getTile(new LandscapeCoord(position));
        if (tile != null)
        {
            height = tile.terrain.SampleHeight(position) - TILE_HEIGHT / 2f;
            return true;
        }
        height = 0f;
        return false;
    }

    public static bool getWorldHeight(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, out float height)
    {
        LandscapeTile tile = getTile(tileCoord);
        if (tile != null)
        {
            height = tile.heightmap[heightmapCoord.x, heightmapCoord.y] * TILE_HEIGHT - TILE_HEIGHT / 2f;
            return true;
        }
        height = 0f;
        return false;
    }

    public static bool getHeight01(Vector3 position, out float height)
    {
        LandscapeTile tile = getTile(new LandscapeCoord(position));
        if (tile != null)
        {
            height = tile.terrain.SampleHeight(position) / TILE_HEIGHT;
            return true;
        }
        height = 0f;
        return false;
    }

    public static bool getHeight01(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, out float height)
    {
        LandscapeTile tile = getTile(tileCoord);
        if (tile != null)
        {
            height = tile.heightmap[heightmapCoord.x, heightmapCoord.y];
            return true;
        }
        height = 0f;
        return false;
    }

    public static bool getNormal(Vector3 position, out Vector3 normal)
    {
        LandscapeCoord coord = new LandscapeCoord(position);
        LandscapeTile tile = getTile(coord);
        if (tile != null)
        {
            normal = tile.data.GetInterpolatedNormal((position.x - (float)coord.x * TILE_SIZE) / TILE_SIZE, (position.z - (float)coord.y * TILE_SIZE) / TILE_SIZE);
            return true;
        }
        normal = Vector3.up;
        return false;
    }

    public static bool getSplatmapMaterial(Vector3 position, out AssetReference<LandscapeMaterialAsset> materialAsset)
    {
        LandscapeCoord tileCoord = new LandscapeCoord(position);
        SplatmapCoord splatmapCoord = new SplatmapCoord(tileCoord, position);
        return getSplatmapMaterial(tileCoord, splatmapCoord, out materialAsset);
    }

    public static bool getSplatmapMaterial(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, out AssetReference<LandscapeMaterialAsset> materialAsset)
    {
        if (getSplatmapLayer(tileCoord, splatmapCoord, out var layer))
        {
            materialAsset = getTile(tileCoord).materials[layer];
            return true;
        }
        materialAsset = AssetReference<LandscapeMaterialAsset>.invalid;
        return false;
    }

    public static bool getSplatmapLayer(Vector3 position, out int layer)
    {
        LandscapeCoord tileCoord = new LandscapeCoord(position);
        SplatmapCoord splatmapCoord = new SplatmapCoord(tileCoord, position);
        return getSplatmapLayer(tileCoord, splatmapCoord, out layer);
    }

    public static bool getSplatmapLayer(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, out int layer)
    {
        LandscapeTile tile = getTile(tileCoord);
        if (tile != null)
        {
            layer = getSplatmapHighestWeightLayerIndex(splatmapCoord, tile.splatmap);
            return true;
        }
        layer = -1;
        return false;
    }

    public static int getSplatmapHighestWeightLayerIndex(SplatmapCoord splatmapCoord, float[,,] currentWeights, int ignoreLayer = -1)
    {
        float num = -1f;
        int result = -1;
        for (int i = 0; i < SPLATMAP_LAYERS; i++)
        {
            if (i != ignoreLayer && currentWeights[splatmapCoord.x, splatmapCoord.y, i] > num)
            {
                num = currentWeights[splatmapCoord.x, splatmapCoord.y, i];
                result = i;
            }
        }
        return result;
    }

    public static int getSplatmapHighestWeightLayerIndex(float[] currentWeights, int ignoreLayer = -1)
    {
        float num = -1f;
        int result = -1;
        for (int i = 0; i < SPLATMAP_LAYERS; i++)
        {
            if (i != ignoreLayer && currentWeights[i] > num)
            {
                num = currentWeights[i];
                result = i;
            }
        }
        return result;
    }

    public static void clearHeightmapTransactions()
    {
        heightmapTransactions.Clear();
    }

    public static void clearSplatmapTransactions()
    {
        splatmapTransactions.Clear();
    }

    public static void clearHoleTransactions()
    {
        holeTransactions.Clear();
    }

    public static bool isPointerInTile(Vector3 worldPosition)
    {
        return getTile(worldPosition) != null;
    }

    public static Vector3 getWorldPosition(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, float height)
    {
        float x = Mathf.RoundToInt((float)tileCoord.x * TILE_SIZE + (float)heightmapCoord.y / (float)HEIGHTMAP_RESOLUTION_MINUS_ONE * TILE_SIZE);
        float y = (0f - TILE_HEIGHT) / 2f + height * TILE_HEIGHT;
        float f = (float)tileCoord.y * TILE_SIZE + (float)heightmapCoord.x / (float)HEIGHTMAP_RESOLUTION_MINUS_ONE * TILE_SIZE;
        f = Mathf.RoundToInt(f);
        return new Vector3(x, y, f);
    }

    public static Vector3 getWorldPosition(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord)
    {
        float f = (float)tileCoord.x * TILE_SIZE + (float)splatmapCoord.y / (float)SPLATMAP_RESOLUTION * TILE_SIZE;
        f = (float)Mathf.RoundToInt(f) + HALF_SPLATMAP_WORLD_UNIT;
        float f2 = (float)tileCoord.y * TILE_SIZE + (float)splatmapCoord.x / (float)SPLATMAP_RESOLUTION * TILE_SIZE;
        f2 = (float)Mathf.RoundToInt(f2) + HALF_SPLATMAP_WORLD_UNIT;
        Vector3 vector = new Vector3(f, 0f, f2);
        getWorldHeight(vector, out var height);
        vector.y = height;
        return vector;
    }

    public static void readHeightmap(Bounds worldBounds, LandscapeReadHeightmapHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                HeightmapBounds heightmapBounds = new HeightmapBounds(landscapeCoord, worldBounds);
                for (int k = heightmapBounds.min.x; k < heightmapBounds.max.x; k++)
                {
                    for (int l = heightmapBounds.min.y; l < heightmapBounds.max.y; l++)
                    {
                        HeightmapCoord heightmapCoord = new HeightmapCoord(k, l);
                        float num = tile.heightmap[k, l];
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, heightmapCoord, num);
                        callback(landscapeCoord, heightmapCoord, worldPosition, num);
                    }
                }
            }
        }
    }

    public static void readSplatmap(Bounds worldBounds, LandscapeReadSplatmapHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                SplatmapBounds splatmapBounds = new SplatmapBounds(landscapeCoord, worldBounds);
                for (int k = splatmapBounds.min.x; k < splatmapBounds.max.x; k++)
                {
                    for (int l = splatmapBounds.min.y; l < splatmapBounds.max.y; l++)
                    {
                        SplatmapCoord splatmapCoord = new SplatmapCoord(k, l);
                        for (int m = 0; m < SPLATMAP_LAYERS; m++)
                        {
                            SPLATMAP_LAYER_BUFFER[m] = tile.splatmap[k, l, m];
                        }
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, splatmapCoord);
                        callback(landscapeCoord, splatmapCoord, worldPosition, SPLATMAP_LAYER_BUFFER);
                    }
                }
            }
        }
    }

    public static void writeHeightmap(Bounds worldBounds, LandscapeWriteHeightmapHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                if (!heightmapTransactions.ContainsKey(landscapeCoord))
                {
                    LandscapeHeightmapTransaction landscapeHeightmapTransaction = new LandscapeHeightmapTransaction(tile);
                    DevkitTransactionManager.recordTransaction(landscapeHeightmapTransaction);
                    heightmapTransactions.Add(landscapeCoord, landscapeHeightmapTransaction);
                }
                HeightmapBounds heightmapBounds = new HeightmapBounds(landscapeCoord, worldBounds);
                for (int k = heightmapBounds.min.x; k <= heightmapBounds.max.x; k++)
                {
                    for (int l = heightmapBounds.min.y; l <= heightmapBounds.max.y; l++)
                    {
                        HeightmapCoord heightmapCoord = new HeightmapCoord(k, l);
                        float num = tile.heightmap[k, l];
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, heightmapCoord, num);
                        tile.heightmap[k, l] = Mathf.Clamp01(callback(landscapeCoord, heightmapCoord, worldPosition, num));
                    }
                }
            }
        }
        for (int m = landscapeBounds.min.x; m <= landscapeBounds.max.x; m++)
        {
            for (int n = landscapeBounds.min.y; n <= landscapeBounds.max.y; n++)
            {
                LandscapeTile tile2 = getTile(new LandscapeCoord(m, n));
                if (tile2 == null)
                {
                    continue;
                }
                if (m < landscapeBounds.max.x)
                {
                    LandscapeTile tile3 = getTile(new LandscapeCoord(m + 1, n));
                    if (tile3 != null)
                    {
                        for (int num2 = 0; num2 <= HEIGHTMAP_RESOLUTION_MINUS_ONE; num2++)
                        {
                            tile2.heightmap[num2, HEIGHTMAP_RESOLUTION_MINUS_ONE] = tile3.heightmap[num2, 0];
                        }
                    }
                }
                if (n < landscapeBounds.max.y)
                {
                    LandscapeTile tile4 = getTile(new LandscapeCoord(m, n + 1));
                    if (tile4 != null)
                    {
                        for (int num3 = 0; num3 <= HEIGHTMAP_RESOLUTION_MINUS_ONE; num3++)
                        {
                            tile2.heightmap[HEIGHTMAP_RESOLUTION_MINUS_ONE, num3] = tile4.heightmap[0, num3];
                        }
                    }
                }
                if (m < landscapeBounds.max.x && n < landscapeBounds.max.y)
                {
                    LandscapeTile tile5 = getTile(new LandscapeCoord(m + 1, n + 1));
                    if (tile5 != null)
                    {
                        tile2.heightmap[HEIGHTMAP_RESOLUTION_MINUS_ONE, HEIGHTMAP_RESOLUTION_MINUS_ONE] = tile5.heightmap[0, 0];
                    }
                }
            }
        }
        for (int num4 = landscapeBounds.min.x; num4 <= landscapeBounds.max.x; num4++)
        {
            for (int num5 = landscapeBounds.min.y; num5 <= landscapeBounds.max.y; num5++)
            {
                getTile(new LandscapeCoord(num4, num5))?.SetHeightsDelayLOD();
            }
        }
        LevelHierarchy.MarkDirty();
    }

    public static void writeSplatmap(Bounds worldBounds, LandscapeWriteSplatmapHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                if (!splatmapTransactions.ContainsKey(landscapeCoord))
                {
                    LandscapeSplatmapTransaction landscapeSplatmapTransaction = new LandscapeSplatmapTransaction(tile);
                    DevkitTransactionManager.recordTransaction(landscapeSplatmapTransaction);
                    splatmapTransactions.Add(landscapeCoord, landscapeSplatmapTransaction);
                }
                SplatmapBounds splatmapBounds = new SplatmapBounds(landscapeCoord, worldBounds);
                for (int k = splatmapBounds.min.x; k <= splatmapBounds.max.x; k++)
                {
                    for (int l = splatmapBounds.min.y; l <= splatmapBounds.max.y; l++)
                    {
                        SplatmapCoord splatmapCoord = new SplatmapCoord(k, l);
                        for (int m = 0; m < SPLATMAP_LAYERS; m++)
                        {
                            SPLATMAP_LAYER_BUFFER[m] = tile.splatmap[k, l, m];
                        }
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, splatmapCoord);
                        callback(landscapeCoord, splatmapCoord, worldPosition, SPLATMAP_LAYER_BUFFER);
                        for (int n = 0; n < SPLATMAP_LAYERS; n++)
                        {
                            tile.splatmap[k, l, n] = Mathf.Clamp01(SPLATMAP_LAYER_BUFFER[n]);
                        }
                    }
                }
                tile.data.SetAlphamaps(0, 0, tile.splatmap);
            }
        }
        LevelHierarchy.MarkDirty();
    }

    public static void writeHoles(Bounds worldBounds, LandscapeWriteHolesHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                if (!holeTransactions.ContainsKey(landscapeCoord))
                {
                    LandscapeHoleTransaction landscapeHoleTransaction = new LandscapeHoleTransaction(tile);
                    DevkitTransactionManager.recordTransaction(landscapeHoleTransaction);
                    holeTransactions.Add(landscapeCoord, landscapeHoleTransaction);
                }
                SplatmapBounds splatmapBounds = new SplatmapBounds(landscapeCoord, worldBounds);
                for (int k = splatmapBounds.min.x; k <= splatmapBounds.max.x; k++)
                {
                    for (int l = splatmapBounds.min.y; l <= splatmapBounds.max.y; l++)
                    {
                        SplatmapCoord splatmapCoord = new SplatmapCoord(k, l);
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, splatmapCoord);
                        bool flag = tile.holes[k, l];
                        bool flag2 = callback(worldPosition, flag);
                        tile.holes[k, l] = flag2;
                        tile.hasAnyHolesData |= flag2 != flag;
                    }
                }
                tile.data.SetHoles(0, 0, tile.holes);
            }
        }
        LevelHierarchy.MarkDirty();
    }

    public static void getHeightmapVertices(Bounds worldBounds, LandscapeGetHeightmapVerticesHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                LandscapeTile tile = getTile(landscapeCoord);
                if (tile == null)
                {
                    continue;
                }
                HeightmapBounds heightmapBounds = new HeightmapBounds(landscapeCoord, worldBounds);
                for (int k = heightmapBounds.min.x; k <= heightmapBounds.max.x; k++)
                {
                    for (int l = heightmapBounds.min.y; l <= heightmapBounds.max.y; l++)
                    {
                        HeightmapCoord heightmapCoord = new HeightmapCoord(k, l);
                        float height = tile.heightmap[k, l];
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, heightmapCoord, height);
                        callback(landscapeCoord, heightmapCoord, worldPosition);
                    }
                }
            }
        }
    }

    public static void getSplatmapVertices(Bounds worldBounds, LandscapeGetSplatmapVerticesHandler callback)
    {
        if (callback == null)
        {
            return;
        }
        LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
        for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
        {
            for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
            {
                LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                if (getTile(landscapeCoord) == null)
                {
                    continue;
                }
                SplatmapBounds splatmapBounds = new SplatmapBounds(landscapeCoord, worldBounds);
                for (int k = splatmapBounds.min.x; k <= splatmapBounds.max.x; k++)
                {
                    for (int l = splatmapBounds.min.y; l <= splatmapBounds.max.y; l++)
                    {
                        SplatmapCoord splatmapCoord = new SplatmapCoord(k, l);
                        Vector3 worldPosition = getWorldPosition(landscapeCoord, splatmapCoord);
                        callback(landscapeCoord, splatmapCoord, worldPosition);
                    }
                }
            }
        }
    }

    public static void applyLOD()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.SyncHeightmap();
        }
    }

    public static void SyncHoles()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.data.SyncTexture(TerrainData.HolesTextureName);
        }
    }

    public static void linkNeighbors()
    {
        bool supportsInstancing = SystemInfo.supportsInstancing;
        foreach (LandscapeTile value2 in tiles.Values)
        {
            value2.terrain.drawInstanced = supportsInstancing;
        }
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile5 in tiles)
        {
            LandscapeTile value = tile5.Value;
            LandscapeTile tile = getTile(new LandscapeCoord(value.coord.x - 1, value.coord.y));
            LandscapeTile tile2 = getTile(new LandscapeCoord(value.coord.x, value.coord.y + 1));
            LandscapeTile tile3 = getTile(new LandscapeCoord(value.coord.x + 1, value.coord.y));
            LandscapeTile tile4 = getTile(new LandscapeCoord(value.coord.x, value.coord.y - 1));
            Terrain left = tile?.terrain;
            Terrain top = tile2?.terrain;
            Terrain right = tile3?.terrain;
            Terrain bottom = tile4?.terrain;
            value.terrain.SetNeighbors(left, top, right, bottom);
        }
        foreach (LandscapeTile value3 in tiles.Values)
        {
            value3.terrain.Flush();
        }
    }

    public static void reconcileNeighbors(LandscapeTile tile)
    {
        LandscapeTile tile2 = getTile(new LandscapeCoord(tile.coord.x - 1, tile.coord.y));
        if (tile2 != null)
        {
            for (int i = 0; i < HEIGHTMAP_RESOLUTION; i++)
            {
                tile.heightmap[i, 0] = tile2.heightmap[i, HEIGHTMAP_RESOLUTION - 1];
            }
        }
        LandscapeTile tile3 = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y - 1));
        if (tile3 != null)
        {
            for (int j = 0; j < HEIGHTMAP_RESOLUTION; j++)
            {
                tile.heightmap[0, j] = tile3.heightmap[HEIGHTMAP_RESOLUTION - 1, j];
            }
        }
        LandscapeTile tile4 = getTile(new LandscapeCoord(tile.coord.x + 1, tile.coord.y));
        if (tile4 != null)
        {
            for (int k = 0; k < HEIGHTMAP_RESOLUTION; k++)
            {
                tile.heightmap[k, HEIGHTMAP_RESOLUTION - 1] = tile4.heightmap[k, 0];
            }
        }
        LandscapeTile tile5 = getTile(new LandscapeCoord(tile.coord.x, tile.coord.y + 1));
        if (tile5 != null)
        {
            for (int l = 0; l < HEIGHTMAP_RESOLUTION; l++)
            {
                tile.heightmap[HEIGHTMAP_RESOLUTION - 1, l] = tile5.heightmap[0, l];
            }
        }
        tile.SetHeightsDelayLOD();
    }

    public static LandscapeTile addTile(LandscapeCoord coord)
    {
        if (instance == null)
        {
            UnturnedLog.info("Adding default landscape to level");
            LevelHierarchy.initItem(new GameObject().AddComponent<Landscape>());
        }
        if (tiles.ContainsKey(coord))
        {
            return null;
        }
        LandscapeTile landscapeTile = new LandscapeTile(coord);
        landscapeTile.enable();
        landscapeTile.applyGraphicsSettings();
        tiles.Add(coord, landscapeTile);
        return landscapeTile;
    }

    protected static void clearTiles()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.disable();
        }
        tiles.Clear();
    }

    public static void CopyLayersToAllTiles(LandscapeTile source)
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            LandscapeTile value = tile.Value;
            if (value != source)
            {
                for (int i = 0; i < SPLATMAP_LAYERS; i++)
                {
                    value.materials[i] = source.materials[i];
                }
                value.updatePrototypes();
            }
        }
    }

    public static LandscapeTile getOrAddTile(Vector3 worldPosition)
    {
        return getOrAddTile(new LandscapeCoord(worldPosition));
    }

    public static LandscapeTile getTile(Vector3 worldPosition)
    {
        return getTile(new LandscapeCoord(worldPosition));
    }

    public static LandscapeTile getOrAddTile(LandscapeCoord coord)
    {
        if (!tiles.TryGetValue(coord, out var value))
        {
            return addTile(coord);
        }
        return value;
    }

    public static LandscapeTile getTile(LandscapeCoord coord)
    {
        tiles.TryGetValue(coord, out var value);
        return value;
    }

    public static bool removeTile(LandscapeCoord coord)
    {
        if (!tiles.TryGetValue(coord, out var value))
        {
            return false;
        }
        value.disable();
        Object.Destroy(value.gameObject);
        tiles.Remove(coord);
        return true;
    }

    public override void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        int num = reader.readArrayLength("Tiles");
        if (instance != this)
        {
            UnturnedLog.warn("Level contains multiple Landscapes. Ignoring {0} tile(s) with instance ID: {1}", num, instanceID);
            return;
        }
        UnturnedLog.info("Loading {0} landscape tiles", num);
        for (int i = 0; i < num; i++)
        {
            reader.readArrayIndex(i);
            LandscapeTile landscapeTile = new LandscapeTile(LandscapeCoord.ZERO);
            landscapeTile.enable();
            landscapeTile.applyGraphicsSettings();
            landscapeTile.read(reader);
            if (tiles.ContainsKey(landscapeTile.coord))
            {
                UnturnedLog.error("Duplicate landscape coord read: " + landscapeTile.coord.ToString());
            }
            else
            {
                tiles.Add(landscapeTile.coord, landscapeTile);
            }
        }
        linkNeighbors();
        applyLOD();
    }

    public override void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.beginArray("Tiles");
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            LandscapeTile value = tile.Value;
            writer.writeValue(value);
        }
        writer.endArray();
        writer.endObject();
    }

    protected void triggerLandscapeLoaded()
    {
        Landscape.loaded?.Invoke();
    }

    protected void handleGraphicsSettingsApplied()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.applyGraphicsSettings();
        }
    }

    protected void handlePlanarReflectionPreRender()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.terrain.basemapDistance = 0f;
        }
    }

    protected void handlePlanarReflectionPostRender()
    {
        float terrainBasemapDistance = GraphicsSettings.terrainBasemapDistance;
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            tile.Value.terrain.basemapDistance = terrainBasemapDistance;
        }
    }

    protected void onSatellitePreCapture()
    {
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            LandscapeTile value = tile.Value;
            value.terrain.basemapDistance = 8192f;
            value.terrain.heightmapPixelError = 1f;
        }
    }

    protected void onSatellitePostCapture()
    {
        float terrainBasemapDistance = GraphicsSettings.terrainBasemapDistance;
        float terrainHeightmapPixelError = GraphicsSettings.terrainHeightmapPixelError;
        foreach (KeyValuePair<LandscapeCoord, LandscapeTile> tile in tiles)
        {
            LandscapeTile value = tile.Value;
            value.terrain.basemapDistance = terrainBasemapDistance;
            value.terrain.heightmapPixelError = terrainHeightmapPixelError;
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
        base.name = "Landscape";
        base.gameObject.layer = 20;
        if (instance == null)
        {
            instance = this;
            clearTiles();
            _disableHoleColliders = false;
            HighlightHoles = false;
            if (Level.isEditor)
            {
                LandscapeHeightmapCopyPool.warmup(DevkitTransactionManager.historyLength);
                LandscapeSplatmapCopyPool.warmup(DevkitTransactionManager.historyLength);
                LandscapeHoleCopyPool.warmup(DevkitTransactionManager.historyLength);
            }
            GraphicsSettings.graphicsSettingsApplied += handleGraphicsSettingsApplied;
            PlanarReflection.preRender += handlePlanarReflectionPreRender;
            PlanarReflection.postRender += handlePlanarReflectionPostRender;
            Level.bindSatelliteCaptureInEditor(onSatellitePreCapture, onSatellitePostCapture);
        }
    }

    protected void Start()
    {
        if (instance == this && shouldTriggerLandscapeLoaded)
        {
            triggerLandscapeLoaded();
        }
    }

    protected void OnDestroy()
    {
        if (instance == this)
        {
            GraphicsSettings.graphicsSettingsApplied -= handleGraphicsSettingsApplied;
            PlanarReflection.preRender -= handlePlanarReflectionPreRender;
            PlanarReflection.postRender -= handlePlanarReflectionPostRender;
            Level.unbindSatelliteCapture(onSatellitePreCapture, onSatellitePostCapture);
            instance = null;
            clearTiles();
            _disableHoleColliders = false;
            HighlightHoles = false;
            LandscapeHeightmapCopyPool.empty();
            LandscapeSplatmapCopyPool.empty();
            LandscapeHoleCopyPool.empty();
        }
    }

    internal IEnumerator AutoConvertLegacyTerrain()
    {
        shouldTriggerLandscapeLoaded = false;
        int num = (int)Level.size / TILE_SIZE_INT;
        int halfTiles = num / 2 + 1;
        for (int tile_x = -halfTiles; tile_x < halfTiles; tile_x++)
        {
            for (int tile_y = -halfTiles; tile_y < halfTiles; tile_y++)
            {
                LandscapeCoord coord = new LandscapeCoord(tile_x, tile_y);
                LandscapeTile tile = getOrAddTile(coord);
                UnturnedLog.info("Auto convert heightmap {0}", coord);
                tile.convertLegacyHeightmap();
                yield return null;
                UnturnedLog.info("Auto convert splatmap {0}", coord);
                tile.convertLegacySplatmap();
                yield return null;
                for (int i = 0; i < SPLATMAP_LAYERS; i++)
                {
                    tile.materials[i] = LevelGround.legacyMaterialGuids[i];
                }
                tile.updatePrototypes();
                yield return null;
            }
        }
        FoliageSystem.CreateInLevelIfMissing();
        triggerLandscapeLoaded();
    }

    private IEnumerator convertLegacyTerrainImpl(InspectableList<AssetReference<LandscapeMaterialAsset>> materials)
    {
        yield return null;
        int size = Level.size;
        int tiles = size / TILE_SIZE_INT;
        for (int tile_x = -tiles; tile_x < tiles; tile_x++)
        {
            for (int tile_y = -tiles; tile_y < tiles; tile_y++)
            {
                LandscapeCoord coord = new LandscapeCoord(tile_x, tile_y);
                LandscapeTile tile = getOrAddTile(coord);
                UnturnedLog.info("Convert heightmap {0}", coord);
                tile.convertLegacyHeightmap();
                yield return null;
                UnturnedLog.info("Convert splatmap {0}", coord);
                tile.convertLegacySplatmap();
                yield return null;
                for (int i = 0; i < SPLATMAP_LAYERS; i++)
                {
                    tile.materials[i] = materials[i];
                }
                UnturnedLog.info("Convert prototypes {0}", coord);
                tile.updatePrototypes();
                yield return null;
            }
        }
        GameObject obj = new GameObject();
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = new Vector3(size, TILE_HEIGHT, size);
        obj.AddComponent<FoliageVolume>().mode = FoliageVolume.EFoliageVolumeMode.ADDITIVE;
    }

    public void convertLegacyTerrain(InspectableList<AssetReference<LandscapeMaterialAsset>> materials)
    {
        if (!hasConverted)
        {
            hasConverted = true;
            StartCoroutine(convertLegacyTerrainImpl(materials));
        }
    }
}
