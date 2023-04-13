namespace SDG.Unturned;

public class OutfitAsset : Asset
{
    public AssetReference<ItemAsset>[] itemAssets;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.TryGetList("Items", out var node))
        {
            itemAssets = node.ParseArrayOfStructs<AssetReference<ItemAsset>>();
        }
        else
        {
            itemAssets = new AssetReference<ItemAsset>[0];
        }
    }
}
