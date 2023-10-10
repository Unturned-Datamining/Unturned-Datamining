using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class VehicleManager : SteamCaller
{
    public delegate void EnterVehicleRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow);

    public delegate void ExitVehicleRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow, ref Vector3 pendingLocation, ref float pendingYaw);

    public delegate void SwapSeatRequestHandler(Player player, InteractableVehicle vehicle, ref bool shouldAllow, byte fromSeatIndex, ref byte toSeatIndex);

    public delegate void ToggleVehicleLockRequested(InteractableVehicle vehicle, ref bool shouldAllow);

    public const byte SAVEDATA_VERSION_ADDED_DECAY = 13;

    public const byte SAVEDATA_VERSION_REPLACED_ID_WITH_GUID = 14;

    public const byte SAVEDATA_VERSION_BATTERY_GUID = 15;

    private const byte SAVEDATA_VERSION_NEWEST = 15;

    public static readonly byte SAVEDATA_VERSION = 15;

    public static VehicleLockpickedSignature onVehicleLockpicked;

    public static DamageVehicleRequestHandler onDamageVehicleRequested;

    public static RepairVehicleRequestHandler onRepairVehicleRequested;

    public static DamageTireRequestHandler onDamageTireRequested;

    public static VehicleCarjackedSignature onVehicleCarjacked;

    public static SiphonVehicleRequestHandler onSiphonVehicleRequested;

    private static VehicleManager manager;

    private static List<InteractableVehicle> _vehicles;

    private static uint highestInstanceID;

    private static ushort respawnVehicleIndex;

    private static float lastTick;

    private static readonly ClientStaticMethod<uint, CSteamID, CSteamID, bool> SendVehicleLockState = ClientStaticMethod<uint, CSteamID, CSteamID, bool>.Get(ReceiveVehicleLockState);

    private static readonly ClientStaticMethod<uint, ushort, ushort> SendVehicleSkin = ClientStaticMethod<uint, ushort, ushort>.Get(ReceiveVehicleSkin);

    private static readonly ClientStaticMethod<uint, bool> SendVehicleSirens = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleSirens);

    private static readonly ClientStaticMethod<uint, bool> SendVehicleBlimp = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleBlimp);

    private static readonly ClientStaticMethod<uint, bool> SendVehicleHeadlights = ClientStaticMethod<uint, bool>.Get(ReceiveVehicleHeadlights);

    private static readonly ClientStaticMethod<uint> SendVehicleHorn = ClientStaticMethod<uint>.Get(ReceiveVehicleHorn);

    private static readonly ClientStaticMethod<uint, ushort> SendVehicleFuel = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleFuel);

    private static readonly ClientStaticMethod<uint, ushort> SendVehicleBatteryCharge = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleBatteryCharge);

    private static readonly ClientStaticMethod<uint, byte> SendVehicleTireAliveMask = ClientStaticMethod<uint, byte>.Get(ReceiveVehicleTireAliveMask);

    private static readonly ClientStaticMethod<uint> SendVehicleExploded = ClientStaticMethod<uint>.Get(ReceiveVehicleExploded);

    private static readonly ClientStaticMethod<uint, ushort> SendVehicleHealth = ClientStaticMethod<uint, ushort>.Get(ReceiveVehicleHealth);

    private static readonly ClientStaticMethod<uint, Vector3, int> SendVehicleRecov = ClientStaticMethod<uint, Vector3, int>.Get(ReceiveVehicleRecov);

    private static uint seq;

    private static readonly ClientStaticMethod SendVehicleStates = ClientStaticMethod.Get(ReceiveVehicleStates);

    private static readonly ClientStaticMethod<uint> SendDestroySingleVehicle = ClientStaticMethod<uint>.Get(ReceiveDestroySingleVehicle);

    private static readonly ClientStaticMethod SendDestroyAllVehicles = ClientStaticMethod.Get(ReceiveDestroyAllVehicles);

    private static readonly ClientStaticMethod SendSingleVehicle = ClientStaticMethod.Get(ReceiveSingleVehicle);

    private static readonly ClientStaticMethod SendMultipleVehicles = ClientStaticMethod.Get(ReceiveMultipleVehicles);

    private static readonly ClientStaticMethod<uint, byte, CSteamID> SendEnterVehicle = ClientStaticMethod<uint, byte, CSteamID>.Get(ReceiveEnterVehicle);

    private static readonly ClientStaticMethod<uint, byte, Vector3, byte, bool> SendExitVehicle = ClientStaticMethod<uint, byte, Vector3, byte, bool>.Get(ReceiveExitVehicle);

    private static readonly ClientStaticMethod<uint, byte, byte> SendSwapVehicleSeats = ClientStaticMethod<uint, byte, byte>.Get(ReceiveSwapVehicleSeats);

    private static readonly ServerStaticMethod SendVehicleLockRequest = ServerStaticMethod.Get(ReceiveVehicleLockRequest);

    private static readonly ServerStaticMethod SendVehicleSkinRequest = ServerStaticMethod.Get(ReceiveVehicleSkinRequest);

    private static readonly ServerStaticMethod<bool> SendToggleVehicleHeadlights = ServerStaticMethod<bool>.Get(ReceiveToggleVehicleHeadlights);

    private static readonly ServerStaticMethod<byte> SendUseVehicleBonus = ServerStaticMethod<byte>.Get(ReceiveUseVehicleBonus);

    private static readonly ServerStaticMethod SendStealVehicleBattery = ServerStaticMethod.Get(ReceiveStealVehicleBattery);

    private static readonly ServerStaticMethod SendVehicleHornRequest = ServerStaticMethod.Get(ReceiveVehicleHornRequest);

    private static readonly ServerStaticMethod<uint, byte[], byte[], byte> SendEnterVehicleRequest = ServerStaticMethod<uint, byte[], byte[], byte>.Get(ReceiveEnterVehicleRequest);

    private static readonly ServerStaticMethod<Vector3> SendExitVehicleRequest = ServerStaticMethod<Vector3>.Get(ReceiveExitVehicleRequest);

    private static readonly ServerStaticMethod<byte> SendSwapVehicleRequest = ServerStaticMethod<byte>.Get(ReceiveSwapVehicleRequest);

    public static Action<InteractableVehicle> OnVehicleExploded;

    private List<InteractableVehicle> vehiclesToSend = new List<InteractableVehicle>();

    private static float lastSendOverflowWarning;

    private bool enableDecayUpdate;

    private int decayUpdateIndex;

    internal const int NETIDS_PER_VEHICLE = 21;

    internal const int POSITION_FRAC_BIT_COUNT = 8;

    internal const int ROTATION_BIT_COUNT = 11;

    public static VehicleManager instance => manager;

    public static List<InteractableVehicle> vehicles => _vehicles;

    public static uint maxInstances => Level.info.size switch
    {
        ELevelSize.TINY => Provider.modeConfigData.Vehicles.Max_Instances_Tiny, 
        ELevelSize.SMALL => Provider.modeConfigData.Vehicles.Max_Instances_Small, 
        ELevelSize.MEDIUM => Provider.modeConfigData.Vehicles.Max_Instances_Medium, 
        ELevelSize.LARGE => Provider.modeConfigData.Vehicles.Max_Instances_Large, 
        ELevelSize.INSANE => Provider.modeConfigData.Vehicles.Max_Instances_Insane, 
        _ => 0u, 
    };

    public static event EnterVehicleRequestHandler onEnterVehicleRequested;

    public static event ExitVehicleRequestHandler onExitVehicleRequested;

    public static event SwapSeatRequestHandler onSwapSeatRequested;

    public static event Action<InteractableVehicle> OnPreDestroyVehicle;

    public static event ToggleVehicleLockRequested OnToggleVehicleLockRequested;

    public static event Action<InteractableVehicle> OnToggledVehicleLock;

    private static uint allocateInstanceID()
    {
        return ++highestInstanceID;
    }

    public static byte getVehicleRandomTireAliveMask(VehicleAsset asset)
    {
        if (asset.canTiresBeDamaged)
        {
            int num = 0;
            for (byte b = 0; b < 8; b = (byte)(b + 1))
            {
                if (UnityEngine.Random.value < Provider.modeConfigData.Vehicles.Has_Tire_Chance)
                {
                    int num2 = 1 << (int)b;
                    num |= num2;
                }
            }
            return (byte)num;
        }
        return byte.MaxValue;
    }

    public static void getVehiclesInRadius(Vector3 center, float sqrRadius, List<InteractableVehicle> result)
    {
        if (vehicles == null)
        {
            return;
        }
        for (int i = 0; i < vehicles.Count; i++)
        {
            InteractableVehicle interactableVehicle = vehicles[i];
            if (!interactableVehicle.isDead && (interactableVehicle.transform.position - center).sqrMagnitude < sqrRadius)
            {
                result.Add(interactableVehicle);
            }
        }
    }

    public static InteractableVehicle findVehicleByNetInstanceID(uint instanceID)
    {
        foreach (InteractableVehicle vehicle in vehicles)
        {
            if (vehicle != null && vehicle.instanceID == instanceID)
            {
                return vehicle;
            }
        }
        return null;
    }

    public static InteractableVehicle getVehicle(uint instanceID)
    {
        return findVehicleByNetInstanceID(instanceID);
    }

    public static void damage(InteractableVehicle vehicle, float damage, float times, bool canRepair, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (vehicle == null || vehicle.asset == null || vehicle.isDead)
        {
            return;
        }
        if (!vehicle.asset.isVulnerable && !vehicle.asset.isVulnerableToExplosions && !vehicle.asset.isVulnerableToEnvironment)
        {
            UnturnedLog.error("Somehow tried to damage completely invulnerable vehicle: " + vehicle?.ToString() + " " + damage + " " + times + " " + canRepair);
        }
        else
        {
            times *= Provider.modeConfigData.Vehicles.Armor_Multiplier;
            ushort pendingTotalDamage = (ushort)(damage * times);
            bool shouldAllow = true;
            onDamageVehicleRequested?.Invoke(instigatorSteamID, vehicle, ref pendingTotalDamage, ref canRepair, ref shouldAllow, damageOrigin);
            if (shouldAllow && pendingTotalDamage >= 1)
            {
                vehicle.askDamage(pendingTotalDamage, canRepair);
            }
        }
    }

    public static void damageTire(InteractableVehicle vehicle, int tireIndex, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (tireIndex >= 0)
        {
            bool shouldAllow = true;
            onDamageTireRequested?.Invoke(instigatorSteamID, vehicle, tireIndex, ref shouldAllow, damageOrigin);
            if (shouldAllow)
            {
                vehicle.askDamageTire(tireIndex);
            }
        }
    }

    public static void repair(InteractableVehicle vehicle, float damage, float times)
    {
        repair(vehicle, damage, times, CSteamID.Nil);
    }

    public static void repair(InteractableVehicle vehicle, float damage, float times, CSteamID instigatorSteamID = default(CSteamID))
    {
        if (!(vehicle == null) && !vehicle.isExploded && !vehicle.isRepaired)
        {
            ushort pendingTotalHealing = (ushort)(damage * times);
            bool shouldAllow = true;
            onRepairVehicleRequested?.Invoke(instigatorSteamID, vehicle, ref pendingTotalHealing, ref shouldAllow);
            if (shouldAllow && pendingTotalHealing >= 1)
            {
                vehicle.askRepair(pendingTotalHealing);
            }
        }
    }

    [Obsolete("spawnVehicleV2 returns the spawned instance")]
    public static void spawnVehicle(ushort id, Vector3 point, Quaternion angle)
    {
        spawnVehicleV2(id, point, angle);
    }

    [Obsolete("spawnLockedVehicleForPlayerV2 returns the spawned instance")]
    public static void spawnLockedVehicleForPlayer(ushort id, Vector3 point, Quaternion angle, Player player)
    {
        spawnLockedVehicleForPlayerV2(id, point, angle, player);
    }

    public static InteractableVehicle spawnVehicleV2(ushort id, Vector3 point, Quaternion angle)
    {
        return spawnVehicleInternal(id, point, angle, CSteamID.Nil);
    }

    public static InteractableVehicle spawnLockedVehicleForPlayerV2(ushort id, Vector3 point, Quaternion angle, Player player)
    {
        if (player == null)
        {
            throw new ArgumentNullException("player");
        }
        return spawnVehicleInternal(id, point, angle, player.channel.owner.playerID.steamID);
    }

    public static InteractableVehicle SpawnVehicleV3(VehicleAsset asset, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, byte[][] turrets, byte tireAliveMask)
    {
        NetId netId = NetIdRegistry.ClaimBlock(21u);
        InteractableVehicle spawnedVehicle = manager.addVehicle(asset.GUID, skinID, mythicID, roadPosition, point, angle, sirens, blimp, headlights, taillights, fuel, isExploded: false, health, batteryCharge, owner, group, locked, null, turrets, allocateInstanceID(), tireAliveMask, netId);
        if (spawnedVehicle == null)
        {
            return null;
        }
        SendSingleVehicle.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), delegate(NetPakWriter writer)
        {
            sendVehicle(spawnedVehicle, writer);
        });
        return spawnedVehicle;
    }

    private static InteractableVehicle spawnVehicleInternal(ushort id, Vector3 point, Quaternion angle, CSteamID owner)
    {
        if (!(Assets.find(EAssetType.VEHICLE, id) is VehicleAsset vehicleAsset))
        {
            return null;
        }
        bool locked = owner != CSteamID.Nil;
        return SpawnVehicleV3(vehicleAsset, 0, 0, 0f, point, angle, sirens: false, blimp: false, headlights: false, taillights: false, vehicleAsset.fuel, vehicleAsset.health, 10000, owner, CSteamID.Nil, locked, null, byte.MaxValue);
    }

    public static void enterVehicle(InteractableVehicle vehicle)
    {
        VehiclePhysicsProfileAsset vehiclePhysicsProfileAsset = vehicle.asset.physicsProfileRef.Find();
        byte[] arg = ((vehiclePhysicsProfileAsset != null) ? vehiclePhysicsProfileAsset.hash : new byte[0]);
        SendEnterVehicleRequest.Invoke(ENetReliability.Unreliable, vehicle.instanceID, vehicle.asset.hash, arg, (byte)vehicle.asset.engine);
    }

    public static void exitVehicle()
    {
        if (Player.player.movement.getVehicle() != null)
        {
            SendExitVehicleRequest.Invoke(ENetReliability.Unreliable, Player.player.movement.getVehicle().GetComponent<Rigidbody>().velocity);
        }
    }

    public static void swapVehicle(byte toSeat)
    {
        if (Player.player.movement.getVehicle() != null)
        {
            SendSwapVehicleRequest.Invoke(ENetReliability.Unreliable, toSeat);
        }
    }

    public static void sendVehicleLock()
    {
        if (Player.player.movement.getVehicle() != null)
        {
            SendVehicleLockRequest.Invoke(ENetReliability.Unreliable);
        }
    }

    public static void sendVehicleSkin()
    {
        if (Player.player.movement.getVehicle() != null)
        {
            SendVehicleSkinRequest.Invoke(ENetReliability.Unreliable);
        }
    }

    public static void sendVehicleHeadlights()
    {
        InteractableVehicle vehicle = Player.player.movement.getVehicle();
        if (!(vehicle == null) && vehicle.asset != null)
        {
            bool flag = !vehicle.headlightsOn;
            if (vehicle.asset.hasHeadlights || !flag)
            {
                SendToggleVehicleHeadlights.Invoke(ENetReliability.Unreliable, flag);
            }
        }
    }

    public static void sendVehicleBonus()
    {
        InteractableVehicle vehicle = Player.player.movement.getVehicle();
        if (vehicle == null)
        {
            return;
        }
        byte arg;
        if (vehicle.asset.hasSirens)
        {
            arg = 0;
        }
        else if (vehicle.asset.hasHook)
        {
            arg = 1;
        }
        else
        {
            if (vehicle.asset.engine != EEngine.BLIMP)
            {
                return;
            }
            arg = 2;
        }
        SendUseVehicleBonus.Invoke(ENetReliability.Unreliable, arg);
    }

    public static void sendVehicleStealBattery()
    {
        if (Player.player.movement.getVehicle() != null)
        {
            SendStealVehicleBattery.Invoke(ENetReliability.Unreliable);
        }
    }

    public static void sendVehicleHorn()
    {
        InteractableVehicle vehicle = Player.player.movement.getVehicle();
        if (vehicle != null && vehicle.asset.hasHorn)
        {
            SendVehicleHornRequest.Invoke(ENetReliability.Unreliable);
        }
    }

    [Obsolete]
    public void sendVehicle(InteractableVehicle vehicle)
    {
    }

    internal static void sendVehicle(InteractableVehicle vehicle, NetPakWriter writer)
    {
        Vector3 value = ((vehicle.asset.engine != EEngine.TRAIN) ? vehicle.transform.position : InteractableVehicle.PackRoadPosition(vehicle.roadPosition));
        writer.WriteGuid(vehicle.asset.GUID);
        writer.WriteUInt16(vehicle.skinID);
        writer.WriteUInt16(vehicle.mythicID);
        writer.WriteClampedVector3(value, 13, 8);
        writer.WriteQuaternion(vehicle.transform.rotation, 11);
        writer.WriteBit(vehicle.sirensOn);
        writer.WriteBit(vehicle.isBlimpFloating);
        writer.WriteBit(vehicle.headlightsOn);
        writer.WriteBit(vehicle.taillightsOn);
        writer.WriteUInt16(vehicle.fuel);
        writer.WriteBit(vehicle.isExploded);
        writer.WriteUInt16(vehicle.health);
        writer.WriteUInt16(vehicle.batteryCharge);
        writer.WriteSteamID(vehicle.lockedOwner);
        writer.WriteSteamID(vehicle.lockedGroup);
        writer.WriteBit(vehicle.isLocked);
        writer.WriteUInt8((byte)vehicle.passengers.Length);
        for (byte b = 0; b < vehicle.passengers.Length; b = (byte)(b + 1))
        {
            Passenger passenger = vehicle.passengers[b];
            if (passenger.player != null)
            {
                writer.WriteSteamID(passenger.player.playerID.steamID);
            }
            else
            {
                writer.WriteSteamID(CSteamID.Nil);
            }
        }
        writer.WriteUInt32(vehicle.instanceID);
        writer.WriteUInt8(vehicle.tireAliveMask);
        writer.WriteNetId(vehicle.GetNetId());
    }

    [Obsolete]
    public void tellVehicleLock(CSteamID steamID, uint instanceID, CSteamID owner, CSteamID group, bool locked)
    {
        ReceiveVehicleLockState(instanceID, owner, group, locked);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleLock")]
    public static void ReceiveVehicleLockState(uint instanceID, CSteamID owner, CSteamID group, bool locked)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellLocked(owner, group, locked);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleSkin(CSteamID steamID, uint instanceID, ushort skinID, ushort mythicID)
    {
        ReceiveVehicleSkin(instanceID, skinID, mythicID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleSkin")]
    public static void ReceiveVehicleSkin(uint instanceID, ushort skinID, ushort mythicID)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellSkin(skinID, mythicID);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleSirens(CSteamID steamID, uint instanceID, bool on)
    {
        ReceiveVehicleSirens(instanceID, on);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleSirens")]
    public static void ReceiveVehicleSirens(uint instanceID, bool on)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellSirens(on);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleBlimp(CSteamID steamID, uint instanceID, bool on)
    {
        ReceiveVehicleBlimp(instanceID, on);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleBlimp")]
    public static void ReceiveVehicleBlimp(uint instanceID, bool on)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellBlimp(on);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleHeadlights(CSteamID steamID, uint instanceID, bool on)
    {
        ReceiveVehicleHeadlights(instanceID, on);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleHeadlights")]
    public static void ReceiveVehicleHeadlights(uint instanceID, bool on)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellHeadlights(on);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleHorn(CSteamID steamID, uint instanceID)
    {
        ReceiveVehicleHorn(instanceID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleHorn")]
    public static void ReceiveVehicleHorn(uint instanceID)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellHorn();
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleFuel(CSteamID steamID, uint instanceID, ushort newFuel)
    {
        ReceiveVehicleFuel(instanceID, newFuel);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleFuel")]
    public static void ReceiveVehicleFuel(uint instanceID, ushort newFuel)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellFuel(newFuel);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleBatteryCharge(CSteamID steamID, uint instanceID, ushort newBatteryCharge)
    {
        ReceiveVehicleBatteryCharge(instanceID, newBatteryCharge);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleBatteryCharge")]
    public static void ReceiveVehicleBatteryCharge(uint instanceID, ushort newBatteryCharge)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellBatteryCharge(newBatteryCharge);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleTireAliveMask(CSteamID steamID, uint instanceID, byte newTireAliveMask)
    {
        ReceiveVehicleTireAliveMask(instanceID, newTireAliveMask);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleTireAliveMask")]
    public static void ReceiveVehicleTireAliveMask(uint instanceID, byte newTireAliveMask)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tireAliveMask = newTireAliveMask;
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleExploded(CSteamID steamID, uint instanceID)
    {
        ReceiveVehicleExploded(instanceID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleExploded")]
    public static void ReceiveVehicleExploded(uint instanceID)
    {
        InteractableVehicle interactableVehicle = findVehicleByNetInstanceID(instanceID);
        if (interactableVehicle == null || interactableVehicle.isExploded)
        {
            return;
        }
        BarricadeManager.trimPlant(interactableVehicle.transform);
        if (interactableVehicle.trainCars != null)
        {
            for (int i = 1; i < interactableVehicle.trainCars.Length; i++)
            {
                BarricadeManager.uprootPlant(interactableVehicle.trainCars[i].root);
            }
        }
        interactableVehicle.tellExploded();
    }

    [Obsolete]
    public void tellVehicleHealth(CSteamID steamID, uint instanceID, ushort newHealth)
    {
        ReceiveVehicleHealth(instanceID, newHealth);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleHealth")]
    public static void ReceiveVehicleHealth(uint instanceID, ushort newHealth)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellHealth(newHealth);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleRecov(CSteamID steamID, uint instanceID, Vector3 newPosition, int newRecov)
    {
        ReceiveVehicleRecov(instanceID, newPosition, newRecov);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleRecov")]
    public static void ReceiveVehicleRecov(uint instanceID, Vector3 newPosition, int newRecov)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].tellRecov(newPosition, newRecov);
                break;
            }
        }
    }

    [Obsolete]
    public void tellVehicleStates(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveVehicleStates(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        if (value <= seq)
        {
            return;
        }
        seq = value;
        reader.ReadUInt16(out var value2);
        if (value2 < 1)
        {
            return;
        }
        for (ushort num = 0; num < value2; num = (ushort)(num + 1))
        {
            reader.ReadUInt32(out var value3);
            reader.ReadClampedVector3(out var value4, 13, 8);
            reader.ReadQuaternion(out var value5, 11);
            reader.ReadUInt8(out var value6);
            reader.ReadUInt8(out var value7);
            reader.ReadUInt8(out var value8);
            InteractableVehicle interactableVehicle = findVehicleByNetInstanceID(value3);
            if (!(interactableVehicle == null))
            {
                interactableVehicle.tellState(value4, value5, value6, value7, value8);
            }
        }
    }

    [Obsolete]
    public void tellVehicleDestroy(CSteamID steamID, uint instanceID)
    {
        ReceiveDestroySingleVehicle(instanceID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleDestroy")]
    public static void ReceiveDestroySingleVehicle(uint instanceID)
    {
        InteractableVehicle interactableVehicle = null;
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                interactableVehicle = vehicles[i];
                vehicles.RemoveAt(i);
                break;
            }
        }
        if (interactableVehicle == null)
        {
            return;
        }
        BarricadeManager.uprootPlant(interactableVehicle.transform);
        if (interactableVehicle.trainCars != null)
        {
            for (int j = 1; j < interactableVehicle.trainCars.Length; j++)
            {
                BarricadeManager.uprootPlant(interactableVehicle.trainCars[j].root);
            }
        }
        VehicleManager.OnPreDestroyVehicle?.TryInvoke("OnPreDestroyVehicle", interactableVehicle);
        NetIdRegistry.ReleaseTransform(interactableVehicle.GetNetId() + 1u, interactableVehicle.transform);
        interactableVehicle.ReleaseNetId();
        EffectManager.ClearAttachments(interactableVehicle.transform);
        UnityEngine.Object.Destroy(interactableVehicle.gameObject);
        respawnVehicleIndex--;
    }

    [Obsolete]
    public void tellVehicleDestroyAll(CSteamID steamID)
    {
        ReceiveDestroyAllVehicles();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVehicleDestroyAll")]
    public static void ReceiveDestroyAllVehicles()
    {
        for (int num = vehicles.Count - 1; num >= 0; num--)
        {
            BarricadeManager.uprootPlant(vehicles[num].transform);
            if (vehicles[num].trainCars != null)
            {
                for (int i = 1; i < vehicles[num].trainCars.Length; i++)
                {
                    BarricadeManager.uprootPlant(vehicles[num].trainCars[i].root);
                }
            }
            VehicleManager.OnPreDestroyVehicle?.TryInvoke("OnPreDestroyVehicle", vehicles[num]);
            NetIdRegistry.ReleaseTransform(vehicles[num].GetNetId() + 1u, vehicles[num].transform);
            vehicles[num].ReleaseNetId();
            EffectManager.ClearAttachments(vehicles[num].transform);
            UnityEngine.Object.Destroy(vehicles[num].gameObject);
            vehicles.RemoveAt(num);
        }
        respawnVehicleIndex = 0;
        vehicles.Clear();
    }

    public static void askVehicleDestroy(InteractableVehicle vehicle)
    {
        if (Provider.isServer)
        {
            vehicle.forceRemoveAllPlayers();
            SendDestroySingleVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);
        }
    }

    public static void askVehicleDestroyAll()
    {
        if (Provider.isServer)
        {
            for (int num = vehicles.Count - 1; num >= 0; num--)
            {
                vehicles[num].forceRemoveAllPlayers();
            }
            SendDestroyAllVehicles.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections());
        }
    }

    [Obsolete]
    public void tellVehicle(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveSingleVehicle(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadClampedVector3(out var value4, 13, 8);
        float roadPosition = InteractableVehicle.UnpackRoadPosition(value4);
        reader.ReadQuaternion(out var value5, 11);
        reader.ReadBit(out var value6);
        reader.ReadBit(out var value7);
        reader.ReadBit(out var value8);
        reader.ReadBit(out var value9);
        reader.ReadUInt16(out var value10);
        reader.ReadBit(out var value11);
        reader.ReadUInt16(out var value12);
        reader.ReadUInt16(out var value13);
        reader.ReadSteamID(out CSteamID value14);
        reader.ReadSteamID(out CSteamID value15);
        reader.ReadBit(out var value16);
        reader.ReadUInt8(out var value17);
        CSteamID[] array = new CSteamID[value17];
        for (int i = 0; i < array.Length; i++)
        {
            reader.ReadSteamID(out array[i]);
        }
        reader.ReadUInt32(out var value18);
        reader.ReadUInt8(out var value19);
        reader.ReadNetId(out var value20);
        manager.addVehicle(value, value2, value3, roadPosition, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16, array, null, value18, value19, value20);
    }

    [Obsolete]
    public void tellVehicles(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMultipleVehicles(in ClientInvocationContext context)
    {
        context.reader.ReadUInt16(out var value);
        for (int i = 0; i < value; i++)
        {
            ReceiveSingleVehicle(in context);
        }
        Level.isLoadingVehicles = false;
    }

    private static void askVehiclesHelper(ITransportConnection transportConnection, int startIndex, int endIndex)
    {
        if (endIndex > vehicles.Count)
        {
            endIndex = vehicles.Count;
        }
        int count = endIndex - startIndex;
        if (count < 1)
        {
            throw new ArgumentException("startIndex or endIndex to askVehiclesHelper invalid");
        }
        SendMultipleVehicles.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt16((ushort)count);
            for (int i = startIndex; i < endIndex; i++)
            {
                sendVehicle(vehicles[i], writer);
            }
        });
    }

    [Obsolete]
    public void askVehicles(CSteamID steamID)
    {
    }

    internal static void SendInitialGlobalState(SteamPlayer client)
    {
        int count = vehicles.Count;
        if (count > 0)
        {
            int num = (count - 1) / 50 + 1;
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                int num3 = num2 + 50;
                askVehiclesHelper(client.transportConnection, num2, num3);
                num2 = num3;
            }
        }
        else
        {
            SendMultipleVehicles.Invoke(ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt16(0);
            });
        }
        BarricadeManager.SendVehicleRegions(client);
    }

    [Obsolete]
    public void tellEnterVehicle(CSteamID steamID, uint instanceID, byte seat, CSteamID player)
    {
        ReceiveEnterVehicle(instanceID, seat, player);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEnterVehicle")]
    public static void ReceiveEnterVehicle(uint instanceID, byte seat, CSteamID player)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                vehicles[i].addPlayer(seat, player);
                break;
            }
        }
    }

    [Obsolete]
    public void tellExitVehicle(CSteamID steamID, uint instanceID, byte seat, Vector3 point, byte angle, bool forceUpdate)
    {
        ReceiveExitVehicle(instanceID, seat, point, angle, forceUpdate);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellExitVehicle")]
    public static void ReceiveExitVehicle(uint instanceID, byte seat, Vector3 point, byte angle, bool forceUpdate)
    {
        InteractableVehicle interactableVehicle = findVehicleByNetInstanceID(instanceID);
        if (interactableVehicle != null)
        {
            interactableVehicle.removePlayer(seat, point, angle, forceUpdate);
        }
    }

    [Obsolete]
    public void tellSwapVehicle(CSteamID steamID, uint instanceID, byte fromSeat, byte toSeat)
    {
        ReceiveSwapVehicleSeats(instanceID, fromSeat, toSeat);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSwapVehicle")]
    public static void ReceiveSwapVehicleSeats(uint instanceID, byte fromSeat, byte toSeat)
    {
        InteractableVehicle interactableVehicle = findVehicleByNetInstanceID(instanceID);
        if (interactableVehicle != null)
        {
            interactableVehicle.swapPlayer(fromSeat, toSeat);
        }
    }

    public static void unlockVehicle(InteractableVehicle vehicle, Player instigatingPlayer)
    {
        if (!(vehicle == null))
        {
            bool allow = true;
            onVehicleLockpicked?.Invoke(vehicle, instigatingPlayer, ref allow);
            if (allow)
            {
                ServerSetVehicleLock(vehicle, CSteamID.Nil, CSteamID.Nil, isLocked: false);
                EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
            }
        }
    }

    public static void carjackVehicle(InteractableVehicle vehicle, Player instigatingPlayer, Vector3 force, Vector3 torque)
    {
        if (!vehicle.isEmpty)
        {
            return;
        }
        if (vehicle.asset != null)
        {
            VehiclePhysicsProfileAsset vehiclePhysicsProfileAsset = vehicle.asset.physicsProfileRef.Find();
            if (vehiclePhysicsProfileAsset != null && vehiclePhysicsProfileAsset.carjackForceMultiplier.HasValue)
            {
                force *= vehiclePhysicsProfileAsset.carjackForceMultiplier.Value;
            }
        }
        bool allow = true;
        onVehicleCarjacked?.Invoke(vehicle, instigatingPlayer, ref allow, ref force, ref torque);
        if (allow)
        {
            Rigidbody component = vehicle.GetComponent<Rigidbody>();
            if ((bool)component)
            {
                component.AddForce(force);
                component.AddTorque(torque);
            }
        }
    }

    public static ushort siphonFromVehicle(InteractableVehicle vehicle, Player instigatingPlayer, ushort desiredAmount)
    {
        bool shouldAllow = true;
        onSiphonVehicleRequested?.Invoke(vehicle, instigatingPlayer, ref shouldAllow, ref desiredAmount);
        if (!shouldAllow)
        {
            return 0;
        }
        if (desiredAmount > vehicle.fuel)
        {
            desiredAmount = vehicle.fuel;
        }
        if (desiredAmount < 1)
        {
            return 0;
        }
        vehicle.askBurnFuel(desiredAmount);
        sendVehicleFuel(vehicle, vehicle.fuel);
        return desiredAmount;
    }

    public static void ServerSetVehicleLock(InteractableVehicle vehicle, CSteamID ownerID, CSteamID groupID, bool isLocked)
    {
        SendVehicleLockState.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, ownerID, groupID, isLocked);
    }

    [Obsolete]
    public void askVehicleLock(CSteamID steamID)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveVehicleLockRequest(in context);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 4, legacyName = "askVehicleLock")]
    public static void ReceiveVehicleLockRequest(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (player == null)
        {
            return;
        }
        InteractableVehicle vehicle = player.movement.getVehicle();
        if (vehicle == null || vehicle.asset == null || !vehicle.checkDriver(player.channel.owner.playerID.steamID))
        {
            return;
        }
        bool isLocked = vehicle.isLocked;
        bool flag = vehicle.asset.canBeLocked && !isLocked;
        if (isLocked != flag)
        {
            bool shouldAllow = true;
            VehicleManager.OnToggleVehicleLockRequested?.Invoke(vehicle, ref shouldAllow);
            if (shouldAllow)
            {
                ServerSetVehicleLock(vehicle, player.channel.owner.playerID.steamID, player.quests.groupID, flag);
                EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
                VehicleManager.OnToggledVehicleLock.TryInvoke("OnToggledVehicleLock", vehicle);
            }
        }
    }

    [Obsolete]
    public void askVehicleSkin(CSteamID steamID)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveVehicleSkinRequest(in context);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askVehicleSkin")]
    public static void ReceiveVehicleSkinRequest(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (player == null)
        {
            return;
        }
        InteractableVehicle vehicle = player.movement.getVehicle();
        if (vehicle == null || !vehicle.checkDriver(player.channel.owner.playerID.steamID))
        {
            return;
        }
        int value = 0;
        ushort num = 0;
        ushort num2 = 0;
        if (player.channel.owner.skinItems != null && player.channel.owner.vehicleSkins != null && player.channel.owner.vehicleSkins.TryGetValue(vehicle.asset.sharedSkinLookupID, out value))
        {
            num = Provider.provider.economyService.getInventorySkinID(value);
            num2 = Provider.provider.economyService.getInventoryMythicID(value);
        }
        if (num != 0)
        {
            if (num == vehicle.skinID && num2 == vehicle.mythicID)
            {
                num = 0;
                num2 = 0;
            }
        }
        else if (!vehicle.isSkinned)
        {
            return;
        }
        SendVehicleSkin.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, num, num2);
    }

    [Obsolete]
    public void askVehicleHeadlights(CSteamID steamID, bool wantsHeadlightsOn)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveToggleVehicleHeadlights(in context, wantsHeadlightsOn);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = "askVehicleHeadlights")]
    public static void ReceiveToggleVehicleHeadlights(in ServerInvocationContext context, bool wantsHeadlightsOn)
    {
        Player player = context.GetPlayer();
        if (!(player == null))
        {
            InteractableVehicle vehicle = player.movement.getVehicle();
            if (!(vehicle == null) && wantsHeadlightsOn != vehicle.headlightsOn && vehicle.canTurnOnLights && vehicle.checkDriver(player.channel.owner.playerID.steamID) && vehicle.asset.hasHeadlights)
            {
                SendVehicleHeadlights.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, wantsHeadlightsOn);
                EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
            }
        }
    }

    [Obsolete]
    public void askVehicleBonus(CSteamID steamID, byte bonusType)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveUseVehicleBonus(in context, bonusType);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 5, legacyName = "askVehicleBonus")]
    public static void ReceiveUseVehicleBonus(in ServerInvocationContext context, byte bonusType)
    {
        Player player = context.GetPlayer();
        if (player == null)
        {
            return;
        }
        InteractableVehicle vehicle = player.movement.getVehicle();
        if (vehicle == null || !vehicle.checkDriver(player.channel.owner.playerID.steamID))
        {
            return;
        }
        switch (bonusType)
        {
        case 0:
            if (vehicle.canTurnOnLights)
            {
                SendVehicleSirens.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, !vehicle.sirensOn);
                EffectManager.TriggerFiremodeEffect(vehicle.transform.position);
            }
            break;
        case 1:
            vehicle.useHook();
            break;
        case 2:
            SendVehicleBlimp.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, !vehicle.isBlimpFloating);
            break;
        }
    }

    [Obsolete]
    public void askVehicleStealBattery(CSteamID steamID)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveStealVehicleBattery(in context);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askVehicleStealBattery")]
    public static void ReceiveStealVehicleBattery(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (!(player == null))
        {
            InteractableVehicle vehicle = player.movement.getVehicle();
            if (!(vehicle == null) && vehicle.checkDriver(player.channel.owner.playerID.steamID) && vehicle.usesBattery && vehicle.hasBattery && vehicle.asset.canStealBattery)
            {
                vehicle.stealBattery(player);
            }
        }
    }

    [Obsolete]
    public void askVehicleHorn(CSteamID steamID)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveVehicleHornRequest(in context);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = "askVehicleHorn")]
    public static void ReceiveVehicleHornRequest(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (!(player == null))
        {
            InteractableVehicle vehicle = player.movement.getVehicle();
            if (!(vehicle == null) && vehicle.asset.hasHorn && vehicle.canUseHorn && vehicle.checkDriver(player.channel.owner.playerID.steamID))
            {
                SendVehicleHorn.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);
            }
        }
    }

    [Obsolete]
    public void askEnterVehicle(CSteamID steamID, uint instanceID, byte[] hash, byte engine)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveEnterVehicleRequest(in context, instanceID, hash, new byte[0], engine);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askEnterVehicle")]
    public static void ReceiveEnterVehicleRequest(in ServerInvocationContext context, uint instanceID, byte[] hash, byte[] physicsProfileHash, byte engine)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || player.equipment.isBusy || (LevelManager.isArenaMode && !LevelManager.isPlayerInArena(player)) || (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished) || player.movement.getVehicle() != null)
        {
            return;
        }
        InteractableVehicle interactableVehicle = null;
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i].instanceID == instanceID)
            {
                interactableVehicle = vehicles[i];
                break;
            }
        }
        if (interactableVehicle == null || (interactableVehicle.asset.shouldVerifyHash && !Hash.verifyHash(hash, interactableVehicle.asset.hash)))
        {
            return;
        }
        if (physicsProfileHash.Length == 0)
        {
            if (interactableVehicle.asset.physicsProfileRef.Find() != null)
            {
                return;
            }
        }
        else
        {
            if (physicsProfileHash.Length != 20)
            {
                context.Kick("invalid vehicle physics profile hash");
                return;
            }
            VehiclePhysicsProfileAsset vehiclePhysicsProfileAsset = interactableVehicle.asset.physicsProfileRef.Find();
            if (vehiclePhysicsProfileAsset == null || !Hash.verifyHash(physicsProfileHash, vehiclePhysicsProfileAsset.hash))
            {
                return;
            }
        }
        if ((EEngine)engine != interactableVehicle.asset.engine || (interactableVehicle.transform.position - player.transform.position).sqrMagnitude > 100f || !interactableVehicle.checkEnter(player) || !interactableVehicle.tryAddPlayer(out var seat, player))
        {
            return;
        }
        Transform seat2 = interactableVehicle.passengers[seat].seat;
        Vector3 position = seat2.position;
        Vector3 end = seat2.position + seat2.up * 2f;
        Vector3 start = player.transform.position + Vector3.up;
        RaycastHit hitInfo;
        bool flag = Physics.Linecast(start, position, out hitInfo, RayMasks.BLOCK_ENTRY, QueryTriggerInteraction.Ignore);
        if (!flag)
        {
            flag = Physics.Linecast(start, end, out hitInfo, RayMasks.BLOCK_ENTRY, QueryTriggerInteraction.Ignore);
        }
        if (flag && !hitInfo.transform.IsChildOf(interactableVehicle.transform))
        {
            return;
        }
        if (VehicleManager.onEnterVehicleRequested != null)
        {
            bool shouldAllow = true;
            VehicleManager.onEnterVehicleRequested(player, interactableVehicle, ref shouldAllow);
            if (!shouldAllow)
            {
                return;
            }
        }
        SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), instanceID, seat, player.channel.owner.playerID.steamID);
    }

    public static bool ServerForcePassengerIntoVehicle(Player player, InteractableVehicle vehicle)
    {
        if (player == null)
        {
            throw new ArgumentNullException("player");
        }
        if (vehicle == null)
        {
            throw new ArgumentNullException("vehicle");
        }
        if (player.life.isDead)
        {
            return false;
        }
        if (player.equipment.isBusy)
        {
            return false;
        }
        if (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished)
        {
            return false;
        }
        if (player.movement.getVehicle() != null)
        {
            return false;
        }
        if (!vehicle.tryAddPlayer(out var seat, player))
        {
            return false;
        }
        SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, seat, player.channel.owner.playerID.steamID);
        return true;
    }

    [Obsolete]
    public void askExitVehicle(CSteamID steamID, Vector3 velocity)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveExitVehicleRequest(in context, velocity);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askExitVehicle")]
    public static void ReceiveExitVehicleRequest(in ServerInvocationContext context, Vector3 velocity)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || player.equipment.isBusy)
        {
            return;
        }
        InteractableVehicle vehicle = player.movement.getVehicle();
        if (vehicle == null || !vehicle.forceRemovePlayer(out var seat, player.channel.owner.playerID.steamID, out var pendingLocation, out var angle))
        {
            return;
        }
        if (VehicleManager.onExitVehicleRequested != null)
        {
            bool shouldAllow = true;
            float pendingYaw = MeasurementTool.byteToAngle(angle);
            VehicleManager.onExitVehicleRequested(player, vehicle, ref shouldAllow, ref pendingLocation, ref pendingYaw);
            angle = MeasurementTool.angleToByte(pendingYaw);
            if (!shouldAllow)
            {
                return;
            }
        }
        sendExitVehicle(vehicle, seat, pendingLocation, angle, forceUpdate: false);
        if (seat == 0 && Dedicator.IsDedicatedServer)
        {
            vehicle.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

    public static void forceRemovePlayer(InteractableVehicle vehicle, CSteamID player)
    {
        if (vehicle.forceRemovePlayer(out var seat, player, out var point, out var angle))
        {
            sendExitVehicle(vehicle, seat, point, angle, forceUpdate: true);
        }
    }

    public static bool forceRemovePlayer(CSteamID player)
    {
        InteractableVehicle interactableVehicle = null;
        byte seat = 0;
        Vector3 point = Vector3.zero;
        byte angle = 0;
        foreach (InteractableVehicle vehicle in vehicles)
        {
            if (!(vehicle == null) && vehicle.forceRemovePlayer(out seat, player, out point, out angle))
            {
                interactableVehicle = vehicle;
                break;
            }
        }
        if (interactableVehicle != null)
        {
            sendExitVehicle(interactableVehicle, seat, point, angle, forceUpdate: true);
            return true;
        }
        return false;
    }

    public static bool removePlayerTeleportUnsafe(InteractableVehicle vehicle, Player player, Vector3 position, float yaw)
    {
        if (vehicle.findPlayerSeat(player, out var seat))
        {
            byte angle = MeasurementTool.angleToByte(yaw);
            sendExitVehicle(vehicle, seat, position, angle, forceUpdate: false);
            return true;
        }
        return false;
    }

    [Obsolete]
    public void askSwapVehicle(CSteamID steamID, byte toSeat)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveSwapVehicleRequest(in context, toSeat);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = "askSwapVehicle")]
    public static void ReceiveSwapVehicleRequest(in ServerInvocationContext context, byte toSeat)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || player.equipment.isBusy || (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished))
        {
            return;
        }
        InteractableVehicle vehicle = player.movement.getVehicle();
        if (vehicle == null || Time.realtimeSinceStartup - vehicle.lastSeat < 1f)
        {
            return;
        }
        vehicle.lastSeat = Time.realtimeSinceStartup;
        if (!vehicle.trySwapPlayer(player, toSeat, out var fromSeat))
        {
            return;
        }
        if (VehicleManager.onSwapSeatRequested != null)
        {
            bool shouldAllow = true;
            VehicleManager.onSwapSeatRequested(player, vehicle, ref shouldAllow, fromSeat, ref toSeat);
            if (!shouldAllow || !vehicle.trySwapPlayer(player, toSeat, out fromSeat))
            {
                return;
            }
        }
        SendSwapVehicleSeats.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, fromSeat, toSeat);
    }

    public static void sendExitVehicle(InteractableVehicle vehicle, byte seat, Vector3 point, byte angle, bool forceUpdate)
    {
        SendExitVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, seat, point, angle, forceUpdate);
    }

    public static void sendVehicleFuel(InteractableVehicle vehicle, ushort newFuel)
    {
        SendVehicleFuel.Invoke(ENetReliability.Unreliable, Provider.GatherClientConnections(), vehicle.instanceID, newFuel);
    }

    public static void sendVehicleBatteryCharge(InteractableVehicle vehicle, ushort newBatteryCharge)
    {
        SendVehicleBatteryCharge.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newBatteryCharge);
    }

    public static void sendVehicleTireAliveMask(InteractableVehicle vehicle, byte newTireAliveMask)
    {
        SendVehicleTireAliveMask.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newTireAliveMask);
    }

    public static void sendVehicleExploded(InteractableVehicle vehicle)
    {
        SendVehicleExploded.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID);
        OnVehicleExploded.TryInvoke("OnVehicleExploded", vehicle);
    }

    public static void sendVehicleHealth(InteractableVehicle vehicle, ushort newHealth)
    {
        SendVehicleHealth.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, newHealth);
    }

    public static void sendVehicleRecov(InteractableVehicle vehicle, Vector3 newPosition, int newRecov)
    {
        if (vehicle.passengers[0].player != null)
        {
            SendVehicleRecov.Invoke(ENetReliability.Reliable, vehicle.passengers[0].player.transportConnection, vehicle.instanceID, newPosition, newRecov);
        }
    }

    private InteractableVehicle addVehicle(Guid assetGuid, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, CSteamID[] passengers, byte[][] turrets, uint instanceID, byte tireAliveMask, NetId netId)
    {
        VehicleAsset vehicleAsset = Assets.find(assetGuid) as VehicleAsset;
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(assetGuid, vehicleAsset, "Vehicle");
        }
        if (vehicleAsset == null)
        {
            return null;
        }
        GameObject orLoadModel = vehicleAsset.GetOrLoadModel();
        if (orLoadModel == null)
        {
            Assets.reportError(vehicleAsset, "unable to spawn any gameobject");
            return null;
        }
        InteractableVehicle interactableVehicle = null;
        try
        {
            Transform obj = UnityEngine.Object.Instantiate(orLoadModel, point, angle).transform;
            obj.name = vehicleAsset.id.ToString();
            Rigidbody orAddComponent = obj.GetOrAddComponent<Rigidbody>();
            orAddComponent.useGravity = true;
            orAddComponent.isKinematic = false;
            interactableVehicle = obj.gameObject.AddComponent<InteractableVehicle>();
            interactableVehicle.roadPosition = roadPosition;
            interactableVehicle.instanceID = instanceID;
            interactableVehicle.AssignNetId(netId);
            interactableVehicle.id = vehicleAsset.id;
            interactableVehicle.skinID = skinID;
            interactableVehicle.mythicID = mythicID;
            interactableVehicle.fuel = fuel;
            interactableVehicle.isExploded = isExploded;
            interactableVehicle.health = health;
            interactableVehicle.batteryCharge = batteryCharge;
            interactableVehicle.init(vehicleAsset);
            interactableVehicle.gatherVehicleColliders();
            interactableVehicle.tellSirens(sirens);
            interactableVehicle.tellBlimp(blimp);
            interactableVehicle.tellHeadlights(headlights);
            interactableVehicle.tellTaillights(taillights);
            interactableVehicle.tellLocked(owner, group, locked);
            interactableVehicle.tireAliveMask = tireAliveMask;
            if (Provider.isServer)
            {
                if (turrets != null && turrets.Length == interactableVehicle.turrets.Length)
                {
                    for (byte b = 0; b < interactableVehicle.turrets.Length; b = (byte)(b + 1))
                    {
                        interactableVehicle.turrets[b].state = turrets[b];
                    }
                }
                else
                {
                    for (byte b2 = 0; b2 < interactableVehicle.turrets.Length; b2 = (byte)(b2 + 1))
                    {
                        if (Assets.find(EAssetType.ITEM, vehicleAsset.turrets[b2].itemID) is ItemAsset itemAsset)
                        {
                            interactableVehicle.turrets[b2].state = itemAsset.getState();
                        }
                        else
                        {
                            interactableVehicle.turrets[b2].state = null;
                        }
                    }
                }
            }
            if (passengers != null)
            {
                for (byte b3 = 0; b3 < passengers.Length; b3 = (byte)(b3 + 1))
                {
                    if (passengers[b3] != CSteamID.Nil)
                    {
                        interactableVehicle.addPlayer(b3, passengers[b3]);
                    }
                }
            }
            if (vehicleAsset.trunkStorage_Y > 0)
            {
                interactableVehicle.trunkItems = new Items(PlayerInventory.STORAGE);
                interactableVehicle.trunkItems.resize(vehicleAsset.trunkStorage_X, vehicleAsset.trunkStorage_Y);
            }
            vehicles.Add(interactableVehicle);
            NetIdRegistry.AssignTransform(++netId, interactableVehicle.transform);
            BarricadeManager.registerVehicleRegion(interactableVehicle.transform, interactableVehicle, 0, ++netId);
            if (interactableVehicle.trainCars != null)
            {
                for (int i = 1; i < interactableVehicle.trainCars.Length; i++)
                {
                    BarricadeManager.registerVehicleRegion(interactableVehicle.trainCars[i].root, interactableVehicle, i, ++netId);
                }
                return interactableVehicle;
            }
            return interactableVehicle;
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Exception while spawning vehicle: {0}", vehicleAsset.name);
            UnturnedLog.exception(e);
            return interactableVehicle;
        }
    }

    private bool canUseSpawnpoint(VehicleSpawnpoint spawn)
    {
        foreach (InteractableVehicle vehicle in vehicles)
        {
            if (!(vehicle == null) && (vehicle.transform.position - spawn.point).sqrMagnitude < 64f)
            {
                return false;
            }
        }
        return true;
    }

    private VehicleSpawnpoint findRandomSpawn()
    {
        List<VehicleSpawnpoint> spawns = LevelVehicles.spawns;
        if (spawns.Count < 1)
        {
            return null;
        }
        int index = UnityEngine.Random.Range(0, spawns.Count);
        VehicleSpawnpoint vehicleSpawnpoint = spawns[index];
        if (vehicleSpawnpoint != null && canUseSpawnpoint(vehicleSpawnpoint))
        {
            return vehicleSpawnpoint;
        }
        return null;
    }

    private InteractableVehicle addVehicleAtSpawn(VehicleSpawnpoint spawn)
    {
        if (spawn == null)
        {
            return null;
        }
        ushort vehicle = LevelVehicles.getVehicle(spawn);
        if (vehicle == 0)
        {
            return null;
        }
        if (!(Assets.find(EAssetType.VEHICLE, vehicle) is VehicleAsset vehicleAsset))
        {
            return null;
        }
        Vector3 point = spawn.point;
        point.y += 0.5f;
        NetId netId = NetIdRegistry.ClaimBlock(21u);
        return addVehicle(vehicleAsset.GUID, 0, 0, 0f, point, Quaternion.Euler(0f, spawn.angle, 0f), sirens: false, blimp: false, headlights: false, taillights: false, ushort.MaxValue, isExploded: false, ushort.MaxValue, ushort.MaxValue, CSteamID.Nil, CSteamID.Nil, locked: false, null, null, allocateInstanceID(), getVehicleRandomTireAliveMask(vehicleAsset), netId);
    }

    private void addVehicleAtSpawnAndReplicate(VehicleSpawnpoint spawn)
    {
        InteractableVehicle character = addVehicleAtSpawn(spawn);
        if (character != null)
        {
            SendSingleVehicle.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), delegate(NetPakWriter writer)
            {
                sendVehicle(character, writer);
            });
        }
    }

    private bool respawnVehicles_Destroy()
    {
        if (respawnVehicleIndex >= vehicles.Count)
        {
            respawnVehicleIndex = (ushort)(vehicles.Count - 1);
        }
        InteractableVehicle interactableVehicle = vehicles[respawnVehicleIndex];
        respawnVehicleIndex++;
        if (respawnVehicleIndex >= vehicles.Count)
        {
            respawnVehicleIndex = 0;
        }
        if (interactableVehicle == null || interactableVehicle.asset == null)
        {
            return false;
        }
        if (interactableVehicle.asset.engine == EEngine.TRAIN)
        {
            return false;
        }
        if (!interactableVehicle.isEmpty)
        {
            return false;
        }
        float respawn_Time = Provider.modeConfigData.Vehicles.Respawn_Time;
        if (false | (interactableVehicle.isExploded && Time.realtimeSinceStartup - interactableVehicle.lastExploded > respawn_Time) | (interactableVehicle.isDrowned && Time.realtimeSinceStartup - interactableVehicle.lastUnderwater > respawn_Time))
        {
            askVehicleDestroy(interactableVehicle);
            return true;
        }
        return false;
    }

    private void despawnAndRespawnVehicles()
    {
        if (Level.info != null && Level.info.type != ELevelType.ARENA && vehicles != null && (vehicles.Count <= 0 || !respawnVehicles_Destroy()) && LevelVehicles.spawns != null && LevelVehicles.spawns.Count != 0 && vehicles.Count < maxInstances)
        {
            VehicleSpawnpoint vehicleSpawnpoint = findRandomSpawn();
            if (vehicleSpawnpoint != null)
            {
                addVehicleAtSpawnAndReplicate(vehicleSpawnpoint);
            }
        }
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        seq = 0u;
        _vehicles = new List<InteractableVehicle>();
        highestInstanceID = 0u;
        respawnVehicleIndex = 0;
        BarricadeManager.clearPlants();
        if (!Provider.isServer)
        {
            return;
        }
        enableDecayUpdate = Provider.modeConfigData.Vehicles.Decay_Time > 0f;
        if (!enableDecayUpdate)
        {
            UnturnedLog.info("Disabling vehicle decay because Decay_Time is negative");
        }
        if (Level.info != null && Level.info.type != ELevelType.ARENA)
        {
            load();
            if (LevelVehicles.spawns.Count > 0)
            {
                List<VehicleSpawnpoint> list = new List<VehicleSpawnpoint>();
                for (int i = 0; i < LevelVehicles.spawns.Count; i++)
                {
                    list.Add(LevelVehicles.spawns[i]);
                }
                while (vehicles.Count < maxInstances && list.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    VehicleSpawnpoint spawn = list[index];
                    list.RemoveAt(index);
                    if (canUseSpawnpoint(spawn))
                    {
                        addVehicleAtSpawn(spawn);
                    }
                }
            }
            foreach (LevelTrainAssociation train in Level.info.configData.Trains)
            {
                bool flag = false;
                foreach (InteractableVehicle vehicle in vehicles)
                {
                    if (vehicle.id == train.VehicleID)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }
                Road road = LevelRoads.getRoad(train.RoadIndex);
                if (road == null)
                {
                    UnturnedLog.error("Failed to find track " + train.RoadIndex + " for train " + train.VehicleID + "!");
                    continue;
                }
                float trackSampledLength = road.trackSampledLength;
                float num = UnityEngine.Random.Range(train.Min_Spawn_Placement, train.Max_Spawn_Placement);
                float roadPosition = trackSampledLength * num;
                if (Assets.find(EAssetType.VEHICLE, train.VehicleID) is VehicleAsset vehicleAsset)
                {
                    NetId netId = NetIdRegistry.ClaimBlock(21u);
                    addVehicle(vehicleAsset.GUID, 0, 0, roadPosition, Vector3.zero, Quaternion.identity, sirens: false, blimp: false, headlights: false, taillights: false, ushort.MaxValue, isExploded: false, ushort.MaxValue, ushort.MaxValue, CSteamID.Nil, CSteamID.Nil, locked: false, null, null, allocateInstanceID(), getVehicleRandomTireAliveMask(vehicleAsset), netId);
                }
                else if ((bool)Assets.shouldLoadAnyAssets)
                {
                    UnturnedLog.error("Failed to find asset for train " + train.VehicleID + "!");
                }
            }
        }
        else
        {
            Level.isLoadingVehicles = false;
        }
        if (vehicles == null)
        {
            return;
        }
        for (int j = 0; j < vehicles.Count; j++)
        {
            if (vehicles[j] != null)
            {
                Rigidbody component = vehicles[j].GetComponent<Rigidbody>();
                if (component != null)
                {
                    component.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
        }
    }

    private void onPostLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP || !Provider.isServer)
        {
            return;
        }
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i] != null)
            {
                Rigidbody component = vehicles[i].GetComponent<Rigidbody>();
                if (component != null)
                {
                    component.constraints = RigidbodyConstraints.None;
                }
            }
        }
    }

    private void onServerDisconnected(CSteamID player)
    {
        if (Provider.isServer)
        {
            forceRemovePlayer(player);
        }
    }

    private void sendVehicleStates()
    {
        seq++;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (steamPlayer == null || steamPlayer.player == null)
            {
                continue;
            }
            ushort updateCount = 0;
            vehiclesToSend.Clear();
            for (int j = 0; j < vehicles.Count; j++)
            {
                InteractableVehicle interactableVehicle = vehicles[j];
                if (!(interactableVehicle == null) && interactableVehicle.updates != null && interactableVehicle.updates.Count != 0 && !interactableVehicle.checkDriver(steamPlayer.playerID.steamID))
                {
                    vehiclesToSend.Add(interactableVehicle);
                    updateCount += (ushort)interactableVehicle.updates.Count;
                }
            }
            if (updateCount == 0)
            {
                continue;
            }
            SendVehicleStates.Invoke(ENetReliability.Unreliable, steamPlayer.transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt32(seq);
                writer.WriteUInt16(updateCount);
                foreach (InteractableVehicle item in vehiclesToSend)
                {
                    for (int l = 0; l < item.updates.Count; l++)
                    {
                        VehicleStateUpdate vehicleStateUpdate = item.updates[l];
                        writer.WriteUInt32(item.instanceID);
                        writer.WriteClampedVector3(vehicleStateUpdate.pos, 13, 8);
                        writer.WriteQuaternion(vehicleStateUpdate.rot, 11);
                        writer.WriteUInt8((byte)(Mathf.Clamp(item.speed, -100f, 100f) + 128f));
                        writer.WriteUInt8((byte)(Mathf.Clamp(item.physicsSpeed, -100f, 100f) + 128f));
                        writer.WriteUInt8((byte)(item.turn + 1));
                    }
                }
                if (writer.errors != 0 && Time.realtimeSinceStartup - lastSendOverflowWarning > 1f)
                {
                    lastSendOverflowWarning = Time.realtimeSinceStartup;
                    CommandWindow.LogWarningFormat("Error {0} writing vehicle states. The vehicle count ({1}) is probably too high. No this is not a bug introduced in the update, rather a warning of a previously silent bug.", writer.errors, _vehicles.Count);
                }
            });
        }
        for (int k = 0; k < vehicles.Count; k++)
        {
            InteractableVehicle interactableVehicle2 = vehicles[k];
            if (!(interactableVehicle2 == null) && interactableVehicle2.updates != null && interactableVehicle2.updates.Count != 0)
            {
                interactableVehicle2.updates.Clear();
            }
        }
    }

    private void Update()
    {
        if (!Provider.isServer || !Level.isLoaded || vehicles == null)
        {
            return;
        }
        if (vehicles.Count > 0 && Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
        {
            lastTick += Provider.UPDATE_TIME;
            if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
            {
                lastTick = Time.realtimeSinceStartup;
            }
            sendVehicleStates();
        }
        despawnAndRespawnVehicles();
        if (enableDecayUpdate && _vehicles.Count > 0)
        {
            UpdateDecay();
        }
    }

    private void Start()
    {
        manager = this;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
        Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(onPostLevelLoaded));
        Provider.onServerDisconnected = (Provider.ServerDisconnected)Delegate.Combine(Provider.onServerDisconnected, new Provider.ServerDisconnected(onServerDisconnected));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Vehicles: {vehicles.Count}");
    }

    public static void load()
    {
        uint num = 0u;
        if (LevelSavedata.fileExists("/Vehicles.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            River river = LevelSavedata.openRiver("/Vehicles.dat", isReading: true);
            byte b = river.readByte();
            if (b > 2)
            {
                ushort num2 = river.readUInt16();
                for (ushort num3 = 0; num3 < num2; num3 = (ushort)(num3 + 1))
                {
                    VehicleAsset vehicleAsset;
                    if (b < 14)
                    {
                        ushort id = river.readUInt16();
                        vehicleAsset = Assets.find(EAssetType.VEHICLE, id) as VehicleAsset;
                    }
                    else
                    {
                        vehicleAsset = Assets.find(river.readGUID()) as VehicleAsset;
                    }
                    uint num4;
                    if (b < 12)
                    {
                        num4 = allocateInstanceID();
                    }
                    else
                    {
                        num4 = river.readUInt32();
                        if (num4 > num)
                        {
                            num = num4;
                        }
                    }
                    ushort skinID = (ushort)((b >= 8) ? river.readUInt16() : 0);
                    ushort mythicID = (ushort)((b >= 9) ? river.readUInt16() : 0);
                    float roadPosition = ((b >= 10) ? river.readSingle() : 0f);
                    Vector3 point = river.readSingleVector3();
                    Quaternion angle = river.readSingleQuaternion();
                    ushort fuel = river.readUInt16();
                    ushort health = river.readUInt16();
                    ushort batteryCharge = 10000;
                    if (b > 5)
                    {
                        batteryCharge = river.readUInt16();
                    }
                    Guid batteryItemGuid = ((b < 15) ? Guid.Empty : river.readGUID());
                    byte tireAliveMask = byte.MaxValue;
                    if (b > 6)
                    {
                        tireAliveMask = river.readByte();
                    }
                    CSteamID owner = CSteamID.Nil;
                    CSteamID group = CSteamID.Nil;
                    bool locked = false;
                    if (b > 4)
                    {
                        owner = river.readSteamID();
                        group = river.readSteamID();
                        locked = river.readBoolean();
                    }
                    byte[][] array = null;
                    if (b > 3)
                    {
                        array = new byte[river.readByte()][];
                        for (byte b2 = 0; b2 < array.Length; b2 = (byte)(b2 + 1))
                        {
                            array[b2] = river.readBytes();
                        }
                    }
                    point.y += 0.02f;
                    bool flag = b >= 11 && river.readBoolean();
                    ItemJar[] array2 = null;
                    if (flag)
                    {
                        array2 = new ItemJar[river.readByte()];
                        for (byte b3 = 0; b3 < array2.Length; b3 = (byte)(b3 + 1))
                        {
                            byte new_x = river.readByte();
                            byte new_y = river.readByte();
                            byte newRot = river.readByte();
                            ushort num5 = river.readUInt16();
                            byte newAmount = river.readByte();
                            byte newQuality = river.readByte();
                            byte[] newState = river.readBytes();
                            if (Assets.find(EAssetType.ITEM, num5) is ItemAsset)
                            {
                                Item newItem = new Item(num5, newAmount, newQuality, newState);
                                array2[b3] = new ItemJar(new_x, new_y, newRot, newItem);
                            }
                        }
                    }
                    float decayTimer = ((b < 13) ? 0f : river.readSingle());
                    if (vehicleAsset != null)
                    {
                        NetId netId = NetIdRegistry.ClaimBlock(21u);
                        InteractableVehicle interactableVehicle = manager.addVehicle(vehicleAsset.GUID, skinID, mythicID, roadPosition, point, angle, sirens: false, blimp: false, headlights: false, taillights: false, fuel, isExploded: false, health, batteryCharge, owner, group, locked, null, array, num4, tireAliveMask, netId);
                        if (interactableVehicle != null)
                        {
                            interactableVehicle.batteryItemGuid = batteryItemGuid;
                            if (flag && array2 != null && array2.Length != 0 && interactableVehicle.trunkItems != null && interactableVehicle.trunkItems.height > 0)
                            {
                                for (byte b4 = 0; b4 < array2.Length; b4 = (byte)(b4 + 1))
                                {
                                    ItemJar itemJar = array2[b4];
                                    if (itemJar != null)
                                    {
                                        interactableVehicle.trunkItems.loadItem(itemJar.x, itemJar.y, itemJar.rot, itemJar.item);
                                    }
                                }
                            }
                            interactableVehicle.decayTimer = decayTimer;
                        }
                    }
                }
            }
            else
            {
                ushort num6 = river.readUInt16();
                for (ushort num7 = 0; num7 < num6; num7 = (ushort)(num7 + 1))
                {
                    river.readUInt16();
                    river.readColor();
                    Vector3 point2 = river.readSingleVector3();
                    Quaternion angle2 = river.readSingleQuaternion();
                    ushort fuel2 = river.readUInt16();
                    ushort health2 = ushort.MaxValue;
                    ushort batteryCharge2 = ushort.MaxValue;
                    byte tireAliveMask2 = byte.MaxValue;
                    ushort id2 = (ushort)UnityEngine.Random.Range(1, 51);
                    if (b > 1)
                    {
                        health2 = river.readUInt16();
                    }
                    point2.y += 0.02f;
                    if (Assets.find(EAssetType.VEHICLE, id2) is VehicleAsset vehicleAsset2)
                    {
                        NetId netId2 = NetIdRegistry.ClaimBlock(21u);
                        manager.addVehicle(vehicleAsset2.GUID, 0, 0, 0f, point2, angle2, sirens: false, blimp: false, headlights: false, taillights: false, fuel2, isExploded: false, health2, batteryCharge2, CSteamID.Nil, CSteamID.Nil, locked: false, null, null, allocateInstanceID(), tireAliveMask2, netId2);
                    }
                }
            }
            river.closeRiver();
        }
        if (num > highestInstanceID)
        {
            highestInstanceID = num;
        }
        Level.isLoadingVehicles = false;
    }

    public static void save()
    {
        River river = LevelSavedata.openRiver("/Vehicles.dat", isReading: false);
        river.writeByte(15);
        ushort num = 0;
        for (ushort num2 = 0; num2 < vehicles.Count; num2 = (ushort)(num2 + 1))
        {
            InteractableVehicle interactableVehicle = vehicles[num2];
            if (!(interactableVehicle == null) && !(interactableVehicle.transform == null) && !interactableVehicle.isAutoClearable)
            {
                num = (ushort)(num + 1);
            }
        }
        river.writeUInt16(num);
        for (ushort num3 = 0; num3 < vehicles.Count; num3 = (ushort)(num3 + 1))
        {
            InteractableVehicle interactableVehicle2 = vehicles[num3];
            if (!(interactableVehicle2 == null) && !(interactableVehicle2.transform == null) && !interactableVehicle2.isAutoClearable)
            {
                Vector3 vector = interactableVehicle2.transform.position;
                if (!vector.IsFinite())
                {
                    vector = new Vector3(0f, Level.HEIGHT - 50f, 0f);
                }
                else if (vector.y > Level.HEIGHT)
                {
                    vector.y = Level.HEIGHT - 50f;
                }
                river.writeGUID(interactableVehicle2.asset.GUID);
                river.writeUInt32(interactableVehicle2.instanceID);
                river.writeUInt16(interactableVehicle2.skinID);
                river.writeUInt16(interactableVehicle2.mythicID);
                river.writeSingle(interactableVehicle2.roadPosition);
                river.writeSingleVector3(vector);
                river.writeSingleQuaternion(interactableVehicle2.transform.rotation);
                river.writeUInt16(interactableVehicle2.fuel);
                river.writeUInt16(interactableVehicle2.health);
                river.writeUInt16(interactableVehicle2.batteryCharge);
                river.writeGUID(interactableVehicle2.batteryItemGuid);
                river.writeByte(interactableVehicle2.tireAliveMask);
                river.writeSteamID(interactableVehicle2.lockedOwner);
                river.writeSteamID(interactableVehicle2.lockedGroup);
                river.writeBoolean(interactableVehicle2.isLocked);
                if (interactableVehicle2.turrets != null)
                {
                    byte b = (byte)interactableVehicle2.turrets.Length;
                    river.writeByte(b);
                    for (byte b2 = 0; b2 < b; b2 = (byte)(b2 + 1))
                    {
                        Passenger passenger = interactableVehicle2.turrets[b2];
                        if (passenger != null && passenger.state != null)
                        {
                            river.writeBytes(passenger.state);
                        }
                        else
                        {
                            river.writeBytes(new byte[0]);
                        }
                    }
                }
                else
                {
                    river.writeByte(0);
                }
                if (interactableVehicle2.trunkItems != null && interactableVehicle2.trunkItems.height > 0)
                {
                    river.writeBoolean(value: true);
                    byte itemCount = interactableVehicle2.trunkItems.getItemCount();
                    river.writeByte(itemCount);
                    for (byte b3 = 0; b3 < itemCount; b3 = (byte)(b3 + 1))
                    {
                        ItemJar item = interactableVehicle2.trunkItems.getItem(b3);
                        river.writeByte(item?.x ?? 0);
                        river.writeByte(item?.y ?? 0);
                        river.writeByte(item?.rot ?? 0);
                        river.writeUInt16(item?.item.id ?? 0);
                        river.writeByte(item?.item.amount ?? 0);
                        river.writeByte(item?.item.quality ?? 0);
                        river.writeBytes((item != null) ? item.item.state : new byte[0]);
                    }
                }
                else
                {
                    river.writeBoolean(value: false);
                }
                river.writeSingle(interactableVehicle2.decayTimer);
            }
        }
        river.closeRiver();
    }

    private void UpdateDecay()
    {
        decayUpdateIndex = (decayUpdateIndex + 1) % _vehicles.Count;
        InteractableVehicle interactableVehicle = _vehicles[decayUpdateIndex];
        if (interactableVehicle == null || interactableVehicle.asset == null || !interactableVehicle.asset.CanDecay)
        {
            return;
        }
        float num = Time.time - interactableVehicle.decayLastUpdateTime;
        interactableVehicle.decayLastUpdateTime = Time.time;
        if (interactableVehicle.isDriven && (interactableVehicle.transform.position - interactableVehicle.decayLastUpdatePosition).sqrMagnitude > 1f)
        {
            interactableVehicle.ResetDecayTimer();
            return;
        }
        interactableVehicle.decayTimer += num;
        if (interactableVehicle.decayTimer > Provider.modeConfigData.Vehicles.Decay_Time)
        {
            interactableVehicle.decayPendingDamage += Provider.modeConfigData.Vehicles.Decay_Damage_Per_Second * num;
            int num2 = Mathf.FloorToInt(interactableVehicle.decayPendingDamage);
            if (num2 > 0)
            {
                interactableVehicle.decayPendingDamage -= num2;
                damage(interactableVehicle, num2, 1f, canRepair: true, CSteamID.Nil, EDamageOrigin.VehicleDecay);
            }
        }
    }
}
