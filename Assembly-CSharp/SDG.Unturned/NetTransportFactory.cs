using System;
using SDG.NetTransport;
using SDG.NetTransport.SteamNetworking;
using SDG.NetTransport.SteamNetworkingSockets;
using SDG.NetTransport.SystemSockets;

namespace SDG.Unturned;

/// <summary>
/// Not extendable until transport API is better finalized.
/// </summary>
internal static class NetTransportFactory
{
    internal const string SystemSocketsTag = "sys";

    internal const string SteamNetworkingSocketsTag = "sns";

    internal const string SteamNetworkingTag = "def";

    private static CommandLineString clImpl = new CommandLineString("-NetTransport");

    private static CommandLineFlag clBypassEnableOldSteamNetworking = new CommandLineFlag(defaultValue: false, "-BypassEnableOldSteamNetworking");

    internal static string GetTag(IServerTransport serverTransport)
    {
        Type type = serverTransport.GetType();
        if (type == typeof(ServerTransport_SystemSockets))
        {
            return "sys";
        }
        if (type == typeof(ServerTransport_SteamNetworkingSockets))
        {
            return "sns";
        }
        if (type == typeof(ServerTransport_SteamNetworking))
        {
            return "def";
        }
        UnturnedLog.warn("Unknown net transport \"{0}\", using default tag", type.Name);
        return "sns";
    }

    internal static IClientTransport CreateClientTransport(string tag)
    {
        if (string.Equals(tag, "sys", StringComparison.OrdinalIgnoreCase))
        {
            return new ClientTransport_SystemSockets();
        }
        if (string.Equals(tag, "sns", StringComparison.OrdinalIgnoreCase))
        {
            return new ClientTransport_SteamNetworkingSockets();
        }
        if (string.Equals(tag, "def", StringComparison.OrdinalIgnoreCase))
        {
            return new ClientTransport_SteamNetworking();
        }
        UnturnedLog.warn("Unknown net transport tag \"{0}\", using default", tag);
        return new ClientTransport_SteamNetworkingSockets();
    }

    internal static IServerTransport CreateServerTransport()
    {
        if (clImpl.hasValue)
        {
            string value = clImpl.value;
            if (string.Equals(value, "SystemSockets", StringComparison.OrdinalIgnoreCase))
            {
                return new ServerTransport_SystemSockets();
            }
            if (string.Equals(value, "SteamNetworkingSockets", StringComparison.OrdinalIgnoreCase))
            {
                return new ServerTransport_SteamNetworkingSockets();
            }
            if (string.Equals(value, "SteamNetworking", StringComparison.OrdinalIgnoreCase))
            {
                if ((bool)clBypassEnableOldSteamNetworking)
                {
                    return new ServerTransport_SteamNetworking();
                }
                UnturnedLog.warn("Old Steam networking is no longer supported. Please remove this option from your command-line arguments.");
            }
            else
            {
                UnturnedLog.warn("Unknown net transport implementation \"{0}\"", value);
            }
        }
        return new ServerTransport_SteamNetworkingSockets();
    }
}
