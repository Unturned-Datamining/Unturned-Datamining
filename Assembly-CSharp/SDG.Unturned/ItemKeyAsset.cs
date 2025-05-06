namespace SDG.Unturned;

public class ItemKeyAsset : ItemAsset
{
    public bool exchangeWithTargetItem;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        exchangeWithTargetItem = p.data.ContainsKey("Exchange_With_Target_Item");
    }
}
