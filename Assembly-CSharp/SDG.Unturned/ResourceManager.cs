using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ResourceManager : SteamCaller
{
    public static readonly byte RESOURCE_REGIONS = 2;

    public static DamageResourceRequestHandler onDamageResourceRequested;

    private static ResourceManager manager;

    private static ResourceRegion[,] regions;

    private static byte respawnResources_X;

    private static byte respawnResources_Y;

    private static readonly ClientStaticMethod<byte, byte> SendClearRegionResources = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionResources);

    private static readonly ServerStaticMethod<byte, byte, ushort> SendForageRequest = ServerStaticMethod<byte, byte, ushort>.Get(ReceiveForageRequest);

    private static readonly ClientStaticMethod<byte, byte, ushort, Vector3> SendResourceDead = ClientStaticMethod<byte, byte, ushort, Vector3>.Get(ReceiveResourceDead);

    private static readonly ClientStaticMethod<byte, byte, ushort> SendResourceAlive = ClientStaticMethod<byte, byte, ushort>.Get(ReceiveResourceAlive);

    private static readonly ClientStaticMethod SendResources = ClientStaticMethod.Get(ReceiveResources);

    private List<Collider> treeColliders = new List<Collider>();

    [Obsolete]
    public void tellClearRegionResources(CSteamID steamID, byte x, byte y)
    {
        ReceiveClearRegionResources(x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellClearRegionResources")]
    public static void ReceiveClearRegionResources(byte x, byte y)
    {
        if (!Provider.isServer && !regions[x, y].isNetworked)
        {
            return;
        }
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion == null)
        {
            return;
        }
        foreach (ResourceSpawnpoint item in treesOrNullInRegion)
        {
            item.revive();
        }
    }

    /// <summary>
    /// Revive all trees in a specific region.
    /// </summary>
    public static void askClearRegionResources(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y))
        {
            List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
            if (treesOrNullInRegion != null && treesOrNullInRegion.Count > 0)
            {
                SendClearRegionResources.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), x, y);
            }
        }
    }

    /// <summary>
    /// Revive trees worldwide. Used between arena rounds.
    /// </summary>
    public static void askClearAllResources()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                askClearRegionResources(b, b2);
            }
        }
    }

    public static void getResourcesInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<Transform> result)
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
            List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(regionCoordinate.x, regionCoordinate.y);
            if (treesOrNullInRegion == null)
            {
                continue;
            }
            foreach (ResourceSpawnpoint item in treesOrNullInRegion)
            {
                if (!(item.model == null) && !item.isDead && (item.point - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(item.model);
                }
            }
        }
    }

    public static void damage(Transform resource, Vector3 direction, float damage, float times, float drop, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown, bool trackKill = true)
    {
        xp = 0u;
        kill = EPlayerKill.NONE;
        ushort pendingTotalDamage = (ushort)(damage * times);
        bool shouldAllow = true;
        onDamageResourceRequested?.Invoke(instigatorSteamID, resource, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        if (!shouldAllow || pendingTotalDamage < 1 || !Regions.tryGetCoordinate(resource.position, out var x, out var y))
        {
            return;
        }
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion == null)
        {
            return;
        }
        for (ushort num = 0; num < treesOrNullInRegion.Count; num++)
        {
            if (resource == treesOrNullInRegion[num].model)
            {
                if (treesOrNullInRegion[num].isDead || !treesOrNullInRegion[num].canBeDamaged)
                {
                    break;
                }
                treesOrNullInRegion[num].askDamage(pendingTotalDamage);
                if (!treesOrNullInRegion[num].isDead)
                {
                    break;
                }
                kill = EPlayerKill.RESOURCE;
                ResourceAsset asset = treesOrNullInRegion[num].asset;
                if (treesOrNullInRegion[num].asset != null)
                {
                    EffectAsset effectAsset = asset.FindExplosionEffectAsset();
                    if (effectAsset != null)
                    {
                        TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                        parameters.position = treesOrNullInRegion[num].GetEffectSpawnPosition();
                        parameters.relevantDistance = EffectManager.MEDIUM;
                        parameters.reliable = true;
                        EffectManager.triggerEffect(parameters);
                    }
                    if (!asset.isForage)
                    {
                        float resource_Drops_Multiplier = Provider.modeConfigData.Objects.Resource_Drops_Multiplier;
                        resource_Drops_Multiplier *= drop;
                        if (asset.rewardID != 0)
                        {
                            Vector3 direction2 = resource.InverseTransformDirection(direction);
                            direction2.y = 0f;
                            direction2.Normalize();
                            Vector3 vector = resource.TransformDirection(direction2);
                            int value = Mathf.CeilToInt((float)UnityEngine.Random.Range(asset.rewardMin, asset.rewardMax + 1) * resource_Drops_Multiplier);
                            value = Mathf.Clamp(value, 0, 100);
                            for (int i = 0; i < value; i++)
                            {
                                ushort num2 = SpawnTableTool.ResolveLegacyId(asset.rewardID, EAssetType.ITEM, asset.OnGetRewardSpawnTableErrorContext);
                                if (num2 != 0)
                                {
                                    ItemManager.dropItem(point: (!asset.hasDebris) ? (resource.position + resource.right * UnityEngine.Random.Range(-2f, 2f) + resource.up * 2f + resource.forward * UnityEngine.Random.Range(-2f, 2f)) : (resource.position + vector * (2 + i) + resource.up * 2f), item: new Item(num2, EItemOrigin.NATURE), playEffect: false, isDropped: Dedicator.IsDedicatedServer, wideSpread: true);
                                }
                            }
                        }
                        else
                        {
                            if (asset.log != 0)
                            {
                                Vector3 direction3 = resource.InverseTransformDirection(direction);
                                direction3.y = 0f;
                                direction3.Normalize();
                                resource.TransformDirection(direction3);
                                int value2 = Mathf.CeilToInt((float)UnityEngine.Random.Range(3, 7) * resource_Drops_Multiplier);
                                value2 = Mathf.Clamp(value2, 0, 100);
                                for (int j = 0; j < value2; j++)
                                {
                                    ItemManager.dropItem(new Item(asset.log, EItemOrigin.NATURE), resource.position + direction * (2 + j * 2) + resource.up, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                }
                            }
                            if (asset.stick != 0)
                            {
                                int value3 = Mathf.CeilToInt((float)UnityEngine.Random.Range(2, 5) * resource_Drops_Multiplier);
                                value3 = Mathf.Clamp(value3, 0, 100);
                                for (int k = 0; k < value3; k++)
                                {
                                    float f = UnityEngine.Random.Range(0f, MathF.PI * 2f);
                                    Vector3 point2 = resource.position + resource.right * Mathf.Sin(f) * 3f + resource.up + resource.forward * Mathf.Cos(f) * 3f;
                                    ItemManager.dropItem(new Item(asset.stick, EItemOrigin.NATURE), point2, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                }
                            }
                        }
                        xp = asset.rewardXP;
                        Vector3 point3 = treesOrNullInRegion[num].point;
                        Guid gUID = asset.GUID;
                        for (int l = 0; l < Provider.clients.Count; l++)
                        {
                            SteamPlayer steamPlayer = Provider.clients[l];
                            if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && (steamPlayer.player.transform.position - point3).sqrMagnitude < 90000f)
                            {
                                steamPlayer.player.quests.trackTreeKill(gUID);
                            }
                        }
                    }
                }
                ServerSetResourceDead(x, y, num, direction * (int)pendingTotalDamage);
                break;
            }
        }
    }

    public static void forage(Transform resource)
    {
        if (!Regions.tryGetCoordinate(resource.position, out var x, out var y))
        {
            return;
        }
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion == null)
        {
            return;
        }
        for (ushort num = 0; num < treesOrNullInRegion.Count; num++)
        {
            if (resource == treesOrNullInRegion[num].model)
            {
                SendForageRequest.Invoke(ENetReliability.Unreliable, x, y, num);
                break;
            }
        }
    }

    [Obsolete]
    public void askForage(CSteamID steamID, byte x, byte y, ushort index)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveForageRequest(in context, x, y, index);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = "askForage")]
    public static void ReceiveForageRequest(in ServerInvocationContext context, byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead)
        {
            return;
        }
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion == null || index >= treesOrNullInRegion.Count || treesOrNullInRegion[index].isDead || (treesOrNullInRegion[index].point - player.transform.position).sqrMagnitude > 400f)
        {
            return;
        }
        ResourceAsset asset = treesOrNullInRegion[index].asset;
        if (asset == null || !asset.isForage)
        {
            return;
        }
        treesOrNullInRegion[index].askDamage(1);
        EffectAsset effectAsset = asset.FindExplosionEffectAsset();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.position = treesOrNullInRegion[index].GetEffectSpawnPosition();
            parameters.relevantDistance = EffectManager.MEDIUM;
            parameters.reliable = true;
            EffectManager.triggerEffect(parameters);
        }
        ushort num = ((asset.rewardID == 0) ? asset.log : SpawnTableTool.ResolveLegacyId(asset.rewardID, EAssetType.ITEM, asset.OnGetRewardSpawnTableErrorContext));
        if (num != 0)
        {
            player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            if (UnityEngine.Random.value < player.skills.mastery(2, 5))
            {
                player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            }
        }
        player.sendStat(EPlayerStat.FOUND_PLANTS);
        player.skills.askPay(asset.forageRewardExperience);
        ServerSetResourceDead(x, y, index, Vector3.zero);
    }

    [Obsolete]
    public void tellResourceDead(CSteamID steamID, byte x, byte y, ushort index, Vector3 ragdoll)
    {
        ReceiveResourceDead(x, y, index, ragdoll);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellResourceDead")]
    public static void ReceiveResourceDead(byte x, byte y, ushort index, Vector3 ragdoll)
    {
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion != null && index < treesOrNullInRegion.Count && (Provider.isServer || regions[x, y].isNetworked))
        {
            treesOrNullInRegion[index].kill(ragdoll);
        }
    }

    [Obsolete]
    public void tellResourceAlive(CSteamID steamID, byte x, byte y, ushort index)
    {
        ReceiveResourceAlive(x, y, index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellResourceAlive")]
    public static void ReceiveResourceAlive(byte x, byte y, ushort index)
    {
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion != null && index < treesOrNullInRegion.Count && (Provider.isServer || regions[x, y].isNetworked))
        {
            treesOrNullInRegion[index].revive();
        }
    }

    [Obsolete]
    public void tellResources(CSteamID steamID, byte x, byte y, bool[] resources)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveResources(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        if (!Regions.checkSafe(value, value2) || regions[value, value2].isNetworked)
        {
            return;
        }
        regions[value, value2].isNetworked = true;
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(value, value2);
        if (treesOrNullInRegion == null)
        {
            return;
        }
        reader.ReadUInt16(out var value3);
        value3 = MathfEx.Min(value3, (ushort)treesOrNullInRegion.Count);
        ushort num = 0;
        bool value4;
        while (num < value3 && reader.ReadBit(out value4))
        {
            if (value4)
            {
                treesOrNullInRegion[num].wipe();
            }
            else
            {
                treesOrNullInRegion[num].revive();
            }
            num++;
        }
    }

    private static void SendResources_Write(NetPakWriter writer, byte x, byte y)
    {
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        ushort num = (ushort)treesOrNullInRegion.Count;
        writer.WriteUInt16(num);
        for (ushort num2 = 0; num2 < num; num2++)
        {
            writer.WriteBit(treesOrNullInRegion[num2].isDead);
        }
    }

    public static ResourceSpawnpoint getResourceSpawnpoint(byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return null;
        }
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
        if (treesOrNullInRegion == null)
        {
            return null;
        }
        if (index >= treesOrNullInRegion.Count)
        {
            return null;
        }
        return treesOrNullInRegion[index];
    }

    public static Transform getResource(byte x, byte y, ushort index)
    {
        ResourceSpawnpoint resourceSpawnpoint = getResourceSpawnpoint(x, y, index);
        if (resourceSpawnpoint != null)
        {
            if (resourceSpawnpoint.model != null)
            {
                return resourceSpawnpoint.model;
            }
            return resourceSpawnpoint.stump;
        }
        return null;
    }

    public static bool tryGetRegion(Transform resource, out byte x, out byte y, out ushort index)
    {
        x = 0;
        y = 0;
        index = 0;
        if (Regions.tryGetCoordinate(resource.position, out x, out y))
        {
            List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(x, y);
            for (index = 0; index < treesOrNullInRegion.Count; index++)
            {
                if (resource == treesOrNullInRegion[index].model || resource == treesOrNullInRegion[index].stump)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool overlapTreeColliders(ResourceSpawnpoint tree, int layerMask)
    {
        treeColliders.Clear();
        if (tree.model == null)
        {
            return false;
        }
        tree.model.GetComponentsInChildren(includeInactive: true, treeColliders);
        foreach (Collider treeCollider in treeColliders)
        {
            if (treeCollider is BoxCollider collider)
            {
                if (collider.OverlapBoxSingle(layerMask, QueryTriggerInteraction.Collide) != null)
                {
                    return true;
                }
            }
            else if (treeCollider is SphereCollider collider2)
            {
                if (collider2.OverlapSphereSingle(layerMask, QueryTriggerInteraction.Collide) != null)
                {
                    return true;
                }
            }
            else if (treeCollider is CapsuleCollider collider3 && collider3.OverlapCapsuleSingle(layerMask, QueryTriggerInteraction.Collide) != null)
            {
                return true;
            }
        }
        return false;
    }

    public static void ServerSetResourceAlive(byte x, byte y, ushort index)
    {
        SendResourceAlive.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClients(x, y), x, y, index);
    }

    public static void ServerSetResourceDead(byte x, byte y, ushort index, Vector3 baseForce)
    {
        SendResourceDead.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClients(x, y), x, y, index, baseForce);
    }

    private bool respawnResources()
    {
        List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(respawnResources_X, respawnResources_Y);
        if (treesOrNullInRegion != null && treesOrNullInRegion.Count > 0)
        {
            if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= treesOrNullInRegion.Count)
            {
                regions[respawnResources_X, respawnResources_Y].respawnResourceIndex = (ushort)(treesOrNullInRegion.Count - 1);
            }
            ResourceSpawnpoint resourceSpawnpoint = treesOrNullInRegion[regions[respawnResources_X, respawnResources_Y].respawnResourceIndex];
            if (resourceSpawnpoint.checkCanReset(Provider.modeConfigData.Objects.Resource_Reset_Multiplier))
            {
                int num = 1536;
                if (Provider.modeConfigData.Objects.Items_Obstruct_Tree_Respawns)
                {
                    num |= 0x8000000;
                }
                if (!overlapTreeColliders(resourceSpawnpoint, num))
                {
                    ServerSetResourceAlive(respawnResources_X, respawnResources_Y, regions[respawnResources_X, respawnResources_Y].respawnResourceIndex);
                }
            }
            return false;
        }
        return true;
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        regions = new ResourceRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b = 0; b < Regions.WORLD_SIZE; b++)
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
            {
                regions[b, b2] = new ResourceRegion();
            }
        }
        respawnResources_X = 0;
        respawnResources_Y = 0;
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
                        if (player.movement.loadedRegions[b, b2].isResourcesLoaded && !Regions.checkArea(b, b2, new_x, new_y, RESOURCE_REGIONS))
                        {
                            player.movement.loadedRegions[b, b2].isResourcesLoaded = false;
                        }
                    }
                    else if (player.channel.IsLocalPlayer && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, RESOURCE_REGIONS))
                    {
                        regions[b, b2].isNetworked = false;
                    }
                }
            }
        }
        if (step != 3 || !Dedicator.IsDedicatedServer || !Regions.checkSafe(new_x, new_y))
        {
            return;
        }
        for (int i = new_x - RESOURCE_REGIONS; i <= new_x + RESOURCE_REGIONS; i++)
        {
            for (int j = new_y - RESOURCE_REGIONS; j <= new_y + RESOURCE_REGIONS; j++)
            {
                if (Regions.checkSafe((byte)i, (byte)j) && !player.movement.loadedRegions[i, j].isResourcesLoaded && LevelGround.GetTreesOrNullInRegion(new Vector2Int(i, j)) != null)
                {
                    player.movement.loadedRegions[i, j].isResourcesLoaded = true;
                    SendResources.Invoke(ENetReliability.Reliable, player.channel.owner.transportConnection, SendResources_Write, (byte)i, (byte)j);
                }
            }
        }
    }

    private void onPlayerCreated(Player player)
    {
        PlayerMovement movement = player.movement;
        movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
    }

    private void Update()
    {
        if (!Provider.isServer || !Level.isLoaded)
        {
            return;
        }
        bool flag = true;
        while (flag)
        {
            flag = respawnResources();
            List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(respawnResources_X, respawnResources_Y);
            regions[respawnResources_X, respawnResources_Y].respawnResourceIndex++;
            if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= treesOrNullInRegion?.Count)
            {
                regions[respawnResources_X, respawnResources_Y].respawnResourceIndex = 0;
            }
            respawnResources_X++;
            if (respawnResources_X >= Regions.WORLD_SIZE)
            {
                respawnResources_X = 0;
                respawnResources_Y++;
                if (respawnResources_Y >= Regions.WORLD_SIZE)
                {
                    respawnResources_Y = 0;
                    flag = false;
                }
            }
        }
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
    }

    private static PooledTransportConnectionList GatherRemoteClients(byte x, byte y)
    {
        return Regions.GatherRemoteClientConnections(x, y, RESOURCE_REGIONS);
    }
}
