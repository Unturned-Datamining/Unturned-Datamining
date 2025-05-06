using UnityEngine;

namespace SDG.Unturned;

public class ItemFisherAsset : ItemAsset
{
    private AudioClip _cast;

    private AudioClip _reel;

    private AudioClip _tug;

    private ushort _rewardID;

    public int rewardExperienceMin;

    public int rewardExperienceMax;

    internal NPCRewardsList rewardsList;

    public AudioClip cast => _cast;

    public AudioClip reel => _reel;

    public AudioClip tug => _tug;

    public ushort rewardID => _rewardID;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _cast = p.bundle.load<AudioClip>("Cast");
        _reel = p.bundle.load<AudioClip>("Reel");
        _tug = p.bundle.load<AudioClip>("Tug");
        _rewardID = p.data.ParseUInt16("Reward_ID", 0);
        rewardExperienceMin = p.data.ParseInt32("Reward_Experience_Min", 3);
        rewardExperienceMax = p.data.ParseInt32("Reward_Experience_Max", 3);
        rewardsList.Parse(p.data, p.localization, this, "Quest_Rewards", "Quest_Reward_");
    }
}
