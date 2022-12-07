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

    public override void grantReward(Player player, bool shouldSend)
    {
        QuestAsset questAsset = GetQuestAsset();
        if (questAsset != null)
        {
            if (shouldSend)
            {
                player.quests.ServerAddQuest(questAsset);
            }
            else
            {
                player.quests.AddQuest(questAsset);
            }
        }
    }

    public NPCQuestReward(Guid newQuestGuid, ushort newID, string newText)
        : base(newText)
    {
        questGuid = newQuestGuid;
        id = newID;
    }
}
