using System.IO;
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Provider.Services.Multiplayer;
using SDG.Provider.Services.Multiplayer.Client;
using SDG.SteamworksProvider.Services.Community;

namespace SDG.SteamworksProvider.Services.Multiplayer.Client;

public class SteamworksClientMultiplayerService : Service, IClientMultiplayerService, IService
{
    public IServerInfo serverInfo { get; protected set; }

    public bool isConnected { get; protected set; }

    public bool isAttempting { get; protected set; }

    public MemoryStream stream { get; protected set; }

    public BinaryReader reader { get; protected set; }

    public BinaryWriter writer { get; protected set; }

    public void connect(IServerInfo newServerInfo)
    {
    }

    public void disconnect()
    {
    }

    public bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel)
    {
        entity = SteamworksCommunityEntity.INVALID;
        length = 0uL;
        return false;
    }

    public void write(ICommunityEntity entity, byte[] data, ulong length)
    {
    }

    public void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel)
    {
    }
}
