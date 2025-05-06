using System.Collections.Generic;

namespace SDG.Unturned;

internal static class PlayerInventorySearchResultPool
{
    private static List<List<PlayerInventorySearchResultV2>> pool = new List<List<PlayerInventorySearchResultV2>>();

    public static List<PlayerInventorySearchResultV2> Claim()
    {
        if (pool.IsEmpty())
        {
            return new List<PlayerInventorySearchResultV2>();
        }
        List<PlayerInventorySearchResultV2> andRemoveTail = pool.GetAndRemoveTail();
        andRemoveTail.Clear();
        return andRemoveTail;
    }

    public static void Release(List<PlayerInventorySearchResultV2> list)
    {
        pool.Add(list);
    }
}
