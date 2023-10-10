using System;

namespace SDG.Unturned;

public class NPCQuestReward : INPCReward
{
    public Guid questGuid { get; private set; }

    [Obsolete]
    public ushort id { get; protected set; }

    public QuestAsset GetQuestAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<QuestAsset>(questGuid, id);
    }

    public override void GrantReward(Player player)
    {
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            player.quests.ServerAddQuest(questAsset);
        }
    }

    public NPCQuestReward(Guid newQuestGuid, ushort newID, string newText)
        : base(newText)
    {
        questGuid = newQuestGuid;
        id = newID;
    }
}
