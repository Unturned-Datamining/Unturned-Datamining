namespace SDG.Unturned;

public class ItemOilPumpAsset : ItemBarricadeAsset
{
    public ushort fuelCapacity { get; protected set; }

    public ItemOilPumpAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        fuelCapacity = data.readUInt16("Fuel_Capacity", 0);
    }
}
