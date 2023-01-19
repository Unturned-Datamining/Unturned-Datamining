using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public abstract class ClientMethodHandle
{
    protected ClientMethodInfo clientMethodInfo;

    public override string ToString()
    {
        if (clientMethodInfo != null)
        {
            return clientMethodInfo.ToString();
        }
        return "invalid";
    }

    protected static THandle GetInternal<THandle, TWriteDelegate>(Type declaringType, string methodName, Func<ClientMethodInfo, TWriteDelegate, THandle> makeHandle) where THandle : ClientMethodHandle where TWriteDelegate : Delegate
    {
        ClientMethodInfo clientMethodInfo = NetReflection.GetClientMethodInfo(declaringType, methodName);
        if (clientMethodInfo != null)
        {
            TWriteDelegate val = NetReflection.CreateClientWriteDelegate<TWriteDelegate>(clientMethodInfo);
            if ((Delegate)val != (Delegate)null)
            {
                return makeHandle(clientMethodInfo, val);
            }
        }
        return null;
    }

    protected NetPakWriter GetWriterWithStaticHeader()
    {
        NetPakWriter invokableWriter = NetMessages.GetInvokableWriter();
        invokableWriter.Reset();
        invokableWriter.WriteEnum(EClientMessage.InvokeMethod);
        invokableWriter.WriteBits(clientMethodInfo.methodIndex, NetReflection.clientMethodsBitCount);
        return invokableWriter;
    }

    protected void SendAndLoopbackIfLocal(ENetReliability reliability, ITransportConnection transportConnection, NetPakWriter writer)
    {
        writer.Flush();
        if (!Dedicator.IsDedicatedServer)
        {
            InvokeLoopback(writer);
        }
        else
        {
            transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
        }
    }

    protected void SendAndLoopbackIfAnyAreLocal(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, NetPakWriter writer)
    {
        writer.Flush();
        bool flag = false;
        foreach (ITransportConnection transportConnection in transportConnections)
        {
            if (!Dedicator.IsDedicatedServer)
            {
                flag = true;
                break;
            }
            transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
        }
        if (flag)
        {
            InvokeLoopback(writer);
        }
    }

    protected void SendAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, NetPakWriter writer)
    {
        writer.Flush();
        foreach (ITransportConnection transportConnection in transportConnections)
        {
            if (!Dedicator.IsDedicatedServer)
            {
                UnturnedLog.error("Local connection {0} passed to SendAndLoopback {1}", transportConnection, this);
                break;
            }
            transportConnection.Send(writer.buffer, writer.writeByteIndex, reliability);
        }
        InvokeLoopback(writer);
    }

    protected ClientMethodHandle(ClientMethodInfo clientMethodInfo)
    {
        this.clientMethodInfo = clientMethodInfo;
    }

    private void InvokeLoopback(NetPakWriter writer)
    {
        NetPakReader invokableReader = NetMessages.GetInvokableReader();
        invokableReader.SetBufferSegmentCopy(writer.buffer, Provider.buffer, writer.writeByteIndex);
        invokableReader.Reset();
        invokableReader.ReadEnum(out var _);
        invokableReader.ReadBits(NetReflection.clientMethodsBitCount, out var _);
        ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Loopback, invokableReader, clientMethodInfo);
        try
        {
            clientMethodInfo.readMethod(in context);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception invoking {0} by client loopback:", clientMethodInfo);
        }
    }
}
