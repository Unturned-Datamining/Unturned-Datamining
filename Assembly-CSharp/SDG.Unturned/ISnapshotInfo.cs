namespace SDG.Unturned;

public interface ISnapshotInfo
{
    ISnapshotInfo lerp(ISnapshotInfo target, float delta);
}
