using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public sealed class ServerInstanceMethod : ServerInstanceMethodBase
{
    public static ServerInstanceMethod Get(Type declaringType, string methodName)
    {
        ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
        if (serverMethodInfo != null)
        {
            return new ServerInstanceMethod(serverMethodInfo);
        }
        return null;
    }

    public void Invoke(NetId netId, ENetReliability reliability)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        callback(writerWithInstanceHeader);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo)
        : base(serverMethodInfo)
    {
    }
}
public sealed class ServerInstanceMethod<T> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T arg);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T arg)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3, T4> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3, T4>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3, T4, T5>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3, T4, T5, T6>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ServerInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private WriteDelegate generatedWrite;

    public static ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfLocal(reliability, writerWithInstanceHeader);
    }

    private ServerInstanceMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
