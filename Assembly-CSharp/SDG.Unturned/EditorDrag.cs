using UnityEngine;

namespace SDG.Unturned;

public class EditorDrag
{
    private Transform _transform;

    private Vector3 _screen;

    public Transform transform => _transform;

    public Vector3 screen => _screen;

    public EditorDrag(Transform newTransform, Vector3 newScreen)
    {
        _transform = newTransform;
        _screen = newScreen;
    }
}
