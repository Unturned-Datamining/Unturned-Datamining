using System;

namespace SDG.Unturned;

public class QuestAsset : Asset
{
    protected NPCConditionsList conditionsList;

    protected NPCRewardsList rewardsList;

    /// <summary>
    /// Rewards to grant when quest is removed without completing.
    /// Not granted when player finishes quest.
    /// </summary>
    protected NPCRewardsList abandonmentRewardsList;

    public string questName { get; protected set; }

    public string questDescription { get; protected set; }

    public INPCCondition[] conditions => conditionsList.conditions;

    public INPCReward[] rewards => rewardsList.rewards;

    public override EAssetType assetCategory => EAssetType.NPC;

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

    public void GrantAbandonmentRewards(Player player)
    {
        abandonmentRewardsList.Grant(player);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        questName = p.localization.format("Name");
        questName = ItemTool.filterRarityRichText(questName);
        string desc = p.localization.format("Description");
        desc = ItemTool.filterRarityRichText(desc);
        RichTextUtil.replaceNewlineMarkup(ref desc);
        questDescription = desc;
        conditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");
        rewardsList.Parse(p.data, p.localization, this, "Rewards", "Reward_");
        abandonmentRewardsList.Parse(p.data, p.localization, this, "AbandonmentRewards", "AbandonmentReward_");
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
