namespace SDG.Unturned;

public class BarricadesConfigData
{
    /// <summary>
    /// How long (in seconds) since the barricade owner/group last played before the barricade won't be saved.
    /// If the server is offline for more than half the Decay_Time, all decay timers are reset.
    /// </summary>
    public uint Decay_Time;

    /// <summary>
    /// Scales the amount of damage taken by "Armor Tier: Low" barricades.
    /// For example, 0.5 halves the amount of damage dealt to barricades.
    /// </summary>
    public float Armor_Lowtier_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by "Armor Tier: High" barricades.
    /// For example, 0.5 halves the amount of damage dealt to barricades.
    /// </summary>
    public float Armor_Hightier_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by barricades from non-"Heavy Weapon" guns.
    /// For example, 2.0 doubles the amount of damage dealt to barricades by non-"Heavy Weapon" guns.
    /// </summary>
    public float Gun_Lowcal_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by barricades from "Heavy Weapon" guns.
    /// For example, 2.0 doubles the amount of damage dealt to barricades by "Heavy Weapon" guns.
    /// </summary>
    public float Gun_Highcal_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by barricades from melee weapons and fists.
    /// For example, 2.0 doubles the amount of damage dealt to barricades by melee.
    /// </summary>
    public float Melee_Damage_Multiplier;

    /// <summary>
    /// Scales the amount of HP restored by melee items like the Blowtorch.
    /// For example, 2.0 doubles the amount of health restored by melee items.
    /// </summary>
    public float Melee_Repair_Multiplier;

    /// <summary>
    /// Should players be allowed to build on their vehicles?
    /// </summary>
    public bool Allow_Item_Placement_On_Vehicle;

    /// <summary>
    /// Should players be allowed to build traps (e.g. barbed wire) on their vehicles?
    /// </summary>
    public bool Allow_Trap_Placement_On_Vehicle;

    /// <summary>
    /// Furthest away from colliders a player can build an item onto their vehicle.
    /// </summary>
    public float Max_Item_Distance_From_Hull;

    /// <summary>
    /// Furthest away from colliders a player can build a trap (e.g. barbed wire) onto their vehicle.
    /// </summary>
    public float Max_Trap_Distance_From_Hull;

    public float getArmorMultiplier(EArmorTier armorTier)
    {
        if (armorTier == EArmorTier.LOW || armorTier != EArmorTier.HIGH)
        {
            return Armor_Lowtier_Multiplier;
        }
        return Armor_Hightier_Multiplier;
    }

    public BarricadesConfigData(EGameMode mode)
    {
        Decay_Time = 604800u;
        Armor_Lowtier_Multiplier = 1f;
        Armor_Hightier_Multiplier = 0.5f;
        Gun_Lowcal_Damage_Multiplier = 1f;
        Gun_Highcal_Damage_Multiplier = 1f;
        Melee_Damage_Multiplier = 1f;
        Melee_Repair_Multiplier = 1f;
        Allow_Item_Placement_On_Vehicle = true;
        Allow_Trap_Placement_On_Vehicle = true;
        Max_Item_Distance_From_Hull = 64f;
        Max_Trap_Distance_From_Hull = 16f;
    }
}
