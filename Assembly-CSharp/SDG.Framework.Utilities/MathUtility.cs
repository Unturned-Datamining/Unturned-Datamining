using UnityEngine;

namespace SDG.Framework.Utilities;

public class MathUtility
{
    public static readonly Quaternion IDENTITY_QUATERNION = Quaternion.identity;

    public static readonly Matrix4x4 IDENTITY_MATRIX = Matrix4x4.identity;

    public static void getPitchYawFromDirection(Vector3 direction, out float pitch, out float yaw)
    {
        pitch = 57.29578f * (0f - Mathf.Sin(direction.y / direction.magnitude));
        yaw = 57.29578f * (0f - Mathf.Atan2(direction.z, direction.x)) + 90f;
    }
}
