namespace SDG.Unturned;

public class ItemOilPumpAsset : ItemBarricadeAsset
{
    public ushort fuelCapacity { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        fuelCapacity = data.ParseUInt16("Fuel_Capacity", 0);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("OilPump");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Fuel_Capacity", fuelCapacity);
    }
}
