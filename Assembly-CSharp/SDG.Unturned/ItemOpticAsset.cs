using UnityEngine;

namespace SDG.Unturned;

public class ItemOpticAsset : ItemAsset
{
    public float zoom { get; private set; }

    public ItemOpticAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        zoom = Mathf.Max(1f, data.readSingle("Zoom"));
    }
}
