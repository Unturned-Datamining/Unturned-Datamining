namespace SDG.Unturned;

/// <summary>
/// 2023-01-25: fixing killing self with explosive to track kill under
/// the assumption that this is only used for tracking stats. (public issue #2692)
/// </summary>
public enum EPlayerKill
{
    NONE,
    PLAYER,
    ZOMBIE,
    MEGA,
    ANIMAL,
    RESOURCE,
    OBJECT
}
