using System;

namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-04-09: this acted as both category AND behaviour modifier, so I'm separating it into a custom tag
/// for categorization and a property for overriding how the blueprint processes input items.
///
/// Nelson 2025-04-10: repair and ammo "types" had a variety of quirks I wanted to sort out:
/// • Moving amount between items required ammo type blueprint, but some modders expressed interest in non-ammo use.
///   (I.e., ideally better supporting amount on non-ammo items going forward.)
/// • Both types ignored output items. Output was used to represent the target item. Similarly, the UI added a fake
///   extra input item representing target item.
/// • PlayerCrafting and PlayerDashboardCraftingUI re-implemented some crafting item searching logic for finding
///   the item to refill or repair that can be converted into input item parameters.
/// The plan at the moment is to make the last input item the "target" item for operations. Legacy ammo/repair
/// blueprints will then default to no output item and add an extra input item. (And add a variety of parameters
/// needed to replicate the specialized item search behaviour.)
/// </summary>
[Obsolete("Separated into Category Tags and EBlueprintOperation.")]
public enum EBlueprintType
{
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    TOOL,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    APPAREL,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    SUPPLY,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    GEAR,
    [Obsolete("Separated into Category Tags and EBlueprintOperation.FillTargetItem.")]
    AMMO,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    BARRICADE,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    STRUCTURE,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    UTILITIES,
    [Obsolete("Only used for categorization. Please use Category Tags instead.")]
    FURNITURE,
    [Obsolete("Separated into Category Tags and EBlueprintOperation.RepairTargetItem.")]
    REPAIR
}
