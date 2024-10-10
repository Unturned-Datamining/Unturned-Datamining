using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Shows inspect buttons for each item mentioned in purchasable box or bundle's description text.
/// </summary>
internal class ItemStoreBundleContentsMenu : SleekFullscreenBox
{
    public static ItemStoreBundleContentsMenu instance;

    private ItemStore.Listing listing;

    private List<int> containedItems;

    private ISleekBox headerBox;

    private SleekEconIcon headerIconImage;

    private ISleekLabel headerLabel;

    private ISleekScrollView scrollView;

    public bool IsOpen { get; private set; }

    public void Open(ItemStore.Listing listing)
    {
        IsOpen = true;
        AnimateIntoView();
        this.listing = listing;
        containedItems = Provider.provider.economyService.GetBundleContents(listing.itemdefid);
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        string inventoryName = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        headerIconImage.SetItemDefId(listing.itemdefid);
        bool flag = Provider.provider.economyService.IsItemBundle(listing.itemdefid);
        inventoryName = "<color=" + Palette.hex(inventoryColor) + ">" + inventoryName + "</color>";
        string key = (flag ? "ListedItemsHeader_Bundle" : "ListedItemsHeader_Box");
        headerLabel.Text = ItemStoreMenu.instance.localization.format(key, inventoryName, containedItems.Count);
        RefreshEntries();
    }

    public void OpenCurrentListing()
    {
        IsOpen = true;
        AnimateIntoView();
    }

    public void Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            AnimateOutOfView(0f, 1f);
        }
    }

    public ItemStoreBundleContentsMenu()
    {
        _ = ItemStoreMenu.instance.localization;
        instance = this;
        base.PositionScale_Y = 1f;
        base.PositionOffset_X = 10f;
        base.PositionOffset_Y = 10f;
        base.SizeOffset_X = -20f;
        base.SizeOffset_Y = -20f;
        base.SizeScale_X = 1f;
        base.SizeScale_Y = 1f;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionScale_X = 0.6f;
        sleekElement.PositionOffset_Y = 10f;
        sleekElement.SizeScale_X = 0.3f;
        sleekElement.SizeScale_Y = 1f;
        sleekElement.SizeOffset_Y = -20f;
        AddChild(sleekElement);
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeScale_X = 1f;
        sleekBox.SizeOffset_X = -30f;
        sleekBox.SizeOffset_Y = 50f;
        sleekElement.AddChild(sleekBox);
        headerIconImage = new SleekEconIcon();
        headerIconImage.PositionOffset_X = 5f;
        headerIconImage.PositionOffset_Y = 5f;
        headerIconImage.SizeOffset_X = 40f;
        headerIconImage.SizeOffset_Y = 40f;
        sleekBox.AddChild(headerIconImage);
        headerLabel = Glazier.Get().CreateLabel();
        headerLabel.PositionOffset_X = 50f;
        headerLabel.SizeScale_X = 1f;
        headerLabel.SizeScale_Y = 1f;
        headerLabel.SizeOffset_X = -50f;
        headerLabel.TextAlignment = TextAnchor.MiddleLeft;
        headerLabel.FontSize = ESleekFontSize.Medium;
        headerLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        headerLabel.AllowRichText = true;
        headerLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekBox.AddChild(headerLabel);
        scrollView = Glazier.Get().CreateScrollView();
        scrollView.PositionOffset_Y = 55f;
        scrollView.SizeScale_X = 1f;
        scrollView.SizeScale_Y = 1f;
        scrollView.SizeOffset_Y = -55f;
        scrollView.ScaleContentToWidth = true;
        scrollView.ReduceWidthWhenScrollbarVisible = false;
        sleekElement.AddChild(scrollView);
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        sleekButtonIcon.PositionOffset_Y = -50f;
        sleekButtonIcon.PositionScale_Y = 1f;
        sleekButtonIcon.SizeOffset_X = 200f;
        sleekButtonIcon.SizeOffset_Y = 50f;
        sleekButtonIcon.text = MenuDashboardUI.localization.format("BackButtonText");
        sleekButtonIcon.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        sleekButtonIcon.onClickedButton += OnClickedBackButton;
        sleekButtonIcon.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        AddChild(sleekButtonIcon);
    }

    private void RefreshEntries()
    {
        scrollView.RemoveAllChildren();
        int num = 0;
        foreach (int containedItem in containedItems)
        {
            SleekItemStoreBundleContentEntry sleekItemStoreBundleContentEntry = new SleekItemStoreBundleContentEntry(containedItem);
            sleekItemStoreBundleContentEntry.SizeOffset_X = -30f;
            sleekItemStoreBundleContentEntry.SizeScale_X = 1f;
            sleekItemStoreBundleContentEntry.SizeOffset_Y = 50f;
            sleekItemStoreBundleContentEntry.PositionOffset_Y = num;
            num += 55;
            scrollView.AddChild(sleekItemStoreBundleContentEntry);
        }
        scrollView.ContentSizeOffset = new Vector2(0f, num - 5);
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        ItemStoreDetailsMenu.instance.OpenCurrentListing();
        Close();
    }
}
