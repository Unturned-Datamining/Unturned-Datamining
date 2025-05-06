using System;

namespace SDG.Unturned;

public class DialogueElement
{
    protected NPCConditionsList conditionsList;

    protected NPCRewardsList rewardsList;

    public byte index { get; protected set; }

    [Obsolete]
    public INPCCondition[] conditions => conditionsList.conditions;

    [Obsolete]
    public INPCReward[] rewards => rewardsList.rewards;

    public bool areConditionsMet(Player player)
    {
        return conditionsList.AreConditionsMet(player);
    }

    public void ApplyConditions(Player player)
    {
        conditionsList.ApplyConditions(player);
    }

    public void GrantRewards(Player player)
    {
        rewardsList.Grant(player);
    }

    public DialogueElement(byte newIndex, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList)
    {
        index = newIndex;
        conditionsList = newConditionsList;
        rewardsList = newRewardsList;
    }

    [Obsolete("Removed shouldSend parameter")]
    public void applyConditions(Player player, bool shouldSend)
    {
        ApplyConditions(player);
    }

    [Obsolete("Removed shouldSend parameter")]
    public void grantRewards(Player player, bool shouldSend)
    {
        GrantRewards(player);
    }
}
