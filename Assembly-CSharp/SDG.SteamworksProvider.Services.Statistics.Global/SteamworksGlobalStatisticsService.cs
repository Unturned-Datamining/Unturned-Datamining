using System;
using SDG.Provider.Services;
using SDG.Provider.Services.Statistics.Global;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Statistics.Global;

public class SteamworksGlobalStatisticsService : Service, IGlobalStatisticsService, IService
{
    private Callback<GlobalStatsReceived_t> globalStatsReceived;

    public event GlobalStatisticsRequestReady onGlobalStatisticsRequestReady;

    private void triggerGlobalStatisticsRequestReady()
    {
        if (this.onGlobalStatisticsRequestReady != null)
        {
            this.onGlobalStatisticsRequestReady();
        }
    }

    public bool getStatistic(string name, out long data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        return SteamUserStats.GetGlobalStat(name, out data);
    }

    public bool getStatistic(string name, out double data)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        return SteamUserStats.GetGlobalStat(name, out data);
    }

    public bool requestStatistics()
    {
        SteamUserStats.RequestGlobalStats(0);
        return true;
    }

    public SteamworksGlobalStatisticsService()
    {
        globalStatsReceived = Callback<GlobalStatsReceived_t>.Create(onGlobalStatsReceived);
    }

    private void onGlobalStatsReceived(GlobalStatsReceived_t callback)
    {
        if (callback.m_nGameID == SteamUtils.GetAppID().m_AppId)
        {
            triggerGlobalStatisticsRequestReady();
        }
    }
}
