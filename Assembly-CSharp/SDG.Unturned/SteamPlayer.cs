using System;
using System.Collections.Generic;
using System.Net;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SteamPlayer : SteamConnectedClientBase
{
    private NetId _netId;

    private SteamPlayerID _playerID;

    private Transform _model;

    private Player _player;

    private bool _isPro;

    private int _channel;

    internal int battlEyeId;

    private bool _isAdmin;

    private float[] pings;

    private float _ping;

    private float _joined;

    public byte face;

    private byte _hair;

    private byte _beard;

    private Color _skin;

    private Color _color;

    private Color _markerColor;

    private bool _hand;

    public int shirtItem;

    public int pantsItem;

    public int hatItem;

    public int backpackItem;

    public int vestItem;

    public int maskItem;

    public int glassesItem;

    public int[] skinItems;

    public string[] skinTags;

    public string[] skinDynamicProps;

    public Dictionary<ushort, int> itemSkins;

    public Dictionary<ushort, int> vehicleSkins;

    public HashSet<ushort> modifiedItems;

    private bool submittedModifiedItems;

    private EPlayerSkillset _skillset;

    private string _language;

    public float timeLastPacketWasReceivedFromClient;

    public float timeLastPingRequestWasSentToClient;

    public float lastChat;

    public float nextVote;

    public bool isMuted;

    [Obsolete("This field should not have been externally used and will be removed in a future version.")]
    public float rpcCredits;

    public float lastReceivedPingRequestRealtime;

    public float[] rpcAllowedTimes = new float[NetReflection.rateLimitedMethodsCount];

    internal int[] rpcHitCount = new int[NetReflection.rateLimitedMethodsCount];

    internal EClientPlatform clientPlatform;

    internal HashSet<Guid> validatedGuids = new HashSet<Guid>();

    public SteamPlayerID playerID => _playerID;

    public Transform model => _model;

    public Player player => _player;

    public bool isPro
    {
        get
        {
            if (OptionsSettings.streamer && playerID.steamID != Provider.user)
            {
                return false;
            }
            return _isPro;
        }
    }

    public int channel => _channel;

    public bool isAdmin
    {
        get
        {
            if (OptionsSettings.streamer && playerID.steamID != Provider.user)
            {
                return false;
            }
            return _isAdmin;
        }
        set
        {
            _isAdmin = value;
        }
    }

    public float ping => _ping;

    public float joined => _joined;

    public byte hair => _hair;

    public byte beard => _beard;

    public Color skin => _skin;

    public Color color => _color;

    public Color markerColor => _markerColor;

    public bool hand => _hand;

    public EPlayerSkillset skillset => _skillset;

    public string language => _language;

    public CSteamID lobbyID { get; private set; }

    public NetId GetNetId()
    {
        return _netId;
    }

    public bool getItemSkinItemDefID(ushort itemID, out int itemdefid)
    {
        itemdefid = 0;
        if (itemSkins == null)
        {
            return false;
        }
        return itemSkins.TryGetValue(itemID, out itemdefid);
    }

    public bool getVehicleSkinItemDefID(ushort vehicleID, out int itemdefid)
    {
        itemdefid = 0;
        if (vehicleSkins == null)
        {
            return false;
        }
        return vehicleSkins.TryGetValue(vehicleID, out itemdefid);
    }

    public bool getTagsAndDynamicPropsForItem(int item, out string tags, out string dynamic_props)
    {
        tags = string.Empty;
        dynamic_props = string.Empty;
        for (int i = 0; i < skinItems.Length; i++)
        {
            if (skinItems[i] == item)
            {
                if (i < skinTags.Length && i < skinDynamicProps.Length)
                {
                    tags = skinTags[i];
                    dynamic_props = skinDynamicProps[i];
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    public bool getDynamicEconDetails(ushort itemID, out DynamicEconDetails details)
    {
        if (!getItemSkinItemDefID(itemID, out var itemdefid))
        {
            details = default(DynamicEconDetails);
            return false;
        }
        return getDynamicEconDetailsForItemDef(itemdefid, out details);
    }

    public bool getDynamicEconDetailsForItemDef(int itemdefid, out DynamicEconDetails details)
    {
        if (!getTagsAndDynamicPropsForItem(itemdefid, out var tags, out var dynamic_props))
        {
            details = default(DynamicEconDetails);
            return false;
        }
        details = new DynamicEconDetails(tags, dynamic_props);
        return true;
    }

    public bool getStatTrackerValue(ushort itemID, out EStatTrackerType type, out int kills)
    {
        if (!getDynamicEconDetails(itemID, out var details))
        {
            type = EStatTrackerType.NONE;
            kills = -1;
            return false;
        }
        return details.getStatTrackerValue(out type, out kills);
    }

    public bool getRagdollEffect(ushort itemID, out ERagdollEffect effect)
    {
        if (!getDynamicEconDetails(itemID, out var details))
        {
            effect = ERagdollEffect.NONE;
            return false;
        }
        return details.getRagdollEffect(out effect);
    }

    public ushort getParticleEffectForItemDef(int itemdefid)
    {
        if (getDynamicEconDetailsForItemDef(itemdefid, out var details))
        {
            return details.getParticleEffect();
        }
        return 0;
    }

    public void incrementStatTrackerValue(ushort itemID, EPlayerStat stat)
    {
        if (!getItemSkinItemDefID(itemID, out var itemdefid) || !getTagsAndDynamicPropsForItem(itemdefid, out var tags, out var dynamic_props))
        {
            return;
        }
        DynamicEconDetails dynamicEconDetails = new DynamicEconDetails(tags, dynamic_props);
        if (!dynamicEconDetails.getStatTrackerValue(out var type, out var kills))
        {
            return;
        }
        switch (type)
        {
        default:
            return;
        case EStatTrackerType.TOTAL:
            if (stat != EPlayerStat.KILLS_ANIMALS && stat != EPlayerStat.KILLS_PLAYERS && stat != EPlayerStat.KILLS_ZOMBIES_MEGA && stat != EPlayerStat.KILLS_ZOMBIES_NORMAL)
            {
                return;
            }
            break;
        case EStatTrackerType.PLAYER:
            if (stat != EPlayerStat.KILLS_PLAYERS)
            {
                return;
            }
            break;
        }
        if (!modifiedItems.Contains(itemID))
        {
            modifiedItems.Add(itemID);
        }
        kills++;
        for (int i = 0; i < skinItems.Length; i++)
        {
            if (skinItems[i] == itemdefid)
            {
                if (i < skinDynamicProps.Length)
                {
                    skinDynamicProps[i] = dynamicEconDetails.getPredictedDynamicPropsJsonForStatTracker(type, kills);
                }
                break;
            }
        }
    }

    public void commitModifiedDynamicProps()
    {
        if (modifiedItems.Count < 1 || submittedModifiedItems)
        {
            return;
        }
        submittedModifiedItems = true;
        SteamInventoryUpdateHandle_t handle = SteamInventory.StartUpdateProperties();
        int num = 0;
        foreach (ushort modifiedItem in modifiedItems)
        {
            if (Characters.getPackageForItemID(modifiedItem, out var package) && getStatTrackerValue(modifiedItem, out var type, out var kills))
            {
                string statTrackerPropertyName = Provider.provider.economyService.getStatTrackerPropertyName(type);
                if (!string.IsNullOrEmpty(statTrackerPropertyName))
                {
                    SteamInventory.SetProperty(handle, new SteamItemInstanceID_t(package), statTrackerPropertyName, kills);
                    num++;
                }
            }
        }
        SteamInventory.SubmitUpdateProperties(handle, out Provider.provider.economyService.commitResult);
        UnturnedLog.info($"Submitted {num} item property update(s)");
    }

    public void lag(float value)
    {
        value = Mathf.Clamp01(value);
        _ping = value;
        for (int num = pings.Length - 1; num > 0; num--)
        {
            pings[num] = pings[num - 1];
            if (pings[num] > 0.001f)
            {
                _ping += pings[num];
            }
        }
        _ping /= pings.Length;
        pings[0] = value;
    }

    public bool isMemberOfSameGroupAs(Player other)
    {
        if (player != null && other != null)
        {
            return player.quests.isMemberOfSameGroupAs(other);
        }
        return false;
    }

    public bool isMemberOfSameGroupAs(SteamPlayer other)
    {
        if (other != null)
        {
            return isMemberOfSameGroupAs(other.player);
        }
        return false;
    }

    public bool getIPv4Address(out uint address)
    {
        if (base.transportConnection != null)
        {
            return base.transportConnection.TryGetIPv4Address(out address);
        }
        address = 0u;
        return false;
    }

    public uint getIPv4AddressOrZero()
    {
        getIPv4Address(out var address);
        return address;
    }

    public IPAddress getAddress()
    {
        if (base.transportConnection != null)
        {
            return base.transportConnection.GetAddress();
        }
        return null;
    }

    public string getAddressString(bool withPort)
    {
        if (base.transportConnection != null)
        {
            return base.transportConnection.GetAddressString(withPort);
        }
        return null;
    }

    public bool Equals(SteamPlayer otherClient)
    {
        if (otherClient != null)
        {
            return playerID.Equals(otherClient.playerID);
        }
        return false;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SteamPlayer);
    }

    public override int GetHashCode()
    {
        return playerID.GetHashCode();
    }

    internal string GetLocalDisplayName()
    {
        if (!string.IsNullOrEmpty(playerID.nickName) && playerID.steamID != Provider.client && player != null && player.quests != null && Player.player != null && player.quests.isMemberOfSameGroupAs(Player.player))
        {
            return playerID.nickName;
        }
        return playerID.characterName;
    }

    public SteamPlayer(ITransportConnection transportConnection, NetId netId, SteamPlayerID newPlayerID, Transform newModel, bool newPro, bool newAdmin, int newChannel, byte newFace, byte newHair, byte newBeard, Color newSkin, Color newColor, Color newMarkerColor, bool newHand, int newShirtItem, int newPantsItem, int newHatItem, int newBackpackItem, int newVestItem, int newMaskItem, int newGlassesItem, int[] newSkinItems, string[] newSkinTags, string[] newSkinDynamicProps, EPlayerSkillset newSkillset, string newLanguage, CSteamID newLobbyID, EClientPlatform clientPlatform)
    {
        base.transportConnection = transportConnection;
        _netId = netId;
        NetIdRegistry.Assign(_netId, this);
        _playerID = newPlayerID;
        _model = newModel;
        model.name = playerID.characterName + " [" + playerID.playerName + "]";
        model.GetComponent<SteamChannel>().id = newChannel;
        model.GetComponent<SteamChannel>().owner = this;
        model.GetComponent<SteamChannel>().isOwner = playerID.steamID == Provider.client;
        model.GetComponent<SteamChannel>().setup();
        _player = model.GetComponent<Player>();
        _player.AssignNetIdBlock(_netId);
        _isPro = newPro;
        _channel = newChannel;
        isAdmin = newAdmin;
        face = newFace;
        _hair = newHair;
        _beard = newBeard;
        _skin = newSkin;
        _color = newColor;
        _markerColor = newMarkerColor;
        _hand = newHand;
        _skillset = newSkillset;
        _language = newLanguage;
        shirtItem = newShirtItem;
        pantsItem = newPantsItem;
        hatItem = newHatItem;
        backpackItem = newBackpackItem;
        vestItem = newVestItem;
        maskItem = newMaskItem;
        glassesItem = newGlassesItem;
        skinItems = newSkinItems;
        skinTags = newSkinTags;
        skinDynamicProps = newSkinDynamicProps;
        itemSkins = new Dictionary<ushort, int>();
        vehicleSkins = new Dictionary<ushort, int>();
        modifiedItems = new HashSet<ushort>();
        for (int i = 0; i < skinItems.Length; i++)
        {
            int num = skinItems[i];
            if (num == 0)
            {
                continue;
            }
            Provider.provider.economyService.getInventoryTargetID(num, out var item_id, out var vehicle_id);
            if (item_id > 0)
            {
                if (!itemSkins.ContainsKey(item_id))
                {
                    itemSkins.Add(item_id, num);
                }
            }
            else if (vehicle_id > 0 && !vehicleSkins.ContainsKey(item_id))
            {
                vehicleSkins.Add(vehicle_id, num);
            }
        }
        pings = new float[4];
        timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
        lastChat = Time.realtimeSinceStartup;
        nextVote = Time.realtimeSinceStartup;
        lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
        _joined = Time.realtimeSinceStartup;
        lobbyID = newLobbyID;
        this.clientPlatform = clientPlatform;
    }
}
