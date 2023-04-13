namespace SDG.Unturned;

public class ItemKeyAsset : ItemAsset
{
    public bool exchangeWithTargetItem;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        exchangeWithTargetItem = data.ContainsKey("Exchange_With_Target_Item");
    }
}
