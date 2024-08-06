namespace SDG.Unturned;

/// <summary>
/// Controls how vehicle's default paint color (if applicable) is chosen.
/// </summary>
internal enum EVehicleDefaultPaintColorMode
{
    /// <summary>
    /// Not configured.
    /// </summary>
    None,
    /// <summary>
    /// Pick from the DefaultPaintColors list.
    /// </summary>
    List,
    /// <summary>
    /// Pick a random HSV using VehicleRandomPaintColorConfiguration.
    /// </summary>
    RandomHueOrGrayscale
}
