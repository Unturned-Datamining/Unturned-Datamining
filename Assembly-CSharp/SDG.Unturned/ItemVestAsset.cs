using UnityEngine;

namespace SDG.Unturned;

public class ItemVestAsset : ItemBagAsset
{
    protected GameObject _vest;

    public GameObject vest => _vest;

    public ItemVestAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
    }
}
