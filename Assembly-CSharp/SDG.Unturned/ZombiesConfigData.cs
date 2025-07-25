namespace SDG.Unturned;

public class ZombiesConfigData
{
    /// <summary>
    /// Percentage [0 to 1] of zombie spawns to use.
    /// For example, if set to 0.2 and an area has 100 zombie spawns, max 20 zombies will spawn at a time.
    /// </summary>
    public float Spawn_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie dropping an item except when dropping more than one item.
    /// </summary>
    public float Loot_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a crawler.
    /// </summary>
    public float Crawler_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a sprinter.
    /// </summary>
    public float Sprinter_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a flanker.
    /// </summary>
    public float Flanker_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a burner.
    /// </summary>
    public float Burner_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as an acid spitter.
    /// </summary>
    public float Acid_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as an electric boss.
    /// </summary>
    public float Boss_Electric_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a ground-pounding boss.
    /// </summary>
    public float Boss_Wind_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a fire-breathing boss.
    /// </summary>
    public float Boss_Fire_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a ghost.
    /// </summary>
    public float Spirit_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a Dying Light Volatile (crossover).
    /// </summary>
    public float DL_Red_Volatile_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as a Dying Light Volatile (crossover).
    /// </summary>
    public float DL_Blue_Volatile_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as the Elver final boss.
    /// </summary>
    public float Boss_Elver_Stomper_Chance;

    /// <summary>
    /// Percentage [0 to 1] chance of zombie spawning as the Kuwait final boss.
    /// </summary>
    public float Boss_Kuwait_Chance;

    /// <summary>
    /// How long (in seconds) before a dead zombie respawns by default.
    /// </summary>
    public float Respawn_Day_Time;

    /// <summary>
    /// How long (in seconds) before a dead zombie respawns during a full moon.
    /// </summary>
    public float Respawn_Night_Time;

    /// <summary>
    /// How long (in seconds) before a dead zombie respawns during a horde beacon.
    /// </summary>
    public float Respawn_Beacon_Time;

    /// <summary>
    /// Minimum seconds between boss zombie spawns for players doing quests.
    /// Players were abusing the spawns to farm boss tier loot.
    /// </summary>
    public float Quest_Boss_Respawn_Interval;

    /// <summary>
    /// Scales the amount of damage dealt by zombies.
    /// For example, 2.0 doubles the amount of damage from zombie attacks.
    /// </summary>
    public float Damage_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by zombies.
    /// For example, 0.5 halves the amount of damage dealt to zombies.
    /// </summary>
    public float Armor_Multiplier;

    /// <summary>
    /// Scales the amount of damage taken by zombies when attacked from behind.
    /// Only certain weapons quality for this modifier.
    /// </summary>
    public float Backstab_Multiplier;

    /// <summary>
    /// Weapon damage multiplier against body, arms, legs. Useful for headshot-only mode.
    /// </summary>
    public float NonHeadshot_Armor_Multiplier;

    /// <summary>
    /// Scales amount of XP gained for killing a zombie during a horde beacon.
    /// </summary>
    public float Beacon_Experience_Multiplier;

    /// <summary>
    /// Scales amount of XP gained for killing a zombie during the full moon.
    /// </summary>
    public float Full_Moon_Experience_Multiplier;

    /// <summary>
    /// Minimum number of loot drops from non-mega non-boss zombies.
    /// Loot_Chance applies if the rolled number of drops between [min, max] is one.
    /// </summary>
    public uint Min_Drops;

    /// <summary>
    /// Maximum number of loot drops from non-mega non-boss zombies.
    /// </summary>
    public uint Max_Drops;

    /// <summary>
    /// Minimum number of loot drops from non-boss mega zombies.
    /// Loot_Chance applies if the rolled number of drops between [min, max] is one.
    /// </summary>
    public uint Min_Mega_Drops;

    /// <summary>
    /// Maximum number of loot drops from non-boss mega zombies.
    /// </summary>
    public uint Max_Mega_Drops;

    /// <summary>
    /// Minimum number of loot drops from boss zombies.
    /// Loot_Chance applies if the rolled number of drops between [min, max] is one.
    /// </summary>
    public uint Min_Boss_Drops;

    /// <summary>
    /// Maximum number of loot drops from boss zombies.
    /// </summary>
    public uint Max_Boss_Drops;

    /// <summary>
    /// If true, all zombies are a bit slower, making it easier to escape them.
    /// </summary>
    public bool Slow_Movement;

    /// <summary>
    /// If false, nothing can stun zombies, making combat harder.
    /// </summary>
    public bool Can_Stun;

