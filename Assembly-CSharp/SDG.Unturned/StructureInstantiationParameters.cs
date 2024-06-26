using System;
using UnityEngine;

namespace SDG.Unturned;

internal struct StructureInstantiationParameters : IComparable<StructureInstantiationParameters>
{
    public StructureRegion region;

    public Guid assetId;

    public Vector3 position;

    public Quaternion rotation;

    public byte hp;

    public ulong owner;

    public ulong group;

    public NetId netId;

    public float sortOrder;

    public int CompareTo(StructureInstantiationParameters other)
    {
        return sortOrder.CompareTo(other.sortOrder);
    }
}
