using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using Steamworks;
using UnityEngine;
using Unturned.UnityEx;

namespace SDG.Unturned;

public class InteractableVehicle : Interactable
{
    private static Collider[] grab = new Collider[4];

    private static List<Material> materials = new List<Material>();

    private static readonly float EXPLODE = 4f;

    private static readonly ushort HEALTH_0 = 100;

    private static readonly ushort HEALTH_1 = 200;

    public uint instanceID;

    public ushort id;

    public Items trunkItems;

    public ushort skinID;

    public ushort mythicID;

    protected SkinAsset skinAsset;

    private List<Mesh> tempMesh;

    protected Material tempMaterial;

    protected Transform effectSlotsRoot;

    protected Transform[] effectSlots;

    protected Transform[] effectSystems;

    public ushort roadIndex;

    public float roadPosition;

    public ushort fuel;

    public ushort health;

    public ushort batteryCharge;

    internal Guid batteryItemGuid;

    private float horned;

    protected VehicleEventHook eventHook;

    private bool _isDrowned;

    private float _lastDead;

    private float _lastUnderwater;

    private float _lastExploded;

    private float _slip;

    public bool isExploded;

    private float _factor;

    private float _speed;

    private float _physicsSpeed;

    private float _spedometer;

    private int _turn;

    private float spin;

    private float _steer;

    private float fly;

    private Rotor[] rotors;

    private ParticleSystem[] exhausts;

    private Transform wheel;

    private Transform overlapFront;

    private Transform overlapBack;

    private Transform pedalLeft;

    private Transform pedalRight;

    private Transform front;

    private Quaternion restWheel;

    private Quaternion restFront;

    private Transform waterCenterTransform;

    private Transform fire;

    private Transform smoke_0;

    private Transform smoke_1;

    [Obsolete]
    public bool isUpdated;

    public List<VehicleStateUpdate> updates;

    private Material[] sirenMaterials;

    private bool sirenState;

    private List<GameObject> sirenGameObjects = new List<GameObject>();

    private List<GameObject> sirenGameObjects_0 = new List<GameObject>();

    private List<GameObject> sirenGameObjects_1 = new List<GameObject>();

    private bool _sirensOn;

    private Transform _headlights;

    private Material headlightsMaterial;

    private bool _headlightsOn;

    private Transform _taillights;

    private Material taillightsMaterial;

    private Material[] taillightMaterials;

    private bool _taillightsOn;

    private CSteamID _lockedOwner;

    private CSteamID _lockedGroup;

    private bool _isLocked;

    private VehicleAsset _asset;

    public float lastSeat;

    private Passenger[] _passengers;

    private Passenger[] _turrets;

    public bool isHooked;

    private Transform buoyancy;

    private Transform hook;

    private List<HookInfo> hooked;

    private Vector3 lastUpdatedPos;

    private NetworkSnapshotBuffer nsb;

    private Vector3 real;

    private float lastTick;

    private float lastWeeoo;

    private AudioSource clipAudioSource;

    private AudioSource engineAudioSource;

    private AudioSource engineAdditiveAudioSource;

    private WindZone windZone;

    private bool isRecovering;

    private float lastRecover;

    private bool isPhysical;

    private bool isFrozen;

    public bool isBlimpFloating;

    private float altSpeedInput;

    private float altSpeedOutput;

    private float speedTraction;

    private float batteryBuffer;

    private float fuelBurnBuffer;

    private bool hasDroppedScrapItemsAlready;

    public bool hasDefaultCenterOfMass;

    public Vector3 defaultCenterOfMass;

    private List<Collider> _vehicleColliders;

    private Transform center;

    private Material skinMaterialToDestroy;

    internal float decayLastUpdateTime;

    internal float decayTimer;

    internal float decayPendingDamage;

    internal Vector3 decayLastUpdatePosition;

    public Road road { get; protected set; }

    public bool isInsideSafezone { get; protected set; }

    public SafezoneNode insideSafezoneNode { get; protected set; }

    public float timeInsideSafezone { get; protected set; }

    public bool isInsideNoDamageZone
    {
        get
        {
            if (insideSafezoneNode != null)
            {
                return insideSafezoneNode.noWeapons;
            }
            return false;
        }
    }

    public bool usesFuel
    {
        get
        {
            if (!asset.isStaminaPowered)
            {
                return !asset.isBatteryPowered;
            }
            return false;
        }
    }

    public bool usesBattery
    {
        get
        {
            if (asset.isStaminaPowered)
            {
                return asset.isBatteryPowered;
            }
            return true;
        }
    }

    public bool usesHealth => asset.engine != EEngine.TRAIN;

    public bool isBoosting { get; protected set; }

    public bool isEngineOn { get; protected set; }

    public bool isEnginePowered
    {
        get
        {
            if (asset.isStaminaPowered)
            {
                return true;
            }
            if (asset.isBatteryPowered)
            {
                return hasBattery;
            }
            if (fuel > 0)
            {
                return isEngineOn;
            }
            return false;
        }
    }

    public bool hasBattery
    {
        get
        {
            if (usesBattery)
            {
                return batteryCharge > 0;
            }
            return true;
        }
    }

    public bool isBatteryFull
    {
        get
        {
            if (usesBattery)
            {
                return batteryCharge >= 10000;
            }
            return true;
        }
    }

    public bool canUseHorn
    {
        get
        {
            if (Time.realtimeSinceStartup - horned > 0.5f)
            {
                if (usesBattery)
                {
                    return hasBattery;
                }
                return true;
            }
            return false;
        }
    }

    public bool canUseTurret => !isDead;

    public bool canTurnOnLights
    {
        get
        {
            if (!usesBattery || hasBattery)
            {
                return !isUnderwater;
            }
            return false;
        }
    }

    public bool isRefillable
    {
        get
        {
            if (usesFuel && fuel < asset.fuel && !isDriven)
            {
                return !isExploded;
            }
            return false;
        }
    }

    public bool isSiphonable
    {
        get
        {
            if (usesFuel && fuel > 0 && !isDriven)
            {
                return !isExploded;
            }
            return false;
        }
    }

    public bool isRepaired => health == asset.health;

    public bool isDriven
    {
        get
        {
            if (passengers != null)
            {
                return passengers[0].player != null;
            }
            return false;
        }
    }

