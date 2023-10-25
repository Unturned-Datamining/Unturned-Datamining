using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public struct ItemDescriptionBuilder
{
    /// <summary>
    /// If true, description should only be populated with contents from prior to the auto-layout UI changes.
    /// </summary>
    public bool shouldRestrictToLegacyContent;

    public List<ItemDescriptionLine> lines;

    /// <summary>
    /// BuildDescription implementations can use this to concatenate longer strings.
    /// </summary>
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
