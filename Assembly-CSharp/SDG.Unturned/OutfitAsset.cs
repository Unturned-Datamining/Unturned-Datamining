using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class OutfitAsset : Asset
{
    public AssetReference<ItemAsset>[] itemAssets;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        int num = reader.readArrayLength("Items");
        itemAssets = new AssetReference<ItemAsset>[num];
        for (int i = 0; i < num; i++)
        {
            itemAssets[i] = reader.readValue<AssetReference<ItemAsset>>(i);
        }
    }

    public OutfitAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
