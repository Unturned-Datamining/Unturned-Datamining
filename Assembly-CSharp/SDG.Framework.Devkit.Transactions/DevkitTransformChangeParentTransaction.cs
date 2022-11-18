using UnityEngine;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitTransformChangeParentTransaction : IDevkitTransaction
{
    protected Transform transform;

    protected TransformDelta parentBefore;

    protected TransformDelta parentAfter;

    public bool delta => parentBefore.parent != parentAfter.parent;

    public void undo()
    {
        parentBefore.set(transform);
    }

    public void redo()
    {
        parentAfter.set(transform);
    }

    public void begin()
    {
        parentBefore = new TransformDelta(transform.parent);
        parentBefore.get(transform);
        transform.parent = parentAfter.parent;
        parentAfter.get(transform);
    }

    public void end()
    {
    }

    public void forget()
    {
    }

    public DevkitTransformChangeParentTransaction(Transform newTransform, Transform newParent)
    {
        transform = newTransform;
        parentAfter = new TransformDelta(newParent);
    }
}
