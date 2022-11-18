using UnityEngine;

namespace SDG.Unturned;

public class ItemFisherAsset : ItemAsset
{
    private AudioClip _cast;

    private AudioClip _reel;

    private AudioClip _tug;

    private ushort _rewardID;

    public AudioClip cast => _cast;

    public AudioClip reel => _reel;

    public AudioClip tug => _tug;

    public ushort rewardID => _rewardID;

    public ItemFisherAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _cast = bundle.load<AudioClip>("Cast");
        _reel = bundle.load<AudioClip>("Reel");
        _tug = bundle.load<AudioClip>("Tug");
        _rewardID = data.readUInt16("Reward_ID", 0);
    }
}
