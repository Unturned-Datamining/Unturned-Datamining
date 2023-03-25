using UnityEngine;

namespace SDG.Unturned;

public static class QuaternionEx
{
    public static bool IsNormalized(this Quaternion quaternion, float threshold = 0.001f)
    {
        return quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w - 1f < threshold * threshold;
    }

    public static Quaternion GetRoundedIfNearlyAxisAligned(this Quaternion quaternion, float tolerance = 0.05f)
    {
        Vector3 eulerAngles = quaternion.eulerAngles;
        Vector3 euler = eulerAngles;
        euler.x = Mathf.RoundToInt(euler.x * (1f / 90f)) * 90;
        euler.y = Mathf.RoundToInt(euler.y * (1f / 90f)) * 90;
        euler.z = Mathf.RoundToInt(euler.z * (1f / 90f)) * 90;
        if (MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.x, euler.x, tolerance) && MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.y, euler.y, tolerance) && MathfEx.IsAngleDegreesNearlyEqual(eulerAngles.z, euler.z, tolerance))
        {
            return Quaternion.Euler(euler);
        }
        return quaternion;
    }
}
