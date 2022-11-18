namespace SDG.Unturned;

public class NPCQuestReward : INPCReward
{
    public ushort id { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            player.quests.sendAddQuest(id);
        }
        else
        {
            player.quests.addQuest(id);
        }
    }

    public NPCQuestReward(ushort newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
