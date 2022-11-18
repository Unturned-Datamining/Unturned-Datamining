using System;
using UnityEngine;

namespace SDG.Framework.Utilities;

public struct AACylinderVolume : IShapeVolume
{
    public Vector3 center;

    public float radius;

    public float height;

    public Bounds worldBounds
    {
        get
        {
            float num = radius * 2f;
            return new Bounds(center, new Vector3(num, height, num));
        }
    }

    public float internalVolume => height * (float)Math.PI * radius * radius;

    public float surfaceArea => (float)Math.PI * radius * radius;

    public bool containsPoint(Vector3 point)
    {
        float num = height / 2f;
        if (point.y > center.y - num && point.y < center.y + num)
        {
            float num2 = radius * radius;
            return (new Vector2(point.x, point.z) - new Vector2(center.x, center.z)).sqrMagnitude < num2;
        }
        return false;
    }

    public AACylinderVolume(Vector3 newCenter, float newRadius, float newHeight)
    {
        center = newCenter;
        radius = newRadius;
        height = newHeight;
    }
}
