using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Kicked
{
    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadString(out var value);
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
        Provider._connectionFailureReason = value;
        Provider.RequestDisconnect("Kicked from server. Reason: \"" + value + "\"");
    }
}
