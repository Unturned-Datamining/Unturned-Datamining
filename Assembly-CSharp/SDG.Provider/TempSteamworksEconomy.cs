using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using SDG.Framework.IO.Deserialization;
using SDG.SteamworksProvider;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Provider;

public class TempSteamworksEconomy
{
    public delegate void InventoryRefreshed();

    public delegate void InventoryDropped(int item, ushort quantity, ulong instance);

    public delegate void InventoryExchanged(List<SteamItemDetails_t> grantedItems);

    public delegate void InventoryExchangeFailed();

    public InventoryRefreshed onInventoryRefreshed;

    public InventoryDropped onInventoryDropped;

    public InventoryExchanged onInventoryExchanged;

    public InventoryExchanged onInventoryPurchased;

    public InventoryExchangeFailed onInventoryExchangeFailed;

    private SteamInventoryResult_t promoResult = SteamInventoryResult_t.Invalid;

    public SteamInventoryResult_t exchangeResult = SteamInventoryResult_t.Invalid;

    public SteamInventoryResult_t dropResult = SteamInventoryResult_t.Invalid;

    public SteamInventoryResult_t wearingResult = SteamInventoryResult_t.Invalid;

    public SteamInventoryResult_t inventoryResult = SteamInventoryResult_t.Invalid;

    public SteamInventoryResult_t commitResult = SteamInventoryResult_t.Invalid;

    public List<SteamItemDetails_t> inventoryDetails = new List<SteamItemDetails_t>(0);

    public Dictionary<ulong, DynamicEconDetails> dynamicInventoryDetails = new Dictionary<ulong, DynamicEconDetails>();

    public bool isInventoryAvailable;

    public bool isExpectingPurchaseResult;

    private SteamworksAppInfo appInfo;

    private float lastHeartbeat;

    private Callback<SteamInventoryResultReady_t> inventoryResultReady;

    public bool canOpenInventory => true;

    public static List<UnturnedEconInfo> econInfo { get; private set; }

    public static byte[] econInfoHash { get; private set; }

    public List<SteamItemDetails_t> inventory => inventoryDetails;

    public bool hasCountryDetails { get; protected set; }

    public bool doesCountryAllowRandomItems { get; protected set; }

    public void open(ulong id)
    {
        SDG.Unturned.Provider.openURL("http://steamcommunity.com/profiles/" + SteamUser.GetSteamID().ToString() + "/inventory/?sellOnLoad=1#" + SteamUtils.GetAppID().ToString() + "_2_" + id);
    }

    public ulong getInventoryPackage(int item)
    {
        if (inventoryDetails != null)
        {
            for (int i = 0; i < inventoryDetails.Count; i++)
            {
                if (inventoryDetails[i].m_iDefinition.m_SteamItemDef == item)
                {
                    return inventoryDetails[i].m_itemId.m_SteamItemInstanceID;
                }
            }
        }
        return 0uL;
    }

    public int countInventoryPackages(int item)
    {
        int num = 0;
        if (inventoryDetails != null)
        {
            for (int i = 0; i < inventoryDetails.Count; i++)
            {
                if (inventoryDetails[i].m_iDefinition.m_SteamItemDef == item)
                {
                    num += inventoryDetails[i].m_unQuantity;
                }
            }
        }
        return num;
    }

