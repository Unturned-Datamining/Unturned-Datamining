using System;

namespace SDG.Unturned;

public class ItemSentryAsset : ItemStorageAsset
{
    protected ESentryMode _sentryMode;

    public float detectionRadius;

    public float targetLossRadius;

    public AssetReference<EffectAsset> targetAcquiredEffect;

    public AssetReference<EffectAsset> targetLostEffect;

    private static AssetReference<EffectAsset> defaultTargetAcquiredEffect = new AssetReference<EffectAsset>("ab5f0056b54545c8a051159659da8bea");

    private static AssetReference<EffectAsset> defaultTargetLostEffect = new AssetReference<EffectAsset>("288b98b718084699ba3653c592e57803");

    public ESentryMode sentryMode => _sentryMode;

    public bool requiresPower { get; protected set; }

    public bool infiniteAmmo { get; protected set; }

    public bool infiniteQuality { get; protected set; }

    public ItemSentryAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (data.has("Mode"))
        {
            _sentryMode = (ESentryMode)Enum.Parse(typeof(ESentryMode), data.readString("Mode"), ignoreCase: true);
        }
        else
        {
            _sentryMode = ESentryMode.NEUTRAL;
        }
        requiresPower = data.readBoolean("Requires_Power", defaultValue: true);
        infiniteAmmo = data.readBoolean("Infinite_Ammo");
        infiniteQuality = data.readBoolean("Infinite_Quality");
        detectionRadius = data.readSingle("Detection_Radius", 48f);
        targetLossRadius = data.readSingle("Target_Loss_Radius", detectionRadius * 1.2f);
        targetAcquiredEffect = data.readAssetReference("Target_Acquired_Effect", in defaultTargetAcquiredEffect);
        targetLostEffect = data.readAssetReference("Target_Lost_Effect", in defaultTargetLostEffect);
    }
}
