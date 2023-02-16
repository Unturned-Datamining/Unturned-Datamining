using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class NetMessages
{
    public delegate void ClientWriteHandler(NetPakWriter writer);

    public delegate void ClientReadHandler(NetPakReader reader);

    public delegate void ServerReadHandler(ITransportConnection transportConnection, NetPakReader reader);

    private static NetPakReader reader;

    private static NetPakWriter writer;

    private static ClientReadHandler[] clientReadCallbacks;

    private static ServerReadHandler[] serverReadCallbacks;

    public static void SendMessageToClient(EClientMessage index, ENetReliability reliability, ITransportConnection transportConnection, ClientWriteHandler callback)
    {
        writer.Reset();
        writer.WriteEnum(index);
        callback(writer);
        writer.Flush();
        transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
    }

    public static void SendMessageToClients(EClientMessage index, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, ClientWriteHandler callback)
    {
        writer.Reset();
        writer.WriteEnum(index);
        callback(writer);
        writer.Flush();
        foreach (ITransportConnection transportConnection in transportConnections)
        {
            transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
        }
    }

    public static void SendMessageToServer(EServerMessage index, ENetReliability reliability, ClientWriteHandler callback)
    {
        if (!Provider.isConnected)
        {
            UnturnedLog.warn($"Ignoring request to send message {index} to server because we are not connected");
            return;
        }
        writer.Reset();
        writer.WriteEnum(index);
        callback(writer);
        writer.Flush();
        Provider.clientTransport.Send(writer.buffer, writer.writeByteIndex, reliability);
    }

    public static void ReceiveMessageFromClient(ITransportConnection transportConnection, byte[] packet, int offset, int size)
    {
        reader.SetBufferSegment(packet, size);
        reader.Reset();
        if (!reader.ReadEnum(out var value))
        {
            UnturnedLog.warn("Received invalid packet index from {0}, so we're refusing them", transportConnection);
            Provider.refuseGarbageConnection(transportConnection, "sv invalid packet index");
            return;
        }
        try
        {
            serverReadCallbacks[(int)value]?.Invoke(transportConnection, reader);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception reading message {0} from client {1}:", value, transportConnection);
        }
    }

    public static void ReceiveMessageFromServer(byte[] packet, int offset, int size)
    {
        reader.SetBufferSegment(packet, size);
        reader.Reset();
        if (!reader.ReadEnum(out var value))
        {
            UnturnedLog.error("Client received invalid message index from server");
            return;
        }
        try
        {
            if ((uint)value <= 1u)
            {
                reader.AlignToByte();
                Provider.legacyReceiveClient(packet, offset, size);
            }
            else
            {
                Provider.timeLastPacketWasReceivedFromServer = Time.realtimeSinceStartup;
                clientReadCallbacks[(int)value]?.Invoke(reader);
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception reading message {0} from server:", value);
        }
    }

    static NetMessages()
    {
        reader = new NetPakReader();
        writer = new NetPakWriter();
        writer.buffer = Block.buffer;
        clientReadCallbacks = new ClientReadHandler[18];
        clientReadCallbacks[2] = ClientMessageHandler_PingRequest.ReadMessage;
        clientReadCallbacks[3] = ClientMessageHandler_PingResponse.ReadMessage;
        clientReadCallbacks[4] = ClientMessageHandler_Shutdown.ReadMessage;
        clientReadCallbacks[5] = ClientMessageHandler_PlayerConnected.ReadMessage;
        clientReadCallbacks[6] = ClientMessageHandler_PlayerDisconnected.ReadMessage;
        clientReadCallbacks[7] = ClientMessageHandler_DownloadWorkshopFiles.ReadMessage;
        clientReadCallbacks[8] = ClientMessageHandler_Verify.ReadMessage;
        clientReadCallbacks[9] = ClientMessageHandler_Accepted.ReadMessage;
        clientReadCallbacks[10] = ClientMessageHandler_Rejected.ReadMessage;
        clientReadCallbacks[11] = ClientMessageHandler_Banned.ReadMessage;
        clientReadCallbacks[12] = ClientMessageHandler_Kicked.ReadMessage;
        clientReadCallbacks[13] = ClientMessageHandler_Admined.ReadMessage;
        clientReadCallbacks[14] = ClientMessageHandler_Unadmined.ReadMessage;
        clientReadCallbacks[15] = ClientMessageHandler_BattlEye.ReadMessage;
        clientReadCallbacks[16] = ClientMessageHandler_QueuePositionChanged.ReadMessage;
        clientReadCallbacks[17] = ClientMessageHandler_InvokeMethod.ReadMessage;
        serverReadCallbacks = new ServerReadHandler[9];
        serverReadCallbacks[0] = ServerMessageHandler_GetWorkshopFiles.ReadMessage;
        serverReadCallbacks[1] = ServerMessageHandler_ReadyToConnect.ReadMessage;
        serverReadCallbacks[2] = ServerMessageHandler_Authenticate.ReadMessage;
        serverReadCallbacks[3] = ServerMessageHandler_BattlEye.ReadMessage;
        serverReadCallbacks[4] = ServerMessageHandler_PingRequest.ReadMessage;
        serverReadCallbacks[5] = ServerMessageHandler_PingResponse.ReadMessage;
        serverReadCallbacks[6] = ServerMessageHandler_InvokeMethod.ReadMessage;
        serverReadCallbacks[7] = ServerMessageHandler_ValidateAssets.ReadMessage;
        serverReadCallbacks[8] = ServerMessageHandler_GracefullyDisconnect.ReadMessage;
    }

    internal static NetPakReader GetInvokableReader()
    {
        return reader;
    }

    internal static NetPakWriter GetInvokableWriter()
    {
        return writer;
    }
}
