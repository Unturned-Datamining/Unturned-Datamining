using SDG.Provider.Services.Multiplayer.Client;
using SDG.Provider.Services.Multiplayer.Server;

namespace SDG.Provider.Services.Multiplayer;

public interface IMultiplayerService : IService
{
    /// <summary>
    /// Current client multiplayer implementation.
    /// </summary>
    IClientMultiplayerService clientMultiplayerService { get; }

    /// <summary>
    /// Current server multiplayer implementation.
    /// </summary>
    IServerMultiplayerService serverMultiplayerService { get; }
}
