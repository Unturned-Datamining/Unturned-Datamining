using System;
using System.IO;
using SDG.Provider.Services.Community;

namespace SDG.Provider.Services.Multiplayer.Server;

public interface IServerMultiplayerService : IService
{
    /// <summary>
    /// Information about currently hosted server.
    /// </summary>
    IServerInfo serverInfo { get; }

    /// <summary>
    /// Whether a server is open.
    /// </summary>
    bool isHosting { get; }

    /// <summary>
    /// Network buffer memory stream.
    /// </summary>
    MemoryStream stream { get; }

    /// <summary>
    /// Network buffer memory stream reader.
    /// </summary>
    BinaryReader reader { get; }

    /// <summary>
    /// Network buffer memory stream writer.
    /// </summary>
    BinaryWriter writer { get; }

    event ServerMultiplayerServiceReadyHandler ready;

    /// <summary>
    /// Open a new server.
    /// </summary>
    void open(uint ip, ushort port, ESecurityMode security);

    /// <summary>
    /// Close an existing server.
    /// </summary>
    void close();

    /// <summary>
    /// Receive a packet from an entity across the network.
    /// </summary>
    /// <param name="entity">Sender of data.</param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <returns>Whether any data was read.</returns>
    bool read(out ICommunityEntity entity, byte[] data, out ulong length, int channel);

    /// <summary>
    /// Send a packet to an entity across the network.
    /// </summary>
    /// <param name="entity">Recipient of data.</param>
    /// <param name="data">Packet to send.</param>
    /// <param name="length">Size of data in array.</param>
    void write(ICommunityEntity entity, byte[] data, ulong length);

    /// <summary>
    /// Send a packet to an entity across the network.
    /// </summary>
    /// <param name="entity">Recipient of data.</param>
    /// <param name="data">Packet to send.</param>
    /// <param name="length">Size of data in array.</param>
    /// <param name="method">Type of send to use.</param>
    [Obsolete("Used by old multiplayer code, please use send without method instead.")]
    void write(ICommunityEntity entity, byte[] data, ulong length, ESendMethod method, int channel);
}
