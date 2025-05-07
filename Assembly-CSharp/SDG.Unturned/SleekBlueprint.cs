using System;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class SleekBlueprint : SleekWrapper
{
    internal delegate void Clicked(BlueprintStatus blueprintStatus);

    private BlueprintStatus blueprintStatus;

    private ISleekButton backgroundButton;

    private ISleekElement container;

    private ISleekImage ignoredIcon;

    private static StringBuilder inputItemsSb = new StringBuilder();

    private static StringBuilder titleSb = new StringBuilder();

    private static StringBuilder descSb = new StringBuilder();

    public Blueprint blueprint => blueprintStatus.blueprint;

    internal event Clicked OnClickedBlueprint;

    private void RefreshIsIgnored()
    {
        ignoredIcon.IsVisible = Player.player.crafting.getIgnoringBlueprint(blueprint);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerCrafting.OnLocalPlayerIgnoredBlueprintsChanged = (System.Action)Delegate.Remove(PlayerCrafting.OnLocalPlayerIgnoredBlueprintsChanged, new System.Action(RefreshIsIgnored));
    }

    internal SleekBlueprint(BlueprintStatus blueprintStatus)
    {
        this.blueprintStatus = blueprintStatus;
        Local localization = PlayerDashboardCraftingUI.localization;
        backgroundButton = Glazier.Get().CreateButton();
        backgroundButton.SizeScale_X = 1f;
        backgroundButton.SizeScale_Y = 1f;
        backgroundButton.OnClicked += onClickedBackgroundButton;
        AddChild(backgroundButton);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 5f;
        sleekLabel.PositionOffset_Y = 5f;
        sleekLabel.SizeOffset_X = -10f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.AllowRichText = true;
        sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        AddChild(sleekLabel);
        inputItemsSb.Clear();
        titleSb.Clear();
        CachingAssetRef[] applicableRequiredNearbyCraftingTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
        if (blueprint.RequiresSkill || applicableRequiredNearbyCraftingTags != null)
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.PositionOffset_X = 5f;
            sleekLabel2.PositionOffset_Y = -35f;
            sleekLabel2.PositionScale_Y = 1f;
            sleekLabel2.SizeOffset_X = -10f;
            sleekLabel2.SizeOffset_Y = 30f;
            sleekLabel2.SizeScale_X = 1f;
            descSb.Clear();
            if (blueprint.RequiresSkill)
            {
                int skillSpecialityIndex = blueprint.SkillSpecialityIndex;
                int skillIndex = blueprint.SkillIndex;
                bool num = Player.player.skills.skills[skillSpecialityIndex][skillIndex].level >= blueprint.level;
                Local localization2 = PlayerDashboardSkillsUI.localization;
                string arg = localization2.format("Speciality_" + skillSpecialityIndex + "_Skill_" + skillIndex);
                string arg2 = localization2.format("Level_" + blueprint.level);
                string value = PlayerDashboardCraftingUI.localization.format("Requirements_Skill", arg, arg2);
                Color color = new SleekColor(num ? ESleekTint.FONT : ESleekTint.BAD).Get();
                descSb.Append("<color=");
                descSb.Append(Palette.hex(color));
                descSb.Append('>');
                descSb.Append(value);
                descSb.Append("</color>");
            }
            if (applicableRequiredNearbyCraftingTags != null)
            {
                for (int i = 0; i < applicableRequiredNearbyCraftingTags.Length; i++)
                {
                    TagAsset tagAsset = applicableRequiredNearbyCraftingTags[i].Get<TagAsset>();
                    if (tagAsset != null)
                    {
                        if (descSb.Length > 0)
                        {
                            descSb.Append(PlayerDashboardCraftingUI.localization.format("Requirements_Separator"));
                        }
                        if (Player.player.crafting.IsCraftingTagAvailable(tagAsset))
                        {
                            descSb.Append(tagAsset.RichTextOrPreferredFontColor);
                            continue;
                        }
                        descSb.Append("<color=");
                        descSb.Append(Palette.hex(OptionsSettings.badColor));
                        descSb.Append('>');
                        descSb.Append(tagAsset.PlainTextName);
                        descSb.Append("</color>");
                    }
                }
            }
            sleekLabel2.AllowRichText = true;
            sleekLabel2.Text = PlayerDashboardCraftingUI.localization.format("Requirements_Label", descSb.ToString());
            sleekLabel2.FontSize = ESleekFontSize.Medium;
            sleekLabel2.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            AddChild(sleekLabel2);
        }
        container = Glazier.Get().CreateFrame();
        container.PositionOffset_Y = 40f;
        container.PositionScale_X = 0.5f;
        container.SizeOffset_Y = -45f;
        container.SizeScale_Y = 1f;
        AddChild(container);
        float num2 = 0f;
        for (int j = 0; j < blueprint.supplies.Length; j++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[j];
            ItemAsset itemAsset = blueprintSupply.FindItemAsset();
            if (itemAsset == null)
            {
                continue;
            }
            BlueprintInputItemStatus blueprintInputItemStatus = blueprintStatus.inputItems[j];
            SleekItemIcon sleekItemIcon = new SleekItemIcon
            {
                PositionOffset_X = num2,
                PositionOffset_Y = -itemAsset.size_y * 25,
                PositionScale_Y = 0.5f,
                SizeOffset_X = itemAsset.size_x * 50,
                SizeOffset_Y = itemAsset.size_y * 50
            };
            container.AddChild(sleekItemIcon);
            byte[] array = blueprintInputItemStatus.FirstItemOrNull?.state;
            if (array == null)
            {
                array = itemAsset.getState(isFull: false);
            }
            sleekItemIcon.Refresh(itemAsset.id, 100, array, itemAsset);
            string text = null;
            ESleekTint eSleekTint = ESleekTint.FONT;
            if (blueprint.Operation == EBlueprintOperation.FillTargetItem && j == 0)
            {
                text = $"x{blueprintInputItemStatus.totalAmount}";
            }
            else if (blueprintInputItemStatus.isMissingRequiredAmount)
            {
                eSleekTint = ESleekTint.BAD;
                text = PlayerDashboardCraftingUI.localization.format("BlueprintAmountLabel", blueprintInputItemStatus.totalAmount, blueprintSupply.amount);
            }
            else if (blueprintSupply.amount > 1)
            {
                text = $"x{blueprintSupply.amount}";
            }
            if (!string.IsNullOrEmpty(text))
            {
                ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
                sleekLabel3.SizeOffset_X = 100f;
                sleekLabel3.SizeOffset_Y = 30f;
                if (itemAsset.size_y > 2)
                {
                    sleekLabel3.PositionOffset_X = -50f;
                    sleekLabel3.PositionOffset_Y = -15f;
                    sleekLabel3.PositionScale_X = 0.5f;
                    sleekLabel3.PositionScale_Y = 0.5f;
                    sleekLabel3.TextAlignment = TextAnchor.MiddleCenter;
                }
                else
                {
                    sleekLabel3.PositionOffset_X = -100f;
                    sleekLabel3.PositionOffset_Y = -30f;
                    sleekLabel3.PositionScale_X = 1f;
                    sleekLabel3.PositionScale_Y = 1f;
                    sleekLabel3.TextAlignment = TextAnchor.LowerRight;
                }
                sleekLabel3.Text = text;
                sleekLabel3.TextColor = eSleekTint;
                sleekLabel3.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
                sleekItemIcon.AddChild(sleekLabel3);
            }
            inputItemsSb.Append(itemAsset.RarityRichTextName);
            if (blueprintSupply.amount > 1)
            {
                inputItemsSb.Append(" x");
                inputItemsSb.Append(blueprintSupply.amount);
            }
            if (!blueprintSupply.ShouldConsume)
            {
                inputItemsSb.Append(' ');
                inputItemsSb.Append(localization.format("BlueprintTitle_ToolItem"));
            }
            num2 += sleekItemIcon.SizeOffset_X;
            num2 += 5f;
            if (j != blueprint.supplies.Length - 1)
            {
                inputItemsSb.Append(localization.format("BlueprintTitle_ItemSeparator"));
                Texture2D texture = PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus");
                ISleekImage sleekImage = Glazier.Get().CreateImage(texture);
                sleekImage.PositionOffset_X = num2;
                sleekImage.PositionOffset_Y = -10f;
                sleekImage.PositionScale_Y = 0.5f;
                sleekImage.SizeOffset_X = 20f;
                sleekImage.SizeOffset_Y = 20f;
                sleekImage.TintColor = ESleekTint.FOREGROUND;
                container.AddChild(sleekImage);
                num2 += sleekImage.SizeOffset_X;
                num2 += 5f;
            }
        }
        if (blueprint.TargetItem != null)
        {
            Texture2D texture2 = PlayerDashboardCraftingUI.icons.load<Texture2D>("Arrow");
            ISleekImage sleekImage2 = Glazier.Get().CreateImage(texture2);
            sleekImage2.PositionOffset_X = num2;
            sleekImage2.PositionOffset_Y = -10f;
            sleekImage2.PositionScale_Y = 0.5f;
            sleekImage2.SizeOffset_X = 20f;
            sleekImage2.SizeOffset_Y = 20f;
            sleekImage2.TintColor = ESleekTint.FOREGROUND;
            container.AddChild(sleekImage2);
            num2 += sleekImage2.SizeOffset_X;
            num2 += 5f;
            _ = blueprint.TargetItem;
            ItemAsset itemAsset2 = blueprint.TargetItem.FindItemAsset();
            if (itemAsset2 != null)
            {
                BlueprintInputItemStatus targetStatus = blueprintStatus.targetStatus;
                SleekItemIcon sleekItemIcon2 = new SleekItemIcon
                {
                    PositionOffset_X = num2,
                    PositionOffset_Y = -itemAsset2.size_y * 25,
                    PositionScale_Y = 0.5f,
                    SizeOffset_X = itemAsset2.size_x * 50,
                    SizeOffset_Y = itemAsset2.size_y * 50
                };
                container.AddChild(sleekItemIcon2);
                num2 += sleekItemIcon2.SizeOffset_X;
                num2 += 5f;
                byte[] array2 = null;
                byte b = 0;
                int num3 = 0;
                Item firstItemOrNull = targetStatus.FirstItemOrNull;
                if (firstItemOrNull != null)
                {
                    array2 = firstItemOrNull.state;
                    b = firstItemOrNull.quality;
                    num3 = firstItemOrNull.amount;
                }
                if (array2 == null)
                {
                    array2 = itemAsset2.getState(isFull: false);
                }
                sleekItemIcon2.Refresh(itemAsset2.id, 100, array2, itemAsset2);
                ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
                sleekLabel4.SizeOffset_X = 100f;
                sleekLabel4.SizeOffset_Y = 30f;
                if (itemAsset2.size_y > 2)
                {
                    sleekLabel4.PositionOffset_X = -50f;
                    sleekLabel4.PositionOffset_Y = -15f;
                    sleekLabel4.PositionScale_X = 0.5f;
                    sleekLabel4.PositionScale_Y = 0.5f;
                    sleekLabel4.TextAlignment = TextAnchor.MiddleCenter;
                }
                else
                {
                    sleekLabel4.PositionOffset_X = -100f;
                    sleekLabel4.PositionOffset_Y = -30f;
                    sleekLabel4.PositionScale_X = 1f;
                    sleekLabel4.PositionScale_Y = 1f;
                    sleekLabel4.TextAlignment = TextAnchor.LowerRight;
                }
                sleekLabel4.AllowRichText = true;
                sleekLabel4.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
                sleekItemIcon2.AddChild(sleekLabel4);
                if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
                {
                    int num4 = 100 - b;
                    Color qualityColor = ItemTool.getQualityColor((float)(int)b / 100f);
                    string arg3 = RichTextUtil.wrapWithColor($"{b}%", qualityColor);
                    sleekLabel4.Text = RichTextUtil.wrapWithColor($"{b} +{num4}%", qualityColor);
                    titleSb.Append(localization.format("BlueprintTitle_OperationRepair", itemAsset2.RarityRichTextName, arg3, inputItemsSb));
                }
                else if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
                {
                    int a = itemAsset2.MaxAmount - num3;
                    int b2 = 0;
                    if (blueprintStatus.inputItems.Count > 0)
                    {
                        b2 = blueprintStatus.inputItems[0].totalAmount;
                    }
                    int num5 = Mathf.Min(a, b2);
                    sleekLabel4.Text = $"x{num3} +{num5}";
                    titleSb.Append(localization.format("BlueprintTitle_OperationFill", itemAsset2.RarityRichTextName, num5, inputItemsSb));
                }
            }
        }
        if (titleSb.Length < 1)
        {
            titleSb.Append(inputItemsSb);
        }
        if (blueprint.outputs != null && blueprint.outputs.Length != 0)
        {
            titleSb.Append(localization.format("BlueprintTitle_OutputSeparator"));
            ISleekImage sleekImage3 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"));
            sleekImage3.PositionOffset_X = num2;
            sleekImage3.PositionOffset_Y = -10f;
            sleekImage3.PositionScale_Y = 0.5f;
            sleekImage3.SizeOffset_X = 20f;
            sleekImage3.SizeOffset_Y = 20f;
            sleekImage3.TintColor = ESleekTint.FOREGROUND;
            container.AddChild(sleekImage3);
            num2 += sleekImage3.SizeOffset_X;
            num2 += 5f;
            for (int k = 0; k < blueprint.outputs.Length; k++)
            {
                BlueprintOutput blueprintOutput = blueprint.outputs[k];
                ItemAsset itemAsset3 = blueprintOutput.FindItemAsset();
                if (itemAsset3 == null)
                {
                    continue;
                }
                titleSb.Append(itemAsset3.RarityRichTextName);
                if (blueprintOutput.amount > 1)
                {
                    titleSb.Append(" x");
                    titleSb.Append(blueprintOutput.amount);
                }
                SleekItemIcon sleekItemIcon3 = new SleekItemIcon
                {
                    PositionOffset_X = num2,
                    PositionOffset_Y = -itemAsset3.size_y * 25,
                    PositionScale_Y = 0.5f,
                    SizeOffset_X = itemAsset3.size_x * 50,
                    SizeOffset_Y = itemAsset3.size_y * 50
                };
                container.AddChild(sleekItemIcon3);
                byte quality;
                byte[] state;
                if (blueprint.transferState)
                {
                    blueprintStatus.GetPreviewOutputTransferState(itemAsset3, out quality, out state);
                }
                else
                {
                    quality = 100;
                    state = itemAsset3.getState();
                }
                sleekItemIcon3.Refresh(itemAsset3.id, quality, state, itemAsset3);
                if (blueprintOutput.amount > 1 || quality != 100)
                {
                    ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
                    sleekLabel5.SizeOffset_X = 100f;
                    sleekLabel5.SizeOffset_Y = 60f;
                    if (itemAsset3.size_y > 2)
                    {
                        sleekLabel5.PositionOffset_X = -50f;
                        sleekLabel5.PositionOffset_Y = -30f;
                        sleekLabel5.PositionScale_X = 0.5f;
                        sleekLabel5.PositionScale_Y = 0.5f;
                        sleekLabel5.TextAlignment = TextAnchor.MiddleCenter;
                    }
                    else
                    {
                        sleekLabel5.PositionOffset_X = -100f;
                        sleekLabel5.PositionOffset_Y = -60f;
                        sleekLabel5.PositionScale_X = 1f;
                        sleekLabel5.PositionScale_Y = 1f;
                        sleekLabel5.TextAlignment = TextAnchor.LowerRight;
                    }
                    sleekLabel5.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
                    sleekLabel5.AllowRichText = true;
                    string text2 = string.Empty;
                    if (quality != 100)
                    {
                        Color qualityColor2 = ItemTool.getQualityColor((float)(int)quality / 100f);
                        text2 = $"<color={Palette.hex(qualityColor2)}>{quality}%</color>";
                    }
                    if (blueprintOutput.amount > 1)
                    {
                        if (!string.IsNullOrEmpty(text2))
                        {
                            text2 += "\n";
                        }
                        text2 += $"x{blueprintOutput.amount}";
                    }
                    sleekLabel5.Text = text2;
                    sleekItemIcon3.AddChild(sleekLabel5);
                }
                num2 += sleekItemIcon3.SizeOffset_X;
                num2 += 5f;
                if (k < blueprint.outputs.Length - 1)
                {
                    titleSb.Append(localization.format("BlueprintTitle_ItemSeparator"));
                    ISleekImage sleekImage4 = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"));
                    sleekImage4.PositionOffset_X = num2;
                    sleekImage4.PositionOffset_Y = -10f;
                    sleekImage4.PositionScale_Y = 0.5f;
                    sleekImage4.SizeOffset_X = 20f;
                    sleekImage4.SizeOffset_Y = 20f;
                    sleekImage4.TintColor = ESleekTint.FOREGROUND;
                    container.AddChild(sleekImage4);
                    num2 += sleekImage4.SizeOffset_X;
                    num2 += 5f;
                }
            }
        }
        string text4 = (sleekLabel.Text = titleSb.ToString());
        if (blueprintStatus.IsCraftable)
        {
            string text5 = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_SkipCraftingTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu));
            string text6 = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_CraftAllTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            backgroundButton.TooltipText = text4 + "\n\n" + text5 + "\n" + text6;
        }
        else
        {
            backgroundButton.TooltipText = text4 + "\n\n" + PlayerDashboardCraftingUI.BuildNotCraftableTooltip(blueprintStatus);
        }
        num2 -= 5f;
        container.PositionOffset_X = (0f - num2) / 2f;
        container.SizeOffset_X = num2;
        ignoredIcon = Glazier.Get().CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("BlueprintHiddenIcon"));
        ignoredIcon.PositionOffset_X = -50f;
        ignoredIcon.PositionOffset_Y = -40f;
        ignoredIcon.PositionScale_X = 1f;
        ignoredIcon.PositionScale_Y = 1f;
        ignoredIcon.SizeOffset_X = 40f;
        ignoredIcon.SizeOffset_Y = 40f;
        ignoredIcon.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
        AddChild(ignoredIcon);
        PlayerCrafting.OnLocalPlayerIgnoredBlueprintsChanged = (System.Action)Delegate.Combine(PlayerCrafting.OnLocalPlayerIgnoredBlueprintsChanged, new System.Action(RefreshIsIgnored));
        RefreshIsIgnored();
    }

    private void onClickedBackgroundButton(ISleekElement internalButton)
    {
        this.OnClickedBlueprint?.Invoke(blueprintStatus);
    }
}
