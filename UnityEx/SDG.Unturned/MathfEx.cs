using System;
using UnityEngine;

namespace SDG.Unturned;

public static class MathfEx
{
    public const float TAU = MathF.PI * 2f;

    public const float HALF_PI = MathF.PI / 2f;

    public static Vector3 Clamp(Vector3 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);
        value.z = Mathf.Clamp(value.z, min, max);
        return value;
    }

    public static bool IsNearlyEqual(float a, float b, float tolerance = 0.01f)
    {
        return Mathf.Abs(b - a) < tolerance;
    }

    public static bool IsAngleDegreesNearlyEqual(float a, float b, float tolerance = 0.1f)
    {
        return Mathf.Abs(Mathf.DeltaAngle(a, b)) < tolerance;
    }

    public static bool IsNearlyZero(float x, float tolerance = 0.01f)
    {
        return Mathf.Abs(x) < tolerance;
    }

    public static bool IsNearlyEqual(Color a, Color b, float tolerance = 0.002f)
    {
        if (IsNearlyEqual(a.r, b.r, tolerance) && IsNearlyEqual(a.g, b.g, tolerance) && IsNearlyEqual(a.b, b.b, tolerance))
        {
            return IsNearlyEqual(a.a, b.a, tolerance);
        }
        return false;
    }

    public static bool IsNearlyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
    {
        if (IsNearlyEqual(a.x, b.x, tolerance) && IsNearlyEqual(a.y, b.y, tolerance))
        {
            return IsNearlyEqual(a.z, b.z, tolerance);
        }
        return false;
    }

    public static bool IsNearlyEqual(Quaternion a, Quaternion b, float tolerance = 0.001f)
    {
        if (IsNearlyEqual(a.x, b.x, tolerance) && IsNearlyEqual(a.y, b.y, tolerance) && IsNearlyEqual(a.z, b.z, tolerance))
        {
            return IsNearlyEqual(a.w, b.w, tolerance);
        }
        return false;
    }

    public static float Square(float x)
    {
        return x * x;
    }

    public static float Cube(float x)
    {
        return x * x * x;
    }

    public static float HorizontalDistanceSquared(Vector3 a, Vector3 b)
    {
        return Square(a.x - b.x) + Square(a.z - b.z);
    }

    public static byte RoundAndClampToByte(float value)
    {
        return (byte)Mathf.Min(Mathf.RoundToInt(value), 255);
    }

    public static sbyte RoundAndClampToSByte(float value)
    {
        return (sbyte)Mathf.Clamp(Mathf.RoundToInt(value), -128, 127);
    }

    public static ushort RoundAndClampToUShort(float value)
    {
        return (ushort)Mathf.Clamp(Mathf.RoundToInt(value), 0, 65535);
    }

    public static short RoundAndClampToShort(float value)
    {
        return (short)Mathf.Clamp(Mathf.RoundToInt(value), -32768, 32767);
    }

    public static uint RoundAndClampToUInt(float value)
    {
        int num = Mathf.RoundToInt(value);
        if (num >= 0)
        {
            return (uint)num;
        }
        return 0u;
    }

    public static Vector2 Clamp01(Vector2 value)
    {
        return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
    }

    public static Color Clamp01(Color value)
    {
        return new Color(Mathf.Clamp01(value.r), Mathf.Clamp01(value.g), Mathf.Clamp01(value.b), Mathf.Clamp01(value.a));
    }

    public static ushort Min(ushort a, ushort b)
    {
        return (ushort)Mathf.Min(a, b);
    }

    public static byte Clamp(byte value, byte min, byte max)
    {
        return (byte)Mathf.Clamp(value, min, max);
    }

    public static ushort Clamp(ushort value, ushort min, ushort max)
    {
        return (ushort)Mathf.Clamp(value, min, max);
    }

    public static float Min(float a, float b, float c)
    {
        return Mathf.Min(Mathf.Min(a, b), c);
    }

    public static float Max(float a, float b, float c)
    {
        return Mathf.Max(Mathf.Max(a, b), c);
    }

    public static uint Min(uint a, uint b)
    {
        if (a >= b)
        {
            return b;
        }
        return a;
    }

    public static byte Min(byte a, byte b)
    {
        if (a >= b)
        {
            return b;
        }
        return a;
    }

    public static byte Max(byte a, byte b)
    {
        if (a <= b)
        {
            return b;
        }
        return a;
    }

    public static uint Max(uint a, uint b)
    {
        if (a <= b)
        {
            return b;
        }
        return a;
    }

    public static byte ClampToByte(int value)
    {
        return (byte)Mathf.Clamp(value, 0, 255);
    }

    public static short ClampToShort(int value)
    {
        return (short)Mathf.Clamp(value, -32768, 32767);
    }

    public static ushort ClampToUShort(int value)
    {
        return (ushort)Mathf.Clamp(value, 0, 65535);
    }

    public static uint ClampToUInt(int value)
    {
        return (uint)Mathf.Clamp(value, 0, int.MaxValue);
    }

    public static int ClampLongToInt(long value, int min, int max)
    {
        if (value >= min)
        {
            if (value <= max)
            {
                return (int)value;
            }
            return max;
        }
        return min;
    }

    public static int ClampLongToInt(long value)
    {
        return ClampLongToInt(value, int.MinValue, int.MaxValue);
    }

    public static uint ClampLongToUInt(long value)
    {
        if (value >= 0)
        {
            if (value <= uint.MaxValue)
            {
                return (uint)value;
            }
            return uint.MaxValue;
        }
        return 0u;
    }

    public static int TruncateToInt(float value)
    {
        if (value >= 0f)
        {
            return Mathf.FloorToInt(value);
        }
        return Mathf.CeilToInt(value);
    }

    public static ushort CeilToUShort(float value)
    {
        return ClampToUShort(Mathf.CeilToInt(value));
    }

    public static uint CeilToUInt(float value)
    {
        return ClampToUInt(Mathf.CeilToInt(value));
    }

    public static Vector2 RandomPositionInCircle(float radius)
    {
        return UnityEngine.Random.insideUnitCircle * radius;
    }

    public static Vector3 RandomPositionInCircleY(Vector3 center, float radius)
    {
        Vector2 vector = RandomPositionInCircle(radius);
        return new Vector3(center.x + vector.x, center.y, center.z + vector.y);
    }

    public static int GetPageCount(int itemCount, int itemsPerPage)
    {
        return (itemCount - 1) / itemsPerPage + 1;
    }

    public static float ProjectRayOntoRay(Vector3 origin0, Vector3 direction0, Vector3 origin1, Vector3 direction1)
    {
        Vector3 rhs = Vector3.Cross(direction1, direction0);
        Vector3 rhs2 = Vector3.Cross(direction0, rhs);
        return Vector3.Dot(origin0 - origin1, rhs2) / Vector3.Dot(direction1, rhs2);
    }

    public static float DistanceBetweenRays(Vector3 origin0, Vector3 direction0, Vector3 origin1, Vector3 direction1)
    {
        return Mathf.Abs(Vector3.Dot(Vector3.Cross(direction0, direction1).normalized, origin1 - origin0));
    }

    public static Vector3 NearestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lhs = lineEnd - lineStart;
        Vector3 rhs = point - lineStart;
        float t = Vector3.Dot(lhs, rhs) / lhs.sqrMagnitude;
        return Vector3.Lerp(lineStart, lineEnd, t);
    }

    public static Vector3 NearestPointOnCircle(Vector3 center, Vector3 normal, float radius, Vector3 point)
    {
        Vector3 normalized = Vector3.ProjectOnPlane(point - center, normal).normalized;
        return center + normalized * radius;
    }

    public static Vector3 InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        return new Vector3(Mathf.InverseLerp(a.x, b.x, value.x), Mathf.InverseLerp(a.y, b.y, value.y), Mathf.InverseLerp(a.z, b.z, value.z));
    }

    public static float SmoothStep01(float t)
    {
        return t * t * (3f - 2f * t);
    }

    public static float SmootherStep01(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }
}
