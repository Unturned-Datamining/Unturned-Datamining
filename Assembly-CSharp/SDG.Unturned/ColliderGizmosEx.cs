using UnityEngine;

namespace SDG.Unturned;

public static class ColliderGizmosEx
{
    public static void DrawCapsuleGizmo(this CapsuleCollider collider, Color color, float lifespan = 0f)
    {
        collider.GetCapsulePoints(out var point, out var point2);
        RuntimeGizmos.Get().Capsule(point, point2, collider.radius, color, lifespan);
    }

    public static void DrawSphereGizmo(this SphereCollider collider, Color color, float lifespan = 0f)
    {
        Vector3 center = collider.TransformSphereCenter();
        RuntimeGizmos.Get().Sphere(center, collider.radius, color, lifespan);
    }
}
