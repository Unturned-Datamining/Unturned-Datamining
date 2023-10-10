using UnityEngine;

namespace SDG.Unturned;

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

    public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip)
    {
        foreach (ushort item in spawn())
        {
            player.inventory.forceAddItem(new Item(item, itemOrigin), shouldAutoEquip, playEffect: false);
        }
    }

    public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip, float countMultiplier)
    {
        foreach (ushort item in spawn(countMultiplier))
        {
            player.inventory.forceAddItem(new Item(item, itemOrigin), shouldAutoEquip, playEffect: false);
        }
    }

    public SpawnTableRewardEnumerator spawn()
    {
        return new SpawnTableRewardEnumerator(tableID, count());
    }

    public SpawnTableRewardEnumerator spawn(float multiplier)
    {
        return new SpawnTableRewardEnumerator(tableID, count(multiplier));
    }
}
