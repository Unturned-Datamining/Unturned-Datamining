namespace SDG.Unturned;

public class NPCBoolFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public bool value { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (shouldSend)
        {
            player.quests.sendSetFlag(id, (short)(value ? 1 : 0));
        }
        else
        {
            player.quests.setFlag(id, (short)(value ? 1 : 0));
        }
    }

    public NPCBoolFlagReward(ushort newID, bool newValue, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
    }
}
