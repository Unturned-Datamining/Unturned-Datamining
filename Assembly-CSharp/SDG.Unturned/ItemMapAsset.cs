namespace SDG.Unturned;

public class ItemMapAsset : ItemAsset
{
    /// <summary>
    /// Does having this item show the compass?
    /// </summary>
    public bool enablesCompass { get; protected set; }

    /// <summary>
    /// Does having this item show the chart?
    /// </summary>
    public bool enablesChart { get; protected set; }

    /// <summary>
    /// Does having this item show the satellite?
    /// </summary>
    public bool enablesMap { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        enablesCompass = data.ContainsKey("Enables_Compass");
        enablesChart = data.ContainsKey("Enables_Chart");
        enablesMap = data.ContainsKey("Enables_Map");
    }
}
