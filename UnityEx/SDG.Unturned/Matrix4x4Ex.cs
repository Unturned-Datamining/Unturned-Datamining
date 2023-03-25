using UnityEngine;

namespace SDG.Unturned;

public static class Matrix4x4Ex
{
    public static Vector3 GetPosition(this Matrix4x4 matrix)
    {
        Vector3 result = default(Vector3);
        result.x = matrix.m03;
        result.y = matrix.m13;
        result.z = matrix.m23;
        return result;
    }

    public static Quaternion GetRotation(this Matrix4x4 matrix)
    {
        Vector3 forward = default(Vector3);
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
        Vector3 upwards = default(Vector3);
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
        return Quaternion.LookRotation(forward, upwards);
    }
}
