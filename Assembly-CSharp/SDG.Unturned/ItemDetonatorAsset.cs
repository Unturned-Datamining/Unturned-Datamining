using UnityEngine;

namespace SDG.Unturned;

public class ItemDetonatorAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    public ItemDetonatorAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
    }
}
