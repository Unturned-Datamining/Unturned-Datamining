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

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _capacity = p.data.ParseUInt32("Capacity");
        _tax = p.data.ParseUInt8("Tax", 0);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Library");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Capacity", capacity);
        orAddDeclaration.Append("Tax", tax);
    }
}
