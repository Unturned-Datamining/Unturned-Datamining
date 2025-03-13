using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ServerMessageHandler_PingRequest
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPending steamPending = Provider.findPendingPlayer(transportConnection);
        if (steamPending != null)
        {
            if (steamPending.averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2f)
            {
                if ((bool)NetMessages.shouldLogBadMessages)
                {
                    UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because they exceeded rate limit");
                }
                Provider.IncrementBadPacketsFromConnection(transportConnection);
            }
            else
            {
                steamPending.lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
                steamPending.incrementNumPingRequestsReceived();
                NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, delegate
                {
                });
            }
            return;
        }
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer != null)
        {
            if (steamPlayer.averagePingRequestsReceivedPerSecond > Provider.PING_REQUEST_INTERVAL * 2f)
            {
                if ((bool)NetMessages.shouldLogBadMessages)
                {
                    UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because they exceeded rate limit");
                }
                Provider.IncrementBadPacketsFromConnection(transportConnection);
            }
            else
            {
                steamPlayer.lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
                steamPlayer.incrementNumPingRequestsReceived();
                NetMessages.SendMessageToClient(EClientMessage.PingResponse, ENetReliability.Unreliable, transportConnection, delegate
                {
                });
            }
        }
        else
        {
            if ((bool)NetMessages.shouldLogBadMessages)
            {
                UnturnedLog.info($"Ignoring PingRequest message from {transportConnection} because there is no associated player");
            }
            Provider.IncrementBadPacketsFromConnection(transportConnection);
        }
    }
}
