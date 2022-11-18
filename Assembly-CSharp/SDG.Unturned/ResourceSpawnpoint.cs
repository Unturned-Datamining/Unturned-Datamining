using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ResourceSpawnpoint
{
    private static List<Collider> colliders = new List<Collider>();

    [Obsolete("Unused index into LevelGround.resources for early versions of the level editor.")]
    public byte type;

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

    private Collider stumpCollider;

    private Transform _skybox;

    public ushort health;

    public float lastDead => _lastDead;

    public bool isDead => health == 0;

    public Vector3 point => _point;

    public bool isGenerated => _isGenerated;

    public Quaternion angle => _angle;

    public Vector3 scale => _scale;

    public ResourceAsset asset => _asset;

    public bool isEnabled { get; private set; }

    public bool isSkyboxEnabled { get; private set; }

    public Transform model => _model;

    public Transform stump => _stump;

    public Transform skybox => _skybox;

    public bool canBeDamaged => areConditionsMet;

    public bool checkCanReset(float multiplier)
    {
        if (isDead && asset != null && asset.reset > 1f)
        {
            return Time.realtimeSinceStartup - lastDead > asset.reset * multiplier;
        }
        return false;
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
        if (!isAlive)
        {
            return;
        }
        isAlive = false;
        if (asset != null)
        {
            health = 0;
            if (asset.isForage)
            {
                model.Find("Forage")?.gameObject.SetActive(value: false);
            }
            else
            {
                if (model != null)
                {
                    model.gameObject.SetActive(value: false);
                }
                if (stump != null)
                {
                    stump.gameObject.SetActive(isEnabled);
                }
                if (stumpCollider != null)
                {
                    stumpCollider.enabled = true;
                }
            }
        }
        if ((bool)skybox)
        {
            skybox.gameObject.SetActive(value: false);
        }
    }

    public void revive()
    {
        if (isAlive)
        {
            return;
        }
        isAlive = true;
        if (asset != null)
        {
            if (asset.isForage)
            {
                model.Find("Forage")?.gameObject.SetActive(value: true);
                health = asset.health;
            }
            else
            {
                if (model != null && areConditionsMet)
                {
                    model.gameObject.SetActive(isEnabled);
                }
                health = asset.health;
                if (stump != null && areConditionsMet)
                {
                    stump.gameObject.SetActive(isEnabled && asset.isSpeedTree);
                }
                if (stumpCollider != null && areConditionsMet)
                {
                    stumpCollider.enabled = false;
                }
            }
        }
        if ((bool)skybox && areConditionsMet)
        {
            skybox.gameObject.SetActive(isSkyboxEnabled);
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
            else
            {
                if (model != null)
                {
                    model.gameObject.SetActive(value: false);
                }
                if (stump != null)
                {
                    stump.gameObject.SetActive(isEnabled);
                }
                if (stumpCollider != null)
                {
                    stumpCollider.enabled = true;
                }
            }
        }
        if ((bool)skybox)
        {
            skybox.gameObject.SetActive(value: false);
        }
    }

    public void forceFullEnable()
    {
        isEnabled = true;
        if (model != null)
        {
            model.gameObject.SetActive(value: true);
        }
        if (stump != null)
        {
            stump.gameObject.SetActive(value: true);
        }
    }

    public void enable()
    {
        isEnabled = true;
        if (asset != null && asset.isForage)
        {
            if (model != null && areConditionsMet)
            {
                model.gameObject.SetActive(value: true);
            }
            return;
        }
        if (model != null && areConditionsMet)
        {
            model.gameObject.SetActive(isAlive);
        }
        if (stump != null && areConditionsMet)
        {
            stump.gameObject.SetActive(!isAlive || (asset != null && asset.isSpeedTree));
        }
        if (stumpCollider != null && areConditionsMet)
        {
            stumpCollider.enabled = !isAlive;
        }
    }

    public void enableSkybox()
    {
        isSkyboxEnabled = true;
        if (skybox != null && areConditionsMet)
        {
            skybox.gameObject.SetActive(isAlive);
        }
    }

    public void disable()
    {
        isEnabled = false;
        if (model != null)
        {
            model.gameObject.SetActive(value: false);
        }
        if (stump != null)
        {
            stump.gameObject.SetActive(value: false);
        }
    }

    public void disableSkybox()
    {
        isSkyboxEnabled = false;
        if (skybox != null)
        {
            skybox.gameObject.SetActive(value: false);
        }
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

    private void updateConditions()
    {
        if (asset == null)
        {
            return;
        }
        bool flag = HolidayUtil.isHolidayActive(asset.holidayRestriction);
        if (areConditionsMet == flag)
        {
            return;
        }
        areConditionsMet = flag;
        if (areConditionsMet)
        {
            if (isEnabled)
            {
                enable();
            }
            if (isSkyboxEnabled)
            {
                enableSkybox();
            }
        }
        else
        {
            if (isEnabled)
            {
                disable();
            }
            if (isSkyboxEnabled)
            {
                disableSkybox();
            }
        }
    }

    public ResourceSpawnpoint(byte newType, ushort newID, Vector3 newPoint, bool newGenerated, NetId netId)
    {
        type = newType;
        id = newID;
        _point = newPoint;
        _isGenerated = newGenerated;
        _asset = Assets.find(EAssetType.RESOURCE, id) as ResourceAsset;
        if (asset == null)
        {
            return;
        }
        health = asset.health;
        isAlive = true;
        areConditionsMet = true;
        float num = Mathf.Sin((point.x + 4096f) * 32f + (point.z + 4096f) * 32f);
        _angle = Quaternion.Euler(num * 5f, num * 360f, 0f);
        _scale = new Vector3(1.1f + asset.scale + num * asset.scale, 1.1f + asset.scale + num * asset.scale, 1.1f + asset.scale + num * asset.scale);
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
            model.name = id.ToString();
            model.localScale = scale;
            isEnabled = true;
            if (!netId.IsNull())
            {
                NetIdRegistry.AssignTransform(netId, model.transform);
            }
        }
        GameObject gameObject3 = null;
        if (asset.stumpGameObject != null)
        {
            gameObject3 = asset.stumpGameObject;
        }
        if (gameObject3 != null)
        {
            _stump = UnityEngine.Object.Instantiate(gameObject3, position, angle).transform;
            stump.name = id.ToString();
            stump.localScale = scale;
            stump.gameObject.SetActive(value: false);
            if (asset.isSpeedTree)
            {
                stumpCollider = stump.GetComponent<Collider>();
                stumpCollider.enabled = false;
            }
        }
        if (asset.holidayRestriction != 0 && !Level.isEditor)
        {
            updateConditions();
        }
    }

    public ResourceSpawnpoint(ushort newID, Vector3 newPoint, bool newGenerated, NetId netId)
        : this(0, newID, newPoint, newGenerated, netId)
    {
    }
}
