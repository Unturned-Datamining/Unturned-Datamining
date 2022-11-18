using SDG.Provider.Services.Matchmaking;
using SDG.Provider.Services.Multiplayer;
using SDG.SteamworksProvider.Services.Multiplayer;

namespace SDG.SteamworksProvider.Services.Matchmaking;

public class SteamworksServerInfoRequestResult : IServerInfoRequestResult
{
    public IServerInfo serverInfo { get; protected set; }

    public SteamworksServerInfoRequestResult(SteamworksServerInfo newServerInfo)
    {
        serverInfo = newServerInfo;
    }
}
