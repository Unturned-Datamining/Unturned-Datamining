using UnityEngine;

namespace SDG.Unturned;

public class ItemMaskAsset : ItemGearAsset
{
    protected GameObject _mask;

    private bool _isEarpiece;

    public GameObject mask => _mask;

    public bool isEarpiece => _isEarpiece;

    public ItemMaskAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            _mask = loadRequiredAsset<GameObject>(bundle, "Mask");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _mask, 10);
                AssetValidation.ValidateClothComponents(this, _mask);
                AssetValidation.searchGameObjectForErrors(this, _mask);
            }
        }
        if (!isPro)
        {
            _isEarpiece = data.has("Earpiece");
        }
    }
}
