namespace SDG.Unturned;

/// <summary>
/// Abstracts pathfinding/navmesh library available.
/// (ASPFP implementation not included in SDK release, but this way licensees of the plugin can
/// re-enable it in custom builds.)
/// </summary>
public static class UnturnedPathfinding
{
    private static IUnturnedPathfindingInterface instance;

    public static IUnturnedPathfindingInterface Get()
    {
        return instance;
    }

    public static void Initialize()
    {
        instance = new UnturnedPathfinding_ASPFP();
    }
}
