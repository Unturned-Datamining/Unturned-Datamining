using System;
using SDG.Provider;
using SDG.Provider.Services.Achievements;
using SDG.Provider.Services.Browser;
using SDG.Provider.Services.Cloud;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Statistics;
using SDG.Provider.Services.Store;
using SDG.Provider.Services.Translation;
using SDG.SteamworksProvider.Services.Achievements;
using SDG.SteamworksProvider.Services.Browser;
using SDG.SteamworksProvider.Services.Cloud;
using SDG.SteamworksProvider.Services.Community;
using SDG.SteamworksProvider.Services.Multiplayer;
using SDG.SteamworksProvider.Services.Statistics;
using SDG.SteamworksProvider.Services.Store;
using SDG.SteamworksProvider.Services.Translation;
using Steamworks;

namespace SDG.SteamworksProvider;

public class SteamworksProvider : IProvider
{
    private SteamworksAppInfo appInfo;

    public IAchievementsService achievementsService { get; protected set; }

    public IBrowserService browserService { get; protected set; }

    public ICloudService cloudService { get; protected set; }

    public ICommunityService communityService { get; protected set; }

    public TempSteamworksEconomy economyService { get; protected set; }

    public TempSteamworksMatchmaking matchmakingService { get; protected set; }

    public IMultiplayerService multiplayerService { get; protected set; }

    public IStatisticsService statisticsService { get; protected set; }

    public IStoreService storeService { get; protected set; }

    public ITranslationService translationService { get; protected set; }

    public TempSteamworksWorkshop workshopService { get; protected set; }

    public void intialize()
    {
        if (!appInfo.isDedicated)
        {
            if (SteamAPI.RestartAppIfNecessary((AppId_t)appInfo.id))
            {
                throw new Exception("Restarting app from Steam.");
            }
            if (!SteamAPI.Init())
            {
                throw new Exception("Steam API initialization failed.");
            }
        }
        initializeServices();
    }

    public void update()
    {
        if (multiplayerService.serverMultiplayerService.isHosting)
        {
            GameServer.RunCallbacks();
        }
        if (!appInfo.isDedicated)
        {
            SteamAPI.RunCallbacks();
        }
        updateServices();
    }

    public void shutdown()
    {
        if (!appInfo.isDedicated)
        {
            SteamAPI.Shutdown();
        }
        shutdownServices();
    }

    private void constructServices()
    {
        achievementsService = new SteamworksAchievementsService();
        economyService = new TempSteamworksEconomy(appInfo);
        multiplayerService = new SteamworksMultiplayerService(appInfo);
        statisticsService = new SteamworksStatisticsService();
        workshopService = new TempSteamworksWorkshop(appInfo);
        if (!appInfo.isDedicated)
        {
            browserService = new SteamworksBrowserService();
            cloudService = new SteamworksCloudService();
            communityService = new SteamworksCommunityService();
            matchmakingService = new TempSteamworksMatchmaking();
            storeService = new SteamworksStoreService(appInfo);
            translationService = new SteamworksTranslationService();
        }
    }

    private void initializeServices()
    {
        if (achievementsService != null)
        {
            achievementsService.initialize();
        }
        if (multiplayerService != null)
        {
            multiplayerService.initialize();
        }
        if (statisticsService != null)
        {
            statisticsService.initialize();
        }
        if (!appInfo.isDedicated)
        {
            if (browserService != null)
            {
                browserService.initialize();
            }
            if (cloudService != null)
            {
                cloudService.initialize();
            }
            if (communityService != null)
            {
                communityService.initialize();
            }
            if (economyService != null)
            {
                economyService.initializeClient();
            }
            if (storeService != null)
            {
                storeService.initialize();
            }
            if (translationService != null)
            {
                translationService.initialize();
            }
        }
    }

    private void updateServices()
    {
        if (achievementsService != null)
        {
            achievementsService.update();
        }
        if (multiplayerService != null)
        {
            multiplayerService.update();
        }
        if (statisticsService != null)
        {
            statisticsService.update();
        }
        if (workshopService != null)
        {
            workshopService.update();
        }
        if (!appInfo.isDedicated)
        {
            if (browserService != null)
            {
                browserService.update();
            }
            if (cloudService != null)
            {
                cloudService.update();
            }
            if (communityService != null)
            {
                communityService.update();
            }
            if (storeService != null)
            {
                storeService.update();
            }
            if (translationService != null)
            {
                translationService.update();
            }
        }
    }

    private void shutdownServices()
    {
        if (achievementsService != null)
        {
            achievementsService.shutdown();
        }
        if (multiplayerService != null)
        {
            multiplayerService.shutdown();
        }
        if (statisticsService != null)
        {
            statisticsService.shutdown();
        }
        if (!appInfo.isDedicated)
        {
            if (browserService != null)
            {
                browserService.shutdown();
            }
            if (cloudService != null)
            {
                cloudService.shutdown();
            }
            if (communityService != null)
            {
                communityService.shutdown();
            }
            if (storeService != null)
            {
                storeService.shutdown();
            }
            if (translationService != null)
            {
                translationService.shutdown();
            }
        }
    }

    public SteamworksProvider(SteamworksAppInfo newAppInfo)
    {
        appInfo = newAppInfo;
        constructServices();
    }
}
