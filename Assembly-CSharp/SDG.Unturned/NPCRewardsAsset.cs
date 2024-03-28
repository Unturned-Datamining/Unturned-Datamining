namespace SDG.Unturned;

public class NPCRewardsAsset : Asset
{
    private NPCRewardsList rewardsList;

    public INPCCondition[] conditions { get; private set; }

    public bool AreConditionsMet(Player player)
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

    public void ApplyConditions(Player player)
    {
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                conditions[i].ApplyCondition(player);
            }
        }
    }

    public void GrantRewards(Player player)
    {
        rewardsList.Grant(player);
    }

    public override string GetTypeFriendlyName()
    {
        return "NPC Rewards List";
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        conditions = new INPCCondition[data.ParseUInt8("Conditions", 0)];
        NPCTool.readConditions(data, localization, "Condition_", conditions, this);
        rewardsList.Parse(data, localization, this, "Rewards", "Reward_");
    }
}
