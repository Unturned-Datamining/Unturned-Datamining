using UnityEngine;

namespace SDG.Unturned;

public class AirdropInfo
{
    public Transform model;

    public ushort id;

    public Vector3 state;

    public Vector3 direction;

    public float speed;

    public float delay;

    public float force;

    public bool dropped;

    /// <summary>
    /// Calculated position (not directly replaced) to spawn falling box.
    /// </summary>
    public Vector3 dropPosition;
}
