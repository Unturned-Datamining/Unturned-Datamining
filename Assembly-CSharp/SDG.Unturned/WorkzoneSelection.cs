using UnityEngine;

namespace SDG.Unturned;

public class WorkzoneSelection
{
    private Transform _transform;

    public Vector3 fromPosition;

    public Vector3 preTransformPosition;

    public Quaternion preTransformRotation;

    public Transform transform => _transform;

    public WorkzoneSelection(Transform newTransform)
    {
        _transform = newTransform;
    }
}
