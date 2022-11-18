using System;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Framework.Utilities;

public class PhysicsUtility
{
    [Obsolete("Hole collision is handled by Unity now")]
    public static bool raycast(Ray ray, out RaycastHit hit, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
        }
        return Physics.Raycast(ray, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static RaycastHit[] raycastAll(Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
        }
        return Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static int sphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(ray, radius, maxDistance, ref layerMask);
        }
        return Physics.SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask, queryTriggerInteraction);
    }
}
