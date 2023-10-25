namespace SDG.Framework.Debug;

public interface IInspectableList
{
    /// <summary>
    /// Whether add can be called from the inspector.
    /// </summary>
    bool canInspectorAdd { get; set; }

    /// <summary>
    /// Whether remove can be called from the inspector.
    /// </summary>
    bool canInspectorRemove { get; set; }

    event InspectableListAddedHandler inspectorAdded;

    event InspectableListRemovedHandler inspectorRemoved;

    event InspectableListChangedHandler inspectorChanged;

    /// <summary>
    /// Called when the inspector adds an element.
    /// </summary>
    void inspectorAdd(object instance);

    /// <summary>
    /// Called when the inspector removes an element.
    /// </summary>
    void inspectorRemove(object instance);

    /// <summary>
    /// Called when the inspector sets an element to a different value.
    /// </summary>
    void inspectorSet(int index);
}
