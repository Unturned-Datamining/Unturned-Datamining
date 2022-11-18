using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SDG.Framework.Utilities;
using SDG.Unturned;

namespace SDG.NetTransport.SystemSockets;

public class ServerTransport_SystemSockets : TransportBase_SystemSockets, IServerTransport
{
    internal struct PendingMessage
    {
        public TransportConnection_SystemSocket transportConnection;

        public byte[] buffer;
    }

    private Socket listenSocket;

    private List<TransportConnection_SystemSocket> connections = new List<TransportConnection_SystemSocket>();

    private Queue<PendingMessage> messages = new Queue<PendingMessage>();

    public void Initialize(ServerTransportConnectionFailureCallback connectionClosedCallback)
    {
        int serverConnectionPort = SDG.Unturned.Provider.GetServerConnectionPort();
        if (!IPAddress.TryParse(SDG.Unturned.Provider.bindAddress, out var address))
        {
            address = IPAddress.Any;
        }
        IPEndPoint localEP = new IPEndPoint(address, serverConnectionPort);
        listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Blocking = false;
        listenSocket.Bind(localEP);
        listenSocket.Listen(10);
        TimeUtility.updated += OnUpdate;
    }

    public void TearDown()
    {
        TimeUtility.updated -= OnUpdate;
        listenSocket.Close();
        listenSocket = null;
        foreach (TransportConnection_SystemSocket connection in connections)
        {
            connection.clientSocket.Close();
        }
        connections.Clear();
    }

    public bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection)
    {
        if (listenSocket == null)
        {
            size = 0L;
            transportConnection = null;
            return false;
        }
        if (messages.Count > 0)
        {
            PendingMessage pendingMessage = messages.Dequeue();
            pendingMessage.buffer.CopyTo(buffer, 0);
            size = pendingMessage.buffer.Length;
            transportConnection = pendingMessage.transportConnection;
            return true;
        }
        transportConnection = null;
        size = 0L;
        return false;
    }

    internal void CloseConnection(TransportConnection_SystemSocket connection)
    {
        connection.clientSocket.Close();
        connections.RemoveFast(connection);
    }

    private void OnUpdate()
    {
        foreach (TransportConnection_SystemSocket connection in connections)
        {
            connection.messageQueue.ReceiveMessages(connection.clientSocket);
            byte[] buffer;
            while (connection.messageQueue.DequeueMessage(out buffer))
            {
                PendingMessage item = default(PendingMessage);
                item.transportConnection = connection;
                item.buffer = buffer;
                messages.Enqueue(item);
            }
        }
        if (SDG.Unturned.Provider.hasRoomForNewConnection)
        {
            try
            {
                Socket socket = listenSocket.Accept();
                socket.Blocking = false;
                TransportConnection_SystemSocket item2 = new TransportConnection_SystemSocket(this, socket);
                connections.Add(item2);
            }
            catch
            {
            }
        }
    }
}
