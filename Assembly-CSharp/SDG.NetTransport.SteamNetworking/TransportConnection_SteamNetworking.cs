using System;
using System.Net;
using SDG.Unturned;
using Steamworks;

namespace SDG.NetTransport.SteamNetworking;

internal struct TransportConnection_SteamNetworking : ITransportConnection, IEquatable<ITransportConnection>
{
    public CSteamID steamId;

    public TransportConnection_SteamNetworking(CSteamID steamId)
    {
        this.steamId = steamId;
    }

    public bool TryGetIPv4Address(out uint address)
    {
        if (SteamGameServerNetworking.GetP2PSessionState(steamId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            address = pConnectionState.m_nRemoteIP;
            return true;
        }
        address = 0u;
        return false;
    }

    public bool TryGetPort(out ushort port)
    {
        if (SteamGameServerNetworking.GetP2PSessionState(steamId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            port = pConnectionState.m_nRemotePort;
            return true;
        }
        port = 0;
        return false;
    }

    public bool TryGetSteamId(out ulong steamId)
    {
        steamId = this.steamId.m_SteamID;
        return steamId != 0;
    }

    public IPAddress GetAddress()
    {
        if (SteamGameServerNetworking.GetP2PSessionState(steamId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            return new IPAddress(pConnectionState.m_nRemoteIP);
        }
        return null;
    }

    public string GetAddressString(bool withPort)
    {
        if (SteamGameServerNetworking.GetP2PSessionState(steamId, out var pConnectionState) && pConnectionState.m_bUsingRelay == 0)
        {
            string text = Parser.getIPFromUInt32(pConnectionState.m_nRemoteIP);
            if (withPort)
            {
                text += ":";
                text += pConnectionState.m_nRemotePort;
            }
            return text;
        }
        return null;
    }

    public void CloseConnection()
    {
        SteamGameServerNetworking.CloseP2PSessionWithUser(steamId);
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        if (!SDG.Unturned.Provider.shouldNetIgnoreSteamId(steamId))
        {
            SteamGameServerNetworking.SendP2PPacket(steamId, buffer, (uint)size, reliability switch
            {
                ENetReliability.Reliable => EP2PSend.k_EP2PSendReliableWithBuffering, 
                _ => EP2PSend.k_EP2PSendUnreliable, 
            });
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is TransportConnection_SteamNetworking)
        {
            return steamId == ((TransportConnection_SteamNetworking)obj).steamId;
        }
        return false;
    }

    public bool Equals(TransportConnection_SteamNetworking other)
    {
        return steamId == other.steamId;
    }

    public bool Equals(ITransportConnection other)
    {
        if (other is TransportConnection_SteamNetworking)
        {
            return steamId == ((TransportConnection_SteamNetworking)other).steamId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return steamId.GetHashCode();
    }

    public override string ToString()
    {
        return steamId.ToString();
    }

    public static implicit operator CSteamID(TransportConnection_SteamNetworking clientId)
    {
        return clientId.steamId;
    }

    public static bool operator ==(TransportConnection_SteamNetworking lhs, TransportConnection_SteamNetworking rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(TransportConnection_SteamNetworking lhs, TransportConnection_SteamNetworking rhs)
    {
        return !(lhs == rhs);
    }
}
