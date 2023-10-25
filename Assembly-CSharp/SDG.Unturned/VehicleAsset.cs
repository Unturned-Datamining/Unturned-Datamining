using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class VehicleAsset : Asset, ISkinableAsset
{
    protected bool _shouldVerifyHash;

    protected string _vehicleName;

    protected float _size2_z;

    protected string _sharedSkinName;

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

    protected float _speedMin;

    protected float _speedMax;

    protected float _steerMin;

    protected float _steerMax;

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

    protected bool _hasHeadlights;

    protected bool _hasSirens;

    protected bool _hasHook;

    protected bool _hasZip;

    protected bool _hasBicycle;

    protected bool _hasCrawler;

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

    public bool shouldVerifyHash => _shouldVerifyHash;

    internal override bool ShouldVerifyHash => _shouldVerifyHash;

    public string vehicleName => _vehicleName;

    public override string FriendlyName => _vehicleName;

    public float size2_z => _size2_z;

    public string sharedSkinName => _sharedSkinName;

    public ushort sharedSkinLookupID => _sharedSkinLookupID;

    public EEngine engine => _engine;

    public EItemRarity rarity => _rarity;

    public AudioClip ignition => _ignition;

    public AudioClip horn => _horn;

    public bool hasHorn { get; protected set; }

    public float pitchIdle => _pitchIdle;

    public float pitchDrive => _pitchDrive;

    public float speedMin => _speedMin;

    public float speedMax => _speedMax;

    public float steerMin => _steerMin;

    public float steerMax => _steerMax;

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
    /// Fuel decrease per second.
    /// </summary>
    public float fuelBurnRate { get; protected set; }

    public bool isReclined { get; protected set; }

    public bool hasCrawler => _hasCrawler;

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
    public bool supportsMobileBuildables { get; protected set; }

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

    /// <summary>
    /// Number of tire visuals to rotate with steering wheel.
    /// </summary>
    public int numSteeringTires { get; protected set; }

    public int[] steeringTireIndices { get; protected set; }

    /// <summary>
    /// Was a center of mass specified in the .dat?
    /// </summary>
    public bool hasCenterOfMassOverride { get; protected set; }

    /// <summary>
    /// If hasCenterOfMassOverride, use this value.
    /// </summary>
    public Vector3 centerOfMass { get; protected set; }

    /// <summary>
    /// If set, override the wheel collider mass with this value.
    /// </summary>
    public float? wheelColliderMassOverride { get; protected set; }

    public AssetReference<VehiclePhysicsProfileAsset> physicsProfileRef { get; protected set; }

    public override EAssetType assetCategory => EAssetType.VEHICLE;

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
            Assets.reportError(this, "missing 'Seats' Transform");
        }
        else if (transform.childCount < 1)
        {
            Assets.reportError(this, "empty 'Seats' Transform has zero children");
        }
        Rigidbody component = asset.GetComponent<Rigidbody>();
        if (component == null)
        {
            Assets.reportError(this, "missing root Rigidbody");
        }
        else if (physicsProfileRef.isNull && MathfEx.IsNearlyEqual(component.mass, 1f))
        {
            bool flag = true;
            Transform transform2 = asset.transform.Find("Tires");
            if (transform2 != null)
            {
                for (int i = 0; i < transform2.childCount; i++)
                {
                    Transform child = transform2.GetChild(i);
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
    }

    /// <summary>
    /// Clip.prefab
    /// </summary>
    protected void OnServerModelLoaded(GameObject asset)
    {
        if (asset == null)
        {
            Assets.reportError(this, "missing \"Clip\" GameObject, loading \"Vehicle\" GameObject instead");
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
            Assets.reportError(this, "missing \"Vehicle\" GameObject");
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
            ServerPrefabUtil.RemoveClientComponents(asset);
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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (id < 200 && !base.OriginAllowsVanillaLegacyId && !data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 200");
        }
        _vehicleName = localization.format("Name");
        _pitchIdle = data.ParseFloat("Pitch_Idle", -1f);
        _pitchDrive = data.ParseFloat("Pitch_Drive", -1f);
        _engine = data.ParseEnum("Engine", EEngine.CAR);
        physicsProfileRef = data.readAssetReference<VehiclePhysicsProfileAsset>("Physics_Profile");
        if (Dedicator.IsDedicatedServer && data.ParseBool("Has_Clip_Prefab", defaultValue: true))
        {
            bundle.loadDeferred("Clip", out legacyServerModel, (LoadedAssetDeferredCallback<GameObject>)OnServerModelLoaded);
        }
        bundle.loadDeferred("Vehicle", out clientModel, (LoadedAssetDeferredCallback<GameObject>)OnClientModelLoaded);
        _size2_z = data.ParseFloat("Size2_Z");
        _sharedSkinName = data.GetString("Shared_Skin_Name");
        if (data.ContainsKey("Shared_Skin_Lookup_ID"))
        {
            _sharedSkinLookupID = data.ParseUInt16("Shared_Skin_Lookup_ID", 0);
        }
        else
        {
            _sharedSkinLookupID = id;
        }
        if (data.ContainsKey("Rarity"))
        {
            _rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), data.GetString("Rarity"), ignoreCase: true);
        }
        else
        {
            _rarity = EItemRarity.COMMON;
        }
        _hasZip = data.ContainsKey("Zip");
        _hasBicycle = data.ContainsKey("Bicycle");
        isReclined = data.ContainsKey("Reclined");
        _hasCrawler = data.ContainsKey("Crawler");
        _hasLockMouse = data.ContainsKey("LockMouse");
        _hasTraction = data.ContainsKey("Traction");
        _hasSleds = data.ContainsKey("Sleds");
        canSpawnWithBattery = !data.ContainsKey("Cannot_Spawn_With_Battery");
        if (data.ContainsKey("Battery_Spawn_Charge_Multiplier"))
        {
            batterySpawnChargeMultiplier = data.ParseFloat("Battery_Spawn_Charge_Multiplier");
        }
        else
        {
            batterySpawnChargeMultiplier = 1f;
        }
        if (data.ContainsKey("Battery_Burn_Rate"))
        {
            batteryBurnRate = data.ParseFloat("Battery_Burn_Rate");
        }
        else
        {
            batteryBurnRate = 20f;
        }
        if (data.ContainsKey("Battery_Charge_Rate"))
        {
            batteryChargeRate = data.ParseFloat("Battery_Charge_Rate");
        }
        else
        {
            batteryChargeRate = 20f;
        }
        batteryDriving = data.ParseEnum("BatteryMode_Driving", EBatteryMode.Charge);
        batteryEmpty = data.ParseEnum("BatteryMode_Empty", EBatteryMode.None);
        batteryHeadlights = data.ParseEnum("BatteryMode_Headlights", EBatteryMode.Burn);
        batterySirens = data.ParseEnum("BatteryMode_Sirens", EBatteryMode.Burn);
        float defaultValue = ((engine == EEngine.CAR) ? 2.05f : 4.2f);
        fuelBurnRate = data.ParseFloat("Fuel_Burn_Rate", defaultValue);
        _ignition = LoadRedirectableAsset<AudioClip>(bundle, "Ignition", data, "IgnitionAudioClip");
        _horn = LoadRedirectableAsset<AudioClip>(bundle, "Horn", data, "HornAudioClip");
        hasHorn = data.ParseBool("Has_Horn", _horn != null);
        _speedMin = data.ParseFloat("Speed_Min");
        _speedMax = data.ParseFloat("Speed_Max");
        if (engine != EEngine.TRAIN)
        {
            _speedMax *= 1.25f;
        }
        _steerMin = data.ParseFloat("Steer_Min");
        _steerMax = data.ParseFloat("Steer_Max") * 0.75f;
        _brake = data.ParseFloat("Brake");
        _lift = data.ParseFloat("Lift");
        _fuelMin = data.ParseUInt16("Fuel_Min", 0);
        _fuelMax = data.ParseUInt16("Fuel_Max", 0);
        _fuel = data.ParseUInt16("Fuel", 0);
        _healthMin = data.ParseUInt16("Health_Min", 0);
        _healthMax = data.ParseUInt16("Health_Max", 0);
        _health = data.ParseUInt16("Health", 0);
        _explosion = data.ParseGuidOrLegacyId("Explosion", out _explosionEffectGuid);
        bool defaultValue2 = !IsExplosionEffectRefNull();
        ShouldExplosionCauseDamage = data.ParseBool("ShouldExplosionCauseDamage", defaultValue2);
        ShouldExplosionBurnMaterials = data.ParseBool("ShouldExplosionBurnMaterials", defaultValue2);
        if (data.ContainsKey("Explosion_Min_Force_Y"))
        {
            minExplosionForce = data.LegacyParseVector3("Explosion_Min_Force");
        }
        else
        {
            minExplosionForce = new Vector3(0f, 1024f, 0f);
        }
        if (data.ContainsKey("Explosion_Max_Force_Y"))
        {
            maxExplosionForce = data.LegacyParseVector3("Explosion_Max_Force");
        }
        else
        {
            maxExplosionForce = new Vector3(0f, 1024f, 0f);
        }
        if (data.ContainsKey("Exit"))
        {
            _exit = data.ParseFloat("Exit");
        }
        else
        {
            _exit = 2f;
        }
        if (data.ContainsKey("Cam_Follow_Distance"))
        {
            _camFollowDistance = data.ParseFloat("Cam_Follow_Distance");
        }
        else
        {
            _camFollowDistance = 5.5f;
        }
        camDriverOffset = data.ParseFloat("Cam_Driver_Offset");
        camPassengerOffset = data.ParseFloat("Cam_Passenger_Offset");
        if (data.ContainsKey("Bumper_Multiplier"))
        {
            _bumperMultiplier = data.ParseFloat("Bumper_Multiplier");
        }
        else
        {
            _bumperMultiplier = 1f;
        }
        if (data.ContainsKey("Passenger_Explosion_Armor"))
        {
            _passengerExplosionArmor = data.ParseFloat("Passenger_Explosion_Armor");
        }
        else
        {
            _passengerExplosionArmor = 1f;
        }
        if (engine == EEngine.HELICOPTER || engine == EEngine.BLIMP)
        {
            _sqrDelta = MathfEx.Square(speedMax * 0.125f);
        }
        else
        {
            _sqrDelta = MathfEx.Square(speedMax * 0.1f);
        }
        if (data.ContainsKey("Valid_Speed_Horizontal"))
        {
            float x = data.ParseFloat("Valid_Speed_Horizontal") * PlayerInput.RATE;
            _sqrDelta = MathfEx.Square(x);
        }
        float defaultValue3;
        float defaultValue4;
        switch (engine)
        {
        case EEngine.CAR:
            defaultValue3 = 12.5f;
            defaultValue4 = 25f;
            break;
        case EEngine.BOAT:
            defaultValue3 = 3.25f;
            defaultValue4 = 25f;
            break;
        default:
            defaultValue3 = 100f;
            defaultValue4 = 100f;
            break;
        }
        validSpeedUp = data.ParseFloat("Valid_Speed_Up", defaultValue3);
        validSpeedDown = data.ParseFloat("Valid_Speed_Down", defaultValue4);
        _turrets = new TurretInfo[data.ParseUInt8("Turrets", 0)];
        for (byte b = 0; b < turrets.Length; b++)
        {
            TurretInfo turretInfo = new TurretInfo();
            turretInfo.seatIndex = data.ParseUInt8("Turret_" + b + "_Seat_Index", 0);
            turretInfo.itemID = data.ParseUInt16("Turret_" + b + "_Item_ID", 0);
            turretInfo.yawMin = data.ParseFloat("Turret_" + b + "_Yaw_Min");
            turretInfo.yawMax = data.ParseFloat("Turret_" + b + "_Yaw_Max");
            turretInfo.pitchMin = data.ParseFloat("Turret_" + b + "_Pitch_Min");
            turretInfo.pitchMax = data.ParseFloat("Turret_" + b + "_Pitch_Max");
            turretInfo.useAimCamera = !data.ContainsKey("Turret_" + b + "_Ignore_Aim_Camera");
            _turrets[b] = turretInfo;
        }
        isVulnerable = !data.ContainsKey("Invulnerable");
        isVulnerableToExplosions = !data.ContainsKey("Explosions_Invulnerable");
        isVulnerableToEnvironment = !data.ContainsKey("Environment_Invulnerable");
        isVulnerableToBumper = !data.ContainsKey("Bumper_Invulnerable");
        canTiresBeDamaged = !data.ContainsKey("Tires_Invulnerable");
        canRepairWhileSeated = data.ParseBool("Can_Repair_While_Seated");
        childExplosionArmorMultiplier = data.ParseFloat("Child_Explosion_Armor_Multiplier", 0.2f);
        if (data.ContainsKey("Air_Turn_Responsiveness"))
        {
            airTurnResponsiveness = data.ParseFloat("Air_Turn_Responsiveness");
        }
        else
        {
            airTurnResponsiveness = 2f;
        }
        if (data.ContainsKey("Air_Steer_Min"))
        {
            airSteerMin = data.ParseFloat("Air_Steer_Min");
        }
        else
        {
            airSteerMin = steerMin;
        }
        if (data.ContainsKey("Air_Steer_Max"))
        {
            airSteerMax = data.ParseFloat("Air_Steer_Max");
        }
        else
        {
            airSteerMax = steerMax;
        }
        bicycleAnimSpeed = data.ParseFloat("Bicycle_Anim_Speed");
        staminaBoost = data.ParseFloat("Stamina_Boost");
        useStaminaBoost = data.ContainsKey("Stamina_Boost");
        isStaminaPowered = data.ContainsKey("Stamina_Powered");
        isBatteryPowered = data.ContainsKey("Battery_Powered");
        supportsMobileBuildables = data.ContainsKey("Supports_Mobile_Buildables");
        shouldSpawnSeatCapsules = data.ParseBool("Should_Spawn_Seat_Capsules");
        canBeLocked = data.ParseBool("Can_Be_Locked", defaultValue: true);
        canStealBattery = data.ParseBool("Can_Steal_Battery", defaultValue: true);
        trunkStorage_X = data.ParseUInt8("Trunk_Storage_X", 0);
        trunkStorage_Y = data.ParseUInt8("Trunk_Storage_Y", 0);
        dropsTableId = data.ParseUInt16("Drops_Table_ID", 962);
        dropsMin = data.ParseUInt8("Drops_Min", 3);
        dropsMax = data.ParseUInt8("Drops_Max", 7);
        tireID = data.ParseUInt16("Tire_ID", 1451);
        int defaultValue5 = ((engine != 0) ? 1 : 2);
        if (hasCrawler)
        {
            defaultValue5 = 0;
        }
        numSteeringTires = data.ParseInt32("Num_Steering_Tires", defaultValue5);
        steeringTireIndices = new int[numSteeringTires];
        for (int i = 0; i < numSteeringTires; i++)
        {
            steeringTireIndices[i] = data.ParseInt32("Steering_Tire_" + i, i);
        }
        hasCenterOfMassOverride = data.ParseBool("Override_Center_Of_Mass");
        if (hasCenterOfMassOverride)
        {
            centerOfMass = data.LegacyParseVector3("Center_Of_Mass");
        }
        if (data.ContainsKey("Wheel_Collider_Mass_Override"))
        {
            wheelColliderMassOverride = data.ParseFloat("Wheel_Collider_Mass_Override", 1f);
        }
        else
        {
            wheelColliderMassOverride = null;
        }
        trainTrackOffset = data.ParseFloat("Train_Track_Offset");
        trainWheelOffset = data.ParseFloat("Train_Wheel_Offset");
        trainCarLength = data.ParseFloat("Train_Car_Length");
        _shouldVerifyHash = !data.ContainsKey("Bypass_Hash_Verification");
        if (!Dedicator.IsDedicatedServer && id < 2000)
        {
            _albedoBase = bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = bundle.load<Texture2D>("Emission_Base");
        }
        CanDecay = engine != EEngine.TRAIN && (isVulnerable | isVulnerableToExplosions | isVulnerableToEnvironment | isVulnerableToBumper);
    }
}
