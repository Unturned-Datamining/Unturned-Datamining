using UnityEngine;

namespace SDG.Unturned;

public class SleekHotbarEntry : SleekWrapper
{
    private bool _isEquipped;

    private SleekItemIcon icon;

    private ISleekLabel hotkeyLabel;

    private ISleekLabel qualityLabel;

    private ItemJar itemJar;

    private ItemAsset displayAsset;

    private byte[] displayState;

    private int displayQuality = -1;

    private bool doesItemHaveQuality;

    public bool IsEquipped
    {
        get
        {
            return _isEquipped;
        }
        set
        {
            _isEquipped = value;
            if (!_isEquipped)
            {
                qualityLabel.IsVisible = false;
            }
            icon.color = new Color(1f, 1f, 1f, _isEquipped ? 0.75f : 0.5f);
            hotkeyLabel.TextColor = new SleekColor(ESleekTint.FONT, _isEquipped ? 1f : 0.75f);
        }
    }

    public void UpdateItem(ItemJar jar)
    {
        itemJar = jar;
        ItemAsset itemAsset = null;
        byte[] array = null;
        if (jar != null && jar.item != null)
        {
            itemAsset = jar.GetAsset();
            array = jar.item.state;
        }
        displayQuality = -1;
        if (displayAsset != itemAsset || displayState != array)
        {
            displayAsset = itemAsset;
            displayState = array;
            base.IsVisible = displayAsset != null;
            doesItemHaveQuality = displayAsset?.showQuality ?? false;
            if (displayAsset != null)
            {
                base.SizeOffset_X = displayAsset.size_x * 25;
                base.SizeOffset_Y = displayAsset.size_y * 25;
                icon.Refresh(jar.item.id, jar.item.quality, jar.item.state, displayAsset);
            }
        }
        if (!doesItemHaveQuality)
        {
            qualityLabel.IsVisible = false;
        }
        UpdateQuality();
    }

    public void UpdateQuality()
    {
        if (doesItemHaveQuality)
        {
            qualityLabel.IsVisible = IsEquipped;
            int num = -1;
            if (itemJar != null && itemJar.item != null)
            {
                num = itemJar.item.quality;
            }
            if (displayQuality != num)
            {
                displayQuality = num;
                qualityLabel.TextColor = ItemTool.getQualityColor((float)displayQuality / 100f);
                qualityLabel.Text = $"{displayQuality}%";
            }
        }
    }

    public SleekHotbarEntry(int hotbarIndex)
    {
        icon = new SleekItemIcon();
        icon.SizeScale_X = 1f;
        icon.SizeScale_Y = 1f;
        icon.color = new Color(1f, 1f, 1f, 0.5f);
        AddChild(icon);
        hotkeyLabel = Glazier.Get().CreateLabel();
        hotkeyLabel.PositionOffset_X = -50f;
        hotkeyLabel.PositionScale_X = 1f;
        hotkeyLabel.SizeOffset_X = 50f;
        hotkeyLabel.SizeOffset_Y = 30f;
        hotkeyLabel.Text = ControlsSettings.getEquipmentHotkeyText(hotbarIndex);
        hotkeyLabel.TextAlignment = TextAnchor.UpperRight;
        hotkeyLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.75f);
        hotkeyLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(hotkeyLabel);
        qualityLabel = Glazier.Get().CreateLabel();
        qualityLabel.PositionOffset_X = -25f;
        qualityLabel.PositionScale_X = 0.5f;
        qualityLabel.PositionScale_Y = 1f;
        qualityLabel.SizeOffset_X = 50f;
        qualityLabel.SizeOffset_Y = 30f;
        qualityLabel.TextAlignment = TextAnchor.UpperCenter;
        qualityLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(qualityLabel);
    }
}
