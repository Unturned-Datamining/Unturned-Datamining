using System.Collections.Generic;

namespace SDG.Unturned;

public class VendorBuying : VendorElement
{
    private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

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

    public bool canSell(Player player)
    {
        if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
        {
            return false;
        }
        search.Clear();
        player.inventory.search(search, base.id, findEmpty: false, findHealthy: true);
        ushort num = 0;
        foreach (InventorySearch item in search)
        {
            num = (ushort)(num + item.jar.item.amount);
        }
        return num >= itemAsset.amount;
    }

    public void sell(Player player)
    {
        if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
        {
            return;
        }
        search.Clear();
        player.inventory.search(search, base.id, findEmpty: false, findHealthy: true);
        search.Sort(qualityAscendingComparator);
        ushort num = itemAsset.amount;
        foreach (InventorySearch item in search)
        {
            if (player.equipment.checkSelection(item.page, item.jar.x, item.jar.y))
            {
                player.equipment.dequip();
            }
            if (item.jar.item.amount > num)
            {
                player.inventory.sendUpdateAmount(item.page, item.jar.x, item.jar.y, (byte)(item.jar.item.amount - num));
                break;
            }
            num = (ushort)(num - item.jar.item.amount);
            player.inventory.sendUpdateAmount(item.page, item.jar.x, item.jar.y, 0);
            player.crafting.removeItem(item.page, item.jar);
            if (item.page < PlayerInventory.SLOTS)
            {
                player.equipment.sendSlot(item.page);
            }
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
        if (!(Assets.find(EAssetType.ITEM, base.id) is ItemAsset itemAsset))
        {
            total = 0;
            amount = 0;
            return;
        }
        search.Clear();
        player.inventory.search(search, base.id, findEmpty: false, findHealthy: true);
        total = 0;
        for (byte b = 0; b < search.Count; b = (byte)(b + 1))
        {
            total += search[b].jar.item.amount;
        }
        amount = itemAsset.amount;
    }

    public VendorBuying(VendorAsset newOuterAsset, byte newIndex, ushort newID, uint newCost, INPCCondition[] newConditions, NPCRewardsList newRewardsList)
        : base(newOuterAsset, newIndex, newID, newCost, newConditions, newRewardsList)
    {
    }
}