    public bool getInventoryPackages(int item, ushort requiredQuantity, out List<EconExchangePair> pairs)
    {
        ushort num = 0;
        pairs = new List<EconExchangePair>();
        if (inventoryDetails != null)
        {
            for (int i = 0; i < inventoryDetails.Count; i++)
            {
                if (inventoryDetails[i].m_iDefinition.m_SteamItemDef != item)
                {
                    continue;
                }
                ushort num2 = inventoryDetails[i].m_unQuantity;
                if (num2 >= 1)
                {
                    ushort num3 = (ushort)(requiredQuantity - num);
                    if (num2 >= num3)
                    {
                        num2 = num3;
                    }
                    EconExchangePair item2 = new EconExchangePair(inventoryDetails[i].m_itemId.m_SteamItemInstanceID, num2);
                    pairs.Add(item2);
                    num = (ushort)(num + num2);
                    if (num == requiredQuantity)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public int getInventoryItem(ulong package)
    {
        if (inventoryDetails != null)
        {
            for (int i = 0; i < inventoryDetails.Count; i++)
            {
                if (inventoryDetails[i].m_itemId.m_SteamItemInstanceID == package)
                {
                    return inventoryDetails[i].m_iDefinition.m_SteamItemDef;
                }
            }
        }
        return 0;
    }

    public string getInventoryTags(ulong instance)
    {
        if (!dynamicInventoryDetails.TryGetValue(instance, out var value))
        {
            return string.Empty;
        }
        return value.tags;
    }

    public string getInventoryDynamicProps(ulong instance)
    {
        if (!dynamicInventoryDetails.TryGetValue(instance, out var value))
        {
            return string.Empty;
        }
        return value.dynamic_props;
    }

    public bool getInventoryStatTrackerValue(ulong instance, out EStatTrackerType type, out int kills)
    {
        if (!dynamicInventoryDetails.TryGetValue(instance, out var value))
        {
            type = EStatTrackerType.NONE;
            kills = -1;
            return false;
        }
        return value.getStatTrackerValue(out type, out kills);
    }

    public bool getInventoryRagdollEffect(ulong instance, out ERagdollEffect effect)
    {
        if (!dynamicInventoryDetails.TryGetValue(instance, out var value))
        {
            effect = ERagdollEffect.NONE;
            return false;
        }
        return value.getRagdollEffect(out effect);
    }

    public ushort getInventoryParticleEffect(ulong instance)
    {
        if (dynamicInventoryDetails.TryGetValue(instance, out var value))
        {
            return value.getParticleEffect();
        }
        return 0;
    }

    public bool IsItemKnown(int item)
    {
        return econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item) != null;
    }

    public string getInventoryName(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return "";
        }
        return unturnedEconInfo.name;
    }

    public string getInventoryType(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return "";
        }
        return unturnedEconInfo.type;
    }

    public string getInventoryDescription(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return "";
        }
        return unturnedEconInfo.description;
    }

