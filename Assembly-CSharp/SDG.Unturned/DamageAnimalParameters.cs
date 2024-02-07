using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Payload for the DamageTool.damageAnimal function.
/// </summary>
public struct DamageAnimalParameters
{
    public Animal animal;

    public Vector3 direction;

    public float damage;

    /// <summary>
    /// Should game mode config damage multiplier be factored in?
    /// </summary>
    public bool applyGlobalArmorMultiplier;

    public ELimb limb;

    public float times;

    public ERagdollEffect ragdollEffect;

    public object instigator;

    /// <summary>
    /// If not null and damage is applied, <see cref="M:SDG.Unturned.Animal.alertDamagedFromPoint(UnityEngine.Vector3)" /> is called with this position.
    /// </summary>
    public Vector3? AlertPosition { get; set; }

    public DamageAnimalParameters(Animal animal, Vector3 direction, float damage)
    {
        this.animal = animal;
        this.direction = direction;
        this.damage = damage;
        applyGlobalArmorMultiplier = true;
        limb = ELimb.SPINE;
        times = 1f;
        ragdollEffect = ERagdollEffect.NONE;
        AlertPosition = null;
        instigator = null;
    }

    public static DamageAnimalParameters makeInstakill(Animal animal)
    {
        DamageAnimalParameters result = new DamageAnimalParameters(animal, Vector3.up, 65000f);
        result.applyGlobalArmorMultiplier = false;
        return result;
    }

    public static DamageAnimalParameters make(Animal animal, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
    {
        float num = multiplier.multiply(limb);
        DamageAnimalParameters result = new DamageAnimalParameters(animal, direction, num);
        result.limb = limb;
        return result;
    }
}
