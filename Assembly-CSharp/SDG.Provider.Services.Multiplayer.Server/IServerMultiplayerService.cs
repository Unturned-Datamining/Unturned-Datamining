using System;
using System.IO;
using SDG.Provider.Services.Community;

namespace SDG.Provider.Services.Multiplayer.Server;

public interface IServerMultiplayerService : IService
{
    IServerInfo serverInfo { get; }

    bool isHosting { get; }

    MemoryStream stream { get; }

    BinaryReader reader { get; }

    BinaryWriter writer { get; }

    event ServerMultiplayerServiceReadyHandler ready;

    void open(uint ip, ushort port, ESecurityMode security);

    void close();

    bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel);

    void write(ICommunityEntity entity, byte[] data, ulong length);

    [Obsolete("Used by old multiplayer code, please use send without method instead.")]
    void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel);
}
