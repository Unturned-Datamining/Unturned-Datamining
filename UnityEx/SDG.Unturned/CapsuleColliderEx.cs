using System;
using UnityEngine;

namespace SDG.Unturned;

public static class CapsuleColliderEx
{
    private static Collider[] singleResult = new Collider[1];

    public static float GetCapsuleVolume(this CapsuleCollider collider)
    {
        float num = Mathf.Abs(collider.radius);
        float num2 = MathF.PI * num * num;
        float num3 = num2 * num;
        return num2 * collider.height + 1.3333334f * num3;
    }

    public static Vector3 TransformCapsuleCenter(this CapsuleCollider collider)
    {
        return collider.transform.TransformPoint(collider.center);
    }

    public static Vector3 GetCapsuleLocalDirection(this CapsuleCollider collider)
    {
        return collider.direction switch
        {
            0 => new Vector3(1f, 0f, 0f), 
            1 => new Vector3(0f, 1f, 0f), 
            2 => new Vector3(0f, 0f, 1f), 
            _ => Vector3.up, 
        };
    }

    public static void GetCapsulePoints(this CapsuleCollider collider, out Vector3 point0, out Vector3 point1)
    {
        Transform transform = collider.transform;
        Vector3 capsuleLocalDirection = collider.GetCapsuleLocalDirection();
        float num = collider.height * 0.5f - collider.radius;
        point0 = transform.TransformPoint(collider.center - capsuleLocalDirection * num);
        point1 = transform.TransformPoint(collider.center + capsuleLocalDirection * num);
    }

    public static int OverlapCapsuleNonAlloc(this CapsuleCollider collider, Collider[] results, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        collider.GetCapsulePoints(out var point, out var point2);
        return Physics.OverlapCapsuleNonAlloc(point, point2, collider.radius, results, mask, queryTriggerInteraction);
    }

    public static Collider OverlapCapsuleSingle(this CapsuleCollider collider, int mask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (collider.OverlapCapsuleNonAlloc(singleResult, mask, queryTriggerInteraction) <= 0)
        {
            return null;
        }
        return singleResult[0];
    }
}
