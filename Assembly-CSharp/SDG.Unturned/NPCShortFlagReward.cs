namespace SDG.Unturned;

public class NPCShortFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public virtual short value { get; protected set; }

    public ENPCModificationType modificationType { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (modificationType == ENPCModificationType.ASSIGN)
        {
            if (shouldSend)
            {
                player.quests.sendSetFlag(id, value);
            }
            else
            {
                player.quests.setFlag(id, value);
            }
            return;
        }
        player.quests.getFlag(id, out var num);
        if (modificationType == ENPCModificationType.INCREMENT)
        {
            num = (short)(num + value);
        }
        else if (modificationType == ENPCModificationType.DECREMENT)
        {
            num = (short)(num - value);
        }
        if (shouldSend)
        {
            player.quests.sendSetFlag(id, num);
        }
        else
        {
            player.quests.setFlag(id, num);
        }
    }

    public NPCShortFlagReward(ushort newID, short newValue, ENPCModificationType newModificationType, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
        modificationType = newModificationType;
    }
}
