using UnityEngine;

namespace SDG.Unturned;

public class EditorSelection
{
    private Transform _transform;

    public Vector3 fromPosition;

    public Quaternion fromRotation;

    public Vector3 fromScale;

    public Matrix4x4 relativeToPivot;

    public Transform transform => _transform;

    public EditorSelection(Transform newTransform, Vector3 newFromPosition, Quaternion newFromRotation, Vector3 newFromScale)
    {
        _transform = newTransform;
        fromPosition = newFromPosition;
        fromRotation = newFromRotation;
        fromScale = newFromScale;
    }
}
