using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Represents an item the vendor is selling to players.
/// </summary>
public class VendorSellingItem : VendorSellingBase
{
    private static List<InventorySearch> search = new List<InventorySearch>();

    public override string displayName => FindItemAsset()?.itemName;

    public override string displayDesc => FindItemAsset()?.itemDescription;

    public override EItemRarity rarity => FindItemAsset()?.rarity ?? EItemRarity.COMMON;

    public ItemAsset FindItemAsset()
    {
        return Assets.FindItemByGuidOrLegacyId<ItemAsset>(base.TargetAssetGuid, base.id);
    }

    public override void buy(Player player)
    {
        base.buy(player);
        ItemAsset itemAsset = FindItemAsset();
        if (itemAsset != null)
        {
            player.inventory.forceAddItem(new Item(itemAsset.id, EItemOrigin.ADMIN), auto: false, playEffect: false);
        }
    }

    public override void format(Player player, out ushort total)
    {
        total = 0;
        ItemAsset itemAsset = FindItemAsset();
        if (itemAsset == null)
        {
            return;
        }
        search.Clear();
        player.inventory.search(search, itemAsset.id, findEmpty: false, findHealthy: true);
        foreach (InventorySearch item in search)
        {
            total += item.jar.item.amount;
        }
    }

    public VendorSellingItem(VendorAsset newOuterAsset, byte newIndex, Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditions, newRewardsList)
    {
    }
}
