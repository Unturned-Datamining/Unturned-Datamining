using UnityEngine;

namespace SDG.Unturned;

public struct YawSnapshotInfo : ISnapshotInfo<YawSnapshotInfo>
{
    public Vector3 pos;

    public float yaw;

    public void lerp(YawSnapshotInfo target, float delta, out YawSnapshotInfo result)
    {
        result = default(YawSnapshotInfo);
        result.pos = Vector3.Lerp(pos, target.pos, delta);
        result.yaw = Mathf.LerpAngle(yaw, target.yaw, delta);
    }

    public YawSnapshotInfo(Vector3 pos, float yaw)
    {
        this.pos = pos;
        this.yaw = yaw;
    }
}
