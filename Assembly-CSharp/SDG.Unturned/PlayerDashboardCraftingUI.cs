using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDashboardCraftingUI
{
    private static readonly int TYPES = 10;

    public static Local localization;

    private static SleekFullscreenBox container;

    public static Bundle icons;

    public static bool active;

    private static ISleekBox backdropBox;

    private static ISleekField searchField;

    private static ISleekButton searchButton;

    private static List<Blueprint> visibleBlueprints;

    private static SleekList<Blueprint> blueprintsScrollBox;

    private static ISleekBox infoBox;

    private static ISleekToggle hideUncraftableToggle;

    public static Blueprint[] filteredBlueprintsOverride;

    private static byte blueprintTypeFilterIndex;

    private static bool hideUncraftable;

    private static string itemNameFilter;

    public static void open()
    {
        if (!active)
        {
            active = true;
            updateSelection(filteredBlueprintsOverride, blueprintTypeFilterIndex, hideUncraftable, itemNameFilter);
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

    private static bool DoesAnyItemNameContainString(Blueprint blueprint, string text)
    {
        for (byte b = 0; b < blueprint.outputs.Length; b++)
        {
            BlueprintOutput blueprintOutput = blueprint.outputs[b];
            if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset && itemAsset.itemName != null && itemAsset.itemName.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        if (blueprint.tool != 0 && Assets.find(EAssetType.ITEM, blueprint.tool) is ItemAsset itemAsset2 && itemAsset2.itemName != null && itemAsset2.itemName.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        for (byte b2 = 0; b2 < blueprint.supplies.Length; b2++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[b2];
            if (Assets.find(EAssetType.ITEM, blueprintSupply.id) is ItemAsset itemAsset3 && itemAsset3.itemName != null && itemAsset3.itemName.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }
        return false;
    }

    public static void updateSelection()
    {
        updateSelection(filteredBlueprintsOverride, blueprintTypeFilterIndex, hideUncraftable, itemNameFilter);
    }

    private static void updateSelection(Blueprint[] newFilteredBlueprintsOverride, byte newBlueprintTypeFilterIndex, bool newHideUncraftable, string newItemNameFilter)
    {
        bool flag = PowerTool.checkFires(Player.player.transform.position, 16f);
        bool flag2 = !string.IsNullOrEmpty(newItemNameFilter);
        List<Blueprint> list;
        if (newFilteredBlueprintsOverride == null)
        {
            list = new List<Blueprint>();
            List<ItemAsset> list2 = new List<ItemAsset>();
            Assets.find(list2);
            foreach (ItemAsset item in list2)
            {
                if (item == null)
                {
                    continue;
                }
                foreach (Blueprint blueprint in item.blueprints)
                {
                    if (flag2 ? DoesAnyItemNameContainString(blueprint, newItemNameFilter) : (blueprint.type == (EBlueprintType)newBlueprintTypeFilterIndex))
                    {
                        list.Add(blueprint);
                    }
                }
            }
        }
        else
        {
            list = new List<Blueprint>(newFilteredBlueprintsOverride);
        }
        if (Level.info != null && Level.info.configData != null && !Level.info.configData.Allow_Crafting)
        {
            newFilteredBlueprintsOverride = new Blueprint[0];
            list.Clear();
        }
        visibleBlueprints.Clear();
        foreach (Blueprint item2 in list)
        {
            if ((item2.skill == EBlueprintSkill.REPAIR && item2.level > Provider.modeConfigData.Gameplay.Repair_Level_Max) || (!string.IsNullOrEmpty(item2.map) && !item2.map.Equals(Level.info.name, StringComparison.InvariantCultureIgnoreCase)) || !item2.areConditionsMet(Player.player) || Player.player.crafting.isBlueprintBlacklisted(item2))
            {
                continue;
            }
            ItemAsset sourceItem = item2.sourceItem;
            int num = 0;
            bool flag3 = true;
            item2.hasSupplies = true;
            item2.hasSkills = item2.skill == EBlueprintSkill.NONE || (item2.skill == EBlueprintSkill.CRAFT && Player.player.skills.skills[2][1].level >= item2.level) || (item2.skill == EBlueprintSkill.COOK && flag && Player.player.skills.skills[2][3].level >= item2.level) || (item2.skill == EBlueprintSkill.REPAIR && Player.player.skills.skills[2][7].level >= item2.level);
            List<InventorySearch>[] array = new List<InventorySearch>[item2.supplies.Length];
            for (int i = 0; i < item2.supplies.Length; i++)
            {
                BlueprintSupply blueprintSupply = item2.supplies[i];
                List<InventorySearch> list3 = Player.player.inventory.search(blueprintSupply.id, findEmpty: false, findHealthy: true);
                ushort num2 = 0;
                foreach (InventorySearch item3 in list3)
                {
                    num2 += item3.jar.item.amount;
                }
                num += num2;
                blueprintSupply.hasAmount = num2;
                if (item2.type == EBlueprintType.AMMO)
                {
                    if (blueprintSupply.hasAmount == 0)
                    {
                        item2.hasSupplies = false;
                        flag3 = false;
                    }
                }
                else if (blueprintSupply.hasAmount < blueprintSupply.amount)
                {
                    item2.hasSupplies = false;
                    if (blueprintSupply.isCritical)
                    {
                        flag3 = false;
                    }
                }
                array[i] = list3;
            }
            if (item2.tool != 0)
            {
                InventorySearch inventorySearch = Player.player.inventory.has(item2.tool);
                item2.tools = ((inventorySearch != null) ? ((ushort)1) : ((ushort)0));
                item2.hasTool = inventorySearch != null;
                if (inventorySearch == null && item2.toolCritical)
                {
                    flag3 = false;
                }
            }
            else
            {
                item2.tools = 1;
                item2.hasTool = true;
            }
            if (item2.type == EBlueprintType.REPAIR)
            {
                List<InventorySearch> list4 = Player.player.inventory.search(sourceItem.id, findEmpty: false, findHealthy: false);
                byte b = byte.MaxValue;
                int num3 = -1;
                for (int j = 0; j < list4.Count; j++)
                {
                    if (list4[j].jar.item.quality < b)
                    {
                        b = list4[j].jar.item.quality;
                        num3 = j;
                    }
                }
                if (num3 >= 0)
                {
                    item2.items = list4[num3].jar.item.quality;
                    num++;
                }
                else
                {
                    item2.items = 0;
                }
                item2.hasItem = num3 >= 0;
            }
            else if (item2.type == EBlueprintType.AMMO)
            {
                List<InventorySearch> list5 = Player.player.inventory.search(sourceItem.id, findEmpty: true, findHealthy: true);
                int num4 = -1;
                int num5 = -1;
                for (int k = 0; k < list5.Count; k++)
                {
                    if (list5[k].jar.item.amount > num4 && list5[k].jar.item.amount < sourceItem.amount)
                    {
                        num4 = list5[k].jar.item.amount;
                        num5 = k;
                    }
                }
                if (num5 >= 0)
                {
                    if (list5[num5].jar.item.id == item2.supplies[0].id)
                    {
                        item2.supplies[0].hasAmount -= (ushort)num4;
                    }
                    item2.supplies[0].amount = (byte)(sourceItem.amount - num4);
                    item2.items = list5[num5].jar.item.amount;
                    num++;
                }
                else
                {
                    item2.supplies[0].amount = 0;
                    item2.items = 0;
                }
                item2.hasItem = num5 >= 0;
                if (num5 < 0)
                {
                    item2.products = 0;
                }
                else if (item2.items + item2.supplies[0].hasAmount > sourceItem.amount)
                {
                    item2.products = sourceItem.amount;
                }
                else
                {
                    item2.products = (ushort)(item2.items + item2.supplies[0].hasAmount);
                }
            }
            else
            {
                item2.hasItem = true;
            }
            if (!flag3 && (!flag2 || !item2.canBeVisibleWhenSearchedWithoutRequiredItems))
            {
                continue;
            }
            if (newHideUncraftable)
            {
                bool ignoringBlueprint = Player.player.crafting.getIgnoringBlueprint(item2);
                if (item2.hasSupplies && item2.hasTool && item2.hasItem && item2.hasSkills && !ignoringBlueprint)
                {
                    visibleBlueprints.Add(item2);
                }
            }
            else if (newFilteredBlueprintsOverride != null)
            {
                if (item2.hasSupplies && item2.hasTool && item2.hasItem && item2.hasSkills)
                {
                    visibleBlueprints.Insert(0, item2);
                }
                else
                {
                    visibleBlueprints.Add(item2);
                }
            }
            else if (item2.hasSupplies && item2.hasTool && item2.hasItem && item2.hasSkills)
            {
                visibleBlueprints.Insert(0, item2);
            }
            else if (flag2 || ((item2.type == EBlueprintType.AMMO || item2.type == EBlueprintType.REPAIR || num != 0) && item2.hasItem))
            {
                visibleBlueprints.Add(item2);
            }
        }
        filteredBlueprintsOverride = newFilteredBlueprintsOverride;
        blueprintTypeFilterIndex = newBlueprintTypeFilterIndex;
        hideUncraftable = newHideUncraftable;
        itemNameFilter = newItemNameFilter;
        blueprintsScrollBox.ForceRebuildElements();
        infoBox.IsVisible = visibleBlueprints.Count == 0;
    }

    private static void onInventoryResized(byte page, byte newWidth, byte newHeight)
    {
        if (active)
        {
            updateSelection();
        }
    }

    private static void onCraftingUpdated()
    {
        if (active)
        {
            updateSelection();
        }
    }

    private static void onClickedTypeButton(ISleekElement button)
    {
        byte newBlueprintTypeFilterIndex = (byte)((button.PositionOffset_X + (float)(-(TYPES * -30 + 5))) / 60f);
        searchField.Text = "";
        updateSelection(null, newBlueprintTypeFilterIndex, hideUncraftable, string.Empty);
    }

    private static void onToggledHideUncraftableToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(filteredBlueprintsOverride, blueprintTypeFilterIndex, state, itemNameFilter);
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        updateSelection(null, blueprintTypeFilterIndex, hideUncraftable, searchField.Text);
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        updateSelection(null, blueprintTypeFilterIndex, hideUncraftable, searchField.Text);
    }

    private static void clickedBlueprint(Blueprint blueprint, bool all)
    {
        if (blueprint.hasSupplies && blueprint.hasTool && blueprint.hasItem && blueprint.hasSkills && !Player.player.equipment.isBusy)
        {
            Player.player.crafting.sendCraft(blueprint.sourceItem.id, blueprint.id, all);
        }
    }

    private static void onClickedBlueprintButton(Blueprint blueprint)
    {
        clickedBlueprint(blueprint, InputEx.GetKey(ControlsSettings.other));
    }

    private static void onClickedBlueprintCraftAllButton(Blueprint blueprint)
    {
        clickedBlueprint(blueprint, all: true);
    }

    private static ISleekElement onCreateBlueprint(Blueprint blueprint)
    {
        SleekBlueprint sleekBlueprint = new SleekBlueprint(blueprint);
        sleekBlueprint.onClickedCraftButton += onClickedBlueprintButton;
        sleekBlueprint.onClickedCraftAllButton += onClickedBlueprintCraftAllButton;
        return sleekBlueprint;
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
        blueprintTypeFilterIndex = byte.MaxValue;
        hideUncraftable = false;
        itemNameFilter = string.Empty;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.PositionOffset_Y = 60f;
        backdropBox.SizeOffset_Y = -60f;
        backdropBox.SizeScale_X = 1f;
        backdropBox.SizeScale_Y = 1f;
        backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        visibleBlueprints = new List<Blueprint>();
        blueprintsScrollBox = new SleekList<Blueprint>();
        blueprintsScrollBox.PositionOffset_X = 10f;
        blueprintsScrollBox.PositionOffset_Y = 110f;
        blueprintsScrollBox.SizeOffset_X = -20f;
        blueprintsScrollBox.SizeOffset_Y = -120f;
        blueprintsScrollBox.SizeScale_X = 1f;
        blueprintsScrollBox.SizeScale_Y = 1f;
        blueprintsScrollBox.itemHeight = 195;
        blueprintsScrollBox.itemPadding = 10;
        blueprintsScrollBox.onCreateElement = onCreateBlueprint;
        blueprintsScrollBox.SetData(visibleBlueprints);
        backdropBox.AddChild(blueprintsScrollBox);
        for (int i = 0; i < TYPES; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Blueprint_" + i));
            sleekButtonIcon.PositionOffset_X = TYPES * -30 + 5 + i * 60;
            sleekButtonIcon.PositionOffset_Y = 10f;
            sleekButtonIcon.PositionScale_X = 0.5f;
            sleekButtonIcon.SizeOffset_X = 50f;
            sleekButtonIcon.SizeOffset_Y = 50f;
            sleekButtonIcon.tooltip = localization.format("Type_" + i + "_Tooltip");
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedTypeButton;
            backdropBox.AddChild(sleekButtonIcon);
        }
        hideUncraftableToggle = Glazier.Get().CreateToggle();
        hideUncraftableToggle.PositionOffset_X = -80f;
        hideUncraftableToggle.PositionOffset_Y = 65f;
        hideUncraftableToggle.PositionScale_X = 1f;
        hideUncraftableToggle.SizeOffset_X = 40f;
        hideUncraftableToggle.SizeOffset_Y = 40f;
        hideUncraftableToggle.AddLabel(localization.format("Hide_Uncraftable_Toggle_Label"), ESleekSide.LEFT);
        hideUncraftableToggle.Value = hideUncraftable;
        hideUncraftableToggle.OnValueChanged += onToggledHideUncraftableToggle;
        backdropBox.AddChild(hideUncraftableToggle);
        searchField = Glazier.Get().CreateStringField();
        searchField.PositionOffset_X = 10f;
        searchField.PositionOffset_Y = 70f;
        searchField.SizeOffset_X = -410f;
        searchField.SizeOffset_Y = 30f;
        searchField.SizeScale_X = 1f;
        searchField.PlaceholderText = localization.format("Search_Field_Hint");
        searchField.OnTextSubmitted += onEnteredSearchField;
        backdropBox.AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.PositionOffset_X = -390f;
        searchButton.PositionOffset_Y = 70f;
        searchButton.PositionScale_X = 1f;
        searchButton.SizeOffset_X = 100f;
        searchButton.SizeOffset_Y = 30f;
        searchButton.Text = localization.format("Search");
        searchButton.TooltipText = localization.format("Search_Tooltip");
        searchButton.OnClicked += onClickedSearchButton;
        backdropBox.AddChild(searchButton);
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionOffset_X = 10f;
        infoBox.PositionOffset_Y = 110f;
        infoBox.SizeOffset_X = -20f;
        infoBox.SizeOffset_Y = 50f;
        infoBox.SizeScale_X = 1f;
        infoBox.Text = localization.format("No_Blueprints");
        infoBox.FontSize = ESleekFontSize.Medium;
        backdropBox.AddChild(infoBox);
        infoBox.IsVisible = false;
        filteredBlueprintsOverride = null;
        blueprintTypeFilterIndex = 0;
        hideUncraftable = false;
        itemNameFilter = string.Empty;
        PlayerInventory inventory = Player.player.inventory;
        inventory.onInventoryResized = (InventoryResized)Delegate.Combine(inventory.onInventoryResized, new InventoryResized(onInventoryResized));
        PlayerCrafting crafting = Player.player.crafting;
        crafting.onCraftingUpdated = (CraftingUpdated)Delegate.Combine(crafting.onCraftingUpdated, new CraftingUpdated(onCraftingUpdated));
    }
}
