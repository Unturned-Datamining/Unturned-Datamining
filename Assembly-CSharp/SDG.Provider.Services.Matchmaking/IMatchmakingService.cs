namespace SDG.Provider.Services.Matchmaking;

public interface IMatchmakingService : IService
{
    IServerInfoRequestHandle requestServerInfo(uint ip, ushort port, ServerInfoRequestReadyCallback callback);
}
