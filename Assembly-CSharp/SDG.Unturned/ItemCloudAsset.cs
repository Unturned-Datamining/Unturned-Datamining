namespace SDG.Unturned;

public class ItemCloudAsset : ItemAsset
{
    private float _gravity;

    public float gravity => _gravity;

    public ItemCloudAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _gravity = data.readSingle("Gravity");
    }
}
