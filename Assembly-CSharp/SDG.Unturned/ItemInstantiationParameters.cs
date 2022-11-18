using System;
using UnityEngine;

namespace SDG.Unturned;

internal struct ItemInstantiationParameters : IComparable<ItemInstantiationParameters>
{
    public byte region_x;

    public byte region_y;

    public ushort assetId;

    public byte amount;

    public byte quality;

    public byte[] state;

    public Vector3 point;

    public uint instanceID;

    public float sortOrder;

    public bool shouldPlayEffect;

    public int CompareTo(ItemInstantiationParameters other)
    {
        return sortOrder.CompareTo(other.sortOrder);
    }
}
