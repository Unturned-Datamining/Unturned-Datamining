namespace SDG.Unturned;

public class Item
{
    private ushort _id;

    public byte amount;

    public byte quality;

    public byte[] state;

    public ushort id => _id;

    /// <summary>
    /// Exposed for Rocket transition to modules backwards compatibility.
    /// </summary>
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

    /// <summary>
    /// Exposed for Rocket transition to modules backwards compatibility.
    /// </summary>
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

    /// <summary>
    /// Ideally in a future rewrite asset overload will become the default rather than the overload taking legacy ID.
    /// </summary>
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
        if (origin != 0)
        {
            amount = MathfEx.Max(itemAsset.amount, 1);
        }
        else
        {
            amount = MathfEx.Max(itemAsset.count, 1);
        }
        if (origin != 0 || ShouldItemTypeSpawnAtFullQuality(itemAsset.type))
        {
            quality = 100;
        }
        else
        {
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

    /// <summary>
    /// If true, item has 100% quality. If false, item has a random quality.
    /// </summary>
    private static bool ShouldItemTypeSpawnAtFullQuality(EItemType type)
    {
        switch (type)
        {
        case EItemType.HAT:
        case EItemType.PANTS:
        case EItemType.SHIRT:
        case EItemType.MASK:
        case EItemType.BACKPACK:
        case EItemType.VEST:
        case EItemType.GLASSES:
            return Provider.modeConfigData.Items.Clothing_Spawns_At_Full_Quality;
        case EItemType.FOOD:
            return Provider.modeConfigData.Items.Food_Spawns_At_Full_Quality;
        case EItemType.WATER:
            return Provider.modeConfigData.Items.Water_Spawns_At_Full_Quality;
        case EItemType.GUN:
        case EItemType.MELEE:
            return Provider.modeConfigData.Items.Weapons_Spawn_At_Full_Quality;
        default:
            return Provider.modeConfigData.Items.Default_Spawns_At_Full_Quality;
        }
    }
}
