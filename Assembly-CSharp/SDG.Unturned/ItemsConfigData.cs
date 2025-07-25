namespace SDG.Unturned;

public class ItemsConfigData
{
    /// <summary>
    /// Percentage [0 to 1] of item spawns to use.
    /// For example, if set to 0.2 and level has 100 item spawns, max 20 items will spawn at a time.
    /// </summary>
    public float Spawn_Chance;

    /// <summary>
    /// How long (in seconds) before an item dropped by a player is despawned.
    /// </summary>
    public float Despawn_Dropped_Time;

    /// <summary>
    /// How long (in seconds) before a spawned item is despawned.
    /// (For example, an item nobody wants to pick up.)
    /// </summary>
    public float Despawn_Natural_Time;

    /// <summary>
    /// When less than the target amount of items are dropped (determined by Spawn_Chance), a new
    /// item is spawned approximately this often (in seconds).
    /// </summary>
    public float Respawn_Time;

    /// <summary>
    /// Percentage [0 to 1] probability of item spawning at max quality.
    /// </summary>
    public float Quality_Full_Chance;

    /// <summary>
    /// When an item spawns without max quality, the random quality is scaled by this factor.
    /// For example, 0.5 halves the initial quality.
    /// </summary>
    public float Quality_Multiplier;

    /// <summary>
    /// Percentage [0 to 1] probability of gun spawning with full ammo.
    /// </summary>
    public float Gun_Bullets_Full_Chance;

    /// <summary>
    /// When a gun spawns without full ammo, the random amount is scaled by this factor.
    /// </summary>
    public float Gun_Bullets_Multiplier;

    /// <summary>
    /// Percentage [0 to 1] probability of magazines spawning with full ammo.
    /// </summary>
    public float Magazine_Bullets_Full_Chance;

    /// <summary>
    /// When a magazine spawns without full ammo, the random amount is scaled by this factor.
    /// </summary>
    public float Magazine_Bullets_Multiplier;

    /// <summary>
    /// Percentage [0 to 1] probability of non-magazines spawning with full amount.
    /// (E.g., ammo boxes.)
    /// </summary>
    public float Crate_Bullets_Full_Chance;

    /// <summary>
    /// When a non-magazine spawns without full amount, the random amount is scaled by this factor.
    /// (E.g., ammo boxes.)
    /// </summary>
    public float Crate_Bullets_Multiplier;

    /// <summary>
    /// Original option for disabling item quality. If false, items spawn at 100% quality and
    /// their quality doesn't decrease. For backwards compatibility, the newer per-item-type
    /// durability options are ignored if this is off.
    /// </summary>
    public bool Has_Durability;

    /// <summary>
    /// Food-specific replacement for Has_Durability. If true, food spawns at 100% quality.
    /// </summary>
    public bool Food_Spawns_At_Full_Quality;

    /// <summary>
    /// Water-specific replacement for Has_Durability. If true, water spawns at 100% quality.
    /// </summary>
    public bool Water_Spawns_At_Full_Quality;

    /// <summary>
    /// Clothing-specific replacement for Has_Durability. If true, clothing spawns at 100% quality.
    /// </summary>
    public bool Clothing_Spawns_At_Full_Quality;

    /// <summary>
    /// Weapon-specific replacement for Has_Durability. If true, weapons spawns at 100% quality.
    /// </summary>
    public bool Weapons_Spawn_At_Full_Quality;

    /// <summary>
    /// Fallback used when spawning an item that doesn't fit into one of the other quality/durability settings.
    /// If true, items spawn at 100% quality.
    /// </summary>
    public bool Default_Spawns_At_Full_Quality;

    /// <summary>
    /// Clothing-specific replacement for Has_Durability. If false, clothing quality
    /// doesn't decrease when damaged.
    /// </summary>
    public bool Clothing_Has_Durability;

    /// <summary>
    /// Melee and gun replacement for Has_Durability. If false, weapons quality
    /// doesn't decrease when used.
    /// </summary>
    public bool Weapons_Have_Durability;

    internal bool ShouldClothingTakeDamage
    {
        get
        {
            if (!Has_Durability)
            {
                return false;
            }
            return Clothing_Has_Durability;
        }
    }

    internal bool ShouldWeaponTakeDamage
    {
        get
        {
            if (!Has_Durability)
            {
                return false;
            }
            return Weapons_Have_Durability;
        }
    }

    public ItemsConfigData(EGameMode mode)
    {
        Despawn_Dropped_Time = 600f;
        Despawn_Natural_Time = 900f;
        switch (mode)
        {
        case EGameMode.EASY:
            Spawn_Chance = 0.35f;
            Respawn_Time = 50f;
            Quality_Full_Chance = 0.1f;
            Quality_Multiplier = 1f;
            Gun_Bullets_Full_Chance = 0.1f;
            Gun_Bullets_Multiplier = 1f;
            Magazine_Bullets_Full_Chance = 0.1f;
            Magazine_Bullets_Multiplier = 1f;
            Crate_Bullets_Full_Chance = 0.1f;
            Crate_Bullets_Multiplier = 1f;
            break;
        case EGameMode.NORMAL:
            Spawn_Chance = 0.35f;
            Respawn_Time = 100f;
            Quality_Full_Chance = 0.1f;
            Quality_Multiplier = 1f;
            Gun_Bullets_Full_Chance = 0.05f;
            Gun_Bullets_Multiplier = 0.25f;
            Magazine_Bullets_Full_Chance = 0.05f;
            Magazine_Bullets_Multiplier = 0.5f;
            Crate_Bullets_Full_Chance = 0.05f;
            Crate_Bullets_Multiplier = 1f;
            break;
        case EGameMode.HARD:
            Spawn_Chance = 0.15f;
            Respawn_Time = 150f;
            Quality_Full_Chance = 0.01f;
            Quality_Multiplier = 1f;
            Gun_Bullets_Full_Chance = 0.025f;
            Gun_Bullets_Multiplier = 0.1f;
            Magazine_Bullets_Full_Chance = 0.025f;
            Magazine_Bullets_Multiplier = 0.25f;
            Crate_Bullets_Full_Chance = 0.025f;
            Crate_Bullets_Multiplier = 0.75f;
            break;
        default:
            Spawn_Chance = 1f;
            Respawn_Time = 1000000f;
            Quality_Full_Chance = 1f;
            Quality_Multiplier = 1f;
            Gun_Bullets_Full_Chance = 1f;
            Gun_Bullets_Multiplier = 1f;
            Magazine_Bullets_Full_Chance = 1f;
            Magazine_Bullets_Multiplier = 1f;
            Crate_Bullets_Full_Chance = 1f;
            Crate_Bullets_Multiplier = 1f;
            break;
        }
        if (mode == EGameMode.EASY)
        {
            Has_Durability = false;
            Food_Spawns_At_Full_Quality = true;
            Water_Spawns_At_Full_Quality = true;
            Clothing_Spawns_At_Full_Quality = true;
            Weapons_Spawn_At_Full_Quality = true;
            Default_Spawns_At_Full_Quality = true;
            Clothing_Has_Durability = false;
            Weapons_Have_Durability = false;
        }
        else
        {
            Has_Durability = true;
            Food_Spawns_At_Full_Quality = false;
            Water_Spawns_At_Full_Quality = false;
            Clothing_Spawns_At_Full_Quality = false;
            Weapons_Spawn_At_Full_Quality = false;
            Default_Spawns_At_Full_Quality = false;
            Clothing_Has_Durability = true;
            Weapons_Have_Durability = true;
        }
    }
}
