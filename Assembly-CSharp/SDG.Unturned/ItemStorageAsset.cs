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

    public ItemStorageAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _storage_x = data.readByte("Storage_X", 0);
        if (storage_x < 1)
        {
            _storage_x = 1;
        }
        _storage_y = data.readByte("Storage_Y", 0);
        if (storage_y < 1)
        {
            _storage_y = 1;
        }
        _isDisplay = data.has("Display");
        shouldCloseWhenOutsideRange = data.readBoolean("Should_Close_When_Outside_Range");
    }
}
