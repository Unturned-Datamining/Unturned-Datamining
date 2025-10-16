using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

internal class ItemDescriptionBuilderUtils
{
    public static StringBuilder descriptionStringBuilder = new StringBuilder(512);

    public static List<ItemDescriptionLine> lines = new List<ItemDescriptionLine>();

    public static string FormatLines()
    {
        lines.Sort();
        int num = 0;
        if (lines.Count > 0)
        {
            num = lines[0].sortOrder;
        }
        descriptionStringBuilder.Clear();
        foreach (ItemDescriptionLine line in lines)
        {
            if (line.sortOrder - num > 100)
            {
                descriptionStringBuilder.AppendLine();
            }
            descriptionStringBuilder.AppendLine(line.text);
            num = line.sortOrder;
        }
        return descriptionStringBuilder.ToString();
    }

    public static ItemDescriptionBuilder CreateForUI(ItemAsset itemAsset)
    {
        ItemDescriptionBuilder result = default(ItemDescriptionBuilder);
        descriptionStringBuilder.Clear();
        result.stringBuilder = descriptionStringBuilder;
        if (!Glazier.Get().SupportsAutomaticLayout)
        {
            result.flags = EItemDescriptionFlags.LegacyContent;
        }
        else
        {
            result.flags = itemAsset.PreferredDescriptionFlags;
        }
        lines.Clear();
        result.lines = lines;
        return result;
    }
}
