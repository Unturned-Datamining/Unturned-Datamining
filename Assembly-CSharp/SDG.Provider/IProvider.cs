using SDG.Provider.Services.Achievements;
using SDG.Provider.Services.Browser;
using SDG.Provider.Services.Cloud;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Statistics;
using SDG.Provider.Services.Store;
using SDG.Provider.Services.Translation;

namespace SDG.Provider;

public interface IProvider
{
    IAchievementsService achievementsService { get; }

    IBrowserService browserService { get; }

    ICloudService cloudService { get; }

    ICommunityService communityService { get; }

    TempSteamworksEconomy economyService { get; }

    TempSteamworksMatchmaking matchmakingService { get; }

    IMultiplayerService multiplayerService { get; }

    IStatisticsService statisticsService { get; }

    IStoreService storeService { get; }

    ITranslationService translationService { get; }

    TempSteamworksWorkshop workshopService { get; }

    void intialize();

    void update();

    void shutdown();
}
