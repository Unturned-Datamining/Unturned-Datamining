namespace SDG.Unturned;

public class ItemGearAsset : ItemClothingAsset
{
    /// <summary>
    /// If set, find a child meshrenderer with this name and change its material to the character hair material.
    /// </summary>
    public string hairOverride { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        base.hairVisible = data.ContainsKey("Hair");
        base.beardVisible = data.ContainsKey("Beard");
        hairOverride = data.GetString("Hair_Override");
    }
}
