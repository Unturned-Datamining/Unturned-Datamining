using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class AirdropAsset : Asset
{
    public static AssetReference<AirdropAsset> defaultAirdrop = new AssetReference<AirdropAsset>("229440c249dc490ba26ce71e8a59d5c6");

    public AssetReference<ItemBarricadeAsset> barricadeRef;

    public MasterBundleReference<GameObject> model;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        barricadeRef = reader.readValue<AssetReference<ItemBarricadeAsset>>("Landed_Barricade");
        model = reader.readValue<MasterBundleReference<GameObject>>("Carepackage_Prefab");
    }

    public AirdropAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
