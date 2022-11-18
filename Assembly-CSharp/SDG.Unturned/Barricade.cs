using System;

namespace SDG.Unturned;

public class Barricade
{
    public ushort health;

    public byte[] state;

    public bool isDead => health == 0;

    public bool isRepaired => health == asset.health;

    [Obsolete]
    public ushort id => asset.id;

    public ItemBarricadeAsset asset { get; private set; }

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
    public Barricade(ushort newID)
    {
        asset = Assets.find(EAssetType.ITEM, newID) as ItemBarricadeAsset;
        if (asset == null)
        {
            health = 0;
            state = new byte[0];
        }
        else
        {
            health = asset.health;
            state = asset.getState();
        }
    }

    [Obsolete]
    public Barricade(ushort newID, ushort newHealth, byte[] newState, ItemBarricadeAsset newAsset)
    {
        health = newHealth;
        state = newState;
        asset = newAsset;
    }

    public Barricade(ItemBarricadeAsset newAsset)
    {
        asset = newAsset;
        if (asset != null)
        {
            health = asset.health;
            state = asset.getState();
        }
        else
        {
            health = 0;
            state = new byte[0];
        }
    }

    public Barricade(ItemBarricadeAsset newAsset, ushort newHealth, byte[] newState)
    {
        asset = newAsset;
        health = newHealth;
        state = newState;
    }

    public override string ToString()
    {
        return asset?.ToString() + " " + health + " " + state.Length;
    }
}
