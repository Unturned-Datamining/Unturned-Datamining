using UnityEngine;

namespace SDG.Unturned;

public struct TransformSnapshotInfo : ISnapshotInfo
{
    public Vector3 pos;

    public Quaternion rot;

    public ISnapshotInfo lerp(ISnapshotInfo targetTemp, float delta)
    {
        TransformSnapshotInfo transformSnapshotInfo = (TransformSnapshotInfo)(object)targetTemp;
        TransformSnapshotInfo transformSnapshotInfo2 = default(TransformSnapshotInfo);
        transformSnapshotInfo2.pos = Vector3.Lerp(pos, transformSnapshotInfo.pos, delta);
        transformSnapshotInfo2.rot = Quaternion.Slerp(rot, transformSnapshotInfo.rot, delta);
        return transformSnapshotInfo2;
    }

    public TransformSnapshotInfo(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
}
