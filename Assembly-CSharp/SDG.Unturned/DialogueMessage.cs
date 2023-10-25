using System;

namespace SDG.Unturned;

public class DialogueMessage : DialogueElement
{
    /// <summary>
    /// Please refer to <see cref="M:SDG.Unturned.DialogueMessage.FindPrevDialogueAsset" />.
    /// </summary>
    public Guid prevGuid;

    public DialoguePage[] pages { get; protected set; }

    public byte[] responses { get; protected set; }

    /// <summary>
    /// Please refer to <see cref="M:SDG.Unturned.DialogueMessage.FindPrevDialogueAsset" />.
    /// </summary>
    public ushort prev
    {
        [Obsolete]
        get;
        protected set; }

    internal byte? faceOverride { get; private set; }

    /// <summary>
    /// The dialogue to go to when a message has no available responses.
    /// If this is not specified the previous dialogue is used as a default.
    /// If neither is available then a default "goodbye" response is added.
    ///
    /// For example, Chief_Police_Doughnuts_Accepted dialogue has a single message
    /// "Let's just keep this between the two of us." shown with "prev" dialogue
    /// set to the NPC's root dialogue asset.
    /// </summary>
    public DialogueAsset FindPrevDialogueAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(prevGuid, prev);
    }

    public DialogueMessage(byte newID, DialoguePage[] newPages, byte[] newResponses, ushort newPrev, Guid newPrevGuid, byte? faceOverride, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newID, newConditions, newRewardsList)
    {
        pages = newPages;
        responses = newResponses;
        prev = newPrev;
        prevGuid = newPrevGuid;
        this.faceOverride = faceOverride;
    }
}
