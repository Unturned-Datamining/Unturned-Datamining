namespace SDG.Framework.Utilities;

/// <summary>
/// For use with PoolablePool when no special construction is required.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when this instance is getting claimed.
    /// </summary>
    void poolClaim();

    /// <summary>
    /// Called when this instance is returned to the pool.
    /// </summary>
    void poolRelease();
}
