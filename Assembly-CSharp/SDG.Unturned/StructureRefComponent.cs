using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class StructureRefComponent : MonoBehaviour, IExplosionDamageable, IEquatable<IExplosionDamageable>, ICraftingTagProvider
{
    internal StructureDrop tempNotSureIfStructureShouldBeAComponentYet;

    private CraftingTagProviderComponent modHook;

    public bool IsEligibleForExplosionDamage
    {
        get
        {
            StructureDrop structureDrop = tempNotSureIfStructureShouldBeAComponentYet;
            if (structureDrop != null)
            {
                ItemStructureAsset asset = structureDrop.asset;
                if (asset != null && !asset.proofExplosion)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool Equals(IExplosionDamageable obj)
    {
        return this == obj;
    }

    public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
    {
        return CollisionUtil.ClosestPoint(base.gameObject, explosionCenter, includeInactive: false, -4194305);
    }

    public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
    {
        if (!damageParameters.shouldAffectStructures)
        {
            return;
        }
        Vector3 vector = damageParameters.closestPoint - explosionParameters.point;
        float magnitude = vector.magnitude;
        if (!(magnitude > explosionParameters.damageRadius))
        {
            Vector3 direction = vector / magnitude;
            if (!damageParameters.LineOfSightTest(explosionParameters.point, direction, magnitude, out var hit) || !(hit.transform != null) || hit.transform.IsChildOf(base.transform))
            {
                StructureManager.damage(base.transform, direction, explosionParameters.structureDamage, 1f - magnitude / explosionParameters.damageRadius, armor: true, explosionParameters.killer, explosionParameters.damageOrigin);
            }
        }
    }

    public Asset GetTagProviderAsset()
    {
        return tempNotSureIfStructureShouldBeAComponentYet?.asset;
    }

    public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
    {
        ItemPlaceableAsset itemPlaceableAsset = tempNotSureIfStructureShouldBeAComponentYet?.asset;
        if (itemPlaceableAsset != null && itemPlaceableAsset.PlaceableProvidedCraftingTags != null)
        {
            for (int i = 0; i < itemPlaceableAsset.PlaceableProvidedCraftingTags.Length; i++)
            {
                TagAsset tagAsset = itemPlaceableAsset.PlaceableProvidedCraftingTags[i].Get<TagAsset>();
                if (tagAsset != null)
                {
                    p.ResultTags.Add(tagAsset);
                }
            }
        }
        if (modHook != null)
        {
            p.ApplyModHooks(modHook);
        }
    }

    public bool HasAnyCraftingTagsConfigured()
    {
        if (!(modHook != null))
        {
            return !(tempNotSureIfStructureShouldBeAComponentYet?.asset?.PlaceableProvidedCraftingTags.IsNullOrEmpty() ?? true);
        }
        return true;
    }

    public bool Equals(ICraftingTagProvider obj)
    {
        return this == obj;
    }

    private void Start()
    {
        modHook = GetComponent<CraftingTagProviderComponent>();
    }

    void IExplosionDamageable.ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
    {
        ApplyExplosionDamage(in explosionParameters, ref damageParameters);
    }
}
