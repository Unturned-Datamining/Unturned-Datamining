namespace SDG.Unturned;

public enum EPhysicsMaterialCharacterFrictionMode
{
    /// <summary>
    /// Velocity is directly set to input velocity.
    /// </summary>
    ImmediatelyResponsive,
    /// <summary>
    /// Velocity is affected by acceleration and deceleration.
    /// </summary>
    Custom
}
