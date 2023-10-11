using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public struct ItemDescriptionBuilder
{
    public bool shouldRestrictToLegacyContent;

    public List<ItemDescriptionLine> lines;

    public StringBuilder stringBuilder;

    public void Append(string text, int sortOrder)
    {
        lines.Add(new ItemDescriptionLine
        {
            text = text,
            sortOrder = sortOrder
        });
    }
}
