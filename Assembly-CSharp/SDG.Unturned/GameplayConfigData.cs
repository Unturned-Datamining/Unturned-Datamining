namespace SDG.Unturned;

public class GameplayConfigData
{
    public uint Repair_Level_Max;

    public bool Hitmarkers;

    public bool Crosshair;

    public bool Ballistics;

    public bool Chart;

    public bool Satellite;

    public bool Compass;

    public bool Group_Map;

    public bool Group_HUD;

    public bool Group_Player_List;

    public bool Allow_Static_Groups;

    public bool Allow_Dynamic_Groups;

    public bool Allow_Lobby_Groups;

    public bool Allow_Shoulder_Camera;

    public bool Can_Suicide;

    public bool Friendly_Fire;

    public bool Bypass_Buildable_Mobility;

    public bool Allow_Holidays = true;

    internal const uint MAX_TIMER_EXIT = 60u;

    public uint Timer_Exit;

    public uint Timer_Respawn;

    public uint Timer_Home;

    public uint Timer_Leave_Group;

    public uint Max_Group_Members;

    public float Explosion_Launch_Speed_Multiplier = 1f;

    public float AirStrafing_Acceleration_Multiplier = 1f;

    public float AirStrafing_Deceleration_Multiplier = 1f;

    public float ThirdPerson_RecoilMultiplier = 2f;

    public float ThirdPerson_SpreadMultiplier = 2f;

    internal static CommandLineFlag _forceTrustClient = new CommandLineFlag(defaultValue: false, "-ForceTrustClient");

    public GameplayConfigData(EGameMode mode)
    {
        Repair_Level_Max = 3u;
        if (mode == EGameMode.HARD)
        {
            Hitmarkers = false;
            Crosshair = false;
        }
        else
        {
            Hitmarkers = true;
            Crosshair = true;
        }
        if (mode == EGameMode.EASY)
        {
            Ballistics = false;
        }
        else
        {
            Ballistics = true;
        }
        Chart = mode == EGameMode.EASY;
        Satellite = false;
        Compass = false;
        Group_Map = mode != EGameMode.HARD;
        Group_HUD = true;
        Group_Player_List = true;
        Allow_Static_Groups = true;
        Allow_Dynamic_Groups = true;
        Allow_Lobby_Groups = true;
        Allow_Shoulder_Camera = true;
        Can_Suicide = true;
        Friendly_Fire = false;
        Bypass_Buildable_Mobility = false;
        Timer_Exit = 10u;
        Timer_Respawn = 10u;
        Timer_Home = 30u;
        Timer_Leave_Group = 30u;
        Max_Group_Members = 0u;
    }

    public void InitSingleplayerDefaults()
    {
        Bypass_Buildable_Mobility = true;
    }
}
