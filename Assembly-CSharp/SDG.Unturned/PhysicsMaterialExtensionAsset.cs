using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class PhysicsMaterialExtensionAsset : PhysicsMaterialAssetBase
{
    public AssetReference<PhysicsMaterialAsset> baseRef;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        baseRef = reader.readValue<AssetReference<PhysicsMaterialAsset>>("Base");
        PhysicMaterialCustomData.RegisterAsset(this);
    }

    public PhysicsMaterialExtensionAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
