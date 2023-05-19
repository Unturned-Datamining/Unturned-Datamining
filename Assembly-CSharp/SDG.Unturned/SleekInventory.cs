using System;
using UnityEngine;

namespace SDG.Unturned;

public class SleekInventory : SleekWrapper
{
    private ItemAsset _itemAsset;

    private VehicleAsset _vehicleAsset;

    private ISleekButton button;

    private ISleekConstraintFrame iconFrame;

    private ISleekImage icon;

    private ISleekLabel nameLabel;

    private ISleekImage equippedIcon;

    private ISleekLabel statTrackerLabel;

    private ISleekLabel ragdollEffectLabel;

    private ISleekLabel particleEffectLabel;

    public ClickedInventory onClickedInventory;

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
        button.isClickable = isClickable;
        if (isLarge)
        {
            iconFrame.sizeOffset_Y = -70;
            nameLabel.fontSize = ESleekFontSize.Large;
            nameLabel.positionOffset_Y = -70;
            nameLabel.sizeOffset_Y = 70;
            equippedIcon.sizeOffset_X = 20;
            equippedIcon.sizeOffset_Y = 20;
            statTrackerLabel.fontSize = ESleekFontSize.Default;
            ragdollEffectLabel.fontSize = ESleekFontSize.Default;
            particleEffectLabel.fontSize = ESleekFontSize.Default;
        }
        else
        {
            iconFrame.sizeOffset_Y = -50;
            nameLabel.fontSize = ESleekFontSize.Default;
            nameLabel.positionOffset_Y = -50;
            nameLabel.sizeOffset_Y = 50;
            equippedIcon.sizeOffset_X = 10;
            equippedIcon.sizeOffset_Y = 10;
            statTrackerLabel.fontSize = ESleekFontSize.Tiny;
            ragdollEffectLabel.fontSize = ESleekFontSize.Tiny;
            particleEffectLabel.fontSize = ESleekFontSize.Tiny;
        }
        if (item != 0)
        {
            if (item < 0)
            {
                _itemAsset = null;
                _vehicleAsset = null;
                icon.texture = (Texture2D)Resources.Load("Economy/Mystery" + (isLarge ? "/Icon_Large" : "/Icon_Small"));
                icon.isVisible = true;
                nameLabel.text = MenuSurvivorsClothingUI.localization.format("Mystery_" + item + "_Text");
                button.tooltipText = MenuSurvivorsClothingUI.localization.format("Mystery_Tooltip");
                button.backgroundColor = SleekColor.BackgroundIfLight(Palette.MYTHICAL);
                button.textColor = Palette.MYTHICAL;
                nameLabel.textColor = Palette.MYTHICAL;
                nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                equippedIcon.isVisible = false;
            }
            else
            {
                Provider.provider.economyService.getInventoryTargetID(item, out var item_guid, out var vehicle_guid);
                if (item_guid == default(Guid) && vehicle_guid == default(Guid))
                {
                    _itemAsset = null;
                    _vehicleAsset = null;
                    icon.texture = null;
                    icon.isVisible = true;
                    nameLabel.text = "itemdefid: " + item;
                    button.tooltipText = "itemdefid: " + item;
                    button.backgroundColor = ESleekTint.BACKGROUND;
                    button.textColor = ESleekTint.FONT;
                    nameLabel.textColor = ESleekTint.FONT;
                    nameLabel.shadowStyle = ETextContrastContext.Default;
                    equippedIcon.isVisible = false;
                    statTrackerLabel.isVisible = false;
                    ragdollEffectLabel.isVisible = false;
                    particleEffectLabel.isVisible = false;
                }
                else
                {
                    _itemAsset = Assets.find<ItemAsset>(item_guid);
                    _vehicleAsset = Assets.find<VehicleAsset>(vehicle_guid);
                    Texture2D texture2D = Provider.provider.economyService.LoadItemIcon(item, isLarge);
                    icon.texture = texture2D;
                    icon.isVisible = texture2D != null;
                    nameLabel.text = Provider.provider.economyService.getInventoryName(item);
                    if (quantity > 1)
                    {
                        ISleekLabel sleekLabel = nameLabel;
                        sleekLabel.text = sleekLabel.text + " x" + quantity;
                    }
                    button.tooltipText = Provider.provider.economyService.getInventoryType(item);
                    Color inventoryColor = Provider.provider.economyService.getInventoryColor(item);
                    button.backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
                    button.textColor = inventoryColor;
                    nameLabel.textColor = inventoryColor;
                    nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                    bool flag = ((itemAsset != null && itemAsset.proPath != null && itemAsset.proPath.Length != 0) ? Characters.isCosmeticEquipped(instance) : Characters.isSkinEquipped(instance));
                    equippedIcon.isVisible = flag;
                    if (equippedIcon.isVisible && equippedIcon.texture == null)
                    {
                        equippedIcon.texture = MenuSurvivorsClothingUI.icons.load<Texture2D>("Equip");
                    }
                }
            }
            nameLabel.isVisible = true;
            if (!Provider.provider.economyService.getInventoryStatTrackerValue(instance, out var type, out var kills))
            {
                statTrackerLabel.isVisible = false;
            }
            else
            {
                statTrackerLabel.isVisible = true;
                statTrackerLabel.textColor = Provider.provider.economyService.getStatTrackerColor(type);
                statTrackerLabel.text = kills.ToString("D7");
            }
            if (!Provider.provider.economyService.getInventoryRagdollEffect(instance, out var _))
            {
                ragdollEffectLabel.isVisible = false;
            }
            else
            {
                ragdollEffectLabel.isVisible = true;
                ragdollEffectLabel.textColor = new Color(0f, 1f, 1f);
                ragdollEffectLabel.text = "0 Kelvin";
            }
            ushort num = Provider.provider.economyService.getInventoryMythicID(item);
            if (num == 0)
            {
                num = Provider.provider.economyService.getInventoryParticleEffect(instance);
            }
            if (num == 0)
            {
                particleEffectLabel.isVisible = false;
            }
            else
            {
                particleEffectLabel.isVisible = true;
                if (Assets.find(EAssetType.MYTHIC, num) is MythicAsset mythicAsset)
                {
                    particleEffectLabel.text = mythicAsset.particleTagName;
                }
                else
                {
                    particleEffectLabel.text = num.ToString();
                }
            }
            if (!string.IsNullOrEmpty(extraTooltip))
            {
                ISleekButton sleekButton = button;
                sleekButton.tooltipText = sleekButton.tooltipText + "\n" + extraTooltip;
            }
        }
        else
        {
            _itemAsset = null;
            button.tooltipText = "";
            button.backgroundColor = ESleekTint.BACKGROUND;
            button.textColor = ESleekTint.FONT;
            icon.isVisible = false;
            nameLabel.isVisible = false;
            equippedIcon.isVisible = false;
            statTrackerLabel.isVisible = false;
            ragdollEffectLabel.isVisible = false;
            particleEffectLabel.isVisible = false;
        }
    }

    private void onClickedButton(ISleekElement button)
    {
        onClickedInventory?.Invoke(this);
    }

    public SleekInventory()
    {
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += onClickedButton;
        AddChild(button);
        button.isClickable = false;
        iconFrame = Glazier.Get().CreateConstraintFrame();
        iconFrame.positionOffset_X = 5;
        iconFrame.positionOffset_Y = 5;
        iconFrame.sizeScale_X = 1f;
        iconFrame.sizeScale_Y = 1f;
        iconFrame.sizeOffset_X = -10;
        iconFrame.constraint = ESleekConstraint.FitInParent;
        AddChild(iconFrame);
        icon = Glazier.Get().CreateImage();
        icon.sizeScale_X = 1f;
        icon.sizeScale_Y = 1f;
        iconFrame.AddChild(icon);
        icon.isVisible = false;
        equippedIcon = Glazier.Get().CreateImage();
        equippedIcon.positionOffset_X = 5;
        equippedIcon.positionOffset_Y = 5;
        equippedIcon.color = ESleekTint.FOREGROUND;
        AddChild(equippedIcon);
        equippedIcon.isVisible = false;
        ragdollEffectLabel = Glazier.Get().CreateLabel();
        ragdollEffectLabel.positionOffset_Y = -30;
        ragdollEffectLabel.positionScale_Y = 1f;
        ragdollEffectLabel.sizeOffset_Y = 30;
        ragdollEffectLabel.sizeScale_X = 1f;
        ragdollEffectLabel.fontAlignment = TextAnchor.LowerRight;
        ragdollEffectLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        ragdollEffectLabel.fontStyle = FontStyle.Italic;
        AddChild(ragdollEffectLabel);
        ragdollEffectLabel.isVisible = false;
        particleEffectLabel = Glazier.Get().CreateLabel();
        particleEffectLabel.sizeOffset_Y = 30;
        particleEffectLabel.sizeScale_X = 1f;
        particleEffectLabel.textColor = Palette.MYTHICAL;
        particleEffectLabel.fontAlignment = TextAnchor.UpperRight;
        particleEffectLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(particleEffectLabel);
        particleEffectLabel.isVisible = false;
        statTrackerLabel = Glazier.Get().CreateLabel();
        statTrackerLabel.positionOffset_Y = -30;
        statTrackerLabel.positionScale_Y = 1f;
        statTrackerLabel.sizeOffset_Y = 30;
        statTrackerLabel.sizeScale_X = 1f;
        statTrackerLabel.fontAlignment = TextAnchor.LowerLeft;
        statTrackerLabel.fontStyle = FontStyle.Italic;
        statTrackerLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(statTrackerLabel);
        statTrackerLabel.isVisible = false;
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionScale_Y = 1f;
        nameLabel.sizeScale_X = 1f;
        AddChild(nameLabel);
        nameLabel.isVisible = false;
    }
}