    public bool anySeatsOccupied
    {
        get
        {
            if (passengers != null)
            {
                Passenger[] array = passengers;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].player != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public bool isDriver => false;

    public bool isEmpty
    {
        get
        {
            for (byte b = 0; b < passengers.Length; b = (byte)(b + 1))
            {
                if (passengers[b].player != null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public bool isDrowned => _isDrowned;

    public bool isUnderwater
    {
        get
        {
            if (waterCenterTransform != null)
            {
                return WaterUtility.isPointUnderwater(waterCenterTransform.position);
            }
            return WaterUtility.isPointUnderwater(base.transform.position + new Vector3(0f, 1f, 0f));
        }
    }

    public bool isBatteryReplaceable
    {
        get
        {
            if (usesBattery && !isBatteryFull && !isDriven)
            {
                return !isExploded;
            }
            return false;
        }
    }

    public bool isTireReplaceable
    {
        get
        {
            if (!isDriven && !isExploded)
            {
                return asset.canTiresBeDamaged;
            }
            return false;
        }
    }

    public bool canBeDamaged => asset.engine != EEngine.TRAIN;

    public bool isGoingToRespawn
    {
        get
        {
            if (!isExploded)
            {
                return isDrowned;
            }
            return true;
        }
    }

    public bool isAutoClearable
    {
        get
        {
            if (isExploded)
            {
                return true;
            }
            if (isUnderwater && buoyancy == null)
            {
                return true;
            }
            if (asset != null)
            {
                if (asset.engine == EEngine.BOAT && fuel == 0)
                {
                    return true;
                }
                _ = asset.engine;
                return false;
            }
            return false;
        }
    }

    public float lastDead => _lastDead;

    public float lastUnderwater => _lastUnderwater;

    public float lastExploded => _lastExploded;

    public float slip => _slip;

    public bool isDead => health == 0;

    public float factor => _factor;

    public float speed => _speed;

    public float physicsSpeed => _physicsSpeed;

    public float spedometer => _spedometer;

    public int turn => _turn;

    public float steer => _steer;

    public TrainCar[] trainCars { get; protected set; }

    public bool sirensOn => _sirensOn;

    public Transform headlights => _headlights;

    public bool headlightsOn => _headlightsOn;

    public Transform taillights => _taillights;

    public bool taillightsOn => _taillightsOn;

    public CSteamID lockedOwner => _lockedOwner;

    public CSteamID lockedGroup => _lockedGroup;

    public bool isLocked => _isLocked;

    public bool isSkinned => skinID != 0;

    public VehicleAsset asset => _asset;

    public Passenger[] passengers => _passengers;

    public Passenger[] turrets => _turrets;

    public Wheel[] tires { get; protected set; }

    private bool usesGravity => asset.engine != EEngine.TRAIN;

    private bool isKinematic => !usesGravity;

    public byte tireAliveMask
    {
        get
        {
            int num = 0;
            for (byte b = 0; b < Mathf.Min(8, tires.Length); b = (byte)(b + 1))
            {
                if (tires[b].isAlive)
                {
                    int num2 = 1 << (int)b;
                    num |= num2;
                }
            }
            return (byte)num;
        }
        set
        {
            for (byte b = 0; b < Mathf.Min(8, tires.Length); b = (byte)(b + 1))
            {
                if (!(tires[b].wheel == null))
                {
                    int num = 1 << (int)b;
                    tires[b].isAlive = (value & num) == num;
                }
            }
        }
    }

    public bool isExitable
    {
        get
        {
            Vector3 point;
            byte angle;
            return tryGetExit(0, out point, out angle);
        }
    }

    public IEnumerable<Collider> vehicleColliders => _vehicleColliders;

    public event VehiclePassengersUpdated onPassengersUpdated;

    public event VehicleLockUpdated onLockUpdated;

    public event VehicleHeadlightsUpdated onHeadlightsUpdated;

    public event VehicleTaillightsUpdated onTaillightsUpdated;

    public event VehicleSirensUpdated onSirensUpdated;

    public event VehicleBlimpUpdated onBlimpUpdated;

    public event VehicleBatteryChangedHandler batteryChanged;

    public event VehicleSkinChangedHandler skinChanged;

    public static event Action<InteractableVehicle> OnHealthChanged_Global;

    public static event Action<InteractableVehicle> OnLockChanged_Global;

    public static event Action<InteractableVehicle> OnFuelChanged_Global;

    public static event Action<InteractableVehicle> OnBatteryLevelChanged_Global;

    public static event Action<InteractableVehicle, int> OnPassengerAdded_Global;

    public static event Action<InteractableVehicle, int, int> OnPassengerChangedSeats_Global;

    public static event Action<InteractableVehicle, int, Player> OnPassengerRemoved_Global;

    internal static Vector3 PackRoadPosition(float roadPosition)
    {
        if (roadPosition >= 16384f)
        {
            return new Vector3(4096f, 4096f, roadPosition - 20480f);
        }
        if (roadPosition >= 8192f)
        {
            return new Vector3(4096f, roadPosition - 12288f, -4096f);
        }
        return new Vector3(roadPosition - 4096f, -4096f, -4096f);
    }

    internal static float UnpackRoadPosition(Vector3 roadPosition)
    {
        return roadPosition.x + roadPosition.y + roadPosition.z + 12288f;
    }

    public void ResetDecayTimer()
    {
        decayTimer = 0f;
        decayPendingDamage = 0f;
        decayLastUpdatePosition = base.transform.position;
    }

    public bool canPlayerRepair(Player player)
    {
        if (!asset.canRepairWhileSeated)
        {
            return player.movement.getVehicle() != this;
        }
        return true;
    }

    public void replaceBattery(Player player, byte quality)
    {
        replaceBattery(player, quality, new Guid("098b13be34a7411db7736b7f866ada69"));
    }

    public void replaceBattery(Player player, byte quality, Guid newBatteryItemGuid)
    {
        giveBatteryItem(player);
        batteryItemGuid = newBatteryItemGuid;
        VehicleManager.sendVehicleBatteryCharge(this, (ushort)(quality * 100));
        ResetDecayTimer();
    }

    public void stealBattery(Player player)
    {
        if (giveBatteryItem(player))
        {
            VehicleManager.sendVehicleBatteryCharge(this, 0);
        }
    }

    protected bool giveBatteryItem(Player player)
    {
        byte b = (byte)Mathf.FloorToInt((float)(int)batteryCharge / 100f);
        if (b == 0)
        {
            return false;
        }
        if (batteryItemGuid == Guid.Empty)
        {
            batteryItemGuid = new Guid("098b13be34a7411db7736b7f866ada69");
        }
        if (Assets.find(batteryItemGuid) is ItemAsset itemAsset)
        {
            Item item = new Item(itemAsset.id, 1, b);
            player.inventory.forceAddItem(item, auto: false);
        }
        return true;
    }

    public void sendTireAliveMaskUpdate()
    {
        VehicleManager.sendVehicleTireAliveMask(this, tireAliveMask);
    }

    public bool isTireCompatible(ushort itemID)
    {
        if (asset != null)
        {
            return asset.tireID == itemID;
        }
        return false;
    }

    public void askRepairTire(int index)
    {
        if (index >= 0)
        {
            tires[index].askRepair();
        }
    }

    public void askDamageTire(int index)
    {
        if (!isInsideNoDamageZone && index >= 0 && (asset == null || asset.canTiresBeDamaged))
        {
            tires[index].askDamage();
        }
    }

    public int getHitTireIndex(Vector3 position)
    {
        for (int i = 0; i < tires.Length; i++)
        {
            WheelCollider wheelCollider = tires[i].wheel;
            if (!(wheelCollider == null) && (wheelCollider.transform.position - position).sqrMagnitude < wheelCollider.radius * wheelCollider.radius)
            {
                return i;
            }
        }
        return -1;
    }

    public int getClosestAliveTireIndex(Vector3 position, bool isAlive)
    {
        int result = -1;
        float num = 16f;
        for (int i = 0; i < tires.Length; i++)
        {
            if (tires[i].isAlive == isAlive && !(tires[i].wheel == null))
            {
                float sqrMagnitude = (tires[i].wheel.transform.position - position).sqrMagnitude;
                if (sqrMagnitude < num)
                {
                    result = i;
                    num = sqrMagnitude;
                }
            }
        }
        return result;
    }

    public void getDisplayFuel(out ushort currentFuel, out ushort maxFuel)
    {
        if (usesFuel)
        {
            currentFuel = fuel;
            maxFuel = asset.fuel;
            return;
        }
        if (passengers[0].player != null && passengers[0].player.player != null)
        {
            currentFuel = passengers[0].player.player.life.stamina;
        }
        else if (Player.player != null)
        {
            currentFuel = Player.player.life.stamina;
        }
        else
        {
            currentFuel = 0;
        }
        maxFuel = 100;
    }

    public void askBurnFuel(ushort amount)
    {
        if (amount != 0 && !isExploded)
        {
            if (amount >= fuel)
            {
                fuel = 0;
            }
            else
            {
                fuel -= amount;
            }
        }
    }

    public void askFillFuel(ushort amount)
    {
        if (amount != 0 && !isExploded)
        {
            if (amount >= asset.fuel - fuel)
            {
                fuel = asset.fuel;
            }
            else
            {
                fuel += amount;
            }
            VehicleManager.sendVehicleFuel(this, fuel);
            ResetDecayTimer();
        }
    }

    protected void simulateBurnFuel()
    {
        if (usesFuel && isEngineOn)
        {
            float rATE = PlayerInput.RATE;
            fuelBurnBuffer += rATE * asset.fuelBurnRate;
            ushort num = (ushort)Mathf.FloorToInt(fuelBurnBuffer);
            if (num > 0)
            {
                fuelBurnBuffer -= (int)num;
                askBurnFuel(num);
            }
        }
    }

    public void askBurnBattery(ushort amount)
    {
        if (amount != 0 && !isExploded)
        {
            if (amount >= batteryCharge)
            {
                batteryCharge = 0;
            }
            else
            {
                batteryCharge -= amount;
            }
        }
    }

    public void askChargeBattery(ushort amount)
    {
        if (amount != 0 && !isExploded)
        {
            if (amount >= 10000 - batteryCharge)
            {
                batteryCharge = 10000;
            }
            else
            {
                batteryCharge += amount;
            }
        }
    }

    public void sendBatteryChargeUpdate()
    {
        VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
    }

    public void askDamage(ushort amount, bool canRepair)
    {
        if (isInsideNoDamageZone || amount == 0)
        {
            return;
        }
        if (isDead)
        {
            if (!canRepair)
            {
                explode();
            }
            return;
        }
        if (amount >= health)
        {
            health = 0;
        }
        else
        {
            health -= amount;
        }
        VehicleManager.sendVehicleHealth(this, health);
        if (isDead && !canRepair)
        {
            explode();
        }
    }

    public void askRepair(ushort amount)
    {
        if (amount != 0 && !isExploded)
        {
            if (amount >= asset.health - health)
            {
                health = asset.health;
            }
            else
            {
                health += amount;
            }
            VehicleManager.sendVehicleHealth(this, health);
        }
    }

    private void explode()
    {
        Vector3 force = new Vector3(UnityEngine.Random.Range(asset.minExplosionForce.x, asset.maxExplosionForce.x), UnityEngine.Random.Range(asset.minExplosionForce.y, asset.maxExplosionForce.y), UnityEngine.Random.Range(asset.minExplosionForce.z, asset.maxExplosionForce.z));
        GetComponent<Rigidbody>().AddForce(force);
        GetComponent<Rigidbody>().AddTorque(16f, 0f, 0f);
        dropTrunkItems();
        if (asset.ShouldExplosionCauseDamage)
        {
            DamageTool.explode(base.transform.position, 8f, EDeathCause.VEHICLE, CSteamID.Nil, 200f, 200f, 200f, 0f, 0f, 500f, 2000f, 500f, out var _, EExplosionDamageType.CONVENTIONAL, 32f, playImpactEffect: true, penetrateBuildables: false, EDamageOrigin.Vehicle_Explosion);
        }
        for (int i = 0; i < passengers.Length; i++)
        {
            Passenger passenger = passengers[i];
            if (passenger == null)
            {
                continue;
            }
            SteamPlayer player = passenger.player;
            if (player == null)
            {
                continue;
            }
            Player player2 = player.player;
            if (!(player2 == null) && !player2.life.isDead)
            {
                if (asset.ShouldExplosionCauseDamage)
                {
                    player2.life.askDamage(101, Vector3.up * 101f, EDeathCause.VEHICLE, ELimb.SPINE, CSteamID.Nil, out var _);
                }
                else
                {
                    VehicleManager.forceRemovePlayer(this, player.playerID.steamID);
                }
            }
        }
        DropScrapItems();
        VehicleManager.sendVehicleExploded(this);
        EffectAsset effectAsset = asset.FindExplosionEffectAsset();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.position = base.transform.position;
            parameters.relevantDistance = EffectManager.LARGE;
            EffectManager.triggerEffect(parameters);
        }
    }

    public bool checkEnter(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        if (isHooked)
        {
            return false;
        }
        _ = Provider.isServer;
        if (isLocked && !(enemyPlayer == lockedOwner))
        {
            if (lockedGroup != CSteamID.Nil)
            {
                return enemyGroup == lockedGroup;
            }
            return false;
        }
        return true;
    }

    public bool checkEnter(Player player)
    {
        if (player == null)
        {
            return false;
        }
        CSteamID steamID = player.channel.owner.playerID.steamID;
        CSteamID groupID = player.quests.groupID;
        return checkEnter(steamID, groupID);
    }

    public override bool checkUseable()
    {
        if (Player.player == null || (base.transform.position - Player.player.transform.position).sqrMagnitude > 100f)
        {
            return false;
        }
        if (!isExploded)
        {
            return checkEnter(Provider.client, Player.player.quests.groupID);
        }
        return false;
    }

    public override void use()
    {
        VehicleManager.enterVehicle(this);
    }

    public override bool checkHighlight(out Color color)
    {
        color = ItemTool.getRarityColorHighlight(asset.rarity);
        return true;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (checkUseable())
        {
            message = EPlayerMessage.VEHICLE_ENTER;
            text = asset.vehicleName;
            color = ItemTool.getRarityColorUI(asset.rarity);
        }
        else
        {
            if (Player.player == null || (base.transform.position - Player.player.transform.position).sqrMagnitude > 100f)
            {
                message = EPlayerMessage.BLOCKED;
            }
            else
            {
                message = EPlayerMessage.LOCKED;
            }
            text = "";
            color = Color.white;
        }
        return !isExploded;
    }

    public void updateVehicle()
    {
        lastUpdatedPos = base.transform.position;
        if (nsb != null)
        {
            nsb.updateLastSnapshot(new TransformSnapshotInfo(base.transform.position, base.transform.rotation));
        }
        real = base.transform.position;
        isRecovering = false;
        lastRecover = Time.realtimeSinceStartup;
        isFrozen = false;
    }

    private Vector3? calculateAverageLocalTireContactPosition()
    {
        if (tires == null)
        {
            return null;
        }
        Vector3 zero = Vector3.zero;
        int num = 0;
        Wheel[] array = tires;
        for (int i = 0; i < array.Length; i++)
        {
            WheelCollider wheelCollider = array[i].wheel;
            if (!(wheelCollider == null))
            {
                Vector3 position = wheelCollider.transform.TransformPoint(wheelCollider.center - new Vector3(0f, wheelCollider.radius, 0f));
                Vector3 vector = base.transform.InverseTransformPoint(position);
                zero += vector;
                num++;
            }
        }
        if (num > 0)
        {
            return zero / num;
        }
        return null;
    }

    public void updatePhysics()
    {
        if (checkDriver(Provider.client) || (Provider.isServer && !isDriven))
        {
            GetComponent<Rigidbody>().useGravity = usesGravity;
            GetComponent<Rigidbody>().isKinematic = isKinematic;
            isPhysical = true;
            if (!isExploded)
            {
                if (tires != null)
                {
                    for (int i = 0; i < tires.Length; i++)
                    {
                        tires[i].isPhysical = true;
                    }
                }
                if (buoyancy != null)
                {
                    buoyancy.gameObject.SetActive(value: true);
                }
            }
        }
        else
        {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;
            isPhysical = false;
            if (tires != null)
            {
                for (int j = 0; j < tires.Length; j++)
                {
                    tires[j].isPhysical = false;
                }
            }
            if (buoyancy != null)
            {
                buoyancy.gameObject.SetActive(value: false);
            }
        }
        if (!hasDefaultCenterOfMass)
        {
            hasDefaultCenterOfMass = true;
            defaultCenterOfMass = GetComponent<Rigidbody>().centerOfMass;
        }
        Vector3 centerOfMass;
        if (asset.hasCenterOfMassOverride)
        {
            centerOfMass = asset.centerOfMass;
        }
        else
        {
            Transform transform = base.transform.Find("Cog");
            if ((bool)transform)
            {
                centerOfMass = transform.localPosition;
            }
            else
            {
                centerOfMass = new Vector3(0f, -0.25f, 0f);
                if (asset.engine == EEngine.CAR)
                {
                    Vector3? vector = calculateAverageLocalTireContactPosition();
                    if (vector.HasValue)
                    {
                        centerOfMass = vector.Value;
                    }
                }
            }
        }
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }

    public void updateEngine()
    {
        synchronizeTaillights();
    }

    public void tellLocked(CSteamID owner, CSteamID group, bool locked)
    {
        _lockedOwner = owner;
        _lockedGroup = group;
        _isLocked = locked;
        if (this.onLockUpdated != null)
        {
            this.onLockUpdated();
        }
        if (eventHook != null)
        {
            if (locked)
            {
                eventHook.OnLocked.TryInvoke(this);
            }
            else
            {
                eventHook.OnUnlocked.TryInvoke(this);
            }
        }
        InteractableVehicle.OnLockChanged_Global.TryInvoke("OnLockChanged_Global", this);
    }

    public void tellSkin(ushort newSkinID, ushort newMythicID)
    {
        skinID = newSkinID;
        mythicID = newMythicID;
        updateSkin();
        if (this.skinChanged != null)
        {
            this.skinChanged();
        }
    }

    public void updateSkin()
    {
    }

    public void tellSirens(bool on)
    {
        _sirensOn = on;
        if (this.onSirensUpdated != null)
        {
            this.onSirensUpdated();
        }
    }

    public void tellBlimp(bool on)
    {
        isBlimpFloating = on;
        if (asset.engine == EEngine.BLIMP)
        {
            int childCount = buoyancy.childCount;
            for (int i = 0; i < childCount; i++)
            {
                buoyancy.GetChild(i).GetComponent<Buoyancy>().enabled = isBlimpFloating;
            }
            if (this.onBlimpUpdated != null)
            {
                this.onBlimpUpdated();
            }
        }
    }

    public void tellHeadlights(bool on)
    {
        _headlightsOn = on;
        if (this.onHeadlightsUpdated != null)
        {
            this.onHeadlightsUpdated();
        }
    }

    public void tellTaillights(bool on)
    {
        _taillightsOn = on;
        if (this.onTaillightsUpdated != null)
        {
            this.onTaillightsUpdated();
        }
    }

    private void synchronizeTaillights()
    {
        bool flag = isDriven && canTurnOnLights;
        if (taillightsOn != flag)
        {
            tellTaillights(flag);
        }
    }

    public void tellHorn()
    {
        horned = Time.realtimeSinceStartup;
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 32f);
        }
        eventHook?.OnHornUsed.TryInvoke(this);
    }

    public void tellFuel(ushort newFuel)
    {
        fuel = newFuel;
        InteractableVehicle.OnFuelChanged_Global.TryInvoke("OnFuelChanged_Global", this);
    }

    public void tellBatteryCharge(ushort newBatteryCharge)
    {
        batteryCharge = newBatteryCharge;
        if (batteryCharge == 0)
        {
            isEngineOn = false;
        }
        if (this.batteryChanged != null)
        {
            this.batteryChanged();
        }
        InteractableVehicle.OnBatteryLevelChanged_Global.TryInvoke("OnBatteryLevelChanged_Global", this);
    }

    public void tellExploded()
    {
        clearHooked();
        isExploded = true;
        _lastExploded = Time.realtimeSinceStartup;
        if (sirensOn)
        {
            tellSirens(on: false);
        }
        if (isBlimpFloating)
        {
            tellBlimp(on: false);
        }
        if (headlightsOn)
        {
            tellHeadlights(on: false);
        }
        if (tires != null)
        {
            for (int i = 0; i < tires.Length; i++)
            {
                tires[i].isPhysical = false;
            }
        }
        if (buoyancy != null)
        {
            buoyancy.gameObject.SetActive(value: false);
        }
    }

    public void updateFires()
    {
    }

    public void tellHealth(ushort newHealth)
    {
        health = newHealth;
        if (isDead)
        {
            _lastDead = Time.realtimeSinceStartup;
        }
        updateFires();
        InteractableVehicle.OnHealthChanged_Global.TryInvoke("OnHealthChanged_Global", this);
    }

    public void tellRecov(Vector3 newPosition, int newRecov)
    {
        lastTick = Time.realtimeSinceStartup;
        GetComponent<Rigidbody>().MovePosition(newPosition);
        isFrozen = true;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        if (passengers[0] != null && passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.input != null)
        {
            passengers[0].player.player.input.recov = newRecov;
        }
    }

    [Obsolete]
    public void tellState(Vector3 newPosition, byte newAngle_X, byte newAngle_Y, byte newAngle_Z, byte newSpeed, byte newPhysicsSpeed, byte newTurn)
    {
        Quaternion newRotation = Quaternion.Euler(MeasurementTool.byteToAngle2(newAngle_X), MeasurementTool.byteToAngle2(newAngle_Y), MeasurementTool.byteToAngle2(newAngle_Z));
        tellState(newPosition, newRotation, newSpeed, newPhysicsSpeed, newTurn);
    }

    public void tellState(Vector3 newPosition, Quaternion newRotation, byte newSpeed, byte newPhysicsSpeed, byte newTurn)
    {
        if (!isDriver)
        {
            lastTick = Time.realtimeSinceStartup;
            lastUpdatedPos = newPosition;
            if (nsb != null)
            {
                nsb.addNewSnapshot(new TransformSnapshotInfo(newPosition, newRotation));
            }
            if (asset.engine == EEngine.TRAIN)
            {
                roadPosition = UnpackRoadPosition(newPosition);
            }
            _speed = newSpeed - 128;
            _physicsSpeed = newPhysicsSpeed - 128;
            _turn = newTurn - 1;
        }
    }

    public bool checkDriver(CSteamID steamID)
    {
        if (isDriven)
        {
            return passengers[0].player.playerID.steamID == steamID;
        }
        return false;
    }

    public void grantTrunkAccess(Player player)
    {
        if (Provider.isServer && trunkItems != null && trunkItems.height > 0)
        {
            player.inventory.openTrunk(trunkItems);
        }
    }

    public void revokeTrunkAccess(Player player)
    {
        if (Provider.isServer)
        {
            player.inventory.closeTrunk();
        }
    }

    public void dropTrunkItems()
    {
        if (Provider.isServer && trunkItems != null)
        {
            for (byte b = 0; b < trunkItems.getItemCount(); b = (byte)(b + 1))
            {
                ItemManager.dropItem(trunkItems.getItem(b).item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            trunkItems.clear();
            trunkItems = null;
            if (passengers[0].player != null && passengers[0].player.player != null)
            {
                revokeTrunkAccess(passengers[0].player.player);
            }
        }
    }

    private void DropScrapItems()
    {
        if (hasDroppedScrapItemsAlready || asset.dropsTableId <= 0)
        {
            return;
        }
        hasDroppedScrapItemsAlready = true;
        int value = UnityEngine.Random.Range(asset.dropsMin, asset.dropsMax);
        value = Mathf.Clamp(value, 0, 100);
        for (int i = 0; i < value; i++)
        {
            float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
            ushort num = SpawnTableTool.resolve(asset.dropsTableId);
            if (num != 0)
            {
                ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), base.transform.position + new Vector3(Mathf.Sin(f) * 3f, 1f, Mathf.Cos(f) * 3f), playEffect: false, isDropped: true, wideSpread: true);
            }
        }
    }

    public void addPlayer(byte seat, CSteamID steamID)
    {
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(steamID);
        if (steamPlayer != null)
        {
            passengers[seat].player = steamPlayer;
            if (steamPlayer.player != null)
            {
                steamPlayer.player.movement.setVehicle(this, seat, passengers[seat].seat, Vector3.zero, 0, forceUpdate: false);
                if (passengers[seat].turret != null)
                {
                    steamPlayer.player.equipment.turretEquipClient();
                    if (Provider.isServer)
                    {
                        steamPlayer.player.equipment.turretEquipServer(passengers[seat].turret.itemID, passengers[seat].state);
                    }
                }
            }
            if (passengers[seat].collider != null)
            {
                passengers[seat].collider.enabled = true;
            }
            updatePhysics();
            if (seat == 0)
            {
                grantTrunkAccess(steamPlayer.player);
            }
        }
        if (seat == 0)
        {
            isEngineOn = (!usesBattery || hasBattery) && !isUnderwater;
        }
        updateEngine();
        if (seat == 0)
        {
            _ = isEnginePowered;
        }
        if (this.onPassengersUpdated != null)
        {
            this.onPassengersUpdated();
        }
        bool flag = false;
        if (eventHook != null)
        {
            if (seat == 0)
            {
                eventHook.OnDriverAdded.TryInvoke(this);
                if (flag)
                {
                    eventHook.OnLocalDriverAdded.TryInvoke(this);
                }
            }
            if (flag)
            {
                eventHook.OnLocalPassengerAdded.TryInvoke(this);
            }
        }
        if (passengers[seat].turretEventHook != null)
        {
            passengers[seat].turretEventHook.OnPassengerAdded.TryInvoke(this);
            if (flag)
            {
                passengers[seat].turretEventHook.OnLocalPassengerAdded.TryInvoke(this);
            }
        }
        InteractableVehicle.OnPassengerAdded_Global.TryInvoke("OnPassengerAdded_Global", this, seat);
    }

    public void removePlayer(byte seatIndex, Vector3 point, byte angle, bool forceUpdate)
    {
        SteamPlayer steamPlayer = null;
        if (passengers != null && seatIndex < passengers.Length)
        {
            Passenger passenger = passengers[seatIndex];
            steamPlayer = passenger.player;
            if (steamPlayer != null && steamPlayer.player != null)
            {
                if (passenger.turret != null)
                {
                    steamPlayer.player.equipment.turretDequipClient();
                    if (Provider.isServer)
                    {
                        steamPlayer.player.equipment.turretDequipServer();
                    }
                }
                steamPlayer.player.movement.setVehicle(null, 0, null, point, angle, forceUpdate);
            }
            if (passengers[seatIndex].collider != null)
            {
                passengers[seatIndex].collider.enabled = false;
            }
            passenger.player = null;
            updatePhysics();
            if (Provider.isServer)
            {
                VehicleManager.sendVehicleFuel(this, fuel);
                VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
            }
            if (seatIndex == 0 && steamPlayer != null && steamPlayer.player != null)
            {
                revokeTrunkAccess(steamPlayer.player);
            }
        }
        if (seatIndex == 0)
        {
            isEngineOn = false;
        }
        updateEngine();
        if (seatIndex == 0)
        {
            altSpeedInput = 0f;
            altSpeedOutput = 0f;
            if (tires != null)
            {
                for (int i = 0; i < tires.Length; i++)
                {
                    tires[i].reset();
                }
            }
        }
        if (this.onPassengersUpdated != null)
        {
            this.onPassengersUpdated();
        }
        bool flag = false;
        if (passengers[seatIndex].turretEventHook != null)
        {
            if (flag)
            {
                passengers[seatIndex].turretEventHook.OnLocalPassengerRemoved.TryInvoke(this);
            }
            passengers[seatIndex].turretEventHook.OnPassengerRemoved.TryInvoke(this);
        }
        if (eventHook != null)
        {
            if (flag)
            {
                eventHook.OnLocalPassengerRemoved.TryInvoke(this);
            }
            if (seatIndex == 0)
            {
                if (flag)
                {
                    eventHook.OnLocalDriverRemoved.TryInvoke(this);
                }
                eventHook.OnDriverRemoved.TryInvoke(this);
            }
        }
        InteractableVehicle.OnPassengerRemoved_Global.TryInvoke("OnPassengerRemoved_Global", this, seatIndex, steamPlayer?.player);
    }

    public void swapPlayer(byte fromSeatIndex, byte toSeatIndex)
    {
        if (passengers != null && fromSeatIndex < passengers.Length && toSeatIndex < passengers.Length)
        {
            Passenger passenger = passengers[fromSeatIndex];
            Passenger passenger2 = passengers[toSeatIndex];
            SteamPlayer player = passenger.player;
            if (player != null && player.player != null)
            {
                if (passenger.turret != null)
                {
                    player.player.equipment.turretDequipClient();
                    if (Provider.isServer)
                    {
                        player.player.equipment.turretDequipServer();
                    }
                }
                player.player.movement.setVehicle(this, toSeatIndex, passengers[toSeatIndex].seat, Vector3.zero, 0, forceUpdate: false);
                if (passenger2.turret != null)
                {
                    player.player.equipment.turretEquipClient();
                    if (Provider.isServer)
                    {
                        player.player.equipment.turretEquipServer(passengers[toSeatIndex].turret.itemID, passengers[toSeatIndex].state);
                    }
                }
            }
            if (passenger.collider != null)
            {
                passenger.collider.enabled = false;
            }
            if (passenger2.collider != null)
            {
                passenger2.collider.enabled = true;
            }
            passenger.player = null;
            passenger2.player = player;
            updatePhysics();
            if (Provider.isServer)
            {
                VehicleManager.sendVehicleFuel(this, fuel);
                VehicleManager.sendVehicleBatteryCharge(this, batteryCharge);
            }
            if (fromSeatIndex == 0 && player != null && player.player != null)
            {
                revokeTrunkAccess(player.player);
            }
            if (toSeatIndex == 0 && player != null && player.player != null)
            {
                grantTrunkAccess(player.player);
            }
        }
        if (toSeatIndex == 0)
        {
            isEngineOn = (!usesBattery || hasBattery) && !isUnderwater;
        }
        if (fromSeatIndex == 0)
        {
            isEngineOn = false;
        }
        updateEngine();
        if (fromSeatIndex == 0)
        {
            altSpeedInput = 0f;
            altSpeedOutput = 0f;
            if (tires != null)
            {
                for (int i = 0; i < tires.Length; i++)
                {
                    tires[i].reset();
                }
            }
        }
        if (this.onPassengersUpdated != null)
        {
            this.onPassengersUpdated();
        }
        bool flag = false;
        if (passengers[fromSeatIndex].turretEventHook != null)
        {
            if (flag)
            {
                passengers[fromSeatIndex].turretEventHook.OnLocalPassengerRemoved.TryInvoke(this);
            }
            passengers[fromSeatIndex].turretEventHook.OnPassengerRemoved.TryInvoke(this);
        }
        if (passengers[toSeatIndex].turretEventHook != null)
        {
            passengers[toSeatIndex].turretEventHook.OnPassengerAdded.TryInvoke(this);
            if (flag)
            {
                passengers[toSeatIndex].turretEventHook.OnLocalPassengerAdded.TryInvoke(this);
            }
        }
        if (eventHook != null)
        {
            if (fromSeatIndex == 0)
            {
                if (flag)
                {
                    eventHook.OnLocalDriverRemoved.TryInvoke(this);
                }
                eventHook.OnDriverRemoved.TryInvoke(this);
            }
            if (toSeatIndex == 0)
            {
                eventHook.OnDriverAdded.TryInvoke(this);
                if (flag)
                {
                    eventHook.OnLocalDriverAdded.TryInvoke(this);
                }
            }
        }
        InteractableVehicle.OnPassengerChangedSeats_Global.TryInvoke("OnPassengerChangedSeats_Global", this, fromSeatIndex, toSeatIndex);
    }

    public bool tryAddPlayer(out byte seat, Player player)
    {
        seat = byte.MaxValue;
        if (player == null)
        {
            return false;
        }
        if (isExploded)
        {
            return false;
        }
        if (!isExitable)
        {
            return false;
        }
        for (byte b = 0; b < passengers.Length; b = (byte)(b + 1))
        {
            if (passengers[b] != null && passengers[b].player == player.channel.owner)
            {
                return false;
            }
        }
        bool flag = player.animator.gesture == EPlayerGesture.ARREST_START;
        for (byte b2 = (byte)(flag ? 1u : 0u); b2 < passengers.Length; b2 = (byte)(b2 + 1))
        {
            if (passengers[b2] != null && passengers[b2].player == null && (!flag || passengers[b2].turret == null))
            {
                seat = b2;
                return true;
            }
        }
        return false;
    }

    public void forceRemoveAllPlayers()
    {
        for (int i = 0; i < passengers.Length; i++)
        {
            Passenger passenger = passengers[i];
            if (passenger == null)
            {
                continue;
            }
            SteamPlayer player = passenger.player;
            if (player != null)
            {
                Player player2 = player.player;
                if (!(player2 == null) && !player2.life.isDead)
                {
                    VehicleManager.forceRemovePlayer(this, player.playerID.steamID);
                }
            }
        }
    }

    public bool forceRemovePlayer(out byte seat, CSteamID player, out Vector3 point, out byte angle)
    {
        seat = byte.MaxValue;
        point = Vector3.zero;
        angle = 0;
        if (findPlayerSeat(player, out seat))
        {
            forceGetExit(passengers[seat]?.player?.player, seat, out point, out angle);
            return true;
        }
        return false;
    }

    public bool findPlayerSeat(CSteamID player, out byte seat)
    {
        seat = byte.MaxValue;
        for (byte b = 0; b < passengers.Length; b = (byte)(b + 1))
        {
            if (passengers[b] != null && passengers[b].player != null && passengers[b].player.playerID.steamID == player)
            {
                seat = b;
                return true;
            }
        }
        return false;
    }

    public bool findPlayerSeat(Player player, out byte seat)
    {
        return findPlayerSeat(player.channel.owner.playerID.steamID, out seat);
    }

    public bool trySwapPlayer(Player player, byte toSeat, out byte fromSeat)
    {
        fromSeat = byte.MaxValue;
        if (toSeat >= passengers.Length)
        {
            return false;
        }
        if (player.animator.gesture == EPlayerGesture.ARREST_START)
        {
            if (toSeat < 1)
            {
                return false;
            }
            if (passengers[toSeat].turret != null)
            {
                return false;
            }
        }
        for (byte b = 0; b < passengers.Length; b = (byte)(b + 1))
        {
            if (passengers[b] != null && passengers[b].player != null && passengers[b].player.player == player)
            {
                if (toSeat != b)
                {
                    fromSeat = b;
                    if (passengers[toSeat].player == null)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }
        return false;
    }

    protected bool isExitPositionEmpty(Vector3 position)
    {
        return PlayerStance.hasTeleportClearanceAtPosition(position);
    }

    protected bool raycastIgnoringVehicleAndChildren(Vector3 origin, Vector3 direction, float maxDistance, out float hitDistance)
    {
        hitDistance = maxDistance;
        bool result = false;
        RaycastHit[] array = Physics.RaycastAll(new Ray(origin, direction), maxDistance, RayMasks.BLOCK_EXIT);
        if (array != null && array.Length != 0)
        {
            RaycastHit[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                RaycastHit raycastHit = array2[i];
                if (raycastHit.transform != null && !raycastHit.transform.IsChildOf(base.transform))
                {
                    hitDistance = Mathf.Min(hitDistance, raycastHit.distance);
                    result = true;
                }
            }
        }
        return result;
    }

    protected Vector3 getExitDistanceInDirection(Vector3 origin, Vector3 direction, float maxDistance, float extraPadding = 0.1f)
    {
        raycastIgnoringVehicleAndChildren(origin, direction, maxDistance, out var hitDistance);
        float num = PlayerStance.RADIUS + extraPadding;
        return origin + direction * (hitDistance - num);
    }

    protected void findGroundForExitPosition(ref Vector3 exitPosition)
    {
        Physics.Raycast(new Ray(exitPosition, Vector3.down), out var hitInfo, 3f, RayMasks.BLOCK_EXIT_FIND_GROUND);
        if (hitInfo.transform != null)
        {
            exitPosition = hitInfo.point + new Vector3(0f, 0.25f, 0f);
        }
    }

    protected bool getSafeExitInDirection(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 exitPosition)
    {
        exitPosition = getExitDistanceInDirection(origin, direction, maxDistance);
        findGroundForExitPosition(ref exitPosition);
        return isExitPositionEmpty(exitPosition);
    }

    protected bool getExitSidePoint(Vector3 direction, out Vector3 exitPosition)
    {
        float num = PlayerStance.RADIUS + 0.1f;
        float maxDistance = asset.exit + Mathf.Abs(speed) * 0.1f + num;
        Vector3 position = center.position;
        return getSafeExitInDirection(position, direction, maxDistance, out exitPosition);
    }

    protected bool getExitUpwardPoint(out Vector3 exitPosition)
    {
        Vector3 position = center.position;
        Vector3 up = center.up;
        exitPosition = getExitDistanceInDirection(position, up, 6f, PlayerMovement.HEIGHT_STAND);
        findGroundForExitPosition(ref exitPosition);
        if (isExitPositionEmpty(exitPosition))
        {
            return true;
        }
        exitPosition = getExitDistanceInDirection(position, Vector3.up, 6f, PlayerMovement.HEIGHT_STAND);
        findGroundForExitPosition(ref exitPosition);
        return isExitPositionEmpty(exitPosition);
    }

    protected bool getExitDownwardPoint(out Vector3 exitPosition)
    {
        Vector3 position = center.position;
        Vector3 direction = -center.up;
        if (getSafeExitInDirection(position, direction, 6f, out exitPosition))
        {
            return true;
        }
        return getSafeExitInDirection(position, Vector3.down, 6f, out exitPosition);
    }

    protected bool getExitForwardPoint(Vector3 direction, out Vector3 exitPosition)
    {
        float maxDistance = 3f + asset.exit * 2f;
        Vector3 position = center.position;
        return getSafeExitInDirection(position, direction, maxDistance, out exitPosition);
    }

    protected void getExitSpawnPoint(Player player, ref Vector3 point, ref byte angle)
    {
        PlayerSpawnpoint spawn = LevelPlayers.getSpawn(Level.info != null && Level.info.type == ELevelType.ARENA && LevelManager.isPlayerInArena(player));
        if (spawn != null)
        {
            point = spawn.point;
            angle = MeasurementTool.angleToByte((int)angle);
        }
        else
        {
            point = new Vector3(0f, 256f, 0f);
            angle = 0;
        }
    }

    internal bool tryGetExit(byte seat, out Vector3 point, out byte angle)
    {
        point = center.position;
        angle = MeasurementTool.angleToByte(center.rotation.eulerAngles.y);
        Vector3 vector = (((int)seat % 2 == 0) ? (-center.right) : center.right);
        if (getExitSidePoint(vector, out point))
        {
            return true;
        }
        vector = -vector;
        if (getExitSidePoint(vector, out point))
        {
            return true;
        }
        if (getExitUpwardPoint(out point))
        {
            return true;
        }
        if (getExitDownwardPoint(out point))
        {
            return true;
        }
        if (getExitForwardPoint(-center.forward, out point))
        {
            return true;
        }
        if (getExitForwardPoint(center.forward, out point))
        {
            return true;
        }
        return false;
    }

    protected void forceGetExit(Player player, byte seat, out Vector3 point, out byte angle)
    {
        if (!tryGetExit(seat, out point, out angle))
        {
            getExitSpawnPoint(player, ref point, ref angle);
        }
    }

    public void simulate(uint simulation, int recov, bool inputStamina, Vector3 point, Quaternion angle, float newSpeed, float newPhysicsSpeed, int newTurn, float delta)
    {
        if (asset.useStaminaBoost)
        {
            bool flag = passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.life.stamina > 0;
            if (inputStamina && flag)
            {
                isBoosting = true;
            }
            else
            {
                isBoosting = false;
            }
        }
        else
        {
            isBoosting = false;
        }
        if (isRecovering)
        {
            if (recov < passengers[0].player.player.input.recov)
            {
                if (Time.realtimeSinceStartup - lastRecover > 5f)
                {
                    lastRecover = Time.realtimeSinceStartup;
                    VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
                }
                return;
            }
            isRecovering = false;
            isFrozen = false;
        }
        if (Dedicator.serverVisibility != ESteamServerVisibility.LAN && !PlayerMovement.forceTrustClient)
        {
            if (asset.engine == EEngine.CAR)
            {
                if (MathfEx.HorizontalDistanceSquared(point, real) > ((usesFuel && fuel == 0) ? 0.5f : asset.sqrDelta))
                {
                    isRecovering = true;
                    lastRecover = Time.realtimeSinceStartup;
                    passengers[0].player.player.input.recov++;
                    VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
                    return;
                }
            }
            else if (asset.engine == EEngine.BOAT)
            {
                if (MathfEx.HorizontalDistanceSquared(point, real) > (WaterUtility.isPointUnderwater(point + new Vector3(0f, -4f, 0f)) ? asset.sqrDelta : 0.5f))
                {
                    isRecovering = true;
                    lastRecover = Time.realtimeSinceStartup;
                    passengers[0].player.player.input.recov++;
                    VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
                    return;
                }
            }
            else if (asset.engine != EEngine.TRAIN && MathfEx.HorizontalDistanceSquared(point, real) > asset.sqrDelta)
            {
                isRecovering = true;
                lastRecover = Time.realtimeSinceStartup;
                passengers[0].player.player.input.recov++;
                VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
                return;
            }
            if (asset.engine != EEngine.TRAIN)
            {
                float num = ((point.y > real.y) ? asset.validSpeedUp : asset.validSpeedDown);
                if (Mathf.Abs(point.y - real.y) / delta > num)
                {
                    isRecovering = true;
                    lastRecover = Time.realtimeSinceStartup;
                    passengers[0].player.player.input.recov++;
                    VehicleManager.sendVehicleRecov(this, real, passengers[0].player.player.input.recov);
                    return;
                }
            }
        }
        if (asset.engine != EEngine.TRAIN)
        {
            UndergroundWhitelist.adjustPosition(ref point, 10f, 2f);
        }
        simulateBurnFuel();
        _speed = newSpeed;
        _physicsSpeed = newSpeed;
        _turn = newTurn;
        real = point;
        Vector3 pos;
        if (asset.engine == EEngine.TRAIN)
        {
            roadPosition = clampRoadPosition(UnpackRoadPosition(point));
            teleportTrain();
            pos = PackRoadPosition(roadPosition);
        }
        else
        {
            GetComponent<Rigidbody>().MovePosition(point);
            GetComponent<Rigidbody>().MoveRotation(angle);
            pos = point;
        }
        if (updates != null && (Mathf.Abs(lastUpdatedPos.x - real.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.y - real.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.z - real.z) > Provider.UPDATE_DISTANCE))
        {
            lastUpdatedPos = real;
            updates.Add(new VehicleStateUpdate(pos, angle));
        }
    }

    public void clearHooked()
    {
        foreach (HookInfo item in hooked)
        {
            if (!(item.vehicle == null))
            {
                item.vehicle.isHooked = false;
                ignoreCollisionWithVehicle(item.vehicle, shouldIgnore: false);
            }
        }
        hooked.Clear();
    }

    public void useHook()
    {
        if (hooked.Count > 0)
        {
            clearHooked();
            return;
        }
        int num = Physics.OverlapSphereNonAlloc(hook.position, 3f, grab, 67108864);
        for (int i = 0; i < num; i++)
        {
            InteractableVehicle vehicle = DamageTool.getVehicle(grab[i].transform);
            if (!(vehicle == null) && !(vehicle == this) && vehicle.isEmpty && !vehicle.isHooked && !vehicle.isExploded && vehicle.asset.engine != EEngine.TRAIN)
            {
                HookInfo hookInfo = new HookInfo();
                hookInfo.target = vehicle.transform;
                hookInfo.vehicle = vehicle;
                hookInfo.deltaPosition = hook.InverseTransformPoint(vehicle.transform.position);
                hookInfo.deltaRotation = Quaternion.FromToRotation(hook.forward, vehicle.transform.forward);
                hooked.Add(hookInfo);
                vehicle.isHooked = true;
                ignoreCollisionWithVehicle(vehicle, shouldIgnore: true);
            }
        }
    }

    public void simulate(uint simulation, int recov, int input_x, int input_y, float look_x, float look_y, bool inputBrake, bool inputStamina, float delta)
    {
        if (Provider.isServer && asset.engine != EEngine.TRAIN)
        {
            Vector3 worldspacePosition = base.transform.position;
            if (UndergroundWhitelist.adjustPosition(ref worldspacePosition, 10f, 2f))
            {
                GetComponent<Rigidbody>().MovePosition(worldspacePosition);
            }
        }
        float num = input_y;
        float num2 = 1f;
        if (asset.useStaminaBoost)
        {
            bool flag = passengers[0].player != null && passengers[0].player.player != null && passengers[0].player.player.life.stamina > 0;
            if (inputStamina && flag)
            {
                isBoosting = true;
            }
            else
            {
                isBoosting = false;
                num *= asset.staminaBoost;
                num2 *= asset.staminaBoost;
            }
        }
        else
        {
            isBoosting = false;
        }
        if (isFrozen)
        {
            isFrozen = false;
            GetComponent<Rigidbody>().useGravity = usesGravity;
            GetComponent<Rigidbody>().isKinematic = isKinematic;
            return;
        }
        if ((usesFuel && fuel == 0) || isUnderwater || isDead || !isEngineOn)
        {
            num = 0f;
            num2 = 1f;
        }
        _factor = Mathf.InverseLerp(0f, (speed < 0f) ? asset.speedMin : asset.speedMax, speed);
        bool flag2 = false;
        if (tires != null)
        {
            for (int i = 0; i < tires.Length; i++)
            {
                tires[i].simulate(input_x, num, inputBrake, delta);
                if (tires[i].isGrounded)
                {
                    flag2 = true;
                }
            }
        }
        switch (asset.engine)
        {
        case EEngine.CAR:
        {
            if (flag2)
            {
                GetComponent<Rigidbody>().AddForce(-base.transform.up * factor * 40f);
            }
            if (!(buoyancy != null))
            {
                break;
            }
            float num3 = Mathf.Lerp(asset.steerMax, asset.steerMin, factor);
            bool flag3 = WaterUtility.isPointUnderwater(base.transform.position + new Vector3(0f, -1f, 0f));
            speedTraction = Mathf.Lerp(speedTraction, flag3 ? 1 : 0, 4f * Time.deltaTime);
            if (!MathfEx.IsNearlyZero(speedTraction))
            {
                if (num > 0f)
                {
                    altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax, delta / 4f);
                }
                else if (num < 0f)
                {
                    altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMin, delta / 4f);
                }
                else
                {
                    altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
                }
                altSpeedOutput = altSpeedInput * speedTraction;
                Vector3 forward = base.transform.forward;
                forward.y = 0f;
                GetComponent<Rigidbody>().AddForce(forward.normalized * altSpeedOutput * 2f * speedTraction);
                GetComponent<Rigidbody>().AddRelativeTorque((float)input_y * -2.5f * speedTraction, (float)input_x * num3 / 8f * speedTraction, (float)input_x * -2.5f * speedTraction);
            }
            break;
        }
        case EEngine.PLANE:
        {
            float num5 = Mathf.Lerp(asset.airSteerMax, asset.airSteerMin, factor);
            if (num > 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax * num2, delta);
            }
            else if (num < 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
            }
            else
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 16f);
            }
            altSpeedOutput = altSpeedInput;
            GetComponent<Rigidbody>().AddForce(base.transform.forward * altSpeedOutput * 2f);
            GetComponent<Rigidbody>().AddForce(Mathf.Lerp(0f, 1f, base.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z / asset.speedMax) * asset.lift * -Physics.gravity);
            if (tires == null || tires.Length == 0 || (!tires[0].isGrounded && !tires[1].isGrounded))
            {
                GetComponent<Rigidbody>().AddRelativeTorque(Mathf.Clamp(look_y, 0f - asset.airTurnResponsiveness, asset.airTurnResponsiveness) * num5, (float)input_x * asset.airTurnResponsiveness * num5 / 4f, Mathf.Clamp(look_x, 0f - asset.airTurnResponsiveness, asset.airTurnResponsiveness) * (0f - num5) / 2f);
            }
            if ((tires == null || tires.Length == 0) && num < 0f)
            {
                GetComponent<Rigidbody>().AddForce(base.transform.forward * asset.speedMin * 4f);
            }
            break;
        }
        case EEngine.HELICOPTER:
        {
            float num6 = Mathf.Lerp(asset.steerMax, asset.steerMin, factor);
            if (num > 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax * num2, delta / 4f);
            }
            else if (num < 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
            }
            else
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 16f);
            }
            altSpeedOutput = altSpeedInput;
            GetComponent<Rigidbody>().AddForce(base.transform.up * altSpeedOutput * 3f);
            GetComponent<Rigidbody>().AddRelativeTorque(Mathf.Clamp(look_y, -2f, 2f) * num6, (float)input_x * num6 / 2f, Mathf.Clamp(look_x, -2f, 2f) * (0f - num6) / 4f);
            break;
        }
        case EEngine.BLIMP:
        {
            float num4 = Mathf.Lerp(asset.steerMax, asset.steerMin, factor);
            if (num > 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax * num2, delta / 4f);
            }
            else if (num < 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMin * num2, delta / 4f);
            }
            else
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
            }
            altSpeedOutput = altSpeedInput;
            GetComponent<Rigidbody>().AddForce(base.transform.forward * altSpeedOutput * 2f);
            if (!isBlimpFloating)
            {
                GetComponent<Rigidbody>().AddForce(-Physics.gravity * 0.5f);
            }
            GetComponent<Rigidbody>().AddRelativeTorque(Mathf.Clamp(look_y, 0f - asset.airTurnResponsiveness, asset.airTurnResponsiveness) * num4 / 4f, (float)input_x * asset.airTurnResponsiveness * num4 * 2f, Mathf.Clamp(look_x, 0f - asset.airTurnResponsiveness, asset.airTurnResponsiveness) * (0f - num4) / 4f);
            break;
        }
        case EEngine.BOAT:
        {
            float num7 = Mathf.Lerp(asset.steerMax, asset.steerMin, factor);
            speedTraction = Mathf.Lerp(speedTraction, WaterUtility.isPointUnderwater(base.transform.position + new Vector3(0f, -1f, 0f)) ? 1 : 0, 4f * Time.deltaTime);
            if (num > 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax * num2, delta / 4f);
            }
            else if (num < 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMin * num2, delta / 4f);
            }
            else
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
            }
            altSpeedOutput = altSpeedInput * speedTraction;
            Vector3 forward2 = base.transform.forward;
            forward2.y = 0f;
            GetComponent<Rigidbody>().AddForce(forward2.normalized * altSpeedOutput * 4f * speedTraction);
            if (tires == null || tires.Length == 0 || (!tires[0].isGrounded && !tires[1].isGrounded))
            {
                GetComponent<Rigidbody>().AddRelativeTorque(num * -10f * speedTraction, (float)input_x * num7 / 2f * speedTraction, (float)input_x * -5f * speedTraction);
            }
            break;
        }
        case EEngine.TRAIN:
            if (num > 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMax * num2, delta / 8f);
            }
            else if (num < 0f)
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, asset.speedMin * num2, delta / 8f);
            }
            else
            {
                altSpeedInput = Mathf.Lerp(altSpeedInput, 0f, delta / 8f);
            }
            altSpeedOutput = altSpeedInput;
            break;
        }
        if (asset.engine == EEngine.CAR)
        {
            _speed = base.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;
            _physicsSpeed = _speed;
        }
        else if (asset.engine == EEngine.TRAIN)
        {
            _speed = altSpeedOutput;
            _physicsSpeed = altSpeedOutput;
        }
        else
        {
            _speed = altSpeedOutput;
            _physicsSpeed = base.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;
        }
        _turn = input_x;
        simulateBurnFuel();
        lastUpdatedPos = base.transform.position;
        if (nsb != null)
        {
            nsb.updateLastSnapshot(new TransformSnapshotInfo(base.transform.position, base.transform.rotation));
        }
    }

    private void moveTrain(Vector3 frontPosition, Vector3 frontNormal, Vector3 frontDirection, Vector3 backPosition, Vector3 backNormal, Vector3 backDirection, TrainCar car)
    {
        Vector3 vector = (frontPosition + backPosition) / 2f;
        Vector3 vector2 = Vector3.Lerp(backNormal, frontNormal, 0.5f);
        Vector3 normalized = (frontPosition - backPosition).normalized;
        Quaternion rotation = Quaternion.LookRotation(frontDirection, frontNormal);
        Quaternion rotation2 = Quaternion.LookRotation(backDirection, backNormal);
        Quaternion quaternion = Quaternion.LookRotation(normalized, vector2);
        if (car.root != null)
        {
            car.root.GetComponent<Rigidbody>().MovePosition(vector + vector2 * asset.trainTrackOffset);
            car.root.GetComponent<Rigidbody>().MoveRotation(quaternion);
            car.root.position = vector + vector2 * asset.trainTrackOffset;
            car.root.rotation = quaternion;
        }
        if (car.trackFront != null)
        {
            car.trackFront.position = vector + normalized * asset.trainWheelOffset;
            car.trackFront.rotation = rotation;
        }
        if (car.trackBack != null)
        {
            car.trackBack.position = vector - normalized * asset.trainWheelOffset;
            car.trackBack.rotation = rotation2;
        }
    }

    private void teleportTrain()
    {
        TrainCar[] array = trainCars;
        foreach (TrainCar trainCar in array)
        {
            road.getTrackData(clampRoadPosition(roadPosition + trainCar.trackPositionOffset + asset.trainWheelOffset), out var position, out var normal, out var direction);
            road.getTrackData(clampRoadPosition(roadPosition + trainCar.trackPositionOffset - asset.trainWheelOffset), out var position2, out var normal2, out var direction2);
            moveTrain(position, normal, direction, position2, normal2, direction2, trainCar);
        }
    }

    private void linkTrain()
    {
        Vector3 vector = trainCars[0].root.position + trainCars[0].root.forward * (0f - asset.trainCarLength) / 2f;
        for (int i = 1; i < trainCars.Length; i++)
        {
            Vector3 vector2 = trainCars[i].root.position + trainCars[i].root.forward * asset.trainCarLength / 2f;
            Vector3 vector3 = vector - vector2;
            float sqrMagnitude = vector3.sqrMagnitude;
            if (sqrMagnitude > 1f)
            {
                float num = Mathf.Clamp01((sqrMagnitude - 1f) / 8f);
                trainCars[i].root.position += vector3 * num;
            }
            vector = trainCars[i].root.position + trainCars[i].root.forward * (0f - asset.trainCarLength) / 2f;
        }
    }

    private TrainCar getTrainCar(Transform root)
    {
        Transform trackFront = root.Find("Objects")?.Find("Track_Front");
        Transform trackBack = root.Find("Objects")?.Find("Track_Back");
        return new TrainCar
        {
            root = root,
            trackFront = trackFront,
            trackBack = trackBack
        };
    }

    private float clampRoadPosition(float newRoadPosition)
    {
        if (road.isLoop)
        {
            if (newRoadPosition < 0f)
            {
                return road.trackSampledLength + newRoadPosition;
            }
            if (newRoadPosition > road.trackSampledLength)
            {
                return newRoadPosition - road.trackSampledLength;
            }
            return newRoadPosition;
        }
        return Mathf.Clamp(newRoadPosition, 0.5f + asset.trainWheelOffset, road.trackSampledLength - (float)(trainCars.Length - 1) * asset.trainCarLength - asset.trainWheelOffset - 0.5f);
    }

    private void Update()
    {
        if (asset == null)
        {
            return;
        }
        float deltaTime = Time.deltaTime;
        if (Provider.isServer && hooked != null)
        {
            for (int i = 0; i < hooked.Count; i++)
            {
                HookInfo hookInfo = hooked[i];
                if (hookInfo != null && !(hookInfo.target == null))
                {
                    hookInfo.target.position = hook.TransformPoint(hookInfo.deltaPosition);
                    hookInfo.target.rotation = hook.rotation * hookInfo.deltaRotation;
                }
            }
        }
        if (isPhysical && updates != null && updates.Count == 0 && (Mathf.Abs(lastUpdatedPos.x - base.transform.position.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.y - base.transform.position.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.z - base.transform.position.z) > Provider.UPDATE_DISTANCE))
        {
            lastUpdatedPos = base.transform.position;
            Vector3 pos = ((asset.engine != EEngine.TRAIN) ? base.transform.position : PackRoadPosition(roadPosition));
            updates.Add(new VehicleStateUpdate(pos, base.transform.rotation));
        }
        if (!Provider.isServer && !isPhysical && asset.engine != EEngine.TRAIN && nsb != null)
        {
            TransformSnapshotInfo transformSnapshotInfo = (TransformSnapshotInfo)(object)nsb.getCurrentSnapshot();
            GetComponent<Rigidbody>().MovePosition(transformSnapshotInfo.pos);
            GetComponent<Rigidbody>().MoveRotation(transformSnapshotInfo.rot);
        }
        if (Provider.isServer && isPhysical && asset.engine != EEngine.TRAIN && !isDriven)
        {
            Vector3 worldspacePosition = base.transform.position;
            if (UndergroundWhitelist.adjustPosition(ref worldspacePosition, 10f, 2f))
            {
                GetComponent<Rigidbody>().MovePosition(worldspacePosition);
            }
        }
        if (headlightsOn && !canTurnOnLights)
        {
            tellHeadlights(on: false);
        }
        if (sirensOn && !canTurnOnLights)
        {
            tellSirens(on: false);
        }
        if (isUnderwater)
        {
            if (!isDrowned)
            {
                _lastUnderwater = Time.realtimeSinceStartup;
                _isDrowned = true;
                tellSirens(on: false);
                tellBlimp(on: false);
                tellHeadlights(on: false);
                updateFires();
            }
        }
        else if (_isDrowned)
        {
            _isDrowned = false;
            updateFires();
        }
        synchronizeTaillights();
        if (isDriver)
        {
            if (!asset.hasTraction)
            {
                bool flag = LevelLighting.isPositionSnowy(base.transform.position);
                if (!flag && Level.info != null && Level.info.configData.Use_Snow_Volumes)
                {
                    AmbianceVolume firstOverlappingVolume = VolumeManager<AmbianceVolume, AmbianceVolumeManager>.Get().GetFirstOverlappingVolume(base.transform.position);
                    if (firstOverlappingVolume != null)
                    {
                        flag = (firstOverlappingVolume.weatherMask & 2) != 0;
                    }
                }
                flag &= LevelLighting.snowyness == ELightingSnow.BLIZZARD;
                _slip = Mathf.Lerp(_slip, flag ? 1 : 0, Time.deltaTime * 0.05f);
            }
            else
            {
                _slip = 0f;
            }
            if (tires != null)
            {
                for (int j = 0; j < tires.Length && tires[j] != null; j++)
                {
                    tires[j].update(deltaTime);
                }
            }
            if (asset.engine == EEngine.TRAIN && road != null)
            {
                TrainCar[] array = trainCars;
                foreach (TrainCar trainCar in array)
                {
                    road.getTrackData(clampRoadPosition(roadPosition + trainCar.trackPositionOffset + asset.trainWheelOffset), out var position, out var normal, out var direction);
                    road.getTrackData(clampRoadPosition(roadPosition + trainCar.trackPositionOffset - asset.trainWheelOffset), out var position2, out var normal2, out var direction2);
                    moveTrain(position, normal, direction, position2, normal2, direction2, trainCar);
                }
                float num = altSpeedOutput * deltaTime;
                Transform transform = ((!(altSpeedOutput > 0f)) ? overlapBack : overlapFront);
                BoxCollider boxCollider = transform?.GetComponent<BoxCollider>();
                bool flag2;
                if (boxCollider != null)
                {
                    flag2 = false;
                    Vector3 vector = transform.position + transform.forward * num / 2f;
                    Vector3 size = boxCollider.size;
                    size.z = num;
                    int num2 = Physics.OverlapBoxNonAlloc(vector, size / 2f, grab, transform.rotation, RayMasks.BLOCK_TRAIN, QueryTriggerInteraction.Ignore);
                    for (int l = 0; l < num2; l++)
                    {
                        bool flag3 = false;
                        for (int m = 0; m < trainCars.Length; m++)
                        {
                            if (grab[l].transform.IsChildOf(trainCars[m].root) || grab[l].transform == trainCars[m].root)
                            {
                                flag3 = true;
                                break;
                            }
                        }
                        if (flag3)
                        {
                            continue;
                        }
                        if (grab[l].CompareTag("Vehicle"))
                        {
                            Rigidbody component = grab[l].GetComponent<Rigidbody>();
                            if (!component.isKinematic)
                            {
                                component.AddForce(base.transform.forward * altSpeedOutput, ForceMode.VelocityChange);
                            }
                        }
                        flag2 = true;
                        break;
                    }
                }
                else
                {
                    flag2 = true;
                }
                if (flag2)
                {
                    if (altSpeedOutput > 0f)
                    {
                        if (altSpeedInput > 0f)
                        {
                            altSpeedInput = 0f;
                        }
                    }
                    else if (altSpeedInput < 0f)
                    {
                        altSpeedInput = 0f;
                    }
                }
                else
                {
                    roadPosition += num;
                    roadPosition = clampRoadPosition(roadPosition);
                }
            }
        }
        if (Provider.isServer)
        {
            if (isDriven)
            {
                if (tires != null)
                {
                    for (int n = 0; n < tires.Length && tires[n] != null; n++)
                    {
                        tires[n].checkForTraps();
                    }
                }
            }
            else
            {
                _speed = base.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;
                _physicsSpeed = _speed;
                _turn = 0;
                real = base.transform.position;
            }
            if (isDead && !isExploded && !isUnderwater && Time.realtimeSinceStartup - lastDead > EXPLODE)
            {
                explode();
            }
        }
        if (!Provider.isServer && !isPhysical && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME * 2f)
        {
            lastTick = Time.realtimeSinceStartup;
            _speed = 0f;
            _physicsSpeed = 0f;
            _turn = 0;
        }
        _ = sirensOn;
        if (usesBattery)
        {
            bool flag4 = false;
            bool flag5 = false;
            if (isDriven && isEnginePowered)
            {
                switch (asset.batteryDriving)
                {
                case EBatteryMode.Burn:
                    flag5 = true;
                    break;
                case EBatteryMode.Charge:
                    flag4 = true;
                    break;
                }
            }
            else
            {
                switch (asset.batteryEmpty)
                {
                case EBatteryMode.Burn:
                    flag5 = true;
                    break;
                case EBatteryMode.Charge:
                    flag4 = true;
                    break;
                }
            }
            if (headlightsOn)
            {
                switch (asset.batteryHeadlights)
                {
                case EBatteryMode.Burn:
                    flag5 = true;
                    break;
                case EBatteryMode.Charge:
                    flag4 = true;
                    break;
                }
            }
            if (sirensOn)
            {
                switch (asset.batterySirens)
                {
                case EBatteryMode.Burn:
                    flag5 = true;
                    break;
                case EBatteryMode.Charge:
                    flag4 = true;
                    break;
                }
            }
            flag4 &= hasBattery;
            float num3 = 0f;
            if (flag4)
            {
                num3 = asset.batteryChargeRate;
            }
            else if (flag5)
            {
                num3 = asset.batteryBurnRate;
            }
            batteryBuffer += deltaTime * num3;
            ushort num4 = (ushort)Mathf.FloorToInt(batteryBuffer);
            if (num4 > 0)
            {
                batteryBuffer -= (int)num4;
                if (flag4)
                {
                    askChargeBattery(num4);
                }
                else if (flag5)
                {
                    askBurnBattery(num4);
                }
            }
        }
        if (Provider.isServer)
        {
            updateSafezoneStatus(deltaTime);
        }
    }

    protected void updateSafezoneStatus(float deltaSeconds)
    {
        isInsideSafezone = LevelNodes.isPointInsideSafezone(base.transform.position, out var outSafezoneNode);
        insideSafezoneNode = outSafezoneNode;
        if (isInsideSafezone)
        {
            timeInsideSafezone += deltaSeconds;
            if (Provider.modeConfigData != null && Provider.modeConfigData.Vehicles.Unlocked_After_Seconds_In_Safezone > 0f && timeInsideSafezone > Provider.modeConfigData.Vehicles.Unlocked_After_Seconds_In_Safezone && isEmpty && isLocked)
            {
                VehicleManager.unlockVehicle(this, null);
            }
        }
        else
        {
            timeInsideSafezone = -1f;
        }
    }

    protected virtual void handleTireAliveChanged(Wheel wheel)
    {
        if (isPhysical)
        {
            GetComponent<Rigidbody>().WakeUp();
        }
    }

    [Obsolete]
    public void safeInit()
    {
        safeInit(Assets.find(EAssetType.VEHICLE, id) as VehicleAsset);
    }

    internal void safeInit(VehicleAsset asset)
    {
        _asset = asset;
    }

    [Obsolete]
    public void init()
    {
        init(Assets.find(EAssetType.VEHICLE, id) as VehicleAsset);
    }

    internal void init(VehicleAsset asset)
    {
        safeInit(asset);
        eventHook = base.gameObject.GetComponent<VehicleEventHook>();
        if (!Provider.isServer)
        {
            nsb = new NetworkSnapshotBuffer(Provider.UPDATE_TIME, Provider.UPDATE_DELAY);
        }
        if (Provider.isServer)
        {
            if (fuel == ushort.MaxValue)
            {
                if (Provider.mode == EGameMode.TUTORIAL)
                {
                    fuel = 0;
                }
                else
                {
                    fuel = (ushort)UnityEngine.Random.Range(asset.fuelMin, asset.fuelMax);
                }
            }
            if (health == ushort.MaxValue)
            {
                health = (ushort)UnityEngine.Random.Range(asset.healthMin, asset.healthMax);
            }
            if (batteryCharge == ushort.MaxValue)
            {
                if (usesBattery)
                {
                    if (asset.canSpawnWithBattery && UnityEngine.Random.value < Provider.modeConfigData.Vehicles.Has_Battery_Chance)
                    {
                        float num = UnityEngine.Random.Range(Provider.modeConfigData.Vehicles.Min_Battery_Charge, Provider.modeConfigData.Vehicles.Max_Battery_Charge);
                        num *= asset.batterySpawnChargeMultiplier;
                        batteryCharge = (ushort)(10000f * num);
                    }
                    else
                    {
                        batteryCharge = 0;
                    }
                }
                else
                {
                    batteryCharge = 10000;
                }
            }
        }
        _sirensOn = false;
        _headlightsOn = false;
        _taillightsOn = false;
        waterCenterTransform = base.transform.Find("Water_Center");
        Transform transform = base.transform.Find("Seats");
        if (transform == null)
        {
            Assets.reportError(asset, "missing 'Seats' Transform");
            transform = new GameObject("Seats").transform;
            transform.parent = base.transform;
        }
        Transform transform2 = base.transform.Find("Objects");
        Transform transform3 = base.transform.Find("Turrets");
        Transform transform4 = base.transform.Find("Train_Cars");
        _passengers = new Passenger[transform.childCount];
        for (int i = 0; i < passengers.Length; i++)
        {
            string text = "Seat_" + i;
            Transform transform5 = transform.Find(text);
            if (transform5 == null)
            {
                Assets.reportError(asset, "missing '{0}' Transform", text);
                transform5 = new GameObject(text).transform;
                transform5.parent = transform;
            }
            Transform newObj = null;
            if (transform2 != null)
            {
                newObj = transform2.Find("Seat_" + i);
            }
            Transform transform6 = null;
            Transform transform7 = null;
            Transform newTurretPitch = null;
            Transform newTurretAim = null;
            if (transform3 != null)
            {
                transform6 = transform3.Find("Turret_" + i);
                if (transform6 != null)
                {
                    transform7 = transform6.Find("Yaw");
                    if (transform7 != null)
                    {
                        Transform transform8 = transform7.Find("Seats");
                        if (transform8 != null)
                        {
                            transform5 = transform8.Find("Seat_" + i);
                        }
                        Transform transform9 = transform7.Find("Objects");
                        if (transform9 != null)
                        {
                            newObj = transform9.Find("Seat_" + i);
                        }
                        newTurretPitch = transform7.Find("Pitch");
                    }
                    newTurretAim = transform6.FindChildRecursive("Aim");
                }
            }
            if (transform4 != null)
            {
                Transform transform10 = transform4.FindChildRecursive(text);
                if (transform10 != null)
                {
                    transform5 = transform10;
                }
            }
            passengers[i] = new Passenger(transform5, newObj, transform7, newTurretPitch, newTurretAim);
            if (transform6 != null)
            {
                passengers[i].turretEventHook = transform6.GetComponent<VehicleTurretEventHook>();
            }
            if (asset.shouldSpawnSeatCapsules)
            {
                Transform obj = new GameObject("Clip")
                {
                    layer = 21
                }.transform;
                obj.parent = transform5;
                obj.localPosition = Vector3.zero;
                obj.localRotation = Quaternion.identity;
                obj.localScale = Vector3.one;
                obj.parent = base.transform;
                CapsuleCollider orAddComponent = obj.GetOrAddComponent<CapsuleCollider>();
                orAddComponent.center = new Vector3(0f, PlayerMovement.HEIGHT_STAND * 0.5f, 0f);
                orAddComponent.height = PlayerMovement.HEIGHT_STAND;
                orAddComponent.radius = PlayerStance.RADIUS;
                orAddComponent.enabled = false;
                passengers[i].collider = orAddComponent;
            }
        }
        _turrets = new Passenger[asset.turrets.Length];
        for (int j = 0; j < turrets.Length; j++)
        {
            TurretInfo turretInfo = asset.turrets[j];
            if (turretInfo.seatIndex < passengers.Length)
            {
                passengers[turretInfo.seatIndex].turret = turretInfo;
                _turrets[j] = passengers[turretInfo.seatIndex];
            }
        }
        Transform transform11 = base.transform.Find("Tires");
        if (transform11 != null)
        {
            tires = new Wheel[transform11.childCount];
            for (int k = 0; k < transform11.childCount; k++)
            {
                string text2 = "Tire_" + k;
                Transform transform12 = transform11.Find(text2);
                if (transform12 == null)
                {
                    Assets.reportError(asset, "missing '{0}' Transform", text2);
                    transform12 = new GameObject(text2).transform;
                    transform12.parent = transform11;
                }
                WheelCollider wheelCollider = transform12.GetComponent<WheelCollider>();
                if (wheelCollider == null)
                {
                    Assets.reportError(asset, "missing '{0}' WheelCollider", text2);
                    wheelCollider = transform12.gameObject.AddComponent<WheelCollider>();
                }
                if (asset.wheelColliderMassOverride.HasValue)
                {
                    wheelCollider.mass = asset.wheelColliderMassOverride.Value;
                }
                Wheel wheel = new Wheel(this, k, wheelCollider, k < 2, k >= transform11.childCount - 2);
                wheel.reset();
                wheel.aliveChanged += handleTireAliveChanged;
                tires[k] = wheel;
            }
        }
        else
        {
            tires = new Wheel[0];
        }
        buoyancy = base.transform.Find("Buoyancy");
        if (buoyancy != null)
        {
            for (int l = 0; l < buoyancy.childCount; l++)
            {
                Transform child = buoyancy.GetChild(l);
                child.gameObject.AddComponent<Buoyancy>().density = buoyancy.childCount * 500;
                if (asset.engine == EEngine.BLIMP)
                {
                    child.GetComponent<Buoyancy>().overrideSurfaceElevation = Level.info.configData.Blimp_Altitude;
                }
            }
        }
        hook = base.transform.Find("Hook");
        hooked = new List<HookInfo>();
        Transform transform13 = base.transform.Find("DepthMask");
        if (transform13 != null)
        {
            Renderer component = transform13.GetComponent<Renderer>();
            if (component != null)
            {
                component.sharedMaterial = Resources.Load<Material>("Materials/DepthMask");
            }
        }
        if (isExploded)
        {
            tellExploded();
        }
        if (asset.engine == EEngine.TRAIN)
        {
            int childCount = transform4.childCount;
            trainCars = new TrainCar[1 + childCount];
            trainCars[0] = getTrainCar(base.transform);
            for (int m = 1; m <= childCount; m++)
            {
                Transform transform14 = transform4.Find("Train_Car_" + m);
                transform14.parent = null;
                transform14.GetOrAddComponent<VehicleRef>().vehicle = this;
                TrainCar trainCar = getTrainCar(transform14);
                trainCar.trackPositionOffset = (float)m * (0f - asset.trainCarLength);
                trainCars[m] = trainCar;
            }
            TrainCar[] array = trainCars;
            foreach (TrainCar trainCar2 in array)
            {
                if (overlapFront == null)
                {
                    overlapFront = trainCar2.root.Find("Overlap_Front");
                }
                if (overlapBack == null)
                {
                    overlapBack = trainCar2.root.Find("Overlap_Back");
                }
                if (overlapFront != null && overlapBack != null)
                {
                    break;
                }
            }
            foreach (LevelTrainAssociation train in Level.info.configData.Trains)
            {
                if (train.VehicleID == id)
                {
                    roadIndex = train.RoadIndex;
                    break;
                }
            }
            road = LevelRoads.getRoad(roadIndex);
            roadPosition = clampRoadPosition(roadPosition);
            teleportTrain();
        }
        if (asset.physicsProfileRef.isValid)
        {
            asset.physicsProfileRef.Find()?.applyTo(this);
        }
        decayLastUpdateTime = Time.time;
        decayLastUpdatePosition = base.transform.position;
    }

    private void Awake()
    {
    }

    private void initBumper(Transform bumper, bool reverse, bool instakill)
    {
        if (!(bumper == null))
        {
            if (Provider.isServer)
            {
                Bumper bumper2 = bumper.gameObject.AddComponent<Bumper>();
                bumper2.reverse = reverse;
                bumper2.instakill = instakill;
                bumper2.init(this);
            }
            else
            {
                UnityEngine.Object.Destroy(bumper.gameObject);
            }
        }
    }

    private void initBumpers(Transform root)
    {
        Transform transform = root.FindChildRecursive("Nav");
        if (transform != null)
        {
            if (Provider.isServer)
            {
                transform.DestroyRigidbody();
            }
            else
            {
                UnityEngine.Object.Destroy(transform.gameObject);
            }
        }
        Transform bumper = root.FindChildRecursive("Bumper");
        initBumper(bumper, reverse: false, asset.engine == EEngine.TRAIN);
        Transform bumper2 = root.FindChildRecursive("Bumper_Front");
        initBumper(bumper2, reverse: false, asset.engine == EEngine.TRAIN);
        Transform bumper3 = root.FindChildRecursive("Bumper_Back");
        initBumper(bumper3, reverse: true, asset.engine == EEngine.TRAIN);
    }

    private void Start()
    {
        if (trainCars != null && trainCars.Length != 0)
        {
            TrainCar[] array = trainCars;
            foreach (TrainCar trainCar in array)
            {
                initBumpers(trainCar.root);
            }
        }
        else
        {
            initBumpers(base.transform);
        }
        updateVehicle();
        updatePhysics();
        updateEngine();
        updates = new List<VehicleStateUpdate>();
    }

    private void OnDestroy()
    {
        dropTrunkItems();
        if (skinMaterialToDestroy != null)
        {
            UnityEngine.Object.Destroy(skinMaterialToDestroy);
            skinMaterialToDestroy = null;
        }
        _ = isExploded;
        if (headlightsMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(headlightsMaterial);
        }
        if (taillightsMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(taillightsMaterial);
        }
        else if (taillightMaterials != null)
        {
            for (int i = 0; i < taillightMaterials.Length; i++)
            {
                if (taillightMaterials[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(taillightMaterials[i]);
                }
            }
        }
        if (sirenMaterials != null)
        {
            for (int j = 0; j < sirenMaterials.Length; j++)
            {
                if (sirenMaterials[j] != null)
                {
                    UnityEngine.Object.DestroyImmediate(sirenMaterials[j]);
                }
            }
        }
        if (rotors == null)
        {
            return;
        }
        for (int k = 0; k < rotors.Length; k++)
        {
            if (rotors[k].material_0 != null)
            {
                DynamicWaterTransparentSort.Get().Unregister(rotors[k].sortHandle_0);
                UnityEngine.Object.DestroyImmediate(rotors[k].material_0);
                rotors[k].material_0 = null;
            }
            if (rotors[k].material_1 != null)
            {
                DynamicWaterTransparentSort.Get().Unregister(rotors[k].sortHandle_1);
                UnityEngine.Object.DestroyImmediate(rotors[k].material_1);
                rotors[k].material_1 = null;
            }
        }
    }

    public void gatherVehicleColliders()
    {
        _vehicleColliders = new List<Collider>();
        base.gameObject.GetComponentsInChildren(includeInactive: true, _vehicleColliders);
        initCenterCollider();
    }

    public void ignoreCollisionWith(IEnumerable<Collider> otherColliders, bool shouldIgnore)
    {
        if (_vehicleColliders == null)
        {
            throw new Exception("gatherVehicleColliders was not called yet");
        }
        for (int num = _vehicleColliders.Count - 1; num >= 0; num--)
        {
            Collider collider = _vehicleColliders[num];
            if (collider == null)
            {
                _vehicleColliders.RemoveAtFast(num);
            }
            else
            {
                foreach (Collider otherCollider in otherColliders)
                {
                    if (!(otherCollider == null))
                    {
                        Physics.IgnoreCollision(collider, otherCollider, shouldIgnore);
                    }
                }
            }
        }
    }

    private void ignoreCollisionWithVehicle(InteractableVehicle otherVehicle, bool shouldIgnore)
    {
        ignoreCollisionWith(otherVehicle.vehicleColliders, shouldIgnore);
    }

    public Vector3 getClosestPointOnHull(Vector3 position)
    {
        if (_vehicleColliders == null)
        {
            throw new Exception("gatherVehicleColliders was not called yet");
        }
        return CollisionUtil.ClosestPoint(_vehicleColliders, position);
    }

    public float getSqrDistanceFromHull(Vector3 position)
    {
        return (getClosestPointOnHull(position) - position).sqrMagnitude;
    }

    private void initCenterCollider()
    {
        center = base.transform.Find("Center");
        if (center != null)
        {
            return;
        }
        center = new GameObject("Center").transform;
        center.parent = base.transform;
        center.localPosition = Vector3.zero;
        center.localRotation = Quaternion.identity;
        center.localScale = Vector3.one;
        float num = 0.001f;
        foreach (Collider vehicleCollider in vehicleColliders)
        {
            if (vehicleCollider.isTrigger)
            {
                continue;
            }
            if (vehicleCollider is BoxCollider collider)
            {
                float boxVolume = collider.GetBoxVolume();
                if (boxVolume > num)
                {
                    num = boxVolume;
                    center.position = collider.TransformBoxCenter();
                }
            }
            else if (vehicleCollider is SphereCollider collider2)
            {
                float sphereVolume = collider2.GetSphereVolume();
                if (sphereVolume > num)
                {
                    num = sphereVolume;
                    center.position = collider2.TransformSphereCenter();
                }
            }
            else if (vehicleCollider is CapsuleCollider collider3)
            {
                float capsuleVolume = collider3.GetCapsuleVolume();
                if (capsuleVolume > num)
                {
                    num = capsuleVolume;
                    center.position = collider3.TransformCapsuleCenter();
                }
            }
        }
    }
}
