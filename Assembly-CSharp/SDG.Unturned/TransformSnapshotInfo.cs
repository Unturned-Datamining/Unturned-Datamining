using UnityEngine;

namespace SDG.Unturned;

public struct TransformSnapshotInfo : ISnapshotInfo<TransformSnapshotInfo>
{
    public Vector3 pos;

    public Quaternion rot;

    public void lerp(TransformSnapshotInfo target, float delta, out TransformSnapshotInfo result)
    {
        result = default(TransformSnapshotInfo);
        result.pos = Vector3.Lerp(pos, target.pos, delta);
        result.rot = Quaternion.Slerp(rot, target.rot, delta);
    }

    public TransformSnapshotInfo(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
}
