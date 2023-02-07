using UnityEngine;

namespace SDG.Unturned;

public class SleekItem : SleekWrapper
{
    private ItemJar _jar;

    private byte _hotkey = byte.MaxValue;

    private ISleekButton button;

    private SleekItemIcon icon;

    private ISleekLabel amountLabel;

    private ISleekImage qualityImage;

    private ISleekLabel hotkeyLabel;

    public ClickedItem onClickedItem;

    public DraggedItem onDraggedItem;

    private bool isTemporary;

    public ItemJar jar => _jar;

    public int hotkey => _hotkey;

    public void enable()
    {
        button.isRaycastTarget = true;
        SleekColor backgroundColor = button.backgroundColor;
        backgroundColor.SetAlpha(1f);
        button.backgroundColor = backgroundColor;
        SleekColor color = icon.color;
        color.SetAlpha(1f);
        icon.color = color;
    }

    public void disable()
    {
        button.isRaycastTarget = false;
        SleekColor backgroundColor = button.backgroundColor;
        backgroundColor.SetAlpha(0.5f);
        button.backgroundColor = backgroundColor;
        SleekColor color = icon.color;
        color.SetAlpha(0.5f);
        icon.color = color;
    }

    public void setEnabled(bool enabled)
    {
        if (enabled)
        {
            enable();
        }
        else
        {
            disable();
        }
    }

    public void SetIsDragItem()
    {
        button.isRaycastTarget = false;
    }

    public void updateHotkey(byte index)
    {
        _hotkey = index;
        if (hotkey == 255)
        {
            hotkeyLabel.text = "";
            hotkeyLabel.isVisible = false;
        }
        else
        {
            hotkeyLabel.text = ControlsSettings.getEquipmentHotkeyText(hotkey);
            hotkeyLabel.isVisible = true;
        }
    }

    public void updateItem(ItemJar newJar)
    {
        if (_jar != null && _jar.item != null && _jar.item.id != newJar.item.id)
        {
            icon.Clear();
        }
        _jar = newJar;
        ItemAsset asset = jar.GetAsset();
        if (asset == null)
        {
            return;
        }
        if (!isTemporary)
        {
            button.tooltipText = asset.itemName;
        }
        if ((int)jar.rot % 2 == 0)
        {
            base.sizeOffset_X = asset.size_x * 50;
            base.sizeOffset_Y = asset.size_y * 50;
            icon.positionOffset_X = 0;
            icon.positionOffset_Y = 0;
        }
        else
        {
            base.sizeOffset_X = asset.size_y * 50;
            base.sizeOffset_Y = asset.size_x * 50;
            int num = Mathf.Abs(asset.size_y - asset.size_x);
            if (asset.size_x > asset.size_y)
            {
                icon.positionOffset_X = -num * 25;
                icon.positionOffset_Y = num * 25;
            }
            else
            {
                icon.positionOffset_X = num * 25;
                icon.positionOffset_Y = -num * 25;
            }
        }
        icon.rot = jar.rot;
        icon.sizeOffset_X = asset.size_x * 50;
        icon.sizeOffset_Y = asset.size_y * 50;
        icon.Refresh(jar.item.id, jar.item.quality, jar.item.state, asset);
        if (asset.size_x == 1 || asset.size_y == 1)
        {
            amountLabel.positionOffset_X = 0;
            amountLabel.positionOffset_Y = -30;
            amountLabel.sizeOffset_X = 0;
            amountLabel.fontSize = ESleekFontSize.Small;
            hotkeyLabel.fontSize = ESleekFontSize.Small;
        }
        else
        {
            amountLabel.positionOffset_X = 5;
            amountLabel.positionOffset_Y = -35;
            amountLabel.sizeOffset_X = -10;
            amountLabel.fontSize = ESleekFontSize.Default;
            hotkeyLabel.fontSize = ESleekFontSize.Default;
        }
        Color rarityColorUI = ItemTool.getRarityColorUI(asset.rarity);
        button.backgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
        button.textColor = rarityColorUI;
        if (asset.showQuality)
        {
            if (asset.size_x == 1 || asset.size_y == 1)
            {
                qualityImage.positionOffset_X = -15;
                qualityImage.positionOffset_Y = -15;
                qualityImage.sizeOffset_X = 10;
                qualityImage.sizeOffset_Y = 10;
                qualityImage.texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_1");
            }
            else
            {
                qualityImage.positionOffset_X = -30;
                qualityImage.positionOffset_Y = -30;
                qualityImage.sizeOffset_X = 20;
                qualityImage.sizeOffset_Y = 20;
                qualityImage.texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0");
            }
            qualityImage.color = ItemTool.getQualityColor((float)(int)jar.item.quality / 100f);
            amountLabel.text = jar.item.quality + "%";
            amountLabel.textColor = qualityImage.color;
            qualityImage.isVisible = true;
            amountLabel.isVisible = true;
        }
        else
        {
            qualityImage.isVisible = false;
            if (asset.amount > 1)
            {
                amountLabel.text = "x" + jar.item.amount;
                amountLabel.textColor = ESleekTint.FONT;
                amountLabel.isVisible = true;
            }
            else
            {
                amountLabel.isVisible = false;
            }
        }
    }

