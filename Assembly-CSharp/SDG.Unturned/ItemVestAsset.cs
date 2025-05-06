using UnityEngine;

namespace SDG.Unturned;

public class ItemVestAsset : ItemBagAsset
{
    protected GameObject _vest;

    public GameObject vest => _vest;

    internal override GameObject ClothingPrefab => vest;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (!Dedicator.IsDedicatedServer)
        {
            _vest = loadRequiredAsset<GameObject>(p.bundle, "Vest");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _vest, 10);
                AssetValidation.ValidateClothComponents(this, _vest);
            }
        }
    }
}
