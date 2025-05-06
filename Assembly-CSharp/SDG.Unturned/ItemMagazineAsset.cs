using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ItemMagazineAsset : ItemCaliberAsset
{
    protected GameObject _magazine;

    private byte _pellets;

    private byte _stuck;

    protected float _range;

    public float playerDamage;

    public float zombieDamage;

    public float animalDamage;

    public float barricadeDamage;

    public float structureDamage;

    public float vehicleDamage;

    public float resourceDamage;

    public float objectDamage;

    public float explosionLaunchSpeed;

    public Guid explosionEffectGuid;

    private ushort _explosion;

    public Guid tracerEffectGuid;

    private ushort _tracer;

    private Guid _impactEffectGuid;

    private ushort _impact;

    private float _speed;

    protected bool _isExplosive;

    public GameObject magazine => _magazine;

    public byte pellets => _pellets;

    public byte stuck => _stuck;

    public float range => _range;

    /// <summary>
    /// Multiplier for explosive projectile damage.
    /// </summary>
    public float projectileDamageMultiplier { get; protected set; }

    /// <summary>
    /// Multiplier for explosive projectile's blast radius.
    /// </summary>
    public float projectileBlastRadiusMultiplier { get; protected set; }

    /// <summary>
    /// Multiplier for explosive projectile's initial force.
    /// </summary>
    public float projectileLaunchForceMultiplier { get; protected set; }

    public ushort explosion => _explosion;

    public bool spawnExplosionOnDedicatedServer { get; protected set; }

    public ushort tracer
    {
        [Obsolete]
        get
        {
            return _tracer;
        }
    }

    public Guid ImpactEffectGuid => _impactEffectGuid;

    public ushort impact
    {
        [Obsolete]
        get
        {
            return _impact;
        }
    }

    public override bool showQuality => stuck > 0;

    public float speed => _speed;

    public bool isExplosive => _isExplosive;

    [Obsolete("Moved into ItemAsset and renamed to ShouldDeleteAtZeroAmount.")]
    public bool deleteEmpty => base.ShouldDeleteAtZeroAmount;

    /// <summary>
    /// Should amount be filled to capacity when detached?
    /// </summary>
    public bool shouldFillAfterDetach { get; protected set; }

    public bool IsExplosionEffectRefNull()
    {
        if (explosion == 0)
        {
            return explosionEffectGuid.IsEmpty();
        }
        return false;
    }

    public EffectAsset FindExplosionEffect()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion);
    }

    public EffectAsset FindTracerEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(tracerEffectGuid, _tracer);
    }

    public bool IsImpactEffectRefNull()
    {
        if (_impact == 0)
        {
            return _impactEffectGuid.IsEmpty();
        }
        return false;
    }

    public EffectAsset FindImpactEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_impactEffectGuid, _impact);
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (_pellets > 1)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_PelletCount", _pellets), 10000);
            }
            if (isExplosive)
            {
                int sortOrder = 30000;
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosiveBullet"), isBeneficial: true), sortOrder++);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), sortOrder++);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _magazine = loadRequiredAsset<GameObject>(p.bundle, "Magazine");
        _pellets = p.data.ParseUInt8("Pellets", 0);
        if (pellets < 1)
        {
            _pellets = 1;
        }
        _stuck = p.data.ParseUInt8("Stuck", 0);
        projectileDamageMultiplier = p.data.ParseFloat("Projectile_Damage_Multiplier", 1f);
        projectileBlastRadiusMultiplier = p.data.ParseFloat("Projectile_Blast_Radius_Multiplier", 1f);
        projectileLaunchForceMultiplier = p.data.ParseFloat("Projectile_Launch_Force_Multiplier", 1f);
        _range = p.data.ParseFloat("Range");
        playerDamage = p.data.ParseFloat("Player_Damage");
        zombieDamage = p.data.ParseFloat("Zombie_Damage");
        animalDamage = p.data.ParseFloat("Animal_Damage");
        barricadeDamage = p.data.ParseFloat("Barricade_Damage");
        structureDamage = p.data.ParseFloat("Structure_Damage");
        vehicleDamage = p.data.ParseFloat("Vehicle_Damage");
        resourceDamage = p.data.ParseFloat("Resource_Damage");
        explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        _explosion = p.data.ParseGuidOrLegacyId("Explosion", out explosionEffectGuid);
        if (p.data.ContainsKey("Object_Damage"))
        {
            objectDamage = p.data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _tracer = p.data.ParseGuidOrLegacyId("Tracer", out tracerEffectGuid);
        _impact = p.data.ParseGuidOrLegacyId("Impact", out _impactEffectGuid);
        _speed = p.data.ParseFloat("Speed");
        if (speed < 0.01f)
        {
            _speed = 1f;
        }
        _isExplosive = p.data.ContainsKey("Explosive");
        spawnExplosionOnDedicatedServer = p.data.ContainsKey("Spawn_Explosion_On_Dedicated_Server");
        shouldFillAfterDetach = p.data.ParseBool("Should_Fill_After_Detach");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Magazine");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Pellets", pellets);
        orAddDeclaration.Append("Stuck", stuck);
        orAddDeclaration.Append("Projectile_Damage_Multiplier", projectileDamageMultiplier);
        orAddDeclaration.Append("Projectile_Blast_Radius_Multiplier", projectileBlastRadiusMultiplier);
        orAddDeclaration.Append("Projectile_Launch_Force_Multiplier", projectileLaunchForceMultiplier);
        orAddDeclaration.Append("Range", range);
        orAddDeclaration.Append("Player_Damage", playerDamage);
        orAddDeclaration.Append("Zombie_Damage", zombieDamage);
        orAddDeclaration.Append("Animal_Damage", animalDamage);
        orAddDeclaration.Append("Barricade_Damage", barricadeDamage);
        orAddDeclaration.Append("Structure_Damage", structureDamage);
        orAddDeclaration.Append("Vehicle_Damage", vehicleDamage);
        orAddDeclaration.Append("Resource_Damage", resourceDamage);
        orAddDeclaration.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
        orAddDeclaration.Append("Explosion", explosion);
        orAddDeclaration.Append("Object_Damage", objectDamage);
        orAddDeclaration.Append("Tracer", tracer);
        orAddDeclaration.Append("Impact", impact);
        orAddDeclaration.Append("Speed", speed);
        orAddDeclaration.Append("Explosive", isExplosive);
        orAddDeclaration.Append("Delete_Empty", deleteEmpty);
        orAddDeclaration.Append("Should_Fill_After_Detach", shouldFillAfterDetach);
    }
}
