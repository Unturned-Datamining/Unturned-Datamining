using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerQuests : PlayerCaller
{
    public delegate void AnyFlagChangedHandler(PlayerQuests quests, PlayerQuestFlag flag);

    public delegate void GroupChangedCallback(PlayerQuests sender, CSteamID oldGroupID, EPlayerGroupRank oldGroupRank, CSteamID newGroupID, EPlayerGroupRank newGroupRank);

    private const byte SAVEDATA_VERSION_ADDED_NPC_SPAWN_ID = 8;

    private const byte SAVEDATA_VERSION_ADDED_TRACKED_QUEST_GUID = 9;

    private const byte SAVEDATA_VERSION_ADDED_QUEST_LIST_GUIDS = 10;

    private const byte SAVEDATA_VERSION_ADDED_NPC_CUTSCENE_MODE = 11;

    private const byte SAVEDATA_VERSION_NEWEST = 11;

    public static readonly byte SAVEDATA_VERSION = 11;

    public static readonly uint DEFAULT_RADIO_FREQUENCY = 460327u;

    private static PlayerQuestFlagComparator flagComparator = new PlayerQuestFlagComparator();

    private static PlayerQuestComparator questComparator = new PlayerQuestComparator();

    public InteractableObjectNPC checkNPC;

    private DialogueAsset serverCurrentDialogueAsset;

    private VendorAsset serverCurrentVendorAsset;

    private DialogueMessage serverCurrentDialogueMessage;

    /// <summary>
    /// The dialogue to go to when a message has no available responses.
    /// If this is not specified the previous dialogue is used as a default.
    /// </summary>
    private DialogueAsset serverDefaultNextDialogueAsset;

    private Dictionary<ushort, PlayerQuestFlag> flagsMap;

    public ExternalConditionsUpdated onExternalConditionsUpdated;

    public FlagsUpdated onFlagsUpdated;

    public FlagUpdated onFlagUpdated;

    public static GroupUpdatedHandler groupUpdated;

    private QuestAsset _trackedQuest;

    private bool _isMarkerPlaced;

    private Vector3 _markerPosition;

    /// <summary>
    /// Overrides label text next to marker on map.
    /// Used by plugins. Not saved to disk.
    /// </summary>
    private string _markerTextOverride;

    private uint _radioFrequency;

    private CSteamID _groupID;

    private EPlayerGroupRank _groupRank;

    /// <summary>
    /// Kept serverside. Used to check whether the player is currently in their Steam group or just a normal in-game group.
    /// </summary>
    private bool inMainGroup;

    /// <summary>
    /// If set, default spawn logic will check for a location node or spawnpoint node matching name.
    /// Saved and loaded between sessions.
    /// </summary>
    public string npcSpawnId;

    private bool npcCutsceneMode;

    private static readonly ClientInstanceMethod<bool> SendCutsceneMode = ClientInstanceMethod<bool>.Get(typeof(PlayerQuests), "ReceiveCutsceneMode");

    private static readonly ClientInstanceMethod<bool, Vector3, string> SendMarkerState = ClientInstanceMethod<bool, Vector3, string>.Get(typeof(PlayerQuests), "ReceiveMarkerState");

    private static readonly ServerInstanceMethod<bool, Vector3> SendSetMarkerRequest = ServerInstanceMethod<bool, Vector3>.Get(typeof(PlayerQuests), "ReceiveSetMarkerRequest");

    private static readonly ClientInstanceMethod<uint> SendRadioFrequencyState = ClientInstanceMethod<uint>.Get(typeof(PlayerQuests), "ReceiveRadioFrequencyState");

    private static readonly ServerInstanceMethod<uint> SendSetRadioFrequencyRequest = ServerInstanceMethod<uint>.Get(typeof(PlayerQuests), "ReceiveSetRadioFrequencyRequest");

    private static readonly ClientInstanceMethod<CSteamID, EPlayerGroupRank> SendGroupState = ClientInstanceMethod<CSteamID, EPlayerGroupRank>.Get(typeof(PlayerQuests), "ReceiveGroupState");

    private static readonly ServerInstanceMethod<CSteamID> SendAcceptGroupInvitationRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveAcceptGroupInvitationRequest");

    private static readonly ServerInstanceMethod<CSteamID> SendDeclineGroupInvitationRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveDeclineGroupInvitationRequest");

    private float lastLeaveGroupRequestRealtime;

    private static readonly ServerInstanceMethod SendLeaveGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), "ReceiveLeaveGroupRequest");

    private static readonly ServerInstanceMethod SendDeleteGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), "ReceiveDeleteGroupRequest");

    private static readonly ServerInstanceMethod SendCreateGroupRequest = ServerInstanceMethod.Get(typeof(PlayerQuests), "ReceiveCreateGroupRequest");

    private static readonly ClientInstanceMethod<CSteamID> SendAddGroupInviteClient = ClientInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveAddGroupInviteClient");

    private static readonly ClientInstanceMethod<CSteamID> SendRemoveGroupInviteClient = ClientInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveRemoveGroupInviteClient");

    private static readonly ServerInstanceMethod<CSteamID> SendAddGroupInviteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveAddGroupInviteRequest");

    private static readonly ServerInstanceMethod<CSteamID> SendPromoteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceivePromoteRequest");

    private static readonly ServerInstanceMethod<CSteamID> SendDemoteRequest = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveDemoteRequest");

    private static readonly ServerInstanceMethod<CSteamID> SendKickFromGroup = ServerInstanceMethod<CSteamID>.Get(typeof(PlayerQuests), "ReceiveKickFromGroup");

    private static readonly ServerInstanceMethod<string> SendRenameGroupRequest = ServerInstanceMethod<string>.Get(typeof(PlayerQuests), "ReceiveRenameGroupRequest");

    private static readonly ServerInstanceMethod<Guid, byte, bool> SendSellToVendor = ServerInstanceMethod<Guid, byte, bool>.Get(typeof(PlayerQuests), "ReceiveSellToVendor");

    private static readonly ServerInstanceMethod<Guid, byte, bool> SendBuyFromVendor = ServerInstanceMethod<Guid, byte, bool>.Get(typeof(PlayerQuests), "ReceiveBuyFromVendor");

    private static readonly ClientInstanceMethod<ushort, short> SendSetFlag = ClientInstanceMethod<ushort, short>.Get(typeof(PlayerQuests), "ReceiveSetFlag");

    private static readonly ClientInstanceMethod<ushort> SendRemoveFlag = ClientInstanceMethod<ushort>.Get(typeof(PlayerQuests), "ReceiveRemoveFlag");

    private static readonly ClientInstanceMethod<Guid> SendAddQuest = ClientInstanceMethod<Guid>.Get(typeof(PlayerQuests), "ReceiveAddQuest");

    private static readonly ClientInstanceMethod<Guid, bool> SendRemoveQuest = ClientInstanceMethod<Guid, bool>.Get(typeof(PlayerQuests), "ReceiveRemoveQuest");

    private static readonly ServerInstanceMethod<Guid> SendTrackQuest = ServerInstanceMethod<Guid>.Get(typeof(PlayerQuests), "ReceiveTrackQuest");

    private static readonly ServerInstanceMethod<Guid> SendAbandonQuestRequest = ServerInstanceMethod<Guid>.Get(typeof(PlayerQuests), "ReceiveAbandonQuestRequest");

    private static readonly ServerInstanceMethod<Guid, byte> SendChooseDialogueResponseRequest = ServerInstanceMethod<Guid, byte>.Get(typeof(PlayerQuests), "ReceiveChooseDialogueResponseRequest");

    private static readonly ServerInstanceMethod<Guid, byte> SendChooseDefaultNextDialogueRequest = ServerInstanceMethod<Guid, byte>.Get(typeof(PlayerQuests), "ReceiveChooseDefaultNextDialogueRequest");

    private static readonly ClientInstanceMethod SendQuests = ClientInstanceMethod.Get(typeof(PlayerQuests), "ReceiveQuests");

    private static readonly ClientInstanceMethod<NetId, Guid, byte, bool> SendTalkWithNpcResponse = ClientInstanceMethod<NetId, Guid, byte, bool>.Get(typeof(PlayerQuests), "ReceiveTalkWithNpcResponse");

    private static readonly ClientInstanceMethod<Guid, byte, bool> SendOpenDialogue = ClientInstanceMethod<Guid, byte, bool>.Get(typeof(PlayerQuests), "ReceiveOpenDialogue");

    private static readonly ClientInstanceMethod<Guid, Guid, byte, bool> SendOpenVendor = ClientInstanceMethod<Guid, Guid, byte, bool>.Get(typeof(PlayerQuests), "ReceiveOpenVendor");

    private GameObject delayedRewardsGameObject;

    private PlayerDelayedQuestRewardsComponent delayedRewardsComponent;

    /// <summary>
    /// Prevent re-creating it during destroy (e.g. plugin granting rewards) from leaking gameobject.
    /// </summary>
    private bool hasCreatedDelayedRewards;

    private bool wasLoadCalled;

    private float lastVehiclePurchaseRealtime = -10f;

    public List<PlayerQuestFlag> flagsList { get; private set; }

    [Obsolete("Replaced by GetTrackedQuest")]
    public ushort TrackedQuestID => _trackedQuest?.id ?? 0;

    public List<PlayerQuest> questsList { get; private set; }

    public bool isMarkerPlaced
    {
        get
        {
            return _isMarkerPlaced;
        }
        private set
        {
            _isMarkerPlaced = value;
        }
    }

    public Vector3 markerPosition
    {
        get
        {
            return _markerPosition;
        }
        private set
        {
            _markerPosition = value;
        }
    }

    public string markerTextOverride
    {
        get
        {
            return _markerTextOverride;
        }
        private set
        {
            _markerTextOverride = value;
        }
    }

    public uint radioFrequency
    {
        get
        {
            return _radioFrequency;
        }
        private set
        {
            _radioFrequency = value;
        }
    }

    public CSteamID groupID
    {
        get
        {
            return _groupID;
        }
        private set
        {
            _groupID = value;
        }
    }

    public EPlayerGroupRank groupRank
    {
        get
        {
            return _groupRank;
        }
        private set
        {
            _groupRank = value;
        }
    }

    public HashSet<CSteamID> groupInvites { get; private set; }

    public bool useMaxGroupMembersLimit => Provider.modeConfigData.Gameplay.Max_Group_Members != 0;

    public bool hasSpaceForMoreMembersInGroup
    {
        get
        {
            if (useMaxGroupMembersLimit)
            {
                return GroupManager.getGroupInfo(groupID)?.hasSpaceForMoreMembersInGroup ?? false;
            }
            return true;
        }
    }

    /// <summary>
    /// Check before allowing changes to this player's <see cref="P:SDG.Unturned.PlayerQuests.groupID" />
    /// </summary>
    public bool canChangeGroupMembership => !LevelManager.isPlayerInArena(base.player);

    /// <summary>
    /// Can rename the group.
    /// </summary>
    public bool hasPermissionToChangeName => groupRank == EPlayerGroupRank.OWNER;

    /// <summary>
    /// Can promote and demote members.
    /// </summary>
    public bool hasPermissionToChangeRank => groupRank == EPlayerGroupRank.OWNER;

    public bool hasPermissionToInviteMembers
    {
        get
        {
            if (groupRank != EPlayerGroupRank.ADMIN)
            {
                return groupRank == EPlayerGroupRank.OWNER;
            }
            return true;
        }
    }

    public bool hasPermissionToKickMembers
    {
        get
        {
            if (groupRank != EPlayerGroupRank.ADMIN)
            {
                return groupRank == EPlayerGroupRank.OWNER;
            }
            return true;
        }
    }

    public bool hasPermissionToCreateGroup => Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups;

    public bool hasPermissionToLeaveGroup
    {
        get
        {
            if (!Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
            {
                return false;
            }
            if (groupRank == EPlayerGroupRank.OWNER)
            {
                GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
                if (groupInfo != null && groupInfo.members > 1)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public bool hasPermissionToDeleteGroup
    {
        get
        {
            if (!Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
            {
                return false;
            }
            if (!inMainGroup)
            {
                return groupRank == EPlayerGroupRank.OWNER;
            }
            return false;
        }
    }

    public bool canBeKickedFromGroup => groupRank != EPlayerGroupRank.OWNER;

    public bool isMemberOfAGroup => groupID != CSteamID.Nil;

    /// <summary>
    /// For level objects with QuestCondition called when quests are added or removed.
    /// </summary>
    internal event Action<ushort> OnLocalPlayerQuestsChanged;

    /// <summary>
    /// Event specifically for plugins to listen to global quest progress.
    /// </summary>
    public static event AnyFlagChangedHandler onAnyFlagChanged;

    /// <summary>
    /// Event for plugins when group or rank changes.
    /// </summary>
    public static event GroupChangedCallback onGroupChanged;

    public event TrackedQuestUpdated TrackedQuestUpdated;

    public event GroupIDChangedHandler groupIDChanged;

    public event GroupRankChangedHandler groupRankChanged;

    public event GroupInvitesChangedHandler groupInvitesChanged;

    public event QuestCompletedHandler questCompleted;

    private static void broadcastGroupChanged(PlayerQuests sender, CSteamID oldGroupID, EPlayerGroupRank oldGroupRank, CSteamID newGroupID, EPlayerGroupRank newGroupRank)
    {
        try
        {
            PlayerQuests.onGroupChanged?.Invoke(sender, oldGroupID, oldGroupRank, newGroupID, newGroupRank);
        }
        catch (Exception e)
        {
            UnturnedLog.warn("Plugin raised an exception from onGroupChanged:");
            UnturnedLog.exception(e);
        }
    }

    private static void triggerGroupUpdated(PlayerQuests sender)
    {
        groupUpdated?.Invoke(sender);
    }

    private void TriggerTrackedQuestUpdated()
    {
        if (this.TrackedQuestUpdated == null)
        {
            return;
        }
        try
        {
            this.TrackedQuestUpdated(this);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception during TriggerTrackedQuestUpdated:");
        }
    }

    private void triggerGroupIDChanged(CSteamID oldGroupID, CSteamID newGroupID)
    {
        this.groupIDChanged?.Invoke(this, oldGroupID, newGroupID);
    }

    private void triggerGroupRankChanged(EPlayerGroupRank oldGroupRank, EPlayerGroupRank newGroupRank)
    {
        this.groupRankChanged?.Invoke(this, oldGroupRank, newGroupRank);
    }

    private void triggerGroupInvitesChanged()
    {
        this.groupInvitesChanged?.Invoke(this);
    }

    private void triggerQuestCompleted(QuestAsset asset)
    {
        this.questCompleted?.Invoke(this, asset);
    }

    public QuestAsset GetTrackedQuest()
    {
        return _trackedQuest;
    }

    /// <summary>
    /// If true, hide viewmodel and prevent using equipped item. For example, to prevent shooting gun on top of a
    /// first-person scene. This could be expanded in the future with other flags and options.
    /// </summary>
    public bool IsCutsceneModeActive()
    {
        return npcCutsceneMode;
    }

    public void ServerSetCutsceneModeActive(bool active)
    {
        if (npcCutsceneMode != active)
        {
            npcCutsceneMode = active;
            if (base.channel.IsLocalPlayer)
            {
                base.player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
            }
            SendCutsceneMode.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), active);
        }
    }

    public bool isMemberOfGroup(CSteamID groupID)
    {
        if (isMemberOfAGroup)
        {
            return this.groupID == groupID;
        }
        return false;
    }

    public bool isMemberOfSameGroupAs(Player player)
    {
        return player.quests.isMemberOfGroup(groupID);
    }

    [Obsolete]
    public void tellSetMarker(CSteamID steamID, bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride)
    {
        ReceiveMarkerState(newIsMarkerPlaced, newMarkerPosition, newMarkerTextOverride);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveCutsceneMode(bool newCutsceneMode)
    {
        npcCutsceneMode = newCutsceneMode;
        if (base.channel.IsLocalPlayer)
        {
            base.player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSetMarker")]
    public void ReceiveMarkerState(bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride)
    {
        isMarkerPlaced = newIsMarkerPlaced;
        markerPosition = newMarkerPosition;
        markerTextOverride = newMarkerTextOverride;
    }

    [Obsolete]
    public void askSetMarker(CSteamID steamID, bool newIsMarkerPlaced, Vector3 newMarkerPosition)
    {
        ReceiveSetMarkerRequest(newIsMarkerPlaced, newMarkerPosition);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5, legacyName = "askSetMarker")]
    public void ReceiveSetMarkerRequest(bool newIsMarkerPlaced, Vector3 newMarkerPosition)
    {
        replicateSetMarker(newIsMarkerPlaced, newMarkerPosition, string.Empty);
    }

    /// <summary>
    /// Called serverside to set marker on clients.
    /// </summary>
    public void replicateSetMarker(bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride = "")
    {
        SendMarkerState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newIsMarkerPlaced, newMarkerPosition, newMarkerTextOverride);
    }

    /// <summary>
    /// Ask server to set marker.
    /// </summary>
    public void sendSetMarker(bool newIsMarkerPlaced, Vector3 newMarkerPosition)
    {
        SendSetMarkerRequest.Invoke(GetNetId(), ENetReliability.Reliable, newIsMarkerPlaced, newMarkerPosition);
    }

    [Obsolete]
    public void tellSetRadioFrequency(CSteamID steamID, uint newRadioFrequency)
    {
        ReceiveRadioFrequencyState(newRadioFrequency);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSetRadioFrequency")]
    public void ReceiveRadioFrequencyState(uint newRadioFrequency)
    {
        radioFrequency = newRadioFrequency;
    }

    [Obsolete]
    public void askSetRadioFrequency(CSteamID steamID, uint newRadioFrequency)
    {
        ReceiveSetRadioFrequencyRequest(newRadioFrequency);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5, legacyName = "askSetRadioFrequency")]
    public void ReceiveSetRadioFrequencyRequest(uint newRadioFrequency)
    {
        SendRadioFrequencyState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newRadioFrequency);
    }

    public void sendSetRadioFrequency(uint newRadioFrequency)
    {
        SendSetRadioFrequencyRequest.Invoke(GetNetId(), ENetReliability.Reliable, newRadioFrequency);
    }

    [Obsolete]
    public void tellSetGroup(CSteamID steamID, CSteamID newGroupID, byte newGroupRank)
    {
        ReceiveGroupState(newGroupID, (EPlayerGroupRank)newGroupRank);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSetGroup")]
    public void ReceiveGroupState(CSteamID newGroupID, EPlayerGroupRank newGroupRank)
    {
        CSteamID cSteamID = groupID;
        groupID = newGroupID;
        EPlayerGroupRank ePlayerGroupRank = groupRank;
        groupRank = newGroupRank;
        if (cSteamID != newGroupID)
        {
            triggerGroupIDChanged(cSteamID, newGroupID);
        }
        if (ePlayerGroupRank != groupRank)
        {
            triggerGroupRankChanged(ePlayerGroupRank, groupRank);
        }
        triggerGroupUpdated(this);
        broadcastGroupChanged(this, cSteamID, ePlayerGroupRank, newGroupID, groupRank);
    }

    private bool removeGroupInvite(CSteamID newGroupID)
    {
        if (groupInvites.Remove(newGroupID))
        {
            triggerGroupInvitesChanged();
            triggerGroupUpdated(this);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Call serverside to replicate new rank to clients
    /// </summary>
    public void changeRank(EPlayerGroupRank newRank)
    {
        SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), groupID, newRank);
    }

    [Obsolete]
    public void askJoinGroupInvite(CSteamID steamID, CSteamID newGroupID)
    {
        ReceiveAcceptGroupInvitationRequest(newGroupID);
    }

    /// <summary>
    /// Set player's group to their Steam group (if any) without testing restrictions.
    /// </summary>
    public void ServerAssignToMainGroup()
    {
        CSteamID group = base.channel.owner.playerID.group;
        inMainGroup = group != CSteamID.Nil;
        EPlayerGroupRank arg = EPlayerGroupRank.MEMBER;
        SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), group, arg);
    }

    public bool ServerAssignToGroup(CSteamID newGroupID, EPlayerGroupRank newRank, bool bypassMemberLimit)
    {
        GroupInfo groupInfo = GroupManager.getGroupInfo(newGroupID);
        if (groupInfo != null && (bypassMemberLimit || groupInfo.hasSpaceForMoreMembersInGroup))
        {
            SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newGroupID, newRank);
            inMainGroup = false;
            groupInfo.members++;
            GroupManager.sendGroupInfo(groupInfo);
            return true;
        }
        return false;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askJoinGroupInvite")]
    public void ReceiveAcceptGroupInvitationRequest(CSteamID newGroupID)
    {
        if (!canChangeGroupMembership)
        {
            return;
        }
        if (newGroupID == base.channel.owner.playerID.group)
        {
            if (Provider.modeConfigData.Gameplay.Allow_Static_Groups)
            {
                ServerAssignToMainGroup();
            }
        }
        else if (ServerRemoveGroupInvitation(newGroupID))
        {
            ServerAssignToGroup(newGroupID, EPlayerGroupRank.MEMBER, bypassMemberLimit: false);
        }
    }

    public void SendAcceptGroupInvitation(CSteamID newGroupID)
    {
        SendAcceptGroupInvitationRequest.Invoke(GetNetId(), ENetReliability.Reliable, newGroupID);
    }

    [Obsolete]
    public void askIgnoreGroupInvite(CSteamID steamID, CSteamID newGroupID)
    {
        ReceiveDeclineGroupInvitationRequest(newGroupID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askJoinGroupInvite")]
    public void ReceiveDeclineGroupInvitationRequest(CSteamID newGroupID)
    {
        ServerRemoveGroupInvitation(newGroupID);
    }

    public void SendDeclineGroupInvitation(CSteamID newGroupID)
    {
        SendDeclineGroupInvitationRequest.Invoke(GetNetId(), ENetReliability.Reliable, newGroupID);
    }

    /// <param name="force">Ignores group changing rules when true.</param>
    public void leaveGroup(bool force = false)
    {
        if (!force && (!canChangeGroupMembership || !hasPermissionToLeaveGroup))
        {
            return;
        }
        GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
        if (groupInfo != null)
        {
            if (groupInfo.members != 0)
            {
                groupInfo.members--;
            }
            GroupManager.sendGroupInfo(groupInfo);
        }
        SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), CSteamID.Nil, EPlayerGroupRank.MEMBER);
        inMainGroup = false;
    }

    [Obsolete]
    public void askLeaveGroup(CSteamID steamID)
    {
        ReceiveLeaveGroupRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askLeaveGroup")]
    public void ReceiveLeaveGroupRequest()
    {
        if (!(Time.realtimeSinceStartup - lastLeaveGroupRequestRealtime < 5f))
        {
            lastLeaveGroupRequestRealtime = Time.realtimeSinceStartup;
            GroupManager.requestGroupExit(base.player);
        }
    }

    public void sendLeaveGroup()
    {
        SendLeaveGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    public void deleteGroup()
    {
        if (canChangeGroupMembership && hasPermissionToDeleteGroup)
        {
            GroupManager.deleteGroup(groupID);
        }
    }

    [Obsolete]
    public void askDeleteGroup(CSteamID steamID)
    {
        ReceiveDeleteGroupRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askDeleteGroup")]
    public void ReceiveDeleteGroupRequest()
    {
        deleteGroup();
    }

    public void sendDeleteGroup()
    {
        SendDeleteGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    [Obsolete]
    public void askCreateGroup(CSteamID steamID)
    {
        ReceiveCreateGroupRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askCreateGroup")]
    public void ReceiveCreateGroupRequest()
    {
        if (canChangeGroupMembership && hasPermissionToCreateGroup)
        {
            CSteamID arg = GroupManager.generateUniqueGroupID();
            GroupInfo groupInfo = GroupManager.addGroup(arg, base.channel.owner.playerID.playerName + "'s Group");
            groupInfo.members++;
            GroupManager.sendGroupInfo(base.channel.GetOwnerTransportConnection(), groupInfo);
            SendGroupState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), arg, EPlayerGroupRank.OWNER);
            inMainGroup = false;
        }
    }

    public void sendCreateGroup()
    {
        SendCreateGroupRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    private void addGroupInvite(CSteamID newGroupID)
    {
        groupInvites.Add(newGroupID);
        triggerGroupInvitesChanged();
        triggerGroupUpdated(this);
    }

    [Obsolete]
    public void tellAddGroupInvite(CSteamID steamID, CSteamID newGroupID)
    {
        ReceiveAddGroupInviteClient(newGroupID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellAddGroupInvite")]
    public void ReceiveAddGroupInviteClient(CSteamID newGroupID)
    {
        addGroupInvite(newGroupID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveRemoveGroupInviteClient(CSteamID newGroupID)
    {
        removeGroupInvite(newGroupID);
    }

    public bool ServerRemoveGroupInvitation(CSteamID groupId)
    {
        if (!removeGroupInvite(groupId))
        {
            return false;
        }
        if (!base.channel.IsLocalPlayer)
        {
            SendRemoveGroupInviteClient.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), groupId);
        }
        return true;
    }

    /// <summary>
    /// Serverside send packet telling player about this invite
    /// </summary>
    public void sendAddGroupInvite(CSteamID newGroupID)
    {
        if (!groupInvites.Contains(newGroupID))
        {
            addGroupInvite(newGroupID);
            GroupInfo groupInfo = GroupManager.getGroupInfo(newGroupID);
            if (groupInfo != null)
            {
                GroupManager.sendGroupInfo(base.channel.GetOwnerTransportConnection(), groupInfo);
            }
            if (!base.channel.IsLocalPlayer)
            {
                SendAddGroupInviteClient.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), newGroupID);
            }
        }
    }

    [Obsolete]
    public void askAddGroupInvite(CSteamID steamID, CSteamID targetID)
    {
        ReceiveAddGroupInviteRequest(targetID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askAddGroupInvite")]
    public void ReceiveAddGroupInviteRequest(CSteamID targetID)
    {
        if (isMemberOfAGroup && hasPermissionToInviteMembers && hasSpaceForMoreMembersInGroup)
        {
            Player player = PlayerTool.getPlayer(targetID);
            if (!(player == null) && !player.quests.isMemberOfAGroup)
            {
                player.quests.sendAddGroupInvite(groupID);
            }
        }
    }

    public void sendAskAddGroupInvite(CSteamID targetID)
    {
        SendAddGroupInviteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
    }

    [Obsolete]
    public void askPromote(CSteamID steamID, CSteamID targetID)
    {
        ReceivePromoteRequest(targetID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askPromote")]
    public void ReceivePromoteRequest(CSteamID targetID)
    {
        if (!isMemberOfAGroup || !hasPermissionToChangeRank)
        {
            return;
        }
        Player player = PlayerTool.getPlayer(targetID);
        if (player == null || !player.quests.isMemberOfSameGroupAs(base.player))
        {
            return;
        }
        if (player.quests.groupRank == EPlayerGroupRank.OWNER)
        {
            CommandWindow.LogWarning("Request to promote owner of group?");
            return;
        }
        player.quests.changeRank(player.quests.groupRank + 1);
        if (player.quests.groupRank == EPlayerGroupRank.OWNER)
        {
            changeRank(EPlayerGroupRank.ADMIN);
        }
    }

    public void sendPromote(CSteamID targetID)
    {
        SendPromoteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
    }

    [Obsolete]
    public void askDemote(CSteamID steamID, CSteamID targetID)
    {
        ReceiveDemoteRequest(targetID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askDemote")]
    public void ReceiveDemoteRequest(CSteamID targetID)
    {
        if (!isMemberOfAGroup || !hasPermissionToChangeRank)
        {
            return;
        }
        Player player = PlayerTool.getPlayer(targetID);
        if (!(player == null) && player.quests.isMemberOfSameGroupAs(base.player))
        {
            if (player.quests.groupRank != EPlayerGroupRank.ADMIN)
            {
                CommandWindow.LogWarning("Request to demote non-admin member of group?");
            }
            else
            {
                player.quests.changeRank(player.quests.groupRank - 1);
            }
        }
    }

    public void sendDemote(CSteamID targetID)
    {
        SendDemoteRequest.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
    }

    [Obsolete]
    public void askKickFromGroup(CSteamID steamID, CSteamID targetID)
    {
        ReceiveKickFromGroup(targetID);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askKickFromGroup")]
    public void ReceiveKickFromGroup(CSteamID targetID)
    {
        if (isMemberOfAGroup && hasPermissionToKickMembers)
        {
            Player player = PlayerTool.getPlayer(targetID);
            if (!(player == null) && player.quests.isMemberOfSameGroupAs(base.player) && player.quests.canBeKickedFromGroup)
            {
                player.quests.leaveGroup();
            }
        }
    }

    public void sendKickFromGroup(CSteamID targetID)
    {
        SendKickFromGroup.Invoke(GetNetId(), ENetReliability.Unreliable, targetID);
    }

    [Obsolete]
    public void askRenameGroup(CSteamID steamID, string newName)
    {
        ReceiveRenameGroupRequest(newName);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askRenameGroup")]
    public void ReceiveRenameGroupRequest(string newName)
    {
        if (newName.Length > 32)
        {
            newName = newName.Substring(0, 32);
        }
        if (isMemberOfAGroup && hasPermissionToChangeName)
        {
            GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
            groupInfo.name = newName;
            GroupManager.sendGroupInfo(groupInfo);
        }
    }

    public void sendRenameGroup(string newName)
    {
        SendRenameGroupRequest.Invoke(GetNetId(), ENetReliability.Reliable, newName);
    }

    public void setFlag(ushort id, short value)
    {
        if (flagsMap.TryGetValue(id, out var value2))
        {
            value2.value = value;
        }
        else
        {
            value2 = new PlayerQuestFlag(id, value);
            flagsMap.Add(id, value2);
            int num = flagsList.BinarySearch(value2, flagComparator);
            num = ~num;
            flagsList.Insert(num, value2);
        }
        if (base.channel.IsLocalPlayer)
        {
            if (id == 29)
            {
                if (value >= 1 && Provider.provider.achievementsService.getAchievement("Ensign", out var has) && !has)
                {
                    Provider.provider.achievementsService.setAchievement("Ensign");
                }
                if (value >= 2 && Provider.provider.achievementsService.getAchievement("Lieutenant", out var has2) && !has2)
                {
                    Provider.provider.achievementsService.setAchievement("Lieutenant");
                }
                if (value >= 3 && Provider.provider.achievementsService.getAchievement("Major", out var has3) && !has3)
                {
                    Provider.provider.achievementsService.setAchievement("Major");
                }
            }
            onFlagUpdated?.Invoke(id);
            TriggerTrackedQuestUpdated();
        }
        if (Provider.isServer && PlayerQuests.onAnyFlagChanged != null)
        {
            PlayerQuests.onAnyFlagChanged(this, value2);
        }
    }

    public bool getFlag(ushort id, out short value)
    {
        if (flagsMap.TryGetValue(id, out var value2))
        {
            value = value2.value;
            return true;
        }
        value = 0;
        return false;
    }

    public void removeFlag(ushort id)
    {
        if (!flagsMap.TryGetValue(id, out var value))
        {
            return;
        }
        int num = flagsList.BinarySearch(value, flagComparator);
        if (num >= 0)
        {
            flagsMap.Remove(id);
            flagsList.RemoveAt(num);
            if (base.channel.IsLocalPlayer)
            {
                onFlagUpdated?.Invoke(id);
                TriggerTrackedQuestUpdated();
            }
        }
    }

    public int countValidQuests()
    {
        int num = 0;
        foreach (PlayerQuest quests in questsList)
        {
            if (quests != null && quests.asset != null)
            {
                num++;
            }
        }
        return num;
    }

    public void AddQuest(QuestAsset questAsset)
    {
        if (questAsset != null)
        {
            if (FindIndexOfQuest(questAsset) < 0)
            {
                PlayerQuest item = new PlayerQuest(questAsset);
                questsList.Add(item);
            }
            TrackQuest(questAsset);
            if (base.channel.IsLocalPlayer && this.OnLocalPlayerQuestsChanged != null)
            {
                this.OnLocalPlayerQuestsChanged(questAsset.id);
            }
        }
    }

    [Obsolete]
    public void addQuest(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            AddQuest(questAsset);
        }
    }

    [Obsolete]
    public bool getQuest(ushort id, out PlayerQuest quest)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset asset)
        {
            int num = FindIndexOfQuest(asset);
            if (num >= 0)
            {
                quest = questsList[num];
                return true;
            }
            quest = null;
            return false;
        }
        quest = null;
        return false;
    }

    public ENPCQuestStatus GetQuestStatus(QuestAsset questAsset)
    {
        if (questAsset == null)
        {
            return ENPCQuestStatus.NONE;
        }
        if (FindIndexOfQuest(questAsset) >= 0)
        {
            if (questAsset.areConditionsMet(base.player))
            {
                return ENPCQuestStatus.READY;
            }
            return ENPCQuestStatus.ACTIVE;
        }
        if (getFlag(questAsset.id, out var _))
        {
            return ENPCQuestStatus.COMPLETED;
        }
        return ENPCQuestStatus.NONE;
    }

    [Obsolete]
    public ENPCQuestStatus getQuestStatus(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            return GetQuestStatus(questAsset);
        }
        return ENPCQuestStatus.NONE;
    }

    public void RemoveQuest(QuestAsset questAsset, bool wasCompleted = false)
    {
        int num = FindIndexOfQuest(questAsset);
        if (num >= 0)
        {
            questsList.RemoveAt(num);
        }
        if (_trackedQuest != null && _trackedQuest == questAsset)
        {
            if (questsList.Count > 0)
            {
                TrackQuest(questsList[0].asset);
            }
            else
            {
                TrackQuest(null);
            }
        }
        if (base.channel.IsLocalPlayer && questAsset != null)
        {
            if (wasCompleted && Provider.provider.achievementsService.getAchievement("Quest", out var has) && !has)
            {
                Provider.provider.achievementsService.setAchievement("Quest");
            }
            this.OnLocalPlayerQuestsChanged?.Invoke(questAsset.id);
        }
        if (questAsset != null && wasCompleted)
        {
            triggerQuestCompleted(questAsset);
        }
    }

    [Obsolete]
    public void removeQuest(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            RemoveQuest(questAsset);
        }
    }

    public void trackHordeKill()
    {
        for (int i = 0; i < questsList.Count; i++)
        {
            PlayerQuest playerQuest = questsList[i];
            if (playerQuest == null || playerQuest.asset == null || playerQuest.asset.conditions == null)
            {
                continue;
            }
            for (int j = 0; j < playerQuest.asset.conditions.Length; j++)
            {
                if (playerQuest.asset.conditions[j] is NPCHordeKillsCondition nPCHordeKillsCondition && nPCHordeKillsCondition.nav == base.player.movement.nav)
                {
                    getFlag(nPCHordeKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCHordeKillsCondition.id, value);
                }
            }
        }
    }

    public void trackZombieKill(Zombie zombie)
    {
        if (zombie == null)
        {
            return;
        }
        float sqrMagnitude = (base.transform.position - zombie.transform.position).sqrMagnitude;
        for (int i = 0; i < questsList.Count; i++)
        {
            PlayerQuest playerQuest = questsList[i];
            if (playerQuest == null || playerQuest.asset == null || playerQuest.asset.conditions == null)
            {
                continue;
            }
            for (int j = 0; j < playerQuest.asset.conditions.Length; j++)
            {
                if (playerQuest.asset.conditions[j] is NPCZombieKillsCondition nPCZombieKillsCondition && (nPCZombieKillsCondition.zombie == EZombieSpeciality.NONE || nPCZombieKillsCondition.zombie == zombie.speciality) && (nPCZombieKillsCondition.nav == byte.MaxValue || nPCZombieKillsCondition.nav == base.player.movement.bound) && (!(nPCZombieKillsCondition.sqrRadius > 0.01f) || !(sqrMagnitude > nPCZombieKillsCondition.sqrRadius)) && (!(nPCZombieKillsCondition.sqrMinRadius > 0.01f) || !(sqrMagnitude < nPCZombieKillsCondition.sqrMinRadius)))
                {
                    getFlag(nPCZombieKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCZombieKillsCondition.id, value);
                }
            }
        }
    }

    public void trackObjectKill(Guid objectGuid, byte nav)
    {
        foreach (PlayerQuest quests in questsList)
        {
            if (quests == null || quests.asset == null || quests.asset.conditions == null)
            {
                continue;
            }
            INPCCondition[] conditions = quests.asset.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i] is NPCObjectKillsCondition nPCObjectKillsCondition && (nPCObjectKillsCondition.nav == byte.MaxValue || nPCObjectKillsCondition.nav == nav) && nPCObjectKillsCondition.objectGuid.Equals(objectGuid))
                {
                    getFlag(nPCObjectKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCObjectKillsCondition.id, value);
                }
            }
        }
    }

    public void trackTreeKill(Guid treeGuid)
    {
        foreach (PlayerQuest quests in questsList)
        {
            if (quests == null || quests.asset == null || quests.asset.conditions == null)
            {
                continue;
            }
            INPCCondition[] conditions = quests.asset.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i] is NPCTreeKillsCondition { treeGuid: var treeGuid2 } nPCTreeKillsCondition && treeGuid2.Equals(treeGuid))
                {
                    getFlag(nPCTreeKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCTreeKillsCondition.id, value);
                }
            }
        }
    }

    public void trackAnimalKill(Animal animal)
    {
        if (animal == null)
        {
            return;
        }
        for (int i = 0; i < questsList.Count; i++)
        {
            PlayerQuest playerQuest = questsList[i];
            if (playerQuest == null || playerQuest.asset == null || playerQuest.asset.conditions == null)
            {
                continue;
            }
            for (int j = 0; j < playerQuest.asset.conditions.Length; j++)
            {
                if (playerQuest.asset.conditions[j] is NPCAnimalKillsCondition nPCAnimalKillsCondition && nPCAnimalKillsCondition.animal == animal.id)
                {
                    getFlag(nPCAnimalKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCAnimalKillsCondition.id, value);
                }
            }
        }
    }

    public void trackPlayerKill(Player enemyPlayer)
    {
        if (enemyPlayer == null)
        {
            return;
        }
        for (int i = 0; i < questsList.Count; i++)
        {
            PlayerQuest playerQuest = questsList[i];
            if (playerQuest == null || playerQuest.asset == null || playerQuest.asset.conditions == null)
            {
                continue;
            }
            for (int j = 0; j < playerQuest.asset.conditions.Length; j++)
            {
                if (playerQuest.asset.conditions[j] is NPCPlayerKillsCondition nPCPlayerKillsCondition)
                {
                    getFlag(nPCPlayerKillsCondition.id, out var value);
                    value++;
                    sendSetFlag(nPCPlayerKillsCondition.id, value);
                }
            }
        }
    }

    /// <summary>
    /// Called on server to finalize and remove quest.
    /// </summary>
    public void CompleteQuest(QuestAsset questAsset, bool ignoreNPC = false)
    {
        if (questAsset != null && (ignoreNPC || (!(checkNPC == null) && !((checkNPC.transform.position - base.transform.position).sqrMagnitude > 400f))) && GetQuestStatus(questAsset) == ENPCQuestStatus.READY)
        {
            ServerRemoveQuest(questAsset, wasCompleted: true);
            sendSetFlag(questAsset.id, 1);
            questAsset.ApplyConditions(base.player);
            questAsset.GrantRewards(base.player);
        }
    }

    [Obsolete]
    public void completeQuest(ushort id, bool ignoreNPC = false)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            CompleteQuest(questAsset);
        }
    }

    [Obsolete]
    public void askSellToVendor(CSteamID steamID, ushort id, byte index)
    {
        throw new NotSupportedException();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askSellToVendor")]
    public void ReceiveSellToVendor(in ServerInvocationContext context, Guid assetGuid, byte index, bool asManyAsPossible)
    {
        if (checkNPC == null || (checkNPC.transform.position - base.transform.position).sqrMagnitude > 400f || serverCurrentVendorAsset == null)
        {
            return;
        }
        VendorAsset vendorAsset = Assets.find<VendorAsset>(assetGuid);
        if (vendorAsset == null || vendorAsset != serverCurrentVendorAsset || vendorAsset.buying == null || index >= vendorAsset.buying.Length)
        {
            return;
        }
        VendorBuying vendorBuying = vendorAsset.buying[index];
        if (vendorBuying == null)
        {
            return;
        }
        int num = 0;
        while (vendorBuying.canSell(base.player) && vendorBuying.areConditionsMet(base.player))
        {
            vendorBuying.ApplyConditions(base.player);
            vendorBuying.GrantRewards(base.player);
            vendorBuying.sell(base.player);
            num++;
            if (!asManyAsPossible || num >= 10)
            {
                break;
            }
        }
    }

    public void sendSellToVendor(Guid assetGuid, byte index, bool asManyAsPossible)
    {
        SendSellToVendor.Invoke(GetNetId(), ENetReliability.Unreliable, assetGuid, index, asManyAsPossible);
    }

    [Obsolete]
    public void sendSellToVendor(ushort id, byte index)
    {
        throw new NotSupportedException();
    }

    [Obsolete]
    public void askBuyFromVendor(CSteamID steamID, ushort id, byte index)
    {
        throw new NotSupportedException();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askBuyFromVendor")]
    public void ReceiveBuyFromVendor(in ServerInvocationContext context, Guid assetGuid, byte index, bool asManyAsPossible)
    {
        if (checkNPC == null || (checkNPC.transform.position - base.transform.position).sqrMagnitude > 400f || serverCurrentVendorAsset == null)
        {
            return;
        }
        VendorAsset vendorAsset = Assets.find<VendorAsset>(assetGuid);
        if (vendorAsset == null || vendorAsset != serverCurrentVendorAsset || vendorAsset.selling == null || index >= vendorAsset.selling.Length)
        {
            return;
        }
        VendorSellingBase vendorSellingBase = vendorAsset.selling[index];
        if (vendorSellingBase == null)
        {
            return;
        }
        if (vendorSellingBase is VendorSellingVehicle)
        {
            asManyAsPossible = false;
            if (Time.realtimeSinceStartup - lastVehiclePurchaseRealtime < 5f)
            {
                lastVehiclePurchaseRealtime = Time.realtimeSinceStartup;
                return;
            }
        }
        int num = 0;
        while (vendorSellingBase.canBuy(base.player) && vendorSellingBase.areConditionsMet(base.player))
        {
            vendorSellingBase.ApplyConditions(base.player);
            vendorSellingBase.GrantRewards(base.player);
            vendorSellingBase.buy(base.player);
            num++;
            if (!asManyAsPossible || num >= 10)
            {
                break;
            }
        }
    }

    public void sendBuyFromVendor(Guid assetGuid, byte index, bool asManyAsPossible)
    {
        SendBuyFromVendor.Invoke(GetNetId(), ENetReliability.Unreliable, assetGuid, index, asManyAsPossible);
    }

    [Obsolete]
    public void sendBuyFromVendor(ushort id, byte index)
    {
        throw new NotSupportedException();
    }

    [Obsolete]
    public void tellSetFlag(CSteamID steamID, ushort id, short value)
    {
        ReceiveSetFlag(id, value);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSetFlag")]
    public void ReceiveSetFlag(ushort id, short value)
    {
        setFlag(id, value);
    }

    public void sendSetFlag(ushort id, short value)
    {
        setFlag(id, value);
        if (!base.channel.IsLocalPlayer)
        {
            SendSetFlag.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), id, value);
        }
    }

    [Obsolete]
    public void tellRemoveFlag(CSteamID steamID, ushort id)
    {
        ReceiveRemoveFlag(id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellRemoveFlag")]
    public void ReceiveRemoveFlag(ushort id)
    {
        removeFlag(id);
    }

    public void sendRemoveFlag(ushort id)
    {
        removeFlag(id);
        if (!base.channel.IsLocalPlayer)
        {
            SendRemoveFlag.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), id);
        }
    }

    [Obsolete]
    public void tellAddQuest(CSteamID steamID, ushort id)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveAddQuest(Guid assetGuid)
    {
        QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
        if (questAsset != null)
        {
            AddQuest(questAsset);
        }
    }

    public void ServerAddQuest(QuestAsset questAsset)
    {
        if (questAsset != null)
        {
            AddQuest(questAsset);
            if (!base.channel.IsLocalPlayer)
            {
                SendAddQuest.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), questAsset.GUID);
            }
        }
    }

    [Obsolete]
    public void sendAddQuest(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            ServerAddQuest(questAsset);
        }
    }

    [Obsolete]
    public void tellRemoveQuest(CSteamID steamID, ushort id)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveRemoveQuest(Guid assetGuid, bool wasCompleted)
    {
        QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
        if (questAsset != null)
        {
            RemoveQuest(questAsset, wasCompleted);
        }
    }

    public void ServerRemoveQuest(QuestAsset questAsset)
    {
        ServerRemoveQuest(questAsset, wasCompleted: false);
    }

    public void ServerRemoveQuest(QuestAsset questAsset, bool wasCompleted = false)
    {
        if (questAsset != null)
        {
            RemoveQuest(questAsset, wasCompleted);
            if (!base.channel.IsLocalPlayer)
            {
                SendRemoveQuest.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), questAsset.GUID, wasCompleted);
            }
            if (!wasCompleted)
            {
                questAsset.GrantAbandonmentRewards(base.player);
            }
        }
    }

    [Obsolete]
    public void sendRemoveQuest(ushort id)
    {
        QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
        ServerRemoveQuest(questAsset);
    }

    public void TrackQuest(QuestAsset questAsset)
    {
        if (_trackedQuest != null && _trackedQuest == questAsset)
        {
            _trackedQuest = null;
        }
        else
        {
            _trackedQuest = questAsset;
        }
        if (base.channel.IsLocalPlayer)
        {
            TriggerTrackedQuestUpdated();
        }
    }

    [Obsolete]
    public void trackQuest(ushort id)
    {
        QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
        TrackQuest(questAsset);
    }

    [Obsolete]
    public void askTrackQuest(CSteamID steamID, ushort id)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5)]
    public void ReceiveTrackQuest(Guid assetGuid)
    {
        QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
        TrackQuest(questAsset);
    }

    public void ClientTrackQuest(QuestAsset questAsset)
    {
        SendTrackQuest.Invoke(GetNetId(), ENetReliability.Reliable, questAsset?.GUID ?? Guid.Empty);
    }

    [Obsolete]
    public void sendTrackQuest(ushort id)
    {
        QuestAsset questAsset = Assets.find(EAssetType.NPC, id) as QuestAsset;
        ClientTrackQuest(questAsset);
    }

    [Obsolete("Identical to ServerRemoveQuest")]
    public void AbandonQuest(QuestAsset questAsset)
    {
        ServerRemoveQuest(questAsset);
    }

    [Obsolete]
    public void abandonQuest(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            AbandonQuest(questAsset);
        }
    }

    [Obsolete]
    public void askAbandonQuest(CSteamID steamID, ushort id)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5)]
    public void ReceiveAbandonQuestRequest(Guid assetGuid)
    {
        QuestAsset questAsset = Assets.find<QuestAsset>(assetGuid);
        if (questAsset != null)
        {
            ServerRemoveQuest(questAsset);
        }
    }

    /// <summary>
    /// Called by quest details UI to request server to abandon quest.
    /// </summary>
    public void ClientAbandonQuest(QuestAsset questAsset)
    {
        if (questAsset != null)
        {
            SendAbandonQuestRequest.Invoke(GetNetId(), ENetReliability.Reliable, questAsset.GUID);
        }
    }

    [Obsolete]
    public void sendAbandonQuest(ushort id)
    {
        if (Assets.find(EAssetType.NPC, id) is QuestAsset questAsset)
        {
            ClientAbandonQuest(questAsset);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 20)]
    public void ReceiveChooseDialogueResponseRequest(in ServerInvocationContext context, Guid assetGuid, byte index)
    {
        if (checkNPC == null || (checkNPC.transform.position - base.transform.position).sqrMagnitude > 400f || serverCurrentDialogueAsset == null || serverCurrentDialogueMessage == null)
        {
            return;
        }
        DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(assetGuid);
        if (dialogueAsset == null || dialogueAsset != serverCurrentDialogueAsset || dialogueAsset.responses == null || index >= dialogueAsset.responses.Length)
        {
            return;
        }
        if (serverCurrentDialogueMessage.responses != null && serverCurrentDialogueMessage.responses.Length != 0)
        {
            bool flag = false;
            for (int i = 0; i < serverCurrentDialogueMessage.responses.Length; i++)
            {
                if (index == serverCurrentDialogueMessage.responses[i])
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return;
            }
        }
        DialogueResponse dialogueResponse = dialogueAsset.responses[index];
        if (dialogueResponse == null || dialogueResponse.conditions == null || !dialogueResponse.areConditionsMet(base.player))
        {
            return;
        }
        if (dialogueResponse.messages != null && dialogueResponse.messages.Length != 0)
        {
            bool flag2 = false;
            for (int j = 0; j < dialogueResponse.messages.Length; j++)
            {
                if (serverCurrentDialogueMessage.index == dialogueResponse.messages[j])
                {
                    flag2 = true;
                    break;
                }
            }
            if (!flag2)
            {
                return;
            }
        }
        dialogueResponse.ApplyConditions(base.player);
        dialogueResponse.GrantRewards(base.player);
        VendorAsset vendorAsset = dialogueResponse.FindVendorAsset();
        DialogueAsset dialogueAsset2 = dialogueResponse.FindDialogueAsset();
        DialogueMessage dialogueMessage = dialogueAsset2?.GetAvailableMessage(base.player);
        if (vendorAsset != null)
        {
            if (dialogueAsset2 == null || dialogueMessage == null)
            {
                dialogueAsset2 = serverCurrentDialogueAsset;
                dialogueMessage = serverCurrentDialogueMessage;
            }
            serverDefaultNextDialogueAsset = dialogueMessage.FindPrevDialogueAsset() ?? serverCurrentDialogueAsset;
            serverCurrentDialogueAsset = dialogueAsset2;
            serverCurrentDialogueMessage = dialogueMessage;
            serverCurrentVendorAsset = vendorAsset;
            SendOpenVendor.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), vendorAsset.GUID, dialogueAsset2.GUID, dialogueMessage.index, serverDefaultNextDialogueAsset != null);
        }
        else if (dialogueAsset2 != null && dialogueMessage != null)
        {
            serverDefaultNextDialogueAsset = dialogueMessage?.FindPrevDialogueAsset() ?? serverCurrentDialogueAsset;
            serverCurrentDialogueAsset = dialogueAsset2;
            serverCurrentDialogueMessage = dialogueMessage;
            serverCurrentVendorAsset = null;
            SendOpenDialogue.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), dialogueAsset2.GUID, dialogueMessage.index, serverDefaultNextDialogueAsset != null);
        }
        if (dialogueMessage != null)
        {
            dialogueMessage.ApplyConditions(base.player);
            dialogueMessage.GrantRewards(base.player);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 20)]
    public void ReceiveChooseDefaultNextDialogueRequest(in ServerInvocationContext context, Guid assetGuid, byte index)
    {
        if (checkNPC == null || (checkNPC.transform.position - base.transform.position).sqrMagnitude > 400f || serverDefaultNextDialogueAsset == null || serverCurrentDialogueAsset == null || serverCurrentDialogueMessage == null)
        {
            return;
        }
        DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(assetGuid);
        if (dialogueAsset != null && dialogueAsset == serverCurrentDialogueAsset && index == serverCurrentDialogueMessage.index)
        {
            DialogueAsset dialogueAsset2 = serverDefaultNextDialogueAsset;
            DialogueMessage dialogueMessage = dialogueAsset2?.GetAvailableMessage(base.player);
            serverDefaultNextDialogueAsset = null;
            if (dialogueMessage != null)
            {
                serverDefaultNextDialogueAsset = dialogueMessage.FindPrevDialogueAsset();
                serverCurrentDialogueAsset = dialogueAsset2;
                serverCurrentDialogueMessage = dialogueMessage;
                serverCurrentVendorAsset = null;
                SendOpenDialogue.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), dialogueAsset2.GUID, dialogueMessage.index, serverDefaultNextDialogueAsset != null);
                dialogueMessage.ApplyConditions(base.player);
                dialogueMessage.GrantRewards(base.player);
            }
        }
    }

    public void ClientChooseDialogueResponse(Guid assetGuid, byte index)
    {
        SendChooseDialogueResponseRequest.Invoke(GetNetId(), ENetReliability.Reliable, assetGuid, index);
    }

    /// <summary>
    /// Called when there are no responses to choose, but server has indicated a next dialogue is available.
    /// </summary>
    public void ClientChooseNextDialogue(Guid assetGuid, byte index)
    {
        SendChooseDefaultNextDialogueRequest.Invoke(GetNetId(), ENetReliability.Reliable, assetGuid, index);
    }

    [Obsolete]
    public void tellQuests(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveQuests(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadBit(out _isMarkerPlaced);
        reader.ReadClampedVector3(out _markerPosition);
        reader.ReadString(out _markerTextOverride);
        reader.ReadUInt32(out _radioFrequency);
        reader.ReadSteamID(out _groupID);
        reader.ReadEnum(out _groupRank);
        if (!base.channel.IsLocalPlayer)
        {
            return;
        }
        reader.ReadUInt16(out var value);
        for (ushort num = 0; num < value; num++)
        {
            reader.ReadUInt16(out var value2);
            reader.ReadInt16(out var value3);
            PlayerQuestFlag playerQuestFlag = new PlayerQuestFlag(value2, value3);
            flagsMap.Add(value2, playerQuestFlag);
            flagsList.Add(playerQuestFlag);
        }
        reader.ReadInt32(out var value4);
        for (int i = 0; i < value4; i++)
        {
            reader.ReadGuid(out var value5);
            QuestAsset questAsset = Assets.find<QuestAsset>(value5);
            if (questAsset != null)
            {
                PlayerQuest item = new PlayerQuest(questAsset);
                questsList.Add(item);
            }
        }
        reader.ReadGuid(out var value6);
        _trackedQuest = Assets.find<QuestAsset>(value6);
        reader.ReadBit(out npcCutsceneMode);
        base.player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
        onFlagsUpdated?.Invoke();
        TriggerTrackedQuestUpdated();
    }

    [Obsolete]
    public void askQuests(CSteamID steamID)
    {
    }

    private void WriteAllState(NetPakWriter writer)
    {
        writer.WriteBit(isMarkerPlaced);
        writer.WriteClampedVector3(markerPosition);
        writer.WriteString(markerTextOverride);
        writer.WriteUInt32(radioFrequency);
        writer.WriteSteamID(groupID);
        writer.WriteEnum(groupRank);
    }

    private void WriteOwnerState(NetPakWriter writer)
    {
        writer.WriteUInt16((ushort)flagsList.Count);
        for (ushort num = 0; num < flagsList.Count; num++)
        {
            PlayerQuestFlag playerQuestFlag = flagsList[num];
            writer.WriteUInt16(playerQuestFlag.id);
            writer.WriteInt16(playerQuestFlag.value);
        }
        writer.WriteInt32(questsList.Count);
        foreach (PlayerQuest quests in questsList)
        {
            writer.WriteGuid(quests?.asset?.GUID ?? Guid.Empty);
        }
        writer.WriteGuid(_trackedQuest?.GUID ?? Guid.Empty);
        writer.WriteBit(npcCutsceneMode);
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        bool sendingToOwner = base.channel.owner == client;
        if (base.channel.IsLocalPlayer && sendingToOwner)
        {
            return;
        }
        if (isMemberOfAGroup)
        {
            GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
            if (groupInfo != null)
            {
                GroupManager.sendGroupInfo(client.transportConnection, groupInfo);
            }
        }
        SendQuests.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, delegate(NetPakWriter writer)
        {
            WriteAllState(writer);
            if (sendingToOwner)
            {
                WriteOwnerState(writer);
            }
        });
    }

    internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
    {
        if (isMemberOfAGroup)
        {
            GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
            if (groupInfo != null)
            {
                GroupManager.sendGroupInfo(transportConnections, groupInfo);
            }
        }
        SendQuests.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, delegate(NetPakWriter writer)
        {
            WriteAllState(writer);
        });
    }

    private void OnPlayerNavChanged(PlayerMovement sender, byte oldNav, byte newNav)
    {
        if (newNav != byte.MaxValue)
        {
            ZombieManager.regions[newNav].UpdateBoss();
        }
    }

    private void onExperienceUpdated(uint experience)
    {
        TriggerTrackedQuestUpdated();
    }

    private void onReputationUpdated(int reputation)
    {
        TriggerTrackedQuestUpdated();
    }

    private void onInventoryStateUpdated()
    {
        TriggerTrackedQuestUpdated();
    }

    private void onTimeOfDayChanged()
    {
        onExternalConditionsUpdated?.Invoke();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveTalkWithNpcResponse(in ClientInvocationContext context, NetId targetNpcNetId, Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
    {
        InteractableObjectNPC npcFromObjectNetId = InteractableObjectNPC.GetNpcFromObjectNetId(targetNpcNetId);
        if (!(npcFromObjectNetId == null))
        {
            DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
            ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, dialogueAsset, "talk with NPC response");
            if (dialogueAsset != null && messageIndex < dialogueAsset.messages.Length)
            {
                checkNPC = npcFromObjectNetId;
                PlayerLifeUI.close();
                PlayerLifeUI.npc = npcFromObjectNetId;
                npcFromObjectNetId.isLookingAtPlayer = true;
                PlayerNPCDialogueUI.open(dialogueAsset, dialogueAsset.messages[messageIndex], hasNextDialogue);
            }
        }
    }

    /// <summary>
    /// Called in singleplayer and on the server after client requests NPC dialogue.
    /// </summary>
    internal void ApproveTalkWithNpcRequest(InteractableObjectNPC targetNpc, DialogueAsset rootDialogueAsset)
    {
        DialogueMessage availableMessage = rootDialogueAsset.GetAvailableMessage(base.player);
        if (availableMessage == null)
        {
            UnturnedLog.warn("Unable to approve talk with NPC (" + targetNpc.npcAsset.FriendlyName + ") request because there is no valid message");
            return;
        }
        checkNPC = targetNpc;
        serverCurrentDialogueAsset = rootDialogueAsset;
        serverCurrentDialogueMessage = availableMessage;
        serverCurrentVendorAsset = null;
        serverDefaultNextDialogueAsset = availableMessage.FindPrevDialogueAsset();
        SendTalkWithNpcResponse.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), targetNpc.GetNpcNetId(), rootDialogueAsset.GUID, serverCurrentDialogueMessage.index, serverDefaultNextDialogueAsset != null);
        serverCurrentDialogueMessage.ApplyConditions(base.player);
        serverCurrentDialogueMessage.GrantRewards(base.player);
    }

    internal void ClearActiveNpc()
    {
        checkNPC = null;
        serverCurrentDialogueAsset = null;
        serverCurrentDialogueMessage = null;
        serverCurrentVendorAsset = null;
        serverDefaultNextDialogueAsset = null;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveOpenDialogue(in ClientInvocationContext context, Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
    {
        DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
        ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, dialogueAsset, "open dialogue");
        if (dialogueAsset != null && dialogueAsset.messages != null)
        {
            _ = dialogueAsset.messages.Length;
            if (PlayerNPCVendorUI.active)
            {
                PlayerNPCVendorUI.close();
            }
            if (PlayerNPCQuestUI.active)
            {
                PlayerNPCQuestUI.close();
            }
            DialogueMessage newMessage = dialogueAsset.messages[messageIndex];
            PlayerNPCDialogueUI.open(dialogueAsset, newMessage, hasNextDialogue);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveOpenVendor(in ClientInvocationContext context, Guid vendorAssetGuid, Guid dialogueAssetGuid, byte messageIndex, bool hasNextDialogue)
    {
        VendorAsset vendorAsset = Assets.find<VendorAsset>(vendorAssetGuid);
        DialogueAsset dialogueAsset = Assets.find<DialogueAsset>(dialogueAssetGuid);
        ClientAssetIntegrity.QueueRequest(vendorAssetGuid, vendorAsset, "open vendor");
        ClientAssetIntegrity.QueueRequest(dialogueAssetGuid, dialogueAsset, "open vendor");
        if (vendorAsset != null && dialogueAsset != null && dialogueAsset.messages != null)
        {
            _ = dialogueAsset.messages.Length;
            if (PlayerNPCDialogueUI.active)
            {
                PlayerNPCDialogueUI.close();
            }
            if (PlayerNPCQuestUI.active)
            {
                PlayerNPCQuestUI.close();
            }
            DialogueMessage newNextMessage = dialogueAsset.messages[messageIndex];
            PlayerNPCVendorUI.open(vendorAsset, dialogueAsset, newNextMessage, hasNextDialogue);
        }
    }

    internal PlayerDelayedQuestRewardsComponent GetOrCreateDelayedQuestRewards()
    {
        if (!hasCreatedDelayedRewards && delayedRewardsComponent == null)
        {
            hasCreatedDelayedRewards = true;
            delayedRewardsGameObject = new GameObject();
            delayedRewardsComponent = delayedRewardsGameObject.AddComponent<PlayerDelayedQuestRewardsComponent>();
            delayedRewardsComponent.player = base.player;
        }
        return delayedRewardsComponent;
    }

    internal void StopDelayedQuestRewards()
    {
        if (delayedRewardsComponent != null)
        {
            delayedRewardsComponent.StopAllCoroutines();
        }
    }

    private void OnLifeUpdated(bool isDead)
    {
        if (isDead)
        {
            StopDelayedQuestRewards();
            ServerSetCutsceneModeActive(active: false);
        }
    }

    internal void InitializePlayer()
    {
        flagsMap = new Dictionary<ushort, PlayerQuestFlag>();
        flagsList = new List<PlayerQuestFlag>();
        questsList = new List<PlayerQuest>();
        groupInvites = new HashSet<CSteamID>();
        if (Provider.isServer || base.channel.IsLocalPlayer)
        {
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(OnLifeUpdated));
        }
        if (Provider.isServer)
        {
            load();
            base.player.movement.PlayerNavChanged += OnPlayerNavChanged;
            if (base.channel.IsLocalPlayer)
            {
                onFlagsUpdated?.Invoke();
            }
        }
        if (base.channel.IsLocalPlayer)
        {
            PlayerSkills skills = base.player.skills;
            skills.onExperienceUpdated = (ExperienceUpdated)Delegate.Combine(skills.onExperienceUpdated, new ExperienceUpdated(onExperienceUpdated));
            PlayerSkills skills2 = base.player.skills;
            skills2.onReputationUpdated = (ReputationUpdated)Delegate.Combine(skills2.onReputationUpdated, new ReputationUpdated(onReputationUpdated));
            PlayerInventory inventory = base.player.inventory;
            inventory.onInventoryStateUpdated = (InventoryStateUpdated)Delegate.Combine(inventory.onInventoryStateUpdated, new InventoryStateUpdated(onInventoryStateUpdated));
            LightingManager.onTimeOfDayChanged = (TimeOfDayChanged)Delegate.Combine(LightingManager.onTimeOfDayChanged, new TimeOfDayChanged(onTimeOfDayChanged));
        }
    }

    private void Start()
    {
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            try
            {
                Player.onPlayerCreated?.Invoke(base.player);
            }
            catch (Exception e)
            {
                UnturnedLog.warn("Exception during onPlayerCreated:");
                UnturnedLog.exception(e);
            }
        }
    }

    private void OnDestroy()
    {
        if (base.channel.IsLocalPlayer)
        {
            LightingManager.onTimeOfDayChanged = (TimeOfDayChanged)Delegate.Remove(LightingManager.onTimeOfDayChanged, new TimeOfDayChanged(onTimeOfDayChanged));
        }
        hasCreatedDelayedRewards = true;
        if (delayedRewardsGameObject != null)
        {
            UnityEngine.Object.Destroy(delayedRewardsGameObject);
            delayedRewardsGameObject = null;
        }
    }

    public void load()
    {
        wasLoadCalled = true;
        isMarkerPlaced = false;
        markerPosition = Vector3.zero;
        markerTextOverride = string.Empty;
        radioFrequency = DEFAULT_RADIO_FREQUENCY;
        if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Quests.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            River river = PlayerSavedata.openRiver(base.channel.owner.playerID, "/Player/Quests.dat", isReading: true);
            byte b = river.readByte();
            if (b > 0)
            {
                if (b > 6)
                {
                    isMarkerPlaced = river.readBoolean();
                    markerPosition = river.readSingleVector3();
                }
                if (b > 5)
                {
                    radioFrequency = river.readUInt32();
                }
                if (b > 2)
                {
                    groupID = river.readSteamID();
                }
                else
                {
                    groupID = CSteamID.Nil;
                }
                if (b > 3)
                {
                    groupRank = (EPlayerGroupRank)river.readByte();
                }
                else
                {
                    groupRank = EPlayerGroupRank.MEMBER;
                }
                if (b > 4)
                {
                    inMainGroup = river.readBoolean();
                }
                else
                {
                    inMainGroup = false;
                }
                ushort num = river.readUInt16();
                for (ushort num2 = 0; num2 < num; num2++)
                {
                    ushort num3 = river.readUInt16();
                    short newValue = river.readInt16();
                    PlayerQuestFlag playerQuestFlag = new PlayerQuestFlag(num3, newValue);
                    flagsMap.Add(num3, playerQuestFlag);
                    flagsList.Add(playerQuestFlag);
                }
                if (b >= 10)
                {
                    int num4 = river.readInt32();
                    for (int i = 0; i < num4; i++)
                    {
                        QuestAsset questAsset = Assets.find<QuestAsset>(river.readGUID());
                        if (questAsset != null)
                        {
                            PlayerQuest item = new PlayerQuest(questAsset);
                            questsList.Add(item);
                        }
                    }
                }
                else
                {
                    ushort num5 = river.readUInt16();
                    for (ushort num6 = 0; num6 < num5; num6++)
                    {
                        PlayerQuest item2 = new PlayerQuest(river.readUInt16());
                        questsList.Add(item2);
                    }
                }
                if (b >= 9)
                {
                    _trackedQuest = Assets.find<QuestAsset>(river.readGUID());
                }
                else if (b > 1)
                {
                    _trackedQuest = Assets.find(EAssetType.NPC, river.readUInt16()) as QuestAsset;
                }
                else
                {
                    _trackedQuest = null;
                }
                if (b < 8)
                {
                    npcSpawnId = null;
                }
                else
                {
                    npcSpawnId = river.readString();
                }
                if (b >= 11)
                {
                    npcCutsceneMode = river.readBoolean();
                }
                else
                {
                    npcCutsceneMode = false;
                }
            }
            river.closeRiver();
        }
        if (base.channel.IsLocalPlayer)
        {
            base.player.animator.NotifyLocalPlayerCutsceneModeActiveChanged(npcCutsceneMode);
        }
        if (Provider.modeConfigData.Gameplay.Allow_Dynamic_Groups)
        {
            if (groupID == CSteamID.Nil)
            {
                if (base.channel.owner.lobbyID != CSteamID.Nil && Provider.modeConfigData.Gameplay.Allow_Lobby_Groups)
                {
                    bool wasCreated;
                    GroupInfo orAddGroup = GroupManager.getOrAddGroup(base.channel.owner.lobbyID, base.channel.owner.playerID.playerName + "'s Group", out wasCreated);
                    if (wasCreated || orAddGroup.hasSpaceForMoreMembersInGroup)
                    {
                        groupID = base.channel.owner.lobbyID;
                        orAddGroup.members++;
                        groupRank = (wasCreated ? EPlayerGroupRank.OWNER : EPlayerGroupRank.MEMBER);
                        inMainGroup = false;
                        GroupManager.sendGroupInfo(orAddGroup);
                    }
                    else
                    {
                        loadMainGroup();
                    }
                }
                else
                {
                    loadMainGroup();
                }
            }
            else if (inMainGroup)
            {
                if (Provider.modeConfigData.Gameplay.Allow_Static_Groups)
                {
                    if (groupID != base.channel.owner.playerID.group)
                    {
                        loadMainGroup();
                    }
                }
                else
                {
                    loadMainGroup();
                }
            }
            else if (GroupManager.getGroupInfo(groupID) == null)
            {
                loadMainGroup();
            }
        }
        else
        {
            loadMainGroup();
        }
    }

    private void loadMainGroup()
    {
        if (Provider.modeConfigData.Gameplay.Allow_Static_Groups)
        {
            groupID = base.channel.owner.playerID.group;
            inMainGroup = groupID != CSteamID.Nil;
        }
        else
        {
            groupID = CSteamID.Nil;
            inMainGroup = false;
        }
        groupRank = EPlayerGroupRank.MEMBER;
    }

    private int FindIndexOfQuest(QuestAsset asset)
    {
        if (asset != null)
        {
            for (int i = 0; i < questsList.Count; i++)
            {
                if (questsList[i].asset == asset)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public void save()
    {
        if (!wasLoadCalled)
        {
            return;
        }
        River river = PlayerSavedata.openRiver(base.channel.owner.playerID, "/Player/Quests.dat", isReading: false);
        river.writeByte(11);
        river.writeBoolean(isMarkerPlaced);
        river.writeSingleVector3(markerPosition);
        river.writeUInt32(radioFrequency);
        river.writeSteamID(groupID);
        river.writeByte((byte)groupRank);
        river.writeBoolean(inMainGroup);
        river.writeUInt16((ushort)flagsList.Count);
        for (ushort num = 0; num < flagsList.Count; num++)
        {
            PlayerQuestFlag playerQuestFlag = flagsList[num];
            river.writeUInt16(playerQuestFlag.id);
            river.writeInt16(playerQuestFlag.value);
        }
        river.writeInt32(questsList.Count);
        foreach (PlayerQuest quests in questsList)
        {
            river.writeGUID(quests.asset?.GUID ?? Guid.Empty);
        }
        river.writeGUID(_trackedQuest?.GUID ?? Guid.Empty);
        river.writeString(string.IsNullOrEmpty(npcSpawnId) ? string.Empty : npcSpawnId);
        river.writeBoolean(npcCutsceneMode);
        river.closeRiver();
    }
}
