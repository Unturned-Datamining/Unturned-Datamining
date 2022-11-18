using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ServerMessageHandler_PingResponse
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer != null && steamPlayer.timeLastPingRequestWasSentToClient > 0f)
        {
            float deltaTime = Time.deltaTime;
            steamPlayer.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
            steamPlayer.lag(Time.realtimeSinceStartup - steamPlayer.timeLastPingRequestWasSentToClient - deltaTime);
            steamPlayer.timeLastPingRequestWasSentToClient = -1f;
        }
    }
}
