using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Payload for the DamageTool.damageZombie function.
/// </summary>
public struct DamageZombieParameters
{
    public Zombie zombie;

    public Vector3 direction;

    public float damage;

    public bool respectArmor;

    /// <summary>
    /// Should game mode config damage multiplier be factored in?
    /// </summary>
    public bool applyGlobalArmorMultiplier;

    public bool allowBackstab;

    public ELimb limb;

    public float times;

    public EZombieStunOverride zombieStunOverride;

    public ERagdollEffect ragdollEffect;

    public object instigator;

    /// <summary>
    /// Equivalent to the "armor" parameter of the legacy damage function.
    /// </summary>
    public bool legacyArmor
    {
        set
        {
            respectArmor = value;
            allowBackstab = value;
        }
    }

    /// <summary>
    /// If not null and damage is applied, <see cref="M:SDG.Unturned.Zombie.alert(SDG.Unturned.Player)" /> is called with this position (startle: true).
    /// </summary>
    public Vector3? AlertPosition { get; set; }

    public DamageZombieParameters(Zombie zombie, Vector3 direction, float damage)
    {
        this.zombie = zombie;
        this.direction = direction;
        this.damage = damage;
        respectArmor = false;
        allowBackstab = false;
        applyGlobalArmorMultiplier = true;
        limb = ELimb.SPINE;
        times = 1f;
        zombieStunOverride = EZombieStunOverride.None;
        ragdollEffect = ERagdollEffect.NONE;
        AlertPosition = null;
        instigator = null;
    }

    public static DamageZombieParameters makeInstakill(Zombie zombie)
    {
        DamageZombieParameters result = new DamageZombieParameters(zombie, Vector3.up, 65000f);
        result.applyGlobalArmorMultiplier = false;
        return result;
    }

    public static DamageZombieParameters make(Zombie zombie, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
    {
        float num = multiplier.multiply(limb);
        DamageZombieParameters result = new DamageZombieParameters(zombie, direction, num);
        result.limb = limb;
        return result;
    }
}
