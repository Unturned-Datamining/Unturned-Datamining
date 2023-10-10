namespace SDG.Unturned;

public class ItemBagAsset : ItemClothingAsset
{
    private byte _width;

    private byte _height;

    public byte width => _width;

    public byte height => _height;

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent && width > 0 && height > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_StorageDimensions", width, height), 2000);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (!isPro)
        {
            _width = data.ParseUInt8("Width", 0);
            _height = data.ParseUInt8("Height", 0);
        }
    }
}
