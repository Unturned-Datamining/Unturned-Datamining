using System;
using UnityEngine;

namespace SDG.Unturned;

public class BulletInfo
{
    /// <summary>
    /// Starting position when the bullet was fired.
    /// </summary>
    public Vector3 origin;

    [Obsolete("Will be removed in a future version. Please use the position property instead.")]
    public Vector3 pos;

    [Obsolete("Will be removed in a future version. Please use the GetDirection method instead.")]
    public Vector3 dir;

    public byte steps;

    public float quality;

    public byte pellet;

    public ushort dropID;

    public byte dropAmount;

    public byte dropQuality;

    public ItemBarrelAsset barrelAsset;

    public ItemMagazineAsset magazineAsset;

    public Vector3 position
    {
        get
        {
            return pos;
        }
        internal set
        {
            pos = value;
        }
    }

    public Vector3 velocity { get; internal set; }

    public Vector3 GetDirection()
    {
        return velocity.normalized;
    }
}
