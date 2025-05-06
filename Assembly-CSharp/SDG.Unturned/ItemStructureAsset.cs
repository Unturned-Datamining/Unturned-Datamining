using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ItemStructureAsset : ItemPlaceableAsset
{
    protected GameObject _structure;

    protected GameObject _nav;

    protected AudioClip _use;

    protected EConstruct _construct;

    protected ushort _health;

    protected float _range;

    private Guid _explosionGuid;

    protected ushort _explosion;

    /// <summary>
    /// If false this structure cannot take damage.
    /// </summary>
    public bool canBeDamaged = true;

    /// <summary>
    /// Modded structures can disable pooling if they have custom incompatible logic.
    /// </summary>
    public bool eligibleForPooling = true;

    public bool requiresPillars = true;

    protected bool _isVulnerable;

    protected bool _isRepairable;

    protected bool _proofExplosion;

    protected bool _isUnpickupable;

    protected bool _isSalvageable;

    protected bool _isSaveable;

    /// <summary>
    /// Optional alternative structure prefab specifically for the client preview spawned.
    /// </summary>
    public MasterBundleReference<GameObject> placementPreviewRef;

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

    /// <summary>
    /// Length of raycast downward from pivot to check floor is above terrain.
    /// Vanilla floors can be placed a maximum of 10 meters above terrain.
    /// </summary>
    public float terrainTestHeight { get; protected set; }

    public override bool shouldFriendlySentryTargetUser => true;

    public EffectAsset FindExplosionEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, _explosion);
    }

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        return !safezone.noBuildables;
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Health", _health), 20000);
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
            if (!_isVulnerable)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Invulnerable"), 20000);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        bool flag;
        if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
        {
            _structure = p.bundle.load<GameObject>("Clip");
            if (structure == null)
            {
                flag = true;
                Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Structure\" GameObject instead");
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
            _structure = p.bundle.load<GameObject>("Structure");
            if (structure == null)
            {
                Assets.ReportError(this, "missing \"Structure\" GameObject");
            }
            else
            {
                AssetValidation.searchGameObjectForErrors(this, structure);
                if (Dedicator.IsDedicatedServer)
                {
                    ServerPrefabUtil.RemoveClientComponents(_structure, this);
                    RemoveClientComponents(_structure);
                }
                else
                {
                    LODGroup component = structure.GetComponent<LODGroup>();
                    if (component != null)
                    {
                        component.DisableCulling();
                    }
                }
            }
        }
        placementPreviewRef = p.data.readMasterBundleReference<GameObject>("PlacementPreviewPrefab", p.bundle);
        _nav = p.bundle.load<GameObject>("Nav");
        _use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "PlacementAudioClip");
        _construct = (EConstruct)Enum.Parse(typeof(EConstruct), p.data.GetString("Construct"), ignoreCase: true);
        _health = p.data.ParseUInt16("Health", 0);
        _range = p.data.ParseFloat("Range");
        _explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
        canBeDamaged = p.data.ParseBool("Can_Be_Damaged", defaultValue: true);
        eligibleForPooling = p.data.ParseBool("Eligible_For_Pooling", defaultValue: true);
        requiresPillars = p.data.ParseBool("Requires_Pillars", defaultValue: true);
        _isVulnerable = p.data.ContainsKey("Vulnerable");
        _isRepairable = !p.data.ContainsKey("Unrepairable");
        _proofExplosion = p.data.ContainsKey("Proof_Explosion");
        _isUnpickupable = p.data.ContainsKey("Unpickupable");
        _isSalvageable = !p.data.ContainsKey("Unsalvageable");
        salvageDurationMultiplier = p.data.ParseFloat("Salvage_Duration_Multiplier", 1f);
        _isSaveable = !p.data.ContainsKey("Unsaveable");
        if (p.data.ContainsKey("Armor_Tier"))
        {
            armorTier = (EArmorTier)Enum.Parse(typeof(EArmorTier), p.data.GetString("Armor_Tier"), ignoreCase: true);
        }
        else if (name.Contains("Metal") || name.Contains("Brick"))
        {
            armorTier = EArmorTier.HIGH;
        }
        else
        {
            armorTier = EArmorTier.LOW;
        }
        foliageCutRadius = p.data.ParseFloat("Foliage_Cut_Radius", 6f);
        terrainTestHeight = p.data.ParseFloat("Terrain_Test_Height", 10f);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Structure");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Construct", construct);
        orAddDeclaration.Append("Health", health);
        orAddDeclaration.Append("Range", range);
        orAddDeclaration.Append("Explosion", explosion);
        orAddDeclaration.Append("Can_Be_Damaged", canBeDamaged);
        orAddDeclaration.Append("Eligible_For_Pooling", eligibleForPooling);
        orAddDeclaration.Append("Requires_Pillars", requiresPillars);
        orAddDeclaration.Append("Vulnerable", isVulnerable);
        orAddDeclaration.Append("Unrepairable", !isRepairable);
        orAddDeclaration.Append("Proof_Explosion", proofExplosion);
        orAddDeclaration.Append("Unpickupable", isUnpickupable);
        orAddDeclaration.Append("Unsalvageable", !isSalvageable);
        orAddDeclaration.Append("Salvage_Duration_Multiplier", salvageDurationMultiplier);
        orAddDeclaration.Append("Unsaveable", !isSaveable);
        orAddDeclaration.Append("Armor_Tier", armorTier);
        orAddDeclaration.Append("Foliage_Cut_Radius", foliageCutRadius);
        orAddDeclaration.Append("Terrain_Test_Height", terrainTestHeight);
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

    /// <summary>
    /// Called on the dedicated server to optimize client prefab for server usage.
    /// </summary>
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
