using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal static class PrefabUtil
{
    private static List<Collider> workingColliders = new List<Collider>();

    public static void DestroyCollidersInChildren(GameObject gameObject, bool includeInactive)
    {
        gameObject.GetComponentsInChildren(includeInactive, workingColliders);
        foreach (Collider workingCollider in workingColliders)
        {
            Object.Destroy(workingCollider);
        }
        workingColliders.Clear();
    }
}
