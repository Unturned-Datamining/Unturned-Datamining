using System;
using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Banned
{
    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadString(out var value);
        reader.ReadUInt32(out var value2);
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.BANNED;
        Provider._connectionFailureReason = value;
        Provider._connectionFailureDuration = value2;
        Provider.RequestDisconnect($"Banned from server. Reason: \"{value}\" Duration: {TimeSpan.FromSeconds(value2)}");
    }
}
