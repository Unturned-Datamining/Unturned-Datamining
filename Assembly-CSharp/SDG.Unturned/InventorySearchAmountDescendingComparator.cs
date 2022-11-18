using System.Collections.Generic;

namespace SDG.Unturned;

public class InventorySearchAmountDescendingComparator : IComparer<InventorySearch>
{
    public int Compare(InventorySearch a, InventorySearch b)
    {
        return b.jar.item.amount - a.jar.item.amount;
    }
}
