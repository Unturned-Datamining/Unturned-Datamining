using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class GroupManager : SteamCaller
{
    public static readonly byte SAVEDATA_VERSION = 3;

    private static GroupManager manager;

    private static CSteamID availableGroupID;

    private static Dictionary<CSteamID, GroupInfo> knownGroups;

    private static List<QueuedGroupExit> queuedExits;

    private static readonly ClientStaticMethod<CSteamID, string, uint> SendGroupInfo = ClientStaticMethod<CSteamID, string, uint>.Get(ReceiveGroupInfo);

    public static GroupManager instance => manager;

    public static event GroupInfoReadyHandler groupInfoReady;

    public static CSteamID generateUniqueGroupID()
    {
        CSteamID result = availableGroupID;
        availableGroupID.SetAccountID(new AccountID_t(availableGroupID.GetAccountID().m_AccountID + 1));
        return result;
    }

    public static GroupInfo addGroup(CSteamID groupID, string name)
    {
        GroupInfo groupInfo = new GroupInfo(groupID, name, 0u);
        knownGroups.Add(groupID, groupInfo);
        return groupInfo;
    }

    public static GroupInfo getGroupInfo(CSteamID groupID)
    {
        GroupInfo value = null;
        knownGroups.TryGetValue(groupID, out value);
        return value;
    }

    public static GroupInfo getOrAddGroup(CSteamID groupID, string name, out bool wasCreated)
    {
        wasCreated = false;
        GroupInfo groupInfo = getGroupInfo(groupID);
        if (groupInfo == null)
        {
            groupInfo = addGroup(groupID, name);
            wasCreated = true;
        }
        return groupInfo;
    }

    public static void deleteGroup(CSteamID groupID)
    {
        knownGroups.Remove(groupID);
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!(client.player == null) && !(client.player.quests == null) && client.player.quests.isMemberOfGroup(groupID))
            {
                client.player.quests.leaveGroup(force: true);
            }
        }
    }

    private static void triggerGroupInfoReady(GroupInfo group)
    {
        GroupManager.groupInfoReady?.Invoke(group);
    }

    [Obsolete]
    public static void sendGroupInfo(CSteamID steamID, GroupInfo group)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendGroupInfo(transportConnection, group);
        }
    }

    public static void sendGroupInfo(ITransportConnection transportConnection, GroupInfo group)
    {
        SendGroupInfo.Invoke(ENetReliability.Reliable, transportConnection, group.groupID, group.name, group.members);
    }

    public static void sendGroupInfo(IEnumerable<ITransportConnection> transportConnections, GroupInfo group)
    {
        SendGroupInfo.Invoke(ENetReliability.Reliable, transportConnections, group.groupID, group.name, group.members);
    }

    public static void sendGroupInfo(GroupInfo group)
    {
        sendGroupInfo(Provider.EnumerateClients_RemotePredicate((SteamPlayer client) => client.player.quests.isMemberOfGroup(group.groupID)), group);
    }

    [Obsolete]
    public void tellGroupInfo(CSteamID steamID, CSteamID groupID, string name, uint members)
    {
        ReceiveGroupInfo(groupID, name, members);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellGroupInfo")]
    public static void ReceiveGroupInfo(CSteamID groupID, string name, uint members)
    {
        GroupInfo groupInfo = getGroupInfo(groupID);
        if (groupInfo == null)
        {
            groupInfo = new GroupInfo(groupID, name, members);
            knownGroups.Add(groupInfo.groupID, groupInfo);
        }
        else
        {
            groupInfo.name = name;
            groupInfo.members = members;
        }
        triggerGroupInfoReady(groupInfo);
    }

    private void onLevelLoaded(int level)
    {
        if (level > Level.BUILD_INDEX_SETUP)
        {
            availableGroupID = new CSteamID(new AccountID_t(1u), EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeConsoleUser);
            knownGroups = new Dictionary<CSteamID, GroupInfo>();
            queuedExits = new List<QueuedGroupExit>();
            if (Provider.isServer && Level.info != null)
            {
                load();
            }
        }
    }

    private void Start()
    {
        manager = this;
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
    }

    public static bool isPlayerInGroupExitQueue(Player player)
    {
        foreach (QueuedGroupExit queuedExit in queuedExits)
        {
            if (queuedExit.playerID == player.channel.owner.playerID.steamID)
            {
                return true;
            }
        }
        return false;
    }

    public static void requestGroupExit(Player player)
    {
        uint timer_Leave_Group = Provider.modeConfigData.Gameplay.Timer_Leave_Group;
        if (timer_Leave_Group != 0)
        {
            alertGroupmatesTimer(player, timer_Leave_Group);
            QueuedGroupExit queuedGroupExit = new QueuedGroupExit();
            queuedGroupExit.playerID = player.channel.owner.playerID.steamID;
            queuedGroupExit.remainingSeconds = timer_Leave_Group;
            queuedExits.Add(queuedGroupExit);
        }
        else
        {
            alertGroupmatesLeft(player);
            player.quests.leaveGroup();
        }
    }

    public static void cancelGroupExit(Player player)
    {
        for (int num = queuedExits.Count - 1; num >= 0; num--)
        {
            if (queuedExits[num].playerID == player.channel.owner.playerID.steamID)
            {
                queuedExits.RemoveAtFast(num);
                break;
            }
        }
    }

    private static void serverSendMessageToGroupmates(Player player, string message)
    {
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!(client.player == null) && client.player.quests.isMemberOfSameGroupAs(player))
            {
                ChatManager.serverSendMessage(message, Color.yellow, null, client);
            }
        }
    }

    private static void alertGroupmatesTimer(Player player, uint remainingSeconds)
    {
        string playerName = player.channel.owner.playerID.playerName;
        string message = Provider.localization.format("Player_Group_Queue_Leave", playerName, remainingSeconds);
        serverSendMessageToGroupmates(player, message);
    }

    private static void alertGroupmatesLeft(Player player)
    {
        string playerName = player.channel.owner.playerID.playerName;
        string message = Provider.localization.format("Player_Group_Left", playerName);
        serverSendMessageToGroupmates(player, message);
    }

    private void tickGroupExitQueue(float deltaTime)
    {
        for (int num = queuedExits.Count - 1; num >= 0; num--)
        {
            QueuedGroupExit queuedGroupExit = queuedExits[num];
            queuedGroupExit.remainingSeconds -= deltaTime;
            if (!(queuedGroupExit.remainingSeconds > 0f))
            {
                queuedExits.RemoveAtFast(num);
                Player player = PlayerTool.getPlayer(queuedGroupExit.playerID);
                if (!(player == null))
                {
                    alertGroupmatesLeft(player);
                    player.quests.leaveGroup();
                }
            }
        }
    }

    private void Update()
    {
        if (Provider.isServer && queuedExits != null && queuedExits.Count > 0)
        {
            tickGroupExitQueue(Time.deltaTime);
        }
    }

    public static void load()
    {
        if (!LevelSavedata.fileExists("/Groups.dat"))
        {
            return;
        }
        River river = LevelSavedata.openRiver("/Groups.dat", isReading: true);
        byte b = river.readByte();
        if (b > 0)
        {
            availableGroupID = river.readSteamID();
            if (b < 3)
            {
                availableGroupID.SetEUniverse(EUniverse.k_EUniversePublic);
                availableGroupID.SetEAccountType(EAccountType.k_EAccountTypeConsoleUser);
            }
            if (b > 1)
            {
                uint num = availableGroupID.GetAccountID().m_AccountID;
                int num2 = river.readInt32();
                for (int i = 0; i < num2; i++)
                {
                    CSteamID cSteamID = river.readSteamID();
                    string text = river.readString();
                    uint num3 = river.readUInt32();
                    if (num3 >= 1 && !string.IsNullOrEmpty(text) && !knownGroups.ContainsKey(cSteamID))
                    {
                        num = MathfEx.Max(num, cSteamID.GetAccountID().m_AccountID + 1);
                        knownGroups.Add(cSteamID, new GroupInfo(cSteamID, text, num3));
                    }
                }
                availableGroupID.SetAccountID(new AccountID_t(num));
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        uint num = availableGroupID.GetAccountID().m_AccountID;
        Dictionary<CSteamID, GroupInfo>.ValueCollection values = knownGroups.Values;
        List<GroupInfo> list = new List<GroupInfo>();
        foreach (GroupInfo item in values)
        {
            if (item.members >= 1 && !string.IsNullOrEmpty(item.name))
            {
                num = MathfEx.Max(num, item.groupID.GetAccountID().m_AccountID + 1);
                list.Add(item);
            }
        }
        availableGroupID.SetAccountID(new AccountID_t(num));
        River river = LevelSavedata.openRiver("/Groups.dat", isReading: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeSteamID(availableGroupID);
        river.writeInt32(list.Count);
        foreach (GroupInfo item2 in list)
        {
            river.writeSteamID(item2.groupID);
            river.writeString(item2.name);
            river.writeUInt32(item2.members);
        }
        river.closeRiver();
    }
}
