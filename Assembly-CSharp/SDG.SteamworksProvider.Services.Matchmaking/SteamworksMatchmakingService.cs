using System.Collections.Generic;
using SDG.Provider.Services;
using SDG.Provider.Services.Matchmaking;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Matchmaking;

public class SteamworksMatchmakingService : Service, IMatchmakingService, IService
{
    public static List<SteamworksServerInfoRequestHandle> serverInfoRequestHandles;

    public IServerInfoRequestHandle requestServerInfo(uint ip, ushort port, ServerInfoRequestReadyCallback callback)
    {
        SteamworksServerInfoRequestHandle steamworksServerInfoRequestHandle = new SteamworksServerInfoRequestHandle(callback);
        HServerQuery hServerQuery = (steamworksServerInfoRequestHandle.query = SteamMatchmakingServers.PingServer(ip, (ushort)(port + 1), steamworksServerInfoRequestHandle.pingResponse = new ISteamMatchmakingPingResponse(steamworksServerInfoRequestHandle.onServerResponded, steamworksServerInfoRequestHandle.onServerFailedToRespond)));
        serverInfoRequestHandles.Add(steamworksServerInfoRequestHandle);
        return steamworksServerInfoRequestHandle;
    }
}
