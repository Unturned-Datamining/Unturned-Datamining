namespace SDG.Unturned;

/// <summary>
/// Controls what blueprint does with input items.
/// Separated from EBlueprintType which acted as both category AND operation.
/// </summary>
public enum EBlueprintOperation
{
    /// <summary>
    /// No special modification to input items.
    /// </summary>
    None,
    /// <summary>
    /// Restore target input item to full quality.
    /// </summary>
    RepairTargetItem,
    /// <summary>
    /// Transfer amount from input items to target item.
    /// </summary>
    FillTargetItem
}
