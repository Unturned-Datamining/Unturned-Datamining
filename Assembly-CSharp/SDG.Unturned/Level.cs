using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Foliage;
using SDG.Framework.Landscapes;
using SDG.Framework.Water;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class Level : MonoBehaviour
{
    public delegate void SatelliteCaptureDelegate();

    private class PreCaptureObjectState
    {
        public bool[,][] wasTreeEnabled = new bool[Regions.WORLD_SIZE, Regions.WORLD_SIZE][];

        public bool[,][] wasTreeSkyboxEnabled = new bool[Regions.WORLD_SIZE, Regions.WORLD_SIZE][];
    }

    private const float STEPS = 19f;

    public static readonly int BUILD_INDEX_SETUP = 0;

    public static readonly int BUILD_INDEX_MENU = 1;

    public static readonly int BUILD_INDEX_GAME = 2;

    public static readonly int BUILD_INDEX_LOADING = 3;

    public static readonly float HEIGHT = 1024f;

    public static readonly float TERRAIN = 256f;

    public static readonly ushort CLIP = 8;

    public static readonly ushort TINY_BORDER = 16;

    public static readonly ushort SMALL_BORDER = 64;

    public static readonly ushort MEDIUM_BORDER = 64;

    public static readonly ushort LARGE_BORDER = 64;

    public static readonly ushort INSANE_BORDER = 128;

    public static readonly ushort TINY_SIZE = 512;

    public static readonly ushort SMALL_SIZE = 1024;

    public static readonly ushort MEDIUM_SIZE = 2048;

    public static readonly ushort LARGE_SIZE = 4096;

    public static readonly ushort INSANE_SIZE = 8192;

    public static readonly byte SAVEDATA_VERSION = 2;

    public static PrePreLevelLoaded onPrePreLevelLoaded;

    public static PreLevelLoaded onPreLevelLoaded;

    public static LevelLoaded onLevelLoaded;

    public static PostLevelLoaded onPostLevelLoaded;

    public static LevelsRefreshed onLevelsRefreshed;

    public static LevelExited onLevelExited;

    private static LevelInfo _info;

    private static GameObject satelliteCaptureGameObject;

    private static Transform satelliteCaptureTransform;

    private static Camera satelliteCaptureCamera;

    private static Transform _level;

    private static Transform _roots;

    private static Transform _clips;

    private static Transform _effects;

    private static Transform _spawns;

    private static Transform _editing;

    private static Level instance;

    /// <summary>
    /// Placeholder created between unloading the main menu and loading into game or editor.
    /// </summary>
    internal static AudioListener placeholderAudioListener;

    /// <summary>
    /// Loading screen music.
    /// </summary>
    private static AudioSource musicAudioSource;

    /// <summary>
    /// Clip to play to fade out loop.
    /// </summary>
    private static AudioClip musicOutroClip;

    private static float musicOutroVolume;

    private static bool _isInitialized;

    private static bool _isEditor;

    public static bool isLoadingContent = true;

    public static bool isLoadingLighting = true;

    public static bool isLoadingVehicles = true;

    public static bool isLoadingBarricades = true;

    public static bool isLoadingStructures = true;

    public static bool isLoadingArea = true;

    private static bool _isLoaded;

    private static List<byte[]> pendingHashes;

    /// <summary>
    /// Useful to narrow down why a player is getting kicked for modified level files when joining a server.
    /// </summary>
    private static CommandLineFlag shouldLogLevelHash = new CommandLineFlag(defaultValue: false, "-LogLevelHash");

    private static List<LevelInfo> knownLevels = new List<LevelInfo>();

    private static FoliageVolumeManager foliageVolumeManager;

    private static UndergroundWhitelistVolumeManager undergroundWhitelistVolumeManager;

    private static PlayerClipVolumeManager playerClipVolumeManager;

    private static NavClipVolumeManager navClipVolumeManager;

    private static WaterVolumeManager waterVolumeManager;

    private static LandscapeHoleVolumeManager landscapeHoleVolumeManager;

    private static DeadzoneVolumeManager deadzoneVolumeManager;

    private static KillVolumeManager killVolumeManager;

    private static EffectVolumeManager effectVolumeManager;

    private static AmbianceVolumeManager ambianceVolumeManager;

    private static TeleporterEntranceVolumeManager entranceVolumeManager;

    private static TeleporterExitVolumeManager exitVolumeManager;

    private static SafezoneVolumeManager safezoneVolumeManager;

    private static ArenaCompactorVolumeManager arenaVolumeManager;

    private static HordePurchaseVolumeManager hordePurchaseVolumeManager;

    private static CartographyVolumeManager cartographyVolumeManager;

    private static OxygenVolumeManager oxygenVolumeManager;

    private static CullingVolumeManager cullingVolumeManager;

    private static AirdropDevkitNodeSystem airdropNodeSystem;

    private static LocationDevkitNodeSystem locationNodeSystem;

    private static SpawnpointSystemV2 spawnpointSystem;

    private static bool _loadingScreenWantsMusic;

    private static CommandLineBool clUseLevelBatching = new CommandLineBool("-UseLevelBatching");

    private static CommandLineFlag shouldLogSpawnTablesAfterLoadingLevel = new CommandLineFlag(defaultValue: false, "-LogSpawnTablesAfterLoadingLevel");

    public static ushort border
    {
        get
        {
            if (info == null)
            {
                return 1;
            }
            if (info.size == ELevelSize.TINY)
            {
                return TINY_BORDER;
            }
            if (info.size == ELevelSize.SMALL)
            {
                return SMALL_BORDER;
            }
            if (info.size == ELevelSize.MEDIUM)
            {
                return MEDIUM_BORDER;
            }
            if (info.size == ELevelSize.LARGE)
            {
                return LARGE_BORDER;
            }
            if (info.size == ELevelSize.INSANE)
            {
                return INSANE_BORDER;
            }
            return 0;
        }
    }

    public static ushort size
    {
        get
        {
            if (info == null)
            {
                return 8;
            }
            if (info.size == ELevelSize.TINY)
            {
                return TINY_SIZE;
            }
            if (info.size == ELevelSize.SMALL)
            {
                return SMALL_SIZE;
            }
            if (info.size == ELevelSize.MEDIUM)
            {
                return MEDIUM_SIZE;
            }
            if (info.size == ELevelSize.LARGE)
            {
                return LARGE_SIZE;
            }
            if (info.size == ELevelSize.INSANE)
            {
                return INSANE_SIZE;
            }
            return 0;
        }
    }

    public static LevelInfo info => _info;

    /// <summary>
    /// Should loading code proceed with redirects?
    /// Disabled by level and when in the editor.
    /// </summary>
    public static bool shouldUseHolidayRedirects { get; private set; }

    public static bool shouldUseLevelBatching { get; private set; }

    public static Transform level => _level;

    public static Transform roots => _roots;

    public static Transform clips => _clips;

    [Obsolete("Was the parent of all effects in the past, but now empty for TransformHierarchy performance.")]
    public static Transform effects
    {
        get
        {
            if (_effects == null)
            {
                _effects = new GameObject().transform;
                _effects.name = "Effects";
                _effects.parent = level;
                _effects.tag = "Logic";
                _effects.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing Level.effecs which has been deprecated.");
            }
            return _effects;
        }
    }

    [Obsolete("Was the parent of gameplay objects in the past, but now empty for TransformHierarchy performance.")]
    public static Transform spawns
    {
        get
        {
            if (_spawns == null)
            {
                _spawns = new GameObject().transform;
                _spawns.name = "Spawns";
                _spawns.parent = level;
                _spawns.tag = "Logic";
                _spawns.gameObject.layer = 8;
                CommandWindow.LogWarningFormat("Plugin referencing Level.spawns which has been deprecated.");
            }
            return _spawns;
        }
    }

    public static Transform editing => _editing;

    public static bool isInitialized => _isInitialized;

    public static bool isEditor => _isEditor;

    public static bool isExiting { get; protected set; }

    public static bool isVR
    {
        get
        {
            if (PlaySettings.isVR)
            {
                return isEditor;
            }
            return false;
        }
    }

    public static bool isLoading
    {
        get
        {
            if (Provider.isConnected)
            {
                if (!isLoadingContent && !isLoadingLighting && !isLoadingVehicles && !isLoadingBarricades && !isLoadingStructures)
                {
                    return isLoadingArea;
                }
                return true;
            }
            if (isEditor)
            {
                return isLoadingContent;
            }
            return false;
        }
    }

    public static bool isLoaded => _isLoaded;

    public static byte[] hash { get; private set; }

    /// <summary>
    /// Display version string of the currently loaded level.
    /// </summary>
    public static string version
    {
        get
        {
            if (info == null || info.configData == null)
            {
                return "0.0.0.0";
            }
            return info.configData.Version;
        }
    }

    /// <summary>
    /// Version string of the currently loaded level packed into an integer.
    /// </summary>
    public static uint packedVersion
    {
        get
        {
            if (info == null || info.configData == null)
            {
                return 0u;
            }
            return info.configData.PackedVersion;
        }
    }

    internal static bool LoadingScreenWantsMusic
    {
        set
        {
            if (_loadingScreenWantsMusic != value)
            {
                _loadingScreenWantsMusic = value;
                if (!_loadingScreenWantsMusic)
                {
                    PlayLoadingOutroMusic();
                }
            }
        }
    }

    public static event LevelLoadingStepHandler loadingSteps;

    public static event SatelliteCaptureDelegate onSatellitePreCapture;

    public static event SatelliteCaptureDelegate onSatellitePostCapture;

    /// <summary>
    /// Is a point safely within the level bounds?
    /// Also checks player clip volumes if legacy borders are disabled.
    /// </summary>
    public static bool checkSafeIncludingClipVolumes(Vector3 point)
    {
        if (info != null && !info.configData.Use_Legacy_Clip_Borders)
        {
            return !VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>.Get().IsPositionInsideAnyVolume(point);
        }
        if (!isPointWithinValidHeight(point.y))
        {
            return false;
        }
        Vector3 vector = new Vector3(Mathf.Abs(point.x), point.y, Mathf.Abs(point.z));
        if (vector.x > (float)(size / 2 - border) || vector.z > (float)(size / 2 - border))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Is given Y (vertical) coordinate within level's height range?
    /// Maps using landscapes have a larger range than older maps.
    /// </summary>
    public static bool isPointWithinValidHeight(float y)
    {
        if (y >= -1024f)
        {
            return y <= 1024f;
        }
        return false;
    }

    [Obsolete("Replaced by checkSafeIncludingClipVolumes or the newer isPointWithinValidHeight")]
    public static bool checkLevel(Vector3 point)
    {
        return checkSafeIncludingClipVolumes(point);
    }

    /// <summary>
    /// Notify menus that levels list has changed.
    /// Used when creating/deleting levels, as well as following workshop changes.
    /// </summary>
    public static void broadcastLevelsRefreshed()
    {
        onLevelsRefreshed?.Invoke();
    }

    /// <summary>
    /// Get level's cached asset, if any.
    /// </summary>
    public static LevelAsset getAsset()
    {
        if (info == null)
        {
            return null;
        }
        return info.resolveAsset();
    }

    private static void updateCachedHolidayRedirects()
    {
        shouldUseHolidayRedirects = !isEditor && info != null && info.configData != null && info.configData.Allow_Holiday_Redirects && HolidayUtil.getActiveHoliday() != ENPCHoliday.NONE;
    }

    private static void UpdateShouldUseLevelBatching()
    {
        shouldUseLevelBatching = (!clUseLevelBatching.hasValue || clUseLevelBatching.value) && !isEditor && !Dedicator.IsDedicatedServer && info != null && info.configData != null && info.configData.Batching_Version > 1;
    }

    public static void includeHash(string id, byte[] pendingHash)
    {
        if ((bool)shouldLogLevelHash)
        {
            UnturnedLog.info($"[{pendingHashes.Count}] Including \"{id}\" in level hash: {Hash.toString(pendingHash)}");
        }
        pendingHashes.Add(pendingHash);
    }

    private static void combineHashes()
    {
        hash = Hash.combine(pendingHashes);
        if ((bool)shouldLogLevelHash)
        {
            UnturnedLog.info("Combined level hash: " + Hash.toString(hash));
        }
    }

    public static void setEnabled(bool isEnabled)
    {
        clips.gameObject.SetActive(isEnabled);
    }

    public static void add(string name, ELevelSize size, ELevelType type)
    {
        if (!ReadWrite.folderExists("/Maps/" + name))
        {
            ReadWrite.createFolder("/Maps/" + name);
            Block block = new Block();
            block.writeByte(SAVEDATA_VERSION);
            block.writeSteamID(Provider.client);
            block.writeByte((byte)size);
            block.writeByte((byte)type);
            ReadWrite.writeBlock("/Maps/" + name + "/Level.dat", useCloud: false, block);
            ReadWrite.copyFile("/Extras/LevelTemplate/Charts.unity3d", "/Maps/" + name + "/Charts.unity3d");
            ReadWrite.copyFile("/Extras/LevelTemplate/Details.unity3d", "/Maps/" + name + "/Terrain/Details.unity3d");
            ReadWrite.copyFile("/Extras/LevelTemplate/Details.dat", "/Maps/" + name + "/Terrain/Details.dat");
            ReadWrite.copyFile("/Extras/LevelTemplate/Materials.unity3d", "/Maps/" + name + "/Terrain/Materials.unity3d");
            ReadWrite.copyFile("/Extras/LevelTemplate/Materials.dat", "/Maps/" + name + "/Terrain/Materials.dat");
            ReadWrite.copyFile("/Extras/LevelTemplate/Resources.dat", "/Maps/" + name + "/Terrain/Resources.dat");
            ReadWrite.copyFile("/Extras/LevelTemplate/Lighting.dat", "/Maps/" + name + "/Environment/Lighting.dat");
            ReadWrite.copyFile("/Extras/LevelTemplate/Roads.unity3d", "/Maps/" + name + "/Environment/Roads.unity3d");
            ReadWrite.copyFile("/Extras/LevelTemplate/Roads.dat", "/Maps/" + name + "/Environment/Roads.dat");
            ReadWrite.copyFile("/Extras/LevelTemplate/Ambience.unity3d", "/Maps/" + name + "/Environment/Ambience.unity3d");
            broadcastLevelsRefreshed();
        }
    }

    public static void remove(string name)
    {
        ReadWrite.deleteFolder("/Maps/" + name);
        broadcastLevelsRefreshed();
    }

    public static void save()
    {
        DirtyManager.save();
        LevelObjects.save();
        LevelLighting.save();
        LevelGround.save();
        LevelRoads.save();
        if (!isVR)
        {
            LevelNavigation.save();
            LevelNodes.save();
            LevelItems.save();
            LevelPlayers.save();
            LevelZombies.save();
            LevelVehicles.save();
            LevelAnimals.save();
            LevelVisibility.save();
        }
        Editor.save();
    }

    public static void edit(LevelInfo newInfo)
    {
        _isEditor = true;
        isExiting = false;
        _info = newInfo;
        LoadingUI.updateScene();
        SceneManager.LoadScene("Game");
        PlayLevelLoadingScreenMusic();
        Provider.resetChannels();
        Provider.updateRichPresence();
        DevkitTransactionManager.resetTransactions();
        updateCachedHolidayRedirects();
        UpdateShouldUseLevelBatching();
    }

    public static void load(LevelInfo newInfo, bool hasAuthority)
    {
        _isEditor = false;
        isExiting = false;
        _info = newInfo;
        LoadingUI.updateScene();
        SceneManager.LoadScene("Game");
        PlayLevelLoadingScreenMusic();
        if (!Dedicator.IsDedicatedServer)
        {
            string text = null;
            if (string.Equals(info.name, "A6 Polaris", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Frost_Visited";
            }
            else if (string.Equals(info.name, "arid", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Arid_Visited";
            }
            else if (string.Equals(info.name, "buak", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Buak_Visited";
            }
            else if (string.Equals(info.name, "elver", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Elver_Visited";
            }
            else if (string.Equals(info.name, "germany", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Peaks";
            }
            else if (string.Equals(info.name, "hawaii", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Hawaii";
            }
            else if (string.Equals(info.name, "ireland", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Ireland_Visited";
            }
            else if (string.Equals(info.name, "kuwait", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Kuwait_Visited";
            }
            else if (string.Equals(info.name, "pei", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "PEI";
            }
            else if (string.Equals(info.name, "russia", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Russia";
            }
            else if (string.Equals(info.name, "washington", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Washington";
            }
            else if (string.Equals(info.name, "yukon", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Yukon";
            }
            if (!string.IsNullOrEmpty(text))
            {
                Provider.provider.achievementsService.setAchievement(text);
            }
        }
        if (hasAuthority)
        {
            string text2 = LevelSavedata.transformName("Cyrpus Survival");
            string text3 = LevelSavedata.transformName("Cyprus Survival");
            if (ReadWrite.folderExists(text2) && !ReadWrite.folderExists(text3))
            {
                ReadWrite.moveFolder(text2, text3);
                UnturnedLog.info("Moved Cyprus save folder");
            }
        }
        Provider.updateRichPresence();
        DevkitTransactionManager.resetTransactions();
        updateCachedHolidayRedirects();
        UpdateShouldUseLevelBatching();
    }

    public static void loading()
    {
        SceneManager.LoadScene("Loading");
    }

    public static void exit()
    {
        onLevelExited?.Invoke();
        _isEditor = false;
        isExiting = true;
        _info = null;
        LoadingUI.updateScene();
        if (!Dedicator.IsDedicatedServer)
        {
            LoadingUI.SetLoadingText("Loading_MainMenu");
            instance.StartCoroutine(instance.ReturnToMainMenu());
        }
    }

    public static LevelInfo getLevel(string name)
    {
        ScanKnownLevels();
        foreach (LevelInfo knownLevel in knownLevels)
        {
            if (string.Equals(name, knownLevel.name, StringComparison.OrdinalIgnoreCase))
            {
                return knownLevel;
            }
        }
        return null;
    }

    private static LevelInfo ReadLevelInfo(string path, bool usePath, ulong publishedFileId = 0uL)
    {
        if (usePath)
        {
            path = ReadWrite.PATH + path;
        }
        return ReadLevelInfo(path, publishedFileId);
    }

    /// <summary>
    /// Load level details from Level.dat in directory path.
    /// </summary>
    private static LevelInfo ReadLevelInfo(string directoryPath, ulong publishedFileId = 0uL)
    {
        try
        {
            string path = Path.Combine(directoryPath, "Level.dat");
            if (!File.Exists(path))
            {
                return null;
            }
            Block block = ReadWrite.readBlock(path, useCloud: false, usePath: false, 0);
            byte b = block.readByte();
            byte[] array = block.getHash();
            bool newEditable = block.readSteamID() == Provider.client || ReadWrite.fileExists(Path.Combine(directoryPath, ".unlocker"), useCloud: false, usePath: false);
            ELevelSize newSize = (ELevelSize)block.readByte();
            ELevelType newType = ELevelType.SURVIVAL;
            if (b > 1)
            {
                newType = (ELevelType)block.readByte();
            }
            string text = ReadWrite.folderName(directoryPath);
            string path2 = Path.Combine(directoryPath, "Config.json");
            LevelInfoConfigData levelInfoConfigData;
            if (File.Exists(path2))
            {
                try
                {
                    using FileStream underlyingStream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
                    using StreamReader streamReader = new StreamReader(sHA1Stream);
                    levelInfoConfigData = JsonConvert.DeserializeObject<LevelInfoConfigData>(streamReader.ReadToEnd());
                    levelInfoConfigData.Hash = sHA1Stream.Hash;
                }
                catch
                {
                    Assets.reportError($"Unable to parse {text}/Config.json! Consider validating with a JSON linter");
                    levelInfoConfigData = null;
                }
                if (levelInfoConfigData == null)
                {
                    levelInfoConfigData = new LevelInfoConfigData();
                }
            }
            else
            {
                levelInfoConfigData = new LevelInfoConfigData();
            }
            if (!Parser.TryGetUInt32FromIP(levelInfoConfigData.Version, out levelInfoConfigData.PackedVersion))
            {
                Assets.reportError($"Unable to parse level \"{text}\" version \"{levelInfoConfigData.PackedVersion}\". Expected format \"#.#.#.#\". Resetting to zero.");
                levelInfoConfigData.Version = "0.0.0.0";
                levelInfoConfigData.PackedVersion = 0u;
            }
            return new LevelInfo(directoryPath, text, newSize, newType, newEditable, levelInfoConfigData, publishedFileId, array);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception reading level info file (" + directoryPath + "):");
            return null;
        }
    }

    private static bool doesLevelPassFilter(LevelInfo levelInfo, ESingleplayerMapCategory categoryFilter)
    {
        switch (categoryFilter)
        {
        case ESingleplayerMapCategory.OFFICIAL:
            return levelInfo.configData.Category == ESingleplayerMapCategory.OFFICIAL;
        case ESingleplayerMapCategory.CURATED:
            if (levelInfo.type != ELevelType.ARENA)
            {
                return levelInfo.isCurated;
            }
            return false;
        case ESingleplayerMapCategory.WORKSHOP:
            if (levelInfo.isFromWorkshop)
            {
                return !levelInfo.isCurated;
            }
            return false;
        case ESingleplayerMapCategory.MISC:
        {
            bool flag = levelInfo.type != ELevelType.ARENA && levelInfo.isCurated;
            bool flag2 = levelInfo.configData.Category == ESingleplayerMapCategory.OFFICIAL || levelInfo.isFromWorkshop || flag;
            if (levelInfo.configData.Category != ESingleplayerMapCategory.MISC)
            {
                if (levelInfo.isEditable)
                {
                    return !flag2;
                }
                return false;
            }
            return true;
        }
        case ESingleplayerMapCategory.ALL:
            return true;
        case ESingleplayerMapCategory.EDITABLE:
            return levelInfo.isEditable;
        default:
            UnturnedLog.warn("Unknown map filter '{0}'", categoryFilter);
            return true;
        }
    }

    public static LevelInfo[] getLevels(ESingleplayerMapCategory categoryFilter)
    {
        ScanKnownLevels();
        List<LevelInfo> list = new List<LevelInfo>();
        foreach (LevelInfo knownLevel in knownLevels)
        {
            if (doesLevelPassFilter(knownLevel, categoryFilter))
            {
                list.Add(knownLevel);
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// Server list allows player to enter a map name when searching, so we try to find a local
    /// copy of the level for version number comparison. (Server map version might differ.)
    /// </summary>
    public static LevelInfo findLevelForServerFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Length < 2)
        {
            return null;
        }
        ScanKnownLevels();
        foreach (LevelInfo knownLevel in knownLevels)
        {
            if (knownLevel.configData != null && knownLevel.configData.PackedVersion != 0 && knownLevel.name.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
            {
                return knownLevel;
            }
        }
        return null;
    }

    private static LevelInfo FindKnownLevelByPath(string path)
    {
        foreach (LevelInfo knownLevel in knownLevels)
        {
            if (string.Equals(knownLevel.path, path))
            {
                return knownLevel;
            }
        }
        return null;
    }

    private static LevelInfo FindKnownLevelByPublishedFileId(ulong fileId)
    {
        foreach (LevelInfo knownLevel in knownLevels)
        {
            if (knownLevel.publishedFileId == fileId)
            {
                return knownLevel;
            }
        }
        return null;
    }

    /// <summary>
    /// Search all map folders to add any previously unregistered maps.
    /// </summary>
    private static void ScanKnownLevels()
    {
        try
        {
            for (int num = knownLevels.Count - 1; num >= 0; num--)
            {
                LevelInfo levelInfo = knownLevels[num];
                if (!Directory.Exists(levelInfo.path))
                {
                    knownLevels.RemoveAt(num);
                    UnturnedLog.info("Removed previously discovered level \"" + levelInfo.name + "\" at \"" + levelInfo.path + "\" (no longer exists)");
                }
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception checking for deleted levels:");
        }
        try
        {
            string[] directories = Directory.GetDirectories(PathEx.Join(UnturnedPaths.RootDirectory, "Maps"));
            foreach (string text in directories)
            {
                if (FindKnownLevelByPath(text) == null)
                {
                    LevelInfo levelInfo2 = ReadLevelInfo(text, 0uL);
                    if (levelInfo2 != null)
                    {
                        knownLevels.Add(levelInfo2);
                        UnturnedLog.info("Discovered level \"" + levelInfo2.name + "\" at \"" + levelInfo2.path + "\"");
                    }
                }
            }
        }
        catch (Exception e2)
        {
            UnturnedLog.exception(e2, "Caught exception loading levels in root Maps folder:");
        }
        if (Provider.provider.workshopService.ugc != null)
        {
            try
            {
                foreach (SteamContent item in Provider.provider.workshopService.ugc)
                {
                    if (item.type == ESteamUGCType.MAP && LocalWorkshopSettings.get().getEnabled(item.publishedFileID) && FindKnownLevelByPublishedFileId(item.publishedFileID.m_PublishedFileId) == null)
                    {
                        LevelInfo levelInfo3 = ReadLevelInfo(ReadWrite.folderFound(item.path, usePath: false), item.publishedFileID.m_PublishedFileId);
                        if (levelInfo3 != null)
                        {
                            knownLevels.Add(levelInfo3);
                            UnturnedLog.info("Discovered level \"" + levelInfo3.name + "\" at \"" + levelInfo3.path + "\"");
                        }
                    }
                }
            }
            catch (Exception e3)
            {
                UnturnedLog.exception(e3, "Caught exception loading levels from Steam Workshop:");
            }
        }
        else
        {
            string text2 = PathEx.Join(UnturnedPaths.RootDirectory, "Bundles", "Workshop", "Maps");
            try
            {
                if (!ReadWrite.folderExists(text2, usePath: false))
                {
                    ReadWrite.createFolder(text2, usePath: false);
                }
                string[] directories = Directory.GetDirectories(text2);
                foreach (string text3 in directories)
                {
                    if (FindKnownLevelByPath(text3) == null)
                    {
                        LevelInfo levelInfo4 = ReadLevelInfo(text3, 0uL);
                        if (levelInfo4 != null)
                        {
                            knownLevels.Add(levelInfo4);
                            UnturnedLog.info("Discovered level \"" + levelInfo4.name + "\" at \"" + levelInfo4.path + "\"");
                        }
                    }
                }
            }
            catch (Exception e4)
            {
                UnturnedLog.exception(e4, "Caught exception loading levels in legacy server global workshop Maps folder (" + text2 + "):");
            }
            string text4 = Path.Combine(ServerSavedata.directory, Provider.serverID, "Workshop", "Maps");
            try
            {
                if (!ReadWrite.folderExists(text4, usePath: false))
                {
                    ReadWrite.createFolder(text4, usePath: false);
                }
                string[] directories = Directory.GetDirectories(text4);
                foreach (string text5 in directories)
                {
                    if (FindKnownLevelByPath(text5) == null)
                    {
                        LevelInfo levelInfo5 = ReadLevelInfo(text5, 0uL);
                        if (levelInfo5 != null)
                        {
                            knownLevels.Add(levelInfo5);
                            UnturnedLog.info("Discovered level \"" + levelInfo5.name + "\" at \"" + levelInfo5.path + "\"");
                        }
                    }
                }
            }
            catch (Exception e5)
            {
                UnturnedLog.exception(e5, "Caught exception loading levels in legacy per-server workshop Maps folder (" + text4 + "):");
            }
            string text6 = Path.Combine(ServerSavedata.directory, Provider.serverID, "Maps");
            try
            {
                if (!ReadWrite.folderExists(text6, usePath: false))
                {
                    ReadWrite.createFolder(text6, usePath: false);
                }
                string[] directories = Directory.GetDirectories(text6);
                foreach (string text7 in directories)
                {
                    if (FindKnownLevelByPath(text7) == null)
                    {
                        LevelInfo levelInfo6 = ReadLevelInfo(text7, 0uL);
                        if (levelInfo6 != null)
                        {
                            knownLevels.Add(levelInfo6);
                            UnturnedLog.info("Discovered level \"" + levelInfo6.name + "\" at \"" + levelInfo6.path + "\"");
                        }
                    }
                }
            }
            catch (Exception e6)
            {
                UnturnedLog.exception(e6, "Caught exception loading levels in legacy per-server Maps folder (" + text6 + "):");
            }
        }
        if (DedicatedUGC.ugc == null)
        {
            return;
        }
        try
        {
            foreach (SteamContent item2 in DedicatedUGC.ugc)
            {
                if (item2.type == ESteamUGCType.MAP && FindKnownLevelByPublishedFileId(item2.publishedFileID.m_PublishedFileId) == null)
                {
                    LevelInfo levelInfo7 = ReadLevelInfo(ReadWrite.folderFound(item2.path, usePath: false), item2.publishedFileID.m_PublishedFileId);
                    if (levelInfo7 != null)
                    {
                        knownLevels.Add(levelInfo7);
                        UnturnedLog.info("Discovered level \"" + levelInfo7.name + "\" at \"" + levelInfo7.path + "\"");
                    }
                }
            }
        }
        catch (Exception e7)
        {
            UnturnedLog.exception(e7, "Caught exception loading levels from server Steam Workshop:");
        }
    }

    public static void bindSatelliteCaptureInEditor(SatelliteCaptureDelegate preCapture, SatelliteCaptureDelegate postCapture)
    {
        if (isEditor)
        {
            onSatellitePreCapture += preCapture;
            onSatellitePostCapture += postCapture;
        }
    }

    public static void unbindSatelliteCapture(SatelliteCaptureDelegate preCapture, SatelliteCaptureDelegate postCapture)
    {
        onSatellitePreCapture -= preCapture;
        onSatellitePostCapture -= postCapture;
    }

    private static PreCaptureObjectState GetObjectState()
    {
        PreCaptureObjectState preCaptureObjectState = new PreCaptureObjectState();
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                foreach (LevelObject item in LevelObjects.objects[b, b2])
                {
                    ObjectAsset asset = item.asset;
                    bool isActiveOverrideForSatelliteCapture = asset != null && asset.holidayRestriction == ENPCHoliday.NONE;
                    item.SetIsActiveOverrideForSatelliteCapture(isActiveOverrideForSatelliteCapture);
                }
                List<ResourceSpawnpoint> list = LevelGround.trees[b, b2];
                preCaptureObjectState.wasTreeEnabled[b, b2] = new bool[list.Count];
                preCaptureObjectState.wasTreeSkyboxEnabled[b, b2] = new bool[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    ResourceSpawnpoint resourceSpawnpoint = list[i];
                    preCaptureObjectState.wasTreeEnabled[b, b2][i] = resourceSpawnpoint.isEnabled;
                    preCaptureObjectState.wasTreeSkyboxEnabled[b, b2][i] = resourceSpawnpoint.isSkyboxEnabled;
                    ResourceAsset asset2 = resourceSpawnpoint.asset;
                    if (asset2 != null && asset2.holidayRestriction == ENPCHoliday.NONE)
                    {
                        resourceSpawnpoint.enable();
                    }
                    else
                    {
                        resourceSpawnpoint.disable();
                    }
                    resourceSpawnpoint.disableSkybox();
                }
            }
        }
        return preCaptureObjectState;
    }

    private static void RestorePreCaptureState(PreCaptureObjectState state)
    {
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                foreach (LevelObject item in LevelObjects.objects[b, b2])
                {
                    item.UpdateActiveAndRenderersEnabled();
                }
                List<ResourceSpawnpoint> list = LevelGround.trees[b, b2];
                for (int i = 0; i < list.Count; i++)
                {
                    ResourceSpawnpoint resourceSpawnpoint = list[i];
                    if (state.wasTreeEnabled[b, b2][i])
                    {
                        resourceSpawnpoint.enable();
                    }
                    else
                    {
                        resourceSpawnpoint.disable();
                    }
                    if (state.wasTreeSkyboxEnabled[b, b2][i])
                    {
                        resourceSpawnpoint.enableSkybox();
                    }
                }
            }
        }
    }

    public static void CaptureSatelliteImage()
    {
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        int num;
        int num2;
        if (mainVolume != null)
        {
            mainVolume.GetSatelliteCaptureTransform(out var position, out var rotation);
            satelliteCaptureTransform.SetPositionAndRotation(position, rotation);
            Vector3 vector = mainVolume.CalculateLocalBounds().size;
            num = Mathf.CeilToInt(vector.x);
            num2 = Mathf.CeilToInt(vector.z);
            satelliteCaptureCamera.aspect = vector.x / vector.z;
            satelliteCaptureCamera.orthographicSize = vector.z * 0.5f;
        }
        else
        {
            num = size;
            num2 = size;
            satelliteCaptureTransform.position = new Vector3(0f, 1028f, 0f);
            satelliteCaptureTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
            satelliteCaptureCamera.orthographicSize = size / 2 - border;
            satelliteCaptureCamera.aspect = 1f;
        }
        RenderTexture temporary = RenderTexture.GetTemporary(num * 2, num2 * 2, 32);
        temporary.name = "Satellite";
        temporary.filterMode = FilterMode.Bilinear;
        satelliteCaptureCamera.targetTexture = temporary;
        bool fog = RenderSettings.fog;
        AmbientMode ambientMode = RenderSettings.ambientMode;
        Color ambientSkyColor = RenderSettings.ambientSkyColor;
        Color ambientEquatorColor = RenderSettings.ambientEquatorColor;
        Color ambientGroundColor = RenderSettings.ambientGroundColor;
        float lodBias = QualitySettings.lodBias;
        float seaFloat = LevelLighting.getSeaFloat("_Shininess");
        Color seaColor = LevelLighting.getSeaColor("_SpecularColor");
        ERenderMode renderMode = GraphicsSettings.renderMode;
        GraphicsSettings.renderMode = ERenderMode.FORWARD;
        GraphicsSettings.apply("capturing satellite");
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Palette.AMBIENT;
        RenderSettings.ambientEquatorColor = Palette.AMBIENT;
        RenderSettings.ambientGroundColor = Palette.AMBIENT;
        LevelLighting.setSeaFloat("_Shininess", 500f);
        LevelLighting.setSeaColor("_SpecularColor", Color.black);
        QualitySettings.lodBias = float.MaxValue;
        PreCaptureObjectState objectState = GetObjectState();
        Level.onSatellitePreCapture?.Invoke();
        satelliteCaptureCamera.Render();
        Level.onSatellitePostCapture?.Invoke();
        RestorePreCaptureState(objectState);
        GraphicsSettings.renderMode = renderMode;
        GraphicsSettings.apply("finished capturing satellite");
        RenderSettings.fog = fog;
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        LevelLighting.setSeaFloat("_Shininess", seaFloat);
        LevelLighting.setSeaColor("_SpecularColor", seaColor);
        QualitySettings.lodBias = lodBias;
        RenderTexture temporary2 = RenderTexture.GetTemporary(num, num2);
        Graphics.Blit(temporary, temporary2);
        RenderTexture.ReleaseTemporary(temporary);
        RenderTexture.active = temporary2;
        Texture2D texture2D = new Texture2D(num, num2);
        texture2D.name = "Satellite";
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
        RenderTexture.ReleaseTemporary(temporary2);
        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                Color pixel = texture2D.GetPixel(i, j);
                if (pixel.a < 1f)
                {
                    pixel.a = 1f;
                    texture2D.SetPixel(i, j, pixel);
                }
            }
        }
        texture2D.Apply();
        byte[] bytes = texture2D.EncodeToPNG();
        ReadWrite.writeBytes(info.path + "/Map.png", useCloud: false, usePath: false, bytes);
        UnityEngine.Object.DestroyImmediate(texture2D);
    }

    private static void FindChartHit(Vector3 pos, out EObjectChart chart, out RaycastHit hit)
    {
        Physics.Raycast(pos, Vector3.down, out hit, HEIGHT, RayMasks.CHART);
        chart = EObjectChart.NONE;
        ObjectAsset asset = LevelObjects.getAsset(hit.transform);
        if (asset != null)
        {
            chart = asset.chart;
        }
        else
        {
            ResourceAsset resourceAsset = LevelGround.FindResourceSpawnpointByTransform(hit.transform)?.asset;
            if (resourceAsset != null)
            {
                chart = resourceAsset.chart;
            }
        }
        if (chart == EObjectChart.IGNORE)
        {
            FindChartHit(hit.point + new Vector3(0f, -0.01f, 0f), out chart, out hit);
        }
    }

    public static void CaptureChartImage()
    {
        Bundle bundle = Bundles.getBundle(info.path + "/Charts.unity3d", prependRoot: false);
        if (bundle == null)
        {
            UnturnedLog.error("Unable to load chart colors");
            return;
        }
        Texture2D heightStrip = bundle.load<Texture2D>("Height_Strip");
        Texture2D layerStrip = bundle.load<Texture2D>("Layer_Strip");
        bundle.unload();
        if (heightStrip == null || layerStrip == null)
        {
            UnturnedLog.error("Unable to find height and layer strip textures");
            return;
        }
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        float terrainMinHeight;
        float terrainMaxHeight;
        int imageWidth;
        int imageHeight;
        float captureWidth;
        float captureHeight;
        if (mainVolume != null)
        {
            mainVolume.GetSatelliteCaptureTransform(out var position, out var rotation);
            satelliteCaptureTransform.SetPositionAndRotation(position, rotation);
            Bounds bounds = mainVolume.CalculateWorldBounds();
            terrainMinHeight = bounds.min.y;
            terrainMaxHeight = bounds.max.y;
            Vector3 vector = mainVolume.CalculateLocalBounds().size;
            imageWidth = Mathf.CeilToInt(vector.x);
            imageHeight = Mathf.CeilToInt(vector.z);
            captureWidth = vector.x;
            captureHeight = vector.z;
        }
        else
        {
            imageWidth = size;
            imageHeight = size;
            captureWidth = (float)(int)size - (float)(int)border * 2f;
            captureHeight = (float)(int)size - (float)(int)border * 2f;
            satelliteCaptureTransform.position = new Vector3(0f, 1028f, 0f);
            satelliteCaptureTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
            terrainMinHeight = WaterVolumeManager.worldSeaLevel;
            terrainMaxHeight = TERRAIN;
        }
        Texture2D texture2D = new Texture2D(imageWidth, imageHeight);
        texture2D.name = "Chart";
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        PreCaptureObjectState objectState = GetObjectState();
        GameObject terrainGO = new GameObject();
        terrainGO.layer = 20;
        for (int i = 0; i < imageWidth; i++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                Color color = GetColor((float)i + 0.25f, (float)j + 0.25f) * 0.25f + GetColor((float)i + 0.25f, (float)j + 0.75f) * 0.25f + GetColor((float)i + 0.75f, (float)j + 0.25f) * 0.25f + GetColor((float)i + 0.75f, (float)j + 0.75f) * 0.25f;
                color.a = 1f;
                texture2D.SetPixel(i, j, color);
            }
        }
        texture2D.Apply();
        RestorePreCaptureState(objectState);
        byte[] bytes = texture2D.EncodeToPNG();
        ReadWrite.writeBytes(info.path + "/Chart.png", useCloud: false, usePath: false, bytes);
        UnityEngine.Object.DestroyImmediate(texture2D);
        Color GetColor(float x, float y)
        {
            float num = x / (float)imageWidth;
            float num2 = y / (float)imageHeight;
            Vector3 position2 = new Vector3((num - 0.5f) * captureWidth, (num2 - 0.5f) * captureHeight, 0f);
            Vector3 vector2 = satelliteCaptureTransform.TransformPoint(position2);
            FindChartHit(vector2, out var chart, out var hit);
            Transform transform = hit.transform;
            Vector3 point = hit.point;
            if (transform == null)
            {
                transform = terrainGO.transform;
                point = vector2;
                point.y = LevelGround.getHeight(vector2);
            }
            int num3 = transform.gameObject.layer;
            switch (chart)
            {
            case EObjectChart.GROUND:
                num3 = 20;
                break;
            case EObjectChart.HIGHWAY:
                num3 = 0;
                break;
            case EObjectChart.ROAD:
                num3 = 1;
                break;
            case EObjectChart.STREET:
                num3 = 2;
                break;
            case EObjectChart.PATH:
                num3 = 3;
                break;
            case EObjectChart.LARGE:
                num3 = 15;
                break;
            case EObjectChart.MEDIUM:
                num3 = 16;
                break;
            case EObjectChart.CLIFF:
                num3 = 4;
                break;
            }
            if (num3 == 19)
            {
                RoadMaterial roadMaterial = LevelRoads.getRoadMaterial(transform);
                if (roadMaterial != null)
                {
                    num3 = ((!roadMaterial.isConcrete) ? 3 : ((!(roadMaterial.width > 8f)) ? 1 : 0));
                }
            }
            if (chart == EObjectChart.WATER)
            {
                return heightStrip.GetPixel(0, 0);
            }
            if (num3 == 20)
            {
                if (WaterUtility.isPointUnderwater(point))
                {
                    return heightStrip.GetPixel(0, 0);
                }
                float num4 = Mathf.InverseLerp(terrainMinHeight, terrainMaxHeight, point.y);
                return heightStrip.GetPixel((int)(num4 * (float)(heightStrip.width - 1)) + 1, 0);
            }
            return layerStrip.GetPixel(num3, 0);
        }
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return null;
        UnturnedLog.info("Returning to main menu");
        SceneManager.LoadScene("Menu");
        if (placeholderAudioListener != null)
        {
            UnityEngine.Object.Destroy(placeholderAudioListener);
            placeholderAudioListener = null;
        }
        Provider.updateRichPresence();
        LevelBatching.Get()?.Destroy();
        DevkitTransactionManager.resetTransactions();
        updateCachedHolidayRedirects();
        UpdateShouldUseLevelBatching();
        isExiting = false;
    }

    public IEnumerator init(int id)
    {
        if (!isVR)
        {
            LevelNavigation.load();
        }
        if (shouldUseLevelBatching)
        {
            LevelBatching.Get()?.Reset();
        }
        LoadingUI.NotifyLevelLoadingProgress(1f / 19f);
        yield return null;
        LevelObjects.load();
        LoadingUI.NotifyLevelLoadingProgress(0.10526316f);
        yield return null;
        LevelLighting.load(size);
        LoadingUI.NotifyLevelLoadingProgress(0.15789473f);
        yield return null;
        LevelGround.load(size);
        LoadingUI.NotifyLevelLoadingProgress(0.21052632f);
        yield return null;
        LevelRoads.load();
        LoadingUI.NotifyLevelLoadingProgress(0.2631579f);
        yield return null;
        if (!isVR)
        {
            LevelNodes.load();
            LoadingUI.NotifyLevelLoadingProgress(0.31578946f);
            yield return null;
            LevelItems.load();
            LoadingUI.NotifyLevelLoadingProgress(0.36842105f);
            yield return null;
        }
        LevelPlayers.load();
        LoadingUI.NotifyLevelLoadingProgress(0.42105263f);
        yield return null;
        if (!isVR)
        {
            LevelZombies.load();
            LoadingUI.NotifyLevelLoadingProgress(0.47368422f);
            yield return null;
            LevelVehicles.load();
            LoadingUI.NotifyLevelLoadingProgress(0.5263158f);
            yield return null;
            LevelAnimals.load();
            LoadingUI.NotifyLevelLoadingProgress(0.57894737f);
            yield return null;
        }
        LevelVisibility.load();
        LoadingUI.NotifyLevelLoadingProgress(0.6315789f);
        yield return null;
        pendingHashes = new List<byte[]>();
        Level.loadingSteps?.Invoke();
        LoadingUI.NotifyLevelLoadingProgress(0.68421054f);
        yield return null;
        if (LevelGround.hasLegacyDataForConversion)
        {
            if (Landscape.instance == null)
            {
                LevelHierarchy.AssignInstanceIdAndMarkDirty(new GameObject().AddComponent<Landscape>());
            }
            yield return Landscape.instance.AutoConvertLegacyTerrain();
        }
        VolumeManager<LandscapeHoleVolume, LandscapeHoleVolumeManager>.Get().ApplyToTerrain();
        if (LevelNodes.hasLegacyVolumesForConversion)
        {
            LevelNodes.AutoConvertLegacyVolumes();
        }
        if (LevelNodes.hasLegacyNodesForConversion)
        {
            LevelNodes.AutoConvertLegacyNodes();
        }
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().RefreshOverlappingObjects();
        LoadingUI.NotifyLevelLoadingProgress(0.7368421f);
        yield return null;
        if (shouldUseLevelBatching)
        {
            LevelBatching.Get()?.ApplyTextureAtlas();
            LoadingUI.NotifyLevelLoadingProgress(0.7894737f);
            yield return null;
            LevelBatching.Get()?.ApplyStaticBatching();
            LoadingUI.NotifyLevelLoadingProgress(0.84210527f);
            yield return null;
        }
        includeHash("Level.dat", info.hash);
        if (info.configData.Hash != null)
        {
            includeHash("Config.json", info.configData.Hash);
        }
        includeHash("Lighting.dat", LevelLighting.hash);
        includeHash("Nodes.dat", LevelNodes.hash);
        includeHash("Objects.dat", LevelObjects.hash);
        includeHash("Resources.dat", LevelGround.treesHash);
        combineHashes();
        Physics.gravity = new Vector3(0f, info.configData.Gravity, 0f);
        LoadingUI.NotifyLevelLoadingProgress(0.8947368f);
        yield return null;
        Resources.UnloadUnusedAssets();
        GC.Collect();
        LoadingUI.NotifyLevelLoadingProgress(0.94736844f);
        yield return null;
        _editing = new GameObject().transform;
        editing.name = "Editing";
        editing.parent = level;
        if (isEditor)
        {
            UnityEngine.Object.Destroy(placeholderAudioListener);
            placeholderAudioListener = null;
            satelliteCaptureGameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Edit/Mapper"));
            satelliteCaptureGameObject.name = "Mapper";
            satelliteCaptureTransform = satelliteCaptureGameObject.transform;
            satelliteCaptureTransform.parent = editing;
            satelliteCaptureCamera = satelliteCaptureGameObject.GetComponent<Camera>();
            Transform obj = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(isVR ? "Edit/VR" : "Edit/Editor"))).transform;
            obj.name = "Editor";
            obj.parent = editing;
            obj.tag = "Logic";
            obj.gameObject.layer = 8;
        }
        yield return null;
        onPrePreLevelLoaded?.Invoke(id);
        yield return null;
        onPreLevelLoaded?.Invoke(id);
        yield return null;
        onLevelLoaded?.Invoke(id);
        yield return null;
        onPostLevelLoaded?.Invoke(id);
        yield return null;
        if (!isEditor && info != null)
        {
            string text = null;
            if (string.Equals(info.name, "germany", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Level/Triggers_Germany";
            }
            else if (string.Equals(info.name, "pei", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Level/Triggers_PEI";
            }
            else if (string.Equals(info.name, "russia", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Level/Triggers_Russia";
            }
            else if (string.Equals(info.name, "tutorial", StringComparison.InvariantCultureIgnoreCase))
            {
                text = "Level/Triggers_Tutorial";
            }
            if (string.IsNullOrEmpty(text))
            {
                UnturnedLog.info("Level \"" + info.name + "\" not using hardcoded special events");
            }
            else
            {
                UnturnedLog.info("Loading hardcoded special events \"" + text + "\" for level \"" + info.name + "\"");
                Transform obj2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(text)).transform;
                obj2.position = Vector3.zero;
                obj2.rotation = Quaternion.identity;
                obj2.name = "Triggers";
                obj2.parent = clips;
            }
        }
        LoadingUI.NotifyLevelLoadingProgress(1f);
        yield return null;
        if ((bool)shouldLogSpawnTablesAfterLoadingLevel)
        {
            SpawnTableTool.LogAllSpawnTables();
        }
        _isLoaded = true;
        isLoadingContent = false;
    }

    private void Awake()
    {
        if (isInitialized)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        _isInitialized = true;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        instance = this;
        foliageVolumeManager = new FoliageVolumeManager();
        undergroundWhitelistVolumeManager = new UndergroundWhitelistVolumeManager();
        playerClipVolumeManager = new PlayerClipVolumeManager();
        navClipVolumeManager = new NavClipVolumeManager();
        waterVolumeManager = new WaterVolumeManager();
        landscapeHoleVolumeManager = new LandscapeHoleVolumeManager();
        deadzoneVolumeManager = new DeadzoneVolumeManager();
        killVolumeManager = new KillVolumeManager();
        effectVolumeManager = new EffectVolumeManager();
        ambianceVolumeManager = new AmbianceVolumeManager();
        entranceVolumeManager = new TeleporterEntranceVolumeManager();
        exitVolumeManager = new TeleporterExitVolumeManager();
        safezoneVolumeManager = new SafezoneVolumeManager();
        arenaVolumeManager = new ArenaCompactorVolumeManager();
        hordePurchaseVolumeManager = new HordePurchaseVolumeManager();
        cartographyVolumeManager = new CartographyVolumeManager();
        oxygenVolumeManager = new OxygenVolumeManager();
        cullingVolumeManager = new CullingVolumeManager();
        LevelBatching.instance = new LevelBatching();
        airdropNodeSystem = new AirdropDevkitNodeSystem();
        locationNodeSystem = new LocationDevkitNodeSystem();
        spawnpointSystem = new SpawnpointSystemV2();
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == BUILD_INDEX_GAME || scene.buildIndex == BUILD_INDEX_LOADING)
        {
            if (placeholderAudioListener == null)
            {
                placeholderAudioListener = instance.gameObject.AddComponent<AudioListener>();
            }
        }
        else if (scene.buildIndex == BUILD_INDEX_MENU && placeholderAudioListener != null)
        {
            UnityEngine.Object.Destroy(placeholderAudioListener);
            placeholderAudioListener = null;
        }
        if (scene.buildIndex == BUILD_INDEX_LOADING)
        {
            return;
        }
        if (scene.buildIndex > BUILD_INDEX_SETUP && info != null)
        {
            _level = new GameObject().transform;
            level.name = info.name;
            level.tag = "Logic";
            level.gameObject.layer = 8;
            _roots = new GameObject().transform;
            roots.name = "Roots";
            roots.parent = level;
            _clips = new GameObject().transform;
            clips.name = "Clips";
            clips.parent = level;
            clips.tag = "Clip";
            clips.gameObject.layer = 21;
            if (info.configData.Use_Legacy_Clip_Borders)
            {
                Transform obj = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Level/Cap"))).transform;
                obj.position = new Vector3(0f, -4f, 0f);
                obj.localScale = new Vector3(size - border * 2 + CLIP * 2, size - border * 2 + CLIP * 2, 1f);
                obj.rotation = Quaternion.Euler(-90f, 0f, 0f);
                obj.name = "Cap";
                obj.parent = clips;
                Transform obj2 = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Level/Cap"))).transform;
                obj2.position = new Vector3(0f, HEIGHT + 4f, 0f);
                obj2.localScale = new Vector3(size - border * 2 + CLIP * 2, size - border * 2 + CLIP * 2, 1f);
                obj2.rotation = Quaternion.Euler(90f, 0f, 0f);
                obj2.name = "Cap";
                obj2.parent = clips;
                Transform transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(isEditor ? "Level/Wall" : "Level/Clip"))).transform;
                transform.position = new Vector3(size / 2 - border, HEIGHT / 8f, 0f);
                transform.localScale = new Vector3(size - border * 2, HEIGHT / 4f, 1f);
                transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                transform.name = "Clip";
                transform.parent = clips;
                if (isEditor)
                {
                    transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)(size - border * 2) / 32f, 4f);
                }
                transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(isEditor ? "Level/Wall" : "Level/Clip"))).transform;
                transform.position = new Vector3(-size / 2 + border, HEIGHT / 8f, 0f);
                transform.localScale = new Vector3(size - border * 2, HEIGHT / 4f, 1f);
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.name = "Clip";
                transform.parent = clips;
                if (isEditor)
                {
                    transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)(size - border * 2) / 32f, 4f);
                }
                transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(isEditor ? "Level/Wall" : "Level/Clip"))).transform;
                transform.position = new Vector3(0f, HEIGHT / 8f, size / 2 - border);
                transform.localScale = new Vector3(size - border * 2, HEIGHT / 4f, 1f);
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                transform.name = "Clip";
                transform.parent = clips;
                if (isEditor)
                {
                    transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)(size - border * 2) / 32f, 4f);
                }
                transform = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(isEditor ? "Level/Wall" : "Level/Clip"))).transform;
                transform.position = new Vector3(0f, HEIGHT / 8f, -size / 2 + border);
                transform.localScale = new Vector3(size - border * 2, HEIGHT / 4f, 1f);
                transform.rotation = Quaternion.identity;
                transform.name = "Clip";
                transform.parent = clips;
                if (isEditor)
                {
                    transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)(size - border * 2) / 32f, 4f);
                }
            }
            StartCoroutine(instance.init(scene.buildIndex));
        }
        else
        {
            isLoadingLighting = true;
            isLoadingVehicles = true;
            isLoadingBarricades = true;
            isLoadingStructures = true;
            isLoadingContent = true;
            isLoadingArea = true;
            _isLoaded = false;
            onLevelLoaded?.Invoke(scene.buildIndex);
            LevelLighting.resetForMainMenu();
        }
        _ = scene.buildIndex;
        _ = BUILD_INDEX_MENU;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private static void PlayLevelLoadingScreenMusic()
    {
        musicOutroClip = null;
        LevelAsset asset = getAsset();
        if (asset == null || asset.loadingScreenMusic == null || asset.loadingScreenMusic.Length == 0 || !(GetOrCreateMusicAudioSource() != null))
        {
            return;
        }
        LevelAsset.LoadingScreenMusic loadingScreenMusic = asset.loadingScreenMusic.RandomOrDefault();
        AudioClip audioClip = loadingScreenMusic.loopRef.loadAsset();
        if (audioClip != null)
        {
            musicAudioSource.clip = audioClip;
            musicAudioSource.volume *= loadingScreenMusic.loopVolume;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
            musicOutroClip = loadingScreenMusic.outroRef.loadAsset();
            musicOutroVolume = loadingScreenMusic.outroVolume;
            if (musicOutroClip == null && loadingScreenMusic.outroRef.isValid)
            {
                UnturnedLog.warn($"Unable to find loading screen music outro \"{loadingScreenMusic.outroRef}\" for level \"{info?.getLocalizedName()}\"");
            }
        }
        else
        {
            UnturnedLog.warn($"Unable to find loading screen music loop \"{loadingScreenMusic.loopRef}\" for level \"{info?.getLocalizedName()}\"");
        }
    }

    private static void PlayLoadingOutroMusic()
    {
        AudioSource orCreateMusicAudioSource = GetOrCreateMusicAudioSource();
        if (orCreateMusicAudioSource != null)
        {
            if (musicOutroClip != null)
            {
                orCreateMusicAudioSource.clip = musicOutroClip;
                orCreateMusicAudioSource.volume *= musicOutroVolume;
                orCreateMusicAudioSource.loop = false;
                orCreateMusicAudioSource.Play();
                musicOutroClip = null;
            }
            else
            {
                orCreateMusicAudioSource.Stop();
            }
        }
    }

    private static AudioSource GetOrCreateMusicAudioSource()
    {
        if (Dedicator.IsDedicatedServer)
        {
            return null;
        }
        if (musicAudioSource == null)
        {
            musicAudioSource = instance.gameObject.AddComponent<AudioSource>();
            musicAudioSource.playOnAwake = false;
            musicAudioSource.spatialBlend = 0f;
            musicAudioSource.ignoreListenerPause = true;
            musicAudioSource.ignoreListenerVolume = true;
            musicAudioSource.bypassEffects = true;
            musicAudioSource.bypassListenerEffects = true;
            musicAudioSource.bypassReverbZones = true;
            musicAudioSource.spatialize = false;
        }
        musicAudioSource.volume = OptionsSettings.volume * OptionsSettings.loadingScreenMusicVolume;
        if (musicAudioSource.volume > 0f)
        {
            return musicAudioSource;
        }
        return null;
    }

    [Obsolete("Replaced by LevelInfo.hash")]
    public static byte[] getLevelHash(string path)
    {
        LevelInfo levelInfo = ReadLevelInfo(path, 0uL);
        if (levelInfo != null)
        {
            return levelInfo.hash;
        }
        return new byte[20];
    }

    [Obsolete("Was unused in vanilla, so simplified to just use the find level by name method.")]
    public static bool exists(string name)
    {
        return getLevel(name) != null;
    }
}
