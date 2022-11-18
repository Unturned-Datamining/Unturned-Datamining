using UnityEngine;

namespace SDG.Unturned;

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
        sleekButton.sizeScale_X = 1f;
        sleekButton.sizeScale_Y = 1f;
        sleekButton.onClickedButton += OnClickedItemButton;
        sleekButton.textColor = inventoryColor;
        sleekButton.tooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        sleekButton.backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        AddChild(sleekButton);
        Texture2D texture = Provider.provider.economyService.LoadItemIcon(listing.itemdefid, large: false);
        ISleekImage sleekImage = Glazier.Get().CreateImage(texture);
        sleekImage.positionOffset_X = 5;
        sleekImage.positionOffset_Y = 5;
        sleekImage.sizeOffset_X = 40;
        sleekImage.sizeOffset_Y = 40;
        sleekButton.AddChild(sleekImage);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 50;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.sizeScale_Y = 1f;
        sleekLabel.sizeOffset_X = -50;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.fontSize = ESleekFontSize.Medium;
        sleekLabel.text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        sleekLabel.textColor = inventoryColor;
        sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        sleekButton.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.sizeScale_X = 1f;
        sleekLabel2.sizeScale_Y = 1f;
        sleekLabel2.fontAlignment = TextAnchor.LowerRight;
        sleekLabel2.textColor = ItemStore.PremiumColor;
        sleekLabel2.text = ItemStore.Get().FormatPrice(listing.currentPrice);
        sleekLabel2.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        sleekButton.AddChild(sleekLabel2);
        if (labelType != 0)
        {
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.sizeScale_X = 1f;
            sleekLabel3.sizeScale_Y = 1f;
            sleekLabel3.fontAlignment = TextAnchor.UpperRight;
            sleekLabel3.textColor = Color.green;
            sleekLabel3.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekButton.AddChild(sleekLabel3);
            switch (labelType)
            {
            case ELabelType.New:
                hasNewLabel = true;
                sleekLabel3.text = Provider.localization.format("New");
                break;
            case ELabelType.Sale:
                sleekLabel3.text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale") + "\n" + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
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
        base.isVisible = false;
    }
}
