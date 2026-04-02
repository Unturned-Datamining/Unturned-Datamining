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

    /// <summary>
    /// Multiplier for interval before a fish takes the bait.
    /// Defaults to 1.
    /// </summary>
    public float FishBiteIntervalMultiplier { get; set; }

    public EFishingRewardMode FishingRewardMode { get; set; }

    /// <summary>
    /// If true, player must complete a challenge when fish takes the bait before catching.
    /// Defaults to false for backwards compatibility.
    /// </summary>
    public bool EnableCatchChallenge { get; set; }

    /// <summary>
    /// Size of window item must be within to catch.
    /// </summary>
    public int CatchChallengeCursorSize { get; set; }

    /// <summary>
    /// Downward acceleration while input is not held.
    /// </summary>
    public int CatchChallengeGravity { get; set; }

    /// <summary>
    /// Upward acceleration while input is held.
    /// </summary>
    public int CatchChallengeAcceleration { get; set; }

    /// <summary>
    /// How much velocity to preserve when bouncing off the top.
    /// </summary>
    public int CatchChallengeUpperRestitution { get; set; }

    /// <summary>
    /// How much velocity to preserve when bouncing off the bottom.
    /// </summary>
    public int CatchChallengeLowerRestitution { get; set; }

    /// <summary>
    /// Multiplier for how long item must be within cursor before catching.
    /// </summary>
    public float CatchChallengeCaptureSpeedMultiplier { get; set; }

    /// <summary>
    /// Multiplier for how long item must be outside cursor before failure.
    /// </summary>
    public float CatchChallengeEscapeSpeedMultiplier { get; set; }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.HasFlag(EItemDescriptionFlags.Uncategorized))
        {
            if (FishBiteIntervalMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_BiteIntervalMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(FishBiteIntervalMultiplier, higherIsPositive: false, higherIsBeneficial: false)), 2000 + DescSort_LowerIsBeneficial(FishBiteIntervalMultiplier));
            }
            if (CatchChallengeCaptureSpeedMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_CaptureSpeedMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(CatchChallengeCaptureSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 2000 + DescSort_HigherIsBeneficial(CatchChallengeCaptureSpeedMultiplier));
            }
            if (CatchChallengeEscapeSpeedMultiplier != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FishingRod_EscapeSpeedMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(CatchChallengeEscapeSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: false)), 2000 + DescSort_LowerIsBeneficial(CatchChallengeEscapeSpeedMultiplier));
            }
        }
    }

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
        FishBiteIntervalMultiplier = p.data.ParseFloat("Fish_Bite_Interval_Multiplier", 1f);
        FishingRewardMode = p.data.ParseEnum("Fishing_Reward_Mode", EFishingRewardMode.Rod);
        EnableCatchChallenge = p.data.ParseBool("CatchChallenge_Enabled");
        CatchChallengeCursorSize = Mathf.RoundToInt(p.data.ParseFloat("CatchChallenge_CursorSize", 0.2f) * 10000f);
        CatchChallengeGravity = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_Gravity", 1f)) * 10000f);
        CatchChallengeAcceleration = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_Acceleration", 1f)) * 10000f);
        CatchChallengeUpperRestitution = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_UpperRestitution", 0.5f)) * 10000f);
        CatchChallengeLowerRestitution = Mathf.RoundToInt(Mathf.Abs(p.data.ParseFloat("CatchChallenge_LowerRestitution", 0.5f)) * 10000f);
        CatchChallengeCaptureSpeedMultiplier = p.data.ParseFloat("CatchChallenge_CaptureSpeed", 1f);
        CatchChallengeEscapeSpeedMultiplier = p.data.ParseFloat("CatchChallenge_EscapeSpeed", 1f);
        if (EnableCatchChallenge && (bool)Assets.shouldValidateAssets)
        {
            ValidateEquipableHasAnimation("Catch_Loop");
            ValidateEquipableHasAnimation("Catch_Failure");
        }
    }
}
