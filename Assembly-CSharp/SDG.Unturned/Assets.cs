using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SDG.Framework.Devkit;
using SDG.Framework.Modules;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned;

public class Assets : MonoBehaviour
{
    internal class AssetMapping
    {
        public Dictionary<EAssetType, Dictionary<ushort, Asset>> legacyAssetsTable;

        public Dictionary<Guid, Asset> assetDictionary;

        public List<Asset> assetList;

        public AssetMapping()
        {
            legacyAssetsTable = new Dictionary<EAssetType, Dictionary<ushort, Asset>>();
            legacyAssetsTable.Add(EAssetType.ITEM, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.EFFECT, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.OBJECT, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.RESOURCE, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.VEHICLE, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.ANIMAL, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.MYTHIC, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.SKIN, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.SPAWN, new Dictionary<ushort, Asset>());
            legacyAssetsTable.Add(EAssetType.NPC, new Dictionary<ushort, Asset>());
            assetDictionary = new Dictionary<Guid, Asset>();
            assetList = new List<Asset>();
        }
    }

    private struct ScannedFileInfo
    {
        public string name;

        public string assetPath;

        public string dataPath;

        public string bundlePath;

        public AssetOrigin origin;

        public bool overrideExistingIDs;

        public MasterBundleConfig masterBundleCfg;

        public string masterBundleRelativePath;

        public ScannedFileInfo(string name, string assetPath, string dataPath, string bundlePath, AssetOrigin origin, bool overrideExistingIDs, MasterBundleConfig masterBundleCfg, string masterBundleRelativePath)
        {
            this.name = name;
            this.assetPath = assetPath;
            this.dataPath = dataPath;
            this.bundlePath = bundlePath;
            this.origin = origin;
            this.overrideExistingIDs = overrideExistingIDs;
            this.masterBundleCfg = masterBundleCfg;
            this.masterBundleRelativePath = masterBundleRelativePath;
        }
    }

    private static readonly float STEPS = 16f;

    private static readonly int ASSETS_PER_STEP = 25;

    private static TypeRegistryDictionary _assetTypes = new TypeRegistryDictionary(typeof(Asset));

    private static TypeRegistryDictionary _useableTypes = new TypeRegistryDictionary(typeof(Useable));

    private static Assets instance;

    private static bool hasLoaded;

    private static bool _isLoading;

    public static AssetsRefreshed onAssetsRefreshed;

    internal static AssetMapping defaultAssetMapping;

    private static AssetMapping currentAssetMapping;

    public static CommandLineFlag shouldLoadAnyAssets = new CommandLineFlag(defaultValue: true, "-SkipAssets");

    public static CommandLineFlag wantsDeferLoadingAssets = new CommandLineFlag(defaultValue: true, "-NoDeferAssets");

    public static CommandLineFlag shouldValidateAssets = new CommandLineFlag(defaultValue: false, "-ValidateAssets");

    public static CommandLineFlag shouldLogWorkshopAssets = new CommandLineFlag(defaultValue: false, "-LogWorkshopAssets");

    public static CommandLineFlag shouldLoadMasterBundlesAsync = new CommandLineFlag(defaultValue: true, "-NoAsyncMB");

    private static CommandLineFlag shouldCollectGarbageAggressively = new CommandLineFlag(defaultValue: false, "-AggressiveGC");

    private static CommandLineFlag shouldLogSpawnInsertions = new CommandLineFlag(defaultValue: false, "-LogSpawnInsertions");

    private static List<MasterBundleConfig> allMasterBundles;

    private static List<MasterBundleConfig> newMasterBundles;

    internal static MasterBundleConfig coreMasterBundle;

    internal static List<AssetOrigin> assetOrigins;

    internal static AssetOrigin coreOrigin;

    private static AssetOrigin reloadOrigin;

    private static AssetOrigin legacyServerSharedOrigin;

    private static AssetOrigin legacyPerServerOrigin;

    private static Queue<ScannedFileInfo> filesScanned;

    private static List<string> errors;

    internal static AssetOrigin legacyOfficialOrigin;

    internal static AssetOrigin legacyMiscOrigin;

    internal static AssetOrigin legacyWorkshopOrigin;

    private static bool hasUnlinkedSpawns;

    internal static readonly ClientStaticMethod<Guid> SendKickForInvalidGuid = ClientStaticMethod<Guid>.Get(ReceiveKickForInvalidGuid);

    internal static readonly ClientStaticMethod<Guid, string, string, byte[], string, string> SendKickForHashMismatch = ClientStaticMethod<Guid, string, string, byte[], string, string>.Get(ReceiveKickForHashMismatch);

    private static DatParser datParser = new DatParser();

    public static int assetsToLoadPerStep => ASSETS_PER_STEP;

    public static TypeRegistryDictionary assetTypes => _assetTypes;

    public static TypeRegistryDictionary useableTypes => _useableTypes;

    public static bool hasLoadedUgc { get; protected set; }

    public static bool hasLoadedMaps { get; protected set; }

    public static bool isLoading => _isLoading;

    public static bool shouldDeferLoadingAssets
    {
        get
        {
            if ((bool)wantsDeferLoadingAssets)
            {
                return !shouldValidateAssets;
            }
            return false;
        }
    }

    public static MasterBundleConfig currentMasterBundle { get; private set; }

    private static string getExceptionMessage(Exception e)
    {
        if (e != null)
        {
            if (e.InnerException != null)
            {
                return e.InnerException.Message;
            }
            return e.Message;
        }
        return "Exception = Null";
    }

    public static void reportError(string error)
    {
        errors.Add(error);
        UnturnedLog.warn(error);
    }

    public static void reportError(Asset offendingAsset, string error)
    {
        error = offendingAsset.getTypeNameAndIdDisplayString() + ": " + error;
        reportError(error);
    }

    public static void reportError(Asset offendingAsset, string format, params object[] args)
    {
        string error = string.Format(format, args);
        reportError(offendingAsset, error);
    }

    public static void reportError(Asset offendingAsset, string format, object arg0)
    {
        string error = string.Format(format, arg0);
        reportError(offendingAsset, error);
    }

    public static void reportError(Asset offendingAsset, string format, object arg0, object arg1)
    {
        string error = string.Format(format, arg0, arg1);
        reportError(offendingAsset, error);
    }

    public static void reportError(Asset offendingAsset, string format, object arg0, object arg1, object arg2)
    {
        string error = string.Format(format, arg0, arg1, arg2);
        reportError(offendingAsset, error);
    }

    public static List<string> getReportedErrorsList()
    {
        return errors;
    }

    [Obsolete]
    public static void rename(Asset asset, string newName)
    {
    }

    internal static AssetOrigin FindWorkshopFileOrigin(ulong workshopFileId)
    {
        foreach (AssetOrigin assetOrigin in assetOrigins)
        {
            if (assetOrigin.workshopFileId == workshopFileId)
            {
                return assetOrigin;
            }
        }
        return null;
    }

    private static AssetOrigin FindLevelOrigin(LevelInfo level)
    {
        if (level.publishedFileId != 0L)
        {
            return FindWorkshopFileOrigin(level.publishedFileId);
        }
        string b = "Map \"" + level.name + "\"";
        foreach (AssetOrigin assetOrigin in assetOrigins)
        {
            if (string.Equals(assetOrigin.name, b))
            {
                return assetOrigin;
            }
        }
        return null;
    }

