using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Shutdown
{
    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadString(out var value);
        Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SHUTDOWN;
        Provider._connectionFailureReason = value;
        Provider.RequestDisconnect("Server was shutdown --- Reason: \"" + value + "\"");
    }
}
