using System;
using UnityEngine;

namespace SDG.Unturned;

internal struct BarricadeInstantiationParameters : IComparable<BarricadeInstantiationParameters>
{
    public BarricadeRegion region;

    public Guid assetId;

    public byte[] state;

    public Vector3 position;

    public Quaternion rotation;

    public byte hp;

    public ulong owner;

    public ulong group;

    public NetId netId;

    public float sortOrder;

    public int CompareTo(BarricadeInstantiationParameters other)
    {
        return sortOrder.CompareTo(other.sortOrder);
    }
}
