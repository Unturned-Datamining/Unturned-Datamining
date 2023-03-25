using UnityEngine;

namespace SDG.Unturned;

public static class BoxColliderExtension
{
    private static Collider[] singleResult = new Collider[1];

    public static float GetBoxVolume(this BoxCollider collider)
    {
        Vector3 size = collider.size;
        return Mathf.Abs(size.x) * Mathf.Abs(size.y) * Mathf.Abs(size.z);
    }

    public static Vector3 TransformBoxCenter(this BoxCollider collider)
    {
        return collider.transform.TransformPoint(collider.center);
    }

    public static int OverlapBoxNonAlloc(this BoxCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        Transform transform = collider.transform;
        return Physics.OverlapBoxNonAlloc(transform.TransformPoint(collider.center), collider.size * 0.5f, results, transform.rotation, mask, queryTriggerInteraction);
    }

    public static Collider OverlapBoxSingle(this BoxCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (collider.OverlapBoxNonAlloc(singleResult, mask, queryTriggerInteraction) <= 0)
        {
            return null;
        }
        return singleResult[0];
    }
}
