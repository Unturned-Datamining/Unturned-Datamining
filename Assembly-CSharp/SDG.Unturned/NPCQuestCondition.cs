using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCQuestCondition : NPCLogicCondition
{
    public Guid questGuid { get; private set; }

    [Obsolete]
    public ushort id { get; protected set; }

    public ENPCQuestStatus status { get; protected set; }

    public bool ignoreNPC { get; protected set; }

    public QuestAsset GetQuestAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<QuestAsset>(questGuid, id);
    }

    public override bool isConditionMet(Player player)
    {
        QuestAsset questAsset = GetQuestAsset();
        return doesLogicPass(player.quests.GetQuestStatus(questAsset), status);
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (!shouldReset)
        {
            return;
        }
        if (shouldSend)
        {
            UnturnedLog.error("Resetting NPC quest condition over network not supported. ID: {0} Status: {1}");
            return;
        }
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            switch (status)
            {
            case ENPCQuestStatus.NONE:
                UnturnedLog.error("Reset none quest status? How should this work?");
                break;
            case ENPCQuestStatus.ACTIVE:
                player.quests.AbandonQuest(questAsset);
                break;
            case ENPCQuestStatus.READY:
                player.quests.CompleteQuest(questAsset, ignoreNPC);
                break;
            case ENPCQuestStatus.COMPLETED:
                player.quests.removeFlag(questAsset.id);
                break;
            }
        }
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        if (id > 0)
        {
            associatedFlags.Add(id);
            return;
        }
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            associatedFlags.Add(questAsset.id);
        }
    }

    public NPCQuestCondition(Guid newQuestGuid, ushort newID, ENPCQuestStatus newStatus, bool newIgnoreNPC, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        questGuid = newQuestGuid;
        id = newID;
        status = newStatus;
        ignoreNPC = newIgnoreNPC;
    }
}
