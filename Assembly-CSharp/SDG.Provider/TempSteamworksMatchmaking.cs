using System;
using System.Collections.Generic;
using SDG.HostBans;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Provider;

public class TempSteamworksMatchmaking
{
    public delegate void MasterServerAdded(int insert, SteamServerInfo server);

    public delegate void MasterServerRemoved();

    public delegate void MasterServerResorted();

    public delegate void MasterServerRefreshed(EMatchMakingServerResponse response);

    public delegate void MasterServerQueryRefreshed(SteamServerInfo server);

    public delegate void AttemptUpdated(int attempt);

    public delegate void TimedOut();

    public delegate void MatchmakingProgressedHandler();

    public delegate void MatchmakingFinishedHandler();

    public delegate void PlayersQueryRefreshed(string name, int score, float time);

    public delegate void RulesQueryRefreshed(Dictionary<string, string> rulesMap);

    public MasterServerAdded onMasterServerAdded;

    public MasterServerRemoved onMasterServerRemoved;

    public MasterServerResorted onMasterServerResorted;

    public MasterServerRefreshed onMasterServerRefreshed;

    public MasterServerQueryRefreshed onMasterServerQueryRefreshed;

    public MatchmakingProgressedHandler matchmakingProgressed;

    public MatchmakingFinishedHandler matchmakingFinished;

    private HashSet<ulong> matchmakingIgnored = new HashSet<ulong>();

    public SteamServerInfo matchmakingBestServer;

    public PlayersQueryRefreshed onPlayersQueryRefreshed;

    public RulesQueryRefreshed onRulesQueryRefreshed;

    private SteamConnectionInfo connectionInfo;

    private ESteamServerList _currentList;

    private List<SteamServerInfo> _serverList = new List<SteamServerInfo>();

    private List<MatchMakingKeyValuePair_t> filters;

    private ISteamMatchmakingPingResponse serverPingResponse;

    private ISteamMatchmakingServerListResponse serverListResponse;

    private ISteamMatchmakingPlayersResponse playersResponse;

    private ISteamMatchmakingRulesResponse rulesResponse;

    private HServerQuery playersQuery = HServerQuery.Invalid;

    private HServerQuery rulesQuery = HServerQuery.Invalid;

    private Dictionary<string, string> rulesMap;

    private HServerQuery serverQuery = HServerQuery.Invalid;

    private int serverQueryAttempts;

    /// <summary>
    /// Reset after starting connection attempt, so set to true afterwards to auto join the server.
    /// </summary>
    public bool autoJoinServerQuery;

    private HServerListRequest serverListRequest = HServerListRequest.Invalid;

    private int serverListRefreshIndex = -1;

    private IComparer<SteamServerInfo> _serverInfoComparer = new SteamServerInfoPingAscendingComparator();

    public ESteamServerList currentList => _currentList;

    public List<SteamServerInfo> serverList => _serverList;

    public bool isAttemptingServerQuery { get; private set; }

    public IComparer<SteamServerInfo> serverInfoComparer => _serverInfoComparer;

    public event AttemptUpdated onAttemptUpdated;

    public event TimedOut onTimedOut;

    public void initializeMatchmaking()
    {
        if (matchmakingBestServer != null)
        {
            matchmakingIgnored.Add(matchmakingBestServer.steamID.m_SteamID);
        }
        matchmakingBestServer = null;
    }

    public void sortMasterServer(IComparer<SteamServerInfo> newServerInfoComparer)
    {
        _serverInfoComparer = newServerInfoComparer;
        serverList.Sort(serverInfoComparer);
        onMasterServerResorted?.Invoke();
    }

    private void cleanupServerQuery()
    {
        if (!(serverQuery == HServerQuery.Invalid))
        {
            SteamMatchmakingServers.CancelServerQuery(serverQuery);
            serverQuery = HServerQuery.Invalid;
        }
    }

    private void cleanupPlayersQuery()
    {
        if (!(playersQuery == HServerQuery.Invalid))
        {
            SteamMatchmakingServers.CancelServerQuery(playersQuery);
            playersQuery = HServerQuery.Invalid;
        }
    }

    private void cleanupRulesQuery()
    {
        if (!(rulesQuery == HServerQuery.Invalid))
        {
            SteamMatchmakingServers.CancelServerQuery(rulesQuery);
            rulesQuery = HServerQuery.Invalid;
        }
    }

