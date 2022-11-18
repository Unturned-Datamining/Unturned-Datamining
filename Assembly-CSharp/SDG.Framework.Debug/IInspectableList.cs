namespace SDG.Framework.Debug;

public interface IInspectableList
{
    bool canInspectorAdd { get; set; }

    bool canInspectorRemove { get; set; }

    event InspectableListAddedHandler inspectorAdded;

    event InspectableListRemovedHandler inspectorRemoved;

    event InspectableListChangedHandler inspectorChanged;

    void inspectorAdd(object instance);

    void inspectorRemove(object instance);

    void inspectorSet(int index);
}
