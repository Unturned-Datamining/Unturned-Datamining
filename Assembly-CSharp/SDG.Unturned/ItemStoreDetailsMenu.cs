using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Examine a store listing with description text.
/// </summary>
internal class ItemStoreDetailsMenu : SleekFullscreenBox
{
    public static ItemStoreDetailsMenu instance;

    private ItemStore.Listing listing;

    private int quantityInCart;

    private ISleekLabel nameLabel;

    private ISleekLabel descriptionLabel;

    private SleekEconIcon iconImage;

    private SleekItemStorePriceBox priceBox;

    private ISleekButton addToCartButton;

    private ISleekButton removeFromCartButton;

    private ISleekInt32Field quantityField;

    private ISleekButton incrementQuantityButton;

    private ISleekButton decrementQuantityButton;

    /// <summary>
    /// Only visible when cart is not empty.
    /// </summary>
    private ISleekButton viewCartButton;

    public bool IsOpen { get; private set; }

    public void Open(ItemStore.Listing listing)
    {
        IsOpen = true;
        AnimateIntoView();
        this.listing = listing;
        quantityInCart = ItemStore.Get().GetQuantityInCart(listing.itemdefid);
        iconImage.SetItemDefId(listing.itemdefid);
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(listing.itemdefid);
        nameLabel.TextColor = inventoryColor;
        nameLabel.Text = Provider.provider.economyService.getInventoryName(listing.itemdefid);
        string inventoryType = Provider.provider.economyService.getInventoryType(listing.itemdefid);
        string inventoryDescription = Provider.provider.economyService.getInventoryDescription(listing.itemdefid);
        descriptionLabel.Text = RichTextUtil.wrapWithColor(inventoryType, inventoryColor) + "\n\n" + inventoryDescription;
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
        sleekBox.SizeScale_Y = 0.4f;
        sleekBox.SizeOffset_Y = -30f;
        sleekElement.AddChild(sleekBox);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.PositionOffset_X = 5f;
        sleekConstraintFrame.PositionOffset_Y = 5f;
        sleekConstraintFrame.SizeScale_X = 1f;
        sleekConstraintFrame.SizeScale_Y = 1f;
        sleekConstraintFrame.SizeOffset_Y = -70f;
        sleekConstraintFrame.Constraint = ESleekConstraint.FitInParent;
        sleekBox.AddChild(sleekConstraintFrame);
        iconImage = new SleekEconIcon();
        iconImage.SizeScale_X = 1f;
        iconImage.SizeScale_Y = 1f;
        sleekConstraintFrame.AddChild(iconImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionScale_Y = 1f;
        nameLabel.PositionOffset_Y = -70f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeOffset_Y = 70f;
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.FontSize = ESleekFontSize.Large;
        sleekBox.AddChild(nameLabel);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.PositionOffset_Y = -25f;
        sleekElement2.PositionScale_Y = 0.4f;
        sleekElement2.SizeOffset_Y = 50f;
        sleekElement2.SizeScale_X = 1f;
        sleekElement.AddChild(sleekElement2);
        priceBox = new SleekItemStorePriceBox();
        priceBox.PositionScale_X = 0.75f;
        priceBox.SizeScale_X = 0.25f;
        priceBox.SizeScale_Y = 1f;
        sleekElement2.AddChild(priceBox);
        addToCartButton = Glazier.Get().CreateButton();
        addToCartButton.SizeScale_X = 0.75f;
        addToCartButton.SizeScale_Y = 1f;
        addToCartButton.FontSize = ESleekFontSize.Medium;
        addToCartButton.Text = localization.format("AddToCart_Label");
        addToCartButton.TooltipText = localization.format("AddToCart_Tooltip");
        addToCartButton.OnClicked += OnClickedAddToCart;
        sleekElement2.AddChild(addToCartButton);
        removeFromCartButton = Glazier.Get().CreateButton();
        removeFromCartButton.SizeScale_X = 0.5f;
        removeFromCartButton.SizeScale_Y = 1f;
        removeFromCartButton.FontSize = ESleekFontSize.Medium;
        removeFromCartButton.Text = localization.format("RemoveFromCart_Label");
        removeFromCartButton.TooltipText = localization.format("RemoveFromCart_Tooltip");
        removeFromCartButton.OnClicked += OnClickedRemoveFromCart;
        sleekElement2.AddChild(removeFromCartButton);
        quantityField = Glazier.Get().CreateInt32Field();
        quantityField.PositionScale_X = 0.5f;
        quantityField.SizeScale_X = 0.25f;
        quantityField.SizeOffset_X = -25f;
        quantityField.SizeScale_Y = 1f;
        quantityField.OnValueChanged += OnTypedQuantity;
        sleekElement2.AddChild(quantityField);
        incrementQuantityButton = Glazier.Get().CreateButton();
        incrementQuantityButton.PositionScale_X = 0.75f;
        incrementQuantityButton.PositionOffset_X = -25f;
        incrementQuantityButton.SizeOffset_X = 25f;
        incrementQuantityButton.SizeOffset_Y = 25f;
        incrementQuantityButton.FontSize = ESleekFontSize.Medium;
        incrementQuantityButton.Text = "+";
        incrementQuantityButton.OnClicked += OnClickedIncrementQuantity;
        sleekElement2.AddChild(incrementQuantityButton);
        decrementQuantityButton = Glazier.Get().CreateButton();
        decrementQuantityButton.PositionScale_X = 0.75f;
        decrementQuantityButton.PositionOffset_X = -25f;
        decrementQuantityButton.PositionOffset_Y = 25f;
        decrementQuantityButton.SizeOffset_X = 25f;
        decrementQuantityButton.SizeOffset_Y = 25f;
        decrementQuantityButton.FontSize = ESleekFontSize.Medium;
        decrementQuantityButton.Text = "-";
        decrementQuantityButton.OnClicked += OnClickedDecrementQuantity;
        sleekElement2.AddChild(decrementQuantityButton);
        ISleekBox sleekBox2 = Glazier.Get().CreateBox();
        sleekBox2.PositionScale_Y = 0.4f;
        sleekBox2.PositionOffset_Y = 30f;
        sleekBox2.SizeOffset_Y = -30f;
        sleekBox2.SizeScale_X = 1f;
        sleekBox2.SizeScale_Y = 0.6f;
        sleekElement.AddChild(sleekBox2);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.PositionOffset_X = 5f;
        descriptionLabel.PositionOffset_Y = 5f;
        descriptionLabel.SizeScale_X = 1f;
        descriptionLabel.SizeScale_Y = 1f;
        descriptionLabel.SizeOffset_X = -10f;
        descriptionLabel.SizeOffset_Y = -10f;
        descriptionLabel.TextAlignment = TextAnchor.UpperLeft;
        descriptionLabel.AllowRichText = true;
        descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekBox2.AddChild(descriptionLabel);
        viewCartButton = Glazier.Get().CreateButton();
        viewCartButton.PositionOffset_Y = -110f;
        viewCartButton.PositionScale_Y = 1f;
        viewCartButton.SizeOffset_X = 200f;
        viewCartButton.SizeOffset_Y = 50f;
        viewCartButton.Text = ItemStoreMenu.instance.localization.format("ViewCart_Label");
        viewCartButton.TooltipText = ItemStoreMenu.instance.localization.format("ViewCart_Tooltip");
        viewCartButton.OnClicked += OnClickedViewCartButton;
        viewCartButton.FontSize = ESleekFontSize.Medium;
        AddChild(viewCartButton);
        ISleekSprite sleekSprite = Glazier.Get().CreateSprite(ItemStoreMenu.instance.icons.load<Sprite>("Cart"));
        sleekSprite.PositionOffset_X = 5f;
        sleekSprite.PositionOffset_Y = 5f;
        sleekSprite.SizeOffset_X = 40f;
        sleekSprite.SizeOffset_Y = 40f;
        sleekSprite.TintColor = ESleekTint.FOREGROUND;
        sleekSprite.DrawMethod = ESleekSpriteType.Regular;
        viewCartButton.AddChild(sleekSprite);
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

    private void RefreshQuantity()
    {
        priceBox.SetPrice(listing.basePrice, listing.currentPrice, quantityInCart);
        bool flag = quantityInCart > 0;
        addToCartButton.IsVisible = !flag;
        removeFromCartButton.IsVisible = flag;
        quantityField.Value = quantityInCart;
        quantityField.IsVisible = flag;
        incrementQuantityButton.IsVisible = flag;
        decrementQuantityButton.IsVisible = flag;
        viewCartButton.IsVisible = !ItemStore.Get().IsCartEmpty;
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
