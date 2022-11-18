using UnityEngine;

namespace SDG.Unturned;

public class EditorCopy
{
    private Vector3 _position;

    private Quaternion _rotation;

    private Vector3 _scale;

    private ObjectAsset _objectAsset;

    private ItemAsset _itemAsset;

    public Vector3 position => _position;

    public Quaternion rotation => _rotation;

    public Vector3 scale => _scale;

    public ObjectAsset objectAsset => _objectAsset;

    public ItemAsset itemAsset => _itemAsset;

    public EditorCopy(Vector3 newPosition, Quaternion newRotation, Vector3 newScale, ObjectAsset newObjectAsset, ItemAsset newItemAsset)
    {
        _position = newPosition;
        _rotation = newRotation;
        _scale = newScale;
        _objectAsset = newObjectAsset;
        _itemAsset = newItemAsset;
    }
}
