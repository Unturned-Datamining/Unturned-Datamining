using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ClientMessageHandler_PingRequest
{
    internal static void ReadMessage(NetPakReader reader)
    {
        NetMessages.SendMessageToServer(EServerMessage.PingResponse, ENetReliability.Unreliable, delegate
        {
        });
    }
}
