namespace SDG.Unturned;

public struct NetworkSnapshot<T> where T : ISnapshotInfo<T>
{
    public T info;

    public float timestamp;
}
