using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Replacement for enum origin.
/// </summary>
public class AssetOrigin
{
    /// <summary>
    /// Hardcoded built-in name, or name of workshop file if known.
    /// </summary>
    public string name;

    /// <summary>
    /// Steam file ID if loaded from the workshop, zero otherwise.
    /// </summary>
    public ulong workshopFileId;

    /// <summary>
    /// If true, when added to asset mapping the new assets will override existing ones.
    /// This ensures workshop files installed by servers take priority and disables warnings about overlapping IDs.
    /// </summary>
    internal bool shouldAssetsOverrideExistingIds;

    /// <summary>
    /// If true, we can re-save .dat/.asset files from this origin.
    /// Defaults to false. Only true for assets in the game install folder.
    /// </summary>
    internal bool canResave;

    internal List<Asset> assets = new List<Asset>();

    public IReadOnlyList<Asset> GetAssets()
    {
        return assets;
    }
}
