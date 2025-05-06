using System.Collections.Generic;

namespace SDG.Unturned;

public class InventorySearchAmountDescendingComparator : IComparer<InventorySearch>, IComparer<PlayerInventorySearchResultV2>
{
    public int Compare(InventorySearch a, InventorySearch b)
    {
        return b.jar.item.amount - a.jar.item.amount;
    }

    public int Compare(PlayerInventorySearchResultV2 a, PlayerInventorySearchResultV2 b)
    {
        return b.Jar.item.amount - a.Jar.item.amount;
    }
}
