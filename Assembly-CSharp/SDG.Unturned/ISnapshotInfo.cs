namespace SDG.Unturned;

public interface ISnapshotInfo<T>
{
    void lerp(T target, float delta, out T result);
}
