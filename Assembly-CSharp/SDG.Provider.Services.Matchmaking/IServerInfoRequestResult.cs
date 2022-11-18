using SDG.Provider.Services.Multiplayer;

namespace SDG.Provider.Services.Matchmaking;

public interface IServerInfoRequestResult
{
    IServerInfo serverInfo { get; }
}
