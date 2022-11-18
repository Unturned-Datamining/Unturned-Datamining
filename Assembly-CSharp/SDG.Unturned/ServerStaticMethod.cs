using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

public sealed class ServerStaticMethod : ServerMethodHandle
{
    public delegate void ReceiveDelegate();

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context);

    public static ServerStaticMethod Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod Get(Type declaringType, string methodName)
    {
        ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(declaringType, methodName);
        if (serverMethodInfo != null)
        {
            return new ServerStaticMethod(serverMethodInfo);
        }
        return null;
    }

    public void Invoke(ENetReliability reliability)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo)
        : base(serverMethodInfo)
    {
    }
}
public sealed class ServerStaticMethod<T> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T arg);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T arg);

    private delegate void WriteDelegate(NetPakWriter writer, T arg);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T arg)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3, T4> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3, T4>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3, T4, T5> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3, T4, T5>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3, T4, T5, T6>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
public sealed class ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> : ServerMethodHandle
{
    public delegate void ReceiveDelegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate void ReceiveDelegateWithContext(in ServerInvocationContext context, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private delegate void WriteDelegate(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    private WriteDelegate generatedWrite;

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegate action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(ReceiveDelegateWithContext action)
    {
        return Get(action.Method.DeclaringType, action.Method.Name);
    }

    public static ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8> Get(Type declaringType, string methodName)
    {
        return ServerMethodHandle.GetInternal(declaringType, methodName, (ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite) => new ServerStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8>(serverMethodInfo, generatedWrite));
    }

    public void Invoke(ENetReliability reliability, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        generatedWrite(writerWithStaticHeader, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        SendAndLoopbackIfLocal(reliability, writerWithStaticHeader);
    }

    private ServerStaticMethod(ServerMethodInfo serverMethodInfo, WriteDelegate generatedWrite)
        : base(serverMethodInfo)
    {
        this.generatedWrite = generatedWrite;
    }
}
