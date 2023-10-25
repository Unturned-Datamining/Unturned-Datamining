using System.Collections.Generic;

namespace SDG.Framework.Debug;

public class InspectableList<T> : List<T>, IInspectableList
{
    /// <summary>
    /// Whether add can be called from the inspector.
    /// </summary>
    public virtual bool canInspectorAdd { get; set; }

    /// <summary>
    /// Whether remove can be called from the inspector.
    /// </summary>
    public virtual bool canInspectorRemove { get; set; }

    public event InspectableListAddedHandler inspectorAdded;

    public event InspectableListRemovedHandler inspectorRemoved;

    public event InspectableListChangedHandler inspectorChanged;

    public new void Add(T item)
    {
        base.Add(item);
        triggerChanged();
    }

    public new bool Remove(T item)
    {
        bool result = base.Remove(item);
        triggerChanged();
        return result;
    }

    public new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        triggerChanged();
    }

    public virtual void inspectorAdd(object instance)
    {
        triggerAdded(instance);
        triggerChanged();
    }

    public virtual void inspectorRemove(object instance)
    {
        triggerRemoved(instance);
        triggerChanged();
    }

    public virtual void inspectorSet(int index)
    {
        triggerChanged();
    }

    protected virtual void triggerAdded(object instance)
    {
        this.inspectorAdded?.Invoke(this, instance);
    }

    protected virtual void triggerRemoved(object instance)
    {
        this.inspectorRemoved?.Invoke(this, instance);
    }

    protected virtual void triggerChanged()
    {
        this.inspectorChanged?.Invoke(this);
    }

    public InspectableList()
    {
        canInspectorAdd = true;
        canInspectorRemove = true;
    }

    public InspectableList(int capacity)
        : base(capacity)
    {
        canInspectorAdd = false;
        canInspectorRemove = false;
    }

    public InspectableList(IEnumerable<T> collection)
        : base(collection)
    {
        canInspectorAdd = true;
        canInspectorRemove = true;
    }
}
