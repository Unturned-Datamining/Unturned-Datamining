using System;
using System.Collections.Generic;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothingUI
{
    private enum ESortMode
    {
        Date,
        Rarity,
        Name,
        Type
    }

    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    public static bool isCrafting;

    private static SleekButtonIcon backButton;

    private static ISleekButton itemstoreButton;

    private static ISleekLabel itemstoreNewLabel;

    private static SleekButtonIcon craftingButton;

    private static List<SteamItemDetails_t> filteredItems;

    private static ISleekConstraintFrame inventory;

    private static ISleekConstraintFrame crafting;

    private static SleekInventory[] packageButtons;

    private static ISleekBox availableBox;

    private static ISleekScrollView craftingScrollBox;

    private static ISleekButton[] craftingButtons;

    private static ISleekBox pageBox;

    private static ISleekBox infoBox;

    private static ISleekField searchField;

    private static ISleekButton searchButton;

    private static ISleekBox filterBox;

    private static ISleekButton cancelFilterButton;

    private static SleekButtonIcon leftButton;

    private static SleekButtonIcon rightButton;

    private static SleekButtonIcon refreshButton;

    private static SleekButtonIcon optionsButton;

    private static ISleekToggle searchDescriptionsToggle;

    private static SleekButtonState sortModeButton;

    private static ISleekToggle reverseSortOrderToggle;

    private static ISleekToggle filterEquippedToggle;

    private static ISleekElement optionsPanel;

    private static ISleekSlider characterSlider;

    private static int pageIndex;

    private static EEconFilterMode filterMode;

    private static ulong filterInstigator;

    private static bool searchDescriptions;

    private static ESortMode sortMode;

    private static bool reverseSortOrder;

    private static bool filterEquipped;

    private MenuSurvivorsClothingItemUI itemUI;

    private MenuSurvivorsClothingInspectUI inspectUI;

    private MenuSurvivorsClothingDeleteUI deleteUI;

    private MenuSurvivorsClothingBoxUI boxUI;

    private ItemStoreMenu itemStoreUI;

    private List<EconCraftOption> econCraftOptions;

    private static int numberOfPages => MathfEx.GetPageCount(filteredItems.Count, 25);

    public static void open()
    {
        if (!active)
        {
            active = true;
            Characters.apply(showItems: false, showCosmetics: true);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            if (!MenuSurvivorsClothingBoxUI.active && !MenuSurvivorsClothingInspectUI.active && !MenuSurvivorsClothingDeleteUI.active && !MenuSurvivorsClothingItemUI.active)
            {
                Characters.apply(showItems: true, showCosmetics: true);
            }
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void setFilter(EEconFilterMode newFilterMode, ulong newFilterInstigator)
    {
        setCrafting(isCrafting: false);
        filterMode = newFilterMode;
        filterInstigator = newFilterInstigator;
        filterBox.isVisible = filterMode != EEconFilterMode.SEARCH;
        cancelFilterButton.isVisible = filterMode != EEconFilterMode.SEARCH;
        if (filterMode != 0)
        {
            searchField.text = string.Empty;
        }
        if (filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT)
        {
            int inventoryItem = Provider.provider.economyService.getInventoryItem(filterInstigator);
            string inventoryName = Provider.provider.economyService.getInventoryName(inventoryItem);
            Color inventoryColor = Provider.provider.economyService.getInventoryColor(inventoryItem);
            string arg = "<color=" + Palette.hex(inventoryColor) + ">" + inventoryName + "</color>";
            filterBox.text = localization.format("Filter_Item_Target", arg);
        }
        updateFilterAndPage();
    }

    private static void updateFilterAndPage()
    {
        updateFilter();
        if (pageIndex >= numberOfPages)
        {
            pageIndex = numberOfPages - 1;
        }
        updatePage();
    }

    public static void viewPage(int newPage)
    {
        pageIndex = newPage;
        updatePage();
    }

    private static void onClickedInventory(SleekInventory button)
    {
        int num = packageButtons.Length * pageIndex;
        int num2 = inventory.FindIndexOfChild(button);
        if (num + num2 >= filteredItems.Count)
        {
            return;
        }
        int item = button.item;
        ulong instance = button.instance;
        ushort quantity = button.quantity;
        if (filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL || filterMode == EEconFilterMode.RAGDOLL_EFFECT)
        {
            bool flag = filterMode == EEconFilterMode.STAT_TRACKER || filterMode == EEconFilterMode.RAGDOLL_EFFECT;
            MenuSurvivorsClothingDeleteUI.viewItem(item, instance, 1, flag ? EDeleteMode.TAG_TOOL_ADD : EDeleteMode.TAG_TOOL_REMOVE, filterInstigator);
            MenuSurvivorsClothingDeleteUI.open();
            setFilter(EEconFilterMode.SEARCH, 0uL);
            close();
        }
        else if (Provider.preferenceData.Allow_Ctrl_Shift_Alt_Salvage && InputEx.GetKey(KeyCode.LeftControl) && InputEx.GetKey(KeyCode.LeftShift) && InputEx.GetKey(KeyCode.LeftAlt))
        {
            MenuSurvivorsClothingDeleteUI.salvageItem(item, instance);
        }
        else if (InputEx.GetKey(ControlsSettings.other) && packageButtons[num2].itemAsset != null)
        {
            if (button.itemAsset.type == EItemType.BOX)
            {
                MenuSurvivorsClothingItemUI.viewItem(item, quantity, instance);
                MenuSurvivorsClothingBoxUI.viewItem(item, quantity, instance);
                MenuSurvivorsClothingBoxUI.open();
                close();
            }
            else
            {
                Characters.ToggleEquipItemByInstanceId(instance);
            }
        }
        else
        {
            MenuSurvivorsClothingItemUI.viewItem(item, quantity, instance);
            MenuSurvivorsClothingItemUI.open();
            close();
        }
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        updateFilterAndPage();
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        updateFilterAndPage();
    }

    private static void onClickedCancelFilterButton(ISleekElement button)
    {
        setFilter(EEconFilterMode.SEARCH, 0uL);
    }

    private static void onClickedLeftButton(ISleekElement button)
    {
        if (pageIndex > 0)
        {
            viewPage(pageIndex - 1);
        }
        else if (numberOfPages > 0)
        {
            viewPage(numberOfPages - 1);
        }
    }

    private static void onClickedRightButton(ISleekElement button)
    {
        if (pageIndex < numberOfPages - 1)
        {
            viewPage(pageIndex + 1);
        }
        else if (numberOfPages > 0)
        {
            viewPage(0);
        }
    }

    private static void onClickedOptionsButton(ISleekElement button)
    {
        optionsPanel.isVisible = !optionsPanel.isVisible;
        optionsButton.icon = icons.load<Texture2D>(optionsPanel.isVisible ? "Right" : "Left");
    }

    private static void onToggledSearchDescriptions(ISleekToggle toggle, bool state)
    {
        searchDescriptions = state;
        updateFilterAndPage();
    }

    private static void onChangedSortMode(SleekButtonState button, int state)
    {
        sortMode = (ESortMode)state;
        updateFilterAndPage();
    }

    private static void onToggledReverseSortOrder(ISleekToggle toggle, bool state)
    {
        reverseSortOrder = state;
        updateFilterAndPage();
    }

    private static void onToggledFilterEquipped(ISleekToggle toggle, bool state)
    {
        filterEquipped = state;
        updateFilterAndPage();
    }

    private static void onClickedRefreshButton(ISleekElement button)
    {
        Provider.provider.economyService.refreshInventory();
    }

    public static void prepareForCraftResult()
    {
        isCrafting = true;
        MenuUI.openAlert(localization.format("Alert_Crafting"), canBeDismissed: false);
    }

    private void onClickedCraftButton(ISleekElement button)
    {
        if (isCrafting)
        {
            return;
        }
        int num = craftingScrollBox.FindIndexOfChild(button);
        if (num != -1)
        {
            EconCraftOption econCraftOption = econCraftOptions[num];
            if (Provider.provider.economyService.getInventoryPackages(19000, econCraftOption.scrapsNeeded, out var pairs))
            {
                prepareForCraftResult();
                Provider.provider.economyService.exchangeInventory(econCraftOption.generate, pairs);
            }
        }
    }

    private static void onInventoryRefreshed()
    {
        infoBox.isVisible = false;
        updateFilter();
        if (pageIndex >= numberOfPages)
        {
            pageIndex = numberOfPages - 1;
        }
        updatePage();
    }

    public static void onInventoryDropped(int item, ushort quantity, ulong instance)
    {
        MenuUI.closeAll();
        MenuUI.alert(localization.format("Origin_Drop"), instance, item, quantity);
        MenuSurvivorsClothingItemUI.viewItem(item, quantity, instance);
        MenuSurvivorsClothingItemUI.open();
    }

    private static void onCharacterUpdated(byte index, Character character)
    {
        updatePage();
    }

    private static void OnPricesReceived()
    {
        if (ItemStore.Get().HasNewListings && !ItemStoreSavedata.WasNewListingsPageSeen())
        {
            itemstoreNewLabel = Glazier.Get().CreateLabel();
            itemstoreNewLabel.sizeScale_X = 1f;
            itemstoreNewLabel.sizeScale_Y = 1f;
            itemstoreNewLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            itemstoreNewLabel.fontAlignment = TextAnchor.UpperRight;
            itemstoreNewLabel.textColor = Color.green;
            itemstoreNewLabel.text = Provider.localization.format("New");
            itemstoreButton.AddChild(itemstoreNewLabel);
        }
        else if (ItemStore.Get().HasDiscountedListings)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.sizeScale_X = 1f;
            sleekLabel.sizeScale_Y = 1f;
            sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekLabel.fontAlignment = TextAnchor.UpperRight;
            sleekLabel.textColor = Color.green;
            sleekLabel.text = localization.format("Itemstore_Sale");
            itemstoreButton.AddChild(sleekLabel);
        }
    }

    private static void applySearchTextFilter()
    {
        string text = searchField.text;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        TokenSearchFilter? tokenSearchFilter = TokenSearchFilter.parse(text);
        if (!tokenSearchFilter.HasValue)
        {
            return;
        }
        for (int num = filteredItems.Count - 1; num >= 0; num--)
        {
            SteamItemDetails_t steamItemDetails_t = filteredItems[num];
            bool flag = false;
            string inventoryName = Provider.provider.economyService.getInventoryName(steamItemDetails_t.m_iDefinition.m_SteamItemDef);
            if (tokenSearchFilter.Value.matches(inventoryName))
            {
                flag = true;
            }
            else
            {
                string inventoryType = Provider.provider.economyService.getInventoryType(steamItemDetails_t.m_iDefinition.m_SteamItemDef);
                if (tokenSearchFilter.Value.matches(inventoryType))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                if (searchDescriptions)
                {
                    string inventoryDescription = Provider.provider.economyService.getInventoryDescription(steamItemDetails_t.m_iDefinition.m_SteamItemDef);
                    if (tokenSearchFilter.Value.matches(inventoryDescription))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    filteredItems.RemoveAtFast(num);
                }
            }
        }
    }

    private static void applyEquippedFilter()
    {
        if (!filterEquipped)
        {
            return;
        }
        for (int num = filteredItems.Count - 1; num >= 0; num--)
        {
            if (!Characters.isEquipped(filteredItems[num].m_itemId.m_SteamItemInstanceID))
            {
                filteredItems.RemoveAtFast(num);
            }
        }
    }

    private static void sortFilteredItems()
    {
        IComparer<SteamItemDetails_t> comparer = sortMode switch
        {
            ESortMode.Rarity => new EconSortMode_Rarity(), 
            ESortMode.Name => new EconSortMode_Name(), 
            ESortMode.Type => new EconSortMode_Type(), 
            _ => null, 
        };
        if (comparer != null)
        {
            filteredItems.Sort(comparer);
        }
        if (reverseSortOrder)
        {
            filteredItems.Reverse();
        }
    }

    private static void updateFilter()
    {
        if (filterMode == EEconFilterMode.STAT_TRACKER)
        {
            filteredItems = new List<SteamItemDetails_t>();
            foreach (SteamItemDetails_t item in Provider.provider.economyService.inventory)
            {
                Guid inventoryItemGuid = Provider.provider.economyService.getInventoryItemGuid(item.m_iDefinition.m_SteamItemDef);
                int inventorySkinID = Provider.provider.economyService.getInventorySkinID(item.m_iDefinition.m_SteamItemDef);
                if (inventoryItemGuid != default(Guid) && inventorySkinID != 0)
                {
                    filteredItems.Add(item);
                }
            }
        }
        else if (filterMode == EEconFilterMode.STAT_TRACKER_REMOVAL)
        {
            filteredItems = new List<SteamItemDetails_t>();
            foreach (SteamItemDetails_t item2 in Provider.provider.economyService.inventory)
            {
                if (Provider.provider.economyService.getInventoryStatTrackerValue(item2.m_itemId.m_SteamItemInstanceID, out var type, out var _) && type != 0)
                {
                    filteredItems.Add(item2);
                }
            }
        }
        else if (filterMode == EEconFilterMode.RAGDOLL_EFFECT_REMOVAL)
        {
            filteredItems = new List<SteamItemDetails_t>();
            foreach (SteamItemDetails_t item3 in Provider.provider.economyService.inventory)
            {
                if (Provider.provider.economyService.getInventoryRagdollEffect(item3.m_itemId.m_SteamItemInstanceID, out var effect) && effect != 0)
                {
                    filteredItems.Add(item3);
                }
            }
        }
        else if (filterMode == EEconFilterMode.RAGDOLL_EFFECT)
        {
            filteredItems = new List<SteamItemDetails_t>();
            foreach (SteamItemDetails_t item4 in Provider.provider.economyService.inventory)
            {
                Guid inventoryItemGuid2 = Provider.provider.economyService.getInventoryItemGuid(item4.m_iDefinition.m_SteamItemDef);
                int inventorySkinID2 = Provider.provider.economyService.getInventorySkinID(item4.m_iDefinition.m_SteamItemDef);
                if (inventoryItemGuid2 != default(Guid) && inventorySkinID2 != 0)
                {
                    Provider.provider.economyService.getInventoryRagdollEffect(item4.m_itemId.m_SteamItemInstanceID, out var effect2);
                    if (effect2 == ERagdollEffect.NONE)
                    {
                        filteredItems.Add(item4);
                    }
                }
            }
        }
        else
        {
            filteredItems = new List<SteamItemDetails_t>(Provider.provider.economyService.inventory);
        }
        applySearchTextFilter();
        applyEquippedFilter();
        sortFilteredItems();
    }

    public static void updatePage()
    {
        availableBox.text = ItemTool.filterRarityRichText(localization.format("Craft_Available", Provider.provider.economyService.countInventoryPackages(19000)));
        pageBox.text = localization.format("Page", pageIndex + 1, numberOfPages);
        if (packageButtons == null)
        {
            return;
        }
        int num = packageButtons.Length * pageIndex;
        for (int i = 0; i < packageButtons.Length; i++)
        {
            if (num + i < filteredItems.Count)
            {
                packageButtons[i].updateInventory(filteredItems[num + i].m_itemId.m_SteamItemInstanceID, filteredItems[num + i].m_iDefinition.m_SteamItemDef, filteredItems[num + i].m_unQuantity, isClickable: true, isLarge: false);
            }
            else
            {
                packageButtons[i].updateInventory(0uL, 0, 0, isClickable: false, isLarge: false);
            }
        }
    }

    private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
    {
        Characters.characterYaw = state * 360f;
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsUI.open();
        close();
    }

    private static void onClickedItemstoreButton(ISleekElement button)
    {
        if (itemstoreNewLabel != null)
        {
            itemstoreButton.RemoveChild(itemstoreNewLabel);
            itemstoreNewLabel = null;
            ItemStoreSavedata.MarkNewListingsPageSeen();
            ItemStore.Get().ViewNewItems();
        }
        else
        {
            ItemStore.Get().ViewStore();
        }
    }

    private static void setCrafting(bool isCrafting)
    {
        inventory.isVisible = !isCrafting;
        crafting.isVisible = isCrafting;
        craftingButton.icon = (inventory.isVisible ? icons.load<Texture2D>("Crafting") : icons.load<Texture2D>("Backpack"));
        craftingButton.text = localization.format(inventory.isVisible ? "Crafting" : "Backpack");
        craftingButton.tooltip = localization.format(inventory.isVisible ? "Crafting_Tooltip" : "Backpack_Tooltip");
    }

    private static void onClickedCraftingButton(ISleekElement button)
    {
        setCrafting(!crafting.isVisible);
    }

    private static void onInventoryExchanged(List<SteamItemDetails_t> grantedItems)
    {
        if (!isCrafting)
        {
            return;
        }
        isCrafting = false;
        MenuUI.closeAlert();
        for (int num = grantedItems.Count - 1; num >= 0; num--)
        {
            if (grantedItems[num].m_iDefinition.m_SteamItemDef == 19000)
            {
                grantedItems.RemoveAtFast(num);
            }
        }
        MenuUI.alertNewItems(localization.format("Origin_Craft"), grantedItems);
        SteamItemDetails_t steamItemDetails_t = grantedItems[0];
        MenuSurvivorsClothingItemUI.viewItem(steamItemDetails_t.m_iDefinition.m_SteamItemDef, steamItemDetails_t.m_unQuantity, steamItemDetails_t.m_itemId.m_SteamItemInstanceID);
        MenuSurvivorsClothingItemUI.open();
        close();
    }

    private static void onInventoryPurchased(List<SteamItemDetails_t> grantedItems)
    {
        MenuUI.closeAlert();
        MenuUI.alertPurchasedItems(localization.format("Origin_Purchase"), grantedItems);
    }

    private static void onInventoryExchangeFailed()
    {
        if (isCrafting)
        {
            UnturnedLog.info("Crafting failed");
            isCrafting = false;
            MenuUI.closeAlert();
        }
    }

    public void OnDestroy()
    {
        boxUI.OnDestroy();
        TempSteamworksEconomy economyService = Provider.provider.economyService;
        economyService.onInventoryExchanged = (TempSteamworksEconomy.InventoryExchanged)Delegate.Remove(economyService.onInventoryExchanged, new TempSteamworksEconomy.InventoryExchanged(onInventoryExchanged));
        TempSteamworksEconomy economyService2 = Provider.provider.economyService;
        economyService2.onInventoryPurchased = (TempSteamworksEconomy.InventoryExchanged)Delegate.Remove(economyService2.onInventoryPurchased, new TempSteamworksEconomy.InventoryExchanged(onInventoryPurchased));
        TempSteamworksEconomy economyService3 = Provider.provider.economyService;
        economyService3.onInventoryExchangeFailed = (TempSteamworksEconomy.InventoryExchangeFailed)Delegate.Remove(economyService3.onInventoryExchangeFailed, new TempSteamworksEconomy.InventoryExchangeFailed(onInventoryExchangeFailed));
        TempSteamworksEconomy economyService4 = Provider.provider.economyService;
        economyService4.onInventoryRefreshed = (TempSteamworksEconomy.InventoryRefreshed)Delegate.Remove(economyService4.onInventoryRefreshed, new TempSteamworksEconomy.InventoryRefreshed(onInventoryRefreshed));
        TempSteamworksEconomy economyService5 = Provider.provider.economyService;
        economyService5.onInventoryDropped = (TempSteamworksEconomy.InventoryDropped)Delegate.Remove(economyService5.onInventoryDropped, new TempSteamworksEconomy.InventoryDropped(onInventoryDropped));
        Characters.onCharacterUpdated = (CharacterUpdated)Delegate.Remove(Characters.onCharacterUpdated, new CharacterUpdated(onCharacterUpdated));
        ItemStore.Get().OnPricesReceived -= OnPricesReceived;
    }

    public MenuSurvivorsClothingUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothing.dat");
        if (icons != null)
        {
            icons.unload();
            icons = null;
        }
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/MenuSurvivorsClothing/MenuSurvivorsClothing.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        pageIndex = 0;
        filterMode = EEconFilterMode.SEARCH;
        inventory = Glazier.Get().CreateConstraintFrame();
        inventory.positionOffset_Y = 80;
        inventory.positionScale_X = 0.5f;
        inventory.sizeScale_X = 0.5f;
        inventory.sizeScale_Y = 1f;
        inventory.sizeOffset_Y = -120;
        inventory.constraint = ESleekConstraint.FitInParent;
        container.AddChild(inventory);
        crafting = Glazier.Get().CreateConstraintFrame();
        crafting.positionOffset_Y = 40;
        crafting.positionScale_X = 0.5f;
        crafting.sizeScale_X = 0.5f;
        crafting.sizeScale_Y = 1f;
        crafting.sizeOffset_Y = -80;
        crafting.constraint = ESleekConstraint.FitInParent;
        container.AddChild(crafting);
        crafting.isVisible = false;
        packageButtons = new SleekInventory[25];
        for (int i = 0; i < packageButtons.Length; i++)
        {
            SleekInventory sleekInventory = new SleekInventory
            {
                positionOffset_X = 5,
                positionOffset_Y = 5,
                positionScale_X = (float)(i % 5) * 0.2f,
                positionScale_Y = (float)Mathf.FloorToInt((float)i / 5f) * 0.2f,
                sizeOffset_X = -10,
                sizeOffset_Y = -10,
                sizeScale_X = 0.2f,
                sizeScale_Y = 0.2f,
                onClickedInventory = onClickedInventory
            };
            inventory.AddChild(sleekInventory);
            packageButtons[i] = sleekInventory;
        }
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_X = 45;
        searchField.positionOffset_Y = -35;
        searchField.sizeOffset_X = -160;
        searchField.sizeOffset_Y = 30;
        searchField.sizeScale_X = 1f;
        searchField.hint = localization.format("Search_Field_Hint");
        searchField.onEntered += onEnteredSearchField;
        inventory.AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.positionOffset_X = -105;
        searchButton.positionOffset_Y = -35;
        searchButton.positionScale_X = 1f;
        searchButton.sizeOffset_X = 100;
        searchButton.sizeOffset_Y = 30;
        searchButton.text = localization.format("Search");
        searchButton.tooltipText = localization.format("Search_Tooltip");
        searchButton.onClickedButton += onClickedSearchButton;
        inventory.AddChild(searchButton);
        filterBox = Glazier.Get().CreateBox();
        filterBox.positionOffset_X = 5;
        filterBox.positionOffset_Y = -75;
        filterBox.sizeOffset_X = -120;
        filterBox.sizeOffset_Y = 30;
        filterBox.sizeScale_X = 1f;
        filterBox.enableRichText = true;
        filterBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        filterBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        inventory.AddChild(filterBox);
        filterBox.isVisible = false;
        cancelFilterButton = Glazier.Get().CreateButton();
        cancelFilterButton.positionOffset_X = -105;
        cancelFilterButton.positionOffset_Y = -75;
        cancelFilterButton.positionScale_X = 1f;
        cancelFilterButton.sizeOffset_X = 100;
        cancelFilterButton.sizeOffset_Y = 30;
        cancelFilterButton.text = localization.format("Cancel_Filter");
        cancelFilterButton.tooltipText = localization.format("Cancel_Filter_Tooltip");
        cancelFilterButton.onClickedButton += onClickedCancelFilterButton;
        inventory.AddChild(cancelFilterButton);
        cancelFilterButton.isVisible = false;
        pageBox = Glazier.Get().CreateBox();
        pageBox.positionOffset_X = -145;
        pageBox.positionOffset_Y = 5;
        pageBox.positionScale_X = 1f;
        pageBox.positionScale_Y = 1f;
        pageBox.sizeOffset_X = 100;
        pageBox.sizeOffset_Y = 30;
        pageBox.fontSize = ESleekFontSize.Medium;
        inventory.AddChild(pageBox);
        infoBox = Glazier.Get().CreateBox();
        infoBox.positionOffset_X = 5;
        infoBox.positionOffset_Y = -25;
        infoBox.positionScale_Y = 0.5f;
        infoBox.sizeScale_X = 1f;
        infoBox.sizeOffset_X = -10;
        infoBox.sizeOffset_Y = 50;
        infoBox.text = localization.format("No_Items");
        infoBox.fontSize = ESleekFontSize.Medium;
        inventory.AddChild(infoBox);
        infoBox.isVisible = !Provider.provider.economyService.isInventoryAvailable;
        leftButton = new SleekButtonIcon(icons.load<Texture2D>("Left"));
        leftButton.positionOffset_X = -185;
        leftButton.positionOffset_Y = 5;
        leftButton.positionScale_X = 1f;
        leftButton.positionScale_Y = 1f;
        leftButton.sizeOffset_X = 30;
        leftButton.sizeOffset_Y = 30;
        leftButton.tooltip = localization.format("Left_Tooltip");
        leftButton.iconColor = ESleekTint.FOREGROUND;
        leftButton.onClickedButton += onClickedLeftButton;
        inventory.AddChild(leftButton);
        rightButton = new SleekButtonIcon(icons.load<Texture2D>("Right"));
        rightButton.positionOffset_X = -35;
        rightButton.positionOffset_Y = 5;
        rightButton.positionScale_X = 1f;
        rightButton.positionScale_Y = 1f;
        rightButton.sizeOffset_X = 30;
        rightButton.sizeOffset_Y = 30;
        rightButton.tooltip = localization.format("Right_Tooltip");
        rightButton.iconColor = ESleekTint.FOREGROUND;
        rightButton.onClickedButton += onClickedRightButton;
        inventory.AddChild(rightButton);
        optionsButton = new SleekButtonIcon(icons.load<Texture2D>("Left"));
        optionsButton.positionOffset_X = 5;
        optionsButton.positionOffset_Y = -35;
        optionsButton.sizeOffset_X = 30;
        optionsButton.sizeOffset_Y = 30;
        optionsButton.tooltip = localization.format("Advanced_Options_Tooltip");
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        optionsButton.onClickedButton += onClickedOptionsButton;
        inventory.AddChild(optionsButton);
        optionsPanel = Glazier.Get().CreateFrame();
        optionsPanel.positionOffset_X = -200;
        optionsPanel.positionOffset_Y = -35;
        optionsPanel.sizeOffset_X = 200;
        optionsPanel.sizeOffset_Y = 400;
        optionsPanel.isVisible = false;
        inventory.AddChild(optionsPanel);
        searchDescriptionsToggle = Glazier.Get().CreateToggle();
        searchDescriptionsToggle.sizeOffset_X = 40;
        searchDescriptionsToggle.sizeOffset_Y = 40;
        searchDescriptionsToggle.addLabel(localization.format("Search_Descriptions_Label"), ESleekSide.RIGHT);
        searchDescriptionsToggle.state = searchDescriptions;
        searchDescriptionsToggle.onToggled += onToggledSearchDescriptions;
        optionsPanel.AddChild(searchDescriptionsToggle);
        sortModeButton = new SleekButtonState(new GUIContent(localization.format("Sort_Mode_Date")), new GUIContent(localization.format("Sort_Mode_Rarity")), new GUIContent(localization.format("Sort_Mode_Name")));
        sortModeButton.positionOffset_Y = 50;
        sortModeButton.sizeOffset_X = 100;
        sortModeButton.sizeOffset_Y = 30;
        sortModeButton.addLabel(localization.format("Sort_Mode_Label"), ESleekSide.RIGHT);
        sortModeButton.tooltip = localization.format("Sort_Mode_Tooltip");
        sortModeButton.state = (int)sortMode;
        sortModeButton.onSwappedState = onChangedSortMode;
        optionsPanel.AddChild(sortModeButton);
        reverseSortOrderToggle = Glazier.Get().CreateToggle();
        reverseSortOrderToggle.positionOffset_Y = 90;
        reverseSortOrderToggle.sizeOffset_X = 40;
        reverseSortOrderToggle.sizeOffset_Y = 40;
        reverseSortOrderToggle.addLabel(localization.format("Reverse_Sort_Order_Label"), ESleekSide.RIGHT);
        reverseSortOrderToggle.state = reverseSortOrder;
        reverseSortOrderToggle.onToggled += onToggledReverseSortOrder;
        optionsPanel.AddChild(reverseSortOrderToggle);
        filterEquippedToggle = Glazier.Get().CreateToggle();
        filterEquippedToggle.positionOffset_Y = 140;
        filterEquippedToggle.sizeOffset_X = 40;
        filterEquippedToggle.sizeOffset_Y = 40;
        filterEquippedToggle.addLabel(localization.format("Filter_Equipped_Label"), ESleekSide.RIGHT);
        filterEquippedToggle.state = filterEquipped;
        filterEquippedToggle.onToggled += onToggledFilterEquipped;
        optionsPanel.AddChild(filterEquippedToggle);
        refreshButton = new SleekButtonIcon(icons.load<Texture2D>("Refresh"));
        refreshButton.positionOffset_X = 5;
        refreshButton.positionOffset_Y = 5;
        refreshButton.positionScale_Y = 1f;
        refreshButton.sizeOffset_X = 30;
        refreshButton.sizeOffset_Y = 30;
        refreshButton.tooltip = localization.format("Refresh_Tooltip");
        refreshButton.iconColor = ESleekTint.FOREGROUND;
        refreshButton.onClickedButton += onClickedRefreshButton;
        inventory.AddChild(refreshButton);
        characterSlider = Glazier.Get().CreateSlider();
        characterSlider.positionOffset_X = 45;
        characterSlider.positionOffset_Y = 10;
        characterSlider.positionScale_Y = 1f;
        characterSlider.sizeOffset_X = -240;
        characterSlider.sizeOffset_Y = 20;
        characterSlider.sizeScale_X = 1f;
        characterSlider.orientation = ESleekOrientation.HORIZONTAL;
        characterSlider.onDragged += onDraggedCharacterSlider;
        inventory.AddChild(characterSlider);
        availableBox = Glazier.Get().CreateBox();
        availableBox.sizeScale_X = 1f;
        availableBox.sizeOffset_Y = 30;
        availableBox.enableRichText = true;
        availableBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        availableBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        crafting.AddChild(availableBox);
        craftingScrollBox = Glazier.Get().CreateScrollView();
        craftingScrollBox.positionOffset_Y = 40;
        craftingScrollBox.sizeScale_X = 1f;
        craftingScrollBox.sizeScale_Y = 1f;
        craftingScrollBox.sizeOffset_Y = -40;
        crafting.AddChild(craftingScrollBox);
        econCraftOptions = new List<EconCraftOption>
        {
            new EconCraftOption("Craft_Common_Cosmetic", 10003, 2),
            new EconCraftOption("Craft_Common_Skin", 10006, 2),
            new EconCraftOption("Craft_Uncommon_Cosmetic", 10004, 13),
            new EconCraftOption("Craft_Uncommon_Skin", 10007, 13),
            new EconCraftOption("Craft_Stat_Tracker_Total_Kills", 19001, 30),
            new EconCraftOption("Craft_Stat_Tracker_Player_Kills", 19002, 30),
            new EconCraftOption("Craft_Ragdoll_Effect_Zero_Kelvin", 19003, 50),
            new EconCraftOption("Craft_Mythical_Skin", 19043, 1000),
            new EconCraftOption("Craft_Stat_Tracker_Removal_Tool", 19004, 15),
            new EconCraftOption("Craft_Ragdoll_Effect_Removal_Tool", 19005, 15)
        };
        if (HolidayUtil.getActiveHoliday() == ENPCHoliday.PRIDE_MONTH)
        {
            econCraftOptions.Add(new EconCraftOption("Craft_ProgressPridePin", 1333, 10));
        }
        craftingButtons = new ISleekButton[econCraftOptions.Count];
        for (int j = 0; j < craftingButtons.Length; j++)
        {
            EconCraftOption econCraftOption = econCraftOptions[j];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_Y = j * 30;
            sleekButton.sizeScale_X = 1f;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.enableRichText = true;
            sleekButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekButton.text = ItemTool.filterRarityRichText(localization.format("Craft_Entry", localization.format(econCraftOption.token), econCraftOption.scrapsNeeded));
            sleekButton.onClickedButton += onClickedCraftButton;
            craftingScrollBox.AddChild(sleekButton);
            craftingButtons[j] = sleekButton;
        }
        craftingScrollBox.scaleContentToWidth = true;
        craftingScrollBox.contentSizeOffset = new Vector2(0f, econCraftOptions.Count * 30);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        itemstoreButton = Glazier.Get().CreateButton();
        itemstoreButton.positionOffset_Y = -170;
        itemstoreButton.positionScale_Y = 1f;
        itemstoreButton.sizeOffset_X = 200;
        itemstoreButton.sizeOffset_Y = 50;
        itemstoreButton.text = localization.format("Itemstore");
        itemstoreButton.tooltipText = localization.format("Itemstore_Tooltip");
        itemstoreButton.onClickedButton += onClickedItemstoreButton;
        itemstoreButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(itemstoreButton);
        if (!Provider.provider.economyService.doesCountryAllowRandomItems && Provider.provider.economyService.hasCountryDetails)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = 210;
            sleekLabel.positionOffset_Y = -170;
            sleekLabel.positionScale_Y = 1f;
            sleekLabel.sizeOffset_X = 200;
            sleekLabel.sizeOffset_Y = 50;
            sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
            sleekLabel.text = localization.format("Itemstore_Region_Box_Disabled", Provider.provider.economyService.getCountryWarningId());
            container.AddChild(sleekLabel);
        }
        craftingButton = new SleekButtonIcon(icons.load<Texture2D>("Crafting"));
        craftingButton.positionOffset_Y = -110;
        craftingButton.positionScale_Y = 1f;
        craftingButton.sizeOffset_X = 200;
        craftingButton.sizeOffset_Y = 50;
        craftingButton.text = localization.format("Crafting");
        craftingButton.tooltip = localization.format("Crafting_Tooltip");
        craftingButton.onClickedButton += onClickedCraftingButton;
        craftingButton.fontSize = ESleekFontSize.Medium;
        craftingButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(craftingButton);
        TempSteamworksEconomy economyService = Provider.provider.economyService;
        economyService.onInventoryExchanged = (TempSteamworksEconomy.InventoryExchanged)Delegate.Combine(economyService.onInventoryExchanged, new TempSteamworksEconomy.InventoryExchanged(onInventoryExchanged));
        TempSteamworksEconomy economyService2 = Provider.provider.economyService;
        economyService2.onInventoryPurchased = (TempSteamworksEconomy.InventoryExchanged)Delegate.Combine(economyService2.onInventoryPurchased, new TempSteamworksEconomy.InventoryExchanged(onInventoryPurchased));
        TempSteamworksEconomy economyService3 = Provider.provider.economyService;
        economyService3.onInventoryExchangeFailed = (TempSteamworksEconomy.InventoryExchangeFailed)Delegate.Combine(economyService3.onInventoryExchangeFailed, new TempSteamworksEconomy.InventoryExchangeFailed(onInventoryExchangeFailed));
        TempSteamworksEconomy economyService4 = Provider.provider.economyService;
        economyService4.onInventoryRefreshed = (TempSteamworksEconomy.InventoryRefreshed)Delegate.Combine(economyService4.onInventoryRefreshed, new TempSteamworksEconomy.InventoryRefreshed(onInventoryRefreshed));
        TempSteamworksEconomy economyService5 = Provider.provider.economyService;
        economyService5.onInventoryDropped = (TempSteamworksEconomy.InventoryDropped)Delegate.Combine(economyService5.onInventoryDropped, new TempSteamworksEconomy.InventoryDropped(onInventoryDropped));
        Characters.onCharacterUpdated = (CharacterUpdated)Delegate.Combine(Characters.onCharacterUpdated, new CharacterUpdated(onCharacterUpdated));
        ItemStore.Get().OnPricesReceived += OnPricesReceived;
        updateFilter();
        updatePage();
        itemUI = new MenuSurvivorsClothingItemUI();
        inspectUI = new MenuSurvivorsClothingInspectUI();
        deleteUI = new MenuSurvivorsClothingDeleteUI();
        boxUI = new MenuSurvivorsClothingBoxUI();
        itemStoreUI = new ItemStoreMenu();
        MenuUI.container.AddChild(itemStoreUI);
    }
}
