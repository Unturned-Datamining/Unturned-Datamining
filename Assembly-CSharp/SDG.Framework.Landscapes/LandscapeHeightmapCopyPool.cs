using SDG.Framework.Utilities;

namespace SDG.Framework.Landscapes;

public static class LandscapeHeightmapCopyPool
{
    private static Pool<float[,]> pool;

    public static void empty()
    {
        pool.empty();
    }

    public static void warmup(uint count)
    {
        pool.warmup(count, handlePoolClaim);
    }

    public static float[,] claim()
    {
        return pool.claim(handlePoolClaim);
    }

    public static void release(float[,] copy)
    {
        pool.release(copy, handlePoolRelease);
    }

    private static float[,] handlePoolClaim(Pool<float[,]> pool)
    {
        return new float[Landscape.HEIGHTMAP_RESOLUTION, Landscape.HEIGHTMAP_RESOLUTION];
    }

    private static void handlePoolRelease(Pool<float[,]> pool, float[,] copy)
    {
    }

    static LandscapeHeightmapCopyPool()
    {
        pool = new Pool<float[,]>();
    }
}
