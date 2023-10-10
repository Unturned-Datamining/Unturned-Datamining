using System;

namespace SDG.Unturned;

public abstract class VendorElement
{
    protected NPCRewardsList rewardsList;

    public VendorAsset outerAsset { get; protected set; }

    public byte index { get; protected set; }

    public ushort id { get; protected set; }

    public uint cost { get; protected set; }

    public INPCCondition[] conditions { get; protected set; }

    public INPCReward[] rewards => rewardsList.rewards;

    public abstract string displayName { get; }

    public virtual string displayDesc => null;

    public virtual bool hasIcon => true;

    public abstract EItemRarity rarity { get; }

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

    public VendorElement(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
    {
        outerAsset = newOuterAsset;
        index = newIndex;
        id = newID;
        cost = newCost;
        conditions = newConditions;
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
