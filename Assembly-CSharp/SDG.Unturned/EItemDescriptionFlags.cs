using System;

namespace SDG.Unturned;

/// <summary>
/// Determines which info is automatically added to the item description.
/// </summary>
[Flags]
public enum EItemDescriptionFlags
{
    /// <summary>
    /// Do not add any of the newer info to the description.
    /// Equivalent to Use_Auto_Stat_Descriptions false.
    /// Also applicable when using IMGUI.
    /// </summary>
    LegacyContent = 0,
    /// <summary>
    /// Include names of gun's attachments in the description.
    /// </summary>
    GunAttachments = 1,
    /// <summary>
    /// Include any other info without its own flag.
    ///
    /// This only exists because description flags are retrofitted over an all-or-nothing
    /// option (Use_Auto_Stat_Description).
    /// </summary>
    Uncategorized = 2,
    /// <summary>
    /// Add as much info to the description as possible.
    /// Equivalent to Use_Auto_Stat_Descriptions true.
    /// </summary>
    All = 3
}
