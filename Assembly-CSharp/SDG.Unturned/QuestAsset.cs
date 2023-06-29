using System;

namespace SDG.Unturned;

public class QuestAsset : Asset
{
    protected NPCRewardsList rewardsList;

    public string questName { get; protected set; }

    public string questDescription { get; protected set; }

    public INPCCondition[] conditions { get; protected set; }

    public INPCReward[] rewards => rewardsList.rewards;

    public override EAssetType assetCategory => EAssetType.NPC;

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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        questName = localization.format("Name");
        questName = ItemTool.filterRarityRichText(questName);
        string desc = localization.format("Description");
        desc = ItemTool.filterRarityRichText(desc);
        RichTextUtil.replaceNewlineMarkup(ref desc);
        questDescription = desc;
        conditions = new INPCCondition[data.ParseUInt8("Conditions", 0)];
        NPCTool.readConditions(data, localization, "Condition_", conditions, this);
        rewardsList.Parse(data, localization, this, "Rewards", "Reward_");
    }
}
