using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class LevelAsset : Asset
{
    public struct SchedulableWeather
    {
        public AssetReference<WeatherAssetBase> assetRef;

        public float minFrequency;

        public float maxFrequency;

        public float minDuration;

        public float maxDuration;
    }

    public struct LoadingScreenMusic
    {
        public MasterBundleReference<AudioClip> loopRef;

        public MasterBundleReference<AudioClip> outroRef;

        public float loopVolume;

        public float outroVolume;
    }

    public class SkillRule
    {
        public int defaultLevel;

        public int maxUnlockableLevel;

        public float costMultiplier;

        /// <summary>
        /// If &gt;= 0, overrides vanilla skill cost.
        /// Defaults to -1.
        /// </summary>
        public int baseCostOverride;

        /// <summary>
        /// If &gt;= 0, overrides vanilla increase in skill cost with each level.
        /// For example, if the base cost is 10 and this is 15, the first level will cost 10 XP,
        /// the second level 25 XP, the third 40 XP, so on and so forth.
        /// Defaults to -1.
        /// </summary>
        public int perLevelCostIncreaseOverride;
    }

    public struct CloudOverrideParticleSystemsPath : IDatParseable
    {
        /// <summary>
        /// t passed into ParticleSystem.Simulate when clouds need an update.
        /// </summary>
        public float WarmupTime;

        public string ComponentPath { get; set; }

        /// <summary>
        /// Multiplier for CloudOverrideParticlesPrefab emission rate according to level's clouds intensity.
        /// </summary>
        public float RateOverTimeScale { get; set; }

        /// <summary>
        /// Particle system's material instance will have these color properties set to the level's cloud color.
        /// Defaults to _Color.
        /// </summary>
        public string[] MaterialColorPropertyNames { get; set; }

        public bool TryParse(IDatNode node)
        {
            if (node is IDatDictionary dictionary)
            {
                ComponentPath = dictionary.GetString("Path");
                RateOverTimeScale = dictionary.ParseFloat("RateOverTimeScale");
                if (dictionary.TryGetList("MaterialColorPropertyNames", out var node2))
                {
                    List<string> list = new List<string>();
                    foreach (IDatValue item in node2)
                    {
                        if (!item.IsValueNullOrEmpty())
                        {
                            list.Add(item.Value);
                        }
                    }
                    MaterialColorPropertyNames = list.ToArray();
                }
                else
                {
                    MaterialColorPropertyNames = new string[1] { "_Color" };
                }
                WarmupTime = dictionary.ParseFloat("WarmupTime");
                return true;
            }
            return false;
        }
    }

    public struct DefaultLoadoutItem : IDatParseable
    {
        public CachingBcAssetRef assetRef;

        public int amount;

        public EItemOrigin origin;

        public ItemAsset ResolveAsset(Func<string> errorContextCallback)
        {
            Asset asset = assetRef.Get();
            if (asset == null)
            {
                UnturnedLog.warn(string.Format("{0} unable to find asset {1}", errorContextCallback?.Invoke() ?? "Unknown", assetRef));
                return null;
            }
            if (asset is SpawnAsset spawnAsset)
            {
                asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, errorContextCallback);
                if (asset == null)
                {
                    return null;
                }
            }
            if (!(asset is ItemAsset result))
            {
                UnturnedLog.warn((errorContextCallback?.Invoke() ?? "Unknown") + " tried to spawn non-item asset " + asset.FriendlyNameWithFriendlyType);
                return null;
            }
            return result;
        }

        public bool TryParse(IDatNode node)
        {
            if (node is IDatDictionary dictionary)
            {
                bool result = dictionary.TryParseBcAssetRef("Asset", EAssetType.ITEM, out assetRef);
                amount = dictionary.ParseInt32("Amount", 1);
                origin = dictionary.ParseEnum("Origin", EItemOrigin.WORLD);
                return result;
            }
            return false;
        }
    }

    internal class TerrainColorRule : IDatParseable
    {
        public enum EComparisonResult
        {
            TooSimilar,
            OutsideHueThreshold,
            OutsideSaturationThreshold,
            OutsideValueThreshold
        }

        public float ruleHue;

        public float ruleSaturation;

        public float ruleValue;

        public float hueThreshold;

        public float saturationThreshold;

        public float valueThreshold;

        public EComparisonResult CompareColors(float inputHue, float inputSaturation, float inputValue)
        {
            float num;
            float num2;
            if (inputHue < ruleHue)
            {
                num = ruleHue - inputHue;
                num2 = inputHue + 1f - ruleHue;
            }
            else
            {
                num = inputHue - ruleHue;
                num2 = ruleHue + 1f - inputHue;
            }
            if (num > hueThreshold && num2 > hueThreshold)
            {
                return EComparisonResult.OutsideHueThreshold;
            }
            if (Mathf.Abs(inputSaturation - ruleSaturation) > saturationThreshold)
            {
                return EComparisonResult.OutsideSaturationThreshold;
            }
            if (Mathf.Abs(inputValue - ruleValue) > valueThreshold)
            {
                return EComparisonResult.OutsideValueThreshold;
            }
            return EComparisonResult.TooSimilar;
        }

        public bool TryParse(IDatNode node)
        {
            if (node is IDatDictionary dictionary)
            {
                Color32 value;
                bool num = dictionary.TryParseColor32RGB("Color", out value);
                Color.RGBToHSV(value, out ruleHue, out ruleSaturation, out ruleValue);
                return num & dictionary.TryParseFloat("HueThreshold", out hueThreshold) & dictionary.TryParseFloat("SaturationThreshold", out saturationThreshold) & dictionary.TryParseFloat("ValueThreshold", out valueThreshold);
            }
            return false;
        }
    }

    public static AssetReference<LevelAsset> defaultLevel = new AssetReference<LevelAsset>(new Guid("12dc9fdbe9974022afd21158ad54b76a"));

    internal static MasterBundleReference<AudioClip> DefaultDeathMusicRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Music/Death.mp3");

    public TypeReference<GameMode> defaultGameMode;

    public List<TypeReference<GameMode>> supportedGameModes;

    public MasterBundleReference<GameObject> dropshipPrefab;

    public AssetReference<AirdropAsset> airdropRef;

    public const float DEFAULT_UNDERWATER_FOG_DENSITY = 0.075f;

    /// <summary>
    /// Player stealth radius cannot go below this value.
    /// </summary>
    public float minStealthRadius;

    /// <summary>
    /// Deal damage and break legs if speed is greater than this value.
    /// </summary>
    public float fallDamageSpeedThreshold;

    /// <summary>
    /// By default players in singleplayer and admins in multiplayer have a faster salvage time.
    /// This option was requested for maps with entirely custom balanced salvage times.
    /// </summary>
    public bool enableAdminFasterSalvageDuration = true;

    public List<AssetReference<CraftingBlacklistAsset>> craftingBlacklists;

    /// <summary>
    /// Cached result of finding all craftingBlacklists.
    /// </summary>
    private List<CraftingBlacklistAsset> resolvedCraftingBlacklists;

    /// <summary>
    /// Determines which weather can naturally occur in this level.
    /// Null if empty.
    /// </summary>
    public SchedulableWeather[] schedulableWeathers;

    /// <summary>
    /// If set, this weather will always be active and scheduled weather is disabled.
    /// </summary>
    public AssetReference<WeatherAssetBase> perpetualWeatherRef;

    public LoadingScreenMusic[] loadingScreenMusic;

    /// <summary>
    /// Defaults to false because some servers have rules and info on the loading screen.
    /// </summary>
    public bool shouldAnimateBackgroundImage;

    /// <summary>
    /// Volume weather mask used while not inside an ambience volume.
    /// </summary>
    public uint globalWeatherMask;

    /// <summary>
    /// Allows level to override skill max levels.
    /// Can be turned off with config Prevent_Level_Skill_Overrides true.
    /// Null if empty, otherwise matches 1:1 with PlayerSkills._skills.
    /// </summary>
    public SkillRule[][] skillRules;

    /// <summary>
    /// If false, clouds are removed from the skybox.
    /// </summary>
    public bool hasClouds = true;

    /// <summary>
    /// Players are kicked from multiplayer if their skin color is within threshold of any of these rules.
    /// </summary>
    internal List<TerrainColorRule> terrainColorRules;

    private CachingAssetRef _defaultFishSpawnTable;

    /// <summary>
    /// Intensity of fog effect while camera is inside a water volume.
    /// Defaults to 0.075.
    /// </summary>
    public float UnderwaterFogDensity { get; set; } = 0.075f;


    /// <summary>
    /// Audio clip to play in 2D when a player dies.
    /// </summary>
    public MasterBundleReference<AudioClip> DeathMusicRef { get; private set; }

    /// <summary>
    /// If set, instantiate this particle system and set its material color to cloud color.
    /// </summary>
    public MasterBundleReference<GameObject> CloudOverridePrefab { get; set; }

    public CloudOverrideParticleSystemsPath[] CloudOverrideParticleSystemPaths { get; set; }

    /// <summary>
    /// If set, overrides the per-skillset items players spawn with.
    /// Can be used to prevent skillset default items in singleplayer.
    /// Server "Loadout" command takes priority over this option.
    /// Defaults to null.
    /// </summary>
    public DefaultLoadoutItem[][] DefaultSkillsetLoadouts { get; set; }

    public bool HasSkillsetLoadoutsOverride => DefaultSkillsetLoadouts != null;

    public EZombieDifficultyAssetPrioritization ZombieDifficultyAssetPrioritization { get; set; }

    /// <summary>
    /// If true, bypasses SafezoneNode no-buildables mode in singleplayer.
    /// </summary>
    public bool ShouldAllowBuildingInSafezonesInSingleplayer { get; set; }

    /// <summary>
    /// Blueprints can test for these tags. (Future extension here?)
    /// </summary>
    public CachingAssetRef[] Tags { get; set; }

    /// <summary>
    /// Fishing rods using per-water-volume fishing spawn table fallback to this table.
    /// </summary>
    public CachingAssetRef DefaultFishSpawnTable
    {
        get
        {
            return _defaultFishSpawnTable;
        }
        set
        {
            _defaultFishSpawnTable = value;
        }
    }

    /// <summary>
    /// If true, this level has assigned fishing spawn tables to water volumes and/or set
    /// the default table. Defaults to false. Enables fishing rods to work on all maps
    /// regardless of when they were designed.
    /// </summary>
    public bool SupportsFishingVolumes { get; set; }

    public DefaultLoadoutItem[] GetSkillsetLoadoutOrNull(EPlayerSkillset skillset)
    {
        if (DefaultSkillsetLoadouts == null)
        {
            return null;
        }
        return DefaultSkillsetLoadouts[(int)skillset];
    }

    public bool isBlueprintBlacklisted(Blueprint blueprint)
    {
        if (craftingBlacklists == null || blueprint == null)
        {
            return false;
        }
        if (resolvedCraftingBlacklists == null)
        {
            resolvedCraftingBlacklists = new List<CraftingBlacklistAsset>(craftingBlacklists.Count);
            foreach (AssetReference<CraftingBlacklistAsset> craftingBlacklist in craftingBlacklists)
            {
                CraftingBlacklistAsset craftingBlacklistAsset = craftingBlacklist.Find();
                if (craftingBlacklistAsset != null)
                {
                    resolvedCraftingBlacklists.Add(craftingBlacklistAsset);
                }
                else
                {
                    Assets.ReportError(this, $"unable to find crafting blacklist {craftingBlacklist}");
                }
            }
        }
        foreach (CraftingBlacklistAsset resolvedCraftingBlacklist in resolvedCraftingBlacklists)
        {
            if (resolvedCraftingBlacklist.isBlueprintBlacklisted(blueprint))
            {
                return true;
            }
        }
        return false;
    }

    public SpawnAsset GetDefaultFishingSpawnTable()
    {
        return _defaultFishSpawnTable.Get<SpawnAsset>();
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        defaultGameMode = p.data.ParseStruct<TypeReference<GameMode>>("Default_Game_Mode");
        if (p.data.TryGetList("Supported_Game_Modes", out var node))
        {
            supportedGameModes = node.ParseListOfStructs<TypeReference<GameMode>>();
        }
        dropshipPrefab = p.data.ParseStruct<MasterBundleReference<GameObject>>("Dropship");
        airdropRef = p.data.ParseStruct<AssetReference<AirdropAsset>>("Airdrop");
        if (p.data.TryGetList("Crafting_Blacklists", out var node2) && node2.Count > 0)
        {
            craftingBlacklists = node2.ParseListOfStructs<AssetReference<CraftingBlacklistAsset>>();
        }
        if (p.data.TryGetList("Weather_Types", out var node3))
        {
            List<SchedulableWeather> list = new List<SchedulableWeather>(node3.Count);
            for (int i = 0; i < node3.Count; i++)
            {
                if (node3[i] is IDatDictionary dictionary)
                {
                    SchedulableWeather item = default(SchedulableWeather);
                    item.assetRef = dictionary.ParseStruct<AssetReference<WeatherAssetBase>>("Asset");
                    item.minFrequency = Mathf.Max(0f, dictionary.ParseFloat("Min_Frequency"));
                    item.maxFrequency = Mathf.Max(0f, dictionary.ParseFloat("Max_Frequency"));
                    item.minDuration = Mathf.Max(0f, dictionary.ParseFloat("Min_Duration"));
                    item.maxDuration = Mathf.Max(0f, dictionary.ParseFloat("Max_Duration"));
                    if (Mathf.Max(item.minDuration, item.maxDuration) > 0.001f)
                    {
                        list.Add(item);
                        continue;
                    }
                    UnturnedLog.warn("Disabling level {0} weather {1} because max duration is zero", this, item.assetRef);
                }
            }
            if (list.Count > 0)
            {
                schedulableWeathers = list.ToArray();
            }
        }
        perpetualWeatherRef = p.data.ParseStruct<AssetReference<WeatherAssetBase>>("Perpetual_Weather_Asset");
        if (p.data.TryGetList("Loading_Screen_Music", out var node4))
        {
            this.loadingScreenMusic = new LoadingScreenMusic[node4.Count];
            for (int j = 0; j < node4.Count; j++)
            {
                if (node4[j] is IDatDictionary datDictionary)
                {
                    LoadingScreenMusic loadingScreenMusic = default(LoadingScreenMusic);
                    loadingScreenMusic.loopRef = datDictionary.ParseStruct<MasterBundleReference<AudioClip>>("Loop");
                    loadingScreenMusic.outroRef = datDictionary.ParseStruct<MasterBundleReference<AudioClip>>("Outro");
                    if (datDictionary.ContainsKey("Loop_Volume"))
                    {
                        loadingScreenMusic.loopVolume = datDictionary.ParseFloat("Loop_Volume");
                    }
                    else
                    {
                        loadingScreenMusic.loopVolume = 1f;
                    }
                    if (datDictionary.ContainsKey("Outro_Volume"))
                    {
                        loadingScreenMusic.outroVolume = datDictionary.ParseFloat("Outro_Volume");
                    }
                    else
                    {
                        loadingScreenMusic.outroVolume = 1f;
                    }
                    this.loadingScreenMusic[j] = loadingScreenMusic;
                }
            }
        }
        if (p.data.TryParseStruct<MasterBundleReference<AudioClip>>("Death_Music", out var value))
        {
            DeathMusicRef = value;
        }
        else
        {
            DeathMusicRef = DefaultDeathMusicRef;
        }
        shouldAnimateBackgroundImage = p.data.ParseBool("Should_Animate_Background_Image");
        if (p.data.ContainsKey("Global_Weather_Mask"))
        {
            globalWeatherMask = p.data.ParseUInt32("Global_Weather_Mask");
        }
        else
        {
            globalWeatherMask = uint.MaxValue;
        }
        if (p.data.TryGetList("Skills", out var node5))
        {
            skillRules = new SkillRule[PlayerSkills.SPECIALITIES][];
            skillRules[0] = new SkillRule[7];
            skillRules[1] = new SkillRule[7];
            skillRules[2] = new SkillRule[8];
            for (int k = 0; k < node5.Count; k++)
            {
                if (!(node5[k] is IDatDictionary datDictionary2))
                {
                    continue;
                }
                string @string = datDictionary2.GetString("Id");
                if (!PlayerSkills.TryParseIndices(@string, out var specialityIndex, out var skillIndex))
                {
                    UnturnedLog.warn("Level {0} unable to parse skill index {1} ({2})", this, k, @string);
                    continue;
                }
                SkillRule skillRule = new SkillRule();
                skillRule.defaultLevel = datDictionary2.ParseInt32("Default_Level");
                if (datDictionary2.ContainsKey("Max_Unlockable_Level"))
                {
                    skillRule.maxUnlockableLevel = datDictionary2.ParseInt32("Max_Unlockable_Level");
                }
                else
                {
                    skillRule.maxUnlockableLevel = -1;
                }
                if (datDictionary2.ContainsKey("Cost_Multiplier"))
                {
                    skillRule.costMultiplier = datDictionary2.ParseFloat("Cost_Multiplier");
                }
                else
                {
                    skillRule.costMultiplier = 1f;
                }
                skillRule.baseCostOverride = datDictionary2.ParseInt32("Base_Cost", -1);
                skillRule.perLevelCostIncreaseOverride = datDictionary2.ParseInt32("Per_Level_Cost_Increase", -1);
                skillRules[specialityIndex][skillIndex] = skillRule;
            }
        }
        minStealthRadius = p.data.ParseFloat("Min_Stealth_Radius");
        fallDamageSpeedThreshold = p.data.ParseFloat("Fall_Damage_Speed_Threshold");
        if (p.data.ContainsKey("Enable_Admin_Faster_Salvage_Duration"))
        {
            enableAdminFasterSalvageDuration = p.data.ParseBool("Enable_Admin_Faster_Salvage_Duration");
        }
        if (p.data.ContainsKey("Has_Clouds"))
        {
            hasClouds = p.data.ParseBool("Has_Clouds");
            if (!hasClouds)
            {
                CloudOverridePrefab = p.data.readMasterBundleReference<GameObject>("CloudOverride_Prefab", p.bundle);
                CloudOverrideParticleSystemPaths = p.data.ParseArrayOfStructs<CloudOverrideParticleSystemsPath>("CloudOverride_ParticleSystems");
            }
        }
        else
        {
            hasClouds = true;
        }
        if (p.data.TryGetDictionary("Skillset_Loadouts", out var node6))
        {
            int num = 11;
            DefaultSkillsetLoadouts = new DefaultLoadoutItem[num][];
            for (int l = 0; l < num; l++)
            {
                EPlayerSkillset ePlayerSkillset = (EPlayerSkillset)l;
                string key = ePlayerSkillset.ToString();
                if (node6.TryGetList(key, out var node7))
                {
                    DefaultSkillsetLoadouts[l] = node7.ParseArrayOfStructs<DefaultLoadoutItem>();
                }
            }
        }
        if (p.data.TryGetList("TerrainColors", out var node8))
        {
            List<TerrainColorRule> list2 = new List<TerrainColorRule>(node8.Count);
            for (int m = 0; m < node8.Count; m++)
            {
                IDatNode node9 = node8[m];
                TerrainColorRule terrainColorRule = new TerrainColorRule();
                if (terrainColorRule.TryParse(node9))
                {
                    bool flag = false;
                    Color[] sKINS = Customization.SKINS;
                    foreach (Color color in sKINS)
                    {
                        Color.RGBToHSV(color, out var H, out var S, out var V);
                        if (terrainColorRule.CompareColors(H, S, V) == TerrainColorRule.EComparisonResult.TooSimilar)
                        {
                            flag = true;
                            string arg = Palette.hex(color);
                            Assets.ReportError(this, $"skipping TerrainColor entry {m} because it blocks default skin color {arg}");
                            break;
                        }
                    }
                    if (!flag)
                    {
                        list2.Add(terrainColorRule);
                    }
                }
                else
                {
                    Assets.ReportError(this, "unable to parse entry in TerrainColors: " + node9.DebugDumpToString());
                }
            }
            if (list2.Count > 0)
            {
                terrainColorRules = list2;
            }
            else
            {
                Assets.ReportError(this, "TerrainColors list is empty");
            }
        }
        UnderwaterFogDensity = p.data.ParseFloat("UnderwaterFogDensity", 0.075f);
        ZombieDifficultyAssetPrioritization = p.data.ParseEnum("ZombieDifficultyAssetPrioritization", EZombieDifficultyAssetPrioritization.NavmeshOverridesTable);
        ShouldAllowBuildingInSafezonesInSingleplayer = p.data.ParseBool("Allow_Building_In_Safezone_In_Singleplayer");
        Tags = p.data.ParseArrayOfStructs<CachingAssetRef>("Tags");
        SupportsFishingVolumes = p.data.ParseBool("Supports_Fishing_Volumes");
        _defaultFishSpawnTable = p.data.ParseAssetRef("Default_Fish_Spawn_Table");
    }

    public string OnGetFishErrorContext()
    {
        return FriendlyName + " level asset";
    }

    public LevelAsset()
    {
        supportedGameModes = new List<TypeReference<GameMode>>();
    }
}
