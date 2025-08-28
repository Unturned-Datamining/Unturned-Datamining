using UnityEngine;

namespace SDG.Unturned;

public class ItemDetonatorAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    /// <summary>
    /// Nelson 2025-08-20: changing this to affect both sentries and safezones. Some modded safezones allow
    /// building but not weapons, so players were using charges to destroy houses. (public issue #5175)
    /// </summary>
    public override bool shouldFriendlySentryTargetUser => true;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _use = p.bundle.load<AudioClip>("Use");
    }
}
