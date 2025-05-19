using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class BarricadeRefComponent : MonoBehaviour, IExplosionDamageable, IEquatable<IExplosionDamageable>, ICraftingTagProvider
{
    internal BarricadeDrop tempNotSureIfBarricadeShouldBeAComponentYet;

    private CraftingTagProviderComponent modHook;

    public bool IsEligibleForExplosionDamage
    {
        get
        {
            BarricadeDrop barricadeDrop = tempNotSureIfBarricadeShouldBeAComponentYet;
            if (barricadeDrop != null)
            {
                ItemBarricadeAsset asset = barricadeDrop.asset;
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
        if (!damageParameters.shouldAffectBarricades)
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
                BarricadeManager.damage(base.transform, explosionParameters.barricadeDamage, 1f - magnitude / explosionParameters.damageRadius, armor: true, explosionParameters.killer, explosionParameters.damageOrigin);
            }
        }
    }

    public Asset GetTagProviderAsset()
    {
        return tempNotSureIfBarricadeShouldBeAComponentYet?.asset;
    }

    public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
    {
        ItemPlaceableAsset itemPlaceableAsset = tempNotSureIfBarricadeShouldBeAComponentYet?.asset;
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
            return !(tempNotSureIfBarricadeShouldBeAComponentYet?.asset?.PlaceableProvidedCraftingTags.IsNullOrEmpty() ?? true);
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
