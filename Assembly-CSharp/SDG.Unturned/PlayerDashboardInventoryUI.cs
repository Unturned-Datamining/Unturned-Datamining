using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDashboardInventoryUI
{
    private static List<InteractableItem> pendingItemsInRadius;

    private static SleekFullscreenBox container;

    public static Local localization;

    public static Bundle icons;

    public static bool active;

    private static ISleekBox backdropBox;

    private static ISleekImage dragOutsideHandler;

    private static ISleekImage dragOutsideClothingHandler;

    private static ISleekImage dragOutsideAreaHandler;

    private static bool hasDragOutsideHandlers;

    private static ItemJar dragJar;

    private static SleekItem dragSource;

    private static SleekItem dragItem;

    private static Vector2 dragOffset;

    private static Vector2 dragPivot;

    private static byte dragFromPage;

    private static byte dragFrom_x;

    private static byte dragFrom_y;

    private static byte dragFromRot;

    private static ISleekImage characterImage;

    private static ISleekSlider characterSlider;

    private static SleekButtonIcon swapCosmeticsButton;

    private static SleekButtonIcon swapSkinsButton;

    private static SleekButtonIcon swapMythicsButton;

    private static SleekPlayer characterPlayer;

    private static SleekSlot[] slots;

    private static ISleekElement box;

    private static ISleekScrollView clothingBox;

    private static ISleekScrollView areaBox;

    private static ISleekButton[] headers;

    private static SleekItemIcon[] headerItemIcons;

    private static SleekItems[] items;

    private static ISleekImage selectionFrame;

    private static ISleekBox selectionBackdropBox;

    private static ISleekBox selectionIconBox;

    private static SleekItemIcon selectionIconImage;

    private static ISleekBox selectionDescriptionBox;

    private static ISleekLabel selectionDescriptionLabel;

    private static ISleekLabel selectionNameLabel;

    private static ISleekLabel selectionHotkeyLabel;

    private static ISleekBox vehicleBox;

    private static ISleekLabel vehicleNameLabel;

    private static ISleekElement vehicleActionsBox;

    private static ISleekElement vehiclePassengersBox;

    private static ISleekButton vehicleLockButton;

    private static ISleekButton vehicleHornButton;

    private static ISleekButton vehicleHeadlightsButton;

    private static ISleekButton vehicleSirensButton;

    private static ISleekButton vehicleBlimpButton;

    private static ISleekButton vehicleHookButton;

    private static ISleekButton vehicleStealBatteryButton;

    private static ISleekButton vehicleSkinButton;

    private static ISleekScrollView selectionActionsBox;

    private static ISleekButton selectionEquipButton;

    private static ISleekButton selectionContextButton;

    private static ISleekButton selectionDropButton;

    private static ISleekButton selectionStorageButton;

    private static ISleekElement selectionExtraActionsBox;

    private static ISleekButton rot_xButton;

    private static ISleekButton rot_yButton;

    private static ISleekButton rot_zButton;

    private static byte _selectedPage;

    private static byte _selected_x;

    private static byte _selected_y;

    private static ItemJar _selectedJar;

    private static ItemAsset _selectedAsset;

    private static Items areaItems;

    private static List<Action> actions;

    private static OncePerFrameGuard eventGuard;

    public static bool isDragging { get; private set; }

    public static byte selectedPage => _selectedPage;

    public static byte selected_x => _selected_x;

    public static byte selected_y => _selected_y;

    public static ItemJar selectedJar => _selectedJar;

    public static ItemAsset selectedAsset => _selectedAsset;

    private static bool isSplitClothingArea => Screen.width >= 1350;

    public static bool WasEventConsumed => eventGuard.HasBeenConsumed;

    public static void open()
    {
        if (!active)
        {
            active = true;
            Player.player.animator.sendGesture(EPlayerGesture.INVENTORY_START, all: false);
            Player.player.character.Find("Camera").gameObject.SetActive(value: true);
            if (isSplitClothingArea)
            {
                clothingBox.sizeOffset_X = -5;
                clothingBox.sizeScale_X = 0.5f;
                areaBox.isVisible = true;
            }
            else
            {
                clothingBox.sizeOffset_X = 0;
                clothingBox.sizeScale_X = 1f;
                areaBox.isVisible = false;
            }
            updateVehicle();
            resetNearbyDrops();
            updateHotkeys();
            if (characterPlayer != null)
            {
                backdropBox.RemoveChild(characterPlayer);
                characterPlayer = null;
            }
            if (Player.player != null)
            {
                characterPlayer = new SleekPlayer(Player.player.channel.owner, isButton: true, SleekPlayer.ESleekPlayerDisplayContext.NONE);
                characterPlayer.positionOffset_X = 10;
                characterPlayer.positionOffset_Y = 10;
                characterPlayer.sizeOffset_X = 410;
                characterPlayer.sizeOffset_Y = 50;
                backdropBox.AddChild(characterPlayer);
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            Player.player.animator.sendGesture(EPlayerGesture.INVENTORY_STOP, all: false);
            Player.player.character.Find("Camera").gameObject.SetActive(value: false);
            stopDrag();
            closeSelection();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void startDrag()
    {
        if (!isDragging)
        {
            isDragging = true;
            setItemsEnabled(enabled: false);
            dragItem.isVisible = true;
            if (hasDragOutsideHandlers)
            {
                dragOutsideHandler.isVisible = true;
                dragOutsideClothingHandler.isVisible = true;
                dragOutsideAreaHandler.isVisible = true;
            }
            PlayInventoryAudio(dragJar.GetAsset());
        }
    }

    public static void stopDrag()
    {
        if (isDragging)
        {
            isDragging = false;
            PlayInventoryAudio(dragJar.GetAsset());
            dragJar.rot = dragFromRot;
            setItemsEnabled(enabled: true);
            dragItem.isVisible = false;
            if (hasDragOutsideHandlers)
            {
                dragOutsideHandler.isVisible = false;
                dragOutsideClothingHandler.isVisible = false;
                dragOutsideAreaHandler.isVisible = false;
            }
        }
    }

    private static void setItemsEnabled(bool enabled)
    {
        SleekSlot[] array = slots;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].isItemEnabled = enabled;
        }
        SleekItems[] array2 = items;
        for (int i = 0; i < array2.Length; i++)
        {
            array2[i].areItemsEnabled = enabled;
        }
    }

    private static void setMiscButtonsEnabled(bool enabled)
    {
        ISleekButton[] array = headers;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].isRaycastTarget = enabled;
        }
        SleekSlot[] array2 = slots;
        for (int i = 0; i < array2.Length; i++)
        {
            array2[i].isImageRaycastTarget = enabled;
        }
        SleekItems[] array3 = items;
        for (int i = 0; i < array3.Length; i++)
        {
            array3[i].isGridRaycastTarget = enabled;
        }
    }

    private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
    {
        PlayerLook.characterYaw = state * 360f;
    }

    private static void onClickedSwapCosmeticsButton(ISleekElement button)
    {
        Player.player.clothing.sendVisualToggle(EVisualToggleType.COSMETIC);
    }

    private static void onClickedSwapSkinsButton(ISleekElement button)
    {
        Player.player.clothing.sendVisualToggle(EVisualToggleType.SKIN);
    }

    private static void onClickedSwapMythicsButton(ISleekElement button)
    {
        Player.player.clothing.sendVisualToggle(EVisualToggleType.MYTHIC);
    }

    private static void onClickedVehicleLockButton(ISleekElement button)
    {
        VehicleManager.sendVehicleLock();
    }

    private static void onClickedVehicleHornButton(ISleekElement button)
    {
        VehicleManager.sendVehicleHorn();
    }

    private static void onClickedVehicleHeadlightsButton(ISleekElement button)
    {
        VehicleManager.sendVehicleHeadlights();
    }

    private static void onClickedVehicleSirensButton(ISleekElement button)
    {
        VehicleManager.sendVehicleBonus();
    }

    private static void onClickedVehicleBlimpButton(ISleekElement button)
    {
        VehicleManager.sendVehicleBonus();
    }

    private static void onClickedVehicleHookButton(ISleekElement button)
    {
        VehicleManager.sendVehicleBonus();
    }

    private static void onClickedVehicleStealBatteryButton(ISleekElement button)
    {
        VehicleManager.sendVehicleStealBattery();
    }

    private static void onClickedVehicleSkinButton(ISleekElement button)
    {
        VehicleManager.sendVehicleSkin();
    }

    private static void onClickedVehiclePassengerButton(ISleekElement button)
    {
        int num = vehiclePassengersBox.FindIndexOfChild(button);
        if (num >= 0)
        {
            VehicleManager.swapVehicle((byte)num);
        }
    }

    private static void ConsumeEvent()
    {
        eventGuard.Consume();
    }

    private static void onClickedEquip(ISleekElement button)
    {
        if (selectedPage != byte.MaxValue)
        {
            checkEquip(selectedPage, selected_x, selected_y, Player.player.inventory.getItem(selectedPage, Player.player.inventory.getIndex(selectedPage, selected_x, selected_y)), byte.MaxValue);
            ConsumeEvent();
        }
    }

    private static void onClickedContext(ISleekElement button)
    {
        if (selectedPage != byte.MaxValue)
        {
            if (selectedAsset.type == EItemType.GUN)
            {
                Player.player.crafting.sendStripAttachments(selectedPage, selected_x, selected_y);
            }
            ConsumeEvent();
            closeSelection();
        }
    }

    private static void onClickedDrop(ISleekElement button)
    {
        if (selectedPage == byte.MaxValue)
        {
            return;
        }
        if (selectedPage == PlayerInventory.AREA)
        {
            if (selectedJar.interactableItem != null)
            {
                ItemManager.takeItem(selectedJar.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
            }
            closeSelection();
        }
        else
        {
            Player.player.inventory.sendDropItem(selectedPage, selected_x, selected_y);
        }
        ConsumeEvent();
    }

    private static void onClickedStore(ISleekElement button)
    {
        if (selectedPage == byte.MaxValue)
        {
            return;
        }
        byte x2;
        byte y2;
        byte rot2;
        if (selectedPage == PlayerInventory.AREA)
        {
            if (selectedJar.interactableItem != null)
            {
                ItemManager.takeItem(selectedJar.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, PlayerInventory.STORAGE);
            }
            closeSelection();
        }
        else if (selectedPage == PlayerInventory.STORAGE)
        {
            if (Player.player.inventory.tryFindSpace(selectedJar.size_x, selectedJar.size_y, out var page, out var x, out var y, out var rot))
            {
                Player.player.inventory.sendDragItem(selectedPage, selected_x, selected_y, page, x, y, rot);
            }
        }
        else if (Player.player.inventory.tryFindSpace(PlayerInventory.STORAGE, selectedJar.size_x, selectedJar.size_y, out x2, out y2, out rot2))
        {
            Player.player.inventory.sendDragItem(selectedPage, selected_x, selected_y, PlayerInventory.STORAGE, x2, y2, rot2);
        }
        ConsumeEvent();
    }

    private static void onClickedAction(ISleekElement button)
    {
        int index = selectionExtraActionsBox.FindIndexOfChild(button);
        Action action = actions[index];
        if (!(Assets.find(EAssetType.ITEM, action.source) is ItemAsset itemAsset))
        {
            return;
        }
        Blueprint[] array = new Blueprint[action.blueprints.Length];
        bool flag = false;
        for (byte b = 0; b < array.Length; b = (byte)(b + 1))
        {
            array[b] = itemAsset.blueprints[action.blueprints[b].id];
            if (action.blueprints[b].isLink)
            {
                flag = true;
            }
        }
        PlayerDashboardCraftingUI.viewBlueprints = array;
        if (!flag)
        {
            PlayerDashboardCraftingUI.updateSelection();
            foreach (Blueprint blueprint in array)
            {
                if (!blueprint.hasSupplies)
                {
                    flag = true;
                    break;
                }
                if (!blueprint.hasTool)
                {
                    flag = true;
                    break;
                }
                if (!blueprint.hasItem)
                {
                    flag = true;
                    break;
                }
                if (!blueprint.hasSkills)
                {
                    flag = true;
                    break;
                }
                if (Player.player.equipment.isBusy)
                {
                    flag = true;
                    break;
                }
            }
        }
        if (flag)
        {
            close();
            PlayerDashboardCraftingUI.open();
            return;
        }
        foreach (Blueprint blueprint2 in array)
        {
            Player.player.crafting.sendCraft(blueprint2.sourceItem.id, blueprint2.id, InputEx.GetKey(ControlsSettings.other));
        }
        PlayerDashboardCraftingUI.viewBlueprints = null;
        closeSelection();
    }

    private static void onClickedRot_XButton(ISleekElement button)
    {
        InteractableStorage interactableStorage = PlayerInteract.interactable as InteractableStorage;
        if (!(interactableStorage == null) && interactableStorage.isDisplay)
        {
            byte rot_x = interactableStorage.rot_x;
            rot_x = (byte)(rot_x + 1);
            if (rot_x > 3)
            {
                rot_x = 0;
            }
            byte rotation = interactableStorage.getRotation(rot_x, interactableStorage.rot_y, interactableStorage.rot_z);
            interactableStorage.ClientSetDisplayRotation(rotation);
        }
    }

    private static void onClickedRot_YButton(ISleekElement button)
    {
        InteractableStorage interactableStorage = PlayerInteract.interactable as InteractableStorage;
        if (!(interactableStorage == null) && interactableStorage.isDisplay)
        {
            byte rot_y = interactableStorage.rot_y;
            rot_y = (byte)(rot_y + 1);
            if (rot_y > 3)
            {
                rot_y = 0;
            }
            byte rotation = interactableStorage.getRotation(interactableStorage.rot_x, rot_y, interactableStorage.rot_z);
            interactableStorage.ClientSetDisplayRotation(rotation);
        }
    }

    private static void onClickedRot_ZButton(ISleekElement button)
    {
        InteractableStorage interactableStorage = PlayerInteract.interactable as InteractableStorage;
        if (!(interactableStorage == null) && interactableStorage.isDisplay)
        {
            byte b = (byte)(interactableStorage.rot_z++ + 1);
            if (b > 3)
            {
                b = 0;
            }
            byte rotation = interactableStorage.getRotation(interactableStorage.rot_x, interactableStorage.rot_y, b);
            interactableStorage.ClientSetDisplayRotation(rotation);
        }
    }

    private static void openSelection(byte page, byte x, byte y)
    {
        _selectedPage = page;
        _selected_x = x;
        _selected_y = y;
        if (!Glazier.Get().SupportsDepth)
        {
            setItemsEnabled(enabled: false);
            setMiscButtonsEnabled(enabled: false);
        }
        selectionFrame.isVisible = true;
        _selectedJar = Player.player.inventory.getItem(page, Player.player.inventory.getIndex(page, x, y));
        if (selectedJar == null)
        {
            return;
        }
        _selectedAsset = selectedJar.GetAsset();
        selectionIconImage.Clear();
        if (selectedAsset == null)
        {
            return;
        }
        int num;
        int num2;
        if (selectedAsset.size_x <= selectedAsset.size_y)
        {
            selectionBackdropBox.sizeOffset_X = 490;
            selectionBackdropBox.sizeOffset_Y = 330;
            selectionIconBox.sizeOffset_X = 210;
            selectionIconBox.sizeOffset_Y = 310;
            selectionDescriptionBox.positionOffset_X = 230;
            selectionDescriptionBox.positionOffset_Y = 10;
            selectionDescriptionBox.sizeOffset_X = 250;
            selectionDescriptionBox.sizeOffset_Y = 150;
            selectionActionsBox.positionOffset_X = 230;
            selectionActionsBox.positionOffset_Y = 170;
            selectionActionsBox.sizeOffset_Y = 150;
            if (selectedAsset.size_x == selectedAsset.size_y)
            {
                num = 200;
                num2 = 200;
            }
            else
            {
                num = 200;
                num2 = 300;
            }
        }
        else
        {
            selectionBackdropBox.sizeOffset_X = 530;
            selectionBackdropBox.sizeOffset_Y = 390;
            selectionIconBox.sizeOffset_X = 510;
            selectionIconBox.sizeOffset_Y = 210;
            selectionDescriptionBox.positionOffset_X = 10;
            selectionDescriptionBox.positionOffset_Y = 230;
            selectionDescriptionBox.sizeOffset_X = 250;
            selectionDescriptionBox.sizeOffset_Y = 150;
            selectionActionsBox.positionOffset_X = 270;
            selectionActionsBox.positionOffset_Y = 230;
            selectionActionsBox.sizeOffset_Y = 150;
            num = 500;
            num2 = 200;
        }
        selectionIconImage.positionOffset_X = -num / 2;
        selectionIconImage.positionOffset_Y = -num2 / 2;
        selectionIconImage.sizeOffset_X = num;
        selectionIconImage.sizeOffset_Y = num2;
        selectionIconImage.Refresh(selectedJar.item.id, selectedJar.item.quality, selectedJar.item.state, selectedAsset, num, num2);
        Vector2 vector = Input.mousePosition;
        vector.y = (float)Screen.height - vector.y;
        vector /= GraphicsSettings.userInterfaceScale;
        selectionBackdropBox.positionOffset_X = (int)Mathf.Clamp(vector.x - (float)(selectionBackdropBox.sizeOffset_X / 2), 0f, (float)Screen.width / GraphicsSettings.userInterfaceScale - (float)selectionBackdropBox.sizeOffset_X);
        selectionBackdropBox.positionOffset_Y = (int)Mathf.Clamp(vector.y - (float)(selectionBackdropBox.sizeOffset_Y / 2), 0f, (float)Screen.height / GraphicsSettings.userInterfaceScale - (float)selectionBackdropBox.sizeOffset_Y);
        Local local = localization;
        int rarity = (int)selectedAsset.rarity;
        string arg = local.format("Rarity_" + rarity);
        Local local2 = localization;
        rarity = (int)selectedAsset.type;
        string arg2 = local2.format("Type_" + rarity);
        selectionDescriptionLabel.text = "<color=" + Palette.hex(ItemTool.getRarityColorUI(selectedAsset.rarity)) + ">" + localization.format("Rarity_Type_Label", arg, arg2) + "</color>\n\n";
        if (selectedAsset.showQuality)
        {
            Color32 color = ItemTool.getQualityColor((float)(int)selectedJar.item.quality / 100f);
            ISleekLabel sleekLabel = selectionDescriptionLabel;
            sleekLabel.text = sleekLabel.text + "<color=" + Palette.hex(color) + ">" + localization.format("Quality", selectedJar.item.quality) + "</color>\n\n";
        }
        if (selectedAsset.amount > 1)
        {
            ISleekLabel sleekLabel2 = selectionDescriptionLabel;
            sleekLabel2.text = sleekLabel2.text + localization.format("Amount", selectedJar.item.amount) + "\n\n";
        }
        selectionDescriptionLabel.text = selectedAsset.getContext(selectionDescriptionLabel.text, selectedJar.item.state);
        selectionDescriptionLabel.text += selectedAsset.itemDescription;
        selectionNameLabel.text = selectedAsset.itemName;
        if (selectedPage < PlayerInventory.SLOTS)
        {
            selectionHotkeyLabel.text = localization.format("Hotkey_Set", ControlsSettings.getEquipmentHotkeyText(selectedPage));
            selectionHotkeyLabel.isVisible = true;
        }
        else if (selectedPage < PlayerInventory.STORAGE)
        {
            selectionHotkeyLabel.text = localization.format("Hotkey_Unset");
            selectionHotkeyLabel.isVisible = true;
            for (byte b = 0; b < Player.player.equipment.hotkeys.Length; b = (byte)(b + 1))
            {
                HotkeyInfo hotkeyInfo = Player.player.equipment.hotkeys[b];
                if (hotkeyInfo.page == selectedPage && hotkeyInfo.x == selected_x && hotkeyInfo.y == selected_y)
                {
                    selectionHotkeyLabel.text = localization.format("Hotkey_Set", ControlsSettings.getEquipmentHotkeyText(b + 2));
                    break;
                }
            }
        }
        else
        {
            selectionHotkeyLabel.isVisible = false;
        }
        if (Player.player.equipment.checkSelection(page, x, y))
        {
            selectionEquipButton.text = localization.format("Dequip_Button");
            selectionEquipButton.tooltipText = localization.format("Dequip_Button_Tooltip");
        }
        else
        {
            selectionEquipButton.text = localization.format("Equip_Button");
            selectionEquipButton.tooltipText = localization.format("Equip_Button_Tooltip");
        }
        if (selectedAsset.type == EItemType.GUN)
        {
            selectionContextButton.text = localization.format("Attachments_Button");
            selectionContextButton.tooltipText = localization.format("Attachments_Button_Tooltip");
            selectionContextButton.isVisible = selectedPage >= PlayerInventory.SLOTS && selectedPage < PlayerInventory.AREA;
        }
        else
        {
            selectionContextButton.isVisible = false;
        }
        bool flag = page == PlayerInventory.AREA;
        if (flag)
        {
            selectionDropButton.text = localization.format("Pickup_Button");
            selectionDropButton.tooltipText = localization.format("Pickup_Button_Tooltip");
        }
        else
        {
            selectionDropButton.text = localization.format("Drop_Button");
            selectionDropButton.tooltipText = localization.format("Drop_Button_Tooltip");
        }
        if (page == PlayerInventory.STORAGE)
        {
            selectionStorageButton.text = localization.format("Take_Button");
            selectionStorageButton.tooltipText = localization.format("Take_Button_Tooltip");
        }
        else
        {
            selectionStorageButton.text = localization.format("Store_Button");
            selectionStorageButton.tooltipText = localization.format("Store_Button_Tooltip");
        }
        selectionEquipButton.isVisible = selectedAsset.canPlayerEquip && page < PlayerInventory.PAGES - 2;
        selectionDropButton.isVisible = flag || selectedAsset.allowManualDrop;
        selectionStorageButton.isVisible = Player.player.inventory.isStoring;
        int num3 = 0;
        if (selectionEquipButton.isVisible)
        {
            selectionEquipButton.positionOffset_Y = num3;
            num3 += 40;
        }
        if (selectionContextButton.isVisible)
        {
            selectionContextButton.positionOffset_Y = num3;
            num3 += 40;
        }
        if (selectionDropButton.isVisible)
        {
            selectionDropButton.positionOffset_Y = num3;
            num3 += 40;
        }
        if (selectionStorageButton.isVisible)
        {
            selectionStorageButton.positionOffset_Y = num3;
            num3 += 40;
        }
        selectionExtraActionsBox.RemoveAllChildren();
        selectionExtraActionsBox.positionOffset_Y = num3;
        int num4 = 0;
        if (page != PlayerInventory.AREA)
        {
            actions.Clear();
            for (int i = 0; i < selectedAsset.actions.Count; i++)
            {
                Action action = selectedAsset.actions[i];
                if (action.type == EActionType.BLUEPRINT)
                {
                    if (page < PlayerInventory.SLOTS || page >= PlayerInventory.STORAGE)
                    {
                        continue;
                    }
                    Blueprint blueprint = (Assets.find(EAssetType.ITEM, action.source) as ItemAsset).blueprints[action.blueprints[0].id];
                    if ((blueprint.skill == EBlueprintSkill.REPAIR && blueprint.level > Provider.modeConfigData.Gameplay.Repair_Level_Max) || (blueprint.type == EBlueprintType.REPAIR && selectedJar.item.quality == 100) || !blueprint.areConditionsMet(Player.player) || Player.player.crafting.isBlueprintBlacklisted(blueprint))
                    {
                        continue;
                    }
                }
                actions.Add(action);
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_Y = num4;
                sleekButton.sizeScale_X = 1f;
                sleekButton.sizeOffset_Y = 30;
                if (!string.IsNullOrEmpty(action.key))
                {
                    sleekButton.text = localization.format(action.key + "_Button");
                    sleekButton.tooltipText = localization.format(action.key + "_Button_Tooltip");
                }
                else
                {
                    sleekButton.text = action.text;
                    sleekButton.tooltipText = action.tooltip;
                }
                sleekButton.onClickedButton += onClickedAction;
                selectionExtraActionsBox.AddChild(sleekButton);
                num4 += 40;
                num3 += 40;
            }
        }
        selectionExtraActionsBox.sizeOffset_Y = num4 - 10;
        selectionActionsBox.contentSizeOffset = new Vector2(0f, num3 - 10);
        selectionNameLabel.textColor = ItemTool.getRarityColorUI(selectedAsset.rarity);
    }

    public static void closeSelection()
    {
        if (selectedPage != byte.MaxValue)
        {
            _selectedPage = byte.MaxValue;
            _selected_x = byte.MaxValue;
            _selected_y = byte.MaxValue;
            if (!Glazier.Get().SupportsDepth)
            {
                setItemsEnabled(enabled: true);
                setMiscButtonsEnabled(enabled: true);
            }
            selectionFrame.isVisible = false;
        }
    }

    private static void onSelectedItem(byte page, byte x, byte y)
    {
        if (page == byte.MaxValue || (page == selectedPage && x == selected_x && y == selected_y))
        {
            closeSelection();
        }
        else if (InputEx.GetKey(ControlsSettings.other))
        {
            ItemJar item = Player.player.inventory.getItem(page, Player.player.inventory.getIndex(page, x, y));
            if (item == null)
            {
                return;
            }
            if (Player.player.inventory.isStoring)
            {
                byte x3;
                byte y3;
                byte rot2;
                if (page == PlayerInventory.AREA)
                {
                    if (item.interactableItem != null)
                    {
                        ItemManager.takeItem(item.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, PlayerInventory.STORAGE);
                    }
                }
                else if (page == PlayerInventory.STORAGE)
                {
                    if (Player.player.inventory.tryFindSpace(item.size_x, item.size_y, out var page2, out var x2, out var y2, out var rot))
                    {
                        Player.player.inventory.sendDragItem(page, x, y, page2, x2, y2, rot);
                    }
                }
                else if (Player.player.inventory.tryFindSpace(PlayerInventory.STORAGE, item.size_x, item.size_y, out x3, out y3, out rot2))
                {
                    Player.player.inventory.sendDragItem(page, x, y, PlayerInventory.STORAGE, x3, y3, rot2);
                }
            }
            else
            {
                checkAction(page, x, y, item);
            }
        }
        else
        {
            openSelection(page, x, y);
        }
    }

    private static bool checkSlot(byte page, byte x, byte y, ItemJar jar, byte slot)
    {
        if (Player.player.inventory.checkSpaceEmpty(slot, byte.MaxValue, byte.MaxValue, 0, 0, 0))
        {
            Player.player.inventory.sendDragItem(page, x, y, slot, 0, 0, 0);
            Player.player.equipment.ClientEquipAfterItemDrag(slot, 0, 0);
            PlayerDashboardUI.close();
            PlayerLifeUI.open();
            return true;
        }
        ItemJar item = Player.player.inventory.getItem(slot, 0);
        byte b = item.rot;
        if (!Player.player.inventory.checkSpaceSwap(page, x, y, jar.size_x, jar.size_y, jar.rot, item.size_x, item.size_y, b))
        {
            b = (byte)((b + 1) % 4);
            if (!Player.player.inventory.checkSpaceSwap(page, x, y, jar.size_x, jar.size_y, jar.rot, item.size_x, item.size_y, b))
            {
                return false;
            }
        }
        Player.player.inventory.sendSwapItem(page, x, y, b, slot, 0, 0, jar.rot);
        Player.player.equipment.ClientEquipAfterItemDrag(slot, 0, 0);
        PlayerDashboardUI.close();
        PlayerLifeUI.open();
        return true;
    }

    private static void checkEquip(byte page, byte x, byte y, ItemJar jar, byte slot)
    {
        if (page == PlayerInventory.AREA)
        {
            if (page == selectedPage && x == selected_x && y == selected_y)
            {
                closeSelection();
            }
            if (jar.interactableItem != null)
            {
                ItemManager.takeItem(jar.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
            }
        }
        else if (!Player.player.equipment.checkSelection(page, x, y))
        {
            ItemAsset asset = jar.GetAsset();
            if (asset == null)
            {
                return;
            }
            if (asset.canPlayerEquip && asset.slot.canEquipInPage(page))
            {
                Player.player.equipment.equip(page, x, y);
                PlayerDashboardUI.close();
                PlayerLifeUI.open();
            }
            else if (asset.slot == ESlotType.PRIMARY)
            {
                checkSlot(page, x, y, jar, 0);
            }
            else
            {
                if (asset.slot != ESlotType.SECONDARY)
                {
                    return;
                }
                if (slot == byte.MaxValue)
                {
                    if (!checkSlot(page, x, y, jar, 1))
                    {
                        checkSlot(page, x, y, jar, 0);
                    }
                }
                else
                {
                    checkSlot(page, x, y, jar, slot);
                }
            }
        }
        else if (Player.player.equipment.isSelected && !Player.player.equipment.isBusy && Player.player.equipment.isEquipped)
        {
            Player.player.equipment.dequip();
            if (page == selectedPage && x == selected_x && y == selected_y)
            {
                closeSelection();
            }
        }
    }

    private static void checkAction(byte page, byte x, byte y, ItemJar jar)
    {
        if (page == PlayerInventory.AREA)
        {
            if (jar.interactableItem != null)
            {
                ItemManager.takeItem(jar.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
            }
            return;
        }
        ItemAsset asset = jar.GetAsset();
        if (asset != null)
        {
            if (asset.type == EItemType.HAT)
            {
                Player.player.clothing.sendSwapHat(page, x, y);
            }
            else if (asset.type == EItemType.SHIRT)
            {
                Player.player.clothing.sendSwapShirt(page, x, y);
            }
            else if (asset.type == EItemType.PANTS)
            {
                Player.player.clothing.sendSwapPants(page, x, y);
            }
            else if (asset.type == EItemType.BACKPACK)
            {
                Player.player.clothing.sendSwapBackpack(page, x, y);
            }
            else if (asset.type == EItemType.VEST)
            {
                Player.player.clothing.sendSwapVest(page, x, y);
            }
            else if (asset.type == EItemType.MASK)
            {
                Player.player.clothing.sendSwapMask(page, x, y);
            }
            else if (asset.type == EItemType.GLASSES)
            {
                Player.player.clothing.sendSwapGlasses(page, x, y);
            }
            else if (asset.canPlayerEquip)
            {
                checkEquip(page, x, y, jar, byte.MaxValue);
            }
        }
    }

    private static void onGrabbedItem(byte page, byte x, byte y, SleekItem item)
    {
        if (InputEx.GetKey(ControlsSettings.other))
        {
            if (page == PlayerInventory.AREA)
            {
                if (item.jar.interactableItem == null)
                {
                    UnturnedLog.warn("onGrabbedItem nearby without interactable");
                }
                else
                {
                    ItemManager.takeItem(item.jar.interactableItem.transform.parent, byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
                }
            }
            else
            {
                Player.player.inventory.sendDropItem(page, x, y);
            }
            return;
        }
        dragJar = Player.player.inventory.getItem(page, Player.player.inventory.getIndex(page, x, y));
        if (dragJar != null)
        {
            dragSource = item;
            dragFromPage = page;
            dragFrom_x = x;
            dragFrom_y = y;
            dragFromRot = dragJar.rot;
            dragOffset = -item.GetNormalizedCursorPosition();
            dragOffset.x *= item.sizeOffset_X;
            dragOffset.y *= item.sizeOffset_Y;
            if (dragJar.rot == 1)
            {
                float x2 = dragOffset.x;
                dragOffset.x = dragOffset.y;
                dragOffset.y = 0f - ((float)(dragJar.size_y * 50) + x2);
            }
            else if (dragJar.rot == 2)
            {
                dragOffset.x = 0f - ((float)(dragJar.size_x * 50) + dragOffset.x);
                dragOffset.y = 0f - ((float)(dragJar.size_y * 50) + dragOffset.y);
            }
            else if (dragJar.rot == 3)
            {
                float x3 = dragOffset.x;
                dragOffset.x = 0f - ((float)(dragJar.size_x * 50) + dragOffset.y);
                dragOffset.y = x3;
            }
            updatePivot();
            dragItem.updateItem(dragJar);
            refreshDraggedVisualPosition();
            startDrag();
        }
    }

    private static void onPlacedItem(byte page, byte x, byte y)
    {
        ConsumeEvent();
        if (dragSource == null || !isDragging)
        {
            return;
        }
        if (page >= PlayerInventory.SLOTS)
        {
            int num = x + (int)(dragPivot.x / 50f);
            int num2 = y + (int)(dragPivot.y / 50f);
            if (num < 0)
            {
                num = 0;
            }
            if (num2 < 0)
            {
                num2 = 0;
            }
            byte b = dragJar.size_x;
            byte b2 = dragJar.size_y;
            if ((int)dragJar.rot % 2 == 1)
            {
                b = dragJar.size_y;
                b2 = dragJar.size_x;
            }
            if (num >= Player.player.inventory.getWidth(page) - b)
            {
                num = (byte)(Player.player.inventory.getWidth(page) - b);
            }
            if (num2 >= Player.player.inventory.getHeight(page) - b2)
            {
                num2 = (byte)(Player.player.inventory.getHeight(page) - b2);
            }
            x = (byte)num;
            y = (byte)num2;
        }
        ItemAsset asset = dragJar.GetAsset();
        if (asset == null || (page < PlayerInventory.SLOTS && !asset.slot.canEquipInPage(page)))
        {
            return;
        }
        if (dragFromPage == page && dragFrom_x == x && dragFrom_y == y && dragFromRot == dragJar.rot)
        {
            stopDrag();
            return;
        }
        if (page == PlayerInventory.AREA)
        {
            stopDrag();
            if (page != dragFromPage)
            {
                Player.player.inventory.sendDropItem(dragFromPage, dragFrom_x, dragFrom_y);
            }
            return;
        }
        if (dragFromPage == PlayerInventory.AREA)
        {
            byte rot = dragJar.rot;
            stopDrag();
            if (page != dragFromPage && Player.player.inventory.checkSpaceEmpty(page, x, y, dragJar.size_x, dragJar.size_y, rot) && dragItem.jar != null && dragItem.jar.interactableItem != null)
            {
                ItemManager.takeItem(dragItem.jar.interactableItem.transform.parent, x, y, rot, page);
            }
            return;
        }
        if (Player.player.inventory.checkSpaceDrag(page, dragFrom_x, dragFrom_y, dragFromRot, x, y, dragJar.rot, dragJar.size_x, dragJar.size_y, page == dragFromPage))
        {
            byte rot2 = dragJar.rot;
            stopDrag();
            Player.player.inventory.sendDragItem(dragFromPage, dragFrom_x, dragFrom_y, page, x, y, rot2);
            if (page < PlayerInventory.SLOTS)
            {
                Player.player.equipment.equip(page, 0, 0);
                PlayerDashboardUI.close();
                PlayerLifeUI.open();
            }
            return;
        }
        if (page < PlayerInventory.SLOTS)
        {
            stopDrag();
            checkEquip(dragFromPage, dragFrom_x, dragFrom_y, dragJar, page);
            return;
        }
        byte find_x;
        byte find_y;
        byte b3 = Player.player.inventory.findIndex(page, x, y, out find_x, out find_y);
        if (b3 == byte.MaxValue)
        {
            return;
        }
        if (dragFromPage == page && dragFrom_x == find_x && dragFrom_y == find_y)
        {
            stopDrag();
            return;
        }
        ItemJar item = Player.player.inventory.getItem(page, b3);
        if (!Player.player.inventory.checkSpaceSwap(page, find_x, find_y, item.size_x, item.size_y, item.rot, dragJar.size_x, dragJar.size_y, dragJar.rot))
        {
            return;
        }
        byte b4 = item.rot;
        if (!Player.player.inventory.checkSpaceSwap(dragFromPage, dragFrom_x, dragFrom_y, dragJar.size_x, dragJar.size_y, dragFromRot, item.size_x, item.size_y, b4))
        {
            b4 = (byte)((b4 + 1) % 4);
            if (!Player.player.inventory.checkSpaceSwap(dragFromPage, dragFrom_x, dragFrom_y, dragJar.size_x, dragJar.size_y, dragFromRot, item.size_x, item.size_y, b4))
            {
                return;
            }
        }
        ItemAsset asset2 = item.GetAsset();
        if (asset2 != null && (dragFromPage >= PlayerInventory.SLOTS || asset2.slot.canEquipInPage(dragFromPage)))
        {
            byte rot3 = dragJar.rot;
            stopDrag();
            Player.player.inventory.sendSwapItem(page, find_x, find_y, rot3, dragFromPage, dragFrom_x, dragFrom_y, b4);
            if (dragFromPage < PlayerInventory.SLOTS)
            {
                checkEquip(dragFromPage, dragFrom_x, dragFrom_y, dragJar, page);
            }
        }
    }

    private static void onClickedCharacter()
    {
        if (dragSource != null && isDragging)
        {
            byte page = dragFromPage;
            byte x = dragFrom_x;
            byte y = dragFrom_y;
            ItemJar jar = dragJar;
            stopDrag();
            checkAction(page, x, y, jar);
        }
        else
        {
            Vector2 normalizedCursorPosition = characterImage.GetNormalizedCursorPosition();
            Vector3 pos = new Vector3(normalizedCursorPosition.x, 1f - normalizedCursorPosition.y, 0f);
            Ray ray = Player.player.look.characterCamera.ViewportPointToRay(pos);
            Physics.Raycast(ray, out var hitInfo, 8f, RayMasks.CLOTHING_INTERACT);
            RuntimeGizmos.Get().Raycast(ray, 8f, hitInfo, Color.green, Color.red, 10f);
            if (hitInfo.collider != null)
            {
                Transform transform = hitInfo.collider.transform;
                if (transform.CompareTag("Player"))
                {
                    switch (DamageTool.getLimb(transform))
                    {
                    case ELimb.LEFT_FOOT:
                    case ELimb.LEFT_LEG:
                    case ELimb.RIGHT_FOOT:
                    case ELimb.RIGHT_LEG:
                        Player.player.clothing.sendSwapPants(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                        break;
                    case ELimb.LEFT_HAND:
                    case ELimb.LEFT_ARM:
                    case ELimb.RIGHT_HAND:
                    case ELimb.RIGHT_ARM:
                    case ELimb.SPINE:
                        Player.player.clothing.sendSwapShirt(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                        break;
                    }
                }
                else if (transform.CompareTag("Enemy"))
                {
                    if (transform.name == "Hat")
                    {
                        Player.player.clothing.sendSwapHat(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    }
                    else if (transform.name == "Glasses")
                    {
                        Player.player.clothing.sendSwapGlasses(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    }
                    else if (transform.name == "Mask")
                    {
                        Player.player.clothing.sendSwapMask(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    }
                    else if (transform.name == "Vest")
                    {
                        Player.player.clothing.sendSwapVest(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    }
                    else if (transform.name == "Backpack")
                    {
                        Player.player.clothing.sendSwapBackpack(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    }
                }
                else if (transform.CompareTag("Item"))
                {
                    Player.player.equipment.dequip();
                }
            }
        }
        ConsumeEvent();
    }

    private static void onClickedOutsideSelection()
    {
        closeSelection();
    }

    private static void onClickedDuringDrag()
    {
        if (dragSource != null && isDragging)
        {
            byte b = dragFromPage;
            byte x = dragFrom_x;
            byte y = dragFrom_y;
            stopDrag();
            if (b != PlayerInventory.AREA)
            {
                Player.player.inventory.sendDropItem(b, x, y);
            }
            ConsumeEvent();
        }
    }

    private static void onRightClickedDuringDrag()
    {
        if (dragSource != null && isDragging)
        {
            stopDrag();
        }
    }

    private static void onItemDropAdded(Transform model, InteractableItem interactableItem)
    {
        if (active && PlayerDashboardUI.active && !(Player.player == null) && areaItems.getItemCount() < 200)
        {
            Vector3 eyesPositionWithoutLeaning = Player.player.look.GetEyesPositionWithoutLeaning();
            if (!((model.position - eyesPositionWithoutLeaning).sqrMagnitude > 16f))
            {
                pendingItemsInRadius.Add(interactableItem);
            }
        }
    }

    private static void onItemDropRemoved(Transform model, InteractableItem interactableItem)
    {
        if (!active || !PlayerDashboardUI.active)
        {
            return;
        }
        ItemJar jar = interactableItem.jar;
        if (jar != null)
        {
            byte index = areaItems.getIndex(jar.x, jar.y);
            if (index == byte.MaxValue)
            {
                pendingItemsInRadius.RemoveFast(interactableItem);
                return;
            }
            areaItems.removeItem(index);
            items[PlayerInventory.AREA - PlayerInventory.SLOTS].removeItem(jar);
        }
    }

    private static void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
    {
        if (oldVehicle != null)
        {
            oldVehicle.onPassengersUpdated -= updateVehicle;
            oldVehicle.onLockUpdated -= onVehicleLockUpdated;
            oldVehicle.onHeadlightsUpdated -= updateVehicle;
            oldVehicle.onSirensUpdated -= updateVehicle;
            oldVehicle.onBlimpUpdated -= updateVehicle;
            oldVehicle.batteryChanged -= updateVehicle;
            oldVehicle.skinChanged -= updateVehicle;
        }
        if (newVehicle != null)
        {
            newVehicle.onPassengersUpdated += updateVehicle;
            newVehicle.onLockUpdated += onVehicleLockUpdated;
            newVehicle.onHeadlightsUpdated += updateVehicle;
            newVehicle.onSirensUpdated += updateVehicle;
            newVehicle.onBlimpUpdated += updateVehicle;
            newVehicle.batteryChanged += updateVehicle;
            newVehicle.skinChanged += updateVehicle;
        }
        updateVehicle();
    }

    private static void onVehicleLockUpdated()
    {
        updateVehicle();
        InteractableVehicle vehicle = Player.player.movement.getVehicle();
        if (!(vehicle == null))
        {
            PlayerUI.message(vehicle.isLocked ? EPlayerMessage.VEHICLE_LOCKED : EPlayerMessage.VEHICLE_UNLOCKED, string.Empty);
        }
    }

    private static void updateVehicle()
    {
        if (!active)
        {
            return;
        }
        InteractableVehicle vehicle = Player.player.movement.getVehicle();
        if (vehicle != null && vehicle.asset != null)
        {
            VehicleAsset asset = vehicle.asset;
            vehicleNameLabel.text = asset.vehicleName;
            vehicleNameLabel.textColor = ItemTool.getRarityColorUI(asset.rarity);
            int num = 0;
            int num2 = 0;
            if (asset.canBeLocked)
            {
                vehicleLockButton.text = localization.format(vehicle.isLocked ? "Vehicle_Lock_Off" : "Vehicle_Lock_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                vehicleLockButton.tooltipText = localization.format("Vehicle_Lock_Tooltip");
                vehicleLockButton.isVisible = true;
                vehicleLockButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleLockButton.isVisible = false;
            }
            if (asset.horn != null)
            {
                vehicleHornButton.text = localization.format("Vehicle_Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
                vehicleHornButton.tooltipText = localization.format("Vehicle_Horn_Tooltip");
                vehicleHornButton.isVisible = true;
                vehicleHornButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleHornButton.isVisible = false;
            }
            if (asset.hasHeadlights)
            {
                vehicleHeadlightsButton.text = localization.format(vehicle.headlightsOn ? "Vehicle_Headlights_Off" : "Vehicle_Headlights_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
                vehicleHeadlightsButton.tooltipText = localization.format("Vehicle_Headlights_Tooltip");
                vehicleHeadlightsButton.isVisible = true;
                vehicleHeadlightsButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleHeadlightsButton.isVisible = false;
            }
            if (asset.hasSirens)
            {
                vehicleSirensButton.text = localization.format(vehicle.sirensOn ? "Vehicle_Sirens_Off" : "Vehicle_Sirens_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
                vehicleSirensButton.tooltipText = localization.format("Vehicle_Sirens_Tooltip");
                vehicleSirensButton.isVisible = true;
                vehicleSirensButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleSirensButton.isVisible = false;
            }
            if (asset.engine == EEngine.BLIMP)
            {
                vehicleBlimpButton.text = localization.format(vehicle.isBlimpFloating ? "Vehicle_Blimp_Off" : "Vehicle_Blimp_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
                vehicleBlimpButton.tooltipText = localization.format("Vehicle_Blimp_Tooltip");
                vehicleBlimpButton.isVisible = true;
                vehicleBlimpButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleBlimpButton.isVisible = false;
            }
            if (asset.hasHook)
            {
                vehicleHookButton.text = localization.format("Vehicle_Hook", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
                vehicleHookButton.tooltipText = localization.format("Vehicle_Hook_Tooltip");
                vehicleHookButton.isVisible = true;
                vehicleHookButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleHookButton.isVisible = false;
            }
            if (vehicle.usesBattery && vehicle.hasBattery && vehicle.asset.canStealBattery)
            {
                vehicleStealBatteryButton.text = localization.format("Vehicle_Steal_Battery");
                vehicleStealBatteryButton.tooltipText = localization.format("Vehicle_Steal_Battery_Tooltip");
                vehicleStealBatteryButton.isVisible = true;
                vehicleStealBatteryButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleStealBatteryButton.isVisible = false;
            }
            int value = 0;
            ushort num3 = 0;
            ushort num4 = 0;
            if (Player.player.channel.owner.skinItems != null && Player.player.channel.owner.vehicleSkins != null && Player.player.channel.owner.vehicleSkins.TryGetValue(vehicle.asset.sharedSkinLookupID, out value))
            {
                num3 = Provider.provider.economyService.getInventorySkinID(value);
                num4 = Provider.provider.economyService.getInventoryMythicID(value);
            }
            bool flag;
            bool flag2;
            if (num3 != 0)
            {
                if (num3 == vehicle.skinID && num4 == vehicle.mythicID)
                {
                    flag = true;
                    flag2 = true;
                }
                else
                {
                    flag = false;
                    flag2 = true;
                }
            }
            else if (vehicle.isSkinned)
            {
                flag = true;
                flag2 = true;
            }
            else
            {
                flag = false;
                flag2 = false;
            }
            if (flag2)
            {
                vehicleSkinButton.text = localization.format(flag ? "Vehicle_Skin_Off" : "Vehicle_Skin_On");
                vehicleSkinButton.tooltipText = localization.format("Vehicle_Skin_Tooltip");
                vehicleSkinButton.isVisible = true;
                vehicleSkinButton.positionOffset_Y = num;
                num += 40;
            }
            else
            {
                vehicleSkinButton.isVisible = false;
            }
            if (Player.player.stance.stance == EPlayerStance.DRIVING)
            {
                vehiclePassengersBox.positionOffset_X = 270;
                vehiclePassengersBox.sizeOffset_X = -280;
                vehicleActionsBox.isVisible = true;
            }
            else
            {
                vehiclePassengersBox.positionOffset_X = 10;
                vehiclePassengersBox.sizeOffset_X = -20;
                vehicleActionsBox.isVisible = false;
            }
            vehiclePassengersBox.RemoveAllChildren();
            for (int i = 0; i < vehicle.passengers.Length; i++)
            {
                Passenger passenger = vehicle.passengers[i];
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_Y = num2;
                sleekButton.sizeOffset_Y = 30;
                sleekButton.sizeScale_X = 1f;
                sleekButton.onClickedButton += onClickedVehiclePassengerButton;
                vehiclePassengersBox.AddChild(sleekButton);
                if (passenger.player != null)
                {
                    string localDisplayName = passenger.player.GetLocalDisplayName();
                    if (i < 12)
                    {
                        sleekButton.text = localization.format("Vehicle_Seat_Slot", localDisplayName, "F" + (i + 1));
                    }
                    else
                    {
                        sleekButton.text = localDisplayName;
                    }
                }
                else if (i < 12)
                {
                    sleekButton.text = localization.format("Vehicle_Seat_Slot", localization.format("Vehicle_Seat_Empty"), "F" + (i + 1));
                }
                else
                {
                    sleekButton.text = localization.format("Vehicle_Seat_Empty");
                }
                num2 += 40;
            }
            vehicleActionsBox.sizeOffset_Y = num - 10;
            vehiclePassengersBox.sizeOffset_Y = num2 - 10;
            vehicleBox.isVisible = true;
            int num5 = Mathf.Max(num, num2);
            vehicleBox.sizeOffset_Y = 60 + num5;
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].textColor = vehicleNameLabel.textColor;
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].text = localization.format("Storage_Trunk", vehicleNameLabel.text);
        }
        else
        {
            vehicleBox.isVisible = false;
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].textColor = ESleekTint.FONT;
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].text = localization.format("Storage");
        }
        updateBoxAreas();
    }

    private static void resetNearbyDrops()
    {
        Vector3 eyesPositionWithoutLeaning = Player.player.look.GetEyesPositionWithoutLeaning();
        pendingItemsInRadius.Clear();
        ItemManager.findSimulatedItemsInRadius(eyesPositionWithoutLeaning, 16f, pendingItemsInRadius);
        areaItems.clear();
        areaItems.resize(8, 3);
        Player.player.inventory.replaceItems(PlayerInventory.AREA, areaItems);
        SleekItems obj = items[PlayerInventory.AREA - PlayerInventory.SLOTS];
        obj.clear();
        obj.resize(areaItems.width, areaItems.height);
        updateBoxAreas();
    }

    private static void onInventoryResized(byte page, byte newWidth, byte newHeight)
    {
        if (page >= PlayerInventory.SLOTS)
        {
            page = (byte)(page - PlayerInventory.SLOTS);
            items[page].resize(newWidth, newHeight);
            if (page > 0)
            {
                headers[page].isVisible = newHeight > 0;
            }
            items[page].isVisible = newHeight > 0;
            if (page == PlayerInventory.STORAGE - PlayerInventory.SLOTS && newHeight == 0)
            {
                items[page].clear();
            }
            updateBoxAreas();
        }
    }

    private static void updateBoxAreas()
    {
        int num = 0;
        int num2 = 0;
        bool flag = isSplitClothingArea;
        if (vehicleBox.isVisible)
        {
            if (flag)
            {
                if (vehicleBox.parent != areaBox)
                {
                    areaBox.AddChild(vehicleBox);
                }
                vehicleBox.positionOffset_Y = num2;
                num2 += vehicleBox.sizeOffset_Y + 10;
            }
            else
            {
                if (vehicleBox.parent != clothingBox)
                {
                    clothingBox.AddChild(vehicleBox);
                }
                vehicleBox.positionOffset_Y = num;
                num += vehicleBox.sizeOffset_Y + 10;
            }
        }
        for (byte b = 0; b < items.Length; b = (byte)(b + 1))
        {
            if (headers[b].isVisible)
            {
                if (flag && (b == PlayerInventory.STORAGE - PlayerInventory.SLOTS || b == PlayerInventory.AREA - PlayerInventory.SLOTS))
                {
                    if (headers[b].parent != areaBox)
                    {
                        areaBox.AddChild(headers[b]);
                    }
                    if (items[b].parent != areaBox)
                    {
                        areaBox.AddChild(items[b]);
                    }
                    headers[b].positionOffset_Y = num2;
                    items[b].positionOffset_Y = num2 + 70;
                    num2 += items[b].sizeOffset_Y + 80;
                }
                else
                {
                    if (headers[b].parent != clothingBox)
                    {
                        clothingBox.AddChild(headers[b]);
                    }
                    if (items[b].parent != clothingBox)
                    {
                        clothingBox.AddChild(items[b]);
                    }
                    headers[b].positionOffset_Y = num;
                    items[b].positionOffset_Y = num + 70;
                    num += items[b].sizeOffset_Y + 80;
                }
            }
        }
        headers[7].isVisible = Player.player.clothing.hatAsset != null;
        if (headers[7].isVisible)
        {
            headers[7].positionOffset_Y = num;
            num += 70;
        }
        headers[8].isVisible = Player.player.clothing.maskAsset != null;
        if (headers[8].isVisible)
        {
            headers[8].positionOffset_Y = num;
            num += 70;
        }
        headers[9].isVisible = Player.player.clothing.glassesAsset != null;
        if (headers[9].isVisible)
        {
            headers[9].positionOffset_Y = num;
            num += 70;
        }
        clothingBox.contentSizeOffset = new Vector2(0f, num - 10);
        areaBox.contentSizeOffset = new Vector2(0f, num2 - 10);
        InteractableStorage interactableStorage = PlayerInteract.interactable as InteractableStorage;
        if (interactableStorage != null && interactableStorage.isDisplay)
        {
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].sizeOffset_X = -180;
            rot_xButton.isVisible = true;
            rot_yButton.isVisible = true;
            rot_zButton.isVisible = true;
        }
        else
        {
            headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].sizeOffset_X = 0;
            rot_xButton.isVisible = false;
            rot_yButton.isVisible = false;
            rot_zButton.isVisible = false;
        }
    }

    private static void updateHotkeys()
    {
        for (byte b = 0; b < PlayerInventory.STORAGE - PlayerInventory.SLOTS; b = (byte)(b + 1))
        {
            items[b].resetHotkeyDisplay();
        }
        for (byte b2 = 0; b2 < Player.player.equipment.hotkeys.Length; b2 = (byte)(b2 + 1))
        {
            HotkeyInfo hotkeyInfo = Player.player.equipment.hotkeys[b2];
            byte button = (byte)(b2 + 2);
            byte b3 = (byte)(hotkeyInfo.page - 2);
            if (hotkeyInfo.id != 0)
            {
                byte index = Player.player.inventory.getIndex(hotkeyInfo.page, hotkeyInfo.x, hotkeyInfo.y);
                ItemJar item = Player.player.inventory.getItem(hotkeyInfo.page, index);
                if (item == null || item.item.id != hotkeyInfo.id)
                {
                    hotkeyInfo.id = 0;
                    hotkeyInfo.page = byte.MaxValue;
                    hotkeyInfo.x = byte.MaxValue;
                    hotkeyInfo.y = byte.MaxValue;
                }
                else
                {
                    items[b3].updateHotkey(item, button);
                }
            }
        }
    }

    private static void onHotkeysUpdated()
    {
        updateHotkeys();
    }

    private static void onInventoryUpdated(byte page, byte index, ItemJar jar)
    {
        if (page < PlayerInventory.SLOTS)
        {
            slots[page].updateItem(jar);
            return;
        }
        page = (byte)(page - PlayerInventory.SLOTS);
        items[page].updateItem(jar);
    }

    private static void onInventoryAdded(byte page, byte index, ItemJar jar)
    {
        if (page < PlayerInventory.SLOTS)
        {
            slots[page].applyItem(jar);
            return;
        }
        page = (byte)(page - PlayerInventory.SLOTS);
        items[page].addItem(jar);
    }

    private static void onInventoryRemoved(byte page, byte index, ItemJar jar)
    {
        if (page == selectedPage && jar.x == selected_x && jar.y == selected_y)
        {
            closeSelection();
        }
        if (page < PlayerInventory.SLOTS)
        {
            slots[page].applyItem(null);
            return;
        }
        page = (byte)(page - PlayerInventory.SLOTS);
        items[page].removeItem(jar);
    }

    private static void onInventoryStored()
    {
        if (Player.player.inventory.shouldStorageOpenDashboard)
        {
            PlayerLifeUI.close();
            PlayerPauseUI.close();
            if (PlayerDashboardUI.active)
            {
                PlayerDashboardCraftingUI.close();
                PlayerDashboardSkillsUI.close();
                PlayerDashboardInformationUI.close();
                open();
            }
            else
            {
                active = true;
                PlayerDashboardCraftingUI.active = false;
                PlayerDashboardSkillsUI.active = false;
                PlayerDashboardInformationUI.active = false;
                PlayerDashboardUI.open();
            }
        }
        if (!isSplitClothingArea)
        {
            clothingBox.ScrollToBottom();
        }
    }

    private static void onShirtUpdated(ushort newShirtObsolete, byte newShirtQuality, byte[] newShirtState)
    {
        ItemAsset shirtAsset = Player.player.clothing.shirtAsset;
        if (shirtAsset != null)
        {
            headers[3].text = shirtAsset.itemName;
            headers[3].GetChildAtIndex(0).sizeOffset_X = shirtAsset.size_x * 25;
            headers[3].GetChildAtIndex(0).sizeOffset_Y = shirtAsset.size_y * 25;
            headers[3].GetChildAtIndex(0).positionOffset_Y = -headers[3].GetChildAtIndex(0).sizeOffset_Y / 2;
            headerItemIcons[3].Refresh(shirtAsset.id, newShirtQuality, newShirtState);
            ((ISleekLabel)headers[3].GetChildAtIndex(2)).text = newShirtQuality + "%";
            Color rarityColorUI = ItemTool.getRarityColorUI(shirtAsset.rarity);
            headers[3].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
            headers[3].textColor = rarityColorUI;
            Color qualityColor = ItemTool.getQualityColor((float)(int)newShirtQuality / 100f);
            ((ISleekImage)headers[3].GetChildAtIndex(1)).color = qualityColor;
            ((ISleekLabel)headers[3].GetChildAtIndex(2)).textColor = qualityColor;
        }
    }

    private static void onPantsUpdated(ushort newPantsObsolete, byte newPantsQuality, byte[] newPantsState)
    {
        if (headers != null)
        {
            ItemAsset pantsAsset = Player.player.clothing.pantsAsset;
            if (pantsAsset != null)
            {
                headers[4].text = pantsAsset.itemName;
                headers[4].GetChildAtIndex(0).sizeOffset_X = pantsAsset.size_x * 25;
                headers[4].GetChildAtIndex(0).sizeOffset_Y = pantsAsset.size_y * 25;
                headers[4].GetChildAtIndex(0).positionOffset_Y = -headers[4].GetChildAtIndex(0).sizeOffset_Y / 2;
                headerItemIcons[4].Refresh(pantsAsset.id, newPantsQuality, newPantsState);
                ((ISleekLabel)headers[4].GetChildAtIndex(2)).text = newPantsQuality + "%";
                Color rarityColorUI = ItemTool.getRarityColorUI(pantsAsset.rarity);
                headers[4].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
                headers[4].textColor = rarityColorUI;
                Color qualityColor = ItemTool.getQualityColor((float)(int)newPantsQuality / 100f);
                ((ISleekImage)headers[4].GetChildAtIndex(1)).color = qualityColor;
                ((ISleekLabel)headers[4].GetChildAtIndex(2)).textColor = qualityColor;
            }
        }
    }

    private static void onHatUpdated(ushort newHatObsolete, byte newHatQuality, byte[] newHatState)
    {
        if (headers != null)
        {
            ItemAsset hatAsset = Player.player.clothing.hatAsset;
            if (hatAsset != null)
            {
                headers[7].text = hatAsset.itemName;
                headers[7].GetChildAtIndex(0).sizeOffset_X = hatAsset.size_x * 25;
                headers[7].GetChildAtIndex(0).sizeOffset_Y = hatAsset.size_y * 25;
                headers[7].GetChildAtIndex(0).positionOffset_Y = -headers[7].GetChildAtIndex(0).sizeOffset_Y / 2;
                headerItemIcons[7].Refresh(newHatObsolete, newHatQuality, newHatState);
                ((ISleekLabel)headers[7].GetChildAtIndex(2)).text = newHatQuality + "%";
                Color rarityColorUI = ItemTool.getRarityColorUI(hatAsset.rarity);
                headers[7].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
                headers[7].textColor = rarityColorUI;
                Color qualityColor = ItemTool.getQualityColor((float)(int)newHatQuality / 100f);
                ((ISleekImage)headers[7].GetChildAtIndex(1)).color = qualityColor;
                ((ISleekLabel)headers[7].GetChildAtIndex(2)).textColor = qualityColor;
            }
            headers[7].isVisible = hatAsset != null;
            updateBoxAreas();
        }
    }

    private static void onBackpackUpdated(ushort newBackpackObsolete, byte newBackpackQuality, byte[] newBackpackState)
    {
        ItemAsset backpackAsset = Player.player.clothing.backpackAsset;
        if (backpackAsset != null)
        {
            headers[1].text = backpackAsset.itemName;
            headers[1].GetChildAtIndex(0).sizeOffset_X = backpackAsset.size_x * 25;
            headers[1].GetChildAtIndex(0).sizeOffset_Y = backpackAsset.size_y * 25;
            headers[1].GetChildAtIndex(0).positionOffset_Y = -headers[1].GetChildAtIndex(0).sizeOffset_Y / 2;
            headerItemIcons[1].Refresh(backpackAsset.id, newBackpackQuality, newBackpackState);
            ((ISleekLabel)headers[1].GetChildAtIndex(2)).text = newBackpackQuality + "%";
            Color rarityColorUI = ItemTool.getRarityColorUI(backpackAsset.rarity);
            headers[1].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
            headers[1].textColor = rarityColorUI;
            Color qualityColor = ItemTool.getQualityColor((float)(int)newBackpackQuality / 100f);
            ((ISleekImage)headers[1].GetChildAtIndex(1)).color = qualityColor;
            ((ISleekLabel)headers[1].GetChildAtIndex(2)).textColor = qualityColor;
        }
    }

    private static void onVestUpdated(ushort newVestObsolete, byte newVestQuality, byte[] newVestState)
    {
        ItemAsset vestAsset = Player.player.clothing.vestAsset;
        if (vestAsset != null)
        {
            headers[2].text = vestAsset.itemName;
            headers[2].GetChildAtIndex(0).sizeOffset_X = vestAsset.size_x * 25;
            headers[2].GetChildAtIndex(0).sizeOffset_Y = vestAsset.size_y * 25;
            headers[2].GetChildAtIndex(0).positionOffset_Y = -headers[2].GetChildAtIndex(0).sizeOffset_Y / 2;
            headerItemIcons[2].Refresh(vestAsset.id, newVestQuality, newVestState);
            ((ISleekLabel)headers[2].GetChildAtIndex(2)).text = newVestQuality + "%";
            Color rarityColorUI = ItemTool.getRarityColorUI(vestAsset.rarity);
            headers[2].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
            headers[2].textColor = rarityColorUI;
            Color qualityColor = ItemTool.getQualityColor((float)(int)newVestQuality / 100f);
            ((ISleekImage)headers[2].GetChildAtIndex(1)).color = qualityColor;
            ((ISleekLabel)headers[2].GetChildAtIndex(2)).textColor = qualityColor;
        }
    }

    private static void onMaskUpdated(ushort newMaskObsolete, byte newMaskQuality, byte[] newMaskState)
    {
        if (headers != null)
        {
            ItemAsset maskAsset = Player.player.clothing.maskAsset;
            if (maskAsset != null)
            {
                headers[8].text = maskAsset.itemName;
                headers[8].GetChildAtIndex(0).sizeOffset_X = maskAsset.size_x * 25;
                headers[8].GetChildAtIndex(0).sizeOffset_Y = maskAsset.size_y * 25;
                headers[8].GetChildAtIndex(0).positionOffset_Y = -headers[8].GetChildAtIndex(0).sizeOffset_Y / 2;
                headerItemIcons[8].Refresh(maskAsset.id, newMaskQuality, newMaskState);
                ((ISleekLabel)headers[8].GetChildAtIndex(2)).text = newMaskQuality + "%";
                Color rarityColorUI = ItemTool.getRarityColorUI(maskAsset.rarity);
                headers[8].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
                headers[8].textColor = rarityColorUI;
                Color qualityColor = ItemTool.getQualityColor((float)(int)newMaskQuality / 100f);
                ((ISleekImage)headers[8].GetChildAtIndex(1)).color = qualityColor;
                ((ISleekLabel)headers[8].GetChildAtIndex(2)).textColor = qualityColor;
            }
            headers[8].isVisible = maskAsset != null;
            updateBoxAreas();
        }
    }

    private static void onGlassesUpdated(ushort newGlassesObsolete, byte newGlassesQuality, byte[] newGlassesState)
    {
        if (headers != null)
        {
            ItemAsset glassesAsset = Player.player.clothing.glassesAsset;
            if (glassesAsset != null)
            {
                headers[9].text = glassesAsset.itemName;
                headers[9].GetChildAtIndex(0).sizeOffset_X = glassesAsset.size_x * 25;
                headers[9].GetChildAtIndex(0).sizeOffset_Y = glassesAsset.size_y * 25;
                headers[9].GetChildAtIndex(0).positionOffset_Y = -headers[9].GetChildAtIndex(0).sizeOffset_Y / 2;
                headerItemIcons[9].Refresh(glassesAsset.id, newGlassesQuality, newGlassesState);
                ((ISleekLabel)headers[9].GetChildAtIndex(2)).text = newGlassesQuality + "%";
                Color rarityColorUI = ItemTool.getRarityColorUI(glassesAsset.rarity);
                headers[9].backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
                headers[9].textColor = rarityColorUI;
                Color qualityColor = ItemTool.getQualityColor((float)(int)newGlassesQuality / 100f);
                ((ISleekImage)headers[9].GetChildAtIndex(1)).color = qualityColor;
                ((ISleekLabel)headers[9].GetChildAtIndex(2)).textColor = qualityColor;
            }
            headers[9].isVisible = glassesAsset != null;
            updateBoxAreas();
        }
    }

    private static void onClickedHeader(ISleekElement button)
    {
        int i;
        for (i = 0; i < headers.Length && headers[i] != button; i++)
        {
        }
        switch (i)
        {
        case 0:
            if (Player.player.equipment.isSelected && !Player.player.equipment.isBusy && Player.player.equipment.isEquipped)
            {
                Player.player.equipment.dequip();
            }
            break;
        case 1:
            Player.player.clothing.sendSwapBackpack(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 2:
            Player.player.clothing.sendSwapVest(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 3:
            Player.player.clothing.sendSwapShirt(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 4:
            Player.player.clothing.sendSwapPants(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 5:
            PlayerDashboardUI.close();
            PlayerLifeUI.open();
            break;
        case 6:
            PlayerDashboardUI.close();
            PlayerLifeUI.open();
            break;
        case 7:
            Player.player.clothing.sendSwapHat(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 8:
            Player.player.clothing.sendSwapMask(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        case 9:
            Player.player.clothing.sendSwapGlasses(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
        }
    }

    private static void updatePivot()
    {
        if (dragJar.rot == 0)
        {
            dragPivot.x = dragOffset.x;
            dragPivot.y = dragOffset.y;
        }
        else if (dragJar.rot == 1)
        {
            dragPivot.x = 0f - ((float)(dragJar.size_y * 50) + dragOffset.y);
            dragPivot.y = dragOffset.x;
        }
        else if (dragJar.rot == 2)
        {
            dragPivot.x = 0f - ((float)(dragJar.size_x * 50) + dragOffset.x);
            dragPivot.y = 0f - ((float)(dragJar.size_y * 50) + dragOffset.y);
        }
        else if (dragJar.rot == 3)
        {
            dragPivot.x = dragOffset.y;
            dragPivot.y = 0f - ((float)(dragJar.size_x * 50) + dragOffset.x);
        }
    }

    private static void refreshDraggedVisualPosition()
    {
        dragItem.positionOffset_X = (int)dragPivot.x;
        dragItem.positionOffset_Y = (int)dragPivot.y;
        Vector2 vector = PlayerUI.container.ViewportToNormalizedPosition(InputEx.NormalizedMousePosition);
        dragItem.positionScale_X = vector.x;
        dragItem.positionScale_Y = vector.y;
    }

    public static void updateDraggedItem()
    {
        if (active && PlayerDashboardUI.active && dragFromPage != byte.MaxValue && dragJar != null && isDragging)
        {
            if (InputEx.GetKeyDown(ControlsSettings.rotate))
            {
                dragJar.rot++;
                dragJar.rot %= 4;
                updatePivot();
                dragItem.updateItem(dragJar);
                PlayInventoryAudio(dragJar.GetAsset());
            }
            refreshDraggedVisualPosition();
        }
    }

    private static void createElementForNearbyDrop(InteractableItem interactableItem)
    {
        while (!areaItems.tryAddItem(interactableItem.item))
        {
            if (areaItems.height < 200)
            {
                areaItems.resize(areaItems.width, (byte)(areaItems.height + 1));
                continue;
            }
            return;
        }
        ItemJar item = areaItems.getItem((byte)(areaItems.getItemCount() - 1));
        item.interactableItem = interactableItem;
        interactableItem.jar = item;
        byte b = (byte)(areaItems.height - (item.y + (((int)item.rot % 2 == 0) ? item.size_y : item.size_x)));
        if (b < 3 && areaItems.height + b <= 200)
        {
            areaItems.resize(areaItems.width, (byte)(areaItems.height + (3 - b)));
        }
        SleekItems obj = items[PlayerInventory.AREA - PlayerInventory.SLOTS];
        obj.resize(areaItems.width, areaItems.height);
        obj.addItem(item);
    }

    public static void updateNearbyDrops()
    {
        if (!active || pendingItemsInRadius.Count < 1)
        {
            return;
        }
        int height = areaItems.height;
        Vector3 eyesPositionWithoutLeaning = Player.player.look.GetEyesPositionWithoutLeaning();
        int num = Mathf.Max(0, pendingItemsInRadius.Count - 20);
        for (int num2 = pendingItemsInRadius.Count - 1; num2 >= num; num2--)
        {
            InteractableItem interactableItem = pendingItemsInRadius[num2];
            pendingItemsInRadius.RemoveAt(num2);
            if (!(interactableItem == null) && interactableItem.item != null)
            {
                Renderer componentInChildren = interactableItem.transform.GetComponentInChildren<Renderer>();
                if (!(componentInChildren == null) && !Physics.Linecast(eyesPositionWithoutLeaning, componentInChildren.bounds.center, RayMasks.BLOCK_PICKUP, QueryTriggerInteraction.Ignore))
                {
                    createElementForNearbyDrop(interactableItem);
                }
            }
        }
        if (areaItems.height > height)
        {
            updateBoxAreas();
        }
    }

    private static void PlayInventoryAudio(ItemAsset item)
    {
    }

    public PlayerDashboardInventoryUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        pendingItemsInRadius = new List<InteractableItem>();
        localization = Localization.read("/Player/PlayerDashboardInventory.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDashboardInventory/PlayerDashboardInventory.unity3d");
        _selectedPage = byte.MaxValue;
        _selected_x = byte.MaxValue;
        _selected_y = byte.MaxValue;
        isDragging = false;
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = true;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.positionOffset_Y = 60;
        backdropBox.sizeOffset_Y = -60;
        backdropBox.sizeScale_X = 1f;
        backdropBox.sizeScale_Y = 1f;
        backdropBox.backgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        characterPlayer = null;
        hasDragOutsideHandlers = Glazier.Get().SupportsDepth;
        if (hasDragOutsideHandlers)
        {
            dragOutsideHandler = Glazier.Get().CreateImage();
            dragOutsideHandler.sizeScale_X = 1f;
            dragOutsideHandler.sizeScale_Y = 1f;
            dragOutsideHandler.onImageClicked += onClickedDuringDrag;
            dragOutsideHandler.onImageRightClicked += onRightClickedDuringDrag;
            dragOutsideHandler.isVisible = false;
            backdropBox.AddChild(dragOutsideHandler);
        }
        else
        {
            dragOutsideHandler = null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.positionOffset_X = 10;
        sleekBox.positionOffset_Y = 70;
        sleekBox.sizeOffset_X = 410;
        sleekBox.sizeOffset_Y = -280;
        sleekBox.sizeScale_Y = 1f;
        backdropBox.AddChild(sleekBox);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.sizeScale_Y = 1f;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        if (Glazier.Get().SupportsDepth)
        {
            sleekConstraintFrame.positionScale_X = -0.5f;
            sleekConstraintFrame.sizeScale_X = 2f;
        }
        else
        {
            sleekConstraintFrame.positionScale_X = 0f;
            sleekConstraintFrame.sizeScale_X = 1f;
        }
        sleekBox.AddChild(sleekConstraintFrame);
        characterImage = Glazier.Get().CreateImage(Resources.Load<Texture>("RenderTextures/Character"));
        characterImage.sizeScale_X = 1f;
        characterImage.sizeScale_Y = 1f;
        characterImage.onImageClicked += onClickedCharacter;
        sleekConstraintFrame.AddChild(characterImage);
        slots = new SleekSlot[PlayerInventory.SLOTS];
        for (byte b = 0; b < slots.Length; b = (byte)(b + 1))
        {
            slots[b] = new SleekSlot(b);
            slots[b].onSelectedItem = onSelectedItem;
            slots[b].onGrabbedItem = onGrabbedItem;
            slots[b].onPlacedItem = onPlacedItem;
            backdropBox.AddChild(slots[b]);
        }
        slots[0].positionOffset_X = 10;
        slots[0].positionOffset_Y = -160;
        slots[0].positionScale_Y = 1f;
        slots[1].positionOffset_X = 270;
        slots[1].positionOffset_Y = -160;
        slots[1].positionScale_Y = 1f;
        slots[1].sizeOffset_X = 150;
        characterSlider = Glazier.Get().CreateSlider();
        characterSlider.sizeOffset_Y = 20;
        characterSlider.sizeScale_X = 1f;
        characterSlider.sizeOffset_X = -120;
        characterSlider.positionOffset_X = 120;
        characterSlider.positionOffset_Y = 15;
        characterSlider.positionScale_Y = 1f;
        characterSlider.orientation = ESleekOrientation.HORIZONTAL;
        characterSlider.onDragged += onDraggedCharacterSlider;
        sleekBox.AddChild(characterSlider);
        swapCosmeticsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Cosmetics"));
        swapCosmeticsButton.positionOffset_Y = 10;
        swapCosmeticsButton.positionScale_Y = 1f;
        swapCosmeticsButton.sizeOffset_X = 30;
        swapCosmeticsButton.sizeOffset_Y = 30;
        swapCosmeticsButton.tooltip = localization.format("Swap_Cosmetics_Tooltip");
        swapCosmeticsButton.iconColor = ESleekTint.FOREGROUND;
        swapCosmeticsButton.onClickedButton += onClickedSwapCosmeticsButton;
        sleekBox.AddChild(swapCosmeticsButton);
        swapSkinsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Skins"));
        swapSkinsButton.positionOffset_X = 40;
        swapSkinsButton.positionOffset_Y = 10;
        swapSkinsButton.positionScale_Y = 1f;
        swapSkinsButton.sizeOffset_X = 30;
        swapSkinsButton.sizeOffset_Y = 30;
        swapSkinsButton.tooltip = localization.format("Swap_Skins_Tooltip");
        swapSkinsButton.iconColor = ESleekTint.FOREGROUND;
        swapSkinsButton.onClickedButton += onClickedSwapSkinsButton;
        sleekBox.AddChild(swapSkinsButton);
        swapMythicsButton = new SleekButtonIcon(icons.load<Texture2D>("Swap_Mythics"));
        swapMythicsButton.positionOffset_X = 80;
        swapMythicsButton.positionOffset_Y = 10;
        swapMythicsButton.positionScale_Y = 1f;
        swapMythicsButton.sizeOffset_X = 30;
        swapMythicsButton.sizeOffset_Y = 30;
        swapMythicsButton.tooltip = localization.format("Swap_Mythics_Tooltip");
        swapMythicsButton.iconColor = ESleekTint.FOREGROUND;
        swapMythicsButton.onClickedButton += onClickedSwapMythicsButton;
        sleekBox.AddChild(swapMythicsButton);
        box = Glazier.Get().CreateFrame();
        box.positionOffset_X = 430;
        box.positionOffset_Y = 10;
        box.sizeOffset_X = -440;
        box.sizeOffset_Y = -20;
        box.sizeScale_X = 1f;
        box.sizeScale_Y = 1f;
        backdropBox.AddChild(box);
        clothingBox = Glazier.Get().CreateScrollView();
        clothingBox.sizeScale_X = 1f;
        clothingBox.sizeScale_Y = 1f;
        clothingBox.contentSizeOffset = new Vector2(0f, 1000f);
        clothingBox.scaleContentToWidth = true;
        box.AddChild(clothingBox);
        areaBox = Glazier.Get().CreateScrollView();
        areaBox.positionOffset_X = 5;
        areaBox.positionScale_X = 0.5f;
        areaBox.sizeOffset_X = -5;
        areaBox.sizeScale_X = 0.5f;
        areaBox.sizeScale_Y = 1f;
        areaBox.contentSizeOffset = new Vector2(0f, 1000f);
        areaBox.scaleContentToWidth = true;
        box.AddChild(areaBox);
        if (hasDragOutsideHandlers)
        {
            dragOutsideClothingHandler = Glazier.Get().CreateImage();
            dragOutsideClothingHandler.sizeScale_X = 1f;
            dragOutsideClothingHandler.sizeScale_Y = 1f;
            dragOutsideClothingHandler.onImageClicked += onClickedDuringDrag;
            dragOutsideClothingHandler.onImageRightClicked += onRightClickedDuringDrag;
            dragOutsideClothingHandler.isVisible = false;
            clothingBox.AddChild(dragOutsideClothingHandler);
            dragOutsideAreaHandler = Glazier.Get().CreateImage();
            dragOutsideAreaHandler.sizeScale_X = 1f;
            dragOutsideAreaHandler.sizeScale_Y = 1f;
            dragOutsideAreaHandler.onImageClicked += onClickedDuringDrag;
            dragOutsideAreaHandler.onImageRightClicked += onRightClickedDuringDrag;
            dragOutsideAreaHandler.isVisible = false;
            areaBox.AddChild(dragOutsideAreaHandler);
        }
        else
        {
            dragOutsideClothingHandler = null;
            dragOutsideAreaHandler = null;
        }
        headers = new ISleekButton[PlayerInventory.PAGES - PlayerInventory.SLOTS + 3];
        for (byte b2 = 0; b2 < headers.Length; b2 = (byte)(b2 + 1))
        {
            headers[b2] = Glazier.Get().CreateButton();
            headers[b2].sizeOffset_Y = 60;
            headers[b2].sizeScale_X = 1f;
            headers[b2].fontSize = ESleekFontSize.Medium;
            headers[b2].onClickedButton += onClickedHeader;
            headers[b2].shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            clothingBox.AddChild(headers[b2]);
            headers[b2].isVisible = false;
        }
        headers[0].isVisible = true;
        headers[PlayerInventory.AREA - PlayerInventory.SLOTS].isVisible = true;
        headerItemIcons = new SleekItemIcon[headers.Length];
        for (byte b3 = 1; b3 < headers.Length; b3 = (byte)(b3 + 1))
        {
            if (b3 != PlayerInventory.STORAGE - PlayerInventory.SLOTS && b3 != PlayerInventory.AREA - PlayerInventory.SLOTS)
            {
                SleekItemIcon sleekItemIcon = new SleekItemIcon
                {
                    positionOffset_X = 5,
                    positionScale_Y = 0.5f
                };
                headerItemIcons[b3] = sleekItemIcon;
                headers[b3].AddChild(sleekItemIcon);
                ISleekImage sleekImage = Glazier.Get().CreateImage();
                sleekImage.positionOffset_X = -25;
                sleekImage.positionOffset_Y = -25;
                sleekImage.positionScale_X = 1f;
                sleekImage.positionScale_Y = 1f;
                sleekImage.sizeOffset_X = 20;
                sleekImage.sizeOffset_Y = 20;
                sleekImage.texture = icons.load<Texture2D>("Quality_0");
                headers[b3].AddChild(sleekImage);
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.positionOffset_X = -105;
                sleekLabel.positionOffset_Y = 5;
                sleekLabel.positionScale_X = 1f;
                sleekLabel.sizeOffset_X = 100;
                sleekLabel.sizeOffset_Y = -10;
                sleekLabel.sizeScale_Y = 1f;
                sleekLabel.fontAlignment = TextAnchor.UpperRight;
                sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                headers[b3].AddChild(sleekLabel);
            }
        }
        headers[0].text = localization.format("Hands");
        headers[PlayerInventory.AREA - PlayerInventory.SLOTS].text = localization.format("Area");
        onShirtUpdated(0, Player.player.clothing.shirtQuality, Player.player.clothing.shirtState);
        onPantsUpdated(0, Player.player.clothing.pantsQuality, Player.player.clothing.pantsState);
        onBackpackUpdated(0, Player.player.clothing.backpackQuality, Player.player.clothing.backpackState);
        onVestUpdated(0, Player.player.clothing.vestQuality, Player.player.clothing.vestState);
        items = new SleekItems[PlayerInventory.PAGES - PlayerInventory.SLOTS];
        for (byte b4 = 0; b4 < items.Length; b4 = (byte)(b4 + 1))
        {
            items[b4] = new SleekItems((byte)(PlayerInventory.SLOTS + b4));
            items[b4].onSelectedItem = onSelectedItem;
            items[b4].onGrabbedItem = onGrabbedItem;
            items[b4].onPlacedItem = onPlacedItem;
            clothingBox.AddChild(items[b4]);
        }
        areaItems = new Items(PlayerInventory.AREA);
        actions = new List<Action>();
        selectionFrame = Glazier.Get().CreateImage();
        selectionFrame.sizeScale_X = 1f;
        selectionFrame.sizeScale_Y = 1f;
        selectionFrame.onImageClicked += onClickedOutsideSelection;
        selectionFrame.onImageRightClicked += onClickedOutsideSelection;
        selectionFrame.texture = (Texture2D)GlazierResources.PixelTexture;
        selectionFrame.color = new Color(0f, 0f, 0f, 0.5f);
        selectionFrame.isVisible = false;
        PlayerUI.container.AddChild(selectionFrame);
        selectionBackdropBox = Glazier.Get().CreateBox();
        selectionBackdropBox.sizeOffset_X = 530;
        selectionBackdropBox.sizeOffset_Y = 440;
        selectionFrame.AddChild(selectionBackdropBox);
        selectionIconBox = Glazier.Get().CreateBox();
        selectionIconBox.positionOffset_X = 10;
        selectionIconBox.positionOffset_Y = 10;
        selectionIconBox.sizeOffset_X = 510;
        selectionIconBox.sizeOffset_Y = 310;
        selectionBackdropBox.AddChild(selectionIconBox);
        selectionIconImage = new SleekItemIcon();
        selectionIconImage.positionScale_X = 0.5f;
        selectionIconImage.positionScale_Y = 0.5f;
        selectionIconBox.AddChild(selectionIconImage);
        selectionDescriptionBox = Glazier.Get().CreateBox();
        selectionDescriptionBox.positionOffset_X = 10;
        selectionDescriptionBox.positionOffset_Y = 330;
        selectionDescriptionBox.sizeOffset_X = 250;
        selectionDescriptionBox.sizeOffset_Y = 100;
        selectionBackdropBox.AddChild(selectionDescriptionBox);
        selectionDescriptionLabel = Glazier.Get().CreateLabel();
        selectionDescriptionLabel.enableRichText = true;
        selectionDescriptionLabel.positionOffset_X = 5;
        selectionDescriptionLabel.positionOffset_Y = 5;
        selectionDescriptionLabel.sizeOffset_X = -10;
        selectionDescriptionLabel.sizeOffset_Y = -10;
        selectionDescriptionLabel.sizeScale_X = 1f;
        selectionDescriptionLabel.sizeScale_Y = 1f;
        selectionDescriptionLabel.fontAlignment = TextAnchor.UpperLeft;
        selectionDescriptionLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        selectionDescriptionLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        selectionDescriptionBox.AddChild(selectionDescriptionLabel);
        selectionNameLabel = Glazier.Get().CreateLabel();
        selectionNameLabel.positionOffset_Y = -70;
        selectionNameLabel.positionScale_Y = 1f;
        selectionNameLabel.sizeOffset_Y = 70;
        selectionNameLabel.sizeScale_X = 1f;
        selectionNameLabel.fontSize = ESleekFontSize.Large;
        selectionNameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        selectionIconBox.AddChild(selectionNameLabel);
        selectionHotkeyLabel = Glazier.Get().CreateLabel();
        selectionHotkeyLabel.positionOffset_X = 5;
        selectionHotkeyLabel.positionOffset_Y = 5;
        selectionHotkeyLabel.sizeOffset_X = -10;
        selectionHotkeyLabel.sizeOffset_Y = 30;
        selectionHotkeyLabel.sizeScale_X = 1f;
        selectionHotkeyLabel.fontAlignment = TextAnchor.UpperRight;
        selectionIconBox.AddChild(selectionHotkeyLabel);
        selectionActionsBox = Glazier.Get().CreateScrollView();
        selectionActionsBox.positionOffset_X = 270;
        selectionActionsBox.positionOffset_Y = 330;
        selectionActionsBox.sizeOffset_X = -280;
        selectionActionsBox.sizeOffset_Y = 100;
        selectionActionsBox.sizeScale_X = 1f;
        selectionActionsBox.scaleContentToWidth = true;
        selectionBackdropBox.AddChild(selectionActionsBox);
        selectionEquipButton = Glazier.Get().CreateButton();
        selectionEquipButton.sizeScale_X = 1f;
        selectionEquipButton.sizeOffset_Y = 30;
        selectionEquipButton.onClickedButton += onClickedEquip;
        selectionActionsBox.AddChild(selectionEquipButton);
        selectionContextButton = Glazier.Get().CreateButton();
        selectionContextButton.sizeScale_X = 1f;
        selectionContextButton.sizeOffset_Y = 30;
        selectionContextButton.onClickedButton += onClickedContext;
        selectionActionsBox.AddChild(selectionContextButton);
        selectionDropButton = Glazier.Get().CreateButton();
        selectionDropButton.sizeScale_X = 1f;
        selectionDropButton.sizeOffset_Y = 30;
        selectionDropButton.onClickedButton += onClickedDrop;
        selectionActionsBox.AddChild(selectionDropButton);
        selectionStorageButton = Glazier.Get().CreateButton();
        selectionStorageButton.sizeScale_X = 1f;
        selectionStorageButton.sizeOffset_Y = 30;
        selectionStorageButton.onClickedButton += onClickedStore;
        selectionActionsBox.AddChild(selectionStorageButton);
        selectionExtraActionsBox = Glazier.Get().CreateFrame();
        selectionExtraActionsBox.sizeScale_X = 1f;
        selectionActionsBox.AddChild(selectionExtraActionsBox);
        vehicleBox = Glazier.Get().CreateBox();
        vehicleBox.sizeScale_X = 1f;
        clothingBox.AddChild(vehicleBox);
        vehicleNameLabel = Glazier.Get().CreateLabel();
        vehicleNameLabel.sizeOffset_Y = 60;
        vehicleNameLabel.sizeScale_X = 1f;
        vehicleNameLabel.fontSize = ESleekFontSize.Medium;
        vehicleNameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        vehicleBox.AddChild(vehicleNameLabel);
        vehicleActionsBox = Glazier.Get().CreateFrame();
        vehicleActionsBox.positionOffset_X = 10;
        vehicleActionsBox.positionOffset_Y = 60;
        vehicleActionsBox.sizeOffset_X = 250;
        vehicleBox.AddChild(vehicleActionsBox);
        vehicleLockButton = Glazier.Get().CreateButton();
        vehicleLockButton.sizeOffset_Y = 30;
        vehicleLockButton.sizeScale_X = 1f;
        vehicleLockButton.onClickedButton += onClickedVehicleLockButton;
        vehicleActionsBox.AddChild(vehicleLockButton);
        vehicleLockButton.isVisible = false;
        vehicleHornButton = Glazier.Get().CreateButton();
        vehicleHornButton.sizeOffset_Y = 30;
        vehicleHornButton.sizeScale_X = 1f;
        vehicleHornButton.onClickedButton += onClickedVehicleHornButton;
        vehicleActionsBox.AddChild(vehicleHornButton);
        vehicleHornButton.isVisible = false;
        vehicleHeadlightsButton = Glazier.Get().CreateButton();
        vehicleHeadlightsButton.sizeOffset_Y = 30;
        vehicleHeadlightsButton.sizeScale_X = 1f;
        vehicleHeadlightsButton.onClickedButton += onClickedVehicleHeadlightsButton;
        vehicleActionsBox.AddChild(vehicleHeadlightsButton);
        vehicleHeadlightsButton.isVisible = false;
        vehicleSirensButton = Glazier.Get().CreateButton();
        vehicleSirensButton.sizeOffset_Y = 30;
        vehicleSirensButton.sizeScale_X = 1f;
        vehicleSirensButton.onClickedButton += onClickedVehicleSirensButton;
        vehicleActionsBox.AddChild(vehicleSirensButton);
        vehicleSirensButton.isVisible = false;
        vehicleBlimpButton = Glazier.Get().CreateButton();
        vehicleBlimpButton.sizeOffset_Y = 30;
        vehicleBlimpButton.sizeScale_X = 1f;
        vehicleBlimpButton.onClickedButton += onClickedVehicleBlimpButton;
        vehicleActionsBox.AddChild(vehicleBlimpButton);
        vehicleBlimpButton.isVisible = false;
        vehicleHookButton = Glazier.Get().CreateButton();
        vehicleHookButton.sizeOffset_Y = 30;
        vehicleHookButton.sizeScale_X = 1f;
        vehicleHookButton.onClickedButton += onClickedVehicleHookButton;
        vehicleActionsBox.AddChild(vehicleHookButton);
        vehicleHookButton.isVisible = false;
        vehicleStealBatteryButton = Glazier.Get().CreateButton();
        vehicleStealBatteryButton.sizeOffset_Y = 30;
        vehicleStealBatteryButton.sizeScale_X = 1f;
        vehicleStealBatteryButton.onClickedButton += onClickedVehicleStealBatteryButton;
        vehicleActionsBox.AddChild(vehicleStealBatteryButton);
        vehicleStealBatteryButton.isVisible = false;
        vehicleSkinButton = Glazier.Get().CreateButton();
        vehicleSkinButton.sizeOffset_Y = 30;
        vehicleSkinButton.sizeScale_X = 1f;
        vehicleSkinButton.onClickedButton += onClickedVehicleSkinButton;
        vehicleActionsBox.AddChild(vehicleSkinButton);
        vehicleSkinButton.isVisible = false;
        vehiclePassengersBox = Glazier.Get().CreateFrame();
        vehiclePassengersBox.positionOffset_Y = 60;
        vehiclePassengersBox.sizeScale_X = 1f;
        vehicleBox.AddChild(vehiclePassengersBox);
        rot_xButton = Glazier.Get().CreateButton();
        rot_xButton.positionScale_X = 1f;
        rot_xButton.sizeOffset_X = 60;
        rot_xButton.sizeOffset_Y = 60;
        rot_xButton.onClickedButton += onClickedRot_XButton;
        rot_xButton.text = localization.format("Rot_X");
        headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_xButton);
        rot_xButton.isVisible = false;
        rot_yButton = Glazier.Get().CreateButton();
        rot_yButton.positionScale_X = 1f;
        rot_yButton.positionOffset_X = 60;
        rot_yButton.sizeOffset_X = 60;
        rot_yButton.sizeOffset_Y = 60;
        rot_yButton.onClickedButton += onClickedRot_YButton;
        rot_yButton.text = localization.format("Rot_Y");
        headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_yButton);
        rot_yButton.isVisible = false;
        rot_zButton = Glazier.Get().CreateButton();
        rot_zButton.positionScale_X = 1f;
        rot_zButton.positionOffset_X = 120;
        rot_zButton.sizeOffset_X = 60;
        rot_zButton.sizeOffset_Y = 60;
        rot_zButton.onClickedButton += onClickedRot_ZButton;
        rot_zButton.text = localization.format("Rot_Z");
        headers[PlayerInventory.STORAGE - PlayerInventory.SLOTS].AddChild(rot_zButton);
        rot_zButton.isVisible = false;
        dragItem = new SleekItem();
        PlayerUI.container.AddChild(dragItem);
        dragItem.isVisible = false;
        dragItem.SetIsDragItem();
        dragOffset = Vector2.zero;
        dragPivot = Vector2.zero;
        dragFromPage = byte.MaxValue;
        dragFrom_x = byte.MaxValue;
        dragFrom_y = byte.MaxValue;
        dragFromRot = 0;
        PlayerInventory inventory = Player.player.inventory;
        inventory.onInventoryResized = (InventoryResized)Delegate.Combine(inventory.onInventoryResized, new InventoryResized(onInventoryResized));
        PlayerInventory inventory2 = Player.player.inventory;
        inventory2.onInventoryUpdated = (InventoryUpdated)Delegate.Combine(inventory2.onInventoryUpdated, new InventoryUpdated(onInventoryUpdated));
        PlayerInventory inventory3 = Player.player.inventory;
        inventory3.onInventoryAdded = (InventoryAdded)Delegate.Combine(inventory3.onInventoryAdded, new InventoryAdded(onInventoryAdded));
        PlayerInventory inventory4 = Player.player.inventory;
        inventory4.onInventoryRemoved = (InventoryRemoved)Delegate.Combine(inventory4.onInventoryRemoved, new InventoryRemoved(onInventoryRemoved));
        PlayerInventory inventory5 = Player.player.inventory;
        inventory5.onInventoryStored = (InventoryStored)Delegate.Combine(inventory5.onInventoryStored, new InventoryStored(onInventoryStored));
        PlayerEquipment equipment = Player.player.equipment;
        equipment.onHotkeysUpdated = (HotkeysUpdated)Delegate.Combine(equipment.onHotkeysUpdated, new HotkeysUpdated(onHotkeysUpdated));
        ItemManager.onItemDropAdded = onItemDropAdded;
        ItemManager.onItemDropRemoved = onItemDropRemoved;
        PlayerMovement movement = Player.player.movement;
        movement.onSeated = (Seated)Delegate.Combine(movement.onSeated, new Seated(onSeated));
        PlayerClothing clothing = Player.player.clothing;
        clothing.onShirtUpdated = (ShirtUpdated)Delegate.Combine(clothing.onShirtUpdated, new ShirtUpdated(onShirtUpdated));
        PlayerClothing clothing2 = Player.player.clothing;
        clothing2.onPantsUpdated = (PantsUpdated)Delegate.Combine(clothing2.onPantsUpdated, new PantsUpdated(onPantsUpdated));
        PlayerClothing clothing3 = Player.player.clothing;
        clothing3.onHatUpdated = (HatUpdated)Delegate.Combine(clothing3.onHatUpdated, new HatUpdated(onHatUpdated));
        PlayerClothing clothing4 = Player.player.clothing;
        clothing4.onBackpackUpdated = (BackpackUpdated)Delegate.Combine(clothing4.onBackpackUpdated, new BackpackUpdated(onBackpackUpdated));
        PlayerClothing clothing5 = Player.player.clothing;
        clothing5.onVestUpdated = (VestUpdated)Delegate.Combine(clothing5.onVestUpdated, new VestUpdated(onVestUpdated));
        PlayerClothing clothing6 = Player.player.clothing;
        clothing6.onMaskUpdated = (MaskUpdated)Delegate.Combine(clothing6.onMaskUpdated, new MaskUpdated(onMaskUpdated));
        PlayerClothing clothing7 = Player.player.clothing;
        clothing7.onGlassesUpdated = (GlassesUpdated)Delegate.Combine(clothing7.onGlassesUpdated, new GlassesUpdated(onGlassesUpdated));
    }
}
