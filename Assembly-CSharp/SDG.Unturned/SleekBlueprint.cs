using UnityEngine;

namespace SDG.Unturned;

public class SleekBlueprint : SleekWrapper
{
    public delegate void Clicked(Blueprint blueprint);

    private Blueprint _blueprint;

    private ISleekButton craftButton;

    private ISleekButton craftAllButton;

    private ISleekButton backgroundButton;

    private ISleekElement container;

    private ISleekToggle ignoreToggleButton;

    public Blueprint blueprint => _blueprint;

    private bool isCraftable
    {
        get
        {
            if (blueprint.hasSupplies && blueprint.hasTool && blueprint.hasItem)
            {
                return blueprint.hasSkills;
            }
            return false;
        }
    }

    public event Clicked onClickedCraftButton;

    public event Clicked onClickedCraftAllButton;

    private void onToggledIgnoring(ISleekToggle toggle, bool toggleState)
    {
        Player.player.crafting.setIgnoringBlueprint(blueprint, !toggleState);
        refreshIgnoring();
    }

    private void refreshIgnoring()
    {
        bool ignoringBlueprint = Player.player.crafting.getIgnoringBlueprint(blueprint);
        if (backgroundButton != null)
        {
            backgroundButton.isClickable = !ignoringBlueprint && isCraftable;
        }
        if (craftButton != null)
        {
            craftButton.isClickable = !ignoringBlueprint;
            craftAllButton.isClickable = craftButton.isClickable;
        }
        ignoreToggleButton.state = !ignoringBlueprint;
    }

