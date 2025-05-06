using System;

namespace SDG.Unturned;

/// <summary>
/// Represents an item the vendor is buying from players.
/// </summary>
public class VendorBuying : VendorElement
{
    private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

    public override string displayName => FindItemAsset()?.itemName;

    public override string displayDesc
    {
        get
        {
            if (descriptionOverride != null)
            {
                return descriptionOverride;
            }
            return FindItemAsset()?.itemDescription;
        }
    }

    public override EItemRarity rarity => FindItemAsset()?.rarity ?? EItemRarity.COMMON;

    public ItemAsset FindItemAsset()
    {
        return Assets.FindItemByGuidOrLegacyId<ItemAsset>(base.TargetAssetGuid, base.id);
    }

    public bool canSell(Player player)
    {
        ItemAsset itemAsset = FindItemAsset();
        if (itemAsset == null)
        {
            return false;
        }
        using ScopedPlayerInventorySearchResultPool scopedPlayerInventorySearchResultPool = default(ScopedPlayerInventorySearchResultPool);
        player.inventory.FindItemsByAsset(scopedPlayerInventorySearchResultPool.PooledResults, itemAsset, includeEmpty: false, includeMaxQuality: true);
        ushort num = 0;
        foreach (PlayerInventorySearchResultV2 pooledResult in scopedPlayerInventorySearchResultPool.PooledResults)
        {
            num += pooledResult.Jar.item.amount;
        }
        return num >= itemAsset.MaxAmount;
    }

    public void sell(Player player)
    {
        ItemAsset itemAsset = FindItemAsset();
        if (itemAsset == null)
        {
            return;
        }
        using ScopedPlayerInventorySearchResultPool scopedPlayerInventorySearchResultPool = default(ScopedPlayerInventorySearchResultPool);
        player.inventory.FindItemsByAsset(scopedPlayerInventorySearchResultPool.PooledResults, itemAsset, includeEmpty: false, includeMaxQuality: true);
        scopedPlayerInventorySearchResultPool.PooledResults.Sort(qualityAscendingComparator);
        int num = itemAsset.MaxAmount;
        foreach (PlayerInventorySearchResultV2 pooledResult in scopedPlayerInventorySearchResultPool.PooledResults)
        {
            uint num2 = pooledResult.DeleteAmount(player, (uint)num);
            num -= (int)num2;
            if (num == 0)
            {
                break;
            }
        }
        if (base.outerAsset.currency.isValid)
        {
            base.outerAsset.currency.Find()?.grantValue(player, base.cost);
        }
        else
        {
            player.skills.askAward(base.cost);
        }
    }

    public void format(Player player, out ushort total, out byte amount)
    {
        ItemAsset itemAsset = FindItemAsset();
        if (itemAsset == null)
        {
            total = 0;
            amount = 0;
            return;
        }
        using ScopedPlayerInventorySearchResultPool scopedPlayerInventorySearchResultPool = default(ScopedPlayerInventorySearchResultPool);
        player.inventory.FindItemsByAsset(scopedPlayerInventorySearchResultPool.PooledResults, itemAsset, includeEmpty: false, includeMaxQuality: true);
        total = 0;
        for (byte b = 0; b < scopedPlayerInventorySearchResultPool.PooledResults.Count; b++)
        {
            total += scopedPlayerInventorySearchResultPool.PooledResults[b].Jar.item.amount;
        }
        amount = itemAsset.MaxAmountAsByte;
    }

    public VendorBuying(VendorAsset newOuterAsset, byte newIndex, Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride)
        : base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
    {
    }
}
