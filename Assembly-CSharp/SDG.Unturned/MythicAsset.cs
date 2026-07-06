using System;
using UnityEngine;

namespace SDG.Unturned;

public class MythicAsset : Asset
{
    protected GameObject _systemArea;

    protected GameObject _systemHook;

    protected GameObject _systemFirst;

    protected GameObject _systemThird;

    public string particleTagName { get; protected set; }

    public GameObject systemArea => _systemArea;

    public GameObject systemHook => _systemHook;

    public GameObject systemFirst => _systemFirst;

    public GameObject systemThird => _systemThird;

    /// <summary>
    /// If true, vest and backpack spawn System_Area instead of System_Hook.
    /// </summary>
    public bool ShouldBodyCosmeticsUseAreaPrefab { get; protected set; }

    public override EAssetType assetCategory => EAssetType.MYTHIC;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (id < 500 && !base.OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 500");
        }
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        particleTagName = p.localization.format("Particle_Tag_Name");
        if (string.IsNullOrEmpty(particleTagName))
        {
            particleTagName = name;
        }
        _systemArea = p.bundle.load<GameObject>("System_Area");
        _systemHook = p.bundle.load<GameObject>("System_Hook");
        _systemFirst = p.bundle.load<GameObject>("System_First");
        _systemThird = p.bundle.load<GameObject>("System_Third");
        ShouldBodyCosmeticsUseAreaPrefab = p.data.ParseBool("Body_Cosmetics_Use_System_Area");
        if ((bool)Assets.shouldValidateAssets)
        {
            if (systemArea != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemArea, 10);
                ValidateRecursively(systemArea.transform);
            }
            if (systemHook != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemHook, 10);
                ValidateRecursively(systemHook.transform);
            }
            if (systemFirst != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemFirst, 11);
                ValidateRecursively(systemFirst.transform);
            }
            if (systemThird != null)
            {
                AssetValidation.ValidateLayersEqualRecursive(this, systemThird, 13);
                ValidateRecursively(systemThird.transform);
            }
        }
        if (systemArea == null && systemHook == null && systemFirst == null && systemThird == null)
        {
            Assets.ReportError(this, "missing all effect prefabs");
        }
    }

    private void ValidateRecursively(Transform transform)
    {
        ParticleSystem component = transform.GetComponent<ParticleSystem>();
        if (component != null)
        {
            ParticleSystem.CollisionModule collision = component.collision;
            if (collision.enabled)
            {
                if ((int)collision.collidesWith != 471449600)
                {
                    ReportAssetError("particle system " + transform.GetSceneHierarchyPath() + " collision mask includes unexpected layers");
                }
                if (!MathfEx.IsNearlyZero(collision.colliderForce))
                {
                    ReportAssetError("particle system " + transform.GetSceneHierarchyPath() + " should have zero collider force scale");
                }
            }
            if (!component.useAutoRandomSeed)
            {
                ReportAssetError("particle system " + transform.GetSceneHierarchyPath() + " auto random seed is OFF");
            }
        }
        foreach (Transform item in transform)
        {
            ValidateRecursively(item);
        }
    }
}
