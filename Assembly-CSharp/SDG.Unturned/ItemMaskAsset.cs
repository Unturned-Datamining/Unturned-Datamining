using UnityEngine;

namespace SDG.Unturned;

public class ItemMaskAsset : ItemGearAsset
{
    protected GameObject _mask;

    private bool _isEarpiece;

    public GameObject mask => _mask;

    public bool isEarpiece => _isEarpiece;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (!Dedicator.IsDedicatedServer)
        {
            _mask = loadRequiredAsset<GameObject>(bundle, "Mask");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _mask, 10);
                AssetValidation.ValidateClothComponents(this, _mask);
            }
        }
        if (!isPro)
        {
            _isEarpiece = data.ContainsKey("Earpiece");
        }
    }
}