    internal static AssetOrigin FindOrAddWorkshopFileOrigin(ulong workshopFileId)
    {
        AssetOrigin assetOrigin = FindWorkshopFileOrigin(workshopFileId);
        if (assetOrigin != null)
        {
            return assetOrigin;
        }
        AssetOrigin assetOrigin2 = new AssetOrigin();
        assetOrigin2.name = $"Workshop File ({workshopFileId})";
        assetOrigin2.workshopFileId = workshopFileId;
        assetOrigins.Add(assetOrigin2);
        return assetOrigin2;
    }

    private static AssetOrigin FindOrAddLevelOrigin(LevelInfo level)
    {
        if (level.publishedFileId != 0L)
        {
            return FindOrAddWorkshopFileOrigin(level.publishedFileId);
        }
        string b = "Map \"" + level.name + "\"";
        foreach (AssetOrigin assetOrigin2 in assetOrigins)
        {
            if (string.Equals(assetOrigin2.name, b))
            {
                return assetOrigin2;
            }
        }
        AssetOrigin assetOrigin = new AssetOrigin();
        assetOrigin.name = b;
        assetOrigins.Add(assetOrigin);
        return assetOrigin;
    }

    [Obsolete]
    public static AssetOrigin ConvertLegacyOrigin(EAssetOrigin legacyOrigin)
    {
        switch (legacyOrigin)
        {
        case EAssetOrigin.OFFICIAL:
            if (legacyOfficialOrigin == null)
            {
                legacyOfficialOrigin = new AssetOrigin();
                legacyOfficialOrigin.name = "Official (Legacy)";
                assetOrigins.Add(legacyOfficialOrigin);
            }
            return legacyOfficialOrigin;
        case EAssetOrigin.MISC:
            if (legacyMiscOrigin == null)
            {
                legacyMiscOrigin = new AssetOrigin();
                legacyMiscOrigin.name = "Misc (Legacy)";
                assetOrigins.Add(legacyMiscOrigin);
            }
            return legacyMiscOrigin;
        default:
            if (legacyWorkshopOrigin == null)
            {
                legacyWorkshopOrigin = new AssetOrigin();
                legacyWorkshopOrigin.name = "Workshop File (Legacy)";
                assetOrigins.Add(legacyWorkshopOrigin);
            }
            return legacyWorkshopOrigin;
        }
    }

    public static Asset find(EAssetType type, ushort id)
    {
        if (type == EAssetType.NONE || id == 0)
        {
            return null;
        }
        currentAssetMapping.legacyAssetsTable[type].TryGetValue(id, out var value);
        return value;
    }

    [Obsolete]
    public static Asset find(EAssetType type, string name)
    {
        return null;
    }

    public static T find<T>(AssetReference<T> reference) where T : Asset
    {
        if (!reference.isValid)
        {
            return null;
        }
        return find(reference.GUID) as T;
    }

    public static T Find_UseDefaultAssetMapping<T>(AssetReference<T> reference) where T : Asset
    {
        if (reference.isNull)
        {
            return null;
        }
        defaultAssetMapping.assetDictionary.TryGetValue(reference.GUID, out var value);
        return value as T;
    }

    public static T load<T>(ContentReference<T> reference) where T : UnityEngine.Object
    {
        if (!reference.isValid)
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = findMasterBundleByName(reference.name);
        if (masterBundleConfig != null && masterBundleConfig.assetBundle != null)
        {
            string text = masterBundleConfig.formatAssetPath(reference.path);
            T val = masterBundleConfig.assetBundle.LoadAsset<T>(text);
            if ((UnityEngine.Object)val == (UnityEngine.Object)null)
            {
                UnturnedLog.warn("Failed to load content reference '{0}' from master bundle '{1}' as {2}", text, reference.name, typeof(T).Name);
            }
            return val;
        }
        return null;
    }

    public static Asset find(Guid GUID)
    {
        currentAssetMapping.assetDictionary.TryGetValue(GUID, out var value);
        return value;
    }

    public static T find<T>(Guid guid) where T : Asset
    {
        return find(guid) as T;
    }

