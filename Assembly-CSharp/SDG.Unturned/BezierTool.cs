using UnityEngine;

namespace SDG.Unturned;

public class BezierTool
{
    /// <param name="a">Start Vertex</param>
    /// <param name="b">Start Vertex + Start Tangent</param>
    /// <param name="c">End Vertex + End Tangent</param>
    /// <param name="d">End Vertex</param>
    public static Vector3 getPosition(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        t = Mathf.Clamp01(t);
        float num = 1f - t;
        return num * num * num * a + 3f * num * num * t * b + 3f * num * t * t * c + t * t * t * d;
    }

    /// <param name="a">Start Vertex</param>
    /// <param name="b">Start Vertex + Start Tangent</param>
    /// <param name="c">End Vertex + End Tangent</param>
    /// <param name="d">End Vertex</param>
    public static Vector3 getVelocity(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        t = Mathf.Clamp01(t);
        float num = 1f - t;
        return 3f * num * num * (b - a) + 6f * num * t * (c - b) + 3f * t * t * (d - c);
    }

    /// <param name="a">Start Vertex</param>
    /// <param name="b">Start Vertex + Start Tangent</param>
    /// <param name="c">End Vertex + End Tangent</param>
    /// <param name="d">End Vertex</param>
    public static float getLengthEstimate(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return ((d - a).magnitude + (d - c).magnitude + (b - c).magnitude + (b - a).magnitude) / 2f;
    }

    /// <param name="a">Start Vertex</param>
    /// <param name="b">Start Vertex + Start Tangent</param>
    /// <param name="c">End Vertex + End Tangent</param>
    /// <param name="d">End Vertex</param>
    /// <param name="distance">World units length along curve.</param>
    /// <param name="uniformInterval">Spacing between points.</param>
    /// <param name="intervalTolerance">Max estimate distance from uniform interval before we have to retry.</param>
    /// <param name="attempts">How many times to retry if the estimate is too far off.</param>
    /// <param name="cachedLength">If length is already known pass it in, otherwise it's recalculated.</param>
    /// <returns>Time along curve. [0-1]</returns>
    public static float getTFromDistance(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float distance, float uniformInterval, float intervalTolerance, int attempts, float cachedLength = -1f)
    {
        if (distance < 0f)
        {
            return 0f;
        }
        float num = ((!(cachedLength < 0f)) ? cachedLength : getLengthEstimate(a, b, c, d));
        if (distance >= num)
        {
            return 1f;
        }
        attempts = Mathf.Max(1, attempts);
        int num2 = Mathf.CeilToInt(num / uniformInterval);
        float num3 = 1f / (float)num2;
        float num4 = num3;
        float num5 = uniformInterval;
        Vector3 vector = a;
        double num6 = 0.0;
        double num7 = 0.0;
        while (num7 < 1.0)
        {
            Vector3 vector2 = vector;
            for (int i = 0; i < attempts; i++)
            {
                float t = (float)(num7 + (double)num4);
                vector2 = getPosition(a, a + b, d + c, d, t);
                num5 = (vector2 - vector).magnitude;
                if (i < attempts - 1)
                {
                    if (Mathf.Abs(num5 - uniformInterval) < intervalTolerance)
                    {
                        break;
                    }
                    num4 *= intervalTolerance / num5;
                }
            }
            double num8 = num6 + (double)num5;
            if ((double)distance >= num6 && (double)distance <= num8)
            {
                float num9 = (float)(((double)distance - num6) / (double)num5);
                num9 *= num4;
                return (float)(num7 + (double)num9);
            }
            vector = vector2;
            num6 += (double)num5;
            num7 += (double)num4;
            num4 = num3;
        }
        UnturnedLog.warn("Failed to find T along curve from distance!\nDistance: " + distance + " Length: " + num + "\nInterval: " + uniformInterval + " Tolerance: " + intervalTolerance + " Attempts: " + attempts);
        return 0.5f;
    }

    /// <param name="a">Start Vertex</param>
    /// <param name="b">Start Vertex + Start Tangent</param>
    /// <param name="c">End Vertex + End Tangent</param>
    /// <param name="d">End Vertex</param>
    /// <param name="uniformInterval">Spacing between points.</param>
    /// <param name="intervalTolerance">Max estimate distance from uniform interval before we have to retry.</param>
    /// <param name="attempts">How many times to retry if the estimate is too far off.</param>
    public static float getLengthBruteForce(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float uniformInterval, float intervalTolerance, int attempts)
    {
        float lengthEstimate = getLengthEstimate(a, b, c, d);
        attempts = Mathf.Max(1, attempts);
        int num = Mathf.CeilToInt(lengthEstimate / uniformInterval);
        float num2 = 1f / (float)num;
        float num3 = num2;
        float num4 = uniformInterval;
        Vector3 vector = a;
        double num5 = 0.0;
        double num6 = 0.0;
        while (num6 < 1.0)
        {
            Vector3 vector2 = vector;
            for (int i = 0; i < attempts; i++)
            {
                float t = (float)(num6 + (double)num3);
                vector2 = getPosition(a, a + b, d + c, d, t);
                num4 = (vector2 - vector).magnitude;
                if (i < attempts - 1)
                {
                    if (Mathf.Abs(num4 - uniformInterval) < intervalTolerance)
                    {
                        break;
                    }
                    if (i < attempts - 1)
                    {
                        num3 *= intervalTolerance / num4;
                    }
                }
            }
            UnturnedLog.info(num4);
            vector = vector2;
            num5 += (double)num4;
            num6 += (double)num3;
            num3 = num2;
        }
        return (float)num5;
    }
}
