using System;

namespace SDG.Unturned;

public class DialogueMessage : DialogueElement
{
    public Guid prevGuid;

    public DialoguePage[] pages { get; protected set; }

    public byte[] responses { get; protected set; }

    public ushort prev
    {
        [Obsolete]
        get;
        protected set; }

    internal byte? faceOverride { get; private set; }

    public DialogueAsset FindPrevDialogueAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(prevGuid, prev);
    }

    public DialogueMessage(byte newID, DialoguePage[] newPages, byte[] newResponses, ushort newPrev, Guid newPrevGuid, byte? faceOverride, INPCCondition[] newConditions, INPCReward[] newRewards)
        : base(newID, newConditions, newRewards)
    {
        pages = newPages;
        responses = newResponses;
        prev = newPrev;
        prevGuid = newPrevGuid;
        this.faceOverride = faceOverride;
    }
}
