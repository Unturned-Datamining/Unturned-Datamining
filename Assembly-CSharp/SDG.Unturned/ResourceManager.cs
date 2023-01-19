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
        if (Provider.isServer || regions[x, y].isNetworked)
        {
            for (int i = 0; i < LevelGround.trees[x, y].Count; i++)
            {
                LevelGround.trees[x, y][i].revive();
            }
        }
    }

    public static void askClearRegionResources(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y) && LevelGround.trees[x, y].Count > 0)
        {
            SendClearRegionResources.InvokeAndLoopback(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), x, y);
        }
    }

    public static void askClearAllResources()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
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
            for (int j = 0; j < LevelGround.trees[regionCoordinate.x, regionCoordinate.y].Count; j++)
            {
                ResourceSpawnpoint resourceSpawnpoint = LevelGround.trees[regionCoordinate.x, regionCoordinate.y][j];
                if (!(resourceSpawnpoint.model == null) && !resourceSpawnpoint.isDead && (resourceSpawnpoint.point - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(resourceSpawnpoint.model);
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
        if (onDamageResourceRequested != null)
        {
            onDamageResourceRequested(instigatorSteamID, resource, ref pendingTotalDamage, ref shouldAllow, damageOrigin);
        }
        if (!shouldAllow || pendingTotalDamage < 1 || !Regions.tryGetCoordinate(resource.position, out var x, out var y))
        {
            return;
        }
        List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
        for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
        {
            if (resource == list[num].model)
            {
                if (list[num].isDead || !list[num].canBeDamaged)
                {
                    break;
                }
                list[num].askDamage(pendingTotalDamage);
                if (!list[num].isDead)
                {
                    break;
                }
                kill = EPlayerKill.RESOURCE;
                ResourceAsset asset = list[num].asset;
                if (list[num].asset != null)
                {
                    EffectAsset effectAsset = asset.FindExplosionEffectAsset();
                    if (effectAsset != null)
                    {
                        TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                        parameters.position = list[num].GetEffectSpawnPosition();
                        parameters.relevantDistance = EffectManager.MEDIUM;
                        EffectManager.triggerEffect(parameters);
                    }
                    if (!asset.isForage)
                    {
                        float resource_Drops_Multiplier = Provider.modeConfigData.Objects.Resource_Drops_Multiplier;
                        resource_Drops_Multiplier *= drop;
                        if (asset.rewardID != 0)
                        {
                            direction.y = 0f;
                            direction.Normalize();
                            int value = Mathf.CeilToInt((float)UnityEngine.Random.Range(asset.rewardMin, asset.rewardMax + 1) * resource_Drops_Multiplier);
                            value = Mathf.Clamp(value, 0, 100);
                            for (int i = 0; i < value; i++)
                            {
                                ushort num2 = SpawnTableTool.resolve(asset.rewardID);
                                if (num2 != 0)
                                {
                                    if (asset.hasDebris)
                                    {
                                        ItemManager.dropItem(new Item(num2, EItemOrigin.NATURE), resource.position + direction * (2 + i) + new Vector3(0f, 2f, 0f), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                    }
                                    else
                                    {
                                        ItemManager.dropItem(new Item(num2, EItemOrigin.NATURE), resource.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 2f, UnityEngine.Random.Range(-2f, 2f)), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (asset.log != 0)
                            {
                                int value2 = Mathf.CeilToInt((float)UnityEngine.Random.Range(3, 7) * resource_Drops_Multiplier);
                                value2 = Mathf.Clamp(value2, 0, 100);
                                for (int j = 0; j < value2; j++)
                                {
                                    ItemManager.dropItem(new Item(asset.log, EItemOrigin.NATURE), resource.position + direction * (2 + j * 2) + Vector3.up, playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                }
                            }
                            if (asset.stick != 0)
                            {
                                int value3 = Mathf.CeilToInt((float)UnityEngine.Random.Range(2, 5) * resource_Drops_Multiplier);
                                value3 = Mathf.Clamp(value3, 0, 100);
                                for (int k = 0; k < value3; k++)
                                {
                                    float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                                    ItemManager.dropItem(new Item(asset.stick, EItemOrigin.NATURE), resource.position + new Vector3(Mathf.Sin(f) * 3f, 1f, Mathf.Cos(f) * 3f), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                                }
                            }
                        }
                        xp = asset.rewardXP;
                        Vector3 point = list[num].point;
                        Guid gUID = asset.GUID;
                        for (int l = 0; l < Provider.clients.Count; l++)
                        {
                            SteamPlayer steamPlayer = Provider.clients[l];
                            if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && (steamPlayer.player.transform.position - point).sqrMagnitude < 90000f)
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
        List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
        for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
        {
            if (resource == list[num].model)
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
        List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
        if (index >= list.Count || list[index].isDead || (list[index].point - player.transform.position).sqrMagnitude > 400f)
        {
            return;
        }
        ResourceAsset asset = list[index].asset;
        if (asset == null || !asset.isForage)
        {
            return;
        }
        list[index].askDamage(1);
        EffectAsset effectAsset = asset.FindExplosionEffectAsset();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.position = list[index].GetEffectSpawnPosition();
            parameters.relevantDistance = EffectManager.MEDIUM;
            EffectManager.triggerEffect(parameters);
        }
        ushort num = ((asset.rewardID == 0) ? asset.log : SpawnTableTool.resolve(asset.rewardID));
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
        if (index < LevelGround.trees[x, y].Count && (Provider.isServer || regions[x, y].isNetworked))
        {
            LevelGround.trees[x, y][index].kill(ragdoll);
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
        if (index < LevelGround.trees[x, y].Count && (Provider.isServer || regions[x, y].isNetworked))
        {
            LevelGround.trees[x, y][index].revive();
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
        List<ResourceSpawnpoint> list = LevelGround.trees[value, value2];
        reader.ReadUInt16(out var value3);
        value3 = MathfEx.Min(value3, (ushort)list.Count);
        ushort num = 0;
        bool value4;
        while (num < value3 && reader.ReadBit(out value4))
        {
            if (value4)
            {
                list[num].wipe();
            }
            else
            {
                list[num].revive();
            }
            num = (ushort)(num + 1);
        }
    }

    internal void SendRegion(SteamPlayer client, byte x, byte y)
    {
        List<ResourceSpawnpoint> regionTrees = LevelGround.trees[x, y];
        SendResources.Invoke(ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteUInt8(x);
            writer.WriteUInt8(y);
            ushort num = (ushort)regionTrees.Count;
            writer.WriteUInt16(num);
            for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
            {
                writer.WriteBit(regionTrees[num2].isDead);
            }
        });
    }

    public static ResourceSpawnpoint getResourceSpawnpoint(byte x, byte y, ushort index)
    {
        if (!Regions.checkSafe(x, y))
        {
            return null;
        }
        List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
        if (index >= list.Count)
        {
            return null;
        }
        return list[index];
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
            List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
            for (index = 0; index < list.Count; index++)
            {
                if (resource == list[index].model || resource == list[index].stump)
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
        SendResourceAlive.InvokeAndLoopback(ENetReliability.Reliable, EnumerateClients_Remote(x, y), x, y, index);
    }

    public static void ServerSetResourceDead(byte x, byte y, ushort index, Vector3 baseForce)
    {
        SendResourceDead.InvokeAndLoopback(ENetReliability.Reliable, EnumerateClients_Remote(x, y), x, y, index, baseForce);
    }

    private bool respawnResources()
    {
        if (LevelGround.trees[respawnResources_X, respawnResources_Y].Count > 0)
        {
            if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= LevelGround.trees[respawnResources_X, respawnResources_Y].Count)
            {
                regions[respawnResources_X, respawnResources_Y].respawnResourceIndex = (ushort)(LevelGround.trees[respawnResources_X, respawnResources_Y].Count - 1);
            }
            ResourceSpawnpoint resourceSpawnpoint = LevelGround.trees[respawnResources_X, respawnResources_Y][regions[respawnResources_X, respawnResources_Y].respawnResourceIndex];
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
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
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
            for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
                {
                    if (Provider.isServer)
                    {
                        if (player.movement.loadedRegions[b, b2].isResourcesLoaded && !Regions.checkArea(b, b2, new_x, new_y, RESOURCE_REGIONS))
                        {
                            player.movement.loadedRegions[b, b2].isResourcesLoaded = false;
                        }
                    }
                    else if (player.channel.isOwner && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, RESOURCE_REGIONS))
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
                if (Regions.checkSafe((byte)i, (byte)j) && !player.movement.loadedRegions[i, j].isResourcesLoaded)
                {
                    player.movement.loadedRegions[i, j].isResourcesLoaded = true;
                    SendRegion(player.channel.owner, (byte)i, (byte)j);
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
            regions[respawnResources_X, respawnResources_Y].respawnResourceIndex++;
            if (regions[respawnResources_X, respawnResources_Y].respawnResourceIndex >= LevelGround.trees[respawnResources_X, respawnResources_Y].Count)
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

    private static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y)
    {
        return Regions.EnumerateClients_Remote(x, y, RESOURCE_REGIONS);
    }
}
