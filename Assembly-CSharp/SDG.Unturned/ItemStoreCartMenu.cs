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
        base.PositionScale_Y = 1f;
        base.PositionOffset_X = 10f;
        base.PositionOffset_Y = 10f;
        base.SizeOffset_X = -20f;
        base.SizeOffset_Y = -20f;
        base.SizeScale_X = 1f;
        base.SizeScale_Y = 1f;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionScale_X = 0.5f;
        sleekElement.PositionOffset_Y = 10f;
        sleekElement.SizeScale_X = 0.5f;
        sleekElement.SizeScale_Y = 1f;
        sleekElement.SizeOffset_Y = -20f;
        AddChild(sleekElement);
        scrollView = Glazier.Get().CreateScrollView();
        scrollView.SizeScale_X = 1f;
        scrollView.SizeScale_Y = 1f;
        scrollView.SizeOffset_Y = -110f;
        scrollView.ScaleContentToWidth = true;
        scrollView.ReduceWidthWhenScrollbarVisible = false;
        sleekElement.AddChild(scrollView);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.SizeScale_X = 1f;
        sleekElement2.SizeOffset_X = -30f;
        sleekElement2.SizeOffset_Y = 105f;
        sleekElement2.PositionScale_Y = 1f;
        sleekElement2.PositionOffset_Y = -105f;
        sleekElement.AddChild(sleekElement2);
        totalPriceBox = new SleekItemStorePriceBox();
        totalPriceBox.PositionScale_X = 0.8f;
        totalPriceBox.SizeScale_X = 0.2f;
        totalPriceBox.SizeOffset_Y = 50f;
        sleekElement2.AddChild(totalPriceBox);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = -5f;
        sleekLabel.SizeScale_X = 0.8f;
        sleekLabel.SizeOffset_X = -5f;
        sleekLabel.SizeOffset_Y = 50f;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.TextAlignment = TextAnchor.MiddleRight;
        sleekLabel.Text = localization.format("TotalPrice_Label");
        sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekElement2.AddChild(sleekLabel);
        startPurchaseButton = Glazier.Get().CreateButton();
        startPurchaseButton.PositionOffset_Y = 55f;
        startPurchaseButton.SizeScale_X = 1f;
        startPurchaseButton.SizeOffset_Y = 50f;
        startPurchaseButton.FontSize = ESleekFontSize.Medium;
        startPurchaseButton.Text = localization.format("StartPurchase_Label");
        startPurchaseButton.TooltipText = localization.format("StartPurchase_Tooltip");
        startPurchaseButton.OnClicked += OnClickedStartPurchase;
        sleekElement2.AddChild(startPurchaseButton);
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
            sleekItemStoreCartEntry.SizeOffset_X = -30f;
            sleekItemStoreCartEntry.SizeScale_X = 1f;
            sleekItemStoreCartEntry.SizeOffset_Y = 50f;
            sleekItemStoreCartEntry.PositionOffset_Y = num;
            num += 55;
            scrollView.AddChild(sleekItemStoreCartEntry);
            entries.Add(sleekItemStoreCartEntry);
        }
        scrollView.ContentSizeOffset = new Vector2(0f, num - 5);
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
        startPurchaseButton.IsClickable = !ItemStore.Get().IsCartEmpty;
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
