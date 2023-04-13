using UnityEngine;

namespace SDG.Unturned;

public class ItemFilterAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
    }
}
