using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class DialogueAsset : Asset
{
    public DialogueMessage[] messages { get; protected set; }

    public DialogueResponse[] responses { get; protected set; }

    public override EAssetType assetCategory => EAssetType.NPC;

    public int getAvailableMessage(Player player)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].areConditionsMet(player))
            {
                return i;
            }
        }
        return -1;
    }

    public void getAvailableResponses(Player player, int messageIndex, List<DialogueResponse> availableResponses)
    {
        DialogueMessage dialogueMessage = messages[messageIndex];
        if (dialogueMessage.responses != null && dialogueMessage.responses.Length != 0)
        {
            for (int i = 0; i < dialogueMessage.responses.Length; i++)
            {
                DialogueResponse dialogueResponse = responses[dialogueMessage.responses[i]];
                if (dialogueResponse.areConditionsMet(player))
                {
                    availableResponses.Add(dialogueResponse);
                }
            }
            return;
        }
        for (int j = 0; j < responses.Length; j++)
        {
            DialogueResponse dialogueResponse2 = responses[j];
            if (dialogueResponse2.messages != null && dialogueResponse2.messages.Length != 0)
            {
                bool flag = false;
                for (int k = 0; k < dialogueResponse2.messages.Length; k++)
                {
                    if (dialogueResponse2.messages[k] == messageIndex)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    continue;
                }
            }
            if (dialogueResponse2.areConditionsMet(player))
            {
                availableResponses.Add(dialogueResponse2);
            }
        }
    }

    public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
    {
        return doesPlayerHaveAccessToVendorInternal(player, vendorAsset, 0);
    }

    private bool doesPlayerHaveAccessToVendorInternal(Player player, VendorAsset vendorAsset, int depth)
    {
        int availableMessage = getAvailableMessage(player);
        if (availableMessage < 0)
        {
            return false;
        }
        List<DialogueResponse> list = new List<DialogueResponse>(responses.Length);
        getAvailableResponses(player, availableMessage, list);
        foreach (DialogueResponse item in list)
        {
            if (item.FindVendorAsset() == vendorAsset)
            {
                return true;
            }
        }
        if (depth < 3)
        {
            foreach (DialogueResponse item2 in list)
            {
                DialogueAsset dialogueAsset = item2.FindDialogueAsset();
                if (dialogueAsset != null && dialogueAsset.doesPlayerHaveAccessToVendorInternal(player, vendorAsset, depth + 1))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public DialogueAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 2000 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        int num = data.readInt32("Messages");
        int num2 = data.readByte("Responses", 0);
        messages = new DialogueMessage[num];
        for (byte b = 0; b < messages.Length; b = (byte)(b + 1))
        {
            DialoguePage[] array = new DialoguePage[data.readByte("Message_" + b + "_Pages", 0)];
            for (byte b2 = 0; b2 < array.Length; b2 = (byte)(b2 + 1))
            {
                string desc = localization.format("Message_" + b + "_Page_" + b2);
                desc = ItemTool.filterRarityRichText(desc);
                RichTextUtil.replaceNewlineMarkup(ref desc);
                if (string.IsNullOrEmpty(desc))
                {
                    throw new NotSupportedException("missing message " + b + " page " + b2);
                }
                array[b2] = new DialoguePage(desc);
            }
            byte[] array2 = new byte[data.readByte("Message_" + b + "_Responses", 0)];
            for (byte b3 = 0; b3 < array2.Length; b3 = (byte)(b3 + 1))
            {
                string text = "Message_" + b + "_Response_" + b3;
                array2[b3] = data.readByte(text, 0);
                if (array2[b3] >= num2)
                {
                    Assets.reportError(this, "{0} out of bounds ({1})", text, num2);
                }
            }
            Guid guid;
            ushort newPrev = data.ReadGuidOrLegacyId("Message_" + b + "_Prev", out guid);
            byte? faceOverride = ((!data.has("Message_" + b + "_FaceOverride")) ? null : new byte?(data.readByte("Message_" + b + "_FaceOverride", 0)));
            INPCCondition[] array3 = new INPCCondition[data.readByte("Message_" + b + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Message_" + b + "_Condition_", array3, this);
            INPCReward[] array4 = new INPCReward[data.readByte("Message_" + b + "_Rewards", 0)];
            NPCTool.readRewards(data, localization, "Message_" + b + "_Reward_", array4, this);
            messages[b] = new DialogueMessage(b, array, array2, newPrev, guid, faceOverride, array3, array4);
        }
        responses = new DialogueResponse[num2];
        for (byte b4 = 0; b4 < responses.Length; b4 = (byte)(b4 + 1))
        {
            byte[] array5 = new byte[data.readByte("Response_" + b4 + "_Messages", 0)];
            for (byte b5 = 0; b5 < array5.Length; b5 = (byte)(b5 + 1))
            {
                string text2 = "Response_" + b4 + "_Message_" + b5;
                array5[b5] = data.readByte(text2, 0);
                if (array5[b5] >= num)
                {
                    Assets.reportError(this, "{0} out of bounds ({1})", text2, num);
                }
            }
            Guid guid2;
            ushort newDialogue = data.ReadGuidOrLegacyId("Response_" + b4 + "_Dialogue", out guid2);
            Guid guid3;
            ushort newQuest = data.ReadGuidOrLegacyId("Response_" + b4 + "_Quest", out guid3);
            Guid guid4;
            ushort newVendor = data.ReadGuidOrLegacyId("Response_" + b4 + "_Vendor", out guid4);
            string desc2 = localization.format("Response_" + b4);
            desc2 = ItemTool.filterRarityRichText(desc2);
            RichTextUtil.replaceNewlineMarkup(ref desc2);
            if (string.IsNullOrEmpty(desc2))
            {
                throw new NotSupportedException("missing response " + b4);
            }
            INPCCondition[] array6 = new INPCCondition[data.readByte("Response_" + b4 + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Response_" + b4 + "_Condition_", array6, this);
            INPCReward[] array7 = new INPCReward[data.readByte("Response_" + b4 + "_Rewards", 0)];
            NPCTool.readRewards(data, localization, "Response_" + b4 + "_Reward_", array7, this);
            responses[b4] = new DialogueResponse(b4, array5, newDialogue, guid2, newQuest, guid3, newVendor, guid4, desc2, array6, array7);
        }
    }
}
