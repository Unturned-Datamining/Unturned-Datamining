using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ServerMessageHandler_PingRequest
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        for (int i = 0; i < Provider.pending.Count; i++)
        {
            if (transportConnection.Equals(Provider.pending[i].transportConnection))
            {
                if (!(Provider.pending[i].averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2f))
                {
                    Provider.pending[i].lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
                    Provider.pending[i].incrementNumPingRequestsReceived();
                    NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, delegate
                    {
                    });
                }
                return;
            }
        }
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer != null && !(steamPlayer.averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2f))
        {
            steamPlayer.lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
            steamPlayer.incrementNumPingRequestsReceived();
            NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, delegate
            {
            });
        }
    }
}
