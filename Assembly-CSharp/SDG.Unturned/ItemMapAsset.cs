namespace SDG.Unturned;

public class ItemMapAsset : ItemAsset
{
    public bool enablesCompass { get; protected set; }

    public bool enablesChart { get; protected set; }

    public bool enablesMap { get; protected set; }

    public ItemMapAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        enablesCompass = data.has("Enables_Compass");
        enablesChart = data.has("Enables_Chart");
        enablesMap = data.has("Enables_Map");
    }
}
