namespace SDG.Unturned;

public enum EZombieDifficultyHealthOverrideMode
{
    /// <summary>
    /// Do not override zombie health.
    /// </summary>
    None,
    /// <summary>
    /// Per-speciality value is a multiplier for health configured in the level editor.
    /// </summary>
    MultiplyEditorHealth,
    /// <summary>
    /// Per-speciality value is a multiplier for vanilla health value.
    /// </summary>
    MultiplyDefaultHealth,
    /// <summary>
    /// Per-speciality value replaces zombie's health.
    /// </summary>
    Replace
}
