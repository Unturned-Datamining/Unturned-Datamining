namespace SDG.Unturned;

public class ZombieDamageMultiplier : IDamageMultiplier
{
    public float damage;

    public float leg;

    public float arm;

    public float spine;

    public float skull;

    public float multiply(ELimb limb)
    {
        return limb switch
        {
            ELimb.LEFT_FOOT => damage * leg, 
            ELimb.LEFT_LEG => damage * leg, 
            ELimb.RIGHT_FOOT => damage * leg, 
            ELimb.RIGHT_LEG => damage * leg, 
            ELimb.LEFT_HAND => damage * arm, 
            ELimb.LEFT_ARM => damage * arm, 
            ELimb.RIGHT_HAND => damage * arm, 
            ELimb.RIGHT_ARM => damage * arm, 
            ELimb.SPINE => damage * spine, 
            ELimb.SKULL => damage * skull, 
            _ => damage, 
        };
    }

    public ZombieDamageMultiplier(float newDamage, float newLeg, float newArm, float newSpine, float newSkull)
    {
        damage = newDamage;
        leg = newLeg;
        arm = newArm;
        spine = newSpine;
        skull = newSkull;
    }
}
