using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageCut
{
    internal FoliageBounds foliageBounds;

    private Vector3 center;

    private float radius;

    private float height;

    public bool ContainsPoint(Vector3 position)
    {
        float num = height * 0.5f;
        if (position.y > center.y - num && position.y < center.y + num)
        {
            float num2 = radius * radius;
            return (new Vector2(position.x, position.z) - new Vector2(center.x, center.z)).sqrMagnitude < num2;
        }
        return false;
    }

    public FoliageCut(Vector3 center, float radius, float height)
    {
        this.center = center;
        this.radius = radius;
        this.height = height;
        float num = radius * 2f;
        Bounds worldBounds = new Bounds(center, new Vector3(num, height, num));
        foliageBounds = new FoliageBounds(worldBounds);
    }
}
