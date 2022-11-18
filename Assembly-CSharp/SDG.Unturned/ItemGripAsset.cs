using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemGripAsset : ItemCaliberAsset
{
    protected GameObject _grip;

    public GameObject grip => _grip;

    [Obsolete]
    public bool isBipod => _isBipod;

    public ItemGripAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _grip = loadRequiredAsset<GameObject>(bundle, "Grip");
    }
}
