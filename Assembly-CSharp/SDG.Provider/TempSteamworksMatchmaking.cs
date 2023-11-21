using System;
using System.Collections.Generic;
using System.Text;
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

    public delegate void PlayersQueryRefreshed(string name, int score, float time);

    public delegate void RulesQueryRefreshed(Dictionary<string, string> rulesMap);

    public MasterServerAdded onMasterServerAdded;

    public MasterServerRemoved onMasterServerRemoved;

    public MasterServerResorted onMasterServerResorted;

    public MasterServerRefreshed onMasterServerRefreshed;

    public MasterServerQueryRefreshed onMasterServerQueryRefreshed;

    public PlayersQueryRefreshed onPlayersQueryRefreshed;

    public RulesQueryRefreshed onRulesQueryRefreshed;

    private SteamConnectionInfo connectionInfo;

    private ESteamServerList _currentList;

    private string currentNameFilter;

    private EPlugins currentPluginsFilter;

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

    private IComparer<SteamServerInfo> _serverInfoComparer = new ServerListComparer_PingAscending();

    public ESteamServerList currentList => _currentList;

    public List<SteamServerInfo> serverList => _serverList;

    public bool isAttemptingServerQuery { get; private set; }

    public IComparer<SteamServerInfo> serverInfoComparer => _serverInfoComparer;

    public event AttemptUpdated onAttemptUpdated;

    public event TimedOut onTimedOut;

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

    public void cancelRequest()
    {
        if (serverListRequest != HServerListRequest.Invalid)
        {
            SteamMatchmakingServers.CancelQuery(serverListRequest);
            cleanupServerListRequest();
        }
    }

    public void refreshMasterServer(ServerListFilters inputFilters)
    {
        _currentList = inputFilters.listSource;
        currentPluginsFilter = inputFilters.plugins;
        currentNameFilter = inputFilters.serverName;
        _serverList.Clear();
        onMasterServerRemoved?.Invoke();
        cleanupServerListRequest();
        if (inputFilters.listSource == ESteamServerList.HISTORY)
        {
            serverListRequest = SteamMatchmakingServers.RequestHistoryServerList(SDG.Unturned.Provider.APP_ID, new MatchMakingKeyValuePair_t[0], 0u, serverListResponse);
            return;
        }
        if (inputFilters.listSource == ESteamServerList.FAVORITES)
        {
            serverListRequest = SteamMatchmakingServers.RequestFavoritesServerList(SDG.Unturned.Provider.APP_ID, new MatchMakingKeyValuePair_t[0], 0u, serverListResponse);
            return;
        }
        if (inputFilters.listSource == ESteamServerList.LAN)
        {
            serverListRequest = SteamMatchmakingServers.RequestLANServerList(SDG.Unturned.Provider.APP_ID, serverListResponse);
            return;
        }
        filters = new List<MatchMakingKeyValuePair_t>();
        MatchMakingKeyValuePair_t item = default(MatchMakingKeyValuePair_t);
        item.m_szKey = "gamedir";
        item.m_szValue = "unturned";
        filters.Add(item);
        LevelInfo levelInfo = Level.findLevelForServerFilter(inputFilters.mapName);
        if (!string.IsNullOrEmpty(inputFilters.mapName) && inputFilters.mapName.Length > 1)
        {
            if (levelInfo == null)
            {
                UnturnedLog.info("No local map matched with '{0}', filtering as-is", inputFilters.mapName);
            }
            else
            {
                UnturnedLog.info("Matched local map '{0}' with '{1}', adjusting filter", levelInfo.name, inputFilters.mapName);
                inputFilters.mapName = levelInfo.name;
            }
            MatchMakingKeyValuePair_t item2 = default(MatchMakingKeyValuePair_t);
            item2.m_szKey = "map";
            item2.m_szValue = inputFilters.mapName.ToLower();
            filters.Add(item2);
        }
        if (inputFilters.attendance == EAttendance.Empty)
        {
            MatchMakingKeyValuePair_t item3 = default(MatchMakingKeyValuePair_t);
            item3.m_szKey = "noplayers";
            item3.m_szValue = "1";
            filters.Add(item3);
        }
        else if (inputFilters.attendance == EAttendance.HasPlayers)
        {
            MatchMakingKeyValuePair_t item4 = default(MatchMakingKeyValuePair_t);
            item4.m_szKey = "hasplayers";
            item4.m_szValue = "1";
            filters.Add(item4);
        }
        if (inputFilters.notFull)
        {
            MatchMakingKeyValuePair_t item5 = default(MatchMakingKeyValuePair_t);
            item5.m_szKey = "notfull";
            item5.m_szValue = "1";
            filters.Add(item5);
        }
        MatchMakingKeyValuePair_t item6 = default(MatchMakingKeyValuePair_t);
        item6.m_szKey = "gamedataand";
        if (inputFilters.password == EPassword.YES)
        {
            item6.m_szValue = "PASS";
        }
        else if (inputFilters.password == EPassword.NO)
        {
            item6.m_szValue = "SSAP";
        }
        if (inputFilters.vacProtection == EVACProtectionFilter.Secure)
        {
            item6.m_szValue += ",";
            item6.m_szValue += "VAC_ON";
            MatchMakingKeyValuePair_t item7 = default(MatchMakingKeyValuePair_t);
            item7.m_szKey = "secure";
            item7.m_szValue = "1";
            filters.Add(item7);
        }
        else if (inputFilters.vacProtection == EVACProtectionFilter.Insecure)
        {
            item6.m_szValue += ",";
            item6.m_szValue += "VAC_OFF";
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
        if (inputFilters.workshop == EWorkshop.YES)
        {
            item8.m_szValue = "WSy";
        }
        else if (inputFilters.workshop == EWorkshop.NO)
        {
            item8.m_szValue = "WSn";
        }
        if (inputFilters.combat == ECombat.PVP)
        {
            item8.m_szValue += ",PVP";
        }
        else if (inputFilters.combat == ECombat.PVE)
        {
            item8.m_szValue += ",PVE";
        }
        if (inputFilters.cheats == ECheats.YES)
        {
            item8.m_szValue += ",CHy";
        }
        else if (inputFilters.cheats == ECheats.NO)
        {
            item8.m_szValue += ",CHn";
        }
        if (inputFilters.camera != ECameraMode.ANY)
        {
            ref string szValue = ref item8.m_szValue;
            szValue = szValue + "," + SDG.Unturned.Provider.getCameraModeTagAbbreviation(inputFilters.camera);
        }
        if (inputFilters.monetization == EServerMonetizationTag.None)
        {
            ref string szValue2 = ref item8.m_szValue;
            szValue2 = szValue2 + "," + SDG.Unturned.Provider.GetMonetizationTagAbbreviation(inputFilters.monetization);
        }
        else
        {
            bool flag = inputFilters.monetization == EServerMonetizationTag.NonGameplay;
            if (!LiveConfig.Get().shouldServersWithoutMonetizationTagBeVisibleInInternetServerList)
            {
                flag |= string.IsNullOrWhiteSpace(currentNameFilter);
            }
            if (flag)
            {
                filters.Add(new MatchMakingKeyValuePair_t("or", "2"));
                filters.Add(new MatchMakingKeyValuePair_t("gametagsand", SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.None)));
                filters.Add(new MatchMakingKeyValuePair_t("gametagsand", SDG.Unturned.Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.NonGameplay)));
            }
        }
        if (inputFilters.gold == EServerListGoldFilter.RequiresGold)
        {
            item8.m_szValue += ",GLD";
        }
        else if (inputFilters.gold == EServerListGoldFilter.DoesNotRequireGold)
        {
            item8.m_szValue += ",F2P";
        }
        if (inputFilters.battlEyeProtection == EBattlEyeProtectionFilter.Secure)
        {
            item8.m_szValue += ",BEy";
        }
        else if (inputFilters.battlEyeProtection == EBattlEyeProtectionFilter.Insecure)
        {
            item8.m_szValue += ",BEn";
        }
        filters.Add(item8);
        StringBuilder stringBuilder = new StringBuilder(128);
        stringBuilder.Append("Refreshing server list with filters:");
        foreach (MatchMakingKeyValuePair_t filter in filters)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(filter.m_szKey);
            stringBuilder.Append('=');
            stringBuilder.Append(filter.m_szValue);
        }
        UnturnedLog.info(stringBuilder.ToString());
        if (inputFilters.listSource == ESteamServerList.INTERNET)
        {
            serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint)filters.Count, serverListResponse);
        }
        else if (inputFilters.listSource == ESteamServerList.FRIENDS)
        {
            serverListRequest = SteamMatchmakingServers.RequestFriendsServerList(SDG.Unturned.Provider.APP_ID, filters.ToArray(), (uint)filters.Count, serverListResponse);
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
        if (currentPluginsFilter == EPlugins.NO)
        {
            if (steamServerInfo.pluginFramework != 0)
            {
                return;
            }
        }
        else if (currentPluginsFilter == EPlugins.YES && steamServerInfo.pluginFramework == SteamServerInfo.EPluginFramework.None)
        {
            return;
        }
        if (steamServerInfo.maxPlayers >= CommandMaxPlayers.MIN_NUMBER && (!string.IsNullOrEmpty(currentNameFilter) || Mathf.Max(steamServerInfo.players, steamServerInfo.maxPlayers) <= CommandMaxPlayers.MAX_NUMBER) && (string.IsNullOrEmpty(currentNameFilter) || steamServerInfo.name.IndexOf(currentNameFilter, StringComparison.OrdinalIgnoreCase) != -1))
        {
            int num = serverList.BinarySearch(steamServerInfo, serverInfoComparer);
            if (num < 0)
            {
                num = ~num;
            }
            serverList.Insert(num, steamServerInfo);
            onMasterServerAdded?.Invoke(num, steamServerInfo);
        }
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
