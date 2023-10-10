using System;
using UnityEngine;

namespace SDG.Framework.Utilities;

public struct SphereVolume : IShapeVolume
{
    public Vector3 center;

    public float radius;

    public Bounds worldBounds
    {
        get
        {
            float num = radius * 2f;
            return new Bounds(center, new Vector3(num, num, num));
        }
    }

    public float internalVolume => 4.1887903f * radius * radius * radius;

    public float surfaceArea => MathF.PI * 4f * radius * radius;

    public bool containsPoint(Vector3 point)
    {
        if (Mathf.Abs(point.x - center.x) >= radius)
        {
            return false;
        }
        if (Mathf.Abs(point.y - center.y) >= radius)
        {
            return false;
        }
        if (Mathf.Abs(point.z - center.z) >= radius)
        {
            return false;
        }
        float num = radius * radius;
        return (point - center).sqrMagnitude < num;
    }

    public SphereVolume(Vector3 newCenter, float newRadius)
    {
        center = newCenter;
        radius = newRadius;
    }
}
