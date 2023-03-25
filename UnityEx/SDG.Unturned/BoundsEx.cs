using UnityEngine;

namespace SDG.Unturned;

public static class BoundsEx
{
    public static bool ContainsXZ(this Bounds bounds, Vector3 point)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        if (point.x >= center.x - extents.x && point.x <= center.x + extents.x && point.z >= center.z - extents.z)
        {
            return point.z <= center.z + extents.z;
        }
        return false;
    }

    public static float CalculateVolume(this Bounds bounds)
    {
        Vector3 size = bounds.size;
        return size.x * size.y * size.z;
    }
}
