namespace UnityEngine;

public static class Vector2Ex
{
    public static Vector2 Cross(this Vector2 vector)
    {
        return new Vector2(vector.y, 0f - vector.x);
    }
}
