using UnityEngine;

namespace SDG.Unturned;

public class ItemDrop
{
    private Transform _model;

    private InteractableItem _interactableItem;

    private uint _instanceID;

    public Transform model => _model;

    public InteractableItem interactableItem => _interactableItem;

    public uint instanceID => _instanceID;

    public ItemDrop(Transform newModel, InteractableItem newInteractableItem, uint newInstanceID)
    {
        _model = newModel;
        _interactableItem = newInteractableItem;
        _instanceID = newInstanceID;
    }
}
