using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Foliage;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class LevelObject
{
    private static List<Rigidbody> reuseableRigidbodyList = new List<Rigidbody>();

    public bool isSpeciallyCulled;

    private bool isDecal;

    private Transform _transform;

    private Transform _skybox;

    private List<Renderer> renderers;

    private ushort _id;

    private Guid _GUID;

    private uint _instanceID;

    internal AssetReference<MaterialPaletteAsset> customMaterialOverride;

    internal int materialIndexOverride = -1;

    public byte[] state;

    private ObjectAsset _asset;

    private InteractableObject _interactableObj;

    private InteractableObjectRubble _rubble;

    internal CullingVolume ownedCullingVolume;

    private bool areConditionsMet;

    private bool haveConditionsBeenChecked;

    internal bool isVisibleInCullingVolume = true;

    private bool areRenderersEnabled;

    private HashSet<ushort> associatedFlags;

    private static CommandLineFlag disableCullingVolumes = new CommandLineFlag(defaultValue: false, "-DisableCullingVolumes");

    public Transform transform => _transform;

    public Transform placeholderTransform { get; protected set; }

    public Transform skybox => _skybox;

    public ushort id => _id;

    public Guid GUID => _GUID;

    public uint instanceID => _instanceID;

    public ObjectAsset asset => _asset;

    public InteractableObject interactable => _interactableObj;

    public InteractableObjectRubble rubble => _rubble;

    public bool canDamageRubble => areConditionsMet;

    public ELevelObjectPlacementOrigin placementOrigin { get; protected set; }

    public bool isCollisionEnabled { get; private set; }

    public bool isVisualEnabled { get; private set; }

    public bool isSkyboxEnabled { get; private set; }

    public bool isLandmarkQualityMet
    {
        get
        {
            if (asset == null)
            {
                return false;
            }
            if (Dedicator.IsDedicatedServer)
            {
                return false;
            }
            return GraphicsSettings.landmarkQuality >= asset.landmarkQuality;
        }
    }

    [Obsolete]
    public string name => null;

    internal void SetActive(bool shouldBeActive)
    {
        isCollisionEnabled = shouldBeActive;
        if (!Dedicator.IsDedicatedServer && (shouldBeActive || asset == null || !asset.isCollisionImportant || !Provider.isServer) && transform != null)
        {
            transform.gameObject.SetActive(isCollisionEnabled && areConditionsMet);
        }
    }

    internal void SetIsVisibleInRegion(bool isVisible)
    {
        isVisualEnabled = isVisible;
        UpdateRenderersEnabled();
    }

    internal void SetIsVisibleInCullingVolume(bool isVisible)
    {
        isVisibleInCullingVolume = isVisible;
        UpdateRenderersEnabled();
    }

    internal void SetSkyboxActive(bool shouldBeActive)
    {
        isSkyboxEnabled = shouldBeActive;
        if (!Dedicator.IsDedicatedServer && skybox != null)
        {
            skybox.gameObject.SetActive(shouldBeActive && areConditionsMet);
        }
    }

    public void destroy()
    {
        if ((bool)transform)
        {
            UnityEngine.Object.Destroy(transform.gameObject);
        }
        if ((bool)skybox)
        {
            UnityEngine.Object.Destroy(skybox.gameObject);
        }
        if (ownedCullingVolume != null)
        {
            UnityEngine.Object.Destroy(ownedCullingVolume.gameObject);
            ownedCullingVolume = null;
        }
    }

    internal void ReapplyMaterialOverrides()
    {
        Material materialOverride = GetMaterialOverride();
        if (materialOverride == null)
        {
            return;
        }
        if (skybox != null)
        {
            renderers.Clear();
            skybox.GetComponentsInChildren(includeInactive: true, renderers);
            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = materialOverride;
            }
        }
        renderers.Clear();
        transform.GetComponentsInChildren(includeInactive: true, renderers);
        foreach (Renderer renderer2 in renderers)
        {
            renderer2.sharedMaterial = materialOverride;
        }
    }

    private Material GetMaterialOverride()
    {
        Material result = null;
        AssetReference<MaterialPaletteAsset> materialPalette = customMaterialOverride;
        if (!materialPalette.isValid)
        {
            materialPalette = asset.materialPalette;
        }
        if (materialPalette.isValid)
        {
            MaterialPaletteAsset materialPaletteAsset = Assets.find(materialPalette);
            if (materialPaletteAsset != null && materialPaletteAsset.materials != null && materialPaletteAsset.materials.Count > 0)
            {
                int index;
                if (materialIndexOverride == -1)
                {
                    UnityEngine.Random.State obj = UnityEngine.Random.state;
                    UnityEngine.Random.InitState((int)instanceID);
                    index = UnityEngine.Random.Range(0, materialPaletteAsset.materials.Count);
                    UnityEngine.Random.state = obj;
                }
                else
                {
                    index = Mathf.Clamp(materialIndexOverride, 0, materialPaletteAsset.materials.Count - 1);
                }
                result = Assets.load(materialPaletteAsset.materials[index]);
            }
        }
        return result;
    }

    private void updateConditions()
    {
        if (asset == null)
        {
            return;
        }
        bool flag = true;
        if (!Dedicator.IsDedicatedServer)
        {
            flag = asset.areConditionsMet(Player.player);
            flag &= OptionsSettings.gore || !asset.isGore;
        }
        if (flag && asset.holidayRestriction != 0)
        {
            flag = HolidayUtil.isHolidayActive(asset.holidayRestriction);
        }
        if (areConditionsMet == flag && haveConditionsBeenChecked)
        {
            return;
        }
        areConditionsMet = flag;
        haveConditionsBeenChecked = true;
        if (areConditionsMet)
        {
            if (isCollisionEnabled && transform != null)
            {
                transform.gameObject.SetActive(value: true);
            }
            if (skybox != null)
            {
                skybox.gameObject.SetActive(isSkyboxEnabled);
            }
        }
        else
        {
            if (transform != null)
            {
                transform.gameObject.SetActive(value: false);
            }
            if (skybox != null)
            {
                skybox.gameObject.SetActive(value: false);
            }
        }
    }

    private void onExternalConditionsUpdated()
    {
        updateConditions();
    }

    private void OnLocalPlayerQuestsChanged(ushort id)
    {
        updateConditions();
    }

    private void OnWeatherBlendAlphaChanged(WeatherAssetBase weatherAsset, float blendAlpha)
    {
        updateConditions();
    }

    private void OnWeatherStatusChanged(WeatherAssetBase weatherAsset, EWeatherStatusChange statusChange)
    {
        updateConditions();
    }

    private void onFlagsUpdated()
    {
        updateConditions();
    }

    private void onFlagUpdated(ushort id)
    {
        if (associatedFlags != null && associatedFlags.Contains(id))
        {
            updateConditions();
        }
    }

    private void onPlayerCreated(Player player)
    {
        if (!player.channel.isOwner)
        {
            return;
        }
        Player.onPlayerCreated = (PlayerCreated)Delegate.Remove(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
        bool flag = false;
        bool flag2 = false;
        INPCCondition[] conditions = asset.conditions;
        foreach (INPCCondition iNPCCondition in conditions)
        {
            if (iNPCCondition is NPCTimeOfDayCondition || iNPCCondition is NPCIsFullMoonCondition)
            {
                flag = true;
            }
            else if (iNPCCondition is NPCQuestCondition)
            {
                flag2 = true;
            }
        }
        conditions = asset.conditions;
        foreach (INPCCondition iNPCCondition2 in conditions)
        {
            if (iNPCCondition2 is NPCWeatherBlendAlphaCondition nPCWeatherBlendAlphaCondition)
            {
                WeatherEventListenerManager.AddBlendAlphaListener(nPCWeatherBlendAlphaCondition.weather.GUID, OnWeatherBlendAlphaChanged);
            }
            else if (iNPCCondition2 is NPCWeatherStatusCondition nPCWeatherStatusCondition)
            {
                WeatherEventListenerManager.AddStatusListener(nPCWeatherStatusCondition.weather.GUID, OnWeatherStatusChanged);
            }
        }
        if (flag)
        {
            PlayerQuests quests = Player.player.quests;
            quests.onExternalConditionsUpdated = (ExternalConditionsUpdated)Delegate.Combine(quests.onExternalConditionsUpdated, new ExternalConditionsUpdated(onExternalConditionsUpdated));
        }
        PlayerQuests quests2 = Player.player.quests;
        quests2.onFlagsUpdated = (FlagsUpdated)Delegate.Combine(quests2.onFlagsUpdated, new FlagsUpdated(onFlagsUpdated));
        associatedFlags = asset.GetConditionAssociatedFlags();
        if (associatedFlags != null)
        {
            PlayerQuests quests3 = Player.player.quests;
            quests3.onFlagUpdated = (FlagUpdated)Delegate.Combine(quests3.onFlagUpdated, new FlagUpdated(onFlagUpdated));
        }
        if (flag2)
        {
            Player.player.quests.OnLocalPlayerQuestsChanged += OnLocalPlayerQuestsChanged;
        }
        updateConditions();
    }

    private void LoadAsset()
    {
        if (!Assets.shouldLoadAnyAssets)
        {
            _asset = null;
            return;
        }
        if (GUID == Guid.Empty)
        {
            _asset = Assets.find(EAssetType.OBJECT, id) as ObjectAsset;
            if (asset != null)
            {
                UnturnedLog.info("Object without GUID loaded by legacy ID {0}, updating to {1} \"{2}\"", asset.id, asset.GUID, asset.FriendlyName);
                _GUID = asset.GUID;
            }
            else
            {
                UnturnedLog.warn("Unable to find object by legacy ID {0}", id);
            }
            return;
        }
        _asset = Assets.find(new AssetReference<ObjectAsset>(GUID));
        if (!Dedicator.IsDedicatedServer)
        {
            ClientAssetIntegrity.QueueRequest(GUID, asset, "Object");
        }
        if (asset == null)
        {
            ClientAssetIntegrity.ServerAddKnownMissingAsset(GUID, "Object");
            _asset = Assets.find(EAssetType.OBJECT, id) as ObjectAsset;
            if (asset != null)
            {
                UnturnedLog.info($"Unable to find object for GUID {GUID:N} found by legacy ID {id}, updating to {asset.GUID:N} \"{asset.FriendlyName}\"");
                _GUID = asset.GUID;
            }
            else
            {
                UnturnedLog.warn($"Unable to find object for GUID {GUID:N}, nor by legacy ID {id}");
            }
        }
    }

    [Obsolete]
    public LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, string newName, Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID)
        : this(newPoint, newRotation, newScale, newID, newName, newGUID, newPlacementOrigin, newInstanceID, AssetReference<MaterialPaletteAsset>.invalid, -1, newIsHierarchyItem: false)
    {
    }

    [Obsolete]
    public LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, string newName, Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, bool newIsHierarchyItem)
        : this(newPoint, newRotation, newScale, newID, newGUID, newPlacementOrigin, newInstanceID, customMaterialOverride, materialIndexOverride, null, NetId.INVALID)
    {
    }

    [Obsolete]
    internal LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, DevkitHierarchyWorldObject devkitOwner, NetId netId)
        : this(newPoint, newRotation, newScale, newID, newGUID, newPlacementOrigin, newInstanceID, customMaterialOverride, materialIndexOverride, netId)
    {
    }

    internal LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, NetId netId)
    {
        _id = newID;
        _GUID = newGUID;
        _instanceID = newInstanceID;
        placementOrigin = newPlacementOrigin;
        this.customMaterialOverride = customMaterialOverride;
        this.materialIndexOverride = materialIndexOverride;
        LoadAsset();
        if (asset == null)
        {
            if ((bool)LevelObjects.preserveMissingAssets)
            {
                placeholderTransform = new GameObject().transform;
                placeholderTransform.position = newPoint;
                placeholderTransform.rotation = newRotation;
                placeholderTransform.localScale = newScale;
            }
            return;
        }
        state = asset.getState();
        areConditionsMet = true;
        haveConditionsBeenChecked = false;
        GameObject orLoadModel = asset.GetOrLoadModel();
        if (Dedicator.IsDedicatedServer)
        {
            if (orLoadModel != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(orLoadModel, newPoint, newRotation);
                _transform = gameObject.transform;
                gameObject.name = asset.name;
                NetIdRegistry.AssignTransform(netId, _transform);
                isDecal = this.transform.Find("Decal");
                if (asset.useScale)
                {
                    this.transform.localScale = newScale;
                }
            }
            renderers = null;
        }
        else if (orLoadModel != null)
        {
            GameObject gameObject2 = UnityEngine.Object.Instantiate(orLoadModel, newPoint, newRotation);
            _transform = gameObject2.transform;
            gameObject2.name = asset.name;
            if (!netId.IsNull())
            {
                NetIdRegistry.AssignTransform(netId, _transform);
            }
            isDecal = this.transform.Find("Decal");
            if (asset.useScale)
            {
                this.transform.localScale = newScale;
            }
            if (asset.useWaterHeightTransparentSort)
            {
                this.transform.gameObject.AddComponent<WaterHeightTransparentSort>();
            }
            if (asset.shouldAddNightLightScript)
            {
                NightLight nightLight = this.transform.gameObject.AddComponent<NightLight>();
                Transform transform = this.transform.Find("Light");
                if ((bool)transform)
                {
                    nightLight.target = transform.GetComponent<Light>();
                }
            }
            renderers = new List<Renderer>();
            Material materialOverride = GetMaterialOverride();
            GameObject gameObject3 = asset.skyboxGameObject?.getOrLoad();
            if (gameObject3 != null)
            {
                GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject3, newPoint, newRotation);
                _skybox = gameObject4.transform;
                gameObject4.name = asset.name + "_Skybox";
                if (asset.useScale)
                {
                    skybox.localScale = newScale;
                }
                SetSkyboxActive(isLandmarkQualityMet);
                skybox.GetComponentsInChildren(includeInactive: true, renderers);
                for (int i = 0; i < renderers.Count; i++)
                {
                    renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                    if (materialOverride != null)
                    {
                        renderers[i].sharedMaterial = materialOverride;
                    }
                }
                renderers.Clear();
            }
            this.transform.GetComponentsInChildren(includeInactive: true, renderers);
            if (materialOverride != null)
            {
                for (int j = 0; j < renderers.Count; j++)
                {
                    renderers[j].sharedMaterial = materialOverride;
                }
            }
            SetActive(asset.isCollisionImportant && Provider.isServer && !Dedicator.IsDedicatedServer);
            SetRenderersEnabled(isEnabled: false);
        }
        if (!(this.transform != null))
        {
            return;
        }
        if (isDecal && !Level.isEditor && asset.interactability == EObjectInteractability.NONE && asset.rubble == EObjectRubble.NONE)
        {
            Collider component = this.transform.GetComponent<Collider>();
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
            }
        }
        if (Level.isEditor)
        {
            Rigidbody orAddComponent = this.transform.GetOrAddComponent<Rigidbody>();
            orAddComponent.useGravity = false;
            orAddComponent.isKinematic = true;
        }
        else
        {
            Rigidbody component2 = this.transform.GetComponent<Rigidbody>();
            if (component2 != null)
            {
                UnityEngine.Object.Destroy(component2);
            }
            if (asset.type == EObjectType.SMALL && asset.interactability == EObjectInteractability.NONE && asset.rubble == EObjectRubble.NONE)
            {
                Collider component3 = this.transform.GetComponent<Collider>();
                if (component3 != null)
                {
                    UnityEngine.Object.Destroy(component3);
                }
            }
        }
        if ((Level.isEditor || Provider.isServer) && asset.type != EObjectType.SMALL)
        {
            GameObject gameObject5 = asset.navGameObject?.getOrLoad();
            if (gameObject5 != null)
            {
                Transform transform2 = UnityEngine.Object.Instantiate(gameObject5).transform;
                transform2.name = "Nav";
                transform2.parent = this.transform;
                transform2.localPosition = Vector3.zero;
                transform2.localRotation = Quaternion.identity;
                transform2.localScale = Vector3.one;
                if (Level.isEditor)
                {
                    Rigidbody orAddComponent2 = transform2.GetOrAddComponent<Rigidbody>();
                    orAddComponent2.useGravity = false;
                    orAddComponent2.isKinematic = true;
                }
                else
                {
                    reuseableRigidbodyList.Clear();
                    transform2.GetComponentsInChildren(reuseableRigidbodyList);
                    foreach (Rigidbody reuseableRigidbody in reuseableRigidbodyList)
                    {
                        UnityEngine.Object.Destroy(reuseableRigidbody);
                    }
                }
            }
        }
        if (Provider.isServer)
        {
            GameObject gameObject6 = asset.triggersGameObject?.getOrLoad();
            if (gameObject6 != null)
            {
                Transform transform3 = UnityEngine.Object.Instantiate(gameObject6).transform;
                transform3.name = "Triggers";
                transform3.parent = this.transform;
                transform3.localPosition = Vector3.zero;
                transform3.localRotation = Quaternion.identity;
                transform3.localScale = Vector3.one;
                if (asset.shouldAddKillTriggers)
                {
                    int childCount = transform3.childCount;
                    for (int k = 0; k < childCount; k++)
                    {
                        Transform child = transform3.GetChild(k);
                        if (child.name.Equals("Kill", StringComparison.InvariantCultureIgnoreCase))
                        {
                            child.tag = "Trap";
                            child.gameObject.layer = 30;
                            child.gameObject.AddComponent<Barrier>();
                        }
                    }
                }
            }
        }
        if (asset.type != EObjectType.SMALL)
        {
            if (Level.isEditor)
            {
                Transform transform4 = this.transform.Find("Block");
                if (transform4 != null && this.transform.GetComponent<Collider>() == null)
                {
                    BoxCollider component4 = transform4.GetComponent<BoxCollider>();
                    if (component4 != null)
                    {
                        BoxCollider boxCollider = this.transform.gameObject.AddComponent<BoxCollider>();
                        boxCollider.center = component4.center;
                        boxCollider.size = component4.size;
                    }
                }
            }
            else if (Provider.isClient)
            {
                GameObject gameObject7 = asset.slotsGameObject?.getOrLoad();
                if (gameObject7 != null)
                {
                    Transform obj = UnityEngine.Object.Instantiate(gameObject7).transform;
                    obj.name = "Slots";
                    obj.parent = this.transform;
                    obj.localPosition = Vector3.zero;
                    obj.localRotation = Quaternion.identity;
                    obj.localScale = Vector3.one;
                    reuseableRigidbodyList.Clear();
                    obj.GetComponentsInChildren(reuseableRigidbodyList);
                    foreach (Rigidbody reuseableRigidbody2 in reuseableRigidbodyList)
                    {
                        UnityEngine.Object.Destroy(reuseableRigidbody2);
                    }
                }
            }
        }
        if (asset.interactability != 0)
        {
            if (asset.interactability == EObjectInteractability.BINARY_STATE)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectBinaryState>();
            }
            else if (asset.interactability == EObjectInteractability.DROPPER)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectDropper>();
            }
            else if (asset.interactability == EObjectInteractability.NOTE)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectNote>();
            }
            else if (asset.interactability == EObjectInteractability.WATER || asset.interactability == EObjectInteractability.FUEL)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectResource>();
            }
            else if (asset.interactability == EObjectInteractability.NPC)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectNPC>();
            }
            else if (asset.interactability == EObjectInteractability.QUEST)
            {
                _interactableObj = this.transform.gameObject.AddComponent<InteractableObjectQuest>();
            }
            if (interactable != null)
            {
                interactable.updateState(asset, state);
            }
        }
        if (asset.rubble != 0)
        {
            if (asset.rubble == EObjectRubble.DESTROY)
            {
                _rubble = this.transform.gameObject.AddComponent<InteractableObjectRubble>();
            }
            if (rubble != null)
            {
                rubble.updateState(asset, state);
            }
            if (asset.rubbleEditor == EObjectRubbleEditor.DEAD && Level.isEditor)
            {
                Transform transform5 = this.transform.Find("Editor");
                if (transform5 != null)
                {
                    transform5.gameObject.SetActive(value: true);
                }
            }
        }
        bool flag = false;
        if (asset.conditions != null && asset.conditions.Length != 0 && !Level.isEditor && !Dedicator.IsDedicatedServer)
        {
            areConditionsMet = false;
            flag = true;
            Player.onPlayerCreated = (PlayerCreated)Delegate.Combine(Player.onPlayerCreated, new PlayerCreated(onPlayerCreated));
        }
        if (!flag && (asset.holidayRestriction != 0 || asset.isGore) && !Level.isEditor)
        {
            areConditionsMet = false;
            updateConditions();
        }
        if (asset.foliage.isValid)
        {
            FoliageSurfaceComponent foliageSurfaceComponent = this.transform.gameObject.AddComponent<FoliageSurfaceComponent>();
            foliageSurfaceComponent.foliage = asset.foliage;
            foliageSurfaceComponent.surfaceCollider = this.transform.gameObject.GetComponent<Collider>();
        }
        if (asset.lod != 0)
        {
            GameObject gameObject8 = new GameObject();
            ownedCullingVolume = gameObject8.AddComponent<CullingVolume>();
            ownedCullingVolume.SetupForLevelObject(this);
        }
    }

    private void UpdateRenderersEnabled()
    {
        bool flag = isVisualEnabled && (isVisibleInCullingVolume || (bool)disableCullingVolumes);
        if (areRenderersEnabled != flag)
        {
            SetRenderersEnabled(flag);
        }
    }

    private void SetRenderersEnabled(bool isEnabled)
    {
        areRenderersEnabled = isEnabled;
        if (Dedicator.IsDedicatedServer || isDecal || renderers == null)
        {
            return;
        }
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = areRenderersEnabled;
            }
        }
    }

    [Obsolete("Replaced by SetActive(true)")]
    public void enableCollision()
    {
        SetActive(shouldBeActive: true);
    }

    [Obsolete("Replaced by SetEnableRenderers(true)")]
    public void enableVisual()
    {
        SetIsVisibleInRegion(isVisible: true);
    }

    [Obsolete("Replaced by SetSkyboxActive(true)")]
    public void enableSkybox()
    {
        SetSkyboxActive(shouldBeActive: true);
    }

    [Obsolete("Replaced by SetActive(false)")]
    public void disableCollision()
    {
        SetActive(shouldBeActive: false);
    }

    [Obsolete("Replaced by SetEnableRenderers(false)")]
    public void disableVisual()
    {
        SetIsVisibleInRegion(isVisible: false);
    }

    [Obsolete("Replaced by SetSkyboxActive(false)")]
    public void disableSkybox()
    {
        SetSkyboxActive(shouldBeActive: false);
    }
}
