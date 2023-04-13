using UnityEngine;

namespace SDG.Unturned;

public class ItemOpticAsset : ItemAsset
{
    public float zoom { get; private set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        zoom = Mathf.Max(1f, data.ParseFloat("Zoom"));
    }
}
