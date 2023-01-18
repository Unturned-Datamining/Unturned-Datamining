using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDG.Unturned;

public class LevelInfoConfigData
{
    public string[] Creators;

    public string[] Collaborators;

    public string[] Thanks;

    public int Item;

    public int[] Associated_Stockpile_Items;

    public string Feedback;

    public AssetReference<LevelAsset> Asset;

    public List<LevelTrainAssociation> Trains;

    public Dictionary<string, object> Mode_Config_Overrides;

    public bool Allow_Underwater_Features;

    public bool Terrain_Snow_Sparkle;

    public bool Use_Legacy_Clip_Borders;

    public bool Use_Legacy_Ground;

    public bool Use_Legacy_Water;

    public bool Use_Vanilla_Bubbles;

    public bool Use_Underground_Whitelist;

    public bool Use_Legacy_Snow_Height;

    public bool Use_Legacy_Fog_Height;

    public bool Use_Legacy_Oxygen_Height;

    public bool Use_Rain_Volumes;

    public bool Use_Snow_Volumes;

    public bool Is_Aurora_Borealis_Visible;

    public bool Snow_Affects_Temperature;

    public ELevelWeatherOverride Weather_Override;

    public bool Has_Atmosphere;

    public bool Allow_Crafting;

    public bool Allow_Skills;

    public bool Allow_Information;

    public bool Allow_Holiday_Redirects;

    public bool Has_Global_Electricity;

    public float Gravity;

    public float Blimp_Altitude;

    public float Max_Walkable_Slope;

    public float Prevent_Building_Near_Spawnpoint_Radius;

    public ESingleplayerMapCategory Category;

    public bool Visible_In_Matchmaking;

    public bool PlayerUI_HealthVisible = true;

    public bool PlayerUI_FoodVisible = true;

    public bool PlayerUI_WaterVisible = true;

    public bool PlayerUI_VirusVisible = true;

    public bool PlayerUI_StaminaVisible = true;

    public bool PlayerUI_OxygenVisible = true;

    public bool PlayerUI_GunVisible = true;

    public string Version;

    [JsonIgnore]
    public uint PackedVersion;

    public int Tips;

    public bool Use_Arena_Compactor;

    public List<ArenaLoadout> Arena_Loadouts;

    public List<ArenaLoadout> Spawn_Loadouts;

    [JsonIgnore]
    public byte[] Hash;

    public LevelInfoConfigData()
    {
        Creators = new string[0];
        Collaborators = new string[0];
        Thanks = new string[0];
        Item = 0;
        Associated_Stockpile_Items = new int[0];
        Feedback = null;
        Asset = AssetReference<LevelAsset>.invalid;
        Trains = new List<LevelTrainAssociation>();
        Mode_Config_Overrides = new Dictionary<string, object>();
        Allow_Underwater_Features = false;
        Terrain_Snow_Sparkle = false;
        Use_Legacy_Clip_Borders = true;
        Use_Legacy_Ground = true;
        Use_Legacy_Water = true;
        Use_Vanilla_Bubbles = true;
        Use_Legacy_Snow_Height = true;
        Use_Legacy_Oxygen_Height = true;
        Use_Rain_Volumes = false;
        Use_Snow_Volumes = false;
        Is_Aurora_Borealis_Visible = false;
        Snow_Affects_Temperature = true;
        Has_Atmosphere = true;
        Allow_Crafting = true;
        Allow_Skills = true;
        Allow_Information = true;
        Gravity = -9.81f;
        Blimp_Altitude = 150f;
        Max_Walkable_Slope = -1f;
        Prevent_Building_Near_Spawnpoint_Radius = 16f;
        Category = ESingleplayerMapCategory.MISC;
        Visible_In_Matchmaking = false;
        Use_Arena_Compactor = true;
        Arena_Loadouts = new List<ArenaLoadout>();
        Spawn_Loadouts = new List<ArenaLoadout>();
        Version = "3.0.0.0";
    }
}
