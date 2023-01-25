namespace SDG.Unturned;

public class Item
{
    private ushort _id;

    public byte amount;

    public byte quality;

    public byte[] state;

    public ushort id => _id;

    public byte durability
    {
        get
        {
            return quality;
        }
        set
        {
            quality = value;
        }
    }

    public byte[] metadata
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
        }
    }

    public ItemAsset GetAsset()
    {
        return Assets.find(EAssetType.ITEM, id) as ItemAsset;
    }

    public T GetAsset<T>() where T : ItemAsset
    {
        return Assets.find(EAssetType.ITEM, id) as T;
    }

    public Item(ushort newID, bool full)
        : this(newID, full ? EItemOrigin.ADMIN : EItemOrigin.WORLD)
    {
    }

    public Item(ItemAsset asset, EItemOrigin origin)
        : this(asset?.id ?? 0, origin)
    {
    }

    public Item(ushort newID, EItemOrigin origin)
    {
        _id = newID;
        if (!(Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset))
        {
            state = new byte[0];
            return;
        }
        if (origin == EItemOrigin.WORLD && !Provider.modeConfigData.Items.Has_Durability)
        {
            origin = EItemOrigin.CRAFT;
        }
        if (origin != 0)
        {
            amount = MathfEx.Max(itemAsset.amount, 1);
            quality = 100;
        }
        else
        {
            amount = MathfEx.Max(itemAsset.count, 1);
            quality = MathfEx.Clamp(itemAsset.quality, 0, 100);
        }
        state = itemAsset.getState(origin);
    }

    public Item(ushort newID, bool full, byte newQuality)
        : this(newID, full ? EItemOrigin.ADMIN : EItemOrigin.WORLD, newQuality)
    {
    }

    public Item(ushort newID, EItemOrigin origin, byte newQuality)
    {
        _id = newID;
        quality = newQuality;
        if (!(Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset))
        {
            state = new byte[0];
            return;
        }
        if (origin == EItemOrigin.WORLD && !Provider.modeConfigData.Items.Has_Durability)
        {
            origin = EItemOrigin.CRAFT;
        }
        if (origin != 0)
        {
            amount = MathfEx.Max(itemAsset.amount, 1);
        }
        else
        {
            amount = MathfEx.Max(itemAsset.count, 1);
        }
        state = itemAsset.getState(origin);
    }

    public Item(ushort newID, byte newAmount, byte newQuality)
    {
        _id = newID;
        amount = newAmount;
        quality = newQuality;
        if (!(Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset))
        {
            state = new byte[0];
        }
        else
        {
            state = itemAsset.getState();
        }
    }

    public Item(ushort newID, byte newAmount, byte newQuality, byte[] newState)
    {
        _id = newID;
        amount = newAmount;
        quality = newQuality;
        state = ((newState != null) ? newState : new byte[0]);
    }

    public override string ToString()
    {
        return id + " " + amount + " " + quality + " " + state.Length;
    }
}
