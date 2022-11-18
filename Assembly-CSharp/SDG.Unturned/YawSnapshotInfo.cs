using UnityEngine;

namespace SDG.Unturned;

public struct YawSnapshotInfo : ISnapshotInfo
{
    public Vector3 pos;

    public float yaw;

    public ISnapshotInfo lerp(ISnapshotInfo targetTemp, float delta)
    {
        YawSnapshotInfo yawSnapshotInfo = (YawSnapshotInfo)(object)targetTemp;
        YawSnapshotInfo yawSnapshotInfo2 = default(YawSnapshotInfo);
        yawSnapshotInfo2.pos = Vector3.Lerp(pos, yawSnapshotInfo.pos, delta);
        yawSnapshotInfo2.yaw = Mathf.LerpAngle(yaw, yawSnapshotInfo.yaw, delta);
        return yawSnapshotInfo2;
    }

    public YawSnapshotInfo(Vector3 pos, float yaw)
    {
        this.pos = pos;
        this.yaw = yaw;
    }
}
