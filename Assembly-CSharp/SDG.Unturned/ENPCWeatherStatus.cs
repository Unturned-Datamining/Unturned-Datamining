namespace SDG.Unturned;

public enum ENPCWeatherStatus
{
    /// <summary>
    /// True while fading in or fully transitioned in. 
    /// </summary>
    Active,
    /// <summary>
    /// True while fading in, but not at full intensity.
    /// </summary>
    Transitioning_In,
    /// <summary>
    /// True while finished fading in.
    /// </summary>
    Fully_Transitioned_In,
    /// <summary>
    /// True while fading out, but not at zero intensity.
    /// </summary>
    Transitioning_Out,
    /// <summary>
    /// True while finished fading out.
    /// </summary>
    Fully_Transitioned_Out,
    /// <summary>
    /// True while fading in or out.
    /// </summary>
    Transitioning
}
