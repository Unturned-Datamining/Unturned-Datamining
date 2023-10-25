using Steamworks;

namespace SDG.Unturned;

public static class SteamIPAddress_tEx
{
    /// <summary>
    /// Steam APIs returned uint32 IPv4 addresses in the past, so Unturned code depends on them in some places.
    /// Ideally these uses should be updated for IPv6 support going forward.
    /// For the meantime this method converts from the new format to the old format for backwards compatibility.
    /// </summary>
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
