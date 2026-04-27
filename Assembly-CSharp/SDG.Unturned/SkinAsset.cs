using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SkinAsset : Asset
{
    protected bool _isPattern;

    protected bool _hasSight;

    protected bool _hasTactical;

    protected bool _hasGrip;

    protected bool _hasBarrel;

    protected bool _hasMagazine;

    protected Material _primarySkin;

    protected Dictionary<ushort, Material> _secondarySkins;

    protected Material _attachmentSkin;

    protected Material _tertiarySkin;

    public List<Mesh> overrideMeshes;

    public bool hasStatTrackerTransformOverride;

    public Vector3 statTrackerPosition;

    public Quaternion statTrackerRotation;

    public bool hasIconTransformOverride;

    public Vector3 iconPosition;

    public Quaternion iconRotation;

    /// <summary>
    /// Used by melee skins to override impact sound.
    /// </summary>
    internal AudioReference specialAudioOverride;

    public bool isPattern => _isPattern;

    public bool hasSight => _hasSight;

    public bool hasTactical => _hasTactical;

    public bool hasGrip => _hasGrip;

    public bool hasBarrel => _hasBarrel;

    public bool hasMagazine => _hasMagazine;

    public Material primarySkin => _primarySkin;

    public Dictionary<ushort, Material> secondarySkins => _secondarySkins;

    public Material attachmentSkin => _attachmentSkin;

    public Material tertiarySkin => _tertiarySkin;

    /// <summary>
    /// Used by dawn and dusk skins which pull per-level lighting colors.
    /// </summary>
    public ELightingTime? lightingTime { get; private set; }

    public override EAssetType assetCategory => EAssetType.SKIN;

    public ERagdollEffect ragdollEffect { get; protected set; }

    /// <summary>
    /// If true, sets the Magazine attachment hook inactive while this skin is applied. (guns only)
    ///
    /// Nelson 2025-03-10: Adding this to address mismatched Ace bullets with certain skins. (public issue #4923)
    /// It should be fine for vanilla guns because there shouldn't be assumptions about Magazine enable/disable,
    /// but modded guns may have different expectations (particularly with GunAttachmentEventHook).
    /// </summary>
    public bool ShouldHideMagazine { get; protected set; }

    /// <summary>
    /// Note: unfortunately it appears the stupid skin system always instantiated materials, but never destroys
    /// them... will need to clean this up, but it will be tricky because the game does not hold a reference to them.
    /// </summary>
    public void SetMaterialProperties(Material instance)
    {
        if (lightingTime.HasValue && LevelLighting.times != null)
        {
            LightingInfo lightingInfo = LevelLighting.times[(int)lightingTime.Value];
            instance.SetVector("_SunColor", lightingInfo.colors[0] * 1.5f);
            instance.SetVector("_RaysColor", lightingInfo.colors[10] * 1.5f);
            instance.SetVector("_SkyColor", lightingInfo.colors[3]);
            instance.SetVector("_EquatorColor", lightingInfo.colors[4]);
            instance.SetVector("_GroundColor", lightingInfo.colors[5]);
        }
    }

    public SkinAsset()
    {
    }

    public SkinAsset(bool isPattern, Material primarySkin, Dictionary<ushort, Material> secondarySkins, Material attachmentSkin, Material tertiarySkin)
    {
        _isPattern = isPattern;
        _hasSight = true;
        _hasTactical = true;
        _hasGrip = true;
        _hasBarrel = true;
        _hasMagazine = true;
        _primarySkin = primarySkin;
        _secondarySkins = secondarySkins;
        _attachmentSkin = attachmentSkin;
        _tertiarySkin = tertiarySkin;
        overrideMeshes = new List<Mesh>(0);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        _isPattern = p.data.ContainsKey("Pattern");
        if (p.data.ContainsKey("LightingTime"))
        {
            lightingTime = p.data.ParseEnum("LightingTime", ELightingTime.DAWN);
        }
        else
        {
            lightingTime = null;
        }
        _hasSight = p.data.ContainsKey("Sight");
        _hasTactical = p.data.ContainsKey("Tactical");
        _hasGrip = p.data.ContainsKey("Grip");
        _hasBarrel = p.data.ContainsKey("Barrel");
        _hasMagazine = p.data.ContainsKey("Magazine");
        ShouldHideMagazine = p.data.ParseBool("Hide_Magazine");
        ragdollEffect = p.data.ParseEnum("Ragdoll_Effect", ERagdollEffect.None);
        specialAudioOverride = p.data.ReadAudioReference("SpecialAudioOverrideDef", p.bundle);
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        _primarySkin = loadRequiredAsset<Material>(p.bundle, "Skin_Primary");
        _secondarySkins = new Dictionary<ushort, Material>();
        ushort num = p.data.ParseUInt16("Secondary_Skins", 0);
        for (ushort num2 = 0; num2 < num; num2++)
        {
            ushort key = p.data.ParseUInt16("Secondary_" + num2, 0);
            if (!secondarySkins.ContainsKey(key))
            {
                Material value = loadRequiredAsset<Material>(p.bundle, "Skin_Secondary_" + key);
                secondarySkins.Add(key, value);
            }
        }
        _attachmentSkin = p.bundle.load<Material>("Skin_Attachment");
        _tertiarySkin = p.bundle.load<Material>("Skin_Tertiary");
        if (attachmentSkin != null && tertiarySkin == null)
        {
            Assets.ReportError(this, "has Skin_Attachment material without a Skin_Tertiary material");
        }
        ushort num3 = p.data.ParseUInt16("Override_Meshes", 0);
        overrideMeshes = new List<Mesh>(num3);
        for (ushort num4 = 0; num4 < num3; num4++)
        {
            GameObject gameObject = p.bundle.load<GameObject>("Override_Mesh_" + num4);
            if (gameObject != null)
            {
                MeshFilter component = gameObject.GetComponent<MeshFilter>();
                if (component != null)
                {
                    if (component.sharedMesh != null)
                    {
                        overrideMeshes.Add(component.sharedMesh);
                    }
                    else
                    {
                        Assets.reportError("missing MeshFilter sharedMesh on " + gameObject.name);
                    }
                }
                else
                {
                    Assets.reportError("missing MeshFilter on " + gameObject.name);
                }
                Transform transform = gameObject.transform.Find("Stat_Tracker");
                if (transform != null)
                {
                    hasStatTrackerTransformOverride = true;
                    statTrackerPosition = transform.localPosition;
                    statTrackerRotation = transform.localRotation;
                }
                Transform transform2 = gameObject.transform.Find("Icon");
                if (transform2 != null)
                {
                    hasIconTransformOverride = true;
                    iconPosition = transform2.localPosition;
                    iconRotation = transform2.localRotation;
                }
            }
            else
            {
                Assets.reportError("missing Override_Mesh_" + num4);
            }
        }
    }
}
