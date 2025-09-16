namespace SDG.Framework.Devkit;

public enum EAmbianceVolumeFogOverrideMode
{
    /// <summary>
    /// Volume doesn't override fog.
    /// </summary>
    None,
    /// <summary>
    /// Volume fog settings are the same at all times of day.
    /// </summary>
    Constant,
    /// <summary>
    /// Volume fog settings vary throughout the day.
    /// </summary>
    PerTimeOfDay
}
