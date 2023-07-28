using System;
using System.Collections.Generic;
using System.Text;
using SDG.Framework.Foliage;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ObjectAsset : Asset
{
    protected string _objectName;

    public EObjectType type;

    private GameObject loadedModel;

    private bool hasLoadedModel;

    private IDeferredAsset<GameObject> clientModel;

    private IDeferredAsset<GameObject> legacyServerModel;

    public IDeferredAsset<GameObject> skyboxGameObject;

    public IDeferredAsset<GameObject> navGameObject;

    public IDeferredAsset<GameObject> slotsGameObject;

    public IDeferredAsset<GameObject> triggersGameObject;

    public bool isSnowshoe;

    public bool shouldExcludeFromCullingVolumes;

    public bool shouldExcludeFromLevelBatching;

    public EObjectChart chart;

    public bool isFuel;

    public bool isRefill;

    public bool isSoft;

    public bool causesFallDamage;

    public bool isCollisionImportant;

    public bool useScale;

    public EObjectLOD lod;

    public float lodBias;

    public Vector3 cullingVolumeLocalPositionOffset;

    public Vector3 cullingVolumeSizeOffset;

    public INPCCondition[] conditions;

    public EObjectInteractability interactability;

    public bool interactabilityRemote;

    public float interactabilityDelay;

    public bool interactabilityEmission;

    public EObjectInteractabilityHint interactabilityHint;

    public string interactabilityText;

    public EObjectInteractabilityPower interactabilityPower;

    public EObjectInteractabilityEditor interactabilityEditor;

    public EObjectInteractabilityNav interactabilityNav;

    public float interactabilityReset;

    public ushort interactabilityResource;

    private byte[] interactabilityResourceState;

    public ushort[] interactabilityDrops;

    public ushort interactabilityRewardID;

    public Guid interactabilityEffectGuid;

    [Obsolete]
    public ushort interactabilityEffect;

    public INPCCondition[] interactabilityConditions;

    protected NPCRewardsList interactabilityRewards;

    public EObjectRubble rubble;

    public float rubbleReset;

    public ushort rubbleHealth;

    public Guid rubbleEffectGuid;

    [Obsolete]
    public ushort rubbleEffect;

    public Guid rubbleFinaleGuid;

    [Obsolete]
    public ushort rubbleFinale;

    public EObjectRubbleEditor rubbleEditor;

    public ushort rubbleRewardID;

    public byte rubbleBladeID;

    public float rubbleRewardProbability;

    public byte rubbleRewardsMin;

    public byte rubbleRewardsMax;

    public uint rubbleRewardXP;

    public bool rubbleIsVulnerable;

    public bool rubbleProofExplosion;

    public AssetReference<FoliageInfoCollectionAsset> foliage;

    public bool useWaterHeightTransparentSort;

    public AssetReference<MaterialPaletteAsset> materialPalette;

    public EGraphicQuality landmarkQuality;

    public bool shouldAddNightLightScript;

    public bool shouldAddKillTriggers;

    public bool allowStructures;

    public bool isGore;

    public AssetReference<ObjectAsset> christmasRedirect;

    public AssetReference<ObjectAsset> halloweenRedirect;

    private static HashSet<ushort> tempAssociatedFlags = new HashSet<ushort>();

    private static List<MeshCollider> navMCs = new List<MeshCollider>();

    public string objectName => holidayRestriction switch
    {
        ENPCHoliday.HALLOWEEN => _objectName + " [HW]", 
        ENPCHoliday.CHRISTMAS => _objectName + " [XMAS]", 
        ENPCHoliday.APRIL_FOOLS => _objectName + " [AF]", 
        ENPCHoliday.VALENTINES => _objectName + " [V]", 
        ENPCHoliday.PRIDE_MONTH => _objectName + " [PM]", 
        _ => _objectName, 
    };

    public override string FriendlyName => objectName;

    public ENPCHoliday holidayRestriction { get; protected set; }

    public override EAssetType assetCategory => EAssetType.OBJECT;

    public GameObject GetOrLoadModel()
    {
        if (!hasLoadedModel)
        {
            hasLoadedModel = true;
            if (legacyServerModel != null)
            {
                loadedModel = legacyServerModel.getOrLoad();
                if (loadedModel == null)
                {
                    loadedModel = clientModel.getOrLoad();
                }
            }
            else
            {
                loadedModel = clientModel.getOrLoad();
            }
        }
        return loadedModel;
    }

    protected void validateModel(GameObject asset)
    {
        if (Mathf.Abs(asset.transform.localScale.x - 1f) > 0.01f || Mathf.Abs(asset.transform.localScale.y - 1f) > 0.01f || Mathf.Abs(asset.transform.localScale.z - 1f) > 0.01f)
        {
            useScale = false;
            Assets.reportError(this, "should have a scale of one");
        }
        else
        {
            useScale = true;
        }
        Transform transform = asset.transform.Find("Block");
        if (transform != null && transform.GetComponent<Collider>() != null && transform.GetComponent<Collider>().sharedMaterial == null)
        {
            Assets.reportError(this, "has a 'Block' collider but no physics material");
        }
        Transform transform2 = asset.transform.Find("Model_0");
        string expectedTag = string.Empty;
        int num = -1;
        if (type == EObjectType.SMALL)
        {
            expectedTag = "Small";
            num = 17;
        }
        else if (type == EObjectType.MEDIUM)
        {
            expectedTag = "Medium";
            num = 16;
        }
        else if (type == EObjectType.LARGE)
        {
            expectedTag = "Large";
            num = 15;
        }
        if (num == -1)
        {
            Assets.reportError(this, "has an unknown tag/layer because it has an unhandled EObjectType");
        }
        else
        {
            fixTagAndLayer(asset, expectedTag, num);
            if (transform2 != null)
            {
                fixTagAndLayer(transform2.gameObject, expectedTag, num);
            }
            AssetValidation.searchGameObjectForErrors(this, asset);
        }
        if (interactability == EObjectInteractability.BINARY_STATE && (bool)Assets.shouldValidateAssets)
        {
            Animation animation = asset.transform.Find("Root")?.GetComponent<Animation>();
            if (animation != null)
            {
                validateAnimation(animation, "Open");
                validateAnimation(animation, "Close");
            }
        }
    }

    protected void OnServerModelLoaded(GameObject asset)
    {
        if (asset == null && type != EObjectType.SMALL)
        {
            Assets.reportError(this, "missing \"Clip\" GameObject, loading \"Object\" GameObject instead");
        }
        if (asset != null)
        {
            validateModel(asset);
        }
    }

    protected void OnClientModelLoaded(GameObject asset)
    {
        if (asset == null)
        {
            Assets.reportError(this, "missing \"Object\" GameObject");
            return;
        }
        validateModel(asset);
        if (Dedicator.IsDedicatedServer)
        {
            ServerPrefabUtil.RemoveClientComponents(asset);
        }
    }

    protected void onNavGameObjectLoaded(GameObject asset)
    {
        if (asset == null && type == EObjectType.LARGE)
        {
            Assets.reportError(this, "missing Nav GameObject. Highly recommended to fix.");
        }
        if (asset != null)
        {
            fixTagAndLayer(asset, "Navmesh", 22);
            if ((bool)Assets.shouldValidateAssets)
            {
                ensureNavMeshReadable();
            }
        }
    }

    protected void onSlotsGameObjectLoaded(GameObject asset)
    {
        if (asset != null)
        {
            asset.SetTagIfUntaggedRecursively("Logic");
        }
    }

    public EffectAsset FindInteractabilityEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(interactabilityEffectGuid, interactabilityEffect);
    }

    public EffectAsset FindRubbleEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(rubbleEffectGuid, rubbleEffect);
    }

    public bool IsRubbleFinaleEffectRefNull()
    {
        if (rubbleFinale == 0)
        {
            return rubbleFinaleGuid.IsEmpty();
        }
        return false;
    }

    public EffectAsset FindRubbleFinaleEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(rubbleFinaleGuid, rubbleFinale);
    }

    public AssetReference<ObjectAsset> getHolidayRedirect()
    {
        return HolidayUtil.getActiveHoliday() switch
        {
            ENPCHoliday.CHRISTMAS => christmasRedirect, 
            ENPCHoliday.HALLOWEEN => halloweenRedirect, 
            _ => AssetReference<ObjectAsset>.invalid, 
        };
    }

    public virtual byte[] getState()
    {
        byte[] array = ((interactability == EObjectInteractability.BINARY_STATE) ? new byte[1] { (byte)((Level.isEditor && interactabilityEditor != 0) ? 1u : 0u) } : ((interactability != EObjectInteractability.WATER && interactability != EObjectInteractability.FUEL) ? null : new byte[2]
        {
            interactabilityResourceState[0],
            interactabilityResourceState[1]
        }));
        if (rubble == EObjectRubble.DESTROY)
        {
            if (array != null)
            {
                byte[] array2 = new byte[array.Length + 1];
                Array.Copy(array, array2, array.Length);
                array = array2;
            }
            else
            {
                array = new byte[1];
            }
            array[array.Length - 1] = (byte)((!Level.isEditor || rubbleEditor != EObjectRubbleEditor.DEAD) ? byte.MaxValue : 0);
        }
        return array;
    }

    public bool areConditionsMet(Player player)
    {
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (!conditions[i].isConditionMet(player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    internal HashSet<ushort> GetConditionAssociatedFlags()
    {
        if (conditions == null)
        {
            return null;
        }
        INPCCondition[] array = conditions;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].GatherAssociatedFlags(tempAssociatedFlags);
        }
        if (tempAssociatedFlags.Count > 0)
        {
            HashSet<ushort> result = tempAssociatedFlags;
            tempAssociatedFlags = new HashSet<ushort>();
            return result;
        }
        return null;
    }

    public bool areInteractabilityConditionsMet(Player player)
    {
        if (interactabilityConditions != null)
        {
            for (int i = 0; i < interactabilityConditions.Length; i++)
            {
                if (!interactabilityConditions[i].isConditionMet(player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void applyInteractabilityConditions(Player player, bool shouldSend)
    {
        if (interactabilityConditions != null)
        {
            for (int i = 0; i < interactabilityConditions.Length; i++)
            {
                interactabilityConditions[i].applyCondition(player, shouldSend);
            }
        }
    }

    public void grantInteractabilityRewards(Player player, bool shouldSend)
    {
        interactabilityRewards.Grant(player, shouldSend);
    }

    protected bool recursivelyFixTag(GameObject parentGameObject, string oldTag, string newTag)
    {
        if (parentGameObject.CompareTag(oldTag))
        {
            parentGameObject.tag = newTag;
            int childCount = parentGameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject gameObject = parentGameObject.transform.GetChild(i).gameObject;
                if (!recursivelyFixTag(gameObject, oldTag, newTag))
                {
                    return false;
                }
            }
            return true;
        }
        Assets.reportError(this, "unable to automatically fix tag for " + objectName + "'s " + parentGameObject.name + "! Trying to convert tag " + oldTag + " to " + newTag);
        return false;
    }

    protected bool recursivelyFixLayer(GameObject parentGameObject, int oldLayer, int newLayer)
    {
        if (parentGameObject.layer == oldLayer)
        {
            parentGameObject.layer = newLayer;
            int childCount = parentGameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject gameObject = parentGameObject.transform.GetChild(i).gameObject;
                if (!recursivelyFixLayer(gameObject, oldLayer, newLayer))
                {
                    return false;
                }
            }
            return true;
        }
        Assets.reportError(this, "Unable to automatically fix layer for " + objectName + "'s " + parentGameObject.name + "! Trying to convert layer " + oldLayer + " to " + newLayer);
        return false;
    }

    protected void fixTagAndLayer(GameObject rootGameObject, string expectedTag, int expectedLayer)
    {
        if (!rootGameObject.CompareTag(expectedTag))
        {
            string tag = rootGameObject.tag;
            recursivelyFixTag(rootGameObject, tag, expectedTag);
        }
        if (rootGameObject.layer != expectedLayer)
        {
            int layer = rootGameObject.layer;
            recursivelyFixLayer(rootGameObject, layer, expectedLayer);
        }
    }

    private void ensureNavMeshReadable()
    {
        navMCs.Clear();
        navGameObject?.getOrLoad().GetComponentsInChildren(includeInactive: true, navMCs);
        foreach (MeshCollider navMC in navMCs)
        {
            if (navMC.sharedMesh == null)
            {
                Assets.reportError(this, "missing mesh for MeshCollider '" + navMC.name + "'");
            }
            else if (!navMC.sharedMesh.isReadable)
            {
                Assets.reportError(this, "mesh must have read/write enabled for MeshCollider '" + navMC.name + "'");
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _objectName = localization.format("Name");
        type = (EObjectType)Enum.Parse(typeof(EObjectType), data.GetString("Type"), ignoreCase: true);
        if (type == EObjectType.NPC)
        {
            if (Dedicator.IsDedicatedServer)
            {
                loadedModel = Resources.Load<GameObject>("Characters/NPC_Server");
            }
            else
            {
                loadedModel = Resources.Load<GameObject>("Characters/NPC_Client");
            }
            hasLoadedModel = true;
            useScale = true;
            interactability = EObjectInteractability.NPC;
            chart = EObjectChart.IGNORE;
        }
        else if (type == EObjectType.DECAL)
        {
            float num = data.ParseFloat("Decal_X");
            float num2 = data.ParseFloat("Decal_Y");
            float num3 = 1f;
            if (data.ContainsKey("Decal_LOD_Bias"))
            {
                num3 = data.ParseFloat("Decal_LOD_Bias");
            }
            Texture2D texture2D = bundle.load<Texture2D>("Decal");
            if (texture2D == null)
            {
                Assets.reportError(this, "missing 'Decal' Texture2D. It will show as pure white without one.");
            }
            bool flag = data.ContainsKey("Decal_Alpha");
            hasLoadedModel = true;
            loadedModel = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(flag ? "Materials/Decal_Template_Alpha" : "Materials/Decal_Template_Masked"));
            loadedModel.transform.position = new Vector3(-10000f, -10000f, -10000f);
            loadedModel.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(loadedModel);
            loadedModel.GetComponent<BoxCollider>().size = new Vector3(num2, num, 1f);
            Decal component = loadedModel.transform.Find("Decal").GetComponent<Decal>();
            Material material = UnityEngine.Object.Instantiate(component.material);
            material.name = "Decal_Deferred";
            material.hideFlags = HideFlags.DontSave;
            material.SetTexture("_MainTex", texture2D);
            component.material = material;
            component.lodBias = num3;
            component.transform.localScale = new Vector3(num, num2, 1f);
            MeshRenderer component2 = loadedModel.transform.Find("Mesh").GetComponent<MeshRenderer>();
            Material material2 = UnityEngine.Object.Instantiate(component2.sharedMaterial);
            material2.name = "Decal_Forward";
            material2.hideFlags = HideFlags.DontSave;
            material2.SetTexture("_MainTex", texture2D);
            component2.sharedMaterial = material2;
            component2.transform.localScale = new Vector3(num2, num, 1f);
            useScale = true;
            chart = EObjectChart.IGNORE;
        }
        else
        {
            if (Dedicator.IsDedicatedServer && data.ParseBool("Has_Clip_Prefab", defaultValue: true))
            {
                bundle.loadDeferred("Clip", out legacyServerModel, (LoadedAssetDeferredCallback<GameObject>)OnServerModelLoaded);
            }
            bundle.loadDeferred("Object", out clientModel, (LoadedAssetDeferredCallback<GameObject>)OnClientModelLoaded);
            if (!Dedicator.IsDedicatedServer)
            {
                bundle.loadDeferred("Skybox", out skyboxGameObject, (LoadedAssetDeferredCallback<GameObject>)null);
            }
            bundle.loadDeferred("Nav", out navGameObject, (LoadedAssetDeferredCallback<GameObject>)onNavGameObjectLoaded);
            bundle.loadDeferred("Slots", out slotsGameObject, (LoadedAssetDeferredCallback<GameObject>)onSlotsGameObjectLoaded);
            bundle.loadDeferred("Triggers", out triggersGameObject, (LoadedAssetDeferredCallback<GameObject>)null);
            isSnowshoe = data.ContainsKey("Snowshoe");
            if (data.ContainsKey("Chart"))
            {
                chart = (EObjectChart)Enum.Parse(typeof(EObjectChart), data.GetString("Chart"), ignoreCase: true);
            }
            else
            {
                chart = EObjectChart.NONE;
            }
            isFuel = data.ContainsKey("Fuel");
            isRefill = data.ContainsKey("Refill");
            isSoft = data.ContainsKey("Soft");
            causesFallDamage = data.ParseBool("Causes_Fall_Damage", defaultValue: true);
            isCollisionImportant = data.ContainsKey("Collision_Important") || type == EObjectType.LARGE;
            shouldExcludeFromCullingVolumes = data.ParseBool("Exclude_From_Culling_Volumes");
            if (isFuel || isRefill)
            {
                Assets.reportError(this, "is using the legacy fuel/water system");
            }
            if (data.ContainsKey("LOD"))
            {
                lod = (EObjectLOD)Enum.Parse(typeof(EObjectLOD), data.GetString("LOD"), ignoreCase: true);
                lodBias = data.ParseFloat("LOD_Bias");
                if (lodBias < 0.01f)
                {
                    lodBias = 1f;
                }
                cullingVolumeLocalPositionOffset = data.LegacyParseVector3("LOD_Center");
                cullingVolumeSizeOffset = data.LegacyParseVector3("LOD_Size");
            }
            if (data.ContainsKey("Interactability"))
            {
                interactability = (EObjectInteractability)Enum.Parse(typeof(EObjectInteractability), data.GetString("Interactability"), ignoreCase: true);
                interactabilityRemote = data.ContainsKey("Interactability_Remote");
                interactabilityDelay = data.ParseFloat("Interactability_Delay");
                interactabilityReset = data.ParseFloat("Interactability_Reset");
                if (data.ContainsKey("Interactability_Hint"))
                {
                    interactabilityHint = (EObjectInteractabilityHint)Enum.Parse(typeof(EObjectInteractabilityHint), data.GetString("Interactability_Hint"), ignoreCase: true);
                }
                interactabilityEmission = data.ContainsKey("Interactability_Emission");
                if (interactability == EObjectInteractability.NOTE)
                {
                    ushort num4 = data.ParseUInt16("Interactability_Text_Lines", 0);
                    StringBuilder stringBuilder = new StringBuilder();
                    for (ushort num5 = 0; num5 < num4; num5 = (ushort)(num5 + 1))
                    {
                        string desc = localization.format("Interactability_Text_Line_" + num5);
                        desc = ItemTool.filterRarityRichText(desc);
                        RichTextUtil.replaceNewlineMarkup(ref desc);
                        stringBuilder.AppendLine(desc);
                    }
                    interactabilityText = stringBuilder.ToString();
                }
                else
                {
                    interactabilityText = localization.read("Interact");
                    if (string.IsNullOrWhiteSpace(interactabilityText))
                    {
                        if (interactability == EObjectInteractability.QUEST)
                        {
                            Assets.reportError(this, "Interact text empty");
                        }
                    }
                    else
                    {
                        interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
                        RichTextUtil.replaceNewlineMarkup(ref interactabilityText);
                    }
                }
                if (data.ContainsKey("Interactability_Power"))
                {
                    interactabilityPower = (EObjectInteractabilityPower)Enum.Parse(typeof(EObjectInteractabilityPower), data.GetString("Interactability_Power"), ignoreCase: true);
                }
                else
                {
                    interactabilityPower = EObjectInteractabilityPower.NONE;
                }
                if (data.ContainsKey("Interactability_Editor"))
                {
                    interactabilityEditor = (EObjectInteractabilityEditor)Enum.Parse(typeof(EObjectInteractabilityEditor), data.GetString("Interactability_Editor"), ignoreCase: true);
                }
                else
                {
                    interactabilityEditor = EObjectInteractabilityEditor.NONE;
                }
                if (data.ContainsKey("Interactability_Nav"))
                {
                    interactabilityNav = (EObjectInteractabilityNav)Enum.Parse(typeof(EObjectInteractabilityNav), data.GetString("Interactability_Nav"), ignoreCase: true);
                }
                else
                {
                    interactabilityNav = EObjectInteractabilityNav.NONE;
                }
                interactabilityDrops = new ushort[data.ParseUInt8("Interactability_Drops", 0)];
                for (byte b = 0; b < interactabilityDrops.Length; b = (byte)(b + 1))
                {
                    interactabilityDrops[b] = data.ParseUInt16("Interactability_Drop_" + b, 0);
                }
                interactabilityRewardID = data.ParseUInt16("Interactability_Reward_ID", 0);
                interactabilityEffect = data.ParseGuidOrLegacyId("Interactability_Effect", out interactabilityEffectGuid);
                interactabilityConditions = new INPCCondition[data.ParseUInt8("Interactability_Conditions", 0)];
                NPCTool.readConditions(data, localization, "Interactability_Condition_", interactabilityConditions, this);
                interactabilityRewards.Parse(data, localization, this, "Interactability_Rewards", "Interactability_Reward_");
                interactabilityResource = data.ParseUInt16("Interactability_Resource", 0);
                interactabilityResourceState = BitConverter.GetBytes(interactabilityResource);
            }
            else
            {
                interactability = EObjectInteractability.NONE;
                interactabilityPower = EObjectInteractabilityPower.NONE;
                interactabilityEditor = EObjectInteractabilityEditor.NONE;
            }
            if (interactability == EObjectInteractability.RUBBLE)
            {
                rubble = EObjectRubble.DESTROY;
                rubbleReset = data.ParseFloat("Interactability_Reset");
                rubbleHealth = data.ParseUInt16("Interactability_Health", 0);
                rubbleEffect = data.ParseGuidOrLegacyId("Interactability_Effect", out rubbleEffectGuid);
                rubbleFinale = data.ParseGuidOrLegacyId("Interactability_Finale", out rubbleFinaleGuid);
                rubbleRewardID = data.ParseUInt16("Interactability_Reward_ID", 0);
                rubbleBladeID = data.ParseUInt8("Interactability_Blade_ID", 0);
                rubbleRewardProbability = data.ParseFloat("Interactability_Reward_Probability", 1f);
                rubbleRewardsMin = data.ParseUInt8("Interactability_Rewards_Min", 1);
                rubbleRewardsMax = data.ParseUInt8("Interactability_Rewards_Max", 1);
                rubbleRewardXP = data.ParseUInt32("Interactability_Reward_XP");
                rubbleIsVulnerable = !data.ContainsKey("Interactability_Invulnerable");
                rubbleProofExplosion = data.ContainsKey("Interactability_Proof_Explosion");
            }
            else if (data.ContainsKey("Rubble"))
            {
                rubble = (EObjectRubble)Enum.Parse(typeof(EObjectRubble), data.GetString("Rubble"), ignoreCase: true);
                rubbleReset = data.ParseFloat("Rubble_Reset");
                rubbleHealth = data.ParseUInt16("Rubble_Health", 0);
                rubbleEffect = data.ParseGuidOrLegacyId("Rubble_Effect", out rubbleEffectGuid);
                rubbleFinale = data.ParseGuidOrLegacyId("Rubble_Finale", out rubbleFinaleGuid);
                rubbleRewardID = data.ParseUInt16("Rubble_Reward_ID", 0);
                rubbleBladeID = data.ParseUInt8("Rubble_Blade_ID", 0);
                rubbleRewardProbability = data.ParseFloat("Rubble_Reward_Probability", 1f);
                rubbleRewardsMin = data.ParseUInt8("Rubble_Rewards_Min", 1);
                rubbleRewardsMax = data.ParseUInt8("Rubble_Rewards_Max", 1);
                rubbleRewardXP = data.ParseUInt32("Rubble_Reward_XP");
                rubbleIsVulnerable = !data.ContainsKey("Rubble_Invulnerable");
                rubbleProofExplosion = data.ContainsKey("Rubble_Proof_Explosion");
                if (data.ContainsKey("Rubble_Editor"))
                {
                    rubbleEditor = (EObjectRubbleEditor)Enum.Parse(typeof(EObjectRubbleEditor), data.GetString("Rubble_Editor"), ignoreCase: true);
                }
                else
                {
                    rubbleEditor = EObjectRubbleEditor.ALIVE;
                }
            }
            if (data.ContainsKey("Foliage"))
            {
                foliage = new AssetReference<FoliageInfoCollectionAsset>(new Guid(data.GetString("Foliage")));
            }
            useWaterHeightTransparentSort = data.ContainsKey("Use_Water_Height_Transparent_Sort");
            shouldAddNightLightScript = data.ContainsKey("Add_Night_Light_Script");
            shouldAddKillTriggers = data.ParseBool("Add_Kill_Triggers");
            allowStructures = data.ContainsKey("Allow_Structures");
            if (data.ContainsKey("Material_Palette"))
            {
                materialPalette = new AssetReference<MaterialPaletteAsset>(data.ParseGuid("Material_Palette"));
            }
            if (data.ContainsKey("Landmark_Quality"))
            {
                landmarkQuality = (EGraphicQuality)Enum.Parse(typeof(EGraphicQuality), data.GetString("Landmark_Quality"), ignoreCase: true);
                if (landmarkQuality < EGraphicQuality.LOW)
                {
                    landmarkQuality = EGraphicQuality.LOW;
                }
            }
            else
            {
                landmarkQuality = EGraphicQuality.LOW;
            }
        }
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
        christmasRedirect = data.readAssetReference<ObjectAsset>("Christmas_Redirect");
        halloweenRedirect = data.readAssetReference<ObjectAsset>("Halloween_Redirect");
        isGore = data.ParseBool("Is_Gore");
        shouldExcludeFromLevelBatching = data.ParseBool("Exclude_From_Level_Batching");
        shouldExcludeFromLevelBatching |= type == EObjectType.NPC || type == EObjectType.DECAL;
        conditions = new INPCCondition[data.ParseUInt8("Conditions", 0)];
        NPCTool.readConditions(data, localization, "Condition_", conditions, this);
    }
}
