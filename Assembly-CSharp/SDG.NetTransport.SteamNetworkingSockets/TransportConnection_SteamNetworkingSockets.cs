using System;
using System.Net;
using Steamworks;

namespace SDG.NetTransport.SteamNetworkingSockets;

/// <summary>
/// Implementing as a struct wrapping the connection handle would remove the cost of looking up the connection,
/// but implementing as a class makes it cheap to cache information like the remote identity.
/// </summary>
internal class TransportConnection_SteamNetworkingSockets : ITransportConnection, IEquatable<ITransportConnection>
{
    internal bool wasClosed;

    internal HSteamNetConnection steamConnectionHandle;

    internal SteamNetworkingIdentity steamIdentity;

    private ServerTransport_SteamNetworkingSockets serverTransport;

    public TransportConnection_SteamNetworkingSockets(ServerTransport_SteamNetworkingSockets serverTransport, ref SteamNetConnectionStatusChangedCallback_t callback)
    {
        this.serverTransport = serverTransport;
        steamConnectionHandle = callback.m_hConn;
        steamIdentity = callback.m_info.m_identityRemote;
    }

    public bool TryGetIPv4Address(out uint address)
    {
        if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out var pInfo))
        {
            address = pInfo.m_addrRemote.GetIPv4();
            return address != 0;
        }
        address = 0u;
        return false;
    }

    public bool TryGetPort(out ushort port)
    {
        if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out var pInfo))
        {
            port = pInfo.m_addrRemote.m_port;
            return port > 0;
        }
        port = 0;
        return false;
    }

    public IPAddress GetAddress()
    {
        if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out var pInfo))
        {
            return new IPAddress(pInfo.m_addrRemote.m_ipv6);
        }
        return null;
    }

    public string GetAddressString(bool withPort)
    {
        if (SteamGameServerNetworkingSockets.GetConnectionInfo(steamConnectionHandle, out var pInfo))
        {
            pInfo.m_addrRemote.ToString(out var buf, withPort);
            return buf;
        }
        return null;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TransportConnection_SteamNetworkingSockets);
    }

    public bool Equals(TransportConnection_SteamNetworkingSockets other)
    {
        if (other != null)
        {
            return steamConnectionHandle == other.steamConnectionHandle;
        }
        return false;
    }

    public bool Equals(ITransportConnection other)
    {
        return Equals(other as TransportConnection_SteamNetworkingSockets);
    }

    public override int GetHashCode()
    {
        return steamConnectionHandle.GetHashCode();
    }

    public override string ToString()
    {
        return serverTransport.IdentityToString(steamIdentity);
    }

    public void CloseConnection()
    {
        serverTransport.CloseConnection(this);
    }

    public unsafe void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        int nSendFlags = serverTransport.ReliabilityToSendFlags(reliability);
        fixed (byte* value = buffer)
        {
            SteamGameServerNetworkingSockets.SendMessageToConnection(pData: new IntPtr(value), hConn: steamConnectionHandle, cbData: (uint)size, nSendFlags: nSendFlags, pOutMessageNumber: out var _);
        }
    }
}
