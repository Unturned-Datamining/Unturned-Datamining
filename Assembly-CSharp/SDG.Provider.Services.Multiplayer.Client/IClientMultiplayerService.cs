using System;
using System.IO;
using SDG.Provider.Services.Community;

namespace SDG.Provider.Services.Multiplayer.Client;

public interface IClientMultiplayerService : IService
{
    IServerInfo serverInfo { get; }

    bool isConnected { get; }

    bool isAttempting { get; }

    MemoryStream stream { get; }

    BinaryReader reader { get; }

    BinaryWriter writer { get; }

    void connect(IServerInfo newServerInfo);

    void disconnect();

    bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel);

    void write(ICommunityEntity entity, byte[] data, ulong length);

    [Obsolete("Used by old multiplayer code, please use send without method instead.")]
    void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel);
}
