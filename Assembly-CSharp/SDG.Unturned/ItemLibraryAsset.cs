namespace SDG.Unturned;

public class ItemLibraryAsset : ItemBarricadeAsset
{
    protected uint _capacity;

    protected byte _tax;

    public uint capacity => _capacity;

    public byte tax => _tax;

    public override byte[] getState(EItemOrigin origin)
    {
        return new byte[20];
    }

    public ItemLibraryAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _capacity = data.readUInt32("Capacity");
        _tax = data.readByte("Tax", 0);
    }
}
