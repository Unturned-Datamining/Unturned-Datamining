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
                button.isClickable = total >= amount;
                amountLabel.text = PlayerNPCVendorUI.localization.format("Amount_Buy", total, amount);
            }
            else if (element is VendorSellingBase vendorSellingBase)
            {
                vendorSellingBase.format(Player.player, out var total2);
                button.isClickable = vendorSellingBase.canBuy(Player.player);
                amountLabel.text = PlayerNPCVendorUI.localization.format("Amount_Sell", total2);
            }
            amountLabel.textColor = (button.isClickable ? ESleekTint.FONT : ESleekTint.BAD);
        }
    }

    public SleekVendor(VendorElement newElement)
    {
        element = newElement;
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += onClickedInternalButton;
        AddChild(button);
        int num = 0;
        base.sizeOffset_Y = 60;
        if (element.hasIcon && Assets.find(EAssetType.ITEM, element.id) is ItemAsset itemAsset)
        {
            SleekItemIcon sleekItemIcon = new SleekItemIcon
            {
                positionOffset_X = 5,
                positionOffset_Y = 5
            };
            if (itemAsset.size_y == 1)
            {
                sleekItemIcon.sizeOffset_X = itemAsset.size_x * 100;
                sleekItemIcon.sizeOffset_Y = itemAsset.size_y * 100;
            }
            else
            {
                sleekItemIcon.sizeOffset_X = itemAsset.size_x * 50;
                sleekItemIcon.sizeOffset_Y = itemAsset.size_y * 50;
            }
            num = sleekItemIcon.sizeOffset_X;
            AddChild(sleekItemIcon);
            sleekItemIcon.Refresh(element.id, 100, itemAsset.getState(isFull: false), itemAsset, sleekItemIcon.sizeOffset_X, sleekItemIcon.sizeOffset_Y);
            base.sizeOffset_Y = sleekItemIcon.sizeOffset_Y + 10;
        }
        string displayName = element.displayName;
        if (!string.IsNullOrEmpty(displayName))
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = num + 10;
            sleekLabel.positionOffset_Y = 5;
            sleekLabel.sizeOffset_X = -num - 15;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.sizeScale_X = 1f;
            sleekLabel.text = displayName;
            sleekLabel.fontSize = ESleekFontSize.Medium;
            sleekLabel.fontAlignment = TextAnchor.UpperLeft;
            sleekLabel.textColor = ItemTool.getRarityColorUI(element.rarity);
            sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            AddChild(sleekLabel);
        }
        string displayDesc = element.displayDesc;
        if (!string.IsNullOrEmpty(displayDesc))
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.positionOffset_X = num + 10;
            sleekLabel2.positionOffset_Y = 25;
            sleekLabel2.sizeOffset_X = -num - 15;
            sleekLabel2.sizeOffset_Y = -30;
            sleekLabel2.sizeScale_X = 1f;
            sleekLabel2.sizeScale_Y = 1f;
            sleekLabel2.fontAlignment = TextAnchor.UpperLeft;
            sleekLabel2.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekLabel2.enableRichText = true;
            sleekLabel2.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekLabel2.text = displayDesc;
            AddChild(sleekLabel2);
        }
        ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
        sleekLabel3.positionOffset_X = num + 10;
        sleekLabel3.positionOffset_Y = -35;
        sleekLabel3.positionScale_Y = 1f;
        sleekLabel3.sizeOffset_X = -num - 15;
        sleekLabel3.sizeOffset_Y = 30;
        sleekLabel3.sizeScale_X = 1f;
        sleekLabel3.fontAlignment = TextAnchor.LowerRight;
        AddChild(sleekLabel3);
        if (element is VendorBuying)
        {
            sleekLabel3.text = PlayerNPCVendorUI.localization.format("Price", formatCost(element.cost));
        }
        else
        {
            sleekLabel3.text = PlayerNPCVendorUI.localization.format("Cost", formatCost(element.cost));
        }
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.positionOffset_X = num + 10;
        amountLabel.positionOffset_Y = -35;
        amountLabel.positionScale_Y = 1f;
        amountLabel.sizeOffset_X = -num - 15;
        amountLabel.sizeOffset_Y = 30;
        amountLabel.sizeScale_X = 1f;
        amountLabel.fontAlignment = TextAnchor.LowerLeft;
        AddChild(amountLabel);
        updateAmount();
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }
}
