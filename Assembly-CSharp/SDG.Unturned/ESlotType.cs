namespace SDG.Unturned;

/// <summary>
/// Used for item placement in displays / holsters, and whether useable can be placed in primary/secondary slot.
/// </summary>
public enum ESlotType
{
    /// <summary>
    /// Cannot be placed in primary nor secondary slots, but can be equipped from bag.
    /// </summary>
    NONE,
    /// <summary>
    /// Can be placed in primary slot, but cannot be equipped in secondary or bag.
    /// </summary>
    PRIMARY,
    /// <summary>
    /// Can be placed in primary or secondary slot, but cannot be equipped from bag.
    /// </summary>
    SECONDARY,
    /// <summary>
    /// Only used by NPCs.
    /// </summary>
    TERTIARY,
    /// <summary>
    /// Can be placed in primary, secondary, or equipped while in bag.
    /// </summary>
    ANY
}
