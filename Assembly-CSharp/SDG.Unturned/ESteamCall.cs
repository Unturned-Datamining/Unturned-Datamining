using System;

namespace SDG.Unturned;

public enum ESteamCall
{
    /// <summary>
    /// Replaced by ServerMethodHandle.
    /// </summary>
    [Obsolete]
    SERVER,
    /// <summary>
    /// Replaced by ClientInstanceMethod.InvokeAndLoopback or ClientStaticMethod.InvokeAndLoopback.
    /// </summary>
    [Obsolete]
    ALL,
    /// <summary>
    /// Replaced by ClientMethodHandle invoked with Provider.EnumerateClients_Remote.
    /// Unlike ESteamCall.CLIENTS this is not loopback invoked.
    /// </summary>
    [Obsolete]
    OTHERS,
    /// <summary>
    /// Replaced by ClientMethodHandle invoked with SteamChannel.GetOwnerTransportConnection.
    /// </summary>
    [Obsolete]
    OWNER,
    /// <summary>
    /// Replaced by ClientMethodHandle invoked with SteamChannel.EnumerateClients_RemoteNotOwner.
    /// </summary>
    [Obsolete]
    NOT_OWNER,
    /// <summary>
    /// Replaced by ClientMethodHandle invoked with Provider.EnumerateClients.
    /// Unlike ESteamCall.OTHERS this will be loopback invoked in singleplayer or listen server.
    /// </summary>
    [Obsolete]
    CLIENTS,
    /// <summary>
    /// May have been used by voice in early versions, but has been completely removed.
    /// </summary>
    [Obsolete]
    PEERS
}
