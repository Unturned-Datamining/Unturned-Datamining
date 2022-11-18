using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public static class CollisionUtil
{
    private static List<Collider> getBoundsWorkingList = new List<Collider>();

    private static Collider[] results = new Collider[1];

    public static bool EncapsulateColliderBounds(GameObject gameObject, bool includeInactive, out Bounds bounds)
    {
        getBoundsWorkingList.Clear();
        gameObject.GetComponentsInChildren(includeInactive, getBoundsWorkingList);
        if (getBoundsWorkingList.Count > 0)
        {
            bounds = getBoundsWorkingList[0].bounds;
            for (int i = 1; i < getBoundsWorkingList.Count; i++)
            {
                bounds.Encapsulate(getBoundsWorkingList[i].bounds);
            }
            return true;
        }
        bounds = default(Bounds);
        return false;
    }

    public static Vector3 ClosestPoint(GameObject gameObject, Vector3 position, bool includeInactive)
    {
        getBoundsWorkingList.Clear();
        gameObject.GetComponentsInChildren(includeInactive, getBoundsWorkingList);
        if (getBoundsWorkingList.Count > 0 && ClosestPoint(getBoundsWorkingList, position, out var result))
        {
            return result;
        }
        return gameObject.transform.position;
    }

    public static bool ClosestPoint(IEnumerable<Collider> colliders, Vector3 position, out Vector3 result)
    {
        bool flag = false;
        result = default(Vector3);
        float num = -1f;
        foreach (Collider collider in colliders)
        {
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                continue;
            }
            if (collider is MeshCollider meshCollider)
            {
                if (!meshCollider.convex)
                {
                    continue;
                }
            }
            else if (!(collider is BoxCollider) && !(collider is SphereCollider) && !(collider is CapsuleCollider))
            {
                continue;
            }
            Vector3 vector = collider.ClosestPoint(position);
            float sqrMagnitude = (vector - position).sqrMagnitude;
            if (flag)
            {
                if (sqrMagnitude < num)
                {
                    result = vector;
                    num = sqrMagnitude;
                }
            }
            else
            {
                flag = true;
                result = vector;
                num = sqrMagnitude;
            }
        }
        return flag;
    }

    public static Vector3 ClosestPoint(IEnumerable<Collider> colliders, Vector3 position)
    {
        if (ClosestPoint(colliders, position, out var result))
        {
            return result;
        }
        return position;
    }

    public static int OverlapBoxColliderNonAlloc(BoxCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction)
    {
        return collider.OverlapBoxNonAlloc(results, mask, queryTriggerInteraction);
    }

    public static bool OverlapSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction) > 0;
    }
}
