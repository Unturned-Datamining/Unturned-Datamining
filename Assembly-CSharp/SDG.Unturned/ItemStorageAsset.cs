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
        if (!builder.shouldRestrictToLegacyContent && storage_x > 0 && storage_y > 0)
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
