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
        button.IsRaycastTarget = true;
        SleekColor backgroundColor = button.BackgroundColor;
        backgroundColor.SetAlpha(1f);
        button.BackgroundColor = backgroundColor;
        SleekColor color = icon.color;
        color.SetAlpha(1f);
        icon.color = color;
    }

    public void disable()
    {
        button.IsRaycastTarget = false;
        SleekColor backgroundColor = button.BackgroundColor;
        backgroundColor.SetAlpha(0.5f);
        button.BackgroundColor = backgroundColor;
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
        button.IsRaycastTarget = false;
    }

    public void updateHotkey(byte index)
    {
        _hotkey = index;
        if (hotkey == 255)
        {
            hotkeyLabel.Text = "";
            hotkeyLabel.IsVisible = false;
        }
        else
        {
            hotkeyLabel.Text = ControlsSettings.getEquipmentHotkeyText(hotkey);
            hotkeyLabel.IsVisible = true;
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
            button.TooltipText = asset.itemName;
        }
        if (jar.rot % 2 == 0)
        {
            base.SizeOffset_X = asset.size_x * 50;
            base.SizeOffset_Y = asset.size_y * 50;
            icon.PositionOffset_X = 0f;
            icon.PositionOffset_Y = 0f;
        }
        else
        {
            base.SizeOffset_X = asset.size_y * 50;
            base.SizeOffset_Y = asset.size_x * 50;
            int num = Mathf.Abs(asset.size_y - asset.size_x);
            if (asset.size_x > asset.size_y)
            {
                icon.PositionOffset_X = -num * 25;
                icon.PositionOffset_Y = num * 25;
            }
            else
            {
                icon.PositionOffset_X = num * 25;
                icon.PositionOffset_Y = -num * 25;
            }
        }
        icon.rot = jar.rot;
        icon.SizeOffset_X = asset.size_x * 50;
        icon.SizeOffset_Y = asset.size_y * 50;
        icon.Refresh(jar.item.id, jar.item.quality, jar.item.state, asset);
        if (asset.size_x == 1 || asset.size_y == 1)
        {
            amountLabel.PositionOffset_X = 0f;
            amountLabel.PositionOffset_Y = -30f;
            amountLabel.SizeOffset_X = 0f;
            amountLabel.FontSize = ESleekFontSize.Small;
            hotkeyLabel.FontSize = ESleekFontSize.Small;
        }
        else
        {
            amountLabel.PositionOffset_X = 5f;
            amountLabel.PositionOffset_Y = -35f;
            amountLabel.SizeOffset_X = -10f;
            amountLabel.FontSize = ESleekFontSize.Default;
            hotkeyLabel.FontSize = ESleekFontSize.Default;
        }
        Color rarityColorUI = ItemTool.getRarityColorUI(asset.rarity);
        button.BackgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
        button.TextColor = rarityColorUI;
        if (asset.showQuality)
        {
            if (asset.size_x == 1 || asset.size_y == 1)
            {
                qualityImage.PositionOffset_X = -15f;
                qualityImage.PositionOffset_Y = -15f;
                qualityImage.SizeOffset_X = 10f;
                qualityImage.SizeOffset_Y = 10f;
                qualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_1");
            }
            else
            {
                qualityImage.PositionOffset_X = -30f;
                qualityImage.PositionOffset_Y = -30f;
                qualityImage.SizeOffset_X = 20f;
                qualityImage.SizeOffset_Y = 20f;
                qualityImage.Texture = PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0");
            }
            qualityImage.TintColor = ItemTool.getQualityColor((float)(int)jar.item.quality / 100f);
            amountLabel.Text = jar.item.quality + "%";
            amountLabel.TextColor = qualityImage.TintColor;
            qualityImage.IsVisible = true;
            amountLabel.IsVisible = true;
        }
        else
        {
            qualityImage.IsVisible = false;
            if (asset.amount > 1)
            {
                amountLabel.Text = "x" + jar.item.amount;
                amountLabel.TextColor = ESleekTint.FONT;
                amountLabel.IsVisible = true;
            }
            else
            {
                amountLabel.IsVisible = false;
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
        button.PositionOffset_X = 1f;
        button.PositionOffset_Y = 1f;
        button.SizeOffset_X = -2f;
        button.SizeOffset_Y = -2f;
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += onClickedButton;
        button.OnRightClicked += onRightClickedButton;
        AddChild(button);
        icon = new SleekItemIcon();
        AddChild(icon);
        icon.isAngled = true;
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.PositionScale_Y = 1f;
        amountLabel.SizeOffset_Y = 30f;
        amountLabel.SizeScale_X = 1f;
        amountLabel.TextAlignment = TextAnchor.LowerLeft;
        amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(amountLabel);
        amountLabel.IsVisible = false;
        qualityImage = Glazier.Get().CreateImage();
        qualityImage.PositionScale_X = 1f;
        qualityImage.PositionScale_Y = 1f;
        AddChild(qualityImage);
        qualityImage.IsVisible = false;
        hotkeyLabel = Glazier.Get().CreateLabel();
        hotkeyLabel.PositionOffset_X = 5f;
        hotkeyLabel.PositionOffset_Y = 5f;
        hotkeyLabel.SizeOffset_X = -10f;
        hotkeyLabel.SizeOffset_Y = 30f;
        hotkeyLabel.SizeScale_X = 1f;
        hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
        hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(hotkeyLabel);
        hotkeyLabel.IsVisible = false;
        updateItem(jar);
    }

    public SleekItem()
    {
        button = Glazier.Get().CreateButton();
        button.PositionOffset_X = 1f;
        button.PositionOffset_Y = 1f;
        button.SizeOffset_X = -2f;
        button.SizeOffset_Y = -2f;
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        AddChild(button);
        icon = new SleekItemIcon();
        AddChild(icon);
        icon.isAngled = true;
        amountLabel = Glazier.Get().CreateLabel();
        amountLabel.PositionScale_Y = 1f;
        amountLabel.SizeOffset_Y = 30f;
        amountLabel.SizeScale_X = 1f;
        amountLabel.TextAlignment = TextAnchor.LowerLeft;
        amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(amountLabel);
        amountLabel.IsVisible = false;
        qualityImage = Glazier.Get().CreateImage();
        qualityImage.PositionScale_X = 1f;
        qualityImage.PositionScale_Y = 1f;
        AddChild(qualityImage);
        qualityImage.IsVisible = false;
        hotkeyLabel = Glazier.Get().CreateLabel();
        hotkeyLabel.PositionOffset_X = 5f;
        hotkeyLabel.PositionOffset_Y = 5f;
        hotkeyLabel.SizeOffset_X = -10f;
        hotkeyLabel.SizeOffset_Y = 30f;
        hotkeyLabel.SizeScale_X = 1f;
        hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
        hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(hotkeyLabel);
        hotkeyLabel.IsVisible = false;
        isTemporary = true;
    }
}
