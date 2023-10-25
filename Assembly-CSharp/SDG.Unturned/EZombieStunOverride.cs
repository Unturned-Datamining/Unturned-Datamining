namespace SDG.Unturned;

/// <summary>
/// Used when damaging zombies to override in which situations they are stunned.
/// </summary>
public enum EZombieStunOverride
{
    /// <summary>
    /// Default stun behaviour determined by damage dealt.
    /// </summary>
    None,
    /// <summary>
    /// Don't stun even if damage is over threshold.
    /// </summary>
    Never,
    /// <summary>
    /// Stun regardless of damage.
    /// </summary>
    Always
}
