using UnityEngine;

namespace SDG.Unturned;

internal class ItemStoreDetailsMenu : SleekFullscreenBox
{
    public static ItemStoreDetailsMenu instance;

    private ItemStore.Listing listing;

    private int quantityInCart;

    private ISleekLabel nameLabel;

    private ISleekLabel descriptionLabel;

    private ISleekImage iconImage;

    private SleekItemStorePriceBox priceBox;

    private ISleekButton addToCartButton;

    private ISleekButton removeFromCartButton;

    private ISleekInt32Field quantityField;

    private ISleekButton incrementQuantityButton;

    private ISleekButton decrementQuantityButton;

    private ISleekButton viewCartButton;

    public bool IsOpen { get; private set; }

    public void Open(ItemStore.Listing listing)
    {
        IsOpen = true;
        AnimateIntoView();
        this.listing = listing;
        quantityInCart = ItemStore.Get().GetQuantityInCart(listing.itemdefid);
        iconImage.texture = Provider.provider.economyService.LoadItemIcon(listing.itemdefid, large: true);
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        nameLabel.textColor = inventoryColor;
        nameLabel.text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        string inventoryType = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        string inventoryDescription = Provider.provider.economyService.getInventoryDescription(listing.itemdefid);
        descriptionLabel.text = RichTextUtil.wrapWithColor(inventoryType, inventoryColor) + "\n\n" + inventoryDescription;
        RefreshQuantity();
    }

    public void Close()
    {
        if (IsOpen)
        {
            IsOpen = false;
            AnimateOutOfView(0f, 1f);
        }
    }

