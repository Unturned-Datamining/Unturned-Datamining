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

    /// <summary>
    /// If false this barricade cannot take damage.
    /// </summary>
    public bool canBeDamaged = true;

    /// <summary>
    /// Modded barricades can disable pooling if they have custom incompatible logic.
    /// </summary>
    public bool eligibleForPooling = true;

    protected bool _isLocked;

    protected bool _isVulnerable;

    protected bool _bypassClaim;

    protected bool _isRepairable;

    protected bool _proofExplosion;

    protected bool _isUnpickupable;

    /// <summary>
    /// Defaults to false, except for explosive charges which bypass claims.
    /// Useful for collectible barricades that raiders can steal without destroying.
    /// </summary>
    public bool shouldBypassPickupOwnership;

    protected bool _isSalvageable;

    protected bool _isSaveable;

    /// <summary>
    /// Optional alternative barricade prefab specifically for the client preview spawned.
    /// </summary>
    public MasterBundleReference<GameObject> placementPreviewRef;

    private Guid _vehicleGuid;

    private ushort _vehicleId;

    /// <summary>
    /// Nelson 2025-09-08: experimentally exposing to PlayerInput for server-side barricade hit validation. If
    /// hasClipPrefab is false then client-supplied colliderTransform must be valid.
    /// </summary>
    internal bool hasClipPrefab;

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

    /// <summary>
    /// Defaults to false, except for explosive charges which bypass claims.
    /// If true the item can be placed inside player clip volumes. (out of bounds)
    /// </summary>
    public bool AllowPlacementInsideClipVolumes { get; private set; }

    public bool isSalvageable => _isSalvageable;

    public float salvageDurationMultiplier { get; protected set; }

    public bool isSaveable => _isSaveable;

    /// <summary>
    /// Should door colliders remain active while animation is playing?
    /// Useful in special cases such as modded elevators, but prone to physics exploits.
    /// </summary>
    public bool allowCollisionWhileAnimating { get; protected set; }

    public override bool shouldFriendlySentryTargetUser => true;

    public bool useWaterHeightTransparentSort { get; protected set; }

    /// <summary>
    /// By default, vehicles with "hooks" (such as the Skycrane) cannot pick up vehicles with barricades attached.
    /// If all barricades on the vehicle set this to true then the vehicle *can* be picked up. Defaults to false.
    /// </summary>
    public bool CanParentVehicleBePickedUp { get; protected set; }

    /// <summary>
    /// Vehicle to place.
    /// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
    /// </summary>
    public Guid VehicleGuid => _vehicleGuid;

    /// <summary>
    /// Legacy ID of vehicle to place.
    /// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
    /// </summary>
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
            byte[] array = new byte[4];
            BitConverter.TryWriteBytes(array, Provider.time);
            return array;
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
        return safezone.CurrentlyAllowsBuilding;
    }

    /// <summary>
    /// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
    /// vehicle spawner requires to properly set paint color.
    /// </summary>
    internal Asset FindVehicleAsset()
    {
        return Assets.FindBaseVehicleAssetByGuidOrLegacyId(_vehicleGuid, _vehicleId);
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.HasFlag(EItemDescriptionFlags.Uncategorized) && build != EBuild.VEHICLE)
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
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotRepair"), isBeneficial: false), 20001);
            }
            if (proofExplosion)
            {
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ExplosionProof"), isBeneficial: true), 19999);
            }
            if (isLocked)
            {
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Lockable"), isBeneficial: true), 19999);
            }
            if (!_isVulnerable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Invulnerable"), 19999);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        hasClipPrefab = p.data.ParseBool("Has_Clip_Prefab", defaultValue: true);
        bool flag;
        if (Dedicator.IsDedicatedServer && hasClipPrefab)
        {
            _barricade = p.bundle.load<GameObject>("Clip");
            if (barricade == null)
            {
                flag = true;
                Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Barricade\" GameObject instead");
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
            _barricade = p.bundle.load<GameObject>("Barricade");
            if (barricade == null)
            {
                Assets.ReportError(this, "missing \"Barricade\" GameObject");
            }
            else if (Dedicator.IsDedicatedServer)
            {
                ServerPrefabUtil.RemoveClientComponents(_barricade, this);
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
        placementPreviewRef = p.data.readMasterBundleReference<GameObject>("PlacementPreviewPrefab", p.bundle);
        _nav = p.bundle.load<GameObject>("Nav");
        _use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "PlacementAudioClip");
        _build = (EBuild)Enum.Parse(typeof(EBuild), p.data.GetString("Build"), ignoreCase: true);
        if ((build == EBuild.DOOR || build == EBuild.GATE || build == EBuild.SHUTTER) && barricade != null && barricade.transform.Find("Placeholder") == null)
        {
            Assets.ReportError(this, "missing 'Placeholder' Collider");
        }
        _health = p.data.ParseUInt16("Health", 0);
        _range = p.data.ParseFloat("Range");
        _radius = p.data.ParseFloat("Radius");
        _offset = p.data.ParseFloat("Offset");
        if (radius > 0.05f && Mathf.Abs(radius - offset) < 0.05f)
        {
            _radius -= 0.05f;
        }
        _explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
        if (build == EBuild.VEHICLE)
        {
            _vehicleId = _explosion;
            _vehicleGuid = _explosionGuid;
        }
        canBeDamaged = p.data.ParseBool("Can_Be_Damaged", defaultValue: true);
        bool defaultValue = build != EBuild.BEACON;
        eligibleForPooling = p.data.ParseBool("Eligible_For_Pooling", defaultValue);
        _isLocked = p.data.ContainsKey("Locked");
        _isVulnerable = p.data.ContainsKey("Vulnerable");
        if (p.data.TryParseBool("Bypass_Claim", out var value))
        {
            _bypassClaim = value;
        }
        else if (p.data.ContainsKey("Bypass_Claim"))
        {
            _bypassClaim = true;
        }
        else
        {
            _bypassClaim = build == EBuild.CHARGE;
        }
        bool defaultValue2 = build != EBuild.BED && build != EBuild.SENTRY && build != EBuild.SENTRY_FREEFORM;
        allowPlacementOnVehicle = p.data.ParseBool("Allow_Placement_On_Vehicle", defaultValue2);
        _isRepairable = !p.data.ContainsKey("Unrepairable");
        _proofExplosion = p.data.ContainsKey("Proof_Explosion");
        _isUnpickupable = p.data.ContainsKey("Unpickupable");
        shouldBypassPickupOwnership = p.data.ParseBool("Bypass_Pickup_Ownership", build == EBuild.CHARGE);
        AllowPlacementInsideClipVolumes = p.data.ParseBool("Allow_Placement_Inside_Clip_Volumes", build == EBuild.CHARGE);
        _isSalvageable = !p.data.ContainsKey("Unsalvageable");
        salvageDurationMultiplier = p.data.ParseFloat("Salvage_Duration_Multiplier", 1f);
        _isSaveable = !p.data.ContainsKey("Unsaveable");
        allowCollisionWhileAnimating = p.data.ParseBool("Allow_Collision_While_Animating");
        useWaterHeightTransparentSort = p.data.ContainsKey("Use_Water_Height_Transparent_Sort");
        if (p.data.ContainsKey("CanVehicleHookWhileAttached"))
        {
            CanParentVehicleBePickedUp = p.data.ParseBool("CanVehicleHookWhileAttached");
        }
        else
        {
            CanParentVehicleBePickedUp = p.data.ParseBool("CanParentVehicleBePickedUp");
        }
        if (p.data.ContainsKey("Armor_Tier"))
        {
            armorTier = (EArmorTier)Enum.Parse(typeof(EArmorTier), p.data.GetString("Armor_Tier"), ignoreCase: true);
        }
        else if (name.Contains("Metal"))
        {
            armorTier = EArmorTier.HIGH;
        }
        else
        {
            armorTier = EArmorTier.LOW;
        }
        if ((build != EBuild.OVEN && build != EBuild.TORCH && build != EBuild.CAMPFIRE) || !p.data.ParseBool("RequiresHeatSourceCraftingTagConversion", defaultValue: true) || !(_barricade != null))
        {
            return;
        }
        Transform transform = _barricade.transform.Find("Fire");
        if (!(transform != null))
        {
            return;
        }
        if (base.PlaceableProvidedCraftingTags == null)
        {
            base.PlaceableProvidedCraftingTags = new CachingAssetRef[1] { PowerTool.VanillaCraftingHeatTag };
        }
        else if ((bool)Assets.shouldValidateAssets)
        {
            bool flag2 = false;
            CachingAssetRef[] placeableProvidedCraftingTags = base.PlaceableProvidedCraftingTags;
            for (int i = 0; i < placeableProvidedCraftingTags.Length; i++)
            {
                if (placeableProvidedCraftingTags[i] == PowerTool.VanillaCraftingHeatTag)
                {
                    flag2 = true;
                    break;
                }
            }
            if (!flag2)
            {
                ReportAssetError("specifies PlaceableProvidedCraftingTags without Heat Source tag but has RequiresHeatSourceCraftingTagConversion enabled");
            }
        }
        CraftingTagModifierComponent craftingTagModifierComponent = transform.gameObject.AddComponent<CraftingTagModifierComponent>();
        craftingTagModifierComponent.tagGuids = new string[1] { "20f30322bbcc4b01a4f116d22b24c21a" };
        craftingTagModifierComponent.mode = CraftingTagModifierComponent.EMode.Remove;
        craftingTagModifierComponent.activationRequirement = CraftingTagModifierComponent.EActivationRequirement.Invert;
        CraftingTagProviderComponent orAddComponent = _barricade.GetOrAddComponent<CraftingTagProviderComponent>();
        if (orAddComponent.modifiers != null && orAddComponent.modifiers.Length != 0)
        {
            ReportAssetError("has RequiresHeatSourceCraftingTagConversion enabled, but barricade already has a CraftingTagProviderComponent attached!");
        }
        orAddComponent.modifiers = new CraftingTagModifierComponent[1] { craftingTagModifierComponent };
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Barricade");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Build", build);
        orAddDeclaration.Append("Health", health);
        orAddDeclaration.Append("Range", range);
        orAddDeclaration.Append("Radius", radius);
        orAddDeclaration.Append("Offset", offset);
        orAddDeclaration.Append("Explosion", explosion);
        orAddDeclaration.Append("Can_Be_Damaged", canBeDamaged);
        orAddDeclaration.Append("Eligible_For_Pooling", eligibleForPooling);
        orAddDeclaration.Append("Locked", isLocked);
        orAddDeclaration.Append("Vulnerable", isVulnerable);
        orAddDeclaration.Append("Bypass_Claim", bypassClaim);
        orAddDeclaration.Append("Allow_Placement_On_Vehicle", allowPlacementOnVehicle);
        orAddDeclaration.Append("Unrepairable", !isRepairable);
        orAddDeclaration.Append("Proof_Explosion", proofExplosion);
        orAddDeclaration.Append("Unpickupable", isUnpickupable);
        orAddDeclaration.Append("Bypass_Pickup_Ownership", shouldBypassPickupOwnership);
        orAddDeclaration.Append("Allow_Placement_Inside_Clip_Volumes", AllowPlacementInsideClipVolumes);
        orAddDeclaration.Append("Unsalvageable", !isSalvageable);
        orAddDeclaration.Append("Salvage_Duration_Multiplier", salvageDurationMultiplier);
        orAddDeclaration.Append("Unsaveable", !isSaveable);
        orAddDeclaration.Append("Allow_Collision_While_Animating", allowCollisionWhileAnimating);
        orAddDeclaration.Append("Use_Water_Height_Transparent_Sort", useWaterHeightTransparentSort);
        orAddDeclaration.Append("CanParentVehicleBePickedUp", CanParentVehicleBePickedUp);
        orAddDeclaration.Append("Armor_Tier", armorTier);
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
