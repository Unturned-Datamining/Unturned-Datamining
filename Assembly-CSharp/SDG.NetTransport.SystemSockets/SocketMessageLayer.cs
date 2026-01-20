using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets;

/// <summary>
/// Implements message boundaries on top of a TCP stream socket.
/// </summary>
internal class SocketMessageLayer
{
    private static byte[] sizeBuffer = new byte[2];

    private static byte[] internalBuffer = new byte[1200];

    private Queue<byte[]> messageQueue = new Queue<byte[]>();

    private byte[] pendingMessage;

    private int pendingMessageTotalSize;

    private int pendingMessageSizeParts;

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
        if (socket.Available < 1)
        {
            return;
        }
        SocketError errorCode;
        int num = socket.Receive(internalBuffer, 0, internalBuffer.Length, SocketFlags.None, out errorCode);
        if (errorCode == SocketError.WouldBlock || errorCode != 0 || num < 1)
        {
            return;
        }
        int num2 = 0;
        while (num2 < num)
        {
            if (pendingMessage == null)
            {
                if (pendingMessageSizeParts < 2)
                {
                    switch (pendingMessageSizeParts)
                    {
                    case 0:
                        pendingMessageTotalSize += internalBuffer[num2] << 8;
                        break;
                    case 1:
                        pendingMessageTotalSize += internalBuffer[num2];
                        break;
                    }
                    pendingMessageSizeParts++;
                    num2++;
                }
                else
                {
                    pendingMessage = new byte[pendingMessageTotalSize];
                    pendingMessageOffset = 0;
                }
            }
            else
            {
                int num3 = num - num2;
                int num4 = pendingMessage.Length - pendingMessageOffset;
                if (num3 < num4)
                {
                    Array.Copy(internalBuffer, num2, pendingMessage, pendingMessageOffset, num3);
                    pendingMessageOffset += num3;
                    num2 += num3;
                    continue;
                }
                Array.Copy(internalBuffer, num2, pendingMessage, pendingMessageOffset, num4);
                num2 += num4;
                messageQueue.Enqueue(pendingMessage);
                pendingMessage = null;
                pendingMessageTotalSize = 0;
                pendingMessageSizeParts = 0;
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
