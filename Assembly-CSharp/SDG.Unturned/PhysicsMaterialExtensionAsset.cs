namespace SDG.Unturned;

public class PhysicsMaterialExtensionAsset : PhysicsMaterialAssetBase
{
    public AssetReference<PhysicsMaterialAsset> baseRef;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        baseRef = data.ParseStruct<AssetReference<PhysicsMaterialAsset>>("Base");
        PhysicMaterialCustomData.RegisterAsset(this);
    }
}
