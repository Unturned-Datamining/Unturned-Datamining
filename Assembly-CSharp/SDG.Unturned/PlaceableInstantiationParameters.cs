using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-05-28: keeping this a struct to simplify memory management (no pool needed). If making this more
/// generic in the future we probably do need to make it a class.
/// </summary>
internal struct PlaceableInstantiationParameters : IComparable<PlaceableInstantiationParameters>
{
    public EPlaceableInstantiationType type;

    public object region;

    public Guid assetId;

    /// <summary>
    /// Only applicable to barricades.
    /// </summary>
    public byte[] state;

    public Vector3 position;

    public Quaternion rotation;

    public byte hp;

    public ulong owner;

    public ulong group;

    public NetId netId;

    public float sortOrder;

    /// <summary>
    /// Preliminary sort order is provided by server, but this takes priority if camera is available.
    /// </summary>
    public void UpdateSortOrder()
    {
        if (MainCamera.instance != null)
        {
            sortOrder = (MainCamera.instance.transform.position - position).sqrMagnitude;
        }
    }

    public int CompareTo(PlaceableInstantiationParameters other)
    {
        return sortOrder.CompareTo(other.sortOrder);
    }

    public override string ToString()
    {
        return $"(Type: {type}, AssetID: {assetId:N})";
    }
}
