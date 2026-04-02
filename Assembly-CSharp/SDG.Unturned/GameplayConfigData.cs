namespace SDG.Unturned;

public class GameplayConfigData
{
    /// <summary>
    /// Blueprints requiring a repair skill level higher than this cannot be crafted.
    /// Restricts players from repairing higher-tier items.
    /// </summary>
    public uint Repair_Level_Max;

    /// <summary>
    /// Should a hit confirmation be shown when players deal damage?
    /// </summary>
    public bool Hitmarkers;

    /// <summary>
    /// Should a crosshair be visible while holding a gun?
    /// </summary>
    public bool Crosshair;

    /// <summary>
    /// Should bullets be affected by gravity and travel time?
    /// </summary>
    public bool Ballistics;

    /// <summary>
    /// Should the player have permanent access to a "paper" map of the level even when they
    /// don't have the associated in-game item?
    /// </summary>
    public bool Chart;

    /// <summary>
    /// Should the player have permanent access to a GPS map of the level even when they
    /// don't have the associated in-game item?
    /// </summary>
    public bool Satellite;

    /// <summary>
    /// Should the player have permanent access to their compass heading HUD even when they
    /// don't have the associated in-game item?
    /// </summary>
    public bool Compass;

    /// <summary>
    /// Should group members and similar info be visible on the in-game map?
    /// </summary>
    public bool Group_Map;

    /// <summary>
    /// Should group member names be visible through walls?
    /// </summary>
    public bool Group_HUD;

    /// <summary>
    /// Should group connections be shown on player list?
    /// </summary>
    public bool Group_Player_List;

    /// <summary>
    /// Should Steam clans/groups be enables as in-game groups?
    /// </summary>
    public bool Allow_Static_Groups;

    /// <summary>
    /// Should players be allowed to create in-game groups and invite members of the server?
    /// </summary>
    public bool Allow_Dynamic_Groups;

    /// <summary>
    /// If true, allow automatically creating an in-game group for members of your Steam lobby.
    /// Requires Allow_Dynamic_Groups to be enabled as well.
    /// </summary>
    public bool Allow_Lobby_Groups;

    /// <summary>
    /// Should the third-person camera extend out to the side?
    /// If false, the third-person camera is centered over your character.
    /// </summary>
    public bool Allow_Shoulder_Camera;

    /// <summary>
    /// Should players be allowed to kill themselves from the pause menu?
    /// </summary>
    public bool Can_Suicide;

    /// <summary>
    /// Is friendly-fire within groups allowed?
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
    /// </summary>
    public bool Allow_Freeform_Buildables;

    /// <summary>
    /// Can "freeform" barricades be placed on vehicles?
    /// </summary>
    public bool Allow_Freeform_Buildables_On_Vehicles;

    /// <summary>
    /// If true, aim flinches away from center when damaged.
    /// </summary>
    public bool Enable_Damage_Flinch;

    /// <summary>
    /// If true, camera will shake near explosions. Can also be toned down client-side in Options menu.
    /// </summary>
    public bool Enable_Explosion_Camera_Shake;

    /// <summary>
    /// If true, crafting blueprints can require nearby workstations.
    /// If false, only the backwards-compatibility "Heat Source" vanilla crafting tag can be required. This
    /// functions identically to the cooking-skill-also-requires-heat behavior from before.
    /// </summary>
    public bool Enable_Workstation_Requirements;

    /// <summary>
    /// If true, client-side options like damage flinch, explosion camera shake, viewmodel bob are ignored.
    /// </summary>
    public bool Disable_Motion_Sickness_Options;

    /// <summary>
    /// If true, hide viewmodel while aiming a dual-render scope and show a 2D overlay instead.
    /// Useful for backwards compatibility with modded scopes that have a small enough
    /// dual-render surface to zoom-*out* when aiming in.
    /// </summary>
    public bool Use_2D_Scope_Overlay;

    internal const uint MAX_TIMER_EXIT = 60u;

    /// <summary>
    /// How long (in seconds) before a player can leave the server through the pause menu.
    /// </summary>
    public uint Timer_Exit;

    /// <summary>
    /// How long (in seconds) after death before a player can respawn.
    /// </summary>
    public uint Timer_Respawn;

    /// <summary>
    /// How long (in seconds) after death before a player can respawn at their bed.
    /// </summary>
    public uint Timer_Home;

    /// <summary>
    /// How long (in seconds) after a player requests to leave an in-game "dynamic" group
    /// before they are actually removed. Gives group members time to take cover.
    /// </summary>
    public uint Timer_Leave_Group;

    /// <summary>
    /// Maximum number of players invitable to an in-game "dynamic" group.
    /// Depends on Allow_Dynamic_Groups.
    /// </summary>
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
    /// Scales magnitude of recoil while using first-person perspective.
    /// </summary>
    public float FirstPerson_RecoilMultiplier = 1f;

    /// <summary>
    /// Scales magnitude of recoil while aiming in first-person perspective.
    /// </summary>
    public float FirstPerson_AimingRecoilMultiplier = 1f;

    /// <summary>
    /// Scales magnitude of recoil inversely with zoom level while aiming in first-person perspective.
    /// </summary>
    public float FirstPerson_AimingZoomRecoilReduction;

    /// <summary>
    /// Scales magnitude of recoil while using third-person perspective.
    /// </summary>
    public float ThirdPerson_RecoilMultiplier = 2f;

    /// <summary>
    /// Scales magnitude of bullet inaccuracy while using third-person perspective.
    /// </summary>
    public float ThirdPerson_SpreadMultiplier = 2f;

    /// <summary>
    /// [0 to 1] Scales how much the first-person move up and down while jumping/landing.
    /// </summary>
    public float Viewmodel_AimingJumpLandMultiplier = 1f;

    /// <summary>
    /// [0 to 1] Scales how much the first-person arms move while ADS.
    /// </summary>
    public float Viewmodel_AimingMisalignmentMultiplier = 1f;

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
        switch (mode)
        {
        case EGameMode.EASY:
            ThirdPerson_RecoilMultiplier = 1f;
            ThirdPerson_SpreadMultiplier = 1f;
            Viewmodel_AimingMisalignmentMultiplier = 0.2f;
            FirstPerson_AimingZoomRecoilReduction = 0.25f;
            break;
        case EGameMode.NORMAL:
            Viewmodel_AimingMisalignmentMultiplier = 0.5f;
            break;
        case EGameMode.HARD:
            Viewmodel_AimingMisalignmentMultiplier = 1f;
            break;
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
        Enable_Workstation_Requirements = true;
    }

    public void InitSingleplayerDefaults()
    {
        Bypass_Buildable_Mobility = true;
    }
}
