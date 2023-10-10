namespace SDG.Unturned;

public class NPCBoolFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public bool value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.quests.sendSetFlag(id, (short)(value ? 1 : 0));
    }

    public NPCBoolFlagReward(ushort newID, bool newValue, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
    }
}
