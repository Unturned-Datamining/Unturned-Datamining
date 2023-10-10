using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class BarricadeManager : SteamCaller
{
    private static Collider[] checkColliders = new Collider[2];

    public const byte SAVEDATA_VERSION_INCLUDE_BUILD_ENUM = 18;

    private const byte SAVEDATA_VERSION_NEWEST = 18;

    public static readonly byte SAVEDATA_VERSION = 18;

    public static readonly byte BARRICADE_REGIONS = 2;

    public static DeployBarricadeRequestHandler onDeployBarricadeRequested;

    [Obsolete("Please use BarricadeDrop.OnSalvageRequested_Global instead")]
    public static SalvageBarricadeRequestHandler onSalvageBarricadeRequested;

    public static DamageBarricadeRequestHandler onDamageBarricadeRequested;

    [Obsolete("Please use InteractableFarm.OnHarvestRequested_Global instead")]
    public static SalvageBarricadeRequestHandler onHarvestPlantRequested;

    public static BarricadeSpawnedHandler onBarricadeSpawned;

    public static ModifySignRequestHandler onModifySignRequested;

    public static OpenStorageRequestHandler onOpenStorageRequested;

    public static TransformBarricadeRequestHandler onTransformRequested;

    private static BarricadeManager manager;

    public static byte version = SAVEDATA_VERSION;

    private static List<VehicleBarricadeRegion> internalVehicleRegions;

    private static List<BarricadeRegion> backwardsCompatVehicleRegions;

    private static List<BarricadeInstantiationParameters> pendingInstantiations;

    private static List<BarricadeInstantiationParameters> instantiationsToInsert;

    private static List<BarricadeRegion> regionsPendingDestroy;

    private static List<Collider> barricadeColliders;

    private static uint instanceCount;

    private static uint serverActiveDate;

    private static readonly ClientStaticMethod<NetId> SendDestroyBarricade = ClientStaticMethod<NetId>.Get(ReceiveDestroyBarricade);

    private static readonly ClientStaticMethod<byte, byte> SendClearRegionBarricades = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionBarricades);

    private static readonly ClientStaticMethod<NetId, Guid, byte[], Vector3, byte, byte, byte, ulong, ulong, NetId> SendSingleBarricade = ClientStaticMethod<NetId, Guid, byte[], Vector3, byte, byte, byte, ulong, ulong, NetId>.Get(ReceiveSingleBarricade);

    private static readonly ClientStaticMethod SendMultipleBarricades = ClientStaticMethod.Get(ReceiveMultipleBarricades);

    private Dictionary<int, Stack<GameObject>> pool;

    private Stopwatch instantiationTimer = new Stopwatch();

    private const int MIN_INSTANTIATIONS_PER_FRAME = 5;

    private const int MIN_DESTROY_PER_FRAME = 10;

    internal const int POSITION_FRAC_BIT_COUNT = 11;

    internal const int NETIDS_PER_BARRICADE = 3;

    public static BarricadeManager instance => manager;

    public static BarricadeRegion[,] regions { get; private set; }

    public static BarricadeRegion[,] BarricadeRegions
    {
        get
        {
            return regions;
        }
        set
        {
            regions = value;
        }
    }

    public static IReadOnlyList<VehicleBarricadeRegion> vehicleRegions { get; private set; }

    [Obsolete("Please update to vehicleRegions instead (this property copies the list)")]
    public static List<BarricadeRegion> plants
    {
        get
        {
            if (backwardsCompatVehicleRegions == null)
            {
                backwardsCompatVehicleRegions = new List<BarricadeRegion>(vehicleRegions);
            }
            return backwardsCompatVehicleRegions;
        }
    }

    public static event RepairBarricadeRequestHandler OnRepairRequested;

    public static event RepairedBarricadeHandler OnRepaired;

    public static void getBarricadesInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
    {
        if (regions == null)
        {
            return;
        }
        for (int i = 0; i < search.Count; i++)
        {
            RegionCoordinate regionCoordinate = search[i];
            if (regions[regionCoordinate.x, regionCoordinate.y] == null)
            {
                continue;
            }
            foreach (BarricadeDrop drop in regions[regionCoordinate.x, regionCoordinate.y].drops)
            {
                Transform model = drop.model;
                if (!(model == null) && (model.position - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(model);
                }
            }
        }
    }

    public static void getBarricadesInRadius(Vector3 center, float sqrRadius, ushort plant, List<Transform> result)
    {
        if (vehicleRegions == null || plant >= vehicleRegions.Count)
        {
            return;
        }
        foreach (BarricadeDrop drop in vehicleRegions[plant].drops)
        {
            Transform model = drop.model;
            if (!(model == null) && (model.position - center).sqrMagnitude < sqrRadius)
            {
                result.Add(model);
            }
        }
    }

    public static void getBarricadesInRadius(Vector3 center, float sqrRadius, List<Transform> result)
    {
        if (vehicleRegions == null)
        {
            return;
        }
        foreach (VehicleBarricadeRegion vehicleRegion in vehicleRegions)
        {
            if (vehicleRegion == null || vehicleRegion.drops.Count < 1)
            {
                continue;
            }
            Transform parent = vehicleRegion.parent;
            if (parent == null || !((parent.position - center).sqrMagnitude < 65536f))
            {
                continue;
            }
            foreach (BarricadeDrop drop in vehicleRegion.drops)
            {
                Transform model = drop.model;
                if (!(model == null) && (model.position - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(model);
                }
            }
        }
    }

    [Obsolete]
    public void tellBarricadeOwnerAndGroup(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ulong newOwner, ulong newGroup)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void changeOwnerAndGroup(Transform transform, ulong newOwner, ulong newGroup)
    {
        if (tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
            if (barricadeDrop != null)
            {
                BarricadeDrop.SendOwnerAndGroup.InvokeAndLoopback(barricadeDrop.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), newOwner, newGroup);
                barricadeDrop.serversideData.owner = newOwner;
                barricadeDrop.serversideData.group = newGroup;
                sendHealthChanged(x, y, plant, barricadeDrop);
            }
        }
    }

    public static void transformBarricade(Transform transform, Vector3 point, float angle_x, float angle_y, float angle_z)
    {
        angle_x = Mathf.RoundToInt(angle_x / 2f) * 2;
        angle_y = Mathf.RoundToInt(angle_y / 2f) * 2;
        angle_z = Mathf.RoundToInt(angle_z / 2f) * 2;
        if (tryGetRegion(transform, out var _, out var _, out var _, out var region))
        {
            BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
            if (barricadeDrop != null)
            {
                BarricadeDrop.SendTransformRequest.Invoke(barricadeDrop.GetNetId(), ENetReliability.Reliable, point, MeasurementTool.angleToByte(angle_x), MeasurementTool.angleToByte(angle_y), MeasurementTool.angleToByte(angle_z));
            }
        }
    }

    [Obsolete]
    public void tellTransformBarricade(CSteamID steamID, byte x, byte y, ushort plant, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static bool ServerSetBarricadeTransform(Transform transform, Vector3 position, Quaternion rotation)
    {
        if (!tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
        if (barricadeDrop == null)
        {
            return false;
        }
        Vector3 eulerAngles = rotation.eulerAngles;
        eulerAngles.x = Mathf.RoundToInt(eulerAngles.x / 2f) * 2;
        eulerAngles.y = Mathf.RoundToInt(eulerAngles.y / 2f) * 2;
        eulerAngles.z = Mathf.RoundToInt(eulerAngles.z / 2f) * 2;
        byte angle_x = MeasurementTool.angleToByte(eulerAngles.x);
        byte angle_y = MeasurementTool.angleToByte(eulerAngles.y);
        byte angle_z = MeasurementTool.angleToByte(eulerAngles.z);
        InternalSetBarricadeTransform(x, y, plant, barricadeDrop, position, angle_x, angle_y, angle_z);
        return true;
    }

    internal static void InternalSetBarricadeTransform(byte x, byte y, ushort plant, BarricadeDrop barricade, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        BarricadeDrop.SendTransform.InvokeAndLoopback(barricade.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), x, y, plant, point, angle_x, angle_y, angle_z);
    }

    [Obsolete]
    public void askTransformBarricade(CSteamID steamID, byte x, byte y, ushort plant, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void poseMannequin(Transform transform, byte poseComp)
    {
        InteractableMannequin component = transform.GetComponent<InteractableMannequin>();
        if (component != null)
        {
            component.ClientSetPose(poseComp);
        }
    }

    [Obsolete]
    public void tellPoseMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte poseComp)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static bool ServerSetMannequinPose(InteractableMannequin mannequin, byte poseComp)
    {
        if (!tryGetRegion(mannequin.transform, out var x, out var y, out var plant, out var _))
        {
            return false;
        }
        InternalSetMannequinPose(mannequin, x, y, plant, poseComp);
        return true;
    }

    internal static void InternalSetMannequinPose(InteractableMannequin mannequin, byte x, byte y, ushort plant, byte poseComp)
    {
        InteractableMannequin.SendPose.InvokeAndLoopback(mannequin.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), poseComp);
        mannequin.rebuildState();
    }

    [Obsolete]
    public void askPoseMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte poseComp)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void tellUpdateMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte[] state)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void updateMannequin(Transform transform, EMannequinUpdateMode updateMode)
    {
        InteractableMannequin component = transform.GetComponent<InteractableMannequin>();
        if (component != null)
        {
            component.ClientRequestUpdate(updateMode);
        }
    }

    [Obsolete]
    public void askUpdateMannequin(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte mode)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void rotDisplay(Transform transform, byte rotComp)
    {
        InteractableStorage component = transform.GetComponent<InteractableStorage>();
        if (component != null)
        {
            component.ClientSetDisplayRotation(rotComp);
        }
    }

    [Obsolete]
    public void tellRotDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte rotComp)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askRotDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte rotComp)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void tellBarricadeHealth(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte hp)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void salvageBarricade(Transform transform)
    {
        BarricadeDrop barricadeDrop = FindBarricadeByRootTransform(transform);
        if (barricadeDrop != null)
        {
            BarricadeDrop.SendSalvageRequest.Invoke(barricadeDrop.GetNetId(), ENetReliability.Reliable);
        }
    }

    [Obsolete]
    public void askSalvageBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void tellTank(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort amount)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete("Moved into InteractableTank.ServerSetAmount")]
    public static void updateTank(Transform transform, ushort amount)
    {
        InteractableTank component = transform.GetComponent<InteractableTank>();
        if (component != null)
        {
            component.ServerSetAmount(amount);
        }
    }

    [Obsolete]
    public static void updateSign(Transform transform, string newText)
    {
        InteractableSign component = transform.GetComponent<InteractableSign>();
        if (component != null)
        {
            component.ClientSetText(newText);
        }
    }

    [Obsolete]
    public void tellUpdateSign(CSteamID steamID, byte x, byte y, ushort plant, ushort index, string newText)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static bool ServerSetSignText(InteractableSign sign, string newText)
    {
        if (sign == null)
        {
            throw new ArgumentNullException("sign");
        }
        if (!tryGetRegion(sign.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        string text = sign.trimText(newText);
        if (!sign.isTextValid(text))
        {
            return false;
        }
        ServerSetSignTextInternal(sign, region, x, y, plant, text);
        return true;
    }

    internal static void ServerSetSignTextInternal(InteractableSign sign, BarricadeRegion region, byte x, byte y, ushort plant, string trimmedText)
    {
        InteractableSign.SendChangeText.InvokeAndLoopback(sign.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), trimmedText);
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(sign.transform);
        byte[] state = barricadeDrop.serversideData.barricade.state;
        byte[] bytes = Encoding.UTF8.GetBytes(trimmedText);
        byte[] array = new byte[17 + bytes.Length];
        Buffer.BlockCopy(state, 0, array, 0, 16);
        array[16] = (byte)bytes.Length;
        if (bytes.Length != 0)
        {
            Buffer.BlockCopy(bytes, 0, array, 17, bytes.Length);
        }
        barricadeDrop.serversideData.barricade.state = array;
    }

    [Obsolete]
    public void askUpdateSign(CSteamID steamID, byte x, byte y, ushort plant, ushort index, string newText)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void updateStereoTrack(Transform transform, Guid newTrack)
    {
        InteractableStereo component = transform.GetComponent<InteractableStereo>();
        if (component != null)
        {
            component.ClientSetTrack(newTrack);
        }
    }

    public static bool ServerSetStereoTrack(InteractableStereo stereo, Guid track)
    {
        if (stereo == null)
        {
            throw new ArgumentNullException("stereo");
        }
        if (!tryGetRegion(stereo.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetStereoTrackInternal(stereo, x, y, plant, region, track);
        return true;
    }

    internal static void ServerSetStereoTrackInternal(InteractableStereo stereo, byte x, byte y, ushort plant, BarricadeRegion region, Guid track)
    {
        InteractableStereo.SendTrack.InvokeAndLoopback(stereo.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), track);
        byte[] state = region.FindBarricadeByRootFast(stereo.transform).serversideData.barricade.state;
        new GuidBuffer(track).Write(state, 0);
    }

    [Obsolete]
    public void tellUpdateStereoTrack(CSteamID steamID, byte x, byte y, ushort plant, ushort index, Guid newTrack)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askUpdateStereoTrack(CSteamID steamID, byte x, byte y, ushort plant, ushort index, Guid newTrack)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void updateStereoVolume(Transform transform, byte newVolume)
    {
        InteractableStereo component = transform.GetComponent<InteractableStereo>();
        if (component != null)
        {
            component.ClientSetVolume(newVolume);
        }
    }

    [Obsolete]
    public void tellUpdateStereoVolume(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte newVolume)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askUpdateStereoVolume(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte newVolume)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void transferLibrary(Transform transform, byte transaction, uint delta)
    {
        InteractableLibrary component = transform.GetComponent<InteractableLibrary>();
        if (component != null)
        {
            component.ClientTransfer(transaction, delta);
        }
    }

    [Obsolete]
    public void tellTransferLibrary(CSteamID steamID, byte x, byte y, ushort plant, ushort index, uint newAmount)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askTransferLibrary(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte transaction, uint delta)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleSafezone(Transform transform)
    {
        InteractableSafezone component = transform.GetComponent<InteractableSafezone>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetSafezonePowered(InteractableSafezone safezone, bool isPowered)
    {
        if (safezone == null)
        {
            throw new ArgumentNullException("safezone");
        }
        if (!tryGetRegion(safezone.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetSafezonePoweredInternal(safezone, x, y, plant, region, isPowered);
        return true;
    }

    internal static void ServerSetSafezonePoweredInternal(InteractableSafezone safezone, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
    {
        InteractableSafezone.SendPowered.InvokeAndLoopback(safezone.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
        region.FindBarricadeByRootFast(safezone.transform).serversideData.barricade.state[0] = (byte)(safezone.isPowered ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleSafezone(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleSafezone(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleOxygenator(Transform transform)
    {
        InteractableOxygenator component = transform.GetComponent<InteractableOxygenator>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetOxygenatorPowered(InteractableOxygenator oxygenator, bool isPowered)
    {
        if (oxygenator == null)
        {
            throw new ArgumentNullException("oxygenator");
        }
        if (!tryGetRegion(oxygenator.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetOxygenatorPoweredInternal(oxygenator, x, y, plant, region, isPowered);
        return true;
    }

    internal static void ServerSetOxygenatorPoweredInternal(InteractableOxygenator oxygenator, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(oxygenator.transform);
        InteractableOxygenator.SendPowered.InvokeAndLoopback(oxygenator.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
        barricadeDrop.serversideData.barricade.state[0] = (byte)(oxygenator.isPowered ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleOxygenator(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleOxygenator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleSpot(Transform transform)
    {
        InteractableSpot component = transform.GetComponent<InteractableSpot>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetSpotPowered(InteractableSpot spot, bool isPowered)
    {
        if (spot == null)
        {
            throw new ArgumentNullException("spot");
        }
        if (!tryGetRegion(spot.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetSpotPoweredInternal(spot, x, y, plant, region, isPowered);
        return true;
    }

    internal static void ServerSetSpotPoweredInternal(InteractableSpot spot, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(spot.transform);
        InteractableSpot.SendPowered.InvokeAndLoopback(spot.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
        barricadeDrop.serversideData.barricade.state[0] = (byte)(spot.isPowered ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleSpot(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleSpot(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void sendFuel(Transform transform, ushort fuel)
    {
        InteractableGenerator component = transform.GetComponent<InteractableGenerator>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var _))
        {
            InteractableGenerator.SendFuel.InvokeAndLoopback(component.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), fuel);
        }
    }

    [Obsolete]
    public void tellFuel(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort fuel)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleGenerator(Transform transform)
    {
        InteractableGenerator component = transform.GetComponent<InteractableGenerator>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetGeneratorPowered(InteractableGenerator generator, bool isPowered)
    {
        if (generator == null)
        {
            throw new ArgumentNullException("generator");
        }
        if (!tryGetRegion(generator.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetGeneratorPoweredInternal(generator, x, y, plant, region, isPowered);
        return true;
    }

    internal static void ServerSetGeneratorPoweredInternal(InteractableGenerator generator, byte x, byte y, ushort plant, BarricadeRegion region, bool isPowered)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(generator.transform);
        InteractableGenerator.SendPowered.InvokeAndLoopback(generator.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isPowered);
        barricadeDrop.serversideData.barricade.state[0] = (byte)(generator.isPowered ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleGenerator(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isPowered)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleGenerator(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleFire(Transform transform)
    {
        InteractableFire component = transform.GetComponent<InteractableFire>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetFireLit(InteractableFire fire, bool isLit)
    {
        if (fire == null)
        {
            throw new ArgumentNullException("fire");
        }
        if (!tryGetRegion(fire.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetFireLitInternal(fire, x, y, plant, region, isLit);
        return true;
    }

    internal static void ServerSetFireLitInternal(InteractableFire fire, byte x, byte y, ushort plant, BarricadeRegion region, bool isLit)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(fire.transform);
        InteractableFire.SendLit.InvokeAndLoopback(fire.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isLit);
        barricadeDrop.serversideData.barricade.state[0] = (byte)(fire.isLit ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleFire(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isLit)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleFire(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleOven(Transform transform)
    {
        InteractableOven component = transform.GetComponent<InteractableOven>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    public static bool ServerSetOvenLit(InteractableOven oven, bool isLit)
    {
        if (oven == null)
        {
            throw new ArgumentNullException("oven");
        }
        if (!tryGetRegion(oven.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetOvenLitInternal(oven, x, y, plant, region, isLit);
        return true;
    }

    internal static void ServerSetOvenLitInternal(InteractableOven oven, byte x, byte y, ushort plant, BarricadeRegion region, bool isLit)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(oven.transform);
        InteractableOven.SendLit.InvokeAndLoopback(oven.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isLit);
        barricadeDrop.serversideData.barricade.state[0] = (byte)(oven.isLit ? 1u : 0u);
    }

    [Obsolete]
    public void tellToggleOven(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isLit)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askToggleOven(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void farm(Transform transform)
    {
        InteractableFarm component = transform.GetComponent<InteractableFarm>();
        if (component != null)
        {
            component.ClientHarvest();
        }
    }

    [Obsolete]
    public void tellFarm(CSteamID steamID, byte x, byte y, ushort plant, ushort index, uint planted)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askFarm(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void updateFarm(Transform transform, uint planted, bool shouldSend)
    {
        InteractableFarm component = transform.GetComponent<InteractableFarm>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            if (shouldSend)
            {
                InteractableFarm.SendPlanted.InvokeAndLoopback(component.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), planted);
            }
            BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(transform);
            BitConverter.GetBytes(planted).CopyTo(barricadeDrop.serversideData.barricade.state, 0);
        }
    }

    [Obsolete]
    public void tellOil(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort fuel)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void sendOil(Transform transform, ushort fuel)
    {
        InteractableOil component = transform.GetComponent<InteractableOil>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var _))
        {
            InteractableOil.SendFuel.InvokeAndLoopback(component.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), fuel);
        }
    }

    [Obsolete]
    public void tellRainBarrel(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isFull)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void updateRainBarrel(Transform transform, bool isFull, bool shouldSend)
    {
        InteractableRainBarrel component = transform.GetComponent<InteractableRainBarrel>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            if (shouldSend)
            {
                InteractableRainBarrel.SendFull.InvokeAndLoopback(component.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isFull);
            }
            region.FindBarricadeByRootFast(transform).serversideData.barricade.state[0] = (byte)(isFull ? 1u : 0u);
        }
    }

    public static void sendStorageDisplay(Transform transform, Item item, ushort skin, ushort mythic, string tags, string dynamicProps)
    {
        InteractableStorage component = transform.GetComponent<InteractableStorage>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var _))
        {
            ushort arg;
            byte arg2;
            byte[] arg3;
            if (item != null)
            {
                arg = item.id;
                arg2 = item.quality;
                arg3 = item.state;
            }
            else
            {
                arg = 0;
                arg2 = 0;
                arg3 = new byte[0];
            }
            InteractableStorage.SendDisplay.Invoke(component.GetNetId(), ENetReliability.Reliable, GatherClientConnections(x, y, plant), arg, arg2, arg3, skin, mythic, tags, dynamicProps);
        }
    }

    [Obsolete]
    public void tellStorageDisplay(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void storeStorage(Transform transform, bool quickGrab)
    {
        InteractableStorage component = transform.GetComponent<InteractableStorage>();
        if (component != null)
        {
            component.ClientInteract(quickGrab);
        }
    }

    [Obsolete]
    public void askStoreStorage(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool quickGrab)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public static void toggleDoor(Transform transform)
    {
        InteractableDoor component = transform.GetComponent<InteractableDoor>();
        if (component != null)
        {
            component.ClientToggle();
        }
    }

    [Obsolete]
    public void tellToggleDoor(CSteamID steamID, byte x, byte y, ushort plant, ushort index, bool isOpen)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static bool ServerSetDoorOpen(InteractableDoor door, bool isOpen)
    {
        if (door == null)
        {
            throw new ArgumentNullException("door");
        }
        if (!tryGetRegion(door.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetDoorOpenInternal(door, x, y, plant, region, isOpen);
        return true;
    }

    internal static void ServerSetDoorOpenInternal(InteractableDoor door, byte x, byte y, ushort plant, BarricadeRegion region, bool isOpen)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(door.transform);
        InteractableDoor.SendOpen.InvokeAndLoopback(door.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), isOpen);
        barricadeDrop.serversideData.barricade.state[16] = (byte)(isOpen ? 1u : 0u);
    }

    [Obsolete]
    public void askToggleDoor(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    private static bool tryGetBedInRegion(BarricadeRegion region, CSteamID owner, ref Vector3 point, ref byte angle)
    {
        foreach (BarricadeDrop drop in region.drops)
        {
            BarricadeData serversideData = drop.serversideData;
            if (serversideData.barricade.state.Length == 0)
            {
                continue;
            }
            InteractableBed interactableBed = drop.interactable as InteractableBed;
            if (!(interactableBed != null) || !(interactableBed.owner == owner) || !Level.checkSafeIncludingClipVolumes(interactableBed.transform.position))
            {
                continue;
            }
            point = interactableBed.transform.position;
            angle = MeasurementTool.angleToByte(serversideData.angle_y * 2 + 90);
            int num = Physics.OverlapCapsuleNonAlloc(point + new Vector3(0f, PlayerStance.RADIUS, 0f), point + new Vector3(0f, 2.5f - PlayerStance.RADIUS, 0f), PlayerStance.RADIUS, checkColliders, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < num; i++)
            {
                if (checkColliders[i].gameObject != interactableBed.gameObject)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public static bool tryGetBed(CSteamID owner, out Vector3 point, out byte angle)
    {
        point = Vector3.zero;
        angle = 0;
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                if (tryGetBedInRegion(regions[b, b2], owner, ref point, ref angle))
                {
                    return true;
                }
            }
        }
        for (ushort num = 0; num < vehicleRegions.Count; num = (ushort)(num + 1))
        {
            if (tryGetBedInRegion(vehicleRegions[num], owner, ref point, ref angle))
            {
                return true;
            }
        }
        return false;
    }

    private static bool UnclaimBedsInRegion(CSteamID owner, BarricadeRegion region, byte x, byte y, ushort plant)
    {
        for (ushort num = 0; num < region.drops.Count; num = (ushort)(num + 1))
        {
            BarricadeDrop barricadeDrop = region.drops[num];
            if (barricadeDrop.serversideData.barricade.state.Length != 0)
            {
                InteractableBed interactableBed = barricadeDrop.interactable as InteractableBed;
                if (interactableBed != null && interactableBed.owner == owner)
                {
                    InteractableBed.SendClaim.InvokeAndLoopback(interactableBed.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), CSteamID.Nil);
                    BitConverter.GetBytes(interactableBed.owner.m_SteamID).CopyTo(barricadeDrop.serversideData.barricade.state, 0);
                    return true;
                }
            }
        }
        return false;
    }

    public static void unclaimBeds(CSteamID owner)
    {
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                BarricadeRegion region = regions[b, b2];
                if (UnclaimBedsInRegion(owner, region, b, b2, ushort.MaxValue))
                {
                    return;
                }
            }
        }
        for (ushort num = 0; num < vehicleRegions.Count; num = (ushort)(num + 1))
        {
            BarricadeRegion region2 = vehicleRegions[num];
            if (UnclaimBedsInRegion(owner, region2, byte.MaxValue, byte.MaxValue, num))
            {
                break;
            }
        }
    }

    [Obsolete]
    public static void claimBed(Transform transform)
    {
        InteractableBed component = transform.GetComponent<InteractableBed>();
        if (component != null)
        {
            component.ClientClaim();
        }
    }

    [Obsolete]
    public void tellClaimBed(CSteamID steamID, byte x, byte y, ushort plant, ushort index, CSteamID owner)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    [Obsolete]
    public void askClaimBed(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static bool ServerUnclaimBed(InteractableBed bed)
    {
        if (bed == null)
        {
            throw new ArgumentNullException("bed");
        }
        if (!tryGetRegion(bed.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        ServerSetBedOwnerInternal(bed, x, y, plant, region, CSteamID.Nil);
        return true;
    }

    public static bool ServerClaimBedForPlayer(InteractableBed bed, Player player)
    {
        if (bed == null)
        {
            throw new ArgumentNullException("bed");
        }
        if (player == null)
        {
            throw new ArgumentNullException("player");
        }
        if (!tryGetRegion(bed.transform, out var x, out var y, out var plant, out var region))
        {
            return false;
        }
        unclaimBeds(player.channel.owner.playerID.steamID);
        ServerSetBedOwnerInternal(bed, x, y, plant, region, player.channel.owner.playerID.steamID);
        return true;
    }

    internal static void ServerSetBedOwnerInternal(InteractableBed bed, byte x, byte y, ushort plant, BarricadeRegion region, CSteamID steamID)
    {
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(bed.transform);
        InteractableBed.SendClaim.InvokeAndLoopback(bed.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), steamID);
        BitConverter.GetBytes(bed.owner.m_SteamID).CopyTo(barricadeDrop.serversideData.barricade.state, 0);
    }

    [Obsolete]
    public void tellShootSentry(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void sendShootSentry(Transform transform)
    {
        InteractableSentry component = transform.GetComponent<InteractableSentry>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var _))
        {
            InteractableSentry.SendShoot.InvokeAndLoopback(component.GetNetId(), ENetReliability.Unreliable, GatherRemoteClientConnections(x, y, plant));
        }
    }

    [Obsolete]
    public void tellAlertSentry(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte yaw, byte pitch)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void sendAlertSentry(Transform transform, float yaw, float pitch)
    {
        InteractableSentry component = transform.GetComponent<InteractableSentry>();
        if (component != null && tryGetRegion(transform, out var x, out var y, out var plant, out var _))
        {
            InteractableSentry.SendAlert.InvokeAndLoopback(component.GetNetId(), ENetReliability.Unreliable, GatherRemoteClientConnections(x, y, plant), MeasurementTool.angleToByte(yaw), MeasurementTool.angleToByte(pitch));
        }
    }

    public static void damage(Transform transform, float damage, float times, bool armor, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (!tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            return;
        }
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
        if (barricadeDrop == null || barricadeDrop.serversideData.barricade.isDead)
        {
            return;
        }
        ItemBarricadeAsset asset = barricadeDrop.asset;
        if (asset == null || !asset.canBeDamaged)
        {
            return;
        }
        if (armor)
        {
            times *= Provider.modeConfigData.Barricades.getArmorMultiplier(asset.armorTier);
        }
        ushort pendingTotalDamage = (ushort)(damage * times);
        bool shouldAllow = true;
        onDamageBarricadeRequested?.Invoke(instigatorSteamID, transform, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        if (!shouldAllow || pendingTotalDamage < 1)
        {
            return;
        }
        barricadeDrop.serversideData.barricade.askDamage(pendingTotalDamage);
        if (barricadeDrop.serversideData.barricade.isDead)
        {
            EffectAsset effectAsset = asset.FindExplosionEffectAsset();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.position = transform.position + Vector3.down * asset.offset;
                parameters.relevantDistance = EffectManager.MEDIUM;
                EffectManager.triggerEffect(parameters);
            }
            asset.SpawnItemDropsOnDestroy(transform.position);
            destroyBarricade(barricadeDrop, x, y, plant);
        }
        else
        {
            sendHealthChanged(x, y, plant, barricadeDrop);
        }
    }

    [Obsolete("Please replace the methods which take an index")]
    public static void destroyBarricade(BarricadeRegion region, byte x, byte y, ushort plant, ushort index)
    {
        destroyBarricade(region.drops[index], x, y, plant);
    }

    public static void destroyBarricade(BarricadeDrop barricade, byte x, byte y, ushort plant)
    {
        if (tryGetRegion(x, y, plant, out var region))
        {
            region.barricades.Remove(barricade.serversideData);
            SendDestroyBarricade.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), barricade.GetNetId());
        }
    }

    private static void sendHealthChanged(byte x, byte y, ushort plant, BarricadeDrop barricade)
    {
        if (plant == ushort.MaxValue)
        {
            BarricadeDrop.SendHealth.Invoke(barricade.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) => client.player != null && Regions.checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, BARRICADE_REGIONS) && OwnershipTool.checkToggle(client.playerID.steamID, barricade.serversideData.owner, client.player.quests.groupID, barricade.serversideData.group)), (byte)((float)(int)barricade.serversideData.barricade.health / (float)(int)barricade.serversideData.barricade.asset.health * 100f));
        }
        else
        {
            BarricadeDrop.SendHealth.Invoke(barricade.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) => OwnershipTool.checkToggle(client.playerID.steamID, barricade.serversideData.owner, client.player.quests.groupID, barricade.serversideData.group)), (byte)((float)(int)barricade.serversideData.barricade.health / (float)(int)barricade.serversideData.barricade.asset.health * 100f));
        }
    }

    public static void repair(Transform transform, float damage, float times)
    {
        repair(transform, damage, times, default(CSteamID));
    }

    public static void repair(Transform transform, float damage, float times, CSteamID instigatorSteamID = default(CSteamID))
    {
        if (!tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            return;
        }
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
        if (barricadeDrop != null && !barricadeDrop.serversideData.barricade.isDead && !barricadeDrop.serversideData.barricade.isRepaired)
        {
            float pendingTotalHealing = damage * times;
            bool shouldAllow = true;
            BarricadeManager.OnRepairRequested?.Invoke(instigatorSteamID, transform, ref pendingTotalHealing, ref shouldAllow);
            ushort num = MathfEx.RoundAndClampToUShort(pendingTotalHealing);
            if (shouldAllow && num >= 1)
            {
                barricadeDrop.serversideData.barricade.askRepair(num);
                sendHealthChanged(x, y, plant, barricadeDrop);
                BarricadeManager.OnRepaired?.Invoke(instigatorSteamID, transform, (int)num);
            }
        }
    }

    public static Transform dropBarricade(Barricade barricade, Transform hit, Vector3 point, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
    {
        if (barricade.asset == null)
        {
            return null;
        }
        bool shouldAllow = true;
        onDeployBarricadeRequested?.Invoke(barricade, barricade.asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        if (!shouldAllow)
        {
            return null;
        }
        Quaternion rotation = getRotation(barricade.asset, angle_x, angle_y, angle_z);
        if (hit != null && hit.transform.CompareTag("Vehicle"))
        {
            return dropPlantedBarricade(hit, barricade, point, rotation, owner, group);
        }
        return dropNonPlantedBarricade(barricade, point, rotation, owner, group);
    }

    private static Transform dropBarricadeIntoRegionInternal(BarricadeRegion region, Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
    {
        Vector3 eulerAngles = rotation.eulerAngles;
        float angle = Mathf.RoundToInt(eulerAngles.x / 2f) * 2;
        float angle2 = Mathf.RoundToInt(eulerAngles.y / 2f) * 2;
        float angle3 = Mathf.RoundToInt(eulerAngles.z / 2f) * 2;
        uint newInstanceID = ++instanceCount;
        BarricadeData barricadeData = new BarricadeData(barricade, point, MeasurementTool.angleToByte(angle), MeasurementTool.angleToByte(angle2), MeasurementTool.angleToByte(angle3), owner, group, Provider.time, newInstanceID);
        NetId netId = NetIdRegistry.ClaimBlock(3u);
        Transform obj = manager.spawnBarricade(region, barricade.asset.GUID, barricade.state, barricadeData.point, barricadeData.angle_x, barricadeData.angle_y, barricadeData.angle_z, 100, barricadeData.owner, barricadeData.group, netId);
        if (obj != null)
        {
            region.drops.GetTail().serversideData = barricadeData;
            region.barricades.Add(barricadeData);
        }
        return obj;
    }

    public static Transform dropPlantedBarricade(Transform parent, Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
    {
        VehicleBarricadeRegion vehicleBarricadeRegion = FindVehicleRegionByTransform(parent);
        if (vehicleBarricadeRegion == null)
        {
            return null;
        }
        Transform obj = dropBarricadeIntoRegionInternal(vehicleBarricadeRegion, barricade, point, rotation, owner, group);
        if (obj != null)
        {
            BarricadeDrop tail = vehicleBarricadeRegion.drops.GetTail();
            BarricadeData serversideData = tail.serversideData;
            SendSingleBarricade.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicleBarricadeRegion._netId, barricade.asset.GUID, barricade.state, serversideData.point, serversideData.angle_x, serversideData.angle_y, serversideData.angle_z, serversideData.owner, serversideData.group, tail.GetNetId());
            BarricadeSpawnedHandler barricadeSpawnedHandler = onBarricadeSpawned;
            if (barricadeSpawnedHandler == null)
            {
                return obj;
            }
            barricadeSpawnedHandler(vehicleBarricadeRegion, tail);
        }
        return obj;
    }

    public static Transform dropNonPlantedBarricade(Barricade barricade, Vector3 point, Quaternion rotation, ulong owner, ulong group)
    {
        if (!Regions.tryGetCoordinate(point, out var x, out var y))
        {
            return null;
        }
        if (!tryGetRegion(x, y, ushort.MaxValue, out var region))
        {
            return null;
        }
        Transform obj = dropBarricadeIntoRegionInternal(region, barricade, point, rotation, owner, group);
        if (obj != null)
        {
            BarricadeDrop tail = region.drops.GetTail();
            BarricadeData serversideData = tail.serversideData;
            SendSingleBarricade.Invoke(ENetReliability.Reliable, Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS), NetId.INVALID, barricade.asset.GUID, barricade.state, serversideData.point, serversideData.angle_x, serversideData.angle_y, serversideData.angle_z, serversideData.owner, serversideData.group, tail.GetNetId());
            BarricadeSpawnedHandler barricadeSpawnedHandler = onBarricadeSpawned;
            if (barricadeSpawnedHandler == null)
            {
                return obj;
            }
            barricadeSpawnedHandler(region, tail);
        }
        return obj;
    }

    [Obsolete]
    public void tellTakeBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
    {
        throw new NotSupportedException("Removed during barricade NetId rewrite");
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveDestroyBarricade(in ClientInvocationContext context, NetId netId)
    {
        BarricadeDrop barricadeDrop = NetIdRegistry.Get<BarricadeDrop>(netId);
        byte x;
        byte y;
        ushort plant;
        BarricadeRegion region;
        if (barricadeDrop == null)
        {
            CancelInstantiationByNetId(netId);
        }
        else if (tryGetRegion(barricadeDrop.model, out x, out y, out plant, out region))
        {
            barricadeDrop.CustomDestroy();
            region.drops.Remove(barricadeDrop);
        }
    }

    [Obsolete]
    public void tellClearRegionBarricades(CSteamID steamID, byte x, byte y)
    {
        ReceiveClearRegionBarricades(x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellClearRegionBarricades")]
    public static void ReceiveClearRegionBarricades(byte x, byte y)
    {
        if (Provider.isServer || regions[x, y].isNetworked)
        {
            BarricadeRegion region = regions[x, y];
            DestroyAllInRegion(region);
            CancelInstantiationsInRegion(region);
        }
    }

    public static void askClearRegionBarricades(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y))
        {
            BarricadeRegion barricadeRegion = regions[x, y];
            if (barricadeRegion.drops.Count > 0)
            {
                barricadeRegion.barricades.Clear();
                SendClearRegionBarricades.InvokeAndLoopback(ENetReliability.Reliable, Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS), x, y);
            }
        }
    }

    public static void askClearAllBarricades()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                askClearRegionBarricades(b, b2);
            }
        }
    }

    public static Quaternion getRotation(ItemBarricadeAsset asset, float angle_x, float angle_y, float angle_z)
    {
        return Quaternion.Euler(0f, angle_y, 0f) * Quaternion.Euler((float)((asset.build != EBuild.DOOR && asset.build != EBuild.GATE && asset.build != EBuild.SHUTTER && asset.build != EBuild.HATCH) ? (-90) : 0) + angle_x, 0f, 0f) * Quaternion.Euler(0f, angle_z, 0f);
    }

    private Transform spawnBarricade(BarricadeRegion region, Guid assetGuid, byte[] state, Vector3 point, byte angle_x, byte angle_y, byte angle_z, byte hp, ulong owner, ulong group, NetId netId)
    {
        ItemBarricadeAsset itemBarricadeAsset = Assets.find(assetGuid) as ItemBarricadeAsset;
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(assetGuid, itemBarricadeAsset, "Barricade");
        }
        if (itemBarricadeAsset == null || itemBarricadeAsset.barricade == null)
        {
            return null;
        }
        Transform transform = null;
        try
        {
            Quaternion quaternion = Quaternion.Euler(angle_x * 2, angle_y * 2, angle_z * 2);
            if (itemBarricadeAsset.eligibleForPooling)
            {
                int instanceID = itemBarricadeAsset.barricade.GetInstanceID();
                Stack<GameObject> orAddNew = pool.GetOrAddNew(instanceID);
                while (orAddNew.Count > 0)
                {
                    GameObject gameObject = orAddNew.Pop();
                    if (gameObject != null)
                    {
                        transform = gameObject.transform;
                        transform.parent = region.parent;
                        transform.localPosition = point;
                        transform.localRotation = quaternion;
                        transform.localScale = Vector3.one;
                        gameObject.SetActive(value: true);
                        break;
                    }
                }
            }
            if (transform == null)
            {
                GameObject gameObject2;
                if (region.parent == null)
                {
                    gameObject2 = UnityEngine.Object.Instantiate(itemBarricadeAsset.barricade, point, quaternion);
                    transform = gameObject2.transform;
                }
                else
                {
                    gameObject2 = UnityEngine.Object.Instantiate(itemBarricadeAsset.barricade, region.parent);
                    transform = gameObject2.transform;
                    transform.localPosition = point;
                    transform.localRotation = quaternion;
                }
                transform.localScale = Vector3.one;
                transform.name = itemBarricadeAsset.id.ToString();
                if (itemBarricadeAsset.useWaterHeightTransparentSort && !Dedicator.IsDedicatedServer)
                {
                    gameObject2.AddComponent<WaterHeightTransparentSort>();
                }
                if (Provider.isServer && itemBarricadeAsset.nav != null)
                {
                    Transform transform2 = UnityEngine.Object.Instantiate(itemBarricadeAsset.nav).transform;
                    transform2.name = "Nav";
                    if (itemBarricadeAsset.build == EBuild.DOOR || itemBarricadeAsset.build == EBuild.GATE || itemBarricadeAsset.build == EBuild.SHUTTER || itemBarricadeAsset.build == EBuild.HATCH)
                    {
                        Transform transform3 = transform.Find("Skeleton").Find("Hinge");
                        if (transform3 != null)
                        {
                            transform2.parent = transform3;
                        }
                        else
                        {
                            transform2.parent = transform;
                        }
                    }
                    else
                    {
                        transform2.parent = transform;
                    }
                    transform2.localPosition = Vector3.zero;
                    transform2.localRotation = Quaternion.identity;
                }
                Transform transform4 = transform.FindChildRecursive("Burning");
                if (transform4 != null)
                {
                    transform4.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.BURNING;
                }
                Transform transform5 = transform.FindChildRecursive("Warm");
                if (transform5 != null)
                {
                    transform5.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.WARM;
                }
            }
            else if (itemBarricadeAsset.useWaterHeightTransparentSort && !Dedicator.IsDedicatedServer)
            {
                transform.GetOrAddComponent<WaterHeightTransparentSort>().updateRenderQueue();
            }
            if (itemBarricadeAsset.build == EBuild.DOOR || itemBarricadeAsset.build == EBuild.GATE || itemBarricadeAsset.build == EBuild.SHUTTER || itemBarricadeAsset.build == EBuild.HATCH)
            {
                InteractableDoor orAddComponent = transform.GetOrAddComponent<InteractableDoor>();
                orAddComponent.updateState(itemBarricadeAsset, state);
                Transform transform6 = transform.Find("Skeleton").Find("Hinge");
                if (transform6 != null)
                {
                    transform6.gameObject.GetOrAddComponent<InteractableDoorHinge>().door = orAddComponent;
                }
                Transform transform7 = transform.Find("Skeleton").Find("Left_Hinge");
                if (transform7 != null)
                {
                    transform7.gameObject.GetOrAddComponent<InteractableDoorHinge>().door = orAddComponent;
                }
                Transform transform8 = transform.Find("Skeleton").Find("Right_Hinge");
                if (transform8 != null)
                {
                    transform8.gameObject.GetOrAddComponent<InteractableDoorHinge>().door = orAddComponent;
                }
            }
            else if (itemBarricadeAsset.build == EBuild.BED)
            {
                transform.GetOrAddComponent<InteractableBed>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.STORAGE || itemBarricadeAsset.build == EBuild.STORAGE_WALL)
            {
                transform.GetOrAddComponent<InteractableStorage>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.FARM)
            {
                transform.GetOrAddComponent<InteractableFarm>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.TORCH || itemBarricadeAsset.build == EBuild.CAMPFIRE)
            {
                transform.GetOrAddComponent<InteractableFire>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.OVEN)
            {
                transform.GetOrAddComponent<InteractableOven>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.SPIKE || itemBarricadeAsset.build == EBuild.WIRE)
            {
                transform.Find("Trap").GetOrAddComponent<InteractableTrap>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.CHARGE)
            {
                InteractableCharge orAddComponent2 = transform.GetOrAddComponent<InteractableCharge>();
                orAddComponent2.updateState(itemBarricadeAsset, state);
                orAddComponent2.owner = owner;
                orAddComponent2.group = group;
            }
            else if (itemBarricadeAsset.build == EBuild.GENERATOR)
            {
                transform.GetOrAddComponent<InteractableGenerator>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.SPOT || itemBarricadeAsset.build == EBuild.CAGE)
            {
                transform.GetOrAddComponent<InteractableSpot>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.SAFEZONE)
            {
                transform.GetOrAddComponent<InteractableSafezone>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.OXYGENATOR)
            {
                transform.GetOrAddComponent<InteractableOxygenator>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.SIGN || itemBarricadeAsset.build == EBuild.SIGN_WALL || itemBarricadeAsset.build == EBuild.NOTE)
            {
                transform.GetOrAddComponent<InteractableSign>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.CLAIM)
            {
                InteractableClaim orAddComponent3 = transform.GetOrAddComponent<InteractableClaim>();
                orAddComponent3.owner = owner;
                orAddComponent3.group = group;
                orAddComponent3.updateState(itemBarricadeAsset);
            }
            else if (itemBarricadeAsset.build == EBuild.BEACON)
            {
                transform.GetOrAddComponent<InteractableBeacon>().updateState(itemBarricadeAsset);
            }
            else if (itemBarricadeAsset.build == EBuild.BARREL_RAIN)
            {
                transform.GetOrAddComponent<InteractableRainBarrel>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.OIL)
            {
                transform.GetOrAddComponent<InteractableOil>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.TANK)
            {
                transform.GetOrAddComponent<InteractableTank>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.SENTRY || itemBarricadeAsset.build == EBuild.SENTRY_FREEFORM)
            {
                InteractableSentry orAddComponent4 = transform.GetOrAddComponent<InteractableSentry>();
                InteractablePower interactablePower = (orAddComponent4.power = transform.GetOrAddComponent<InteractablePower>());
                orAddComponent4.updateState(itemBarricadeAsset, state);
                interactablePower.RefreshIsConnectedToPower();
            }
            else if (itemBarricadeAsset.build == EBuild.LIBRARY)
            {
                transform.GetOrAddComponent<InteractableLibrary>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.MANNEQUIN)
            {
                transform.GetOrAddComponent<InteractableMannequin>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.STEREO)
            {
                transform.GetOrAddComponent<InteractableStereo>().updateState(itemBarricadeAsset, state);
            }
            else if (itemBarricadeAsset.build == EBuild.CLOCK && !Dedicator.IsDedicatedServer)
            {
                transform.GetOrAddComponent<InteractableClock>().updateState(itemBarricadeAsset, state);
            }
            if (!itemBarricadeAsset.isUnpickupable)
            {
                Interactable2HP orAddComponent5 = transform.GetOrAddComponent<Interactable2HP>();
                orAddComponent5.hp = hp;
                if (itemBarricadeAsset.build == EBuild.DOOR || itemBarricadeAsset.build == EBuild.GATE || itemBarricadeAsset.build == EBuild.SHUTTER || itemBarricadeAsset.build == EBuild.HATCH)
                {
                    Transform transform9 = transform.Find("Skeleton").Find("Hinge");
                    if (transform9 != null)
                    {
                        Interactable2SalvageBarricade orAddComponent6 = transform9.GetOrAddComponent<Interactable2SalvageBarricade>();
                        orAddComponent6.root = transform;
                        orAddComponent6.hp = orAddComponent5;
                        orAddComponent6.owner = owner;
                        orAddComponent6.group = group;
                        orAddComponent6.salvageDurationMultiplier = itemBarricadeAsset.salvageDurationMultiplier;
                        orAddComponent6.shouldBypassPickupOwnership = itemBarricadeAsset.shouldBypassPickupOwnership;
                    }
                    Transform transform10 = transform.Find("Skeleton").Find("Left_Hinge");
                    if (transform10 != null)
                    {
                        Interactable2SalvageBarricade orAddComponent7 = transform10.GetOrAddComponent<Interactable2SalvageBarricade>();
                        orAddComponent7.root = transform;
                        orAddComponent7.hp = orAddComponent5;
                        orAddComponent7.owner = owner;
                        orAddComponent7.group = group;
                        orAddComponent7.salvageDurationMultiplier = itemBarricadeAsset.salvageDurationMultiplier;
                        orAddComponent7.shouldBypassPickupOwnership = itemBarricadeAsset.shouldBypassPickupOwnership;
                    }
                    Transform transform11 = transform.Find("Skeleton").Find("Right_Hinge");
                    if (transform11 != null)
                    {
                        Interactable2SalvageBarricade orAddComponent8 = transform11.GetOrAddComponent<Interactable2SalvageBarricade>();
                        orAddComponent8.root = transform;
                        orAddComponent8.hp = orAddComponent5;
                        orAddComponent8.owner = owner;
                        orAddComponent8.group = group;
                        orAddComponent8.salvageDurationMultiplier = itemBarricadeAsset.salvageDurationMultiplier;
                        orAddComponent8.shouldBypassPickupOwnership = itemBarricadeAsset.shouldBypassPickupOwnership;
                    }
                }
                else
                {
                    Interactable2SalvageBarricade orAddComponent9 = transform.GetOrAddComponent<Interactable2SalvageBarricade>();
                    orAddComponent9.root = transform;
                    orAddComponent9.hp = orAddComponent5;
                    orAddComponent9.owner = owner;
                    orAddComponent9.group = group;
                    orAddComponent9.salvageDurationMultiplier = itemBarricadeAsset.salvageDurationMultiplier;
                    orAddComponent9.shouldBypassPickupOwnership = itemBarricadeAsset.shouldBypassPickupOwnership;
                }
            }
            if (region.parent != null)
            {
                barricadeColliders.Clear();
                transform.GetComponentsInChildren(barricadeColliders);
                foreach (Collider barricadeCollider in barricadeColliders)
                {
                    bool num = barricadeCollider is MeshCollider;
                    if (num)
                    {
                        barricadeCollider.enabled = false;
                    }
                    if (barricadeCollider.GetComponent<Rigidbody>() == null)
                    {
                        Rigidbody rigidbody = barricadeCollider.gameObject.AddComponent<Rigidbody>();
                        rigidbody.useGravity = false;
                        rigidbody.isKinematic = true;
                    }
                    if (num)
                    {
                        barricadeCollider.enabled = true;
                    }
                    if (barricadeCollider.gameObject.layer == 27)
                    {
                        barricadeCollider.gameObject.layer = 14;
                    }
                }
                transform.gameObject.SetActive(value: false);
                transform.gameObject.SetActive(value: true);
                InteractableVehicle component = region.parent.GetComponent<InteractableVehicle>();
                if (component != null)
                {
                    component.ignoreCollisionWith(barricadeColliders, shouldIgnore: true);
                }
            }
            BarricadeDrop barricadeDrop = new BarricadeDrop(transform, transform.GetComponent<Interactable>(), itemBarricadeAsset);
            barricadeDrop.AssignNetId(netId);
            transform.GetOrAddComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet = barricadeDrop;
            region.drops.Add(barricadeDrop);
            return transform;
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Exception while spawning barricade: {0}", itemBarricadeAsset);
            UnturnedLog.exception(e);
            if (transform != null)
            {
                UnityEngine.Object.Destroy(transform.gameObject);
                return null;
            }
            return transform;
        }
    }

    [Obsolete]
    public void tellBarricade(CSteamID steamID, byte x, byte y, ushort plant, ushort id, byte[] state, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, uint instanceID)
    {
        throw new NotSupportedException("Barricades no longer function without a unique NetId");
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveSingleBarricade(in ClientInvocationContext context, NetId parentNetId, Guid assetId, byte[] state, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, NetId netId)
    {
        BarricadeRegion region;
        if (parentNetId.IsNull())
        {
            if (!Regions.tryGetCoordinate(point, out var x, out var y) || !tryGetRegion(x, y, ushort.MaxValue, out region))
            {
                return;
            }
        }
        else
        {
            region = NetIdRegistry.Get<BarricadeRegion>(parentNetId);
            if (region == null)
            {
                return;
            }
        }
        if (Provider.isServer || region.isNetworked)
        {
            float sortOrder = 0f;
            if (MainCamera.instance != null)
            {
                sortOrder = (MainCamera.instance.transform.position - point).sqrMagnitude;
            }
            BarricadeInstantiationParameters item = default(BarricadeInstantiationParameters);
            item.region = region;
            item.assetId = assetId;
            item.state = state;
            item.position = point;
            item.angle_x = angle_x;
            item.angle_y = angle_y;
            item.angle_z = angle_z;
            item.hp = 100;
            item.owner = owner;
            item.group = group;
            item.netId = netId;
            item.sortOrder = sortOrder;
            NetInvocationDeferralRegistry.MarkDeferred(netId, 3u);
            pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(item), item);
        }
    }

    [Obsolete]
    public void tellBarricades(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMultipleBarricades(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadNetId(out var value3);
        BarricadeRegion region;
        if (value3 == NetId.INVALID)
        {
            if (!tryGetRegion(value, value2, ushort.MaxValue, out region))
            {
                return;
            }
        }
        else
        {
            region = NetIdRegistry.Get<BarricadeRegion>(value3);
            if (region == null)
            {
                return;
            }
        }
        reader.ReadUInt8(out var value4);
        if (value3 == NetId.INVALID)
        {
            if (value4 == 0)
            {
                if (region.isNetworked)
                {
                    return;
                }
                DestroyAllInRegion(region);
            }
            else if (!region.isNetworked)
            {
                return;
            }
        }
        region.isNetworked = true;
        reader.ReadUInt16(out var value5);
        if (value5 > 0)
        {
            reader.ReadFloat(out var value6);
            instantiationsToInsert.Clear();
            for (ushort num = 0; num < value5; num = (ushort)(num + 1))
            {
                BarricadeInstantiationParameters item = default(BarricadeInstantiationParameters);
                item.region = region;
                item.sortOrder = value6;
                reader.ReadGuid(out item.assetId);
                reader.ReadUInt8(out var value7);
                byte[] array = new byte[value7];
                reader.ReadBytes(array);
                item.state = array;
                reader.ReadClampedVector3(out item.position, 13, 11);
                reader.ReadUInt8(out item.angle_x);
                reader.ReadUInt8(out item.angle_y);
                reader.ReadUInt8(out item.angle_z);
                reader.ReadUInt8(out item.hp);
                reader.ReadUInt64(out item.owner);
                reader.ReadUInt64(out item.group);
                reader.ReadNetId(out item.netId);
                NetInvocationDeferralRegistry.MarkDeferred(item.netId, 3u);
                instantiationsToInsert.Add(item);
            }
            pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
        }
        Level.isLoadingBarricades = false;
    }

    [Obsolete]
    public void askBarricades(CSteamID steamID, byte x, byte y, ushort plant)
    {
    }

    internal void SendRegion(SteamPlayer client, BarricadeRegion region, byte x, byte y, NetId parentNetId, float sortOrder)
    {
        if (region.drops.Count > 0)
        {
            byte packet = 0;
            int index = 0;
            int count = 0;
            while (index < region.drops.Count)
            {
                int num = 0;
                while (count < region.drops.Count)
                {
                    num += 44 + region.drops[count].serversideData.barricade.state.Length;
                    count++;
                    if (num > Block.BUFFER_SIZE / 2)
                    {
                        break;
                    }
                }
                SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
                {
                    writer.WriteUInt8(x);
                    writer.WriteUInt8(y);
                    writer.WriteNetId(parentNetId);
                    writer.WriteUInt8(packet);
                    writer.WriteUInt16((ushort)(count - index));
                    writer.WriteFloat(sortOrder);
                    for (; index < count; index++)
                    {
                        BarricadeDrop barricadeDrop = region.drops[index];
                        BarricadeData serversideData = barricadeDrop.serversideData;
                        InteractableStorage interactableStorage = barricadeDrop.interactable as InteractableStorage;
                        writer.WriteGuid(barricadeDrop.asset.GUID);
                        if (interactableStorage != null)
                        {
                            byte[] array;
                            if (interactableStorage.isDisplay)
                            {
                                byte[] bytes = Encoding.UTF8.GetBytes(interactableStorage.displayTags);
                                byte[] bytes2 = Encoding.UTF8.GetBytes(interactableStorage.displayDynamicProps);
                                array = new byte[20 + ((interactableStorage.displayItem != null) ? interactableStorage.displayItem.state.Length : 0) + 4 + 1 + bytes.Length + 1 + bytes2.Length + 1];
                                if (interactableStorage.displayItem != null)
                                {
                                    Array.Copy(BitConverter.GetBytes(interactableStorage.displayItem.id), 0, array, 16, 2);
                                    array[18] = interactableStorage.displayItem.quality;
                                    array[19] = (byte)interactableStorage.displayItem.state.Length;
                                    Array.Copy(interactableStorage.displayItem.state, 0, array, 20, interactableStorage.displayItem.state.Length);
                                    Array.Copy(BitConverter.GetBytes(interactableStorage.displaySkin), 0, array, 20 + interactableStorage.displayItem.state.Length, 2);
                                    Array.Copy(BitConverter.GetBytes(interactableStorage.displayMythic), 0, array, 20 + interactableStorage.displayItem.state.Length + 2, 2);
                                    array[20 + interactableStorage.displayItem.state.Length + 4] = (byte)bytes.Length;
                                    Array.Copy(bytes, 0, array, 20 + interactableStorage.displayItem.state.Length + 5, bytes.Length);
                                    array[20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length] = (byte)bytes2.Length;
                                    Array.Copy(bytes2, 0, array, 20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length + 1, bytes2.Length);
                                    array[20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length + 1 + bytes2.Length] = interactableStorage.rot_comp;
                                }
                            }
                            else
                            {
                                array = new byte[16];
                            }
                            Array.Copy(serversideData.barricade.state, 0, array, 0, 16);
                            writer.WriteUInt8((byte)array.Length);
                            writer.WriteBytes(array);
                        }
                        else
                        {
                            writer.WriteUInt8((byte)serversideData.barricade.state.Length);
                            writer.WriteBytes(serversideData.barricade.state);
                        }
                        writer.WriteClampedVector3(serversideData.point, 13, 11);
                        writer.WriteUInt8(serversideData.angle_x);
                        writer.WriteUInt8(serversideData.angle_y);
                        writer.WriteUInt8(serversideData.angle_z);
                        writer.WriteUInt8((byte)Mathf.RoundToInt((float)(int)serversideData.barricade.health / (float)(int)serversideData.barricade.asset.health * 100f));
                        writer.WriteUInt64(serversideData.owner);
                        writer.WriteUInt64(serversideData.group);
                        writer.WriteNetId(barricadeDrop.GetNetId());
                    }
                });
                packet++;
            }
        }
        else
        {
            SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt8(x);
                writer.WriteUInt8(y);
                writer.WriteNetId(NetId.INVALID);
                writer.WriteUInt8(0);
                writer.WriteUInt16(0);
            });
        }
    }

    public static void clearPlants()
    {
        internalVehicleRegions = new List<VehicleBarricadeRegion>();
        vehicleRegions = internalVehicleRegions.AsReadOnly();
        backwardsCompatVehicleRegions = null;
    }

    [Obsolete("Plugins should not be calling this")]
    public static void waterPlant(Transform parent)
    {
        InteractableVehicle vehicle = DamageTool.getVehicle(parent);
        registerVehicleRegion(parent, vehicle, 0, NetId.INVALID);
    }

    internal static void registerVehicleRegion(Transform parent, InteractableVehicle vehicle, int subvehicleIndex, NetId netId)
    {
        VehicleBarricadeRegion vehicleBarricadeRegion = new VehicleBarricadeRegion(parent, vehicle, subvehicleIndex);
        vehicleBarricadeRegion.isNetworked = true;
        vehicleBarricadeRegion._netId = netId;
        NetIdRegistry.Assign(netId, vehicleBarricadeRegion);
        internalVehicleRegions.Add(vehicleBarricadeRegion);
        backwardsCompatVehicleRegions = null;
    }

    public static void uprootPlant(Transform parent)
    {
        for (ushort num = 0; num < vehicleRegions.Count; num = (ushort)(num + 1))
        {
            VehicleBarricadeRegion vehicleBarricadeRegion = vehicleRegions[num];
            if (vehicleBarricadeRegion.parent == parent)
            {
                vehicleBarricadeRegion.barricades.Clear();
                DestroyAllInRegion(vehicleBarricadeRegion);
                CancelInstantiationsInRegion(vehicleBarricadeRegion);
                NetIdRegistry.Release(vehicleBarricadeRegion._netId);
                internalVehicleRegions.RemoveAt(num);
                backwardsCompatVehicleRegions = null;
                break;
            }
        }
    }

    public static void trimPlant(Transform parent)
    {
        for (ushort num = 0; num < vehicleRegions.Count; num = (ushort)(num + 1))
        {
            BarricadeRegion barricadeRegion = vehicleRegions[num];
            if (barricadeRegion.parent == parent)
            {
                barricadeRegion.barricades.Clear();
                DestroyAllInRegion(barricadeRegion);
                CancelInstantiationsInRegion(barricadeRegion);
                break;
            }
        }
    }

    [Obsolete]
    public static void askPlants(CSteamID steamID)
    {
    }

    internal static void SendVehicleRegions(SteamPlayer client)
    {
        foreach (VehicleBarricadeRegion vehicleRegion in vehicleRegions)
        {
            if (vehicleRegion.drops.Count > 0)
            {
                float sqrMagnitude = (client.player.transform.position - vehicleRegion.parent.position).sqrMagnitude;
                manager.SendRegion(client, vehicleRegion, byte.MaxValue, byte.MaxValue, vehicleRegion._netId, sqrMagnitude);
            }
        }
    }

    public static BarricadeDrop FindBarricadeByRootTransform(Transform transform)
    {
        if (tryGetRegion(transform, out var _, out var _, out var _, out var region))
        {
            return region.FindBarricadeByRootTransform(transform);
        }
        return null;
    }

    [Obsolete("Please use FindBarricadeByRootTransform instead")]
    public static bool tryGetInfo(Transform barricade, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region)
    {
        x = 0;
        y = 0;
        plant = 0;
        index = 0;
        region = null;
        if (tryGetRegion(barricade, out x, out y, out plant, out region))
        {
            for (index = 0; index < region.drops.Count; index++)
            {
                if (barricade == region.drops[index].model)
                {
                    return true;
                }
            }
        }
        return false;
    }

    [Obsolete("Please use FindBarricadeByRootTransform instead")]
    public static bool tryGetInfo(Transform barricade, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop)
    {
        x = 0;
        y = 0;
        plant = 0;
        index = 0;
        region = null;
        drop = null;
        if (tryGetRegion(barricade, out x, out y, out plant, out region))
        {
            for (index = 0; index < region.drops.Count; index++)
            {
                if (barricade == region.drops[index].model)
                {
                    drop = region.drops[index];
                    return true;
                }
            }
        }
        return false;
    }

    public static bool tryGetPlant(Transform parent, out byte x, out byte y, out ushort plant, out BarricadeRegion region)
    {
        x = byte.MaxValue;
        y = byte.MaxValue;
        plant = ushort.MaxValue;
        region = null;
        if (parent == null)
        {
            return false;
        }
        for (plant = 0; plant < vehicleRegions.Count; plant++)
        {
            region = vehicleRegions[plant];
            if (region.parent == parent)
            {
                return true;
            }
        }
        return false;
    }

    public static bool tryGetRegion(Transform barricade, out byte x, out byte y, out ushort plant, out BarricadeRegion region)
    {
        x = 0;
        y = 0;
        plant = 0;
        region = null;
        if (barricade == null)
        {
            return false;
        }
        if (barricade.parent != null && barricade.parent.CompareTag("Vehicle"))
        {
            for (plant = 0; plant < vehicleRegions.Count; plant++)
            {
                region = vehicleRegions[plant];
                if (region.parent == barricade.parent)
                {
                    return true;
                }
            }
        }
        else
        {
            plant = ushort.MaxValue;
            if (Regions.tryGetCoordinate(barricade.position, out x, out y))
            {
                region = regions[x, y];
                return true;
            }
        }
        return false;
    }

    public static InteractableVehicle getVehicleFromPlant(ushort plant)
    {
        if (plant < vehicleRegions.Count)
        {
            return vehicleRegions[plant].vehicle;
        }
        return null;
    }

    public static BarricadeRegion getRegionFromVehicle(InteractableVehicle vehicle)
    {
        return findRegionFromVehicle(vehicle);
    }

    public static VehicleBarricadeRegion findRegionFromVehicle(InteractableVehicle vehicle, int subvehicleIndex = 0)
    {
        if (vehicle == null)
        {
            return null;
        }
        foreach (VehicleBarricadeRegion vehicleRegion in vehicleRegions)
        {
            if (vehicleRegion.vehicle == vehicle && vehicleRegion.subvehicleIndex == subvehicleIndex)
            {
                return vehicleRegion;
            }
        }
        return null;
    }

    public static VehicleBarricadeRegion findVehicleRegionByNetInstanceID(uint instanceID, int subvehicleIndex = 0)
    {
        foreach (VehicleBarricadeRegion vehicleRegion in vehicleRegions)
        {
            if (vehicleRegion.vehicle.instanceID == instanceID && vehicleRegion.subvehicleIndex == subvehicleIndex)
            {
                return vehicleRegion;
            }
        }
        return null;
    }

    public static VehicleBarricadeRegion FindVehicleRegionByTransform(Transform parent)
    {
        foreach (VehicleBarricadeRegion internalVehicleRegion in internalVehicleRegions)
        {
            if (internalVehicleRegion.parent == parent)
            {
                return internalVehicleRegion;
            }
        }
        return null;
    }

    public static bool tryGetRegion(byte x, byte y, ushort plant, out BarricadeRegion region)
    {
        region = null;
        if (plant < ushort.MaxValue)
        {
            if (plant < vehicleRegions.Count)
            {
                region = vehicleRegions[plant];
                return true;
            }
            return false;
        }
        if (Regions.checkSafe(x, y))
        {
            region = regions[x, y];
            return true;
        }
        return false;
    }

    [Obsolete]
    public void tellBarricadeUpdateState(CSteamID steamID, byte x, byte y, ushort plant, ushort index, byte[] newState)
    {
        throw new NotSupportedException("Moved into instance method as part of barricade NetId rewrite");
    }

    public static void updateState(Transform transform, byte[] state, int size)
    {
        updateStateInternal(transform, state, size);
    }

    public static void updateReplicatedState(Transform transform, byte[] state, int size)
    {
        updateStateInternal(transform, state, size, shouldReplicate: true);
    }

    private static void updateStateInternal(Transform transform, byte[] state, int size, bool shouldReplicate = false)
    {
        if (tryGetRegion(transform, out var x, out var y, out var plant, out var region))
        {
            BarricadeDrop barricadeDrop = region.FindBarricadeByRootTransform(transform);
            if (barricadeDrop.serversideData.barricade.state.Length != size)
            {
                barricadeDrop.serversideData.barricade.state = new byte[size];
            }
            Array.Copy(state, barricadeDrop.serversideData.barricade.state, size);
            if (shouldReplicate)
            {
                BarricadeDrop.SendUpdateState.InvokeAndLoopback(barricadeDrop.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y, plant), state);
            }
        }
    }

    private static void updateActivity(BarricadeRegion region, CSteamID owner, CSteamID group)
    {
        foreach (BarricadeDrop drop in region.drops)
        {
            BarricadeData serversideData = drop.serversideData;
            if (OwnershipTool.checkToggle(owner, serversideData.owner, group, serversideData.group))
            {
                serversideData.objActiveDate = Provider.time;
            }
        }
    }

    private static void updateActivity(CSteamID owner, CSteamID group)
    {
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                updateActivity(regions[b, b2], owner, group);
            }
        }
        for (ushort num = 0; num < vehicleRegions.Count; num = (ushort)(num + 1))
        {
            updateActivity(vehicleRegions[num], owner, group);
        }
    }

    internal static void ClearNetworkStuff()
    {
        pendingInstantiations = new List<BarricadeInstantiationParameters>();
        instantiationsToInsert = new List<BarricadeInstantiationParameters>();
        regionsPendingDestroy = new List<BarricadeRegion>();
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        regions = new BarricadeRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                regions[b, b2] = new BarricadeRegion(null);
            }
        }
        barricadeColliders = new List<Collider>();
        version = SAVEDATA_VERSION;
        instanceCount = 0u;
        pool = new Dictionary<int, Stack<GameObject>>();
        if (Provider.isServer)
        {
            load();
        }
    }

    private void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
    {
        if (step == 0)
        {
            for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
                {
                    if (Provider.isServer)
                    {
                        if (player.movement.loadedRegions[b, b2].isBarricadesLoaded && !Regions.checkArea(b, b2, new_x, new_y, BARRICADE_REGIONS))
                        {
                            player.movement.loadedRegions[b, b2].isBarricadesLoaded = false;
                        }
                    }
                    else if (player.channel.IsLocalPlayer && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, BARRICADE_REGIONS))
                    {
                        if (regions[b, b2].drops.Count > 0)
                        {
                            regions[b, b2].isPendingDestroy = true;
                            regionsPendingDestroy.Add(regions[b, b2]);
                        }
                        CancelInstantiationsInRegion(regions[b, b2]);
                        regions[b, b2].isNetworked = false;
                    }
                }
            }
        }
        if (step != 2 || !Dedicator.IsDedicatedServer || !Regions.checkSafe(new_x, new_y))
        {
            return;
        }
        Vector3 position = player.transform.position;
        for (int i = new_x - BARRICADE_REGIONS; i <= new_x + BARRICADE_REGIONS; i++)
        {
            for (int j = new_y - BARRICADE_REGIONS; j <= new_y + BARRICADE_REGIONS; j++)
            {
                if (Regions.checkSafe((byte)i, (byte)j) && !player.movement.loadedRegions[i, j].isBarricadesLoaded)
                {
                    player.movement.loadedRegions[i, j].isBarricadesLoaded = true;
                    float sortOrder = Regions.HorizontalDistanceFromCenterSquared(i, j, position);
                    SendRegion(player.channel.owner, regions[i, j], (byte)i, (byte)j, NetId.INVALID, sortOrder);
                }
            }
        }
    }

    private void onPlayerCreated(Player player)
    {
        PlayerMovement movement = player.movement;
        movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
        if (Provider.isServer)
        {
            updateActivity(player.channel.owner.playerID.steamID, player.quests.groupID);
        }
    }

    private void Start()
    {
        manager = this;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        Level.onPreLevelLoaded = (PreLevelLoaded)Delegate.Combine(Level.onPreLevelLoaded, new PreLevelLoaded(onLevelLoaded));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        int num = 0;
        int num2 = 0;
        BarricadeRegion[,] array = regions;
        foreach (BarricadeRegion barricadeRegion in array)
        {
            if (barricadeRegion.drops.Count > 0)
            {
                num++;
            }
            num2 += barricadeRegion.drops.Count;
        }
        results.Add($"Barricade regions: {num}");
        results.Add($"Barricades placed on ground: {num2}");
        int num3 = 0;
        int num4 = 0;
        foreach (VehicleBarricadeRegion internalVehicleRegion in internalVehicleRegions)
        {
            if (internalVehicleRegion.drops.Count > 0)
            {
                num3++;
            }
            num4 += internalVehicleRegion.drops.Count;
        }
        results.Add($"Barricade vehicle regions: {internalVehicleRegions.Count}");
        results.Add($"Vehicles with barricades: {num3}");
        results.Add($"Barricades placed on vehicles: {num4}");
    }

    public static void load()
    {
        bool flag = false;
        if (LevelSavedata.fileExists("/Barricades.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            River river = LevelSavedata.openRiver("/Barricades.dat", isReading: true);
            version = river.readByte();
            if (version > 6)
            {
                serverActiveDate = river.readUInt32();
            }
            else
            {
                serverActiveDate = Provider.time;
            }
            if (version < 15)
            {
                instanceCount = 0u;
            }
            else
            {
                instanceCount = river.readUInt32();
            }
            if (version > 0)
            {
                for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
                {
                    for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
                    {
                        BarricadeRegion region = regions[b, b2];
                        loadRegion(version, river, region);
                    }
                }
                if (version > 1)
                {
                    if (version > 13)
                    {
                        ushort num = river.readUInt16();
                        for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
                        {
                            uint num3 = river.readUInt32();
                            int num4 = ((version >= 16) ? river.readByte() : 0);
                            BarricadeRegion barricadeRegion = findVehicleRegionByNetInstanceID(num3, num4);
                            if (barricadeRegion == null)
                            {
                                CommandWindow.LogWarning($"Barricades associated with missing vehicle instance ID '{num3}' subindex {num4} were lost");
                                barricadeRegion = regions[0, 0];
                            }
                            loadRegion(version, river, barricadeRegion);
                        }
                    }
                    else
                    {
                        ushort a = river.readUInt16();
                        a = (ushort)Mathf.Min(a, vehicleRegions.Count);
                        for (int i = 0; i < a; i++)
                        {
                            BarricadeRegion region2 = vehicleRegions[i];
                            loadRegion(version, river, region2);
                        }
                    }
                }
            }
            if (version < 11)
            {
                flag = true;
            }
        }
        else
        {
            flag = true;
        }
        if (flag && LevelObjects.buildables != null)
        {
            int num5 = 0;
            for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3 = (byte)(b3 + 1))
            {
                for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4 = (byte)(b4 + 1))
                {
                    List<LevelBuildableObject> list = LevelObjects.buildables[b3, b4];
                    if (list != null && list.Count != 0)
                    {
                        BarricadeRegion barricadeRegion2 = regions[b3, b4];
                        for (int j = 0; j < list.Count; j++)
                        {
                            LevelBuildableObject levelBuildableObject = list[j];
                            if (levelBuildableObject != null && levelBuildableObject.asset is ItemBarricadeAsset itemBarricadeAsset)
                            {
                                Vector3 eulerAngles = levelBuildableObject.rotation.eulerAngles;
                                byte newAngle_X = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.x / 2f) * 2);
                                byte newAngle_Y = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.y / 2f) * 2);
                                byte newAngle_Z = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.z / 2f) * 2);
                                Barricade barricade = new Barricade(itemBarricadeAsset);
                                BarricadeData barricadeData = new BarricadeData(barricade, levelBuildableObject.point, newAngle_X, newAngle_Y, newAngle_Z, 0uL, 0uL, uint.MaxValue, ++instanceCount);
                                NetId netId = NetIdRegistry.ClaimBlock(3u);
                                if (manager.spawnBarricade(barricadeRegion2, barricade.asset.GUID, barricade.state, barricadeData.point, barricadeData.angle_x, barricadeData.angle_y, barricadeData.angle_z, (byte)Mathf.RoundToInt((float)(int)barricade.health / (float)(int)itemBarricadeAsset.health * 100f), 0uL, 0uL, netId) != null)
                                {
                                    barricadeRegion2.drops.GetTail().serversideData = barricadeData;
                                    barricadeRegion2.barricades.Add(barricadeData);
                                    num5++;
                                }
                                else
                                {
                                    UnturnedLog.warn($"Failed to spawn default barricade object {itemBarricadeAsset.name} at {levelBuildableObject.point}");
                                }
                            }
                        }
                    }
                }
            }
            UnturnedLog.info($"Spawned {num5} default barricades from level");
        }
        Level.isLoadingBarricades = false;
    }

    public static void save()
    {
        River river = LevelSavedata.openRiver("/Barricades.dat", isReading: false);
        river.writeByte(18);
        river.writeUInt32(Provider.time);
        river.writeUInt32(instanceCount);
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                BarricadeRegion region = regions[b, b2];
                saveRegion(river, region);
            }
        }
        ushort num = 0;
        foreach (VehicleBarricadeRegion vehicleRegion in vehicleRegions)
        {
            InteractableVehicle vehicle = vehicleRegion.vehicle;
            if (vehicle != null && !vehicle.isAutoClearable)
            {
                num = (ushort)(num + 1);
            }
        }
        river.writeUInt16(num);
        foreach (VehicleBarricadeRegion vehicleRegion2 in vehicleRegions)
        {
            InteractableVehicle vehicle2 = vehicleRegion2.vehicle;
            if (vehicle2 != null && !vehicle2.isAutoClearable)
            {
                river.writeUInt32(vehicle2.instanceID);
                river.writeByte((byte)vehicleRegion2.subvehicleIndex);
                saveRegion(river, vehicleRegion2);
            }
        }
        river.closeRiver();
    }

    [Conditional("LOG_BARRICADE_LOADING")]
    private static void LogLoading(string message)
    {
        UnturnedLog.info(message);
    }

    private static void loadRegion(byte version, River river, BarricadeRegion region)
    {
        ushort num = river.readUInt16();
        for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
        {
            ItemBarricadeAsset itemBarricadeAsset;
            if (version < 17)
            {
                ushort id = river.readUInt16();
                itemBarricadeAsset = Assets.find(EAssetType.ITEM, id) as ItemBarricadeAsset;
            }
            else
            {
                itemBarricadeAsset = Assets.find(river.readGUID()) as ItemBarricadeAsset;
            }
            uint newInstanceID = ((version >= 15) ? river.readUInt32() : (++instanceCount));
            ushort num3 = river.readUInt16();
            byte[] array = river.readBytes();
            Vector3 vector = river.readSingleVector3();
            byte b = 0;
            if (version > 2)
            {
                b = river.readByte();
            }
            byte b2 = river.readByte();
            byte b3 = 0;
            if (version > 3)
            {
                b3 = river.readByte();
            }
            ulong num4 = 0uL;
            ulong num5 = 0uL;
            if (version > 4)
            {
                num4 = river.readUInt64();
                num5 = river.readUInt64();
            }
            uint newObjActiveDate;
            if (version > 5)
            {
                newObjActiveDate = river.readUInt32();
                if (Provider.time - serverActiveDate > Provider.modeConfigData.Barricades.Decay_Time / 2u)
                {
                    newObjActiveDate = Provider.time;
                }
            }
            else
            {
                newObjActiveDate = Provider.time;
            }
            byte b4 = ((version < 18) ? byte.MaxValue : river.readByte());
            if (itemBarricadeAsset != null)
            {
                if (version >= 18 && b4 != (byte)itemBarricadeAsset.build)
                {
                    UnturnedLog.info("Discarding barricade \"" + itemBarricadeAsset.FriendlyName + "\" because asset Build property changed which might cause bigger problems (public issue #3725)");
                }
                else
                {
                    if (itemBarricadeAsset.type == EItemType.TANK && array.Length < 2)
                    {
                        array = itemBarricadeAsset.getState(EItemOrigin.ADMIN);
                    }
                    if (itemBarricadeAsset.build == EBuild.OIL && array.Length < 2)
                    {
                        array = itemBarricadeAsset.getState(EItemOrigin.ADMIN);
                    }
                    if (version < 10)
                    {
                        Vector3 eulerAngles = getRotation(itemBarricadeAsset, b * 2, b2 * 2, b3 * 2).eulerAngles;
                        b = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.x / 2f) * 2);
                        b2 = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.y / 2f) * 2);
                        b3 = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.z / 2f) * 2);
                    }
                    NetId netId = NetIdRegistry.ClaimBlock(3u);
                    if (manager.spawnBarricade(region, itemBarricadeAsset.GUID, array, vector, b, b2, b3, (byte)Mathf.RoundToInt((float)(int)num3 / (float)(int)itemBarricadeAsset.health * 100f), num4, num5, netId) != null)
                    {
                        BarricadeData item = (region.drops.GetTail().serversideData = new BarricadeData(new Barricade(itemBarricadeAsset, num3, array), vector, b, b2, b3, num4, num5, newObjActiveDate, newInstanceID));
                        region.barricades.Add(item);
                    }
                }
            }
        }
    }

    private static void saveRegion(River river, BarricadeRegion region)
    {
        uint time = Provider.time;
        ushort num = 0;
        foreach (BarricadeDrop drop in region.drops)
        {
            BarricadeData serversideData = drop.serversideData;
            if ((!Dedicator.IsDedicatedServer || Provider.modeConfigData.Barricades.Decay_Time == 0 || time < serversideData.objActiveDate || time - serversideData.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time) && serversideData.barricade.asset.isSaveable)
            {
                num = (ushort)(num + 1);
            }
        }
        river.writeUInt16(num);
        foreach (BarricadeDrop drop2 in region.drops)
        {
            BarricadeData serversideData2 = drop2.serversideData;
            if ((!Dedicator.IsDedicatedServer || Provider.modeConfigData.Barricades.Decay_Time == 0 || time < serversideData2.objActiveDate || time - serversideData2.objActiveDate < Provider.modeConfigData.Barricades.Decay_Time) && serversideData2.barricade.asset.isSaveable)
            {
                river.writeGUID(drop2.asset.GUID);
                river.writeUInt32(serversideData2.instanceID);
                river.writeUInt16(serversideData2.barricade.health);
                river.writeBytes(serversideData2.barricade.state);
                river.writeSingleVector3(serversideData2.point);
                river.writeByte(serversideData2.angle_x);
                river.writeByte(serversideData2.angle_y);
                river.writeByte(serversideData2.angle_z);
                river.writeUInt64(serversideData2.owner);
                river.writeUInt64(serversideData2.group);
                river.writeUInt32(serversideData2.objActiveDate);
                river.writeByte((byte)drop2.asset.build);
            }
        }
    }

    public static PooledTransportConnectionList GatherClientConnections(byte x, byte y, ushort plant)
    {
        if (plant == ushort.MaxValue)
        {
            return Regions.GatherClientConnections(x, y, BARRICADE_REGIONS);
        }
        return Provider.GatherClientConnections();
    }

    [Obsolete("Replaced by GatherClients")]
    public static IEnumerable<ITransportConnection> EnumerateClients(byte x, byte y, ushort plant)
    {
        return GatherClientConnections(x, y, plant);
    }

    public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y, ushort plant)
    {
        if (plant == ushort.MaxValue)
        {
            return Regions.GatherRemoteClientConnections(x, y, BARRICADE_REGIONS);
        }
        return Provider.GatherRemoteClientConnections();
    }

    [Obsolete("Replaced by GatherRemoteClients")]
    public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y, ushort plant)
    {
        return GatherRemoteClientConnections(x, y, plant);
    }

    private static void DestroyAllInRegion(BarricadeRegion region)
    {
        if (region.drops.Count > 0)
        {
            region.DestroyAll();
        }
        if (region.isPendingDestroy)
        {
            region.isPendingDestroy = false;
            regionsPendingDestroy.RemoveFast(region);
        }
    }

    private static void CancelInstantiationsInRegion(BarricadeRegion region)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].region == region)
            {
                NetInvocationDeferralRegistry.Cancel(pendingInstantiations[num].netId, 3u);
                pendingInstantiations.RemoveAt(num);
            }
        }
    }

    private static void CancelInstantiationByNetId(NetId netId)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].netId == netId)
            {
                NetInvocationDeferralRegistry.Cancel(netId, 3u);
                pendingInstantiations.RemoveAt(num);
                break;
            }
        }
    }

    internal void DestroyOrReleaseBarricade(ItemBarricadeAsset asset, GameObject instance)
    {
        EffectManager.ClearAttachments(instance.transform);
        if (asset.eligibleForPooling)
        {
            instance.SetActive(value: false);
            instance.transform.parent = null;
            int instanceID = asset.barricade.GetInstanceID();
            pool.GetOrAddNew(instanceID).Push(instance);
        }
        else
        {
            UnityEngine.Object.Destroy(instance);
        }
    }

    private void Update()
    {
        if (!Provider.isConnected)
        {
            return;
        }
        if (pendingInstantiations != null && pendingInstantiations.Count > 0)
        {
            instantiationTimer.Restart();
            int num = 0;
            do
            {
                BarricadeInstantiationParameters barricadeInstantiationParameters = pendingInstantiations[num];
                if (spawnBarricade(barricadeInstantiationParameters.region, barricadeInstantiationParameters.assetId, barricadeInstantiationParameters.state, barricadeInstantiationParameters.position, barricadeInstantiationParameters.angle_x, barricadeInstantiationParameters.angle_y, barricadeInstantiationParameters.angle_z, barricadeInstantiationParameters.hp, barricadeInstantiationParameters.owner, barricadeInstantiationParameters.group, barricadeInstantiationParameters.netId) != null)
                {
                    NetInvocationDeferralRegistry.Invoke(barricadeInstantiationParameters.netId, 3u);
                }
                else
                {
                    NetInvocationDeferralRegistry.Cancel(barricadeInstantiationParameters.netId, 3u);
                }
                num++;
            }
            while (num < pendingInstantiations.Count && (instantiationTimer.ElapsedMilliseconds < 1 || num < 5));
            pendingInstantiations.RemoveRange(0, num);
            instantiationTimer.Stop();
        }
        if (regionsPendingDestroy == null || regionsPendingDestroy.Count <= 0)
        {
            return;
        }
        instantiationTimer.Restart();
        int num2 = 0;
        do
        {
            BarricadeRegion tail = regionsPendingDestroy.GetTail();
            if (tail.drops.Count > 0)
            {
                tail.DestroyTail();
                num2++;
                if (tail.drops.Count < 1)
                {
                    tail.isPendingDestroy = false;
                    regionsPendingDestroy.RemoveTail();
                }
            }
            else
            {
                tail.isPendingDestroy = false;
                regionsPendingDestroy.RemoveTail();
            }
        }
        while (regionsPendingDestroy.Count > 0 && (instantiationTimer.ElapsedMilliseconds < 1 || num2 < 10));
        instantiationTimer.Stop();
    }
}
