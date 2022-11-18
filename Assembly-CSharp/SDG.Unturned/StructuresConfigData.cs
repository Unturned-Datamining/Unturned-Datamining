namespace SDG.Unturned;

public class StructuresConfigData
{
    public uint Decay_Time;

    public float Armor_Lowtier_Multiplier;

    public float Armor_Hightier_Multiplier;

    public float Gun_Lowcal_Damage_Multiplier;

    public float Gun_Highcal_Damage_Multiplier;

    public float Melee_Damage_Multiplier;

    public float Melee_Repair_Multiplier;

    public float getArmorMultiplier(EArmorTier armorTier)
    {
        if (armorTier == EArmorTier.LOW || armorTier != EArmorTier.HIGH)
        {
            return Armor_Lowtier_Multiplier;
        }
        return Armor_Hightier_Multiplier;
    }

    public StructuresConfigData(EGameMode mode)
    {
        Decay_Time = 604800u;
        Armor_Lowtier_Multiplier = 1f;
        Armor_Hightier_Multiplier = 0.5f;
        Gun_Lowcal_Damage_Multiplier = 1f;
        Gun_Highcal_Damage_Multiplier = 1f;
        Melee_Damage_Multiplier = 1f;
        Melee_Repair_Multiplier = 1f;
    }
}
