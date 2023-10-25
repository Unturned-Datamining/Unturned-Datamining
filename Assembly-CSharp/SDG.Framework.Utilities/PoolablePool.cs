using System;

namespace SDG.Framework.Utilities;

/// <summary>
/// Pool of objects that implement the IPoolable interface.
///
/// Useful for types that do not need special construction,
/// and want notification when claimed and released.
/// </summary>
public static class PoolablePool<T> where T : IPoolable
{
    private static Pool<T> pool;

    public static void empty()
    {
        pool.empty();
    }

    public static void warmup(uint count)
    {
        pool.warmup(count, handlePoolClaim);
    }

    public static T claim()
    {
        T result = pool.claim(handlePoolClaim);
        result.poolClaim();
        return result;
    }

    public static void release(T poolable)
    {
        pool.release(poolable, handlePoolRelease);
    }

    private static T handlePoolClaim(Pool<T> pool)
    {
        return Activator.CreateInstance<T>();
    }

    private static void handlePoolRelease(Pool<T> pool, T poolable)
    {
        poolable.poolRelease();
    }

    static PoolablePool()
    {
        pool = new Pool<T>();
    }
}
