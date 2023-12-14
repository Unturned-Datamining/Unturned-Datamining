using System.Net;
using System.Net.Sockets;
using SDG.Framework.Utilities;
using SDG.Unturned;
using Unturned.SystemEx;

namespace SDG.NetTransport.SystemSockets;

/// <summary>
/// Implementation using .NET Berkeley sockets.
/// </summary>
public class ClientTransport_SystemSockets : TransportBase_SystemSockets, IClientTransport
{
    private Socket socket;

    private SocketMessageLayer messageQueue;

    private uint remoteAddress;

    private ushort remotePort;

    public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
    {
        uint value = SDG.Unturned.Provider.CurrentServerConnectParameters.address.value;
        long address = ((value & 0xFF) << 24) | (((value >> 8) & 0xFF) << 16) | (((value >> 16) & 0xFF) << 8) | ((value >> 24) & 0xFF);
        int connectionPort = SDG.Unturned.Provider.CurrentServerConnectParameters.connectionPort;
        remoteAddress = value;
        remotePort = SDG.Unturned.Provider.CurrentServerConnectParameters.connectionPort;
        IPEndPoint remoteEP = new IPEndPoint(address, connectionPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(remoteEP);
        socket.Blocking = false;
        messageQueue = new SocketMessageLayer();
        TimeUtility.updated += OnUpdate;
        callback();
    }

    public void TearDown()
    {
        TimeUtility.updated -= OnUpdate;
        socket.Close();
        socket = null;
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        if (socket != null)
        {
            messageQueue.SendMessage(socket, buffer, (int)size);
        }
    }

    public bool Receive(byte[] buffer, out long size)
    {
        if (socket == null)
        {
            size = 0L;
            return false;
        }
        if (messageQueue.DequeueMessage(out var buffer2))
        {
            buffer2.CopyTo(buffer, 0);
            size = buffer2.Length;
            return true;
        }
        size = 0L;
        return false;
    }

    public bool TryGetIPv4Address(out IPv4Address address)
    {
        address = new IPv4Address(remoteAddress);
        return true;
    }

    public bool TryGetConnectionPort(out ushort connectionPort)
    {
        connectionPort = remotePort;
        return true;
    }

    public bool TryGetQueryPort(out ushort queryPort)
    {
        queryPort = MathfEx.ClampToUShort(remotePort - 1);
        return true;
    }

    private void OnUpdate()
    {
        messageQueue.ReceiveMessages(socket);
    }
}
