using System;
using System.Collections.Generic;
using SDG.NetPak;

namespace SDG.Unturned;

public static class NetInvocationDeferralRegistry
{
    private struct DeferredInvocation
    {
        public NetId netId;

        public uint scratch;

        public int scratchBitCount;

        public byte[] buffer;

        public ClientMethodInfo methodInfo;

        public NetInvokeDeferred callback;
    }

    private static Dictionary<NetId, List<DeferredInvocation>> deferrals = new Dictionary<NetId, List<DeferredInvocation>>();

    private static List<List<DeferredInvocation>> pool = new List<List<DeferredInvocation>>();

    public static void Defer(NetId key, in ClientInvocationContext context, NetInvokeDeferred callback)
    {
        if (deferrals.TryGetValue(key, out var value))
        {
            NetPakReader reader = context.reader;
            DeferredInvocation item = default(DeferredInvocation);
            item.netId = key;
            item.buffer = new byte[reader.RemainingSegmentLength];
            if (reader.SaveState(out item.scratch, out item.scratchBitCount, item.buffer))
            {
                item.methodInfo = context.clientMethodInfo;
                item.callback = callback;
                value.Add(item);
            }
        }
    }

    public static void MarkDeferred(NetId key, uint blockSize = 1u)
    {
        if (!deferrals.TryGetValue(key, out var value))
        {
            value = ((pool.Count <= 0) ? new List<DeferredInvocation>() : pool.GetAndRemoveTail());
            for (uint num = 0u; num < blockSize; num++)
            {
                deferrals.Add(key + num, value);
            }
        }
    }

    public static void Cancel(NetId key, uint blockSize = 1u)
    {
        if (deferrals.TryGetValue(key, out var value))
        {
            for (uint num = 0u; num < blockSize; num++)
            {
                deferrals.Remove(key + num);
            }
            value.Clear();
            pool.Add(value);
        }
    }

    private static void Invoke(object voidNetObj, DeferredInvocation invocation)
    {
        NetPakReader invokableReader = NetMessages.GetInvokableReader();
        invokableReader.LoadState(invocation.scratch, invocation.scratchBitCount, invocation.buffer, invocation.buffer.Length);
        ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Deferred, invokableReader, invocation.methodInfo);
        try
        {
            invocation.callback(voidNetObj, in context);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception invoking {0} deferred:", invocation.methodInfo);
        }
    }

    public static void Invoke(NetId key, uint blockSize = 1u)
    {
        if (!deferrals.TryGetValue(key, out var value))
        {
            return;
        }
        for (uint num = 0u; num < blockSize; num++)
        {
            deferrals.Remove(key + num);
        }
        foreach (DeferredInvocation item in value)
        {
            object obj = NetIdRegistry.Get(item.netId);
            if (obj == null)
            {
                break;
            }
            Invoke(obj, item);
        }
        value.Clear();
        pool.Add(value);
    }

    internal static void Clear()
    {
        deferrals.Clear();
        pool.Clear();
    }
}
