using UnityEngine;

namespace SDG.Unturned;

public struct PitchYawSnapshotInfo : ISnapshotInfo
{
    public Vector3 pos;

    public float pitch;

    public float yaw;

    public ISnapshotInfo lerp(ISnapshotInfo targetTemp, float delta)
    {
        PitchYawSnapshotInfo pitchYawSnapshotInfo = (PitchYawSnapshotInfo)(object)targetTemp;
        PitchYawSnapshotInfo pitchYawSnapshotInfo2 = default(PitchYawSnapshotInfo);
        pitchYawSnapshotInfo2.pos = Vector3.Lerp(pos, pitchYawSnapshotInfo.pos, delta);
        pitchYawSnapshotInfo2.pitch = Mathf.LerpAngle(pitch, pitchYawSnapshotInfo.pitch, delta);
        pitchYawSnapshotInfo2.yaw = Mathf.LerpAngle(yaw, pitchYawSnapshotInfo.yaw, delta);
        return pitchYawSnapshotInfo2;
    }

    public PitchYawSnapshotInfo(Vector3 pos, float pitch, float yaw)
    {
        this.pos = pos;
        this.pitch = pitch;
        this.yaw = yaw;
    }
}
