using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class SteamConnectedClientBase
{
    private float firstPingRequestRealtime;

    public int numPingRequestsReceived { get; private set; }

    public float realtimeSinceFirstPingRequest => Time.realtimeSinceStartup - firstPingRequestRealtime;

    public float averagePingRequestsReceivedPerSecond
    {
        get
        {
            if (numPingRequestsReceived < 1)
            {
                return -1f;
            }
            float num = realtimeSinceFirstPingRequest;
            if (num < 10f)
            {
                return -1f;
            }
            return (float)numPingRequestsReceived / num;
        }
    }

    public ITransportConnection transportConnection { get; protected set; }

    public void incrementNumPingRequestsReceived()
    {
        if (numPingRequestsReceived == 0)
        {
            firstPingRequestRealtime = Time.realtimeSinceStartup;
        }
        numPingRequestsReceived++;
    }
}
