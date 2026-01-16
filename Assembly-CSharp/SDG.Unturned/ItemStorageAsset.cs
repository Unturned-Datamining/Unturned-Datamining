using Unturned.SystemEx;

namespace SDG.Unturned;

public class ItemStorageAsset : ItemBarricadeAsset
{
    protected byte _storage_x;

    protected byte _storage_y;

    protected bool _isDisplay;

    public byte storage_x => _storage_x;

    public byte storage_y => _storage_y;

    public bool isDisplay => _isDisplay;

    public bool shouldCloseWhenOutsideRange { get; protected set; }

    /// <summary>
    /// If true, players can interact with the barricade to open storage.
    /// Defaults to true.
    /// Useful for pre-placed sentries to prevent stealing their guns.
    /// </summary>
    public bool CanPlayersOpen { get; set; }

    /// <summary>
    /// If true, any stored items are despawned rather than dropped when destroyed.
    /// Defaults to false.
    /// </summary>
    public bool ShouldDeleteContainedItemsOnDestroy { get; set; }

    public LevelAsset.DefaultLoadoutItem[] DefaultContainedItems { get; set; }

    public void AddDefaultContainedItemsToStorage(InteractableStorage storage)
    {
        if (storage == null || DefaultContainedItems.IsNullOrEmpty())
        {
            return;
        }
        LevelAsset.DefaultLoadoutItem[] defaultContainedItems = DefaultContainedItems;
        for (int i = 0; i < defaultContainedItems.Length; i++)
        {
            LevelAsset.DefaultLoadoutItem defaultLoadoutItem = defaultContainedItems[i];
            ItemAsset itemAsset = defaultLoadoutItem.ResolveAsset(OnGetDefaultContainedItemsErrorContext);
            if (itemAsset != null)
            {
                for (int j = 0; j < defaultLoadoutItem.amount; j++)
                {
                    storage.items.tryAddItem(new Item(itemAsset, defaultLoadoutItem.origin), isStateUpdatable: false);
                }
            }
        }
        storage.items.onStateUpdated?.Invoke();
    }

    private string OnGetDefaultContainedItemsErrorContext()
    {
        return base.FriendlyNameWithFriendlyType + " default contained items";
    }

    public override byte[] getState(EItemOrigin origin)
    {
        if (isDisplay)
        {
            return new byte[21];
        }
        return new byte[17];
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.HasFlag(EItemDescriptionFlags.Uncategorized) && storage_x > 0 && storage_y > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_StorageDimensions", storage_x, storage_y), 2000);
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _storage_x = p.data.ParseUInt8("Storage_X", 0);
        if (storage_x < 1)
        {
            _storage_x = 1;
        }
        _storage_y = p.data.ParseUInt8("Storage_Y", 0);
        if (storage_y < 1)
        {
            _storage_y = 1;
        }
        _isDisplay = p.data.ContainsKey("Display");
        shouldCloseWhenOutsideRange = p.data.ParseBool("Should_Close_When_Outside_Range");
        CanPlayersOpen = p.data.ParseBool("Can_Players_Open", defaultValue: true);
        ShouldDeleteContainedItemsOnDestroy = p.data.ParseBool("Delete_Contained_Items_On_Destroy");
        if (p.data.TryGetList("Default_Contained_Items", out var node))
        {
            DefaultContainedItems = node.ParseArrayOfStructs<LevelAsset.DefaultLoadoutItem>();
        }
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Storage");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Storage_X", storage_x);
        orAddDeclaration.Append("Storage_Y", storage_y);
        orAddDeclaration.Append("Display", isDisplay);
        orAddDeclaration.Append("Should_Close_When_Outside_Range", shouldCloseWhenOutsideRange);
    }
}