    /// <summary>
    /// If true, only certain weapons and attacks can stun zombie (e.g., backstabs).
    /// Not applicable if Can_Stun is false.
    /// </summary>
    public bool Only_Critical_Stuns;

    /// <summary>
    /// If true, attacking a zombie uses the weapon's PvP damage values rather than zombie-specific damage.
    /// </summary>
    public bool Weapons_Use_Player_Damage;

    /// <summary>
    /// If true, zombies will attack barricades obstructing their movement.
    /// </summary>
    public bool Can_Target_Barricades;

    /// <summary>
    /// If true, zombies will attack structures obstructing their movement.
    /// </summary>
    public bool Can_Target_Structures;

    /// <summary>
    /// If true, zombies will attack vehicles obstructing their movement.
    /// </summary>
    public bool Can_Target_Vehicles;

    /// <summary>
    /// If true, zombies will attack level objects (e.g., fences) obstructing their movement.
    /// </summary>
    public bool Can_Target_Objects;

    /// <summary>
    /// If greater than zero, maximum number of items a horde beacon can drop.
    /// Useful to clamp the number of drops when a large number of players participate.
    /// </summary>
    public uint Beacon_Max_Rewards;

    /// <summary>
    /// If greater than zero, maximum player count for horde beacon loot scaling.
    /// Useful to clamp the number of drops when a large number of players participate.
    /// </summary>
    public uint Beacon_Max_Participants;

    /// <summary>
    /// Scales total number of horde beacon loot drops, applied before Beacon_Max_Rewards.
    /// </summary>
    public float Beacon_Rewards_Multiplier;

    public ZombiesConfigData(EGameMode mode)
    {
        Respawn_Day_Time = 360f;
        Respawn_Night_Time = 30f;
        Respawn_Beacon_Time = 0f;
        Quest_Boss_Respawn_Interval = 600f;
        switch (mode)
        {
        case EGameMode.EASY:
            Spawn_Chance = 0.2f;
            Loot_Chance = 0.55f;
            Crawler_Chance = 0f;
            Sprinter_Chance = 0f;
            Flanker_Chance = 0f;
            Burner_Chance = 0f;
            Acid_Chance = 0f;
            break;
        case EGameMode.NORMAL:
            Spawn_Chance = 0.25f;
            Loot_Chance = 0.5f;
            Crawler_Chance = 0.15f;
            Sprinter_Chance = 0.15f;
            Flanker_Chance = 0.025f;
            Burner_Chance = 0.025f;
            Acid_Chance = 0.025f;
            break;
        case EGameMode.HARD:
            Spawn_Chance = 0.3f;
            Loot_Chance = 0.3f;
            Crawler_Chance = 0.125f;
            Sprinter_Chance = 0.175f;
            Flanker_Chance = 0.05f;
            Burner_Chance = 0.05f;
            Acid_Chance = 0.05f;
            break;
        default:
            Spawn_Chance = 1f;
            Loot_Chance = 0f;
            Crawler_Chance = 0f;
            Sprinter_Chance = 0f;
            Flanker_Chance = 0f;
            Burner_Chance = 0f;
            Acid_Chance = 0f;
            break;
        }
        Boss_Electric_Chance = 0f;
        Boss_Wind_Chance = 0f;
        Boss_Fire_Chance = 0f;
        Spirit_Chance = 0f;
        DL_Red_Volatile_Chance = 0f;
        DL_Blue_Volatile_Chance = 0f;
        Boss_Elver_Stomper_Chance = 0f;
        Boss_Kuwait_Chance = 0f;
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
        Backstab_Multiplier = 1.25f;
        NonHeadshot_Armor_Multiplier = 1f;
        Beacon_Experience_Multiplier = 1f;
        Full_Moon_Experience_Multiplier = 2f;
        Min_Drops = 1u;
        Max_Drops = 1u;
        Min_Mega_Drops = 5u;
        Max_Mega_Drops = 5u;
        Min_Boss_Drops = 8u;
        Max_Boss_Drops = 10u;
        Slow_Movement = mode == EGameMode.EASY;
        Can_Stun = mode != EGameMode.HARD;
        Only_Critical_Stuns = mode == EGameMode.HARD;
        Weapons_Use_Player_Damage = mode == EGameMode.HARD;
        Can_Target_Barricades = true;
        Can_Target_Structures = true;
        Can_Target_Vehicles = true;
        Can_Target_Objects = true;
        Beacon_Max_Rewards = 0u;
        Beacon_Max_Participants = 0u;
        Beacon_Rewards_Multiplier = 1f;
    }
}
