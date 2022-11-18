namespace SDG.Unturned;

public class ItemKeyAsset : ItemAsset
{
    public bool exchangeWithTargetItem;

    public ItemKeyAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        exchangeWithTargetItem = data.has("Exchange_With_Target_Item");
    }
}
