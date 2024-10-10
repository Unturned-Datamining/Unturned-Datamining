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

    public int sight { get; protected set; }

    public int tactical { get; protected set; }

    public int grip { get; protected set; }

    public int barrel { get; protected set; }

    public int magazine { get; protected set; }

    public int ammo { get; protected set; }

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
            byte[] array = null;
            if (itemAsset is ItemGunAsset gunAsset)
            {
                array = GetGunStateOverride(gunAsset);
            }
            Item item = ((array == null) ? new Item(itemAsset.id, EItemOrigin.ADMIN) : new Item(itemAsset.id, 1, 100, array));
            player.inventory.forceAddItem(item, auto: false, playEffect: false);
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

    /// <summary>
    /// Refer to NPCItemReward state.
    /// </summary>
    internal byte[] GetGunStateOverride(ItemGunAsset gunAsset)
    {
        if (sight > -1 || tactical > -1 || grip > -1 || barrel > -1 || magazine > -1 || ammo > -1)
        {
            ushort num = ((sight > -1) ? MathfEx.ClampToUShort(sight) : gunAsset.sightID);
            ushort num2 = ((tactical > -1) ? MathfEx.ClampToUShort(tactical) : gunAsset.tacticalID);
            ushort num3 = ((grip > -1) ? MathfEx.ClampToUShort(grip) : gunAsset.gripID);
            ushort num4 = ((barrel > -1) ? MathfEx.ClampToUShort(barrel) : gunAsset.barrelID);
            ushort num5 = ((magazine > -1) ? MathfEx.ClampToUShort(magazine) : gunAsset.getMagazineID());
            byte b = ((ammo > -1) ? MathfEx.ClampToByte(ammo) : gunAsset.ammoMax);
            return gunAsset.getState(num, num2, num3, num4, num5, b);
        }
        return null;
    }

    public VendorSellingItem(VendorAsset newOuterAsset, byte newIndex, Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, INPCCondition[] newConditions, NPCRewardsList newRewardsList, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo)
        : base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditions, newRewardsList)
    {
        sight = newSight;
        tactical = newTactical;
        grip = newGrip;
        barrel = newBarrel;
        magazine = newMagazine;
        ammo = newAmmo;
    }
}