    public ItemStoreDetailsMenu()
    {
        Local localization = ItemStoreMenu.instance.localization;
        instance = this;
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.positionScale_X = 0.6f;
        sleekElement.positionOffset_Y = 10;
        sleekElement.sizeScale_X = 0.3f;
        sleekElement.sizeScale_Y = 1f;
        sleekElement.sizeOffset_Y = -20;
        AddChild(sleekElement);
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.sizeScale_X = 1f;
        sleekBox.sizeScale_Y = 0.4f;
        sleekBox.sizeOffset_Y = -30;
        sleekElement.AddChild(sleekBox);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.positionOffset_X = 5;
        sleekConstraintFrame.positionOffset_Y = 5;
        sleekConstraintFrame.sizeScale_X = 1f;
        sleekConstraintFrame.sizeScale_Y = 1f;
        sleekConstraintFrame.sizeOffset_Y = -70;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        sleekBox.AddChild(sleekConstraintFrame);
        iconImage = Glazier.Get().CreateImage();
        iconImage.sizeScale_X = 1f;
        iconImage.sizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(iconImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionScale_Y = 1f;
        nameLabel.positionOffset_Y = -70;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeOffset_Y = 70;
        nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.fontSize = ESleekFontSize.Large;
        sleekBox.AddChild(nameLabel);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.positionOffset_Y = -25;
        sleekElement2.positionScale_Y = 0.4f;
        sleekElement2.sizeOffset_Y = 50;
        sleekElement2.sizeScale_X = 1f;
        sleekElement.AddChild(sleekElement2);
        priceBox = new SleekItemStorePriceBox();
        priceBox.positionScale_X = 0.75f;
        priceBox.sizeScale_X = 0.25f;
        priceBox.sizeScale_Y = 1f;
        sleekElement2.AddChild(priceBox);
        addToCartButton = Glazier.Get().CreateButton();
        addToCartButton.sizeScale_X = 0.75f;
        addToCartButton.sizeScale_Y = 1f;
        addToCartButton.fontSize = ESleekFontSize.Medium;
        addToCartButton.text = localization.format("AddToCart_Label");
        addToCartButton.tooltipText = localization.format("AddToCart_Tooltip");
        addToCartButton.onClickedButton += OnClickedAddToCart;
        sleekElement2.AddChild(addToCartButton);
        removeFromCartButton = Glazier.Get().CreateButton();
        removeFromCartButton.sizeScale_X = 0.5f;
        removeFromCartButton.sizeScale_Y = 1f;
        removeFromCartButton.fontSize = ESleekFontSize.Medium;
        removeFromCartButton.text = localization.format("RemoveFromCart_Label");
        removeFromCartButton.tooltipText = localization.format("RemoveFromCart_Tooltip");
        removeFromCartButton.onClickedButton += OnClickedRemoveFromCart;
        sleekElement2.AddChild(removeFromCartButton);
        quantityField = Glazier.Get().CreateInt32Field();
        quantityField.positionScale_X = 0.5f;
        quantityField.sizeScale_X = 0.25f;
        quantityField.sizeOffset_X = -25;
        quantityField.sizeScale_Y = 1f;
        quantityField.onTypedInt += OnTypedQuantity;
        sleekElement2.AddChild(quantityField);
        incrementQuantityButton = Glazier.Get().CreateButton();
        incrementQuantityButton.positionScale_X = 0.75f;
        incrementQuantityButton.positionOffset_X = -25;
        incrementQuantityButton.sizeOffset_X = 25;
        incrementQuantityButton.sizeOffset_Y = 25;
        incrementQuantityButton.fontSize = ESleekFontSize.Medium;
        incrementQuantityButton.text = "+";
        incrementQuantityButton.onClickedButton += OnClickedIncrementQuantity;
        sleekElement2.AddChild(incrementQuantityButton);
        decrementQuantityButton = Glazier.Get().CreateButton();
        decrementQuantityButton.positionScale_X = 0.75f;
        decrementQuantityButton.positionOffset_X = -25;
        decrementQuantityButton.positionOffset_Y = 25;
        decrementQuantityButton.sizeOffset_X = 25;
        decrementQuantityButton.sizeOffset_Y = 25;
        decrementQuantityButton.fontSize = ESleekFontSize.Medium;
        decrementQuantityButton.text = "-";
        decrementQuantityButton.onClickedButton += OnClickedDecrementQuantity;
        sleekElement2.AddChild(decrementQuantityButton);
        ISleekBox sleekBox2 = Glazier.Get().CreateBox();
        sleekBox2.positionScale_Y = 0.4f;
        sleekBox2.positionOffset_Y = 30;
        sleekBox2.sizeOffset_Y = -30;
        sleekBox2.sizeScale_X = 1f;
        sleekBox2.sizeScale_Y = 0.6f;
        sleekElement.AddChild(sleekBox2);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.positionOffset_X = 5;
        descriptionLabel.positionOffset_Y = 5;
        descriptionLabel.sizeScale_X = 1f;
        descriptionLabel.sizeScale_Y = 1f;
        descriptionLabel.sizeOffset_X = -10;
        descriptionLabel.sizeOffset_Y = -10;
        descriptionLabel.fontAlignment = TextAnchor.UpperLeft;
        descriptionLabel.enableRichText = true;
        descriptionLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        descriptionLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        sleekBox2.AddChild(descriptionLabel);
        viewCartButton = Glazier.Get().CreateButton();
        viewCartButton.positionOffset_Y = -110;
        viewCartButton.positionScale_Y = 1f;
        viewCartButton.sizeOffset_X = 200;
        viewCartButton.sizeOffset_Y = 50;
        viewCartButton.text = ItemStoreMenu.instance.localization.format("ViewCart_Label");
        viewCartButton.tooltipText = ItemStoreMenu.instance.localization.format("ViewCart_Tooltip");
        viewCartButton.onClickedButton += OnClickedViewCartButton;
        viewCartButton.fontSize = ESleekFontSize.Medium;
        AddChild(viewCartButton);
        ISleekSprite sleekSprite = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
        sleekSprite.positionOffset_X = 5;
        sleekSprite.positionOffset_Y = 5;
        sleekSprite.sizeOffset_X = 40;
        sleekSprite.sizeOffset_Y = 40;
        sleekSprite.color = ESleekTint.FOREGROUND;
        sleekSprite.drawMethod = ESleekSpriteType.Regular;
        viewCartButton.AddChild(sleekSprite);
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

    private void RefreshQuantity()
    {
        priceBox.SetPrice(listing.basePrice, listing.currentPrice, quantityInCart);
        bool flag = quantityInCart > 0;
        addToCartButton.isVisible = !flag;
        removeFromCartButton.isVisible = flag;
        quantityField.state = quantityInCart;
        quantityField.isVisible = flag;
        incrementQuantityButton.isVisible = flag;
        decrementQuantityButton.isVisible = flag;
        viewCartButton.isVisible = !ItemStore.Get().IsCartEmpty;
    }

    private void SetQuantityInCart(int value)
    {
        quantityInCart = value;
        ItemStore.Get().SetQuantityInCart(listing.itemdefid, quantityInCart);
        RefreshQuantity();
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
        SetQuantityInCart(quantityInCart + 1);
    }

    private void OnClickedDecrementQuantity(ISleekElement button)
    {
        SetQuantityInCart(quantityInCart - 1);
    }

    private void OnClickedViewCartButton(ISleekElement button)
    {
        ItemStoreCartMenu.instance.Open();
        Close();
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        ItemStoreMenu.instance.Open();
        Close();
    }
}
