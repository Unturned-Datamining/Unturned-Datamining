using System.Collections.Generic;

namespace SDG.Unturned;

public class InventorySearchAmountAscendingComparator : IComparer<InventorySearch>, IComparer<PlayerInventorySearchResultV2>
{
    public int Compare(InventorySearch a, InventorySearch b)
    {
        return a.jar.item.amount - b.jar.item.amount;
    }

    public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
    {
        return a.Jar.item.amount - b.Jar.item.amount;
    }
}
