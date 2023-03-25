using UnityEngine;

namespace SDG.Unturned;

public static class SphereColliderEx
{
    private static Collider[] singleResult = new Collider[1];

    public static float GetSphereVolume(this SphereCollider collider)
    {
        return 4.1887903f * MathfEx.Cube(Mathf.Abs(collider.radius));
    }

    public static Vector3 TransformSphereCenter(this SphereCollider collider)
    {
        return collider.transform.TransformPoint(collider.center);
    }

    public static int OverlapSphereNonAlloc(this SphereCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return Physics.OverlapSphereNonAlloc(collider.transform.TransformPoint(collider.center), collider.radius, results, mask, queryTriggerInteraction);
    }

    public static Collider OverlapSphereSingle(this SphereCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (collider.OverlapSphereNonAlloc(singleResult, mask, queryTriggerInteraction) <= 0)
        {
            return null;
        }
        return singleResult[0];
    }
}
