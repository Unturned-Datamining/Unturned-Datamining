using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SteamPending : SteamConnectedClientBase
{
    private SteamPlayerID _playerID;

    private bool _isPro;

    private byte _face;

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

    public ulong packageShirt;

    public ulong packagePants;

    public ulong packageHat;

    public ulong packageBackpack;

    public ulong packageVest;

    public ulong packageMask;

    public ulong packageGlasses;

    public ulong[] packageSkins;

    public SteamInventoryResult_t inventoryResult = SteamInventoryResult_t.Invalid;

    public SteamItemDetails_t[] inventoryDetails;

    public Dictionary<ulong, DynamicEconDetails> dynamicInventoryDetails = new Dictionary<ulong, DynamicEconDetails>();

    public bool assignedPro;

    public bool assignedAdmin;

    public bool hasAuthentication;

    public bool hasProof;

    public bool hasGroup;

    private EPlayerSkillset _skillset;

    private string _language;

    public float lastReceivedPingRequestRealtime;

    private float sentVerifyPacketRealtime;

    internal EClientPlatform clientPlatform;

    internal int lastNotifiedQueuePosition;

    public SteamPlayerID playerID => _playerID;

    public bool isPro => _isPro;

    public byte face => _face;

    public byte hair => _hair;

    public byte beard => _beard;

    public Color skin => _skin;

    public Color color => _color;

    public Color markerColor => _markerColor;

    public bool hand => _hand;

    public bool canAcceptYet
    {
        get
        {
            if (hasAuthentication && hasProof)
            {
                return hasGroup;
            }
            return false;
        }
    }

    public EPlayerSkillset skillset => _skillset;

    public string language => _language;

    public bool hasSentVerifyPacket => sentVerifyPacketRealtime > -0.5f;

    public float realtimeSinceSentVerifyPacket => Time.realtimeSinceStartup - sentVerifyPacketRealtime;

    public CSteamID lobbyID { get; private set; }

    public void sendVerifyPacket()
    {
        if (hasSentVerifyPacket)
        {
            return;
        }
        sentVerifyPacketRealtime = Time.realtimeSinceStartup;
        if (!playerID.steamID.IsValid())
        {
            return;
        }
        NetMessages.SendMessageToClient(EClientMessage.Verify, ENetReliability.Reliable, base.transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteList(MasterBundleValidation.eligibleBundleNames, (string name) => writer.WriteString(name), ClientMessageHandler_Verify.BUNDLE_NAMES_LENGTH);
        });
    }

    public void inventoryDetailsReady()
    {
        shirtItem = getInventoryItem(packageShirt);
        pantsItem = getInventoryItem(packagePants);
        hatItem = getInventoryItem(packageHat);
        backpackItem = getInventoryItem(packageBackpack);
        vestItem = getInventoryItem(packageVest);
        maskItem = getInventoryItem(packageMask);
        glassesItem = getInventoryItem(packageGlasses);
        List<int> list = new List<int>();
        List<string> list2 = new List<string>();
        List<string> list3 = new List<string>();
        for (int i = 0; i < packageSkins.Length; i++)
        {
            ulong num = packageSkins[i];
            if (num == 0L)
            {
                continue;
            }
            int inventoryItem = getInventoryItem(num);
            if (inventoryItem != 0)
            {
                list.Add(inventoryItem);
                if (dynamicInventoryDetails.TryGetValue(num, out var value))
                {
                    list2.Add(value.tags);
                    list3.Add(value.dynamic_props);
                }
                else
                {
                    list2.Add(string.Empty);
                    list3.Add(string.Empty);
                }
            }
        }
        skinItems = list.ToArray();
        skinTags = list2.ToArray();
        skinDynamicProps = list3.ToArray();
        hasProof = true;
        if (canAcceptYet)
        {
            Provider.accept(this);
        }
    }

    public int getInventoryItem(ulong package)
    {
        if (inventoryDetails != null)
        {
            for (int i = 0; i < inventoryDetails.Length; i++)
            {
                if (inventoryDetails[i].m_itemId.m_SteamItemInstanceID == package)
                {
                    return inventoryDetails[i].m_iDefinition.m_SteamItemDef;
                }
            }
        }
        return 0;
    }

    public SteamPending(ITransportConnection transportConnection, SteamPlayerID newPlayerID, bool newPro, byte newFace, byte newHair, byte newBeard, Color newSkin, Color newColor, Color newMarkerColor, bool newHand, ulong newPackageShirt, ulong newPackagePants, ulong newPackageHat, ulong newPackageBackpack, ulong newPackageVest, ulong newPackageMask, ulong newPackageGlasses, ulong[] newPackageSkins, EPlayerSkillset newSkillset, string newLanguage, CSteamID newLobbyID, EClientPlatform clientPlatform)
    {
        base.transportConnection = transportConnection;
        _playerID = newPlayerID;
        _isPro = newPro;
        _face = newFace;
        _hair = newHair;
        _beard = newBeard;
        _skin = newSkin;
        _color = newColor;
        _markerColor = newMarkerColor;
        _hand = newHand;
        _skillset = newSkillset;
        _language = newLanguage;
        packageShirt = newPackageShirt;
        packagePants = newPackagePants;
        packageHat = newPackageHat;
        packageBackpack = newPackageBackpack;
        packageVest = newPackageVest;
        packageMask = newPackageMask;
        packageGlasses = newPackageGlasses;
        packageSkins = newPackageSkins;
        lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
        sentVerifyPacketRealtime = -1f;
        lobbyID = newLobbyID;
        this.clientPlatform = clientPlatform;
    }

    public SteamPending()
    {
        _playerID = new SteamPlayerID(CSteamID.Nil, 0, "Player Name", "Character Name", "Nick Name", CSteamID.Nil);
        lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
        sentVerifyPacketRealtime = -1f;
    }

    internal string GetQueueStateDebugString()
    {
        if (hasSentVerifyPacket)
        {
            if (canAcceptYet)
            {
                return "ready to accept from queue";
            }
            return $"hasAuthentication: {hasAuthentication} hasProof: {hasProof} hasGroup: {hasGroup}";
        }
        return "normal waiting in queue";
    }
}
