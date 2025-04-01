namespace SDG.Unturned;

public struct RegionVisibilityData
{
    /// <summary>
    /// If -1, this region is finished activating/deactivating.
    /// Otherwise, incremented once per frame until per-region count is reached.
    ///
    /// Per-region data is removed when isInsideMask is false and progressIndex is -1.
    /// </summary>
    public int progressIndex;

    /// <summary>
    /// If true, this region is within MaxDistance of current CameraCoord.
    /// </summary>
    public bool isInsideMask;
}
