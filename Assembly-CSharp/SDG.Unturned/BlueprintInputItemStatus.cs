using System.Collections.Generic;

namespace SDG.Unturned;

internal class BlueprintInputItemStatus
{
    public List<PlayerInventorySearchResultV2> searchResults;

    public int totalAmount;

    /// <summary>
    /// If not zero, use this amount instead of <see cref="F:SDG.Unturned.BlueprintSupply.amount" />.
    /// Used by <see cref="F:SDG.Unturned.EBlueprintOperation.FillTargetItem" /> as amount of ammo needed.
    /// </summary>
    public int requiredAmountOverride;

    public bool isMissingRequiredAmount;

    public PlayerInventorySearchResultV2? FirstResultOrNull
    {
        get
        {
            if (searchResults.Count <= 0)
            {
                return null;
            }
            return searchResults[0];
        }
    }

    public Item FirstItemOrNull => FirstResultOrNull?.Jar?.item;
}
