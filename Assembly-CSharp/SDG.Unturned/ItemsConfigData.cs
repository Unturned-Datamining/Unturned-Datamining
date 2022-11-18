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

    public bool Has_Durability;

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
        }
        else
        {
            Has_Durability = true;
        }
    }
}
