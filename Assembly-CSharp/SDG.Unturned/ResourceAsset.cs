using System;
using System.Collections.Generic;
using UnityEngine;

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

    public ResourceAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 50 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 50");
        }
        isSpeedTree = false;
        defaultLODWeights = data.has("SpeedTree_Default_LOD_Weights");
        _resourceName = localization.format("Name");
        if (data.readBoolean("Has_Clip_Prefab", defaultValue: true))
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
        health = data.readUInt16("Health", 0);
        radius = data.readSingle("Radius");
        scale = Mathf.Abs(data.readSingle("Scale"));
        verticalOffset = data.readSingle("Vertical_Offset", -0.75f);
        explosion = data.ReadGuidOrLegacyId("Explosion", out _explosionGuid);
        log = data.readUInt16("Log", 0);
        stick = data.readUInt16("Stick", 0);
        rewardID = data.readUInt16("Reward_ID", 0);
        rewardXP = data.readUInt32("Reward_XP");
        if (data.has("Reward_Min"))
        {
            rewardMin = data.readByte("Reward_Min", 0);
        }
        else
        {
            rewardMin = 6;
        }
        if (data.has("Reward_Max"))
        {
            rewardMax = data.readByte("Reward_Max", 0);
        }
        else
        {
            rewardMax = 9;
        }
        bladeID = data.readByte("BladeID", 0);
        vulnerableToFists = data.readBoolean("Vulnerable_To_Fists");
        vulnerableToAllMeleeWeapons = data.readBoolean("Vulnerable_To_All_Melee_Weapons");
        reset = data.readSingle("Reset");
        isForage = data.has("Forage");
        if (isForage && _modelGameObject != null)
        {
            Transform transform = _modelGameObject.transform.Find("Forage");
            if (transform != null)
            {
                transform.gameObject.layer = 14;
            }
        }
        forageRewardExperience = data.readUInt32("Forage_Reward_Experience", 1u);
        if (isForage)
        {
            interactabilityText = localization.read("Interact");
            interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
        }
        hasDebris = !data.has("No_Debris");
        if (data.has("Holiday_Restriction"))
        {
            holidayRestriction = (ENPCHoliday)Enum.Parse(typeof(ENPCHoliday), data.readString("Holiday_Restriction"), ignoreCase: true);
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
    }
}
