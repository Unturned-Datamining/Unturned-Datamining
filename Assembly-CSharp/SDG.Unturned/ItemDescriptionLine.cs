using System;

namespace SDG.Unturned;

public struct ItemDescriptionLine : IComparable<ItemDescriptionLine>
{
    public string text;

    public int sortOrder;

    public int CompareTo(ItemDescriptionLine other)
    {
        if (sortOrder == other.sortOrder)
        {
            return text.CompareTo(other.text);
        }
        return sortOrder.CompareTo(other.sortOrder);
    }
}
