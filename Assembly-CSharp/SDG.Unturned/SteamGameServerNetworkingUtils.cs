using Steamworks;

namespace SDG.Unturned;

public static class SteamGameServerNetworkingUtils
{
    public static bool getIPv4Address(CSteamID steamIDRemote, out uint address)
    {
        if (SteamGameServerNetworking.GetP2PSessionState(steamIDRemote, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            address = pConnectionState.m_nRemoteIP;
            return true;
        }
        address = 0u;
        return false;
    }

    public static uint getIPv4AddressOrZero(CSteamID steamIDRemote)
    {
        getIPv4Address(steamIDRemote, out var address);
        return address;
    }
}
