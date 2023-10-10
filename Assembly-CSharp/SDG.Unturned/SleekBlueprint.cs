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
            backgroundButton.IsClickable = !ignoringBlueprint && isCraftable;
        }
        if (craftButton != null)
        {
            craftButton.IsClickable = !ignoringBlueprint;
            craftAllButton.IsClickable = craftButton.IsClickable;
        }
        ignoreToggleButton.Value = !ignoringBlueprint;
    }

    public SleekBlueprint(Blueprint newBlueprint)
    {
        _blueprint = newBlueprint;
        bool supportsDepth = Glazier.Get().SupportsDepth;
        if (supportsDepth)
        {
            backgroundButton = Glazier.Get().CreateButton();
            backgroundButton.SizeScale_X = 1f;
            backgroundButton.SizeScale_Y = 1f;
            backgroundButton.OnClicked += onClickedBackgroundButton;
            AddChild(backgroundButton);
        }
        else
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.SizeScale_X = 1f;
            sleekBox.SizeScale_Y = 1f;
            AddChild(sleekBox);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 5f;
        sleekLabel.PositionOffset_Y = 5f;
        sleekLabel.SizeOffset_X = -10f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.TextColor = (isCraftable ? ESleekTint.FONT : ESleekTint.BAD);
        sleekLabel.TextContrastContext = ((!isCraftable) ? ETextContrastContext.InconspicuousBackdrop : ETextContrastContext.Default);
        sleekLabel.FontSize = ESleekFontSize.Medium;
        AddChild(sleekLabel);
        if (blueprint.skill != 0)
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.PositionOffset_X = 5f;
            sleekLabel2.PositionOffset_Y = -35f;
            sleekLabel2.PositionScale_Y = 1f;
            sleekLabel2.SizeOffset_X = -10f;
            sleekLabel2.SizeOffset_Y = 30f;
            sleekLabel2.SizeScale_X = 1f;
            sleekLabel2.Text = PlayerDashboardCraftingUI.localization.format("Skill_" + (int)blueprint.skill, PlayerDashboardSkillsUI.localization.format("Level_" + blueprint.level));
            sleekLabel2.TextColor = (blueprint.hasSkills ? ESleekTint.FONT : ESleekTint.BAD);
            sleekLabel2.TextContrastContext = ((!blueprint.hasSkills) ? ETextContrastContext.InconspicuousBackdrop : ETextContrastContext.Default);
            sleekLabel2.FontSize = ESleekFontSize.Medium;
            AddChild(sleekLabel2);
        }
        container = Glazier.Get().CreateFrame();
        container.PositionOffset_Y = 40f;
        container.PositionScale_X = 0.5f;
        container.SizeOffset_Y = -45f;
        container.SizeScale_Y = 1f;
        AddChild(container);
        int num = 0;
        for (int i = 0; i < blueprint.supplies.Length; i++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[i];
            if (!(Assets.find(EAssetType.ITEM, blueprintSupply.id) is ItemAsset itemAsset))
            {
                continue;
            }
            sleekLabel.Text += itemAsset.itemName;
            SleekItemIcon sleekItemIcon = new SleekItemIcon
            {
                PositionOffset_X = num,
                PositionOffset_Y = -itemAsset.size_y * 25,
                PositionScale_Y = 0.5f,
                SizeOffset_X = itemAsset.size_x * 50,
                SizeOffset_Y = itemAsset.size_y * 50
            };
            container.AddChild(sleekItemIcon);
            sleekItemIcon.Refresh(blueprintSupply.id, 100, itemAsset.getState(isFull: false), itemAsset);
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.PositionOffset_X = -100f;
            sleekLabel3.PositionOffset_Y = -30f;
            sleekLabel3.PositionScale_Y = 1f;
            sleekLabel3.SizeOffset_X = 100f;
            sleekLabel3.SizeOffset_Y = 30f;
            sleekLabel3.SizeScale_X = 1f;
            sleekLabel3.TextAlignment = TextAnchor.MiddleRight;
            sleekLabel3.Text = blueprintSupply.hasAmount + "/" + blueprintSupply.amount;
            sleekLabel3.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon.AddChild(sleekLabel3);
            ISleekLabel sleekLabel4 = sleekLabel;
            sleekLabel4.Text = sleekLabel4.Text + " " + blueprintSupply.hasAmount + "/" + blueprintSupply.amount;
            if (blueprint.type == EBlueprintType.AMMO)
            {
                if (blueprintSupply.hasAmount == 0 || blueprintSupply.amount == 0)
                {
                    sleekLabel3.TextColor = ESleekTint.BAD;
                }
            }
            else if (blueprintSupply.hasAmount < blueprintSupply.amount)
            {
                sleekLabel3.TextColor = ESleekTint.BAD;
            }
            num += itemAsset.size_x * 50 + 25;
            if (i < blueprint.supplies.Length - 1 || blueprint.tool != 0 || blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO)
            {
                sleekLabel.Text += " + ";
                ISleekImage sleekImage = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage.PositionOffset_X = num;
                sleekImage.PositionOffset_Y = -20f;
                sleekImage.PositionScale_Y = 0.5f;
                sleekImage.SizeOffset_X = 40f;
                sleekImage.SizeOffset_Y = 40f;
                sleekImage.TintColor = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage);
                num += 65;
            }
        }
        if (blueprint.tool != 0 && Assets.find(EAssetType.ITEM, blueprint.tool) is ItemAsset itemAsset2)
        {
            sleekLabel.Text += itemAsset2.itemName;
            SleekItemIcon sleekItemIcon2 = new SleekItemIcon
            {
                PositionOffset_X = num,
                PositionOffset_Y = -itemAsset2.size_y * 25,
                PositionScale_Y = 0.5f,
                SizeOffset_X = itemAsset2.size_x * 50,
                SizeOffset_Y = itemAsset2.size_y * 50
            };
            container.AddChild(sleekItemIcon2);
            sleekItemIcon2.Refresh(blueprint.tool, 100, itemAsset2.getState(), itemAsset2);
            ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
            sleekLabel5.PositionOffset_X = -100f;
            sleekLabel5.PositionOffset_Y = -30f;
            sleekLabel5.PositionScale_Y = 1f;
            sleekLabel5.SizeOffset_X = 100f;
            sleekLabel5.SizeOffset_Y = 30f;
            sleekLabel5.SizeScale_X = 1f;
            sleekLabel5.TextAlignment = TextAnchor.MiddleRight;
            sleekLabel5.Text = blueprint.tools + "/1";
            sleekLabel5.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon2.AddChild(sleekLabel5);
            sleekLabel.Text = sleekLabel.Text + " " + blueprint.tools + "/1";
            if (!blueprint.hasTool)
            {
                sleekLabel5.TextColor = ESleekTint.BAD;
            }
            num += itemAsset2.size_x * 50 + 25;
            if (blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO)
            {
                sleekLabel.Text += " + ";
                ISleekImage sleekImage2 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage2.PositionOffset_X = num;
                sleekImage2.PositionOffset_Y = -20f;
                sleekImage2.PositionScale_Y = 0.5f;
                sleekImage2.SizeOffset_X = 40f;
                sleekImage2.SizeOffset_Y = 40f;
                sleekImage2.TintColor = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage2);
                num += 65;
            }
        }
        if ((blueprint.type == EBlueprintType.REPAIR || blueprint.type == EBlueprintType.AMMO) && Assets.find(EAssetType.ITEM, blueprint.outputs[0].id) is ItemAsset itemAsset3)
        {
            sleekLabel.Text += itemAsset3.itemName;
            SleekItemIcon sleekItemIcon3 = new SleekItemIcon
            {
                PositionOffset_X = num,
                PositionOffset_Y = -itemAsset3.size_y * 25,
                PositionScale_Y = 0.5f,
                SizeOffset_X = itemAsset3.size_x * 50,
                SizeOffset_Y = itemAsset3.size_y * 50
            };
            container.AddChild(sleekItemIcon3);
            sleekItemIcon3.Refresh(blueprint.outputs[0].id, 100, itemAsset3.getState(), itemAsset3);
            ISleekLabel sleekLabel6 = Glazier.Get().CreateLabel();
            sleekLabel6.PositionOffset_X = -100f;
            sleekLabel6.PositionOffset_Y = -30f;
            sleekLabel6.PositionScale_Y = 1f;
            sleekLabel6.SizeOffset_X = 100f;
            sleekLabel6.SizeOffset_Y = 30f;
            sleekLabel6.SizeScale_X = 1f;
            sleekLabel6.TextAlignment = TextAnchor.MiddleRight;
            if (blueprint.type == EBlueprintType.REPAIR)
            {
                sleekLabel.Text = sleekLabel.Text + " " + blueprint.items + "%";
                sleekLabel6.Text = blueprint.items + "%";
                sleekLabel6.TextColor = ItemTool.getQualityColor((float)(int)blueprint.items / 100f);
            }
            else if (blueprint.type == EBlueprintType.AMMO)
            {
                ISleekLabel sleekLabel4 = sleekLabel;
                sleekLabel4.Text = sleekLabel4.Text + " " + blueprint.items + "/" + blueprint.products;
                sleekLabel6.Text = blueprint.items + "/" + itemAsset3.amount;
            }
            if (!blueprint.hasItem)
            {
                sleekLabel6.TextColor = ESleekTint.BAD;
            }
            sleekLabel6.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekItemIcon3.AddChild(sleekLabel6);
            num += itemAsset3.size_x * 50 + 25;
        }
        sleekLabel.Text += " = ";
        ISleekImage sleekImage3 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"));
        sleekImage3.PositionOffset_X = num;
        sleekImage3.PositionOffset_Y = -20f;
        sleekImage3.PositionScale_Y = 0.5f;
        sleekImage3.SizeOffset_X = 40f;
        sleekImage3.SizeOffset_Y = 40f;
        sleekImage3.TintColor = ESleekTint.FOREGROUND;
        container.AddChild(sleekImage3);
        num += 65;
        for (int j = 0; j < blueprint.outputs.Length; j++)
        {
            BlueprintOutput blueprintOutput = blueprint.outputs[j];
            if (!(Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset4))
            {
                continue;
            }
            sleekLabel.Text += itemAsset4.itemName;
            SleekItemIcon sleekItemIcon4 = new SleekItemIcon
            {
                PositionOffset_X = num,
                PositionOffset_Y = -itemAsset4.size_y * 25,
                PositionScale_Y = 0.5f,
                SizeOffset_X = itemAsset4.size_x * 50,
                SizeOffset_Y = itemAsset4.size_y * 50
            };
            container.AddChild(sleekItemIcon4);
            sleekItemIcon4.Refresh(blueprintOutput.id, 100, itemAsset4.getState(), itemAsset4);
            ISleekLabel sleekLabel7 = Glazier.Get().CreateLabel();
            sleekLabel7.PositionOffset_X = -100f;
            sleekLabel7.PositionOffset_Y = -30f;
            sleekLabel7.PositionScale_Y = 1f;
            sleekLabel7.SizeOffset_X = 100f;
            sleekLabel7.SizeOffset_Y = 30f;
            sleekLabel7.SizeScale_X = 1f;
            sleekLabel7.TextAlignment = TextAnchor.MiddleRight;
            sleekLabel7.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            if (blueprint.type == EBlueprintType.REPAIR)
            {
                sleekLabel.Text += " 100%";
                sleekLabel7.Text = "100%";
                sleekLabel7.TextColor = Palette.COLOR_G;
            }
            else if (blueprint.type == EBlueprintType.AMMO)
            {
                if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemAsset itemAsset5)
                {
                    ISleekLabel sleekLabel4 = sleekLabel;
                    sleekLabel4.Text = sleekLabel4.Text + " " + blueprint.products + "/" + itemAsset5.amount;
                    sleekLabel7.Text = blueprint.products + "/" + itemAsset5.amount;
                }
            }
            else
            {
                sleekLabel.Text = sleekLabel.Text + " x" + blueprintOutput.amount;
                sleekLabel7.Text = "x" + blueprintOutput.amount;
            }
            sleekItemIcon4.AddChild(sleekLabel7);
            num += itemAsset4.size_x * 50;
            if (j < blueprint.outputs.Length - 1)
            {
                num += 25;
                sleekLabel.Text += " + ";
                ISleekImage sleekImage4 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                sleekImage4.PositionOffset_X = num;
                sleekImage4.PositionOffset_Y = -20f;
                sleekImage4.PositionScale_Y = 0.5f;
                sleekImage4.SizeOffset_X = 40f;
                sleekImage4.SizeOffset_Y = 40f;
                sleekImage4.TintColor = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage4);
                num += 65;
            }
        }
        container.PositionOffset_X = -num / 2;
        container.SizeOffset_X = num;
        if (!supportsDepth)
        {
            craftButton = Glazier.Get().CreateButton();
            craftButton.PositionOffset_X = -70f;
            craftButton.PositionOffset_Y = -35f;
            craftButton.PositionScale_X = 0.75f;
            craftButton.PositionScale_Y = 1f;
            craftButton.SizeOffset_X = 140f;
            craftButton.SizeOffset_Y = 30f;
            craftButton.Text = PlayerDashboardCraftingUI.localization.format("Craft");
            craftButton.TextColor = sleekLabel.TextColor;
            craftButton.OnClicked += onClickedButton;
            AddChild(craftButton);
            craftAllButton = Glazier.Get().CreateButton();
            craftAllButton.PositionOffset_X = -70f;
            craftAllButton.PositionOffset_Y = -35f;
            craftAllButton.PositionScale_X = 0.25f;
            craftAllButton.PositionScale_Y = 1f;
            craftAllButton.SizeOffset_X = 140f;
            craftAllButton.SizeOffset_Y = 30f;
            craftAllButton.Text = PlayerDashboardCraftingUI.localization.format("Craft_All");
            craftAllButton.TextColor = sleekLabel.TextColor;
            craftAllButton.OnClicked += onClickedAltButton;
            AddChild(craftAllButton);
        }
        ignoreToggleButton = Glazier.Get().CreateToggle();
        ignoreToggleButton.PositionOffset_X = -50f;
        ignoreToggleButton.PositionOffset_Y = -40f;
        ignoreToggleButton.PositionScale_X = 1f;
        ignoreToggleButton.PositionScale_Y = 1f;
        ignoreToggleButton.SizeOffset_X = 40f;
        ignoreToggleButton.SizeOffset_Y = 40f;
        ignoreToggleButton.OnValueChanged += onToggledIgnoring;
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
