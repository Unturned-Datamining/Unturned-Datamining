using UnityEngine;

namespace SDG.Unturned;

public class ItemGrowerAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    public ItemGrowerAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
    }
}
