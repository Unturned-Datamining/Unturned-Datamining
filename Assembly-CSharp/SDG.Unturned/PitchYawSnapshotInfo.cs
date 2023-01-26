using UnityEngine;

namespace SDG.Unturned;

public struct PitchYawSnapshotInfo : ISnapshotInfo<PitchYawSnapshotInfo>
{
    public Vector3 pos;

    public float pitch;

    public float yaw;

    public void lerp(PitchYawSnapshotInfo target, float delta, out PitchYawSnapshotInfo result)
    {
        result = default(PitchYawSnapshotInfo);
        result.pos = Vector3.Lerp(pos, target.pos, delta);
        result.pitch = Mathf.LerpAngle(pitch, target.pitch, delta);
        result.yaw = Mathf.LerpAngle(yaw, target.yaw, delta);
    }

    public PitchYawSnapshotInfo(Vector3 pos, float pitch, float yaw)
    {
        this.pos = pos;
        this.pitch = pitch;
        this.yaw = yaw;
    }
}
