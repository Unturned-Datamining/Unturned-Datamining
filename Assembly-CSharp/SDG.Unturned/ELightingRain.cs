namespace SDG.Unturned;

public enum ELightingRain
{
    /// <summary>
    /// Corresponds to not active and not blending with new weather system. 
    /// </summary>
    NONE,
    /// <summary>
    /// Corresponds to transitioning in with new weather system. 
    /// </summary>
    PRE_DRIZZLE,
    /// <summary>
    /// Corresponds to active with new weather system. 
    /// </summary>
    DRIZZLE,
    /// <summary>
    /// Corresponds to transitioning out with new weather system. 
    /// </summary>
    POST_DRIZZLE
}
