namespace SDG.Unturned;

public class StructuresConfigData
{
    /// <summary>
    /// How long (in seconds) since the structure owner/group last played before the structure won't be saved.
    /// If the server is offline for more than half the Decay_Time, all decay timers are reset.
    /// </summary>
    public uint Decay_Time;

    /// <summary>
    /// Scales the amount of damage taken by "Armor Tier: Low" structures.
    /// For example, 0.5 halves the amount of damage dealt to structures.
    /// </summary>
    public float Armor_Lowtier_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by "Armor Tier: High" structures.
    /// For example, 0.5 halves the amount of damage dealt to structures.
    /// </summary>
    public float Armor_Hightier_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by structures from non-"Heavy Weapon" guns.
    /// For example, 2.0 doubles the amount of damage dealt to structures by non-"Heavy Weapon" guns.
    /// </summary>
    public float Gun_Lowcal_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by structures from "Heavy Weapon" guns.
    /// For example, 2.0 doubles the amount of damage dealt to structures by "Heavy Weapon" guns.
    /// </summary>
    public float Gun_Highcal_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by structures from melee weapons and fists.
    /// For example, 2.0 doubles the amount of damage dealt to structures by melee.
    /// </summary>
    public float Melee_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of HP restored by melee items like the Blowtorch.
    /// For example, 2.0 doubles the amount of health restored by melee items.
    /// </summary>
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
