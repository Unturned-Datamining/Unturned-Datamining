namespace SDG.Unturned;

public struct PopulateAssetParameters
{
    public Bundle bundle;

    public IDatDictionary data;

    public Local localization;

    /// <summary>
    /// If true, PopulateAsset can modify data. For example, to replace deprecated properties.
    /// Only true if asset re-saving and asset metadata parsing are enabled, and asset origin allows re-saving.
    /// Modifications are not saved if asset has any errors in order to avoid losing data.
    /// </summary>
    public bool CanPerformDataConversions { get; internal set; }
}
