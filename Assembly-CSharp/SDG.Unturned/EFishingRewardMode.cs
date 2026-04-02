namespace SDG.Unturned;

public enum EFishingRewardMode
{
    /// <summary>
    /// Fishing rod itself defines rewards. Ignore per-volume rewards.
    /// Default. (backwards compatibility)
    /// </summary>
    Rod,
    /// <summary>
    /// Use per-volume (or per-level if volume unspecified) rewards.
    /// If level doesn't support volume rewards, fallback to Rod rewards.
    /// </summary>
    WaterVolumes
}
