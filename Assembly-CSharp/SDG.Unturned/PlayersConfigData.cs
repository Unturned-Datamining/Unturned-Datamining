namespace SDG.Unturned;

public class PlayersConfigData
{
    /// <summary>
    /// Amount of health players spawn with. [0 to 100]
    /// </summary>
    public uint Health_Default;

    /// <summary>
    /// Player must have more than this amount of food to begin regenerating health.
    /// </summary>
    public uint Health_Regen_Min_Food;

    /// <summary>
    /// Player must have more than this amount of water to begin regenerating health.
    /// </summary>
    public uint Health_Regen_Min_Water;

    /// <summary>
    /// How quickly players health regenerates with sufficient food and water.
    /// Lower values regenerate health faster, higher values regenerate health slower.
    /// </summary>
    public uint Health_Regen_Ticks;

    /// <summary>
    /// Amount of food players spawn with. [0 to 100]
    /// </summary>
    public uint Food_Default;

    /// <summary>
    /// How quickly players food meter depletes.
    /// Lower values burn food faster, higher values burn food slower.
    /// </summary>
    public uint Food_Use_Ticks;

    /// <summary>
    /// How quickly players starve to death.
    /// Lower values kill the player faster, higher values kill the player slower.
    /// </summary>
    public uint Food_Damage_Ticks;

    /// <summary>
    /// Amount of water players spawn with. [0 to 100]
    /// </summary>
    public uint Water_Default;

    /// <summary>
    /// How quickly players water meter depletes.
    /// Lower values lose water faster, higher values lose water slower.
    /// </summary>
    public uint Water_Use_Ticks;

    /// <summary>
    /// How quickly players dehydrate to death.
    /// Lower values kill the player faster, higher values kill the player slower.
    /// </summary>
    public uint Water_Damage_Ticks;

    /// <summary>
    /// Amount of immunity players spawn with. [0 to 100]
    /// </summary>
    public uint Virus_Default;

    /// <summary>
    /// When immunity is below this amount it will gradually begin depleting.
    /// </summary>
    public uint Virus_Infect;

    /// <summary>
    /// How quickly players immunity depletes when below Virus_Infect.
    /// Lower values deplete faster, higher values deplete slower.
    /// </summary>
    public uint Virus_Use_Ticks;

    /// <summary>
    /// How quickly players die at zero immunity.
    /// Lower values kill the player faster, higher values kill the player slower.
    /// </summary>
    public uint Virus_Damage_Ticks;

    /// <summary>
    /// How quickly broken legs heal automatically.
    /// Depends on Can_Fix_Legs.
    /// Lower values heal faster, higher values heal slower.
    /// </summary>
    public uint Leg_Regen_Ticks;

    /// <summary>
    /// How frequently players lose health while bleeding.
    /// Lower values kill the player faster, higher values kill the player slower.
    /// </summary>
    public uint Bleed_Damage_Ticks;

    /// <summary>
    /// How quickly bleeding heals automatically.
    /// Depends on Can_Stop_Bleeding.
    /// Lower values heal faster, higher values heal slower.
    /// </summary>
    public uint Bleed_Regen_Ticks;

    /// <summary>
    /// Scales the amount of damage taken by players.
    /// For example, 0.5 halves the amount of damage dealt to players.
    /// </summary>
    public float Armor_Multiplier;

    /// <summary>
    /// Scales the amount of XP gained from all activities.
    /// </summary>
    public float Experience_Multiplier;

    /// <summary>
    /// Scales the radius within zombies and animals will detect the player.
    /// </summary>
    public float Detect_Radius_Multiplier;

    /// <summary>
    /// How close an attack is to a player to be considered aggressive.
    /// For example, when a bullet passes within this distance of a player the shooter is
    /// considered the aggressor.
    /// </summary>
    public float Ray_Aggressor_Distance;

    /// <summary>
    /// Percentage [0 to 1] of skill levels to retain when killed by another player.
    /// </summary>
    public float Lose_Skills_PvP;

    /// <summary>
    /// Percentage [0 to 1] of skill levels to retain when killed by the environment (e.g., zombies).
    /// </summary>
    public float Lose_Skills_PvE;

    /// <summary>
    /// Number of skill levels to remove when killed by another player.
    /// </summary>
    public uint Lose_Skill_Levels_PvP;

    /// <summary>
    /// Number of skill levels to remove when killed by the environment (e.g., zombies).
    /// </summary>
    public uint Lose_Skill_Levels_PvE;

    /// <summary>
    /// Percentage [0 to 1] of XP to retain when killed by another player.
    /// </summary>
    public float Lose_Experience_PvP;

    /// <summary>
    /// Percentage [0 to 1] of XP to retain when killed by the environment (e.g., zombies).
    /// </summary>
    public float Lose_Experience_PvE;

    /// <summary>
    /// Percentage [0 to 1] chance to lose each inventory item when killed by another player.
    /// Depends on Lose_Clothes_PvP because losing storage will drop contained items.
    /// </summary>
    public float Lose_Items_PvP;

    /// <summary>
    /// Percentage [0 to 1] chance to lose each inventory item when killed by the environment (e.g., zombies).
    /// Depends on Lose_Clothes_PvE because losing storage will drop contained items.
    /// </summary>
    public float Lose_Items_PvE;

    /// <summary>
    /// If true, drop all clothing items when killed by another player.
    /// </summary>
    public bool Lose_Clothes_PvP;

    /// <summary>
    /// If true, drop all clothing items when killed by the environment (e.g., zombies).
    /// </summary>
    public bool Lose_Clothes_PvE;

    /// <summary>
    /// If true, drop primary and secondary weapon when killed by another player.
    /// </summary>
    public bool Lose_Weapons_PvP;

    /// <summary>
    /// If true, drop primary and secondary weapon when killed by the environment (e.g., zombies).
    /// </summary>
    public bool Lose_Weapons_PvE;

    /// <summary>
    /// If false, players have no health loss from falling long distances.
    /// </summary>
    public bool Can_Hurt_Legs;

    /// <summary>
    /// If false, players cannot break their leg when falling long distances.
    /// </summary>
    public bool Can_Break_Legs;

    /// <summary>
    /// If false, broken legs cannot automatically heal themselves after Leg_Regen_Ticks.
    /// </summary>
    public bool Can_Fix_Legs;

    /// <summary>
    /// If false, damage cannot cause players to bleed.
    /// </summary>
    public bool Can_Start_Bleeding;

    /// <summary>
    /// If false, bleeding cannot automatically heal itself after Bleed_Regen_Ticks.
    /// </summary>
    public bool Can_Stop_Bleeding;

    /// <summary>
    /// Should all skills default to max level?
    /// </summary>
    public bool Spawn_With_Max_Skills;

    /// <summary>
    /// Should cardio, diving, exercise, and parkour default to max level?
    /// </summary>
    public bool Spawn_With_Stamina_Skills;

    /// <summary>
    /// If true, skills related to player's skillset/speciality are half cost.
    /// </summary>
    public bool Skillset_Reduces_Skill_Cost = true;

    /// <summary>
    /// If true, skills related to player's skillset/speciality cannot lose levels on death.
    /// </summary>
    public bool Skillset_Prevents_Skill_Loss = true;

    /// <summary>
    /// Should guns with Instakill Headshots (snipers) bypass armor?
    /// </summary>
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
