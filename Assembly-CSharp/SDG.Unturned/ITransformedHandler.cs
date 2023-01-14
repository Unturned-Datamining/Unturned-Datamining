using UnityEngine;

namespace SDG.Unturned;

public interface ITransformedHandler
{
    void OnTransformed(Vector3 oldPosition, Quaternion oldRotation, Vector3 oldLocalScale, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale, bool modifyRotation, bool modifyScale);
}
