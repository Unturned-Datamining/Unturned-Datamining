namespace SDG.Unturned;

public class EventsConfigData
{
    /// <summary>
    /// Minimum number of in-game days between legacy rain events. 
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Rain_Frequency_Min;

    /// <summary>
    /// Maximum number of in-game days between legacy rain events. 
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Rain_Frequency_Max;

    /// <summary>
    /// Minimum number of in-game days a legacy rain event lasts. Zero turns off legacy rain.
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Rain_Duration_Min;

    /// <summary>
    /// Maximum number of in-game days a legacy rain event lasts. Zero turns off legacy rain.
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Rain_Duration_Max;

    /// <summary>
    /// Minimum number of in-game days between legacy snow events. 
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Snow_Frequency_Min;

    /// <summary>
    /// Maximum number of in-game days between legacy snow events. 
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Snow_Frequency_Max;

    /// <summary>
    /// Minimum number of in-game days a legacy snow event lasts. Zero turns off legacy snow.
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Snow_Duration_Min;

    /// <summary>
    /// Maximum number of in-game days a legacy snow event lasts. Zero turns off legacy snow.
    /// Only applicable for backwards compatibility with levels using the legacy weather features.
    /// </summary>
    public float Snow_Duration_Max;

    /// <summary>
    /// Scales number of in-game days between weather events. (Levels using the newer weather
    /// features can have multiple weather types with different frequencies.) If this was
    /// accidentally set to a high value you can use the "/weather 0" command to reschedule
    /// the next weather event.
    ///
    /// Lower values cause more frequent weather, higher values cause less frequent weather.
    /// (Misnomer, sorry!)
    /// </summary>
    public float Weather_Frequency_Multiplier;

    /// <summary>
    /// Scales number of in-game days a weather event lasts. (Levels using the newer weather
    /// features can have multiple weather types with different durations.)
    /// Zero turns off weather entirely.
    /// </summary>
    public float Weather_Duration_Multiplier;

    /// <summary>
    /// Minimum number of in-game days between airdrops. Depends on Use_Airdrops.
    /// </summary>
    public float Airdrop_Frequency_Min;

    /// <summary>
    /// Maximum number of in-game days between airdrops. Depends on Use_Airdrops.
    /// </summary>
    public float Airdrop_Frequency_Max;

    /// <summary>
    /// How fast (in meters per second) the airdrop plane flies across the level.
    /// Lower values give players more time to react and chase the airplane.
    /// </summary>
    public float Airdrop_Speed;

    /// <summary>
    /// Amount of upward force applied to the carepackage, resisting gravity.
    /// Higher values require players to wait longer for the carepackage.
    /// (This isn't intuitive, sorry!)
    /// </summary>
    public float Airdrop_Force;

    /// <summary>
    /// Minimum number of teams needed to start an arena match.
    /// </summary>
    public uint Arena_Min_Players;

    /// <summary>
    /// Base damage per second while standing outside the arena field.
    /// </summary>
    public uint Arena_Compactor_Damage;

    /// <summary>
    /// Accumulating additional damage per second while standing outside the arena field.
    /// </summary>
    public float Arena_Compactor_Extra_Damage_Per_Second;

    /// <summary>
    /// How long (in seconds) between match ready and teleporting players into the arena.
    /// </summary>
    public uint Arena_Clear_Timer;

    /// <summary>
    /// How long (in seconds) after a winner is announced to wait before restarting.
    /// </summary>
    public uint Arena_Finale_Timer;

    /// <summary>
    /// How long (in seconds) to wait in intermission before starting the next match.
    /// </summary>
    public uint Arena_Restart_Timer;

    /// <summary>
    /// How long (in seconds) before first arena circle starts shrinking.
    /// </summary>
    public uint Arena_Compactor_Delay_Timer;

    /// <summary>
    /// How long (in seconds) after arena circle finishes shrinking to start shrinking again.
    /// </summary>
    public uint Arena_Compactor_Pause_Timer;

    /// <summary>
    /// Should airplanes fly over the level dropping carepackages?
    /// </summary>
    public bool Use_Airdrops;

    /// <summary>
    /// If true, arena selects multiple smaller circles within the initial circle.
    /// Otherwise, arena cricle shrinks toward its initial center.
    /// </summary>
    public bool Arena_Use_Compactor_Pause;

    /// <summary>
    /// How quickly (in meters per second) the arena radius shrinks on "Tiny" size levels.
    /// </summary>
    public float Arena_Compactor_Speed_Tiny;

    /// <summary>
    /// How quickly (in meters per second) the arena radius shrinks on "Small" size levels.
    /// </summary>
    public float Arena_Compactor_Speed_Small;

    /// <summary>
    /// How quickly (in meters per second) the arena radius shrinks on "Medium" size levels.
    /// </summary>
    public float Arena_Compactor_Speed_Medium;

    /// <summary>
    /// How quickly (in meters per second) the arena radius shrinks on "Large" size levels.
    /// </summary>
    public float Arena_Compactor_Speed_Large;

    /// <summary>
    /// How quickly (in meters per second) the arena radius shrinks on "Insane" size levels.
    /// </summary>
    public float Arena_Compactor_Speed_Insane;

    /// <summary>
    /// Percentage [0 to 1] of arena circle radius retained when selecting next smaller circle.
    /// Depends on Arena_Use_Compactor_Pause.
    /// </summary>
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
