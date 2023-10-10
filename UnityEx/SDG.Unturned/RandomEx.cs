using System;
using UnityEngine;

namespace SDG.Unturned;

public static class RandomEx
{
    public static Vector3 GetRandomForwardVectorInCone(float halfAngleRadians)
    {
        halfAngleRadians = Mathf.Min(halfAngleRadians, 1.5697963f);
        float num = Mathf.Sin(halfAngleRadians * Mathf.Sqrt(UnityEngine.Random.value));
        float f = MathF.PI * 2f * UnityEngine.Random.value;
        float num2 = Mathf.Cos(f);
        float num3 = Mathf.Sin(f);
        float num4 = num2 * num;
        float num5 = num3 * num;
        float z = Mathf.Sqrt(1f - num4 * num4 - num5 * num5);
        return new Vector3(num4, num5, z);
    }
}
