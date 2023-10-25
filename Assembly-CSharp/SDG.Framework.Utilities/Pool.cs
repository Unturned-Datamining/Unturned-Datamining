using System;
using System.Collections.Generic;

namespace SDG.Framework.Utilities;

public class Pool<T>
{
    public delegate T PoolClaimHandler(Pool<T> pool);

    public delegate void PoolReleaseHandler(Pool<T> pool, T item);

    public delegate void PoolClaimedHandler(Pool<T> pool, T item);

    public delegate void PoolReleasedHandler(Pool<T> pool, T item);

    protected Queue<T> pool;

    /// <summary>
    /// Number of items in underlying queue.
    /// </summary>
    public int count => pool.Count;

    public event PoolClaimedHandler claimed;

    public event PoolReleasedHandler released;

    public void empty()
    {
        pool.Clear();
    }

    public void warmup(uint count)
    {
        warmup(count, null);
    }

    public void warmup(uint count, PoolClaimHandler callback)
    {
        if (callback == null)
        {
            callback = handleClaim;
        }
        for (uint num = 0u; num < count; num++)
        {
            T item = callback(this);
            release(item);
        }
    }

    public T claim()
    {
        return claim(null);
    }

    public T claim(PoolClaimHandler callback)
    {
        T val = ((pool.Count > 0) ? pool.Dequeue() : ((callback == null) ? handleClaim(this) : callback(this)));
        triggerClaimed(val);
        return val;
    }

    public void release(T item)
    {
        release(item, null);
    }

    public void release(T item, PoolReleasedHandler callback)
    {
        if (item != null)
        {
            callback?.Invoke(this, item);
            triggerReleased(item);
            pool.Enqueue(item);
        }
    }

    protected T handleClaim(Pool<T> pool)
    {
        return Activator.CreateInstance<T>();
    }

    protected void triggerClaimed(T item)
    {
        this.claimed?.Invoke(this, item);
    }

    protected void triggerReleased(T item)
    {
        this.released?.Invoke(this, item);
    }

    public Pool()
    {
        pool = new Queue<T>();
    }
}
