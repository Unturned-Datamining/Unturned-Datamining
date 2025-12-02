using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned;

public class PlayerCrafting : PlayerCaller
{
    private const byte SAVEDATA_VERSION_BLUEPRINT_IGNORE_BY_GUID = 2;

    private const byte SAVEDATA_VERSION_ADDED_BLUEPRINT_PREFERENCES = 3;

    private const byte SAVEDATA_VERSION_NEWEST = 3;

    private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

    private static InventorySearchQualityDescendingComparator qualityDescendingComparator = new InventorySearchQualityDescendingComparator();

    private static InventorySearchAmountAscendingComparator amountAscendingComparator = new InventorySearchAmountAscendingComparator();

    private static InventorySearchAmountDescendingComparator amountDescendingComparator = new InventorySearchAmountDescendingComparator();

    private static Comparison<PlayerInventorySearchResultV2> qualityAscendingComparison = qualityAscendingComparator.Compare;

    private static Comparison<PlayerInventorySearchResultV2> qualityDescendingComparison = qualityDescendingComparator.Compare;

    private static Comparison<PlayerInventorySearchResultV2> amountAscendingComparison = amountAscendingComparator.Compare;

    private static Comparison<PlayerInventorySearchResultV2> amountDescendingComparison = amountDescendingComparator.Compare;

    [Obsolete("Use the static onCraftBlueprintRequested for ease-of-use instead.")]
    public PlayerCraftingRequestHandler onCraftingRequested;

    [Obsolete("Please use V2 which takes a reference to the underlying blueprint")]
    public static PlayerCraftingRequestHandler onCraftBlueprintRequested;

    public static PlayerCraftingRequestHandlerV2 OnCraftBlueprintRequestedV2;

    public CraftingUpdated onCraftingUpdated;

    private static Comparison<NearbyCraftingTagProvider> localPlayerNearbyTagProvidersComparison = CompareLocalPlayerNearbyTagProviders;

