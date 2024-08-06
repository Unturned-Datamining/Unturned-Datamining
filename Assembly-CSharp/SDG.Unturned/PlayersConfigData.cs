namespace SDG.Unturned;

public class PlayersConfigData
{
    public uint Health_Default;

    public uint Health_Regen_Min_Food;

    public uint Health_Regen_Min_Water;

    public uint Health_Regen_Ticks;

    public uint Food_Default;

    public uint Food_Use_Ticks;

    public uint Food_Damage_Ticks;

    public uint Water_Default;

    public uint Water_Use_Ticks;

    public uint Water_Damage_Ticks;

    public uint Virus_Default;

    public uint Virus_Infect;

    public uint Virus_Use_Ticks;

    public uint Virus_Damage_Ticks;

    public uint Leg_Regen_Ticks;

    public uint Bleed_Damage_Ticks;

    public uint Bleed_Regen_Ticks;

    public float Armor_Multiplier;

    public float Experience_Multiplier;

    public float Detect_Radius_Multiplier;

    public float Ray_Aggressor_Distance;

    /// <summary>
    /// [0, 1] percentage of skill levels to retain after death.
    /// </summary>
    public float Lose_Skills_PvP;

    /// <summary>
    /// [0, 1] percentage of skill levels to retain after death.
    /// </summary>
    public float Lose_Skills_PvE;

    /// <summary>
    /// Number of skill levels to remove after death.
    /// </summary>
    public uint Lose_Skill_Levels_PvP;

    /// <summary>
    /// Number of skill levels to remove after death.
    /// </summary>
    public uint Lose_Skill_Levels_PvE;

    /// <summary>
    /// [0, 1] percentage of experience points to retain after death.
    /// </summary>
    public float Lose_Experience_PvP;

    /// <summary>
    /// [0, 1] percentage of experience points to retain after death.
    /// </summary>
    public float Lose_Experience_PvE;

    public float Lose_Items_PvP;

    public float Lose_Items_PvE;

    public bool Lose_Clothes_PvP;

    public bool Lose_Clothes_PvE;

    public bool Lose_Weapons_PvP;

    public bool Lose_Weapons_PvE;

    public bool Can_Hurt_Legs;

    public bool Can_Break_Legs;

    public bool Can_Fix_Legs;

    public bool Can_Start_Bleeding;

    public bool Can_Stop_Bleeding;

    public bool Spawn_With_Max_Skills;

    public bool Spawn_With_Stamina_Skills;

    public bool Allow_Instakill_Headshots;

    /// <summary>
    /// Should each character slot have separate savedata?
    /// </summary>
    public bool Allow_Per_Character_Saves;

    /// <summary>
    /// If true, players will be kicked if their skin color is too similar to one of the level's terrain colors.
    /// </summary>
    public bool Enable_Terrain_Color_Kick = true;

    public PlayersConfigData(EGameMode mode)
    {
        Health_Default = 100u;
        Health_Regen_Min_Food = 90u;
        Health_Regen_Min_Water = 90u;
        Health_Regen_Ticks = 60u;
        Food_Damage_Ticks = 15u;
        Water_Damage_Ticks = 20u;
        Virus_Default = 100u;
        Virus_Infect = 50u;
        Virus_Use_Ticks = 125u;
        Virus_Damage_Ticks = 25u;
        Leg_Regen_Ticks = 750u;
        Bleed_Damage_Ticks = 10u;
        Bleed_Regen_Ticks = 750u;
        if (mode == EGameMode.HARD)
        {
            Food_Default = 85u;
            Water_Default = 85u;
        }
        else
        {
            Food_Default = 100u;
            Water_Default = 100u;
        }
        switch (mode)
        {
        case EGameMode.EASY:
            Food_Use_Ticks = 350u;
            Water_Use_Ticks = 320u;
            break;
        case EGameMode.HARD:
            Food_Use_Ticks = 250u;
            Water_Use_Ticks = 220u;
            break;
        default:
            Food_Use_Ticks = 300u;
            Water_Use_Ticks = 270u;
            break;
        }
        switch (mode)
        {
        case EGameMode.EASY:
            Experience_Multiplier = 1.5f;
            break;
        case EGameMode.NORMAL:
            Experience_Multiplier = 1f;
            break;
        case EGameMode.HARD:
            Experience_Multiplier = 1.5f;
            break;
        default:
            Experience_Multiplier = 10f;
            break;
        }
        switch (mode)
        {
        case EGameMode.EASY:
            Detect_Radius_Multiplier = 0.5f;
            break;
        case EGameMode.HARD:
            Detect_Radius_Multiplier = 1.25f;
            break;
        default:
            Detect_Radius_Multiplier = 1f;
            break;
        }
        Ray_Aggressor_Distance = 8f;
        Armor_Multiplier = 1f;
        Lose_Skills_PvP = 1f;
        Lose_Skills_PvE = 1f;
        Lose_Skill_Levels_PvP = 1u;
        Lose_Skill_Levels_PvE = 1u;
        Lose_Experience_PvP = 0.5f;
        Lose_Experience_PvE = 0.5f;
        Lose_Items_PvP = 1f;
        Lose_Items_PvE = 1f;
        Lose_Clothes_PvP = true;
        Lose_Clothes_PvE = true;
        Lose_Weapons_PvP = true;
        Lose_Weapons_PvE = true;
        Can_Hurt_Legs = true;
        if (mode == EGameMode.EASY)
        {
            Can_Break_Legs = false;
            Can_Start_Bleeding = false;
            Lose_Skill_Levels_PvP = 0u;
            Lose_Skill_Levels_PvE = 0u;
        }
        else
        {
            Can_Break_Legs = true;
            Can_Start_Bleeding = true;
        }
        if (mode == EGameMode.HARD)
        {
            Can_Fix_Legs = false;
            Can_Stop_Bleeding = false;
            Lose_Skill_Levels_PvP = 2u;
            Lose_Skill_Levels_PvE = 2u;
        }
        else
        {
            Can_Fix_Legs = true;
            Can_Stop_Bleeding = true;
        }
        Spawn_With_Max_Skills = false;
        Spawn_With_Stamina_Skills = false;
        Allow_Instakill_Headshots = mode == EGameMode.HARD;
        Allow_Per_Character_Saves = false;
    }

    public void InitSingleplayerDefaults()
    {
        Allow_Per_Character_Saves = true;
    }
}
