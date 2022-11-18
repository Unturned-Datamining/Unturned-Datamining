using System.Collections.Generic;

namespace SDG.Unturned;

internal class ItemCurrencyComparer : Comparer<ItemCurrencyAsset.Entry>
{
    public override int Compare(ItemCurrencyAsset.Entry x, ItemCurrencyAsset.Entry y)
    {
        return x.value.CompareTo(y.value);
    }
}
