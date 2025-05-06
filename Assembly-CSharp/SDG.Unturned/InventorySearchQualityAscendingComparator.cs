using System.Collections.Generic;

namespace SDG.Unturned;

public class InventorySearchQualityAscendingComparator : IComparer<InventorySearch>, IComparer<PlayerInventorySearchResultV2>
{
    public int Compare(InventorySearch a, InventorySearch b)
    {
        return a.jar.item.quality - b.jar.item.quality;
    }

    public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
    {
        return a.Jar.item.quality - b.Jar.item.quality;
    }
}
