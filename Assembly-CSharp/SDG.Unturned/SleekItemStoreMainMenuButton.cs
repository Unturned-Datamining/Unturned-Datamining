using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Displays a single random item. Placed under the other main menu buttons.
/// </summary>
internal class SleekItemStoreMainMenuButton : SleekWrapper
{
    public enum ELabelType
    {
        None,
        New,
        Sale
    }

    private ItemStore.Listing listing;

    private bool hasNewLabel;

    public SleekItemStoreMainMenuButton(ItemStore.Listing listing, ELabelType labelType)
    {
        this.listing = listing;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.SizeScale_X = 1f;
        sleekButton.SizeScale_Y = 1f;
        sleekButton.OnClicked += OnClickedItemButton;
        sleekButton.TextColor = inventoryColor;
        sleekButton.TooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        sleekButton.BackgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        AddChild(sleekButton);
        SleekEconIcon sleekEconIcon = new SleekEconIcon();
        sleekEconIcon.PositionOffset_X = 5f;
        sleekEconIcon.PositionOffset_Y = 5f;
        sleekEconIcon.SizeOffset_X = 40f;
        sleekEconIcon.SizeOffset_Y = 40f;
        sleekEconIcon.SetItemDefId(listing.itemdefid);
        sleekButton.AddChild(sleekEconIcon);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 50f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeScale_Y = 1f;
        sleekLabel.SizeOffset_X = -50f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.Text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        sleekLabel.TextColor = inventoryColor;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekButton.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.SizeScale_Y = 1f;
        sleekLabel2.TextAlignment = TextAnchor.LowerRight;
        sleekLabel2.TextColor = ItemStore.PremiumColor;
        sleekLabel2.Text = ItemStore.Get().FormatPrice(listing.currentPrice);
        sleekLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekButton.AddChild(sleekLabel2);
        if (labelType != 0)
        {
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.SizeScale_X = 1f;
            sleekLabel3.SizeScale_Y = 1f;
            sleekLabel3.TextAlignment = TextAnchor.UpperRight;
            sleekLabel3.TextColor = Color.green;
            sleekLabel3.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekButton.AddChild(sleekLabel3);
            switch (labelType)
            {
            case ELabelType.New:
                hasNewLabel = true;
                sleekLabel3.Text = Provider.localization.format("New");
                break;
            case ELabelType.Sale:
                sleekLabel3.Text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale") + "\n" + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
                break;
            }
        }
    }

    private void OnClickedItemButton(ISleekElement button)
    {
        if (hasNewLabel)
        {
            ItemStoreSavedata.MarkNewListingSeen(listing.itemdefid);
        }
        ItemStore.Get().ViewItem(listing.itemdefid);
        base.IsVisible = false;
    }
}
