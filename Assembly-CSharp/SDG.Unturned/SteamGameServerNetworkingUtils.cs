using System;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

[Obsolete("Should not be specific to SteamGameServerNetworking after NetTransport rewrite")]
public static class SteamGameServerNetworkingUtils
{
    [Obsolete("Should not be specific to SteamGameServerNetworking")]
    public static bool getIPv4Address(CSteamID steamIDRemote, out uint address)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamIDRemote);
        if (transportConnection != null)
        {
            return transportConnection.TryGetIPv4Address(out address);
        }
        address = 0u;
        return false;
    }

    [Obsolete("Should not be specific to SteamGameServerNetworking")]
    public static uint getIPv4AddressOrZero(CSteamID steamIDRemote)
    {
        getIPv4Address(steamIDRemote, out var address);
        return address;
    }
}
