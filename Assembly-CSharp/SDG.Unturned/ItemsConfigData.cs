namespace SDG.Unturned;

public class ItemsConfigData
{
    public float Spawn_Chance;

    public float Despawn_Dropped_Time;

    public float Despawn_Natural_Time;

    public float Respawn_Time;

    public float Quality_Full_Chance;

    public float Quality_Multiplier;

    public float Gun_Bullets_Full_Chance;

    public float Gun_Bullets_Multiplier;

    public float Magazine_Bullets_Full_Chance;

    public float Magazine_Bullets_Multiplier;

    public float Crate_Bullets_Full_Chance;

    public float Crate_Bullets_Multiplier;

    /// <summary>
    /// Original option for disabling item quality. Defaults to true. If false, items spawn at 100% quality and
    /// their quality doesn't decrease. For backwards compatibility, the newer per-item-type durability options
    /// are ignored if this is off.
    /// </summary>
    public bool Has_Durability;

    /// <summary>
    /// Food-specific replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to false. If true, food spawns at 100% quality.
    /// </summary>
    public bool Food_Spawns_At_Full_Quality;

    /// <summary>
    /// Water-specific replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to false. If true, water spawns at 100% quality.
    /// </summary>
    public bool Water_Spawns_At_Full_Quality;

    /// <summary>
    /// Clothing-specific replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to false. If true, clothing spawns at 100% quality.
    /// </summary>
    public bool Clothing_Spawns_At_Full_Quality;

    /// <summary>
    /// Weapon-specific replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to false. If true, weapons spawns at 100% quality.
    /// </summary>
    public bool Weapons_Spawn_At_Full_Quality;

    /// <summary>
    /// Fallback used when spawning an item that doesn't fit into one of the other quality/durability settings.
    /// Defaults to false. If true, items spawn at 100% quality.
    /// </summary>
    public bool Default_Spawns_At_Full_Quality;

    /// <summary>
    /// Clothing-specific replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to true. If false, clothing quality
    /// doesn't decrease when damaged.
    /// </summary>
    public bool Clothing_Has_Durability;

    /// <summary>
    /// Melee and gun replacement for <see cref="F:SDG.Unturned.ItemsConfigData.Has_Durability" />. Defaults to true. If false, weapons quality
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
