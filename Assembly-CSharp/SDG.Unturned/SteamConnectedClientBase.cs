using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Some new code common to SteamPending and SteamPlayer.
/// </summary>
public class SteamConnectedClientBase
{
    /// <summary>
    /// Realtime the first ping request was received.
    /// </summary>
    private float firstPingRequestRealtime;

    /// <summary>
    /// Number of ping requests the server has received from this client.
    /// </summary>
    public int numPingRequestsReceived { get; private set; }

    /// <summary>
    /// Realtime passed since the first ping request was received from this client.
    /// </summary>
    public float realtimeSinceFirstPingRequest => Time.realtimeSinceStartup - firstPingRequestRealtime;

    /// <summary>
    /// Average number of ping requests received from this client per second.
    /// Begins tracking 10 seconds after the first ping request was received, or -1 if average is unknown yet.
    /// </summary>
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

    /// <summary>
    /// Only set on server. Associates player with their connection.
    /// </summary>
    public ITransportConnection transportConnection { get; protected set; }

    /// <summary>
    /// Called when a ping request is received from this client.
    /// </summary>
    public void incrementNumPingRequestsReceived()
    {
        if (numPingRequestsReceived == 0)
        {
            firstPingRequestRealtime = Time.realtimeSinceStartup;
        }
        numPingRequestsReceived++;
    }
}