    private void cleanupServerListRequest()
    {
        if (!(serverListRequest == HServerListRequest.Invalid))
        {
            SteamMatchmakingServers.ReleaseRequest(serverListRequest);
            serverListRequest = HServerListRequest.Invalid;
            serverListRefreshIndex = -1;
        }
    }

    public void connect(SteamConnectionInfo info)
    {
        if (!SDG.Unturned.Provider.isConnected)
        {
            connectionInfo = info;
            serverQueryAttempts = 0;
            isAttemptingServerQuery = true;
            autoJoinServerQuery = false;
            attemptServerQuery();
        }
    }

    public void cancel()
    {
        if (isAttemptingServerQuery)
        {
            serverQueryAttempts = 99;
            onPingFailedToRespond();
        }
    }

    private void attemptServerQuery()
    {
        cleanupServerQuery();
        serverQuery = SteamMatchmakingServers.PingServer(connectionInfo.ip, connectionInfo.port, serverPingResponse);
        serverQueryAttempts++;
        this.onAttemptUpdated?.Invoke(serverQueryAttempts);
    }

    public void refreshMasterServer(ESteamServerList list, string filterMap, EPassword filterPassword, EWorkshop filterWorkshop, EPlugins filterPlugins, EAttendance filterAttendance, bool filterNotFull, EVACProtectionFilter filterVACProtection, EBattlEyeProtectionFilter filterBattlEyeProtection, bool filterPro, ECombat filterCombat, ECheats filterCheats, EGameMode filterMode, ECameraMode filterCamera, EServerMonetizationTag filterMonetization)
    {
        _currentList = list;
        _serverList.Clear();
        onMasterServerRemoved?.Invoke();
        cleanupServerListRequest();
        switch (list)
        {
        case ESteamServerList.HISTORY:
            serverListRequest = SteamMatchmakingServers.RequestHistoryServerList(SDG.Unturned.Provider.APP_ID, new MatchMakingKeyValuePair_t[0], 0u, serverListResponse);
            return;
        case ESteamServerList.FAVORITES:
            serverListRequest = SteamMatchmakingServers.RequestFavoritesServerList(SDG.Unturned.Provider.APP_ID, new MatchMakingKeyValuePair_t[0], 0u, serverListResponse);
            return;
        case ESteamServerList.LAN:
            serverListRequest = SteamMatchmakingServers.RequestLANServerList(SDG.Unturned.Provider.APP_ID, serverListResponse);
            return;
        }
        filters = new List<MatchMakingKeyValuePair_t>();
        MatchMakingKeyValuePair_t item = default(MatchMakingKeyValuePair_t);
        item.m_szKey = "gamedir";
        item.m_szValue = "unturned";
        filters.Add(item);
        LevelInfo levelInfo = Level.findLevelForServerFilter(filterMap);
        if (!string.IsNullOrEmpty(filterMap) && filterMap.Length > 1)
        {
            if (levelInfo == null)
            {
                UnturnedLog.info("No local map matched with '{0}', filtering as-is", filterMap);
            }
            else
            {
                UnturnedLog.info("Matched local map '{0}' with '{1}', adjusting filter", levelInfo.name, filterMap);
                filterMap = levelInfo.name;
            }
            MatchMakingKeyValuePair_t item2 = default(MatchMakingKeyValuePair_t);
            item2.m_szKey = "map";
            item2.m_szValue = filterMap.ToLower();
            filters.Add(item2);
        }
        switch (filterAttendance)
        {
        case EAttendance.Empty:
        {
            MatchMakingKeyValuePair_t item4 = default(MatchMakingKeyValuePair_t);
            item4.m_szKey = "noplayers";
            item4.m_szValue = "1";
            filters.Add(item4);
            break;
        }
        case EAttendance.HasPlayers:
        {
            MatchMakingKeyValuePair_t item3 = default(MatchMakingKeyValuePair_t);
            item3.m_szKey = "hasplayers";
            item3.m_szValue = "1";
            filters.Add(item3);
            break;
        }
        }
        if (filterNotFull)
        {
            MatchMakingKeyValuePair_t item5 = default(MatchMakingKeyValuePair_t);
            item5.m_szKey = "notfull";
            item5.m_szValue = "1";
            filters.Add(item5);
        }
        MatchMakingKeyValuePair_t item6 = default(MatchMakingKeyValuePair_t);
        item6.m_szKey = "gamedataand";
        switch (filterPassword)
        {
        case EPassword.YES:
            item6.m_szValue = "PASS";
            break;
        case EPassword.NO:
            item6.m_szValue = "SSAP";
            break;
        }
        switch (filterVACProtection)
        {
        case EVACProtectionFilter.Secure:
        {
            item6.m_szValue += ",";
            item6.m_szValue += "VAC_ON";
            MatchMakingKeyValuePair_t item7 = default(MatchMakingKeyValuePair_t);
            item7.m_szKey = "secure";
            item7.m_szValue = "1";
            filters.Add(item7);
            break;
        }
        case EVACProtectionFilter.Insecure:
            item6.m_szValue += ",";
            item6.m_szValue += "VAC_OFF";
            break;
        }
        item6.m_szValue += ",GAME_VERSION_";
        item6.m_szValue += VersionUtils.binaryToHexadecimal(SDG.Unturned.Provider.APP_VERSION_PACKED);
        if (levelInfo != null && levelInfo.configData != null && levelInfo.configData.PackedVersion != 0)
        {
            item6.m_szValue += ",MAP_VERSION_";
            item6.m_szValue += VersionUtils.binaryToHexadecimal(levelInfo.configData.PackedVersion);
        }
        filters.Add(item6);
        MatchMakingKeyValuePair_t item8 = default(MatchMakingKeyValuePair_t);
        item8.m_szKey = "gametagsand";
        switch (filterWorkshop)
        {
        case EWorkshop.YES:
            item8.m_szValue = "WSy";
            break;
        case EWorkshop.NO:
            item8.m_szValue = "WSn";
            break;
        }
        switch (filterCombat)
        {
        case ECombat.PVP:
            item8.m_szValue += ",PVP";
            break;
        case ECombat.PVE:
            item8.m_szValue += ",PVE";
            break;
        }
        switch (filterCheats)
        {
        case ECheats.YES:
            item8.m_szValue += ",CHy";
            break;
        case ECheats.NO:
            item8.m_szValue += ",CHn";
            break;
        }
        if (filterMode != EGameMode.ANY)
        {
            ref string szValue = ref item8.m_szValue;
            szValue = szValue + "," + SDG.Unturned.Provider.getModeTagAbbreviation(filterMode);
        }
        if (filterCamera != ECameraMode.ANY)
        {
            ref string szValue2 = ref item8.m_szValue;
            szValue2 = szValue2 + "," + SDG.Unturned.Provider.getCameraModeTagAbbreviation(filterCamera);
        }
        if (filterMonetization == EServerMonetizationTag.None)
        {
            ref string szValue3 = ref item8.m_szValue;
            szValue3 = szValue3 + "," + SDG.Unturned.Provider.GetMonetizationTagAbbreviation(filterMonetization);
        }
        else
        {
            bool flag = filterMonetization == EServerMonetizationTag.NonGameplay;
            if (!LiveConfig.Get().shouldServersWithoutMonetizationTagBeVisibleInInternetServerList)
            {
                flag |= string.IsNullOrWhiteSpace(PlaySettings.serversName);
            }
            if (flag)
            {
                filters.Add(new MatchMakingKeyValuePair_t("or", "2"));
                filters.Add(new MatchMakingKeyValuePair_t("gametagsand", SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.None)));
                filters.Add(new MatchMakingKeyValuePair_t("gametagsand", SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.NonGameplay)));
            }
        }
        if (filterPro)
        {
            item8.m_szValue += ",GLD";
        }
        else
        {
            item8.m_szValue += ",F2P";
        }
        switch (filterBattlEyeProtection)
        {
        case EBattlEyeProtectionFilter.Secure:
            item8.m_szValue += ",BEy";
            break;
        case EBattlEyeProtectionFilter.Insecure:
            item8.m_szValue += ",BEn";
            break;
        }
        filters.Add(item8);
        switch (list)
        {
        case ESteamServerList.INTERNET:
            serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint)filters.Count, serverListResponse);
            break;
        case ESteamServerList.FRIENDS:
            serverListRequest = SteamMatchmakingServers.RequestFriendsServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint)filters.Count, serverListResponse);
            break;
        }
    }

    public void refreshPlayers(uint ip, ushort port)
    {
        cleanupPlayersQuery();
        playersQuery = SteamMatchmakingServers.PlayerDetails(ip, port, playersResponse);
    }

    public void refreshRules(uint ip, ushort port)
    {
        cleanupRulesQuery();
        rulesMap = new Dictionary<string, string>();
        rulesQuery = SteamMatchmakingServers.ServerRules(ip, port, rulesResponse);
    }

    private void onPingResponded(gameserveritem_t data)
    {
        isAttemptingServerQuery = false;
        cleanupServerQuery();
        if (data.m_nAppID == SDG.Unturned.Provider.APP_ID.m_AppId)
        {
            SteamServerInfo steamServerInfo = new SteamServerInfo(data, SteamServerInfo.EInfoSource.DirectConnect);
            if (!steamServerInfo.isPro || SDG.Unturned.Provider.isPro)
            {
                bool flag = false;
                if (autoJoinServerQuery && (!steamServerInfo.isPassworded || !string.IsNullOrEmpty(connectionInfo.password)))
                {
                    if (new IPv4Address(steamServerInfo.ip).IsWideAreaNetwork)
                    {
                        EInternetMultiplayerAvailability internetMultiplayerAvailability = SDG.Unturned.Provider.GetInternetMultiplayerAvailability();
                        if (internetMultiplayerAvailability != 0)
                        {
                            UnturnedLog.info($"Cannot directly connect because Internet multiplayer is unavailable ({internetMultiplayerAvailability})");
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    SDG.Unturned.Provider.connect(steamServerInfo, connectionInfo.password, null);
                }
                else
                {
                    MenuUI.closeAll();
                    MenuUI.closeAlert();
                    MenuPlayServerInfoUI.open(steamServerInfo, connectionInfo.password, MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT);
                }
            }
            else
            {
                SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_SERVER;
            }
        }
        else
        {
            SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.TIMED_OUT;
        }
        this.onTimedOut?.Invoke();
    }

    private void onPingFailedToRespond()
    {
        if (serverQueryAttempts < 10)
        {
            attemptServerQuery();
            return;
        }
        isAttemptingServerQuery = false;
        cleanupServerQuery();
        SDG.Unturned.Provider._connectionFailureInfo = ESteamConnectionFailureInfo.TIMED_OUT;
        this.onTimedOut?.Invoke();
    }

    private void onServerListResponded(HServerListRequest request, int index)
    {
        if (request != serverListRequest)
        {
            return;
        }
        gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, index);
        if (matchmakingIgnored.Contains(serverDetails.m_steamID.m_SteamID))
        {
            return;
        }
        if (_currentList == ESteamServerList.INTERNET && !serverDetails.m_steamID.BPersistentGameServerAccount())
        {
            UnturnedLog.info("Ignoring server \"" + serverDetails.GetServerName() + "\" because it is anonymous on the internet list");
            return;
        }
        if (serverDetails.m_nAppID != SDG.Unturned.Provider.APP_ID.m_AppId)
        {
            UnturnedLog.info($"Ignoring server \"{serverDetails.GetServerName()}\" because it has a different AppID ({serverDetails.m_nAppID})");
            return;
        }
        IPv4Address ip = new IPv4Address(serverDetails.m_NetAdr.GetIP());
        EHostBanFlags eHostBanFlags = HostBansManager.Get().MatchBasicDetails(ip, serverDetails.m_NetAdr.GetQueryPort(), serverDetails.GetServerName(), serverDetails.m_steamID.m_SteamID);
        if (eHostBanFlags.HasFlag(EHostBanFlags.HiddenFromAllServerLists) || eHostBanFlags.HasFlag(EHostBanFlags.Blocked) || (_currentList == ESteamServerList.INTERNET && eHostBanFlags.HasFlag(EHostBanFlags.HiddenFromInternetServerList)))
        {
            return;
        }
        SteamServerInfo steamServerInfo = new SteamServerInfo(serverDetails, _currentList switch
        {
            ESteamServerList.FRIENDS => SteamServerInfo.EInfoSource.FriendServerList, 
            ESteamServerList.FAVORITES => SteamServerInfo.EInfoSource.FavoriteServerList, 
            ESteamServerList.HISTORY => SteamServerInfo.EInfoSource.HistoryServerList, 
            ESteamServerList.LAN => SteamServerInfo.EInfoSource.LanServerList, 
            _ => SteamServerInfo.EInfoSource.InternetServerList, 
        });
        eHostBanFlags |= HostBansManager.Get().MatchExtendedDetails(steamServerInfo.descText, steamServerInfo.thumbnailURL);
        if (eHostBanFlags.HasFlag(EHostBanFlags.HiddenFromAllServerLists) || eHostBanFlags.HasFlag(EHostBanFlags.Blocked) || (_currentList == ESteamServerList.INTERNET && eHostBanFlags.HasFlag(EHostBanFlags.HiddenFromInternetServerList)))
        {
            return;
        }
        steamServerInfo.SetServerListHostBanFlags(eHostBanFlags);
        if (index == serverListRefreshIndex)
        {
            onMasterServerQueryRefreshed?.Invoke(steamServerInfo);
            return;
        }
        if (FilterSettings.filterPlugins == EPlugins.NO)
        {
            if (steamServerInfo.pluginFramework != 0)
            {
                return;
            }
        }
        else if (FilterSettings.filterPlugins == EPlugins.YES && steamServerInfo.pluginFramework == SteamServerInfo.EPluginFramework.None)
        {
            return;
        }
        if (steamServerInfo.maxPlayers < CommandMaxPlayers.MIN_NUMBER || (string.IsNullOrEmpty(PlaySettings.serversName) && Mathf.Max(steamServerInfo.players, steamServerInfo.maxPlayers) > CommandMaxPlayers.MAX_NUMBER) || (PlaySettings.serversName != null && PlaySettings.serversName.Length > 1 && steamServerInfo.name.IndexOf(PlaySettings.serversName, StringComparison.OrdinalIgnoreCase) == -1))
        {
            return;
        }
        int num = serverList.BinarySearch(steamServerInfo, serverInfoComparer);
        if (num < 0)
        {
            num = ~num;
        }
        serverList.Insert(num, steamServerInfo);
        onMasterServerAdded?.Invoke(num, steamServerInfo);
        matchmakingBestServer = null;
        int num2 = 25;
        while (matchmakingBestServer == null && num2 <= OptionsSettings.maxMatchmakingPing)
        {
            int num3 = -1;
            foreach (SteamServerInfo server in serverList)
            {
                if (server.players < OptionsSettings.minMatchmakingPlayers)
                {
                    break;
                }
                if (server.players != num3)
                {
                    num3 = server.players;
                    if (server.ping <= num2)
                    {
                        matchmakingBestServer = server;
                        break;
                    }
                }
            }
            num2 += 25;
        }
        matchmakingProgressed?.Invoke();
    }

    private void onServerListFailedToRespond(HServerListRequest request, int index)
    {
    }

    private void onRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
    {
        if (!(request == serverListRequest))
        {
            return;
        }
        onMasterServerRefreshed?.Invoke(response);
        matchmakingFinished?.Invoke();
        if (response == EMatchMakingServerResponse.eNoServersListedOnMasterServer || serverList.Count == 0)
        {
            UnturnedLog.info("No servers found on the master server.");
            return;
        }
        switch (response)
        {
        case EMatchMakingServerResponse.eServerFailedToRespond:
            UnturnedLog.error("Failed to connect to the master server.");
            break;
        case EMatchMakingServerResponse.eServerResponded:
            UnturnedLog.info("Successfully refreshed the master server.");
            break;
        }
    }

    private void onAddPlayerToList(string name, int score, float time)
    {
        onPlayersQueryRefreshed?.Invoke(name, score, time);
    }

    private void onPlayersFailedToRespond()
    {
        UnturnedLog.warn("Server players query failed to respond");
    }

    private void onPlayersRefreshComplete()
    {
    }

    private void onRulesResponded(string key, string value)
    {
        if (rulesMap != null)
        {
            rulesMap.Add(key, value);
        }
    }

    private void onRulesFailedToRespond()
    {
        UnturnedLog.warn("Server rules query failed to respond");
    }

    private void onRulesRefreshComplete()
    {
        onRulesQueryRefreshed?.Invoke(rulesMap);
    }

    public TempSteamworksMatchmaking()
    {
        serverPingResponse = new ISteamMatchmakingPingResponse(onPingResponded, onPingFailedToRespond);
        serverListResponse = new ISteamMatchmakingServerListResponse(onServerListResponded, onServerListFailedToRespond, onRefreshComplete);
        playersResponse = new ISteamMatchmakingPlayersResponse(onAddPlayerToList, onPlayersFailedToRespond, onPlayersRefreshComplete);
        rulesResponse = new ISteamMatchmakingRulesResponse(onRulesResponded, onRulesFailedToRespond, onRulesRefreshComplete);
    }
}
