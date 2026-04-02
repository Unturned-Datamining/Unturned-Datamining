using System;
using System.Collections.Generic;
using System.Text;
using SDG.Framework.Foliage;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ObjectAsset : Asset, IArmorFalloff
{
    protected string _objectName;

    public EObjectType type;

    private GameObject loadedModel;

    /// <summary>
    /// Prevents calling getOrLoad redundantly if asset does not exist.
    /// </summary>
    private bool hasCalledLoadModel;

    private IDeferredAsset<GameObject> clientModel;

    private IDeferredAsset<GameObject> legacyServerModel;

    /// <summary>
    /// If set, overrides model prefab in the level editor. 
    /// </summary>
    private IDeferredAsset<GameObject> editorModel;

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

    /// <summary>
    /// Should landing on this object inflict fall damage?
    /// </summary>
    public bool causesFallDamage;

    public bool isCollisionImportant;

    public bool useScale;

    public EObjectLOD lod;

    public float lodBias;

    public Vector3 cullingVolumeLocalPositionOffset;

    public Vector3 cullingVolumeSizeOffset;

    [Obsolete]
    public INPCCondition[] conditions;

    internal NPCConditionsList visibilityConditionsList;

    public EObjectInteractability interactability;

    public bool interactabilityRemote;

    public float interactabilityDelay;

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

    public EItemOrigin interactabilityRewardItemOrigin;

    public Guid interactabilityEffectGuid;

    [Obsolete]
    public ushort interactabilityEffect;

    internal AssetReference<DialogueAsset> interactabilityDialogueRef;

    /// <summary>
    /// Same as interactabilityDialogueRef, not public because it really needs to be cleaned up. :(
    /// </summary>
    internal string interactabilityChildPathOverride;

    internal EInteractableObjectBinaryStateEmissiveMaterialMode iobsEmissiveMaterialMode;

    [Obsolete]
    public INPCCondition[] interactabilityConditions;

    internal NPCConditionsList interactabilityConditionsList;

    protected NPCRewardsList interactabilityRewards;

    public EObjectRubble rubble;

    public float rubbleReset;

    public ushort rubbleHealth;

    /// <summary>
    /// Effect played when single segment is destroyed.
    /// </summary>
    public Guid rubbleEffectGuid;

    [Obsolete]
    public ushort rubbleEffect;

    /// <summary>
    /// Effect played when all segments are destroyed.
    /// </summary>
    public Guid rubbleFinaleGuid;

    [Obsolete]
    public ushort rubbleFinale;

    public EObjectRubbleEditor rubbleEditor;

    public ushort rubbleRewardID;

    /// <summary>
    /// Weapon must have matching blade ID to damage object.
    /// Both weapons and objects default to zero so they can be damaged by default.
    /// </summary>
    public byte rubbleBladeID;

    /// <summary>
    /// [0, 1] probability of dropping any rewards.
    /// </summary>
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

    /// <summary>
    /// Should colliders in the Triggers GameObject with "Kill" name kill players?
    /// If Triggers GameObject is not set, searches Object instead.
    /// </summary>
    public bool shouldAddKillTriggers;

    public bool allowStructures;

    /// <summary>
    /// Should this object only be visible if gore is enabled?
    /// Allows pre-placed blood splatters to be hidden for younger players.
    /// </summary>
    public bool isGore;

    /// <summary>
    /// Object to use during the Christmas event instead.
    /// </summary>
    public AssetReference<ObjectAsset> christmasRedirect;

    /// <summary>
    /// Object to use during the Halloween event instead.
    /// </summary>
    public AssetReference<ObjectAsset> halloweenRedirect;

    private static HashSet<ushort> tempAssociatedFlags = new HashSet<ushort>();

    private static List<MeshCollider> navMCs = new List<MeshCollider>();

    public float ArmorFalloffMaxRange { get; set; }

    public float ArmorFalloffRange { get; set; }

    public float ArmorFalloffMultiplier { get; set; }

    public string objectName => holidayRestriction switch
    {
        ENPCHoliday.HALLOWEEN => _objectName + " [HW]", 
        ENPCHoliday.CHRISTMAS => _objectName + " [XMAS]", 
        ENPCHoliday.APRIL_FOOLS => _objectName + " [AF]", 
        ENPCHoliday.VALENTINES => _objectName + " [V]", 
        ENPCHoliday.PRIDE_MONTH => _objectName + " [PM]", 
        ENPCHoliday.LUNAR_NEW_YEAR => _objectName + " [LNY]", 
        _ => _objectName, 
    };

    public override string FriendlyName => objectName;

    /// <summary>
    /// If true, object will be hidden when rendering GPS/satellite view.
    /// Defaults to true if <see cref="P:SDG.Unturned.ObjectAsset.holidayRestriction" /> is set.
    /// </summary>
    public bool ShouldExcludeFromSatelliteCapture { get; private set; }

    /// <summary>
    /// If true, Nav game object will be instantiated in singleplayer and on dedicated server. Useful for objects
    /// which need to affect navmesh baking without colliding with zombies during gameplay.
    /// Defaults to true for "medium" and "large" objects.
    /// </summary>
    public bool ShouldLoadNavOnServer { get; private set; }

    /// <summary>
    /// If true, Nav game object will be instantiated in the level editor. Useful for objects which need collision
    /// with zombies during gameplay without affecting navmesh baking.
    /// Defaults to true for "medium" and "large" objects.
    /// </summary>
    public bool ShouldLoadNavInEditor { get; private set; }

    /// <summary>
    /// If true, object is not loaded when clutter is turned off in graphics menu.
    /// </summary>
    public bool IsClutter { get; set; }

    /// <summary>
    /// If set, overrides objectName as character name in dialogue.
    /// </summary>
    public string InteractabilityDialogueDisplayName { get; set; }

    /// <summary>
    /// If true, zombies can attack this object if it's blocking them. Defaults to false.
    /// </summary>
    public bool RubbleCanZombiesDamage { get; protected set; }

    /// <summary>
    /// Multiplier for damage from zombies if RubbleCanZombiesDamage is true.
    /// </summary>
    public float RubbleZombieDamageMultiplier { get; protected set; }

    /// <summary>
    /// Controls how rubble affects Nav game object.
    /// </summary>
    public EObjectRubbleNavMode RubbleNavMode { get; protected set; }

    /// <summary>
    /// If set (&gt;0), alerts nearby entities when an individual section is destroyed.
    /// </summary>
    public float RubbleSectionDestroyedAlertRadius { get; protected set; }

    /// <summary>
    /// If set (&gt;0), alerts nearby entities when all sections are destroyed.
    /// </summary>
    public float RubbleAllSectionsDestroyedAlertRadius { get; protected set; }

    /// <summary>
    /// If true, all sections respawn at the same time.
    /// </summary>
    public bool RubbleRespawnAllSectionsSimultaneously { get; set; }

    /// <summary>
    /// Only activated during this holiday.
    /// </summary>
    public ENPCHoliday holidayRestriction { get; protected set; }

    public override EAssetType assetCategory => EAssetType.OBJECT;

    public GameObject GetOrLoadModel(bool isEditor = false)
    {
        if (!hasCalledLoadModel)
        {
            hasCalledLoadModel = true;
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
        if (isEditor && editorModel != null)
        {
            GameObject orLoad = editorModel.getOrLoad();
            if (orLoad != null)
            {
                return orLoad;
            }
        }
        return loadedModel;
    }

    protected void validateModel(GameObject asset)
    {
        if (Mathf.Abs(asset.transform.localScale.x - 1f) > 0.01f || Mathf.Abs(asset.transform.localScale.y - 1f) > 0.01f || Mathf.Abs(asset.transform.localScale.z - 1f) > 0.01f)
        {
            useScale = false;
            Assets.ReportError(this, "should have a scale of one");
        }
        else
        {
            useScale = true;
        }
        Transform transform = asset.transform.Find("Block");
        if (transform != null && transform.GetComponent<Collider>() != null && transform.GetComponent<Collider>().sharedMaterial == null)
        {
            Assets.ReportError(this, "has a 'Block' collider but no physics material");
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
            Assets.ReportError(this, "has an unknown tag/layer because it has an unhandled EObjectType");
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
        if (!Assets.shouldValidateAssets)
        {
            return;
        }
        if (interactability == EObjectInteractability.BINARY_STATE)
        {
            Animation animation = asset.transform.Find("Root")?.GetComponent<Animation>();
            if (animation != null)
            {
                validateAnimation(animation, "Open");
                validateAnimation(animation, "Close");
            }
        }
        if (interactability == EObjectInteractability.RUBBLE || rubble != 0)
        {
            Transform transform3 = asset.transform.Find("Sections");
            if (transform3 != null && transform3.childCount > 8)
            {
                Assets.ReportError(this, $"destructible has {transform3.childCount} sections, but the maximum supported is 8");
            }
        }
    }

    /// <summary>
    /// Clip.prefab
    /// </summary>
    protected void OnServerModelLoaded(GameObject asset)
    {
        if (asset == null && type != EObjectType.SMALL)
        {
            Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Object\" GameObject instead");
        }
        if (asset != null)
        {
            validateModel(asset);
        }
    }

    /// <summary>
    /// Object.prefab
    /// </summary>
    protected void OnClientModelLoaded(GameObject asset)
    {
        if (asset == null)
        {
            Assets.ReportError(this, "missing \"Object\" GameObject");
            return;
        }
        validateModel(asset);
        if (Dedicator.IsDedicatedServer)
        {
            ServerPrefabUtil.RemoveClientComponents(asset, this);
        }
    }

    protected void onNavGameObjectLoaded(GameObject asset)
    {
        if (asset == null && type == EObjectType.LARGE)
        {
            Assets.ReportError(this, "missing Nav GameObject. Highly recommended to fix.");
        }
        if (asset != null)
        {
            fixTagAndLayer(asset, "Navmesh", 22);
            if ((bool)Assets.shouldValidateAssets)
            {
                ensureNavMeshReadable();
            }
        }
        if (!Assets.shouldValidateAssets)
        {
            return;
        }
        if (interactabilityNav != 0)
        {
            if (asset == null)
            {
                ReportAssetError($"has Interactability_Nav_Mode {interactabilityNav} but no Nav prefab");
            }
            else if (asset.GetComponentInChildren<Collider>(includeInactive: true) == null)
            {
                Component component = null;
                Type cutComponentType = UnturnedPathfinding.Get().GetCutComponentType();
                if (cutComponentType != null)
                {
                    component = asset.GetComponentInChildren(cutComponentType, includeInactive: true);
                }
                if (component == null)
                {
                    ReportAssetError($"has Interactability_Nav_Mode {interactabilityNav} but Nav prefab has no collision or navmesh cuts");
                }
            }
        }
        if (RubbleNavMode == EObjectRubbleNavMode.Unaffected)
        {
            return;
        }
        if (asset == null)
        {
            ReportAssetError($"has Rubble_Nav_Mode {RubbleNavMode} but no Nav prefab");
        }
        else if (asset.GetComponentInChildren<Collider>(includeInactive: true) == null)
        {
            Component component2 = null;
            Type cutComponentType2 = UnturnedPathfinding.Get().GetCutComponentType();
            if (cutComponentType2 != null)
            {
                component2 = asset.GetComponentInChildren(cutComponentType2, includeInactive: true);
            }
            if (component2 == null)
            {
                ReportAssetError($"has Rubble_Nav_Mode {RubbleNavMode} but Nav prefab has no collision or navmesh cuts");
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

    /// <summary>
    /// Property is not exposed at the moment because interactability properties should really be moved into some
    /// sort of sub-asset.
    /// </summary>
    public DialogueAsset FindInteractabilityDialogueAsset()
    {
        return interactabilityDialogueRef.Find();
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

    /// <summary>
    /// Get asset ref to replace this one for holiday, or null if it should not be redirected.
    /// </summary>
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
            array[^1] = (byte)((!Level.isEditor || rubbleEditor != EObjectRubbleEditor.DEAD) ? byte.MaxValue : 0);
        }
        return array;
    }

    public bool areConditionsMet(Player player)
    {
        return visibilityConditionsList.AreConditionsMet(player);
    }

    /// <summary>
    /// If any conditions use flags they will be added to a set,
    /// otherwise null is returned.
    /// </summary>
    internal HashSet<ushort> GetConditionAssociatedFlags()
    {
        if (visibilityConditionsList.conditions == null)
        {
            return null;
        }
        INPCCondition[] array = visibilityConditionsList.conditions;
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
        return interactabilityConditionsList.AreConditionsMet(player);
    }

    public void ApplyInteractabilityConditions(Player player)
    {
        interactabilityConditionsList.ApplyConditions(player);
    }

    public void GrantInteractabilityRewards(Player player)
    {
        interactabilityRewards.Grant(player);
    }

    /// <summary>
    /// Recursively change all children including root from oldTag to newTag.
    /// Aborts if a child doesn't match the old tag because it might be something we shouldn't change the tag of.
    /// <return>True if tags were all successfully changed.</return>
    /// </summary>
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
        Assets.ReportError(this, "unable to automatically fix tag for " + objectName + "'s " + parentGameObject.name + "! Trying to convert tag " + oldTag + " to " + newTag);
        return false;
    }

    /// <summary>
    /// Recursively change all children including root from oldLayer to newLayer.
    /// Aborts if a child doesn't match the old layer because it might be something we shouldn't change the layer of.
    /// <return>True if layers were all successfully changed.</return>
    /// </summary>
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
        Assets.ReportError(this, "Unable to automatically fix layer for " + objectName + "'s " + parentGameObject.name + "! Trying to convert layer " + oldLayer + " to " + newLayer);
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

    /// <summary>
    /// Called if we have a valid Nav GameObject.
    /// Recast requires any meshes used on the Nav objects to be CPU readable, so we log errors here if they're not marked as such.
    /// </summary>
    private void ensureNavMeshReadable()
    {
        navMCs.Clear();
        navGameObject?.getOrLoad().GetComponentsInChildren(includeInactive: true, navMCs);
        foreach (MeshCollider navMC in navMCs)
        {
            if (navMC.sharedMesh == null)
            {
                Assets.ReportError(this, "missing mesh for MeshCollider '" + navMC.name + "'");
            }
            else if (!navMC.sharedMesh.isReadable)
            {
                Assets.ReportError(this, "mesh must have read/write enabled for MeshCollider '" + navMC.name + "'");
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _objectName = p.localization.format("Name");
        type = (EObjectType)Enum.Parse(typeof(EObjectType), p.data.GetString("Type"), ignoreCase: true);
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
            hasCalledLoadModel = true;
            useScale = true;
            interactability = EObjectInteractability.NPC;
            chart = EObjectChart.IGNORE;
        }
        else if (type == EObjectType.DECAL)
        {
            hasCalledLoadModel = true;
            float num = p.data.ParseFloat("Decal_X", 1f);
            float num2 = p.data.ParseFloat("Decal_Y", 1f);
            if (Dedicator.IsDedicatedServer)
            {
                loadedModel = new GameObject("Decal_Template");
                loadedModel.transform.position = new Vector3(-10000f, -10000f, -10000f);
                loadedModel.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(loadedModel);
                loadedModel.AddComponent<BoxCollider>().size = new Vector3(num2, num, 1f);
            }
            else
            {
                float num3 = 1f;
                if (p.data.ContainsKey("Decal_LOD_Bias"))
                {
                    num3 = p.data.ParseFloat("Decal_LOD_Bias");
                }
                Texture2D texture2D = p.bundle.load<Texture2D>("Decal");
                if (texture2D == null)
                {
                    Assets.ReportError(this, "missing 'Decal' Texture2D. It will show as pure white without one.");
                }
                bool flag = p.data.ContainsKey("Decal_Alpha");
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
            }
            useScale = true;
            chart = EObjectChart.IGNORE;
        }
        else
        {
            if (p.data.ContainsKey("Interactability"))
            {
                interactability = (EObjectInteractability)Enum.Parse(typeof(EObjectInteractability), p.data.GetString("Interactability"), ignoreCase: true);
                interactabilityRemote = p.data.ContainsKey("Interactability_Remote");
                interactabilityDelay = p.data.ParseFloat("Interactability_Delay");
                interactabilityReset = p.data.ParseFloat("Interactability_Reset");
                if (p.data.ContainsKey("Interactability_Hint"))
                {
                    interactabilityHint = (EObjectInteractabilityHint)Enum.Parse(typeof(EObjectInteractabilityHint), p.data.GetString("Interactability_Hint"), ignoreCase: true);
                }
                if (interactability == EObjectInteractability.NOTE)
                {
                    ushort num4 = p.data.ParseUInt16("Interactability_Text_Lines", 0);
                    StringBuilder stringBuilder = new StringBuilder();
                    for (ushort num5 = 0; num5 < num4; num5++)
                    {
                        string desc = p.localization.format("Interactability_Text_Line_" + num5);
                        desc = ItemTool.filterRarityRichText(desc);
                        RichTextUtil.replaceNewlineMarkup(ref desc);
                        stringBuilder.AppendLine(desc);
                    }
                    interactabilityText = stringBuilder.ToString();
                }
                else
                {
                    interactabilityText = p.localization.FormatOrEmpty("Interact");
                    if (string.IsNullOrWhiteSpace(interactabilityText))
                    {
                        if (interactability == EObjectInteractability.QUEST)
                        {
                            Assets.ReportError(this, "Interact text empty");
                        }
                    }
                    else
                    {
                        interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
                        RichTextUtil.replaceNewlineMarkup(ref interactabilityText);
                    }
                }
                if (interactability == EObjectInteractability.DIALOGUE)
                {
                    InteractabilityDialogueDisplayName = p.localization.FormatOrNull("Dialogue_Name");
                }
                if (p.data.ContainsKey("Interactability_Power"))
                {
                    interactabilityPower = (EObjectInteractabilityPower)Enum.Parse(typeof(EObjectInteractabilityPower), p.data.GetString("Interactability_Power"), ignoreCase: true);
                }
                else
                {
                    interactabilityPower = EObjectInteractabilityPower.NONE;
                }
                if (p.data.ContainsKey("Interactability_Editor"))
                {
                    interactabilityEditor = (EObjectInteractabilityEditor)Enum.Parse(typeof(EObjectInteractabilityEditor), p.data.GetString("Interactability_Editor"), ignoreCase: true);
                }
                else
                {
                    interactabilityEditor = EObjectInteractabilityEditor.NONE;
                }
                if (p.data.ContainsKey("Interactability_Nav"))
                {
                    interactabilityNav = (EObjectInteractabilityNav)Enum.Parse(typeof(EObjectInteractabilityNav), p.data.GetString("Interactability_Nav"), ignoreCase: true);
                }
                else
                {
                    interactabilityNav = EObjectInteractabilityNav.NONE;
                }
                interactabilityDrops = new ushort[p.data.ParseUInt8("Interactability_Drops", 0)];
                for (byte b = 0; b < interactabilityDrops.Length; b++)
                {
                    interactabilityDrops[b] = p.data.ParseUInt16("Interactability_Drop_" + b, 0);
                }
                interactabilityRewardID = p.data.ParseUInt16("Interactability_Reward_ID", 0);
                interactabilityRewardItemOrigin = p.data.ParseEnum("Interactability_RewardItem_Origin", EItemOrigin.NATURE);
                interactabilityEffect = p.data.ParseGuidOrLegacyId("Interactability_Effect", out interactabilityEffectGuid);
                if (interactability == EObjectInteractability.DIALOGUE)
                {
                    interactabilityDialogueRef = p.data.readAssetReference<DialogueAsset>("Interactability_Dialogue");
                }
                if (interactability == EObjectInteractability.BINARY_STATE)
                {
                    interactabilityChildPathOverride = p.data.GetString("Interactability_Animation_Component_Path");
                    iobsEmissiveMaterialMode = p.data.ParseEnum("Interactability_Emissive_Material_Mode", EInteractableObjectBinaryStateEmissiveMaterialMode.Auto);
                }
                interactabilityConditionsList.Parse(p.data, p.localization, this, "Interactability_Conditions", "Interactability_Condition_");
                interactabilityConditions = interactabilityConditionsList.conditions;
                interactabilityRewards.Parse(p.data, p.localization, this, "Interactability_Rewards", "Interactability_Reward_");
                interactabilityResource = p.data.ParseUInt16("Interactability_Resource", 0);
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
                rubbleReset = p.data.ParseFloat("Interactability_Reset");
                rubbleHealth = p.data.ParseUInt16("Interactability_Health", 0);
                rubbleEffect = p.data.ParseGuidOrLegacyId("Interactability_Effect", out rubbleEffectGuid);
                rubbleFinale = p.data.ParseGuidOrLegacyId("Interactability_Finale", out rubbleFinaleGuid);
                rubbleRewardID = p.data.ParseUInt16("Interactability_Reward_ID", 0);
                rubbleBladeID = p.data.ParseUInt8("Interactability_Blade_ID", 0);
                rubbleRewardProbability = p.data.ParseFloat("Interactability_Reward_Probability", 1f);
                rubbleRewardsMin = p.data.ParseUInt8("Interactability_Rewards_Min", 1);
                rubbleRewardsMax = p.data.ParseUInt8("Interactability_Rewards_Max", 1);
                rubbleRewardXP = p.data.ParseUInt32("Interactability_Reward_XP");
                rubbleIsVulnerable = !p.data.ContainsKey("Interactability_Invulnerable");
                rubbleProofExplosion = p.data.ContainsKey("Interactability_Proof_Explosion");
                RubbleCanZombiesDamage = p.data.ParseBool("Interactability_Can_Zombies_Damage");
                RubbleZombieDamageMultiplier = p.data.ParseFloat("Interactability_Zombie_Damage_Multiplier", 1f);
                RubbleSectionDestroyedAlertRadius = p.data.ParseFloat("Interactability_Section_Destroyed_Alert_Radius", -1f);
                RubbleAllSectionsDestroyedAlertRadius = p.data.ParseFloat("Interactability_All_Sections_Destroyed_Alert_Radius", -1f);
                RubbleRespawnAllSectionsSimultaneously = p.data.ParseBool("Interactability_Respawn_All_Sections_Simultaneously");
                RubbleNavMode = p.data.ParseEnum("Interactability_Nav_Mode", EObjectRubbleNavMode.Unaffected);
            }
            else if (p.data.ContainsKey("Rubble"))
            {
                rubble = (EObjectRubble)Enum.Parse(typeof(EObjectRubble), p.data.GetString("Rubble"), ignoreCase: true);
                rubbleReset = p.data.ParseFloat("Rubble_Reset");
                rubbleHealth = p.data.ParseUInt16("Rubble_Health", 0);
                rubbleEffect = p.data.ParseGuidOrLegacyId("Rubble_Effect", out rubbleEffectGuid);
                rubbleFinale = p.data.ParseGuidOrLegacyId("Rubble_Finale", out rubbleFinaleGuid);
                rubbleRewardID = p.data.ParseUInt16("Rubble_Reward_ID", 0);
                rubbleBladeID = p.data.ParseUInt8("Rubble_Blade_ID", 0);
                rubbleRewardProbability = p.data.ParseFloat("Rubble_Reward_Probability", 1f);
                rubbleRewardsMin = p.data.ParseUInt8("Rubble_Rewards_Min", 1);
                rubbleRewardsMax = p.data.ParseUInt8("Rubble_Rewards_Max", 1);
                rubbleRewardXP = p.data.ParseUInt32("Rubble_Reward_XP");
                rubbleIsVulnerable = !p.data.ContainsKey("Rubble_Invulnerable");
                rubbleProofExplosion = p.data.ContainsKey("Rubble_Proof_Explosion");
                RubbleCanZombiesDamage = p.data.ParseBool("Rubble_Can_Zombies_Damage");
                RubbleZombieDamageMultiplier = p.data.ParseFloat("Rubble_Zombie_Damage_Multiplier", 1f);
                RubbleSectionDestroyedAlertRadius = p.data.ParseFloat("Rubble_Section_Destroyed_Alert_Radius", -1f);
                RubbleAllSectionsDestroyedAlertRadius = p.data.ParseFloat("Rubble_All_Sections_Destroyed_Alert_Radius", -1f);
                RubbleRespawnAllSectionsSimultaneously = p.data.ParseBool("Rubble_Respawn_All_Sections_Simultaneously");
                RubbleNavMode = p.data.ParseEnum("Rubble_Nav_Mode", EObjectRubbleNavMode.Unaffected);
                if (p.data.ContainsKey("Rubble_Editor"))
                {
                    rubbleEditor = (EObjectRubbleEditor)Enum.Parse(typeof(EObjectRubbleEditor), p.data.GetString("Rubble_Editor"), ignoreCase: true);
                }
                else
                {
                    rubbleEditor = EObjectRubbleEditor.ALIVE;
                }
            }
            if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
            {
                p.bundle.loadDeferred("Clip", out legacyServerModel, (LoadedAssetDeferredCallback<GameObject>)OnServerModelLoaded);
            }
            p.bundle.loadDeferred("Object", out clientModel, (LoadedAssetDeferredCallback<GameObject>)OnClientModelLoaded);
            if (p.data.ParseBool("Has_Editor_Prefab"))
            {
                p.bundle.loadDeferred("Editor", out editorModel, (LoadedAssetDeferredCallback<GameObject>)null);
            }
            if (!Dedicator.IsDedicatedServer)
            {
                p.bundle.loadDeferred("Skybox", out skyboxGameObject, (LoadedAssetDeferredCallback<GameObject>)null);
            }
            p.bundle.loadDeferred("Nav", out navGameObject, (LoadedAssetDeferredCallback<GameObject>)onNavGameObjectLoaded);
            p.bundle.loadDeferred("Slots", out slotsGameObject, (LoadedAssetDeferredCallback<GameObject>)onSlotsGameObjectLoaded);
            p.bundle.loadDeferred("Triggers", out triggersGameObject, (LoadedAssetDeferredCallback<GameObject>)null);
            isSnowshoe = p.data.ContainsKey("Snowshoe");
            if (p.data.ContainsKey("Chart"))
            {
                chart = (EObjectChart)Enum.Parse(typeof(EObjectChart), p.data.GetString("Chart"), ignoreCase: true);
            }
            else
            {
                chart = EObjectChart.NONE;
            }
            isFuel = p.data.ContainsKey("Fuel");
            isRefill = p.data.ContainsKey("Refill");
            isSoft = p.data.ContainsKey("Soft");
            causesFallDamage = p.data.ParseBool("Causes_Fall_Damage", defaultValue: true);
            isCollisionImportant = p.data.ContainsKey("Collision_Important") || type == EObjectType.LARGE;
            shouldExcludeFromCullingVolumes = p.data.ParseBool("Exclude_From_Culling_Volumes");
            if (isFuel || isRefill)
            {
                Assets.ReportError(this, "is using the legacy fuel/water system");
            }
            if (p.data.ContainsKey("LOD"))
            {
                lod = (EObjectLOD)Enum.Parse(typeof(EObjectLOD), p.data.GetString("LOD"), ignoreCase: true);
                lodBias = p.data.ParseFloat("LOD_Bias");
                if (lodBias < 0.01f)
                {
                    lodBias = 1f;
                }
                cullingVolumeLocalPositionOffset = p.data.LegacyParseVector3("LOD_Center");
                cullingVolumeSizeOffset = p.data.LegacyParseVector3("LOD_Size");
            }
            if (p.data.ContainsKey("Foliage"))
            {
                foliage = new AssetReference<FoliageInfoCollectionAsset>(new Guid(p.data.GetString("Foliage")));
            }
            useWaterHeightTransparentSort = p.data.ContainsKey("Use_Water_Height_Transparent_Sort");
            shouldAddNightLightScript = p.data.ContainsKey("Add_Night_Light_Script");
            shouldAddKillTriggers = p.data.ParseBool("Add_Kill_Triggers");
            allowStructures = p.data.ContainsKey("Allow_Structures");
            if (p.data.ContainsKey("Material_Palette"))
            {
                materialPalette = new AssetReference<MaterialPaletteAsset>(p.data.ParseGuid("Material_Palette"));
            }
            if (p.data.ContainsKey("Landmark_Quality"))
            {
                landmarkQuality = (EGraphicQuality)Enum.Parse(typeof(EGraphicQuality), p.data.GetString("Landmark_Quality"), ignoreCase: true);
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
        if (p.data.ContainsKey("Holiday_Restriction"))
        {
            holidayRestriction = (ENPCHoliday)Enum.Parse(typeof(ENPCHoliday), p.data.GetString("Holiday_Restriction"), ignoreCase: true);
            if (holidayRestriction == ENPCHoliday.NONE)
            {
                Assets.ReportError(this, "has no holiday restriction, so value is ignored");
            }
        }
        else
        {
            holidayRestriction = ENPCHoliday.NONE;
        }
        christmasRedirect = p.data.readAssetReference<ObjectAsset>("Christmas_Redirect");
        halloweenRedirect = p.data.readAssetReference<ObjectAsset>("Halloween_Redirect");
        isGore = p.data.ParseBool("Is_Gore");
        shouldExcludeFromLevelBatching = p.data.ParseBool("Exclude_From_Level_Batching");
        shouldExcludeFromLevelBatching |= type == EObjectType.NPC || type == EObjectType.DECAL;
        bool defaultValue = holidayRestriction != ENPCHoliday.NONE;
        ShouldExcludeFromSatelliteCapture = p.data.ParseBool("Exclude_From_Satellite_Capture", defaultValue);
        bool defaultValue2 = type == EObjectType.MEDIUM || type == EObjectType.LARGE;
        ShouldLoadNavOnServer = p.data.ParseBool("Load_Nav_On_Server", defaultValue2);
        ShouldLoadNavInEditor = p.data.ParseBool("Load_Nav_In_Editor", defaultValue2);
        visibilityConditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");
        conditions = visibilityConditionsList.conditions;
        IsClutter = p.data.ParseBool("Is_Clutter");
        this.PopulateArmorFalloff(in p);
    }

    [Obsolete("Removed shouldSend parameter")]
    public void applyInteractabilityConditions(Player player, bool shouldSend)
    {
        ApplyInteractabilityConditions(player);
    }

    [Obsolete("Removed shouldSend parameter")]
    public void grantInteractabilityRewards(Player player, bool shouldSend)
    {
        GrantInteractabilityRewards(player);
    }
}
