namespace SDG.Unturned;

public class PlayerDamageMultiplier : IDamageMultiplier
{
    public float damage;

    public float leg;

    public float arm;

    public float spine;

    public float skull;

    public float multiply(ELimb limb)
    {
        switch (limb)
        {
        case ELimb.LEFT_FOOT:
            return damage * leg;
        case ELimb.LEFT_LEG:
            return damage * leg;
        case ELimb.RIGHT_FOOT:
            return damage * leg;
        case ELimb.RIGHT_LEG:
            return damage * leg;
        case ELimb.LEFT_HAND:
            return damage * arm;
        case ELimb.LEFT_ARM:
            return damage * arm;
        case ELimb.RIGHT_HAND:
            return damage * arm;
        case ELimb.RIGHT_ARM:
            return damage * arm;
        case ELimb.SPINE:
            return damage * spine;
        case ELimb.SKULL:
            return damage * skull;
        case ELimb.LEFT_BACK:
        case ELimb.RIGHT_BACK:
        case ELimb.LEFT_FRONT:
        case ELimb.RIGHT_FRONT:
            return damage * leg;
        default:
            return damage;
        }
    }

    public PlayerDamageMultiplier(float newDamage, float newLeg, float newArm, float newSpine, float newSkull)
    {
        damage = newDamage;
        leg = newLeg;
        arm = newArm;
        spine = newSpine;
        skull = newSkull;
    }
}
