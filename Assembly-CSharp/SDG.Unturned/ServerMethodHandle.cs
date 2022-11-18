using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public abstract class ServerMethodHandle
{
    protected ServerMethodInfo serverMethodInfo;

    public override string ToString()
    {
        if (serverMethodInfo != null)
        {
            return serverMethodInfo.ToString();
        }
        return "invalid";
    }

    protected static THandle GetInternal<THandle, TWriteDelegate>(Type declaringType, string methodName, Func<ServerMethodInfo, TWriteDelegate, THandle> makeHandle) where THandle : ServerMethodHandle where TWriteDelegate : Delegate
    {
        ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
        if (serverMethodInfo != null)
        {
            TWriteDelegate val = NetReflection.CreateServerWriteDelegate<TWriteDelegate>(serverMethodInfo);
            if ((Delegate)val != (Delegate)null)
            {
                return makeHandle(serverMethodInfo, val);
            }
        }
        return null;
    }

    protected NetPakWriter GetWriterWithStaticHeader()
    {
        NetPakWriter invokableWriter = NetMessages.GetInvokableWriter();
        invokableWriter.Reset();
        invokableWriter.WriteEnum(EServerMessage.InvokeMethod);
        invokableWriter.WriteBits(serverMethodInfo.methodIndex, NetReflection.serverMethodsBitCount);
        return invokableWriter;
    }

    protected void SendAndLoopbackIfLocal(ENetReliability reliability, NetPakWriter writer)
    {
        writer.Flush();
        InvokeLoopback(writer);
    }

    protected ServerMethodHandle(ServerMethodInfo serverMethodInfo)
    {
        this.serverMethodInfo = serverMethodInfo;
    }

    private void InvokeLoopback(NetPakWriter writer)
    {
        NetPakReader invokableReader = NetMessages.GetInvokableReader();
        invokableReader.SetBufferSegmentCopy(writer.buffer, Provider.buffer, writer.writeByteIndex);
        invokableReader.Reset();
        invokableReader.ReadEnum(out var _);
        invokableReader.ReadBits(NetReflection.serverMethodsBitCount, out var _);
        SteamPlayer callingPlayer = null;
        ServerInvocationContext context = new ServerInvocationContext(ServerInvocationContext.EOrigin.Loopback, callingPlayer, invokableReader, serverMethodInfo);
        try
        {
            serverMethodInfo.readMethod(in context);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception invoking {0} by server loopback:", serverMethodInfo);
        }
    }
}
