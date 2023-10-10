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

    private bool _deleteEmpty;

    public GameObject magazine => _magazine;

    public byte pellets => _pellets;

    public byte stuck => _stuck;

    public float range => _range;

    public float projectileDamageMultiplier { get; protected set; }

    public float projectileBlastRadiusMultiplier { get; protected set; }

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

    public bool deleteEmpty => _deleteEmpty;

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
                builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosiveBullet"), isBeneficial: true), 20001);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), 20002);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), 20003);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), 20003);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _magazine = loadRequiredAsset<GameObject>(bundle, "Magazine");
        _pellets = data.ParseUInt8("Pellets", 0);
        if (pellets < 1)
        {
            _pellets = 1;
        }
        _stuck = data.ParseUInt8("Stuck", 0);
        projectileDamageMultiplier = data.ParseFloat("Projectile_Damage_Multiplier", 1f);
        projectileBlastRadiusMultiplier = data.ParseFloat("Projectile_Blast_Radius_Multiplier", 1f);
        projectileLaunchForceMultiplier = data.ParseFloat("Projectile_Launch_Force_Multiplier", 1f);
        _range = data.ParseFloat("Range");
        playerDamage = data.ParseFloat("Player_Damage");
        zombieDamage = data.ParseFloat("Zombie_Damage");
        animalDamage = data.ParseFloat("Animal_Damage");
        barricadeDamage = data.ParseFloat("Barricade_Damage");
        structureDamage = data.ParseFloat("Structure_Damage");
        vehicleDamage = data.ParseFloat("Vehicle_Damage");
        resourceDamage = data.ParseFloat("Resource_Damage");
        explosionLaunchSpeed = data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        _explosion = data.ParseGuidOrLegacyId("Explosion", out explosionEffectGuid);
        if (data.ContainsKey("Object_Damage"))
        {
            objectDamage = data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _tracer = data.ParseGuidOrLegacyId("Tracer", out tracerEffectGuid);
        _impact = data.ParseGuidOrLegacyId("Impact", out _impactEffectGuid);
        _speed = data.ParseFloat("Speed");
        if (speed < 0.01f)
        {
            _speed = 1f;
        }
        _isExplosive = data.ContainsKey("Explosive");
        spawnExplosionOnDedicatedServer = data.ContainsKey("Spawn_Explosion_On_Dedicated_Server");
        _deleteEmpty = data.ContainsKey("Delete_Empty");
        shouldFillAfterDetach = data.ParseBool("Should_Fill_After_Detach");
    }
}
