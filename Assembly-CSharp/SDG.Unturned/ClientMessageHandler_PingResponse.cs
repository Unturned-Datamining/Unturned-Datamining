using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

internal static class ClientMessageHandler_PingResponse
{
    internal static void ReadMessage(NetPakReader reader)
    {
        if (Provider.timeLastPingRequestWasSentToServer > 0f)
        {
            float deltaTime = Time.deltaTime;
            Provider.lag(Time.realtimeSinceStartup - Provider.timeLastPingRequestWasSentToServer - deltaTime);
            Provider.timeLastPingRequestWasSentToServer = -1f;
        }
    }
}
