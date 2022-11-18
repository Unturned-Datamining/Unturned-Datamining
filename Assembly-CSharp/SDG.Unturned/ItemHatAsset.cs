using UnityEngine;

namespace SDG.Unturned;

public class ItemHatAsset : ItemGearAsset
{
    protected GameObject _hat;

    public GameObject hat => _hat;

    public ItemHatAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
    }
}
