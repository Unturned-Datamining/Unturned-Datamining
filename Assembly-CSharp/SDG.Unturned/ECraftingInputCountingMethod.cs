namespace SDG.Unturned;

public enum ECraftingInputCountingMethod
{
    /// <summary>
    /// Sum up number of items found, ignoring amount.
    /// Default except as described in TotalAmount comment.
    /// </summary>
    TotalItems,
    /// <summary>
    /// Sum up "amount" of each item. Optionally counting zero as one (ShouldCountEmptyAsOne).
    /// Default for legacy "ammo type" blueprints and FillTargetItem operation.
    /// </summary>
    TotalAmount
}
