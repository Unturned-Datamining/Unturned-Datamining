using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal static class TransportConnectionListPool
{
    private static List<PooledTransportConnectionList> available = new List<PooledTransportConnectionList>();

    private static List<PooledTransportConnectionList> claimed = new List<PooledTransportConnectionList>();

    private static int lastWarningFrameNumber = -1;

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
}
