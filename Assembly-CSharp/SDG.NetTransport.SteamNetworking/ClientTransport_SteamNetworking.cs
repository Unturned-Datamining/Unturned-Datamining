using SDG.Unturned;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.NetTransport.SteamNetworking;

/// <summary>
/// SteamNetworking is deprecated.
/// </summary>
public class ClientTransport_SteamNetworking : TransportBase_SteamNetworking, IClientTransport
{
    private static Callback<P2PSessionRequest_t> p2pSessionRequest;

    private CSteamID serverId => SDG.Unturned.Provider.server;

    public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
    {
        p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        callback();
    }

    public void TearDown()
    {
        p2pSessionRequest.Dispose();
        Steamworks.SteamNetworking.CloseP2PSessionWithUser(serverId);
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        Steamworks.SteamNetworking.SendP2PPacket(serverId, buffer, (uint)size, reliability switch
        {
            ENetReliability.Reliable => EP2PSend.k_EP2PSendReliableWithBuffering, 
            _ => EP2PSend.k_EP2PSendUnreliable, 
        });
    }

    public bool Receive(byte[] buffer, out long size)
    {
        size = 0L;
        int nChannel = 0;
        if (!Steamworks.SteamNetworking.ReadP2PPacket(buffer, (uint)buffer.Length, out var pcubMsgSize, out var psteamIDRemote, nChannel))
        {
            return false;
        }
        if (psteamIDRemote != serverId)
        {
            return false;
        }
        size = pcubMsgSize;
        return true;
    }

    public bool TryGetIPv4Address(out IPv4Address address)
    {
        if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            address = new IPv4Address(pConnectionState.m_nRemoteIP);
            return address.value != 0;
        }
        address = IPv4Address.Zero;
        return false;
    }

    public bool TryGetConnectionPort(out ushort connectionPort)
    {
        if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            connectionPort = pConnectionState.m_nRemotePort;
            return connectionPort > 0;
        }
        connectionPort = 0;
        return false;
    }

    public bool TryGetQueryPort(out ushort queryPort)
    {
        if (Steamworks.SteamNetworking.GetP2PSessionState(serverId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            queryPort = MathfEx.ClampToUShort(pConnectionState.m_nRemotePort - 1);
            return queryPort > 0;
        }
        queryPort = 0;
        return false;
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t callback)
    {
        if (callback.m_steamIDRemote == serverId)
        {
            Steamworks.SteamNetworking.AcceptP2PSessionWithUser(serverId);
        }
    }
}
