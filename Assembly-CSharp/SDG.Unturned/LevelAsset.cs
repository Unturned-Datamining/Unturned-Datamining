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
    }

    public static AssetReference<LevelAsset> defaultLevel = new AssetReference<LevelAsset>(new Guid("12dc9fdbe9974022afd21158ad54b76a"));

    public TypeReference<GameMode> defaultGameMode;

    public List<TypeReference<GameMode>> supportedGameModes;

    public MasterBundleReference<GameObject> dropshipPrefab;

    public AssetReference<AirdropAsset> airdropRef;

    public float minStealthRadius;

    public float fallDamageSpeedThreshold;

    public bool enableAdminFasterSalvageDuration = true;

    public List<AssetReference<CraftingBlacklistAsset>> craftingBlacklists;

    private List<CraftingBlacklistAsset> resolvedCraftingBlacklists;

    public SchedulableWeather[] schedulableWeathers;

    public AssetReference<WeatherAssetBase> perpetualWeatherRef;

    public LoadingScreenMusic[] loadingScreenMusic;

    public bool shouldAnimateBackgroundImage;

    public uint globalWeatherMask;

    public SkillRule[][] skillRules;

    public bool hasClouds = true;

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
                    Assets.reportError(this, $"unable to find crafting blacklist {craftingBlacklist}");
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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        defaultGameMode = data.ParseStruct<TypeReference<GameMode>>("Default_Game_Mode");
        if (data.TryGetList("Supported_Game_Modes", out var node))
        {
            supportedGameModes = node.ParseListOfStructs<TypeReference<GameMode>>();
        }
        dropshipPrefab = data.ParseStruct<MasterBundleReference<GameObject>>("Dropship");
        airdropRef = data.ParseStruct<AssetReference<AirdropAsset>>("Airdrop");
        if (data.TryGetList("Crafting_Blacklists", out var node2) && node2.Count > 0)
        {
            craftingBlacklists = node2.ParseListOfStructs<AssetReference<CraftingBlacklistAsset>>();
        }
        if (data.TryGetList("Weather_Types", out var node3))
        {
            List<SchedulableWeather> list = new List<SchedulableWeather>(node3.Count);
            for (int i = 0; i < node3.Count; i++)
            {
                if (node3[i] is DatDictionary datDictionary)
                {
                    SchedulableWeather item = default(SchedulableWeather);
                    item.assetRef = datDictionary.ParseStruct<AssetReference<WeatherAssetBase>>("Asset");
                    item.minFrequency = Mathf.Max(0f, datDictionary.ParseFloat("Min_Frequency"));
                    item.maxFrequency = Mathf.Max(0f, datDictionary.ParseFloat("Max_Frequency"));
                    item.minDuration = Mathf.Max(0f, datDictionary.ParseFloat("Min_Duration"));
                    item.maxDuration = Mathf.Max(0f, datDictionary.ParseFloat("Max_Duration"));
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
        perpetualWeatherRef = data.ParseStruct<AssetReference<WeatherAssetBase>>("Perpetual_Weather_Asset");
        if (data.TryGetList("Loading_Screen_Music", out var node4))
        {
            this.loadingScreenMusic = new LoadingScreenMusic[node4.Count];
            for (int j = 0; j < node4.Count; j++)
            {
                if (node4[j] is DatDictionary datDictionary2)
                {
                    LoadingScreenMusic loadingScreenMusic = default(LoadingScreenMusic);
                    loadingScreenMusic.loopRef = datDictionary2.ParseStruct<MasterBundleReference<AudioClip>>("Loop");
                    loadingScreenMusic.outroRef = datDictionary2.ParseStruct<MasterBundleReference<AudioClip>>("Outro");
                    if (datDictionary2.ContainsKey("Loop_Volume"))
                    {
                        loadingScreenMusic.loopVolume = datDictionary2.ParseFloat("Loop_Volume");
                    }
                    else
                    {
                        loadingScreenMusic.loopVolume = 1f;
                    }
                    if (datDictionary2.ContainsKey("Outro_Volume"))
                    {
                        loadingScreenMusic.outroVolume = datDictionary2.ParseFloat("Outro_Volume");
                    }
                    else
                    {
                        loadingScreenMusic.outroVolume = 1f;
                    }
                    this.loadingScreenMusic[j] = loadingScreenMusic;
                }
            }
        }
        shouldAnimateBackgroundImage = data.ParseBool("Should_Animate_Background_Image");
        if (data.ContainsKey("Global_Weather_Mask"))
        {
            globalWeatherMask = data.ParseUInt32("Global_Weather_Mask");
        }
        else
        {
            globalWeatherMask = uint.MaxValue;
        }
        if (data.TryGetList("Skills", out var node5))
        {
            skillRules = new SkillRule[PlayerSkills.SPECIALITIES][];
            skillRules[0] = new SkillRule[7];
            skillRules[1] = new SkillRule[7];
            skillRules[2] = new SkillRule[8];
            for (int k = 0; k < node5.Count; k++)
            {
                if (!(node5[k] is DatDictionary datDictionary3))
                {
                    continue;
                }
                string @string = datDictionary3.GetString("Id");
                if (!PlayerSkills.TryParseIndices(@string, out var specialityIndex, out var skillIndex))
                {
                    UnturnedLog.warn("Level {0} unable to parse skill index {1} ({2})", this, k, @string);
                    continue;
                }
                SkillRule skillRule = new SkillRule();
                skillRule.defaultLevel = datDictionary3.ParseInt32("Default_Level");
                if (datDictionary3.ContainsKey("Max_Unlockable_Level"))
                {
                    skillRule.maxUnlockableLevel = datDictionary3.ParseInt32("Max_Unlockable_Level");
                }
                else
                {
                    skillRule.maxUnlockableLevel = -1;
                }
                if (datDictionary3.ContainsKey("Cost_Multiplier"))
                {
                    skillRule.costMultiplier = datDictionary3.ParseFloat("Cost_Multiplier");
                }
                else
                {
                    skillRule.costMultiplier = 1f;
                }
                skillRules[specialityIndex][skillIndex] = skillRule;
            }
        }
        minStealthRadius = data.ParseFloat("Min_Stealth_Radius");
        fallDamageSpeedThreshold = data.ParseFloat("Fall_Damage_Speed_Threshold");
        if (data.ContainsKey("Enable_Admin_Faster_Salvage_Duration"))
        {
            enableAdminFasterSalvageDuration = data.ParseBool("Enable_Admin_Faster_Salvage_Duration");
        }
        if (data.ContainsKey("Has_Clouds"))
        {
            hasClouds = data.ParseBool("Has_Clouds");
        }
        else
        {
            hasClouds = true;
        }
    }

    public LevelAsset()
    {
        supportedGameModes = new List<TypeReference<GameMode>>();
    }
}