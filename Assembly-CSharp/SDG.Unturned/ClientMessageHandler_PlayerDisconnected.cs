using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_PlayerDisconnected
{
    internal static void ReadMessage(NetPakReader reader)
    {
        if (reader.ReadUInt8(out var value))
        {
            Provider.removePlayer(value);
        }
    }
}
