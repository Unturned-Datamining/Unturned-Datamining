using UnityEngine;

namespace SDG.Unturned;

public class ItemHatAsset : ItemGearAsset
{
    protected GameObject _hat;

    public GameObject hat => _hat;

    public ItemHatAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            _hat = loadRequiredAsset<GameObject>(bundle, "Hat");
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.ValidateLayersEqual(this, _hat, 10);
                AssetValidation.ValidateClothComponents(this, _hat);
                AssetValidation.searchGameObjectForErrors(this, _hat);
            }
        }
    }
}
