using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemGripAsset : ItemCaliberAsset
{
    protected GameObject _grip;

    public GameObject grip => _grip;

    [Obsolete]
    public bool isBipod => _isBipod;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _grip = loadRequiredAsset<GameObject>(bundle, "Grip");
    }
}
