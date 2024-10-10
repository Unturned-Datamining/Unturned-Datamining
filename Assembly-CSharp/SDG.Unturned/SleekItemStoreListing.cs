using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStoreListing : SleekWrapper
{
    public bool canShowAsInCart = true;

    private ItemStore.Listing listing;

    private bool hasNewLabel;

    private ISleekButton button;

    private SleekEconIcon iconImage;

    private ISleekLabel nameAndPriceLabel;

    /// <summary>
    /// Icon visible when this listing is in the cart.
    /// </summary>
    private ISleekSprite cartImage;

    /// <summary>
    /// "SALE" or "NEW" text visible when applicable.
    /// </summary>
    private ISleekLabel stampLabel;

    public SleekItemStoreListing()
    {
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += OnClickedButton;
        AddChild(button);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.PositionOffset_X = 5f;
        sleekConstraintFrame.PositionOffset_Y = 5f;
        sleekConstraintFrame.SizeScale_X = 1f;
        sleekConstraintFrame.SizeScale_Y = 1f;
        sleekConstraintFrame.SizeOffset_X = -10f;
        sleekConstraintFrame.SizeOffset_Y = -50f;
        sleekConstraintFrame.Constraint = ESleekConstraint.FitInParent;
        AddChild(sleekConstraintFrame);
        iconImage = new SleekEconIcon();
        iconImage.SizeScale_X = 1f;
        iconImage.SizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(iconImage);
        nameAndPriceLabel = Glazier.Get().CreateLabel();
        nameAndPriceLabel.PositionScale_Y = 1f;
        nameAndPriceLabel.PositionOffset_Y = -50f;
        nameAndPriceLabel.SizeScale_X = 1f;
        nameAndPriceLabel.SizeOffset_Y = 50f;
        nameAndPriceLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameAndPriceLabel.TextAlignment = TextAnchor.LowerLeft;
        nameAndPriceLabel.AllowRichText = true;
        AddChild(nameAndPriceLabel);
        cartImage = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
        cartImage.PositionOffset_X = 5f;
        cartImage.PositionOffset_Y = 5f;
        cartImage.SizeOffset_X = 20f;
        cartImage.SizeOffset_Y = 20f;
        cartImage.DrawMethod = ESleekSpriteType.Regular;
        cartImage.TintColor = ESleekTint.FOREGROUND;
        AddChild(cartImage);
        stampLabel = Glazier.Get().CreateLabel();
        stampLabel.SizeScale_X = 1f;
        stampLabel.SizeOffset_Y = 50f;
        stampLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        stampLabel.TextAlignment = TextAnchor.UpperRight;
        stampLabel.TextColor = Color.green;
        AddChild(stampLabel);
    }

    public void RefreshInCart()
    {
        cartImage.IsVisible = canShowAsInCart && button.IsClickable && ItemStore.Get().GetQuantityInCart(listing.itemdefid) > 0;
    }

    public void SetListing(ItemStore.Listing listing)
    {
        button.IsClickable = true;
        iconImage.IsVisible = true;
        nameAndPriceLabel.IsVisible = true;
        this.listing = listing;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        string inventoryName = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        string text = ItemStore.Get().FormatPrice(listing.currentPrice);
        nameAndPriceLabel.Text = RichTextUtil.wrapWithColor(inventoryName, inventoryColor) + "\n" + RichTextUtil.wrapWithColor(text, ItemStore.PremiumColor);
        nameAndPriceLabel.TextColor = inventoryColor;
        button.BackgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        button.TextColor = inventoryColor;
        button.TooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        iconImage.SetItemDefId(listing.itemdefid);
        if (listing.isNew && !ItemStoreSavedata.WasNewListingSeen(listing.itemdefid))
        {
            hasNewLabel = true;
            stampLabel.Text = Provider.localization.format("New");
            stampLabel.IsVisible = true;
        }
        else if (listing.currentPrice < listing.basePrice)
        {
            hasNewLabel = false;
            stampLabel.Text = MenuSurvivorsClothingUI.localization.format("Itemstore_Sale") + "\n" + ItemStore.Get().FormatDiscount(listing.currentPrice, listing.basePrice);
            stampLabel.IsVisible = true;
        }
        else
        {
            hasNewLabel = false;
            stampLabel.IsVisible = false;
        }
        RefreshInCart();
    }

    public void ClearListing()
    {
        button.IsClickable = false;
        iconImage.IsVisible = false;
        nameAndPriceLabel.IsVisible = false;
        cartImage.IsVisible = false;
        button.TooltipText = null;
        stampLabel.IsVisible = false;
    }

    private void OnClickedButton(ISleekElement button)
    {
        if (hasNewLabel)
        {
            ItemStoreSavedata.MarkNewListingSeen(listing.itemdefid);
            stampLabel.IsVisible = false;
        }
        ItemStore.Get().ViewItem(listing.itemdefid);
    }
}
