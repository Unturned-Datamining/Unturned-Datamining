using System;
using UnityEngine;

namespace SDG.Unturned;

public class SleekInventory : SleekWrapper
{
    private ItemAsset _itemAsset;

    private VehicleAsset _vehicleAsset;

    private ISleekButton button;

    private ISleekConstraintFrame iconFrame;

    private SleekEconIcon icon;

    private ISleekLabel nameLabel;

    private ISleekImage equippedIcon;

    private ISleekLabel statTrackerLabel;

    private ISleekLabel ragdollEffectLabel;

    private ISleekLabel particleEffectLabel;

    public ClickedInventory onClickedInventory;

    /// <summary>
    /// Hack, we put this string on a newline for box probabilities.
    /// </summary>
    public string extraTooltip;

    public ItemAsset itemAsset => _itemAsset;

    public VehicleAsset vehicleAsset => _vehicleAsset;

    public ulong instance { get; protected set; }

    public int item { get; protected set; }

    public ushort quantity { get; protected set; }

    public void updateInventory(ulong newInstance, int newItem, ushort newQuantity, bool isClickable, bool isLarge)
    {
        instance = newInstance;
        item = newItem;
        quantity = newQuantity;
        button.IsClickable = isClickable;
        if (isLarge)
        {
            iconFrame.SizeOffset_Y = -70f;
            nameLabel.FontSize = ESleekFontSize.Large;
            nameLabel.PositionOffset_Y = -70f;
            nameLabel.SizeOffset_Y = 70f;
            equippedIcon.SizeOffset_X = 20f;
            equippedIcon.SizeOffset_Y = 20f;
            statTrackerLabel.FontSize = ESleekFontSize.Default;
            ragdollEffectLabel.FontSize = ESleekFontSize.Default;
            particleEffectLabel.FontSize = ESleekFontSize.Default;
        }
        else
        {
            iconFrame.SizeOffset_Y = -50f;
            nameLabel.FontSize = ESleekFontSize.Default;
            nameLabel.PositionOffset_Y = -50f;
            nameLabel.SizeOffset_Y = 50f;
            equippedIcon.SizeOffset_X = 10f;
            equippedIcon.SizeOffset_Y = 10f;
            statTrackerLabel.FontSize = ESleekFontSize.Tiny;
            ragdollEffectLabel.FontSize = ESleekFontSize.Tiny;
            particleEffectLabel.FontSize = ESleekFontSize.Tiny;
        }
        if (item != 0)
        {
            if (item < 0)
            {
                _itemAsset = null;
                _vehicleAsset = null;
                icon.SetIsBoxMythicalIcon();
                icon.IsVisible = true;
                nameLabel.Text = MenuSurvivorsClothingUI.localization.format("Mystery_" + item + "_Text");
                button.TooltipText = MenuSurvivorsClothingUI.localization.format("Mystery_Tooltip");
                button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.MYTHICAL);
                button.TextColor = Palette.MYTHICAL;
                nameLabel.TextColor = Palette.MYTHICAL;
                nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                equippedIcon.IsVisible = false;
            }
            else
            {
                Provider.provider.economyService.getInventoryTargetID(item, out var item_guid, out var vehicle_guid);
                if (item_guid == default(Guid) && vehicle_guid == default(Guid))
                {
                    _itemAsset = null;
                    _vehicleAsset = null;
                    icon.SetItemDefId(-1);
                    icon.IsVisible = false;
                    nameLabel.Text = "itemdefid: " + item;
                    button.TooltipText = "itemdefid: " + item;
                    button.BackgroundColor = ESleekTint.BACKGROUND;
                    button.TextColor = ESleekTint.FONT;
                    nameLabel.TextColor = ESleekTint.FONT;
                    nameLabel.TextContrastContext = ETextContrastContext.Default;
                    equippedIcon.IsVisible = false;
                    statTrackerLabel.IsVisible = false;
                    ragdollEffectLabel.IsVisible = false;
                    particleEffectLabel.IsVisible = false;
                }
                else
                {
                    _itemAsset = Assets.find<ItemAsset>(item_guid);
                    _vehicleAsset = Assets.find<VehicleAsset>(vehicle_guid);
                    icon.SetItemDefId(item);
                    icon.IsVisible = true;
                    nameLabel.Text = Provider.provider.economyService.getInventoryName(item);
                    if (quantity > 1)
                    {
                        ISleekLabel sleekLabel = nameLabel;
                        sleekLabel.Text = sleekLabel.Text + " x" + quantity;
                    }
                    button.TooltipText = Provider.provider.economyService.getInventoryType(item);
                    Color inventoryColor = Provider.provider.economyService.getInventoryColor(item);
                    button.BackgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
                    button.TextColor = inventoryColor;
                    nameLabel.TextColor = inventoryColor;
                    nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                    bool isVisible = ((itemAsset != null && itemAsset.proPath != null && itemAsset.proPath.Length != 0) ? Characters.isCosmeticEquipped(instance) : Characters.isSkinEquipped(instance));
                    equippedIcon.IsVisible = isVisible;
                    if (equippedIcon.IsVisible && equippedIcon.Texture == null)
                    {
                        equippedIcon.Texture = MenuSurvivorsClothingUI.icons.load<Texture2D>("Equip");
                    }
                }
            }
            nameLabel.IsVisible = true;
            if (!Provider.provider.economyService.getInventoryStatTrackerValue(instance, out var type, out var kills))
            {
                statTrackerLabel.IsVisible = false;
            }
            else
            {
                statTrackerLabel.IsVisible = true;
                statTrackerLabel.TextColor = Provider.provider.economyService.getStatTrackerColor(type);
                statTrackerLabel.Text = kills.ToString("D7");
            }
            if (!Provider.provider.economyService.getInventoryRagdollEffect(instance, out var effect))
            {
                ragdollEffectLabel.IsVisible = false;
            }
            else
            {
                ragdollEffectLabel.IsVisible = true;
                switch (effect)
                {
                case ERagdollEffect.ZERO_KELVIN:
                    ragdollEffectLabel.TextColor = new Color(0f, 1f, 1f);
                    ragdollEffectLabel.Text = "0 Kelvin";
                    break;
                case ERagdollEffect.JADED:
                    ragdollEffectLabel.TextColor = new Color32(76, 166, 90, byte.MaxValue);
                    ragdollEffectLabel.Text = "Jaded";
                    break;
                case ERagdollEffect.SOUL_CRYSTAL_GREEN:
                    ragdollEffectLabel.TextColor = Palette.MYTHICAL;
                    ragdollEffectLabel.Text = "Green Soul Crystal";
                    break;
                case ERagdollEffect.SOUL_CRYSTAL_MAGENTA:
                    ragdollEffectLabel.TextColor = Palette.MYTHICAL;
                    ragdollEffectLabel.Text = "Magenta Soul Crystal";
                    break;
                case ERagdollEffect.SOUL_CRYSTAL_RED:
                    ragdollEffectLabel.TextColor = Palette.MYTHICAL;
                    ragdollEffectLabel.Text = "Red Soul Crystal";
                    break;
                case ERagdollEffect.SOUL_CRYSTAL_YELLOW:
                    ragdollEffectLabel.TextColor = Palette.MYTHICAL;
                    ragdollEffectLabel.Text = "Yellow Soul Crystal";
                    break;
                default:
                    ragdollEffectLabel.TextColor = Color.red;
                    ragdollEffectLabel.Text = effect.ToString();
                    break;
                }
            }
            ushort num = Provider.provider.economyService.getInventoryMythicID(item);
            if (num == 0)
            {
                num = Provider.provider.economyService.getInventoryParticleEffect(instance);
            }
            if (num == 0)
            {
                particleEffectLabel.IsVisible = false;
            }
            else
            {
                particleEffectLabel.IsVisible = true;
                if (Assets.find(EAssetType.MYTHIC, num) is MythicAsset mythicAsset)
                {
                    particleEffectLabel.Text = mythicAsset.particleTagName;
                }
                else
                {
                    particleEffectLabel.Text = num.ToString();
                }
            }
            if (!string.IsNullOrEmpty(extraTooltip))
            {
                ISleekButton sleekButton = button;
                sleekButton.TooltipText = sleekButton.TooltipText + "\n" + extraTooltip;
            }
        }
        else
        {
            _itemAsset = null;
            button.TooltipText = "";
            button.BackgroundColor = ESleekTint.BACKGROUND;
            button.TextColor = ESleekTint.FONT;
            icon.IsVisible = false;
            nameLabel.IsVisible = false;
            equippedIcon.IsVisible = false;
            statTrackerLabel.IsVisible = false;
            ragdollEffectLabel.IsVisible = false;
            particleEffectLabel.IsVisible = false;
        }
    }

    private void onClickedButton(ISleekElement button)
    {
        onClickedInventory?.Invoke(this);
    }

    public SleekInventory()
    {
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += onClickedButton;
        AddChild(button);
        button.IsClickable = false;
        iconFrame = Glazier.Get().CreateConstraintFrame();
        iconFrame.PositionOffset_X = 5f;
        iconFrame.PositionOffset_Y = 5f;
        iconFrame.SizeScale_X = 1f;
        iconFrame.SizeScale_Y = 1f;
        iconFrame.SizeOffset_X = -10f;
        iconFrame.Constraint = ESleekConstraint.FitInParent;
        AddChild(iconFrame);
        icon = new SleekEconIcon();
        icon.SizeScale_X = 1f;
        icon.SizeScale_Y = 1f;
        iconFrame.AddChild(icon);
        icon.IsVisible = false;
        equippedIcon = Glazier.Get().CreateImage();
        equippedIcon.PositionOffset_X = 5f;
        equippedIcon.PositionOffset_Y = 5f;
        equippedIcon.TintColor = ESleekTint.FOREGROUND;
        AddChild(equippedIcon);
        equippedIcon.IsVisible = false;
        ragdollEffectLabel = Glazier.Get().CreateLabel();
        ragdollEffectLabel.PositionOffset_Y = -30f;
        ragdollEffectLabel.PositionScale_Y = 1f;
        ragdollEffectLabel.SizeOffset_Y = 30f;
        ragdollEffectLabel.SizeScale_X = 1f;
        ragdollEffectLabel.TextAlignment = TextAnchor.LowerRight;
        ragdollEffectLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        ragdollEffectLabel.FontStyle = FontStyle.Italic;
        AddChild(ragdollEffectLabel);
        ragdollEffectLabel.IsVisible = false;
        particleEffectLabel = Glazier.Get().CreateLabel();
        particleEffectLabel.SizeOffset_Y = 30f;
        particleEffectLabel.SizeScale_X = 1f;
        particleEffectLabel.TextColor = Palette.MYTHICAL;
        particleEffectLabel.TextAlignment = TextAnchor.UpperRight;
        particleEffectLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(particleEffectLabel);
        particleEffectLabel.IsVisible = false;
        statTrackerLabel = Glazier.Get().CreateLabel();
        statTrackerLabel.PositionOffset_Y = -30f;
        statTrackerLabel.PositionScale_Y = 1f;
        statTrackerLabel.SizeOffset_Y = 30f;
        statTrackerLabel.SizeScale_X = 1f;
        statTrackerLabel.TextAlignment = TextAnchor.LowerLeft;
        statTrackerLabel.FontStyle = FontStyle.Italic;
        statTrackerLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        AddChild(statTrackerLabel);
        statTrackerLabel.IsVisible = false;
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionScale_Y = 1f;
        nameLabel.SizeScale_X = 1f;
        AddChild(nameLabel);
        nameLabel.IsVisible = false;
    }
}
