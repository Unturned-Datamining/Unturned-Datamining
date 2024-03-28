using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerNPCQuestUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    public static Bundle icons;

    public static bool active;

    private static QuestAsset quest;

    /// <summary>
    /// Valid when opened in Begin or End mode.
    ///
    /// If the quest is ready to complete the UI is opened in End mode to allow
    /// the player to see what rewards they will receive after clicking continue. 
    /// Otherwise, in Begin mode the UI is opened to allow the player to review
    /// the conditions before accepting or declining the request.
    ///
    /// If the player cancels the pending response is NOT chosen.
    /// </summary>
    private static DialogueResponse pendingResponse;

    /// <summary>
    /// Valid when opened in Begin or End mode.
    /// The player clicked pendingResponse in this dialogue to open the quest UI.
    /// </summary>
    private static DialogueAsset dialogueContext;

    private static EQuestViewMode mode;

    private static ISleekBox questBox;

    private static ISleekLabel nameLabel;

    private static ISleekLabel descriptionLabel;

    private static ISleekScrollView conditionsAndRewardsScrollView;

    private static ISleekLabel conditionsLabel;

    private static ISleekElement conditionsContainer;

    private static ISleekLabel rewardsLabel;

    private static ISleekElement rewardsContainer;

    private static ISleekElement beginContainer;

    private static ISleekButton acceptButton;

    private static ISleekButton declineButton;

    private static ISleekElement endContainer;

    private static ISleekButton continueButton;

    private static ISleekElement detailsContainer;

    private static ISleekButton trackButton;

    private static ISleekButton abandonButton;

    private static ISleekButton returnButton;

    private const int LOWER_BUTTONS_HEIGHT = 50;

    private const int LOWER_BUTTONS_VERTICAL_OFFSET = 10;

    private const int QUEST_BOX_INNER_SPACING = 5;

    private static List<bool> areConditionsMet = new List<bool>(8);

    public static void open(QuestAsset newQuest, DialogueAsset newDialogueContext, DialogueResponse newPendingResponse, EQuestViewMode newMode)
    {
        if (!active)
        {
            active = true;
            updateQuest(newQuest, newDialogueContext, newPendingResponse, newMode);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void closeNicely()
    {
        close();
        if (mode == EQuestViewMode.BEGIN || mode == EQuestViewMode.END)
        {
            PlayerNPCDialogueUI.OpenCurrentDialogue();
        }
        else if (mode == EQuestViewMode.DETAILS)
        {
            PlayerDashboardInventoryUI.active = false;
            PlayerDashboardCraftingUI.active = false;
            PlayerDashboardSkillsUI.active = false;
            PlayerDashboardInformationUI.active = true;
            PlayerDashboardUI.open();
        }
    }

    private static void updateQuest(QuestAsset newQuest, DialogueAsset newDialogueContext, DialogueResponse newPendingResponse, EQuestViewMode newMode)
    {
        quest = newQuest;
        pendingResponse = newPendingResponse;
        dialogueContext = newDialogueContext;
        mode = newMode;
        if (quest == null)
        {
            return;
        }
        beginContainer.IsVisible = mode == EQuestViewMode.BEGIN;
        endContainer.IsVisible = mode == EQuestViewMode.END;
        detailsContainer.IsVisible = mode == EQuestViewMode.DETAILS;
        SetButtonsAreClickable(isClickable: true);
        if (mode == EQuestViewMode.DETAILS)
        {
            if (Player.player.quests.GetTrackedQuest() == quest)
            {
                trackButton.Text = localization.format("Track_Off");
            }
            else
            {
                trackButton.Text = localization.format("Track_On");
            }
        }
        nameLabel.Text = quest.questName;
        descriptionLabel.Text = quest.questDescription;
        float num = 0f;
        if (quest.conditions != null && quest.conditions.Length != 0)
        {
            conditionsLabel.IsVisible = true;
            conditionsContainer.IsVisible = true;
            conditionsContainer.RemoveAllChildren();
            float num2 = 0f;
            areConditionsMet.Clear();
            INPCCondition[] conditions = quest.conditions;
            foreach (INPCCondition iNPCCondition in conditions)
            {
                areConditionsMet.Add(iNPCCondition.isConditionMet(Player.player));
            }
            for (int j = 0; j < quest.conditions.Length; j++)
            {
                INPCCondition iNPCCondition2 = quest.conditions[j];
                if (iNPCCondition2.AreUIRequirementsMet(areConditionsMet))
                {
                    bool flag = areConditionsMet[j];
                    Texture2D icon = null;
                    if (mode != 0)
                    {
                        icon = ((!flag) ? icons.load<Texture2D>("Incomplete") : icons.load<Texture2D>("Complete"));
                    }
                    ISleekElement sleekElement = iNPCCondition2.createUI(Player.player, icon);
                    if (sleekElement != null)
                    {
                        sleekElement.PositionOffset_Y = num2;
                        conditionsContainer.AddChild(sleekElement);
                        num2 += sleekElement.SizeOffset_Y;
                    }
                }
            }
            conditionsContainer.SizeOffset_Y = num2;
            num += conditionsLabel.SizeOffset_Y;
            num += conditionsContainer.SizeOffset_Y;
        }
        else
        {
            conditionsLabel.IsVisible = false;
            conditionsContainer.IsVisible = false;
        }
        if (quest.rewards != null && quest.rewards.Length != 0)
        {
            rewardsLabel.IsVisible = true;
            rewardsContainer.IsVisible = true;
            rewardsContainer.RemoveAllChildren();
            float num3 = 0f;
            for (int k = 0; k < quest.rewards.Length; k++)
            {
                ISleekElement sleekElement2 = quest.rewards[k].createUI(Player.player);
                if (sleekElement2 != null)
                {
                    sleekElement2.PositionOffset_Y = num3;
                    rewardsContainer.AddChild(sleekElement2);
                    num3 += sleekElement2.SizeOffset_Y;
                }
            }
            rewardsLabel.PositionOffset_Y = num;
            num += rewardsLabel.SizeOffset_Y;
            rewardsContainer.PositionOffset_Y = num;
            rewardsContainer.SizeOffset_Y = num3;
            num += rewardsContainer.SizeOffset_Y;
        }
        else
        {
            rewardsLabel.IsVisible = false;
            rewardsContainer.IsVisible = false;
        }
        conditionsAndRewardsScrollView.ContentSizeOffset = new Vector2(0f, num);
        float num4 = (float)Screen.height / GraphicsSettings.userInterfaceScale;
        num4 -= 10f;
        num4 -= 10f;
        num4 -= 50f;
        num4 -= 10f;
        float num5 = conditionsAndRewardsScrollView.PositionOffset_Y + num + 5f;
        if (num5 >= num4)
        {
            questBox.PositionOffset_Y = 0f;
            questBox.PositionScale_Y = 0f;
            questBox.SizeOffset_Y = -60f;
            questBox.SizeScale_Y = 1f;
        }
        else
        {
            questBox.PositionOffset_Y = num5 * -0.5f - 30f;
            questBox.PositionScale_Y = 0.5f;
            questBox.SizeOffset_Y = num5;
            questBox.SizeScale_Y = 0f;
        }
    }

    private static void onClickedAcceptButton(ISleekElement button)
    {
        SetButtonsAreClickable(isClickable: false);
        Player.player.quests.ClientChooseDialogueResponse(dialogueContext.GUID, pendingResponse.index);
    }

    private static void onClickedDeclineButton(ISleekElement button)
    {
        SetButtonsAreClickable(isClickable: false);
        close();
        PlayerNPCDialogueUI.OpenCurrentDialogue();
    }

    private static void onClickedContinueButton(ISleekElement button)
    {
        SetButtonsAreClickable(isClickable: false);
        Player.player.quests.ClientChooseDialogueResponse(dialogueContext.GUID, pendingResponse.index);
    }

    private static void onClickedTrackButton(ISleekElement button)
    {
        Player.player.quests.ClientTrackQuest(quest);
        if (!Provider.isServer)
        {
            Player.player.quests.TrackQuest(quest);
        }
        closeNicely();
    }

    private static void onClickedAbandonButton(ISleekElement button)
    {
        Player.player.quests.ClientAbandonQuest(quest);
        closeNicely();
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        closeNicely();
    }

    private static void SetButtonsAreClickable(bool isClickable)
    {
        acceptButton.IsClickable = isClickable;
        declineButton.IsClickable = isClickable;
        continueButton.IsClickable = isClickable;
    }

    public PlayerNPCQuestUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerNPCQuest.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerNPCQuest/PlayerNPCQuest.unity3d");
        container = new SleekFullscreenBox();
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        questBox = Glazier.Get().CreateBox();
        questBox.PositionOffset_X = -250f;
        questBox.PositionScale_X = 0.5f;
        questBox.SizeOffset_X = 500f;
        container.AddChild(questBox);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 5f;
        nameLabel.PositionOffset_Y = 5f;
        nameLabel.SizeOffset_X = -10f;
        nameLabel.SizeOffset_Y = 30f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.TextAlignment = TextAnchor.UpperLeft;
        nameLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.AllowRichText = true;
        nameLabel.FontSize = ESleekFontSize.Medium;
        questBox.AddChild(nameLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.PositionOffset_X = 5f;
        descriptionLabel.PositionOffset_Y = nameLabel.SizeOffset_Y;
        descriptionLabel.SizeOffset_X = -10f;
        descriptionLabel.SizeOffset_Y = 70f;
        descriptionLabel.SizeScale_X = 1f;
        descriptionLabel.TextAlignment = TextAnchor.UpperLeft;
        descriptionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        descriptionLabel.AllowRichText = true;
        questBox.AddChild(descriptionLabel);
        conditionsAndRewardsScrollView = Glazier.Get().CreateScrollView();
        conditionsAndRewardsScrollView.PositionOffset_X = 5f;
        conditionsAndRewardsScrollView.PositionOffset_Y = descriptionLabel.PositionOffset_Y + descriptionLabel.SizeOffset_Y + 5f;
        conditionsAndRewardsScrollView.SizeOffset_X = -10f;
        conditionsAndRewardsScrollView.SizeOffset_Y = 0f - conditionsAndRewardsScrollView.PositionOffset_Y - 5f;
        conditionsAndRewardsScrollView.SizeScale_X = 1f;
        conditionsAndRewardsScrollView.SizeScale_Y = 1f;
        conditionsAndRewardsScrollView.ScaleContentToWidth = true;
        questBox.AddChild(conditionsAndRewardsScrollView);
        conditionsLabel = Glazier.Get().CreateLabel();
        conditionsLabel.SizeOffset_Y = 30f;
        conditionsLabel.SizeScale_X = 1f;
        conditionsLabel.TextAlignment = TextAnchor.MiddleLeft;
        conditionsLabel.Text = localization.format("Conditions");
        conditionsLabel.FontSize = ESleekFontSize.Medium;
        conditionsAndRewardsScrollView.AddChild(conditionsLabel);
        conditionsContainer = Glazier.Get().CreateFrame();
        conditionsContainer.PositionOffset_Y = 30f;
        conditionsContainer.SizeScale_X = 1f;
        conditionsAndRewardsScrollView.AddChild(conditionsContainer);
        rewardsLabel = Glazier.Get().CreateLabel();
        rewardsLabel.SizeOffset_Y = 30f;
        rewardsLabel.SizeScale_X = 1f;
        rewardsLabel.TextAlignment = TextAnchor.MiddleLeft;
        rewardsLabel.Text = localization.format("Rewards");
        rewardsLabel.FontSize = ESleekFontSize.Medium;
        conditionsAndRewardsScrollView.AddChild(rewardsLabel);
        rewardsContainer = Glazier.Get().CreateFrame();
        rewardsContainer.SizeScale_X = 1f;
        conditionsAndRewardsScrollView.AddChild(rewardsContainer);
        beginContainer = Glazier.Get().CreateFrame();
        beginContainer.PositionOffset_Y = 10f;
        beginContainer.PositionScale_Y = 1f;
        beginContainer.SizeOffset_Y = 50f;
        beginContainer.SizeScale_X = 1f;
        questBox.AddChild(beginContainer);
        beginContainer.IsVisible = false;
        endContainer = Glazier.Get().CreateFrame();
        endContainer.PositionOffset_Y = 10f;
        endContainer.PositionScale_Y = 1f;
        endContainer.SizeOffset_Y = 50f;
        endContainer.SizeScale_X = 1f;
        questBox.AddChild(endContainer);
        endContainer.IsVisible = false;
        detailsContainer = Glazier.Get().CreateFrame();
        detailsContainer.PositionOffset_Y = 10f;
        detailsContainer.PositionScale_Y = 1f;
        detailsContainer.SizeOffset_Y = 50f;
        detailsContainer.SizeScale_X = 1f;
        questBox.AddChild(detailsContainer);
        detailsContainer.IsVisible = false;
        acceptButton = Glazier.Get().CreateButton();
        acceptButton.SizeOffset_X = -5f;
        acceptButton.SizeScale_X = 0.5f;
        acceptButton.SizeScale_Y = 1f;
        acceptButton.Text = localization.format("Accept");
        acceptButton.TooltipText = localization.format("Accept_Tooltip");
        acceptButton.FontSize = ESleekFontSize.Medium;
        acceptButton.OnClicked += onClickedAcceptButton;
        beginContainer.AddChild(acceptButton);
        declineButton = Glazier.Get().CreateButton();
        declineButton.PositionOffset_X = 5f;
        declineButton.PositionScale_X = 0.5f;
        declineButton.SizeOffset_X = -5f;
        declineButton.SizeScale_X = 0.5f;
        declineButton.SizeScale_Y = 1f;
        declineButton.Text = localization.format("Decline");
        declineButton.TooltipText = localization.format("Decline_Tooltip");
        declineButton.FontSize = ESleekFontSize.Medium;
        declineButton.OnClicked += onClickedDeclineButton;
        beginContainer.AddChild(declineButton);
        continueButton = Glazier.Get().CreateButton();
        continueButton.SizeScale_X = 1f;
        continueButton.SizeScale_Y = 1f;
        continueButton.Text = localization.format("Continue");
        continueButton.TooltipText = localization.format("Continue_Tooltip");
        continueButton.FontSize = ESleekFontSize.Medium;
        continueButton.OnClicked += onClickedContinueButton;
        endContainer.AddChild(continueButton);
        trackButton = Glazier.Get().CreateButton();
        trackButton.SizeOffset_X = -5f;
        trackButton.SizeScale_X = 0.333f;
        trackButton.SizeScale_Y = 1f;
        trackButton.TooltipText = localization.format("Track_Tooltip");
        trackButton.FontSize = ESleekFontSize.Medium;
        trackButton.OnClicked += onClickedTrackButton;
        detailsContainer.AddChild(trackButton);
        abandonButton = Glazier.Get().CreateButton();
        abandonButton.PositionOffset_X = 5f;
        abandonButton.PositionScale_X = 0.333f;
        abandonButton.SizeOffset_X = -10f;
        abandonButton.SizeScale_X = 0.333f;
        abandonButton.SizeScale_Y = 1f;
        abandonButton.Text = localization.format("Abandon");
        abandonButton.TooltipText = localization.format("Abandon_Tooltip");
        abandonButton.FontSize = ESleekFontSize.Medium;
        abandonButton.OnClicked += onClickedAbandonButton;
        detailsContainer.AddChild(abandonButton);
        returnButton = Glazier.Get().CreateButton();
        returnButton.PositionOffset_X = 5f;
        returnButton.PositionScale_X = 0.667f;
        returnButton.SizeOffset_X = -5f;
        returnButton.SizeScale_X = 0.333f;
        returnButton.SizeScale_Y = 1f;
        returnButton.Text = localization.format("Return");
        returnButton.TooltipText = localization.format("Return_Tooltip");
        returnButton.FontSize = ESleekFontSize.Medium;
        returnButton.OnClicked += onClickedReturnButton;
        detailsContainer.AddChild(returnButton);
    }
}
