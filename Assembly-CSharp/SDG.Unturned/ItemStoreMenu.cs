using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class ItemStoreMenu : SleekFullscreenBox
{
    private enum ECategoryFilter
    {
        None,
        Bundles,
        Specials,
        New,
        Featured
    }

    public static ItemStoreMenu instance;

    private ItemStoreCartMenu cartMenu;

    private ItemStoreDetailsMenu detailsMenu;

    private SleekItemStoreListing[] listingButtons;

    private ISleekElement categoryButtonsFrame;

    private ISleekField searchField;

    private ISleekBox pageBox;

    private ISleekButton viewCartButton;

    private List<ItemStore.Listing> filteredListings;

    private int pageIndex;

    private int pageCount;

    private bool areListingsDirty;

    private ECategoryFilter categoryFilter;

    public Local localization { get; private set; }

    public Bundle icons { get; private set; }

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        AnimateIntoView();
        viewCartButton.isVisible = !ItemStore.Get().IsCartEmpty;
        if (areListingsDirty)
        {
            areListingsDirty = false;
            FilterListings();
        }
        else
        {
            RefreshListingsInCart();
        }
    }

    public void OpenNewItems()
    {
        searchField.text = string.Empty;
        categoryFilter = ECategoryFilter.New;
        areListingsDirty = true;
        Open();
    }

    public void Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            AnimateOutOfView(0f, 1f);
        }
    }

    public ItemStoreMenu()
    {
        localization = Localization.read("/Menu/Survivors/ItemStoreMenu.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/ItemStore/ItemStore.unity3d");
        instance = this;
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.positionOffset_Y = 70;
        sleekConstraintFrame.positionScale_X = 0.5f;
        sleekConstraintFrame.sizeScale_X = 0.5f;
        sleekConstraintFrame.sizeScale_Y = 1f;
        sleekConstraintFrame.sizeOffset_Y = -105;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        AddChild(sleekConstraintFrame);
        listingButtons = new SleekItemStoreListing[25];
        for (int i = 0; i < 25; i++)
        {
            SleekItemStoreListing sleekItemStoreListing = new SleekItemStoreListing
            {
                positionOffset_X = 5,
                positionOffset_Y = 5,
                positionScale_X = (float)(i % 5) * 0.2f,
                positionScale_Y = (float)Mathf.FloorToInt((float)i / 5f) * 0.2f,
                sizeOffset_X = -10,
                sizeOffset_Y = -10,
                sizeScale_X = 0.2f,
                sizeScale_Y = 0.2f
            };
            sleekConstraintFrame.AddChild(sleekItemStoreListing);
            listingButtons[i] = sleekItemStoreListing;
        }
        categoryButtonsFrame = Glazier.Get().CreateFrame();
        categoryButtonsFrame.positionOffset_Y = -70;
        categoryButtonsFrame.sizeScale_X = 1f;
        categoryButtonsFrame.sizeOffset_Y = 30;
        sleekConstraintFrame.AddChild(categoryButtonsFrame);
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_Y = -35;
        searchField.sizeOffset_X = -110;
        searchField.sizeOffset_Y = 30;
        searchField.sizeScale_X = 1f;
        searchField.hint = MenuSurvivorsClothingUI.localization.format("Search_Field_Hint");
        searchField.onEntered += OnEnteredSearchField;
        sleekConstraintFrame.AddChild(searchField);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_X = -105;
        sleekButton.positionOffset_Y = -35;
        sleekButton.positionScale_X = 1f;
        sleekButton.sizeOffset_X = 100;
        sleekButton.sizeOffset_Y = 30;
        sleekButton.text = MenuSurvivorsClothingUI.localization.format("Search");
        sleekButton.tooltipText = MenuSurvivorsClothingUI.localization.format("Search_Tooltip");
        sleekButton.onClickedButton += OnClickedSearchButton;
        sleekConstraintFrame.AddChild(sleekButton);
        pageBox = Glazier.Get().CreateBox();
        pageBox.positionOffset_X = -50;
        pageBox.positionOffset_Y = 5;
        pageBox.positionScale_X = 0.5f;
        pageBox.positionScale_Y = 1f;
        pageBox.sizeOffset_X = 100;
        pageBox.sizeOffset_Y = 30;
        pageBox.fontSize = ESleekFontSize.Medium;
        sleekConstraintFrame.AddChild(pageBox);
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuSurvivorsClothingUI.icons.load<Texture2D>("Left"));
        sleekButtonIcon.positionOffset_X = -85;
        sleekButtonIcon.positionOffset_Y = 5;
        sleekButtonIcon.positionScale_X = 0.5f;
        sleekButtonIcon.positionScale_Y = 1f;
        sleekButtonIcon.sizeOffset_X = 30;
        sleekButtonIcon.sizeOffset_Y = 30;
        sleekButtonIcon.tooltip = MenuSurvivorsClothingUI.localization.format("Left_Tooltip");
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        sleekButtonIcon.onClickedButton += OnClickedLeftPageButton;
        sleekConstraintFrame.AddChild(sleekButtonIcon);
        SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(MenuSurvivorsClothingUI.icons.load<Texture2D>("Right"));
        sleekButtonIcon2.positionOffset_X = 55;
        sleekButtonIcon2.positionOffset_Y = 5;
        sleekButtonIcon2.positionScale_X = 0.5f;
        sleekButtonIcon2.positionScale_Y = 1f;
        sleekButtonIcon2.sizeOffset_X = 30;
        sleekButtonIcon2.sizeOffset_Y = 30;
        sleekButtonIcon2.tooltip = MenuSurvivorsClothingUI.localization.format("Right_Tooltip");
        sleekButtonIcon2.iconColor = ESleekTint.FOREGROUND;
        sleekButtonIcon2.onClickedButton += OnClickedRightPageButton;
        sleekConstraintFrame.AddChild(sleekButtonIcon2);
        viewCartButton = Glazier.Get().CreateButton();
        viewCartButton.positionOffset_Y = -110;
        viewCartButton.positionScale_Y = 1f;
        viewCartButton.sizeOffset_X = 200;
        viewCartButton.sizeOffset_Y = 50;
        viewCartButton.text = localization.format("ViewCart_Label");
        viewCartButton.tooltipText = localization.format("ViewCart_Tooltip");
        viewCartButton.onClickedButton += OnClickedViewCartButton;
        viewCartButton.fontSize = ESleekFontSize.Medium;
        AddChild(viewCartButton);
        ISleekSprite sleekSprite = Glazier.Get().CreateSprite(icons.load<Sprite>("Cart"));
        sleekSprite.positionOffset_X = 5;
        sleekSprite.positionOffset_Y = 5;
        sleekSprite.sizeOffset_X = 40;
        sleekSprite.sizeOffset_Y = 40;
        sleekSprite.color = ESleekTint.FOREGROUND;
        sleekSprite.drawMethod = ESleekSpriteType.Regular;
        viewCartButton.AddChild(sleekSprite);
        SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        sleekButtonIcon3.positionOffset_Y = -50;
        sleekButtonIcon3.positionScale_Y = 1f;
        sleekButtonIcon3.sizeOffset_X = 200;
        sleekButtonIcon3.sizeOffset_Y = 50;
        sleekButtonIcon3.text = MenuDashboardUI.localization.format("BackButtonText");
        sleekButtonIcon3.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        sleekButtonIcon3.onClickedButton += OnClickedBackButton;
        sleekButtonIcon3.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon3.iconColor = ESleekTint.FOREGROUND;
        AddChild(sleekButtonIcon3);
        cartMenu = new ItemStoreCartMenu();
        MenuUI.container.AddChild(cartMenu);
        detailsMenu = new ItemStoreDetailsMenu();
        MenuUI.container.AddChild(detailsMenu);
        ItemStore.Get().OnPricesReceived += OnPricesReceived;
        ItemStore.Get().OnPurchaseResult += OnPurchaseResult;
        ItemStore.Get().RequestPrices();
    }

    public override void OnDestroy()
    {
        ItemStore.Get().OnPricesReceived -= OnPricesReceived;
        ItemStore.Get().OnPurchaseResult -= OnPurchaseResult;
        base.OnDestroy();
    }

    private void OnPricesReceived()
    {
        filteredListings = new List<ItemStore.Listing>(ItemStore.Get().GetListings().Length);
        CreateFilterCategoryButtons();
        areListingsDirty = true;
    }

    private void OnPurchaseResult(ItemStore.EPurchaseResult result)
    {
        switch (result)
        {
        case ItemStore.EPurchaseResult.UnableToInitialize:
            MenuUI.alert(localization.format("PurchaseResult_UnableToInitialize"));
            break;
        case ItemStore.EPurchaseResult.Denied:
            MenuUI.alert(localization.format("PurchaseResult_Denied"));
            break;
        }
    }

    private void BuildListingsFromIndices(int[] listingIndices)
    {
        ItemStore.Listing[] listings = ItemStore.Get().GetListings();
        foreach (int num in listingIndices)
        {
            filteredListings.Add(listings[num]);
        }
    }

    private void BuildCategoryListings()
    {
        if (categoryFilter == ECategoryFilter.Specials)
        {
            BuildListingsFromIndices(ItemStore.Get().GetDiscountedListingIndices());
            return;
        }
        if (categoryFilter == ECategoryFilter.New)
        {
            BuildListingsFromIndices(ItemStore.Get().GetNewListingIndices());
            return;
        }
        if (categoryFilter == ECategoryFilter.Featured)
        {
            BuildListingsFromIndices(ItemStore.Get().GetFeaturedListingIndices());
            return;
        }
        ItemStore.Listing[] listings = ItemStore.Get().GetListings();
        filteredListings.AddRange(listings);
        if (categoryFilter != ECategoryFilter.Bundles)
        {
            return;
        }
        for (int num = filteredListings.Count - 1; num >= 0; num--)
        {
            if (!Provider.provider.economyService.getInventoryType(filteredListings[num].itemdefid).EndsWith("Bundle"))
            {
                filteredListings.RemoveAtFast(num);
            }
        }
    }

    private void ApplySearchTextFilter()
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
        for (int num = filteredListings.Count - 1; num >= 0; num--)
        {
            int itemdefid = filteredListings[num].itemdefid;
            string inventoryName = Provider.provider.economyService.getInventoryName(itemdefid);
            if (!tokenSearchFilter.Value.matches(inventoryName))
            {
                string inventoryType = Provider.provider.economyService.getInventoryType(itemdefid);
                if (!tokenSearchFilter.Value.matches(inventoryType))
                {
                    filteredListings.RemoveAtFast(num);
                }
            }
        }
    }

    private void SortListings()
    {
        filteredListings.Sort(delegate(ItemStore.Listing lhs, ItemStore.Listing rhs)
        {
            string inventoryName = Provider.provider.economyService.getInventoryName(lhs.itemdefid);
            string inventoryName2 = Provider.provider.economyService.getInventoryName(rhs.itemdefid);
            return inventoryName.CompareTo(inventoryName2);
        });
    }

    private void FilterListings()
    {
        filteredListings.Clear();
        BuildCategoryListings();
        ApplySearchTextFilter();
        SortListings();
        pageCount = MathfEx.GetPageCount(filteredListings.Count, listingButtons.Length);
        if (pageIndex >= pageCount)
        {
            pageIndex = pageCount - 1;
        }
        RefreshPage();
    }

    private void RefreshListingsInCart()
    {
        SleekItemStoreListing[] array = listingButtons;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].RefreshInCart();
        }
    }

    private void RefreshPage()
    {
        pageBox.text = MenuSurvivorsClothingUI.localization.format("Page", pageIndex + 1, pageCount);
        int num = pageIndex * listingButtons.Length;
        int num2 = Mathf.Min(filteredListings.Count - num, listingButtons.Length);
        for (int i = 0; i < num2; i++)
        {
            int index = num + i;
            listingButtons[i].SetListing(filteredListings[index]);
        }
        for (int j = num2; j < listingButtons.Length; j++)
        {
            listingButtons[j].ClearListing();
        }
    }

    private void CreateFilterCategoryButtons()
    {
        ItemStore itemStore = ItemStore.Get();
        bool hasDiscountedListings = itemStore.HasDiscountedListings;
        bool hasNewListings = itemStore.HasNewListings;
        bool hasFeaturedListings = itemStore.HasFeaturedListings;
        int num = 2 + (hasNewListings ? 1 : 0) + (hasDiscountedListings ? 1 : 0) + (hasFeaturedListings ? 1 : 0);
        float num2 = 0f;
        float num3 = 1f / (float)num;
        if (hasNewListings)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionScale_X = num2;
            sleekButton.sizeScale_X = num3;
            sleekButton.sizeScale_Y = 1f;
            sleekButton.text = localization.format("FilterNewButton_Label") + " x" + itemStore.GetNewListingIndices().Length;
            sleekButton.tooltipText = localization.format("FilterNewButton_Tooltip");
            sleekButton.onClickedButton += OnClickedFilterNew;
            categoryButtonsFrame.AddChild(sleekButton);
            num2 += num3;
        }
        if (hasFeaturedListings)
        {
            ISleekButton sleekButton2 = Glazier.Get().CreateButton();
            sleekButton2.positionScale_X = num2;
            sleekButton2.sizeScale_X = num3;
            sleekButton2.sizeScale_Y = 1f;
            sleekButton2.text = localization.format("FilterFeaturedButton_Label") + " x" + itemStore.GetFeaturedListingIndices().Length;
            sleekButton2.tooltipText = localization.format("FilterFeaturedButton_Label");
            sleekButton2.onClickedButton += OnClickedFilterFeatured;
            categoryButtonsFrame.AddChild(sleekButton2);
            num2 += num3;
        }
        ISleekButton sleekButton3 = Glazier.Get().CreateButton();
        sleekButton3.positionScale_X = num2;
        sleekButton3.sizeScale_X = num3;
        sleekButton3.sizeScale_Y = 1f;
        sleekButton3.text = localization.format("FilterAllButton_Label");
        sleekButton3.tooltipText = localization.format("FilterAllButton_Tooltip");
        sleekButton3.onClickedButton += OnClickedFilterAll;
        categoryButtonsFrame.AddChild(sleekButton3);
        num2 += num3;
        ISleekButton sleekButton4 = Glazier.Get().CreateButton();
        sleekButton4.positionScale_X = num2;
        sleekButton4.sizeScale_X = num3;
        sleekButton4.sizeScale_Y = 1f;
        sleekButton4.text = localization.format("FilterBundlesButton_Label");
        sleekButton4.tooltipText = localization.format("FilterBundlesButton_Tooltip");
        sleekButton4.onClickedButton += OnClickedFilterBundles;
        categoryButtonsFrame.AddChild(sleekButton4);
        num2 += num3;
        if (hasDiscountedListings)
        {
            ISleekButton sleekButton5 = Glazier.Get().CreateButton();
            sleekButton5.positionScale_X = num2;
            sleekButton5.sizeScale_X = num3;
            sleekButton5.sizeScale_Y = 1f;
            sleekButton5.text = localization.format("FilterSpecialsButton_Label") + " x" + itemStore.GetDiscountedListingIndices().Length;
            sleekButton5.tooltipText = localization.format("FilterSpecialsButton_Tooltip");
            sleekButton5.onClickedButton += OnClickedFilterSpecials;
            categoryButtonsFrame.AddChild(sleekButton5);
        }
    }

    private void OnClickedLeftPageButton(ISleekElement button)
    {
        if (pageIndex > 0)
        {
            pageIndex--;
        }
        else
        {
            pageIndex = pageCount - 1;
        }
        RefreshPage();
    }

    private void OnClickedRightPageButton(ISleekElement button)
    {
        if (pageIndex < pageCount - 1)
        {
            pageIndex++;
        }
        else
        {
            pageIndex = 0;
        }
        RefreshPage();
    }

    private void OnClickedFilterAll(ISleekElement button)
    {
        categoryFilter = ECategoryFilter.None;
        FilterListings();
    }

    private void OnClickedFilterBundles(ISleekElement button)
    {
        categoryFilter = ECategoryFilter.Bundles;
        FilterListings();
    }

    private void OnClickedFilterSpecials(ISleekElement button)
    {
        categoryFilter = ECategoryFilter.Specials;
        FilterListings();
    }

    private void OnClickedFilterNew(ISleekElement button)
    {
        categoryFilter = ECategoryFilter.New;
        FilterListings();
    }

    private void OnClickedFilterFeatured(ISleekElement button)
    {
        categoryFilter = ECategoryFilter.Featured;
        FilterListings();
    }

    private void OnEnteredSearchField(ISleekField field)
    {
        FilterListings();
    }

    private void OnClickedSearchButton(ISleekElement button)
    {
        FilterListings();
    }

    private void OnClickedViewCartButton(ISleekElement button)
    {
        cartMenu.Open();
        Close();
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsClothingUI.open();
        Close();
    }
}
