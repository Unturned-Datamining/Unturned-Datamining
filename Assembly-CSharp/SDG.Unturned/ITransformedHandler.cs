using UnityEngine;

namespace SDG.Unturned;

public interface ITransformedHandler
{
    void OnTranslatedAndRotated(Vector3 oldPosition, Quaternion oldRotation, Vector3 newPosition, Quaternion newRotation, bool modifyRotation);

    void OnTransformed(Matrix4x4 oldLocalToWorldMatrix, Matrix4x4 newLocalToWorldMatrix);
}
