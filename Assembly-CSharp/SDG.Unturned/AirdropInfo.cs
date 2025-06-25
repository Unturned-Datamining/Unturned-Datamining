using System;
using UnityEngine;

namespace SDG.Unturned;

public class AirdropInfo
{
    public Transform model;

    [Obsolete("Replaced by CargoSpawnTableRef which is only set on the server")]
    public ushort id;

    /// <summary>
    /// Current position.
    /// </summary>
    public Vector3 state;

    [Obsolete("Replaced by Velocity property")]
    public Vector3 direction;

    [Obsolete("Replaced by Velocity property")]
    public float speed;

    [Obsolete("Replaced by ServerTimeUntilDrop which is only set on the server")]
    public float delay;

    [Obsolete("Replaced by ServerConstantForce which is only set on the server")]
    public float force;

    [Obsolete("Replaced by ServerHasDeployedCarepackage which is only set on the server")]
    public bool dropped;

    [Obsolete("Replaced by ServerDropPosition which is only set on the server")]
    public Vector3 dropPosition;

    public Vector3 Velocity { get; set; }

    public CachingAssetRef ServerCargoSpawnTableRef { get; set; }

    public float ServerConstantForce
    {
        get
        {
            return force;
        }
        set
        {
            force = value;
        }
    }

    public Vector3 ServerDropPosition
    {
        get
        {
            return dropPosition;
        }
        set
        {
            dropPosition = value;
        }
    }

    public bool ServerHasDeployedCarepackage
    {
        get
        {
            return dropped;
        }
        set
        {
            dropped = value;
        }
    }

    public float ServerTimeUntilDrop
    {
        get
        {
            return delay;
        }
        set
        {
            delay = value;
        }
    }
}
