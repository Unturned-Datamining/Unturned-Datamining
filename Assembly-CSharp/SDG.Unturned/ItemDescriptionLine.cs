using System;

namespace SDG.Unturned;

public struct ItemDescriptionLine : IComparable<ItemDescriptionLine>
{
    public string text;

    public int priority;

    public int CompareTo(ItemDescriptionLine other)
    {
        if (priority == other.priority)
        {
            return text.CompareTo(other.text);
        }
        return priority.CompareTo(other.priority);
    }
}