    public static EffectAsset FindEffectAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.EFFECT, legacyId) as EffectAsset;
        }
        return find<EffectAsset>(guid);
    }

    public static T FindNpcAssetByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : Asset
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.NPC, legacyId) as T;
        }
        return find<T>(guid);
    }

    public static VehicleAsset FindVehicleAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.VEHICLE, legacyId) as VehicleAsset;
        }
        return find<VehicleAsset>(guid);
    }

    internal static T FindItemByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : ItemAsset
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.ITEM, legacyId) as T;
        }
        return find<T>(guid);
    }

    public static Asset[] find(EAssetType type)
    {
        switch (type)
        {
        case EAssetType.NONE:
            return null;
        case EAssetType.OBJECT:
            throw new NotSupportedException();
        default:
        {
            Asset[] array = new Asset[currentAssetMapping.legacyAssetsTable[type].Values.Count];
            int num = 0;
            {
                foreach (KeyValuePair<ushort, Asset> item in currentAssetMapping.legacyAssetsTable[type])
                {
                    array[num] = item.Value;
                    num++;
                }
                return array;
            }
        }
        }
    }

    public static void find<T>(List<T> results) where T : Asset
    {
        FindAssetsInListByType(currentAssetMapping.assetList, results);
    }

    internal static void FindAssetsByType_UseDefaultAssetMapping<T>(List<T> results) where T : Asset
    {
        FindAssetsInListByType(defaultAssetMapping.assetList, results);
    }

    private static void FindAssetsInListByType<T>(List<Asset> assetList, List<T> results) where T : Asset
    {
        foreach (Asset asset in assetList)
        {
            if (asset is T item)
            {
                results.Add(item);
            }
        }
    }

    public static Asset findByAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        path = Path.GetFullPath(path);
        foreach (Asset asset in currentAssetMapping.assetList)
        {
            if (path.Equals(asset.absoluteOriginFilePath))
            {
                return asset;
            }
        }
        return null;
    }

    public static void runtimeCreate(Type type)
    {
        try
        {
            if (Activator.CreateInstance(type) is Asset asset)
            {
                asset.GUID = Guid.NewGuid();
                addToMapping(asset, overrideExistingID: false, defaultAssetMapping);
                if (asset is IDirtyable)
                {
                    (asset as IDirtyable).isDirty = true;
                }
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e);
        }
    }

    [Obsolete]
    public static void add(Asset asset, bool overrideExistingID)
    {
        addToMapping(asset, overrideExistingID, defaultAssetMapping);
    }

    internal static void addToMapping(Asset asset, bool overrideExistingID, AssetMapping assetMapping)
    {
        if (asset == null)
        {
            return;
        }
        EAssetType assetCategory = asset.assetCategory;
        if (assetCategory == EAssetType.SPAWN)
        {
            hasUnlinkedSpawns = true;
        }
        switch (assetCategory)
        {
        case EAssetType.OBJECT:
            if (overrideExistingID)
            {
                assetMapping.legacyAssetsTable[assetCategory].Remove(asset.id);
                assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            }
            else if (!assetMapping.legacyAssetsTable[assetCategory].ContainsKey(asset.id))
            {
                assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            }
            break;
        default:
            if (overrideExistingID)
            {
                assetMapping.legacyAssetsTable[assetCategory].Remove(asset.id);
            }
            else if (assetMapping.legacyAssetsTable[assetCategory].ContainsKey(asset.id))
            {
                assetMapping.legacyAssetsTable[assetCategory].TryGetValue(asset.id, out var value);
                reportError(asset, "short ID is already taken by " + value.getTypeNameAndIdDisplayString() + "!");
                return;
            }
            assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            break;
        case EAssetType.NONE:
            break;
        }
        if (asset.GUID != Guid.Empty)
        {
            if (overrideExistingID)
            {
                if (assetMapping.assetDictionary.TryGetValue(asset.GUID, out var value2))
                {
                    assetMapping.assetDictionary.Remove(value2.GUID);
                    assetMapping.assetList.Remove(value2);
                }
            }
            else if (assetMapping.assetDictionary.ContainsKey(asset.GUID))
            {
                assetMapping.assetDictionary.TryGetValue(asset.GUID, out var value3);
                reportError(asset, "long GUID " + asset.GUID.ToString("N") + " is already taken by " + value3.getTypeNameAndIdDisplayString() + "!");
                return;
            }
            assetMapping.assetDictionary.Add(asset.GUID, asset);
            assetMapping.assetList.Add(asset);
        }
        if (asset.origin != null && asset.origin.workshopFileId != 0L && (bool)shouldLogWorkshopAssets)
        {
            UnturnedLog.info(asset.getTypeNameAndIdDisplayString());
        }
    }

    private static void AddAssetsFromOriginToCurrentMapping(AssetOrigin origin)
    {
        UnturnedLog.info($"Adding {origin.assets.Count} asset(s) from origin \"{origin.name}\" to server mapping");
        foreach (Asset asset in origin.assets)
        {
            addToMapping(asset, overrideExistingID: true, currentAssetMapping);
        }
    }

    internal static void ApplyServerAssetMapping(LevelInfo pendingLevel, List<PublishedFileId_t> serverWorkshopFileIds)
    {
        currentAssetMapping = new AssetMapping();
        AddAssetsFromOriginToCurrentMapping(coreOrigin);
        AssetOrigin assetOrigin = null;
        if (pendingLevel != null)
        {
            assetOrigin = FindLevelOrigin(pendingLevel);
            if (assetOrigin != null)
            {
                AddAssetsFromOriginToCurrentMapping(assetOrigin);
            }
        }
        if (serverWorkshopFileIds == null)
        {
            return;
        }
        foreach (PublishedFileId_t serverWorkshopFileId in serverWorkshopFileIds)
        {
            AssetOrigin assetOrigin2 = FindWorkshopFileOrigin(serverWorkshopFileId.m_PublishedFileId);
            if (assetOrigin2 != null)
            {
                if (assetOrigin2 != assetOrigin)
                {
                    AddAssetsFromOriginToCurrentMapping(assetOrigin2);
                }
            }
            else
            {
                UnturnedLog.error($"Unable to find assets for server mapping (file ID {serverWorkshopFileId})");
            }
        }
    }

    internal static void ClearServerAssetMapping()
    {
        currentAssetMapping = defaultAssetMapping;
    }

    public static void refresh()
    {
        instance.StartCoroutine(instance.init());
    }

    private static MasterBundleConfig findMasterBundle(string path, bool usePath, ulong workshopFileId)
    {
        string text = path;
        if (usePath)
        {
            text = ReadWrite.PATH + path;
        }
        if (MasterBundleHelper.containsMasterBundle(text))
        {
            try
            {
                text = Path.GetFullPath(text);
                MasterBundleConfig masterBundleConfig = new MasterBundleConfig(text, workshopFileId);
                MasterBundleConfig masterBundleConfig2 = findMasterBundleByName(masterBundleConfig.assetBundleName);
                if (masterBundleConfig2 == null)
                {
                    masterBundleConfig2 = findPendingMasterBundleByName(masterBundleConfig.assetBundleName);
                }
                if (masterBundleConfig2 != null)
                {
                    UnturnedLog.info("Found duplicate of master bundle '{0}' originally in '{1}' at '{2}'", masterBundleConfig.assetBundleName, masterBundleConfig2.directoryPath, masterBundleConfig.directoryPath);
                    return masterBundleConfig2;
                }
                newMasterBundles.Add(masterBundleConfig);
                return masterBundleConfig;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e);
            }
        }
        return null;
    }

    public static MasterBundleConfig findMasterBundleByPath(string path)
    {
        int num = 0;
        MasterBundleConfig result = null;
        foreach (MasterBundleConfig allMasterBundle in allMasterBundles)
        {
            if (allMasterBundle.directoryPath.Length >= num && path.StartsWith(allMasterBundle.directoryPath))
            {
                num = allMasterBundle.directoryPath.Length;
                result = allMasterBundle;
            }
        }
        return result;
    }

    public static MasterBundleConfig findMasterBundleInListByName(List<MasterBundleConfig> list, string name, bool matchExtension = true)
    {
        foreach (MasterBundleConfig item in list)
        {
            if ((matchExtension ? item.assetBundleName : item.assetBundleNameWithoutExtension).Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return item;
            }
        }
        return null;
    }

    public static MasterBundleConfig findMasterBundleByName(string name, bool matchExtension = true)
    {
        return findMasterBundleInListByName(allMasterBundles, name, matchExtension);
    }

    private static MasterBundleConfig findPendingMasterBundleByName(string name, bool matchExtension = true)
    {
        return findMasterBundleInListByName(newMasterBundles, name, matchExtension);
    }

    private static void loadNewMasterBundles()
    {
        foreach (MasterBundleConfig newMasterBundle in newMasterBundles)
        {
            if (newMasterBundle.load())
            {
                allMasterBundles.Add(newMasterBundle);
                UnturnedLog.info("Loaded master bundle: " + newMasterBundle.getAssetBundlePath());
            }
            else
            {
                UnturnedLog.warn("Failed to load pending master bundle: " + newMasterBundle.getAssetBundlePath());
            }
        }
        newMasterBundles.Clear();
    }

    private static IEnumerator loadNewMasterBundlesAsync()
    {
        if ((bool)shouldLoadMasterBundlesAsync)
        {
            List<MasterBundleConfig> list = new List<MasterBundleConfig>(newMasterBundles);
            newMasterBundles.Clear();
            foreach (MasterBundleConfig config in list)
            {
                yield return config.loadAsync();
                if (config.assetBundle != null)
                {
                    allMasterBundles.Add(config);
                    UnturnedLog.info("Loaded master bundle async: " + config.getAssetBundlePath());
                }
                else
                {
                    UnturnedLog.warn("Failed to load pending master bundle async: " + config.getAssetBundlePath());
                }
            }
        }
        else
        {
            loadNewMasterBundles();
            yield return null;
        }
    }

    private static void unloadAllMasterBundles()
    {
        foreach (MasterBundleConfig allMasterBundle in allMasterBundles)
        {
            allMasterBundle.unload();
        }
        allMasterBundles.Clear();
    }

    private static void scanFolder(string path, AssetOrigin origin, bool overrideExistingIDs, MasterBundleConfig parentMasterBundle = null, string relativePath = "")
    {
        string fileName = Path.GetFileName(path);
        MasterBundleConfig masterBundleConfig = findMasterBundle(path, usePath: false, origin.workshopFileId);
        if (masterBundleConfig != null)
        {
            parentMasterBundle = masterBundleConfig;
            relativePath = string.Empty;
        }
        if (ReadWrite.fileExists(path + "/" + fileName + ".asset", useCloud: false, usePath: false))
        {
            filesScanned.Enqueue(new ScannedFileInfo(fileName, path + "/" + fileName + ".asset", path, path, origin, overrideExistingIDs, parentMasterBundle, relativePath));
        }
        else if (ReadWrite.fileExists(path + "/" + fileName + ".dat", useCloud: false, usePath: false))
        {
            filesScanned.Enqueue(new ScannedFileInfo(fileName, null, path, path, origin, overrideExistingIDs, parentMasterBundle, relativePath));
        }
        else if (ReadWrite.fileExists(path + "/Asset.dat", useCloud: false, usePath: false))
        {
            filesScanned.Enqueue(new ScannedFileInfo(fileName, null, path, path, origin, overrideExistingIDs, parentMasterBundle, relativePath));
        }
        else
        {
            string[] files = Directory.GetFiles(path, "*.asset");
            for (int i = 0; i < files.Length; i++)
            {
                filesScanned.Enqueue(new ScannedFileInfo(Path.GetFileNameWithoutExtension(files[i]), files[i], path, path, origin, overrideExistingIDs, parentMasterBundle, relativePath));
            }
        }
        string[] folders = ReadWrite.getFolders(path, usePath: false);
        for (int j = 0; j < folders.Length; j++)
        {
            string fileName2 = Path.GetFileName(folders[j]);
            string text = "/" + fileName2;
            scanFolder(path + text, origin, overrideExistingIDs, parentMasterBundle, relativePath + text);
        }
    }

    private static void tryLoadFile(ScannedFileInfo file)
    {
        try
        {
            loadFile(file);
        }
        catch (Exception e)
        {
            UnturnedLog.error("Exception loading file {0}:", file.name);
            UnturnedLog.exception(e);
        }
    }

    private static void loadFile(ScannedFileInfo file)
    {
        string assetPath;
        if (!string.IsNullOrEmpty(file.assetPath))
        {
            assetPath = file.assetPath;
            assetPath = Path.GetFullPath(assetPath);
        }
        else
        {
            assetPath = file.dataPath + "/" + file.name + ".dat";
            assetPath = Path.GetFullPath(assetPath);
            if (!ReadWrite.fileExists(assetPath, useCloud: false, usePath: false))
            {
                assetPath = file.dataPath + "/Asset.dat";
                assetPath = Path.GetFullPath(assetPath);
            }
        }
        DatDictionary datDictionary = null;
        byte[] hash;
        try
        {
            using (FileStream underlyingStream = new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
                using StreamReader inputReader = new StreamReader(sHA1Stream);
                datDictionary = datParser.Parse(inputReader);
                hash = sHA1Stream.Hash;
            }
            if (datParser.HasError)
            {
                reportError("Error parsing \"" + assetPath + "\": \"" + datParser.ErrorMessage + "\"");
            }
            if (datDictionary == null)
            {
                reportError("Unable to read \"" + assetPath + "\"");
                return;
            }
        }
        catch (Exception e)
        {
            reportError("Caught exception while reading \"" + assetPath + "\": " + getExceptionMessage(e));
            UnturnedLog.exception(e);
            return;
        }
        Guid value = default(Guid);
        Type type = null;
        if (datDictionary.TryGetDictionary("Metadata", out var node))
        {
            if (!node.TryParseGuid("GUID", out value))
            {
                reportError("Unable to parse Metadata.GUID in \"" + assetPath + "\"");
                return;
            }
            type = node.ParseType("Type");
            if (type == null)
            {
                reportError("Unable to parse Metadata.Type in \"" + assetPath + "\"");
                return;
            }
        }
        else if (!datDictionary.TryParseGuid("GUID", out value))
        {
            value = Guid.NewGuid();
            try
            {
                string text = File.ReadAllText(assetPath);
                text = "GUID " + value.ToString("N") + "\n" + text;
                File.WriteAllText(assetPath, text);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, "Caught IO exception adding GUID to \"" + assetPath + "\":");
            }
        }
        if (value.IsEmpty())
        {
            reportError("Cannot use empty GUID in \"" + assetPath + "\"");
            return;
        }
        DatDictionary datDictionary2 = datDictionary;
        if (datDictionary.TryGetDictionary("Asset", out var node2))
        {
            datDictionary2 = node2;
        }
        if (type == null)
        {
            string @string = datDictionary2.GetString("Type");
            if (string.IsNullOrEmpty(@string))
            {
                reportError("Missing asset Type in \"" + assetPath + "\"");
                return;
            }
            type = assetTypes.getType(@string);
            if (type == null)
            {
                reportError("Unhandled asset type \"" + @string + "\" in \"" + assetPath + "\"");
                return;
            }
        }
        if (!typeof(Asset).IsAssignableFrom(type))
        {
            reportError($"Type \"{type}\" is not a valid asset type in \"{assetPath}\"");
            return;
        }
        MasterBundleConfig masterBundleConfig = file.masterBundleCfg;
        string string2 = datDictionary2.GetString("Master_Bundle_Override");
        if (string2 != null)
        {
            masterBundleConfig = findMasterBundleByName(string2);
            if (masterBundleConfig == null)
            {
                UnturnedLog.warn("Unable to find master bundle override '{0}' for '{1}'", string2, file.name);
            }
        }
        else if (datDictionary2.ContainsKey("Exclude_From_Master_Bundle"))
        {
            masterBundleConfig = null;
        }
        if (masterBundleConfig != null && masterBundleConfig.assetBundle == null)
        {
            UnturnedLog.warn("Skipping master bundle '{0}' for '{1}' because asset bundle is null", masterBundleConfig.assetBundleName, file.name);
            masterBundleConfig = null;
        }
        currentMasterBundle = masterBundleConfig;
        int a = -1;
        Bundle bundle;
        if (masterBundleConfig != null)
        {
            string string3 = datDictionary2.GetString("Bundle_Override_Path", file.masterBundleRelativePath);
            bundle = new MasterBundle(masterBundleConfig, string3, file.name);
            a = masterBundleConfig.version;
        }
        else if (datDictionary2.ContainsKey("Bundle_Override_Path"))
        {
            string string4 = datDictionary2.GetString("Bundle_Override_Path");
            int num = string4.LastIndexOf('/');
            string text2 = ((num != -1) ? string4.Substring(num + 1) : string4);
            string4 = string4 + "/" + text2 + ".unity3d";
            bundle = new Bundle(string4, usePath: false, file.name);
        }
        else
        {
            bundle = new Bundle(file.bundlePath + "/" + file.name + ".unity3d", usePath: false);
        }
        bundle.isCoreAsset = file.origin == coreOrigin;
        int num2 = datDictionary2.ParseInt32("Asset_Bundle_Version", 1);
        if (num2 < 1)
        {
            reportError(file.name + " Lowest individual asset bundle version is 1 (default), associated with 5.5.");
            num2 = 1;
        }
        else if (num2 > 4)
        {
            reportError(file.name + " Highest individual asset bundle version is 4, associated with 2020 LTS.");
            num2 = 4;
        }
        int num3 = Mathf.Max(a, num2);
        bundle.convertShadersToStandard = num3 < 2;
        bundle.consolidateShaders = num3 < 3 || (datDictionary2.ContainsKey("Enable_Shader_Consolidation") && !datDictionary2.ContainsKey("Disable_Shader_Consolidation"));
        Local localization = Localization.tryRead(file.dataPath, usePath: false);
        ushort id = datDictionary2.ParseUInt16("ID", 0);
        Asset asset;
        try
        {
            asset = Activator.CreateInstance(type) as Asset;
        }
        catch (Exception e3)
        {
            reportError($"Caught exception while constructing {type} in \"{assetPath}\": {getExceptionMessage(e3)}");
            UnturnedLog.exception(e3);
            bundle.unload();
            currentMasterBundle = null;
            return;
        }
        if (asset == null)
        {
            reportError($"Failed to construct {type} in \"{assetPath}\"");
            bundle.unload();
            currentMasterBundle = null;
            return;
        }
        try
        {
            asset.id = id;
            asset.GUID = value;
            asset.hash = hash;
            asset.requiredShaderUpgrade = bundle.convertShadersToStandard || bundle.consolidateShaders;
            asset.absoluteOriginFilePath = assetPath;
            asset.origin = file.origin;
            asset.origin.assets.Add(asset);
            asset.PopulateAsset(bundle, datDictionary2, localization);
            addToMapping(asset, file.overrideExistingIDs, defaultAssetMapping);
            bundle.unload();
        }
        catch (Exception e4)
        {
            reportError("Caught exception while populating \"" + assetPath + "\": " + getExceptionMessage(e4));
            UnturnedLog.exception(e4);
            bundle.unload();
        }
        currentMasterBundle = null;
    }

    [Obsolete]
    public static void load(string path, bool usePath, bool loadFromResources, bool canUse, EAssetOrigin origin, bool overrideExistingIDs)
    {
        load(path, usePath, loadFromResources, canUse, origin, overrideExistingIDs, 0uL);
    }

    [Obsolete("Remove unused loadFromResources which was used by vanilla assets before masterbundles, and canUse which was for timed curated maps.")]
    public static void load(string path, bool usePath, bool loadFromResources, bool canUse, EAssetOrigin origin, bool overrideExistingIDs, ulong workshopFileId)
    {
        load(path, usePath, origin, overrideExistingIDs, workshopFileId);
    }

    [Obsolete("Replaced origin enum with class")]
    public static void load(string path, bool usePath, EAssetOrigin legacyOrigin, bool overrideExistingIDs, ulong workshopFileId)
    {
        if (usePath)
        {
            path = ReadWrite.PATH + path;
        }
        AssetOrigin origin = ConvertLegacyOrigin(legacyOrigin);
        load(path, origin, overrideExistingIDs);
    }

    public static void load(string absoluteDirectoryPath, AssetOrigin origin, bool overrideExistingIDs)
    {
        scanFolder(absoluteDirectoryPath, origin, overrideExistingIDs);
        loadNewMasterBundles();
        while (filesScanned.Count > 0)
        {
            tryLoadFile(filesScanned.Dequeue());
        }
    }

    public static void reload(string absolutePath)
    {
        UnturnedLog.info("Reloading {0}", absolutePath);
        MasterBundleConfig masterBundleConfig = findMasterBundleByPath(absolutePath);
        string text = string.Empty;
        if (masterBundleConfig != null)
        {
            text = absolutePath.Substring(masterBundleConfig.directoryPath.Length);
            text = text.Replace('\\', '/');
            UnturnedLog.info("Master bundle: {0} Relative path: {1}", masterBundleConfig, text);
        }
        scanFolder(absolutePath, reloadOrigin, overrideExistingIDs: true, masterBundleConfig, text);
        while (filesScanned.Count > 0)
        {
            tryLoadFile(filesScanned.Dequeue());
        }
    }

    public static void linkSpawnsIfDirty()
    {
        if (hasUnlinkedSpawns)
        {
            UnturnedLog.info("Linking spawns because changes were detected");
            linkSpawns();
        }
        else
        {
            UnturnedLog.info("Skipping link spawns because no changes were detected");
        }
    }

    public static void linkSpawns()
    {
        if (!hasUnlinkedSpawns)
        {
            return;
        }
        hasUnlinkedSpawns = false;
        Dictionary<ushort, Asset>.ValueCollection values = defaultAssetMapping.legacyAssetsTable[EAssetType.SPAWN].Values;
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        foreach (SpawnAsset item in values)
        {
            if (item.insertRoots.Count < 1)
            {
                continue;
            }
            foreach (SpawnTable insertRoot in item.insertRoots)
            {
                if (insertRoot.spawnID == 0)
                {
                    continue;
                }
                if (!(find(EAssetType.SPAWN, insertRoot.spawnID) is SpawnAsset spawnAsset2))
                {
                    reportError(item, "unable to find root {0} during link", insertRoot.spawnID);
                    continue;
                }
                insertRoot.spawnID = item.id;
                insertRoot.isLink = true;
                spawnAsset2.tables.Add(insertRoot);
                if (insertRoot.isOverride)
                {
                    spawnAsset2.markOverridden();
                }
                spawnAsset2.markTablesDirty();
                num++;
                if ((bool)shouldLogSpawnInsertions)
                {
                    if (insertRoot.isOverride)
                    {
                        UnturnedLog.info("Spawn {0} overriding {1}", item.name, spawnAsset2.name);
                    }
                    else
                    {
                        UnturnedLog.info("Spawn {0} inserted into {1}", item.name, spawnAsset2.name);
                    }
                }
            }
            item.insertRoots.Clear();
        }
        foreach (SpawnAsset item2 in values)
        {
            if (item2.areTablesDirty)
            {
                item2.sortAndNormalizeWeights();
                num2++;
            }
        }
        foreach (SpawnAsset item3 in values)
        {
            foreach (SpawnTable table in item3.tables)
            {
                if (table.spawnID != 0 && !table.hasNotifiedChild)
                {
                    table.hasNotifiedChild = true;
                    if (!(find(EAssetType.SPAWN, table.spawnID) is SpawnAsset spawnAsset5))
                    {
                        reportError(item3, "unable to find child table {0} during link", table.spawnID);
                        continue;
                    }
                    SpawnTable spawnTable = new SpawnTable();
                    spawnTable.assetID = 0;
                    spawnTable.spawnID = item3.id;
                    spawnTable.weight = table.weight;
                    spawnTable.chance = table.chance;
                    spawnTable.isLink = table.isLink;
                    spawnAsset5.roots.Add(spawnTable);
                    num3++;
                }
            }
        }
        UnturnedLog.info("Link spawns: {0} children, {1} sorted/normalized and {2} parents", num, num2, num3);
    }

    public static void initializeMasterBundleValidation()
    {
        MasterBundleValidation.initialize(allMasterBundles);
    }

    private IEnumerator loadAllFilesScanned(string loadingKey, int stepNum, bool formatKey = true)
    {
        while (filesScanned.Count > 0)
        {
            tryLoadFile(filesScanned.Dequeue());
            if (filesScanned.Count % assetsToLoadPerStep == 0)
            {
                LoadingUI.assetsLoad(loadingKey, filesScanned.Count, (float)stepNum / STEPS, 1f / STEPS, formatKey);
                if ((bool)shouldCollectGarbageAggressively)
                {
                    cleanupMemory();
                }
                yield return null;
            }
        }
    }

    private IEnumerator loadingStep(string relativePath, string loadingKey, int stepNum, string masterBundleRelativePath)
    {
        scanFolder(ReadWrite.PATH + relativePath, coreOrigin, overrideExistingIDs: false, coreMasterBundle, masterBundleRelativePath);
        LoadingUI.assetsScan(loadingKey, filesScanned.Count);
        yield return null;
        yield return loadNewMasterBundlesAsync();
        yield return loadAllFilesScanned(loadingKey, stepNum);
        cleanupMemory();
    }

    private void checkForBlueprintErrors()
    {
        Func<Blueprint, Blueprint, bool> func = delegate(Blueprint myBlueprint, Blueprint yourBlueprint)
        {
            if (myBlueprint.type != yourBlueprint.type)
            {
                return false;
            }
            if (myBlueprint.outputs.Length != yourBlueprint.outputs.Length)
            {
                return false;
            }
            if (myBlueprint.supplies.Length != yourBlueprint.supplies.Length)
            {
                return false;
            }
            if (myBlueprint.questConditions.Length != yourBlueprint.questConditions.Length)
            {
                return false;
            }
            if (myBlueprint.questRewards.Length != yourBlueprint.questRewards.Length)
            {
                return false;
            }
            if (myBlueprint.tool != yourBlueprint.tool)
            {
                return false;
            }
            for (byte b5 = 0; b5 < myBlueprint.outputs.Length; b5 = (byte)(b5 + 1))
            {
                if (myBlueprint.outputs[b5].id != yourBlueprint.outputs[b5].id)
                {
                    return false;
                }
            }
            for (byte b6 = 0; b6 < myBlueprint.supplies.Length; b6 = (byte)(b6 + 1))
            {
                if (myBlueprint.supplies[b6].id != yourBlueprint.supplies[b6].id)
                {
                    return false;
                }
            }
            for (int k = 0; k < myBlueprint.questConditions.Length; k++)
            {
                if (!myBlueprint.questConditions[k].Equals(yourBlueprint.questConditions[k]))
                {
                    return false;
                }
            }
            for (int l = 0; l < myBlueprint.questRewards.Length; l++)
            {
                if (!myBlueprint.questRewards[l].Equals(yourBlueprint.questRewards[l]))
                {
                    return false;
                }
            }
            return true;
        };
        Asset[] array = find(EAssetType.ITEM);
        if (array == null)
        {
            return;
        }
        for (int i = 0; i < array.Length; i++)
        {
            ItemAsset itemAsset = (ItemAsset)array[i];
            for (byte b = 0; b < itemAsset.blueprints.Count; b = (byte)(b + 1))
            {
                Blueprint blueprint = itemAsset.blueprints[b];
                for (byte b2 = 0; b2 < itemAsset.blueprints.Count; b2 = (byte)(b2 + 1))
                {
                    if (b2 != b)
                    {
                        Blueprint arg = itemAsset.blueprints[b2];
                        if (func(blueprint, arg))
                        {
                            reportError(itemAsset, "has an identical blueprint: " + blueprint);
                        }
                    }
                }
            }
            for (int j = 0; j < array.Length; j++)
            {
                if (j == i)
                {
                    continue;
                }
                ItemAsset itemAsset2 = (ItemAsset)array[j];
                for (byte b3 = 0; b3 < itemAsset.blueprints.Count; b3 = (byte)(b3 + 1))
                {
                    Blueprint blueprint2 = itemAsset.blueprints[b3];
                    for (byte b4 = 0; b4 < itemAsset2.blueprints.Count; b4 = (byte)(b4 + 1))
                    {
                        Blueprint arg2 = itemAsset2.blueprints[b4];
                        if (func(blueprint2, arg2))
                        {
                            reportError(itemAsset, "shares an identical blueprint with " + itemAsset2.itemName + ": " + blueprint2);
                        }
                    }
                }
            }
        }
    }

    private void checkForNpcErrors()
    {
        Asset[] array = find(EAssetType.NPC);
        for (int i = 0; i < array.Length; i++)
        {
            if (!(array[i] is DialogueAsset dialogueAsset))
            {
                continue;
            }
            int num = dialogueAsset.responses.Length;
            for (int j = 0; j < num; j++)
            {
                DialogueResponse dialogueResponse = dialogueAsset.responses[j];
                if (!dialogueResponse.IsDialogueRefNull() && dialogueResponse.FindDialogueAsset() == null)
                {
                    reportError(dialogueAsset, "unable to find dialogue asset for response " + j);
                }
                if (!dialogueResponse.IsVendorRefNull() && dialogueResponse.FindVendorAsset() == null)
                {
                    reportError(dialogueAsset, "unable to find vendor asset for response " + j);
                }
            }
        }
    }

    private void cleanupMemory()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private IEnumerator dedicatedServerUgcLoadingStep()
    {
        if (!ReadWrite.folderExists("/Bundles/Workshop/Content", usePath: true))
        {
            ReadWrite.createFolder("/Bundles/Workshop/Content", usePath: true);
        }
        scanFolder(ReadWrite.PATH + "/Bundles/Workshop/Content", legacyServerSharedOrigin, overrideExistingIDs: false);
        LoadingUI.assetsScan("Workshop_Shared", filesScanned.Count);
        yield return null;
        yield return loadNewMasterBundlesAsync();
        yield return loadAllFilesScanned("Workshop_Shared", 11);
        cleanupMemory();
        if (!ReadWrite.folderExists(ServerSavedata.directory + "/" + Provider.serverID + "/Workshop/Content", usePath: true))
        {
            ReadWrite.createFolder(ServerSavedata.directory + "/" + Provider.serverID + "/Workshop/Content", usePath: true);
        }
        scanFolder(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Workshop/Content", legacyPerServerOrigin, overrideExistingIDs: false);
        LoadingUI.assetsScan("Workshop_Server", filesScanned.Count);
        yield return null;
        yield return loadNewMasterBundlesAsync();
        yield return loadAllFilesScanned("Workshop_Server", 12);
        cleanupMemory();
        scanFolder(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Bundles", legacyPerServerOrigin, overrideExistingIDs: false);
        LoadingUI.assetsScan("Bundles_Server", filesScanned.Count);
        yield return null;
        yield return loadNewMasterBundlesAsync();
        yield return loadAllFilesScanned("Bundles_Server", 13);
        cleanupMemory();
    }

    private IEnumerator clientUgcLoadingStep()
    {
        if (Provider.provider.workshopService.ugc != null)
        {
            SteamContent[] array = Provider.provider.workshopService.ugc.ToArray();
            hasLoadedUgc = true;
            SteamContent[] array2 = array;
            foreach (SteamContent steamContent in array2)
            {
                if (LocalWorkshopSettings.get().getEnabled(steamContent.publishedFileID) && (steamContent.type == ESteamUGCType.OBJECT || steamContent.type == ESteamUGCType.ITEM || steamContent.type == ESteamUGCType.VEHICLE))
                {
                    AssetOrigin origin = FindOrAddWorkshopFileOrigin(steamContent.publishedFileID.m_PublishedFileId);
                    scanFolder(steamContent.path, origin, overrideExistingIDs: false);
                    LoadingUI.assetsScan("Workshop_Client", filesScanned.Count);
                    yield return null;
                }
            }
        }
        yield return loadNewMasterBundlesAsync();
        yield return loadAllFilesScanned("Workshop_Client", 14);
        cleanupMemory();
    }

    private IEnumerator sandboxLoadingStep()
    {
        string path = Path.Combine(ReadWrite.PATH, "Sandbox");
        if (Directory.Exists(path))
        {
            string[] folders = ReadWrite.getFolders(path, usePath: false);
            string[] array = folders;
            foreach (string text in array)
            {
                UnturnedLog.info("Sandbox: {0}", text);
                AssetOrigin assetOrigin = new AssetOrigin();
                assetOrigin.name = "Sandbox Folder \"" + text + "\"";
                assetOrigins.Add(assetOrigin);
                load(text, assetOrigin, overrideExistingIDs: true);
                yield return null;
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
        yield return null;
    }

    private IEnumerator mapLoadingStep()
    {
        LevelInfo[] levels = Level.getLevels(ESingleplayerMapCategory.ALL);
        hasLoadedMaps = true;
        foreach (LevelInfo levelInfo in levels)
        {
            if (levelInfo != null)
            {
                string levelDisplayName = levelInfo.getLocalizedName();
                string path = Path.Combine(levelInfo.path, "Bundles");
                if (ReadWrite.folderExists(path, usePath: false))
                {
                    AssetOrigin origin = FindOrAddLevelOrigin(levelInfo);
                    scanFolder(path, origin, overrideExistingIDs: false);
                }
                LoadingUI.assetsScan(levelDisplayName, filesScanned.Count, formatKey: false);
                yield return null;
                yield return loadNewMasterBundlesAsync();
                yield return loadAllFilesScanned(levelDisplayName, 15, formatKey: false);
                cleanupMemory();
            }
        }
    }

    public IEnumerator init()
    {
        _isLoading = true;
        double startTime = Time.realtimeSinceStartupAsDouble;
        if (errors == null)
        {
            errors = new List<string>();
        }
        else
        {
            errors.Clear();
        }
        defaultAssetMapping = new AssetMapping();
        currentAssetMapping = defaultAssetMapping;
        filesScanned = new Queue<ScannedFileInfo>();
        newMasterBundles = new List<MasterBundleConfig>();
        if (allMasterBundles == null)
        {
            allMasterBundles = new List<MasterBundleConfig>();
        }
        else
        {
            unloadAllMasterBundles();
        }
        assetOrigins = new List<AssetOrigin>();
        coreOrigin = new AssetOrigin();
        coreOrigin.name = "Vanilla Built-in Assets";
        assetOrigins.Add(coreOrigin);
        reloadOrigin = new AssetOrigin();
        reloadOrigin.name = "Reloaded Assets (Debug)";
        assetOrigins.Add(reloadOrigin);
        legacyServerSharedOrigin = new AssetOrigin();
        legacyServerSharedOrigin.name = "Server Common (Legacy)";
        assetOrigins.Add(legacyServerSharedOrigin);
        legacyPerServerOrigin = new AssetOrigin();
        legacyPerServerOrigin.name = "Per-Server (Legacy)";
        assetOrigins.Add(legacyPerServerOrigin);
        yield return null;
        if ((bool)shouldLoadAnyAssets)
        {
            coreMasterBundle = findMasterBundle("/Bundles", usePath: true, 0uL);
            yield return loadNewMasterBundlesAsync();
            if (!Dedicator.IsDedicatedServer)
            {
                Provider.initAutoSubscribeMaps();
            }
            yield return loadingStep("/Bundles/Assets", "Asset", 0, "/Assets");
            yield return loadingStep("/Bundles/Items", "Item", 1, "/Items");
            yield return loadingStep("/Bundles/Effects", "Effect", 2, "/Effects");
            yield return loadingStep("/Bundles/Objects", "Object", 3, "/Objects");
            yield return loadingStep("/Bundles/Trees", "Resource", 4, "/Trees");
            yield return loadingStep("/Bundles/Vehicles", "Vehicle", 5, "/Vehicles");
            yield return loadingStep("/Bundles/Animals", "Animal", 6, "/Animals");
            yield return loadingStep("/Bundles/Mythics", "Mythic", 7, "/Mythics");
            yield return loadingStep("/Bundles/Skins", "Skin", 8, "/Skins");
            yield return loadingStep("/Bundles/Spawns", "Spawn", 9, "/Spawns");
            yield return loadingStep("/Bundles/NPCs", "NPC", 10, "/NPCs");
            if (Dedicator.IsDedicatedServer)
            {
                yield return dedicatedServerUgcLoadingStep();
            }
            else
            {
                yield return clientUgcLoadingStep();
            }
            yield return sandboxLoadingStep();
            yield return mapLoadingStep();
        }
        LoadingUI.updateKey("Loading_Clean");
        yield return null;
        cleanupMemory();
        LoadingUI.updateKey("Loading_Blueprints");
        yield return null;
        if ((bool)shouldValidateAssets)
        {
            checkForBlueprintErrors();
        }
        LoadingUI.updateKey("Loading_Spawns");
        yield return null;
        if (!Dedicator.IsDedicatedServer)
        {
            linkSpawns();
        }
        if ((bool)shouldValidateAssets)
        {
            checkForNpcErrors();
        }
        LoadingUI.updateKey("Loading_Misc");
        yield return null;
        onAssetsRefreshed?.Invoke();
        yield return null;
        UnturnedLog.info($"Loading assets took {Time.realtimeSinceStartupAsDouble - startTime}s");
        _isLoading = false;
        if (!hasLoaded)
        {
            hasLoaded = true;
            if (Dedicator.IsDedicatedServer)
            {
                Provider.host();
                yield break;
            }
            UnturnedLog.info("Launching main menu");
            SceneManager.LoadScene("Menu");
        }
    }

    private bool TestDedicatedServerSteamRedist()
    {
        string text = PathEx.Join(UnityPaths.GameDirectory, "linux64", "steamclient.so");
        if (!File.Exists(text))
        {
            CommandWindow.LogError("Missing steamclient redist file at: " + text);
            return false;
        }
        try
        {
            FileInfo fileInfo = new FileInfo(text);
            DateTime dateTime = new DateTime(2021, 9, 14, 21, 30, 0, DateTimeKind.Utc);
            if (fileInfo.LastWriteTimeUtc >= dateTime)
            {
                return true;
            }
            CommandWindow.LogError($"Out-of-date steamclient redist file (expected: {dateTime} actual: {fileInfo.LastWriteTimeUtc})");
            return false;
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Unable to get steamclient redist file info");
            return false;
        }
    }

    private void Start()
    {
        if (Dedicator.IsDedicatedServer)
        {
            Module moduleByName = ModuleHook.getModuleByName("Rocket.Unturned");
            if (moduleByName != null)
            {
                uint uInt32FromIP = Parser.getUInt32FromIP("4.9.3.1");
                if (moduleByName.config.Version_Internal < uInt32FromIP)
                {
                    CommandWindow.LogError("Upgrading to the officially maintained version of Rocket, or a custom fork of it, is required.");
                    CommandWindow.LogErrorFormat("Installed version: {0} Maintained version: 4.9.3.3+", moduleByName.config.Version);
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("--- Overview ---");
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("SDG maintains a fork of Rocket called the Legally Distinct Missile (or LDM) after the resignation of its original community team. Using this fork is important because it preserves compatibility, and has fixes for important legacy Rocket issues like multithreading exceptions and teleportation exploits.");
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("--- Installation ---");
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("The dedicated server includes the latest version, so an external download is not necessary:");
                    CommandWindow.Log("1. Copy the Rocket.Unturned module from the game's Extras directory.");
                    CommandWindow.Log("2. Paste it into the game's Modules directory.");
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("--- Resources ---");
                    CommandWindow.Log(string.Empty);
                    CommandWindow.Log("https://github.com/SmartlyDressedGames/Legally-Distinct-Missile");
                    CommandWindow.Log("https://www.reddit.com/r/UnturnedLDM/");
                    CommandWindow.Log("https://forum.smartlydressedgames.com/c/modding/ldm");
                    CommandWindow.Log("https://steamcommunity.com/app/304930/discussions/17/");
                    return;
                }
            }
            CommandWindow.LogError("Hosting dedicated servers using client files has been deprecated since June 2019.");
            CommandWindow.Log("Please use the standalone dedicated server app ID 1110390 available through SteamCMD instead.");
            CommandWindow.Log("For more information and an installation guide read more at:");
            CommandWindow.Log("https://docs.smartlydressedgames.com/en/stable/servers/server-hosting.html");
        }
        else
        {
            refresh();
        }
    }

    private void Awake()
    {
        instance = this;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveKickForInvalidGuid(Guid guid)
    {
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CUSTOM;
        Asset asset = find(guid);
        if (asset != null)
        {
            Provider._connectionFailureReason = $"Server missing asset: \"{asset.FriendlyName}\" File: \"{asset.name}\" Id: {guid:N}";
        }
        else
        {
            Provider._connectionFailureReason = string.Concat(string.Concat(string.Concat("Client and server are both missing unknown asset! ID: " + guid.ToString("N"), "\nThis probably means either an invalid ID was sent by the server,"), "\nthe ID got corrupted for example by plugins modifying network traffic,"), "\nor a required level asset like materials/foliage/trees/objects is missing.");
        }
        Provider.RequestDisconnect($"Kicked for sending invalid asset guid: {guid:N}");
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveKickForHashMismatch(Guid guid, string serverName, string serverFriendlyName, byte[] serverHash, string serverAssetBundleNameWithoutExtension, string serverAssetOrigin)
    {
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CUSTOM;
        Asset asset = find(guid);
        if (asset != null)
        {
            string text = asset.origin?.name;
            if (string.IsNullOrEmpty(text))
            {
                text = "Unknown";
            }
            string text2;
            if (string.Equals(asset.name, serverName) && string.Equals(asset.FriendlyName, serverFriendlyName))
            {
                if (!string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle != null && !string.Equals(asset.originMasterBundle.assetBundleNameWithoutExtension, serverAssetBundleNameWithoutExtension))
                {
                    text2 = $"Client and server loaded \"{serverFriendlyName}\" from different asset bundles! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nClient asset bundle is \"" + asset.originMasterBundle.assetBundleNameWithoutExtension + "\", whereas server asset bundle is \"" + serverAssetBundleNameWithoutExtension + "\".";
                }
                else if (!string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle == null)
                {
                    text2 = $"Client loaded \"{serverFriendlyName}\" from legacy asset bundle but server did not! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nServer asset bundle name: \"" + serverAssetBundleNameWithoutExtension + "\".";
                }
                else if (string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle != null)
                {
                    text2 = $"Server loaded \"{serverFriendlyName}\" from legacy asset bundle but client did not! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nClient asset bundle name: \"" + asset.originMasterBundle.assetBundleNameWithoutExtension + "\"";
                }
                else if (Hash.verifyHash(asset.hash, serverHash))
                {
                    text2 = $"Server asset bundle hash out of date for \"{serverFriendlyName}\"! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nThis probably means the mod creator should re-export the \"" + serverAssetBundleNameWithoutExtension + "\" asset bundle.";
                }
                else
                {
                    text2 = $"Client and server disagree on asset \"{asset.FriendlyName}\" configuration. (File: \"{asset.name}\" ID: {guid:N})";
                    text2 += "\nUsually this means the files are different versions in which case updating the client and server might fix it.";
                    text2 += "\nAlternatively the file may have been corrupted, locally modified, or modified on the server.";
                    text2 = text2 + "\nClient hash is " + Hash.toString(asset.hash) + ", whereas server hash is " + Hash.toString(serverHash) + ".";
                }
            }
            else
            {
                text2 = $"Client and server have different assets with the same ID! ({guid:N})";
                text2 += "\nThis probably means an existing file was copied, but the mod creator can fix it by changing the ID.";
                text2 = ((!string.Equals(asset.FriendlyName, serverFriendlyName)) ? (text2 + "\nClient display name is \"" + asset.FriendlyName + "\", whereas server display name is \"" + serverFriendlyName + "\".") : (text2 + "\nDisplay name \"" + serverFriendlyName + "\" matches between client and server."));
                text2 = ((!string.Equals(asset.name, serverName)) ? (text2 + "\nClient file name is \"" + asset.name + "\", whereas server file name is \"" + serverName + "\".") : (text2 + "\nFile name \"" + asset.name + "\" matches between client and server."));
            }
            text2 = (Provider._connectionFailureReason = ((!string.Equals(text, serverAssetOrigin)) ? (text2 + "\nClient asset is from " + text + ", whereas server asset is from " + serverAssetOrigin + ".") : (text2 + "\nClient and server agree this asset is from " + text + ".")));
        }
        else
        {
            Provider._connectionFailureReason = $"Unknown asset hash mismatch? (should never happen) Name: \"{serverFriendlyName}\" File: \"{serverName}\" Id: {guid:N}";
        }
        Provider.RequestDisconnect($"Kicked for asset hash mismatch guid: {guid:N} serverName: \"{serverName}\" serverFriendlyName: \"{serverFriendlyName}\" serverHash: {Hash.toString(serverHash)} serverAssetBundleName: \"{serverAssetBundleNameWithoutExtension}\" serverAssetOrigin: \"{serverAssetOrigin}\"");
    }
}
