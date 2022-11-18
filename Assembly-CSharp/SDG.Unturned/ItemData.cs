using UnityEngine;

namespace SDG.Unturned;

public class ItemData
{
    private Item _item;

    private uint _instanceID;

    private Vector3 _point;

    private bool _isDropped;

    private float _lastDropped;

    public Item item => _item;

    public uint instanceID => _instanceID;

    public Vector3 point => _point;

    public bool isDropped => _isDropped;

    public float lastDropped => _lastDropped;

    public ItemData(Item newItem, uint newInstanceID, Vector3 newPoint, bool newDropped)
    {
        _item = newItem;
        _instanceID = newInstanceID;
        _point = newPoint;
        _isDropped = newDropped;
        _lastDropped = Time.realtimeSinceStartup;
    }
}
