namespace SDG.Unturned;

public class NPCEventReward : INPCReward
{
    public string id { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (Provider.isServer)
        {
            NPCEventManager.broadcastEvent(player, id, shouldReplicate: true);
        }
    }

    public NPCEventReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
