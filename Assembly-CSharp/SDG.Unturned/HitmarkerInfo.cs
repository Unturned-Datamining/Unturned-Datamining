using UnityEngine;

namespace SDG.Unturned;

public struct HitmarkerInfo
{
    public float aliveTime;

    public Vector3 worldPosition;

    public bool shouldFollowWorldPosition;

    public SleekHitmarker sleekElement;

    public int bulletIndex;
}
