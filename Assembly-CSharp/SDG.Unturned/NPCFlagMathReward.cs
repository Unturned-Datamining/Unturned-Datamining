namespace SDG.Unturned;

public class NPCFlagMathReward : INPCReward
{
    private short defaultFlag_B_Value;

    public ushort flag_A_ID { get; protected set; }

    public ushort flag_B_ID { get; protected set; }

    public ENPCOperationType operationType { get; protected set; }

    public override void grantReward(Player player, bool shouldSend)
    {
        player.quests.getFlag(flag_A_ID, out var value);
        if (flag_B_ID == 0 || !player.quests.getFlag(flag_B_ID, out var value2))
        {
            value2 = defaultFlag_B_Value;
        }
        switch (operationType)
        {
        default:
            return;
        case ENPCOperationType.ASSIGN:
            value = value2;
            break;
        case ENPCOperationType.ADDITION:
            value = (short)(value + value2);
            break;
        case ENPCOperationType.SUBTRACTION:
            value = (short)(value - value2);
            break;
        case ENPCOperationType.MULTIPLICATION:
            value = (short)(value * value2);
            break;
        case ENPCOperationType.DIVISION:
            value = (short)(value / value2);
            break;
        case ENPCOperationType.MODULO:
            value = (short)(value % value2);
            break;
        }
        if (shouldSend)
        {
            player.quests.sendSetFlag(flag_A_ID, value);
        }
        else
        {
            player.quests.setFlag(flag_A_ID, value);
        }
    }

    public NPCFlagMathReward(ushort newFlag_A_ID, ushort newFlag_B_ID, short newFlag_B_Value, ENPCOperationType newOperationType, string newText)
        : base(newText)
    {
        flag_A_ID = newFlag_A_ID;
        flag_B_ID = newFlag_B_ID;
        defaultFlag_B_Value = newFlag_B_Value;
        operationType = newOperationType;
    }
}
