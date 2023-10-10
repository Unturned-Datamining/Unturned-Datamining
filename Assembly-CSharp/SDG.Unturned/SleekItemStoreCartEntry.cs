using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStoreCartEntry : SleekWrapper
{
    private ItemStore.CartEntry cartEntry;

    private ItemStore.Listing listing;

    private ISleekButton itemButton;

    private SleekEconIcon iconImage;

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
        itemButton.SizeScale_X = 0.4f;
        itemButton.SizeScale_Y = 1f;
        itemButton.OnClicked += OnClickedItemButton;
        itemButton.TooltipText = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        AddChild(itemButton);
        iconImage = new SleekEconIcon();
        iconImage.PositionOffset_X = 5f;
        iconImage.PositionOffset_Y = 5f;
        iconImage.SizeOffset_X = 40f;
        iconImage.SizeOffset_Y = 40f;
        iconImage.SetItemDefId(listing.itemdefid);
        itemButton.AddChild(iconImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 50f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeScale_Y = 1f;
        nameLabel.SizeOffset_X = -50f;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.FontSize = ESleekFontSize.Medium;
        nameLabel.Text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        itemButton.AddChild(nameLabel);
        addToCartButton = Glazier.Get().CreateButton();
        addToCartButton.PositionScale_X = 0.4f;
        addToCartButton.SizeScale_X = 0.6f;
        addToCartButton.SizeScale_Y = 1f;
        addToCartButton.Text = localization.format("AddToCart_Label");
        addToCartButton.TooltipText = localization.format("AddToCart_Tooltip");
        addToCartButton.OnClicked += OnClickedAddToCart;
        addToCartButton.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        AddChild(addToCartButton);
        removeFromCartButton = Glazier.Get().CreateButton();
        removeFromCartButton.PositionScale_X = 0.4f;
        removeFromCartButton.SizeScale_X = 0.2f;
        removeFromCartButton.SizeScale_Y = 1f;
        removeFromCartButton.Text = localization.format("RemoveFromCart_Label");
        removeFromCartButton.TooltipText = localization.format("RemoveFromCart_Tooltip");
        removeFromCartButton.OnClicked += OnClickedRemoveFromCart;
        AddChild(removeFromCartButton);
        quantityField = Glazier.Get().CreateInt32Field();
        quantityField.PositionScale_X = 0.6f;
        quantityField.SizeScale_X = 0.2f;
        quantityField.SizeOffset_X = -25f;
        quantityField.SizeScale_Y = 1f;
        quantityField.OnValueChanged += OnTypedQuantity;
        AddChild(quantityField);
        incrementQuantityButton = Glazier.Get().CreateButton();
        incrementQuantityButton.PositionScale_X = 0.8f;
        incrementQuantityButton.PositionOffset_X = -25f;
        incrementQuantityButton.SizeOffset_X = 25f;
        incrementQuantityButton.SizeOffset_Y = 25f;
        incrementQuantityButton.FontSize = ESleekFontSize.Medium;
        incrementQuantityButton.Text = "+";
        incrementQuantityButton.OnClicked += OnClickedIncrementQuantity;
        AddChild(incrementQuantityButton);
        decrementQuantityButton = Glazier.Get().CreateButton();
        decrementQuantityButton.PositionScale_X = 0.8f;
        decrementQuantityButton.PositionOffset_X = -25f;
        decrementQuantityButton.PositionOffset_Y = 25f;
        decrementQuantityButton.SizeOffset_X = 25f;
        decrementQuantityButton.SizeOffset_Y = 25f;
        decrementQuantityButton.FontSize = ESleekFontSize.Medium;
        decrementQuantityButton.Text = "-";
        decrementQuantityButton.OnClicked += OnClickedDecrementQuantity;
        AddChild(decrementQuantityButton);
        priceBox = new SleekItemStorePriceBox();
        priceBox.PositionScale_X = 0.8f;
        priceBox.SizeScale_X = 0.2f;
        priceBox.SizeOffset_Y = 50f;
        AddChild(priceBox);
        RefreshQuantity();
    }

    private void RefreshQuantity()
    {
        priceBox.SetPrice(listing.basePrice, listing.currentPrice, cartEntry.quantity);
        quantityField.Value = cartEntry.quantity;
        bool flag = cartEntry.quantity > 0;
        addToCartButton.IsVisible = !flag;
        removeFromCartButton.IsVisible = flag;
        quantityField.Value = cartEntry.quantity;
        quantityField.IsVisible = flag;
        incrementQuantityButton.IsVisible = flag;
        decrementQuantityButton.IsVisible = flag;
        priceBox.IsVisible = flag;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        itemButton.TextColor = inventoryColor;
        inventoryColor.a = (flag ? 1f : 0.5f);
        SleekColor backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        backgroundColor.SetAlpha(inventoryColor.a);
        itemButton.BackgroundColor = backgroundColor;
        iconImage.color = new SleekColor(ESleekTint.NONE, flag ? 1f : 0.5f);
        nameLabel.TextColor = inventoryColor;
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
