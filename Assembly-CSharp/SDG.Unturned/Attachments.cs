using System;
using UnityEngine;

namespace SDG.Unturned;

public class Attachments : MonoBehaviour
{
    private ItemGunAsset _gunAsset;

    private SkinAsset _skinAsset;

    private ushort _sightID;

    private ushort _tacticalID;

    private ushort _gripID;

    private ushort _barrelID;

    private ushort _magazineID;

    private ItemSightAsset _sightAsset;

    private ItemTacticalAsset _tacticalAsset;

    private ItemGripAsset _gripAsset;

    private ItemBarrelAsset _barrelAsset;

    private ItemMagazineAsset _magazineAsset;

    private Transform _sightModel;

    private Transform _tacticalModel;

    private Transform _gripModel;

    private Transform _barrelModel;

    private Transform _magazineModel;

    private Transform _sightHook;

    private Transform _viewHook;

    private Transform _tacticalHook;

    private Transform _gripHook;

    private Transform _barrelHook;

    private Transform _magazineHook;

    private Transform _ejectHook;

    private Transform _lightHook;

    private Transform _light2Hook;

    private Transform _aimHook;

    private Transform _scopeHook;

    private Transform _reticuleHook;

    private Transform _leftHook;

    private Transform _rightHook;

    private Transform _nockHook;

    private Transform _restHook;

    private LineRenderer _rope;

    public bool isSkinned;

    public bool shouldDestroyColliders;

    private bool wasSkinned;

    private Material tempSightMaterial;

    private Material tempTacticalMaterial;

    private Material tempGripMaterial;

    private Material tempBarrelMaterial;

    private Material tempMagazineMaterial;

    private Material instantiatedSightSkin;

    private Material instantiatedTacticalSkin;

    private Material instantiatedGripSkin;

    private Material instantiatedBarrelSkin;

    private Material instantiatedMagazineSkin;

    private Material reticuleMaterial;

    public ItemGunAsset gunAsset => _gunAsset;

    public SkinAsset skinAsset => _skinAsset;

    public ushort sightID => _sightID;

    public ushort tacticalID => _tacticalID;

    public ushort gripID => _gripID;

    public ushort barrelID => _barrelID;

    public ushort magazineID => _magazineID;

    public ItemSightAsset sightAsset => _sightAsset;

    public ItemTacticalAsset tacticalAsset => _tacticalAsset;

    public ItemGripAsset gripAsset => _gripAsset;

    public ItemBarrelAsset barrelAsset => _barrelAsset;

    public ItemMagazineAsset magazineAsset => _magazineAsset;

    public Transform sightModel => _sightModel;

    public Transform tacticalModel => _tacticalModel;

    public Transform gripModel => _gripModel;

    public Transform barrelModel => _barrelModel;

    public Transform magazineModel => _magazineModel;

    public Transform sightHook => _sightHook;

    public Transform viewHook => _viewHook;

    public Transform tacticalHook => _tacticalHook;

    public Transform gripHook => _gripHook;

    public Transform barrelHook => _barrelHook;

    public Transform magazineHook => _magazineHook;

    public Transform ejectHook => _ejectHook;

    public Transform lightHook => _lightHook;

    public Transform light2Hook => _light2Hook;

    public Transform aimHook => _aimHook;

    public Transform scopeHook => _scopeHook;

    public Transform reticuleHook => _reticuleHook;

    public Transform leftHook => _leftHook;

    public Transform rightHook => _rightHook;

    public Transform nockHook => _nockHook;

    public Transform restHook => _restHook;

    public LineRenderer rope => _rope;

    public void applyVisual()
    {
        if (isSkinned != wasSkinned)
        {
            wasSkinned = isSkinned;
            if (tempSightMaterial != null)
            {
                HighlighterTool.rematerialize(sightModel, tempSightMaterial, out tempSightMaterial);
            }
            if (tempTacticalMaterial != null)
            {
                HighlighterTool.rematerialize(tacticalModel, tempTacticalMaterial, out tempTacticalMaterial);
            }
            if (tempGripMaterial != null)
            {
                HighlighterTool.rematerialize(gripModel, tempGripMaterial, out tempGripMaterial);
            }
            if (tempBarrelMaterial != null)
            {
                HighlighterTool.rematerialize(barrelModel, tempBarrelMaterial, out tempBarrelMaterial);
            }
            if (tempMagazineMaterial != null)
            {
                HighlighterTool.rematerialize(magazineModel, tempMagazineMaterial, out tempMagazineMaterial);
            }
        }
    }

