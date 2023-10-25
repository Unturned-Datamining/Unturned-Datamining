using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Nelson 2023-08-11: this probably should be rewritten a bit if used in the future
/// because the error context currently assumes this is an item reward for consumables.
/// </summary>
public struct SpawnTableReward
{
    public ushort tableID;

    public int min;

    public int max;

    public SpawnTableReward(ushort tableID, int min, int max)
    {
        this.tableID = tableID;
        this.min = min;
        this.max = max;
    }

    public int count()
    {
        return Random.Range(min, max + 1);
    }

    public int count(float multiplier)
    {
        return Mathf.CeilToInt((float)count() * multiplier);
    }

    /// <summary>
    /// Resolve table as items and grant random number to player.
    /// </summary>
    public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip)
    {
        foreach (ushort item in spawn())
        {
            player.inventory.forceAddItem(new Item(item, itemOrigin), shouldAutoEquip, playEffect: false);
        }
    }

    /// <summary>
    /// Resolve table as items and grant random number to player.
    /// </summary>
    public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip, float countMultiplier)
    {
        foreach (ushort item in spawn(countMultiplier))
        {
            player.inventory.forceAddItem(new Item(item, itemOrigin), shouldAutoEquip, playEffect: false);
        }
    }

    /// <summary>
    /// Enumerate random number of valid assetIDs.
    /// </summary>
    public SpawnTableRewardEnumerator spawn()
    {
        return new SpawnTableRewardEnumerator(tableID, count());
    }

    public SpawnTableRewardEnumerator spawn(float multiplier)
    {
        return new SpawnTableRewardEnumerator(tableID, count(multiplier));
    }
}
