using System;

namespace SDG.Unturned;

public class ItemSentryAsset : ItemStorageAsset
{
    protected ESentryMode _sentryMode;

    /// <summary>
    /// Players/zombies within this range are treated as potential targets while scanning.
    /// </summary>
    public float detectionRadius;

    /// <summary>
    /// Will not lose current target within this range. Prevents target from popping in and out of range.
    /// </summary>
    public float targetLossRadius;

    public AssetReference<EffectAsset> targetAcquiredEffect;

    public AssetReference<EffectAsset> targetLostEffect;

    private static AssetReference<EffectAsset> defaultTargetAcquiredEffect = new AssetReference<EffectAsset>("ab5f0056b54545c8a051159659da8bea");

    private static AssetReference<EffectAsset> defaultTargetLostEffect = new AssetReference<EffectAsset>("288b98b718084699ba3653c592e57803");

    public ESentryMode sentryMode => _sentryMode;

    public bool requiresPower { get; protected set; }

    public bool infiniteAmmo { get; protected set; }

    public bool infiniteQuality { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.ContainsKey("Mode"))
        {
            _sentryMode = (ESentryMode)Enum.Parse(typeof(ESentryMode), data.GetString("Mode"), ignoreCase: true);
        }
        else
        {
            _sentryMode = ESentryMode.NEUTRAL;
        }
        requiresPower = data.ParseBool("Requires_Power", defaultValue: true);
        infiniteAmmo = data.ParseBool("Infinite_Ammo");
        infiniteQuality = data.ParseBool("Infinite_Quality");
        detectionRadius = data.ParseFloat("Detection_Radius", 48f);
        targetLossRadius = data.ParseFloat("Target_Loss_Radius", detectionRadius * 1.2f);
        targetAcquiredEffect = data.readAssetReference("Target_Acquired_Effect", in defaultTargetAcquiredEffect);
        targetLostEffect = data.readAssetReference("Target_Lost_Effect", in defaultTargetLostEffect);
    }
}
