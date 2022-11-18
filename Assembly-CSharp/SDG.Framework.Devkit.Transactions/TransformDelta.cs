using UnityEngine;

namespace SDG.Framework.Devkit.Transactions;

public class TransformDelta
{
    public Transform parent;

    public Vector3 localPosition;

    public Quaternion localRotation;

    public Vector3 localScale;

    public void get(Transform transform)
    {
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        localScale = transform.localScale;
    }

    public void set(Transform transform)
    {
        transform.parent = parent;
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
        transform.localScale = localScale;
    }

    public TransformDelta(Transform newParent)
    {
        parent = newParent;
    }
}
