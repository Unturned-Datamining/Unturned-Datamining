namespace SDG.Unturned;

public class EventsConfigData
{
    public float Rain_Frequency_Min;

    public float Rain_Frequency_Max;

    public float Rain_Duration_Min;

    public float Rain_Duration_Max;

    public float Snow_Frequency_Min;

    public float Snow_Frequency_Max;

    public float Snow_Duration_Min;

    public float Snow_Duration_Max;

    /// <summary>
    /// Each per-level custom weather frequency is multiplied by this value.
    /// </summary>
    public float Weather_Frequency_Multiplier;

    /// <summary>
    /// Each per-level custom weather duration is multiplied by this value.
    /// </summary>
    public float Weather_Duration_Multiplier;

    public float Airdrop_Frequency_Min;

    public float Airdrop_Frequency_Max;

    public float Airdrop_Speed;

    public float Airdrop_Force;

    public uint Arena_Min_Players;

    public uint Arena_Compactor_Damage;

    public float Arena_Compactor_Extra_Damage_Per_Second;

    public uint Arena_Clear_Timer;

    public uint Arena_Finale_Timer;

    public uint Arena_Restart_Timer;

    public uint Arena_Compactor_Delay_Timer;

    public uint Arena_Compactor_Pause_Timer;

    public bool Use_Airdrops;

    public bool Arena_Use_Compactor_Pause;

    public float Arena_Compactor_Speed_Tiny;

    public float Arena_Compactor_Speed_Small;

    public float Arena_Compactor_Speed_Medium;

    public float Arena_Compactor_Speed_Large;

    public float Arena_Compactor_Speed_Insane;

    public float Arena_Compactor_Shrink_Factor;

    public EventsConfigData(EGameMode mode)
    {
        Rain_Frequency_Min = 2.3f;
        Rain_Frequency_Max = 5.6f;
        Rain_Duration_Min = 0.05f;
        Rain_Duration_Max = 0.15f;
        Snow_Frequency_Min = 1.3f;
        Snow_Frequency_Max = 4.6f;
        Snow_Duration_Min = 0.2f;
        Snow_Duration_Max = 0.5f;
        Weather_Frequency_Multiplier = 1f;
        Weather_Duration_Multiplier = 1f;
        Airdrop_Frequency_Min = 0.8f;
        Airdrop_Frequency_Max = 6.5f;
        Airdrop_Speed = 128f;
        Airdrop_Force = 9.5f;
        Arena_Clear_Timer = 5u;
        Arena_Finale_Timer = 10u;
        Arena_Restart_Timer = 15u;
        Arena_Compactor_Delay_Timer = 1u;
        Arena_Compactor_Pause_Timer = 5u;
        Arena_Min_Players = 2u;
        Arena_Compactor_Damage = 9u;
        Arena_Compactor_Extra_Damage_Per_Second = 1f;
        Use_Airdrops = true;
        Arena_Use_Compactor_Pause = true;
        Arena_Compactor_Speed_Tiny = 0.5f;
        Arena_Compactor_Speed_Small = 1.5f;
        Arena_Compactor_Speed_Medium = 3f;
        Arena_Compactor_Speed_Large = 4.5f;
        Arena_Compactor_Speed_Insane = 6f;
        Arena_Compactor_Shrink_Factor = 0.5f;
    }
}
