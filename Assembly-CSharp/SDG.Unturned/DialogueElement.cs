namespace SDG.Unturned;

public class DialogueElement
{
    protected NPCRewardsList rewardsList;

    public byte index { get; protected set; }

    public INPCCondition[] conditions { get; protected set; }

    public INPCReward[] rewards => rewardsList.rewards;

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
        rewardsList.Grant(player, shouldSend);
    }

    public DialogueElement(byte newIndex, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
    {
        index = newIndex;
        conditions = newConditions;
        rewardsList = newRewardsList;
    }
}
