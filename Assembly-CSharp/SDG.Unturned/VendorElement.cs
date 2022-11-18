namespace SDG.Unturned;

public abstract class VendorElement
{
    public VendorAsset outerAsset { get; protected set; }

    public byte index { get; protected set; }

    public ushort id { get; protected set; }

    public uint cost { get; protected set; }

    public INPCCondition[] conditions { get; protected set; }

    public INPCReward[] rewards { get; protected set; }

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

    public VendorElement(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, INPCCondition[] newConditions, INPCReward[] newRewards)
    {
        outerAsset = newOuterAsset;
        index = newIndex;
        id = newID;
        cost = newCost;
        conditions = newConditions;
        rewards = newRewards;
    }
}
