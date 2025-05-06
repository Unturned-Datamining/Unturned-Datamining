using UnityEngine;

namespace SDG.Unturned;

public class ItemHatAsset : ItemGearAsset
{
    protected GameObject _hat;

    public GameObject hat => _hat;

    internal override GameObject ClothingPrefab => hat;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (!Dedicator.IsDedicatedServer)
        {
            _hat = loadRequiredAsset<GameObject>(p.bundle, "Hat");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _hat, 10);
                AssetValidation.ValidateClothComponents(this, _hat);
            }
        }
    }
}
