using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ResourceSpawnpoint
{
    private static List<Collider> colliders = new List<Collider>();

    [Obsolete("Unused index into LevelGround.resources for early versions of the level editor.")]
    public byte type;

    [Obsolete("Trees are now saved by asset GUID. Please use the asset property rather than finding asset by legacy ID.")]
    public ushort id;

    private float _lastDead;

    private bool areConditionsMet;

    private bool isAlive;

    private Vector3 _point;

    private bool _isGenerated;

    private Quaternion _angle;

    private Vector3 _scale;

    private ResourceAsset _asset;

    private Transform _model;

    private Transform _stump;

    private Transform _skybox;

    public ushort health;

    public Guid guid { get; protected set; }

    public float lastDead => _lastDead;

    public bool isDead => health == 0;

    public Vector3 point => _point;

    public bool isGenerated => _isGenerated;

    public Quaternion angle => _angle;

    public Vector3 scale => _scale;

    public ResourceAsset asset => _asset;

    /// <summary>
    /// Tree activation is time-sliced, so this does not necessarily match whether the region is active.
    /// </summary>
    internal bool isActiveInRegion { get; private set; }

    internal bool isSkyboxActiveInRegion { get; private set; }

    public Transform model => _model;

    public Transform stump => _stump;

    public Transform skybox => _skybox;

    /// <summary>
    /// Can this tree be damaged?
    /// Allows holiday restrictions to be taken into account. (Otherwise holiday trees could be destroyed out of season.)
    /// </summary>
    public bool canBeDamaged => areConditionsMet;

    [Obsolete("Renamed to isActiveInRegion")]
    public bool isEnabled => isActiveInRegion;

    [Obsolete("Renamed to isSkyboxActiveInRegion")]
    public bool isSkyboxEnabled => isSkyboxActiveInRegion;

    public bool checkCanReset(float multiplier)
    {
        if (isDead && asset != null && asset.reset > 1f)
        {
            return Time.realtimeSinceStartup - lastDead > asset.reset * multiplier;
        }
        return false;
    }

    internal void SetIsActiveInRegion(bool isActive)
    {
        if (isActiveInRegion != isActive)
        {
            isActiveInRegion = isActive;
            UpdateActive();
        }
    }

    internal void SetIsSkyboxActiveInRegion(bool isActive)
    {
        if (isSkyboxActiveInRegion != isActive)
        {
            isSkyboxActiveInRegion = isActive;
            UpdateSkyboxActive();
        }
    }

    public void askDamage(ushort amount)
    {
        if (amount != 0 && !isDead)
        {
            if (amount >= health)
            {
                health = 0;
            }
            else
            {
                health -= amount;
            }
        }
    }

    public void wipe()
    {
        if (isAlive)
        {
            isAlive = false;
            health = 0;
            UpdateActive();
        }
    }

    public void revive()
    {
        if (!isAlive)
        {
            isAlive = true;
            health = asset?.health ?? 1;
            UpdateActive();
        }
    }

    public void kill(Vector3 ragdoll)
    {
        if (!isAlive)
        {
            return;
        }
        isAlive = false;
        _lastDead = Time.realtimeSinceStartup;
        if (asset != null)
        {
            health = 0;
            if (asset.isForage)
            {
                model.Find("Forage")?.gameObject.SetActive(value: false);
            }
            else if (!Dedicator.IsDedicatedServer && asset.hasDebris && GraphicsSettings.debris)
            {
                ragdoll.y += 8f;
                ragdoll.x += UnityEngine.Random.Range(-16f, 16f);
                ragdoll.z += UnityEngine.Random.Range(-16f, 16f);
                ragdoll *= (float)((Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT) ? 4 : 2);
                if (model != null && asset.modelGameObject != null)
                {
                    Vector3 position = model.position + model.up * asset.DebrisVerticalOffset;
                    GameObject original = ((!(asset.debrisGameObject == null)) ? asset.debrisGameObject : asset.modelGameObject);
                    Transform transform = UnityEngine.Object.Instantiate(original, position, model.rotation).transform;
                    transform.name = asset.name + "_Debris";
                    transform.localScale = model.localScale;
                    transform.tag = "Debris";
                    transform.gameObject.layer = 12;
                    transform.gameObject.AddComponent<Rigidbody>();
                    transform.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    transform.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                    transform.GetComponent<Rigidbody>().AddForce(ragdoll);
                    transform.GetComponent<Rigidbody>().drag = 1f;
                    transform.GetComponent<Rigidbody>().angularDrag = 1f;
                    UnityEngine.Object.Destroy(transform.gameObject, 8f);
                    if (stump != null && stump.gameObject.activeSelf && asset.ShouldIgnoreCollisionBetweenStumpAndDebris)
                    {
                        Collider component = transform.GetComponent<Collider>();
                        if (component != null)
                        {
                            stump.GetComponents(colliders);
                            for (int i = 0; i < colliders.Count; i++)
                            {
                                Physics.IgnoreCollision(component, colliders[i]);
                            }
                        }
                    }
                }
            }
        }
        UpdateActive();
    }

    public void destroy()
    {
        if (model != null)
        {
            UnityEngine.Object.Destroy(model.gameObject);
        }
        if (stump != null)
        {
            UnityEngine.Object.Destroy(stump.gameObject);
        }
        if (skybox != null)
        {
            UnityEngine.Object.Destroy(skybox.gameObject);
        }
    }

    internal Vector3 GetEffectSpawnPosition()
    {
        if (model == null)
        {
            return point;
        }
        Transform transform = model.Find("Effect");
        if (transform != null)
        {
            return transform.position;
        }
        if (asset.hasDebris)
        {
            return model.position + Vector3.up * 8f;
        }
        return model.position;
    }

    internal void SetIsActiveOverrideForSatelliteCapture(bool isActive)
    {
        if (model != null)
        {
            model.gameObject.SetActive(isActive);
        }
        if (stump != null)
        {
            stump.gameObject.SetActive(value: false);
        }
        if (skybox != null)
        {
            skybox.gameObject.SetActive(value: false);
        }
    }

    internal void UpdateActive()
    {
        bool flag = isActiveInRegion || GraphicsSettings.WantsCinematicMode;
        bool flag2 = isAlive;
        bool flag3 = !isAlive;
        if (asset != null && asset.isForage)
        {
            flag2 = true;
            flag3 = true;
            model?.Find("Forage")?.gameObject.SetActive(isAlive);
        }
        bool flag4 = areConditionsMet && (Dedicator.IsDedicatedServer || flag);
        if (model != null)
        {
            model.gameObject.SetActive(flag4 && flag2);
        }
        if (stump != null)
        {
            stump.gameObject.SetActive(flag4 && flag3);
        }
        if (!Dedicator.IsDedicatedServer)
        {
            UpdateSkyboxActive();
        }
    }

    internal void UpdateSkyboxActive()
    {
        if (skybox != null)
        {
            bool flag = GraphicsSettings.landmarkQuality >= EGraphicQuality.MEDIUM && !GraphicsSettings.WantsCinematicMode;
            skybox.gameObject.SetActive(!isActiveInRegion && isSkyboxActiveInRegion && flag && areConditionsMet && isAlive);
        }
    }

    public ResourceSpawnpoint(byte newType, ushort newID, Guid newGuid, Vector3 newPoint, Quaternion newRotation, Vector3 newScale, bool newGenerated, NetId netId)
    {
        type = newType;
        id = newID;
        guid = newGuid;
        _point = newPoint;
        _angle = newRotation;
        _scale = newScale;
        _isGenerated = newGenerated;
        if (guid == Guid.Empty)
        {
            _asset = Assets.find(EAssetType.RESOURCE, id) as ResourceAsset;
            if (asset != null)
            {
                UnturnedLog.info($"Tree without GUID loaded by legacy ID {id}, updating to {asset.GUID:N} \"{asset.FriendlyName}\"");
                guid = asset.GUID;
            }
        }
        else
        {
            _asset = Assets.find(guid) as ResourceAsset;
            if (!Dedicator.IsDedicatedServer)
            {
                ClientAssetIntegrity.QueueRequest(guid, asset, "Tree");
            }
            if (asset == null)
            {
                ClientAssetIntegrity.ServerAddKnownMissingAsset(guid, "Tree");
            }
        }
        if (asset != null)
        {
            health = asset.health;
            isAlive = true;
            areConditionsMet = true;
            GameObject gameObject = null;
            if (asset.modelGameObject != null)
            {
                gameObject = asset.modelGameObject;
            }
            Vector3 position = point + Vector3.up * scale.y * asset.verticalOffset;
            if (gameObject != null)
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, angle);
                _model = gameObject2.transform;
                gameObject2.GetOrAddComponent<TreeRefComponent>().owner = this;
                model.name = asset.name;
                model.localScale = scale;
                if (!Dedicator.IsDedicatedServer)
                {
                    if (!Level.isEditor && asset.isForage)
                    {
                        Transform transform = model.Find("Forage");
                        if (transform != null)
                        {
                            transform.gameObject.AddComponent<InteractableForage>().asset = asset;
                        }
                    }
                    if (asset.skyboxGameObject != null)
                    {
                        Quaternion rotation = angle * Quaternion.Euler(-90f, 0f, 0f);
                        GameObject gameObject3 = UnityEngine.Object.Instantiate(asset.skyboxGameObject, position, rotation);
                        _skybox = gameObject3.transform;
                        skybox.name = asset.name + "_Skybox";
                        skybox.localScale = new Vector3(skybox.localScale.x * scale.x, skybox.localScale.z * scale.z, skybox.localScale.z * scale.z);
                        if (asset.skyboxMaterial != null)
                        {
                            skybox.GetComponent<MeshRenderer>().sharedMaterial = asset.skyboxMaterial;
                        }
                    }
                }
                if (!netId.IsNull())
                {
                    NetIdRegistry.AssignTransform(netId, model.transform);
                }
            }
            GameObject gameObject4 = null;
            if (asset.stumpGameObject != null)
            {
                gameObject4 = asset.stumpGameObject;
            }
            if (gameObject4 != null)
            {
                _stump = UnityEngine.Object.Instantiate(gameObject4, position, angle).transform;
                stump.name = asset.name + "_Stump";
                stump.localScale = scale;
            }
            if (asset.holidayRestriction != 0 && !Level.isEditor)
            {
                areConditionsMet = HolidayUtil.isHolidayActive(asset.holidayRestriction);
            }
        }
        UpdateActive();
    }

    public ResourceSpawnpoint(byte newType, ushort newID, Vector3 newPoint, bool newGenerated, NetId netId)
        : this(newType, newID, Guid.Empty, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
    {
    }

    public ResourceSpawnpoint(ushort newID, Vector3 newPoint, bool newGenerated, NetId netId)
        : this(0, newID, Guid.Empty, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
    {
    }

    public ResourceSpawnpoint(ushort newID, Guid guid, Vector3 newPoint, bool newGenerated, NetId netId)
        : this(0, newID, guid, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
    {
    }

    [Obsolete("Replaced by SetIsActiveInRegion(true)")]
    public void enable()
    {
        SetIsActiveInRegion(isActive: true);
    }

    [Obsolete("Replaced by SetIsSkyboxActiveInRegion(true)")]
    public void enableSkybox()
    {
        SetIsSkyboxActiveInRegion(isActive: true);
    }

    [Obsolete("Replaced by SetIsActiveInRegion(false)")]
    public void disable()
    {
        SetIsActiveInRegion(isActive: false);
    }

    [Obsolete("Replaced by SetIsSkyboxActiveInRegion(false)")]
    public void disableSkybox()
    {
        SetIsSkyboxActiveInRegion(isActive: false);
    }

    [Obsolete]
    public void forceFullEnable()
    {
        SetIsActiveOverrideForSatelliteCapture(isActive: true);
    }
}
