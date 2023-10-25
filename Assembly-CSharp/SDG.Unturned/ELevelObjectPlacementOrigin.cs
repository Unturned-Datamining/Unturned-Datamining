namespace SDG.Unturned;

public enum ELevelObjectPlacementOrigin
{
    /// <summary>
    /// Manually placed from the asset browser or old editor.
    /// </summary>
    MANUAL,
    /// <summary>
    /// Spawned by foliage baking system.
    /// </summary>
    GENERATED,
    /// <summary>
    /// Brushed on with the foliage tool.
    /// </summary>
    PAINTED
}
