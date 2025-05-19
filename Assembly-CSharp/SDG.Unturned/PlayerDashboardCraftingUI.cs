using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class PlayerDashboardCraftingUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static Bundle icons;

    public static bool active;

    private static ISleekBox backdropBox;

    private static ISleekField searchField;

    private static ISleekButton searchButton;

    /// <summary>
    /// List of all loaded blueprints potentially craftable by player. Updated when assets are refreshed. This
    /// allows us to skip blueprints that will never be craftable (such as level-specific blueprints).
    /// </summary>
    private static List<Blueprint> loadedBlueprints;

    private static int assetListChangeCounter;

    /// <summary>
    /// Recycled list of assets with blueprints.
    /// </summary>
    private static List<IBlueprintOwner> blueprintOwners = new List<IBlueprintOwner>();

    /// <summary>
    /// Subset of loadedBlueprints.
    /// </summary>
    private static List<Blueprint> filteredBlueprints = new List<Blueprint>();

    private static List<BlueprintStatus> visibleBlueprints;

    /// <summary>
    /// Center column.
    /// </summary>
    private static ISleekElement blueprintsContainer;

    private static SleekButtonIcon filteringDescriptionButton;

    private static SleekList<BlueprintStatus> blueprintsScrollBox;

    private static Stack<SleekBlueprint> pooledBlueprintWidgets;

    private static ISleekBox blueprintsListEmptyInfoBox;

    private static ISleekButton resetFiltersButton;

    private static ISleekToggle hideUncraftableToggle;

    private static ISleekToggle showIgnoredToggle;

    private static SleekButtonIcon favoritesButton;

    /// <summary>
    /// Used by inventory item context menu to override which blueprints are shown.
    /// </summary>
    public static Blueprint[] filteredBlueprintsOverride;

    private static HashSet<TagAsset> filterAnyOfCategories;

    private static HashSet<TagAsset> filterRequiresAnyOfTags;

    private static ICraftingTagProvider filterTagProvider;

    private static List<BlueprintStatus> updatedBlueprints;

    private static List<BlueprintStatus> blueprintStatusPool;

    private static bool hideUncraftable;

    private static bool showIgnored;

    private static bool filterFavorites;

    private static string itemNameFilter;

    /// <summary>
    /// Left-hand column.
    /// </summary>
    private static ISleekScrollView filtersScrollView;

    private static ISleekElement categoriesContainer;

    private static ISleekLabel categoriesHeader;

    private static List<SleekTagButton> categoryTagButtons;

    private static ISleekElement tagProvidersContainer;

    private static ISleekLabel tagProvidersHeader;

    private static List<SleekCraftingTagProviderButton> tagProviderButtons;

    /// <summary>
    /// Right-hand column.
    /// </summary>
    private static SleekSelectedBlueprint selectedBlueprintMenu;

    private static HashSet<ItemAsset> availableItemAssets = new HashSet<ItemAsset>();

    private static StringBuilder filteringDescriptionSb = new StringBuilder();

    private static StringBuilder filteringDescriptionCategoriesSb = new StringBuilder();

    private static Comparison<BlueprintStatus> visibleBlueprintsComparison = CompareVisibleBlueprints;

    private static CustomSampler refreshCraftableBlueprintsSampler = CustomSampler.Create("RefreshCraftableBlueprints");

    private static void SetSelectedBlueprintStatus(BlueprintStatus status)
    {
        selectedBlueprintMenu.IsVisible = status != null;
        selectedBlueprintMenu.SetSelectedBlueprintStatus(status);
        blueprintsContainer.SizeOffset_X = (selectedBlueprintMenu.IsVisible ? (-500) : (-260));
    }

    public static void open()
    {
        if (!active)
        {
            active = true;
            RefreshBlueprintList();
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            filteredBlueprintsOverride = null;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    internal static void BuildNotCraftableTooltip(StringBuilder craftTooltipBuilder, BlueprintStatus status)
    {
        craftTooltipBuilder.AppendLine(localization.format("NotCraftable_Header"));
        Blueprint blueprint = status.blueprint;
        if (status.isMissingTargetItem && blueprint.TargetItem != null)
        {
            ItemAsset itemAsset = blueprint.TargetItem.FindItemAsset();
            if (itemAsset != null)
            {
                craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
                craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingInputItem"), itemAsset.itemName);
                craftTooltipBuilder.AppendLine();
            }
        }
        if (status.totalMissingInputItemsCount > 0 && !blueprint.supplies.IsNullOrEmpty())
        {
            for (int i = 0; i < blueprint.supplies.Length; i++)
            {
                BlueprintSupply blueprintSupply = blueprint.supplies[i];
                if (status.inputItems[i].isMissingRequiredAmount)
                {
                    ItemAsset itemAsset2 = blueprintSupply.FindItemAsset();
                    if (itemAsset2 != null)
                    {
                        craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
                        craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingInputItem"), itemAsset2.itemName);
                        craftTooltipBuilder.AppendLine();
                    }
                }
            }
        }
        if (status.isMissingRequiredSkill)
        {
            craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
            craftTooltipBuilder.AppendLine(localization.format("NotCraftable_MissingSkill"));
        }
        if (status.missingCraftingTagsCount > 0)
        {
            CachingAssetRef[] applicableRequiredNearbyCraftingTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
            if (applicableRequiredNearbyCraftingTags != null)
            {
                for (int j = 0; j < applicableRequiredNearbyCraftingTags.Length; j++)
                {
                    TagAsset tagAsset = applicableRequiredNearbyCraftingTags[j].Get<TagAsset>();
                    if (tagAsset != null && !Player.player.crafting.IsCraftingTagAvailable(tagAsset))
                    {
                        craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
                        craftTooltipBuilder.AppendFormat(localization.format("NotCraftable_MissingCraftingTag"), tagAsset.PlainTextName);
                        craftTooltipBuilder.AppendLine();
                    }
                }
            }
        }
        if (status.isMissingAnyNpcConditions)
        {
            craftTooltipBuilder.Append(localization.format("NotCraftable_LineItemPrefix"));
            craftTooltipBuilder.AppendLine(localization.format("NotCraftable_UnmetConditions"));
        }
    }

    private static bool DoesAnyItemNameContainString(Blueprint blueprint)
    {
        string value = itemNameFilter;
        for (byte b = 0; b < blueprint.outputs.Length; b++)
        {
            ItemAsset itemAsset = blueprint.outputs[b].FindItemAsset();
            if (itemAsset != null && itemAsset.itemName != null && itemAsset.itemName.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        for (byte b2 = 0; b2 < blueprint.supplies.Length; b2++)
        {
            ItemAsset itemAsset2 = blueprint.supplies[b2].FindItemAsset();
            if (itemAsset2 != null && itemAsset2.itemName != null && itemAsset2.itemName.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        ItemAsset itemAsset3 = blueprint.TargetItem?.FindItemAsset();
        if (itemAsset3 != null && !string.IsNullOrEmpty(itemAsset3.itemName) && itemAsset3.itemName.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if all filtered blueprints are craftable. (hacked-in for item action menu)
    /// </summary>
    public static bool UpdateFilteredBlueprintsAndGetAreAllCraftable()
    {
        RefreshBlueprintList();
        foreach (BlueprintStatus updatedBlueprint in updatedBlueprints)
        {
            if (!updatedBlueprint.IsCraftable)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// If asset mapping has changed, find all assets with blueprints and gather the ones that can ever be crafted
    /// on this level. (I.e., excluding ones that we shouldn't waste time considering.)
    /// </summary>
    private static void RefreshLoadedBlueprintsIfNecessary()
    {
        if (!Assets.HasCurrentAssetMappingChanged(ref assetListChangeCounter))
        {
            return;
        }
        loadedBlueprints.Clear();
        HashSet<TagAsset> hashSet = new HashSet<TagAsset>();
        blueprintOwners.Clear();
        Assets.find(blueprintOwners);
        PlayerCrafting crafting = Player.player.crafting;
        foreach (IBlueprintOwner blueprintOwner in blueprintOwners)
        {
            foreach (Blueprint blueprint in blueprintOwner.GetBlueprints())
            {
                if (!crafting.IsBlueprintPermanentlyDisabled(blueprint))
                {
                    loadedBlueprints.Add(blueprint);
                    TagAsset categoryTag = blueprint.GetCategoryTag();
                    if (categoryTag != null)
                    {
                        hashSet.Add(categoryTag);
                    }
                }
            }
        }
        RefreshCategoryTagButtons(hashSet);
    }

    private static void RefreshCraftableBlueprints()
    {
        availableItemAssets.Clear();
        Player.player.crafting.GatherUniqueInputItems(availableItemAssets);
        foreach (Blueprint loadedBlueprint in loadedBlueprints)
        {
            if (loadedBlueprint.ContainsAnyOfItems(availableItemAssets))
            {
                filteredBlueprints.Add(loadedBlueprint);
            }
        }
    }

    private static void RefreshCategoryTagButtons(HashSet<TagAsset> allCategoryTags)
    {
        List<TagAsset> list = allCategoryTags.ToList();
        list.Sort(CompareCategoryTags);
        categoriesContainer.IsVisible = list.Count > 0;
        if (!categoriesContainer.IsVisible)
        {
            return;
        }
        int i;
        for (i = 0; i < list.Count; i++)
        {
            SleekTagButton sleekTagButton;
            if (i < categoryTagButtons.Count)
            {
                sleekTagButton = categoryTagButtons[i];
                sleekTagButton.IsVisible = true;
            }
            else
            {
                sleekTagButton = new SleekTagButton();
                sleekTagButton.SizeOffset_X = 50f;
                sleekTagButton.SizeOffset_Y = 50f;
                sleekTagButton.OnClicked += OnClickedCategoryFilterButton;
                sleekTagButton.TooltipAppendedText = "\n\n" + localization.format("CombineFiltersTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.modify));
                categoriesContainer.AddChild(sleekTagButton);
                categoryTagButtons.Add(sleekTagButton);
            }
            sleekTagButton.TagRef = list[i];
            sleekTagButton.PositionScale_X = 0.5f;
            sleekTagButton.PositionOffset_X = -100 + i % 4 * 50;
            sleekTagButton.PositionOffset_Y = 40 + i / 4 * 50;
        }
        categoriesContainer.SizeOffset_Y = 40 + MathfEx.GetPageCount(list.Count, 4) * 50;
        while (i < categoryTagButtons.Count)
        {
            categoryTagButtons[i].IsVisible = false;
        }
    }

    private static void RefreshTagProviderButtons()
    {
        tagProvidersContainer.IsVisible = PlayerCrafting.localPlayerNearbyTagProviders.Count > 0;
        if (!tagProvidersContainer.IsVisible)
        {
            return;
        }
        int i = 0;
        foreach (NearbyCraftingTagProvider localPlayerNearbyTagProvider in PlayerCrafting.localPlayerNearbyTagProviders)
        {
            SleekCraftingTagProviderButton sleekCraftingTagProviderButton;
            if (i < tagProviderButtons.Count)
            {
                sleekCraftingTagProviderButton = tagProviderButtons[i];
                sleekCraftingTagProviderButton.IsVisible = true;
            }
            else
            {
                sleekCraftingTagProviderButton = new SleekCraftingTagProviderButton();
                sleekCraftingTagProviderButton.SizeScale_X = 1f;
                sleekCraftingTagProviderButton.SizeOffset_Y = 50f;
                sleekCraftingTagProviderButton.OnClicked += OnClickedNearbyTagProviderButton;
                tagProvidersContainer.AddChild(sleekCraftingTagProviderButton);
                tagProviderButtons.Add(sleekCraftingTagProviderButton);
            }
            sleekCraftingTagProviderButton.SetTagProvider(localPlayerNearbyTagProvider);
            sleekCraftingTagProviderButton.PositionOffset_Y = 40 + i * 50;
            i++;
        }
        tagProvidersContainer.SizeOffset_Y = 40 + i * 50;
        for (; i < tagProviderButtons.Count; i++)
        {
            tagProviderButtons[i].IsVisible = false;
        }
    }

    private static void OrganizeFiltersColumn()
    {
        float num = 100f;
        if (showIgnoredToggle.IsVisible)
        {
            showIgnoredToggle.PositionOffset_Y = num;
            num += showIgnoredToggle.SizeOffset_Y;
        }
        if (favoritesButton.IsVisible)
        {
            favoritesButton.PositionOffset_Y = num;
            num += favoritesButton.SizeOffset_Y;
            num += 10f;
        }
        if (categoriesContainer.IsVisible)
        {
            categoriesContainer.PositionOffset_Y = num;
            num += categoriesContainer.SizeOffset_Y;
            num += 10f;
        }
        if (tagProvidersContainer.IsVisible)
        {
            tagProvidersContainer.PositionOffset_Y = num;
            num += tagProvidersContainer.SizeOffset_Y;
            num += 10f;
        }
        filtersScrollView.ContentSizeOffset = new Vector2(0f, num - 10f);
    }

    private static void RefreshBlueprintList()
    {
        Player.player.crafting.UpdateAvailableCraftingTags();
        RefreshTagProviderButtons();
        bool flag = !string.IsNullOrEmpty(itemNameFilter);
        bool flag2 = false;
        filteringDescriptionSb.Clear();
        if (filterFavorites)
        {
            flag2 = true;
            if (filteringDescriptionSb.Length > 0)
            {
                filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
            }
            string format = localization.format("FilteringDescription_Favorites_Format");
            string arg = "<color=" + Palette.hex(OptionsSettings.fontColor) + ">" + localization.format("FilteringDescription_Favorites_Label") + "</color>";
            filteringDescriptionSb.AppendFormat(format, arg);
        }
        if (filterAnyOfCategories != null && filterAnyOfCategories.Count > 0)
        {
            flag2 = true;
            if (filteringDescriptionSb.Length > 0)
            {
                filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
            }
            if (filterAnyOfCategories.Count == 1)
            {
                TagAsset tagAsset = filterAnyOfCategories.First();
                string format2 = localization.format("FilteringDescription_Category");
                filteringDescriptionSb.AppendFormat(format2, tagAsset?.RichTextOrPreferredFontColor);
            }
            else
            {
                filteringDescriptionCategoriesSb.Clear();
                foreach (TagAsset filterAnyOfCategory in filterAnyOfCategories)
                {
                    if (filteringDescriptionCategoriesSb.Length > 0)
                    {
                        filteringDescriptionCategoriesSb.Append(localization.format("FilteringDescription_Category_Separator"));
                    }
                    filteringDescriptionCategoriesSb.Append(filterAnyOfCategory?.RichTextOrPreferredFontColor);
                }
                string format3 = localization.format("FilteringDescription_Category_Multiple");
                filteringDescriptionSb.AppendFormat(format3, filteringDescriptionCategoriesSb);
            }
        }
        if (filterTagProvider != null)
        {
            flag2 = true;
            if (filteringDescriptionSb.Length > 0)
            {
                filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
            }
            string format4 = localization.format("FilteringDescription_TagProvider");
            Asset tagProviderAsset = filterTagProvider.GetTagProviderAsset();
            string arg2 = ((tagProviderAsset == null) ? filterTagProvider.ToString() : ((!(tagProviderAsset is ItemAsset itemAsset)) ? tagProviderAsset.FriendlyName : itemAsset.RarityRichTextName));
            filteringDescriptionSb.AppendFormat(format4, arg2);
        }
        else if (filterRequiresAnyOfTags.Count == 1)
        {
            flag2 = true;
            if (filteringDescriptionSb.Length > 0)
            {
                filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
            }
            string format5 = localization.format("FilteringDescription_Tag");
            TagAsset tagAsset2 = filterRequiresAnyOfTags.First();
            filteringDescriptionSb.AppendFormat(format5, tagAsset2.RichTextOrPreferredFontColor);
        }
        if (flag)
        {
            flag2 = true;
            if (filteringDescriptionSb.Length > 0)
            {
                filteringDescriptionSb.Append(localization.format("FilteringDescription_Separator"));
            }
            string format6 = localization.format("FilteringDescription_Name");
            string arg3 = "<color=" + Palette.hex(OptionsSettings.fontColor) + ">" + itemNameFilter + "</color>";
            filteringDescriptionSb.AppendFormat(format6, arg3);
        }
        filteringDescriptionButton.IsVisible = flag2;
        if (flag2)
        {
            filteringDescriptionButton.text = localization.format("FilteringDescription_Format", filteringDescriptionSb);
            blueprintsScrollBox.PositionOffset_Y = filteringDescriptionButton.SizeOffset_Y;
        }
        else
        {
            blueprintsScrollBox.PositionOffset_Y = 0f;
        }
        blueprintsScrollBox.SizeOffset_Y = 0f - blueprintsScrollBox.PositionOffset_Y;
        filteredBlueprints.Clear();
        if (Level.IsCraftingAllowedByLevel)
        {
            if (filteredBlueprintsOverride == null)
            {
                RefreshLoadedBlueprintsIfNecessary();
                if (flag2)
                {
                    foreach (Blueprint loadedBlueprint in loadedBlueprints)
                    {
                        if (filterFavorites && PlayerCrafting.GetBlueprintPreferences(loadedBlueprint) != EBlueprintPreferences.Favorited)
                        {
                            continue;
                        }
                        if (filterAnyOfCategories.Count > 0)
                        {
                            TagAsset categoryTag = loadedBlueprint.GetCategoryTag();
                            if (categoryTag == null || !filterAnyOfCategories.Contains(categoryTag))
                            {
                                continue;
                            }
                        }
                        if (filterRequiresAnyOfTags.Count > 0)
                        {
                            bool flag3 = false;
                            foreach (TagAsset filterRequiresAnyOfTag in filterRequiresAnyOfTags)
                            {
                                if (loadedBlueprint.DoesRequireNearbyCraftingTag(filterRequiresAnyOfTag))
                                {
                                    flag3 = true;
                                    break;
                                }
                            }
                            if (!flag3)
                            {
                                continue;
                            }
                        }
                        if (!flag || DoesAnyItemNameContainString(loadedBlueprint))
                        {
                            filteredBlueprints.Add(loadedBlueprint);
                        }
                    }
                }
                else
                {
                    RefreshCraftableBlueprints();
                }
            }
            else
            {
                filteredBlueprints.AddRange(filteredBlueprintsOverride);
            }
        }
        OrganizeFiltersColumn();
        blueprintStatusPool.AddRange(updatedBlueprints);
        updatedBlueprints.Clear();
        visibleBlueprints.Clear();
        Blueprint selectedBlueprint = selectedBlueprintMenu.SelectedBlueprint;
        BlueprintStatus selectedBlueprintStatus = null;
        bool flag4 = flag || filterFavorites;
        foreach (Blueprint filteredBlueprint in filteredBlueprints)
        {
            if (!showIgnored && PlayerCrafting.GetBlueprintPreferences(filteredBlueprint) == EBlueprintPreferences.Ignored)
            {
                continue;
            }
            BlueprintStatus blueprintStatus = CreateBlueprintStatus();
            blueprintStatus.blueprint = filteredBlueprint;
            updatedBlueprints.Add(blueprintStatus);
            UpdateBlueprintStatusParameters updateBlueprintStatusParameters = default(UpdateBlueprintStatusParameters);
            updateBlueprintStatusParameters.status = blueprintStatus;
            updateBlueprintStatusParameters.shouldExitEarly = false;
            UpdateBlueprintStatusParameters p = updateBlueprintStatusParameters;
            Player.player.crafting.UpdateBlueprintStaticStatus(in p);
            Player.player.crafting.UpdateBlueprintDynamicStatus(in p);
            if ((!hideUncraftable || blueprintStatus.IsCraftable) && ((!blueprintStatus.isMissingAnyCriticalInputItem && blueprintStatus.hasAnyInputItem) || (filteredBlueprint.canBeVisibleWhenSearchedWithoutRequiredItems && flag4)) && (!blueprintStatus.isMissingAnyNpcConditions || filteredBlueprint.CanBeVisibleWithUnmetConditions))
            {
                blueprintStatus.UpdateCraftabilityScore();
                visibleBlueprints.Add(blueprintStatus);
                if (filteredBlueprint == selectedBlueprint)
                {
                    selectedBlueprintStatus = blueprintStatus;
                }
            }
        }
        visibleBlueprints.Sort(visibleBlueprintsComparison);
        SetSelectedBlueprintStatus(selectedBlueprintStatus);
        blueprintsScrollBox.ForceRebuildElements();
        blueprintsListEmptyInfoBox.IsVisible = visibleBlueprints.Count == 0;
        if (blueprintsListEmptyInfoBox.IsVisible)
        {
            blueprintsListEmptyInfoBox.PositionOffset_Y = blueprintsScrollBox.PositionOffset_Y;
            resetFiltersButton.IsVisible = flag2;
            if (flag2)
            {
                blueprintsListEmptyInfoBox.Text = localization.format("No_Blueprints");
            }
            else if (availableItemAssets.Count < 1)
            {
                blueprintsListEmptyInfoBox.Text = localization.format("NoBlueprints_ZeroAvailableItems");
            }
            else
            {
                blueprintsListEmptyInfoBox.Text = localization.format("NoBlueprints_HasAvailableItems");
            }
        }
    }

    private static void onInventoryResized(byte page, byte newWidth, byte newHeight)
    {
        if (active)
        {
            RefreshBlueprintList();
        }
    }

    private static void onCraftingUpdated()
    {
        if (active)
        {
            RefreshBlueprintList();
        }
    }

    private static void ClearFilters()
    {
        filteredBlueprintsOverride = null;
        filterAnyOfCategories.Clear();
        filterRequiresAnyOfTags.Clear();
        filterTagProvider = null;
        searchField.Text = "";
        itemNameFilter = null;
        filterFavorites = false;
    }

    private static void OnClickedCategoryFilterButton(CachingAssetRef categoryTagRef)
    {
        filteredBlueprintsOverride = null;
        TagAsset tagAsset = categoryTagRef.Get<TagAsset>();
        if (tagAsset == null)
        {
            UnturnedLog.info("Clicked category tag is missing");
            return;
        }
        if (!filterAnyOfCategories.Remove(tagAsset))
        {
            if (!InputEx.GetKey(ControlsSettings.modify))
            {
                ClearFilters();
            }
            filterAnyOfCategories.Add(tagAsset);
        }
        RefreshBlueprintList();
    }

    private static void OnClickedNearbyTagProviderButton(ICraftingTagProvider tagProvider)
    {
        if (tagProvider == null)
        {
            UnturnedLog.info("Clicked nearby crafting tag provider has been destroyed");
            return;
        }
        filteredBlueprintsOverride = null;
        if (filterTagProvider == tagProvider)
        {
            filterTagProvider = null;
            filterRequiresAnyOfTags.Clear();
        }
        else
        {
            if (!InputEx.GetKey(ControlsSettings.modify))
            {
                ClearFilters();
            }
            filterTagProvider = tagProvider;
            filterRequiresAnyOfTags.Clear();
            CraftingTagProviderGetAvailableTagsParameters p = default(CraftingTagProviderGetAvailableTagsParameters);
            p.ResultTags = filterRequiresAnyOfTags;
            tagProvider.GetAvailableTags(ref p);
        }
        RefreshBlueprintList();
    }

    private static void onToggledHideUncraftableToggle(ISleekToggle toggle, bool state)
    {
        hideUncraftable = state;
        RefreshBlueprintList();
    }

    private static void OnShowIgnoredToggled(ISleekToggle toggle, bool state)
    {
        showIgnored = state;
        RefreshBlueprintList();
    }

    private static void OnClickedClearFilters(ISleekElement button)
    {
        ClearFilters();
        RefreshBlueprintList();
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        filteredBlueprintsOverride = null;
        if (!Input.GetKey(ControlsSettings.modify))
        {
            filterAnyOfCategories.Clear();
            filterRequiresAnyOfTags.Clear();
            filterTagProvider = null;
            filterFavorites = false;
        }
        itemNameFilter = searchField.Text;
        RefreshBlueprintList();
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        onEnteredSearchField(searchField);
    }

    private static void OnClickedFavoritesButton(ISleekElement button)
    {
        filteredBlueprintsOverride = null;
        if (filterFavorites)
        {
            filterFavorites = false;
        }
        else
        {
            if (!InputEx.GetKey(ControlsSettings.modify))
            {
                ClearFilters();
            }
            filterFavorites = true;
        }
        RefreshBlueprintList();
    }

    private static void OnClickedBlueprint(BlueprintStatus blueprintStatus)
    {
        bool key = InputEx.GetKey(ControlsSettings.SkipActionCraftingMenu);
        bool key2 = InputEx.GetKey(ControlsSettings.other);
        if ((key2 || key != OptionsSettings.ShouldClickBlueprintToCraft) && blueprintStatus.IsCraftable)
        {
            if (!Player.player.equipment.isBusy)
            {
                Player.player.crafting.SendRequestToCraft(blueprintStatus.blueprint, key2);
            }
        }
        else if (selectedBlueprintMenu.SelectedBlueprint == blueprintStatus.blueprint)
        {
            SetSelectedBlueprintStatus(null);
        }
        else
        {
            SetSelectedBlueprintStatus(blueprintStatus);
        }
    }

    private static ISleekElement onCreateBlueprint(BlueprintStatus blueprintStatus)
    {
        if (pooledBlueprintWidgets.TryPop(out var result))
        {
            result.IsVisible = true;
        }
        else
        {
            result = new SleekBlueprint();
            result.OnClickedBlueprint += OnClickedBlueprint;
        }
        result.SetBlueprintStatus(blueprintStatus);
        return result;
    }

    private static void OnRemoveBlueprintElement(ISleekElement element)
    {
        SleekBlueprint sleekBlueprint = (SleekBlueprint)element;
        sleekBlueprint.IsVisible = false;
        pooledBlueprintWidgets.Push(sleekBlueprint);
    }

    /// <summary>
    /// Get a blank status from the pool or construct a new one.
    /// </summary>
    private static BlueprintStatus CreateBlueprintStatus()
    {
        BlueprintStatus blueprintStatus;
        if (blueprintStatusPool.Count > 0)
        {
            blueprintStatus = blueprintStatusPool.GetAndRemoveTail();
            blueprintStatus.Reset();
        }
        else
        {
            blueprintStatus = new BlueprintStatus();
        }
        return blueprintStatus;
    }

    private static void RefreshShowIgnoredToggleAndFavoritesButtonVisible()
    {
        bool flag = false;
        bool hasIgnoredAnyBlueprints = PlayerCrafting.HasIgnoredAnyBlueprints;
        if (showIgnoredToggle.IsVisible != hasIgnoredAnyBlueprints)
        {
            flag = true;
            showIgnoredToggle.IsVisible = hasIgnoredAnyBlueprints;
        }
        bool hasFavoritedAnyBlueprints = PlayerCrafting.HasFavoritedAnyBlueprints;
        if (favoritesButton.IsVisible != hasFavoritedAnyBlueprints)
        {
            flag = true;
            favoritesButton.IsVisible = hasFavoritedAnyBlueprints;
        }
        if (flag)
        {
            OrganizeFiltersColumn();
        }
    }

    internal void OnDestroy()
    {
        PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged = (System.Action)Delegate.Remove(PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged, new System.Action(RefreshShowIgnoredToggleAndFavoritesButtonVisible));
    }

    public PlayerDashboardCraftingUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerDashboardCrafting.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDashboardCrafting/PlayerDashboardCrafting.unity3d");
        container = new SleekFullscreenBox();
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        filteredBlueprintsOverride = null;
        filterAnyOfCategories = new HashSet<TagAsset>();
        hideUncraftable = false;
        showIgnored = false;
        itemNameFilter = string.Empty;
        filterRequiresAnyOfTags = new HashSet<TagAsset>();
        filterTagProvider = null;
        filterFavorites = false;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.PositionOffset_Y = 60f;
        backdropBox.SizeOffset_Y = -60f;
        backdropBox.SizeScale_X = 1f;
        backdropBox.SizeScale_Y = 1f;
        backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        loadedBlueprints = new List<Blueprint>();
        assetListChangeCounter = -1;
        visibleBlueprints = new List<BlueprintStatus>();
        updatedBlueprints = new List<BlueprintStatus>();
        blueprintStatusPool = new List<BlueprintStatus>();
        pooledBlueprintWidgets = new Stack<SleekBlueprint>();
        blueprintsContainer = Glazier.Get().CreateFrame();
        blueprintsContainer.PositionOffset_X = 250f;
        blueprintsContainer.PositionOffset_Y = 10f;
        blueprintsContainer.SizeOffset_X = -260f;
        blueprintsContainer.SizeScale_X = 1f;
        blueprintsContainer.SizeScale_Y = 1f;
        blueprintsContainer.SizeOffset_Y = -20f;
        backdropBox.AddChild(blueprintsContainer);
        filteringDescriptionButton = new SleekButtonIcon(icons.load<Texture2D>("CancelFiltering"), 40);
        filteringDescriptionButton.SizeOffset_Y = 50f;
        filteringDescriptionButton.SizeScale_X = 1f;
        filteringDescriptionButton.enableRichText = true;
        filteringDescriptionButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        filteringDescriptionButton.fontSize = ESleekFontSize.Medium;
        filteringDescriptionButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        filteringDescriptionButton.iconColor = ESleekTint.FOREGROUND;
        filteringDescriptionButton.onClickedButton += OnClickedClearFilters;
        filteringDescriptionButton.tooltip = localization.format("ResetFilters_Label") + "\n" + localization.format("ResetFilters_Tooltip");
        blueprintsContainer.AddChild(filteringDescriptionButton);
        blueprintsScrollBox = new SleekList<BlueprintStatus>();
        blueprintsScrollBox.PositionOffset_Y = 40f;
        blueprintsScrollBox.SizeScale_X = 1f;
        blueprintsScrollBox.SizeScale_Y = 1f;
        blueprintsScrollBox.itemHeight = 160;
        blueprintsScrollBox.onCreateElement = onCreateBlueprint;
        SleekList<BlueprintStatus> sleekList = blueprintsScrollBox;
        sleekList.OnRemoveElement = (Action<ISleekElement>)Delegate.Combine(sleekList.OnRemoveElement, new Action<ISleekElement>(OnRemoveBlueprintElement));
        blueprintsScrollBox.SetData(visibleBlueprints);
        blueprintsContainer.AddChild(blueprintsScrollBox);
        filtersScrollView = Glazier.Get().CreateScrollView();
        filtersScrollView.PositionOffset_X = 10f;
        filtersScrollView.PositionOffset_Y = 10f;
        filtersScrollView.SizeOffset_X = 230f;
        filtersScrollView.SizeOffset_Y = -20f;
        filtersScrollView.SizeScale_Y = 1f;
        filtersScrollView.ScaleContentToWidth = true;
        backdropBox.AddChild(filtersScrollView);
        categoriesContainer = Glazier.Get().CreateFrame();
        categoriesContainer.SizeScale_X = 1f;
        categoriesContainer.SizeOffset_Y = 50f;
        filtersScrollView.AddChild(categoriesContainer);
        categoryTagButtons = new List<SleekTagButton>();
        categoriesHeader = Glazier.Get().CreateLabel();
        categoriesHeader.SizeScale_X = 1f;
        categoriesHeader.SizeOffset_Y = 40f;
        categoriesHeader.Text = localization.format("Header_Categories");
        categoriesHeader.FontSize = ESleekFontSize.Medium;
        categoriesHeader.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        categoriesContainer.AddChild(categoriesHeader);
        tagProvidersContainer = Glazier.Get().CreateFrame();
        tagProvidersContainer.SizeScale_X = 1f;
        filtersScrollView.AddChild(tagProvidersContainer);
        tagProviderButtons = new List<SleekCraftingTagProviderButton>();
        tagProvidersHeader = Glazier.Get().CreateLabel();
        tagProvidersHeader.SizeScale_X = 1f;
        tagProvidersHeader.SizeOffset_Y = 40f;
        tagProvidersHeader.Text = localization.format("Header_TagProviders");
        tagProvidersHeader.FontSize = ESleekFontSize.Medium;
        tagProvidersHeader.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        tagProvidersContainer.AddChild(tagProvidersHeader);
        hideUncraftableToggle = Glazier.Get().CreateToggle();
        hideUncraftableToggle.PositionOffset_Y = 60f;
        hideUncraftableToggle.SizeOffset_X = 40f;
        hideUncraftableToggle.SizeOffset_Y = 40f;
        hideUncraftableToggle.AddLabel(localization.format("Hide_Uncraftable_Toggle_Label"), ESleekSide.RIGHT);
        hideUncraftableToggle.TooltipText = localization.format("Hide_Uncraftable_Toggle_Tooltip");
        hideUncraftableToggle.Value = hideUncraftable;
        hideUncraftableToggle.OnValueChanged += onToggledHideUncraftableToggle;
        filtersScrollView.AddChild(hideUncraftableToggle);
        showIgnoredToggle = Glazier.Get().CreateToggle();
        showIgnoredToggle.PositionOffset_Y = 100f;
        showIgnoredToggle.SizeOffset_X = 40f;
        showIgnoredToggle.SizeOffset_Y = 40f;
        showIgnoredToggle.AddLabel(localization.format("Show_Ignored_Toggle_Label"), ESleekSide.RIGHT);
        showIgnoredToggle.TooltipText = localization.format("Show_Ignored_Toggle_Tooltip");
        showIgnoredToggle.Value = showIgnored;
        showIgnoredToggle.OnValueChanged += OnShowIgnoredToggled;
        filtersScrollView.AddChild(showIgnoredToggle);
        searchField = Glazier.Get().CreateStringField();
        searchField.SizeScale_X = 1f;
        searchField.SizeOffset_Y = 30f;
        searchField.PlaceholderText = localization.format("Search_Field_Hint");
        searchField.OnTextSubmitted += onEnteredSearchField;
        searchField.TooltipText = localization.format("CombineFiltersTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.modify));
        filtersScrollView.AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.PositionOffset_Y = 30f;
        searchButton.SizeScale_X = 1f;
        searchButton.SizeOffset_Y = 30f;
        searchButton.Text = localization.format("Search");
        searchButton.TooltipText = localization.format("Search_Tooltip") + "\n\n" + searchField.TooltipText;
        searchButton.OnClicked += onClickedSearchButton;
        filtersScrollView.AddChild(searchButton);
        favoritesButton = new SleekButtonIcon(icons.load<Texture2D>("FavoriteBlueprintIcon"), 40);
        favoritesButton.SizeScale_X = 1f;
        favoritesButton.SizeOffset_Y = 50f;
        favoritesButton.text = localization.format("Favorites_Label");
        favoritesButton.tooltip = localization.format("Favorites_Tooltip") + "\n\n" + searchField.TooltipText;
        favoritesButton.onClickedButton += OnClickedFavoritesButton;
        filtersScrollView.AddChild(favoritesButton);
        RefreshShowIgnoredToggleAndFavoritesButtonVisible();
        blueprintsListEmptyInfoBox = Glazier.Get().CreateBox();
        blueprintsListEmptyInfoBox.PositionOffset_Y = 40f;
        blueprintsListEmptyInfoBox.SizeOffset_Y = 50f;
        blueprintsListEmptyInfoBox.SizeScale_X = 1f;
        blueprintsListEmptyInfoBox.FontSize = ESleekFontSize.Medium;
        blueprintsContainer.AddChild(blueprintsListEmptyInfoBox);
        blueprintsListEmptyInfoBox.IsVisible = false;
        resetFiltersButton = Glazier.Get().CreateButton();
        resetFiltersButton.PositionOffset_X = -150f;
        resetFiltersButton.PositionOffset_Y = 10f;
        resetFiltersButton.PositionScale_X = 0.5f;
        resetFiltersButton.PositionScale_Y = 1f;
        resetFiltersButton.SizeOffset_X = 300f;
        resetFiltersButton.SizeOffset_Y = 30f;
        resetFiltersButton.Text = localization.format("ResetFilters_Label");
        resetFiltersButton.TooltipText = localization.format("ResetFilters_Tooltip");
        resetFiltersButton.OnClicked += OnClickedClearFilters;
        blueprintsListEmptyInfoBox.AddChild(resetFiltersButton);
        selectedBlueprintMenu = new SleekSelectedBlueprint();
        selectedBlueprintMenu.PositionOffset_X = -240f;
        selectedBlueprintMenu.PositionOffset_Y = 10f;
        selectedBlueprintMenu.PositionScale_X = 1f;
        selectedBlueprintMenu.SizeOffset_X = 230f;
        selectedBlueprintMenu.SizeScale_Y = 1f;
        selectedBlueprintMenu.SizeOffset_Y = -20f;
        selectedBlueprintMenu.IsVisible = false;
        backdropBox.AddChild(selectedBlueprintMenu);
        PlayerInventory inventory = Player.player.inventory;
        inventory.onInventoryResized = (InventoryResized)Delegate.Combine(inventory.onInventoryResized, new InventoryResized(onInventoryResized));
        PlayerCrafting crafting = Player.player.crafting;
        crafting.onCraftingUpdated = (CraftingUpdated)Delegate.Combine(crafting.onCraftingUpdated, new CraftingUpdated(onCraftingUpdated));
        PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged = (System.Action)Delegate.Combine(PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged, new System.Action(RefreshShowIgnoredToggleAndFavoritesButtonVisible));
    }

    private static int CompareCategoryTags(TagAsset lhs, TagAsset rhs)
    {
        return lhs.FriendlyName.CompareTo(rhs.FriendlyName);
    }

    private static string GetBlueprintStatusSortString(BlueprintStatus status)
    {
        Blueprint blueprint = status.blueprint;
        if (blueprint.TargetItem != null)
        {
            return blueprint.TargetItem.FindItemAsset()?.itemName;
        }
        if (blueprint.outputs != null && blueprint.outputs.Length == 1)
        {
            return blueprint.outputs[0].FindItemAsset()?.itemName;
        }
        if (blueprint.supplies != null && blueprint.supplies.Length == 1)
        {
            return blueprint.supplies[0].FindItemAsset()?.itemName;
        }
        return blueprint.GetOwnerAsset()?.FriendlyName;
    }

    private static int CompareVisibleBlueprints(BlueprintStatus lhs, BlueprintStatus rhs)
    {
        if (filterRequiresAnyOfTags != null && filterRequiresAnyOfTags.Count > 1)
        {
            int num = lhs.blueprint.CountOverlappingRequiredNearbyCraftingTags(filterRequiresAnyOfTags);
            int num2 = rhs.blueprint.CountOverlappingRequiredNearbyCraftingTags(filterRequiresAnyOfTags);
            if (num != num2)
            {
                return -num.CompareTo(num2);
            }
        }
        int num3 = -lhs.normalizedCraftability.CompareTo(rhs.normalizedCraftability);
        if (num3 != 0)
        {
            return num3;
        }
        string blueprintStatusSortString = GetBlueprintStatusSortString(lhs);
        string blueprintStatusSortString2 = GetBlueprintStatusSortString(rhs);
        if (string.IsNullOrEmpty(blueprintStatusSortString) == string.IsNullOrEmpty(blueprintStatusSortString2))
        {
            return blueprintStatusSortString?.CompareTo(blueprintStatusSortString2) ?? 0;
        }
        if (blueprintStatusSortString != null)
        {
            return -1;
        }
        return 1;
    }
}
