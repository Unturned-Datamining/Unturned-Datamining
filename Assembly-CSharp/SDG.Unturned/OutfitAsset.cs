namespace SDG.Unturned;

public class OutfitAsset : Asset
{
    public AssetReference<ItemAsset>[] itemAssets;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.TryGetList("Items", out var node))
        {
            itemAssets = node.ParseArrayOfStructs<AssetReference<ItemAsset>>();
        }
        else
        {
            itemAssets = new AssetReference<ItemAsset>[0];
        }
    }
}
