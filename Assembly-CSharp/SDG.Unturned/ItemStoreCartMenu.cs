using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class ItemStoreCartMenu : SleekFullscreenBox
{
    public static ItemStoreCartMenu instance;

    private ISleekScrollView scrollView;

    private List<SleekItemStoreCartEntry> entries = new List<SleekItemStoreCartEntry>();

    private SleekItemStorePriceBox totalPriceBox;

    private ISleekButton startPurchaseButton;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        AnimateIntoView();
        RefreshCartEntries();
        ItemStore.Get().OnCartChanged += OnCartChanged;
    }

    public void Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            AnimateOutOfView(0f, 1f);
            ItemStore.Get().OnCartChanged -= OnCartChanged;
        }
    }

    public override void OnDestroy()
    {
        ItemStore.Get().OnCartChanged -= OnCartChanged;
        base.OnDestroy();
    }

    public ItemStoreCartMenu()
    {
        instance = this;
        Local localization = ItemStoreMenu.instance.localization;
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.positionScale_X = 0.5f;
        sleekElement.positionOffset_Y = 10;
        sleekElement.sizeScale_X = 0.5f;
        sleekElement.sizeScale_Y = 1f;
        sleekElement.sizeOffset_Y = -20;
        AddChild(sleekElement);
        scrollView = Glazier.Get().CreateScrollView();
        scrollView.sizeScale_X = 1f;
        scrollView.sizeScale_Y = 1f;
        scrollView.sizeOffset_Y = -110;
        scrollView.scaleContentToWidth = true;
        scrollView.reduceWidthWhenScrollbarVisible = false;
        sleekElement.AddChild(scrollView);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.sizeScale_X = 1f;
        sleekElement2.sizeOffset_X = -30;
        sleekElement2.sizeOffset_Y = 105;
        sleekElement2.positionScale_Y = 1f;
        sleekElement2.positionOffset_Y = -105;
        sleekElement.AddChild(sleekElement2);
        totalPriceBox = new SleekItemStorePriceBox();
        totalPriceBox.positionScale_X = 0.8f;
        totalPriceBox.sizeScale_X = 0.2f;
        totalPriceBox.sizeOffset_Y = 50;
        sleekElement2.AddChild(totalPriceBox);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = -5;
        sleekLabel.sizeScale_X = 0.8f;
        sleekLabel.sizeOffset_X = -5;
        sleekLabel.sizeOffset_Y = 50;
        sleekLabel.fontSize = ESleekFontSize.Medium;
        sleekLabel.fontAlignment = TextAnchor.MiddleRight;
        sleekLabel.text = localization.format("TotalPrice_Label");
        sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        sleekElement2.AddChild(sleekLabel);
        startPurchaseButton = Glazier.Get().CreateButton();
        startPurchaseButton.positionOffset_Y = 55;
        startPurchaseButton.sizeScale_X = 1f;
        startPurchaseButton.sizeOffset_Y = 50;
        startPurchaseButton.fontSize = ESleekFontSize.Medium;
        startPurchaseButton.text = localization.format("StartPurchase_Label");
        startPurchaseButton.tooltipText = localization.format("StartPurchase_Tooltip");
        startPurchaseButton.onClickedButton += OnClickedStartPurchase;
        sleekElement2.AddChild(startPurchaseButton);
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        sleekButtonIcon.positionOffset_Y = -50;
        sleekButtonIcon.positionScale_Y = 1f;
        sleekButtonIcon.sizeOffset_X = 200;
        sleekButtonIcon.sizeOffset_Y = 50;
        sleekButtonIcon.text = MenuDashboardUI.localization.format("BackButtonText");
        sleekButtonIcon.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        sleekButtonIcon.onClickedButton += OnClickedBackButton;
        sleekButtonIcon.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        AddChild(sleekButtonIcon);
    }

    private void RefreshCartEntries()
    {
        scrollView.RemoveAllChildren();
        int num = 0;
        entries.Clear();
        foreach (ItemStore.CartEntry item in ItemStore.Get().GetCart())
        {
            if (!ItemStore.Get().FindListing(item.itemdefid, out var listing))
            {
                UnturnedLog.warn("Item store itemdefid {0} x{1} in cart without listing", item.itemdefid, item.quantity);
                continue;
            }
            SleekItemStoreCartEntry sleekItemStoreCartEntry = new SleekItemStoreCartEntry(item, listing);
            sleekItemStoreCartEntry.sizeOffset_X = -30;
            sleekItemStoreCartEntry.sizeScale_X = 1f;
            sleekItemStoreCartEntry.sizeOffset_Y = 50;
            sleekItemStoreCartEntry.positionOffset_Y = num;
            num += 55;
            scrollView.AddChild(sleekItemStoreCartEntry);
            entries.Add(sleekItemStoreCartEntry);
        }
        scrollView.contentSizeOffset = new Vector2(0f, num - 5);
        OnCartChanged();
    }

    private void OnCartChanged()
    {
        ulong num = 0uL;
        ulong num2 = 0uL;
        foreach (SleekItemStoreCartEntry entry in entries)
        {
            entry.GetTotalPrice(out var basePrice, out var currentPrice);
            num += basePrice;
            num2 += currentPrice;
        }
        totalPriceBox.SetPrice(num, num2, 1);
        startPurchaseButton.isClickable = !ItemStore.Get().IsCartEmpty;
    }

    private void OnClickedStartPurchase(ISleekElement button)
    {
        MenuSurvivorsClothingUI.open();
        Close();
        ItemStore.Get().StartPurchase();
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        ItemStoreMenu.instance.Open();
        Close();
    }
}