    public SleekBlueprint(Blueprint newBlueprint)
    {
        _blueprint = newBlueprint;
        bool supportsDepth = Glazier.Get().SupportsDepth;
        if (supportsDepth)
        {
            backgroundButton = Glazier.Get().CreateButton();
            backgroundButton.sizeScale_X = 1f;
            backgroundButton.sizeScale_Y = 1f;
            backgroundButton.onClickedButton += onClickedBackgroundButton;
            AddChild(backgroundButton);
        }
        else
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.sizeScale_X = 1f;
            sleekBox.sizeScale_Y = 1f;
            AddChild(sleekBox);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 5;
        sleekLabel.positionOffset_Y = 5;
        sleekLabel.sizeOffset_X = -10;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.textColor = (isCraftable ? ESleekTint.FONT : ESleekTint.BAD);
        sleekLabel.shadowStyle = ((!isCraftable) ? ETextContrastContext.InconspicuousBackdrop : ETextContrastContext.Default);
        sleekLabel.fontSize = ESleekFontSize.Medium;
        AddChild(sleekLabel);
        if (blueprint.skill != 0)
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.positionOffset_X = 5;
            sleekLabel2.positionOffset_Y = -35;
            sleekLabel2.positionScale_Y = 1f;
            sleekLabel2.sizeOffset_X = -10;
            sleekLabel2.sizeOffset_Y = 30;
            sleekLabel2.sizeScale_X = 1f;
            sleekLabel2.text = PlayerDashboardCraftingUI.localization.format("Skill_" + (int)blueprint.skill, PlayerDashboardSkillsUI.localization.format("Level_" + blueprint.level));
            sleekLabel2.textColor = (blueprint.hasSkills ? ESleekTint.FONT : ESleekTint.BAD);
            sleekLabel2.shadowStyle = ((!blueprint.hasSkills) ? ETextContrastContext.InconspicuousBackdrop : ETextContrastContext.Default);
            sleekLabel2.fontSize = ESleekFontSize.Medium;
            AddChild(sleekLabel2);
        }
        container = Glazier.Get().CreateFrame();
        container.positionOffset_Y = 40;
        container.positionScale_X = 0.5f;
        container.sizeOffset_Y = -45;
        container.sizeScale_Y = 1f;
        AddChild(container);
        int num = 0;
        for (int i = 0; i < blueprint.supplies.Length; i++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[i];
            if (!(Assets.find(EAssetType.ITEM, blueprintSupply.id) is ItemAsset itemAsset))
            {
                continue;
            }
            sleekLabel.text += itemAsset.itemName;
            SleekItemIcon sleekItemIcon = new SleekItemIcon
            {
                positionOffset_X = num,
                positionOffset_Y = -itemAsset.size_y * 25,
                positionScale_Y = 0.5f,
                sizeOffset_X = itemAsset.size_x * 50,
                sizeOffset_Y = itemAsset.size_y * 50
            };
            container.AddChild(sleekItemIcon);
            sleekItemIcon.Refresh(blueprintSupply.id, 100, itemAsset.getState(isFull: false), itemAsset);
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.positionOffset_X = -100;
            sleekLabel3.positionOffset_Y = -30;
            sleekLabel3.positionScale_Y = 1f;
            sleekLabel3.sizeOffset_X = 100;
            sleekLabel3.sizeOffset_Y = 30;
            sleekLabel3.sizeScale_X = 1f;
            sleekLabel3.fontAlignment = TextAnchor.MiddleRight;
            sleekLabel3.text = blueprintSupply.hasAmount + "/" + blueprintSupply.amount;
            sleekLabel3.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon.AddChild(sleekLabel3);
            ISleekLabel sleekLabel4 = sleekLabel;
            sleekLabel4.text = sleekLabel4.text + " " + blueprintSupply.hasAmount + "/" + blueprintSupply.amount;
            if (blueprint.type == EBlueprintType.AMMO)
            {
                if (blueprintSupply.hasAmount == 0 || blueprintSupply.amount == 0)
                {
                    sleekLabel3.textColor = ESleekTint.BAD;
                }
            }
            else if (blueprintSupply.hasAmount < blueprintSupply.amount)
            {
                sleekLabel3.textColor = ESleekTint.BAD;
            }
            num += itemAsset.size_x * 50 + 25;
            if (i < blueprint.supplies.Length - 1 || blueprint.tool != 0 || blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO)
            {
                sleekLabel.text += " + ";
                ISleekImage sleekImage = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage.positionOffset_X = num;
                sleekImage.positionOffset_Y = -20;
                sleekImage.positionScale_Y = 0.5f;
                sleekImage.sizeOffset_X = 40;
                sleekImage.sizeOffset_Y = 40;
                sleekImage.color = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage);
                num += 65;
            }
        }
        if (blueprint.tool != 0 && Assets.find(EAssetType.ITEM, blueprint.tool) is ItemAsset itemAsset2)
        {
            sleekLabel.text += itemAsset2.itemName;
            SleekItemIcon sleekItemIcon2 = new SleekItemIcon
            {
                positionOffset_X = num,
                positionOffset_Y = -itemAsset2.size_y * 25,
                positionScale_Y = 0.5f,
                sizeOffset_X = itemAsset2.size_x * 50,
                sizeOffset_Y = itemAsset2.size_y * 50
            };
            container.AddChild(sleekItemIcon2);
            sleekItemIcon2.Refresh(blueprint.tool, 100, itemAsset2.getState(), itemAsset2);
            ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
            sleekLabel5.positionOffset_X = -100;
            sleekLabel5.positionOffset_Y = -30;
            sleekLabel5.positionScale_Y = 1f;
            sleekLabel5.sizeOffset_X = 100;
            sleekLabel5.sizeOffset_Y = 30;
            sleekLabel5.sizeScale_X = 1f;
            sleekLabel5.fontAlignment = TextAnchor.MiddleRight;
            sleekLabel5.text = blueprint.tools + "/1";
            sleekLabel5.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon2.AddChild(sleekLabel5);
            sleekLabel.text = sleekLabel.text + " " + blueprint.tools + "/1";
            if (!blueprint.hasTool)
            {
                sleekLabel5.textColor = ESleekTint.BAD;
            }
            num += itemAsset2.size_x * 50 + 25;
            if (blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO)
            {
                sleekLabel.text += " + ";
                ISleekImage sleekImage2 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage2.positionOffset_X = num;
                sleekImage2.positionOffset_Y = -20;
                sleekImage2.positionScale_Y = 0.5f;
                sleekImage2.sizeOffset_X = 40;
                sleekImage2.sizeOffset_Y = 40;
                sleekImage2.color = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage2);
                num += 65;
            }
        }
        if ((blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO) && Assets.find(EAssetType.ITEM, blueprint.outputs[0].id) is ItemAsset itemAsset3)
        {
            sleekLabel.text += itemAsset3.itemName;
            SleekItemIcon sleekItemIcon3 = new SleekItemIcon
            {
                positionOffset_X = num,
                positionOffset_Y = -itemAsset3.size_y * 25,
                positionScale_Y = 0.5f,
                sizeOffset_X = itemAsset3.size_x * 50,
                sizeOffset_Y = itemAsset3.size_y * 50
            };
            container.AddChild(sleekItemIcon3);
            sleekItemIcon3.Refresh(blueprint.outputs[0].id, 100, itemAsset3.getState(), itemAsset3);
            ISleekLabel sleekLabel6 = Glazier.Get().CreateLabel();
            sleekLabel6.positionOffset_X = -100;
            sleekLabel6.positionOffset_Y = -30;
            sleekLabel6.positionScale_Y = 1f;
            sleekLabel6.sizeOffset_X = 100;
            sleekLabel6.sizeOffset_Y = 30;
            sleekLabel6.sizeScale_X = 1f;
            sleekLabel6.fontAlignment = TextAnchor.MiddleRight;
            if (blueprint.type == EBlueprintType.REPAIR)
            {
                sleekLabel.text = sleekLabel.text + " " + blueprint.items + "%";
                sleekLabel6.text = blueprint.items + "%";
                sleekLabel6.textColor = ItemTool.getQualityColor((float)(int)blueprint.items / 100f);
            }
            else if (blueprint.type == EBlueprintType.AMMO)
            {
                ISleekLabel sleekLabel4 = sleekLabel;
                sleekLabel4.text = sleekLabel4.text + " " + blueprint.items + "/" + blueprint.products;
                sleekLabel6.text = blueprint.items + "/" + itemAsset3.amount;
            }
            if (!blueprint.hasItem)
            {
                sleekLabel6.textColor = ESleekTint.BAD;
            }
            sleekLabel6.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon3.AddChild(sleekLabel6);
            num += itemAsset3.size_x * 50 + 25;
        }
        sleekLabel.text += " = ";
        ISleekImage sleekImage3 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"));
        sleekImage3.positionOffset_X = num;
        sleekImage3.positionOffset_Y = -20;
        sleekImage3.positionScale_Y = 0.5f;
        sleekImage3.sizeOffset_X = 40;
        sleekImage3.sizeOffset_Y = 40;
        sleekImage3.color = ESleekTint.FOREGROUND;
        container.AddChild(sleekImage3);
        num += 65;
        for (int j = 0; j < blueprint.outputs.Length; j++)
        {
            BlueprintOutput blueprintOutput = blueprint.outputs[j];
            if (!(Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset4))
            {
                continue;
            }
            sleekLabel.text += itemAsset4.itemName;
            SleekItemIcon sleekItemIcon4 = new SleekItemIcon
            {
                positionOffset_X = num,
                positionOffset_Y = -itemAsset4.size_y * 25,
                positionScale_Y = 0.5f,
                sizeOffset_X = itemAsset4.size_x * 50,
                sizeOffset_Y = itemAsset4.size_y * 50
            };
            container.AddChild(sleekItemIcon4);
            sleekItemIcon4.Refresh(blueprintOutput.id, 100, itemAsset4.getState(), itemAsset4);
            ISleekLabel sleekLabel7 = Glazier.Get().CreateLabel();
            sleekLabel7.positionOffset_X = -100;
            sleekLabel7.positionOffset_Y = -30;
            sleekLabel7.positionScale_Y = 1f;
            sleekLabel7.sizeOffset_X = 100;
            sleekLabel7.sizeOffset_Y = 30;
            sleekLabel7.sizeScale_X = 1f;
            sleekLabel7.fontAlignment = TextAnchor.MiddleRight;
            sleekLabel7.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            if (blueprint.type == EBlueprintType.REPAIR)
            {
                sleekLabel.text += " 100%";
                sleekLabel7.text = "100%";
                sleekLabel7.textColor = Palette.COLOR_G;
            }
            else if (blueprint.type == EBlueprintType.AMMO)
            {
                if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset5)
                {
                    ISleekLabel sleekLabel4 = sleekLabel;
                    sleekLabel4.text = sleekLabel4.text + " " + blueprint.products + "/" + itemAsset5.amount;
                    sleekLabel7.text = blueprint.products + "/" + itemAsset5.amount;
                }
            }
            else
            {
                sleekLabel.text = sleekLabel.text + " x" + blueprintOutput.amount;
                sleekLabel7.text = "x" + blueprintOutput.amount;
            }
            sleekItemIcon4.AddChild(sleekLabel7);
            num += itemAsset4.size_x * 50;
            if (j < blueprint.outputs.Length - 1)
            {
                num += 25;
                sleekLabel.text += " + ";
                ISleekImage sleekImage4 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage4.positionOffset_X = num;
                sleekImage4.positionOffset_Y = -20;
                sleekImage4.positionScale_Y = 0.5f;
                sleekImage4.sizeOffset_X = 40;
                sleekImage4.sizeOffset_Y = 40;
                sleekImage4.color = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage4);
                num += 65;
            }
        }
        container.positionOffset_X = -num / 2;
        container.sizeOffset_X = num;
        if (!supportsDepth)
        {
            craftButton = Glazier.Get().CreateButton();
            craftButton.positionOffset_X = -70;
            craftButton.positionOffset_Y = -35;
            craftButton.positionScale_X = 0.75f;
            craftButton.positionScale_Y = 1f;
            craftButton.sizeOffset_X = 140;
            craftButton.sizeOffset_Y = 30;
            craftButton.text = PlayerDashboardCraftingUI.localization.format("Craft");
            craftButton.textColor = sleekLabel.textColor;
            craftButton.onClickedButton += onClickedButton;
            AddChild(craftButton);
            craftAllButton = Glazier.Get().CreateButton();
            craftAllButton.positionOffset_X = -70;
            craftAllButton.positionOffset_Y = -35;
            craftAllButton.positionScale_X = 0.25f;
            craftAllButton.positionScale_Y = 1f;
            craftAllButton.sizeOffset_X = 140;
            craftAllButton.sizeOffset_Y = 30;
            craftAllButton.text = PlayerDashboardCraftingUI.localization.format("Craft_All");
            craftAllButton.textColor = sleekLabel.textColor;
            craftAllButton.onClickedButton += onClickedAltButton;
            AddChild(craftAllButton);
        }
        ignoreToggleButton = Glazier.Get().CreateToggle();
        ignoreToggleButton.positionOffset_X = -50;
        ignoreToggleButton.positionOffset_Y = -40;
        ignoreToggleButton.positionScale_X = 1f;
        ignoreToggleButton.positionScale_Y = 1f;
        ignoreToggleButton.sizeOffset_X = 40;
        ignoreToggleButton.sizeOffset_Y = 40;
        ignoreToggleButton.onToggled += onToggledIgnoring;
        AddChild(ignoreToggleButton);
        refreshIgnoring();
    }

    private void onClickedBackgroundButton(ISleekElement internalButton)
    {
        this.onClickedCraftButton?.Invoke(blueprint);
    }

    private void onClickedButton(ISleekElement internalButton)
    {
        this.onClickedCraftButton?.Invoke(blueprint);
    }

    private void onClickedAltButton(ISleekElement internalButton)
    {
        this.onClickedCraftAllButton?.Invoke(blueprint);
    }
}
