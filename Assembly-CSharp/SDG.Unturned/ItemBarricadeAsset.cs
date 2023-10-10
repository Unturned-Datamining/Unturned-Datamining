using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemBarricadeAsset : ItemPlaceableAsset
{
    protected GameObject _barricade;

    protected GameObject _nav;

    protected AudioClip _use;

    protected EBuild _build;

    protected ushort _health;

    protected float _range;

    protected float _radius;

    protected float _offset;

    private Guid _explosionGuid;

    protected ushort _explosion;

    public bool canBeDamaged = true;

    public bool eligibleForPooling = true;

    protected bool _isLocked;

    protected bool _isVulnerable;

    protected bool _bypassClaim;

    protected bool _isRepairable;

    protected bool _proofExplosion;

    protected bool _isUnpickupable;

    public bool shouldBypassPickupOwnership;

    protected bool _isSalvageable;

    protected bool _isSaveable;

    public MasterBundleReference<GameObject> placementPreviewRef;

    private Guid _vehicleGuid;

    private ushort _vehicleId;

    public GameObject barricade => _barricade;

    [Obsolete("Only one of Barricade.prefab or Clip.prefab are loaded now as _barricade")]
    public GameObject clip => _barricade;

    public GameObject nav => _nav;

    public AudioClip use => _use;

    public EBuild build => _build;

    public ushort health => _health;

    public float range => _range;

    public float radius => _radius;

    public float offset => _offset;

    public Guid explosionGuid => _explosionGuid;

    public ushort explosion
    {
        [Obsolete]
        get
        {
            return _explosion;
        }
    }

    public bool isLocked => _isLocked;

    public bool isVulnerable => _isVulnerable;

    public EArmorTier armorTier { get; protected set; }

    public bool bypassClaim => _bypassClaim;

    public bool allowPlacementOnVehicle { get; protected set; }

    public bool isRepairable => _isRepairable;

    public bool proofExplosion => _proofExplosion;

    public bool isUnpickupable => _isUnpickupable;

    public bool AllowPlacementInsideClipVolumes { get; private set; }

    public bool isSalvageable => _isSalvageable;

    public float salvageDurationMultiplier { get; protected set; }

    public bool isSaveable => _isSaveable;

    public bool allowCollisionWhileAnimating { get; protected set; }

    public override bool shouldFriendlySentryTargetUser => true;

    public bool useWaterHeightTransparentSort { get; protected set; }

    public Guid VehicleGuid => _vehicleGuid;

    public ushort VehicleId
    {
        [Obsolete]
        get
        {
            return _vehicleId;
        }
    }

    public override byte[] getState(EItemOrigin origin)
    {
        if (build == EBuild.DOOR || build == EBuild.GATE || build == EBuild.SHUTTER || build == EBuild.HATCH)
        {
            return new byte[17];
        }
        if (build == EBuild.BED)
        {
            return new byte[8];
        }
        if (build == EBuild.FARM)
        {
            return new byte[4];
        }
        if (build == EBuild.TORCH || build == EBuild.CAMPFIRE || build == EBuild.OVEN || build == EBuild.SPOT || build == EBuild.SAFEZONE || build == EBuild.OXYGENATOR || build == EBuild.BARREL_RAIN || build == EBuild.CAGE)
        {
            return new byte[1];
        }
        if (build == EBuild.OIL)
        {
            return new byte[2];
        }
        if (build == EBuild.SIGN || build == EBuild.SIGN_WALL || build == EBuild.NOTE)
        {
            return new byte[17];
        }
        if (build == EBuild.STEREO)
        {
            return new byte[17];
        }
        if (build == EBuild.MANNEQUIN)
        {
            return new byte[73];
        }
        return new byte[0];
    }

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, _explosion);
    }

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        return !safezone.noBuildables;
    }

    internal VehicleAsset FindVehicleAsset()
    {
        return Assets.FindVehicleAssetByGuidOrLegacyId(_vehicleGuid, _vehicleId);
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent && build != EBuild.VEHICLE)
        {
            if (_health > 0)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Health", _health), 20000);
            }
            switch (armorTier)
            {
            case EArmorTier.LOW:
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ArmorTier_Low"), 20000);
                break;
            case EArmorTier.HIGH:
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ArmorTier_High"), 20000);
                break;
            }
            if (_isUnpickupable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotPickup"), 20000);
            }
            else if (!_isSalvageable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotSalvage"), 20000);
            }
            if (!isRepairable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotRepair"), 20000);
            }
            if (proofExplosion)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ExplosionProof"), 20000);
            }
            if (isLocked)
            {
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Lockable"), isBeneficial: true), 20000);
            }
            if (!_isVulnerable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Invulnerable"), 20000);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        bool flag;
        if (Dedicator.IsDedicatedServer && data.ParseBool("Has_Clip_Prefab", defaultValue: true))
        {
            _barricade = bundle.load<GameObject>("Clip");
            if (barricade == null)
            {
                flag = true;
                Assets.reportError(this, "missing \"Clip\" GameObject, loading \"Barricade\" GameObject instead");
            }
            else
            {
                flag = false;
            }
        }
        else
        {
            flag = true;
        }
        if (flag)
        {
            _barricade = bundle.load<GameObject>("Barricade");
            if (barricade == null)
            {
                Assets.reportError(this, "missing \"Barricade\" GameObject");
            }
            else if (Dedicator.IsDedicatedServer)
            {
                ServerPrefabUtil.RemoveClientComponents(_barricade);
            }
        }
        if (barricade != null)
        {
            if ((bool)Assets.shouldValidateAssets)
            {
                AssetValidation.searchGameObjectForErrors(this, barricade);
            }
            barricade.transform.localPosition = Vector3.zero;
            barricade.transform.localRotation = Quaternion.identity;
        }
        placementPreviewRef = data.readMasterBundleReference<GameObject>("PlacementPreviewPrefab", bundle);
        _nav = bundle.load<GameObject>("Nav");
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "PlacementAudioClip");
        _build = (EBuild)Enum.Parse(typeof(EBuild), data.GetString("Build"), ignoreCase: true);
        if ((build == EBuild.DOOR || build == EBuild.GATE || build == EBuild.SHUTTER) && barricade != null && barricade.transform.Find("Placeholder") == null)
        {
            Assets.reportError(this, "missing 'Placeholder' Collider");
        }
        _health = data.ParseUInt16("Health", 0);
        _range = data.ParseFloat("Range");
        _radius = data.ParseFloat("Radius");
        _offset = data.ParseFloat("Offset");
        if (radius > 0.05f && Mathf.Abs(radius - offset) < 0.05f)
        {
            _radius -= 0.05f;
        }
        _explosion = data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
        if (build == EBuild.VEHICLE)
        {
            _vehicleId = _explosion;
            _vehicleGuid = _explosionGuid;
        }
        canBeDamaged = data.ParseBool("Can_Be_Damaged", defaultValue: true);
        bool defaultValue = build != EBuild.BEACON;
        eligibleForPooling = data.ParseBool("Eligible_For_Pooling", defaultValue);
        _isLocked = data.ContainsKey("Locked");
        _isVulnerable = data.ContainsKey("Vulnerable");
        _bypassClaim = data.ContainsKey("Bypass_Claim");
        bool defaultValue2 = build != EBuild.BED && build != EBuild.SENTRY && build != EBuild.SENTRY_FREEFORM;
        allowPlacementOnVehicle = data.ParseBool("Allow_Placement_On_Vehicle", defaultValue2);
        _isRepairable = !data.ContainsKey("Unrepairable");
        _proofExplosion = data.ContainsKey("Proof_Explosion");
        _isUnpickupable = data.ContainsKey("Unpickupable");
        shouldBypassPickupOwnership = data.ParseBool("Bypass_Pickup_Ownership", build == EBuild.CHARGE);
        AllowPlacementInsideClipVolumes = data.ParseBool("Allow_Placement_Inside_Clip_Volumes", build == EBuild.CHARGE);
        _isSalvageable = !data.ContainsKey("Unsalvageable");
        salvageDurationMultiplier = data.ParseFloat("Salvage_Duration_Multiplier", 1f);
        _isSaveable = !data.ContainsKey("Unsaveable");
        allowCollisionWhileAnimating = data.ParseBool("Allow_Collision_While_Animating");
        useWaterHeightTransparentSort = data.ContainsKey("Use_Water_Height_Transparent_Sort");
        if (data.ContainsKey("Armor_Tier"))
        {
            armorTier = (EArmorTier)Enum.Parse(typeof(EArmorTier), data.GetString("Armor_Tier"), ignoreCase: true);
        }
        else if (name.Contains("Metal"))
        {
            armorTier = EArmorTier.HIGH;
        }
        else
        {
            armorTier = EArmorTier.LOW;
        }
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        if (name.Contains("Seed", StringComparison.InvariantCultureIgnoreCase))
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/Seeds.asset");
        }
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
}
