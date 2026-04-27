using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// <summary>
        /// Calling this "legacy" is a bit of a stretch because even most of the vanilla assets are
        /// built around the 16-bit IDs. Ideally no new code should be relying on 16-bit IDs however.
        /// </summary>
        public Dictionary<EAssetType, Dictionary<ushort, Asset>> legacyAssetsTable;

        public Dictionary<Guid, Asset> assetDictionary;

        public List<Asset> assetList;

        /// <summary>
        /// Incremented when assets are added or removed.
        /// Used by boombox UI to only refresh songs list if assets have changed.
        /// </summary>
        public int modificationCounter;

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
            modificationCounter = 0;
        }
    }

    private static TypeRegistryDictionary _assetTypes = new TypeRegistryDictionary(typeof(Asset));

    private static TypeRegistryDictionary _useableTypes = new TypeRegistryDictionary(typeof(Useable));

    private static Assets instance;

    /// <summary>
    /// The first time asset loading finishes it will load the main menu.
    /// </summary>
    private static bool hasFinishedInitialStartupLoading;

    /// <summary>
    /// If true, either loading during initial startup or full refresh.
    /// </summary>
    private static bool isLoadingAllAssets;

    /// <summary>
    /// If true, currently searching locations added after initial startup loading.
    /// </summary>
    private static bool isLoadingFromUpdate;

    public static AssetsRefreshed onAssetsRefreshed;

    internal static System.Action OnNewAssetsFinishedLoading;

    internal static AssetMapping defaultAssetMapping;

    /// <summary>
    /// In singleplayer and the level editor this is the same as defaultAssetMapping,
    /// but when playing on a server a subset of assets based on the server's workshop files is used.
    /// </summary>
    private static AssetMapping currentAssetMapping;

    /// <summary>
    /// Should folders be scanned for and load .dat and asset bundle files?
    /// Plugin developers find it useful to quickly launch the server.
    /// </summary>
    public static CommandLineFlag shouldLoadAnyAssets = new CommandLineFlag(defaultValue: true, "-SkipAssets");

    /// <summary>
    /// Do we want to enable shouldDeferLoadingAssets?
    /// </summary>
    public static CommandLineFlag wantsDeferLoadingAssets = new CommandLineFlag(defaultValue: true, "-NoDeferAssets");

    /// <summary>
    /// Should extra validation be performed on assets as they load?
    /// Useful for developing, but it does slow down loading.
    /// </summary>
    public static CommandLineFlag shouldValidateAssets = new CommandLineFlag(defaultValue: false, "-ValidateAssets");

    /// <summary>
    /// Should asset file metadata such as line numbers and comments be parsed?
    /// Useful for development (e.g., error messages), but may slow down loading and increases RAM usage.
    /// </summary>
    public static CommandLineFlag shouldParseMetadata = new CommandLineFlag(defaultValue: false, "-ParseAssetMetadata");

    /// <summary>
    /// Should asset files be re-saved after all loading is finished?
    /// Requires asset metadata. Useful for automatically upgrading .dat/.asset files.
    /// </summary>
    public static CommandLineFlag shouldResaveAssets = new CommandLineFlag(defaultValue: false, "-ResaveAssets");

    /// <summary>
    /// Should workshop asset names and IDs be logged while loading?
    /// Useful when debugging unknown workshop content.
    /// </summary>
    public static CommandLineFlag shouldLogWorkshopAssets = new CommandLineFlag(defaultValue: false, "-LogWorkshopAssets");

    /// <summary>
    /// Should GC and clear unused assets be called after every loading frame?
    /// Potentially useful for players running out of RAM, refer to:
    /// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/1352#issuecomment-751138105
    /// </summary>
    private static CommandLineFlag shouldCollectGarbageAggressively = new CommandLineFlag(defaultValue: false, "-AggressiveGC");

    /// <summary>
    /// Should modded spawn tables being inserted into parents be logged?
    /// Useful for debugging workshop spawn table problems.
    /// </summary>
    private static CommandLineFlag shouldLogSpawnInsertions = new CommandLineFlag(defaultValue: false, "-LogSpawnInsertions");

    /// <summary>
    /// Loaded master bundles.
    /// </summary>
    private static List<MasterBundleConfig> allMasterBundles;

    /// <summary>
    /// Loading master bundles.
    /// </summary>
    private static List<MasterBundleConfig> pendingMasterBundles;

    private static Queue<AssetsWorker.AssetDefinition> pendingAssetsToLoad;

    /// <summary>
    /// While an asset is being loaded, this is the asset.
    /// Used by some error logging.
    /// Note: not ideal because any global state like this prevents parallelization.
    /// </summary>
    internal static Asset currentAsset;

    internal static List<AssetOrigin> assetOrigins;

    internal static AssetOrigin coreOrigin;

    internal static AssetOrigin reloadOrigin;

    private static AssetOrigin legacyServerSharedOrigin;

    private static AssetOrigin legacyPerServerOrigin;

    private static List<string> errors;

    /// <summary>
    /// Do we have any new spawn assets that have not been linked yet?
    /// Used to skip linking spawns if not required when downloading assets.
    /// </summary>
    private static bool hasUnlinkedSpawns;

    internal static readonly ClientStaticMethod<Guid> SendKickForInvalidGuid = ClientStaticMethod<Guid>.Get(ReceiveKickForInvalidGuid);

    internal static readonly ClientStaticMethod<Guid, string, string, byte[], string, string> SendKickForHashMismatch = ClientStaticMethod<Guid, string, string, byte[], string, string>.Get(ReceiveKickForHashMismatch);

    internal static AssetLoadingStats loadingStats = new AssetLoadingStats();

    private AssetsWorker worker;

    internal static AssetOrigin legacyOfficialOrigin;

    internal static AssetOrigin legacyMiscOrigin;

    internal static AssetOrigin legacyWorkshopOrigin;

    public static TypeRegistryDictionary assetTypes => _assetTypes;

    public static TypeRegistryDictionary useableTypes => _useableTypes;

    /// <summary>
    /// Has initial client UGC loading step run yet?
    /// Used to defer asset loading for workshop installs that occured during startup.
    /// </summary>
    public static bool hasLoadedUgc { get; protected set; }

    /// <summary>
    /// Has initial map loading step run yet?
    /// Used to defer map loading for workshop installs that occured during startup.
    /// </summary>
    public static bool hasLoadedMaps { get; protected set; }

    public static bool isLoading
    {
        get
        {
            if (!isLoadingAllAssets)
            {
                return isLoadingFromUpdate;
            }
            return true;
        }
    }

    internal static bool ShouldWaitForNewAssetsToFinishLoading
    {
        get
        {
            if (!isLoading)
            {
                return instance.worker.IsWorking;
            }
            return true;
        }
    }

    /// <summary>
    /// Should some specific asset types which opt-in be allowed to defer loading from asset bundles until used?
    /// Disabled by asset validation because all assets need to be loaded.
    /// </summary>
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

    /// <summary>
    /// Master bundle from root /Bundles directory containing vanilla assets.
    /// </summary>
    internal static MasterBundleConfig coreMasterBundle { get; private set; }

    /// <summary>
    /// While an asset is being loaded, this is the master bundle for that asset.
    /// Used by master bundle pointer as a default.
    /// </summary>
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

    public static void ReportError(IAssetErrorContext context, string error)
    {
        if (context is Asset asset)
        {
            asset.HasErrors = true;
        }
        reportError(context.AssetErrorPrefix + ": " + error);
    }

    public static void ReportError(IAssetErrorContext context, string format, params object[] args)
    {
        string error = string.Format(format, args);
        ReportError(context, error);
    }

    public static void ReportError(IAssetErrorContext context, string format, object arg0)
    {
        string error = string.Format(format, arg0);
        ReportError(context, error);
    }

    public static void ReportError(IAssetErrorContext context, string format, object arg0, object arg1)
    {
        string error = string.Format(format, arg0, arg1);
        ReportError(context, error);
    }

    public static void ReportError(IAssetErrorContext context, string format, object arg0, object arg1, object arg2)
    {
        string error = string.Format(format, arg0, arg1, arg2);
        ReportError(context, error);
    }

    public static List<string> getReportedErrorsList()
    {
        return errors;
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

    internal static AssetOrigin FindOrAddWorkshopFileOrigin(ulong workshopFileId, bool shouldOverrideIds)
    {
        AssetOrigin assetOrigin = FindWorkshopFileOrigin(workshopFileId);
        if (assetOrigin != null)
        {
            return assetOrigin;
        }
        AssetOrigin assetOrigin2 = new AssetOrigin();
        assetOrigin2.name = $"Workshop File ({workshopFileId})";
        assetOrigin2.workshopFileId = workshopFileId;
        assetOrigin2.shouldAssetsOverrideExistingIds = shouldOverrideIds;
        assetOrigins.Add(assetOrigin2);
        return assetOrigin2;
    }

    internal static AssetOrigin FindOrAddLevelOrigin(LevelInfo level)
    {
        if (level.publishedFileId != 0L)
        {
            return FindOrAddWorkshopFileOrigin(level.publishedFileId, shouldOverrideIds: false);
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
        assetOrigin.canResave = true;
        assetOrigins.Add(assetOrigin);
        return assetOrigin;
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    public static Asset find(EAssetType type, ushort id)
    {
        if (type == EAssetType.NONE || id == 0)
        {
            return null;
        }
        currentAssetMapping.legacyAssetsTable[type].TryGetValue(id, out var value);
        int num = 0;
        while (value is RedirectorAsset redirectorAsset)
        {
            currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out value);
            num++;
            if (num > 32)
            {
                value = null;
                UnturnedLog.warn($"Infinite asset director loop encountered when resolving Type: {type} Legacy ID: {id}");
                break;
            }
        }
        return value;
    }

    /// <summary>
    /// Find an asset by GUID reference.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    /// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
    public static T find<T>(AssetReference<T> reference) where T : Asset
    {
        if (!reference.isValid)
        {
            return null;
        }
        return find(reference.GUID) as T;
    }

    /// <summary>
    /// Find an asset by GUID reference.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// Maybe considered a hack? Ignores the current per-server asset mapping.
    /// </summary>
    /// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
    public static T Find_UseDefaultAssetMapping<T>(AssetReference<T> reference) where T : Asset
    {
        return Find_UseDefaultAssetMapping(reference.GUID) as T;
    }

    /// <summary>
    /// Find an asset by GUID reference.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// Maybe considered a hack? Ignores the current per-server asset mapping.
    /// </summary>
    /// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
    public static Asset Find_UseDefaultAssetMapping(Guid assetGuid)
    {
        defaultAssetMapping.assetDictionary.TryGetValue(assetGuid, out var value);
        int num = 0;
        while (value is RedirectorAsset redirectorAsset)
        {
            currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out value);
            num++;
            if (num > 32)
            {
                value = null;
                UnturnedLog.warn($"Infinite asset director loop encountered when resolving: {assetGuid:N}");
                break;
            }
        }
        return value;
    }

    /// <summary>
    /// Load content from an assetbundle.
    /// </summary>
    public static T load<T>(ContentReference<T> reference) where T : UnityEngine.Object
    {
        if (!reference.isValid)
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = findMasterBundleByName(reference.name);
        if (masterBundleConfig != null && masterBundleConfig.assetBundle != null)
        {
            string text = masterBundleConfig.FormatAssetPathAndCache(reference.path);
            T val = masterBundleConfig.assetBundle.LoadAsset<T>(text);
            if (val == null)
            {
                UnturnedLog.warn("Failed to load content reference '{0}' from master bundle '{1}' as {2}", text, reference.name, typeof(T).Name);
            }
            return val;
        }
        return null;
    }

    /// <summary>
    /// Find an asset by GUID reference.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    /// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
    public static Asset find(Guid GUID)
    {
        currentAssetMapping.assetDictionary.TryGetValue(GUID, out var value);
        int num = 0;
        while (value is RedirectorAsset redirectorAsset)
        {
            currentAssetMapping.assetDictionary.TryGetValue(redirectorAsset.TargetGuid, out value);
            num++;
            if (num > 32)
            {
                value = null;
                UnturnedLog.warn($"Infinite asset director loop encountered when resolving: {GUID}");
                break;
            }
        }
        return value;
    }

    /// <summary>
    /// Find an asset by GUID reference.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    /// <returns>Asset with matching GUID if it exists, null otherwise.</returns>
    public static T find<T>(Guid guid) where T : Asset
    {
        return find(guid) as T;
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    public static EffectAsset FindEffectAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.EFFECT, legacyId) as EffectAsset;
        }
        return find<EffectAsset>(guid);
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    public static T FindNpcAssetByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : Asset
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.NPC, legacyId) as T;
        }
        return find<T>(guid);
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// Note: this method doesn't handle redirects by VehicleRedirectorAsset.
    /// </summary>
    public static VehicleAsset FindVehicleAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.VEHICLE, legacyId) as VehicleAsset;
        }
        return find<VehicleAsset>(guid);
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// Note: this method doesn't handle redirects by VehicleRedirectorAsset.
    /// </summary>
    public static Asset FindBaseVehicleAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.VEHICLE, legacyId);
        }
        return find(guid);
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    public static SpawnAsset FindSpawnAssetByGuidOrLegacyId(Guid guid, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.SPAWN, legacyId) as SpawnAsset;
        }
        return find<SpawnAsset>(guid);
    }

    /// <summary>
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    internal static T FindItemByGuidOrLegacyId<T>(Guid guid, ushort legacyId) where T : ItemAsset
    {
        if (guid.IsEmpty())
        {
            return find(EAssetType.ITEM, legacyId) as T;
        }
        return find<T>(guid);
    }

    /// <summary>
    /// Useful if GUID can reference a different asset type than legacy ID. For example, gun magazine GUID can
    /// reference a SpawnAsset while its legacy ID cannot.
    /// This method supports <see cref="T:SDG.Unturned.RedirectorAsset" />.
    /// </summary>
    internal static Asset FindByGuidOrLegacyId(Guid guid, EAssetType legacyAssetType, ushort legacyId)
    {
        if (guid.IsEmpty())
        {
            return find(legacyAssetType, legacyId);
        }
        return find(guid);
    }

    /// <summary>
    /// Append assets that extend from result type.
    /// </summary>
    public static void find<T>(List<T> results) where T : class
    {
        FindAssetsInListByType(currentAssetMapping.assetList, results);
    }

    internal static bool HasCurrentAssetMappingChanged(ref int counter)
    {
        bool result = currentAssetMapping.modificationCounter != counter;
        counter = currentAssetMapping.modificationCounter;
        return result;
    }

    internal static bool HasDefaultAssetMappingChanged(ref int counter)
    {
        bool result = defaultAssetMapping.modificationCounter != counter;
        counter = defaultAssetMapping.modificationCounter;
        return result;
    }

    /// <summary>
    /// Maybe considered a hack? Ignores the current per-server asset mapping.
    /// Append assets that extend from result type.
    /// </summary>
    internal static void FindAssetsByType_UseDefaultAssetMapping<T>(List<T> results) where T : class
    {
        FindAssetsInListByType(defaultAssetMapping.assetList, results);
    }

    private static void FindAssetsInListByType<T>(List<Asset> assetList, List<T> results) where T : class
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

    internal static Asset CreateAtRuntime(Type type, ushort legacyId)
    {
        try
        {
            if (Activator.CreateInstance(type) is Asset asset)
            {
                asset.GUID = Guid.NewGuid();
                asset.id = legacyId;
                AddToMapping(asset, overrideExistingID: false, defaultAssetMapping);
                if (asset is IDirtyable)
                {
                    (asset as IDirtyable).isDirty = true;
                }
                asset.OnCreatedAtRuntime();
                return asset;
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e);
        }
        return null;
    }

    internal static T CreateAtRuntime<T>(ushort legacyId) where T : Asset
    {
        return CreateAtRuntime(typeof(T), legacyId) as T;
    }

    internal static void AddToMapping(Asset asset, bool overrideExistingID, AssetMapping assetMapping)
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
        bool flag = false;
        switch (assetCategory)
        {
        case EAssetType.OBJECT:
            if (overrideExistingID)
            {
                if (assetMapping.legacyAssetsTable[assetCategory].TryGetValue(asset.id, out var value3))
                {
                    assetMapping.legacyAssetsTable[assetCategory].Remove(asset.id);
                    value3.hasBeenReplaced = true;
                    flag = true;
                }
                assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            }
            else if (!assetMapping.legacyAssetsTable[assetCategory].ContainsKey(asset.id))
            {
                assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            }
            break;
        default:
            if (asset.id != 0)
            {
                if (overrideExistingID)
                {
                    if (assetMapping.legacyAssetsTable[assetCategory].TryGetValue(asset.id, out var value))
                    {
                        assetMapping.legacyAssetsTable[assetCategory].Remove(asset.id);
                        value.hasBeenReplaced = true;
                        flag = true;
                    }
                }
                else if (assetMapping.legacyAssetsTable[assetCategory].ContainsKey(asset.id))
                {
                    assetMapping.legacyAssetsTable[assetCategory].TryGetValue(asset.id, out var value2);
                    ReportError(asset, $"legacy ID {asset.id} already taken by {value2.FriendlyNameWithFriendlyType} in {value2.GetOriginName()}!");
                    return;
                }
                assetMapping.legacyAssetsTable[assetCategory].Add(asset.id, asset);
            }
            else
            {
                bool flag2;
                switch (assetCategory)
                {
                case EAssetType.ITEM:
                    flag2 = !(asset is ItemAsset itemAsset) || !itemAsset.isPro;
                    break;
                case EAssetType.EFFECT:
                case EAssetType.VEHICLE:
                case EAssetType.SPAWN:
                case EAssetType.NPC:
                    flag2 = false;
                    break;
                default:
                    flag2 = true;
                    break;
                }
                if (flag2)
                {
                    ReportError(asset, "needs a non-zero ID");
                }
            }
            break;
        case EAssetType.NONE:
            break;
        }
        if (asset.GUID != Guid.Empty)
        {
            if (overrideExistingID)
            {
                if (assetMapping.assetDictionary.TryGetValue(asset.GUID, out var value4))
                {
                    assetMapping.assetDictionary.Remove(value4.GUID);
                    assetMapping.assetList.Remove(value4);
                    value4.hasBeenReplaced = true;
                    flag = true;
                }
            }
            else if (assetMapping.assetDictionary.ContainsKey(asset.GUID))
            {
                assetMapping.assetDictionary.TryGetValue(asset.GUID, out var value5);
                ReportError(asset, "GUID already taken by " + value5.FriendlyNameWithFriendlyType + " in " + value5.GetOriginName() + "!");
                return;
            }
            assetMapping.assetDictionary.Add(asset.GUID, asset);
            assetMapping.assetList.Add(asset);
        }
        assetMapping.modificationCounter++;
        if (flag && assetCategory == EAssetType.VEHICLE && Level.isLoaded && Provider.isServer && VehicleManager.vehicles != null && VehicleManager.vehicles.Count > 0)
        {
            VehicleManager.shouldRespawnReloadedVehicles = true;
        }
        if (asset.origin != null && asset.origin.workshopFileId != 0L && (bool)shouldLogWorkshopAssets)
        {
            UnturnedLog.info(asset.getTypeNameAndIdDisplayString());
        }
    }

    private static void AddAssetsFromOriginToCurrentMapping(AssetOrigin origin)
    {
        UnturnedLog.info($"Adding {origin.assets.Count} asset(s) from origin {origin.name} to server mapping");
        foreach (Asset asset in origin.assets)
        {
            AddToMapping(asset, overrideExistingID: true, currentAssetMapping);
        }
    }

    /// <summary>
    /// While playing on server the client will use the same dictionary/list of assets the server uses in order
    /// to reduce issues with ID conflicts.
    ///
    /// 2023-05-27: server now ALSO uses the same logic to ensure IDs are applied in consistent order regardless
    /// of multi-threaded loading order.
    /// </summary>
    internal static void ApplyServerAssetMapping(LevelInfo pendingLevel, List<PublishedFileId_t> serverWorkshopFileIds)
    {
        currentAssetMapping = new AssetMapping();
        List<AssetOrigin> list = new List<AssetOrigin>();
        list.Add(coreOrigin);
        AssetOrigin assetOrigin = null;
        if (pendingLevel != null)
        {
            assetOrigin = FindLevelOrigin(pendingLevel);
            if (assetOrigin != null)
            {
                list.Add(assetOrigin);
            }
        }
        if (serverWorkshopFileIds != null)
        {
            foreach (PublishedFileId_t serverWorkshopFileId in serverWorkshopFileIds)
            {
                AssetOrigin assetOrigin2 = FindWorkshopFileOrigin(serverWorkshopFileId.m_PublishedFileId);
                if (assetOrigin2 != null)
                {
                    if (assetOrigin2 != assetOrigin)
                    {
                        list.Add(assetOrigin2);
                    }
                }
                else
                {
                    UnturnedLog.info($"Unable to find assets for server mapping (file ID {serverWorkshopFileId})");
                }
            }
        }
        if (Dedicator.IsDedicatedServer)
        {
            foreach (AssetOrigin assetOrigin3 in assetOrigins)
            {
                if (assetOrigin3 != reloadOrigin && assetOrigin3.assets.Count >= 1 && !list.Contains(assetOrigin3))
                {
                    UnturnedLog.info("Inserting asset origin " + assetOrigin3.name + " before other assets to reduce chances of ID conflicts because otherwise it was not included");
                    list.Insert(0, assetOrigin3);
                }
            }
        }
        foreach (AssetOrigin item in list)
        {
            AddAssetsFromOriginToCurrentMapping(item);
        }
    }

    internal static void ClearServerAssetMapping()
    {
        currentAssetMapping = defaultAssetMapping;
    }

    public static void RequestReloadAllAssets()
    {
        if (hasFinishedInitialStartupLoading && !isLoading)
        {
            instance.StartCoroutine(instance.LoadAllAssets());
        }
    }

    /// <summary>
    /// Search all loaded master bundles for one in path's hierarchy.
    /// </summary>
    public static MasterBundleConfig findMasterBundleByPath(string path)
    {
        int num = 0;
        MasterBundleConfig result = null;
        foreach (MasterBundleConfig allMasterBundle in allMasterBundles)
        {
            if (allMasterBundle.directoryPath.Length < num || !path.StartsWith(allMasterBundle.directoryPath))
            {
                continue;
            }
            if (path.Length > allMasterBundle.directoryPath.Length)
            {
                char c = path[allMasterBundle.directoryPath.Length];
                if (c != '/' && c != '\\')
                {
                    continue;
                }
            }
            num = allMasterBundle.directoryPath.Length;
            result = allMasterBundle;
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

    /// <summary>
    /// Find loaded master bundle by name.
    /// </summary>
    public static MasterBundleConfig findMasterBundleByName(string name, bool matchExtension = true)
    {
        return findMasterBundleInListByName(allMasterBundles, name, matchExtension);
    }

    /// <summary>
    /// Unload all asset bundles from memory, and empty known list.
    /// Called when reloading assets.
    /// </summary>
    private static void UnloadAllMasterBundles()
    {
        foreach (MasterBundleConfig allMasterBundle in allMasterBundles)
        {
            allMasterBundle.unload();
        }
        allMasterBundles.Clear();
    }

    /// <summary>
    /// Catches exceptions thrown by LoadFile to avoid breaking loading.
    /// </summary>
    private static void TryLoadFile(AssetsWorker.AssetDefinition file)
    {
        try
        {
            loadingStats.totalFilesLoaded++;
            LoadFile(file);
        }
        catch (Exception e)
        {
            UnturnedLog.error("Exception loading file {0}:", file.path);
            UnturnedLog.exception(e);
        }
    }

    private static void LoadFile(AssetsWorker.AssetDefinition file)
    {
        string path = file.path;
        IDatDictionary assetData = file.assetData;
        byte[] array = file.hash;
        if (path.Length > 260)
        {
            reportError("Asset path exceeds 260 characters and might not load properly on Windows: \"" + path + "\"");
        }
        if (file.assetErrors != null)
        {
            foreach (string assetError in file.assetErrors)
            {
                reportError("Error parsing \"" + path + "\": \"" + assetError + "\"");
            }
        }
        string directoryName = Path.GetDirectoryName(path);
        string text = (path.EndsWith("Asset.dat", StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(directoryName) : Path.GetFileNameWithoutExtension(path));
        Guid value = default(Guid);
        Type type = null;
        if (assetData.TryGetDictionary("Metadata", out var node))
        {
            if (!node.TryParseGuid("GUID", out value))
            {
                reportError("Unable to parse Metadata.GUID in \"" + path + "\"");
                return;
            }
            type = node.ParseType("Type");
            if (type == null)
            {
                reportError("Unable to parse Metadata.Type in \"" + path + "\"");
                return;
            }
        }
        else if (!assetData.ContainsKey("GUID"))
        {
            value = Guid.NewGuid();
            try
            {
                string text2 = File.ReadAllText(path);
                text2 = "GUID " + value.ToString("N") + Environment.NewLine + text2;
                File.WriteAllText(path, text2);
                UnturnedLog.info($"Assigned GUID {value:N} to asset \"{path}\"");
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught IO exception adding GUID to \"" + path + "\":");
            }
        }
        else if (!assetData.TryParseGuid("GUID", out value))
        {
            reportError("Unable to parse GUID in \"" + path + "\"");
            return;
        }
        if (value.IsEmpty())
        {
            reportError("Cannot use empty GUID in \"" + path + "\"");
            return;
        }
        IDatDictionary datDictionary = assetData;
        if (assetData.TryGetDictionary("Asset", out var node2))
        {
            datDictionary = node2;
        }
        if (type == null)
        {
            string @string = datDictionary.GetString("Type");
            if (string.IsNullOrEmpty(@string))
            {
                reportError("Missing asset Type in \"" + path + "\"");
                return;
            }
            type = assetTypes.getType(@string);
            if (type == null)
            {
                type = datDictionary.ParseType("Type");
                if (type == null)
                {
                    reportError("Unhandled asset type \"" + @string + "\" in \"" + path + "\"");
                    return;
                }
            }
        }
        if (!typeof(Asset).IsAssignableFrom(type))
        {
            reportError($"Type \"{type}\" is not a valid asset type in \"{path}\"");
            return;
        }
        MasterBundleConfig masterBundleConfig = findMasterBundleByPath(path);
        string string2 = datDictionary.GetString("Master_Bundle_Override");
        if (string2 != null)
        {
            masterBundleConfig = findMasterBundleByName(string2);
            if (masterBundleConfig == null)
            {
                UnturnedLog.warn("Unable to find master bundle override '{0}' for '{1}'", string2, path);
            }
        }
        else if (datDictionary.ContainsKey("Exclude_From_Master_Bundle"))
        {
            masterBundleConfig = null;
        }
        if (masterBundleConfig != null && masterBundleConfig.assetBundle == null)
        {
            UnturnedLog.warn("Skipping master bundle '{0}' for '{1}' because asset bundle is null", masterBundleConfig.assetBundleName, path);
            masterBundleConfig = null;
        }
        currentMasterBundle = masterBundleConfig;
        int a = -1;
        Bundle bundle;
        if (masterBundleConfig != null)
        {
            if (!datDictionary.TryGetString("Bundle_Override_Path", out var value2))
            {
                value2 = (datDictionary.ParseBool("Bundle_Path_Include_Filename") ? Path.ChangeExtension(path, null).Substring(masterBundleConfig.directoryPath.Length) : directoryName.Substring(masterBundleConfig.directoryPath.Length));
                value2 = value2.Replace('\\', '/');
            }
            bundle = new MasterBundle(masterBundleConfig, value2, text);
            string text3 = value2.ToLowerInvariant() + "/" + text.ToLowerInvariant();
            array = Hash.combine(array, Hash.SHA1(text3));
            a = masterBundleConfig.version;
        }
        else if (datDictionary.ContainsKey("Bundle_Override_Path"))
        {
            string string3 = datDictionary.GetString("Bundle_Override_Path");
            int num = string3.LastIndexOf('/');
            string text4 = ((num != -1) ? string3.Substring(num + 1) : string3);
            string3 = string3 + "/" + text4 + ".unity3d";
            bundle = new Bundle(string3, usePath: false, text);
        }
        else
        {
            bundle = new Bundle(directoryName + "/" + text + ".unity3d", usePath: false);
        }
        int num2 = datDictionary.ParseInt32("Asset_Bundle_Version", 1);
        if (num2 < 1)
        {
            reportError(text + " Lowest individual asset bundle version is 1 (default), associated with 5.5.");
            num2 = 1;
        }
        else if (num2 > 6)
        {
            reportError(text + " Highest individual asset bundle version is 6, associated with 2022 LTS.");
            num2 = 6;
        }
        int num3 = Mathf.Max(a, num2);
        bundle.convertShadersToStandard = num3 < 2;
        bundle.consolidateShaders = num3 < 3 || (datDictionary.ContainsKey("Enable_Shader_Consolidation") && !datDictionary.ContainsKey("Disable_Shader_Consolidation"));
        Local localization = new Local(file.translationData, file.fallbackTranslationData);
        ushort id = datDictionary.ParseUInt16("ID", 0);
        Asset asset;
        try
        {
            asset = Activator.CreateInstance(type) as Asset;
        }
        catch (Exception e2)
        {
            reportError($"Caught exception while constructing {type} in \"{path}\": {getExceptionMessage(e2)}");
            UnturnedLog.exception(e2);
            bundle.unload();
            currentMasterBundle = null;
            currentAsset = null;
            return;
        }
        if (asset == null)
        {
            reportError($"Failed to construct {type} in \"{path}\"");
            bundle.unload();
            currentMasterBundle = null;
            currentAsset = null;
            return;
        }
        currentAsset = asset;
        try
        {
            asset.id = id;
            asset.GUID = value;
            asset.hash = array;
            asset.requiredShaderUpgrade = bundle.convertShadersToStandard || bundle.consolidateShaders;
            asset.HasErrors = file.assetErrors != null && file.assetErrors.Count > 0;
            asset.absoluteOriginFilePath = path;
            asset.origin = file.origin;
            int num4;
            if ((bool)shouldResaveAssets)
            {
                AssetOrigin origin = asset.origin;
                if (origin != null && origin.canResave)
                {
                    num4 = (((bool)shouldParseMetadata) ? 1 : 0);
                    goto IL_0646;
                }
            }
            num4 = 0;
            goto IL_0646;
            IL_0646:
            bool flag = (byte)num4 != 0;
            if (flag)
            {
                asset.OriginParsedData = assetData;
            }
            if (datDictionary.ParseBool("Keep_Localization_Loaded"))
            {
                asset.Localization = localization;
            }
            PopulateAssetParameters populateAssetParameters = default(PopulateAssetParameters);
            populateAssetParameters.bundle = bundle;
            populateAssetParameters.data = datDictionary;
            populateAssetParameters.localization = localization;
            populateAssetParameters.CanPerformDataConversions = flag;
            PopulateAssetParameters p = populateAssetParameters;
            asset.PopulateAsset(in p);
            asset.origin.assets.Add(asset);
            AddToMapping(asset, file.origin.shouldAssetsOverrideExistingIds, defaultAssetMapping);
            bundle.unload();
        }
        catch (Exception e3)
        {
            reportError("Caught exception while populating \"" + path + "\": " + getExceptionMessage(e3));
            UnturnedLog.exception(e3);
            bundle.unload();
        }
        currentMasterBundle = null;
        currentAsset = null;
    }

    /// <summary>
    /// Called when a new workshop item is installed either on client or server.
    /// </summary>
    public static void RequestAddSearchLocation(string absoluteDirectoryPath, AssetOrigin origin)
    {
        instance.AddSearchLocation(absoluteDirectoryPath, origin);
    }

    /// <summary>
    /// Reload assets in given folder.
    /// </summary>
    public static void reload(string absolutePath)
    {
        if (hasFinishedInitialStartupLoading && !isLoading)
        {
            loadingStats.Reset();
            RequestAddSearchLocation(absolutePath, reloadOrigin);
        }
    }

    public static void ReloadAsset(Asset asset)
    {
        reload(Path.GetDirectoryName(asset.absoluteOriginFilePath));
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

    /// <summary>
    /// Can now be safely called multiple times on client in order to handle spawns for downloaded maps.
    /// Spawn tables have "roots" which allow mods to insert custom spawns into the vanilla spawn tables.
    /// This method is used after workshop assets are loaded on client, or after the dedicated server is done downloading workshop content.
    /// </summary>
    public static void linkSpawns()
    {
        if (!hasUnlinkedSpawns)
        {
            return;
        }
        hasUnlinkedSpawns = false;
        List<SpawnAsset> list = new List<SpawnAsset>();
        FindAssetsByType_UseDefaultAssetMapping(list);
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        foreach (SpawnAsset item in list)
        {
            if (item.insertRoots.Count < 1)
            {
                continue;
            }
            foreach (SpawnTable insertRoot in item.insertRoots)
            {
                SpawnAsset spawnAsset;
                if (insertRoot.legacySpawnId != 0)
                {
                    spawnAsset = find(EAssetType.SPAWN, insertRoot.legacySpawnId) as SpawnAsset;
                    if (spawnAsset == null)
                    {
                        ReportError(item, "unable to find root {0} during link", insertRoot.legacySpawnId);
                        continue;
                    }
                }
                else
                {
                    if (insertRoot.targetGuid.IsEmpty())
                    {
                        continue;
                    }
                    Asset asset = find(insertRoot.targetGuid);
                    if (asset == null)
                    {
                        ReportError(item, "unable to find root {0} during link", insertRoot.targetGuid);
                        continue;
                    }
                    spawnAsset = asset as SpawnAsset;
                    if (spawnAsset == null)
                    {
                        ReportError(item, $"root {insertRoot.targetGuid} found as {asset.GetTypeFriendlyName()} {asset.FriendlyName} (not a spawn table)");
                        continue;
                    }
                }
                insertRoot.legacySpawnId = 0;
                insertRoot.targetGuid = item.GUID;
                insertRoot.isLink = true;
                spawnAsset.tables.Add(insertRoot);
                if (insertRoot.isOverride)
                {
                    spawnAsset.markOverridden();
                }
                spawnAsset.markTablesDirty();
                num++;
                if ((bool)shouldLogSpawnInsertions)
                {
                    if (insertRoot.isOverride)
                    {
                        UnturnedLog.info("Spawn {0} overriding {1}", item.name, spawnAsset.name);
                    }
                    else
                    {
                        UnturnedLog.info("Spawn {0} inserted into {1}", item.name, spawnAsset.name);
                    }
                }
            }
            item.insertRoots.Clear();
        }
        foreach (SpawnAsset item2 in list)
        {
            if (item2.areTablesDirty)
            {
                item2.sortAndNormalizeWeights();
                num2++;
            }
        }
        foreach (SpawnAsset item3 in list)
        {
            foreach (SpawnTable table in item3.tables)
            {
                if (table.hasNotifiedChild)
                {
                    continue;
                }
                table.hasNotifiedChild = true;
                SpawnAsset spawnAsset2;
                if (table.legacySpawnId != 0)
                {
                    spawnAsset2 = find(EAssetType.SPAWN, table.legacySpawnId) as SpawnAsset;
                    if (spawnAsset2 == null)
                    {
                        ReportError(item3, "unable to find child table {0} during link", table.legacySpawnId);
                        continue;
                    }
                }
                else
                {
                    if (table.targetGuid.IsEmpty())
                    {
                        continue;
                    }
                    Asset asset2 = find(table.targetGuid);
                    if (asset2 == null)
                    {
                        ReportError(item3, "unable to find child {0} during link", table.targetGuid);
                        continue;
                    }
                    spawnAsset2 = asset2 as SpawnAsset;
                    if (spawnAsset2 == null)
                    {
                        continue;
                    }
                }
                SpawnTable spawnTable = new SpawnTable();
                spawnTable.targetGuid = item3.GUID;
                spawnTable.weight = table.weight;
                spawnTable.normalizedWeight = table.normalizedWeight;
                spawnTable.isLink = table.isLink;
                spawnTable.isOverride = table.isOverride;
                spawnAsset2.roots.Add(spawnTable);
                num3++;
            }
        }
        UnturnedLog.info("Link spawns: {0} children, {1} sorted/normalized and {2} parents", num, num2, num3);
    }

    public static void initializeMasterBundleValidation()
    {
        MasterBundleValidation.initialize(allMasterBundles);
    }

    /// <summary>
    /// Look through all item blueprints and log errors if there are duplicates.
    /// </summary>
    private void CheckForBlueprintErrors()
    {
        Func<Blueprint, Blueprint, bool> func = delegate(Blueprint myBlueprint, Blueprint yourBlueprint)
        {
            if (myBlueprint.Operation != yourBlueprint.Operation)
            {
                return false;
            }
            if (myBlueprint.SkillSpecialityIndex != yourBlueprint.SkillSpecialityIndex)
            {
                return false;
            }
            if (myBlueprint.SkillIndex != yourBlueprint.SkillIndex)
            {
                return false;
            }
            if (myBlueprint.CategoryTagRef != yourBlueprint.CategoryTagRef)
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
            if (myBlueprint.questConditions != null != (yourBlueprint.questConditions != null))
            {
                return false;
            }
            if (myBlueprint.questConditions != null && myBlueprint.questConditions.Length != yourBlueprint.questConditions.Length)
            {
                return false;
            }
            if (myBlueprint.questRewards != null != (yourBlueprint.questRewards != null))
            {
                return false;
            }
            if (myBlueprint.questRewards != null && myBlueprint.questRewards.Length != yourBlueprint.questRewards.Length)
            {
                return false;
            }
            if (myBlueprint.RequiresNearbyCraftingTags != null != (yourBlueprint.RequiresNearbyCraftingTags != null))
            {
                return false;
            }
            if (myBlueprint.RequiresNearbyCraftingTags != null && myBlueprint.RequiresNearbyCraftingTags.Length != yourBlueprint.RequiresNearbyCraftingTags.Length)
            {
                return false;
            }
            if (myBlueprint.TargetItem != null != (yourBlueprint.TargetItem != null))
            {
                return false;
            }
            if (myBlueprint.TargetItem != null && !myBlueprint.TargetItem.Equals(yourBlueprint.TargetItem))
            {
                return false;
            }
            for (byte b5 = 0; b5 < myBlueprint.outputs.Length; b5++)
            {
                if (myBlueprint.outputs[b5].ItemRef != yourBlueprint.outputs[b5].ItemRef)
                {
                    return false;
                }
            }
            for (byte b6 = 0; b6 < myBlueprint.supplies.Length; b6++)
            {
                if (!myBlueprint.supplies[b6].Equals(yourBlueprint.supplies[b6]))
                {
                    return false;
                }
            }
            if (myBlueprint.questConditions != null)
            {
                for (int m = 0; m < myBlueprint.questConditions.Length; m++)
                {
                    if (!myBlueprint.questConditions[m].Equals(yourBlueprint.questConditions[m]))
                    {
                        return false;
                    }
                }
            }
            if (myBlueprint.questRewards != null)
            {
                for (int n = 0; n < myBlueprint.questRewards.Length; n++)
                {
                    if (!myBlueprint.questRewards[n].Equals(yourBlueprint.questRewards[n]))
                    {
                        return false;
                    }
                }
            }
            if (myBlueprint.RequiresNearbyCraftingTags != null)
            {
                for (int num = 0; num < myBlueprint.RequiresNearbyCraftingTags.Length; num++)
                {
                    if (!myBlueprint.RequiresNearbyCraftingTags[num].Equals(yourBlueprint.RequiresNearbyCraftingTags[num]))
                    {
                        return false;
                    }
                }
            }
            return true;
        };
        List<ItemAsset> list = new List<ItemAsset>();
        find(list);
        if (list.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            ItemAsset itemAsset = list[i];
            for (byte b = 0; b < itemAsset.blueprints.Count; b++)
            {
                Blueprint blueprint = itemAsset.blueprints[b];
                for (byte b2 = 0; b2 < itemAsset.blueprints.Count; b2++)
                {
                    if (b2 != b)
                    {
                        Blueprint arg = itemAsset.blueprints[b2];
                        if (func(blueprint, arg))
                        {
                            ReportError(itemAsset, $"blueprint [{b}] is identical to blueprint [{b2}]");
                        }
                    }
                }
                if (blueprint.supplies != null && blueprint.supplies.Length > 1)
                {
                    for (int j = 0; j < blueprint.supplies.Length - 1; j++)
                    {
                        for (int k = j + 1; k < blueprint.supplies.Length; k++)
                        {
                            BlueprintSupply obj = blueprint.supplies[j];
                            BlueprintSupply other = blueprint.supplies[k];
                            if (obj.Equals(other))
                            {
                                ReportError(itemAsset, $"blueprint [{b}] input items [{j}] and [{k}] are identical");
                            }
                        }
                    }
                }
            }
            for (int l = 0; l < list.Count; l++)
            {
                if (l == i)
                {
                    continue;
                }
                ItemAsset itemAsset2 = list[l];
                for (byte b3 = 0; b3 < itemAsset.blueprints.Count; b3++)
                {
                    Blueprint arg2 = itemAsset.blueprints[b3];
                    for (byte b4 = 0; b4 < itemAsset2.blueprints.Count; b4++)
                    {
                        Blueprint arg3 = itemAsset2.blueprints[b4];
                        if (func(arg2, arg3))
                        {
                            ReportError(itemAsset, $"blueprint [{b3}] is identical to {itemAsset2.itemName} blueprint [{b4}]");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Look through all dialogue and check that their referenced
    /// dialogueID or vendorID is an actual loaded asset.
    /// </summary>
    private void CheckForNpcErrors()
    {
        List<DialogueAsset> list = new List<DialogueAsset>();
        find(list);
        foreach (DialogueAsset item in list)
        {
            int num = item.responses.Length;
            for (int i = 0; i < num; i++)
            {
                DialogueResponse dialogueResponse = item.responses[i];
                if (!dialogueResponse.IsDialogueRefNull() && dialogueResponse.FindDialogueAsset() == null)
                {
                    ReportError(item, "unable to find dialogue asset for response " + i);
                }
                if (!dialogueResponse.IsVendorRefNull() && dialogueResponse.FindVendorAsset() == null)
                {
                    ReportError(item, "unable to find vendor asset for response " + i);
                }
            }
        }
    }

    /// <summary>
    /// Manually run asset unload and garbage collection in the hope
    /// that it will minimize RAM allocated during loading.
    /// </summary>
    private void CleanupMemory()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /// <summary>
    /// Helper for Assets.init.
    /// Load all non-map assets from:
    /// 	/Bundles/Workshop/Content
    /// 	/Servers/ServerID/Workshop/Content
    /// 	/Servers/ServerID/Bundles
    /// </summary>
    private void AddDedicatedServerUgcSearchLocations()
    {
        string path = Path.Combine(ReadWrite.PATH, "Bundles", "Workshop", "Content");
        if (ReadWrite.folderExists(path, usePath: false))
        {
            AddSearchLocation(path, legacyServerSharedOrigin);
        }
        string path2 = Path.Combine(ReadWrite.PATH, ServerSavedata.directoryName, Provider.serverID, "Workshop", "Content");
        if (ReadWrite.folderExists(path2, usePath: false))
        {
            AddSearchLocation(path2, legacyPerServerOrigin);
        }
        string path3 = Path.Combine(ReadWrite.PATH, ServerSavedata.directoryName, Provider.serverID, "Bundles");
        if (ReadWrite.folderExists(path3, usePath: false))
        {
            AddSearchLocation(path3, legacyPerServerOrigin);
        }
    }

    /// <summary>
    /// Helper for Assets.init.
    /// Load all non-map assets from subscribed UGC.
    /// </summary>
    private void AddClientUgcSearchLocations()
    {
        if (Provider.provider.workshopService.ugc == null)
        {
            return;
        }
        SteamContent[] array = Provider.provider.workshopService.ugc.ToArray();
        hasLoadedUgc = true;
        SteamContent[] array2 = array;
        foreach (SteamContent steamContent in array2)
        {
            if (LocalWorkshopSettings.get().getEnabled(steamContent.publishedFileID) && (steamContent.type == ESteamUGCType.OBJECT || steamContent.type == ESteamUGCType.ITEM || steamContent.type == ESteamUGCType.VEHICLE))
            {
                AssetOrigin origin = FindOrAddWorkshopFileOrigin(steamContent.publishedFileID.m_PublishedFileId, shouldOverrideIds: false);
                AddSearchLocation(steamContent.path, origin);
            }
        }
    }

    /// <summary>
    /// Helper for modders creating workshop content.
    /// Loads folders in the "Sandbox" directory the same way workshop files are loaded.
    /// </summary>
    private void AddSandboxSearchLocations()
    {
        string path = Path.Combine(ReadWrite.PATH, "Sandbox");
        if (Directory.Exists(path))
        {
            string[] folders = ReadWrite.getFolders(path, usePath: false);
            foreach (string path2 in folders)
            {
                string fileName = Path.GetFileName(path2);
                UnturnedLog.info("Sandbox: {0}", fileName);
                AssetOrigin assetOrigin = new AssetOrigin();
                assetOrigin.name = "Sandbox Folder \"" + fileName + "\"";
                assetOrigin.shouldAssetsOverrideExistingIds = true;
                assetOrigin.canResave = true;
                assetOrigins.Add(assetOrigin);
                AddSearchLocation(path2, assetOrigin);
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Helper for Assets.init.
    /// Load all assets in each map's Bundles folder, and content in map's Content folder.
    /// </summary>
    private void AddMapSearchLocations()
    {
        LevelInfo[] levels = Level.getLevels(ESingleplayerMapCategory.ALL);
        hasLoadedMaps = true;
        foreach (LevelInfo levelInfo in levels)
        {
            if (levelInfo != null)
            {
                string path = Path.Combine(levelInfo.path, "Bundles");
                if (ReadWrite.folderExists(path, usePath: false))
                {
                    AssetOrigin origin = FindOrAddLevelOrigin(levelInfo);
                    AddSearchLocation(path, origin);
                }
            }
        }
    }

    private void AddSearchLocation(string path, AssetOrigin origin)
    {
        path = Path.GetFullPath(path);
        UnturnedLog.info(origin.name + " added asset search location \"" + path + "\"");
        worker.RequestSearch(path, origin);
    }

    private MasterBundleConfig FindAndRemoveLoadedPendingMasterBundle()
    {
        for (int num = pendingMasterBundles.Count - 1; num >= 0; num--)
        {
            MasterBundleConfig masterBundleConfig = pendingMasterBundles[num];
            if (masterBundleConfig.assetBundleCreateRequest.isDone)
            {
                pendingMasterBundles.RemoveAtFast(num);
                return masterBundleConfig;
            }
        }
        return null;
    }

    private IEnumerator LoadAssetsFromWorkerThread()
    {
        double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
        int gcFrameCount = 0;
        while (worker.IsWorking || pendingMasterBundles.Count > 0 || pendingAssetsToLoad.Count > 0)
        {
            if (worker.TryDequeueResult(out var result))
            {
                switch (result.ResultType)
                {
                case AssetsWorker.EResultType.MasterBundle:
                {
                    AssetsWorker.MasterBundle masterBundle = (AssetsWorker.MasterBundle)result;
                    MasterBundleConfig config = masterBundle.config;
                    pendingMasterBundles.Add(config);
                    config.StartLoad(masterBundle.assetBundleData, masterBundle.hash);
                    loadingStats.isLoadingAssetBundles = true;
                    break;
                }
                case AssetsWorker.EResultType.Asset:
                {
                    AssetsWorker.AssetDefinition item = (AssetsWorker.AssetDefinition)result;
                    pendingAssetsToLoad.Enqueue(item);
                    break;
                }
                case AssetsWorker.EResultType.Exception:
                {
                    AssetsWorker.ExceptionDetails exceptionDetails = (AssetsWorker.ExceptionDetails)result;
                    UnturnedLog.exception(exceptionDetails.exception, exceptionDetails.message);
                    break;
                }
                }
            }
            AssetsWorker.AssetDefinition result2;
            if (pendingMasterBundles.Count > 0)
            {
                MasterBundleConfig masterBundleConfig = FindAndRemoveLoadedPendingMasterBundle();
                if (masterBundleConfig != null)
                {
                    masterBundleConfig.FinishLoad();
                    loadingStats.totalMasterBundlesLoaded++;
                    if (masterBundleConfig.assetBundle != null)
                    {
                        if (masterBundleConfig.origin == coreOrigin && string.Equals(masterBundleConfig.assetBundleName, "core.masterbundle", StringComparison.InvariantCulture))
                        {
                            coreMasterBundle = masterBundleConfig;
                        }
                        allMasterBundles.Add(masterBundleConfig);
                    }
                    else
                    {
                        MasterBundleConfig masterBundleConfig2 = findMasterBundleByName(masterBundleConfig.assetBundleName);
                        if (masterBundleConfig2 != null)
                        {
                            masterBundleConfig.CopyAssetBundleFromDuplicateConfig(masterBundleConfig2);
                            if (masterBundleConfig.assetBundle != null)
                            {
                                UnturnedLog.info("Using \"" + masterBundleConfig2.assetBundleName + "\" in \"" + masterBundleConfig2.directoryPath + "\" as fallback asset bundle for \"" + masterBundleConfig.directoryPath + "\"");
                                allMasterBundles.Add(masterBundleConfig);
                            }
                            else
                            {
                                UnturnedLog.info("Unable to use \"" + masterBundleConfig2.assetBundleName + "\" in \"" + masterBundleConfig2.directoryPath + "\" as fallback asset bundle for \"" + masterBundleConfig.directoryPath + "\"");
                            }
                        }
                        else
                        {
                            UnturnedLog.info("Unable to find a fallback asset bundle for \"" + masterBundleConfig.assetBundleName + "\"");
                        }
                    }
                }
                if (pendingMasterBundles.Count < 1)
                {
                    loadingStats.isLoadingAssetBundles = false;
                }
            }
            else if (coreMasterBundle != null && pendingAssetsToLoad.TryDequeue(out result2))
            {
                TryLoadFile(result2);
            }
            if (Time.realtimeSinceStartupAsDouble - realtimeSinceStartupAsDouble > 0.05)
            {
                SyncAssetDefinitionLoadingProgress();
                int num = gcFrameCount + 1;
                gcFrameCount = num;
                if (gcFrameCount % 25 == 0 && (bool)shouldCollectGarbageAggressively)
                {
                    CleanupMemory();
                }
                yield return null;
                realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
            }
        }
    }

    internal static void SyncAssetDefinitionLoadingProgress()
    {
        loadingStats.totalRegisteredSearchLocations = instance.worker.totalSearchLocationRequests;
        loadingStats.totalSearchLocationsFinishedSearching = instance.worker.totalSearchLocationsFinishedSearching;
        loadingStats.totalMasterBundlesFound = instance.worker.totalMasterBundlesFound;
        loadingStats.totalFilesFound = instance.worker.totalAssetDefinitionsFound;
        loadingStats.totalFilesRead = instance.worker.totalAssetDefinitionsRead;
        LoadingUI.NotifyAssetDefinitionLoadingProgress();
    }

    private IEnumerator LoadAllAssets()
    {
        isLoadingAllAssets = true;
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
        coreMasterBundle = null;
        if (allMasterBundles == null)
        {
            allMasterBundles = new List<MasterBundleConfig>();
            pendingMasterBundles = new List<MasterBundleConfig>();
            pendingAssetsToLoad = new Queue<AssetsWorker.AssetDefinition>();
        }
        else
        {
            UnloadAllMasterBundles();
            pendingAssetsToLoad.Clear();
        }
        assetOrigins = new List<AssetOrigin>();
        loadingStats.Reset();
        coreOrigin = new AssetOrigin();
        coreOrigin.name = "Vanilla Built-in Assets";
        coreOrigin.canResave = Application.isEditor;
        assetOrigins.Add(coreOrigin);
        reloadOrigin = new AssetOrigin();
        reloadOrigin.name = "Reloaded Assets (Debug)";
        reloadOrigin.shouldAssetsOverrideExistingIds = true;
        assetOrigins.Add(reloadOrigin);
        legacyServerSharedOrigin = new AssetOrigin();
        legacyServerSharedOrigin.name = "Server Common (Legacy)";
        assetOrigins.Add(legacyServerSharedOrigin);
        legacyPerServerOrigin = new AssetOrigin();
        legacyPerServerOrigin.name = "Per-Server (Legacy)";
        assetOrigins.Add(legacyPerServerOrigin);
        yield return null;
        ResourceHash.Initialize();
        if ((bool)shouldLoadAnyAssets)
        {
            AddSearchLocation(Path.Combine(ReadWrite.PATH, "Bundles"), coreOrigin);
            if (Dedicator.IsDedicatedServer)
            {
                AddDedicatedServerUgcSearchLocations();
            }
            else
            {
                AddClientUgcSearchLocations();
            }
            AddSandboxSearchLocations();
            AddMapSearchLocations();
            yield return null;
            if (!Dedicator.IsDedicatedServer)
            {
                Provider.initAutoSubscribeMaps();
            }
            yield return LoadAssetsFromWorkerThread();
        }
        LoadingUI.SetLoadingText("Loading_Blueprints");
        yield return null;
        if ((bool)shouldValidateAssets)
        {
            CheckForBlueprintErrors();
        }
        LoadingUI.SetLoadingText("Loading_Spawns");
        yield return null;
        if (!Dedicator.IsDedicatedServer)
        {
            linkSpawns();
        }
        if ((bool)shouldValidateAssets)
        {
            CheckForNpcErrors();
        }
        CleanupMemory();
        if ((bool)shouldResaveAssets && (bool)shouldParseMetadata)
        {
            ResaveAssets();
        }
        LoadingUI.SetLoadingText("Loading_Misc");
        yield return null;
        onAssetsRefreshed?.Invoke();
        yield return null;
        UnturnedLog.info($"Loading all assets took {Time.realtimeSinceStartupAsDouble - startTime}s");
        isLoadingAllAssets = false;
    }

    private IEnumerator StartupAssetLoading()
    {
        yield return LoadAllAssets();
        hasFinishedInitialStartupLoading = true;
        if (Dedicator.IsDedicatedServer)
        {
            Provider.host();
            yield break;
        }
        LoadingUI.SetLoadingText("Loading_MainMenu");
        yield return null;
        UnturnedLog.info("Launching main menu");
        SceneManager.LoadScene("Menu");
    }

    private void ResaveAssets()
    {
        UnturnedLog.info("Re-saving assets!");
        DatWriter datWriter = new DatWriter();
        MetadataPreservingDatWriter metadataPreservingDatWriter = new MetadataPreservingDatWriter();
        foreach (Asset asset in defaultAssetMapping.assetList)
        {
            if (asset.OriginParsedData == null)
            {
                continue;
            }
            if (asset.HasErrors)
            {
                UnturnedLog.info($"Skipping re-saving asset {asset} because it loaded with errors and may lose data");
                continue;
            }
            try
            {
                asset.PreResaveAsset(asset.OriginParsedData);
                using StreamWriter output = new StreamWriter(asset.absoluteOriginFilePath, append: false, Encoding.UTF8);
                datWriter.SetOutput(output);
                metadataPreservingDatWriter.WriteRootDictionary(asset.OriginParsedData, datWriter);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception re-saving asset {asset}");
            }
        }
    }

    private IEnumerator LoadNewAssetsFromUpdate()
    {
        isLoadingFromUpdate = true;
        double startTime = Time.realtimeSinceStartupAsDouble;
        yield return LoadAssetsFromWorkerThread();
        linkSpawnsIfDirty();
        CleanupMemory();
        UnturnedLog.info($"Loading new assets took {Time.realtimeSinceStartupAsDouble - startTime}s");
        isLoadingFromUpdate = false;
        OnNewAssetsFinishedLoading?.Invoke();
    }

    /// <summary>
    /// Not the tidiest place for this, but it allows startup to pause and show error message.
    /// Occasionally there have been reports of the steamclient redist files being out of date on the dedicated
    /// server causing problems. For example: https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2866#issuecomment-965945985
    /// </summary>
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
            worker = new AssetsWorker();
            worker.Initialize();
            StartCoroutine(StartupAssetLoading());
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        worker.Update();
        if (!isLoading && worker.IsWorking)
        {
            StartCoroutine(LoadNewAssetsFromUpdate());
        }
    }

    private void OnDestroy()
    {
        worker.Shutdown();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveKickForInvalidGuid(Guid guid)
    {
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CUSTOM;
        Asset asset = find(guid);
        if (asset != null)
        {
            Provider._connectionFailureReason = string.Concat($"Server missing asset: \"{asset.FriendlyName}\" File: \"{asset.name}\" Id: {guid:N}" + "\nFile path: \"" + asset.absoluteOriginFilePath + "\"", "\nClient asset is from ", asset.GetOriginName(), ".");
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
        Asset asset = find(guid);
        bool flag;
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
                    flag = true;
                }
                else if (!string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle == null)
                {
                    text2 = $"Client loaded \"{serverFriendlyName}\" from legacy asset bundle but server did not! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nServer asset bundle name: \"" + serverAssetBundleNameWithoutExtension + "\".";
                    flag = true;
                }
                else if (string.IsNullOrEmpty(serverAssetBundleNameWithoutExtension) && asset.originMasterBundle != null)
                {
                    text2 = $"Server loaded \"{serverFriendlyName}\" from legacy asset bundle but client did not! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nClient asset bundle name: \"" + asset.originMasterBundle.assetBundleNameWithoutExtension + "\"";
                    flag = true;
                }
                else if (Hash.verifyHash(asset.hash, serverHash))
                {
                    text2 = $"Server asset bundle hash out of date for \"{serverFriendlyName}\"! (File: \"{asset.name}\" ID: {guid:N})";
                    text2 = text2 + "\nThis probably means the mod creator should re-export the \"" + serverAssetBundleNameWithoutExtension + "\" asset bundle.";
                    flag = false;
                }
                else
                {
                    text2 = $"Client and server disagree on asset \"{asset.FriendlyName}\" configuration. (File: \"{asset.name}\" ID: {guid:N})";
                    text2 += "\nUsually this means the files are different versions in which case updating the client and server might fix it.";
                    text2 += "\nAlternatively the file may have been corrupted, locally modified, or modified on the server.";
                    text2 = text2 + "\nClient hash is " + Hash.toString(asset.hash) + ", whereas server hash is " + Hash.toString(serverHash) + ".";
                    flag = true;
                }
            }
            else
            {
                text2 = $"Client and server have different assets with the same ID! ({guid:N})";
                text2 += "\nThis probably means an existing file was copied, but the mod creator can fix it by changing the ID.";
                text2 = ((!string.Equals(asset.FriendlyName, serverFriendlyName)) ? (text2 + "\nClient display name is \"" + asset.FriendlyName + "\", whereas server display name is \"" + serverFriendlyName + "\".") : (text2 + "\nDisplay name \"" + serverFriendlyName + "\" matches between client and server."));
                text2 = ((!string.Equals(asset.name, serverName)) ? (text2 + "\nClient file name is \"" + asset.name + "\", whereas server file name is \"" + serverName + "\".") : (text2 + "\nFile name \"" + asset.name + "\" matches between client and server."));
                flag = true;
            }
            text2 = ((!string.Equals(text, serverAssetOrigin)) ? (text2 + "\nClient asset is from " + text + ", whereas server asset is from " + serverAssetOrigin + ".") : (text2 + "\nClient and server agree this asset is from " + text + "."));
            Provider._connectionFailureReason = text2;
        }
        else
        {
            Provider._connectionFailureReason = $"Unknown asset hash mismatch? (should never happen) Name: \"{serverFriendlyName}\" File: \"{serverName}\" Id: {guid:N}";
            flag = true;
        }
        Provider._connectionFailureInfo = (flag ? ESteamConnectionFailureInfo.CUSTOM_SHOULD_VERIFY_GAME_FILES : ESteamConnectionFailureInfo.CUSTOM);
        Provider.RequestDisconnect($"Kicked for asset hash mismatch guid: {guid:N} serverName: \"{serverName}\" serverFriendlyName: \"{serverFriendlyName}\" serverHash: {Hash.toString(serverHash)} serverAssetBundleName: \"{serverAssetBundleNameWithoutExtension}\" serverAssetOrigin: \"{serverAssetOrigin}\"");
    }

    [Obsolete("Renamed to RequestAddSearchLocation")]
    public static void load(string absoluteDirectoryPath, AssetOrigin origin, bool overrideExistingIDs)
    {
        RequestAddSearchLocation(absoluteDirectoryPath, origin);
    }

    [Obsolete("Renamed to RequestReloadAllAssets")]
    public static void refresh()
    {
        RequestReloadAllAssets();
    }

    [Obsolete]
    public static void rename(Asset asset, string newName)
    {
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

    [Obsolete]
    public static Asset find(EAssetType type, string name)
    {
        return null;
    }

    [Obsolete]
    public static void add(Asset asset, bool overrideExistingID)
    {
        AddToMapping(asset, overrideExistingID, defaultAssetMapping);
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

    [Obsolete("Please use the method which takes a List instead.")]
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

    [Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
    public static void reportError(Asset offendingAsset, string error)
    {
        reportError(offendingAsset, error);
    }

    [Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
    public static void reportError(Asset offendingAsset, string format, params object[] args)
    {
        string error = string.Format(format, args);
        reportError(offendingAsset, error);
    }

    [Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
    public static void reportError(Asset offendingAsset, string format, object arg0)
    {
        string error = string.Format(format, arg0);
        reportError(offendingAsset, error);
    }

    [Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
    public static void reportError(Asset offendingAsset, string format, object arg0, object arg1)
    {
        string error = string.Format(format, arg0, arg1);
        reportError(offendingAsset, error);
    }

    [Obsolete("Renamed to ReportError with an IAssetErrorContext parameter")]
    public static void reportError(Asset offendingAsset, string format, object arg0, object arg1, object arg2)
    {
        string error = string.Format(format, arg0, arg1, arg2);
        reportError(offendingAsset, error);
    }
}
