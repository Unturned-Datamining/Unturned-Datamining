namespace SDG.Unturned;

public enum ECraftingInputPrioritization
{
    /// <summary>
    /// Sort items with lowest "amount" to front of list.
    /// </summary>
    LowestAmount,
    /// <summary>
    /// Sort items with highest "amount" to front of list.
    /// </summary>
    HighestAmount,
    /// <summary>
    /// Sort items with lowest quality% to front of list.
    /// </summary>
    LowestQuality,
    /// <summary>
    /// Sort items with highest quality% to front of list.
    /// </summary>
    HighestQuality
}
