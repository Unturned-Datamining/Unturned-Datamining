using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Pool lists to avoid loopback re-using an existing list.
/// Callers do not need to manually return lists because they are reset before each frame.
/// </summary>
internal static class TransportConnectionListPool
{
    private static List<PooledTransportConnectionList> available;

    private static List<PooledTransportConnectionList> claimed;

    private static int lastWarningFrameNumber;

    public static PooledTransportConnectionList Get()
    {
        PooledTransportConnectionList pooledTransportConnectionList;
        if (available.Count > 0)
        {
            pooledTransportConnectionList = available.GetAndRemoveTail();
            if (pooledTransportConnectionList.Count > 0)
            {
                pooledTransportConnectionList.Clear();
                int frameCount = Time.frameCount;
                if (frameCount != lastWarningFrameNumber)
                {
                    lastWarningFrameNumber = frameCount;
                    UnturnedLog.warn("PooledConnectionList was used after end of frame! Plugins should not hold onto these lists.");
                }
            }
        }
        else
        {
            pooledTransportConnectionList = new PooledTransportConnectionList(Provider.maxPlayers);
        }
        claimed.Add(pooledTransportConnectionList);
        return pooledTransportConnectionList;
    }

    public static void ReleaseAll()
    {
        foreach (PooledTransportConnectionList item in claimed)
        {
            item.Clear();
            available.Add(item);
        }
        claimed.Clear();
    }

    static TransportConnectionListPool()
    {
        available = new List<PooledTransportConnectionList>();
        claimed = new List<PooledTransportConnectionList>();
        lastWarningFrameNumber = -1;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private static void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Transport connection list pool size: {available.Count}");
        results.Add($"Transport connection list active count: {claimed.Count}");
    }
}
