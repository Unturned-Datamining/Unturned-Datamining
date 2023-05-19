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
        for (byte b = 0; b < blueprint.outputs.Length; b = (byte)(b + 1))
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
        for (byte b2 = 0; b2 < blueprint.supplies.Length; b2 = (byte)(b2 + 1))
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
                for (int i = 0; i < item.blueprints.Count; i++)
                {
                    Blueprint blueprint = item.blueprints[i];
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
        for (int j = 0; j < list.Count; j++)
        {
            Blueprint blueprint2 = list[j];
            if ((blueprint2.skill == EBlueprintSkill.REPAIR && blueprint2.level > Provider.modeConfigData.Gameplay.Repair_Level_Max) || (!string.IsNullOrEmpty(blueprint2.map) && !blueprint2.map.Equals(Level.info.name, StringComparison.InvariantCultureIgnoreCase)) || !blueprint2.areConditionsMet(Player.player) || Player.player.crafting.isBlueprintBlacklisted(blueprint2))
            {
                continue;
            }
            ItemAsset sourceItem = blueprint2.sourceItem;
            ushort num = 0;
            bool flag3 = true;
            blueprint2.hasSupplies = true;
            blueprint2.hasSkills = blueprint2.skill == EBlueprintSkill.NONE || (blueprint2.skill == EBlueprintSkill.CRAFT && Player.player.skills.skills[2][1].level >= blueprint2.level) || (blueprint2.skill == EBlueprintSkill.COOK && flag && Player.player.skills.skills[2][3].level >= blueprint2.level) || (blueprint2.skill == EBlueprintSkill.REPAIR && Player.player.skills.skills[2][7].level >= blueprint2.level);
            List<InventorySearch>[] array = new List<InventorySearch>[blueprint2.supplies.Length];
            for (byte b = 0; b < blueprint2.supplies.Length; b = (byte)(b + 1))
            {
                BlueprintSupply blueprintSupply = blueprint2.supplies[b];
                List<InventorySearch> list3 = Player.player.inventory.search(blueprintSupply.id, findEmpty: false, findHealthy: true);
                ushort num2 = 0;
                foreach (InventorySearch item2 in list3)
                {
                    num2 = (ushort)(num2 + item2.jar.item.amount);
                }
                num = (ushort)(num + num2);
                blueprintSupply.hasAmount = num2;
                if (blueprint2.type == EBlueprintType.AMMO)
                {
                    if (blueprintSupply.hasAmount == 0)
                    {
                        blueprint2.hasSupplies = false;
                        flag3 = false;
                    }
                }
                else if (blueprintSupply.hasAmount < blueprintSupply.amount)
                {
                    blueprint2.hasSupplies = false;
                    if (blueprintSupply.isCritical)
                    {
                        flag3 = false;
                    }
                }
                array[b] = list3;
            }
            if (blueprint2.tool != 0)
            {
                InventorySearch inventorySearch = Player.player.inventory.has(blueprint2.tool);
                blueprint2.tools = ((inventorySearch != null) ? ((ushort)1) : ((ushort)0));
                blueprint2.hasTool = inventorySearch != null;
                if (inventorySearch == null && blueprint2.toolCritical)
                {
                    flag3 = false;
                }
            }
            else
            {
                blueprint2.tools = 1;
                blueprint2.hasTool = true;
            }
            if (blueprint2.type == EBlueprintType.REPAIR)
            {
                List<InventorySearch> list4 = Player.player.inventory.search(sourceItem.id, findEmpty: false, findHealthy: false);
                byte b2 = byte.MaxValue;
                byte b3 = byte.MaxValue;
                for (byte b4 = 0; b4 < list4.Count; b4 = (byte)(b4 + 1))
                {
                    if (list4[b4].jar.item.quality < b2)
                    {
                        b2 = list4[b4].jar.item.quality;
                        b3 = b4;
                    }
                }
                if (b3 != byte.MaxValue)
                {
                    blueprint2.items = list4[b3].jar.item.quality;
                    num = (ushort)(num + 1);
                }
                else
                {
                    blueprint2.items = 0;
                }
                blueprint2.hasItem = b3 != byte.MaxValue;
            }
            else if (blueprint2.type == EBlueprintType.AMMO)
            {
                List<InventorySearch> list5 = Player.player.inventory.search(sourceItem.id, findEmpty: true, findHealthy: true);
                int num3 = -1;
                byte b5 = byte.MaxValue;
                for (byte b6 = 0; b6 < list5.Count; b6 = (byte)(b6 + 1))
                {
                    if (list5[b6].jar.item.amount > num3 && list5[b6].jar.item.amount < sourceItem.amount)
                    {
                        num3 = list5[b6].jar.item.amount;
                        b5 = b6;
                    }
                }
                if (b5 != byte.MaxValue)
                {
                    if (list5[b5].jar.item.id == blueprint2.supplies[0].id)
                    {
                        blueprint2.supplies[0].hasAmount -= (ushort)num3;
                    }
                    blueprint2.supplies[0].amount = (byte)(sourceItem.amount - num3);
                    blueprint2.items = list5[b5].jar.item.amount;
                    num = (ushort)(num + 1);
                }
                else
                {
                    blueprint2.supplies[0].amount = 0;
                    blueprint2.items = 0;
                }
                blueprint2.hasItem = b5 != byte.MaxValue;
                if (b5 == byte.MaxValue)
                {
                    blueprint2.products = 0;
                }
                else if (blueprint2.items + blueprint2.supplies[0].hasAmount > sourceItem.amount)
                {
                    blueprint2.products = sourceItem.amount;
                }
                else
                {
                    blueprint2.products = (ushort)(blueprint2.items + blueprint2.supplies[0].hasAmount);
                }
            }
            else
            {
                blueprint2.hasItem = true;
            }
            if (!(flag2 || flag3))
            {
                continue;
            }
            if (newHideUncraftable)
            {
                bool ignoringBlueprint = Player.player.crafting.getIgnoringBlueprint(blueprint2);
                if (blueprint2.hasSupplies && blueprint2.hasTool && blueprint2.hasItem && blueprint2.hasSkills && !ignoringBlueprint)
                {
                    visibleBlueprints.Add(blueprint2);
                }
            }
            else if (newFilteredBlueprintsOverride != null)
            {
                if (blueprint2.hasSupplies && blueprint2.hasTool && blueprint2.hasItem && blueprint2.hasSkills)
                {
                    visibleBlueprints.Insert(0, blueprint2);
                }
                else
                {
                    visibleBlueprints.Add(blueprint2);
                }
            }
            else if (blueprint2.hasSupplies && blueprint2.hasTool && blueprint2.hasItem && blueprint2.hasSkills)
            {
                visibleBlueprints.Insert(0, blueprint2);
            }
            else if (flag2 || ((blueprint2.type == EBlueprintType.AMMO || blueprint2.type == EBlueprintType.REPAIR || num != 0) && blueprint2.hasItem))
            {
                visibleBlueprints.Add(blueprint2);
            }
        }
        filteredBlueprintsOverride = newFilteredBlueprintsOverride;
        blueprintTypeFilterIndex = newBlueprintTypeFilterIndex;
        hideUncraftable = newHideUncraftable;
        itemNameFilter = newItemNameFilter;
        blueprintsScrollBox.ForceRebuildElements();
        infoBox.isVisible = visibleBlueprints.Count == 0;
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
        byte newBlueprintTypeFilterIndex = (byte)((button.positionOffset_X + -(TYPES * -30 + 5)) / 60);
        searchField.text = "";
        updateSelection(null, newBlueprintTypeFilterIndex, hideUncraftable, string.Empty);
    }

    private static void onToggledHideUncraftableToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(filteredBlueprintsOverride, blueprintTypeFilterIndex, state, itemNameFilter);
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        updateSelection(null, blueprintTypeFilterIndex, hideUncraftable, searchField.text);
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        updateSelection(null, blueprintTypeFilterIndex, hideUncraftable, searchField.text);
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
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        blueprintTypeFilterIndex = byte.MaxValue;
        hideUncraftable = false;
        itemNameFilter = string.Empty;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.positionOffset_Y = 60;
        backdropBox.sizeOffset_Y = -60;
        backdropBox.sizeScale_X = 1f;
        backdropBox.sizeScale_Y = 1f;
        backdropBox.backgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        visibleBlueprints = new List<Blueprint>();
        blueprintsScrollBox = new SleekList<Blueprint>();
        blueprintsScrollBox.positionOffset_X = 10;
        blueprintsScrollBox.positionOffset_Y = 110;
        blueprintsScrollBox.sizeOffset_X = -20;
        blueprintsScrollBox.sizeOffset_Y = -120;
        blueprintsScrollBox.sizeScale_X = 1f;
        blueprintsScrollBox.sizeScale_Y = 1f;
        blueprintsScrollBox.itemHeight = 195;
        blueprintsScrollBox.itemPadding = 10;
        blueprintsScrollBox.onCreateElement = onCreateBlueprint;
        blueprintsScrollBox.SetData(visibleBlueprints);
        backdropBox.AddChild(blueprintsScrollBox);
        for (int i = 0; i < TYPES; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Blueprint_" + i));
            sleekButtonIcon.positionOffset_X = TYPES * -30 + 5 + i * 60;
            sleekButtonIcon.positionOffset_Y = 10;
            sleekButtonIcon.positionScale_X = 0.5f;
            sleekButtonIcon.sizeOffset_X = 50;
            sleekButtonIcon.sizeOffset_Y = 50;
            sleekButtonIcon.tooltip = localization.format("Type_" + i + "_Tooltip");
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedTypeButton;
            backdropBox.AddChild(sleekButtonIcon);
        }
        hideUncraftableToggle = Glazier.Get().CreateToggle();
        hideUncraftableToggle.positionOffset_X = -80;
        hideUncraftableToggle.positionOffset_Y = 65;
        hideUncraftableToggle.positionScale_X = 1f;
        hideUncraftableToggle.sizeOffset_X = 40;
        hideUncraftableToggle.sizeOffset_Y = 40;
        hideUncraftableToggle.addLabel(localization.format("Hide_Uncraftable_Toggle_Label"), ESleekSide.LEFT);
        hideUncraftableToggle.state = hideUncraftable;
        hideUncraftableToggle.onToggled += onToggledHideUncraftableToggle;
        backdropBox.AddChild(hideUncraftableToggle);
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_X = 10;
        searchField.positionOffset_Y = 70;
        searchField.sizeOffset_X = -410;
        searchField.sizeOffset_Y = 30;
        searchField.sizeScale_X = 1f;
        searchField.hint = localization.format("Search_Field_Hint");
        searchField.onEntered += onEnteredSearchField;
        backdropBox.AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.positionOffset_X = -390;
        searchButton.positionOffset_Y = 70;
        searchButton.positionScale_X = 1f;
        searchButton.sizeOffset_X = 100;
        searchButton.sizeOffset_Y = 30;
        searchButton.text = localization.format("Search");
        searchButton.tooltipText = localization.format("Search_Tooltip");
        searchButton.onClickedButton += onClickedSearchButton;
        backdropBox.AddChild(searchButton);
        infoBox = Glazier.Get().CreateBox();
        infoBox.positionOffset_X = 10;
        infoBox.positionOffset_Y = 110;
        infoBox.sizeOffset_X = -20;
        infoBox.sizeOffset_Y = 50;
        infoBox.sizeScale_X = 1f;
        infoBox.text = localization.format("No_Blueprints");
        infoBox.fontSize = ESleekFontSize.Medium;
        backdropBox.AddChild(infoBox);
        infoBox.isVisible = false;
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
