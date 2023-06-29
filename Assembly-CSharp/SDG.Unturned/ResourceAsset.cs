using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class ResourceAsset : Asset
{
    private static List<MeshFilter> meshes = new List<MeshFilter>();

    private static Shader shader;

    protected string _resourceName;

    protected GameObject _modelGameObject;

    protected GameObject _stumpGameObject;

    protected GameObject _skyboxGameObject;

    protected GameObject _debrisGameObject;

    public ushort health;

    public uint rewardXP;

    public float radius;

    public float scale;

    public float verticalOffset;

    private Guid _explosionGuid;

    public ushort explosion;

    public ushort log;

    public ushort stick;

    public byte rewardMin;

    public byte rewardMax;

    public ushort rewardID;

    public bool isForage;

    public uint forageRewardExperience;

    public string interactabilityText;

    public bool hasDebris;

    public byte bladeID;

    public float reset;

    public bool isSpeedTree;

    public bool defaultLODWeights;

    public AssetReference<ResourceAsset> christmasRedirect;

    public AssetReference<ResourceAsset> halloweenRedirect;

    public EObjectChart chart;

    public bool shouldExcludeFromLevelBatching;

    public string resourceName => holidayRestriction switch
    {
        ENPCHoliday.HALLOWEEN => _resourceName + " [HW]", 
        ENPCHoliday.CHRISTMAS => _resourceName + " [XMAS]", 
        _ => _resourceName, 
    };

    public override string FriendlyName => resourceName;

    public GameObject modelGameObject => _modelGameObject;

    public GameObject stumpGameObject => _stumpGameObject;

    public GameObject skyboxGameObject => _skyboxGameObject;

    public GameObject debrisGameObject => _debrisGameObject;

    public Material skyboxMaterial { get; private set; }

    public Guid explosionGuid => _explosionGuid;

    public bool vulnerableToFists { get; protected set; }

    public bool vulnerableToAllMeleeWeapons { get; protected set; }

    public override EAssetType assetCategory => EAssetType.RESOURCE;

    public ENPCHoliday holidayRestriction { get; protected set; }

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, explosion);
    }

    public AssetReference<ResourceAsset> getHolidayRedirect()
    {
        return HolidayUtil.getActiveHoliday() switch
        {
            ENPCHoliday.CHRISTMAS => christmasRedirect, 
            ENPCHoliday.HALLOWEEN => halloweenRedirect, 
            _ => AssetReference<ResourceAsset>.invalid, 
        };
    }

    protected void applyDefaultLODs(LODGroup lod, bool fade)
    {
        LOD[] lODs = lod.GetLODs();
        lODs[0].screenRelativeTransitionHeight = (fade ? 0.7f : 0.6f);
        lODs[1].screenRelativeTransitionHeight = (fade ? 0.5f : 0.4f);
        lODs[2].screenRelativeTransitionHeight = 0.15f;
        lODs[3].screenRelativeTransitionHeight = 0.03f;
        lod.SetLODs(lODs);
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (id < 50 && !base.OriginAllowsVanillaLegacyId && !data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 50");
        }
        if (Dedicator.IsDedicatedServer || GraphicsSettings.treeMode == ETreeGraphicMode.LEGACY)
        {
            isSpeedTree = false;
        }
        else
        {
            isSpeedTree = data.ContainsKey("SpeedTree");
        }
        defaultLODWeights = data.ContainsKey("SpeedTree_Default_LOD_Weights");
        _resourceName = localization.format("Name");
        if (Dedicator.IsDedicatedServer)
        {
            if (data.ParseBool("Has_Clip_Prefab", defaultValue: true))
            {
                _modelGameObject = bundle.load<GameObject>("Resource_Clip");
                if (_modelGameObject == null)
                {
                    Assets.reportError(this, "missing \"Resource_Clip\" GameObject, loading \"Resource\" GameObject instead");
                }
                _stumpGameObject = bundle.load<GameObject>("Stump_Clip");
                if (_stumpGameObject == null)
                {
                    Assets.reportError(this, "missing \"Stump_Clip\" GameObject, loading \"Stump\" GameObject instead");
                }
            }
            if (_modelGameObject == null)
            {
                _modelGameObject = bundle.load<GameObject>("Resource");
                if (_modelGameObject == null)
                {
                    Assets.reportError(this, "missing \"Resource\" GameObject");
                }
                else
                {
                    ServerPrefabUtil.RemoveClientComponents(_modelGameObject);
                }
            }
            if (_stumpGameObject == null)
            {
                _stumpGameObject = bundle.load<GameObject>("Stump");
                if (_stumpGameObject == null)
                {
                    Assets.reportError(this, "missing \"Stump\" GameObject");
                }
                else
                {
                    ServerPrefabUtil.RemoveClientComponents(_stumpGameObject);
                }
            }
        }
        else
        {
            _modelGameObject = null;
            _stumpGameObject = null;
            _skyboxGameObject = null;
            _debrisGameObject = null;
            if (GraphicsSettings.treeMode == ETreeGraphicMode.LEGACY)
            {
                _modelGameObject = bundle.load<GameObject>("Resource_Old");
            }
            if (_modelGameObject == null)
            {
                _modelGameObject = bundle.load<GameObject>("Resource");
            }
            if (defaultLODWeights)
            {
                Transform transform = modelGameObject.transform.Find("Billboard");
                if (transform != null)
                {
                    BillboardRenderer component = transform.GetComponent<BillboardRenderer>();
                    if (component != null)
                    {
                        component.shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
            }
            if (GraphicsSettings.treeMode == ETreeGraphicMode.LEGACY)
            {
                _stumpGameObject = bundle.load<GameObject>("Stump_Old");
            }
            if (_stumpGameObject == null)
            {
                _stumpGameObject = bundle.load<GameObject>("Stump");
            }
            if (GraphicsSettings.treeMode == ETreeGraphicMode.LEGACY)
            {
                _skyboxGameObject = bundle.load<GameObject>("Skybox_Old");
            }
            if (_skyboxGameObject == null)
            {
                _skyboxGameObject = bundle.load<GameObject>("Skybox");
            }
            if (defaultLODWeights)
            {
                Transform transform2 = skyboxGameObject.transform.Find("Model_0");
                if (transform2 != null)
                {
                    BillboardRenderer component2 = transform2.GetComponent<BillboardRenderer>();
                    if (component2 != null)
                    {
                        component2.shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
            }
            if (GraphicsSettings.treeMode == ETreeGraphicMode.LEGACY)
            {
                _debrisGameObject = bundle.load<GameObject>("Debris_Old");
            }
            if (isSpeedTree)
            {
                if (_debrisGameObject == null)
                {
                    _debrisGameObject = bundle.load<GameObject>("Debris");
                }
                if (modelGameObject != null)
                {
                    LODGroup component3 = modelGameObject.GetComponent<LODGroup>();
                    if (component3 != null)
                    {
                        if (GraphicsSettings.treeMode == ETreeGraphicMode.SPEEDTREE_FADE_SPEEDTREE)
                        {
                            component3.fadeMode = LODFadeMode.SpeedTree;
                            if (defaultLODWeights && GraphicsSettings.treeMode != 0)
                            {
                                applyDefaultLODs(component3, fade: true);
                            }
                        }
                        else
                        {
                            component3.fadeMode = LODFadeMode.None;
                            if (defaultLODWeights && GraphicsSettings.treeMode != 0)
                            {
                                applyDefaultLODs(component3, fade: false);
                            }
                        }
                    }
                }
                if (stumpGameObject != null)
                {
                    LODGroup component4 = stumpGameObject.GetComponent<LODGroup>();
                    if (component4 != null)
                    {
                        component4.fadeMode = LODFadeMode.None;
                    }
                }
                if (debrisGameObject != null)
                {
                    LODGroup component5 = debrisGameObject.GetComponent<LODGroup>();
                    if (component5 != null)
                    {
                        if (GraphicsSettings.treeMode == ETreeGraphicMode.SPEEDTREE_FADE_SPEEDTREE)
                        {
                            component5.fadeMode = LODFadeMode.SpeedTree;
                            if (defaultLODWeights && GraphicsSettings.treeMode != 0)
                            {
                                applyDefaultLODs(component5, fade: true);
                            }
                        }
                        else
                        {
                            component5.fadeMode = LODFadeMode.None;
                            if (defaultLODWeights && GraphicsSettings.treeMode != 0)
                            {
                                applyDefaultLODs(component5, fade: false);
                            }
                        }
                    }
                }
            }
            if (data.ContainsKey("Auto_Skybox") && !isSpeedTree && (bool)skyboxGameObject)
            {
                Transform transform3 = modelGameObject.transform.Find("Model_0");
                if ((bool)transform3)
                {
                    meshes.Clear();
                    transform3.GetComponentsInChildren(includeInactive: true, meshes);
                    if (meshes.Count > 0)
                    {
                        Bounds bounds = default(Bounds);
                        for (int i = 0; i < meshes.Count; i++)
                        {
                            Mesh sharedMesh = meshes[i].sharedMesh;
                            if (!(sharedMesh == null))
                            {
                                Bounds bounds2 = sharedMesh.bounds;
                                bounds.Encapsulate(bounds2.min);
                                bounds.Encapsulate(bounds2.max);
                            }
                        }
                        if (bounds.min.y < 0f)
                        {
                            float num = Mathf.Abs(bounds.min.z);
                            bounds.center += new Vector3(0f, 0f, num / 2f);
                            bounds.size -= new Vector3(0f, 0f, num);
                        }
                        float num2 = Mathf.Max(bounds.size.x, bounds.size.y);
                        float z = bounds.size.z;
                        skyboxGameObject.transform.localScale = new Vector3(z, z, z);
                        Transform transform4 = UnityEngine.Object.Instantiate(modelGameObject).transform;
                        Transform transform5 = new GameObject().transform;
                        transform5.parent = transform4;
                        transform5.localPosition = new Vector3(0f, z / 2f, (0f - num2) / 2f);
                        transform5.localRotation = Quaternion.identity;
                        Transform transform6 = new GameObject().transform;
                        transform6.parent = transform4;
                        transform6.localPosition = new Vector3((0f - num2) / 2f, z / 2f, 0f);
                        transform6.localRotation = Quaternion.Euler(0f, 90f, 0f);
                        if (!shader)
                        {
                            shader = Shader.Find("Custom/Card");
                        }
                        Texture2D card = ItemTool.getCard(transform4, transform5, transform6, 64, 64, z / 2f, num2);
                        skyboxMaterial = new Material(shader);
                        skyboxMaterial.mainTexture = card;
                    }
                }
            }
        }
        if (_modelGameObject != null)
        {
            _modelGameObject.SetTagIfUntaggedRecursively("Resource");
        }
        if (_stumpGameObject != null)
        {
            _stumpGameObject.SetTagIfUntaggedRecursively("Resource");
        }
        if (_skyboxGameObject != null)
        {
            _skyboxGameObject.SetTagIfUntaggedRecursively("Resource");
        }
        health = data.ParseUInt16("Health", 0);
        radius = data.ParseFloat("Radius");
        scale = Mathf.Abs(data.ParseFloat("Scale"));
        verticalOffset = data.ParseFloat("Vertical_Offset", -0.75f);
        explosion = data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
        log = data.ParseUInt16("Log", 0);
        stick = data.ParseUInt16("Stick", 0);
        rewardID = data.ParseUInt16("Reward_ID", 0);
        rewardXP = data.ParseUInt32("Reward_XP");
        if (data.ContainsKey("Reward_Min"))
        {
            rewardMin = data.ParseUInt8("Reward_Min", 0);
        }
        else
        {
            rewardMin = 6;
        }
        if (data.ContainsKey("Reward_Max"))
        {
            rewardMax = data.ParseUInt8("Reward_Max", 0);
        }
        else
        {
            rewardMax = 9;
        }
        bladeID = data.ParseUInt8("BladeID", 0);
        vulnerableToFists = data.ParseBool("Vulnerable_To_Fists");
        vulnerableToAllMeleeWeapons = data.ParseBool("Vulnerable_To_All_Melee_Weapons");
        reset = data.ParseFloat("Reset");
        isForage = data.ContainsKey("Forage");
        if (isForage && _modelGameObject != null)
        {
            Transform transform7 = _modelGameObject.transform.Find("Forage");
            if (transform7 != null)
            {
                transform7.gameObject.layer = 14;
            }
        }
        forageRewardExperience = data.ParseUInt32("Forage_Reward_Experience", 1u);
        if (isForage)
        {
            interactabilityText = localization.read("Interact");
            interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
        }
        hasDebris = !data.ContainsKey("No_Debris");
        if (data.ContainsKey("Holiday_Restriction"))
        {
            holidayRestriction = (ENPCHoliday)Enum.Parse(typeof(ENPCHoliday), data.GetString("Holiday_Restriction"), ignoreCase: true);
            if (holidayRestriction == ENPCHoliday.NONE)
            {
                Assets.reportError(this, "has no holiday restriction, so value is ignored");
            }
        }
        else
        {
            holidayRestriction = ENPCHoliday.NONE;
        }
        christmasRedirect = data.readAssetReference<ResourceAsset>("Christmas_Redirect");
        halloweenRedirect = data.readAssetReference<ResourceAsset>("Halloween_Redirect");
        chart = data.ParseEnum("Chart", EObjectChart.NONE);
        shouldExcludeFromLevelBatching = data.ParseBool("Exclude_From_Level_Batching");
        shouldExcludeFromLevelBatching |= isSpeedTree;
    }
}