    public bool getInventoryMarketable(int item)
    {
        return econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item)?.marketable ?? false;
    }

    public int getInventoryScraps(int item)
    {
        return econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item)?.scraps ?? 0;
    }

    public int getScrapExchangeItem(int item)
    {
        return getInventoryScraps(item) switch
        {
            1 => 19006, 
            2 => 19007, 
            3 => 19008, 
            4 => 19009, 
            5 => 19010, 
            10 => 19011, 
            _ => 0, 
        };
    }

    public Color getInventoryColor(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return Color.white;
        }
        if (unturnedEconInfo.name_color != null && unturnedEconInfo.name_color.Length > 0 && uint.TryParse(unturnedEconInfo.name_color, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var result))
        {
            uint num = (result >> 16) & 0xFF;
            uint num2 = (result >> 8) & 0xFFu;
            uint num3 = result & 0xFFu;
            return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f);
        }
        return Color.white;
    }

    public UnturnedEconInfo.EQuality getInventoryQuality(int item)
    {
        return econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item)?.quality ?? UnturnedEconInfo.EQuality.None;
    }

    public UnturnedEconInfo.ERarity getInventoryRarity(int item)
    {
        return getInventoryQuality(item) switch
        {
            UnturnedEconInfo.EQuality.Common => UnturnedEconInfo.ERarity.Common, 
            UnturnedEconInfo.EQuality.Uncommon => UnturnedEconInfo.ERarity.Uncommon, 
            UnturnedEconInfo.EQuality.Gold => UnturnedEconInfo.ERarity.Gold, 
            UnturnedEconInfo.EQuality.Rare => UnturnedEconInfo.ERarity.Rare, 
            UnturnedEconInfo.EQuality.Epic => UnturnedEconInfo.ERarity.Epic, 
            UnturnedEconInfo.EQuality.Legendary => UnturnedEconInfo.ERarity.Legendary, 
            UnturnedEconInfo.EQuality.Mythical => UnturnedEconInfo.ERarity.Mythical, 
            UnturnedEconInfo.EQuality.Premium => UnturnedEconInfo.ERarity.Premium, 
            UnturnedEconInfo.EQuality.Achievement => UnturnedEconInfo.ERarity.Achievement, 
            _ => UnturnedEconInfo.ERarity.Unknown, 
        };
    }

    public EItemRarity getGameRarity(int item)
    {
        return getInventoryQuality(item) switch
        {
            UnturnedEconInfo.EQuality.Uncommon => EItemRarity.UNCOMMON, 
            UnturnedEconInfo.EQuality.Rare => EItemRarity.RARE, 
            UnturnedEconInfo.EQuality.Epic => EItemRarity.EPIC, 
            UnturnedEconInfo.EQuality.Legendary => EItemRarity.LEGENDARY, 
            UnturnedEconInfo.EQuality.Mythical => EItemRarity.MYTHICAL, 
            _ => EItemRarity.COMMON, 
        };
    }

    public Color getStatTrackerColor(EStatTrackerType type)
    {
        return type switch
        {
            EStatTrackerType.NONE => Color.white, 
            EStatTrackerType.TOTAL => new Color(1f, 0.5f, 0f), 
            EStatTrackerType.PLAYER => new Color(1f, 0f, 0f), 
            _ => Color.black, 
        };
    }

    public string getStatTrackerPropertyName(EStatTrackerType type)
    {
        return type switch
        {
            EStatTrackerType.TOTAL => "total_kills", 
            EStatTrackerType.PLAYER => "player_kills", 
            _ => null, 
        };
    }

    public ushort getInventoryMythicID(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return 0;
        }
        return (ushort)unturnedEconInfo.item_effect;
    }

    public void getInventoryTargetID(int item, out ushort item_id, out ushort vehicle_id)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            item_id = 0;
            vehicle_id = 0;
        }
        else
        {
            item_id = (ushort)unturnedEconInfo.item_id;
            vehicle_id = (ushort)unturnedEconInfo.vehicle_id;
        }
    }

    public ushort getInventoryItemID(int item)
    {
        getInventoryTargetID(item, out var item_id, out var _);
        return item_id;
    }

    public ushort getInventorySkinID(int item)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == item);
        if (unturnedEconInfo == null)
        {
            return 0;
        }
        return (ushort)unturnedEconInfo.item_skin;
    }

    public Texture2D LoadItemIcon(int itemdefid, bool large)
    {
        UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == itemdefid);
        if (unturnedEconInfo == null)
        {
            return null;
        }
        if (unturnedEconInfo.item_id != 0)
        {
            if (!(Assets.find(EAssetType.ITEM, (ushort)unturnedEconInfo.item_id) is ItemAsset itemAsset))
            {
                return null;
            }
            if (itemAsset.econIconUseId)
            {
                return Resources.Load<Texture2D>("Economy/Item/" + itemdefid + (large ? "/Icon_Large" : "/Icon_Small"));
            }
            if (itemAsset.proPath == null || itemAsset.proPath.Length == 0)
            {
                Asset asset = Assets.find(EAssetType.SKIN, (ushort)unturnedEconInfo.item_skin);
                if (asset == null)
                {
                    return null;
                }
                return Resources.Load<Texture2D>("Economy/Skins/" + itemAsset.name + "/" + asset.name + (large ? "/Icon_Large" : "/Icon_Small"));
            }
            return Resources.Load<Texture2D>("Economy" + itemAsset.proPath + (large ? "/Icon_Large" : "/Icon_Small"));
        }
        if (unturnedEconInfo.vehicle_id != 0)
        {
            if (!(Assets.find(EAssetType.VEHICLE, (ushort)unturnedEconInfo.vehicle_id) is VehicleAsset vehicleAsset))
            {
                return null;
            }
            Asset asset2 = Assets.find(EAssetType.SKIN, (ushort)unturnedEconInfo.item_skin);
            if (asset2 == null)
            {
                return null;
            }
            return Resources.Load<Texture2D>("Economy/Skins/" + vehicleAsset.sharedSkinName + "/" + asset2.name + (large ? "/Icon_Large" : "/Icon_Small"));
        }
        return Resources.Load<Texture2D>("Economy/Item/" + itemdefid + (large ? "/Icon_Large" : "/Icon_Small"));
    }

    public void consumeItem(ulong instance, uint quantity)
    {
        UnturnedLog.info("Consume item: {0} x{1}", instance, quantity);
        SteamInventory.ConsumeItem(out var _, (SteamItemInstanceID_t)instance, quantity);
    }

    public void exchangeInventory(int generate, List<EconExchangePair> destroy)
    {
        UnturnedLog.info("Exchange these item instances for " + generate);
        for (int i = 0; i < destroy.Count; i++)
        {
            ulong instanceId = destroy[i].instanceId;
            int num = -1;
            for (int j = 0; j < inventoryDetails.Count; j++)
            {
                if (inventoryDetails[j].m_itemId.m_SteamItemInstanceID == instanceId)
                {
                    num = j;
                    break;
                }
            }
            if (num == -1)
            {
                UnturnedLog.error("Unable to find item for exchange: {0}", instanceId);
                return;
            }
            SteamItemDetails_t steamItemDetails_t = inventoryDetails[num];
            UnturnedLog.info(steamItemDetails_t.m_iDefinition.m_SteamItemDef + " [" + instanceId + "] x" + destroy[i].quantity);
            if (destroy[i].quantity >= steamItemDetails_t.m_unQuantity)
            {
                UnturnedLog.info("Locally removed item - Instance: {0} Definition: {1}", instanceId, steamItemDetails_t.m_iDefinition.m_SteamItemDef);
                inventoryDetails.RemoveAtFast(num);
                dynamicInventoryDetails.Remove(instanceId);
            }
        }
        SteamItemDef_t[] array = new SteamItemDef_t[1];
        uint[] array2 = new uint[1];
        array[0] = (SteamItemDef_t)generate;
        array2[0] = 1u;
        SteamItemInstanceID_t[] array3 = new SteamItemInstanceID_t[destroy.Count];
        uint[] array4 = new uint[destroy.Count];
        for (int k = 0; k < destroy.Count; k++)
        {
            array3[k] = (SteamItemInstanceID_t)destroy[k].instanceId;
            array4[k] = destroy[k].quantity;
        }
        SteamInventory.ExchangeItems(out exchangeResult, array, array2, (uint)array.Length, array3, array4, (uint)array3.Length);
    }

    public void updateInventory()
    {
        if (!(Time.realtimeSinceStartup - lastHeartbeat < 30f))
        {
            lastHeartbeat = Time.realtimeSinceStartup;
            SteamInventory.SendItemDropHeartbeat();
        }
    }

    public void dropInventory()
    {
        SteamItemDef_t steamItemDef_t = new SteamItemDef_t(SDG.Unturned.Provider.statusData.Stockpile.Playtime_Generator);
        if (steamItemDef_t.m_SteamItemDef > 0)
        {
            UnturnedLog.info($"Requesting playtime drop ({steamItemDef_t})");
            SteamInventory.TriggerItemDrop(out dropResult, steamItemDef_t);
        }
        GrantPromoItems();
    }

    public void GrantPromoItems()
    {
        if (promoResult == SteamInventoryResult_t.Invalid)
        {
            UnturnedLog.info("Requesting promo item grant");
            SteamInventory.GrantPromoItems(out promoResult);
        }
    }

    private void loadInventoryFromResponseFile(string filePath)
    {
        UnturnedLog.info("Loading Steam inventory from GetInventory response file: {0}", filePath);
        List<SteamGetInventoryResponse.Item> list = SteamGetInventoryResponse.parse(filePath);
        dynamicInventoryDetails.Clear();
        inventoryDetails = new List<SteamItemDetails_t>(list.Count);
        foreach (SteamGetInventoryResponse.Item item2 in list)
        {
            SteamItemDetails_t item = default(SteamItemDetails_t);
            item.m_iDefinition = new SteamItemDef_t(item2.itemdefid);
            item.m_itemId = new SteamItemInstanceID_t(item2.itemid);
            item.m_unFlags = 0;
            item.m_unQuantity = item2.quantity;
            inventoryDetails.Add(item);
        }
        if (onInventoryRefreshed != null)
        {
            onInventoryRefreshed();
        }
        isInventoryAvailable = true;
        SDG.Unturned.Provider.isLoadingInventory = false;
    }

    public void refreshInventory()
    {
        UnturnedLog.info("Refreshing Steam inventory");
        string text = Path.Combine(ReadWrite.PATH, "SteamInventory.json");
        if (File.Exists(text))
        {
            try
            {
                loadInventoryFromResponseFile(text);
                return;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e);
                return;
            }
        }
        if (!SteamInventory.GetAllItems(out inventoryResult))
        {
            SDG.Unturned.Provider.isLoadingInventory = false;
        }
    }

    private void addLocalItem(SteamItemDetails_t item, string tags, string dynamic_props)
    {
        inventoryDetails.Add(item);
        dynamicInventoryDetails.Remove(item.m_itemId.m_SteamItemInstanceID);
        DynamicEconDetails value = default(DynamicEconDetails);
        value.tags = (string.IsNullOrEmpty(tags) ? string.Empty : tags);
        value.dynamic_props = (string.IsNullOrEmpty(dynamic_props) ? string.Empty : dynamic_props);
        dynamicInventoryDetails.Add(item.m_itemId.m_SteamItemInstanceID, value);
    }

    private void removeLocalItem(SteamItemDetails_t item)
    {
        for (int i = 0; i < inventoryDetails.Count; i++)
        {
            if (inventoryDetails[i].m_itemId == item.m_itemId)
            {
                inventoryDetails.RemoveAtFast(i);
                break;
            }
        }
        dynamicInventoryDetails.Remove(item.m_itemId.m_SteamItemInstanceID);
    }

    private bool updateLocalItem(SteamItemDetails_t item, SteamInventoryResult_t resultHandle, uint resultIndex)
    {
        removeLocalItem(item);
        bool num = (item.m_unFlags & 0x100) != 0;
        bool flag = item.m_unQuantity < 1;
        if (num || flag)
        {
            UnturnedLog.info("Locally removed item - Instance: {0} Definition: {1}", item.m_itemId, item.m_iDefinition);
            return false;
        }
        uint punValueBufferSizeOut = 1024u;
        SteamInventory.GetResultItemProperty(resultHandle, resultIndex, "tags", out var pchValueBuffer, ref punValueBufferSizeOut);
        uint punValueBufferSizeOut2 = 1024u;
        SteamInventory.GetResultItemProperty(resultHandle, resultIndex, "dynamic_props", out var pchValueBuffer2, ref punValueBufferSizeOut2);
        addLocalItem(item, pchValueBuffer, pchValueBuffer2);
        UnturnedLog.info("Locally added or updated item - Instance: {0} Definition: {1} Tags: {2} Props: {3}", item.m_itemId, item.m_iDefinition, pchValueBuffer, pchValueBuffer2);
        return true;
    }

    private void handleServerResultReady(SteamInventoryResultReady_t callback)
    {
        SteamPending steamPending = null;
        for (int i = 0; i < SDG.Unturned.Provider.pending.Count; i++)
        {
            if (SDG.Unturned.Provider.pending[i].inventoryResult == callback.m_handle)
            {
                steamPending = SDG.Unturned.Provider.pending[i];
                break;
            }
        }
        if (steamPending == null)
        {
            return;
        }
        if (callback.m_result != EResult.k_EResultOK || !SteamGameServerInventory.CheckResultSteamID(callback.m_handle, steamPending.playerID.steamID))
        {
            UnturnedLog.info("inventory auth: " + callback.m_result.ToString() + " " + SteamGameServerInventory.CheckResultSteamID(callback.m_handle, steamPending.playerID.steamID));
            SDG.Unturned.Provider.reject(steamPending.playerID.steamID, ESteamRejection.AUTH_ECON_VERIFY);
            return;
        }
        uint punOutItemsArraySize = 0u;
        if (SteamGameServerInventory.GetResultItems(callback.m_handle, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            steamPending.inventoryDetails = new SteamItemDetails_t[punOutItemsArraySize];
            SteamGameServerInventory.GetResultItems(callback.m_handle, steamPending.inventoryDetails, ref punOutItemsArraySize);
            for (uint num = 0u; num < punOutItemsArraySize; num++)
            {
                uint punValueBufferSizeOut = 1024u;
                SteamGameServerInventory.GetResultItemProperty(callback.m_handle, num, "tags", out var pchValueBuffer, ref punValueBufferSizeOut);
                uint punValueBufferSizeOut2 = 1024u;
                SteamGameServerInventory.GetResultItemProperty(callback.m_handle, num, "dynamic_props", out var pchValueBuffer2, ref punValueBufferSizeOut2);
                DynamicEconDetails value = default(DynamicEconDetails);
                value.tags = (string.IsNullOrEmpty(pchValueBuffer) ? string.Empty : pchValueBuffer);
                value.dynamic_props = (string.IsNullOrEmpty(pchValueBuffer2) ? string.Empty : pchValueBuffer2);
                steamPending.dynamicInventoryDetails.Add(steamPending.inventoryDetails[num].m_itemId.m_SteamItemInstanceID, value);
            }
        }
        steamPending.inventoryDetailsReady();
    }

    private void handleClientExchangeResultReady(SteamInventoryResultReady_t callback)
    {
        SteamInventoryResult_t handle = callback.m_handle;
        List<SteamItemDetails_t> list = new List<SteamItemDetails_t>();
        uint punOutItemsArraySize = 0u;
        if (SteamInventory.GetResultItems(handle, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            UnturnedLog.info("Exchange result items: {0}", punOutItemsArraySize);
            SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
            if (SteamInventory.GetResultItems(handle, array, ref punOutItemsArraySize))
            {
                for (uint num = 0u; num < punOutItemsArraySize; num++)
                {
                    SteamItemDetails_t item = array[num];
                    if (updateLocalItem(item, handle, num))
                    {
                        list.Add(item);
                    }
                }
            }
        }
        if (list.Count > 0)
        {
            if (onInventoryExchanged != null)
            {
                onInventoryExchanged(list);
            }
            if (onInventoryRefreshed != null)
            {
                onInventoryRefreshed();
            }
        }
        else if (onInventoryExchangeFailed != null)
        {
            onInventoryExchangeFailed();
        }
    }

    private void handleClientPurchaseResultReady(SteamInventoryResultReady_t callback)
    {
        SteamInventoryResult_t handle = callback.m_handle;
        List<SteamItemDetails_t> list = new List<SteamItemDetails_t>();
        uint punOutItemsArraySize = 0u;
        if (SteamInventory.GetResultItems(handle, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            UnturnedLog.info("Purchase result items: {0}", punOutItemsArraySize);
            SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
            if (SteamInventory.GetResultItems(handle, array, ref punOutItemsArraySize))
            {
                for (uint num = 0u; num < punOutItemsArraySize; num++)
                {
                    SteamItemDetails_t item = array[num];
                    if (updateLocalItem(item, handle, num))
                    {
                        list.Add(item);
                    }
                }
            }
        }
        if (list.Count > 0)
        {
            onInventoryPurchased(list);
        }
        if (onInventoryRefreshed != null)
        {
            onInventoryRefreshed();
        }
    }

    private void UpdateLocalItemsFromUnknownResult(SteamInventoryResult_t resultHandle)
    {
        uint punOutItemsArraySize = 0u;
        if (SteamInventory.GetResultItems(resultHandle, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
            if (SteamInventory.GetResultItems(resultHandle, array, ref punOutItemsArraySize))
            {
                for (uint num = 0u; num < punOutItemsArraySize; num++)
                {
                    updateLocalItem(array[num], resultHandle, num);
                }
            }
        }
        if (onInventoryRefreshed != null)
        {
            onInventoryRefreshed();
        }
    }

    private void DumpInventoryResult(SteamInventoryResult_t handle)
    {
        uint punOutItemsArraySize = 0u;
        if (!SteamInventory.GetResultItems(handle, null, ref punOutItemsArraySize))
        {
            UnturnedLog.warn("Unable to get result items length from handle {0}", handle);
            return;
        }
        if (punOutItemsArraySize < 1)
        {
            UnturnedLog.info("Handle {0} result items empty", handle);
            return;
        }
        UnturnedLog.info("Handle {0} contains {1} result item(s)", handle, punOutItemsArraySize);
        SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
        if (!SteamInventory.GetResultItems(handle, array, ref punOutItemsArraySize))
        {
            UnturnedLog.warn("Unable to get result items from handle {0}", handle);
            return;
        }
        for (uint num = 0u; num < punOutItemsArraySize; num++)
        {
            SteamItemDetails_t steamItemDetails_t = array[num];
            UnturnedLog.info("[{0}] Instance: {1} Def: {2} Quantity: {3} Flags: {4}", num, steamItemDetails_t.m_itemId, steamItemDetails_t.m_iDefinition, steamItemDetails_t.m_unQuantity, (ESteamItemFlags)steamItemDetails_t.m_unFlags);
        }
    }

    private void handleClientResultReady(SteamInventoryResultReady_t callback)
    {
        if (promoResult != SteamInventoryResult_t.Invalid && callback.m_handle == promoResult)
        {
            SteamInventory.DestroyResult(promoResult);
            promoResult = SteamInventoryResult_t.Invalid;
        }
        else if (exchangeResult != SteamInventoryResult_t.Invalid && callback.m_handle == exchangeResult)
        {
            handleClientExchangeResultReady(callback);
            SteamInventory.DestroyResult(exchangeResult);
            exchangeResult = SteamInventoryResult_t.Invalid;
        }
        else if (dropResult != SteamInventoryResult_t.Invalid && callback.m_handle == dropResult)
        {
            SteamItemDetails_t[] array = null;
            string pchValueBuffer = null;
            string pchValueBuffer2 = null;
            uint punOutItemsArraySize = 0u;
            if (SteamInventory.GetResultItems(dropResult, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
            {
                array = new SteamItemDetails_t[punOutItemsArraySize];
                SteamInventory.GetResultItems(dropResult, array, ref punOutItemsArraySize);
                uint punValueBufferSizeOut = 1024u;
                SteamInventory.GetResultItemProperty(dropResult, 0u, "tags", out pchValueBuffer, ref punValueBufferSizeOut);
                uint punValueBufferSizeOut2 = 1024u;
                SteamInventory.GetResultItemProperty(dropResult, 0u, "dynamic_props", out pchValueBuffer2, ref punValueBufferSizeOut2);
            }
            UnturnedLog.info("onInventoryResultReady: Drop {0}", punOutItemsArraySize);
            if (array != null && punOutItemsArraySize != 0)
            {
                SteamItemDetails_t item = array[0];
                addLocalItem(item, pchValueBuffer, pchValueBuffer2);
                if (onInventoryDropped != null)
                {
                    onInventoryDropped(item.m_iDefinition.m_SteamItemDef, item.m_unQuantity, item.m_itemId.m_SteamItemInstanceID);
                }
                if (onInventoryRefreshed != null)
                {
                    onInventoryRefreshed();
                }
            }
            SteamInventory.DestroyResult(dropResult);
            dropResult = SteamInventoryResult_t.Invalid;
        }
        else
        {
            if (wearingResult != SteamInventoryResult_t.Invalid && callback.m_handle == wearingResult)
            {
                return;
            }
            if (inventoryResult != SteamInventoryResult_t.Invalid && callback.m_handle == inventoryResult)
            {
                dynamicInventoryDetails.Clear();
                uint punOutItemsArraySize2 = 0u;
                if (SteamInventory.GetResultItems(inventoryResult, null, ref punOutItemsArraySize2) && punOutItemsArraySize2 != 0)
                {
                    SteamItemDetails_t[] array2 = new SteamItemDetails_t[punOutItemsArraySize2];
                    SteamInventory.GetResultItems(inventoryResult, array2, ref punOutItemsArraySize2);
                    for (uint num = 0u; num < punOutItemsArraySize2; num++)
                    {
                        uint punValueBufferSizeOut3 = 1024u;
                        SteamInventory.GetResultItemProperty(inventoryResult, num, "tags", out var pchValueBuffer3, ref punValueBufferSizeOut3);
                        uint punValueBufferSizeOut4 = 1024u;
                        SteamInventory.GetResultItemProperty(inventoryResult, num, "dynamic_props", out var pchValueBuffer4, ref punValueBufferSizeOut4);
                        DynamicEconDetails value = default(DynamicEconDetails);
                        value.tags = (string.IsNullOrEmpty(pchValueBuffer3) ? string.Empty : pchValueBuffer3);
                        value.dynamic_props = (string.IsNullOrEmpty(pchValueBuffer4) ? string.Empty : pchValueBuffer4);
                        dynamicInventoryDetails.Add(array2[num].m_itemId.m_SteamItemInstanceID, value);
                    }
                    inventoryDetails = new List<SteamItemDetails_t>(array2);
                }
                if (onInventoryRefreshed != null)
                {
                    onInventoryRefreshed();
                }
                isInventoryAvailable = true;
                SDG.Unturned.Provider.isLoadingInventory = false;
                SteamInventory.DestroyResult(inventoryResult);
                inventoryResult = SteamInventoryResult_t.Invalid;
            }
            else if (commitResult != SteamInventoryResult_t.Invalid && callback.m_handle == commitResult)
            {
                UnturnedLog.info("Commit dynamic properties result: " + callback.m_result);
                SteamInventory.DestroyResult(commitResult);
                commitResult = SteamInventoryResult_t.Invalid;
            }
            else if (isExpectingPurchaseResult)
            {
                isExpectingPurchaseResult = false;
                handleClientPurchaseResultReady(callback);
                SteamInventory.DestroyResult(callback.m_handle);
            }
            else
            {
                UnturnedLog.info("Unexpected client inventory result ready callback  - Handle: {0} Result: {1}", callback.m_handle, callback.m_result);
                UpdateLocalItemsFromUnknownResult(callback.m_handle);
                DumpInventoryResult(callback.m_handle);
                SteamInventory.DestroyResult(callback.m_handle);
            }
        }
    }

    private void onInventoryResultReady(SteamInventoryResultReady_t callback)
    {
        if (appInfo.isDedicated)
        {
            handleServerResultReady(callback);
        }
        else
        {
            handleClientResultReady(callback);
        }
    }

    public void loadTranslationEconInfo()
    {
        if (SDG.Unturned.Provider.language == "English")
        {
            return;
        }
        string text = SDG.Unturned.Provider.localizationRoot + "/EconInfo.json";
        if (!ReadWrite.fileExists(text, useCloud: false, usePath: false))
        {
            UnturnedLog.warn("Looked for economy translation at: {0}", text);
            return;
        }
        JSONDeserializer jSONDeserializer = new JSONDeserializer();
        new List<UnturnedEconInfo>();
        foreach (UnturnedEconInfo translatedItem in ((IDeserializer)jSONDeserializer).deserialize<List<UnturnedEconInfo>>(text))
        {
            UnturnedEconInfo unturnedEconInfo = econInfo.Find((UnturnedEconInfo x) => x.itemdefid == translatedItem.itemdefid);
            if (unturnedEconInfo != null)
            {
                unturnedEconInfo.name = translatedItem.name;
                unturnedEconInfo.type = translatedItem.type;
                unturnedEconInfo.description = translatedItem.description;
            }
        }
    }

    public string getCountryWarningId()
    {
        return SteamUtils.GetIPCountry();
    }

    public bool isItemHiddenByCountryRestrictions(int itemdefid)
    {
        if (doesCountryAllowRandomItems)
        {
            return false;
        }
        ushort inventoryItemID = getInventoryItemID(itemdefid);
        if (Assets.find(EAssetType.ITEM, inventoryItemID) is ItemAsset itemAsset)
        {
            if (itemAsset.type != EItemType.KEY)
            {
                return itemAsset.type == EItemType.BOX;
            }
            return true;
        }
        return false;
    }

    private void initCountryRestrictions()
    {
        string iPCountry = SteamUtils.GetIPCountry();
        if (string.IsNullOrWhiteSpace(iPCountry))
        {
            hasCountryDetails = false;
            doesCountryAllowRandomItems = false;
            UnturnedLog.warn("Unable to determine country/region");
        }
        else
        {
            hasCountryDetails = true;
            doesCountryAllowRandomItems = true;
            doesCountryAllowRandomItems &= !string.Equals(iPCountry, "BE");
            doesCountryAllowRandomItems &= !string.Equals(iPCountry, "NL");
        }
    }

    public void initializeClient()
    {
        initCountryRestrictions();
    }

    public TempSteamworksEconomy(SteamworksAppInfo newAppInfo)
    {
        appInfo = newAppInfo;
        using (FileStream underlyingStream = new FileStream((UnityPaths.ProjectDirectory == null) ? PathEx.Join(UnturnedPaths.RootDirectory, "EconInfo.json") : PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "Shared_Release", "EconInfo.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
            using StreamReader reader = new StreamReader(sHA1Stream);
            using JsonTextReader reader2 = new JsonTextReader(reader);
            econInfo = new JsonSerializer().Deserialize<List<UnturnedEconInfo>>(reader2);
            econInfoHash = sHA1Stream.Hash;
        }
        if (appInfo.isDedicated)
        {
            inventoryResultReady = Callback<SteamInventoryResultReady_t>.CreateGameServer(onInventoryResultReady);
        }
        else
        {
            inventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(onInventoryResultReady);
        }
    }
}
