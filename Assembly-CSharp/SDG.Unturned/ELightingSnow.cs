namespace SDG.Unturned;

public enum ELightingSnow
{
    /// <summary>
    /// Corresponds to not active and not blending with new weather system. 
    /// </summary>
    NONE,
    /// <summary>
    /// Corresponds to transitioning in with new weather system. 
    /// </summary>
    PRE_BLIZZARD,
    /// <summary>
    /// Corresponds to active with new weather system. 
    /// </summary>
    BLIZZARD,
    /// <summary>
    /// Corresponds to transitioning out with new weather system. 
    /// </summary>
    POST_BLIZZARD
}
