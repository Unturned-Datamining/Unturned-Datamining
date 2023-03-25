using UnityEngine;

namespace SDG.Unturned;

public static class RaycastHitEx
{
    public static string ToDebugString(this RaycastHit hit)
    {
        return $"Collider: {hit.collider} Rigidbody: {hit.rigidbody} Transform: {hit.transform.GetSceneHierarchyPath()} Position: {hit.point} Normal: {hit.normal}";
    }
}
