using System;

namespace SDG.Unturned;

public abstract class VendorElement
{
    protected NPCConditionsList conditionsList;

    protected NPCRewardsList rewardsList;

    /// <summary>
    /// If not null, replaces item/vehicle description.
    /// </summary>
    protected string descriptionOverride;

    /// <summary>
    /// Vendor asset that owns this buy/sell record.
    /// </summary>
    public VendorAsset outerAsset { get; protected set; }

    public byte index { get; protected set; }

    public Guid TargetAssetGuid { get; protected set; }

    [Obsolete]
    public ushort id { get; protected set; }

    public uint cost { get; protected set; }

    [Obsolete]
    public INPCCondition[] conditions => conditionsList.conditions;

    [Obsolete]
    public INPCReward[] rewards => rewardsList.rewards;

    public abstract string displayName { get; }

    public virtual string displayDesc => null;

    public virtual bool hasIcon => true;

    public abstract EItemRarity rarity { get; }

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

    public VendorElement(VendorAsset newOuterAsset, byte newIndex, Guid newGuid, ushort newLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
    {
        outerAsset = newOuterAsset;
        index = newIndex;
        TargetAssetGuid = newGuid;
        id = newLegacyId;
        cost = newCost;
        conditionsList = newConditionsList;
        rewardsList = newRewardsList;
        descriptionOverride = newDescriptionOverride;
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
