namespace SDG.Unturned;

/// <summary>
/// Controls whether vehicle allows barricades to be attached to it.
/// </summary>
public enum EVehicleBuildablePlacementRule
{
    /// <summary>
    /// Vehicle does not override placement. This means, by default, that barricades can be placed on the vehicle
    /// unless the barricade sets Allow_Placement_On_Vehicle to false. (e.g., beds and sentry guns) Note that
    /// gameplay config Bypass_Buildable_Mobility, if true, takes priority.
    /// </summary>
    None,
    /// <summary>
    /// Vehicle allows any barricade to be placed on it, regardless of the barricade's Allow_Placement_On_Vehicle
    /// setting. The legacy option for this was the Supports_Mobile_Buildables flag. Vanilla trains originally
    /// used this option, but it was exploited to move beds into tunnel walls.
    /// </summary>
    AlwaysAllow,
    /// <summary>
    /// Vehicle prevents any barricade from being placed on it. Note that gameplay config Bypass_Buildable_Mobility,
    /// if true, takes priority.
    /// </summary>
    Block
}
