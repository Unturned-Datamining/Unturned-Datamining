using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class SleekBlueprint : SleekWrapper
{
    internal delegate void Clicked(BlueprintStatus blueprintStatus);

    private BlueprintStatus blueprintStatus;

    private ISleekButton backgroundButton;

    private ISleekLabel titleLabel;

    private ISleekLabel descriptionLabel;

    private ISleekElement formulaImagesContainer;

    private ISleekElement formulaLabelsContainer;

    private ISleekImage preferencesIcon;

    private List<SleekItemIcon> pooledItemIcons = new List<SleekItemIcon>();

    private List<ISleekImage> pooledImages = new List<ISleekImage>();

    private List<ISleekLabel> pooledLabels = new List<ISleekLabel>();

    private static StringBuilder inputItemsSb = new StringBuilder();

    private static StringBuilder titleSb = new StringBuilder();

    private static StringBuilder descSb = new StringBuilder();

    private static StringBuilder tooltipSb = new StringBuilder();

    public Blueprint blueprint => blueprintStatus.blueprint;

    internal event Clicked OnClickedBlueprint;

    private void RefreshPreferencesAndTooltip()
    {
        EBlueprintPreferences blueprintPreferences = PlayerCrafting.GetBlueprintPreferences(blueprint);
        if (blueprintPreferences != 0)
        {
            string name = null;
            switch (blueprintPreferences)
            {
            case EBlueprintPreferences.Ignored:
                name = "BlueprintHiddenIcon";
                break;
            case EBlueprintPreferences.Favorited:
                name = "FavoriteBlueprintIcon";
                break;
            }
            preferencesIcon.Texture = PlayerDashboardCraftingUI.icons.load<Texture2D>(name);
            preferencesIcon.IsVisible = true;
        }
        else
        {
            preferencesIcon.IsVisible = false;
        }
        Local localization = PlayerDashboardCraftingUI.localization;
        tooltipSb.Clear();
        tooltipSb.AppendLine(titleLabel.Text);
        TagAsset categoryTag = blueprint.GetCategoryTag();
        if (categoryTag != null)
        {
            tooltipSb.AppendFormat(localization.format("BlueprintCategoryLabel"), categoryTag.RichTextOrPreferredFontColor);
            tooltipSb.AppendLine();
        }
        tooltipSb.AppendLine();
        if (blueprintPreferences != EBlueprintPreferences.Ignored && blueprintStatus.IsCraftable)
        {
            string keyCodeText = MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu);
            if (OptionsSettings.ShouldClickBlueprintToCraft)
            {
                string format = localization.format("InvertSkipCraftingTooltip");
                tooltipSb.AppendFormat(format, keyCodeText);
            }
            else
            {
                string format2 = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_SkipCraftingTooltip");
                tooltipSb.AppendFormat(format2, keyCodeText);
            }
            tooltipSb.AppendLine();
            string keyCodeText2 = MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other);
            tooltipSb.AppendFormat(PlayerDashboardInventoryUI.localization.format("ActionBlueprint_CraftAllTooltip"), keyCodeText2);
        }
        else
        {
            PlayerDashboardCraftingUI.BuildNotCraftableTooltip(tooltipSb, blueprintStatus, blueprintPreferences);
        }
        backgroundButton.TooltipText = tooltipSb.ToString();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged = (System.Action)Delegate.Remove(PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged, new System.Action(RefreshPreferencesAndTooltip));
    }

    internal SleekBlueprint()
    {
        backgroundButton = Glazier.Get().CreateButton();
        backgroundButton.SizeScale_X = 1f;
        backgroundButton.SizeScale_Y = 1f;
        backgroundButton.OnClicked += onClickedBackgroundButton;
        AddChild(backgroundButton);
        titleLabel = Glazier.Get().CreateLabel();
        titleLabel.PositionOffset_X = 5f;
        titleLabel.PositionOffset_Y = 5f;
        titleLabel.SizeOffset_X = -10f;
        titleLabel.SizeOffset_Y = 30f;
        titleLabel.SizeScale_X = 1f;
        titleLabel.AllowRichText = true;
        titleLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        titleLabel.FontSize = ESleekFontSize.Medium;
        AddChild(titleLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.PositionOffset_X = 5f;
        descriptionLabel.PositionOffset_Y = -35f;
        descriptionLabel.PositionScale_Y = 1f;
        descriptionLabel.SizeOffset_X = -10f;
        descriptionLabel.SizeOffset_Y = 30f;
        descriptionLabel.SizeScale_X = 1f;
        descriptionLabel.AllowRichText = true;
        descriptionLabel.FontSize = ESleekFontSize.Medium;
        descriptionLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(descriptionLabel);
        formulaImagesContainer = Glazier.Get().CreateFrame();
        formulaImagesContainer.PositionScale_X = 0.5f;
        formulaImagesContainer.SizeScale_Y = 1f;
        AddChild(formulaImagesContainer);
        formulaLabelsContainer = Glazier.Get().CreateFrame();
        formulaLabelsContainer.PositionScale_X = 0.5f;
        formulaLabelsContainer.SizeScale_Y = 1f;
        AddChild(formulaLabelsContainer);
        preferencesIcon = Glazier.Get().CreateImage();
        preferencesIcon.PositionOffset_X = -50f;
        preferencesIcon.PositionOffset_Y = -40f;
        preferencesIcon.PositionScale_X = 1f;
        preferencesIcon.PositionScale_Y = 1f;
        preferencesIcon.SizeOffset_X = 40f;
        preferencesIcon.SizeOffset_Y = 40f;
        preferencesIcon.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
        AddChild(preferencesIcon);
        PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged = (System.Action)Delegate.Combine(PlayerCrafting.OnLocalPlayerBlueprintPreferencesChanged, new System.Action(RefreshPreferencesAndTooltip));
    }

    internal void SetBlueprintStatus(BlueprintStatus blueprintStatus)
    {
        this.blueprintStatus = blueprintStatus;
        backgroundButton.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, blueprintStatus.IsCraftable ? 1f : 0.5f);
        Local localization = PlayerDashboardCraftingUI.localization;
        inputItemsSb.Clear();
        titleSb.Clear();
        int i = 0;
        int j = 0;
        int k = 0;
        CachingAssetRef[] applicableRequiredNearbyCraftingTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
        if (blueprint.RequiresSkill || applicableRequiredNearbyCraftingTags != null)
        {
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
                for (int l = 0; l < applicableRequiredNearbyCraftingTags.Length; l++)
                {
                    TagAsset tagAsset = applicableRequiredNearbyCraftingTags[l].Get<TagAsset>();
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
            descriptionLabel.Text = PlayerDashboardCraftingUI.localization.format("Requirements_Label", descSb.ToString());
            descriptionLabel.IsVisible = true;
        }
        else
        {
            descriptionLabel.IsVisible = false;
        }
        float num2 = 0f;
        for (int m = 0; m < blueprint.supplies.Length; m++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[m];
            ItemAsset itemAsset = blueprintSupply.FindItemAsset();
            if (itemAsset != null)
            {
                BlueprintInputItemStatus blueprintInputItemStatus = blueprintStatus.inputItems[m];
                SleekItemIcon sleekItemIcon = CreateItemIcon(itemAsset, ref i);
                sleekItemIcon.PositionOffset_X = num2;
                byte[] array = blueprintInputItemStatus.FirstItemOrNull?.state;
                if (array == null)
                {
                    array = itemAsset.getState(isFull: false);
                }
                sleekItemIcon.Refresh(itemAsset.id, 100, array, itemAsset, Mathf.RoundToInt(sleekItemIcon.SizeOffset_X), Mathf.RoundToInt(sleekItemIcon.SizeOffset_Y));
                ISleekLabel sleekLabel = CreateLabel(ref k);
                sleekLabel.PositionOffset_X = sleekItemIcon.PositionOffset_X - 100f;
                sleekLabel.PositionOffset_Y = sleekItemIcon.PositionOffset_Y;
                sleekLabel.PositionScale_Y = sleekItemIcon.PositionScale_Y;
                sleekLabel.SizeOffset_X = sleekItemIcon.SizeOffset_X + 100f;
                sleekLabel.SizeOffset_Y = sleekItemIcon.SizeOffset_Y;
                sleekLabel.AllowRichText = false;
                if (blueprint.Operation == EBlueprintOperation.FillTargetItem && m == 0)
                {
                    sleekLabel.TextColor = ESleekTint.FONT;
                    sleekLabel.Text = $"x{blueprintInputItemStatus.totalAmount}";
                }
                else
                {
                    sleekLabel.TextColor = (blueprintInputItemStatus.isMissingRequiredAmount ? ESleekTint.BAD : ESleekTint.FONT);
                    sleekLabel.Text = PlayerDashboardCraftingUI.localization.format("BlueprintAmountLabel", blueprintInputItemStatus.totalAmount, blueprintSupply.amount);
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
                if (m != blueprint.supplies.Length - 1)
                {
                    inputItemsSb.Append(localization.format("BlueprintTitle_ItemSeparator"));
                    Texture2D texture = PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus");
                    ISleekImage sleekImage = CreateImage(texture, ref j);
                    sleekImage.PositionOffset_X = num2;
                    num2 += sleekImage.SizeOffset_X;
                    num2 += 5f;
                }
            }
        }
        if (blueprint.TargetItem != null)
        {
            Texture2D texture2 = PlayerDashboardCraftingUI.icons.load<Texture2D>("Arrow");
            ISleekImage sleekImage2 = CreateImage(texture2, ref j);
            sleekImage2.PositionOffset_X = num2;
            num2 += sleekImage2.SizeOffset_X;
            num2 += 5f;
            _ = blueprint.TargetItem;
            ItemAsset itemAsset2 = blueprint.TargetItem.FindItemAsset();
            if (itemAsset2 != null)
            {
                BlueprintInputItemStatus targetStatus = blueprintStatus.targetStatus;
                SleekItemIcon sleekItemIcon2 = CreateItemIcon(itemAsset2, ref i);
                sleekItemIcon2.PositionOffset_X = num2;
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
                sleekItemIcon2.Refresh(itemAsset2.id, 100, array2, itemAsset2, Mathf.RoundToInt(sleekItemIcon2.SizeOffset_X), Mathf.RoundToInt(sleekItemIcon2.SizeOffset_Y));
                ISleekLabel sleekLabel2 = CreateLabel(ref k);
                sleekLabel2.PositionOffset_X = sleekItemIcon2.PositionOffset_X - 100f;
                sleekLabel2.PositionOffset_Y = sleekItemIcon2.PositionOffset_Y;
                sleekLabel2.PositionScale_Y = sleekItemIcon2.PositionScale_Y;
                sleekLabel2.SizeOffset_X = sleekItemIcon2.SizeOffset_X + 100f;
                sleekLabel2.SizeOffset_Y = sleekItemIcon2.SizeOffset_Y;
                sleekLabel2.TextColor = ESleekTint.FOREGROUND;
                sleekLabel2.AllowRichText = true;
                if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
                {
                    int num4 = 100 - b;
                    Color qualityColor = ItemTool.getQualityColor((float)(int)b / 100f);
                    string arg3 = RichTextUtil.wrapWithColor($"{b}%", qualityColor);
                    sleekLabel2.Text = RichTextUtil.wrapWithColor($"{b} +{num4}%", qualityColor);
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
                    sleekLabel2.Text = $"x{num3} +{num5}";
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
            ISleekImage sleekImage3 = CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Equals"), ref j);
            sleekImage3.PositionOffset_X = num2;
            num2 += sleekImage3.SizeOffset_X;
            num2 += 5f;
            for (int n = 0; n < blueprint.outputs.Length; n++)
            {
                BlueprintOutput blueprintOutput = blueprint.outputs[n];
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
                SleekItemIcon sleekItemIcon3 = CreateItemIcon(itemAsset3, ref i);
                sleekItemIcon3.PositionOffset_X = num2;
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
                sleekItemIcon3.Refresh(itemAsset3.id, quality, state, itemAsset3, Mathf.RoundToInt(sleekItemIcon3.SizeOffset_X), Mathf.RoundToInt(sleekItemIcon3.SizeOffset_Y));
                if (blueprintOutput.amount > 1 || quality != 100)
                {
                    ISleekLabel sleekLabel3 = CreateLabel(ref k);
                    sleekLabel3.PositionOffset_X = sleekItemIcon3.PositionOffset_X - 100f;
                    sleekLabel3.PositionOffset_Y = sleekItemIcon3.PositionOffset_Y;
                    sleekLabel3.PositionScale_Y = sleekItemIcon3.PositionScale_Y;
                    sleekLabel3.SizeOffset_X = sleekItemIcon3.SizeOffset_X + 100f;
                    sleekLabel3.SizeOffset_Y = sleekItemIcon3.SizeOffset_Y;
                    sleekLabel3.AllowRichText = true;
                    sleekLabel3.TextColor = ESleekTint.FOREGROUND;
                    string text = string.Empty;
                    if (quality != 100)
                    {
                        Color qualityColor2 = ItemTool.getQualityColor((float)(int)quality / 100f);
                        text = $"<color={Palette.hex(qualityColor2)}>{quality}%</color>";
                    }
                    if (blueprintOutput.amount > 1)
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            text += "\n";
                        }
                        text += $"x{blueprintOutput.amount}";
                    }
                    sleekLabel3.Text = text;
                }
                num2 += sleekItemIcon3.SizeOffset_X;
                num2 += 5f;
                if (n < blueprint.outputs.Length - 1)
                {
                    titleSb.Append(localization.format("BlueprintTitle_ItemSeparator"));
                    ISleekImage sleekImage4 = CreateImage(PlayerDashboardCraftingUI.icons.load<Texture2D>("Plus"), ref j);
                    sleekImage4.PositionOffset_X = num2;
                    num2 += sleekImage4.SizeOffset_X;
                    num2 += 5f;
                }
            }
        }
        string text2 = titleSb.ToString();
        titleLabel.Text = text2;
        num2 -= 5f;
        formulaLabelsContainer.PositionOffset_X = (0f - num2) / 2f;
        formulaLabelsContainer.SizeOffset_X = num2;
        formulaImagesContainer.PositionOffset_X = (0f - num2) / 2f;
        formulaImagesContainer.SizeOffset_X = num2;
        RefreshPreferencesAndTooltip();
        for (; i < pooledItemIcons.Count; i++)
        {
            pooledItemIcons[i].IsVisible = false;
        }
        for (; j < pooledImages.Count; j++)
        {
            pooledImages[j].IsVisible = false;
        }
        for (; k < pooledLabels.Count; k++)
        {
            pooledLabels[k].IsVisible = false;
        }
    }

    private void onClickedBackgroundButton(ISleekElement internalButton)
    {
        this.OnClickedBlueprint?.Invoke(blueprintStatus);
    }

    private SleekItemIcon CreateItemIcon(ItemAsset asset, ref int index)
    {
        float num;
        float sizeOffset_X;
        if (asset.size_y > 2)
        {
            num = 100f;
            sizeOffset_X = num * ((float)(int)asset.size_x / (float)(int)asset.size_y);
        }
        else
        {
            sizeOffset_X = (float)(int)asset.size_x * 50f;
            num = (float)(int)asset.size_y * 50f;
        }
        SleekItemIcon sleekItemIcon;
        if (index < pooledItemIcons.Count)
        {
            sleekItemIcon = pooledItemIcons[index];
            sleekItemIcon.IsVisible = true;
            sleekItemIcon.Clear();
        }
        else
        {
            sleekItemIcon = new SleekItemIcon();
            sleekItemIcon.PositionScale_Y = 0.5f;
            pooledItemIcons.Add(sleekItemIcon);
            formulaImagesContainer.AddChild(sleekItemIcon);
        }
        index++;
        sleekItemIcon.PositionOffset_Y = (0f - num) / 2f;
        sleekItemIcon.SizeOffset_X = sizeOffset_X;
        sleekItemIcon.SizeOffset_Y = num;
        return sleekItemIcon;
    }

    private ISleekImage CreateImage(Texture2D texture, ref int index)
    {
        ISleekImage sleekImage;
        if (index < pooledImages.Count)
        {
            sleekImage = pooledImages[index];
            sleekImage.IsVisible = true;
            sleekImage.Texture = texture;
        }
        else
        {
            sleekImage = Glazier.Get().CreateImage(texture);
            sleekImage.PositionOffset_Y = -10f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekImage.TintColor = ESleekTint.FOREGROUND;
            pooledImages.Add(sleekImage);
            formulaImagesContainer.AddChild(sleekImage);
        }
        index++;
        return sleekImage;
    }

    private ISleekLabel CreateLabel(ref int index)
    {
        ISleekLabel sleekLabel;
        if (index < pooledLabels.Count)
        {
            sleekLabel = pooledLabels[index];
            sleekLabel.IsVisible = true;
        }
        else
        {
            sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.TextAlignment = TextAnchor.LowerRight;
            sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            pooledLabels.Add(sleekLabel);
            formulaLabelsContainer.AddChild(sleekLabel);
        }
        index++;
        return sleekLabel;
    }
}
