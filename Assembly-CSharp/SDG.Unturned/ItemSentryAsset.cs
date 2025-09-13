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

    /// <summary>
    /// [0, 1] percentage whether a shot decreases ammo count. Defaults to 100%.
    /// For example, 0.25 means 25% of shots will use a bullet, while the remaining 75% will be free.
    /// </summary>
    public float AmmoConsumptionProbability { get; protected set; }

    /// <summary>
    /// [0, 1] percentage whether a shot decreases quality. Defaults to 100%.
    /// Combined with the gun's chance of decreasing quality.
    /// </summary>
    public float QualityConsumptionProbability { get; protected set; }

    /// <summary>
    /// If true, this sentry can attack players. Defaults to true.
    /// </summary>
    public bool CanTargetPlayers { get; set; }

    /// <summary>
    /// If true, this sentry can attack zombies. Defaults to true.
    /// </summary>
    public bool CanTargetZombies { get; set; }

    /// <summary>
    /// If true, this sentry can attack animals. Defaults to true.
    /// </summary>
    public bool CanTargetAnimals { get; set; }

    /// <summary>
    /// If true, this sentry can attack vehicles. Defaults to true.
    /// </summary>
    public bool CanTargetVehicles { get; set; }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (!infiniteAmmo && AmmoConsumptionProbability < 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AmmoConsumptionProbability", AmmoConsumptionProbability.ToString("P0")), 2000);
            }
            if (!infiniteQuality && QualityConsumptionProbability < 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_QualityConsumptionProbability", QualityConsumptionProbability.ToString("P0")), 2000);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.ContainsKey("Mode"))
        {
            _sentryMode = (ESentryMode)Enum.Parse(typeof(ESentryMode), p.data.GetString("Mode"), ignoreCase: true);
        }
        else
        {
            _sentryMode = ESentryMode.NEUTRAL;
        }
        requiresPower = p.data.ParseBool("Requires_Power", defaultValue: true);
        infiniteAmmo = p.data.ParseBool("Infinite_Ammo");
        infiniteQuality = p.data.ParseBool("Infinite_Quality");
        AmmoConsumptionProbability = p.data.ParseFloat("AmmoConsumptionProbability", 1f);
        QualityConsumptionProbability = p.data.ParseFloat("QualityConsumptionProbability", 1f);
        detectionRadius = p.data.ParseFloat("Detection_Radius", 48f);
        targetLossRadius = p.data.ParseFloat("Target_Loss_Radius", detectionRadius * 1.2f);
        if (targetLossRadius < detectionRadius - 1E-05f)
        {
            ReportAssetError($"Target_Loss_Radius ({targetLossRadius}) is less than Detection_Radius ({detectionRadius})");
        }
        CanTargetPlayers = p.data.ParseBool("Target_Players", defaultValue: true);
        CanTargetZombies = p.data.ParseBool("Target_Zombies", defaultValue: true);
        CanTargetAnimals = p.data.ParseBool("Target_Animals", defaultValue: true);
        CanTargetVehicles = p.data.ParseBool("Target_Vehicles", defaultValue: true);
        targetAcquiredEffect = p.data.readAssetReference("Target_Acquired_Effect", in defaultTargetAcquiredEffect);
        targetLostEffect = p.data.readAssetReference("Target_Lost_Effect", in defaultTargetLostEffect);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Sentry");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Mode", sentryMode);
        orAddDeclaration.Append("Requires_Power", requiresPower);
        orAddDeclaration.Append("Infinite_Ammo", infiniteAmmo);
        orAddDeclaration.Append("Infinite_Quality", infiniteQuality);
        orAddDeclaration.Append("AmmoConsumptionProbability", AmmoConsumptionProbability);
        orAddDeclaration.Append("QualityConsumptionProbability", QualityConsumptionProbability);
        orAddDeclaration.Append("Detection_Radius", detectionRadius);
        orAddDeclaration.Append("Target_Loss_Radius", targetLossRadius);
        orAddDeclaration.Append("Target_Acquired_Effect", targetAcquiredEffect);
        orAddDeclaration.Append("Target_Lost_Effect", targetLostEffect);
    }
}
