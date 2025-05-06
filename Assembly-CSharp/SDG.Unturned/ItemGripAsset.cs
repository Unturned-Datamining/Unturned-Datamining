using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemGripAsset : ItemCaliberAsset
{
    protected GameObject _grip;

    public GameObject grip => _grip;

    [Obsolete]
    public bool isBipod => _isBipod;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _grip = loadRequiredAsset<GameObject>(p.bundle, "Grip");
    }
}
