using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerNPCDialogueUI
{
    private const string KEYWORD_PAUSE = "<pause>";

    private static SleekFullscreenBox container;

    private static Local localization;

    public static Bundle icons;

    public static bool active;

    private static DialogueAsset dialogue;

    private static DialogueMessage message;

    private static bool hasNextDialogue;

    private static List<DialogueResponse> responses = new List<DialogueResponse>();

    private static ISleekBox dialogueBox;

    private static ISleekLabel characterLabel;

    private static ISleekLabel messageLabel;

    private static ISleekLabel pageLabel;

    private static ISleekScrollView responseBox;

    private static List<SleekButtonIcon> responseButtons = new List<SleekButtonIcon>();

    private static int dialoguePageIndex;

    private static string pageFormattedText;

    private static float pageAnimationTime;

    private static float pauseTimer;

    private static StringBuilder animatedTextBuilder = new StringBuilder();

    private static string animatedTextClosingRichTags;

    private static int animatedCharsVisibleCount;

    private static int pageAnimationTimeVisibleCharsOffset;

    private static float responsesVisibleTime;

    private static int visibleResponsesCount;

    public static bool CanAdvanceToNextPage { get; private set; }

    public static bool IsDialogueAnimating { get; private set; }

    public static void OpenCurrentDialogue()
    {
        open(dialogue, message, hasNextDialogue);
    }

    public static void open(DialogueAsset newDialogue, DialogueMessage newMessage, bool newHasNextDialogue)
    {
        if (active)
        {
            updateDialogue(newDialogue, newMessage, newHasNextDialogue);
            return;
        }
        active = true;
        if (PlayerLifeUI.npc != null && PlayerLifeUI.npc.npcAsset != null)
        {
            characterLabel.Text = PlayerLifeUI.npc.npcAsset.GetNameShownToPlayer(Player.player);
        }
        else
        {
            characterLabel.Text = "null";
        }
        updateDialogue(newDialogue, newMessage, newHasNextDialogue);
        container.AnimateIntoView();
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void AddDefaultGoodbyeResponse()
    {
        Local local = Level.info?.getLocalization();
        string text = ((local == null || !local.has("DefaultGoodbyeResponse")) ? localization.format("Goodbye") : local.format("DefaultGoodbyeResponse"));
        if (!string.IsNullOrEmpty(text))
        {
            responses.Add(new DialogueResponse(0, null, 0, default(Guid), 0, default(Guid), 0, default(Guid), text, null, default(NPCRewardsList)));
        }
    }

    private static void updateDialogue(DialogueAsset newDialogue, DialogueMessage newMessage, bool newHasNextDialogue)
    {
        dialogue = newDialogue;
        message = newMessage;
        hasNextDialogue = newHasNextDialogue;
        if (dialogue == null)
        {
            return;
        }
        responseBox.IsVisible = false;
        responseBox.ContentSizeOffset = Vector2.zero;
        responses.Clear();
        dialogue.getAvailableResponses(Player.player, newMessage.index, responses);
        if (PlayerLifeUI.npc != null)
        {
            PlayerLifeUI.npc.SetFaceOverride(message.faceOverride);
        }
        if (responses.Count == 0 && !hasNextDialogue)
        {
            AddDefaultGoodbyeResponse();
        }
        responseBox.RemoveAllChildren();
        responseButtons.Clear();
        for (int i = 0; i < responses.Count; i++)
        {
            DialogueResponse dialogueResponse = responses[i];
            string text = dialogueResponse.text;
            text = text.Replace("<name_npc>", (PlayerLifeUI.npc != null) ? PlayerLifeUI.npc.npcAsset.GetNameShownToPlayer(Player.player) : "null");
            text = text.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
            QuestAsset questAsset = dialogueResponse.FindQuestAsset();
            Texture2D newIcon = null;
            if (questAsset != null)
            {
                newIcon = ((Player.player.quests.GetQuestStatus(questAsset) != ENPCQuestStatus.READY) ? icons.load<Texture2D>("Quest_Begin") : icons.load<Texture2D>("Quest_End"));
            }
            else if (!dialogueResponse.IsVendorRefNull())
            {
                newIcon = icons.load<Texture2D>("Vendor");
            }
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(newIcon);
            sleekButtonIcon.PositionOffset_Y = i * 30;
            sleekButtonIcon.SizeOffset_Y = 30f;
            sleekButtonIcon.SizeScale_X = 1f;
            sleekButtonIcon.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekButtonIcon.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekButtonIcon.enableRichText = true;
            sleekButtonIcon.text = text;
            sleekButtonIcon.onClickedButton += onClickedResponseButton;
            responseBox.AddChild(sleekButtonIcon);
            sleekButtonIcon.IsVisible = false;
            responseButtons.Add(sleekButtonIcon);
        }
        dialoguePageIndex = 0;
        UpdatePage();
    }

    private static void UpdatePage()
    {
        messageLabel.Text = string.Empty;
        pageLabel.IsVisible = false;
        pageAnimationTime = 0f;
        pauseTimer = 0f;
        animatedTextBuilder.Length = 0;
        animatedTextClosingRichTags = string.Empty;
        animatedCharsVisibleCount = 0;
        pageAnimationTimeVisibleCharsOffset = 0;
        responsesVisibleTime = 0f;
        visibleResponsesCount = 0;
        IsDialogueAnimating = true;
        CanAdvanceToNextPage = false;
        if (message != null && message.pages != null && dialoguePageIndex < message.pages.Length)
        {
            pageFormattedText = message.pages[dialoguePageIndex].text;
            pageFormattedText = pageFormattedText.Replace("<name_npc>", (PlayerLifeUI.npc != null) ? PlayerLifeUI.npc.npcAsset.GetNameShownToPlayer(Player.player) : "null");
            pageFormattedText = pageFormattedText.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
        }
        else
        {
            pageFormattedText = "?";
        }
        if (OptionsSettings.talk)
        {
            SkipAnimation();
        }
    }

    private static bool DoNextCharsMatchKeyword(string text, int index, string keyword)
    {
        if (index + keyword.Length > text.Length)
        {
            return false;
        }
        for (int i = 0; i < keyword.Length; i++)
        {
            if (text[index + i] != keyword[i])
            {
                return false;
            }
        }
        return true;
    }

    private static bool FindNextRichTextMarkupSpan(string text, int index, out int begin, out int end)
    {
        begin = 0;
        end = 0;
        while (index < text.Length)
        {
            if (text[index] == '<')
            {
                if (begin == 0)
                {
                    begin = index;
                }
            }
            else if (text[index] == '>' && (index == text.Length - 1 || text[index + 1] != '<'))
            {
                end = index;
                return true;
            }
            index++;
        }
        return false;
    }

    public static void AdvancePage()
    {
        if (dialoguePageIndex == message.pages.Length - 1)
        {
            Player.player.quests.ClientChooseNextDialogue(dialogue.GUID, message.index);
            return;
        }
        dialoguePageIndex++;
        UpdatePage();
    }

    private static void OnPageAnimationFinished()
    {
        IsDialogueAnimating = false;
        if (message != null && message.pages != null)
        {
            if (dialoguePageIndex < message.pages.Length - 1)
            {
                CanAdvanceToNextPage = true;
                pageLabel.Text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                pageLabel.IsVisible = true;
            }
            else if (dialoguePageIndex == message.pages.Length - 1 && hasNextDialogue)
            {
                CanAdvanceToNextPage = true;
                pageLabel.Text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                pageLabel.IsVisible = true;
                responseBox.IsVisible = true;
            }
            else
            {
                responseBox.IsVisible = true;
            }
        }
        else
        {
            responseBox.IsVisible = true;
        }
    }

    public static void SkipAnimation()
    {
        messageLabel.Text = pageFormattedText.Replace("<pause>", "");
        visibleResponsesCount = responses.Count;
        for (int i = 0; i < responses.Count; i++)
        {
            responseButtons[i].IsVisible = true;
        }
        responseBox.ContentSizeOffset = new Vector2(0f, responses.Count * 30);
        OnPageAnimationFinished();
    }

    public static void UpdateAnimation()
    {
        if (dialogue == null)
        {
            return;
        }
        if (IsDialogueAnimating)
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
            }
            else
            {
                pageAnimationTime += Time.deltaTime;
            }
            int num = Mathf.Min(pageFormattedText.Length, Mathf.CeilToInt(pageAnimationTime * 30f) + pageAnimationTimeVisibleCharsOffset);
            if (animatedCharsVisibleCount == num)
            {
                return;
            }
            while (animatedCharsVisibleCount < pageFormattedText.Length && animatedCharsVisibleCount < num)
            {
                char c = pageFormattedText[animatedCharsVisibleCount];
                if (c == '<')
                {
                    int begin;
                    int end;
                    if (animatedTextClosingRichTags.Length > 0)
                    {
                        num += animatedTextClosingRichTags.Length;
                        animatedCharsVisibleCount += animatedTextClosingRichTags.Length;
                        pageAnimationTimeVisibleCharsOffset += animatedTextClosingRichTags.Length;
                        animatedTextBuilder.Append(animatedTextClosingRichTags);
                        animatedTextClosingRichTags = string.Empty;
                    }
                    else if (DoNextCharsMatchKeyword(pageFormattedText, animatedCharsVisibleCount, "<pause>"))
                    {
                        pauseTimer += 0.5f;
                        num = (animatedCharsVisibleCount += "<pause>".Length);
                        pageAnimationTimeVisibleCharsOffset += "<pause>".Length - 1;
                    }
                    else if (FindNextRichTextMarkupSpan(pageFormattedText, animatedCharsVisibleCount, out begin, out end))
                    {
                        int num2 = end - begin + 1;
                        num += num2;
                        animatedCharsVisibleCount += num2;
                        pageAnimationTimeVisibleCharsOffset += num2;
                        animatedTextBuilder.Append(pageFormattedText.Substring(begin, num2));
                        if (FindNextRichTextMarkupSpan(pageFormattedText, end + 1, out begin, out end))
                        {
                            num2 = end - begin + 1;
                            animatedTextClosingRichTags = pageFormattedText.Substring(begin, num2);
                        }
                    }
                    else
                    {
                        animatedTextBuilder.Append(c);
                        animatedCharsVisibleCount++;
                    }
                }
                else
                {
                    animatedTextBuilder.Append(c);
                    animatedCharsVisibleCount++;
                }
            }
            messageLabel.Text = animatedTextBuilder.ToString() + animatedTextClosingRichTags;
            if (animatedCharsVisibleCount == pageFormattedText.Length)
            {
                OnPageAnimationFinished();
            }
            return;
        }
        responsesVisibleTime += Time.deltaTime;
        int num3 = Mathf.Min(responses.Count, Mathf.FloorToInt(responsesVisibleTime * 10f));
        if (visibleResponsesCount != num3)
        {
            while (visibleResponsesCount < num3)
            {
                responseButtons[visibleResponsesCount].IsVisible = true;
                responseBox.ContentSizeOffset = new Vector2(0f, num3 * 30);
                visibleResponsesCount++;
            }
        }
    }

    private static void onClickedResponseButton(ISleekElement button)
    {
        SetResponseButtonsAreClickable(clickable: false);
        int index = responseBox.FindIndexOfChild(button);
        DialogueResponse dialogueResponse = responses[index];
        QuestAsset questAsset = dialogueResponse.FindQuestAsset();
        if (questAsset != null)
        {
            close();
            PlayerNPCQuestUI.open(questAsset, dialogue, dialogueResponse, (Player.player.quests.GetQuestStatus(questAsset) == ENPCQuestStatus.READY) ? EQuestViewMode.END : EQuestViewMode.BEGIN);
            return;
        }
        DialogueAsset dialogueAsset = dialogueResponse.FindDialogueAsset();
        VendorAsset vendorAsset = dialogueResponse.FindVendorAsset();
        if (dialogueAsset != null || vendorAsset != null)
        {
            Player.player.quests.ClientChooseDialogueResponse(dialogue.GUID, dialogueResponse.index);
            return;
        }
        close();
        PlayerLifeUI.open();
    }

    private static void SetResponseButtonsAreClickable(bool clickable)
    {
        foreach (SleekButtonIcon responseButton in responseButtons)
        {
            responseButton.isClickable = clickable;
        }
    }

    public PlayerNPCDialogueUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerNPCDialogue.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerNPCDialogue/PlayerNPCDialogue.unity3d");
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
        dialogueBox = Glazier.Get().CreateBox();
        dialogueBox.PositionOffset_X = -250f;
        dialogueBox.PositionOffset_Y = -200f;
        dialogueBox.PositionScale_X = 0.5f;
        dialogueBox.PositionScale_Y = 0.85f;
        dialogueBox.SizeOffset_X = 500f;
        dialogueBox.SizeOffset_Y = 100f;
        container.AddChild(dialogueBox);
        characterLabel = Glazier.Get().CreateLabel();
        characterLabel.PositionOffset_X = 5f;
        characterLabel.PositionOffset_Y = 5f;
        characterLabel.SizeOffset_X = -10f;
        characterLabel.SizeOffset_Y = 30f;
        characterLabel.SizeScale_X = 1f;
        characterLabel.TextAlignment = TextAnchor.UpperLeft;
        characterLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        characterLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        characterLabel.AllowRichText = true;
        characterLabel.FontSize = ESleekFontSize.Medium;
        dialogueBox.AddChild(characterLabel);
        messageLabel = Glazier.Get().CreateLabel();
        messageLabel.PositionOffset_X = 5f;
        messageLabel.PositionOffset_Y = 30f;
        messageLabel.SizeOffset_X = -10f;
        messageLabel.SizeOffset_Y = -35f;
        messageLabel.SizeScale_X = 1f;
        messageLabel.SizeScale_Y = 1f;
        messageLabel.TextAlignment = TextAnchor.UpperLeft;
        messageLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        messageLabel.AllowRichText = true;
        dialogueBox.AddChild(messageLabel);
        pageLabel = Glazier.Get().CreateLabel();
        pageLabel.PositionOffset_X = -30f;
        pageLabel.PositionOffset_Y = -30f;
        pageLabel.PositionScale_X = 1f;
        pageLabel.PositionScale_Y = 1f;
        pageLabel.SizeOffset_X = 30f;
        pageLabel.SizeOffset_Y = 30f;
        pageLabel.TextAlignment = TextAnchor.LowerRight;
        dialogueBox.AddChild(pageLabel);
        responseBox = Glazier.Get().CreateScrollView();
        responseBox.PositionOffset_X = -250f;
        responseBox.PositionOffset_Y = -100f;
        responseBox.PositionScale_X = 0.5f;
        responseBox.PositionScale_Y = 0.85f;
        responseBox.SizeOffset_X = 500f;
        responseBox.SizeScale_Y = 0.15f;
        responseBox.ScaleContentToWidth = true;
        container.AddChild(responseBox);
        responseBox.IsVisible = false;
        responseButtons.Clear();
    }
}
