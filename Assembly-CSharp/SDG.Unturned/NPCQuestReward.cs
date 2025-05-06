using System;

namespace SDG.Unturned;

public class NPCQuestReward : INPCReward
{
    private CachingBcAssetRef _questAssetRef;

    public CachingBcAssetRef QuestAssetRef => _questAssetRef;

    [Obsolete]
    public Guid questGuid => QuestAssetRef.Guid;

    [Obsolete]
    public ushort id => QuestAssetRef.LegacyId;

    public QuestAsset GetQuestAsset()
    {
        return _questAssetRef.Get<QuestAsset>();
    }

    public override void GrantReward(Player player)
    {
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            player.quests.ServerAddQuest(questAsset);
        }
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (!p.data.TryParseBcAssetRef("ID", EAssetType.NPC, out _questAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.NPC, out _questAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
    }

    public NPCQuestReward()
    {
    }

    [Obsolete]
    public NPCQuestReward(Guid newQuestGuid, ushort newID, string newText)
        : base(newText)
    {
        _questAssetRef = new CachingBcAssetRef(newQuestGuid, EAssetType.NPC, newID);
    }
}
