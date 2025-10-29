namespace SDG.Unturned;

public enum EZombieDifficultyAssetPrioritization
{
    /// <summary>
    /// Default. Per-navmesh difficulty asset takes priority over per-table/type difficulty asset.
    /// If per-navmesh asset is null the per-table asset is the fallback.
    /// </summary>
    NavmeshOverridesTable,
    /// <summary>
    /// Per-table/type difficulty asset takes priority over per-navmesh difficulty asset.
    /// If per-table asset is null the per-navmesh asset is the fallback.
    /// </summary>
    TableOverridesNavmesh
}
