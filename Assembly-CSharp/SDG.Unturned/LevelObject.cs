using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Foliage;
using UnityEngine;

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

    private bool areConditionsMet;

    private bool haveConditionsBeenChecked;

    public Transform transform => _transform;

    public Transform placeholderTransform { get; protected set; }

    public Transform skybox => _skybox;

    public ushort id => _id;

    [Obsolete]
    public string name => null;

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
            _ = asset;
            return false;
        }
    }

    public void enableCollision()
    {
        isCollisionEnabled = true;
    }

    public void enableVisual()
    {
        isVisualEnabled = true;
    }

    public void enableSkybox()
    {
        isSkyboxEnabled = true;
    }

    public void disableCollision()
    {
        isCollisionEnabled = false;
    }

    public void disableVisual()
    {
        isVisualEnabled = false;
    }

    public void disableSkybox()
    {
        isSkyboxEnabled = false;
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
        if (asset == null || asset.conditions == null)
        {
            return;
        }
        INPCCondition[] conditions = asset.conditions;
        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i].isAssociatedWithFlag(id))
            {
                updateConditions();
                break;
            }
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
        PlayerQuests quests3 = Player.player.quests;
        quests3.onFlagUpdated = (FlagUpdated)Delegate.Combine(quests3.onFlagUpdated, new FlagUpdated(onFlagUpdated));
        if (flag2)
        {
            Player.player.quests.OnLocalPlayerQuestsChanged += OnLocalPlayerQuestsChanged;
        }
        updateConditions();
    }

    private void findAsset()
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
                UnturnedLog.info("Object without GUID loaded by legacy ID {0}, updating to {1} \"{2}\"", asset.id, asset.GUID, asset.name);
                _GUID = asset.GUID;
            }
            else
            {
                UnturnedLog.warn("Unable to find object by legacy ID {0}", id);
            }
            return;
        }
        _asset = Assets.find(new AssetReference<ObjectAsset>(GUID));
        if (asset == null)
        {
            _asset = Assets.find(EAssetType.OBJECT, id) as ObjectAsset;
            if (asset != null)
            {
                UnturnedLog.info("Unable to find object for GUID {0} found by legacy ID {1}, updating to {2} \"{3}\"", GUID, id, asset.GUID, asset.name);
                _GUID = asset.GUID;
            }
            else
            {
                UnturnedLog.warn("Unable to find object for GUID {0}, nor by legacy ID {1}", GUID, id);
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
        findAsset();
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
        if (orLoadModel != null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(orLoadModel, newPoint, newRotation);
            _transform = gameObject.transform;
            gameObject.name = id.ToString();
            NetIdRegistry.AssignTransform(netId, _transform);
            isDecal = this.transform.Find("Decal");
            if (asset.useScale)
            {
                this.transform.localScale = newScale;
            }
        }
        renderers = null;
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
            if (this.transform.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rigidbody = this.transform.gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }
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
            GameObject gameObject2 = asset.navGameObject?.getOrLoad();
            if (gameObject2 != null)
            {
                Transform transform = UnityEngine.Object.Instantiate(gameObject2).transform;
                transform.name = "Nav";
                transform.parent = this.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                if (Level.isEditor)
                {
                    if (transform.GetComponent<Rigidbody>() == null)
                    {
                        Rigidbody rigidbody2 = transform.gameObject.AddComponent<Rigidbody>();
                        rigidbody2.useGravity = false;
                        rigidbody2.isKinematic = true;
                    }
                }
                else
                {
                    reuseableRigidbodyList.Clear();
                    transform.GetComponentsInChildren(reuseableRigidbodyList);
                    foreach (Rigidbody reuseableRigidbody in reuseableRigidbodyList)
                    {
                        UnityEngine.Object.Destroy(reuseableRigidbody);
                    }
                }
            }
        }
        if (Provider.isServer)
        {
            GameObject gameObject3 = asset.triggersGameObject?.getOrLoad();
            if (gameObject3 != null)
            {
                Transform transform2 = UnityEngine.Object.Instantiate(gameObject3).transform;
                transform2.name = "Triggers";
                transform2.parent = this.transform;
                transform2.localPosition = Vector3.zero;
                transform2.localRotation = Quaternion.identity;
                transform2.localScale = Vector3.one;
                if (asset.shouldAddKillTriggers)
                {
                    int childCount = transform2.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        Transform child = transform2.GetChild(i);
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
                Transform transform3 = this.transform.Find("Block");
                if (transform3 != null && this.transform.GetComponent<Collider>() == null)
                {
                    BoxCollider component4 = transform3.GetComponent<BoxCollider>();
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
                GameObject gameObject4 = asset.slotsGameObject?.getOrLoad();
                if (gameObject4 != null)
                {
                    Transform obj = UnityEngine.Object.Instantiate(gameObject4).transform;
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
                Transform transform4 = this.transform.Find("Editor");
                if (transform4 != null)
                {
                    transform4.gameObject.SetActive(value: true);
                }
            }
        }
        bool flag = false;
        if (asset.conditions != null && asset.conditions.Length != 0)
        {
            _ = Level.isEditor;
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
    }
}
