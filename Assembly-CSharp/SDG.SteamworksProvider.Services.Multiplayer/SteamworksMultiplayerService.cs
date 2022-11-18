using SDG.Provider.Services;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Multiplayer.Client;
using SDG.Provider.Services.Multiplayer.Server;
using SDG.SteamworksProvider.Services.Multiplayer.Client;
using SDG.SteamworksProvider.Services.Multiplayer.Server;

namespace SDG.SteamworksProvider.Services.Multiplayer;

public class SteamworksMultiplayerService : IMultiplayerService, IService
{
    private SteamworksAppInfo appInfo;

    public IClientMultiplayerService clientMultiplayerService { get; protected set; }

    public IServerMultiplayerService serverMultiplayerService { get; protected set; }

    public void initialize()
    {
        serverMultiplayerService.initialize();
        if (!appInfo.isDedicated)
        {
            clientMultiplayerService.initialize();
        }
    }

    public void update()
    {
        serverMultiplayerService.update();
        if (!appInfo.isDedicated)
        {
            clientMultiplayerService.update();
        }
    }

    public void shutdown()
    {
        serverMultiplayerService.shutdown();
        if (!appInfo.isDedicated)
        {
            clientMultiplayerService.shutdown();
        }
    }

    public SteamworksMultiplayerService(SteamworksAppInfo newAppInfo)
    {
        appInfo = newAppInfo;
        serverMultiplayerService = new SteamworksServerMultiplayerService(appInfo);
        if (!appInfo.isDedicated)
        {
            clientMultiplayerService = new SteamworksClientMultiplayerService();
        }
    }
}
