namespace SDG.Unturned;

internal enum EInteractableObjectBinaryStateEmissiveMaterialMode
{
    /// <summary>
    /// Default. Create a material instance for child renderer of Toggle game object.
    /// Downside of this is exclusion from level batching texture atlas.
    /// </summary>
    Auto,
    /// <summary>
    /// Object does not have any toggleable emissive materials.
    /// </summary>
    None
}
