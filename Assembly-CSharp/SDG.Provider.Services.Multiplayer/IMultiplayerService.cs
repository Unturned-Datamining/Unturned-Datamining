using SDG.Provider.Services.Multiplayer.Client;
using SDG.Provider.Services.Multiplayer.Server;

namespace SDG.Provider.Services.Multiplayer;

public interface IMultiplayerService : IService
{
    IClientMultiplayerService clientMultiplayerService { get; }

    IServerMultiplayerService serverMultiplayerService { get; }
}
