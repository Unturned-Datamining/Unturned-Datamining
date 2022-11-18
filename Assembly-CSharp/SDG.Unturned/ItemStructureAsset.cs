using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ItemStructureAsset : ItemAsset
{
    protected GameObject _structure;

    protected GameObject _nav;

    protected AudioClip _use;

    protected EConstruct _construct;

    protected ushort _health;

    protected float _range;

    private Guid _explosionGuid;

    protected ushort _explosion;

    public bool canBeDamaged = true;

    public bool eligibleForPooling = true;

    public bool requiresPillars = true;

    protected bool _isVulnerable;

    protected bool _isRepairable;

    protected bool _proofExplosion;

    protected bool _isUnpickupable;

    protected bool _isSalvageable;

    protected bool _isSaveable;

    private static List<Transform> transformsToDestroy = new List<Transform>();

    public GameObject structure => _structure;

    [Obsolete("Only one of Structure.prefab or Clip.prefab are loaded now as _structure")]
    public GameObject clip => _structure;

    public GameObject nav => _nav;

    public AudioClip use => _use;

    public EConstruct construct => _construct;

    public ushort health => _health;

    public float range => _range;

    public Guid explosionGuid => _explosionGuid;

    public ushort explosion
    {
        [Obsolete]
        get
        {
            return _explosion;
        }
    }

    public bool isVulnerable => _isVulnerable;

    public bool isRepairable => _isRepairable;

    public bool proofExplosion => _proofExplosion;

    public bool isUnpickupable => _isUnpickupable;

    public bool isSalvageable => _isSalvageable;

    public float salvageDurationMultiplier { get; protected set; }

    public bool isSaveable => _isSaveable;

    public EArmorTier armorTier { get; protected set; }

    public float foliageCutRadius { get; protected set; }

    public override bool shouldFriendlySentryTargetUser => true;

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, _explosion);
    }

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        return !safezone.noBuildables;
    }

    public ItemStructureAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        bool flag;
        if (data.readBoolean("Has_Clip_Prefab", defaultValue: true))
        {
            _structure = bundle.load<GameObject>("Clip");
            if (structure == null)
            {
                flag = true;
                Assets.reportError(this, "missing \"Clip\" GameObject, loading \"Structure\" GameObject instead");
            }
            else
            {
                flag = false;
                AssetValidation.searchGameObjectForErrors(this, structure);
            }
        }
        else
        {
            flag = true;
        }
        if (flag)
        {
            _structure = bundle.load<GameObject>("Structure");
            if (structure == null)
            {
                Assets.reportError(this, "missing \"Structure\" GameObject");
            }
            else
            {
                AssetValidation.searchGameObjectForErrors(this, structure);
                ServerPrefabUtil.RemoveClientComponents(_structure);
                RemoveClientComponents(_structure);
            }
        }
        _nav = bundle.load<GameObject>("Nav");
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "PlacementAudioClip");
        _construct = (EConstruct)Enum.Parse(typeof(EConstruct), data.readString("Construct"), ignoreCase: true);
        _health = data.readUInt16("Health", 0);
        _range = data.readSingle("Range");
        _explosion = data.ReadGuidOrLegacyId("Explosion", out _explosionGuid);
        canBeDamaged = data.readBoolean("Can_Be_Damaged", defaultValue: true);
        eligibleForPooling = data.readBoolean("Eligible_For_Pooling", defaultValue: true);
        requiresPillars = data.readBoolean("Requires_Pillars", defaultValue: true);
        _isVulnerable = data.has("Vulnerable");
        _isRepairable = !data.has("Unrepairable");
        _proofExplosion = data.has("Proof_Explosion");
        _isUnpickupable = data.has("Unpickupable");
        _isSalvageable = !data.has("Unsalvageable");
        salvageDurationMultiplier = data.readSingle("Salvage_Duration_Multiplier", 1f);
        _isSaveable = !data.has("Unsaveable");
        if (data.has("Armor_Tier"))
        {
            armorTier = (EArmorTier)Enum.Parse(typeof(EArmorTier), data.readString("Armor_Tier"), ignoreCase: true);
        }
        else if (name.Contains("Metal") || name.Contains("Brick"))
        {
            armorTier = EArmorTier.HIGH;
        }
        else
        {
            armorTier = EArmorTier.LOW;
        }
        foliageCutRadius = data.readSingle("Foliage_Cut_Radius", 6f);
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        if (name.Contains("Metal", StringComparison.InvariantCultureIgnoreCase))
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallMetal.asset");
        }
        if (size_x <= 1 || size_y <= 1)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/LightMetalEquipment.asset");
        }
        if (size_x <= 2 || size_y <= 2)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/MediumMetalEquipment.asset");
        }
        return new AudioReference("core.masterbundle", "Sounds/Inventory/HeavyMetalEquipment.asset");
    }

    private void RemoveClientComponents(GameObject gameObject)
    {
        foreach (Transform item in gameObject.transform)
        {
            if (item.name == "Climb" || item.name == "Hatch" || item.name == "Slot" || item.name == "Door" || item.name == "Gate")
            {
                transformsToDestroy.Add(item);
            }
        }
        foreach (Transform item2 in transformsToDestroy)
        {
            UnityEngine.Object.DestroyImmediate(item2.gameObject, allowDestroyingAssets: true);
        }
        transformsToDestroy.Clear();
    }
}
