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

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (_isFlash)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_Flash"), 2000);
            }
            if (_isSticky)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_Sticky"), 2000);
            }
            if (_explodeOnImpact)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_ExplodeOnImpact"), 2000);
            }
            if (_isExplosive || _isFlash)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Throwable_FuseLength", $"{_fuseLength:0.0} s"), 2000);
            }
            if (_isExplosive)
            {
                BuildExplosiveDescription(builder, itemInstance);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
        _throwable = bundle.load<GameObject>("Throwable");
        _explosion = data.ParseGuidOrLegacyId("Explosion", out explosionEffectGuid);
        _isExplosive = data.ContainsKey("Explosive");
        _isFlash = data.ContainsKey("Flash");
        _isSticky = data.ContainsKey("Sticky");
        _explodeOnImpact = data.ContainsKey("Explode_On_Impact");
        if (data.ContainsKey("Fuse_Length"))
        {
            _fuseLength = data.ParseFloat("Fuse_Length");
        }
        else if (isExplosive || isFlash)
        {
            _fuseLength = 2.5f;
        }
        else
        {
            _fuseLength = 180f;
        }
        explosionLaunchSpeed = data.ParseFloat("Explosion_Launch_Speed", playerDamageMultiplier.damage * 0.1f);
        strongThrowForce = data.ParseFloat("Strong_Throw_Force", 1100f);
        weakThrowForce = data.ParseFloat("Weak_Throw_Force", 600f);
        boostForceMultiplier = data.ParseFloat("Boost_Throw_Force_Multiplier", 1.4f);
    }
}
