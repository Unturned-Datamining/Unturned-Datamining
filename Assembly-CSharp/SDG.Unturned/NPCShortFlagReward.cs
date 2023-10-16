namespace SDG.Unturned;

public class NPCShortFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public virtual short value { get; protected set; }

    public ENPCModificationType modificationType { get; protected set; }

    public override void GrantReward(Player player)
    {
        if (modificationType == ENPCModificationType.ASSIGN)
        {
            player.quests.sendSetFlag(id, value);
            return;
        }
        player.quests.getFlag(id, out var num);
        if (modificationType == ENPCModificationType.INCREMENT)
        {
            num += value;
        }
        else if (modificationType == ENPCModificationType.DECREMENT)
        {
            num -= value;
        }
        player.quests.sendSetFlag(id, num);
    }

    public NPCShortFlagReward(ushort newID, short newValue, ENPCModificationType newModificationType, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
        modificationType = newModificationType;
    }
}
