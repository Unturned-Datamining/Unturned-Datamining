using System.Collections.Generic;

namespace SDG.Unturned;

public struct ItemDescriptionBuilder
{
    public bool shouldRestrictToLegacyContent;

    public List<ItemDescriptionLine> lines;

    public void Append(string text, int priority)
    {
        lines.Add(new ItemDescriptionLine
        {
            text = text,
            priority = priority
        });
    }
}
