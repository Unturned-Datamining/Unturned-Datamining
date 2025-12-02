using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class VehicleAsset : Asset, ISkinableAsset, IArmorFalloff
{
    protected bool _shouldVerifyHash;

    protected string _vehicleName;

    protected float _size2_z;

    protected string _sharedSkinName;

    private Guid _sharedSkinLookupGuid;

    protected ushort _sharedSkinLookupID;

    protected EEngine _engine;

    protected EItemRarity _rarity;

    private GameObject loadedModel;

    /// <summary>
    /// Prevents calling getOrLoad redundantly if asset does not exist.
    /// </summary>
    private bool hasLoadedModel;

    private IDeferredAsset<GameObject> clientModel;

    private IDeferredAsset<GameObject> legacyServerModel;

    protected AudioClip _ignition;

    protected AudioClip _horn;

    protected float _pitchIdle;

    protected float _pitchDrive;

    internal EVehicleEngineSoundType engineSoundType;

    internal RpmEngineSoundConfiguration engineSoundConfiguration;

    protected float _steerMin;

    protected float _steerMax;

    /// <summary>
    /// Torque on Z axis applied according to steering input for bikes and motorcycles.
    /// </summary>
    internal float steeringLeaningForceMultiplier;

    /// <summary>
    /// If true, leaning force is multiplied by normalized speed to the power of steeringLeaningForceSpeedExponent.
    /// Defaults to false.
    /// </summary>
    internal bool steeringLeaningForceShouldScaleWithSpeed;

    /// <summary>
    /// Refer to steeringLeaningForceShouldScaleWithSpeed.
    /// </summary>
    internal float steeringLeaningForceSpeedExponent;

    protected float _brake;

    protected float _lift;

    protected ushort _fuelMin;

    protected ushort _fuelMax;

    protected ushort _fuel;

    protected ushort _healthMin;

    protected ushort _healthMax;

    protected ushort _health;

    private Guid _explosionEffectGuid;

    protected ushort _explosion;

    /// <summary>
    /// Only used if ShouldExplosionBurnMaterials. Optional. Allows specifying which renderers to burn.
    /// </summary>
    internal PaintableVehicleSection[] explosionBurnMaterialSections;

    protected bool _hasHeadlights;

    protected bool _hasSirens;

    protected bool _hasHook;

    protected int _numSeats;

    protected bool _hasZip;

    protected bool _hasBicycle;

    protected bool _hasLockMouse;

    protected bool _hasTraction;

    protected bool _hasSleds;

    protected float _exit;

    protected float _sqrDelta;

    protected float _camFollowDistance;

    protected float _bumperMultiplier;

    protected float _passengerExplosionArmor;

    protected TurretInfo[] _turrets;

    protected Texture2D _albedoBase;

    protected Texture2D _metallicBase;

    protected Texture2D _emissionBase;

    /// <summary>
    /// To non-explosions.
    /// </summary>
    public bool isVulnerable;

    public bool isVulnerableToExplosions;

    /// <summary>
    /// Mega zombie rocks, zombies, animals.
    /// </summary>
    public bool isVulnerableToEnvironment;

    /// <summary>
    /// Crashing into stuff.
    /// </summary>
    public bool isVulnerableToBumper;

    public bool canTiresBeDamaged;

    /// <summary>
    /// If greater than zero, torque is applied on the local Z axis multiplied by this factor.
    /// Note that <see cref="F:SDG.Unturned.VehicleAsset.rollAngularVelocityDamping" /> is critical for damping this force.
    /// </summary>
    internal float wheelBalancingForceMultiplier = -1f;

    /// <summary>
    /// Exponent on the [0, 1] factor representing how aligned the vehicle is with the ground up vector.
    /// </summary>
    internal float wheelBalancingUprightExponent;

    /// <summary>
    /// If greater than zero, an acceleration is applied to angular velocity on Z axis toward zero.
    /// </summary>
    internal float rollAngularVelocityDamping = -1f;

    internal VehicleWheelConfiguration[] wheelConfiguration;

    /// <summary>
    /// Indices of wheels using replicated collider pose (if any).
    /// Null if not configured or no wheels using this feature.
    /// Allows client and server to replicate only the suspension value without other context.
    /// </summary>
    internal int[] replicatedWheelIndices;

    /// <summary>
    /// Indices of wheels with motor torque applied (if any).
    /// Used for engine RPM calculation.
    /// </summary>
    internal int[] poweredWheelIndices;

    internal float reverseGearRatio;

    internal float[] forwardGearRatios;

    /// <summary>
    /// List of transforms to register with DynamicWaterTransparentSort.
    /// </summary>
    internal PaintableVehicleSection[] extraTransparentSections;

    internal CrawlerTrackTilingMaterial[] crawlerTrackTilingMaterials;

    internal EVehicleDefaultPaintColorMode defaultPaintColorMode;

    /// <summary>
    /// Null if <see cref="F:SDG.Unturned.VehicleAsset.defaultPaintColorMode" /> isn't <see cref="F:SDG.Unturned.EVehicleDefaultPaintColorMode.RandomHueOrGrayscale" />.
    /// </summary>
    internal VehicleRandomPaintColorConfiguration randomPaintColorConfiguration;

    private static readonly Guid VANILLA_BATTERY_ITEM = new Guid("098b13be34a7411db7736b7f866ada69");

    protected bool _hasCrawler;

    private static CommandLineFlag clLogWheelConfiguration = new CommandLineFlag(defaultValue: false, "-LogVehicleWheelConfigurations");

    public float ArmorFalloffMaxRange { get; set; }

    public float ArmorFalloffRange { get; set; }

    public float ArmorFalloffMultiplier { get; set; }

    public bool shouldVerifyHash => _shouldVerifyHash;

    internal override bool ShouldVerifyHash => _shouldVerifyHash;

    public string vehicleName => _vehicleName;

    public override string FriendlyName => _vehicleName;

    public float size2_z => _size2_z;

    public string sharedSkinName => _sharedSkinName;

    /// <summary>
    /// Please refer to: <seealso cref="M:SDG.Unturned.VehicleAsset.FindSharedSkinVehicleAsset" />
    /// </summary>
    public Guid SharedSkinLookupGuid => _sharedSkinLookupGuid;

    /// <summary>
    /// Please refer to: <seealso cref="M:SDG.Unturned.VehicleAsset.FindSharedSkinVehicleAsset" />
    /// </summary>
    [Obsolete]
    public ushort sharedSkinLookupID => _sharedSkinLookupID;

    public EEngine engine => _engine;

    public EItemRarity rarity => _rarity;

    public AudioClip ignition => _ignition;

    public AudioClip horn => _horn;

    public bool hasHorn { get; protected set; }

    public float pitchIdle => _pitchIdle;

    public float pitchDrive => _pitchDrive;

    /// <summary>
    /// Maximum (negative) velocity to aim for while accelerating backward.
    /// </summary>
    public float TargetReverseVelocity { get; private set; }

    /// <summary>
    /// Maximum speed to aim for while accelerating backward.
    /// </summary>
    public float TargetReverseSpeed => Mathf.Abs(TargetReverseVelocity);

    /// <summary>
    /// Maximum velocity to aim for while accelerating forward.
    /// </summary>
    public float TargetForwardVelocity { get; private set; }

    /// <summary>
    /// Maximum speed to aim for while accelerating forward.
    /// </summary>
    public float TargetForwardSpeed => Mathf.Abs(TargetForwardVelocity);

    /// <summary>
    /// Steering angle range at target maximum speed (for the current forward/backward direction).
    /// Reducing steering range at higher speeds keeps the vehicle controlable with digital (non-analog) input.
    /// </summary>
    public float MaxSteeringAngleAtFullSpeed
    {
        get
        {
            return _steerMin;
        }
        set
        {
            _steerMin = value;
        }
    }

    /// <summary>
    /// Steering angle range at zero speed (idle/parked).
    /// For example, 45 means the wheels connected to steering can rotate ±45 degrees.
    /// </summary>
    public float MaxSteeringAngle
    {
        get
        {
            return _steerMax;
        }
        set
        {
            _steerMax = value;
        }
    }

    /// <summary>
    /// Steering angle rotation change in degrees per second.
    /// </summary>
    public float SteeringAngleTurnSpeed { get; private set; }

    /// <summary>
    /// Added or subtracted from wheel motor torque in <see cref="F:SDG.Unturned.EWheelSteeringMode.CrawlerTrack" /> mode.
    /// </summary>
    public float CrawlerTrackSteeringTorque { get; private set; }

    /// <summary>
    /// When a wheel is in <see cref="F:SDG.Unturned.EWheelSteeringMode.CrawlerTrack" /> mode and a steering input is applied the
    /// <see cref="P:UnityEngine.WheelCollider.sidewaysFriction" /> stiffness is multiplied by this factor. This allows the vehicle
    /// to rotate in-place with a lower steering torque, which helps prevent the vehicle from going out of control
    /// while turning and accelerating.
    /// </summary>
    public float CrawlerTrackSteeringSidewaysFrictionMultiplier { get; private set; }

    /// <summary>
    /// Multiplier for <see cref="P:SDG.Unturned.VehicleAsset.CrawlerTrackSteeringTorque" /> and <see cref="P:SDG.Unturned.VehicleAsset.CrawlerTrackSteeringSidewaysFrictionMultiplier" />
    /// while at target maximum speed (for the current forward/backward direction).
    /// </summary>
    public float CrawlerTrackSteeringMaxSpeedScale { get; private set; }

    public float brake => _brake;

    public float lift => _lift;

    public ushort fuelMin => _fuelMin;

    public ushort fuelMax => _fuelMax;

    public ushort fuel => _fuel;

    public ushort healthMin => _healthMin;

    public ushort healthMax => _healthMax;

    public ushort health => _health;

    public Guid ExplosionEffectGuid => _explosionEffectGuid;

    public ushort explosion
    {
        [Obsolete]
        get
        {
            return _explosion;
        }
    }

    public Vector3 minExplosionForce { get; set; }

    public Vector3 maxExplosionForce { get; set; }

    [Obsolete("Separated into ShouldExplosionCauseDamage and ShouldExplosionBurnMaterials.")]
    public bool isExplosive => !IsExplosionEffectRefNull();

    /// <summary>
    /// If true, explosion will damage nearby entities and kill passengers.
    /// </summary>
    public bool ShouldExplosionCauseDamage { get; protected set; }

    public bool ShouldExplosionBurnMaterials { get; protected set; }

    public bool hasHeadlights => _hasHeadlights;

    public bool hasSirens => _hasSirens;

    public bool hasHook => _hasHook;

    public int numSeats => _numSeats;

    public bool hasZip => _hasZip;

    /// <summary>
    /// When true the bicycle animation is used and extra speed is stamina powered.
    /// Bad way to implement it.
    /// </summary>
    public bool hasBicycle => _hasBicycle;

    /// <summary>
    /// Can this vehicle ever spawn with a charged battery?
    /// Uses game mode battery stats when true, or overrides by preventing battery spawn when false.
    /// </summary>
    public bool canSpawnWithBattery { get; protected set; }

    /// <summary>
    /// Battery charge when first spawning in is multiplied by this [0, 1] number.
    /// </summary>
    public float batterySpawnChargeMultiplier { get; protected set; }

    /// <summary>
    /// Battery decrease per second.
    /// </summary>
    public float batteryBurnRate { get; protected set; }

    /// <summary>
    /// Battery increase per second.
    /// </summary>
    public float batteryChargeRate { get; protected set; }

    public EBatteryMode batteryDriving { get; protected set; }

    public EBatteryMode batteryEmpty { get; protected set; }

    public EBatteryMode batteryHeadlights { get; protected set; }

    public EBatteryMode batterySirens { get; protected set; }

    /// <summary>
    /// Battery item given to the player when a specific battery hasn't been manually
    /// installed yet. Defaults to the vanilla car battery (098b13be34a7411db7736b7f866ada69).
    /// </summary>
    public Guid defaultBatteryGuid { get; protected set; }

    /// <summary>
    /// Fuel decrease per second.
    /// </summary>
    public float fuelBurnRate { get; protected set; }

    public bool isReclined { get; protected set; }

    public bool hasLockMouse => _hasLockMouse;

    public bool hasTraction => _hasTraction;

    public bool hasSleds => _hasSleds;

    public float exit => _exit;

    public float sqrDelta => _sqrDelta;

    /// <summary>
    /// Client sends physics simulation results to server. If upward (+Y) speed exceeds this, mark the move invalid.
    /// </summary>
    public float validSpeedUp { get; protected set; }

    /// <summary>
    /// Client sends physics simulation results to server. If downward (-Y) speed exceeds this, mark the move invalid.
    /// </summary>
    public float validSpeedDown { get; protected set; }

    /// <summary>
    /// If distance between client-submitted hit position and vehicle pivot point is too high the hit will be
    /// marked invalid. This multiplies the distance threshold, useful for very fast vehicles.
    /// </summary>
    public float ValidHitDistanceMultiplier { get; protected set; }

    public float camFollowDistance => _camFollowDistance;

    /// <summary>
    /// Vertical first-person view translation.
    /// </summary>
    public float camDriverOffset { get; protected set; }

    /// <summary>
    /// Vertical first-person view translation.
    /// </summary>
    public float camPassengerOffset { get; protected set; }

    public float bumperMultiplier => _bumperMultiplier;

    /// <summary>
    /// Base damage to players when traveling at 1 m/s. Defaults to 10.
    /// </summary>
    public float BumperPlayerDamage { get; set; }

    /// <summary>
    /// Base damage to zombies when traveling at 1 m/s. Defaults to 15.
    /// </summary>
    public float BumperZombieDamage { get; set; }

    /// <summary>
    /// Base damage to animals when traveling at 1 m/s. Defaults to 15.
    /// </summary>
    public float BumperAnimalDamage { get; set; }

    /// <summary>
    /// Base damage to objects when traveling at 1 m/s. Defaults to 30.
    /// </summary>
    public float BumperObjectDamage { get; set; }

    /// <summary>
    /// Base damage to trees when traveling at 1 m/s. Defaults to 85.
    /// </summary>
    public float BumperResourceDamage { get; set; }

    /// <summary>
    /// If speed multiplied by <see cref="P:SDG.Unturned.VehicleAsset.bumperMultiplier" /> is less than this, no damage is applied.
    /// Defaults to 3.
    /// </summary>
    public float BumperSpeedDamageThreshold { get; set; }

    /// <summary>
    /// Multiplier for damage from crashing into things.
    /// Not applicable if <see cref="F:SDG.Unturned.VehicleAsset.isVulnerableToBumper" /> is false.
    /// Defaults to 1.
    /// </summary>
    public float BumperSelfDamageMultiplier { get; set; }

    public float passengerExplosionArmor => _passengerExplosionArmor;

    public TurretInfo[] turrets => _turrets;

    public Texture albedoBase => _albedoBase;

    public Texture metallicBase => _metallicBase;

    public Texture emissionBase => _emissionBase;

    public bool CanDecay { get; private set; }

    /// <summary>
    /// Can this vehicle be repaired by a seated player?
    /// </summary>
    public bool canRepairWhileSeated { get; protected set; }

    public float childExplosionArmorMultiplier { get; protected set; }

    public float airTurnResponsiveness { get; protected set; }

    public float airSteerMin { get; protected set; }

    public float airSteerMax { get; protected set; }

    public float bicycleAnimSpeed { get; protected set; }

    public float trainTrackOffset { get; protected set; }

    public float trainWheelOffset { get; protected set; }

    public float trainCarLength { get; protected set; }

    public float staminaBoost { get; protected set; }

    public bool useStaminaBoost { get; protected set; }

    public bool isStaminaPowered { get; protected set; }

    public bool isBatteryPowered { get; protected set; }

    /// <summary>
    /// Can mobile barricades e.g. bed or sentry guns be placed on this vehicle?
    /// </summary>
    [Obsolete("Replaced by BuildablePlacementRule")]
    public bool supportsMobileBuildables => BuildablePlacementRule == EVehicleBuildablePlacementRule.AlwaysAllow;

    public EVehicleBuildablePlacementRule BuildablePlacementRule { get; protected set; }

    /// <summary>
    /// Should capsule colliders be added to seat transforms?
    /// Useful to prevent bikes from leaning into walls.
    /// </summary>
    public bool shouldSpawnSeatCapsules { get; protected set; }

    /// <summary>
    /// Can players lock the vehicle to their clan/group?
    /// True by default, but mods want to be able to disable.
    /// </summary>
    public bool canBeLocked { get; protected set; }

    /// <summary>
    /// Can players steal the battery?
    /// </summary>
    public bool canStealBattery { get; protected set; }

    public byte trunkStorage_X { get; set; }

    public byte trunkStorage_Y { get; set; }

    /// <summary>
    /// Spawn table to drop items from on death.
    /// </summary>
    public ushort dropsTableId { get; protected set; }

    /// <summary>
    /// Minimum number of items to drop on death.
    /// </summary>
    public byte dropsMin { get; protected set; }

    /// <summary>
    /// Maximum number of items to drop on death.
    /// </summary>
    public byte dropsMax { get; protected set; }

    /// <summary>
    /// Item ID of compatible tire.
    /// </summary>
    public ushort tireID { get; protected set; }

    internal bool UsesEngineRpmAndGears
    {
        get
        {
            if (forwardGearRatios != null)
            {
                return forwardGearRatios.Length != 0;
            }
            return false;
        }
    }

    /// <summary>
    /// If false, only grounded wheels are included when calculating wheel RPM.
    /// </summary>
    public bool ShouldIncludeAirbornWheelsInAverageRpm { get; set; } = true;


    /// <summary>
    /// If this and UsesEngineRpmAndGears are true, HUD will show RPM and gear number.
    /// </summary>
    public bool AllowsEngineRpmAndGearsInHud { get; protected set; }

    /// <summary>
    /// When engine RPM dips below this value shift to the next lower gear if available.
    /// </summary>
    public float GearShiftDownThresholdRpm { get; private set; }

    /// <summary>
    /// When engine RPM exceeds this value shift to the next higher gear if available.
    /// </summary>
    public float GearShiftUpThresholdRpm { get; private set; }

    /// <summary>
    /// How long after changing gears before throttle is engaged again.
    /// </summary>
    public float GearShiftDuration { get; private set; }

    /// <summary>
    /// How long between changing gears to allow another automatic gear change.
    /// </summary>
    public float GearShiftInterval { get; private set; }

    /// <summary>
    /// If true, engine can skip from (for example) 1st to 3rd gear if it keeps RPM within
    /// the acceptable range.
    /// </summary>
    public bool GearShiftAllowSkippingGears { get; set; } = true;


    /// <summary>
    /// Minimum engine RPM.
    /// </summary>
    public float EngineIdleRpm { get; private set; }

    /// <summary>
    /// Maximum engine RPM.
    /// </summary>
    public float EngineMaxRpm { get; private set; }

    /// <summary>
    /// How quickly RPM can increase in RPM/s.
    /// e.g., 1000 will take 2 seconds to go from 2000 to 4000 RPM.
    /// Defaults to -1 which instantly changes RPM.
    /// </summary>
    public float EngineRpmIncreaseRate { get; private set; }

    /// <summary>
    /// How quickly RPM can decrease in RPM/s.
    /// e.g., 1000 will take 2 seconds to go from 4000 to 2000 RPM.
    /// Defaults to -1 which instantly changes RPM.
    /// </summary>
    public float EngineRpmDecreaseRate { get; private set; }

    /// <summary>
    /// Maximum torque (multiplied by output of torque curve).
    /// </summary>
    public float EngineMaxTorque { get; private set; }

    /// <summary>
    /// If true, wheel RPM is reduced according to the difference between expected and actual
    /// wheel RPM divided by torque reduction threshold.
    /// </summary>
    public bool EngineRpmMismatchTorqueReductionEnabled { get; set; }

    /// <summary>
    /// If torque reduction is enabled, torque is reduced to zero when difference between
    /// expected and actual RPM is greater than this threshold.
    /// </summary>
    public float EngineRpmMismatchTorqueReductionThreshold { get; set; }

    /// <summary>
    /// If true, prevent changing gears when the difference between expected and actual
    /// wheel RPM exceeds threshold.
    /// </summary>
    public bool EngineRpmMismatchGearShiftPreventShifting { get; set; }

    public float EngineRpmMismatchGearShiftUpMinThreshold { get; set; }

    /// <summary>
    /// If prevent shifting is enabled, prevent changing gears up when the difference between
    /// expected and actual wheel RPM is greater than this threshold.
    /// I.e., if (expected - actual &gt; max) it cannot shift up.
    /// </summary>
    public float EngineRpmMismatchGearShiftUpMaxThreshold { get; set; }

    public float EngineRpmMismatchGearShiftDownMinThreshold { get; set; }

    /// <summary>
    /// If prevent shifting is enabled, prevent changing gears down when the difference between
    /// expected and actual wheel RPM is greater than this threshold.
    /// I.e., if (expected - actual &gt; max) it cannot shift down.
    /// </summary>
    public float EngineRpmMismatchGearShiftDownMaxThreshold { get; set; }

    /// <summary>
    /// Was a center of mass specified in the .dat?
    /// </summary>
    public bool hasCenterOfMassOverride { get; protected set; }

    /// <summary>
    /// If hasCenterOfMassOverride, use this value.
    /// </summary>
    public Vector3 centerOfMass { get; protected set; }

    public float carjackForceMultiplier { get; protected set; }

    /// <summary>
    /// Multiplier for otherwise not-yet-configurable plane/heli/boat forces.
    /// Nelson 2024-03-06: Required for increasing mass of vehicles without significantly messing with behavior.
    /// </summary>
    public float engineForceMultiplier { get; protected set; }

    /// <summary>
    /// If set, override the wheel collider mass with this value.
    /// </summary>
    public float? wheelColliderMassOverride { get; protected set; }

    public AssetReference<VehiclePhysicsProfileAsset> physicsProfileRef { get; protected set; }

    /// <summary>
    /// Null if vehicle doesn't support paint color.
    /// </summary>
    public PaintableVehicleSection[] PaintableVehicleSections { get; protected set; }

    /// <summary>
    /// Null if vehicle doesn't support paint color.
    /// </summary>
    public List<Color32> DefaultPaintColors { get; protected set; }

    public bool SupportsPaintColor
    {
        get
        {
            if (PaintableVehicleSections != null)
            {
                return PaintableVehicleSections.Length != 0;
            }
            return false;
        }
    }

    /// <summary>
    /// If true, Vehicle Paint items can be used on this vehicle.
    /// Always false if <see cref="P:SDG.Unturned.VehicleAsset.SupportsPaintColor" /> is false.
    ///
    /// Certain vehicles may support paint colors without also being paintable by players. For example, the creator
    /// of a vehicle may want to use color variants without also allowing players to make it bright pink.
    /// </summary>
    public bool IsPaintable { get; protected set; }

    /// <summary>
    /// Get number of reverse gear ratios.
    /// Exposed for plugin use.
    /// </summary>
    public int ReverseGearsCount => 1;

    /// <summary>
    /// Get number of forward gear ratios.
    /// Exposed for plugin use.
    /// </summary>
    public int ForwardGearsCount
    {
        get
        {
            float[] array = forwardGearRatios;
            if (array == null)
            {
                return 0;
            }
            return array.Length;
        }
    }

    public override EAssetType assetCategory => EAssetType.VEHICLE;

    /// <summary>
    /// Number of tire visuals to rotate with steering wheel.
    /// </summary>
    [Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
    public int numSteeringTires { get; protected set; }

    [Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
    public int[] steeringTireIndices { get; protected set; }

    [Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
    public bool hasCrawler => _hasCrawler;

    [Obsolete("Renamed to TargetReverseVelocity.")]
    public float speedMin => TargetReverseVelocity;

    [Obsolete("Renamed to TargetForwardVelocity.")]
    public float speedMax => TargetForwardVelocity;

    [Obsolete("Renamed to MaxSteeringAngle")]
    public float steerMax => _steerMax;

    [Obsolete("Renamed to MaxSteeringAngleAtFullSpeed")]
    public float steerMin => _steerMin;

    /// <summary>
    /// Supports redirects by VehicleRedirectorAsset.
    ///
    /// "Shared Skins" were implemented when there were several asset variants of each vehicle. For example,
    /// Off_Roader_Orange, Off_Roader_Purple, Off_Roader_Green, etc. Each vehicle had their "shared skin" set to
    /// the same ID, and the skin asset had its target ID set to the shared ID. This isn't as necessary after
    /// merging vanilla vehicle variants, but some mods may rely on it, and it needed GUID support now that the
    /// target vehicle might not have a legacy ID.
    /// </summary>
    public VehicleAsset FindSharedSkinVehicleAsset()
    {
        Asset asset = Assets.FindBaseVehicleAssetByGuidOrLegacyId(_sharedSkinLookupGuid, _sharedSkinLookupID);
        if (asset is VehicleRedirectorAsset { TargetVehicle: var targetVehicle })
        {
            asset = targetVehicle.Find();
        }
        return asset as VehicleAsset;
    }

    public GameObject GetOrLoadModel()
    {
        if (!hasLoadedModel)
        {
            hasLoadedModel = true;
            if (legacyServerModel != null)
            {
                loadedModel = legacyServerModel.getOrLoad();
                if (loadedModel == null)
                {
                    loadedModel = clientModel.getOrLoad();
                }
            }
            else
            {
                loadedModel = clientModel.getOrLoad();
            }
        }
        return loadedModel;
    }

    protected void onModelLoaded(GameObject asset)
    {
        Transform transform = asset.transform.Find("Seats");
        if (transform == null)
        {
            Assets.ReportError(this, "missing 'Seats' Transform");
        }
        else if (transform.childCount < 1)
        {
            Assets.ReportError(this, "empty 'Seats' Transform has zero children");
        }
        else
        {
            for (int i = 0; i <= transform.childCount; i++)
            {
                if ((bool)transform.Find($"Seat_{i}"))
                {
                    _numSeats++;
                }
            }
        }
        Rigidbody component = asset.GetComponent<Rigidbody>();
        if (component == null)
        {
            Assets.ReportError(this, "missing root Rigidbody");
        }
        else if (physicsProfileRef.isNull && MathfEx.IsNearlyEqual(component.mass, 1f))
        {
            bool flag = true;
            Transform transform2 = asset.transform.Find("Tires");
            if (transform2 != null)
            {
                for (int j = 0; j < transform2.childCount; j++)
                {
                    Transform child = transform2.GetChild(j);
                    if (!(child == null))
                    {
                        WheelCollider component2 = child.GetComponent<WheelCollider>();
                        if (!(component2 == null) && !MathfEx.IsNearlyEqual(component2.mass, 1f))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                switch (engine)
                {
                case EEngine.BOAT:
                    physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Boat;
                    break;
                case EEngine.CAR:
                    physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Car;
                    break;
                case EEngine.HELICOPTER:
                    physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Helicopter;
                    break;
                case EEngine.PLANE:
                    physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Plane;
                    break;
                }
            }
        }
        asset.SetTagIfUntaggedRecursively("Vehicle");
        if (wheelConfiguration == null)
        {
            BuildAutomaticWheelConfiguration(asset);
        }
    }

    /// <summary>
    /// Clip.prefab
    /// </summary>
    protected void OnServerModelLoaded(GameObject asset)
    {
        if (asset == null)
        {
            Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Vehicle\" GameObject instead");
            return;
        }
        _hasHeadlights = true;
        _hasSirens = true;
        _hasHook = true;
        onModelLoaded(asset);
    }

    /// <summary>
    /// Vehicle.prefab
    /// </summary>
    protected void OnClientModelLoaded(GameObject asset)
    {
        if (asset == null)
        {
            Assets.ReportError(this, "missing \"Vehicle\" GameObject");
            return;
        }
        AssetValidation.searchGameObjectForErrors(this, asset);
        _hasHeadlights = asset.transform.Find("Headlights") != null;
        _hasSirens = asset.transform.Find("Sirens") != null;
        _hasHook = asset.transform.Find("Hook") != null;
        if (_pitchIdle < 0f)
        {
            _pitchIdle = 0.5f;
            AudioSource component = asset.GetComponent<AudioSource>();
            if (component != null)
            {
                AudioClip clip = component.clip;
                if (clip != null)
                {
                    if (clip.name == "Engine_Large")
                    {
                        _pitchIdle = 0.625f;
                    }
                    else if (clip.name == "Engine_Small")
                    {
                        _pitchIdle = 0.75f;
                    }
                }
            }
        }
        if (_pitchDrive < 0f)
        {
            if (engine == EEngine.HELICOPTER)
            {
                _pitchDrive = 0.03f;
            }
            else if (engine == EEngine.BLIMP)
            {
                _pitchDrive = 0.1f;
            }
            else
            {
                _pitchDrive = 0.05f;
                AudioSource component2 = asset.GetComponent<AudioSource>();
                if (component2 != null)
                {
                    AudioClip clip2 = component2.clip;
                    if (clip2 != null)
                    {
                        if (clip2.name == "Engine_Large")
                        {
                            _pitchDrive = 0.025f;
                        }
                        else if (clip2.name == "Engine_Small")
                        {
                            _pitchDrive = 0.075f;
                        }
                    }
                }
            }
        }
        onModelLoaded(asset);
        if (Dedicator.IsDedicatedServer)
        {
            ServerPrefabUtil.RemoveClientComponents(asset, this);
        }
    }

    public void DebugDumpWheelConfigurationToStringBuilder(StringBuilder output)
    {
        output.Append(vehicleName);
        if (wheelConfiguration == null || wheelConfiguration.Length < 1)
        {
            output.AppendLine(" wheel configuration(s): N/A");
            return;
        }
        output.AppendLine(" wheel configuration(s):");
        for (int i = 0; i < wheelConfiguration.Length; i++)
        {
            output.Append(i);
            output.AppendLine(":");
            VehicleWheelConfiguration vehicleWheelConfiguration = wheelConfiguration[i];
            output.Append("Wheel collider path: \"");
            output.Append(vehicleWheelConfiguration.wheelColliderPath);
            output.AppendLine("\"");
            output.Append("Is collider steered: ");
            output.Append(vehicleWheelConfiguration.steeringMode == EWheelSteeringMode.SteeringAngle);
            output.AppendLine();
            output.Append("Is collider powered: ");
            output.Append(vehicleWheelConfiguration.isColliderPowered);
            output.AppendLine();
            output.Append("Model path: \"");
            output.Append(vehicleWheelConfiguration.modelPath);
            output.AppendLine("\"");
            output.Append("Is model steered: ");
            output.Append(vehicleWheelConfiguration.isModelSteered);
            output.AppendLine();
        }
    }

    public string DebugDumpWheelConfigurationToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpWheelConfigurationToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }

    private void LogWheelConfigurationDatConversion()
    {
        string message;
        using (StringWriter stringWriter = new StringWriter())
        {
            using DatWriter datWriter = new DatWriter(stringWriter);
            datWriter.WriteListStart("WheelConfigurations");
            VehicleWheelConfiguration[] array = wheelConfiguration;
            foreach (VehicleWheelConfiguration vehicleWheelConfiguration in array)
            {
                datWriter.WriteDictionaryStart();
                datWriter.WriteKeyValue("WheelColliderPath", vehicleWheelConfiguration.wheelColliderPath);
                datWriter.WriteKeyValue("IsColliderPowered", vehicleWheelConfiguration.isColliderPowered);
                datWriter.WriteKeyValue("ModelPath", vehicleWheelConfiguration.modelPath);
                datWriter.WriteKeyValue("IsModelSteered", vehicleWheelConfiguration.isModelSteered);
                datWriter.WriteKeyValue("SteeringMode", vehicleWheelConfiguration.steeringMode.ToString());
                datWriter.WriteDictionaryEnd();
            }
            datWriter.WriteListEnd();
            message = stringWriter.ToString();
        }
        UnturnedLog.info("Converted \"" + FriendlyName + "\" wheel configuration:");
        UnturnedLog.info(message);
    }

    /// <summary>
    /// Nelson 2024-02-28: Prior to the VehicleWheelConfiguration class, most of the wheel configuration was
    /// inferred during InteractableVehicle initialization from the children of the "Tires" and "Wheels" transforms.
    /// Confusingly, "Tires" only contains WheelColliders and "Wheels" only contains the visual models. Rather than
    /// keeping the old behavior in InteractableVehicle alongside the newer more configurable one, we match the old
    /// behavior here to generate an equivalent configuration.
    ///
    /// Note that <see cref="P:SDG.Unturned.VehicleAsset.steeringTireIndices" /> must be initialized before this is called (by loading model).
    /// </summary>
    private void BuildAutomaticWheelConfiguration(GameObject vehicleGameObject)
    {
        Transform transform = vehicleGameObject.transform;
        List<VehicleWheelConfiguration> list = new List<VehicleWheelConfiguration>();
        Transform transform2 = transform.Find("Tires");
        if (transform2 != null)
        {
            for (int i = 0; i < transform2.childCount; i++)
            {
                string text = "Tire_" + i;
                Transform transform3 = transform2.Find(text);
                if (transform3 == null)
                {
                    Assets.ReportError(this, "missing \"{0}\" Transform", text);
                    continue;
                }
                if (transform3.GetComponent<WheelCollider>() == null)
                {
                    Assets.ReportError(this, "missing \"{0}\" WheelCollider", text);
                    continue;
                }
                VehicleWheelConfiguration vehicleWheelConfiguration = new VehicleWheelConfiguration();
                vehicleWheelConfiguration.wasAutomaticallyGenerated = true;
                vehicleWheelConfiguration.wheelColliderPath = "Tires/" + text;
                vehicleWheelConfiguration.steeringMode = ((i < 2) ? EWheelSteeringMode.SteeringAngle : EWheelSteeringMode.None);
                vehicleWheelConfiguration.isColliderPowered = i >= transform2.childCount - 2;
                list.Add(vehicleWheelConfiguration);
            }
        }
        Transform transform4 = transform.Find("Wheels");
        if (transform4 != null)
        {
            foreach (VehicleWheelConfiguration item in list)
            {
                Transform transform5 = transform.Find(item.wheelColliderPath);
                if (transform5 == null)
                {
                    continue;
                }
                int num = -1;
                float num2 = 16f;
                for (int j = 0; j < transform4.childCount; j++)
                {
                    Transform child = transform4.GetChild(j);
                    float sqrMagnitude = (transform5.position - child.position).sqrMagnitude;
                    if (sqrMagnitude < num2)
                    {
                        num = j;
                        num2 = sqrMagnitude;
                    }
                }
                if (num == -1)
                {
                    continue;
                }
                Transform transform6 = transform4.GetChild(num);
                if (transform6.childCount < 1)
                {
                    Transform transform7 = transform.FindChildRecursive("Wheel_" + num);
                    if (transform7 != null)
                    {
                        transform6 = transform7;
                    }
                }
                string text2 = transform6.name;
                Transform parent = transform6.parent;
                while (parent != transform)
                {
                    text2 = parent.name + "/" + text2;
                    parent = parent.parent;
                }
                item.modelPath = text2;
            }
            foreach (Transform item2 in transform4)
            {
                if (item2.childCount < 1)
                {
                    continue;
                }
                bool flag = false;
                foreach (VehicleWheelConfiguration item3 in list)
                {
                    if (!string.IsNullOrEmpty(item3.modelPath))
                    {
                        Transform transform9 = transform.Find(item3.modelPath);
                        if (!(transform9 == null) && transform9 == item2)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    VehicleWheelConfiguration vehicleWheelConfiguration2 = new VehicleWheelConfiguration();
                    vehicleWheelConfiguration2.wasAutomaticallyGenerated = true;
                    vehicleWheelConfiguration2.modelPath = "Wheels/" + item2.name;
                    list.Add(vehicleWheelConfiguration2);
                }
            }
            if (steeringTireIndices != null)
            {
                int[] array = steeringTireIndices;
                for (int k = 0; k < array.Length; k++)
                {
                    int num3 = array[k];
                    string n = "Wheel_" + num3;
                    Transform transform10 = transform4.Find(n);
                    if (transform10 == null)
                    {
                        transform10 = transform.FindChildRecursive(n);
                        if (transform10 == null && num3 < transform4.childCount)
                        {
                            transform10 = transform4.GetChild(num3);
                        }
                    }
                    if (transform10 == null)
                    {
                        continue;
                    }
                    VehicleWheelConfiguration vehicleWheelConfiguration3 = null;
                    foreach (VehicleWheelConfiguration item4 in list)
                    {
                        if (!string.IsNullOrEmpty(item4.modelPath))
                        {
                            Transform transform11 = transform.Find(item4.modelPath);
                            if (!(transform11 == null) && transform11 == transform10)
                            {
                                vehicleWheelConfiguration3 = item4;
                                break;
                            }
                        }
                    }
                    if (vehicleWheelConfiguration3 != null)
                    {
                        vehicleWheelConfiguration3.isModelSteered = true;
                    }
                    else
                    {
                        Assets.ReportError(this, "unable to match physical tire with steering tire model {0}", num3);
                    }
                }
            }
        }
        wheelConfiguration = list.ToArray();
        if ((bool)clLogWheelConfiguration)
        {
            LogWheelConfigurationDatConversion();
        }
    }

    public bool IsExplosionEffectRefNull()
    {
        if (_explosion == 0)
        {
            return _explosionEffectGuid.IsEmpty();
        }
        return false;
    }

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionEffectGuid, _explosion);
    }

    /// <summary>
    /// Pick a random paint color according to <see cref="F:SDG.Unturned.VehicleAsset.defaultPaintColorMode" />. Null if unsupported or not configured.
    /// </summary>
    public Color32? GetRandomDefaultPaintColor()
    {
        if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
        {
            if (DefaultPaintColors != null && DefaultPaintColors.Count > 0)
            {
                return DefaultPaintColors.RandomOrDefault();
            }
        }
        else if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale && randomPaintColorConfiguration != null)
        {
            if (UnityEngine.Random.value < randomPaintColorConfiguration.grayscaleChance)
            {
                float num = UnityEngine.Random.Range(randomPaintColorConfiguration.minValue, randomPaintColorConfiguration.maxValue);
                return new Color(num, num, num, 1f);
            }
            float value = UnityEngine.Random.value;
            float s = UnityEngine.Random.Range(randomPaintColorConfiguration.minSaturation, randomPaintColorConfiguration.maxSaturation);
            float v = UnityEngine.Random.Range(randomPaintColorConfiguration.minValue, randomPaintColorConfiguration.maxValue);
            return Color.HSVToRGB(value, s, v);
        }
        return null;
    }

    /// <summary>
    /// Returns reverseGearRatio for negative gears, actual value for valid gear number, otherwise zero.
    /// Exposed for plugin use.
    /// </summary>
    public float GetEngineGearRatio(int gearNumber)
    {
        if (gearNumber < 0)
        {
            return reverseGearRatio;
        }
        int num = gearNumber - 1;
        if (forwardGearRatios != null && num >= 0 && num < forwardGearRatios.Length)
        {
            return forwardGearRatios[num];
        }
        return 0f;
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _vehicleName = p.localization.format("Name");
        _pitchIdle = p.data.ParseFloat("Pitch_Idle", -1f);
        _pitchDrive = p.data.ParseFloat("Pitch_Drive", -1f);
        _engine = p.data.ParseEnum("Engine", EEngine.CAR);
        physicsProfileRef = p.data.readAssetReference<VehiclePhysicsProfileAsset>("Physics_Profile");
        int defaultValue = ((engine != 0) ? 1 : 2);
        _hasCrawler = p.data.ContainsKey("Crawler");
        if (hasCrawler)
        {
            defaultValue = 0;
        }
        numSteeringTires = p.data.ParseInt32("Num_Steering_Tires", defaultValue);
        steeringTireIndices = new int[numSteeringTires];
        for (int i = 0; i < numSteeringTires; i++)
        {
            steeringTireIndices[i] = p.data.ParseInt32("Steering_Tire_" + i, i);
        }
        if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
        {
            p.bundle.loadDeferred("Clip", out legacyServerModel, (LoadedAssetDeferredCallback<GameObject>)OnServerModelLoaded);
        }
        p.bundle.loadDeferred("Vehicle", out clientModel, (LoadedAssetDeferredCallback<GameObject>)OnClientModelLoaded);
        _size2_z = p.data.ParseFloat("Size2_Z");
        _sharedSkinName = p.data.GetString("Shared_Skin_Name");
        if (p.data.ContainsKey("Shared_Skin_Lookup_ID"))
        {
            _sharedSkinLookupID = p.data.ParseGuidOrLegacyId("Shared_Skin_Lookup_ID", out _sharedSkinLookupGuid);
        }
        else
        {
            _sharedSkinLookupGuid = GUID;
            _sharedSkinLookupID = id;
        }
        if (p.data.ContainsKey("Rarity"))
        {
            _rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), p.data.GetString("Rarity"), ignoreCase: true);
        }
        else
        {
            _rarity = EItemRarity.COMMON;
        }
        _hasZip = p.data.ContainsKey("Zip");
        _hasBicycle = p.data.ContainsKey("Bicycle");
        isReclined = p.data.ContainsKey("Reclined");
        _hasLockMouse = p.data.ContainsKey("LockMouse");
        _hasTraction = p.data.ContainsKey("Traction");
        _hasSleds = p.data.ContainsKey("Sleds");
        canSpawnWithBattery = !p.data.ContainsKey("Cannot_Spawn_With_Battery");
        if (p.data.ContainsKey("Battery_Spawn_Charge_Multiplier"))
        {
            batterySpawnChargeMultiplier = p.data.ParseFloat("Battery_Spawn_Charge_Multiplier");
        }
        else
        {
            batterySpawnChargeMultiplier = 1f;
        }
        if (p.data.ContainsKey("Battery_Burn_Rate"))
        {
            batteryBurnRate = p.data.ParseFloat("Battery_Burn_Rate");
        }
        else
        {
            batteryBurnRate = 20f;
        }
        if (p.data.ContainsKey("Battery_Charge_Rate"))
        {
            batteryChargeRate = p.data.ParseFloat("Battery_Charge_Rate");
        }
        else
        {
            batteryChargeRate = 20f;
        }
        batteryDriving = p.data.ParseEnum("BatteryMode_Driving", EBatteryMode.Charge);
        batteryEmpty = p.data.ParseEnum("BatteryMode_Empty", EBatteryMode.None);
        batteryHeadlights = p.data.ParseEnum("BatteryMode_Headlights", EBatteryMode.Burn);
        batterySirens = p.data.ParseEnum("BatteryMode_Sirens", EBatteryMode.Burn);
        defaultBatteryGuid = p.data.ParseGuid("Default_Battery", VANILLA_BATTERY_ITEM);
        float defaultValue2 = ((engine == EEngine.CAR) ? 2.05f : 4.2f);
        fuelBurnRate = p.data.ParseFloat("Fuel_Burn_Rate", defaultValue2);
        _ignition = LoadRedirectableAsset<AudioClip>(p.bundle, "Ignition", p.data, "IgnitionAudioClip");
        _horn = LoadRedirectableAsset<AudioClip>(p.bundle, "Horn", p.data, "HornAudioClip");
        hasHorn = p.data.ParseBool("Has_Horn", _horn != null);
        TargetReverseVelocity = p.data.ParseFloat("Speed_Min");
        TargetForwardVelocity = p.data.ParseFloat("Speed_Max");
        if (engine != EEngine.TRAIN)
        {
            TargetForwardVelocity *= 1.25f;
        }
        if (!p.data.TryParseFloat("Steering_Angle_Max", out _steerMax))
        {
            _steerMax = p.data.ParseFloat("Steer_Max") * 0.75f;
        }
        if (p.data.TryParseFloat("Steering_Angle_FullSpeed_Factor", out var value))
        {
            _steerMin = _steerMax * value;
        }
        else
        {
            _steerMin = p.data.ParseFloat("Steer_Min");
        }
        CrawlerTrackSteeringTorque = p.data.ParseFloat("CrawlerTrackSteering_Torque");
        CrawlerTrackSteeringSidewaysFrictionMultiplier = p.data.ParseFloat("CrawlerTrackSteering_SidewaysFrictionMultiplier", 1f);
        CrawlerTrackSteeringMaxSpeedScale = p.data.ParseFloat("CrawlerTrackSteering_MaxSpeedScale", 1f);
        SteeringAngleTurnSpeed = p.data.ParseFloat("Steering_Angle_Turn_Speed", _steerMax * 5f);
        steeringLeaningForceMultiplier = p.data.ParseFloat("Steering_LeaningForceMultiplier", -1f);
        steeringLeaningForceShouldScaleWithSpeed = p.data.ParseBool("Steering_LeaningForce_ScaleWithSpeed");
        if (steeringLeaningForceShouldScaleWithSpeed)
        {
            steeringLeaningForceSpeedExponent = p.data.ParseFloat("Steering_LeaningForce_SpeedExponent", 1f);
        }
        _brake = p.data.ParseFloat("Brake");
        _lift = p.data.ParseFloat("Lift");
        _fuelMin = p.data.ParseUInt16("Fuel_Min", 0);
        _fuelMax = p.data.ParseUInt16("Fuel_Max", 0);
        _fuel = p.data.ParseUInt16("Fuel", 0);
        _healthMin = p.data.ParseUInt16("Health_Min", 0);
        _healthMax = p.data.ParseUInt16("Health_Max", 0);
        _health = p.data.ParseUInt16("Health", 0);
        _explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionEffectGuid);
        bool defaultValue3 = !IsExplosionEffectRefNull();
        ShouldExplosionCauseDamage = p.data.ParseBool("ShouldExplosionCauseDamage", defaultValue3);
        ShouldExplosionBurnMaterials = p.data.ParseBool("ShouldExplosionBurnMaterials", defaultValue3);
        if (ShouldExplosionBurnMaterials)
        {
            explosionBurnMaterialSections = p.data.ParseArrayOfStructs<PaintableVehicleSection>("ExplosionBurnMaterialSections");
        }
        float num = p.data.ParseFloat("Explosion_Force_Multiplier", 1f);
        if (p.data.TryParseVector3("Explosion_Min_Force", out var value2))
        {
            minExplosionForce = value2 * num;
        }
        else if (p.data.ContainsKey("Explosion_Min_Force_Y"))
        {
            minExplosionForce = p.data.LegacyParseVector3("Explosion_Min_Force") * num;
        }
        else
        {
            minExplosionForce = new Vector3(0f, 1024f * num, 0f);
        }
        if (p.data.TryParseVector3("Explosion_Max_Force", out var value3))
        {
            maxExplosionForce = value3 * num;
        }
        else if (p.data.ContainsKey("Explosion_Max_Force_Y"))
        {
            maxExplosionForce = p.data.LegacyParseVector3("Explosion_Max_Force") * num;
        }
        else
        {
            maxExplosionForce = new Vector3(0f, 1024f * num, 0f);
        }
        if (p.data.ContainsKey("Exit"))
        {
            _exit = p.data.ParseFloat("Exit");
        }
        else
        {
            _exit = 2f;
        }
        if (p.data.ContainsKey("Cam_Follow_Distance"))
        {
            _camFollowDistance = p.data.ParseFloat("Cam_Follow_Distance");
        }
        else
        {
            _camFollowDistance = 5.5f;
        }
        camDriverOffset = p.data.ParseFloat("Cam_Driver_Offset");
        camPassengerOffset = p.data.ParseFloat("Cam_Passenger_Offset");
        if (p.data.ContainsKey("Bumper_Multiplier"))
        {
            _bumperMultiplier = p.data.ParseFloat("Bumper_Multiplier");
        }
        else
        {
            _bumperMultiplier = 1f;
        }
        BumperSpeedDamageThreshold = p.data.ParseFloat("Bumper_SpeedDamageThreshold", 3f);
        BumperPlayerDamage = p.data.ParseFloat("Bumper_PlayerDamage", 10f);
        BumperZombieDamage = p.data.ParseFloat("Bumper_ZombieDamage", 15f);
        BumperAnimalDamage = p.data.ParseFloat("Bumper_AnimalDamage", 15f);
        BumperObjectDamage = p.data.ParseFloat("Bumper_ObjectDamage", 30f);
        BumperResourceDamage = p.data.ParseFloat("Bumper_ResourceDamage", 85f);
        BumperSelfDamageMultiplier = p.data.ParseFloat("Bumper_SelfDamageMultiplier", 1f);
        if (p.data.ContainsKey("Passenger_Explosion_Armor"))
        {
            _passengerExplosionArmor = p.data.ParseFloat("Passenger_Explosion_Armor");
        }
        else
        {
            _passengerExplosionArmor = 1f;
        }
        if (engine == EEngine.HELICOPTER || engine == EEngine.BLIMP)
        {
            _sqrDelta = MathfEx.Square(TargetForwardVelocity * 0.125f);
        }
        else
        {
            _sqrDelta = MathfEx.Square(TargetForwardVelocity * 0.1f);
        }
        if (p.data.ContainsKey("Valid_Speed_Horizontal"))
        {
            float x = p.data.ParseFloat("Valid_Speed_Horizontal") * PlayerInput.RATE;
            _sqrDelta = MathfEx.Square(x);
        }
        float defaultValue4;
        float defaultValue5;
        switch (engine)
        {
        case EEngine.CAR:
            defaultValue4 = 12.5f;
            defaultValue5 = 25f;
            break;
        case EEngine.BOAT:
            defaultValue4 = 3.25f;
            defaultValue5 = 25f;
            break;
        default:
            defaultValue4 = 100f;
            defaultValue5 = 100f;
            break;
        }
        validSpeedUp = p.data.ParseFloat("Valid_Speed_Up", defaultValue4);
        validSpeedDown = p.data.ParseFloat("Valid_Speed_Down", defaultValue5);
        ValidHitDistanceMultiplier = p.data.ParseFloat("Valid_Hit_Distance_Multiplier", 1f);
        _turrets = new TurretInfo[p.data.ParseUInt8("Turrets", 0)];
        for (byte b = 0; b < turrets.Length; b++)
        {
            TurretInfo turretInfo = new TurretInfo();
            turretInfo.seatIndex = p.data.ParseUInt8("Turret_" + b + "_Seat_Index", 0);
            turretInfo.itemID = p.data.ParseUInt16("Turret_" + b + "_Item_ID", 0);
            turretInfo.yawMin = p.data.ParseFloat("Turret_" + b + "_Yaw_Min");
            turretInfo.yawMax = p.data.ParseFloat("Turret_" + b + "_Yaw_Max");
            turretInfo.pitchMin = p.data.ParseFloat("Turret_" + b + "_Pitch_Min");
            turretInfo.pitchMax = p.data.ParseFloat("Turret_" + b + "_Pitch_Max");
            turretInfo.useAimCamera = !p.data.ContainsKey("Turret_" + b + "_Ignore_Aim_Camera");
            _turrets[b] = turretInfo;
        }
        isVulnerable = !p.data.ContainsKey("Invulnerable");
        isVulnerableToExplosions = !p.data.ContainsKey("Explosions_Invulnerable");
        isVulnerableToEnvironment = !p.data.ContainsKey("Environment_Invulnerable");
        isVulnerableToBumper = !p.data.ContainsKey("Bumper_Invulnerable");
        canTiresBeDamaged = !p.data.ContainsKey("Tires_Invulnerable");
        canRepairWhileSeated = p.data.ParseBool("Can_Repair_While_Seated");
        childExplosionArmorMultiplier = p.data.ParseFloat("Child_Explosion_Armor_Multiplier", 0.2f);
        if (p.data.ContainsKey("Air_Turn_Responsiveness"))
        {
            airTurnResponsiveness = p.data.ParseFloat("Air_Turn_Responsiveness");
        }
        else
        {
            airTurnResponsiveness = 2f;
        }
        if (p.data.ContainsKey("Air_Steer_Min"))
        {
            airSteerMin = p.data.ParseFloat("Air_Steer_Min");
        }
        else
        {
            airSteerMin = MaxSteeringAngleAtFullSpeed;
        }
        if (p.data.ContainsKey("Air_Steer_Max"))
        {
            airSteerMax = p.data.ParseFloat("Air_Steer_Max");
        }
        else
        {
            airSteerMax = MaxSteeringAngle;
        }
        bicycleAnimSpeed = p.data.ParseFloat("Bicycle_Anim_Speed");
        staminaBoost = p.data.ParseFloat("Stamina_Boost");
        useStaminaBoost = p.data.ContainsKey("Stamina_Boost");
        isStaminaPowered = p.data.ContainsKey("Stamina_Powered");
        isBatteryPowered = p.data.ContainsKey("Battery_Powered");
        if (p.data.TryParseEnum<EVehicleBuildablePlacementRule>("Buildable_Placement_Rule", out var value4))
        {
            BuildablePlacementRule = value4;
        }
        else if (p.data.ContainsKey("Supports_Mobile_Buildables"))
        {
            BuildablePlacementRule = EVehicleBuildablePlacementRule.AlwaysAllow;
        }
        else
        {
            BuildablePlacementRule = EVehicleBuildablePlacementRule.None;
        }
        shouldSpawnSeatCapsules = p.data.ParseBool("Should_Spawn_Seat_Capsules");
        canBeLocked = p.data.ParseBool("Can_Be_Locked", defaultValue: true);
        canStealBattery = p.data.ParseBool("Can_Steal_Battery", defaultValue: true);
        trunkStorage_X = p.data.ParseUInt8("Trunk_Storage_X", 0);
        trunkStorage_Y = p.data.ParseUInt8("Trunk_Storage_Y", 0);
        dropsTableId = p.data.ParseUInt16("Drops_Table_ID", 962);
        dropsMin = p.data.ParseUInt8("Drops_Min", 3);
        dropsMax = p.data.ParseUInt8("Drops_Max", 7);
        tireID = p.data.ParseUInt16("Tire_ID", 1451);
        hasCenterOfMassOverride = p.data.ParseBool("Override_Center_Of_Mass");
        if (hasCenterOfMassOverride)
        {
            centerOfMass = p.data.LegacyParseVector3("Center_Of_Mass");
        }
        carjackForceMultiplier = p.data.ParseFloat("Carjack_Force_Multiplier", 1f);
        engineForceMultiplier = p.data.ParseFloat("Engine_Force_Multiplier", 1f);
        if (p.data.ContainsKey("Wheel_Collider_Mass_Override"))
        {
            wheelColliderMassOverride = p.data.ParseFloat("Wheel_Collider_Mass_Override", 1f);
        }
        else
        {
            wheelColliderMassOverride = null;
        }
        trainTrackOffset = p.data.ParseFloat("Train_Track_Offset");
        trainWheelOffset = p.data.ParseFloat("Train_Wheel_Offset");
        trainCarLength = p.data.ParseFloat("Train_Car_Length");
        _shouldVerifyHash = !p.data.ContainsKey("Bypass_Hash_Verification");
        if (!Dedicator.IsDedicatedServer && id < 2000)
        {
            _albedoBase = p.bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = p.bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = p.bundle.load<Texture2D>("Emission_Base");
        }
        CanDecay = engine != EEngine.TRAIN && (isVulnerable | isVulnerableToExplosions | isVulnerableToEnvironment | isVulnerableToBumper);
        PaintableVehicleSections = p.data.ParseArrayOfStructs<PaintableVehicleSection>("PaintableSections");
        if (SupportsPaintColor)
        {
            IsPaintable = p.data.ParseBool("IsPaintable", defaultValue: true);
        }
        else
        {
            IsPaintable = false;
        }
        crawlerTrackTilingMaterials = p.data.ParseArrayOfStructs<CrawlerTrackTilingMaterial>("CrawlerTrackTilingMaterials");
        if (p.data.TryGetList("AdditionalTransparentSections", out var node))
        {
            extraTransparentSections = node.ParseArrayOfStructs<PaintableVehicleSection>();
        }
        IDatList node2;
        bool flag = p.data.TryGetList("DefaultPaintColors", out node2);
        defaultPaintColorMode = p.data.ParseEnum("DefaultPaintColor_Mode", flag ? EVehicleDefaultPaintColorMode.List : EVehicleDefaultPaintColorMode.None);
        if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
        {
            DefaultPaintColors = new List<Color32>(node2.Count);
            foreach (IDatNode item in node2)
            {
                if (item is IDatValue node3 && node3.TryParseColor32RGB(out var value5))
                {
                    DefaultPaintColors.Add(value5);
                }
            }
        }
        else if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale)
        {
            randomPaintColorConfiguration = new VehicleRandomPaintColorConfiguration();
            if (p.data.TryGetDictionary("DefaultPaintColor_Configuration", out var node4))
            {
                if (!randomPaintColorConfiguration.TryParse(node4))
                {
                    Assets.ReportError(this, "unable to parse DefaultPaintColor_Configuration");
                }
            }
            else
            {
                Assets.ReportError(this, "missing DefaultPaintColor_Configuration");
            }
        }
        wheelBalancingForceMultiplier = p.data.ParseFloat("WheelBalancing_ForceMultiplier", -1f);
        wheelBalancingUprightExponent = p.data.ParseFloat("WheelBalancing_UprightExponent", 1.5f);
        rollAngularVelocityDamping = p.data.ParseFloat("RollAngularVelocityDamping", -1f);
        ShouldIncludeAirbornWheelsInAverageRpm = p.data.ParseBool("Include_Airborn_Wheels_In_Average_RPM", defaultValue: true);
        if (p.data.TryGetList("WheelConfigurations", out var node5))
        {
            List<VehicleWheelConfiguration> list = new List<VehicleWheelConfiguration>();
            List<int> list2 = new List<int>();
            List<int> list3 = new List<int>();
            foreach (IDatNode item2 in node5)
            {
                VehicleWheelConfiguration vehicleWheelConfiguration = new VehicleWheelConfiguration();
                if (vehicleWheelConfiguration.TryParse(item2))
                {
                    if (vehicleWheelConfiguration.modelUseColliderPose)
                    {
                        int count = list.Count;
                        list2.Add(count);
                    }
                    if (vehicleWheelConfiguration.isColliderPowered)
                    {
                        int count2 = list.Count;
                        list3.Add(count2);
                    }
                    list.Add(vehicleWheelConfiguration);
                }
                else
                {
                    Assets.reportError("Unable to parse entry in WheelConfigurations list: " + item2.DebugDumpToString());
                }
            }
            wheelConfiguration = list.ToArray();
            if (list2.Count > 0)
            {
                replicatedWheelIndices = list2.ToArray();
            }
            if (list3.Count > 0)
            {
                poweredWheelIndices = list3.ToArray();
            }
        }
        reverseGearRatio = p.data.ParseFloat("ReverseGearRatio", 1f);
        if (p.data.TryGetList("ForwardGearRatios", out var node6))
        {
            List<float> list4 = new List<float>();
            foreach (IDatNode item3 in node6)
            {
                if (item3 is IDatValue valueNode && valueNode.TryParseFloat(out var value6))
                {
                    list4.Add(value6);
                }
            }
            if (list4.Count > 0)
            {
                forwardGearRatios = list4.ToArray();
                AllowsEngineRpmAndGearsInHud = p.data.ParseBool("GearShift_VisibleInHUD", defaultValue: true);
            }
        }
        GearShiftDownThresholdRpm = p.data.ParseFloat("GearShift_DownThresholdRPM", 1500f);
        GearShiftUpThresholdRpm = p.data.ParseFloat("GearShift_UpThresholdRPM", 5500f);
        GearShiftDuration = p.data.ParseFloat("GearShift_Duration", 0.5f);
        GearShiftInterval = p.data.ParseFloat("GearShift_Interval", 1f);
        GearShiftAllowSkippingGears = p.data.ParseBool("GearShift_AllowSkippingGears", defaultValue: true);
        EngineIdleRpm = p.data.ParseFloat("EngineIdleRPM", 1000f);
        EngineMaxRpm = p.data.ParseFloat("EngineMaxRPM", 7000f);
        EngineRpmIncreaseRate = p.data.ParseFloat("EngineRPM_IncreaseRate", -1f);
        EngineRpmDecreaseRate = p.data.ParseFloat("EngineRPM_DecreaseRate", -1f);
        EngineMaxTorque = p.data.ParseFloat("EngineMaxTorque", 1f);
        EngineRpmMismatchTorqueReductionEnabled = p.data.ParseBool("EngineRPMMismatch_TorqueReduction_Enabled");
        EngineRpmMismatchTorqueReductionThreshold = p.data.ParseFloat("EngineRPMMismatch_TorqueReduction_Threshold");
        EngineRpmMismatchGearShiftPreventShifting = p.data.ParseBool("EngineRPMMismatch_GearShift_PreventShifting");
        if (EngineRpmMismatchGearShiftPreventShifting)
        {
            EngineRpmMismatchGearShiftUpMinThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_UpMinThreshold");
            EngineRpmMismatchGearShiftUpMaxThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_UpMaxThreshold");
            EngineRpmMismatchGearShiftDownMinThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_DownMinThreshold");
            EngineRpmMismatchGearShiftDownMaxThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_DownMaxThreshold");
        }
        engineSoundType = p.data.ParseEnum("EngineSound_Type", EVehicleEngineSoundType.Legacy);
        if (engineSoundType == EVehicleEngineSoundType.EngineRPMSimple)
        {
            engineSoundConfiguration = new RpmEngineSoundConfiguration();
            if (p.data.TryGetDictionary("EngineSound", out var node7))
            {
                engineSoundConfiguration.TryParse(node7);
            }
        }
        if (UsesEngineRpmAndGears && (bool)Assets.shouldValidateAssets)
        {
            GameObject orLoadModel = GetOrLoadModel();
            if (orLoadModel != null && orLoadModel.GetComponent<EngineCurvesComponent>() == null)
            {
                Assets.ReportError(this, "needs EngineCurvesComponent on vehicle prefab for engine RPM and gearbox to work properly");
            }
        }
        this.PopulateArmorFalloff(in p);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Locale_Vehicle");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Name", FriendlyName);
        CargoDeclaration orAddDeclaration2 = builder.GetOrAddDeclaration("Vehicle");
        orAddDeclaration2.Append("GUID", GUID);
        orAddDeclaration2.Append("Air_Steer_Max", airSteerMax);
        orAddDeclaration2.Append("Air_Steer_Min", airSteerMin);
        orAddDeclaration2.Append("Air_Turn_Responsiveness", airTurnResponsiveness);
        orAddDeclaration2.Append("BatteryMode_Driving", batteryDriving);
        orAddDeclaration2.Append("BatteryMode_Empty", batteryEmpty);
        orAddDeclaration2.Append("BatteryMode_Headlights", batteryHeadlights);
        orAddDeclaration2.Append("BatteryMode_Sirens", batterySirens);
        orAddDeclaration2.Append("Battery_Burn_Rate", batteryBurnRate);
        orAddDeclaration2.Append("Battery_Charge_Rate", batteryChargeRate);
        orAddDeclaration2.Append("Battery_Powered", isBatteryPowered);
        orAddDeclaration2.Append("Battery_Spawn_Charge_Multiplier", batterySpawnChargeMultiplier);
        orAddDeclaration2.Append("Bicycle", hasBicycle);
        orAddDeclaration2.Append("Buildable_Placement_Rule", BuildablePlacementRule);
        orAddDeclaration2.Append("isVulnerableToBumper", isVulnerableToBumper);
        orAddDeclaration2.Append("Bumper_Multiplier", bumperMultiplier);
        orAddDeclaration2.Append("Brake", brake);
        orAddDeclaration2.Append("Can_Be_Locked", canBeLocked);
        orAddDeclaration2.Append("Can_Repair_While_Seated", canRepairWhileSeated);
        orAddDeclaration2.Append("Can_Steal_Battery", canStealBattery);
        orAddDeclaration2.Append("canSpawnWithBattery", canSpawnWithBattery);
        orAddDeclaration2.Append("Carjack_Force_Multiplier", carjackForceMultiplier);
        orAddDeclaration2.Append("Child_Explosion_Armor_Multiplier", childExplosionArmorMultiplier);
        orAddDeclaration2.Append("Crawler", _hasCrawler);
        orAddDeclaration2.Append("DefaultPaintColor_Mode", defaultPaintColorMode);
        orAddDeclaration2.Append("Default_Battery", defaultBatteryGuid);
        orAddDeclaration2.Append("Drops_Max", dropsMax);
        orAddDeclaration2.Append("Drops_Min", dropsMin);
        orAddDeclaration2.Append("Drops_Table_ID", dropsTableId);
        orAddDeclaration2.Append("Engine", engine);
        orAddDeclaration2.Append("EngineIdleRPM", EngineIdleRpm);
        orAddDeclaration2.Append("EngineMaxRPM", EngineMaxRpm);
        orAddDeclaration2.Append("EngineMaxTorque", EngineMaxTorque);
        orAddDeclaration2.Append("EngineRPM_DecreaseRate", EngineRpmDecreaseRate);
        orAddDeclaration2.Append("EngineRPM_IncreaseRate", EngineRpmIncreaseRate);
        orAddDeclaration2.Append("Engine_Force_Multiplier", engineForceMultiplier);
        orAddDeclaration2.Append("isVulnerableToEnvironment", isVulnerableToEnvironment);
        orAddDeclaration2.Append("isVulnerableToExplosions", isVulnerableToExplosions);
        orAddDeclaration2.Append("Fuel", fuel);
        orAddDeclaration2.Append("Fuel_Burn_Rate", fuelBurnRate);
        orAddDeclaration2.Append("Fuel_Max", fuelMax);
        orAddDeclaration2.Append("Fuel_Min", fuelMin);
        orAddDeclaration2.Append("Has_Horn", hasHorn);
        orAddDeclaration2.Append("Health", health);
        orAddDeclaration2.Append("Health_Max", healthMax);
        orAddDeclaration2.Append("Health_Min", healthMin);
        orAddDeclaration2.Append("HornAudioClip", (object)horn);
        orAddDeclaration2.Append("isVulnerable", isVulnerable);
        orAddDeclaration2.Append("IsPaintable", IsPaintable);
        orAddDeclaration2.Append("Lift", lift);
        orAddDeclaration2.Append("LockMouse", hasLockMouse);
        orAddDeclaration2.Append("Passenger_Explosion_Armor", passengerExplosionArmor);
        orAddDeclaration2.Append("Physics_Profile", physicsProfileRef);
        orAddDeclaration2.Append("Rarity", rarity);
        orAddDeclaration2.Append("ShouldExplosionCauseDamage", ShouldExplosionCauseDamage);
        orAddDeclaration2.Append("Should_Spawn_Seat_Capsules", shouldSpawnSeatCapsules);
        orAddDeclaration2.Append("Sleds", hasSleds);
        orAddDeclaration2.Append("TargetForwardVelocity", TargetForwardVelocity);
        orAddDeclaration2.Append("TargetReverseVelocity", TargetReverseVelocity);
        orAddDeclaration2.Append("Stamina_Boost", staminaBoost);
        orAddDeclaration2.Append("Stamina_Powered", isStaminaPowered);
        orAddDeclaration2.Append("steerMax", MaxSteeringAngle);
        orAddDeclaration2.Append("steerMin", MaxSteeringAngleAtFullSpeed);
        orAddDeclaration2.Append("Steering_Angle_Turn_Speed", SteeringAngleTurnSpeed);
        orAddDeclaration2.Append("Steering_LeaningForceMultiplier", steeringLeaningForceMultiplier);
        orAddDeclaration2.Append("Tire_ID", tireID);
        orAddDeclaration2.Append("canTiresBeDamaged", canTiresBeDamaged);
        orAddDeclaration2.Append("Traction", hasTraction);
        orAddDeclaration2.Append("Trunk_Storage_X", trunkStorage_X);
        orAddDeclaration2.Append("Trunk_Storage_Y", trunkStorage_Y);
        orAddDeclaration2.Append("Turrets", turrets.Length);
        orAddDeclaration2.Append("Valid_Speed_Down", validSpeedDown);
        orAddDeclaration2.Append("Valid_Speed_Up", validSpeedUp);
        orAddDeclaration2.Append("Valid_Hit_Distance_Multiplier", ValidHitDistanceMultiplier);
        orAddDeclaration2.Append("hasSirens", hasSirens);
        orAddDeclaration2.Append("hasHook", hasHook);
        orAddDeclaration2.Append("hasHeadlights", hasHeadlights);
        orAddDeclaration2.Append("numSeats", numSeats);
        if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale)
        {
            CargoDeclaration orAddDeclaration3 = builder.GetOrAddDeclaration("Vehicle_VehicleRandomPaintColorConfiguration");
            orAddDeclaration3.Append("GUID", GUID);
            orAddDeclaration3.Append("MinSaturation", randomPaintColorConfiguration.minSaturation);
            orAddDeclaration3.Append("MaxSaturation", randomPaintColorConfiguration.maxSaturation);
            orAddDeclaration3.Append("MinValue", randomPaintColorConfiguration.minValue);
            orAddDeclaration3.Append("MaxValue", randomPaintColorConfiguration.maxValue);
            orAddDeclaration3.Append("GrayscaleChance", randomPaintColorConfiguration.grayscaleChance);
        }
        if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
        {
            for (byte b = 0; b < DefaultPaintColors.Count; b++)
            {
                CargoDeclaration cargoDeclaration = builder.AddDeclaration("Vehicle_DefaultPaintColors");
                cargoDeclaration.Append("GUID", GUID);
                cargoDeclaration.Append("Color", DefaultPaintColors[b]);
            }
        }
        for (byte b2 = 0; b2 < turrets.Length; b2++)
        {
            CargoDeclaration cargoDeclaration2 = builder.AddDeclaration("Vehicle_TurretInfo");
            cargoDeclaration2.Append("GUID", GUID);
            cargoDeclaration2.Append("turretIndex", b2);
            cargoDeclaration2.Append("useAimCamera", turrets[b2].useAimCamera);
            cargoDeclaration2.Append("Item_ID", turrets[b2].itemID);
            cargoDeclaration2.Append("Pitch_Max", turrets[b2].pitchMax);
            cargoDeclaration2.Append("Pitch_Min", turrets[b2].pitchMin);
            cargoDeclaration2.Append("Seat_Index", turrets[b2].seatIndex);
            cargoDeclaration2.Append("Yaw_Max", turrets[b2].yawMax);
            cargoDeclaration2.Append("Yaw_Min", turrets[b2].yawMin);
        }
    }
}
