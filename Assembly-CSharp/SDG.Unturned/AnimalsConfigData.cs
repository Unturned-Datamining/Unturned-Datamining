namespace SDG.Unturned;

public class AnimalsConfigData
{
    public float Respawn_Time;

    public float Damage_Multiplier;

    public float Armor_Multiplier;

    public uint Max_Instances_Tiny;

    public uint Max_Instances_Small;

    public uint Max_Instances_Medium;

    public uint Max_Instances_Large;

    public uint Max_Instances_Insane;

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
