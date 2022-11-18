using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public sealed class ClientStaticMethod : ClientMethodHandle
{
    public delegate void ReceiveDelegate();

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context);

    public static ClientStaticMethod Get(ReceiveDelegate action)
    {
        Type declaringType = action.Method.DeclaringType;
        string name = action.Method.Name;
        return Get(declaringType, name);
    }

    public static ClientStaticMethod Get(ReceiveDelegateWithContext action)
    {
        Type declaringType = action.Method.DeclaringType;
        string name = action.Method.Name;
        return Get(declaringType, name);
    }

    public static ClientStaticMethod Get(Type declaringType, string methodName)
    {
        ClientMethodInfo clientMethodInfo = NetReflection.GetClientMethodInfo(declaringType, methodName);
        if (clientMethodInfo != null)
        {
            return new ClientStaticMethod(clientMethodInfo);
        }
        return null;
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        callback(writerWithStaticHeader);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        callback(writerWithStaticHeader);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, Action<NetPakWriter> callback)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        callback(writerWithStaticHeader);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo)
        : base(clientMethodInfo)
    {
    }
}
public sealed class ClientStaticMethod<T> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T arg);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T arg);

    private delegate void WriteDelegate(NetPakWriter writer, T arg);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T arg)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T arg)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ClientMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

    public delegate void ReceiveDelegateWithContext(in ClientInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);

    private WriteDelegate generatedWrite;

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Get(Type declaringType, string methodName)
    {
        return ClientMethodHandle.GetInternal(declaringType, methodName, (ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite) => new ClientStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(clientMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, ITransportConnection transportConnection, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopbackIfLocal(reliability, transportConnection, writerWithStaticHeader);
    }

    public void Invoke(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopbackIfAnyAreLocal(reliability, transportConnections, writerWithStaticHeader);
    }

    public void InvokeAndLoopback(ENetReliability reliability, IEnumerable<ITransportConnection> transportConnections, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        SendAndLoopback(reliability, transportConnections, writerWithStaticHeader);
    }

    private ClientStaticMethod(ClientMethodInfo clientMethodInfo, WriteDelegate generatedWrite)
        : base(clientMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
