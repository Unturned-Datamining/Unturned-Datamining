using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStoreCartEntry : SleekWrapper
{
    private ItemStore.CartEntry cartEntry;

    private ItemStore.Listing listing;

    private ISleekButton itemButton;

    private ISleekImage iconImage;

    private ISleekLabel nameLabel;

    private SleekItemStorePriceBox priceBox;

    private ISleekButton addToCartButton;

    private ISleekButton removeFromCartButton;

    private ISleekInt32Field quantityField;

    private ISleekButton incrementQuantityButton;

    private ISleekButton decrementQuantityButton;

    public void GetTotalPrice(out ulong basePrice, out ulong currentPrice)
    {
        basePrice = listing.basePrice * (uint)cartEntry.quantity;
        currentPrice = listing.currentPrice * (uint)cartEntry.quantity;
    }

    public SleekItemStoreCartEntry(ItemStore.CartEntry cartEntry, ItemStore.Listing listing)
    {
        Local localization = ItemStoreMenu.instance.localization;
        this.cartEntry = cartEntry;
        this.listing = listing;
        itemButton = Glazier.Get().CreateButton();
        itemButton.sizeScale_X = 0.4f;
        itemButton.sizeScale_Y = 1f;
        itemButton.onClickedButton += OnClickedItemButton;
        itemButton.tooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        AddChild(itemButton);
        Texture2D texture = Provider.provider.economyService.LoadItemIcon(listing.itemdefid, large: false);
        iconImage = Glazier.Get().CreateImage(texture);
        iconImage.positionOffset_X = 5;
        iconImage.positionOffset_Y = 5;
        iconImage.sizeOffset_X = 40;
        iconImage.sizeOffset_Y = 40;
        itemButton.AddChild(iconImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 50;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeScale_Y = 1f;
        nameLabel.sizeOffset_X = -50;
        nameLabel.fontAlignment = TextAnchor.MiddleLeft;
        nameLabel.fontSize = ESleekFontSize.Medium;
        nameLabel.text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        itemButton.AddChild(nameLabel);
        addToCartButton = Glazier.Get().CreateButton();
        addToCartButton.positionScale_X = 0.4f;
        addToCartButton.sizeScale_X = 0.6f;
        addToCartButton.sizeScale_Y = 1f;
        addToCartButton.text = localization.format("AddToCart_Label");
        addToCartButton.tooltipText = localization.format("AddToCart_Tooltip");
        addToCartButton.onClickedButton += OnClickedAddToCart;
        addToCartButton.backgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        AddChild(addToCartButton);
        removeFromCartButton = Glazier.Get().CreateButton();
        removeFromCartButton.positionScale_X = 0.4f;
        removeFromCartButton.sizeScale_X = 0.2f;
        removeFromCartButton.sizeScale_Y = 1f;
        removeFromCartButton.text = localization.format("RemoveFromCart_Label");
        removeFromCartButton.tooltipText = localization.format("RemoveFromCart_Tooltip");
        removeFromCartButton.onClickedButton += OnClickedRemoveFromCart;
        AddChild(removeFromCartButton);
        quantityField = Glazier.Get().CreateInt32Field();
        quantityField.positionScale_X = 0.6f;
        quantityField.sizeScale_X = 0.2f;
        quantityField.sizeOffset_X = -25;
        quantityField.sizeScale_Y = 1f;
        quantityField.onTypedInt += OnTypedQuantity;
        AddChild(quantityField);
        incrementQuantityButton = Glazier.Get().CreateButton();
        incrementQuantityButton.positionScale_X = 0.8f;
        incrementQuantityButton.positionOffset_X = -25;
        incrementQuantityButton.sizeOffset_X = 25;
        incrementQuantityButton.sizeOffset_Y = 25;
        incrementQuantityButton.fontSize = ESleekFontSize.Medium;
        incrementQuantityButton.text = "+";
        incrementQuantityButton.onClickedButton += OnClickedIncrementQuantity;
        AddChild(incrementQuantityButton);
        decrementQuantityButton = Glazier.Get().CreateButton();
        decrementQuantityButton.positionScale_X = 0.8f;
        decrementQuantityButton.positionOffset_X = -25;
        decrementQuantityButton.positionOffset_Y = 25;
        decrementQuantityButton.sizeOffset_X = 25;
        decrementQuantityButton.sizeOffset_Y = 25;
        decrementQuantityButton.fontSize = ESleekFontSize.Medium;
        decrementQuantityButton.text = "-";
        decrementQuantityButton.onClickedButton += OnClickedDecrementQuantity;
        AddChild(decrementQuantityButton);
        priceBox = new SleekItemStorePriceBox();
        priceBox.positionScale_X = 0.8f;
        priceBox.sizeScale_X = 0.2f;
        priceBox.sizeOffset_Y = 50;
        AddChild(priceBox);
        RefreshQuantity();
    }

    private void RefreshQuantity()
    {
        priceBox.SetPrice(listing.basePrice, listing.currentPrice, cartEntry.quantity);
        quantityField.state = cartEntry.quantity;
        bool flag = cartEntry.quantity > 0;
        addToCartButton.isVisible = !flag;
        removeFromCartButton.isVisible = flag;
        quantityField.state = cartEntry.quantity;
        quantityField.isVisible = flag;
        incrementQuantityButton.isVisible = flag;
        decrementQuantityButton.isVisible = flag;
        priceBox.isVisible = flag;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        itemButton.textColor = inventoryColor;
        inventoryColor.a = (flag ? 1f : 0.5f);
        SleekColor backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        backgroundColor.SetAlpha(inventoryColor.a);
        itemButton.backgroundColor = backgroundColor;
        iconImage.color = new SleekColor(ESleekTint.NONE, flag ? 1f : 0.5f);
        nameLabel.textColor = inventoryColor;
    }

    private void SetQuantityInCart(int value)
    {
        cartEntry.quantity = value;
        ItemStore.Get().SetQuantityInCart(cartEntry.itemdefid, value);
        RefreshQuantity();
    }

    private void OnClickedItemButton(ISleekElement button)
    {
        ItemStoreDetailsMenu.instance.Open(listing);
        ItemStoreCartMenu.instance.Close();
    }

    private void OnClickedAddToCart(ISleekElement button)
    {
        SetQuantityInCart(1);
    }

    private void OnClickedRemoveFromCart(ISleekElement button)
    {
        SetQuantityInCart(0);
    }

    private void OnTypedQuantity(ISleekInt32Field field, int value)
    {
        SetQuantityInCart(Mathf.Max(0, value));
    }

    private void OnClickedIncrementQuantity(ISleekElement button)
    {
        SetQuantityInCart(cartEntry.quantity + 1);
    }

    private void OnClickedDecrementQuantity(ISleekElement button)
    {
        SetQuantityInCart(cartEntry.quantity - 1);
    }
}
