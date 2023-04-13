namespace SDG.Unturned;

public class ItemGearAsset : ItemClothingAsset
{
    public string hairOverride { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        base.hairVisible = data.ContainsKey("Hair");
        base.beardVisible = data.ContainsKey("Beard");
        hairOverride = data.GetString("Hair_Override");
    }
}
