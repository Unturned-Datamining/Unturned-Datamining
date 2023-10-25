namespace SDG.Unturned;

public enum EBoxProbabilityModel
{
    /// <summary>
    /// Each quality tier has different rarities.
    /// Legendary: 5% Epic: 20% Rare: 75%
    /// </summary>
    Original,
    /// <summary>
    /// Each item has an equal chance regardless of quality.
    /// </summary>
    Equalized
}
