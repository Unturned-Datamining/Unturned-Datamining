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

    public bool isVulnerable;

    public bool isVulnerableToExplosions;

    public bool isVulnerableToEnvironment;

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

    public bool ShouldExplosionCauseDamage { get; protected set; }

    public bool ShouldExplosionBurnMaterials { get; protected set; }

    public bool hasHeadlights => _hasHeadlights;

    public bool hasSirens => _hasSirens;

    public bool hasHook => _hasHook;

    public bool hasZip => _hasZip;

    public bool hasBicycle => _hasBicycle;

    public bool canSpawnWithBattery { get; protected set; }

    public float batterySpawnChargeMultiplier { get; protected set; }

    public float batteryBurnRate { get; protected set; }

    public float batteryChargeRate { get; protected set; }

    public EBatteryMode batteryDriving { get; protected set; }

    public EBatteryMode batteryEmpty { get; protected set; }

    public EBatteryMode batteryHeadlights { get; protected set; }

    public EBatteryMode batterySirens { get; protected set; }

    public float fuelBurnRate { get; protected set; }

    public bool isReclined { get; protected set; }

    public bool hasCrawler => _hasCrawler;

    public bool hasLockMouse => _hasLockMouse;

    public bool hasTraction => _hasTraction;

    public bool hasSleds => _hasSleds;

    public float exit => _exit;

    public float sqrDelta => _sqrDelta;

    public float validSpeedUp { get; protected set; }

    public float validSpeedDown { get; protected set; }

    public float camFollowDistance => _camFollowDistance;

    public float camDriverOffset { get; protected set; }

    public float camPassengerOffset { get; protected set; }

    public float bumperMultiplier => _bumperMultiplier;

    public float passengerExplosionArmor => _passengerExplosionArmor;

    public TurretInfo[] turrets => _turrets;

    public Texture albedoBase => _albedoBase;

    public Texture metallicBase => _metallicBase;

    public Texture emissionBase => _emissionBase;

    public bool CanDecay { get; private set; }

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

    public bool supportsMobileBuildables { get; protected set; }

    public bool shouldSpawnSeatCapsules { get; protected set; }

    public bool canBeLocked { get; protected set; }

    public bool canStealBattery { get; protected set; }

    public byte trunkStorage_X { get; set; }

    public byte trunkStorage_Y { get; set; }

    public ushort dropsTableId { get; protected set; }

    public byte dropsMin { get; protected set; }

    public byte dropsMax { get; protected set; }

    public ushort tireID { get; protected set; }

    public int numSteeringTires { get; protected set; }

    public int[] steeringTireIndices { get; protected set; }

    public bool hasCenterOfMassOverride { get; protected set; }

    public Vector3 centerOfMass { get; protected set; }

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

    public VehicleAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 200 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 200");
        }
        _vehicleName = localization.format("Name");
        _pitchIdle = data.readSingle("Pitch_Idle", -1f);
        _pitchDrive = data.readSingle("Pitch_Drive", -1f);
        _engine = data.readEnum("Engine", EEngine.CAR);
        physicsProfileRef = data.readAssetReference<VehiclePhysicsProfileAsset>("Physics_Profile");
        if (Dedicator.IsDedicatedServer && data.readBoolean("Has_Clip_Prefab", defaultValue: true))
        {
            bundle.loadDeferred("Clip", out legacyServerModel, (LoadedAssetDeferredCallback<GameObject>)OnServerModelLoaded);
        }
        bundle.loadDeferred("Vehicle", out clientModel, (LoadedAssetDeferredCallback<GameObject>)OnClientModelLoaded);
        _size2_z = data.readSingle("Size2_Z");
        _sharedSkinName = data.readString("Shared_Skin_Name");
        if (data.has("Shared_Skin_Lookup_ID"))
        {
            _sharedSkinLookupID = data.readUInt16("Shared_Skin_Lookup_ID", 0);
        }
        else
        {
            _sharedSkinLookupID = id;
        }
        if (data.has("Rarity"))
        {
            _rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), data.readString("Rarity"), ignoreCase: true);
        }
        else
        {
            _rarity = EItemRarity.COMMON;
        }
        _hasZip = data.has("Zip");
        _hasBicycle = data.has("Bicycle");
        isReclined = data.has("Reclined");
        _hasCrawler = data.has("Crawler");
        _hasLockMouse = data.has("LockMouse");
        _hasTraction = data.has("Traction");
        _hasSleds = data.has("Sleds");
        canSpawnWithBattery = !data.has("Cannot_Spawn_With_Battery");
        if (data.has("Battery_Spawn_Charge_Multiplier"))
        {
            batterySpawnChargeMultiplier = data.readSingle("Battery_Spawn_Charge_Multiplier");
        }
        else
        {
            batterySpawnChargeMultiplier = 1f;
        }
        if (data.has("Battery_Burn_Rate"))
        {
            batteryBurnRate = data.readSingle("Battery_Burn_Rate");
        }
        else
        {
            batteryBurnRate = 20f;
        }
        if (data.has("Battery_Charge_Rate"))
        {
            batteryChargeRate = data.readSingle("Battery_Charge_Rate");
        }
        else
        {
            batteryChargeRate = 20f;
        }
        batteryDriving = data.readEnum("BatteryMode_Driving", EBatteryMode.Charge);
        batteryEmpty = data.readEnum("BatteryMode_Empty", EBatteryMode.None);
        batteryHeadlights = data.readEnum("BatteryMode_Headlights", EBatteryMode.Burn);
        batterySirens = data.readEnum("BatteryMode_Sirens", EBatteryMode.Burn);
        float defaultValue = ((engine == EEngine.CAR) ? 2.05f : 4.2f);
        fuelBurnRate = data.readSingle("Fuel_Burn_Rate", defaultValue);
        _ignition = LoadRedirectableAsset<AudioClip>(bundle, "Ignition", data, "IgnitionAudioClip");
        _horn = LoadRedirectableAsset<AudioClip>(bundle, "Horn", data, "HornAudioClip");
        hasHorn = data.readBoolean("Has_Horn", _horn != null);
        _speedMin = data.readSingle("Speed_Min");
        _speedMax = data.readSingle("Speed_Max");
        if (engine != EEngine.TRAIN)
        {
            _speedMax *= 1.25f;
        }
        _steerMin = data.readSingle("Steer_Min");
        _steerMax = data.readSingle("Steer_Max") * 0.75f;
        _brake = data.readSingle("Brake");
        _lift = data.readSingle("Lift");
        _fuelMin = data.readUInt16("Fuel_Min", 0);
        _fuelMax = data.readUInt16("Fuel_Max", 0);
        _fuel = data.readUInt16("Fuel", 0);
        _healthMin = data.readUInt16("Health_Min", 0);
        _healthMax = data.readUInt16("Health_Max", 0);
        _health = data.readUInt16("Health", 0);
        _explosion = data.ReadGuidOrLegacyId("Explosion", out _explosionEffectGuid);
        bool defaultValue2 = !IsExplosionEffectRefNull();
        ShouldExplosionCauseDamage = data.readBoolean("ShouldExplosionCauseDamage", defaultValue2);
        ShouldExplosionBurnMaterials = data.readBoolean("ShouldExplosionBurnMaterials", defaultValue2);
        if (data.has("Explosion_Min_Force_Y"))
        {
            minExplosionForce = data.readVector3("Explosion_Min_Force");
        }
        else
        {
            minExplosionForce = new Vector3(0f, 1024f, 0f);
        }
        if (data.has("Explosion_Max_Force_Y"))
        {
            maxExplosionForce = data.readVector3("Explosion_Max_Force");
        }
        else
        {
            maxExplosionForce = new Vector3(0f, 1024f, 0f);
        }
        if (data.has("Exit"))
        {
            _exit = data.readSingle("Exit");
        }
        else
        {
            _exit = 2f;
        }
        if (data.has("Cam_Follow_Distance"))
        {
            _camFollowDistance = data.readSingle("Cam_Follow_Distance");
        }
        else
        {
            _camFollowDistance = 5.5f;
        }
        camDriverOffset = data.readSingle("Cam_Driver_Offset");
        camPassengerOffset = data.readSingle("Cam_Passenger_Offset");
        if (data.has("Bumper_Multiplier"))
        {
            _bumperMultiplier = data.readSingle("Bumper_Multiplier");
        }
        else
        {
            _bumperMultiplier = 1f;
        }
        if (data.has("Passenger_Explosion_Armor"))
        {
            _passengerExplosionArmor = data.readSingle("Passenger_Explosion_Armor");
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
        if (data.has("Valid_Speed_Horizontal"))
        {
            float x = data.readSingle("Valid_Speed_Horizontal") * PlayerInput.RATE;
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
        validSpeedUp = data.readSingle("Valid_Speed_Up", defaultValue3);
        validSpeedDown = data.readSingle("Valid_Speed_Down", defaultValue4);
        _turrets = new TurretInfo[data.readByte("Turrets", 0)];
        for (byte b = 0; b < turrets.Length; b = (byte)(b + 1))
        {
            TurretInfo turretInfo = new TurretInfo
            {
                seatIndex = data.readByte("Turret_" + b + "_Seat_Index", 0),
                itemID = data.readUInt16("Turret_" + b + "_Item_ID", 0),
                yawMin = data.readSingle("Turret_" + b + "_Yaw_Min"),
                yawMax = data.readSingle("Turret_" + b + "_Yaw_Max"),
                pitchMin = data.readSingle("Turret_" + b + "_Pitch_Min"),
                pitchMax = data.readSingle("Turret_" + b + "_Pitch_Max"),
                useAimCamera = !data.has("Turret_" + b + "_Ignore_Aim_Camera"),
                aimOffset = data.readSingle("Turret_" + b + "_Aim_Offset")
            };
            _turrets[b] = turretInfo;
        }
        isVulnerable = !data.has("Invulnerable");
        isVulnerableToExplosions = !data.has("Explosions_Invulnerable");
        isVulnerableToEnvironment = !data.has("Environment_Invulnerable");
        isVulnerableToBumper = !data.has("Bumper_Invulnerable");
        canTiresBeDamaged = !data.has("Tires_Invulnerable");
        canRepairWhileSeated = data.readBoolean("Can_Repair_While_Seated");
        childExplosionArmorMultiplier = data.readSingle("Child_Explosion_Armor_Multiplier", 0.2f);
        if (data.has("Air_Turn_Responsiveness"))
        {
            airTurnResponsiveness = data.readSingle("Air_Turn_Responsiveness");
        }
        else
        {
            airTurnResponsiveness = 2f;
        }
        if (data.has("Air_Steer_Min"))
        {
            airSteerMin = data.readSingle("Air_Steer_Min");
        }
        else
        {
            airSteerMin = steerMin;
        }
        if (data.has("Air_Steer_Max"))
        {
            airSteerMax = data.readSingle("Air_Steer_Max");
        }
        else
        {
            airSteerMax = steerMax;
        }
        bicycleAnimSpeed = data.readSingle("Bicycle_Anim_Speed");
        staminaBoost = data.readSingle("Stamina_Boost");
        useStaminaBoost = data.has("Stamina_Boost");
        isStaminaPowered = data.has("Stamina_Powered");
        isBatteryPowered = data.has("Battery_Powered");
        supportsMobileBuildables = data.has("Supports_Mobile_Buildables");
        shouldSpawnSeatCapsules = data.readBoolean("Should_Spawn_Seat_Capsules");
        canBeLocked = data.readBoolean("Can_Be_Locked", defaultValue: true);
        canStealBattery = data.readBoolean("Can_Steal_Battery", defaultValue: true);
        trunkStorage_X = data.readByte("Trunk_Storage_X", 0);
        trunkStorage_Y = data.readByte("Trunk_Storage_Y", 0);
        dropsTableId = data.readUInt16("Drops_Table_ID", 962);
        dropsMin = data.readByte("Drops_Min", 3);
        dropsMax = data.readByte("Drops_Max", 7);
        tireID = data.readUInt16("Tire_ID", 1451);
        int defaultValue5 = ((engine != 0) ? 1 : 2);
        if (hasCrawler)
        {
            defaultValue5 = 0;
        }
        numSteeringTires = data.readInt32("Num_Steering_Tires", defaultValue5);
        steeringTireIndices = new int[numSteeringTires];
        for (int i = 0; i < numSteeringTires; i++)
        {
            steeringTireIndices[i] = data.readInt32("Steering_Tire_" + i, i);
        }
        hasCenterOfMassOverride = data.readBoolean("Override_Center_Of_Mass");
        if (hasCenterOfMassOverride)
        {
            centerOfMass = data.readVector3("Center_Of_Mass");
        }
        if (data.has("Wheel_Collider_Mass_Override"))
        {
            wheelColliderMassOverride = data.readSingle("Wheel_Collider_Mass_Override", 1f);
        }
        else
        {
            wheelColliderMassOverride = null;
        }
        trainTrackOffset = data.readSingle("Train_Track_Offset");
        trainWheelOffset = data.readSingle("Train_Wheel_Offset");
        trainCarLength = data.readSingle("Train_Car_Length");
        _shouldVerifyHash = !data.has("Bypass_Hash_Verification");
        if (!Dedicator.IsDedicatedServer && id < 2000)
        {
            _albedoBase = bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = bundle.load<Texture2D>("Emission_Base");
        }
        CanDecay = engine != EEngine.TRAIN && (isVulnerable | isVulnerableToExplosions | isVulnerableToEnvironment | isVulnerableToBumper);
    }
}
