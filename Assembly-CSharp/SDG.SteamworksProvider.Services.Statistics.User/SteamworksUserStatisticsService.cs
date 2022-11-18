using System;
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Statistics.User;
using SDG.SteamworksProvider.Services.Community;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Statistics.User;

public class SteamworksUserStatisticsService : Service, IUserStatisticsService, IService
{
    private Callback<UserStatsReceived_t> userStatsReceivedCallback;

    public event UserStatisticsRequestReady onUserStatisticsRequestReady;

    private void triggerUserStatisticsRequestReady(ICommunityEntity entityID)
    {
        if (this.onUserStatisticsRequestReady != null)
        {
            this.onUserStatisticsRequestReady(entityID);
        }
    }

    public bool getStatistic(string name, out int data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        return SteamUserStats.GetStat(name, out data);
    }

    public bool setStatistic(string name, int data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        bool result = SteamUserStats.SetStat(name, data);
        SteamUserStats.StoreStats();
        return result;
    }

    public bool getStatistic(string name, out float data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        return SteamUserStats.GetStat(name, out data);
    }

    public bool setStatistic(string name, float data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        bool result = SteamUserStats.SetStat(name, data);
        SteamUserStats.StoreStats();
        return result;
    }

    public bool requestStatistics()
    {
        SteamUserStats.RequestCurrentStats();
        return true;
    }

    public SteamworksUserStatisticsService()
    {
        userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(onUserStatsReceived);
    }

    private void onUserStatsReceived(UserStatsReceived_t callback)
    {
        if (callback.m_nGameID == SteamUtils.GetAppID().m_AppId)
        {
            SteamworksCommunityEntity steamworksCommunityEntity = new SteamworksCommunityEntity(callback.m_steamIDUser);
            triggerUserStatisticsRequestReady(steamworksCommunityEntity);
        }
    }
}
