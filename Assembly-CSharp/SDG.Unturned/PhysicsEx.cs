using System;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned;

public static class PhysicsEx
{
    [Obsolete("Hole collision is handled by Unity now")]
    public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
        }
        return Physics.Raycast(ray, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(new Ray(origin, direction), maxDistance, ref layerMask);
        }
        return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(new Ray(origin, direction), radius, maxDistance, ref layerMask);
        }
        return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }

    [Obsolete("Hole collision is handled by Unity now")]
    public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(ray, radius, maxDistance, ref layerMask);
        }
        return Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }
}