    private void onClickedButton(ISleekElement button)
    {
        onDraggedItem?.Invoke(this);
    }

    private void onRightClickedButton(ISleekElement button)
    {
        onClickedItem?.Invoke(this);
    }

    public SleekItem(ItemJar jar)
    {
        button = Glazier.Get().CreateButton();
        button.positionOffset_X = 1;
        button.positionOffset_Y = 1;
        button.sizeOffset_X = -2;
        button.sizeOffset_Y = -2;
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += onClickedButton;
        button.onRightClickedButton += onRightClickedButton;
        AddChild(button);
        icon = new SleekItemIcon();
        AddChild(icon);
        icon.isAngled = true;
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.positionScale_Y = 1f;
        amountLabel.sizeOffset_Y = 30;
        amountLabel.sizeScale_X = 1f;
        amountLabel.fontAlignment = TextAnchor.LowerLeft;
        amountLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(amountLabel);
        amountLabel.isVisible = false;
        qualityImage = Glazier.Get().CreateImage();
        qualityImage.positionScale_X = 1f;
        qualityImage.positionScale_Y = 1f;
        AddChild(qualityImage);
        qualityImage.isVisible = false;
        hotkeyLabel = Glazier.Get().CreateLabel();
        hotkeyLabel.positionOffset_X = 5;
        hotkeyLabel.positionOffset_Y = 5;
        hotkeyLabel.sizeOffset_X = -10;
        hotkeyLabel.sizeOffset_Y = 30;
        hotkeyLabel.sizeScale_X = 1f;
        hotkeyLabel.fontAlignment = TextAnchor.UpperRight;
        hotkeyLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(hotkeyLabel);
        hotkeyLabel.isVisible = false;
        updateItem(jar);
    }

    public SleekItem()
    {
        button = Glazier.Get().CreateButton();
        button.positionOffset_X = 1;
        button.positionOffset_Y = 1;
        button.sizeOffset_X = -2;
        button.sizeOffset_Y = -2;
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        AddChild(button);
        icon = new SleekItemIcon();
        AddChild(icon);
        icon.isAngled = true;
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.positionScale_Y = 1f;
        amountLabel.sizeOffset_Y = 30;
        amountLabel.sizeScale_X = 1f;
        amountLabel.fontAlignment = TextAnchor.LowerLeft;
        amountLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(amountLabel);
        amountLabel.isVisible = false;
        qualityImage = Glazier.Get().CreateImage();
        qualityImage.positionScale_X = 1f;
        qualityImage.positionScale_Y = 1f;
        AddChild(qualityImage);
        qualityImage.isVisible = false;
        hotkeyLabel = Glazier.Get().CreateLabel();
        hotkeyLabel.positionOffset_X = 5;
        hotkeyLabel.positionOffset_Y = 5;
        hotkeyLabel.sizeOffset_X = -10;
        hotkeyLabel.sizeOffset_Y = 30;
        hotkeyLabel.sizeScale_X = 1f;
        hotkeyLabel.fontAlignment = TextAnchor.UpperRight;
        hotkeyLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(hotkeyLabel);
        hotkeyLabel.isVisible = false;
        isTemporary = true;
    }
}