    private static readonly ServerInstanceMethod<byte, byte, byte> SendStripAttachments = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerCrafting), "ReceiveStripAttachments");

    private static readonly ClientInstanceMethod SendRefreshCrafting = ClientInstanceMethod.Get(typeof(PlayerCrafting), "ReceiveRefreshCrafting");

    private static readonly ServerInstanceMethod<Guid, byte, bool> SendCraft = ServerInstanceMethod<Guid, byte, bool>.Get(typeof(PlayerCrafting), "ReceiveCraft");

    internal static System.Action OnLocalPlayerBlueprintPreferencesChanged;

    private static Dictionary<Guid, List<BlueprintPreferencesPair>> localPlayerBlueprintPreferences = new Dictionary<Guid, List<BlueprintPreferencesPair>>();

    private static int ignoredBlueprintsCount;

    private static int favoritedBlueprintsCount;

    private static bool isLoadingBlueprintPreferences;

    /// <summary>
    /// Why isn't tags list public visibility? Because if adding features to (for example) consume a resource when
    /// crafting tag provider is used that will require an API change.
    /// </summary>
    private HashSet<TagAsset> nearbyCraftingTags = new HashSet<TagAsset>();

    internal static List<NearbyCraftingTagProvider> localPlayerNearbyTagProviders = new List<NearbyCraftingTagProvider>();

    private static HashSet<ICraftingTagProvider> tempTagProviders = new HashSet<ICraftingTagProvider>();

    private static HashSet<TagAsset> tempTags = new HashSet<TagAsset>();

    private static Stack<HashSet<TagAsset>> tagPool = new Stack<HashSet<TagAsset>>();

    private static BlueprintStatus activeBlueprintStatus = new BlueprintStatus();

    private static CustomSampler updateBlueprintDynamicStatusSampler = CustomSampler.Create("UpdateBlueprintDynamicStatus");

    public static bool HasIgnoredAnyBlueprints => ignoredBlueprintsCount > 0;

    public static bool HasFavoritedAnyBlueprints => favoritedBlueprintsCount > 0;

    [Obsolete("Removed from dedicated server builds and made static")]
    public bool IsIgnoringAnyBlueprints => localPlayerBlueprintPreferences.Count > 0;

    /// <summary>
    /// Find nearby crafting tag providers and query their tags.
    /// </summary>
    public void UpdateAvailableCraftingTags()
    {
        Vector3 position = base.transform.position + Vector3.up;
        float radius = 8f;
        nearbyCraftingTags.Clear();
        if (!base.channel.IsLocalPlayer)
        {
            CraftingTagPhysicsUtil.QueryAvailableTags(position, radius, nearbyCraftingTags);
            return;
        }
        foreach (NearbyCraftingTagProvider localPlayerNearbyTagProvider in localPlayerNearbyTagProviders)
        {
            tagPool.Push(localPlayerNearbyTagProvider.tags);
        }
        localPlayerNearbyTagProviders.Clear();
        tempTagProviders.Clear();
        CraftingTagPhysicsUtil.QueryTagProviders(position, radius, tempTagProviders);
        CraftingTagProviderGetAvailableTagsParameters p = default(CraftingTagProviderGetAvailableTagsParameters);
        foreach (ICraftingTagProvider tempTagProvider in tempTagProviders)
        {
            Asset tagProviderAsset = tempTagProvider.GetTagProviderAsset();
            if (tagProviderAsset == null)
            {
                if (tempTagProvider is Component component)
                {
                    UnturnedLog.warn("Crafting tag provider without asset: " + component.GetSceneHierarchyPath());
                }
                else
                {
                    UnturnedLog.warn($"Crafting tag provider without asset: {tempTagProvider}");
                }
                continue;
            }
            tempTags.Clear();
            p.ResultTags = tempTags;
            tempTagProvider.GetAvailableTags(ref p);
            if (tempTags.Count <= 0)
            {
                continue;
            }
            foreach (TagAsset tempTag in tempTags)
            {
                nearbyCraftingTags.Add(tempTag);
            }
            NearbyCraftingTagProvider nearbyCraftingTagProvider = default(NearbyCraftingTagProvider);
            nearbyCraftingTagProvider.component = tempTagProvider;
            nearbyCraftingTagProvider.asset = tagProviderAsset;
            nearbyCraftingTagProvider.tags = tempTags;
            NearbyCraftingTagProvider item = nearbyCraftingTagProvider;
            if (!localPlayerNearbyTagProviders.Contains(item))
            {
                localPlayerNearbyTagProviders.Add(item);
                if (!tagPool.TryPop(out tempTags))
                {
                    tempTags = new HashSet<TagAsset>();
                }
            }
        }
        localPlayerNearbyTagProviders.Sort(localPlayerNearbyTagProvidersComparison);
    }

    private static int CompareLocalPlayerNearbyTagProviders(NearbyCraftingTagProvider lhs, NearbyCraftingTagProvider rhs)
    {
        return lhs.asset.FriendlyName.CompareTo(rhs.asset.FriendlyName);
    }

    /// <summary>
    /// Tests whether nearby tags include specified tag.
    /// Doesn't update nearby tags, so call UpdateAvailableCraftingTags if out-of-date.
    /// </summary>
    public bool IsCraftingTagAvailable(TagAsset tag)
    {
        if (tag == null)
        {
            return false;
        }
        return nearbyCraftingTags.Contains(tag);
    }

    public bool isBlueprintBlacklisted(Blueprint blueprint)
    {
        return Level.getAsset()?.isBlueprintBlacklisted(blueprint) ?? false;
    }

    private bool stripAttachments(byte page, ItemJar jar)
    {
        ItemAsset asset = jar.GetAsset();
        if (asset != null && asset.type == EItemType.GUN && jar.item.state != null && jar.item.state.Length == 18)
        {
            if (((ItemGunAsset)asset).hasSight)
            {
                ushort num = BitConverter.ToUInt16(jar.item.state, 0);
                if (num != 0 && num != ((ItemGunAsset)asset).sightID)
                {
                    base.player.inventory.forceAddItem(new Item(num, full: false, jar.item.state[13]), auto: true);
                    jar.item.state[0] = 0;
                    jar.item.state[1] = 0;
                    jar.item.state[13] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasTactical)
            {
                ushort num2 = BitConverter.ToUInt16(jar.item.state, 2);
                if (num2 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num2, full: false, jar.item.state[14]), auto: true);
                    jar.item.state[2] = 0;
                    jar.item.state[3] = 0;
                    jar.item.state[14] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasGrip)
            {
                ushort num3 = BitConverter.ToUInt16(jar.item.state, 4);
                if (num3 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num3, full: false, jar.item.state[15]), auto: true);
                    jar.item.state[4] = 0;
                    jar.item.state[5] = 0;
                    jar.item.state[15] = 0;
                }
            }
            if (((ItemGunAsset)asset).hasBarrel)
            {
                ushort num4 = BitConverter.ToUInt16(jar.item.state, 6);
                if (num4 != 0)
                {
                    base.player.inventory.forceAddItem(new Item(num4, full: false, jar.item.state[16]), auto: true);
                    jar.item.state[6] = 0;
                    jar.item.state[7] = 0;
                    jar.item.state[16] = 0;
                }
            }
            if (((ItemGunAsset)asset).allowMagazineChange)
            {
                ushort num5 = BitConverter.ToUInt16(jar.item.state, 8);
                if (num5 != 0 && jar.item.state[10] > 0)
                {
                    base.player.inventory.forceAddItem(new Item(num5, jar.item.state[10], jar.item.state[17]), auto: true);
                    jar.item.state[8] = 0;
                    jar.item.state[9] = 0;
                    jar.item.state[10] = 0;
                    jar.item.state[17] = 0;
                }
            }
            return true;
        }
        return false;
    }

    public void removeItem(byte page, ItemJar jar)
    {
        base.player.inventory.removeItem(page, base.player.inventory.getIndex(page, jar.x, jar.y));
        stripAttachments(page, jar);
    }

    [Obsolete]
    public void askStripAttachments(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveStripAttachments(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askStripAttachments")]
    public void ReceiveStripAttachments(byte page, byte x, byte y)
    {
        if (page < PlayerInventory.SLOTS || page >= PlayerInventory.PAGES - 1)
        {
            return;
        }
        if (base.player.equipment.checkSelection(page, x, y))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            if (item != null && stripAttachments(page, item))
            {
                base.player.inventory.sendUpdateInvState(page, x, y, item.item.state);
            }
        }
    }

    public void sendStripAttachments(byte page, byte x, byte y)
    {
        SendStripAttachments.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
    }

    [Obsolete]
    public void tellCraft(CSteamID steamID)
    {
        ReceiveRefreshCrafting();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellCraft")]
    public void ReceiveRefreshCrafting()
    {
        onCraftingUpdated?.Invoke();
    }

    /// <summary>
    /// Requested for plugin use.
    /// Notifies owner they should refresh the crafting menu.
    /// </summary>
    public void ServerRefreshOwnerCrafting()
    {
        SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
    }

    internal bool IsBlueprintPermanentlyDisabled(Blueprint blueprint)
    {
        if (isBlueprintBlacklisted(blueprint))
        {
            return true;
        }
        if (blueprint.GetLegacyBlueprintSkill() == EBlueprintSkill.REPAIR && blueprint.level > Provider.modeConfigData.Gameplay.Repair_Level_Max)
        {
            return true;
        }
        if (!string.IsNullOrEmpty(blueprint.map) && !blueprint.map.Equals(Level.info.name, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        if (!Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables && !Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables_On_Vehicles && blueprint.IsOutputFreeformBuildable)
        {
            return true;
        }
        if (blueprint.Operation != 0 && (blueprint.TargetItem == null || blueprint.TargetItem.FindItemAsset() == null))
        {
            return true;
        }
        if (blueprint.RequiresStaticTags != null)
        {
            CachingAssetRef[] requiresStaticTags = blueprint.RequiresStaticTags;
            if (requiresStaticTags != null)
            {
                for (int i = 0; i < requiresStaticTags.Length; i++)
                {
                    TagAsset tagAsset = requiresStaticTags[i].Get<TagAsset>();
                    if (tagAsset == null)
                    {
                        return true;
                    }
                    if (!Level.IsTagEnabled(tagAsset))
                    {
                        UnturnedLog.info($"Cannot craft blueprint {blueprint} because tag {tagAsset} is unavailable");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Update anything that will not change as blueprint is invoked repeatedly on server.
    /// </summary>
    internal void UpdateBlueprintStaticStatus(in UpdateBlueprintStatusParameters p, bool bypassWorkstationRequirements)
    {
        Blueprint blueprint = p.status.blueprint;
        if (blueprint.RequiresSkill)
        {
            int playerSkillLevel = blueprint.GetPlayerSkillLevel(base.player);
            if (playerSkillLevel < blueprint.level)
            {
                p.status.isMissingRequiredSkill = true;
                p.logCallback?.Invoke($"skill {blueprint.DebugGetSkillName()} level {playerSkillLevel}) is less than required {blueprint.level}");
                if (p.shouldExitEarly)
                {
                    return;
                }
            }
        }
        if (bypassWorkstationRequirements)
        {
            return;
        }
        CachingAssetRef[] applicableRequiredNearbyCraftingTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
        if (applicableRequiredNearbyCraftingTags == null)
        {
            return;
        }
        for (int i = 0; i < applicableRequiredNearbyCraftingTags.Length; i++)
        {
            TagAsset tagAsset = applicableRequiredNearbyCraftingTags[i].Get<TagAsset>();
            if (tagAsset != null && !IsCraftingTagAvailable(tagAsset))
            {
                p.status.missingCraftingTagsCount++;
                p.logCallback?.Invoke("requires nearby crafting tag \"" + tagAsset.PlainTextName + "\"");
                if (p.shouldExitEarly)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Update anything that can change as blueprint is invoked repeatedly on server.
    /// </summary>
    internal void UpdateBlueprintDynamicStatus(in UpdateBlueprintStatusParameters p)
    {
        Blueprint blueprint = p.status.blueprint;
        if (!blueprint.areConditionsMet(base.player))
        {
            p.status.isMissingAnyNpcConditions = true;
            p.logCallback?.Invoke("NPC conditions not met");
            if (p.shouldExitEarly)
            {
                return;
            }
        }
        PlayerInventorySearchResultV2? playerInventorySearchResultV = null;
        if (blueprint.TargetItem != null)
        {
            BlueprintInputItemStatus blueprintInputItemStatus = p.status.AddTargetItem();
            UpdateBlueprintInputItemStatus(in p, blueprint.TargetItem, blueprintInputItemStatus, null);
            if (blueprintInputItemStatus.isMissingRequiredAmount)
            {
                p.status.isMissingTargetItem = true;
                p.logCallback?.Invoke("missing target item");
                if (p.shouldExitEarly)
                {
                    return;
                }
            }
            playerInventorySearchResultV = blueprintInputItemStatus.FirstResultOrNull;
        }
        BlueprintSupply[] supplies = blueprint.supplies;
        int num = ((supplies != null) ? supplies.Length : 0);
        ItemJar ignoreTargetItem = playerInventorySearchResultV?.Jar;
        for (int i = 0; i < num; i++)
        {
            BlueprintSupply inputItemConfig = blueprint.supplies[i];
            BlueprintInputItemStatus inputStatus = p.status.AddInputItem();
            if (UpdateBlueprintInputItemStatus(in p, inputItemConfig, inputStatus, ignoreTargetItem) && p.shouldExitEarly)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns true if should exit early.
    /// If updating behavior here please remember to update <see cref="M:SDG.Unturned.PlayerCrafting.GatherUniqueInputItems(System.Collections.Generic.HashSet{SDG.Unturned.ItemAsset})" />.
    /// </summary>
    private bool UpdateBlueprintInputItemStatus(in UpdateBlueprintStatusParameters p, BlueprintSupply inputItemConfig, BlueprintInputItemStatus inputStatus, ItemJar ignoreTargetItem)
    {
        _ = p.status.blueprint;
        ItemAsset itemAsset = inputItemConfig.FindItemAsset();
        if (itemAsset == null)
        {
            p.status.totalMissingInputItemsCount += inputItemConfig.amount;
            p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
            inputStatus.isMissingRequiredAmount = true;
            p.logCallback?.Invoke($"no asset for input item {inputItemConfig.ItemRef}");
            return true;
        }
        PlayerInventorySearchParameters playerInventorySearchParameters = default(PlayerInventorySearchParameters);
        playerInventorySearchParameters.Results = inputStatus.searchResults;
        playerInventorySearchParameters.IncludeEquipmentSlots = !inputItemConfig.ShouldConsume;
        playerInventorySearchParameters.IncludeActiveStorageContainer = !inputItemConfig.ShouldConsume;
        playerInventorySearchParameters.AssetRef = inputItemConfig.ItemRef;
        playerInventorySearchParameters.IncludeEmpty = inputItemConfig.ShouldIncludeEmptyAmount;
        playerInventorySearchParameters.ExcludeFullAmount = inputItemConfig.ShouldExcludeFullAmount;
        playerInventorySearchParameters.IncludeMaxQuality = inputItemConfig.ShouldIncludeMaxQuality;
        playerInventorySearchParameters.ItemToIgnore = ignoreTargetItem;
        PlayerInventorySearchParameters parameters = playerInventorySearchParameters;
        base.player.inventory.SearchContents(in parameters);
        if (inputStatus.searchResults.Count < 1)
        {
            p.status.totalMissingInputItemsCount += inputItemConfig.amount;
            p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
            inputStatus.isMissingRequiredAmount = true;
            p.logCallback?.Invoke($"no results for supply item {itemAsset}");
            return true;
        }
        p.status.hasAnyInputItem = true;
        switch (inputItemConfig.CountingMethod)
        {
        case ECraftingInputCountingMethod.TotalItems:
            inputStatus.totalAmount = inputStatus.searchResults.Count;
            break;
        case ECraftingInputCountingMethod.TotalAmount:
            foreach (PlayerInventorySearchResultV2 searchResult in inputStatus.searchResults)
            {
                inputStatus.totalAmount += (inputItemConfig.ShouldCountEmptyAsOne ? Mathf.Max(1, searchResult.Jar.item.amount) : searchResult.Jar.item.amount);
            }
            break;
        default:
            UnturnedLog.warn($"unhandled crafting input counting method ({inputItemConfig.CountingMethod})");
            return true;
        }
        if (inputStatus.totalAmount < inputItemConfig.amount)
        {
            p.status.totalMissingInputItemsCount += inputItemConfig.amount - inputStatus.totalAmount;
            p.status.isMissingAnyCriticalInputItem |= inputItemConfig.isCritical;
            inputStatus.isMissingRequiredAmount = true;
            p.logCallback?.Invoke($"input item ({itemAsset}) x{inputStatus.totalAmount} less than required {inputItemConfig.amount}");
            if (p.shouldExitEarly)
            {
                return true;
            }
        }
        if (!inputItemConfig.ShouldConsume)
        {
            inputStatus.totalAmount = Mathf.Min(inputStatus.totalAmount, inputItemConfig.amount);
        }
        switch (inputItemConfig.Prioritization)
        {
        case ECraftingInputPrioritization.LowestAmount:
            inputStatus.searchResults.Sort(amountAscendingComparison);
            break;
        case ECraftingInputPrioritization.HighestAmount:
            inputStatus.searchResults.Sort(amountDescendingComparison);
            break;
        case ECraftingInputPrioritization.LowestQuality:
            inputStatus.searchResults.Sort(qualityAscendingComparison);
            break;
        case ECraftingInputPrioritization.HighestQuality:
            inputStatus.searchResults.Sort(qualityDescendingComparison);
            break;
        default:
            UnturnedLog.warn($"unhandled crafting input prioritization ({inputItemConfig.Prioritization})");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Find all item assets available to the player for crafting.
    /// Used to more quickly identify blueprints that might be craftable, rather than testing all blueprints.
    /// If updating behavior here please remember to update <see cref="M:SDG.Unturned.PlayerCrafting.UpdateBlueprintInputItemStatus(SDG.Unturned.UpdateBlueprintStatusParameters@,SDG.Unturned.BlueprintSupply,SDG.Unturned.BlueprintInputItemStatus,SDG.Unturned.ItemJar)" />.
    /// </summary>
    internal void GatherUniqueInputItems(HashSet<ItemAsset> results)
    {
        for (int i = 0; i <= PlayerInventory.STORAGE; i++)
        {
            base.player.inventory.items[i]?.GatherUniqueItems(results);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceiveCraft(in ServerInvocationContext context, Guid assetGuid, byte index, bool asManyAsPossible)
    {
        Asset asset = Assets.find(assetGuid);
        if (asset == null)
        {
            return;
        }
        ushort id = asset.id;
        ushort itemID = id;
        bool shouldAllow = true;
        if (onCraftBlueprintRequested != null)
        {
            onCraftBlueprintRequested(this, ref itemID, ref index, ref shouldAllow);
        }
        else
        {
            onCraftingRequested?.Invoke(this, ref itemID, ref index, ref shouldAllow);
        }
        if (!shouldAllow)
        {
            return;
        }
        if (itemID != id)
        {
            asset = Assets.find(EAssetType.ITEM, itemID);
            if (asset == null)
            {
                return;
            }
        }
        if (asset is IBlueprintOwner blueprintOwner)
        {
            Blueprint blueprintByIndex = blueprintOwner.GetBlueprintByIndex(index);
            if (blueprintByIndex != null)
            {
                HandleCraftRequestInternal(in context, blueprintByIndex, asManyAsPossible, playEffect: true, bypassWorkstationRequirements: false);
            }
        }
    }

    /// <summary>
    /// Allows housing planner to craft without playing effect, without also allowing
    /// cheaters to craft without playing effect. (if it were an RPC param)
    /// </summary>
    internal bool HandleCraftRequestInternal(in ServerInvocationContext context, Blueprint blueprint, bool asManyAsPossible, bool playEffect, bool bypassWorkstationRequirements)
    {
        if (!Level.IsCraftingAllowedByLevel)
        {
            return false;
        }
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        bool shouldAllow = true;
        if (OnCraftBlueprintRequestedV2 != null)
        {
            try
            {
                OnCraftBlueprintRequestedV2(this, ref blueprint, ref shouldAllow);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught plugin exception during OnCraftBlueprintRequestedV2 for {blueprint}:");
            }
        }
        if (!shouldAllow || blueprint == null)
        {
            return false;
        }
        if (IsBlueprintPermanentlyDisabled(blueprint))
        {
            return false;
        }
        if (!bypassWorkstationRequirements && blueprint.GetApplicableRequiredNearbyCraftingTags() != null)
        {
            UpdateAvailableCraftingTags();
        }
        activeBlueprintStatus.Reset();
        activeBlueprintStatus.blueprint = blueprint;
        UpdateBlueprintStatusParameters p = default(UpdateBlueprintStatusParameters);
        p.status = activeBlueprintStatus;
        p.shouldExitEarly = true;
        UpdateBlueprintStaticStatus(in p, bypassWorkstationRequirements);
        if (!activeBlueprintStatus.IsCraftable)
        {
            return false;
        }
        bool flag = false;
        for (int i = 0; i < 64; i++)
        {
            activeBlueprintStatus.ResetDynamicStatus();
            UpdateBlueprintDynamicStatus(in p);
            if (!activeBlueprintStatus.IsCraftable)
            {
                break;
            }
            PlayerInventorySearchResultV2? playerInventorySearchResultV = null;
            if (blueprint.Operation != 0)
            {
                if (blueprint.TargetItem == null)
                {
                    break;
                }
                BlueprintInputItemStatus targetStatus = activeBlueprintStatus.targetStatus;
                if (targetStatus.searchResults.Count < 1)
                {
                    break;
                }
                playerInventorySearchResultV = targetStatus.searchResults[0];
            }
            if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
            {
                PlayerInventorySearchResultV2 value = playerInventorySearchResultV.Value;
                int a = value.GetAsset().MaxAmount - value.Jar.item.amount;
                if (activeBlueprintStatus.inputItems.Count > 0)
                {
                    BlueprintInputItemStatus blueprintInputItemStatus = activeBlueprintStatus.inputItems[0];
                    a = (blueprintInputItemStatus.requiredAmountOverride = Mathf.Min(a, blueprintInputItemStatus.totalAmount));
                    base.player.inventory.sendUpdateAmount(value.Page, value.Jar.x, value.Jar.y, (byte)(value.Jar.item.amount + a));
                }
            }
            for (int j = 0; j < blueprint.supplies.Length; j++)
            {
                BlueprintSupply blueprintSupply = blueprint.supplies[j];
                if (!blueprintSupply.ShouldConsume)
                {
                    continue;
                }
                BlueprintInputItemStatus blueprintInputItemStatus2 = activeBlueprintStatus.inputItems[j];
                List<PlayerInventorySearchResultV2> searchResults = blueprintInputItemStatus2.searchResults;
                int num = ((blueprintInputItemStatus2.requiredAmountOverride > 0) ? blueprintInputItemStatus2.requiredAmountOverride : blueprintSupply.amount);
                switch (blueprintSupply.CountingMethod)
                {
                case ECraftingInputCountingMethod.TotalItems:
                    foreach (PlayerInventorySearchResultV2 item2 in searchResults)
                    {
                        item2.Delete(base.player);
                        num--;
                        if (num == 0)
                        {
                            break;
                        }
                    }
                    break;
                case ECraftingInputCountingMethod.TotalAmount:
                    foreach (PlayerInventorySearchResultV2 item3 in searchResults)
                    {
                        if (item3.Jar.item.amount == 0 && blueprintSupply.ShouldCountEmptyAsOne)
                        {
                            item3.Delete(base.player);
                            num--;
                        }
                        else
                        {
                            uint num2 = item3.DeleteAmount(base.player, (uint)num, alwaysDeleteAtZeroAmount: false);
                            num -= (int)num2;
                        }
                        if (num == 0)
                        {
                            break;
                        }
                    }
                    break;
                }
            }
            if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
            {
                PlayerInventorySearchResultV2 value2 = playerInventorySearchResultV.Value;
                base.player.inventory.sendUpdateQuality(value2.Page, value2.Jar.x, value2.Jar.y, 100);
                ItemAsset asset = value2.GetAsset();
                if (asset != null && asset.type == EItemType.REFILL && value2.Jar.item.state.Length == 1 && value2.Jar.item.state[0] == 3)
                {
                    value2.Jar.item.state[0] = 1;
                    base.player.inventory.sendUpdateInvState(value2.Page, value2.Jar.x, value2.Jar.y, value2.Jar.item.state);
                }
            }
            BlueprintOutput[] outputs = blueprint.outputs;
            foreach (BlueprintOutput blueprintOutput in outputs)
            {
                ItemAsset itemAsset = blueprintOutput.FindItemAsset();
                if (itemAsset == null)
                {
                    continue;
                }
                for (int l = 0; l < blueprintOutput.amount; l++)
                {
                    if (blueprint.transferState)
                    {
                        PlayerInventorySearchResultV2 playerInventorySearchResultV2 = p.status.inputItems[0].searchResults[0];
                        ItemAsset asset2 = playerInventorySearchResultV2.GetAsset();
                        Item item = new Item(itemAsset.id, playerInventorySearchResultV2.Jar.item.amount, playerInventorySearchResultV2.Jar.item.quality, playerInventorySearchResultV2.Jar.item.state);
                        if (asset2 != null && asset2.type == EItemType.GUN && itemAsset != null && itemAsset.type == EItemType.GUN && item.state.Length >= 12)
                        {
                            if (blueprint.withoutAttachments)
                            {
                                for (int m = 0; m < item.state.Length; m++)
                                {
                                    item.state[m] = 0;
                                }
                            }
                            if (itemAsset is ItemGunAsset itemGunAsset)
                            {
                                item.state[11] = (byte)itemGunAsset.firemode;
                            }
                        }
                        base.player.inventory.forceAddItem(item, auto: true);
                    }
                    else
                    {
                        base.player.inventory.forceAddItem(new Item(itemAsset.id, blueprintOutput.origin), auto: true);
                    }
                }
            }
            blueprint.ApplyConditions(base.player);
            blueprint.GrantRewards(base.player);
            flag = true;
            if (!asManyAsPossible || blueprint.Operation != 0)
            {
                break;
            }
        }
        if (flag)
        {
            SendRefreshCrafting.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection());
            base.player.sendStat(EPlayerStat.FOUND_CRAFTS);
            if (playEffect)
            {
                EffectAsset effectAsset = blueprint.FindBuildEffectAsset();
                if (effectAsset != null)
                {
                    TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                    parameters.position = base.transform.position;
                    parameters.relevantDistance = EffectManager.SMALL;
                    EffectManager.triggerEffect(parameters);
                    if (Provider.isServer)
                    {
                        AlertTool.alert(base.transform.position, 8f);
                    }
                }
            }
        }
        return flag;
    }

    [Obsolete("Please use SendRequestToCraft which takes a blueprint parameter")]
    public void sendCraft(ushort id, byte index, bool force)
    {
        if (Assets.find(EAssetType.ITEM, id) is ItemAsset blueprintOwner)
        {
            Blueprint blueprintByIndex = blueprintOwner.GetBlueprintByIndex(index);
            if (blueprintByIndex != null)
            {
                SendRequestToCraft(blueprintByIndex, force);
            }
        }
    }

    public void SendRequestToCraft(Blueprint blueprint, bool asManyAsPossible)
    {
        Asset ownerAsset = blueprint.GetOwnerAsset();
        if (ownerAsset == null)
        {
            UnturnedLog.warn($"Unable to craft blueprint without owner asset {blueprint}");
        }
        else
        {
            SendCraft.Invoke(GetNetId(), ENetReliability.Unreliable, ownerAsset.GUID, blueprint.Index, asManyAsPossible);
        }
    }

    /// <summary>
    /// Get local player's per-blueprint preferences.
    /// </summary>
    public static EBlueprintPreferences GetBlueprintPreferences(Blueprint blueprint)
    {
        if (blueprint == null)
        {
            return EBlueprintPreferences.None;
        }
        Asset ownerAsset = blueprint.GetOwnerAsset();
        if (ownerAsset == null)
        {
            return EBlueprintPreferences.None;
        }
        if (localPlayerBlueprintPreferences.TryGetValue(ownerAsset.GUID, out var value))
        {
            foreach (BlueprintPreferencesPair item in value)
            {
                if (item.index == blueprint.Index)
                {
                    return item.preferences;
                }
            }
        }
        return EBlueprintPreferences.None;
    }

    /// <summary>
    /// Set local player's per-blueprint preferences.
    /// This is helpful both to prevent accidentally crafting certain blueprints (like blindfolds) when click to
    /// craft is enabled, and to save frequently used blueprints.
    /// </summary>
    public static void SetBlueprintPreferences(Blueprint blueprint, EBlueprintPreferences preferences)
    {
        if (blueprint == null)
        {
            return;
        }
        Asset ownerAsset = blueprint.GetOwnerAsset();
        if (ownerAsset == null)
        {
            return;
        }
        bool flag;
        if (localPlayerBlueprintPreferences.TryGetValue(ownerAsset.GUID, out var value))
        {
            byte index = blueprint.Index;
            int num = -1;
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].index == index)
                {
                    num = i;
                    break;
                }
            }
            if (num >= 0)
            {
                EBlueprintPreferences preferences2 = value[num].preferences;
                flag = preferences != preferences2;
                if (flag)
                {
                    switch (preferences2)
                    {
                    case EBlueprintPreferences.Ignored:
                        ignoredBlueprintsCount--;
                        break;
                    case EBlueprintPreferences.Favorited:
                        favoritedBlueprintsCount--;
                        break;
                    }
                    if (preferences != 0)
                    {
                        value[num] = new BlueprintPreferencesPair
                        {
                            index = blueprint.Index,
                            preferences = preferences
                        };
                    }
                    else
                    {
                        value.RemoveAt(num);
                    }
                }
            }
            else
            {
                flag = preferences != EBlueprintPreferences.None;
                if (flag)
                {
                    value.Add(new BlueprintPreferencesPair
                    {
                        index = index,
                        preferences = preferences
                    });
                }
            }
        }
        else
        {
            flag = preferences != EBlueprintPreferences.None;
            if (flag)
            {
                value = new List<BlueprintPreferencesPair>
                {
                    new BlueprintPreferencesPair
                    {
                        index = blueprint.Index,
                        preferences = preferences
                    }
                };
                localPlayerBlueprintPreferences.Add(ownerAsset.GUID, value);
            }
        }
        if (flag)
        {
            switch (preferences)
            {
            case EBlueprintPreferences.Ignored:
                ignoredBlueprintsCount++;
                break;
            case EBlueprintPreferences.Favorited:
                favoritedBlueprintsCount++;
                break;
            }
        }
        if (!isLoadingBlueprintPreferences && flag)
        {
            OnLocalPlayerBlueprintPreferencesChanged?.Invoke();
        }
    }

    internal void InitializePlayer()
    {
        if (base.channel.IsLocalPlayer)
        {
            LoadBlueprintPreferences();
        }
    }

    private void OnDestroy()
    {
        if (base.channel.IsLocalPlayer)
        {
            SaveBlueprintPreferences();
        }
    }

    private void LoadBlueprintPreferences()
    {
        isLoadingBlueprintPreferences = true;
        localPlayerBlueprintPreferences.Clear();
        ignoredBlueprintsCount = 0;
        favoritedBlueprintsCount = 0;
        try
        {
            if (ReadWrite.fileExists("/Cloud/Ignored_Blueprints.dat", useCloud: false))
            {
                Block block = ReadWrite.readBlock("/Cloud/Ignored_Blueprints.dat", useCloud: false, 0);
                byte b = block.readByte();
                int a = block.readInt32();
                a = Mathf.Min(a, 10000);
                if (b >= 2)
                {
                    for (int i = 0; i < a; i++)
                    {
                        IBlueprintOwner blueprintOwner = Assets.Find_UseDefaultAssetMapping(block.readGUID()) as IBlueprintOwner;
                        int num = block.readInt32();
                        for (int j = 0; j < num; j++)
                        {
                            byte index = block.readByte();
                            EBlueprintPreferences preferences = (EBlueprintPreferences)((b < 3) ? 1 : block.readByte());
                            if (blueprintOwner != null)
                            {
                                Blueprint blueprintByIndex = blueprintOwner.GetBlueprintByIndex(index);
                                if (blueprintByIndex != null)
                                {
                                    SetBlueprintPreferences(blueprintByIndex, preferences);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < a; k++)
                    {
                        ushort num2 = block.readUInt16();
                        byte index2 = block.readByte();
                        if (num2 != 0 && Assets.find(EAssetType.ITEM, num2) is ItemAsset blueprintOwner2)
                        {
                            Blueprint blueprintByIndex2 = blueprintOwner2.GetBlueprintByIndex(index2);
                            if (blueprintByIndex2 != null)
                            {
                                SetBlueprintPreferences(blueprintByIndex2, EBlueprintPreferences.Ignored);
                            }
                        }
                    }
                }
                OnLocalPlayerBlueprintPreferencesChanged?.Invoke();
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception loading ignored blueprints:");
        }
        isLoadingBlueprintPreferences = false;
    }

    private void SaveBlueprintPreferences()
    {
        Block block = new Block();
        block.writeByte(3);
        block.writeInt32(localPlayerBlueprintPreferences.Count);
        foreach (KeyValuePair<Guid, List<BlueprintPreferencesPair>> localPlayerBlueprintPreference in localPlayerBlueprintPreferences)
        {
            block.writeGUID(localPlayerBlueprintPreference.Key);
            block.writeInt32(localPlayerBlueprintPreference.Value.Count);
            foreach (BlueprintPreferencesPair item in localPlayerBlueprintPreference.Value)
            {
                block.writeByte(item.index);
                block.writeByte((byte)item.preferences);
            }
        }
        ReadWrite.writeBlock("/Cloud/Ignored_Blueprints.dat", useCloud: false, block);
    }

    [Obsolete]
    public void askCraft(CSteamID steamID, ushort id, byte index, bool force)
    {
    }

    [Obsolete("Should not have been called externally to begin with")]
    public void ReceiveCraft(in ServerInvocationContext context, ushort id, byte index, bool force)
    {
        if (Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset)
        {
            ReceiveCraft(in context, itemAsset.GUID, index, force);
        }
    }

    [Obsolete("Removed from dedicated server builds and made static")]
    public bool getIgnoringBlueprint(Blueprint blueprint)
    {
        return GetBlueprintPreferences(blueprint) == EBlueprintPreferences.Ignored;
    }

    [Obsolete("Removed from dedicated server builds and made static")]
    public void setIgnoringBlueprint(Blueprint blueprint, bool isIgnoring)
    {
        SetBlueprintPreferences(blueprint, isIgnoring ? EBlueprintPreferences.Ignored : EBlueprintPreferences.None);
    }
}
