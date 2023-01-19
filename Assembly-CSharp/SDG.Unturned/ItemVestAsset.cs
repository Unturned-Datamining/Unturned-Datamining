using UnityEngine;

namespace SDG.Unturned;

public class ItemVestAsset : ItemBagAsset
{
    protected GameObject _vest;

    public GameObject vest => _vest;

    public ItemVestAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            _vest = loadRequiredAsset<GameObject>(bundle, "Vest");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _vest, 10);
                AssetValidation.ValidateClothComponents(this, _vest);
                AssetValidation.searchGameObjectForErrors(this, _vest);
            }
        }
    }
}
