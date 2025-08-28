using UnityEngine;

namespace SDG.Unturned;

public class ItemVestAsset : ItemBagAsset
{
    protected GameObject _vest;

    /// <summary>
    /// If true and player has no shirt equipped, use fallback shirt as equipped shirt.
    /// Used by oversize vest and zip-up vest so they are visible without a shirt equipped.
    /// </summary>
    internal bool hasFallbackShirt;

    internal CachingAssetRef fallbackShirt;

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
            hasFallbackShirt = p.data.ParseBool("Has_Fallback_Shirt");
            if (hasFallbackShirt)
            {
                fallbackShirt = p.data.ParseAssetRef("Fallback_Shirt");
            }
        }
    }
}
