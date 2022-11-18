namespace SDG.Unturned;

public class AnimalDamageMultiplier : IDamageMultiplier
{
    public static readonly float MULTIPLIER_EASY = 1.25f;

    public static readonly float MULTIPLIER_HARD = 0.75f;

    public float damage;

    public float leg;

    public float spine;

    public float skull;

    public float multiply(ELimb limb)
    {
        return limb switch
        {
            ELimb.LEFT_BACK => damage * leg, 
            ELimb.RIGHT_BACK => damage * leg, 
            ELimb.LEFT_FRONT => damage * leg, 
            ELimb.RIGHT_FRONT => damage * leg, 
            ELimb.SPINE => damage * spine, 
            ELimb.SKULL => damage * skull, 
            _ => damage, 
        };
    }

    public AnimalDamageMultiplier(float newDamage, float newLeg, float newSpine, float newSkull)
    {
        damage = newDamage;
        leg = newLeg;
        spine = newSpine;
        skull = newSkull;
    }
}