    public void updateGun(ItemGunAsset newGunAsset, SkinAsset newSkinAsset)
    {
        _gunAsset = newGunAsset;
        _skinAsset = newSkinAsset;
    }

    public static void parseFromItemState(byte[] state, out ushort sight, out ushort tactical, out ushort grip, out ushort barrel, out ushort magazine)
    {
        if (state == null || state.Length < 10)
        {
            sight = 0;
            tactical = 0;
            grip = 0;
            barrel = 0;
            magazine = 0;
        }
        else
        {
            sight = BitConverter.ToUInt16(state, 0);
            tactical = BitConverter.ToUInt16(state, 2);
            grip = BitConverter.ToUInt16(state, 4);
            barrel = BitConverter.ToUInt16(state, 6);
            magazine = BitConverter.ToUInt16(state, 8);
        }
    }

    public void updateAttachments(byte[] state, bool viewmodel)
    {
        if (state == null || state.Length != 18)
        {
            return;
        }
        base.transform.localScale = Vector3.one;
        parseFromItemState(state, out _sightID, out _tacticalID, out _gripID, out _barrelID, out _magazineID);
        DestroySkinMaterials();
        if (sightModel != null)
        {
            UnityEngine.Object.Destroy(sightModel.gameObject);
            _sightModel = null;
        }
        try
        {
            _sightAsset = Assets.find(EAssetType.ITEM, sightID) as ItemSightAsset;
        }
        catch
        {
            _sightAsset = null;
        }
        tempSightMaterial = null;
        if (sightAsset != null && sightHook != null && sightAsset.sight != null)
        {
            _sightModel = UnityEngine.Object.Instantiate(sightAsset.sight).transform;
            sightModel.name = sightAsset.GUID.ToString("N");
            sightModel.transform.parent = sightHook;
            sightModel.transform.localPosition = Vector3.zero;
            sightModel.transform.localRotation = Quaternion.identity;
            sightModel.localScale = Vector3.one;
            if (shouldDestroyColliders && sightAsset.shouldDestroyAttachmentColliders)
            {
                PrefabUtil.DestroyCollidersInChildren(sightModel.gameObject, includeInactive: true);
            }
            sightModel.DestroyRigidbody();
            if (viewmodel)
            {
                Layerer.viewmodel(sightModel);
            }
            if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
            {
                Material value = null;
                if (!skinAsset.secondarySkins.TryGetValue(sightID, out value) && skinAsset.hasSight && sightAsset.isPaintable)
                {
                    if (sightAsset.albedoBase != null && skinAsset.attachmentSkin != null)
                    {
                        instantiatedSightSkin = UnityEngine.Object.Instantiate(skinAsset.attachmentSkin);
                        sightAsset.applySkinBaseTextures(instantiatedSightSkin);
                        skinAsset.SetMaterialProperties(instantiatedSightSkin);
                        value = instantiatedSightSkin;
                    }
                    else if (skinAsset.tertiarySkin != null)
                    {
                        instantiatedSightSkin = UnityEngine.Object.Instantiate(skinAsset.tertiarySkin);
                        skinAsset.SetMaterialProperties(instantiatedSightSkin);
                        value = instantiatedSightSkin;
                    }
                }
                if (value != null)
                {
                    HighlighterTool.rematerialize(sightModel, value, out tempSightMaterial);
                }
            }
        }
        if (tacticalModel != null)
        {
            UnityEngine.Object.Destroy(tacticalModel.gameObject);
            _tacticalModel = null;
        }
        try
        {
            _tacticalAsset = Assets.find(EAssetType.ITEM, tacticalID) as ItemTacticalAsset;
        }
        catch
        {
            _tacticalAsset = null;
        }
        tempTacticalMaterial = null;
        if (tacticalAsset != null && tacticalHook != null && tacticalAsset.tactical != null)
        {
            _tacticalModel = UnityEngine.Object.Instantiate(tacticalAsset.tactical).transform;
            tacticalModel.name = tacticalAsset.GUID.ToString("N");
            tacticalModel.transform.parent = tacticalHook;
            tacticalModel.transform.localPosition = Vector3.zero;
            tacticalModel.transform.localRotation = Quaternion.identity;
            tacticalModel.localScale = Vector3.one;
            if (shouldDestroyColliders && tacticalAsset.shouldDestroyAttachmentColliders)
            {
                PrefabUtil.DestroyCollidersInChildren(tacticalModel.gameObject, includeInactive: true);
            }
            tacticalModel.DestroyRigidbody();
            if (viewmodel)
            {
                Layerer.viewmodel(tacticalModel);
            }
            if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
            {
                Material value2 = null;
                if (!skinAsset.secondarySkins.TryGetValue(tacticalID, out value2) && skinAsset.hasTactical && tacticalAsset.isPaintable)
                {
                    if (tacticalAsset.albedoBase != null && skinAsset.attachmentSkin != null)
                    {
                        instantiatedTacticalSkin = UnityEngine.Object.Instantiate(skinAsset.attachmentSkin);
                        tacticalAsset.applySkinBaseTextures(instantiatedTacticalSkin);
                        skinAsset.SetMaterialProperties(instantiatedTacticalSkin);
                        value2 = instantiatedTacticalSkin;
                    }
                    else if (skinAsset.tertiarySkin != null)
                    {
                        instantiatedTacticalSkin = UnityEngine.Object.Instantiate(skinAsset.tertiarySkin);
                        skinAsset.SetMaterialProperties(instantiatedTacticalSkin);
                        value2 = instantiatedTacticalSkin;
                    }
                }
                if (value2 != null)
                {
                    HighlighterTool.rematerialize(tacticalModel, value2, out tempTacticalMaterial);
                }
            }
        }
        if (gripModel != null)
        {
            UnityEngine.Object.Destroy(gripModel.gameObject);
            _gripModel = null;
        }
        try
        {
            _gripAsset = Assets.find(EAssetType.ITEM, gripID) as ItemGripAsset;
        }
        catch
        {
            _gripAsset = null;
        }
        tempGripMaterial = null;
        if (gripAsset != null && gripHook != null && gripAsset.grip != null)
        {
            _gripModel = UnityEngine.Object.Instantiate(gripAsset.grip).transform;
            gripModel.name = gripAsset.GUID.ToString("N");
            gripModel.transform.parent = gripHook;
            gripModel.transform.localPosition = Vector3.zero;
            gripModel.transform.localRotation = Quaternion.identity;
            gripModel.localScale = Vector3.one;
            if (shouldDestroyColliders && gripAsset.shouldDestroyAttachmentColliders)
            {
                PrefabUtil.DestroyCollidersInChildren(gripModel.gameObject, includeInactive: true);
            }
            gripModel.DestroyRigidbody();
            if (viewmodel)
            {
                Layerer.viewmodel(gripModel);
            }
            if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
            {
                Material value3 = null;
                if (!skinAsset.secondarySkins.TryGetValue(gripID, out value3) && skinAsset.hasGrip && gripAsset.isPaintable)
                {
                    if (gripAsset.albedoBase != null && skinAsset.attachmentSkin != null)
                    {
                        instantiatedGripSkin = UnityEngine.Object.Instantiate(skinAsset.attachmentSkin);
                        gripAsset.applySkinBaseTextures(instantiatedGripSkin);
                        skinAsset.SetMaterialProperties(instantiatedGripSkin);
                        value3 = instantiatedGripSkin;
                    }
                    else if (skinAsset.tertiarySkin != null)
                    {
                        instantiatedGripSkin = UnityEngine.Object.Instantiate(skinAsset.tertiarySkin);
                        skinAsset.SetMaterialProperties(instantiatedGripSkin);
                        value3 = instantiatedGripSkin;
                    }
                }
                if (value3 != null)
                {
                    HighlighterTool.rematerialize(gripModel, value3, out tempGripMaterial);
                }
            }
        }
        if (barrelModel != null)
        {
            UnityEngine.Object.Destroy(barrelModel.gameObject);
            _barrelModel = null;
        }
        try
        {
            _barrelAsset = Assets.find(EAssetType.ITEM, barrelID) as ItemBarrelAsset;
        }
        catch
        {
            _barrelAsset = null;
        }
        tempBarrelMaterial = null;
        if (barrelAsset != null && barrelHook != null && barrelAsset.barrel != null)
        {
            _barrelModel = UnityEngine.Object.Instantiate(barrelAsset.barrel).transform;
            barrelModel.name = barrelAsset.GUID.ToString("N");
            barrelModel.transform.parent = barrelHook;
            barrelModel.transform.localPosition = Vector3.zero;
            barrelModel.transform.localRotation = Quaternion.identity;
            barrelModel.localScale = Vector3.one;
            if (shouldDestroyColliders && barrelAsset.shouldDestroyAttachmentColliders)
            {
                PrefabUtil.DestroyCollidersInChildren(barrelModel.gameObject, includeInactive: true);
            }
            barrelModel.DestroyRigidbody();
            if (viewmodel)
            {
                Layerer.viewmodel(barrelModel);
            }
            if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
            {
                Material value4 = null;
                if (!skinAsset.secondarySkins.TryGetValue(barrelID, out value4) && skinAsset.hasBarrel && barrelAsset.isPaintable)
                {
                    if (barrelAsset.albedoBase != null && skinAsset.attachmentSkin != null)
                    {
                        instantiatedBarrelSkin = UnityEngine.Object.Instantiate(skinAsset.attachmentSkin);
                        barrelAsset.applySkinBaseTextures(instantiatedBarrelSkin);
                        skinAsset.SetMaterialProperties(instantiatedBarrelSkin);
                        value4 = instantiatedBarrelSkin;
                    }
                    else if (skinAsset.tertiarySkin != null)
                    {
                        instantiatedBarrelSkin = UnityEngine.Object.Instantiate(skinAsset.tertiarySkin);
                        skinAsset.SetMaterialProperties(instantiatedBarrelSkin);
                        value4 = instantiatedBarrelSkin;
                    }
                }
                if (value4 != null)
                {
                    HighlighterTool.rematerialize(barrelModel, value4, out tempBarrelMaterial);
                }
            }
        }
        if (magazineModel != null)
        {
            UnityEngine.Object.Destroy(magazineModel.gameObject);
            _magazineModel = null;
        }
        try
        {
            _magazineAsset = Assets.find(EAssetType.ITEM, magazineID) as ItemMagazineAsset;
        }
        catch
        {
            _magazineAsset = null;
        }
        tempMagazineMaterial = null;
        if (magazineAsset != null && magazineHook != null && magazineAsset.magazine != null)
        {
            Transform transform = null;
            if (magazineAsset.calibers.Length != 0)
            {
                transform = magazineHook.Find("Caliber_" + magazineAsset.calibers[0]);
            }
            if (transform == null)
            {
                transform = magazineHook;
            }
            _magazineModel = UnityEngine.Object.Instantiate(magazineAsset.magazine).transform;
            magazineModel.name = magazineAsset.GUID.ToString("N");
            magazineModel.transform.parent = transform;
            magazineModel.transform.localPosition = Vector3.zero;
            magazineModel.transform.localRotation = Quaternion.identity;
            magazineModel.localScale = Vector3.one;
            if (shouldDestroyColliders && magazineAsset.shouldDestroyAttachmentColliders)
            {
                PrefabUtil.DestroyCollidersInChildren(magazineModel.gameObject, includeInactive: true);
            }
            magazineModel.DestroyRigidbody();
            if (viewmodel)
            {
                Layerer.viewmodel(magazineModel);
            }
            if (gunAsset != null && skinAsset != null && skinAsset.secondarySkins != null)
            {
                Material value5 = null;
                if (!skinAsset.secondarySkins.TryGetValue(magazineID, out value5) && skinAsset.hasMagazine && magazineAsset.isPaintable)
                {
                    if (magazineAsset.albedoBase != null && skinAsset.attachmentSkin != null)
                    {
                        instantiatedMagazineSkin = UnityEngine.Object.Instantiate(skinAsset.attachmentSkin);
                        magazineAsset.applySkinBaseTextures(instantiatedMagazineSkin);
                        skinAsset.SetMaterialProperties(instantiatedMagazineSkin);
                        value5 = instantiatedMagazineSkin;
                    }
                    else if (skinAsset.tertiarySkin != null)
                    {
                        instantiatedMagazineSkin = UnityEngine.Object.Instantiate(skinAsset.tertiarySkin);
                        skinAsset.SetMaterialProperties(instantiatedMagazineSkin);
                        value5 = instantiatedMagazineSkin;
                    }
                }
                if (value5 != null)
                {
                    HighlighterTool.rematerialize(magazineModel, value5, out tempMagazineMaterial);
                }
            }
        }
        if (tacticalModel != null && tacticalModel.childCount > 0)
        {
            Transform transform2 = tacticalModel.Find("Model_0");
            _lightHook = transform2?.Find("Light");
            _light2Hook = transform2?.Find("Light2");
            if (viewmodel)
            {
                if (lightHook != null)
                {
                    lightHook.tag = "Viewmodel";
                    lightHook.gameObject.layer = 11;
                    Transform transform3 = lightHook.Find("Light");
                    if (transform3 != null)
                    {
                        transform3.tag = "Viewmodel";
                        transform3.gameObject.layer = 11;
                    }
                }
                if (light2Hook != null)
                {
                    light2Hook.tag = "Viewmodel";
                    light2Hook.gameObject.layer = 11;
                    Transform transform4 = light2Hook.Find("Light");
                    if (transform4 != null)
                    {
                        transform4.tag = "Viewmodel";
                        transform4.gameObject.layer = 11;
                    }
                }
            }
            else
            {
                LightLODTool.applyLightLOD(lightHook);
                LightLODTool.applyLightLOD(light2Hook);
            }
        }
        else
        {
            _lightHook = null;
            _light2Hook = null;
        }
        if (sightModel != null)
        {
            Transform transform5 = sightModel.Find("Model_0");
            _aimHook = transform5?.Find("Aim");
            if (aimHook != null)
            {
                Transform transform6 = aimHook.parent.Find("Reticule");
                if (transform6 != null)
                {
                    Renderer component = transform6.GetComponent<Renderer>();
                    if (component != null)
                    {
                        reticuleMaterial = component.material;
                        if (reticuleMaterial != null)
                        {
                            reticuleMaterial.SetColor("_Color", OptionsSettings.criticalHitmarkerColor);
                            reticuleMaterial.SetColor("_EmissionColor", OptionsSettings.criticalHitmarkerColor);
                        }
                    }
                }
            }
            _reticuleHook = transform5?.Find("Reticule");
        }
        else
        {
            _aimHook = null;
            _reticuleHook = null;
        }
        if (aimHook != null)
        {
            _scopeHook = aimHook.Find("Scope");
        }
        else
        {
            _scopeHook = null;
        }
        if (rope != null && viewmodel)
        {
            rope.tag = "Viewmodel";
            rope.gameObject.layer = 11;
        }
        wasSkinned = true;
        applyVisual();
    }

