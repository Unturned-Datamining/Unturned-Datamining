using System.Collections.Generic;

namespace SDG.Unturned;

public class InventorySearchQualityDescendingComparator : IComparer<InventorySearch>, IComparer<PlayerInventorySearchResultV2>
{
    public int Compare(InventorySearch a, InventorySearch b)
    {
        return b.jar.item.quality - a.jar.item.quality;
    }

    public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
    {
        return b.Jar.item.quality - a.Jar.item.quality;
    }
}
