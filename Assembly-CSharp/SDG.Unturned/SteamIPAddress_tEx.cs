using Steamworks;

namespace SDG.Unturned;

public static class SteamIPAddress_tEx
{
    public static bool TryGetIPv4Address(this SteamIPAddress_t steamIPAddress, out uint address)
    {
        if (steamIPAddress.GetIPType() == ESteamIPType.k_ESteamIPTypeIPv4)
        {
            byte[] addressBytes = steamIPAddress.ToIPAddress().GetAddressBytes();
            address = (uint)((addressBytes[0] << 24) | (addressBytes[1] << 16) | (addressBytes[2] << 8) | addressBytes[3]);
            return true;
        }
        address = 0u;
        return false;
    }
}
