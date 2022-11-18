using System.Collections.Generic;

namespace SDG.Unturned;

public class VendorSellingItem : VendorSellingBase
{
    private static List<InventorySearch> search = new List<InventorySearch>();

    public override string displayName
    {
        get
        {
            if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
            {
                return null;
            }
            return itemAsset.itemName;
        }
    }

    public override string displayDesc
    {
        get
        {
            if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
            {
                return null;
            }
            return itemAsset.itemDescription;
        }
    }

    public override EItemRarity rarity
    {
        get
        {
            if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
            {
                return EItemRarity.COMMON;
            }
            return itemAsset.rarity;
        }
    }

    public override void buy(Player player)
    {
        base.buy(player);
        player.inventory.forceAddItem(new Item(base.id, EItemOrigin.ADMIN), auto: false, playEffect: false);
    }

    public override void format(Player player, out ushort total)
    {
        search.Clear();
        player.inventory.search(search, base.id, findEmpty: false, findHealthy: true);
        total = 0;
        foreach (InventorySearch item in search)
        {
            total += item.jar.item.amount;
        }
    }

    public VendorSellingItem(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, INPCCondition[] newConditions, INPCReward[] newRewards)
        : base(newOuterAsset, newIndex, newID, newCost, newConditions, newRewards)
    {
    }
}
