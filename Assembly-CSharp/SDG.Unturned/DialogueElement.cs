namespace SDG.Unturned;

public class DialogueElement
{
    public byte index { get; protected set; }

    public INPCCondition[] conditions { get; protected set; }

    public INPCReward[] rewards { get; protected set; }

    public bool areConditionsMet(Player player)
    {
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (!conditions[i].isConditionMet(player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void applyConditions(Player player, bool shouldSend)
    {
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                conditions[i].applyCondition(player, shouldSend);
            }
        }
    }

    public void grantRewards(Player player, bool shouldSend)
    {
        if (rewards != null)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].grantReward(player, shouldSend);
            }
        }
    }

    public DialogueElement(byte newIndex, INPCCondition[] newConditions, INPCReward[] newRewards)
    {
        index = newIndex;
        conditions = newConditions;
        rewards = newRewards;
    }
}
