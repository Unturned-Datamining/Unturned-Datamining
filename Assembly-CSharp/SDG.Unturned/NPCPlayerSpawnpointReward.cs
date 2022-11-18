namespace SDG.Unturned;

public class NPCPlayerSpawnpointReward : INPCReward
{
    public string id { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (Provider.isServer)
        {
            player.quests.npcSpawnId = id;
        }
    }

    public NPCPlayerSpawnpointReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
