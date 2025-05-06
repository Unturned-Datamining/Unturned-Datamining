using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class SleekSelectedBlueprint : SleekWrapper
{
    private ItemAsset currentPrimaryItemAsset;

    private BlueprintStatus status;

    private ISleekScrollView detailScrollView;

    private SleekButtonIcon craftButton;

    private SleekButtonState visibilityButton;

    private ISleekElement summaryContainer;

    private ISleekLabel titleLabel;

    private SleekItemIcon primaryItemIcon;

    private ISleekLabel descriptionLabel;

    private ISleekElement inputItemsContainer;

    private ISleekLabel inputItemsLabel;

    private ISleekLabel toolItemsLabel;

    private ISleekElement outputItemsContainer;

    private ISleekElement skillContainer;

    private ISleekBox skillBox;

    private ISleekElement requiredTagsContainer;

    private List<SleekSelectedBlueprintRequiredTag> requiredTags = new List<SleekSelectedBlueprintRequiredTag>();

    private ISleekElement conditionsContainer;

    private ISleekElement conditionsElementsContainer;

    private ISleekElement rewardsContainer;

    private ISleekElement rewardsElementsContainer;

    private List<int> consumingInputIndices = new List<int>();

    private List<int> nonConsumingInputIndices = new List<int>();

    private SleekSelectedBlueprintItem targetItemWidget;

    private List<SleekSelectedBlueprintItem> inputItemWidgets = new List<SleekSelectedBlueprintItem>();

    private List<SleekSelectedBlueprintItem> outputItemWidgets = new List<SleekSelectedBlueprintItem>();

    /// <summary>
    /// Note: this can be different from status.blueprint after status refreshes because status is pooled.
    /// </summary>
    public Blueprint SelectedBlueprint { get; private set; }

    internal void SetSelectedBlueprintStatus(BlueprintStatus status)
    {
        this.status = status;
        SelectedBlueprint = status?.blueprint;
        if (SelectedBlueprint != null)
        {
            float num = 0f;
            PopulateSummary();
            if (summaryContainer.IsVisible)
            {
                summaryContainer.PositionOffset_Y = num;
                num += summaryContainer.SizeOffset_Y;
            }
            PopulateInputItems();
            if (inputItemsContainer.IsVisible)
            {
                inputItemsContainer.PositionOffset_Y = num;
                num += inputItemsContainer.SizeOffset_Y;
            }
            PopulateOutputItems();
            if (outputItemsContainer.IsVisible)
            {
                outputItemsContainer.PositionOffset_Y = num;
                num += outputItemsContainer.SizeOffset_Y;
            }
            PopulateSkills();
            if (skillContainer.IsVisible)
            {
                skillContainer.PositionOffset_Y = num;
                num += skillContainer.SizeOffset_Y;
            }
            PopulateRequiredTags();
            if (requiredTagsContainer.IsVisible)
            {
                requiredTagsContainer.PositionOffset_Y = num;
                num += requiredTagsContainer.SizeOffset_Y;
            }
            PopulateConditions();
            if (conditionsContainer.IsVisible)
            {
                conditionsContainer.PositionOffset_Y = num;
                num += conditionsContainer.SizeOffset_Y;
            }
            PopulateRewards();
            if (rewardsContainer.IsVisible)
            {
                rewardsContainer.PositionOffset_Y = num;
                num += rewardsContainer.SizeOffset_Y;
            }
            detailScrollView.ContentSizeOffset = new Vector2(0f, num);
            RefreshCraftButtonTooltip();
            RefreshIsIgnoring(Player.player.crafting.getIgnoringBlueprint(status.blueprint));
        }
    }

    private void SetPrimaryItem(ItemAsset asset, byte[] state)
    {
        if (currentPrimaryItemAsset != asset)
        {
            currentPrimaryItemAsset = asset;
            primaryItemIcon.Clear();
        }
        if (asset.size_y >= asset.size_x)
        {
            primaryItemIcon.SizeOffset_Y = primaryItemIcon.SizeOffset_X;
        }
        else
        {
            float num = (float)(int)asset.size_y / (float)(int)asset.size_x;
            primaryItemIcon.SizeOffset_Y = primaryItemIcon.SizeOffset_X * num;
        }
        primaryItemIcon.Refresh(asset.id, 100, state, asset, Mathf.RoundToInt(primaryItemIcon.SizeOffset_X), Mathf.RoundToInt(primaryItemIcon.SizeOffset_Y));
    }

    /// <summary>
    /// Update the title box describing the "most important" item: item to repair, salvage, craft, etc.
    /// </summary>
    private void PopulateSummary()
    {
        Local localization = PlayerDashboardCraftingUI.localization;
        if (SelectedBlueprint.TargetItem != null)
        {
            ItemAsset itemAsset = SelectedBlueprint.TargetItem.FindItemAsset();
            if (itemAsset == null)
            {
                summaryContainer.IsVisible = false;
                return;
            }
            Item firstItemOrNull = status.targetStatus.FirstItemOrNull;
            byte[] array = firstItemOrNull?.state;
            if (array == null)
            {
                array = itemAsset.getState();
            }
            SetPrimaryItem(itemAsset, array);
            if (SelectedBlueprint.Operation == EBlueprintOperation.RepairTargetItem)
            {
                titleLabel.Text = localization.format("Details_RepairTitle", itemAsset.RarityRichTextName);
                byte b = firstItemOrNull?.quality ?? 0;
                int num = 100 - b;
                Color qualityColor = ItemTool.getQualityColor((float)(int)b / 100f);
                Color cOLOR_G = Palette.COLOR_G;
                string arg = RichTextUtil.wrapWithColor($"{b}%", qualityColor);
                string arg2 = RichTextUtil.wrapWithColor("100%", cOLOR_G);
                string arg3 = RichTextUtil.wrapWithColor($"{num}%", cOLOR_G);
                descriptionLabel.Text = localization.format("Details_RepairDescription", arg3, arg, arg2);
            }
            else if (SelectedBlueprint.Operation == EBlueprintOperation.FillTargetItem)
            {
                titleLabel.Text = localization.format("Details_FillTitle", itemAsset.RarityRichTextName);
                int num2 = firstItemOrNull?.amount ?? 0;
                int a = itemAsset.MaxAmount - num2;
                int b2 = 0;
                if (status.inputItems.Count > 0)
                {
                    b2 = status.inputItems[0].totalAmount;
                }
                int num3 = Mathf.Min(a, b2);
                int num4 = num2 + num3;
                descriptionLabel.Text = localization.format("Details_FillDescription", num3, num2, num4);
            }
            else
            {
                summaryContainer.IsVisible = false;
            }
        }
        else if (SelectedBlueprint.CategoryTagRef == EBlueprintTypeEx.salvageCategoryTagRef && SelectedBlueprint.supplies != null && SelectedBlueprint.supplies.Length == 1)
        {
            ItemAsset itemAsset2 = SelectedBlueprint.supplies[0].FindItemAsset();
            if (itemAsset2 == null)
            {
                summaryContainer.IsVisible = false;
                return;
            }
            titleLabel.Text = localization.format("Details_SalvageTitle", itemAsset2.RarityRichTextName);
            byte[] array2 = status.inputItems[0].FirstItemOrNull?.state;
            if (array2 == null)
            {
                array2 = itemAsset2.getState();
            }
            SetPrimaryItem(itemAsset2, array2);
            Local localization2 = PlayerDashboardInventoryUI.localization;
            int rarity = (int)itemAsset2.rarity;
            string arg4 = localization2.format("Rarity_" + rarity);
            Local localization3 = PlayerDashboardInventoryUI.localization;
            rarity = (int)itemAsset2.type;
            string arg5 = localization3.format("Type_" + rarity);
            descriptionLabel.Text = RichTextUtil.wrapWithColor(PlayerDashboardInventoryUI.localization.format("Rarity_Type_Label", arg4, arg5), ItemTool.getRarityColorUI(itemAsset2.rarity));
        }
        else if (SelectedBlueprint.outputs.Length == 1)
        {
            ItemAsset itemAsset3 = SelectedBlueprint.outputs[0].FindItemAsset();
            if (itemAsset3 == null)
            {
                summaryContainer.IsVisible = false;
                return;
            }
            titleLabel.Text = localization.format("Details_CraftTitle", itemAsset3.RarityRichTextName);
            byte[] state;
            if (SelectedBlueprint.transferState)
            {
                status.GetPreviewOutputTransferState(itemAsset3, out var _, out state);
            }
            else
            {
                state = itemAsset3.getState();
            }
            SetPrimaryItem(itemAsset3, state);
            Local localization4 = PlayerDashboardInventoryUI.localization;
            int rarity = (int)itemAsset3.rarity;
            string arg6 = localization4.format("Rarity_" + rarity);
            Local localization5 = PlayerDashboardInventoryUI.localization;
            rarity = (int)itemAsset3.type;
            string arg7 = localization5.format("Type_" + rarity);
            descriptionLabel.Text = RichTextUtil.wrapWithColor(PlayerDashboardInventoryUI.localization.format("Rarity_Type_Label", arg6, arg7), ItemTool.getRarityColorUI(itemAsset3.rarity));
        }
        else
        {
            summaryContainer.IsVisible = false;
        }
        summaryContainer.IsVisible = true;
        descriptionLabel.PositionOffset_Y = primaryItemIcon.PositionOffset_Y + primaryItemIcon.SizeOffset_Y;
        summaryContainer.SizeOffset_Y = descriptionLabel.PositionOffset_Y + descriptionLabel.SizeOffset_Y;
    }

    private void PopulateInputItems()
    {
        inputItemsContainer.IsVisible = SelectedBlueprint.supplies.Length != 0;
        if (!inputItemsContainer.IsVisible)
        {
            return;
        }
        consumingInputIndices.Clear();
        nonConsumingInputIndices.Clear();
        for (int i = 0; i < SelectedBlueprint.supplies.Length; i++)
        {
            if (SelectedBlueprint.supplies[i].ShouldConsume)
            {
                consumingInputIndices.Add(i);
            }
            else
            {
                nonConsumingInputIndices.Add(i);
            }
        }
        float offset = 0f;
        int j = 0;
        if (consumingInputIndices.Count > 0)
        {
            inputItemsLabel.IsVisible = true;
            offset += 40f;
            foreach (int consumingInputIndex in consumingInputIndices)
            {
                AddInputItemWidget(consumingInputIndex, ref j, ref offset);
            }
        }
        else
        {
            inputItemsLabel.IsVisible = false;
        }
        if (nonConsumingInputIndices.Count > 0)
        {
            toolItemsLabel.IsVisible = true;
            toolItemsLabel.PositionOffset_Y = offset;
            offset += 40f;
            foreach (int nonConsumingInputIndex in nonConsumingInputIndices)
            {
                AddInputItemWidget(nonConsumingInputIndex, ref j, ref offset);
            }
        }
        else
        {
            toolItemsLabel.IsVisible = false;
        }
        inputItemsContainer.SizeOffset_Y = offset;
        for (; j < inputItemWidgets.Count; j++)
        {
            inputItemWidgets[j].IsVisible = false;
        }
    }

    private void AddInputItemWidget(int inputItemIndex, ref int widgetIndex, ref float offset)
    {
        BlueprintSupply config = SelectedBlueprint.supplies[inputItemIndex];
        BlueprintInputItemStatus blueprintInputItemStatus = status.inputItems[inputItemIndex];
        SleekSelectedBlueprintItem sleekSelectedBlueprintItem;
        if (widgetIndex < inputItemWidgets.Count)
        {
            sleekSelectedBlueprintItem = inputItemWidgets[widgetIndex];
            sleekSelectedBlueprintItem.IsVisible = true;
        }
        else
        {
            sleekSelectedBlueprintItem = new SleekSelectedBlueprintItem();
            sleekSelectedBlueprintItem.SizeScale_X = 1f;
            inputItemsContainer.AddChild(sleekSelectedBlueprintItem);
            inputItemWidgets.Add(sleekSelectedBlueprintItem);
        }
        sleekSelectedBlueprintItem.PositionOffset_Y = offset;
        sleekSelectedBlueprintItem.blueprintStatus = status;
        sleekSelectedBlueprintItem.SetInputItem(config, blueprintInputItemStatus, inputItemIndex);
        offset += sleekSelectedBlueprintItem.SizeOffset_Y;
        widgetIndex++;
    }

    private void PopulateOutputItems()
    {
        outputItemsContainer.IsVisible = SelectedBlueprint.outputs.Length != 0;
        if (!outputItemsContainer.IsVisible)
        {
            return;
        }
        float num = 40f;
        int i = 0;
        for (int j = 0; j < SelectedBlueprint.outputs.Length; j++)
        {
            BlueprintOutput output = SelectedBlueprint.outputs[j];
            SleekSelectedBlueprintItem sleekSelectedBlueprintItem;
            if (i < outputItemWidgets.Count)
            {
                sleekSelectedBlueprintItem = outputItemWidgets[i];
                sleekSelectedBlueprintItem.IsVisible = true;
            }
            else
            {
                sleekSelectedBlueprintItem = new SleekSelectedBlueprintItem();
                sleekSelectedBlueprintItem.SizeScale_X = 1f;
                outputItemsContainer.AddChild(sleekSelectedBlueprintItem);
                outputItemWidgets.Add(sleekSelectedBlueprintItem);
            }
            sleekSelectedBlueprintItem.PositionOffset_Y = num;
            sleekSelectedBlueprintItem.blueprintStatus = status;
            sleekSelectedBlueprintItem.SetOutputItem(status, output, j);
            num += sleekSelectedBlueprintItem.SizeOffset_Y;
            i++;
        }
        outputItemsContainer.SizeOffset_Y = num;
        for (; i < outputItemWidgets.Count; i++)
        {
            outputItemWidgets[i].IsVisible = false;
        }
    }

    private void PopulateSkills()
    {
        skillContainer.IsVisible = SelectedBlueprint.skill != EBlueprintSkill.NONE;
        if (skillContainer.IsVisible)
        {
            int num;
            int num2;
            switch (SelectedBlueprint.skill)
            {
            case EBlueprintSkill.CRAFT:
                num = 2;
                num2 = 1;
                break;
            case EBlueprintSkill.COOK:
                num = 2;
                num2 = 3;
                break;
            case EBlueprintSkill.REPAIR:
                num = 2;
                num2 = 7;
                break;
            default:
                num = 0;
                num2 = 0;
                UnturnedLog.error($"Unknown blueprint skill requirement: {SelectedBlueprint.skill}");
                break;
            }
            bool flag = Player.player.skills.skills[num][num2].level >= SelectedBlueprint.level;
            Local localization = PlayerDashboardSkillsUI.localization;
            string arg = localization.format("Speciality_" + num + "_Skill_" + num2);
            string arg2 = localization.format("Level_" + SelectedBlueprint.level);
            skillBox.Text = PlayerDashboardCraftingUI.localization.format("Requirements_Skill", arg, arg2);
            skillBox.TextColor = (flag ? ESleekTint.FONT : ESleekTint.BAD);
        }
    }

    private void PopulateRequiredTags()
    {
        CachingAssetRef[] applicableRequiredNearbyCraftingTags = SelectedBlueprint.GetApplicableRequiredNearbyCraftingTags();
        requiredTagsContainer.IsVisible = !applicableRequiredNearbyCraftingTags.IsNullOrEmpty();
        if (!requiredTagsContainer.IsVisible)
        {
            return;
        }
        int i = 0;
        for (int j = 0; j < applicableRequiredNearbyCraftingTags.Length; j++)
        {
            TagAsset tagAsset = applicableRequiredNearbyCraftingTags[j].Get<TagAsset>();
            if (tagAsset != null)
            {
                SleekSelectedBlueprintRequiredTag sleekSelectedBlueprintRequiredTag;
                if (i < requiredTags.Count)
                {
                    sleekSelectedBlueprintRequiredTag = requiredTags[i];
                    sleekSelectedBlueprintRequiredTag.IsVisible = true;
                }
                else
                {
                    sleekSelectedBlueprintRequiredTag = new SleekSelectedBlueprintRequiredTag();
                    sleekSelectedBlueprintRequiredTag.SizeScale_X = 1f;
                    sleekSelectedBlueprintRequiredTag.SizeOffset_Y = 50f;
                    requiredTagsContainer.AddChild(sleekSelectedBlueprintRequiredTag);
                    requiredTags.Add(sleekSelectedBlueprintRequiredTag);
                }
                sleekSelectedBlueprintRequiredTag.SetTag(tagAsset, !Player.player.crafting.IsCraftingTagAvailable(tagAsset));
                sleekSelectedBlueprintRequiredTag.PositionOffset_Y = 40 + i * 50;
                i++;
            }
        }
        requiredTagsContainer.SizeOffset_Y = 40 + i * 50;
        for (; i < requiredTags.Count; i++)
        {
            requiredTags[i].IsVisible = false;
        }
    }

    private void PopulateConditions()
    {
        conditionsContainer.IsVisible = !SelectedBlueprint.questConditions.IsNullOrEmpty();
        if (!conditionsContainer.IsVisible)
        {
            return;
        }
        conditionsElementsContainer.RemoveAllChildren();
        bool isVisible = false;
        float num = 0f;
        for (int i = 0; i < SelectedBlueprint.questConditions.Length; i++)
        {
            ISleekElement sleekElement = SelectedBlueprint.questConditions[i].createUI(Player.player, null);
            if (sleekElement != null)
            {
                sleekElement.PositionOffset_Y = num;
                conditionsElementsContainer.AddChild(sleekElement);
                num += sleekElement.SizeOffset_Y;
                isVisible = true;
            }
        }
        conditionsContainer.IsVisible = isVisible;
        conditionsElementsContainer.SizeOffset_Y = num;
        conditionsContainer.SizeOffset_Y = num + 40f;
    }

    private void PopulateRewards()
    {
        rewardsContainer.IsVisible = !SelectedBlueprint.questRewards.IsNullOrEmpty();
        if (!rewardsContainer.IsVisible)
        {
            return;
        }
        rewardsElementsContainer.RemoveAllChildren();
        bool isVisible = false;
        float num = 0f;
        for (int i = 0; i < SelectedBlueprint.questRewards.Length; i++)
        {
            ISleekElement sleekElement = SelectedBlueprint.questRewards[i].createUI(Player.player);
            if (sleekElement != null)
            {
                sleekElement.PositionOffset_Y = num;
                rewardsElementsContainer.AddChild(sleekElement);
                num += sleekElement.SizeOffset_Y;
                isVisible = true;
            }
        }
        rewardsContainer.IsVisible = isVisible;
        rewardsElementsContainer.SizeOffset_Y = num;
        rewardsContainer.SizeOffset_Y = num + 40f;
    }

    private void OnClickedCraftButton(ISleekElement button)
    {
        if (!Player.player.equipment.isBusy)
        {
            bool key = InputEx.GetKey(ControlsSettings.other);
            Player.player.crafting.SendRequestToCraft(SelectedBlueprint, key);
        }
    }

    private void OnSwappedVisibilityState(SleekButtonState button, int state)
    {
        bool isIgnoring = state == 1;
        Player.player.crafting.setIgnoringBlueprint(status.blueprint, isIgnoring);
        RefreshIsIgnoring(isIgnoring);
    }

    private void RefreshCraftButtonTooltip()
    {
        if (status.IsCraftable)
        {
            craftButton.tooltip = PlayerDashboardInventoryUI.localization.format("ActionBlueprint_CraftAllTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
        }
        else
        {
            craftButton.tooltip = PlayerDashboardCraftingUI.BuildNotCraftableTooltip(status);
        }
    }

    private void RefreshIsIgnoring(bool isIgnoring)
    {
        craftButton.isClickable = !isIgnoring && status.IsCraftable;
        visibilityButton.state = (isIgnoring ? 1 : 0);
    }

    public SleekSelectedBlueprint()
    {
        Local localization = PlayerDashboardCraftingUI.localization;
        Bundle icons = PlayerDashboardCraftingUI.icons;
        detailScrollView = Glazier.Get().CreateScrollView();
        detailScrollView.SizeScale_X = 1f;
        detailScrollView.SizeScale_Y = 1f;
        detailScrollView.SizeOffset_Y = -90f;
        detailScrollView.ScaleContentToWidth = true;
        AddChild(detailScrollView);
        craftButton = new SleekButtonIcon(icons.load<Texture2D>("CraftIcon"), 40);
        craftButton.PositionOffset_Y = -80f;
        craftButton.PositionScale_Y = 1f;
        craftButton.SizeScale_X = 1f;
        craftButton.SizeOffset_Y = 50f;
        craftButton.onClickedButton += OnClickedCraftButton;
        craftButton.text = localization.format("Craft");
        craftButton.fontSize = ESleekFontSize.Medium;
        craftButton.iconColor = ESleekTint.FOREGROUND;
        AddChild(craftButton);
        visibilityButton = new SleekButtonState(20, new GUIContent(localization.format("VisibilityButton_Visible_Label"), icons.load<Texture2D>("BlueprintVisibleIcon"), localization.format("VisibilityButton_Visible_Tooltip")), new GUIContent(localization.format("VisibilityButton_Hidden_Label"), icons.load<Texture2D>("BlueprintHiddenIcon"), localization.format("VisibilityButton_Hidden_Tooltip")));
        visibilityButton.PositionOffset_Y = -30f;
        visibilityButton.PositionScale_Y = 1f;
        visibilityButton.SizeScale_X = 1f;
        visibilityButton.SizeOffset_Y = 30f;
        SleekButtonState sleekButtonState = visibilityButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedVisibilityState));
        visibilityButton.UseContentTooltip = true;
        visibilityButton.button.iconColor = ESleekTint.FOREGROUND;
        AddChild(visibilityButton);
        summaryContainer = Glazier.Get().CreateBox();
        summaryContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(summaryContainer);
        titleLabel = Glazier.Get().CreateLabel();
        titleLabel.SizeScale_X = 1f;
        titleLabel.SizeOffset_Y = 40f;
        titleLabel.FontSize = ESleekFontSize.Medium;
        titleLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        titleLabel.AllowRichText = true;
        titleLabel.TextColor = ESleekTint.FONT;
        summaryContainer.AddChild(titleLabel);
        primaryItemIcon = new SleekItemIcon();
        primaryItemIcon.PositionOffset_X = -100f;
        primaryItemIcon.PositionScale_X = 0.5f;
        primaryItemIcon.PositionOffset_Y = 40f;
        primaryItemIcon.SizeOffset_X = 200f;
        summaryContainer.AddChild(primaryItemIcon);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.SizeScale_X = 1f;
        descriptionLabel.SizeOffset_Y = 40f;
        descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        descriptionLabel.AllowRichText = true;
        descriptionLabel.TextColor = ESleekTint.FONT;
        summaryContainer.AddChild(descriptionLabel);
        inputItemsContainer = Glazier.Get().CreateFrame();
        inputItemsContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(inputItemsContainer);
        inputItemsLabel = Glazier.Get().CreateLabel();
        inputItemsLabel.SizeScale_X = 1f;
        inputItemsLabel.SizeOffset_Y = 40f;
        inputItemsLabel.FontSize = ESleekFontSize.Medium;
        inputItemsLabel.Text = localization.format("Details_InputItems");
        inputItemsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        inputItemsContainer.AddChild(inputItemsLabel);
        toolItemsLabel = Glazier.Get().CreateLabel();
        toolItemsLabel.SizeScale_X = 1f;
        toolItemsLabel.SizeOffset_Y = 40f;
        toolItemsLabel.FontSize = ESleekFontSize.Medium;
        toolItemsLabel.Text = localization.format("Details_ToolItems");
        toolItemsLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        inputItemsContainer.AddChild(toolItemsLabel);
        outputItemsContainer = Glazier.Get().CreateFrame();
        outputItemsContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(outputItemsContainer);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeOffset_Y = 40f;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.Text = localization.format("Details_OutputItems");
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        outputItemsContainer.AddChild(sleekLabel);
        skillContainer = Glazier.Get().CreateFrame();
        skillContainer.SizeScale_X = 1f;
        skillContainer.SizeOffset_Y = 70f;
        detailScrollView.AddChild(skillContainer);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.SizeOffset_Y = 40f;
        sleekLabel2.FontSize = ESleekFontSize.Medium;
        sleekLabel2.Text = localization.format("Details_RequiredSkills");
        sleekLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        skillContainer.AddChild(sleekLabel2);
        skillBox = Glazier.Get().CreateBox();
        skillBox.PositionOffset_Y = 40f;
        skillBox.SizeScale_X = 1f;
        skillBox.SizeOffset_Y = 30f;
        skillBox.FontSize = ESleekFontSize.Medium;
        skillBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        skillContainer.AddChild(skillBox);
        requiredTagsContainer = Glazier.Get().CreateFrame();
        requiredTagsContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(requiredTagsContainer);
        ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
        sleekLabel3.SizeScale_X = 1f;
        sleekLabel3.SizeOffset_Y = 40f;
        sleekLabel3.FontSize = ESleekFontSize.Medium;
        sleekLabel3.Text = localization.format("Details_RequiredTags");
        sleekLabel3.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        requiredTagsContainer.AddChild(sleekLabel3);
        conditionsContainer = Glazier.Get().CreateFrame();
        conditionsContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(conditionsContainer);
        ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
        sleekLabel4.SizeScale_X = 1f;
        sleekLabel4.SizeOffset_Y = 40f;
        sleekLabel4.FontSize = ESleekFontSize.Medium;
        sleekLabel4.Text = localization.format("Details_Conditions");
        sleekLabel4.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        conditionsContainer.AddChild(sleekLabel4);
        conditionsElementsContainer = Glazier.Get().CreateFrame();
        conditionsElementsContainer.PositionOffset_Y = 40f;
        conditionsElementsContainer.SizeScale_X = 1f;
        conditionsContainer.AddChild(conditionsElementsContainer);
        rewardsContainer = Glazier.Get().CreateFrame();
        rewardsContainer.SizeScale_X = 1f;
        detailScrollView.AddChild(rewardsContainer);
        ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
        sleekLabel5.SizeScale_X = 1f;
        sleekLabel5.SizeOffset_Y = 40f;
        sleekLabel5.FontSize = ESleekFontSize.Medium;
        sleekLabel5.Text = localization.format("Details_Rewards");
        sleekLabel5.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        rewardsContainer.AddChild(sleekLabel5);
        rewardsElementsContainer = Glazier.Get().CreateFrame();
        rewardsElementsContainer.PositionOffset_Y = 40f;
        rewardsElementsContainer.SizeScale_X = 1f;
        rewardsContainer.AddChild(rewardsElementsContainer);
    }
}
