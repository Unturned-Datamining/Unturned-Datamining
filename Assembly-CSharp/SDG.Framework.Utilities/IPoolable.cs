namespace SDG.Framework.Utilities;

public interface IPoolable
{
    void poolClaim();

    void poolRelease();
}
