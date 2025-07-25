namespace SDG.Unturned;

public class AnimalsConfigData
{
    /// <summary>
    /// How long (in seconds) before a dead animal respawns.
    /// </summary>
    public float Respawn_Time;

    /// <summary>
    /// Scales the amount of damage dealt by animals.
    /// For example, 2.0 doubles the amount of damage from animal attacks.
    /// </summary>
    public float Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by animals.
    /// For example, 0.5 halves the amount of damage dealt to animals.
    /// </summary>
    public float Armor_Multiplier;

    /// <summary>
    /// Maximum number of animals on "Tiny" size levels.
    /// </summary>
    public uint Max_Instances_Tiny;

    /// <summary>
    /// Maximum number of animals on "Small" size levels.
    /// </summary>
    public uint Max_Instances_Small;

    /// <summary>
    /// Maximum number of animals on "Medium" size levels.
    /// </summary>
    public uint Max_Instances_Medium;

    /// <summary>
    /// Maximum number of animals on "Large" size levels.
    /// </summary>
    public uint Max_Instances_Large;

    /// <summary>
    /// Maximum number of animals on "Insane" size levels.
    /// </summary>
    public uint Max_Instances_Insane;

    /// <summary>
    /// If true, attacking an animal uses the weapon's PvP damage values rather than animal-specific damage.
    /// </summary>
    public bool Weapons_Use_Player_Damage;

    public AnimalsConfigData(EGameMode mode)
    {
        Respawn_Time = 180f;
        switch (mode)
        {
        case EGameMode.EASY:
            Damage_Multiplier = 0.75f;
            Armor_Multiplier = 1.25f;
            break;
        case EGameMode.HARD:
            Damage_Multiplier = 1.5f;
            Armor_Multiplier = 0.75f;
            break;
        default:
            Damage_Multiplier = 1f;
            Armor_Multiplier = 1f;
            break;
        }
        Max_Instances_Tiny = 4u;
        Max_Instances_Small = 8u;
        Max_Instances_Medium = 16u;
        Max_Instances_Large = 32u;
        Max_Instances_Insane = 64u;
        Weapons_Use_Player_Damage = mode == EGameMode.HARD;
    }
}
