using System;
using System.Collections.Generic;
using System.Diagnostics;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ItemManager : SteamCaller
{
    public static readonly byte ITEM_REGIONS = 1;

    public static ServerSpawningItemDropHandler onServerSpawningItemDrop;

    public static TakeItemRequestHandler onTakeItemRequested;

    public static ItemDropAdded onItemDropAdded;

    public static ItemDropRemoved onItemDropRemoved;

    private static ItemManager manager;

    public static List<InteractableItem> clampedItems;

    private static List<ItemInstantiationParameters> pendingInstantiations = new List<ItemInstantiationParameters>();

    private static List<ItemInstantiationParameters> instantiationsToInsert = new List<ItemInstantiationParameters>();

    private static List<ItemRegion> regionsPendingDestroy = new List<ItemRegion>();

    private static uint instanceCount;

    private static int clampItemIndex;

    private static byte despawnItems_X;

    private static byte despawnItems_Y;

    private static byte respawnItems_X;

    private static byte respawnItems_Y;

    private static readonly ClientStaticMethod<byte, byte, uint, bool> SendDestroyItem = ClientStaticMethod<byte, byte, uint, bool>.Get(ReceiveDestroyItem);

    private static readonly ServerStaticMethod<byte, byte, uint, byte, byte, byte, byte> SendTakeItemRequest = ServerStaticMethod<byte, byte, uint, byte, byte, byte, byte>.Get(ReceiveTakeItemRequest);

    private static readonly ClientStaticMethod<byte, byte> SendClearRegionItems = ClientStaticMethod<byte, byte>.Get(ReceiveClearRegionItems);

    private static List<RegionCoordinate> clearItemRegions = new List<RegionCoordinate>(4);

    private static readonly ClientStaticMethod<byte, byte, ushort, byte, byte, byte[], Vector3, uint, bool> SendItem = ClientStaticMethod<byte, byte, ushort, byte, byte, byte[], Vector3, uint, bool>.Get(ReceiveItem);

    private static readonly ClientStaticMethod SendItems = ClientStaticMethod.Get(ReceiveItems);

    private Stopwatch instantiationTimer = new Stopwatch();

    private const int MIN_INSTANTIATIONS_PER_FRAME = 5;

    private const int MIN_DESTROY_PER_FRAME = 10;

    public static ItemManager instance => manager;

    public static ItemRegion[,] regions { get; private set; }

    public static void getItemsInRadius(Vector3 center, float sqrRadius, List<RegionCoordinate> search, List<InteractableItem> result)
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
            for (int j = 0; j < regions[regionCoordinate.x, regionCoordinate.y].drops.Count; j++)
            {
                ItemDrop itemDrop = regions[regionCoordinate.x, regionCoordinate.y].drops[j];
                if ((itemDrop.model.position - center).sqrMagnitude < sqrRadius)
                {
                    result.Add(itemDrop.interactableItem);
                }
            }
        }
    }

    public static void findSimulatedItemsInRadius(Vector3 center, float sqrRadius, List<InteractableItem> result)
    {
        if (clampedItems == null)
        {
            return;
        }
        foreach (InteractableItem clampedItem in clampedItems)
        {
            if (!(clampedItem == null) && (clampedItem.transform.position - center).sqrMagnitude <= sqrRadius)
            {
                result.Add(clampedItem);
            }
        }
    }

    public static void takeItem(Transform item, byte to_x, byte to_y, byte to_rot, byte to_page)
    {
        if (!Regions.tryGetCoordinate(item.position, out var x, out var y))
        {
            return;
        }
        ItemRegion itemRegion = regions[x, y];
        for (int i = 0; i < itemRegion.drops.Count; i++)
        {
            if (itemRegion.drops[i].model == item)
            {
                SendTakeItemRequest.Invoke(ENetReliability.Unreliable, x, y, itemRegion.drops[i].instanceID, to_x, to_y, to_rot, to_page);
                break;
            }
        }
    }

    public static void dropItem(Item item, Vector3 point, bool playEffect, bool isDropped, bool wideSpread)
    {
        if (regions == null || manager == null)
        {
            return;
        }
        if (wideSpread)
        {
            point.x += UnityEngine.Random.Range(-0.75f, 0.75f);
            point.z += UnityEngine.Random.Range(-0.75f, 0.75f);
        }
        else
        {
            point.x += UnityEngine.Random.Range(-0.125f, 0.125f);
            point.z += UnityEngine.Random.Range(-0.125f, 0.125f);
        }
        if (!Regions.tryGetCoordinate(point, out var x, out var y))
        {
            return;
        }
        ItemAsset asset = item.GetAsset();
        if (asset == null || asset.isPro)
        {
            return;
        }
        if (point.y > 0f)
        {
            Physics.Raycast(point + Vector3.up, Vector3.down, out var hitInfo, Mathf.Min(point.y + 1f, Level.HEIGHT), RayMasks.BLOCK_ITEM);
            if (hitInfo.collider != null)
            {
                point.y = hitInfo.point.y;
            }
        }
        bool shouldAllow = true;
        onServerSpawningItemDrop?.Invoke(item, ref point, ref shouldAllow);
        if (shouldAllow)
        {
            ItemData itemData = new ItemData(item, ++instanceCount, point, isDropped);
            regions[x, y].items.Add(itemData);
            SendItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(x, y, ITEM_REGIONS), x, y, item.id, item.amount, item.quality, item.state, point, itemData.instanceID, playEffect);
        }
    }

    [Obsolete]
    public void tellTakeItem(CSteamID steamID, byte x, byte y, uint instanceID)
    {
    }

    private static void PlayInventoryAudio(ItemAsset item, Vector3 position)
    {
        if (item != null && !item.inventoryAudio.IsNullOrEmpty)
        {
            float pitchMultiplier;
            float volumeMultiplier;
            AudioClip audioClip = item.inventoryAudio.LoadAudioClip(out volumeMultiplier, out pitchMultiplier);
            if (!(audioClip == null))
            {
                volumeMultiplier *= 0.25f;
                OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(position, audioClip);
                oneShotAudioParameters.volume = volumeMultiplier;
                oneShotAudioParameters.pitch = pitchMultiplier;
                oneShotAudioParameters.SetLinearRolloff(0.5f, 8f);
                oneShotAudioParameters.Play();
            }
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveDestroyItem(byte x, byte y, uint instanceID, bool shouldPlayEffect)
    {
        if (!Provider.isServer && !regions[x, y].isNetworked)
        {
            return;
        }
        ItemRegion itemRegion = regions[x, y];
        for (ushort num = 0; num < itemRegion.drops.Count; num = (ushort)(num + 1))
        {
            if (itemRegion.drops[num].instanceID == instanceID)
            {
                onItemDropRemoved?.Invoke(itemRegion.drops[num].model, itemRegion.drops[num].interactableItem);
                if (shouldPlayEffect)
                {
                    PlayInventoryAudio(itemRegion.drops[num].interactableItem.asset, itemRegion.drops[num].model.position);
                }
                UnityEngine.Object.Destroy(itemRegion.drops[num].model.gameObject);
                itemRegion.drops.RemoveAt(num);
                return;
            }
        }
        CancelInstantiationByInstanceId(instanceID);
    }

    [Obsolete]
    public void askTakeItem(CSteamID steamID, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveTakeItemRequest(in context, x, y, instanceID, to_x, to_y, to_rot, to_page);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10, legacyName = "askTakeItem")]
    public static void ReceiveTakeItemRequest(in ServerInvocationContext context, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page)
    {
        if (!Regions.checkSafe(x, y))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || player.animator.gesture == EPlayerGesture.ARREST_START)
        {
            return;
        }
        ItemRegion itemRegion = regions[x, y];
        for (ushort num = 0; num < itemRegion.items.Count; num = (ushort)(num + 1))
        {
            ItemData itemData = itemRegion.items[num];
            if (itemData.instanceID == instanceID)
            {
                if (Dedicator.IsDedicatedServer && (itemData.point - player.transform.position).sqrMagnitude > 400f)
                {
                    break;
                }
                bool shouldAllow = true;
                if (onTakeItemRequested != null)
                {
                    try
                    {
                        onTakeItemRequested(player, x, y, instanceID, to_x, to_y, to_rot, to_page, itemData, ref shouldAllow);
                    }
                    catch (Exception e)
                    {
                        UnturnedLog.exception(e, "Caught exception invoking onTakeItemRequested:");
                    }
                }
                if (!shouldAllow)
                {
                    break;
                }
                if ((to_page != byte.MaxValue) ? player.inventory.tryAddItem(regions[x, y].items[num].item, to_x, to_y, to_page, to_rot) : player.inventory.tryAddItem(regions[x, y].items[num].item, auto: true))
                {
                    if (!player.equipment.wasTryingToSelect && !player.equipment.isSelected)
                    {
                        player.animator.sendGesture(EPlayerGesture.PICKUP, all: true);
                    }
                    regions[x, y].items.RemoveAt(num);
                    player.sendStat(EPlayerStat.FOUND_ITEMS);
                    SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(x, y, ITEM_REGIONS), x, y, instanceID, arg4: true);
                }
                else
                {
                    player.sendMessage(EPlayerMessage.SPACE);
                }
                break;
            }
        }
    }

    [Obsolete]
    public void tellClearRegionItems(CSteamID steamID, byte x, byte y)
    {
        ReceiveClearRegionItems(x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellClearRegionItems")]
    public static void ReceiveClearRegionItems(byte x, byte y)
    {
        if (Provider.isServer || regions[x, y].isNetworked)
        {
            DestroyAllInRegion(regions[x, y]);
            CancelInstantiationsInRegion(x, y);
        }
    }

    public static void askClearRegionItems(byte x, byte y)
    {
        if (Provider.isServer && Regions.checkSafe(x, y))
        {
            ItemRegion itemRegion = regions[x, y];
            if (itemRegion.items.Count > 0)
            {
                itemRegion.items.Clear();
                SendClearRegionItems.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(x, y, ITEM_REGIONS), x, y);
            }
        }
    }

    public static void askClearAllItems()
    {
        if (!Provider.isServer)
        {
            return;
        }
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                askClearRegionItems(b, b2);
            }
        }
    }

    public static void ServerClearItemsInSphere(Vector3 center, float radius)
    {
        clearItemRegions.Clear();
        Regions.getRegionsInRadius(center, radius, clearItemRegions);
        float num = MathfEx.Square(radius);
        foreach (RegionCoordinate clearItemRegion in clearItemRegions)
        {
            ItemRegion itemRegion = regions[clearItemRegion.x, clearItemRegion.y];
            for (int num2 = itemRegion.items.Count - 1; num2 >= 0; num2--)
            {
                ItemData itemData = itemRegion.items[num2];
                if (!((itemData.point - center).sqrMagnitude > num))
                {
                    uint instanceID = itemData.instanceID;
                    itemRegion.items.RemoveAt(num2);
                    SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(clearItemRegion.x, clearItemRegion.y, ITEM_REGIONS), clearItemRegion.x, clearItemRegion.y, instanceID, arg4: false);
                }
            }
        }
    }

    private void spawnItem(byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID, bool shouldPlayEffect)
    {
        if (Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset)
        {
            Transform transform = new GameObject().transform;
            transform.name = id.ToString();
            transform.transform.position = point;
            Transform item = ItemTool.getItem(id, 0, quality, state, viewmodel: false, itemAsset, null);
            item.parent = transform;
            InteractableItem interactableItem = item.gameObject.AddComponent<InteractableItem>();
            interactableItem.item = new Item(id, amount, quality, state);
            interactableItem.asset = itemAsset;
            item.position = point + Vector3.up * 0.75f;
            item.rotation = Quaternion.Euler(-90 + UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(-15, 15));
            item.gameObject.AddComponent<Rigidbody>();
            item.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
            item.GetComponent<Rigidbody>().drag = 0.5f;
            item.GetComponent<Rigidbody>().angularDrag = 0.1f;
            if (LevelObjects.loads[x, y] != -1)
            {
                item.GetComponent<Rigidbody>().useGravity = false;
                item.GetComponent<Rigidbody>().isKinematic = true;
            }
            ItemDrop item2 = new ItemDrop(transform, interactableItem, instanceID);
            regions[x, y].drops.Add(item2);
            onItemDropAdded?.Invoke(item, interactableItem);
            if (shouldPlayEffect)
            {
                PlayInventoryAudio(itemAsset, point);
            }
        }
    }

    [Obsolete]
    public void tellItem(CSteamID steamID, byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID)
    {
        ReceiveItem(x, y, id, amount, quality, state, point, instanceID, shouldPlayEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveItem(byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID, bool shouldPlayEffect)
    {
        if (Regions.checkSafe(x, y) && regions[x, y].isNetworked)
        {
            float sortOrder = 0f;
            if (MainCamera.instance != null)
            {
                sortOrder = (MainCamera.instance.transform.position - point).sqrMagnitude;
            }
            ItemInstantiationParameters item = default(ItemInstantiationParameters);
            item.region_x = x;
            item.region_y = y;
            item.assetId = id;
            item.amount = amount;
            item.quality = quality;
            item.state = state;
            item.point = point;
            item.instanceID = instanceID;
            item.sortOrder = sortOrder;
            item.shouldPlayEffect = shouldPlayEffect;
            pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(item), item);
        }
    }

    [Obsolete]
    public void tellItems(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveItems(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        if (!Regions.checkSafe(value, value2))
        {
            return;
        }
        reader.ReadUInt8(out var value3);
        if (value3 == 0)
        {
            if (regions[value, value2].isNetworked)
            {
                return;
            }
            DestroyAllInRegion(regions[value, value2]);
        }
        else if (!regions[value, value2].isNetworked)
        {
            return;
        }
        regions[value, value2].isNetworked = true;
        reader.ReadUInt16(out var value4);
        if (value4 > 0)
        {
            reader.ReadFloat(out var value5);
            instantiationsToInsert.Clear();
            for (ushort num = 0; num < value4; num = (ushort)(num + 1))
            {
                ItemInstantiationParameters item = default(ItemInstantiationParameters);
                item.region_x = value;
                item.region_y = value2;
                item.sortOrder = value5;
                reader.ReadUInt16(out item.assetId);
                reader.ReadUInt8(out item.amount);
                reader.ReadUInt8(out item.quality);
                reader.ReadUInt8(out var value6);
                byte[] array = new byte[value6];
                reader.ReadBytes(array);
                item.state = array;
                reader.ReadClampedVector3(out item.point);
                reader.ReadUInt32(out item.instanceID);
                instantiationsToInsert.Add(item);
            }
            pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
        }
    }

    [Obsolete]
    public void askItems(CSteamID steamID, byte x, byte y)
    {
    }

    internal void askItems(ITransportConnection transportConnection, byte x, byte y, float sortOrder)
    {
        if (regions[x, y].items.Count > 0)
        {
            byte packet = 0;
            int index = 0;
            int count = 0;
            while (index < regions[x, y].items.Count)
            {
                int num = 0;
                while (count < regions[x, y].items.Count)
                {
                    num += 4 + regions[x, y].items[count].item.state.Length + 12 + 4;
                    count++;
                    if (num > Block.BUFFER_SIZE / 2)
                    {
                        break;
                    }
                }
                SendItems.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
                {
                    writer.WriteUInt8(x);
                    writer.WriteUInt8(y);
                    writer.WriteUInt8(packet);
                    writer.WriteUInt16((ushort)(count - index));
                    writer.WriteFloat(sortOrder);
                    for (; index < count; index++)
                    {
                        ItemData itemData = regions[x, y].items[index];
                        writer.WriteUInt16(itemData.item.id);
                        writer.WriteUInt8(itemData.item.amount);
                        writer.WriteUInt8(itemData.item.quality);
                        writer.WriteUInt8((byte)itemData.item.state.Length);
                        writer.WriteBytes(itemData.item.state);
                        writer.WriteClampedVector3(itemData.point);
                        writer.WriteUInt32(itemData.instanceID);
                    }
                });
                packet++;
            }
        }
        else
        {
            SendItems.Invoke(ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
            {
                writer.WriteUInt8(x);
                writer.WriteUInt8(y);
                writer.WriteUInt8(0);
                writer.WriteUInt16(0);
            });
        }
    }

    private bool despawnItems()
    {
        if (Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return false;
        }
        if (regions[despawnItems_X, despawnItems_Y].items.Count > 0)
        {
            for (int i = 0; i < regions[despawnItems_X, despawnItems_Y].items.Count; i++)
            {
                if (Time.realtimeSinceStartup - regions[despawnItems_X, despawnItems_Y].items[i].lastDropped > (regions[despawnItems_X, despawnItems_Y].items[i].isDropped ? Provider.modeConfigData.Items.Despawn_Dropped_Time : Provider.modeConfigData.Items.Despawn_Natural_Time))
                {
                    uint instanceID = regions[despawnItems_X, despawnItems_Y].items[i].instanceID;
                    regions[despawnItems_X, despawnItems_Y].items.RemoveAt(i);
                    SendDestroyItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(despawnItems_X, despawnItems_Y, ITEM_REGIONS), despawnItems_X, despawnItems_Y, instanceID, arg4: false);
                    break;
                }
            }
            return true;
        }
        return false;
    }

    private bool respawnItems()
    {
        if (Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return false;
        }
        if (LevelItems.spawns[respawnItems_X, respawnItems_Y].Count > 0 && Time.realtimeSinceStartup - regions[respawnItems_X, respawnItems_Y].lastRespawn > Provider.modeConfigData.Items.Respawn_Time)
        {
            int count = regions[respawnItems_X, respawnItems_Y].items.Count;
            int num = (int)((float)LevelItems.spawns[respawnItems_X, respawnItems_Y].Count * Provider.modeConfigData.Items.Spawn_Chance);
            bool flag = false;
            for (int i = count; i < num; i++)
            {
                ItemSpawnpoint itemSpawnpoint = LevelItems.spawns[respawnItems_X, respawnItems_Y][UnityEngine.Random.Range(0, LevelItems.spawns[respawnItems_X, respawnItems_Y].Count)];
                bool flag2 = true;
                if (!SafezoneManager.checkPointValid(itemSpawnpoint.point))
                {
                    flag2 = false;
                }
                for (ushort num2 = 0; num2 < regions[respawnItems_X, respawnItems_Y].items.Count; num2 = (ushort)(num2 + 1))
                {
                    if ((regions[respawnItems_X, respawnItems_Y].items[num2].point - itemSpawnpoint.point).sqrMagnitude < 4f)
                    {
                        flag2 = false;
                        break;
                    }
                }
                if (!flag2)
                {
                    continue;
                }
                ushort item = LevelItems.getItem(itemSpawnpoint);
                if (Assets.find(EAssetType.ITEM, item) is ItemAsset)
                {
                    Item item2 = new Item(item, EItemOrigin.WORLD);
                    Vector3 location = itemSpawnpoint.point;
                    bool shouldAllow = true;
                    onServerSpawningItemDrop?.Invoke(item2, ref location, ref shouldAllow);
                    if (!shouldAllow)
                    {
                        continue;
                    }
                    ItemData itemData = new ItemData(item2, ++instanceCount, itemSpawnpoint.point, newDropped: false);
                    regions[respawnItems_X, respawnItems_Y].items.Add(itemData);
                    SendItem.Invoke(ENetReliability.Reliable, Regions.EnumerateClients(respawnItems_X, respawnItems_Y, ITEM_REGIONS), respawnItems_X, respawnItems_Y, item2.id, item2.amount, item2.quality, item2.state, location, itemData.instanceID, arg9: false);
                }
                else if ((bool)Assets.shouldLoadAnyAssets)
                {
                    UnturnedLog.error("Failed to respawn an item with ID " + item + " from type " + LevelItems.tables[itemSpawnpoint.type].name + " [" + itemSpawnpoint.type + "]");
                }
                flag = true;
            }
            if (flag)
            {
                regions[respawnItems_X, respawnItems_Y].lastRespawn = Time.realtimeSinceStartup;
                return true;
            }
        }
        return false;
    }

    private void generateItems(byte x, byte y)
    {
        if (Level.info == null || Level.info.type == ELevelType.ARENA)
        {
            return;
        }
        List<ItemData> list = new List<ItemData>();
        if (LevelItems.spawns[x, y].Count > 0)
        {
            List<ItemSpawnpoint> list2 = new List<ItemSpawnpoint>();
            for (int i = 0; i < LevelItems.spawns[x, y].Count; i++)
            {
                ItemSpawnpoint itemSpawnpoint = LevelItems.spawns[x, y][i];
                if (SafezoneManager.checkPointValid(itemSpawnpoint.point))
                {
                    list2.Add(itemSpawnpoint);
                }
            }
            while ((float)list.Count < (float)LevelItems.spawns[x, y].Count * Provider.modeConfigData.Items.Spawn_Chance && list2.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list2.Count);
                ItemSpawnpoint itemSpawnpoint2 = list2[index];
                list2.RemoveAt(index);
                ushort item = LevelItems.getItem(itemSpawnpoint2);
                if (Assets.find(EAssetType.ITEM, item) is ItemAsset)
                {
                    Item item2 = new Item(item, EItemOrigin.WORLD);
                    Vector3 location = itemSpawnpoint2.point;
                    bool shouldAllow = true;
                    onServerSpawningItemDrop?.Invoke(item2, ref location, ref shouldAllow);
                    if (shouldAllow)
                    {
                        list.Add(new ItemData(item2, ++instanceCount, location, newDropped: false));
                    }
                }
                else if ((bool)Assets.shouldLoadAnyAssets)
                {
                    UnturnedLog.error("Failed to generate an item with ID " + item + " from type " + LevelItems.tables[itemSpawnpoint2.type].name + " [" + itemSpawnpoint2.type + "]");
                }
            }
        }
        for (int j = 0; j < regions[x, y].items.Count; j++)
        {
            if (regions[x, y].items[j].isDropped)
            {
                list.Add(regions[x, y].items[j]);
            }
        }
        regions[x, y].items = list;
    }

    internal static void ClearNetworkStuff()
    {
        pendingInstantiations = new List<ItemInstantiationParameters>();
        instantiationsToInsert = new List<ItemInstantiationParameters>();
        regionsPendingDestroy = new List<ItemRegion>();
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        regions = new ItemRegion[Regions.WORLD_SIZE, Regions.WORLD_SIZE];
        for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
        {
            for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
            {
                regions[b, b2] = new ItemRegion();
            }
        }
        clampedItems = new List<InteractableItem>();
        instanceCount = 0u;
        clampItemIndex = 0;
        despawnItems_X = 0;
        despawnItems_Y = 0;
        respawnItems_X = 0;
        respawnItems_Y = 0;
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        for (byte b3 = 0; b3 < Regions.WORLD_SIZE; b3 = (byte)(b3 + 1))
        {
            for (byte b4 = 0; b4 < Regions.WORLD_SIZE; b4 = (byte)(b4 + 1))
            {
                generateItems(b3, b4);
            }
        }
    }

    private void onRegionActivated(byte x, byte y)
    {
        if (regions == null || regions[x, y] == null)
        {
            return;
        }
        for (int i = 0; i < regions[x, y].drops.Count; i++)
        {
            ItemDrop itemDrop = regions[x, y].drops[i];
            if (itemDrop != null && !(itemDrop.interactableItem == null))
            {
                Rigidbody component = itemDrop.interactableItem.GetComponent<Rigidbody>();
                if (!(component == null))
                {
                    component.useGravity = true;
                    component.isKinematic = false;
                }
            }
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
                    if (player.channel.isOwner && regions[b, b2].isNetworked && !Regions.checkArea(b, b2, new_x, new_y, ITEM_REGIONS))
                    {
                        if (regions[b, b2].drops.Count > 0)
                        {
                            regions[b, b2].isPendingDestroy = true;
                            regionsPendingDestroy.Add(regions[b, b2]);
                        }
                        CancelInstantiationsInRegion(b, b2);
                        regions[b, b2].isNetworked = false;
                    }
                    if (Provider.isServer && player.movement.loadedRegions[b, b2].isItemsLoaded && !Regions.checkArea(b, b2, new_x, new_y, ITEM_REGIONS))
                    {
                        player.movement.loadedRegions[b, b2].isItemsLoaded = false;
                    }
                }
            }
        }
        if (step != 5 || !Provider.isServer || !Regions.checkSafe(new_x, new_y))
        {
            return;
        }
        Vector3 position = player.transform.position;
        for (int i = new_x - ITEM_REGIONS; i <= new_x + ITEM_REGIONS; i++)
        {
            for (int j = new_y - ITEM_REGIONS; j <= new_y + ITEM_REGIONS; j++)
            {
                if (!Regions.checkSafe((byte)i, (byte)j) || player.movement.loadedRegions[i, j].isItemsLoaded)
                {
                    continue;
                }
                if (player.channel.isOwner)
                {
                    generateItems((byte)i, (byte)j);
                }
                player.movement.loadedRegions[i, j].isItemsLoaded = true;
                float sortOrder = Regions.HorizontalDistanceFromCenterSquared(i, j, position);
                if (Dedicator.IsDedicatedServer)
                {
                    askItems(player.channel.owner.transportConnection, (byte)i, (byte)j, sortOrder);
                    continue;
                }
                DestroyAllInRegion(regions[i, j]);
                regions[i, j].isNetworked = true;
                if (regions[i, j].items.Count <= 0)
                {
                    continue;
                }
                instantiationsToInsert.Clear();
                foreach (ItemData item2 in regions[i, j].items)
                {
                    ItemInstantiationParameters item = default(ItemInstantiationParameters);
                    item.region_x = (byte)i;
                    item.region_y = (byte)j;
                    item.sortOrder = sortOrder;
                    item.assetId = item2.item.id;
                    item.amount = item2.item.amount;
                    item.quality = item2.item.quality;
                    item.state = item2.item.state;
                    item.point = item2.point;
                    item.instanceID = item2.instanceID;
                    item.sortOrder = sortOrder;
                    instantiationsToInsert.Add(item);
                }
                pendingInstantiations.InsertRange(pendingInstantiations.FindInsertionIndex(instantiationsToInsert[0]), instantiationsToInsert);
            }
        }
    }

    private void onPlayerCreated(Player player)
    {
        PlayerMovement movement = player.movement;
        movement.onRegionUpdated = (PlayerRegionUpdated)Delegate.Combine(movement.onRegionUpdated, new PlayerRegionUpdated(onRegionUpdated));
    }

    private static void DestroyAllInRegion(ItemRegion region)
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

    private static void CancelInstantiationsInRegion(byte x, byte y)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].region_x == x && pendingInstantiations[num].region_y == y)
            {
                pendingInstantiations.RemoveAt(num);
            }
        }
    }

    private static void CancelInstantiationByInstanceId(uint instanceId)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].instanceID == instanceId)
            {
                pendingInstantiations.RemoveAt(num);
                break;
            }
        }
    }

    private void Update()
    {
        if (!Provider.isServer && clampedItems != null && clampedItems.Count > 0)
        {
            if (clampItemIndex >= clampedItems.Count)
            {
                clampItemIndex = 0;
            }
            InteractableItem interactableItem = clampedItems[clampItemIndex];
            if (interactableItem != null)
            {
                interactableItem.clampRange();
                clampItemIndex++;
            }
            else
            {
                clampedItems.RemoveAtFast(clampItemIndex);
            }
        }
        if (Provider.isConnected)
        {
            if (pendingInstantiations != null && pendingInstantiations.Count > 0)
            {
                instantiationTimer.Restart();
                int num = 0;
                do
                {
                    ItemInstantiationParameters itemInstantiationParameters = pendingInstantiations[num];
                    spawnItem(itemInstantiationParameters.region_x, itemInstantiationParameters.region_y, itemInstantiationParameters.assetId, itemInstantiationParameters.amount, itemInstantiationParameters.quality, itemInstantiationParameters.state, itemInstantiationParameters.point, itemInstantiationParameters.instanceID, itemInstantiationParameters.shouldPlayEffect);
                    num++;
                }
                while (num < pendingInstantiations.Count && (instantiationTimer.ElapsedMilliseconds < 1 || num < 5));
                pendingInstantiations.RemoveRange(0, num);
                instantiationTimer.Stop();
            }
            if (regionsPendingDestroy != null && regionsPendingDestroy.Count > 0)
            {
                instantiationTimer.Restart();
                int num2 = 0;
                do
                {
                    ItemRegion tail = regionsPendingDestroy.GetTail();
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
        if (!Dedicator.IsDedicatedServer || !Level.isLoaded)
        {
            return;
        }
        bool flag;
        do
        {
            flag = despawnItems();
            despawnItems_X++;
            if (despawnItems_X >= Regions.WORLD_SIZE)
            {
                despawnItems_X = 0;
                despawnItems_Y++;
                if (despawnItems_Y >= Regions.WORLD_SIZE)
                {
                    despawnItems_Y = 0;
                    break;
                }
            }
        }
        while (!flag);
        bool flag2;
        do
        {
            flag2 = respawnItems();
            respawnItems_X++;
            if (respawnItems_X >= Regions.WORLD_SIZE)
            {
                respawnItems_X = 0;
                respawnItems_Y++;
                if (respawnItems_Y >= Regions.WORLD_SIZE)
                {
                    respawnItems_Y = 0;
                    break;
                }
            }
        }
        while (!flag2);
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        LevelObjects.onRegionActivated = (RegionActivated)Delegate.Combine(LevelObjects.onRegionActivated, new RegionActivated(onRegionActivated));
        Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
    }
}
