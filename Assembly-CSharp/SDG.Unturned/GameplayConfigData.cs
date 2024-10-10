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

    /// <summary>
    /// Should group connections be shown on player list?
    /// </summary>
    public bool Group_Player_List;

    public bool Allow_Static_Groups;

    public bool Allow_Dynamic_Groups;

    /// <summary>
    /// If true, allow automatically creating an in-game group for members of your Steam lobby.
    /// Requires Allow_Dynamic_Groups to be enabled as well.
    /// </summary>
    public bool Allow_Lobby_Groups;

    public bool Allow_Shoulder_Camera;

    public bool Can_Suicide;

    /// <summary>
    /// Is friendly-fire allowed?
    /// </summary>
    public bool Friendly_Fire;

    /// <summary>
    /// Are sentry guns and beds allowed on vehicles?
    /// </summary>
    public bool Bypass_Buildable_Mobility;

    /// <summary>
    /// Should holiday (Halloween and Christmas) content like NPC outfits and decorations be loaded?
    /// </summary>
    public bool Allow_Holidays = true;

    /// <summary>
    /// Can "freeform" barricades be placed in the world?
    /// Defaults to true.
    /// </summary>
    public bool Allow_Freeform_Buildables;

    /// <summary>
    /// Can "freeform" barricades be placed on vehicles?
    /// Defaults to true.
    /// </summary>
    public bool Allow_Freeform_Buildables_On_Vehicles;

    /// <summary>
    /// If true, aim flinches away from center when damaged.
    /// Defaults to true.
    /// </summary>
    public bool Enable_Damage_Flinch;

    /// <summary>
    /// If true, camera will shake near explosions. Can also be toned down client-side in Options menu.
    /// Defaults to true.
    /// </summary>
    public bool Enable_Explosion_Camera_Shake;

    internal const uint MAX_TIMER_EXIT = 60u;

    public uint Timer_Exit;

    public uint Timer_Respawn;

    public uint Timer_Home;

    public uint Timer_Leave_Group;

    public uint Max_Group_Members;

    /// <summary>
    /// Scales velocity added to players by explosion knock-back.
    /// </summary>
    public float Explosion_Launch_Speed_Multiplier = 1f;

    /// <summary>
    /// Scales midair input change in player direction.
    /// </summary>
    public float AirStrafing_Acceleration_Multiplier = 1f;

    /// <summary>
    /// Scales midair decrease in speed while faster than max walk speed.
    /// </summary>
    public float AirStrafing_Deceleration_Multiplier = 1f;

    /// <summary>
    /// Scales magnitude of recoil while using third-person perspective.
    /// </summary>
    public float ThirdPerson_RecoilMultiplier = 2f;

    /// <summary>
    /// Scales magnitude of bullet inaccuracy while using third-person perspective.
    /// </summary>
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
        Allow_Freeform_Buildables = true;
        Allow_Freeform_Buildables_On_Vehicles = true;
        Enable_Damage_Flinch = true;
        Enable_Explosion_Camera_Shake = true;
    }

    public void InitSingleplayerDefaults()
    {
        Bypass_Buildable_Mobility = true;
    }
}
