namespace SDG.Unturned;

/// <summary>
/// Adds custom data to base physics material asset.
/// For example how a vanilla material should respond to custom laser guns.
/// </summary>
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
