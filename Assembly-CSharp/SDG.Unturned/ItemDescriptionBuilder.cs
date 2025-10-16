using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public struct ItemDescriptionBuilder
{
    public EItemDescriptionFlags flags;

    public List<ItemDescriptionLine> lines;

    /// <summary>
    /// BuildDescription implementations can use this to concatenate longer strings.
    /// </summary>
    public StringBuilder stringBuilder;

    public bool HasFlag(EItemDescriptionFlags flag)
    {
        return flags.HasFlag(flag);
    }

    public void Append(string text, int sortOrder)
    {
        lines.Add(new ItemDescriptionLine
        {
            text = text,
            sortOrder = sortOrder
        });
    }
}
