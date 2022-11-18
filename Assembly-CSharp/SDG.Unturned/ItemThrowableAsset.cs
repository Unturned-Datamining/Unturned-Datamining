using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemThrowableAsset : ItemWeaponAsset
{
    protected AudioClip _use;

    protected GameObject _throwable;

    public Guid explosionEffectGuid;

    private ushort _explosion;

    private bool _isExplosive;

    private bool _isFlash;

    private bool _isSticky;

    private bool _explodeOnImpact;

    private float _fuseLength;

    public float strongThrowForce;

    public float weakThrowForce;

    public float boostForceMultiplier;

    public float explosionLaunchSpeed;

    public AudioClip use => _use;

    public GameObject throwable => _throwable;

    public ushort explosion => _explosion;

    public bool isExplosive => _isExplosive;

    public bool isFlash => _isFlash;

    public bool isSticky => _isSticky;

    public bool explodeOnImpact => _explodeOnImpact;

    public float fuseLength => _fuseLength;

    public override bool shouldFriendlySentryTargetUser
    {
        get
        {
            if (!isExplosive && !isFlash)
            {
                return explodeOnImpact;
            }
            return true;
        }
    }

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        if (safezone.noWeapons)
        {
            return false;
        }
        return true;
    }

    public ItemThrowableAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
        _throwable = bundle.load<GameObject>("Throwable");
        _explosion = data.ReadGuidOrLegacyId("Explosion", out explosionEffectGuid);
        _isExplosive = data.has("Explosive");
        _isFlash = data.has("Flash");
        _isSticky = data.has("Sticky");
        _explodeOnImpact = data.has("Explode_On_Impact");
        if (data.has("Fuse_Length"))
        {
            _fuseLength = data.readSingle("Fuse_Length");
        }
        else if (isExplosive || isFlash)
        {
            _fuseLength = 2.5f;
        }
        else
        {
            _fuseLength = 180f;
        }
        explosionLaunchSpeed = data.readSingle("Explosion_Launch_Speed", playerDamageMultiplier.damage * 0.1f);
        strongThrowForce = data.readSingle("Strong_Throw_Force", 1100f);
        weakThrowForce = data.readSingle("Weak_Throw_Force", 600f);
        boostForceMultiplier = data.readSingle("Boost_Throw_Force_Multiplier", 1.4f);
    }
}
