using System;

namespace SDG.Unturned;

/// <summary>
/// Represents an item the vendor is selling to players.
/// </summary>
public class VendorSellingItem : VendorSellingBase
{
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
        using ScopedPlayerInventorySearchResultPool scopedPlayerInventorySearchResultPool = default(ScopedPlayerInventorySearchResultPool);
        player.inventory.FindItemsByAsset(scopedPlayerInventorySearchResultPool.PooledResults, itemAsset, includeEmpty: false, includeMaxQuality: true);
        foreach (PlayerInventorySearchResultV2 pooledResult in scopedPlayerInventorySearchResultPool.PooledResults)
        {
            total += pooledResult.Jar.item.amount;
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
            ushort num5 = ((magazine > -1) ? MathfEx.ClampToUShort(magazine) : gunAsset.GetDefaultMagazineLegacyId());
            byte b = ((ammo > -1) ? MathfEx.ClampToByte(ammo) : gunAsset.ammoMax);
            return gunAsset.getState(num, num2, num3, num4, num5, b);
        }
        return null;
    }

    public VendorSellingItem(VendorAsset newOuterAsset, byte newIndex, Guid newTargetAssetGuid, ushort newTargetAssetLegacyId, uint newCost, NPCConditionsList newConditionsList, NPCRewardsList newRewardsList, string newDescriptionOverride, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo)
        : base(newOuterAsset, newIndex, newTargetAssetGuid, newTargetAssetLegacyId, newCost, newConditionsList, newRewardsList, newDescriptionOverride)
    {
        sight = newSight;
        tactical = newTactical;
        grip = newGrip;
        barrel = newBarrel;
        magazine = newMagazine;
        ammo = newAmmo;
    }
}