    private void Awake()
    {
        _sightHook = base.transform.Find("Sight");
        _viewHook = base.transform.Find("View");
        _tacticalHook = base.transform.Find("Tactical");
        _gripHook = base.transform.Find("Grip");
        _barrelHook = base.transform.Find("Barrel");
        _magazineHook = base.transform.Find("Magazine");
        _ejectHook = base.transform.Find("Eject");
        _leftHook = base.transform.Find("Left");
        _rightHook = base.transform.Find("Right");
        _nockHook = base.transform.Find("Nock");
        _restHook = base.transform.Find("Rest");
        Transform transform = base.transform.Find("Rope");
        if (transform != null)
        {
            _rope = (LineRenderer)transform.GetComponent<Renderer>();
        }
    }

    private void DestroySkinMaterials()
    {
        if (instantiatedSightSkin != null)
        {
            UnityEngine.Object.Destroy(instantiatedSightSkin);
            instantiatedSightSkin = null;
        }
        if (instantiatedTacticalSkin != null)
        {
            UnityEngine.Object.Destroy(instantiatedTacticalSkin);
            instantiatedTacticalSkin = null;
        }
        if (instantiatedGripSkin != null)
        {
            UnityEngine.Object.Destroy(instantiatedGripSkin);
            instantiatedGripSkin = null;
        }
        if (instantiatedBarrelSkin != null)
        {
            UnityEngine.Object.Destroy(instantiatedBarrelSkin);
            instantiatedBarrelSkin = null;
        }
        if (instantiatedMagazineSkin != null)
        {
            UnityEngine.Object.Destroy(instantiatedMagazineSkin);
            instantiatedMagazineSkin = null;
        }
        if (reticuleMaterial != null)
        {
            UnityEngine.Object.Destroy(reticuleMaterial);
            reticuleMaterial = null;
        }
    }

    private void OnDestroy()
    {
        DestroySkinMaterials();
    }
}
