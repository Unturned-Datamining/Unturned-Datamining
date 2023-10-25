namespace SDG.Framework.Devkit.Tools;

public enum EDevkitLandscapeToolHeightmapFlattenMethod
{
    /// <summary>
    /// Directly blend current value toward target value.
    /// </summary>
    REGULAR,
    /// <summary>
    /// Only blend current value toward target value if current is greater than target.
    /// </summary>
    MIN,
    /// <summary>
    /// Only blend current value toward target value if current is less than target.
    /// </summary>
    MAX
}
