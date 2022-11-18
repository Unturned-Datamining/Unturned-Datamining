using SDG.Provider.Services.Matchmaking;
using SDG.SteamworksProvider.Services.Multiplayer;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Matchmaking;

public class SteamworksServerInfoRequestHandle : IServerInfoRequestHandle
{
    public HServerQuery query;

    public ISteamMatchmakingPingResponse pingResponse;

    private ServerInfoRequestReadyCallback callback;

    public void onServerResponded(gameserveritem_t server)
    {
        SteamworksServerInfoRequestResult result = new SteamworksServerInfoRequestResult(new SteamworksServerInfo(server));
        triggerCallback(result);
        cleanupQuery();
        SteamworksMatchmakingService.serverInfoRequestHandles.Remove(this);
    }

    public void onServerFailedToRespond()
    {
        SteamworksServerInfoRequestResult result = new SteamworksServerInfoRequestResult(null);
        triggerCallback(result);
        cleanupQuery();
        SteamworksMatchmakingService.serverInfoRequestHandles.Remove(this);
    }

    public void triggerCallback(IServerInfoRequestResult result)
    {
        if (callback != null)
        {
            callback(this, result);
        }
    }

    private void cleanupQuery()
    {
        SteamMatchmakingServers.CancelServerQuery(query);
        query = HServerQuery.Invalid;
    }

    public SteamworksServerInfoRequestHandle(ServerInfoRequestReadyCallback newCallback)
    {
        callback = newCallback;
    }
}
