namespace SDG.Unturned;

/// <summary>
/// Implemented by assets which gun supports checking for damage falloff.
/// When implemented, PopulateAsset should call PopulateArmorFalloff.
/// </summary>
public interface IArmorFalloff
{
    /// <summary>
    /// Ranged damage (guns) from greater than this distance finishes decreasing toward falloff multiplier.
    /// Defaults to -1, in which case armor falloff is ignored.
    /// </summary>
    float ArmorFalloffMaxRange { get; set; }

    /// <summary>
    /// Ranged damage (guns) from greater than this distance begins decreasing toward falloff multiplier.
    /// Defaults to ArmorFalloffMaxRange.
    /// </summary>
    float ArmorFalloffRange { get; set; }

    /// <summary>
    /// [0, 1] normalized percentage of incoming damage to apply past IncomingDamageFalloffMaxRange.
    /// </summary>
    float ArmorFalloffMultiplier { get; set; }
}
