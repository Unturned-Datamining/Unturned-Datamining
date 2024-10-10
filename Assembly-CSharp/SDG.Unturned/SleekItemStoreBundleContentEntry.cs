using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStoreBundleContentEntry : SleekWrapper
{
    private int itemdefid;

    private ISleekButton itemButton;

    private SleekEconIcon iconImage;

    private ISleekLabel nameLabel;

    public SleekItemStoreBundleContentEntry(int itemdefid)
    {
        _ = ItemStoreMenu.instance.localization;
        this.itemdefid = itemdefid;
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(itemdefid);
        itemButton = Glazier.Get().CreateButton();
        itemButton.SizeScale_X = 1f;
        itemButton.SizeScale_Y = 1f;
        itemButton.OnClicked += OnClickedItemButton;
        itemButton.TooltipText = Provider.provider.economyService.getInventoryType(itemdefid);
        itemButton.TextColor = inventoryColor;
        AddChild(itemButton);
        iconImage = new SleekEconIcon();
        iconImage.PositionOffset_X = 5f;
        iconImage.PositionOffset_Y = 5f;
        iconImage.SizeOffset_X = 40f;
        iconImage.SizeOffset_Y = 40f;
        iconImage.SetItemDefId(itemdefid);
        itemButton.AddChild(iconImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 50f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeScale_Y = 1f;
        nameLabel.SizeOffset_X = -50f;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.FontSize = ESleekFontSize.Medium;
        nameLabel.Text = Provider.provider.economyService.getInventoryName(itemdefid);
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.TextColor = inventoryColor;
        itemButton.AddChild(nameLabel);
    }

    private void OnClickedItemButton(ISleekElement button)
    {
        MenuSurvivorsClothingInspectUI.viewItem(itemdefid, 0uL);
        MenuSurvivorsClothingInspectUI.open(EMenuSurvivorsClothingInspectUIOpenContext.ItemStoreBundleContents);
        ItemStoreBundleContentsMenu.instance.Close();
    }
}
