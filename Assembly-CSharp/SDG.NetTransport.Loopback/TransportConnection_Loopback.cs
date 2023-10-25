using System;
using System.Net;

namespace SDG.NetTransport.Loopback;

/// <summary>
/// Dummy connection used in singleplayer.
/// </summary>
public struct TransportConnection_Loopback : ITransportConnection, IEquatable<ITransportConnection>
{
    public static readonly ITransportConnection DedicatedServer = DedicatedServerLoopback;

    private int id;

    private static int counter;

    private static readonly TransportConnection_Loopback DedicatedServerLoopback = Create();

    public static TransportConnection_Loopback Create()
    {
        return new TransportConnection_Loopback(++counter);
    }

    public bool TryGetIPv4Address(out uint address)
    {
        address = 0u;
        return false;
    }

    public bool TryGetPort(out ushort port)
    {
        port = 0;
        return false;
    }

    public IPAddress GetAddress()
    {
        return null;
    }

    public string GetAddressString(bool withPort)
    {
        return null;
    }

    public void CloseConnection()
    {
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        throw new NotSupportedException();
    }

    public override bool Equals(object obj)
    {
        if (obj is TransportConnection_Loopback)
        {
            return id == ((TransportConnection_Loopback)obj).id;
        }
        return false;
    }

    public bool Equals(TransportConnection_Loopback other)
    {
        return id == other.id;
    }

    public bool Equals(ITransportConnection other)
    {
        if (other is TransportConnection_Loopback)
        {
            return id == ((TransportConnection_Loopback)other).id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override string ToString()
    {
        if (this == DedicatedServerLoopback)
        {
            return "DedicatedServerLoopback";
        }
        return $"Loopback_{id.ToString()}";
    }

    public static bool operator ==(TransportConnection_Loopback lhs, TransportConnection_Loopback rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(TransportConnection_Loopback lhs, TransportConnection_Loopback rhs)
    {
        return !(lhs == rhs);
    }

    private TransportConnection_Loopback(int id)
    {
        this.id = id;
    }
}
