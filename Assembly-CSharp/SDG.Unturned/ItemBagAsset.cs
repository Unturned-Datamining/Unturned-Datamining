namespace SDG.Unturned;

public class ItemBagAsset : ItemClothingAsset
{
    private byte _width;

    private byte _height;

    public byte width => _width;

    public byte height => _height;

    public ItemBagAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (!isPro)
        {
            _width = data.readByte("Width", 0);
            _height = data.readByte("Height", 0);
        }
    }
}
