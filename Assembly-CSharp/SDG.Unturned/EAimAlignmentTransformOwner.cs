namespace SDG.Unturned;

/// <summary>
/// Controls where attachments looks for ADS alignment transform.
/// </summary>
public enum EAimAlignmentTransformOwner
{
    /// <summary>
    /// Look for aim alignment transform relative to sight model.
    /// Defaults to Model_0/Aim.
    /// </summary>
    Sight,
    /// <summary>
    /// Look for aim alignment transform relative to equipable prefab.
    /// Requires setting AimAlignment_Path.
    /// </summary>
    Gun
}
