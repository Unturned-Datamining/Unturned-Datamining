using UnityEngine;

namespace SDG.Unturned;

public static class Vector3Ex
{
    public static bool IsNormalized(this Vector3 vector, float threshold = 0.001f)
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z - 1f < threshold * threshold;
    }

    public static bool ContainsNaN(this Vector3 vector)
    {
        if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y))
        {
            return float.IsNaN(vector.z);
        }
        return true;
    }

    public static bool ContainsInfinity(this Vector3 vector)
    {
        if (!float.IsInfinity(vector.x) && !float.IsInfinity(vector.y))
        {
            return float.IsInfinity(vector.z);
        }
        return true;
    }

    public static bool IsFinite(this Vector3 vector)
    {
        if (!vector.ContainsNaN())
        {
            return !vector.ContainsInfinity();
        }
        return false;
    }

    public static bool IsNearlyZero(this Vector3 vector, float tolerance = 0.001f)
    {
        return MathfEx.IsNearlyZero(vector.x, tolerance) & MathfEx.IsNearlyZero(vector.y, tolerance) & MathfEx.IsNearlyZero(vector.z, tolerance);
    }

    public static bool IsNearlyEqual(this Vector3 vector, Vector3 other, float tolerance = 0.001f)
    {
        return MathfEx.IsNearlyEqual(vector.x, other.x, tolerance) & MathfEx.IsNearlyEqual(vector.y, other.y, tolerance) & MathfEx.IsNearlyEqual(vector.z, other.z, tolerance);
    }

    public static bool AreComponentsNearlyEqual(this Vector3 vector, float tolerance = 0.001f)
    {
        return MathfEx.IsNearlyEqual(vector.x, vector.y, tolerance) & MathfEx.IsNearlyEqual(vector.y, vector.z, tolerance);
    }

    public static Vector3 GetRoundedIfNearlyEqualToOne(this Vector3 vector, float tolerance = 0.001f)
    {
        if (MathfEx.IsNearlyEqual(vector.x, 1f, tolerance))
        {
            vector.x = 1f;
        }
        else if (MathfEx.IsNearlyEqual(vector.x, -1f, tolerance))
        {
            vector.x = -1f;
        }
        if (MathfEx.IsNearlyEqual(vector.y, 1f, tolerance))
        {
            vector.y = 1f;
        }
        else if (MathfEx.IsNearlyEqual(vector.y, -1f, tolerance))
        {
            vector.y = -1f;
        }
        if (MathfEx.IsNearlyEqual(vector.z, 1f, tolerance))
        {
            vector.z = 1f;
        }
        else if (MathfEx.IsNearlyEqual(vector.z, -1f, tolerance))
        {
            vector.z = -1f;
        }
        return vector;
    }

    public static Vector3 GetAbs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static float GetMin(this Vector3 vector)
    {
        return MathfEx.Min(vector.x, vector.y, vector.z);
    }

    public static float GetMax(this Vector3 vector)
    {
        return MathfEx.Max(vector.x, vector.y, vector.z);
    }

    public static Vector3 GetHorizontal(this Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    public static float GetHorizontalMagnitude(this Vector3 vector)
    {
        return Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
    }

    public static float GetHorizontalSqrMagnitude(this Vector3 vector)
    {
        return vector.x * vector.x + vector.z * vector.z;
    }

    public static Vector3 ClampHorizontalMagnitude(this Vector3 vector, float maxMagnitude)
    {
        if (maxMagnitude <= 0f)
        {
            return Vector3.zero;
        }
        float horizontalSqrMagnitude = vector.GetHorizontalSqrMagnitude();
        float num = maxMagnitude * maxMagnitude;
        if (horizontalSqrMagnitude > num)
        {
            float num2 = maxMagnitude / Mathf.Sqrt(horizontalSqrMagnitude);
            return new Vector3(vector.x * num2, vector.y, vector.z * num2);
        }
        return vector;
    }

    public static Vector3 ClampMagnitude(this Vector3 vector, float maxMagnitude)
    {
        if (maxMagnitude <= 0f)
        {
            return Vector3.zero;
        }
        float sqrMagnitude = vector.sqrMagnitude;
        float num = MathfEx.Square(maxMagnitude);
        if (sqrMagnitude > num)
        {
            float num2 = Mathf.Sqrt(sqrMagnitude);
            return vector * (maxMagnitude / num2);
        }
        return vector;
    }

    public static bool TryParseVector3(string s, out Vector3 result)
    {
        if (string.IsNullOrEmpty(s))
        {
            result = default(Vector3);
            return false;
        }
        int num = s.IndexOf('(');
        int num3;
        int num4;
        if (num >= 0)
        {
            int num2 = s.IndexOf(')', num + 2);
            if (num2 < 0)
            {
                result = default(Vector3);
                return false;
            }
            num3 = num + 1;
            num4 = num2 - 1;
        }
        else
        {
            num3 = 0;
            num4 = s.Length - 1;
        }
        int num5 = s.IndexOf(',', num3);
        if (num5 < 0 || num5 + 2 > num4)
        {
            result = default(Vector3);
            return false;
        }
        int num6 = s.IndexOf(',', num5 + 2);
        if (num6 < 0 || num6 + 1 > num4)
        {
            result = default(Vector3);
            return false;
        }
        if (!float.TryParse(s.Substring(num3, num5 - num3), out result.x))
        {
            result = default(Vector3);
            return false;
        }
        if (!float.TryParse(s.Substring(num5 + 1, num6 - num5 - 1), out result.y))
        {
            result = default(Vector3);
            return false;
        }
        if (!float.TryParse(s.Substring(num6 + 1, num4 - num6), out result.z))
        {
            result = default(Vector3);
            return false;
        }
        return true;
    }
}
