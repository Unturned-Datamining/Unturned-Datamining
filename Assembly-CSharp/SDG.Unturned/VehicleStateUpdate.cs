using UnityEngine;

namespace SDG.Unturned;

public struct VehicleStateUpdate
{
    public Vector3 pos;

    public Quaternion rot;

    public VehicleStateUpdate(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
}
