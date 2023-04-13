using UnityEngine;

namespace SDG.Unturned;

public class AirdropAsset : Asset
{
    public static AssetReference<AirdropAsset> defaultAirdrop = new AssetReference<AirdropAsset>("229440c249dc490ba26ce71e8a59d5c6");

    public AssetReference<ItemBarricadeAsset> barricadeRef;

    public MasterBundleReference<GameObject> model;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        barricadeRef = data.ParseStruct<AssetReference<ItemBarricadeAsset>>("Landed_Barricade");
        model = data.ParseStruct<MasterBundleReference<GameObject>>("Carepackage_Prefab");
    }
}
