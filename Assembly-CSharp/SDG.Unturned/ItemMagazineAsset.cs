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

    public ItemMagazineAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _magazine = loadRequiredAsset<GameObject>(bundle, "Magazine");
        _pellets = data.readByte("Pellets", 0);
        if (pellets < 1)
        {
            _pellets = 1;
        }
        _stuck = data.readByte("Stuck", 0);
        projectileDamageMultiplier = data.readSingle("Projectile_Damage_Multiplier", 1f);
        projectileBlastRadiusMultiplier = data.readSingle("Projectile_Blast_Radius_Multiplier", 1f);
        projectileLaunchForceMultiplier = data.readSingle("Projectile_Launch_Force_Multiplier", 1f);
        _range = data.readSingle("Range");
        playerDamage = data.readSingle("Player_Damage");
        zombieDamage = data.readSingle("Zombie_Damage");
        animalDamage = data.readSingle("Animal_Damage");
        barricadeDamage = data.readSingle("Barricade_Damage");
        structureDamage = data.readSingle("Structure_Damage");
        vehicleDamage = data.readSingle("Vehicle_Damage");
        resourceDamage = data.readSingle("Resource_Damage");
        explosionLaunchSpeed = data.readSingle("Explosion_Launch_Speed", playerDamage * 0.1f);
        _explosion = data.ReadGuidOrLegacyId("Explosion", out explosionEffectGuid);
        if (data.has("Object_Damage"))
        {
            objectDamage = data.readSingle("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _tracer = data.ReadGuidOrLegacyId("Tracer", out tracerEffectGuid);
        _impact = data.ReadGuidOrLegacyId("Impact", out _impactEffectGuid);
        _speed = data.readSingle("Speed");
        if (speed < 0.01f)
        {
            _speed = 1f;
        }
        _isExplosive = data.has("Explosive");
        spawnExplosionOnDedicatedServer = data.has("Spawn_Explosion_On_Dedicated_Server");
        _deleteEmpty = data.has("Delete_Empty");
        shouldFillAfterDetach = data.readBoolean("Should_Fill_After_Detach");
    }
}
