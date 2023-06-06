using System;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class DialogueResponse : DialogueElement
{
    public Guid dialogueGuid;

    public Guid questGuid;

    public Guid vendorGuid;

    public byte[] messages { get; protected set; }

    public ushort dialogue
    {
        [Obsolete]
        get;
        protected set; }

    public ushort quest
    {
        [Obsolete]
        get;
        protected set; }

    public ushort vendor
    {
        [Obsolete]
        get;
        protected set; }

    public string text { get; protected set; }

    public bool IsDialogueRefNull()
    {
        if (dialogue == 0)
        {
            return dialogueGuid.IsEmpty();
        }
        return false;
    }

    public DialogueAsset FindDialogueAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(dialogueGuid, dialogue);
    }

    public bool IsQuestRefNull()
    {
        if (quest == 0)
        {
            return questGuid.IsEmpty();
        }
        return false;
    }

    public QuestAsset FindQuestAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<QuestAsset>(questGuid, quest);
    }

    public bool IsVendorRefNull()
    {
        if (vendor == 0)
        {
            return vendorGuid.IsEmpty();
        }
        return false;
    }

    public VendorAsset FindVendorAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<VendorAsset>(vendorGuid, vendor);
    }

    public DialogueResponse(byte newID, byte[] newMessages, ushort newDialogue, Guid newDialogueGuid, ushort newQuest, Guid newQuestGuid, ushort newVendor, Guid newVendorGuid, string newText, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newID, newConditions, newRewardsList)
    {
        messages = newMessages;
        dialogue = newDialogue;
        dialogueGuid = newDialogueGuid;
        quest = newQuest;
        questGuid = newQuestGuid;
        vendor = newVendor;
        vendorGuid = newVendorGuid;
        text = newText;
    }
}
