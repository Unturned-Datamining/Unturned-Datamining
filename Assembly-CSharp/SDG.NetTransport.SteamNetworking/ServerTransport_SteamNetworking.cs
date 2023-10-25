using SDG.Unturned;
using Steamworks;

namespace SDG.NetTransport.SteamNetworking;

/// <summary>
/// SteamNetworking is deprecated.
/// </summary>
public class ServerTransport_SteamNetworking : TransportBase_SteamNetworking, IServerTransport
{
    private Callback<P2PSessionRequest_t> p2pSessionRequest;

    public void Initialize(ServerTransportConnectionFailureCallback connectionClosedCallback)
    {
        p2pSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
    }

    public void TearDown()
    {
        p2pSessionRequest.Dispose();
    }

    public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
    {
        transportConnection = null;
        size = 0L;
        int nChannel = 0;
        if (!SteamGameServerNetworking.ReadP2PPacket(buffer, (uint)buffer.Length, out var pcubMsgSize, out var psteamIDRemote, nChannel))
        {
            return false;
        }
        if (pcubMsgSize > buffer.Length)
        {
            pcubMsgSize = (uint)buffer.Length;
        }
        size = pcubMsgSize;
        transportConnection = new TransportConnection_SteamNetworking(psteamIDRemote);
        return true;
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t callback)
    {
        CSteamID steamIDRemote = callback.m_steamIDRemote;
        if (!SDG.Unturned.Provider.shouldNetIgnoreSteamId(steamIDRemote) && steamIDRemote.BIndividualAccount())
        {
            SteamGameServerNetworking.AcceptP2PSessionWithUser(steamIDRemote);
        }
    }
}
