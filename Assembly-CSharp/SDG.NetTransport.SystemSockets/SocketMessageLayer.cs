using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets;

internal class SocketMessageLayer
{
    private static byte[] sizeBuffer = new byte[2];

    private static byte[] internalBuffer = new byte[1200];

    private Queue<byte[]> messageQueue = new Queue<byte[]>();

    private byte[] pendingMessage;

    private int pendingMessageOffset;

    public void SendMessage(Socket socket, byte[] buffer, int size)
    {
        sizeBuffer[0] = (byte)((uint)(size >> 8) & 0xFFu);
        sizeBuffer[1] = (byte)((uint)size & 0xFFu);
        socket.Send(sizeBuffer);
        socket.Send(buffer, 0, size, SocketFlags.None, out var _);
    }

    public void ReceiveMessages(Socket socket)
    {
        int num = ((pendingMessage != null) ? 1 : 2);
        if (socket.Available < num)
        {
            return;
        }
        SocketError errorCode;
        int num2 = socket.Receive(internalBuffer, 0, internalBuffer.Length, SocketFlags.None, out errorCode);
        if (errorCode == SocketError.WouldBlock || errorCode != 0 || num2 < 1)
        {
            return;
        }
        int num3 = 0;
        while (num3 < num2)
        {
            if (pendingMessage == null)
            {
                int num4 = (internalBuffer[num3] << 8) | internalBuffer[num3 + 1];
                pendingMessage = new byte[num4];
                pendingMessageOffset = 0;
                num3 += 2;
                continue;
            }
            int num5 = num2 - num3;
            int num6 = pendingMessage.Length - pendingMessageOffset;
            if (num5 < num6)
            {
                Array.Copy(internalBuffer, num3, pendingMessage, pendingMessageOffset, num5);
                pendingMessageOffset += num5;
                num3 += num5;
            }
            else
            {
                Array.Copy(internalBuffer, num3, pendingMessage, pendingMessageOffset, num6);
                num3 += num6;
                messageQueue.Enqueue(pendingMessage);
                pendingMessage = null;
            }
        }
    }

    public bool DequeueMessage(out byte[] buffer)
    {
        if (messageQueue.Count > 0)
        {
            buffer = messageQueue.Dequeue();
            return true;
        }
        buffer = null;
        return false;
    }
}
