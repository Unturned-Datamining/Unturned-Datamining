namespace SDG.Unturned;

public class ItemGearAsset : ItemClothingAsset
{
    public string hairOverride { get; protected set; }

    public ItemGearAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        base.hairVisible = data.has("Hair");
        base.beardVisible = data.has("Beard");
        hairOverride = data.readString("Hair_Override");
    }
}
