namespace SDG.Unturned;

public class ItemOilPumpAsset : ItemBarricadeAsset
{
    public ushort fuelCapacity { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        fuelCapacity = data.ParseUInt16("Fuel_Capacity", 0);
    }
}
