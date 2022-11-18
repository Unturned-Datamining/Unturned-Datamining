using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStoreListing : SleekWrapper
{
    private ItemStore.Listing listing;

    private bool hasNewLabel;

    private ISleekButton button;

    private ISleekImage iconImage;

    private ISleekLabel nameAndPriceLabel;

    private ISleekSprite cartImage;

    private ISleekLabel stampLabel;

    public SleekItemStoreListing()
    {
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += OnClickedButton;
        AddChild(button);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.positionOffset_X = 5;
        sleekConstraintFrame.positionOffset_Y = 5;
        sleekConstraintFrame.sizeScale_X = 1f;
        sleekConstraintFrame.sizeScale_Y = 1f;
        sleekConstraintFrame.sizeOffset_X = -10;
        sleekConstraintFrame.sizeOffset_Y = -50;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        AddChild(sleekConstraintFrame);
        iconImage = Glazier.Get().CreateImage();
        iconImage.sizeScale_X = 1f;
        iconImage.sizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(iconImage);
        nameAndPriceLabel = Glazier.Get().CreateLabel();
        nameAndPriceLabel.positionScale_Y = 1f;
        nameAndPriceLabel.positionOffset_Y = -50;
        nameAndPriceLabel.sizeScale_X = 1f;
        nameAndPriceLabel.sizeOffset_Y = 50;
        nameAndPriceLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        nameAndPriceLabel.fontAlignment = TextAnchor.LowerLeft;
        nameAndPriceLabel.enableRichText = true;
        AddChild(nameAndPriceLabel);
        cartImage = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
        cartImage.positionOffset_X = 5;
        cartImage.positionOffset_Y = 5;
        cartImage.sizeOffset_X = 20;
        cartImage.sizeOffset_Y = 20;
        cartImage.drawMethod = ESleekSpriteType.Regular;
        cartImage.color = ESleekTint.FOREGROUND;
        AddChild(cartImage);
        stampLabel = Glazier.Get().CreateLabel();
        stampLabel.sizeScale_X = 1f;
        stampLabel.sizeOffset_Y = 50;
        stampLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        stampLabel.fontAlignment = TextAnchor.UpperRight;
        stampLabel.textColor = Color.green;
        AddChild(stampLabel);
    }

    public void RefreshInCart()
    {
        cartImage.isVisible = button.isClickable && ItemStore.Get().GetQuantityInCart(listing.itemdefid) > 0;
    }

    public void SetListing(ItemStore.Listing listing)
    {
        button.isClickable = true;
        iconImage.isVisible = true;
        nameAndPriceLabel.isVisible = true;
        this.listing = listing;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        string inventoryName = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        string text = ItemStore.Get().FormatPrice(listing.currentPrice);
        nameAndPriceLabel.text = RichTextUtil.wrapWithColor(inventoryName, inventoryColor) + "\n" + RichTextUtil.wrapWithColor(text, ItemStore.PremiumColor);
        nameAndPriceLabel.textColor = inventoryColor;
        button.backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        button.textColor = inventoryColor;
        button.tooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        Texture2D texture2D = Provider.provider.economyService.LoadItemIcon(listing.itemdefid, large: false);
        iconImage.texture = texture2D;
        iconImage.isVisible = texture2D != null;
        if (listing.isNew && !ItemStoreSavedata.WasNewListingSeen(listing.itemdefid))
        {
            hasNewLabel = true;
            stampLabel.text = Provider.localization.format("New");
            stampLabel.isVisible = true;
        }
        else if (listing.currentPrice < listing.basePrice)
        {
            hasNewLabel = false;
            stampLabel.text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale") + "\n" + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
            stampLabel.isVisible = true;
        }
        else
        {
            hasNewLabel = false;
            stampLabel.isVisible = false;
        }
        RefreshInCart();
    }

    public void ClearListing()
    {
        button.isClickable = false;
        iconImage.isVisible = false;
        nameAndPriceLabel.isVisible = false;
        cartImage.isVisible = false;
        button.tooltipText = null;
        stampLabel.isVisible = false;
    }

    private void OnClickedButton(ISleekElement button)
    {
        if (hasNewLabel)
        {
            ItemStoreSavedata.MarkNewListingSeen(listing.itemdefid);
            stampLabel.isVisible = false;
        }
        ItemStoreDetailsMenu.instance.Open(listing);
        ItemStoreMenu.instance.Close();
    }
}
