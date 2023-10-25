using System;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Extensions to the built-in Physics class.
///
/// Shares similar functionality to the SDG.Framework.Utilities.PhysicsUtility class, but that should be moved here
/// because the "framework" is unused and and the long name is annoying.
/// </summary>
public static class PhysicsEx
{
    /// <summary>
    /// Wrapper that respects landscape hole volumes.
    /// </summary>
    [Obsolete("Hole collision is handled by Unity now")]
    public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(ray, maxDistance, ref layerMask);
        }
        return Physics.Raycast(ray, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Wrapper that respects landscape hole volumes.
    /// </summary>
    [Obsolete("Hole collision is handled by Unity now")]
    public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.raycastIgnoreLandscapeIfNecessary(new Ray(origin, direction), maxDistance, ref layerMask);
        }
        return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Wrapper that respects landscape hole volumes.
    /// </summary>
    [Obsolete("Hole collision is handled by Unity now")]
    public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if ((layerMask & 0x100000) == 1048576)
        {
            LandscapeHoleUtility.spherecastIgnoreLandscapeIfNecessary(new Ray(origin, direction), radius, maxDistance, ref layerMask);
        }
        return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Wrapper that respects landscape hole volumes.
    /// </summary>
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
