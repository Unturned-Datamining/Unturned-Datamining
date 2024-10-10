using UnityEngine;

namespace SDG.Unturned;

public class SleekVendor : SleekWrapper
{
    private VendorElement element;

    private ISleekButton button;

    private ISleekLabel amountLabel;

    public event ClickedButton onClickedButton;

    protected string formatCost(uint value)
    {
        if (element.outerAsset.currency.isValid)
        {
            ItemCurrencyAsset itemCurrencyAsset = element.outerAsset.currency.Find();
            if (itemCurrencyAsset != null && !string.IsNullOrEmpty(itemCurrencyAsset.valueFormat))
            {
                return string.Format(itemCurrencyAsset.valueFormat, value);
            }
        }
        return value.ToString();
    }

    public void updateAmount()
    {
        if (element != null && amountLabel != null)
        {
            if (element is VendorBuying vendorBuying)
            {
                vendorBuying.format(Player.player, out var total, out var amount);
                button.IsClickable = total >= amount;
                amountLabel.Text = PlayerNPCVendorUI.localization.format("Amount_Buy", total, amount);
            }
            else if (element is VendorSellingBase vendorSellingBase)
            {
                vendorSellingBase.format(Player.player, out var total2);
                button.IsClickable = vendorSellingBase.canBuy(Player.player);
                amountLabel.Text = PlayerNPCVendorUI.localization.format("Amount_Sell", total2);
            }
            amountLabel.TextColor = (button.IsClickable ? ESleekTint.FONT : ESleekTint.BAD);
        }
    }

    public SleekVendor(VendorElement newElement)
    {
        element = newElement;
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += onClickedInternalButton;
        AddChild(button);
        float num = 0f;
        base.SizeOffset_Y = 60f;
        if (element.hasIcon)
        {
            ItemAsset itemAsset = Assets.FindItemByGuidOrLegacyId<ItemAsset>(element.TargetAssetGuid, element.id);
            if (itemAsset != null)
            {
                SleekItemIcon sleekItemIcon = new SleekItemIcon
                {
                    PositionOffset_X = 5f,
                    PositionOffset_Y = 5f
                };
                if (itemAsset.size_y == 1)
                {
                    sleekItemIcon.SizeOffset_X = itemAsset.size_x * 100;
                    sleekItemIcon.SizeOffset_Y = itemAsset.size_y * 100;
                }
                else
                {
                    sleekItemIcon.SizeOffset_X = itemAsset.size_x * 50;
                    sleekItemIcon.SizeOffset_Y = itemAsset.size_y * 50;
                }
                num = sleekItemIcon.PositionOffset_X + sleekItemIcon.SizeOffset_X;
                AddChild(sleekItemIcon);
                byte[] array = null;
                if (itemAsset is ItemGunAsset gunAsset && element is VendorSellingItem vendorSellingItem)
                {
                    array = vendorSellingItem.GetGunStateOverride(gunAsset);
                }
                if (array == null)
                {
                    array = itemAsset.getState(isFull: false);
                }
                sleekItemIcon.Refresh(itemAsset.id, 100, array, itemAsset, Mathf.RoundToInt(sleekItemIcon.SizeOffset_X), Mathf.RoundToInt(sleekItemIcon.SizeOffset_Y));
                base.SizeOffset_Y = sleekItemIcon.SizeOffset_Y + 10f;
            }
        }
        else if (element is VendorSellingVehicle { paintColor: var color } vendorSellingVehicle)
        {
            if (!color.HasValue && vendorSellingVehicle.FindAsset() is VehicleRedirectorAsset vehicleRedirectorAsset)
            {
                color = vehicleRedirectorAsset.SpawnPaintColor;
            }
            if (color.HasValue)
            {
                ISleekImage sleekImage = Glazier.Get().CreateImage();
                sleekImage.PositionOffset_X = 10f;
                sleekImage.PositionOffset_Y = 10f;
                sleekImage.SizeOffset_X = 20f;
                sleekImage.SizeOffset_Y = 40f;
                sleekImage.Texture = (Texture2D)GlazierResources.PixelTexture;
                sleekImage.TintColor = color.Value;
                AddChild(sleekImage);
                num = sleekImage.PositionOffset_X + sleekImage.SizeOffset_X;
            }
        }
        string displayName = element.displayName;
        if (!string.IsNullOrEmpty(displayName))
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_X = num + 5f;
            sleekLabel.PositionOffset_Y = 5f;
            sleekLabel.SizeOffset_X = 0f - num - 10f;
            sleekLabel.SizeOffset_Y = 30f;
            sleekLabel.SizeScale_X = 1f;
            sleekLabel.Text = displayName;
            sleekLabel.FontSize = ESleekFontSize.Medium;
            sleekLabel.TextAlignment = TextAnchor.UpperLeft;
            sleekLabel.TextColor = ItemTool.getRarityColorUI(element.rarity);
            sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            AddChild(sleekLabel);
        }
        string displayDesc = element.displayDesc;
        if (!string.IsNullOrEmpty(displayDesc))
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.PositionOffset_X = num + 5f;
            sleekLabel2.PositionOffset_Y = 25f;
            sleekLabel2.SizeOffset_X = 0f - num - 10f;
            sleekLabel2.SizeOffset_Y = -30f;
            sleekLabel2.SizeScale_X = 1f;
            sleekLabel2.SizeScale_Y = 1f;
            sleekLabel2.TextAlignment = TextAnchor.UpperLeft;
            sleekLabel2.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekLabel2.AllowRichText = true;
            sleekLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekLabel2.Text = displayDesc;
            AddChild(sleekLabel2);
        }
        ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
        sleekLabel3.PositionOffset_X = num + 5f;
        sleekLabel3.PositionOffset_Y = -35f;
        sleekLabel3.PositionScale_Y = 1f;
        sleekLabel3.SizeOffset_X = 0f - num - 10f;
        sleekLabel3.SizeOffset_Y = 30f;
        sleekLabel3.SizeScale_X = 1f;
        sleekLabel3.TextAlignment = TextAnchor.LowerRight;
        AddChild(sleekLabel3);
        if (element is VendorBuying)
        {
            sleekLabel3.Text = PlayerNPCVendorUI.localization.format("Price", formatCost(element.cost));
        }
        else
        {
            sleekLabel3.Text = PlayerNPCVendorUI.localization.format("Cost", formatCost(element.cost));
        }
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.PositionOffset_X = num + 5f;
        amountLabel.PositionOffset_Y = -35f;
        amountLabel.PositionScale_Y = 1f;
        amountLabel.SizeOffset_X = 0f - num - 10f;
        amountLabel.SizeOffset_Y = 30f;
        amountLabel.SizeScale_X = 1f;
        amountLabel.TextAlignment = TextAnchor.LowerLeft;
        AddChild(amountLabel);
        updateAmount();
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }
}
