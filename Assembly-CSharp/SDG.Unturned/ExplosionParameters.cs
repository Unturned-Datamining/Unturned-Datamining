using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public struct ExplosionParameters
{
    public Vector3 point;

    public float damageRadius;

    public EDeathCause cause;

    public CSteamID killer;

    public EExplosionDamageType damageType;

    public float alertRadius;

    public bool playImpactEffect;

    public bool penetrateBuildables;

    public EDamageOrigin damageOrigin;

    public ERagdollEffect ragdollEffect;

    public float playerDamage;

    public float zombieDamage;

    public float animalDamage;

    public float barricadeDamage;

    public float structureDamage;

    public float vehicleDamage;

    public float resourceDamage;

    public float objectDamage;

    public float launchSpeed;

    public ExplosionParameters(Vector3 point, float damageRadius, EDeathCause cause, CSteamID killer)
    {
        this.point = point;
        this.damageRadius = damageRadius;
        this.cause = cause;
        this.killer = killer;
        damageType = EExplosionDamageType.CONVENTIONAL;
        alertRadius = 32f;
        playImpactEffect = true;
        penetrateBuildables = false;
        damageOrigin = EDamageOrigin.Unknown;
        ragdollEffect = ERagdollEffect.NONE;
        playerDamage = 0f;
        zombieDamage = 0f;
        animalDamage = 0f;
        barricadeDamage = 0f;
        structureDamage = 0f;
        vehicleDamage = 0f;
        resourceDamage = 0f;
        objectDamage = 0f;
        launchSpeed = 0f;
    }

    public ExplosionParameters(Vector3 point, float damageRadius, EDeathCause cause)
        : this(point, damageRadius, cause, CSteamID.Nil)
    {
    }
}
