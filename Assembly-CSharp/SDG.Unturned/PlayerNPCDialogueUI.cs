using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerNPCDialogueUI
{
    private static readonly char[] KEYWORD_PAUSE = "<pause>".ToCharArray();

    private static SleekFullscreenBox container;

    private static Local localization;

    public static Bundle icons;

    public static bool active;

    private static DialogueAsset dialogue;

    private static DialogueMessage message;

    private static DialogueAsset prevDialogue;

    private static List<DialogueResponse> responses = new List<DialogueResponse>();

    private static ISleekBox dialogueBox;

    private static ISleekLabel characterLabel;

    private static ISleekLabel messageLabel;

    private static ISleekLabel pageLabel;

    private static ISleekScrollView responseBox;

    private static List<SleekButtonIcon> responseButtons = new List<SleekButtonIcon>();

    private static byte dialoguePage;

    private static string dialogueText;

    private static char[] dialogueCharacters;

    private static float dialogueTime;

    private static float dialoguePause;

    private static StringBuilder dialogueBuilder = new StringBuilder();

    private static string dialogueAppend;

    private static int dialogueIndex;

    private static int dialogueOffset;

    private static float responseTime;

    private static int responseIndex;

    public static bool dialogueHasNextPage { get; private set; }

    public static bool dialogueAnimating { get; private set; }

    public static void open(DialogueAsset newDialogue, DialogueAsset newPrevDialogue)
    {
        if (!active)
        {
            active = true;
            if (PlayerLifeUI.npc != null && PlayerLifeUI.npc.npcAsset != null)
            {
                characterLabel.text = PlayerLifeUI.npc.npcAsset.npcName;
            }
            else
            {
                characterLabel.text = "?";
            }
            updateDialogue(newDialogue, newPrevDialogue);
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

    private static void updateDialogue(DialogueAsset newDialogue, DialogueAsset newPrevDialogue)
    {
        dialogue = newDialogue;
        prevDialogue = newPrevDialogue;
        if (dialogue == null)
        {
            return;
        }
        responseBox.isVisible = false;
        responseBox.contentSizeOffset = Vector2.zero;
        int availableMessage = dialogue.getAvailableMessage(Player.player);
        if (availableMessage == -1)
        {
            return;
        }
        message = dialogue.messages[availableMessage];
        if ((message.conditions != null && message.conditions.Length != 0) || (message.rewards != null && message.rewards.Length != 0))
        {
            Player.player.quests.sendRegisterMessage(dialogue.GUID);
            if (!Provider.isServer)
            {
                Player.player.quests.registerMessage(dialogue.GUID);
            }
        }
        responses.Clear();
        dialogue.getAvailableResponses(Player.player, availableMessage, responses);
        if (PlayerLifeUI.npc != null)
        {
            PlayerLifeUI.npc.SetFaceOverride(message.faceOverride);
        }
        prevDialogue = message.FindPrevDialogueAsset();
        if (responses.Count == 0)
        {
            if (prevDialogue == null)
            {
                Local local = Level.info?.getLocalization();
                string text = ((local == null || !local.has("DefaultGoodbyeResponse")) ? localization.format("Goodbye") : local.format("DefaultGoodbyeResponse"));
                if (!string.IsNullOrEmpty(text))
                {
                    responses.Add(new DialogueResponse(0, null, 0, default(Guid), 0, default(Guid), 0, default(Guid), text, null, null));
                }
            }
        }
        else
        {
            prevDialogue = null;
        }
        responseBox.RemoveAllChildren();
        responseButtons.Clear();
        for (int i = 0; i < responses.Count; i++)
        {
            DialogueResponse dialogueResponse = responses[i];
            string text2 = dialogueResponse.text;
            text2 = text2.Replace("<name_npc>", (PlayerLifeUI.npc != null) ? PlayerLifeUI.npc.npcAsset.npcName : "?");
            text2 = text2.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
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
            sleekButtonIcon.positionOffset_Y = i * 30;
            sleekButtonIcon.sizeOffset_Y = 30;
            sleekButtonIcon.sizeScale_X = 1f;
            sleekButtonIcon.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekButtonIcon.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekButtonIcon.enableRichText = true;
            sleekButtonIcon.text = text2;
            sleekButtonIcon.onClickedButton += onClickedResponseButton;
            responseBox.AddChild(sleekButtonIcon);
            sleekButtonIcon.isVisible = false;
            responseButtons.Add(sleekButtonIcon);
        }
        dialoguePage = 0;
        updatePage();
    }

    private static void updatePage()
    {
        messageLabel.text = string.Empty;
        pageLabel.isVisible = false;
        dialogueTime = 0f;
        dialoguePause = 0f;
        dialogueBuilder.Length = 0;
        dialogueAppend = string.Empty;
        dialogueIndex = 0;
        dialogueOffset = 0;
        responseTime = 0f;
        responseIndex = 0;
        dialogueAnimating = true;
        dialogueHasNextPage = false;
        if (message != null && message.pages != null && dialoguePage < message.pages.Length)
        {
            dialogueText = message.pages[dialoguePage].text;
            dialogueText = dialogueText.Replace("<name_npc>", (PlayerLifeUI.npc != null) ? PlayerLifeUI.npc.npcAsset.npcName : "?");
            dialogueText = dialogueText.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
        }
        else
        {
            dialogueText = "?";
        }
        dialogueCharacters = dialogueText.ToCharArray();
        if (OptionsSettings.talk)
        {
            skipText();
        }
    }

    private static bool findKeyword(char[] characters, int index, char[] keyword)
    {
        if (index + keyword.Length > characters.Length)
        {
            return false;
        }
        for (int i = 0; i < keyword.Length; i++)
        {
            if (characters[index + i] != keyword[i])
            {
                return false;
            }
        }
        return true;
    }

    private static bool findTags(char[] characters, int index, out int begin, out int end)
    {
        begin = 0;
        end = 0;
        while (index < characters.Length)
        {
            if (characters[index] == '<')
            {
                if (begin == 0)
                {
                    begin = index;
                }
            }
            else if (characters[index] == '>' && (index == characters.Length - 1 || characters[index + 1] != '<'))
            {
                end = index;
                return true;
            }
            index++;
        }
        return false;
    }

    public static void nextPage()
    {
        if (dialoguePage == message.pages.Length - 1)
        {
            updateDialogue(prevDialogue, null);
            return;
        }
        dialoguePage++;
        updatePage();
    }

    private static void finishPage()
    {
        dialogueAnimating = false;
        if (message != null && message.pages != null)
        {
            if (dialoguePage < message.pages.Length - 1)
            {
                dialogueHasNextPage = true;
                pageLabel.text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                pageLabel.isVisible = true;
            }
            else if (dialoguePage == message.pages.Length - 1 && prevDialogue != null)
            {
                dialogueHasNextPage = true;
                pageLabel.text = localization.format("Page", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                pageLabel.isVisible = true;
                responseBox.isVisible = true;
            }
            else
            {
                responseBox.isVisible = true;
            }
        }
        else
        {
            responseBox.isVisible = true;
        }
    }

    public static void skipText()
    {
        messageLabel.text = dialogueText.Replace("<pause>", "");
        responseIndex = responses.Count;
        for (int i = 0; i < responses.Count; i++)
        {
            responseButtons[i].isVisible = true;
        }
        responseBox.contentSizeOffset = new Vector2(0f, responses.Count * 30);
        finishPage();
    }

    public static void updateText()
    {
        if (dialogue == null)
        {
            return;
        }
        if (dialogueAnimating)
        {
            if (dialoguePause > 0f)
            {
                dialoguePause -= Time.deltaTime;
            }
            else
            {
                dialogueTime += Time.deltaTime;
            }
            int num = Mathf.Min(dialogueCharacters.Length, Mathf.CeilToInt(dialogueTime * 30f) + dialogueOffset);
            if (dialogueIndex == num)
            {
                return;
            }
            while (dialogueIndex < dialogueCharacters.Length && dialogueIndex < num)
            {
                char c = dialogueCharacters[dialogueIndex];
                if (c == '<')
                {
                    int begin;
                    int end;
                    if (dialogueAppend.Length > 0)
                    {
                        num += dialogueAppend.Length;
                        dialogueIndex += dialogueAppend.Length;
                        dialogueOffset += dialogueAppend.Length;
                        dialogueBuilder.Append(dialogueAppend);
                        dialogueAppend = string.Empty;
                    }
                    else if (findKeyword(dialogueCharacters, dialogueIndex, KEYWORD_PAUSE))
                    {
                        dialoguePause += 0.5f;
                        num = (dialogueIndex += KEYWORD_PAUSE.Length);
                        dialogueOffset += KEYWORD_PAUSE.Length - 1;
                    }
                    else if (findTags(dialogueCharacters, dialogueIndex, out begin, out end))
                    {
                        int num2 = end - begin + 1;
                        num += num2;
                        dialogueIndex += num2;
                        dialogueOffset += num2;
                        dialogueBuilder.Append(dialogueText.Substring(begin, num2));
                        if (findTags(dialogueCharacters, end + 1, out begin, out end))
                        {
                            num2 = end - begin + 1;
                            dialogueAppend = dialogueText.Substring(begin, num2);
                        }
                    }
                }
                else
                {
                    dialogueBuilder.Append(c);
                    dialogueIndex++;
                }
            }
            messageLabel.text = dialogueBuilder.ToString() + dialogueAppend;
            if (dialogueIndex == dialogueCharacters.Length)
            {
                finishPage();
            }
            return;
        }
        responseTime += Time.deltaTime;
        int num3 = Mathf.Min(responses.Count, Mathf.FloorToInt(responseTime * 10f));
        if (responseIndex != num3)
        {
            while (responseIndex < num3)
            {
                responseButtons[responseIndex].isVisible = true;
                responseBox.contentSizeOffset = new Vector2(0f, num3 * 30);
                responseIndex++;
            }
        }
    }

    public static void registerResponse(DialogueAsset dialogue, DialogueResponse response)
    {
        if (dialogue != null && response != null && ((response.conditions != null && response.conditions.Length != 0) || (response.rewards != null && response.rewards.Length != 0)))
        {
            Player.player.quests.sendRegisterResponse(dialogue.GUID, response.index);
            if (!Provider.isServer)
            {
                Player.player.quests.registerResponse(dialogue.GUID, response.index);
            }
        }
    }

    private static void onClickedResponseButton(ISleekElement button)
    {
        setResponseButtonsAreClickable(clickable: false);
        byte index = (byte)responseBox.FindIndexOfChild(button);
        DialogueResponse dialogueResponse = responses[index];
        DialogueAsset dialogueAsset = dialogueResponse.FindDialogueAsset();
        QuestAsset questAsset = dialogueResponse.FindQuestAsset();
        if (questAsset != null)
        {
            close();
            PlayerNPCQuestUI.open(questAsset, dialogueResponse, dialogueAsset, dialogue, (Player.player.quests.GetQuestStatus(questAsset) == ENPCQuestStatus.READY) ? EQuestViewMode.END : EQuestViewMode.BEGIN);
            return;
        }
        registerResponse(dialogue, dialogueResponse);
        VendorAsset vendorAsset = dialogueResponse.FindVendorAsset();
        if (vendorAsset != null)
        {
            close();
            DialogueAsset newPrevDialogue = dialogue;
            if (dialogueAsset == null)
            {
                dialogueAsset = dialogue;
                newPrevDialogue = prevDialogue;
            }
            PlayerNPCVendorUI.open(vendorAsset, dialogueResponse, dialogueAsset, newPrevDialogue);
        }
        else if (dialogueAsset != null)
        {
            updateDialogue(dialogueAsset, dialogue);
        }
        else
        {
            close();
            PlayerLifeUI.open();
        }
    }

    private static void setResponseButtonsAreClickable(bool clickable)
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
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        dialogueBox = Glazier.Get().CreateBox();
        dialogueBox.positionOffset_X = -250;
        dialogueBox.positionOffset_Y = -200;
        dialogueBox.positionScale_X = 0.5f;
        dialogueBox.positionScale_Y = 0.85f;
        dialogueBox.sizeOffset_X = 500;
        dialogueBox.sizeOffset_Y = 100;
        container.AddChild(dialogueBox);
        characterLabel = Glazier.Get().CreateLabel();
        characterLabel.positionOffset_X = 5;
        characterLabel.positionOffset_Y = 5;
        characterLabel.sizeOffset_X = -10;
        characterLabel.sizeOffset_Y = 30;
        characterLabel.sizeScale_X = 1f;
        characterLabel.fontAlignment = TextAnchor.UpperLeft;
        characterLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        characterLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        characterLabel.enableRichText = true;
        characterLabel.fontSize = ESleekFontSize.Medium;
        dialogueBox.AddChild(characterLabel);
        messageLabel = Glazier.Get().CreateLabel();
        messageLabel.positionOffset_X = 5;
        messageLabel.positionOffset_Y = 30;
        messageLabel.sizeOffset_X = -10;
        messageLabel.sizeOffset_Y = -35;
        messageLabel.sizeScale_X = 1f;
        messageLabel.sizeScale_Y = 1f;
        messageLabel.fontAlignment = TextAnchor.UpperLeft;
        messageLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        messageLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        messageLabel.enableRichText = true;
        dialogueBox.AddChild(messageLabel);
        pageLabel = Glazier.Get().CreateLabel();
        pageLabel.positionOffset_X = -30;
        pageLabel.positionOffset_Y = -30;
        pageLabel.positionScale_X = 1f;
        pageLabel.positionScale_Y = 1f;
        pageLabel.sizeOffset_X = 30;
        pageLabel.sizeOffset_Y = 30;
        pageLabel.fontAlignment = TextAnchor.LowerRight;
        dialogueBox.AddChild(pageLabel);
        responseBox = Glazier.Get().CreateScrollView();
        responseBox.positionOffset_X = -250;
        responseBox.positionOffset_Y = -100;
        responseBox.positionScale_X = 0.5f;
        responseBox.positionScale_Y = 0.85f;
        responseBox.sizeOffset_X = 500;
        responseBox.sizeScale_Y = 0.15f;
        responseBox.scaleContentToWidth = true;
        container.AddChild(responseBox);
        responseBox.isVisible = false;
        responseButtons.Clear();
    }
}
