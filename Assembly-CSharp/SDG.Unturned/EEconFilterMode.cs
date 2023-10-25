namespace SDG.Unturned;

public enum EEconFilterMode
{
    SEARCH,
    /// <summary>
    /// Find an item to apply stat tracker tool to.
    /// </summary>
    STAT_TRACKER,
    /// <summary>
    /// Find an item with a stat tracker to remove.
    /// </summary>
    STAT_TRACKER_REMOVAL,
    /// <summary>
    /// Find an item with a ragdoll effect to remove.
    /// </summary>
    RAGDOLL_EFFECT_REMOVAL,
    /// <summary>
    /// Find an item to apply ragdoll effect tool to.
    /// </summary>
    RAGDOLL_EFFECT
}
