namespace SDG.Unturned;

public abstract class VendorSellingBase : VendorElement
{
    public bool canBuy(Player player)
    {
        if (base.outerAsset.currency.isValid)
        {
            ItemCurrencyAsset itemCurrencyAsset = base.outerAsset.currency.Find();
            if (itemCurrencyAsset == null)
            {
                Assets.reportError(base.outerAsset, "missing currency asset");
                return false;
            }
            return itemCurrencyAsset.canAfford(player, base.cost);
        }
        return player.skills.experience >= base.cost;
    }

    public virtual void buy(Player player)
    {
        if (base.outerAsset.currency.isValid)
        {
            ItemCurrencyAsset itemCurrencyAsset = base.outerAsset.currency.Find();
            if (itemCurrencyAsset == null)
            {
                Assets.reportError(base.outerAsset, "missing currency asset");
            }
            else if (!itemCurrencyAsset.spendValue(player, base.cost))
            {
                UnturnedLog.error("Spending {0} currency at vendor went wrong (this should never happen)", base.cost);
            }
        }
        else
        {
            player.skills.askSpend(base.cost);
        }
    }

    public virtual void format(Player player, out ushort total)
    {
        total = 0;
    }

    public VendorSellingBase(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, INPCCondition[] newConditions, INPCReward[] newRewards)
        : base(newOuterAsset, newIndex, newID, newCost, newConditions, newRewards)
    {
    }
}
