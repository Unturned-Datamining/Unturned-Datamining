using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public sealed class ClientInstanceMethod : ClientInstanceMethodBase
{
    public static ClientInstanceMethod Get(Type declaringType, string methodName)
    {
        ClientMethodInfo clientMethodInfo = NetReflection.GetClientMethodInfo(declaringType, methodName);
        if (clientMethodInfo != null)
        {
            return new ClientInstanceMethod(clientMethodInfo);
        }
        return null;
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        callback(writerWithInstanceHeader);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        callback(writerWithInstanceHeader);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        callback(writerWithInstanceHeader);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo)
        : base(clientMethodInfo)
    {
    }
}
public sealed class ClientInstanceMethod<T> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T arg);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T arg)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ClientInstanceMethodBase
{
    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

    private WriteDelegate generatedWrite;

    public static ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientInstanceMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithInstanceHeader);
    }

    public void Invoke(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithInstanceHeader);
    }

    public void InvokeAndLoopback(NetId netId, ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithInstanceHeader = GetWriterWithInstanceHeader(netId);
        generatedWrite(writerWithInstanceHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopback(reliability, transportConnections, writerWithInstanceHeader);
    }

    private ClientInstanceMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
