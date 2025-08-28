using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public static class CraftingTagPhysicsUtil
{
    private static Collider[] colliders = new Collider[256];

    private static HashSet<ICraftingTagProvider> tagProviders = new HashSet<ICraftingTagProvider>();

    private static HashSet<TagAsset> pendingTags = new HashSet<TagAsset>();

    public static void QueryTagProviders(Vector3 position, float radius, HashSet<ICraftingTagProvider> results)
    {
        if (radius < float.Epsilon)
        {
            return;
        }
        int num = Physics.OverlapSphereNonAlloc(position, radius, colliders, 470007808, QueryTriggerInteraction.Collide);
        if (num < 1)
        {
            return;
        }
        if (num == colliders.Length)
        {
            colliders = Physics.OverlapSphere(position, radius, 470007808, QueryTriggerInteraction.Collide);
            num = colliders.Length;
        }
        for (int i = 0; i < num; i++)
        {
            Transform transform = colliders[i]?.transform;
            if (transform == null)
            {
                continue;
            }
            ICraftingTagProvider componentInParent = transform.GetComponentInParent<ICraftingTagProvider>();
            if (componentInParent == null || !componentInParent.HasAnyCraftingTagsConfigured())
            {
                continue;
            }
            Transform transform2 = (componentInParent as Component)?.transform;
            if (!(transform2 == null))
            {
                int num2 = 268468224;
                num2 &= ~(1 << transform2.gameObject.layer);
                if (!Physics.Linecast(position, transform2.position, out var _, num2, QueryTriggerInteraction.Ignore))
                {
                    results.Add(componentInParent);
                }
            }
        }
    }

    public static void QueryAvailableTags(Vector3 position, float radius, HashSet<TagAsset> results)
    {
        tagProviders.Clear();
        QueryTagProviders(position, radius, tagProviders);
        CraftingTagProviderGetAvailableTagsParameters p = default(CraftingTagProviderGetAvailableTagsParameters);
        p.ResultTags = pendingTags;
        foreach (ICraftingTagProvider tagProvider in tagProviders)
        {
            pendingTags.Clear();
            tagProvider.GetAvailableTags(ref p);
            foreach (TagAsset pendingTag in pendingTags)
            {
                results.Add(pendingTag);
            }
        }
    }

    public static bool IsTagAvailableAtPosition(Vector3 position, float radius, TagAsset tag)
    {
        if (tag == null)
        {
            return false;
        }
        tagProviders.Clear();
        QueryTagProviders(position, radius, tagProviders);
        CraftingTagProviderGetAvailableTagsParameters p = default(CraftingTagProviderGetAvailableTagsParameters);
        p.ResultTags = pendingTags;
        foreach (ICraftingTagProvider tagProvider in tagProviders)
        {
            pendingTags.Clear();
            tagProvider.GetAvailableTags(ref p);
            if (pendingTags.Contains(tag))
            {
                return true;
            }
        }
        return false;
    }
}
