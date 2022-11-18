using System.Net;
using System.Net.Sockets;
using SDG.Framework.Utilities;
using SDG.Unturned;

namespace SDG.NetTransport.SystemSockets;

public class ClientTransport_SystemSockets : TransportBase_SystemSockets, IClientTransport
{
    private Socket socket;

    private SocketMessageLayer messageQueue;

    public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
    {
        uint ip = SDG.Unturned.Provider.currentServerInfo.ip;
        long address = ((ip & 0xFF) << 24) | (((ip >> 8) & 0xFF) << 16) | (((ip >> 16) & 0xFF) << 8) | ((ip >> 24) & 0xFF);
        int connectionPort = SDG.Unturned.Provider.currentServerInfo.connectionPort;
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

    private void OnUpdate()
    {
        messageQueue.ReceiveMessages(socket);
    }
}
