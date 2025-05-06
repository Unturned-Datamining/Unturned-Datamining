using UnityEngine;

namespace SDG.Unturned;

public class ItemFilterAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _use = p.bundle.load<AudioClip>("Use");
    }
}
