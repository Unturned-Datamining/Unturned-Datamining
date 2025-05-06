using System;

namespace SDG.Unturned;

public class NPCRewardsAsset : Asset
{
    private NPCConditionsList conditionsList;

    private NPCRewardsList rewardsList;

    [Obsolete]
    public INPCCondition[] conditions => conditionsList.conditions;

    public bool AreConditionsMet(Player player)
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

    public override string GetTypeFriendlyName()
    {
        return "NPC Rewards List";
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        conditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");
        rewardsList.Parse(p.data, p.localization, this, "Rewards", "Reward_");
    }
}
