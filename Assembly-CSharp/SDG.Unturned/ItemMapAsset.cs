namespace SDG.Unturned;

public class ItemMapAsset : ItemAsset
{
    public bool enablesCompass { get; protected set; }

    public bool enablesChart { get; protected set; }

    public bool enablesMap { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        enablesCompass = data.ContainsKey("Enables_Compass");
        enablesChart = data.ContainsKey("Enables_Chart");
        enablesMap = data.ContainsKey("Enables_Map");
    }
}
