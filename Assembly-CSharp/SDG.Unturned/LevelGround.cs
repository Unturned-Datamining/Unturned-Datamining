using System;
using System.Collections.Generic;
using System.IO;
using SDG.Framework.Foliage;
using SDG.Framework.Landscapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class LevelGround : MonoBehaviour
{
    private static int _Triplanar_Primary_Size;

    private static float _triplanarPrimarySize;

    private static int _Triplanar_Primary_Weight;

    private static float _triplanarPrimaryWeight;

    private static int _Triplanar_Secondary_Size;

    private static float _triplanarSecondarySize;

    private static int _Triplanar_Secondary_Weight;

    private static float _triplanarSecondaryWeight;

    private static int _Triplanar_Tertiary_Size;

    private static float _triplanarTertiarySize;

    private static int _Triplanar_Tertiary_Weight;

    private static float _triplanarTertiaryWeight;

    private static Collider[] obstructionColliders;

    public static readonly byte SAVEDATA_TREES_VERSION;

    public static readonly byte RESOURCE_REGIONS;

    public static readonly byte ALPHAMAPS;

    private static float[,,] alphamapHQ;

    private static float[,,] alphamap2HQ;

    public static bool hasLegacyDataForConversion;

    public static AssetReference<LandscapeMaterialAsset>[] legacyMaterialGuids;

    private static Transform _models;

    private static Transform _models2;

    private static List<ResourceSpawnpoint>[,] _trees;

    private static int _total;

    private static bool[,] _regions;

    private static int[,] loads;

    private static bool isRegionalVisibilityDirty;

    private static Terrain _terrain;

    private static Terrain _terrain2;

    private static TerrainData _data;

    private static TerrainData _data2;

    public static float triplanarPrimarySize
    {
        get
        {
            return _triplanarPrimarySize;
        }
        set
        {
            _triplanarPrimarySize = value;
            Shader.SetGlobalFloat(_Triplanar_Primary_Size, triplanarPrimarySize);
            UnturnedLog.info("Set triplanar_primary_size to: " + triplanarPrimarySize);
        }
    }

    public static float triplanarPrimaryWeight
    {
        get
        {
            return _triplanarPrimaryWeight;
        }
        set
        {
            _triplanarPrimaryWeight = value;
            Shader.SetGlobalFloat(_Triplanar_Primary_Weight, triplanarPrimaryWeight);
            UnturnedLog.info("Set triplanar_primary_weight to: " + triplanarPrimaryWeight);
        }
    }

    public static float triplanarSecondarySize
    {
        get
        {
            return _triplanarSecondarySize;
        }
        set
        {
            _triplanarSecondarySize = value;
            Shader.SetGlobalFloat(_Triplanar_Secondary_Size, triplanarSecondarySize);
            UnturnedLog.info("Set triplanar_secondary_size to: " + triplanarSecondarySize);
        }
    }

    public static float triplanarSecondaryWeight
    {
        get
        {
            return _triplanarSecondaryWeight;
        }
        set
        {
            _triplanarSecondaryWeight = value;
            Shader.SetGlobalFloat(_Triplanar_Secondary_Weight, triplanarSecondaryWeight);
            UnturnedLog.info("Set triplanar_secondary_weight to: " + triplanarSecondaryWeight);
        }
    }

    public static float triplanarTertiarySize
    {
        get
        {
            return _triplanarTertiarySize;
        }
        set
        {
            _triplanarTertiarySize = value;
            Shader.SetGlobalFloat(_Triplanar_Tertiary_Size, triplanarTertiarySize);
            UnturnedLog.info("Set triplanar_tertiary_size to: " + triplanarTertiarySize);
        }
    }

    public static float triplanarTertiaryWeight
    {
        get
        {
            return _triplanarTertiaryWeight;
        }
        set
        {
            _triplanarTertiaryWeight = value;
            Shader.SetGlobalFloat(_Triplanar_Tertiary_Weight, triplanarTertiaryWeight);
            UnturnedLog.info("Set triplanar_tertiary_weight to: " + triplanarTertiaryWeight);
        }
    }

    public static byte[] treesHash { get; private set; }

    [Obsolete("Legacy terrain game object only exists for auto-conversion")]
    public static Transform models => _models;

    [Obsolete("Legacy terrain game object only exists for auto-conversion")]
    public static Transform models2 => _models2;

    public static List<ResourceSpawnpoint>[,] trees => _trees;

    public static int total => _total;

    public static bool[,] regions => _regions;

    public static bool shouldInstantlyLoad { get; private set; }

    [Obsolete("Legacy terrain only exists for auto-conversion")]
    public static Terrain terrain => _terrain;

    [Obsolete("Legacy terrain only exists for auto-conversion")]
    public static Terrain terrain2 => _terrain2;

    public static TerrainData data => _data;

    public static TerrainData data2 => _data2;

    internal static ResourceSpawnpoint FindResourceSpawnpointByTransform(Transform transform)
    {
        if (transform != null)
        {
            transform = transform.root;
        }
        if (transform != null && Regions.tryGetCoordinate(transform.position, out var x, out var y))
        {
            foreach (ResourceSpawnpoint item in _trees[x, y])
            {
                if (item.model == transform)
                {
                    return item;
                }
            }
        }
        return null;
    }

    [Obsolete]
    public static Vector3 checkSafe(Vector3 point)
    {
        UndergroundWhitelist.adjustPosition(ref point, 0.5f, 1f);
        return point;
    }

    [Obsolete]
    public static int getMaterialIndex(Vector3 point)
    {
        return 0;
    }

    public static float getHeight(Vector3 point)
    {
        Landscape.getWorldHeight(point, out var height);
        return height;
    }

    public static float getConversionHeight(Vector3 point)
    {
        if (point.x < (float)(-Level.size / 2) || point.z < (float)(-Level.size / 2) || point.x > (float)((int)Level.size / 2) || point.z > (float)((int)Level.size / 2))
        {
            return _terrain2.SampleHeight(point);
        }
        return Mathf.Max(_terrain.SampleHeight(point), _terrain2.SampleHeight(point));
    }

    public static float getConversionWeight(Vector3 point, int layer)
    {
        if (point.x < (float)(-Level.size / 2) || point.z < (float)(-Level.size / 2) || point.x > (float)((int)Level.size / 2) || point.z > (float)((int)Level.size / 2) || _terrain2.SampleHeight(point) > _terrain.SampleHeight(point))
        {
            int alphamap2_X = getAlphamap2_X(point);
            if (alphamap2_X < 0 || alphamap2_X >= data2.alphamapWidth)
            {
                return 0f;
            }
            int alphamap2_Y = getAlphamap2_Y(point);
            if (alphamap2_Y < 0 || alphamap2_Y >= data2.alphamapWidth)
            {
                return 0f;
            }
            return alphamap2HQ[alphamap2_Y, alphamap2_X, layer];
        }
        int alphamap_X = getAlphamap_X(point);
        if (alphamap_X < 0 || alphamap_X >= data.alphamapWidth)
        {
            return 0f;
        }
        int alphamap_Y = getAlphamap_Y(point);
        if (alphamap_Y < 0 || alphamap_Y >= data.alphamapWidth)
        {
            return 0f;
        }
        return alphamapHQ[alphamap_Y, alphamap_X, layer];
    }

    public static Vector3 getNormal(Vector3 point)
    {
        Landscape.getNormal(point, out var normal);
        return normal;
    }

    public static int getAlphamap_X(Vector3 point)
    {
        return (int)((point.x - _terrain.transform.position.x) / data.size.x * (float)data.alphamapWidth);
    }

    public static int getAlphamap_Y(Vector3 point)
    {
        return (int)((point.z - _terrain.transform.position.z) / data.size.z * (float)data.alphamapHeight);
    }

    public static int getAlphamap2_X(Vector3 point)
    {
        return (int)((point.x - _terrain2.transform.position.x) / data2.size.x * (float)data2.alphamapWidth);
    }

    public static int getAlphamap2_Y(Vector3 point)
    {
        return (int)((point.z - _terrain2.transform.position.z) / data2.size.z * (float)data2.alphamapHeight);
    }

    public static int getHeightmap_X(Vector3 point)
    {
        return (int)((point.x - _terrain.transform.position.x) / data.size.x * (float)data.heightmapResolution);
    }

    public static int getHeightmap_Y(Vector3 point)
    {
        return (int)((point.z - _terrain.transform.position.z) / data.size.z * (float)data.heightmapResolution);
    }

    public static int getHeightmap2_X(Vector3 point)
    {
        return (int)((point.x - _terrain2.transform.position.x) / data2.size.x * (float)data2.heightmapResolution);
    }

    public static int getHeightmap2_Y(Vector3 point)
    {
        return (int)((point.z - _terrain2.transform.position.z) / data2.size.z * (float)data2.heightmapResolution);
    }

    [Obsolete]
    public static void cutFoliage(Vector3 point, float radius = 6f)
    {
    }

    protected static void handlePreBakeTile(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
    {
        if (!bakeSettings.bakeResources || !Regions.tryGetCoordinate(foliageTile.worldBounds.center, out var x, out var y))
        {
            return;
        }
        for (int num = trees[x, y].Count - 1; num >= 0; num--)
        {
            ResourceSpawnpoint resourceSpawnpoint = trees[x, y][num];
            if (resourceSpawnpoint.isGenerated)
            {
                Vector3 min = foliageTile.worldBounds.min;
                if (!(resourceSpawnpoint.point.x < min.x) && !(resourceSpawnpoint.point.z < min.z))
                {
                    Vector3 max = foliageTile.worldBounds.max;
                    if (!(resourceSpawnpoint.point.x > max.x) && !(resourceSpawnpoint.point.z > max.z))
                    {
                        resourceSpawnpoint.destroy();
                        trees[x, y].RemoveAt(num);
                    }
                }
            }
        }
        regions[x, y] = false;
    }

    protected static void handlePostBake()
    {
        onRegionUpdated(byte.MaxValue, byte.MaxValue, EditorArea.instance.region_x, EditorArea.instance.region_y);
    }

    public static void addSpawn(Vector3 point, ushort id, bool isGenerated = false)
    {
        if (Regions.tryGetCoordinate(point, out var x, out var y))
        {
            ResourceSpawnpoint resourceSpawnpoint = new ResourceSpawnpoint(id, point, isGenerated, NetId.INVALID);
            resourceSpawnpoint.enable();
            resourceSpawnpoint.disableSkybox();
            trees[x, y].Add(resourceSpawnpoint);
            _total++;
        }
    }

    [Obsolete("Replaced by version which takes ID rather than index.")]
    public static void addSpawn(Vector3 point, byte index, bool isGenerated = false)
    {
    }

    public static void removeSpawn(Vector3 point, float radius)
    {
        radius *= radius;
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                List<ResourceSpawnpoint> list = new List<ResourceSpawnpoint>();
                for (int i = 0; i < trees[b, b2].Count; i++)
                {
                    ResourceSpawnpoint resourceSpawnpoint = trees[b, b2][i];
                    if ((resourceSpawnpoint.point - point).sqrMagnitude < radius)
                    {
                        resourceSpawnpoint.destroy();
                    }
                    else
                    {
                        list.Add(resourceSpawnpoint);
                    }
                }
                _trees[b, b2] = list;
            }
        }
    }

    protected static void loadSplatPrototypes()
    {
        string path = Level.info.path + "/Terrain/Materials.unity3d";
        if (!ReadWrite.fileExists(path, useCloud: false, usePath: false))
        {
            return;
        }
        byte[] array = Hash.SHA1(ReadWrite.readBytes(path, useCloud: false, usePath: false));
        UnturnedLog.info("Legacy terrain material hash: " + Hash.ToCodeString(array));
        byte[] hash_ = new byte[20]
        {
            79, 148, 129, 14, 170, 79, 47, 23, 60, 241,
            103, 67, 234, 176, 132, 142, 99, 41, 88, 207
        };
        if (Hash.verifyHash(array, hash_))
        {
            UnturnedLog.info("Matched PEI legacy terrain materials hash");
            legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("92cb5a3afd534054a64eb320b50c48de");
            legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("22b77c4c51514b0fbb66765eedf1a7f4");
            legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba");
            legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("9a2de27c10aa41438154105292b2fd4a");
            legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b");
            legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("a9f5c606fe0d433ab167fbe8e3273055");
            legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade");
            legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("2e329671e8c9432eae580f7807acc021");
            return;
        }
        byte[] hash_2 = new byte[20]
        {
            176, 124, 229, 38, 61, 181, 234, 222, 248, 79,
            43, 20, 216, 9, 223, 252, 102, 128, 208, 3
        };
        if (Hash.verifyHash(array, hash_2))
        {
            UnturnedLog.info("Matched Russia legacy terrain materials hash");
            legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("17b88227113041869ba4661b227a0590");
            legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("8a2b6f2215d6460f8b6fece2ccd9c208");
            legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("79787e2ca948457a9a322179cf580386");
            legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("db482f0f23d1414096114aee61195058");
            legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("ceb122707edc4f349be0a97d8f05fd09");
            legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("b4ffe0d7b8ed4ff2b4c302c489108b02");
            legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b");
            legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619");
            return;
        }
        byte[] hash_3 = new byte[20]
        {
            66, 161, 124, 248, 128, 110, 137, 204, 192, 128,
            38, 81, 246, 158, 24, 67, 76, 246, 198, 76
        };
        if (Hash.verifyHash(array, hash_3))
        {
            UnturnedLog.info("Matched Washington legacy terrain materials hash");
            legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42");
            legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("e981f9fae3fa43d68a9a0bfa6472a69f");
            legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("5020515a0b9a4b1eb610c006d81f806c");
            legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6");
            legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b");
            legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619");
            legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("d691f78202c84951a3a697f310abd115");
            legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("50acf0bddd844f93addd0097f7d95d95");
            return;
        }
        byte[] hash_4 = new byte[20]
        {
            251, 186, 29, 144, 240, 171, 74, 86, 200, 30,
            241, 240, 4, 191, 77, 80, 77, 197, 180, 206
        };
        if (Hash.verifyHash(array, hash_4))
        {
            UnturnedLog.info("Matched Yukon legacy terrain materials hash");
            legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42");
            legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("e981f9fae3fa43d68a9a0bfa6472a69f");
            legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba");
            legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6");
            legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b");
            legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619");
            legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade");
            legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("50acf0bddd844f93addd0097f7d95d95");
            return;
        }
        byte[] hash_5 = new byte[20]
        {
            96, 162, 240, 106, 199, 227, 25, 76, 211, 1,
            18, 104, 64, 34, 127, 188, 128, 134, 1, 11
        };
        if (Hash.verifyHash(array, hash_5))
        {
            UnturnedLog.info("Matched Greece legacy terrain materials hash");
            legacyMaterialGuids[0] = new AssetReference<LandscapeMaterialAsset>("cf33a7a8fd52461bb523d84234b3a232");
            legacyMaterialGuids[1] = new AssetReference<LandscapeMaterialAsset>("1a917f0fbc0f48d2a4dfda5e15a623df");
            legacyMaterialGuids[2] = new AssetReference<LandscapeMaterialAsset>("76c32cee254f4aeda910d3d8d9788a46");
            legacyMaterialGuids[3] = new AssetReference<LandscapeMaterialAsset>("98c2ae7c2aad48148c9daeb6fab4aa2a");
            legacyMaterialGuids[4] = new AssetReference<LandscapeMaterialAsset>("c743d33c42f54753a529886997626040");
            legacyMaterialGuids[5] = new AssetReference<LandscapeMaterialAsset>("e476cd429bdb41fcafa1df84663e0a47");
            legacyMaterialGuids[6] = new AssetReference<LandscapeMaterialAsset>("30fe7a6c14ee4064865054b82cd71d13");
            legacyMaterialGuids[7] = new AssetReference<LandscapeMaterialAsset>("1b30a651c2ff4c90b8c62d6d9212c146");
            return;
        }
        UnturnedLog.info("Unable to match legacy terrain materials hash, using names instead");
        try
        {
            Bundle bundle = Bundles.getBundle(path, prependRoot: false);
            Texture2D[] array2 = bundle.loadAll<Texture2D>();
            int num = 0;
            Texture2D[] array3 = array2;
            for (int i = 0; i < array3.Length; i++)
            {
                string text = array3[i].name;
                if (text.IndexOf("_Mask") == -1)
                {
                    if (text.IndexOf("Farm", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("22b77c4c51514b0fbb66765eedf1a7f4");
                    }
                    else if (text.IndexOf("Road", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("8729d40d361c4947be4188c70dd7100b");
                    }
                    else if (text.IndexOf("Grass", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("3d7717c2bc074401853b2fdacd9db1ba");
                    }
                    else if (text.IndexOf("Gravel", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("a14df8dd9bb44f1d967a53f43bde54e6");
                    }
                    else if (text.IndexOf("Sand", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("684f4b28200d4ceb9c5362d78d2c9619");
                    }
                    else if (text.IndexOf("Snow", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("e25f0351181f4ad1a9c0dc31d2fedade");
                    }
                    else if (text.IndexOf("Stone", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("8a2b6f2215d6460f8b6fece2ccd9c208");
                    }
                    else if (text.IndexOf("Dirt", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("e52b20e26b7c47c89aa5a350938f8f42");
                    }
                    else if (text.IndexOf("Leaves", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("b4ffe0d7b8ed4ff2b4c302c489108b02");
                    }
                    else if (text.IndexOf("Dead", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        legacyMaterialGuids[num] = new AssetReference<LandscapeMaterialAsset>("17b88227113041869ba4661b227a0590");
                    }
                    if (legacyMaterialGuids[num].isNull)
                    {
                        UnturnedLog.warn($"Unable to match layer {num} name \"{text}\" with any known materials");
                    }
                    else
                    {
                        UnturnedLog.info($"Matched layer {num} name \"{text}\" with \"{legacyMaterialGuids[num].Find()?.name}\"");
                    }
                    num++;
                    if (num >= 8)
                    {
                        break;
                    }
                }
            }
            bundle.unload();
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception loading legacy terrain materials:");
        }
        AssetReference<LandscapeMaterialAsset>[] array4 = new AssetReference<LandscapeMaterialAsset>[8]
        {
            new AssetReference<LandscapeMaterialAsset>("64357418ae184a959186d1f592a93761"),
            new AssetReference<LandscapeMaterialAsset>("8ea9b170d93e4f9a8a0e0c61cd4bee6a"),
            new AssetReference<LandscapeMaterialAsset>("e54a3da2c46e4927848fed4cdead560a"),
            new AssetReference<LandscapeMaterialAsset>("713fe6ff00e647408047d5dd39d815c0"),
            new AssetReference<LandscapeMaterialAsset>("00ddd72266914141b39e33227942a7df"),
            new AssetReference<LandscapeMaterialAsset>("498ca625072d443a876b2a4f11896018"),
            new AssetReference<LandscapeMaterialAsset>("9889a0b5aad04ddd8c4c463f3e1b79f6"),
            new AssetReference<LandscapeMaterialAsset>("5fd97d1f946c45a79e3d47b49d0348d8")
        };
        int num2 = 0;
        for (int j = 0; j < 8; j++)
        {
            if (legacyMaterialGuids[j].isNull)
            {
                UnturnedLog.warn($"Defaulting empty layer {j} to \"{array4[num2].Find()?.name}\"");
                legacyMaterialGuids[j] = array4[num2];
                num2++;
            }
        }
    }

    protected static void loadTrees()
    {
        _trees = new List<ResourceSpawnpoint>[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        _total = 0;
        _regions = new bool[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        loads = new int[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        shouldInstantlyLoad = true;
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                loads[b, b2] = -1;
            }
        }
        for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3 = (byte)(b3 + 1))
        {
            for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4 = (byte)(b4 + 1))
            {
                trees[b3, b4] = new List<ResourceSpawnpoint>();
            }
        }
        treesHash = new byte[20];
        if (!ReadWrite.fileExists(Level.info.path + "/Terrain/Trees.dat", useCloud: false, usePath: false))
        {
            return;
        }
        River river = new River(Level.info.path + "/Terrain/Trees.dat", usePath: false);
        byte b5 = river.readByte();
        bool flag = true;
        if (b5 > 3)
        {
            TreeRedirectorMap treeRedirectorMap = null;
            if (Level.shouldUseHolidayRedirects)
            {
                treeRedirectorMap = new TreeRedirectorMap();
            }
            for (byte b6 = 0; b6 < Regions.WORLD_SIZE; b6 = (byte)(b6 + 1))
            {
                for (byte b7 = 0; b7 < Regions.WORLD_SIZE; b7 = (byte)(b7 + 1))
                {
                    ushort num = river.readUInt16();
                    for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
                    {
                        if (b5 > 4)
                        {
                            ushort num3 = river.readUInt16();
                            Vector3 newPoint = river.readSingleVector3();
                            bool newGenerated = river.readBoolean();
                            if (num3 != 0)
                            {
                                if (treeRedirectorMap != null)
                                {
                                    num3 = treeRedirectorMap.redirect(num3);
                                }
                                if (num3 != 0)
                                {
                                    NetId treeNetId = LevelNetIdRegistry.GetTreeNetId(b6, b7, num2);
                                    ResourceSpawnpoint resourceSpawnpoint = new ResourceSpawnpoint(num3, newPoint, newGenerated, treeNetId);
                                    if (resourceSpawnpoint.asset == null && (bool)Assets.shouldLoadAnyAssets)
                                    {
                                        UnturnedLog.error("Tree with no asset in region {0}, {1}: {2}", b6, b7, num3);
                                        flag = false;
                                    }
                                    trees[b6, b7].Add(resourceSpawnpoint);
                                    _total++;
                                }
                            }
                        }
                        else
                        {
                            byte b8 = river.readByte();
                            ushort num4 = 3;
                            Vector3 newPoint2 = river.readSingleVector3();
                            bool newGenerated2 = river.readBoolean();
                            NetId treeNetId2 = LevelNetIdRegistry.GetTreeNetId(b6, b7, num2);
                            ResourceSpawnpoint resourceSpawnpoint2 = new ResourceSpawnpoint(num4, newPoint2, newGenerated2, treeNetId2);
                            if (resourceSpawnpoint2.asset == null && (bool)Assets.shouldLoadAnyAssets)
                            {
                                UnturnedLog.error("Tree with no asset in region {0}, {1}: {2} {3}", b6, b7, num4, b8);
                                flag = false;
                            }
                            trees[b6, b7].Add(resourceSpawnpoint2);
                            _total++;
                        }
                    }
                }
            }
            if (treeRedirectorMap != null && !treeRedirectorMap.hasAllAssets && (bool)Assets.shouldLoadAnyAssets)
            {
                flag = false;
                UnturnedLog.error("Zeroing trees hash because redirected asset(s) missing");
            }
        }
        if (flag)
        {
            treesHash = river.getHash();
        }
        river.closeRiver();
    }

    public static void load(ushort size)
    {
        hasLegacyDataForConversion = false;
        if (!Level.info.configData.Use_Legacy_Ground)
        {
            loadTrees();
            return;
        }
        if (File.Exists(GetConversionMarkerFilePath()))
        {
            UnturnedLog.info("Skipping legacy terrain loading because it has already been converted");
            loadTrees();
            return;
        }
        hasLegacyDataForConversion = true;
        legacyMaterialGuids = new AssetReference<LandscapeMaterialAsset>[8];
        _models = new GameObject().transform;
        _models.name = "Ground";
        _models.parent = Level.level;
        _models.tag = "Ground";
        _models.gameObject.layer = 20;
        _terrain = _models.gameObject.AddComponent<Terrain>();
        _terrain.drawInstanced = SystemInfo.supportsInstancing;
        _terrain.name = "Ground";
        _terrain.heightmapPixelError = 200f;
        _terrain.transform.position = new Vector3(-size / 2, 0f, -size / 2);
        _terrain.reflectionProbeUsage = ReflectionProbeUsage.Simple;
        _terrain.shadowCastingMode = ShadowCastingMode.Off;
        _terrain.drawHeightmap = false;
        _terrain.drawTreesAndFoliage = false;
        _terrain.treeDistance = 0f;
        _terrain.treeBillboardDistance = 0f;
        _terrain.treeCrossFadeLength = 0f;
        _terrain.treeMaximumFullLODCount = 0;
        _data = new TerrainData();
        data.name = "Ground";
        data.heightmapResolution = (int)size / 8;
        data.alphamapResolution = (int)size / 4;
        data.size = new Vector3((int)size, Level.TERRAIN, (int)size);
        data.wavingGrassTint = Color.white;
        byte b = 0;
        byte b2 = 0;
        if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heights.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Terrain/Heights.dat", useCloud: false, usePath: false, 0);
            b = block.readByte();
            b2 = block.readByte();
        }
        if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heightmap.png", useCloud: false, usePath: false))
        {
            byte[] array = ReadWrite.readBytes(Level.info.path + "/Terrain/Heightmap.png", useCloud: false, usePath: false);
            Texture2D texture2D = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
            texture2D.name = "Heightmap_Load";
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.LoadImage(array);
            float[,] array2 = new float[texture2D.width, texture2D.height];
            for (int i = 0; i < texture2D.width; i++)
            {
                for (int j = 0; j < texture2D.height; j++)
                {
                    if (b > 0)
                    {
                        byte[] value = new byte[4]
                        {
                            (byte)(texture2D.GetPixel(i, j).r * 255f),
                            (byte)(texture2D.GetPixel(i, j).g * 255f),
                            (byte)(texture2D.GetPixel(i, j).b * 255f),
                            (byte)(texture2D.GetPixel(i, j).a * 255f)
                        };
                        array2[i, j] = BitConverter.ToSingle(value, 0);
                    }
                    else
                    {
                        array2[i, j] = texture2D.GetPixel(i, j).r;
                    }
                }
            }
            data.SetHeights(0, 0, array2);
            UnityEngine.Object.DestroyImmediate(texture2D);
        }
        else
        {
            float[,] array3 = new float[data.heightmapResolution, data.heightmapResolution];
            for (int k = 0; k < data.heightmapResolution; k++)
            {
                for (int l = 0; l < data.heightmapResolution; l++)
                {
                    array3[k, l] = 0.03f;
                }
            }
            data.SetHeights(0, 0, array3);
        }
        loadSplatPrototypes();
        alphamapHQ = new float[data.alphamapWidth, data.alphamapHeight, ALPHAMAPS * 4];
        for (int m = 0; m < ALPHAMAPS; m++)
        {
            bool flag = false;
            if (ReadWrite.fileExists(Level.info.path + "/Terrain/AlphamapHQ_" + m + ".png", useCloud: false, usePath: false))
            {
                byte[] array4 = ReadWrite.readBytes(Level.info.path + "/Terrain/AlphamapHQ_" + m + ".png", useCloud: false, usePath: false);
                Texture2D texture2D2 = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
                texture2D2.name = "AlphamapHQ_Load";
                texture2D2.hideFlags = HideFlags.HideAndDontSave;
                texture2D2.LoadImage(array4);
                for (int n = 0; n < texture2D2.width; n++)
                {
                    for (int num = 0; num < texture2D2.height; num++)
                    {
                        Color pixel = texture2D2.GetPixel(n, num);
                        alphamapHQ[n, num, m * 4] = pixel.r;
                        alphamapHQ[n, num, m * 4 + 1] = pixel.g;
                        alphamapHQ[n, num, m * 4 + 2] = pixel.b;
                        alphamapHQ[n, num, m * 4 + 3] = pixel.a;
                    }
                }
                UnityEngine.Object.DestroyImmediate(texture2D2);
                flag = true;
            }
            if (flag || !ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap_" + m + ".png", useCloud: false, usePath: false))
            {
                continue;
            }
            byte[] array5 = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap_" + m + ".png", useCloud: false, usePath: false);
            Texture2D texture2D3 = new Texture2D(data.heightmapResolution, data.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
            texture2D3.name = "Alphamap_Load";
            texture2D3.hideFlags = HideFlags.HideAndDontSave;
            texture2D3.LoadImage(array5);
            for (int num2 = 0; num2 < texture2D3.width; num2++)
            {
                for (int num3 = 0; num3 < texture2D3.height; num3++)
                {
                    Color pixel2 = texture2D3.GetPixel(num2, num3);
                    alphamapHQ[num2, num3, m * 4] = pixel2.r;
                    alphamapHQ[num2, num3, m * 4 + 1] = pixel2.g;
                    alphamapHQ[num2, num3, m * 4 + 2] = pixel2.b;
                    alphamapHQ[num2, num3, m * 4 + 3] = pixel2.a;
                }
            }
            UnityEngine.Object.DestroyImmediate(texture2D3);
        }
        data.baseMapResolution = (int)size / 8;
        data.baseMapResolution = (int)size / 4;
        _terrain.terrainData = data;
        _terrain.terrainData.wavingGrassAmount = 0f;
        _terrain.terrainData.wavingGrassSpeed = 1f;
        _terrain.terrainData.wavingGrassStrength = 1f;
        loadTrees();
        _models2 = new GameObject().transform;
        _models2.name = "Ground2";
        _models2.parent = Level.level;
        _models2.tag = "Ground2";
        _models2.gameObject.layer = 31;
        _terrain2 = _models2.gameObject.AddComponent<Terrain>();
        _terrain2.drawInstanced = _terrain.drawInstanced;
        _terrain2.name = "Ground2";
        _terrain2.heightmapPixelError = 200f;
        _terrain2.transform.position = new Vector3(-size, 0f, -size);
        _terrain2.reflectionProbeUsage = ReflectionProbeUsage.Simple;
        _terrain2.shadowCastingMode = ShadowCastingMode.Off;
        _terrain2.drawHeightmap = _terrain.drawHeightmap;
        _terrain2.drawTreesAndFoliage = false;
        _terrain2.treeDistance = 0f;
        _terrain2.treeBillboardDistance = 0f;
        _terrain2.treeCrossFadeLength = 0f;
        _terrain2.treeMaximumFullLODCount = 0;
        _data2 = new TerrainData();
        data2.name = "Ground2";
        data2.heightmapResolution = (int)size / 16;
        data2.alphamapResolution = (int)size / 8;
        data2.size = new Vector3(size * 2, Level.TERRAIN, size * 2);
        if (ReadWrite.fileExists(Level.info.path + "/Terrain/Heightmap2.png", useCloud: false, usePath: false))
        {
            byte[] array6 = ReadWrite.readBytes(Level.info.path + "/Terrain/Heightmap2.png", useCloud: false, usePath: false);
            Texture2D texture2D4 = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
            texture2D4.name = "Heightmap2_Load";
            texture2D4.hideFlags = HideFlags.HideAndDontSave;
            texture2D4.LoadImage(array6);
            float[,] array7 = new float[texture2D4.width, texture2D4.height];
            for (int num4 = 0; num4 < texture2D4.width; num4++)
            {
                for (int num5 = 0; num5 < texture2D4.height; num5++)
                {
                    if (b2 > 0)
                    {
                        byte[] value2 = new byte[4]
                        {
                            (byte)(texture2D4.GetPixel(num4, num5).r * 255f),
                            (byte)(texture2D4.GetPixel(num4, num5).g * 255f),
                            (byte)(texture2D4.GetPixel(num4, num5).b * 255f),
                            (byte)(texture2D4.GetPixel(num4, num5).a * 255f)
                        };
                        array7[num4, num5] = BitConverter.ToSingle(value2, 0);
                    }
                    else
                    {
                        array7[num4, num5] = texture2D4.GetPixel(num4, num5).r;
                    }
                }
            }
            data2.SetHeights(0, 0, array7);
            UnityEngine.Object.DestroyImmediate(texture2D4);
        }
        else
        {
            float[,] array8 = new float[data2.heightmapResolution, data2.heightmapResolution];
            for (int num6 = 0; num6 < data2.heightmapResolution; num6++)
            {
                for (int num7 = 0; num7 < data2.heightmapResolution; num7++)
                {
                    array8[num6, num7] = 0f;
                }
            }
            data2.SetHeights(0, 0, array8);
        }
        alphamap2HQ = new float[data2.alphamapWidth, data2.alphamapHeight, ALPHAMAPS * 4];
        for (int num8 = 0; num8 < ALPHAMAPS; num8++)
        {
            bool flag2 = false;
            if (ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap2HQ_" + num8 + ".png", useCloud: false, usePath: false))
            {
                byte[] array9 = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap2HQ_" + num8 + ".png", useCloud: false, usePath: false);
                Texture2D texture2D5 = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
                texture2D5.name = "Alphamap2HQ_Load";
                texture2D5.hideFlags = HideFlags.HideAndDontSave;
                texture2D5.LoadImage(array9);
                for (int num9 = 0; num9 < texture2D5.width; num9++)
                {
                    for (int num10 = 0; num10 < texture2D5.height; num10++)
                    {
                        Color pixel3 = texture2D5.GetPixel(num9, num10);
                        alphamap2HQ[num9, num10, num8 * 4] = pixel3.r;
                        alphamap2HQ[num9, num10, num8 * 4 + 1] = pixel3.g;
                        alphamap2HQ[num9, num10, num8 * 4 + 2] = pixel3.b;
                        alphamap2HQ[num9, num10, num8 * 4 + 3] = pixel3.a;
                    }
                }
                UnityEngine.Object.DestroyImmediate(texture2D5);
                flag2 = true;
            }
            if (flag2 || !ReadWrite.fileExists(Level.info.path + "/Terrain/Alphamap2_" + num8 + ".png", useCloud: false, usePath: false))
            {
                continue;
            }
            byte[] array10 = ReadWrite.readBytes(Level.info.path + "/Terrain/Alphamap2_" + num8 + ".png", useCloud: false, usePath: false);
            Texture2D texture2D6 = new Texture2D(data2.heightmapResolution, data2.heightmapResolution, TextureFormat.ARGB32, mipChain: false);
            texture2D6.name = "Alphamap2_Load";
            texture2D6.hideFlags = HideFlags.HideAndDontSave;
            texture2D6.LoadImage(array10);
            for (int num11 = 0; num11 < texture2D6.width; num11++)
            {
                for (int num12 = 0; num12 < texture2D6.height; num12++)
                {
                    Color pixel4 = texture2D6.GetPixel(num11, num12);
                    alphamap2HQ[num11, num12, num8 * 4] = pixel4.r;
                    alphamap2HQ[num11, num12, num8 * 4 + 1] = pixel4.g;
                    alphamap2HQ[num11, num12, num8 * 4 + 2] = pixel4.b;
                    alphamap2HQ[num11, num12, num8 * 4 + 3] = pixel4.a;
                }
            }
            UnityEngine.Object.DestroyImmediate(texture2D6);
        }
        data2.baseMapResolution = (int)size / 8;
        data2.baseMapResolution = (int)size / 4;
        _terrain2.terrainData = data2;
        data2.wavingGrassTint = Color.white;
    }

    protected static void saveTrees()
    {
        River river = new River(Level.info.path + "/Terrain/Trees.dat", usePath: false);
        river.writeByte(SAVEDATA_TREES_VERSION);
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                List<ResourceSpawnpoint> list = trees[b, b2];
                river.writeUInt16((ushort)list.Count);
                for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
                {
                    ResourceSpawnpoint resourceSpawnpoint = list[num];
                    if (resourceSpawnpoint != null && resourceSpawnpoint.model != null && resourceSpawnpoint.id != 0)
                    {
                        river.writeUInt16(resourceSpawnpoint.id);
                        river.writeSingleVector3(resourceSpawnpoint.point);
                        river.writeBoolean(resourceSpawnpoint.isGenerated);
                    }
                    else
                    {
                        river.writeUInt16(0);
                        river.writeSingleVector3(Vector3.zero);
                        river.writeBoolean(value: true);
                    }
                }
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        if (!Level.info.configData.Use_Legacy_Ground)
        {
            saveTrees();
            return;
        }
        if (hasLegacyDataForConversion)
        {
            File.WriteAllText(GetConversionMarkerFilePath(), "1");
        }
        saveTrees();
    }

    private static string GetConversionMarkerFilePath()
    {
        return Path.Combine(Level.info.path, "Terrain", "TerrainWasAutoConverted.txt");
    }

    private static void onRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y)
    {
        bool canIncrementIndex = true;
        onRegionUpdated(null, old_x, old_y, new_x, new_y, 0, ref canIncrementIndex);
    }

    private static void onPlayerTeleported(Player player, Vector3 position)
    {
        shouldInstantlyLoad = true;
    }

    private static void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
    {
        if (step != 0)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                if (regions[b, b2] && !Regions.checkArea(b, b2, new_x, new_y, RESOURCE_REGIONS))
                {
                    regions[b, b2] = false;
                    if (shouldInstantlyLoad)
                    {
                        List<ResourceSpawnpoint> list = trees[b, b2];
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].disable();
                            if (GraphicsSettings.landmarkQuality >= EGraphicQuality.MEDIUM)
                            {
                                list[i].enableSkybox();
                            }
                        }
                    }
                    else
                    {
                        loads[b, b2] = 0;
                        isRegionalVisibilityDirty = true;
                    }
                }
            }
        }
        if (Regions.checkSafe(new_x, new_y))
        {
            for (int j = new_x - RESOURCE_REGIONS; j <= new_x + RESOURCE_REGIONS; j++)
            {
                for (int k = new_y - RESOURCE_REGIONS; k <= new_y + RESOURCE_REGIONS; k++)
                {
                    if (!Regions.checkSafe((byte)j, (byte)k) || regions[j, k])
                    {
                        continue;
                    }
                    regions[j, k] = true;
                    if (shouldInstantlyLoad)
                    {
                        List<ResourceSpawnpoint> list2 = trees[j, k];
                        for (int l = 0; l < list2.Count; l++)
                        {
                            list2[l].enable();
                            list2[l].disableSkybox();
                        }
                    }
                    else
                    {
                        loads[j, k] = 0;
                        isRegionalVisibilityDirty = true;
                    }
                }
            }
        }
        shouldInstantlyLoad = false;
    }

    private static void onPlayerCreated(Player player)
    {
        if (player.channel.isOwner)
        {
            Player player2 = Player.player;
            player2.onPlayerTeleported = (PlayerTeleported)Delegate.Combine(player2.onPlayerTeleported, new PlayerTeleported(onPlayerTeleported));
            PlayerMovement movement = Player.player.movement;
            movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
        }
    }

    private static void handleEditorAreaRegistered(EditorArea area)
    {
        area.onRegionUpdated = (EditorRegionUpdated)Delegate.Combine(area.onRegionUpdated, new EditorRegionUpdated(onRegionUpdated));
    }

    private void tickRegionalVisibility()
    {
        bool flag = true;
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                if (loads[b, b2] != -1)
                {
                    if (loads[b, b2] >= trees[b, b2].Count)
                    {
                        loads[b, b2] = -1;
                    }
                    else
                    {
                        if (regions[b, b2])
                        {
                            if (!trees[b, b2][loads[b, b2]].isEnabled)
                            {
                                trees[b, b2][loads[b, b2]].enable();
                            }
                            if (trees[b, b2][loads[b, b2]].isSkyboxEnabled)
                            {
                                trees[b, b2][loads[b, b2]].disableSkybox();
                            }
                        }
                        else
                        {
                            if (trees[b, b2][loads[b, b2]].isEnabled)
                            {
                                trees[b, b2][loads[b, b2]].disable();
                            }
                            if (!trees[b, b2][loads[b, b2]].isSkyboxEnabled && GraphicsSettings.landmarkQuality >= EGraphicQuality.MEDIUM)
                            {
                                trees[b, b2][loads[b, b2]].enableSkybox();
                            }
                        }
                        loads[b, b2]++;
                        flag = false;
                    }
                }
            }
        }
        if (flag)
        {
            isRegionalVisibilityDirty = false;
        }
    }

    private void Update()
    {
        _ = Level.isLoaded;
    }

    private void Start()
    {
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
        EditorArea.registered += handleEditorAreaRegistered;
        if (_Triplanar_Primary_Size == -1)
        {
            _Triplanar_Primary_Size = Shader.PropertyToID("_Triplanar_Primary_Size");
        }
        Shader.SetGlobalFloat(_Triplanar_Primary_Size, triplanarPrimarySize);
        if (_Triplanar_Primary_Weight == -1)
        {
            _Triplanar_Primary_Weight = Shader.PropertyToID("_Triplanar_Primary_Weight");
        }
        Shader.SetGlobalFloat(_Triplanar_Primary_Weight, triplanarPrimaryWeight);
        if (_Triplanar_Secondary_Size == -1)
        {
            _Triplanar_Secondary_Size = Shader.PropertyToID("_Triplanar_Secondary_Size");
        }
        Shader.SetGlobalFloat(_Triplanar_Secondary_Size, triplanarSecondarySize);
        if (_Triplanar_Secondary_Weight == -1)
        {
            _Triplanar_Secondary_Weight = Shader.PropertyToID("_Triplanar_Secondary_Weight");
        }
        Shader.SetGlobalFloat(_Triplanar_Secondary_Weight, triplanarSecondaryWeight);
        if (_Triplanar_Tertiary_Size == -1)
        {
            _Triplanar_Tertiary_Size = Shader.PropertyToID("_Triplanar_Tertiary_Size");
        }
        Shader.SetGlobalFloat(_Triplanar_Tertiary_Size, triplanarTertiarySize);
        if (_Triplanar_Tertiary_Weight == -1)
        {
            _Triplanar_Tertiary_Weight = Shader.PropertyToID("_Triplanar_Tertiary_Weight");
        }
        Shader.SetGlobalFloat(_Triplanar_Tertiary_Weight, triplanarTertiaryWeight);
    }

    static LevelGround()
    {
        _Triplanar_Primary_Size = -1;
        _triplanarPrimarySize = 16f;
        _Triplanar_Primary_Weight = -1;
        _triplanarPrimaryWeight = 0.4f;
        _Triplanar_Secondary_Size = -1;
        _triplanarSecondarySize = 64f;
        _Triplanar_Secondary_Weight = -1;
        _triplanarSecondaryWeight = 0.4f;
        _Triplanar_Tertiary_Size = -1;
        _triplanarTertiarySize = 4f;
        _Triplanar_Tertiary_Weight = -1;
        _triplanarTertiaryWeight = 0.2f;
        obstructionColliders = new Collider[16];
        SAVEDATA_TREES_VERSION = 5;
        RESOURCE_REGIONS = 3;
        ALPHAMAPS = 2;
        isRegionalVisibilityDirty = true;
        FoliageSystem.preBakeTile += handlePreBakeTile;
        FoliageSystem.postBake += handlePostBake;
    }
}
