using System.Collections.Generic;

namespace SDG.Unturned;

public static class PlayerInventorySearchResultV2ListEx
{
    /// <summary>
    /// -1 if no eligible item is found.
    /// If includeMaxQuality is true an item with quality of 100 can be "lowest quality", otherwise item has to
    /// be less than 100 quality.
    /// </summary>
    public static int IndexOfItemWithLowestQuality(this List<PlayerInventorySearchResultV2> searchResults, bool includeMaxQuality = true)
    {
        byte b = (byte)(includeMaxQuality ? byte.MaxValue : 100);
        int result = -1;
        for (int i = 0; i < searchResults.Count; i++)
        {
            if (searchResults[i].Jar.item.quality < b)
            {
                b = searchResults[i].Jar.item.quality;
                result = i;
            }
        }
        return result;
    }
}
