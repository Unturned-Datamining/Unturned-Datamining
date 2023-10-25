namespace SDG.Unturned;

public enum ESensitivityScalingMode
{
    /// <summary>
    /// Project current field of view onto screen compared to desired field of view.
    /// </summary>
    ProjectionRatio,
    /// <summary>
    /// Multiply sensitivity according to scope/optic zoom. For example an 8x zoom has 1/8th sensitivity.
    /// </summary>
    ZoomFactor,
    /// <summary>
    /// Preserve how sensitivity felt prior to 3.22.8.0 update.
    /// </summary>
    Legacy,
    /// <summary>
    /// Do not adjust sensitivity while aiming.
    /// </summary>
    None
}
