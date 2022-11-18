using System;

namespace SDG.Unturned;

public class Structure
{
    public ushort health;

    public bool isDead => health == 0;

    public bool isRepaired => health == asset.health;

    [Obsolete]
    public ushort id => asset.id;

    public ItemStructureAsset asset { get; private set; }

    public void askDamage(ushort amount)
    {
        if (amount != 0 && !isDead)
        {
            if (amount >= health)
            {
                health = 0;
            }
            else
            {
                health -= amount;
            }
        }
    }

    public void askRepair(ushort amount)
    {
        if (amount != 0 && !isDead)
        {
            if (amount >= asset.health - health)
            {
                health = asset.health;
            }
            else
            {
                health += amount;
            }
        }
    }

    [Obsolete]
    public Structure(ushort newID)
    {
        asset = Assets.find(EAssetType.ITEM, newID) as ItemStructureAsset;
        health = asset.health;
    }

    [Obsolete]
    public Structure(ushort newID, ushort newHealth, ItemStructureAsset newAsset)
    {
        health = newHealth;
        asset = newAsset;
    }

    public Structure(ItemStructureAsset newAsset, ushort newHealth)
    {
        asset = newAsset;
        health = newHealth;
    }

    public override string ToString()
    {
        return asset?.ToString() + " " + health;
    }
}
