using UnityEngine;

namespace SDG.Unturned;

public struct DamageAnimalParameters
{
    public Animal animal;

    public Vector3 direction;

    public float damage;

    public bool applyGlobalArmorMultiplier;

    public ELimb limb;

    public float times;

    public ERagdollEffect ragdollEffect;

    public object instigator;

    public DamageAnimalParameters(Animal animal, Vector3 direction, float damage)
    {
        this.animal = animal;
        this.direction = direction;
        this.damage = damage;
        applyGlobalArmorMultiplier = true;
        limb = ELimb.SPINE;
        times = 1f;
        ragdollEffect = ERagdollEffect.NONE;
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
