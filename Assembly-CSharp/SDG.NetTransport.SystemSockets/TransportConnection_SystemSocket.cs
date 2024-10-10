using System;
using System.Net;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets;

internal class TransportConnection_SystemSocket : ITransportConnection, IEquatable<ITransportConnection>
{
    public ServerTransport_SystemSockets serverTransport;

    public Socket clientSocket;

    public SocketMessageLayer messageQueue = new SocketMessageLayer();

    public TransportConnection_SystemSocket(ServerTransport_SystemSockets serverTransport, Socket clientSocket)
    {
        this.serverTransport = serverTransport;
        this.clientSocket = clientSocket;
    }

    public bool TryGetIPv4Address(out uint address)
    {
        if (clientSocket.RemoteEndPoint is IPEndPoint iPEndPoint)
        {
            byte[] addressBytes = iPEndPoint.Address.GetAddressBytes();
            if (addressBytes.Length == 4)
            {
                address = ((uint)(addressBytes[0] << 24) & 0xFFu) | ((uint)(addressBytes[0] << 16) & 0xFFu) | ((uint)(addressBytes[0] << 8) & 0xFFu) | (addressBytes[0] & 0xFFu);
                return true;
            }
        }
        address = 0u;
        return false;
    }

    public bool TryGetPort(out ushort port)
    {
        if (clientSocket.RemoteEndPoint is IPEndPoint iPEndPoint)
        {
            port = (ushort)iPEndPoint.Port;
            return true;
        }
        port = 0;
        return false;
    }

    public bool TryGetSteamId(out ulong steamId)
    {
        steamId = 0uL;
        return false;
    }

    public IPAddress GetAddress()
    {
        if (clientSocket.RemoteEndPoint is IPEndPoint iPEndPoint)
        {
            return iPEndPoint.Address;
        }
        return null;
    }

    public string GetAddressString(bool withPort)
    {
        if (clientSocket.RemoteEndPoint is IPEndPoint iPEndPoint)
        {
            string text = iPEndPoint.Address.ToString();
            if (withPort)
            {
                text += ":";
                text += iPEndPoint.Port;
            }
            return text;
        }
        return null;
    }

    public void CloseConnection()
    {
        serverTransport.CloseConnection(this);
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        messageQueue.SendMessage(clientSocket, buffer, (int)size);
    }

    bool IEquatable<ITransportConnection>.Equals(ITransportConnection other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return clientSocket.GetHashCode();
    }

    public override string ToString()
    {
        if (clientSocket.RemoteEndPoint == null)
        {
            return "Invalid Socket";
        }
        return clientSocket.RemoteEndPoint.ToString();
    }
}
