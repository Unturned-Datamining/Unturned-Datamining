using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCQuestCondition : NPCLogicCondition
{
    private CachingBcAssetRef _questAssetRef;

    public CachingBcAssetRef QuestAssetRef => _questAssetRef;

    public ENPCQuestStatus status { get; protected set; }

    public bool ignoreNPC { get; protected set; }

    [Obsolete]
    public Guid questGuid => QuestAssetRef.Guid;

    [Obsolete]
    public ushort id => QuestAssetRef.LegacyId;

    public QuestAsset GetQuestAsset()
    {
        return _questAssetRef.Get<QuestAsset>();
    }

    public override bool isConditionMet(Player player)
    {
        QuestAsset questAsset = GetQuestAsset();
        return doesLogicPass(player.quests.GetQuestStatus(questAsset), status);
    }

    public override void ApplyCondition(Player player)
    {
        if (!shouldReset)
        {
            return;
        }
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            switch (status)
            {
            case ENPCQuestStatus.ACTIVE:
                player.quests.ServerRemoveQuest(questAsset);
                break;
            case ENPCQuestStatus.READY:
                player.quests.CompleteQuest(questAsset, ignoreNPC);
                break;
            case ENPCQuestStatus.COMPLETED:
                player.quests.sendRemoveFlag(questAsset.id);
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

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (!p.data.TryParseBcAssetRef("ID", EAssetType.NPC, out _questAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseEnum<ENPCQuestStatus>("Status", out var value))
        {
            status = value;
            if (value == ENPCQuestStatus.NONE && shouldReset)
            {
                p.ReportError("Quest condition has Reset enabled with Status None (probably accidental)");
            }
        }
        else
        {
            p.ReportRequiredOptionInvalid("Status");
        }
        ignoreNPC = p.data.ParseBool("Ignore_NPC");
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.NPC, out _questAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseEnum<ENPCQuestStatus>(p.legacyPrefix + "_Status", out var value))
        {
            status = value;
            if (value == ENPCQuestStatus.NONE && shouldReset)
            {
                p.ReportError("Quest condition has Reset enabled with Status None (probably accidental)");
            }
        }
        else
        {
            p.ReportRequiredOptionInvalid("Status");
        }
        ignoreNPC = p.data.ContainsKey(p.legacyPrefix + "_Ignore_NPC");
    }

    public NPCQuestCondition()
    {
    }

    [Obsolete]
    public NPCQuestCondition(Guid newQuestGuid, ushort newID, ENPCQuestStatus newStatus, bool newIgnoreNPC, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        _questAssetRef = new CachingBcAssetRef(newQuestGuid, EAssetType.NPC, newID);
        status = newStatus;
        ignoreNPC = newIgnoreNPC;
    }
}
