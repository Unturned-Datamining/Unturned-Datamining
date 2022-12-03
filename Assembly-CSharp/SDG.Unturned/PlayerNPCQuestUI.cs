using UnityEngine;

namespace SDG.Unturned;

public class PlayerNPCQuestUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    public static Bundle icons;

    public static bool active;

    private static QuestAsset quest;

    private static DialogueResponse response;

    private static DialogueAsset acceptDialogue;

    private static DialogueAsset declineDialogue;

    private static EQuestViewMode mode;

    private static ISleekBox questBox;

    private static ISleekLabel nameLabel;

    private static ISleekLabel descriptionLabel;

    private static ISleekScrollView detailsBox;

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

    public static void open(QuestAsset newQuest, DialogueResponse newResponse, DialogueAsset newAcceptDialogue, DialogueAsset newDeclineDialogue, EQuestViewMode newMode)
    {
        if (!active)
        {
            active = true;
            updateQuest(newQuest, newResponse, newAcceptDialogue, newDeclineDialogue, newMode);
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
        if (mode == EQuestViewMode.BEGIN)
        {
            PlayerNPCDialogueUI.open(declineDialogue, null);
        }
        else if (mode == EQuestViewMode.END)
        {
            PlayerNPCDialogueUI.registerResponse(declineDialogue, response);
            PlayerNPCDialogueUI.open(acceptDialogue, declineDialogue);
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

    private static void updateQuest(QuestAsset newQuest, DialogueResponse newResponse, DialogueAsset newAcceptDialogue, DialogueAsset newDeclineDialogue, EQuestViewMode newMode)
    {
        quest = newQuest;
        response = newResponse;
        acceptDialogue = newAcceptDialogue;
        declineDialogue = newDeclineDialogue;
        mode = newMode;
        if (quest == null)
        {
            return;
        }
        beginContainer.isVisible = mode == EQuestViewMode.BEGIN;
        acceptButton.isClickable = true;
        declineButton.isClickable = true;
        endContainer.isVisible = mode == EQuestViewMode.END;
        continueButton.isClickable = true;
        detailsContainer.isVisible = mode == EQuestViewMode.DETAILS;
        if (mode == EQuestViewMode.DETAILS)
        {
            if (Player.player.quests.TrackedQuestID == quest.id)
            {
                trackButton.text = localization.format("Track_Off");
            }
            else
            {
                trackButton.text = localization.format("Track_On");
            }
        }
        nameLabel.text = quest.questName;
        descriptionLabel.text = quest.questDescription;
        int num = Screen.height - 80;
        int num2 = 0;
        if (quest.conditions != null && quest.conditions.Length != 0)
        {
            conditionsLabel.isVisible = true;
            conditionsContainer.isVisible = true;
            conditionsContainer.RemoveAllChildren();
            int num3 = 0;
            for (int i = 0; i < quest.conditions.Length; i++)
            {
                INPCCondition obj = quest.conditions[i];
                bool flag = obj.isConditionMet(Player.player);
                Texture2D icon = null;
                if (mode != 0)
                {
                    icon = ((!flag) ? icons.load<Texture2D>("Incomplete") : icons.load<Texture2D>("Complete"));
                }
                ISleekElement sleekElement = obj.createUI(Player.player, icon);
                if (sleekElement != null)
                {
                    sleekElement.positionOffset_Y = num3;
                    conditionsContainer.AddChild(sleekElement);
                    num3 += sleekElement.sizeOffset_Y;
                }
            }
            conditionsContainer.sizeOffset_Y = num3;
            num2 += 30;
            num2 += num3;
        }
        else
        {
            conditionsLabel.isVisible = false;
            conditionsContainer.isVisible = false;
        }
        if (quest.rewards != null && quest.rewards.Length != 0)
        {
            rewardsLabel.isVisible = true;
            rewardsContainer.isVisible = true;
            rewardsContainer.RemoveAllChildren();
            int num4 = 0;
            for (int j = 0; j < quest.rewards.Length; j++)
            {
                ISleekElement sleekElement2 = quest.rewards[j].createUI(Player.player);
                if (sleekElement2 != null)
                {
                    sleekElement2.positionOffset_Y = num4;
                    rewardsContainer.AddChild(sleekElement2);
                    num4 += sleekElement2.sizeOffset_Y;
                }
            }
            rewardsLabel.positionOffset_Y = num2;
            rewardsContainer.positionOffset_Y = num2 + 30;
            rewardsContainer.sizeOffset_Y = num4;
            num2 += 30;
            num2 += num4;
        }
        else
        {
            rewardsLabel.isVisible = false;
            rewardsContainer.isVisible = false;
        }
        detailsBox.contentSizeOffset = new Vector2(0f, num2);
        if (num2 + 105 > num)
        {
            questBox.positionOffset_Y = 0;
            questBox.positionScale_Y = 0f;
            questBox.sizeOffset_Y = num;
            detailsBox.positionOffset_Y = -num + 100;
            detailsBox.sizeOffset_Y = num - 105;
        }
        else
        {
            questBox.positionOffset_Y = -num2 / 2 - 80;
            questBox.positionScale_Y = 0.5f;
            questBox.sizeOffset_Y = num2 + 100;
            detailsBox.positionOffset_Y = -5 - num2;
            detailsBox.sizeOffset_Y = num2;
        }
    }

    private static void onClickedAcceptButton(ISleekElement button)
    {
        acceptButton.isClickable = false;
        declineButton.isClickable = false;
        close();
        PlayerNPCDialogueUI.registerResponse(declineDialogue, response);
        PlayerNPCDialogueUI.open(acceptDialogue, declineDialogue);
    }

    private static void onClickedDeclineButton(ISleekElement button)
    {
        acceptButton.isClickable = false;
        declineButton.isClickable = false;
        close();
        PlayerNPCDialogueUI.open(declineDialogue, null);
    }

    private static void onClickedContinueButton(ISleekElement button)
    {
        continueButton.isClickable = false;
        close();
        PlayerNPCDialogueUI.registerResponse(declineDialogue, response);
        PlayerNPCDialogueUI.open(acceptDialogue, declineDialogue);
    }

    private static void onClickedTrackButton(ISleekElement button)
    {
        Player.player.quests.sendTrackQuest(quest.id);
        if (!Provider.isServer)
        {
            Player.player.quests.trackQuest(quest.id);
        }
        closeNicely();
    }

    private static void onClickedAbandonButton(ISleekElement button)
    {
        Player.player.quests.sendAbandonQuest(quest.id);
        if (!Provider.isServer)
        {
            Player.player.quests.abandonQuest(quest.id);
        }
        closeNicely();
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        closeNicely();
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
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        questBox = Glazier.Get().CreateBox();
        questBox.positionOffset_X = -250;
        questBox.positionScale_X = 0.5f;
        questBox.sizeOffset_X = 500;
        container.AddChild(questBox);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 5;
        nameLabel.positionOffset_Y = 5;
        nameLabel.sizeOffset_X = -10;
        nameLabel.sizeOffset_Y = 30;
        nameLabel.sizeScale_X = 1f;
        nameLabel.fontAlignment = TextAnchor.UpperLeft;
        nameLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.enableRichText = true;
        nameLabel.fontSize = ESleekFontSize.Medium;
        questBox.AddChild(nameLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.positionOffset_X = 5;
        descriptionLabel.positionOffset_Y = 30;
        descriptionLabel.sizeOffset_X = -10;
        descriptionLabel.sizeOffset_Y = -35;
        descriptionLabel.sizeScale_X = 1f;
        descriptionLabel.sizeScale_Y = 1f;
        descriptionLabel.fontAlignment = TextAnchor.UpperLeft;
        descriptionLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        descriptionLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        descriptionLabel.enableRichText = true;
        questBox.AddChild(descriptionLabel);
        detailsBox = Glazier.Get().CreateScrollView();
        detailsBox.positionOffset_X = 5;
        detailsBox.positionScale_Y = 1f;
        detailsBox.sizeOffset_X = -10;
        detailsBox.sizeScale_X = 1f;
        detailsBox.scaleContentToWidth = true;
        questBox.AddChild(detailsBox);
        conditionsLabel = Glazier.Get().CreateLabel();
        conditionsLabel.sizeOffset_Y = 30;
        conditionsLabel.sizeScale_X = 1f;
        conditionsLabel.fontAlignment = TextAnchor.MiddleLeft;
        conditionsLabel.text = localization.format("Conditions");
        conditionsLabel.fontSize = ESleekFontSize.Medium;
        detailsBox.AddChild(conditionsLabel);
        conditionsContainer = Glazier.Get().CreateFrame();
        conditionsContainer.positionOffset_Y = 30;
        conditionsContainer.sizeScale_X = 1f;
        detailsBox.AddChild(conditionsContainer);
        rewardsLabel = Glazier.Get().CreateLabel();
        rewardsLabel.sizeOffset_Y = 30;
        rewardsLabel.sizeScale_X = 1f;
        rewardsLabel.fontAlignment = TextAnchor.MiddleLeft;
        rewardsLabel.text = localization.format("Rewards");
        rewardsLabel.fontSize = ESleekFontSize.Medium;
        detailsBox.AddChild(rewardsLabel);
        rewardsContainer = Glazier.Get().CreateFrame();
        rewardsContainer.sizeScale_X = 1f;
        detailsBox.AddChild(rewardsContainer);
        beginContainer = Glazier.Get().CreateFrame();
        beginContainer.positionOffset_Y = 10;
        beginContainer.positionScale_Y = 1f;
        beginContainer.sizeOffset_Y = 50;
        beginContainer.sizeScale_X = 1f;
        questBox.AddChild(beginContainer);
        beginContainer.isVisible = false;
        endContainer = Glazier.Get().CreateFrame();
        endContainer.positionOffset_Y = 10;
        endContainer.positionScale_Y = 1f;
        endContainer.sizeOffset_Y = 50;
        endContainer.sizeScale_X = 1f;
        questBox.AddChild(endContainer);
        endContainer.isVisible = false;
        detailsContainer = Glazier.Get().CreateFrame();
        detailsContainer.positionOffset_Y = 10;
        detailsContainer.positionScale_Y = 1f;
        detailsContainer.sizeOffset_Y = 50;
        detailsContainer.sizeScale_X = 1f;
        questBox.AddChild(detailsContainer);
        detailsContainer.isVisible = false;
        acceptButton = Glazier.Get().CreateButton();
        acceptButton.sizeOffset_X = -5;
        acceptButton.sizeScale_X = 0.5f;
        acceptButton.sizeScale_Y = 1f;
        acceptButton.text = localization.format("Accept");
        acceptButton.tooltipText = localization.format("Accept_Tooltip");
        acceptButton.fontSize = ESleekFontSize.Medium;
        acceptButton.onClickedButton += onClickedAcceptButton;
        beginContainer.AddChild(acceptButton);
        declineButton = Glazier.Get().CreateButton();
        declineButton.positionOffset_X = 5;
        declineButton.positionScale_X = 0.5f;
        declineButton.sizeOffset_X = -5;
        declineButton.sizeScale_X = 0.5f;
        declineButton.sizeScale_Y = 1f;
        declineButton.text = localization.format("Decline");
        declineButton.tooltipText = localization.format("Decline_Tooltip");
        declineButton.fontSize = ESleekFontSize.Medium;
        declineButton.onClickedButton += onClickedDeclineButton;
        beginContainer.AddChild(declineButton);
        continueButton = Glazier.Get().CreateButton();
        continueButton.sizeScale_X = 1f;
        continueButton.sizeScale_Y = 1f;
        continueButton.text = localization.format("Continue");
        continueButton.tooltipText = localization.format("Continue_Tooltip");
        continueButton.fontSize = ESleekFontSize.Medium;
        continueButton.onClickedButton += onClickedContinueButton;
        endContainer.AddChild(continueButton);
        trackButton = Glazier.Get().CreateButton();
        trackButton.sizeOffset_X = -5;
        trackButton.sizeScale_X = 0.333f;
        trackButton.sizeScale_Y = 1f;
        trackButton.tooltipText = localization.format("Track_Tooltip");
        trackButton.fontSize = ESleekFontSize.Medium;
        trackButton.onClickedButton += onClickedTrackButton;
        detailsContainer.AddChild(trackButton);
        abandonButton = Glazier.Get().CreateButton();
        abandonButton.positionOffset_X = 5;
        abandonButton.positionScale_X = 0.333f;
        abandonButton.sizeOffset_X = -10;
        abandonButton.sizeScale_X = 0.333f;
        abandonButton.sizeScale_Y = 1f;
        abandonButton.text = localization.format("Abandon");
        abandonButton.tooltipText = localization.format("Abandon_Tooltip");
        abandonButton.fontSize = ESleekFontSize.Medium;
        abandonButton.onClickedButton += onClickedAbandonButton;
        detailsContainer.AddChild(abandonButton);
        returnButton = Glazier.Get().CreateButton();
        returnButton.positionOffset_X = 5;
        returnButton.positionScale_X = 0.667f;
        returnButton.sizeOffset_X = -5;
        returnButton.sizeScale_X = 0.333f;
        returnButton.sizeScale_Y = 1f;
        returnButton.text = localization.format("Return");
        returnButton.tooltipText = localization.format("Return_Tooltip");
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.onClickedButton += onClickedReturnButton;
        detailsContainer.AddChild(returnButton);
    }
}
