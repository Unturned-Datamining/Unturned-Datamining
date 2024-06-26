using System;
using System.Collections.Generic;
using System.Diagnostics;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class StructureManager : SteamCaller
{
    public const byte SAVEDATA_VERSION_INITIAL = 8;

    public const byte SAVEDATA_VERSION_REPLACE_EULER_ANGLES_WITH_QUATERNION = 9;

    private const byte SAVEDATA_VERSION_NEWEST = 9;

    public static readonly byte SAVEDATA_VERSION = 9;

    public static readonly byte STRUCTURE_REGIONS = 2;

    public static readonly float WALL = 2.125f;

    public static readonly float PILLAR = 3.1f;

    public static readonly float HEIGHT = 2.125f;

    public static DeployStructureRequestHandler onDeployStructureRequested;

    [Obsolete("Please use StructureDrop.OnSalvageRequested_Global instead")]
    public static SalvageStructureRequestHandler onSalvageStructureRequested;

    public static DamageStructureRequestHandler onDamageStructureRequested;

    public static RepairStructureRequestHandler OnRepairRequested;

    public static RepairedStructureHandler OnRepaired;

    public static StructureSpawnedHandler onStructureSpawned;

    public static TransformStructureRequestHandler onTransformRequested;

    private static StructureManager manager;

    internal static HousingConnections housingConnections;

    private static List<StructureInstantiationParameters> pendingInstantiations;

    private static List<StructureInstantiationParameters> instantiationsToInsert;

    private static List<StructureRegion> regionsPendingDestroy;

    private static uint instanceCount;

    private static uint serverActiveDate;

    private static readonly ClientStaticMethod<NetId, Vector3, bool> SendDestroyStructure = ClientStaticMethod<NetId, Vector3, bool>.Get(ReceiveDestroyStructure);

    private static readonly ClientStaticMethod<byte, byte> SendClearRegionStructures = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionStructures);

    private static readonly ClientStaticMethod<byte, byte, Guid, Vector3, Quaternion, ulong, ulong, NetId> SendSingleStructure = ClientStaticMethod<byte, byte, Guid, Vector3, Quaternion, ulong, ulong, NetId>.Get(ReceiveSingleStructure);

    private static ClientStaticMethod SendMultipleStructures = ClientStaticMethod.Get(ReceiveMultipleStructures);

    /// <summary>
    /// Maps prefab unique id to inactive list.
    /// </summary>
    private Dictionary<int, Stack<GameObject>> pool;

    private Stopwatch instantiationTimer = new Stopwatch();

    /// <summary>
    /// Instantiate at least this many structures per frame even if we exceed our time budget.
    /// </summary>
    private const int MIN_INSTANTIATIONS_PER_FRAME = 5;

    private const int MIN_DESTROY_PER_FRAME = 10;

    internal const int POSITION_FRAC_BIT_COUNT = 11;

    /// <summary>
    /// +0 = StructureDrop
    /// +1 = root transform
    /// </summary>
    internal const int NETIDS_PER_STRUCTURE = 2;

    /// <summary>
    /// Exposed for Rocket transition to modules backwards compatibility.
    /// </summary>
    public static StructureManager instance => manager;

    public static StructureRegion[,] regions { get; private set; }

    public static void getStructuresInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
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
            foreach (StructureDrop drop in regions[regionCoordinate.x, regionCoordinate.y].drops)
            {
                if ((drop.model.position - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(drop.model);
                }
            }
        }
    }

    [Obsolete]
    public void tellStructureOwnerAndGroup(CSteamID steamID, byte x, byte y, ushort index, ulong newOwner, ulong newGroup)
    {
        throw new NotSupportedException("Moved into instance method as part of structure NetId rewrite");
    }

    public static void changeOwnerAndGroup(Transform transform, ulong newOwner, ulong newGroup)
    {
        if (tryGetRegion(transform, out var x, out var y, out var region))
        {
            StructureDrop structureDrop = region.FindStructureByRootTransform(transform);
            if (structureDrop != null)
            {
                StructureDrop.SendOwnerAndGroup.InvokeAndLoopback(structureDrop.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y), newOwner, newGroup);
                structureDrop.serversideData.owner = newOwner;
                structureDrop.serversideData.group = newGroup;
                sendHealthChanged(x, y, structureDrop);
            }
        }
    }

    public static void transformStructure(Transform transform, Vector3 point, Quaternion rotation)
    {
        StructureDrop structureDrop = StructureDrop.FindByRootFast(transform);
        if (structureDrop != null)
        {
            StructureDrop.SendTransformRequest.Invoke(structureDrop.GetNetId(), ENetReliability.Reliable, point, rotation);
        }
    }

    [Obsolete]
    public void tellTransformStructure(CSteamID steamID, byte x, byte y, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        throw new NotSupportedException("Moved into instance method as part of structure NetId rewrite");
    }

    public static bool ServerSetStructureTransform(Transform transform, Vector3 position, Quaternion rotation)
    {
        if (!tryGetRegion(transform, out var x, out var y, out var region))
        {
            return false;
        }
        StructureDrop structureDrop = region.FindStructureByRootTransform(transform);
        if (structureDrop == null)
        {
            return false;
        }
        InternalSetStructureTransform(x, y, structureDrop, position, rotation);
        return true;
    }

    internal static void InternalSetStructureTransform(byte x, byte y, StructureDrop drop, Vector3 point, Quaternion rotation)
    {
        StructureDrop.SendTransform.InvokeAndLoopback(drop.GetNetId(), ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, point, rotation);
    }

    [Obsolete]
    public void askTransformStructure(CSteamID steamID, byte x, byte y, uint instanceID, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        throw new NotSupportedException("Moved into instance method as part of structure NetId rewrite");
    }

    [Obsolete]
    public void tellStructureHealth(CSteamID steamID, byte x, byte y, ushort index, byte hp)
    {
        throw new NotSupportedException("Moved into instance method as part of structure NetId rewrite");
    }

    public static void salvageStructure(Transform transform)
    {
        StructureDrop structureDrop = FindStructureByRootTransform(transform);
        if (structureDrop != null)
        {
            StructureDrop.SendSalvageRequest.Invoke(structureDrop.GetNetId(), ENetReliability.Reliable);
        }
    }

    [Obsolete]
    public void askSalvageStructure(CSteamID steamID, byte x, byte y, ushort index)
    {
        throw new NotSupportedException("Moved into instance method as part of structure NetId rewrite");
    }

    public static void damage(Transform transform, Vector3 direction, float damage, float times, bool armor, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (!tryGetRegion(transform, out var x, out var y, out var region))
        {
            return;
        }
        StructureDrop structureDrop = region.FindStructureByRootTransform(transform);
        if (structureDrop == null || structureDrop.serversideData.structure.isDead)
        {
            return;
        }
        ItemStructureAsset asset = structureDrop.asset;
        if (asset == null || !asset.canBeDamaged)
        {
            return;
        }
        if (armor)
        {
            times *= Provider.modeConfigData.Structures.getArmorMultiplier(asset.armorTier);
        }
        ushort pendingTotalDamage = (ushort)(damage * times);
        bool shouldAllow = true;
        onDamageStructureRequested?.Invoke(instigatorSteamID, transform, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        if (!shouldAllow || pendingTotalDamage < 1)
        {
            return;
        }
        structureDrop.serversideData.structure.askDamage(pendingTotalDamage);
        if (structureDrop.serversideData.structure.isDead)
        {
            EffectAsset effectAsset = asset.FindExplosionEffectAsset();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.position = transform.position + Vector3.down * HEIGHT;
                parameters.relevantDistance = EffectManager.MEDIUM;
                EffectManager.triggerEffect(parameters);
            }
            asset.SpawnItemDropsOnDestroy(transform.position);
            destroyStructure(structureDrop, x, y, direction * (int)pendingTotalDamage, wasPickedUp: false);
        }
        else
        {
            sendHealthChanged(x, y, structureDrop);
        }
    }

    [Obsolete("Please replace the methods which take an index")]
    public static void destroyStructure(StructureRegion region, byte x, byte y, ushort index, Vector3 ragdoll)
    {
        destroyStructure(region.drops[index], x, y, ragdoll, wasPickedUp: false);
    }

    public static void destroyStructure(StructureDrop structure, byte x, byte y, Vector3 ragdoll)
    {
        destroyStructure(structure, x, y, ragdoll, wasPickedUp: false);
    }

    /// <summary>
    /// Remove structure instance on server and client.
    /// </summary>
    public static void destroyStructure(StructureDrop structure, byte x, byte y, Vector3 ragdoll, bool wasPickedUp)
    {
        if (tryGetRegion(x, y, out var region))
        {
            region.structures.Remove(structure.serversideData);
            SendDestroyStructure.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), structure.GetNetId(), ragdoll, wasPickedUp);
        }
    }

    /// <summary>
    /// Used by ownership change and damaged event to tell relevant clients the new health.
    /// </summary>
    private static void sendHealthChanged(byte x, byte y, StructureDrop structure)
    {
        StructureDrop.SendHealth.Invoke(structure.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) => client.player != null && OwnershipTool.checkToggle(client.playerID.steamID, structure.serversideData.owner, client.player.quests.groupID, structure.serversideData.group) && Regions.checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, STRUCTURE_REGIONS)), (byte)Mathf.RoundToInt((float)(int)structure.serversideData.structure.health / (float)(int)structure.asset.health * 100f));
    }

    public static void repair(Transform structure, float damage, float times)
    {
        repair(structure, damage, times, default(CSteamID));
    }

    public static void repair(Transform transform, float damage, float times, CSteamID instigatorSteamID = default(CSteamID))
    {
        if (!tryGetRegion(transform, out var x, out var y, out var region))
        {
            return;
        }
        StructureDrop structureDrop = region.FindStructureByRootTransform(transform);
        if (structureDrop != null && !structureDrop.serversideData.structure.isDead && !structureDrop.serversideData.structure.isRepaired)
        {
            float pendingTotalHealing = damage * times;
            bool shouldAllow = true;
            OnRepairRequested?.Invoke(instigatorSteamID, transform, ref pendingTotalHealing, ref shouldAllow);
            ushort num = MathfEx.RoundAndClampToUShort(pendingTotalHealing);
            if (shouldAllow && num >= 1)
            {
                structureDrop.serversideData.structure.askRepair(num);
                sendHealthChanged(x, y, structureDrop);
                OnRepaired?.Invoke(instigatorSteamID, transform, (int)num);
            }
        }
    }

    public static StructureDrop FindStructureByRootTransform(Transform transform)
    {
        if (tryGetRegion(transform, out var _, out var _, out var region))
        {
            return region.FindStructureByRootTransform(transform);
        }
        return null;
    }

    [Obsolete("Please use FindStructureByRootTransform instead")]
    public static bool tryGetInfo(Transform structure, out byte x, out byte y, out ushort index, out StructureRegion region)
    {
        x = 0;
        y = 0;
        index = 0;
        region = null;
        if (tryGetRegion(structure, out x, out y, out region))
        {
            for (index = 0; index < region.drops.Count; index++)
            {
                if (structure == region.drops[index].model)
                {
                    return true;
                }
            }
        }
        return false;
    }

    [Obsolete("Please use FindStructureByRootTransform instead")]
    public static bool tryGetInfo(Transform structure, out byte x, out byte y, out ushort index, out StructureRegion region, out StructureDrop drop)
    {
        x = 0;
        y = 0;
        index = 0;
        region = null;
        drop = null;
        if (tryGetRegion(structure, out x, out y, out region))
        {
            for (index = 0; index < region.drops.Count; index++)
            {
                if (structure == region.drops[index].model)
                {
                    drop = region.drops[index];
                    return true;
                }
            }
        }
        return false;
    }

    public static bool tryGetRegion(Transform structure, out byte x, out byte y, out StructureRegion region)
    {
        x = 0;
        y = 0;
        region = null;
        if (structure == null)
        {
            return false;
        }
        if (Regions.tryGetCoordinate(structure.position, out x, out y))
        {
            region = regions[x, y];
            return true;
        }
        return false;
    }

    public static bool tryGetRegion(byte x, byte y, out StructureRegion region)
    {
        region = null;
        if (Regions.checkSafe(x, y))
        {
            region = regions[x, y];
            return true;
        }
        return false;
    }

    /// <summary>
    /// Legacy function for UseableStructure.
    /// </summary>
    public static bool dropStructure(Structure structure, Vector3 point, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
    {
        if (structure.asset == null)
        {
            return false;
        }
        bool shouldAllow = true;
        onDeployStructureRequested?.Invoke(structure, structure.asset, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        if (!shouldAllow)
        {
            return false;
        }
        Quaternion rotation = Quaternion.Euler(-90f, angle_y, 0f);
        return dropReplicatedStructure(structure, point, rotation, owner, group);
    }

    /// <summary>
    /// Spawn a new structure and replicate it.
    /// </summary>
    public static bool dropReplicatedStructure(Structure structure, Vector3 point, Quaternion rotation, ulong owner, ulong group)
    {
        if (!Regions.tryGetCoordinate(point, out var x, out var y))
        {
            return false;
        }
        if (!tryGetRegion(x, y, out var region))
        {
            return false;
        }
        StructureData structureData = new StructureData(structure, point, rotation, owner, group, Provider.time, ++instanceCount);
        NetId netId = NetIdRegistry.ClaimBlock(2u);
        if (manager.spawnStructure(region, structure.asset.GUID, structureData.point, structureData.rotation, 100, structureData.owner, structureData.group, netId) != null)
        {
            StructureDrop tail = region.drops.GetTail();
            tail.serversideData = structureData;
            region.structures.Add(structureData);
            SendSingleStructure.Invoke(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y, structure.asset.GUID, structureData.point, structureData.rotation, structureData.owner, structureData.group, netId);
            onStructureSpawned?.Invoke(region, tail);
        }
        return true;
    }

    [Obsolete]
    public void tellTakeStructure(CSteamID steamID, byte x, byte y, ushort index, Vector3 ragdoll)
    {
        throw new NotSupportedException("Removed during structure NetId rewrite");
    }

    /// <summary>
    /// Not an instance method because structure might not exist yet, in which case we cancel instantiation.
    /// </summary>
    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveDestroyStructure(in ClientInvocationContext context, NetId netId, Vector3 ragdoll, bool wasPickedUp)
    {
        StructureDrop structureDrop = NetIdRegistry.Get<StructureDrop>(netId);
        if (structureDrop == null)
        {
            CancelInstantiationByNetId(netId);
        }
        else
        {
            if (!tryGetRegion(structureDrop.model, out var _, out var _, out var region))
            {
                return;
            }
            if (Dedicator.IsDedicatedServer || !GraphicsSettings.debris || wasPickedUp)
            {
                instance.DestroyOrReleaseStructure(structureDrop);
                structureDrop.model.position = Vector3.zero;
            }
            else
            {
                ItemStructureAsset asset = structureDrop.asset;
                if (asset != null && asset.construct != 0 && asset.construct != EConstruct.ROOF && asset.construct != EConstruct.FLOOR_POLY && asset.construct != EConstruct.ROOF_POLY)
                {
                    Vector3 position = structureDrop.model.position;
                    Quaternion rotation = structureDrop.model.rotation;
                    instance.DestroyOrReleaseStructure(structureDrop);
                    structureDrop.model.position = Vector3.zero;
                    GameObject gameObject = UnityEngine.Object.Instantiate(asset.structure, position, rotation);
                    Transform transform = gameObject.transform;
                    ragdoll.y += 8f;
                    ragdoll.x += UnityEngine.Random.Range(-16f, 16f);
                    ragdoll.z += UnityEngine.Random.Range(-16f, 16f);
                    ragdoll *= 2f;
                    EffectManager.RegisterDebris(gameObject);
                    MeshCollider component = gameObject.GetComponent<MeshCollider>();
                    if (component != null)
                    {
                        component.convex = true;
                    }
                    foreach (Transform item in transform)
                    {
                        if (item.CompareTag("Logic"))
                        {
                            UnityEngine.Object.Destroy(item.gameObject);
                        }
                    }
                    gameObject.tag = "Debris";
                    gameObject.SetLayerRecursively(12);
                    Rigidbody orAddComponent = gameObject.GetOrAddComponent<Rigidbody>();
                    orAddComponent.useGravity = true;
                    orAddComponent.isKinematic = false;
                    orAddComponent.AddForce(ragdoll);
                    orAddComponent.drag = 0.5f;
                    orAddComponent.angularDrag = 0.1f;
                    transform.localScale *= 0.75f;
                    UnityEngine.Object.Destroy(gameObject, 8f);
                }
                else
                {
                    instance.DestroyOrReleaseStructure(structureDrop);
                    structureDrop.model.position = Vector3.zero;
                }
            }
            structureDrop.ReleaseNetId();
            region.drops.Remove(structureDrop);
        }
    }

    [Obsolete]
    public void tellClearRegionStructures(CSteamID steamID, byte x, byte y)
    {
        ReceiveClearRegionStructures(x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellClearRegionStructures")]
    public static void ReceiveClearRegionStructures(byte x, byte y)
    {
        if (Provider.isServer || regions[x, y].isNetworked)
        {
            StructureRegion region = regions[x, y];
            DestroyAllInRegion(region);
            CancelInstantiationsInRegion(region);
        }
    }

    public static void askClearRegionStructures(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y))
        {
            StructureRegion structureRegion = regions[x, y];
            if (structureRegion.drops.Count > 0)
            {
                structureRegion.structures.Clear();
                SendClearRegionStructures.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(x, y), x, y);
            }
        }
    }

    public static void askClearAllStructures()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                askClearRegionStructures(b, b2);
            }
        }
    }

    private Transform spawnStructure(StructureRegion region, Guid assetId, Vector3 point, Quaternion rotation, byte hp, ulong owner, ulong group, NetId netId)
    {
        ItemStructureAsset itemStructureAsset = Assets.find(assetId) as ItemStructureAsset;
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(assetId, itemStructureAsset, "Structure");
        }
        if (itemStructureAsset == null || itemStructureAsset.structure == null)
        {
            return null;
        }
        Transform transform = null;
        try
        {
            if (itemStructureAsset.eligibleForPooling)
            {
                int instanceID = itemStructureAsset.structure.GetInstanceID();
                Stack<GameObject> orAddNew = pool.GetOrAddNew(instanceID);
                while (orAddNew.Count > 0)
                {
                    GameObject gameObject = orAddNew.Pop();
                    if (gameObject != null)
                    {
                        transform = gameObject.transform;
                        transform.SetPositionAndRotation(point, rotation);
                        gameObject.SetActive(value: true);
                        break;
                    }
                }
            }
            if (transform == null)
            {
                GameObject obj = UnityEngine.Object.Instantiate(itemStructureAsset.structure, point, rotation);
                transform = obj.transform;
                obj.name = itemStructureAsset.id.ToString();
                if (Provider.isServer && itemStructureAsset.nav != null)
                {
                    Transform obj2 = UnityEngine.Object.Instantiate(itemStructureAsset.nav).transform;
                    obj2.name = "Nav";
                    obj2.parent = transform;
                    obj2.localPosition = Vector3.zero;
                    obj2.localRotation = Quaternion.identity;
                }
            }
            if (!itemStructureAsset.isUnpickupable)
            {
                Interactable2HP orAddComponent = transform.GetOrAddComponent<Interactable2HP>();
                orAddComponent.hp = hp;
                Interactable2SalvageStructure orAddComponent2 = transform.GetOrAddComponent<Interactable2SalvageStructure>();
                orAddComponent2.hp = orAddComponent;
                orAddComponent2.owner = owner;
                orAddComponent2.group = group;
                orAddComponent2.salvageDurationMultiplier = itemStructureAsset.salvageDurationMultiplier;
            }
            StructureDrop structureDrop = new StructureDrop(transform, itemStructureAsset);
            transform.GetOrAddComponent<StructureRefComponent>().tempNotSureIfStructureShouldBeAComponentYet = structureDrop;
            structureDrop.AssignNetId(netId);
            region.drops.Add(structureDrop);
            if (transform != null)
            {
                structureDrop.AddFoliageCut();
                try
                {
                    housingConnections.LinkConnections(structureDrop);
                }
                catch (Exception e)
                {
                    UnturnedLog.exception(e, "Caught exception while linking housing connections:");
                }
            }
        }
        catch (Exception e2)
        {
            UnturnedLog.warn("Exception while spawning structure: {0}", itemStructureAsset);
            UnturnedLog.exception(e2);
            if (transform != null)
            {
                UnityEngine.Object.Destroy(transform.gameObject);
                transform = null;
            }
        }
        return transform;
    }

    [Obsolete]
    public void tellStructure(CSteamID steamID, byte x, byte y, ushort id, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, uint instanceID)
    {
        throw new NotSupportedException("Structures no longer function without a unique NetId");
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellStructure")]
    public static void ReceiveSingleStructure(byte x, byte y, Guid id, Vector3 point, Quaternion rotation, ulong owner, ulong group, NetId netId)
    {
        if (tryGetRegion(x, y, out var region) && (Provider.isServer || region.isNetworked))
        {
            float sortOrder = 0f;
            if (MainCamera.instance != null)
            {
                sortOrder = (MainCamera.instance.transform.position - point).sqrMagnitude;
            }
            StructureInstantiationParameters item = default(StructureInstantiationParameters);
            item.region = region;
            item.assetId = id;
            item.position = point;
            item.rotation = rotation;
            item.hp = 100;
            item.owner = owner;
            item.group = group;
            item.netId = netId;
            item.sortOrder = sortOrder;
            NetInvocationDeferralRegistry.MarkDeferred(item.netId, 2u);
            pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(item), item);
        }
    }

    [Obsolete]
    public void tellStructures(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveMultipleStructures(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        if (!tryGetRegion(value, value2, out var region))
        {
            return;
        }
        reader.ReadUInt8(out var value3);
        if (value3 == 0)
        {
            if (region.isNetworked)
            {
                return;
            }
            DestroyAllInRegion(regions[value, value2]);
        }
        else if (!region.isNetworked)
        {
            return;
        }
        region.isNetworked = true;
        reader.ReadUInt16(out var value4);
        if (value4 > 0)
        {
            reader.ReadFloat(out var value5);
            instantiationsToInsert.Clear();
            for (ushort num = 0; num < value4; num++)
            {
                StructureInstantiationParameters item = default(StructureInstantiationParameters);
                item.region = region;
                item.sortOrder = value5;
                reader.ReadGuid(out item.assetId);
                reader.ReadClampedVector3(out item.position, 13, 11);
                reader.ReadQuaternion(out item.rotation);
                reader.ReadUInt8(out item.hp);
                reader.ReadUInt64(out item.owner);
                reader.ReadUInt64(out item.group);
                reader.ReadNetId(out item.netId);
                NetInvocationDeferralRegistry.MarkDeferred(item.netId, 2u);
                instantiationsToInsert.Add(item);
            }
            pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
        }
        Level.isLoadingStructures = false;
    }

    [Obsolete]
    public void askStructures(CSteamID steamID, byte x, byte y)
    {
    }

    internal void askStructures(ITransportConnection transportConnection, byte x, byte y, float sortOrder)
    {
        if (!tryGetRegion(x, y, out var region))
        {
            return;
        }
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
                    num += 44;
                    count++;
                    if (num > Block.BUFFER_SIZE / 2)
                    {
                        break;
                    }
                }
                SendMultipleStructures.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
                {
                    writer.WriteUInt8(x);
                    writer.WriteUInt8(y);
                    writer.WriteUInt8(packet);
                    writer.WriteUInt16((ushort)(count - index));
                    writer.WriteFloat(sortOrder);
                    for (; index < count; index++)
                    {
                        StructureData serversideData = region.drops[index].serversideData;
                        writer.WriteGuid(serversideData.structure.asset.GUID);
                        writer.WriteClampedVector3(serversideData.point, 13, 11);
                        writer.WriteQuaternion(serversideData.rotation);
                        writer.WriteUInt8((byte)Mathf.RoundToInt((float)(int)serversideData.structure.health / (float)(int)serversideData.structure.asset.health * 100f));
                        writer.WriteUInt64(serversideData.owner);
                        writer.WriteUInt64(serversideData.group);
                        writer.WriteNetId(region.drops[index].GetNetId());
                    }
                });
                packet++;
            }
        }
        else
        {
            SendMultipleStructures.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt8(x);
                writer.WriteUInt8(y);
                writer.WriteUInt8(0);
                writer.WriteUInt16(0);
            });
        }
    }

    private static void updateActivity(StructureRegion region, CSteamID owner, CSteamID group)
    {
        foreach (StructureDrop drop in region.drops)
        {
            StructureData serversideData = drop.serversideData;
            if (OwnershipTool.checkToggle(owner, serversideData.owner, group, serversideData.group))
            {
                serversideData.objActiveDate = Provider.time;
            }
        }
    }

    private static void updateActivity(CSteamID owner, CSteamID group)
    {
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                updateActivity(regions[b, b2], owner, group);
            }
        }
    }

    /// <summary>
    /// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
    /// </summary>
    internal static void ClearNetworkStuff()
    {
        pendingInstantiations = new List<StructureInstantiationParameters>();
        instantiationsToInsert = new List<StructureInstantiationParameters>();
        regionsPendingDestroy = new List<StructureRegion>();
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        regions = new StructureRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                regions[b, b2] = new StructureRegion();
            }
        }
        instanceCount = 0u;
        pool = new Dictionary<int, Stack<GameObject>>();
        housingConnections = new HousingConnections();
        if (Provider.isServer)
        {
            load();
        }
    }

    private void onRegionUpdated(Player player, byte old_x, byte old_y, byte new_x, byte new_y, byte step, ref bool canIncrementIndex)
    {
        if (step == 0)
        {
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    if (Provider.isServer)
                    {
                        if (player.movement.loadedRegions[b, b2].isStructuresLoaded && !Regions.checkArea(b, b2, new_x, new_y, STRUCTURE_REGIONS))
                        {
                            player.movement.loadedRegions[b, b2].isStructuresLoaded = false;
                        }
                    }
                    else if (player.channel.IsLocalPlayer && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, STRUCTURE_REGIONS))
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
        if (step != 1 || !Dedicator.IsDedicatedServer || !Regions.checkSafe(new_x, new_y))
        {
            return;
        }
        Vector3 position = player.transform.position;
        for (int i = new_x - STRUCTURE_REGIONS; i <= new_x + STRUCTURE_REGIONS; i++)
        {
            for (int j = new_y - STRUCTURE_REGIONS; j <= new_y + STRUCTURE_REGIONS; j++)
            {
                if (Regions.checkSafe((byte)i, (byte)j) && !player.movement.loadedRegions[i, j].isStructuresLoaded)
                {
                    player.movement.loadedRegions[i, j].isStructuresLoaded = true;
                    float sortOrder = Regions.HorizontalDistanceFromCenterSquared(i, j, position);
                    askStructures(player.channel.owner.transportConnection, (byte)i, (byte)j, sortOrder);
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
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        int num = 0;
        int num2 = 0;
        StructureRegion[,] array = regions;
        foreach (StructureRegion structureRegion in array)
        {
            if (structureRegion.drops.Count > 0)
            {
                num++;
            }
            num2 += structureRegion.drops.Count;
        }
        results.Add($"Structure regions: {num}");
        results.Add($"Structures placed: {num2}");
        if (housingConnections != null)
        {
            housingConnections.OnLogMemoryUsage(results);
        }
    }

    public static void load()
    {
        bool flag = false;
        if (LevelSavedata.fileExists("/Structures.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            River river = LevelSavedata.openRiver("/Structures.dat", isReading: true);
            byte b = river.readByte();
            if (b > 3)
            {
                serverActiveDate = river.readUInt32();
            }
            else
            {
                serverActiveDate = Provider.time;
            }
            if (b < 7)
            {
                instanceCount = 0u;
            }
            else
            {
                instanceCount = river.readUInt32();
            }
            if (b > 1)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3++)
                    {
                        StructureRegion region = regions[b2, b3];
                        loadRegion(b, river, region);
                    }
                }
            }
            if (b < 6)
            {
                flag = true;
            }
            river.closeRiver();
        }
        else
        {
            flag = true;
        }
        if (flag && LevelObjects.buildables != null)
        {
            int num = 0;
            for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4++)
            {
                for (byte b5 = 0; b5 < Regions.WORLD_SIZE; b5++)
                {
                    List<LevelBuildableObject> list = LevelObjects.buildables[b4, b5];
                    if (list != null && list.Count != 0)
                    {
                        StructureRegion structureRegion = regions[b4, b5];
                        for (int i = 0; i < list.Count; i++)
                        {
                            LevelBuildableObject levelBuildableObject = list[i];
                            if (levelBuildableObject != null && levelBuildableObject.asset is ItemStructureAsset itemStructureAsset)
                            {
                                Structure structure = new Structure(itemStructureAsset, itemStructureAsset.health);
                                StructureData structureData = new StructureData(structure, levelBuildableObject.point, levelBuildableObject.rotation, 0uL, 0uL, uint.MaxValue, ++instanceCount);
                                NetId netId = NetIdRegistry.ClaimBlock(2u);
                                if (manager.spawnStructure(structureRegion, itemStructureAsset.GUID, structureData.point, structureData.rotation, (byte)Mathf.RoundToInt((float)(int)structure.health / (float)(int)itemStructureAsset.health * 100f), 0uL, 0uL, netId) != null)
                                {
                                    structureRegion.drops.GetTail().serversideData = structureData;
                                    structureRegion.structures.Add(structureData);
                                    num++;
                                }
                                else
                                {
                                    UnturnedLog.warn($"Failed to spawn default structure object {itemStructureAsset.name} at {levelBuildableObject.point}");
                                }
                            }
                        }
                    }
                }
            }
            UnturnedLog.info($"Spawned {num} default structures from level");
        }
        Level.isLoadingStructures = false;
    }

    public static void save()
    {
        River river = LevelSavedata.openRiver("/Structures.dat", isReading: false);
        river.writeByte(9);
        river.writeUInt32(Provider.time);
        river.writeUInt32(instanceCount);
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                StructureRegion region = regions[b, b2];
                saveRegion(river, region);
            }
        }
        river.closeRiver();
    }

    private static void loadRegion(byte version, River river, StructureRegion region)
    {
        ushort num = river.readUInt16();
        for (ushort num2 = 0; num2 < num; num2++)
        {
            ItemStructureAsset itemStructureAsset;
            if (version < 8)
            {
                ushort id = river.readUInt16();
                itemStructureAsset = Assets.find(EAssetType.ITEM, id) as ItemStructureAsset;
            }
            else
            {
                itemStructureAsset = Assets.find(river.readGUID()) as ItemStructureAsset;
            }
            uint newInstanceID = ((version >= 7) ? river.readUInt32() : (++instanceCount));
            ushort num3 = river.readUInt16();
            Vector3 vector = river.readSingleVector3();
            Quaternion quaternion;
            if (version < 9)
            {
                byte b = 0;
                if (version > 4)
                {
                    b = river.readByte();
                }
                byte b2 = river.readByte();
                byte b3 = 0;
                if (version > 4)
                {
                    b3 = river.readByte();
                }
                quaternion = ((version >= 5) ? Quaternion.Euler((float)(int)b * 2f, (float)(int)b2 * 2f, (float)(int)b3 * 2f) : Quaternion.Euler(-90f, b2 * 2, 0f));
            }
            else
            {
                quaternion = river.readSingleQuaternion();
            }
            ulong num4 = 0uL;
            ulong num5 = 0uL;
            if (version > 2)
            {
                num4 = river.readUInt64();
                num5 = river.readUInt64();
            }
            uint newObjActiveDate;
            if (version > 3)
            {
                newObjActiveDate = river.readUInt32();
                if (Provider.time - serverActiveDate > Provider.modeConfigData.Structures.Decay_Time / 2)
                {
                    newObjActiveDate = Provider.time;
                }
            }
            else
            {
                newObjActiveDate = Provider.time;
            }
            if (itemStructureAsset != null)
            {
                NetId netId = NetIdRegistry.ClaimBlock(2u);
                if (manager.spawnStructure(region, itemStructureAsset.GUID, vector, quaternion, (byte)Mathf.RoundToInt((float)(int)num3 / (float)(int)itemStructureAsset.health * 100f), num4, num5, netId) != null)
                {
                    StructureData item = (region.drops.GetTail().serversideData = new StructureData(new Structure(itemStructureAsset, num3), vector, quaternion, num4, num5, newObjActiveDate, newInstanceID));
                    region.structures.Add(item);
                }
            }
        }
    }

    private static void saveRegion(River river, StructureRegion region)
    {
        uint time = Provider.time;
        ushort num = 0;
        foreach (StructureDrop drop in region.drops)
        {
            StructureData serversideData = drop.serversideData;
            if ((!Dedicator.IsDedicatedServer || Provider.modeConfigData.Structures.Decay_Time == 0 || time < serversideData.objActiveDate || time - serversideData.objActiveDate < Provider.modeConfigData.Structures.Decay_Time) && serversideData.structure.asset.isSaveable)
            {
                num++;
            }
        }
        river.writeUInt16(num);
        foreach (StructureDrop drop2 in region.drops)
        {
            StructureData serversideData2 = drop2.serversideData;
            if ((!Dedicator.IsDedicatedServer || Provider.modeConfigData.Structures.Decay_Time == 0 || time < serversideData2.objActiveDate || time - serversideData2.objActiveDate < Provider.modeConfigData.Structures.Decay_Time) && serversideData2.structure.asset.isSaveable)
            {
                river.writeGUID(drop2.asset.GUID);
                river.writeUInt32(serversideData2.instanceID);
                river.writeUInt16(serversideData2.structure.health);
                river.writeSingleVector3(serversideData2.point);
                river.writeSingleQuaternion(serversideData2.rotation);
                river.writeUInt64(serversideData2.owner);
                river.writeUInt64(serversideData2.group);
                river.writeUInt32(serversideData2.objActiveDate);
            }
        }
    }

    public static PooledTransportConnectionList GatherRemoteClientConnections(byte x, byte y)
    {
        return Regions.GatherRemoteClientConnections(x, y, STRUCTURE_REGIONS);
    }

    [Obsolete("Replaced by GatherRemoteClientConnections")]
    public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y)
    {
        return GatherRemoteClientConnections(x, y);
    }

    private static void DestroyAllInRegion(StructureRegion region)
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

    private static void CancelInstantiationsInRegion(StructureRegion region)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].region == region)
            {
                NetInvocationDeferralRegistry.Cancel(pendingInstantiations[num].netId, 2u);
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
                NetInvocationDeferralRegistry.Cancel(netId, 2u);
                pendingInstantiations.RemoveAt(num);
                break;
            }
        }
    }

    internal void DestroyOrReleaseStructure(StructureDrop drop)
    {
        try
        {
            housingConnections.UnlinkConnections(drop);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception while unlinking housing connections:");
        }
        drop.RemoveFoliageCut();
        EffectManager.ClearAttachments(drop.model);
        if (drop.asset.eligibleForPooling)
        {
            drop.model.gameObject.SetActive(value: false);
            int instanceID = drop.asset.structure.GetInstanceID();
            pool.GetOrAddNew(instanceID).Push(drop.model.gameObject);
        }
        else
        {
            UnityEngine.Object.Destroy(drop.model.gameObject);
        }
    }

    private void Update()
    {
        if (MainCamera.instance != null)
        {
            housingConnections.DrawGizmos();
        }
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
                StructureInstantiationParameters structureInstantiationParameters = pendingInstantiations[num];
                if (spawnStructure(structureInstantiationParameters.region, structureInstantiationParameters.assetId, structureInstantiationParameters.position, structureInstantiationParameters.rotation, structureInstantiationParameters.hp, structureInstantiationParameters.owner, structureInstantiationParameters.group, structureInstantiationParameters.netId) != null)
                {
                    NetInvocationDeferralRegistry.Invoke(structureInstantiationParameters.netId, 2u);
                }
                else
                {
                    NetInvocationDeferralRegistry.Cancel(structureInstantiationParameters.netId, 2u);
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
            StructureRegion tail = regionsPendingDestroy.GetTail();
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
