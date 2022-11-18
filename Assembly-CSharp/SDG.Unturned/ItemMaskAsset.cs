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
        if (!isPro)
        {
            _isEarpiece = data.has("Earpiece");
        }
    }
}
