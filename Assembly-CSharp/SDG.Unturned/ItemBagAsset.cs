namespace SDG.Unturned;

public class ItemBagAsset : ItemClothingAsset
{
    private byte _width;

    private byte _height;

    public byte width => _width;

    public byte height => _height;

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
