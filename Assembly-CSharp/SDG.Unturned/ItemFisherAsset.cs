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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _cast = bundle.load<AudioClip>("Cast");
        _reel = bundle.load<AudioClip>("Reel");
        _tug = bundle.load<AudioClip>("Tug");
        _rewardID = data.ParseUInt16("Reward_ID", 0);
        rewardExperienceMin = data.ParseInt32("Reward_Experience_Min", 3);
        rewardExperienceMax = data.ParseInt32("Reward_Experience_Max", 3);
        rewardsList.Parse(data, localization, this, "Quest_Rewards", "Quest_Reward_");
    }
}
