namespace SDG.Unturned;

public enum EDamageFlinchMode
{
    /// <summary>
    /// If hit from the left view rolls right, if hit from the right view rolls left. This may reduce motion
    /// sickness for some players.
    /// </summary>
    RollOnly,
    /// <summary>
    /// Rotate on all axes according to damage direction. This may induce motion sickness.
    /// </summary>
    Directional
}
