using System;
using System.Collections.Generic;
using SDG.Framework.Debug;
using SDG.Framework.IO.FormattedFiles;
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

    public InspectableList<TypeReference<GameMode>> supportedGameModes;

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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        defaultGameMode = reader.readValue<TypeReference<GameMode>>("Default_Game_Mode");
        int num = reader.readArrayLength("Supported_Game_Modes");
        for (int i = 0; i < num; i++)
        {
            supportedGameModes.Add(reader.readValue<TypeReference<GameMode>>(i));
        }
        dropshipPrefab = reader.readValue<MasterBundleReference<GameObject>>("Dropship");
        airdropRef = reader.readValue<AssetReference<AirdropAsset>>("Airdrop");
        int num2 = reader.readArrayLength("Crafting_Blacklists");
        if (num2 > 0)
        {
            craftingBlacklists = new List<AssetReference<CraftingBlacklistAsset>>(num2);
            for (int j = 0; j < num2; j++)
            {
                craftingBlacklists.Add(reader.readValue<AssetReference<CraftingBlacklistAsset>>(j));
            }
        }
        int num3 = reader.readArrayLength("Weather_Types");
        if (num3 > 0)
        {
            List<SchedulableWeather> list = new List<SchedulableWeather>(num3);
            for (int k = 0; k < num3; k++)
            {
                SchedulableWeather item = default(SchedulableWeather);
                IFormattedFileReader formattedFileReader = reader.readObject(k);
                item.assetRef = formattedFileReader.readValue<AssetReference<WeatherAssetBase>>("Asset");
                item.minFrequency = Mathf.Max(0f, formattedFileReader.readValue<float>("Min_Frequency"));
                item.maxFrequency = Mathf.Max(0f, formattedFileReader.readValue<float>("Max_Frequency"));
                item.minDuration = Mathf.Max(0f, formattedFileReader.readValue<float>("Min_Duration"));
                item.maxDuration = Mathf.Max(0f, formattedFileReader.readValue<float>("Max_Duration"));
                if (Mathf.Max(item.minDuration, item.maxDuration) > 0.001f)
                {
                    list.Add(item);
                    continue;
                }
                UnturnedLog.warn("Disabling level {0} weather {1} because max duration is zero", this, item.assetRef);
            }
            if (list.Count > 0)
            {
                schedulableWeathers = list.ToArray();
            }
        }
        perpetualWeatherRef = reader.readValue<AssetReference<WeatherAssetBase>>("Perpetual_Weather_Asset");
        int num4 = reader.readArrayLength("Loading_Screen_Music");
        if (num4 > 0)
        {
            this.loadingScreenMusic = new LoadingScreenMusic[num4];
            for (int l = 0; l < num4; l++)
            {
                IFormattedFileReader formattedFileReader2 = reader.readObject(l);
                LoadingScreenMusic loadingScreenMusic = default(LoadingScreenMusic);
                loadingScreenMusic.loopRef = formattedFileReader2.readValue<MasterBundleReference<AudioClip>>("Loop");
                loadingScreenMusic.outroRef = formattedFileReader2.readValue<MasterBundleReference<AudioClip>>("Outro");
                if (formattedFileReader2.containsKey("Loop_Volume"))
                {
                    loadingScreenMusic.loopVolume = formattedFileReader2.readValue<float>("Loop_Volume");
                }
                else
                {
                    loadingScreenMusic.loopVolume = 1f;
                }
                if (formattedFileReader2.containsKey("Outro_Volume"))
                {
                    loadingScreenMusic.outroVolume = formattedFileReader2.readValue<float>("Outro_Volume");
                }
                else
                {
                    loadingScreenMusic.outroVolume = 1f;
                }
                this.loadingScreenMusic[l] = loadingScreenMusic;
            }
        }
        shouldAnimateBackgroundImage = reader.readValue<bool>("Should_Animate_Background_Image");
        if (reader.containsKey("Global_Weather_Mask"))
        {
            globalWeatherMask = reader.readValue<uint>("Global_Weather_Mask");
        }
        else
        {
            globalWeatherMask = uint.MaxValue;
        }
        int num5 = reader.readArrayLength("Skills");
        if (num5 > 0)
        {
            skillRules = new SkillRule[PlayerSkills.SPECIALITIES][];
            skillRules[0] = new SkillRule[7];
            skillRules[1] = new SkillRule[7];
            skillRules[2] = new SkillRule[8];
            for (int m = 0; m < num5; m++)
            {
                IFormattedFileReader formattedFileReader3 = reader.readObject(m);
                string text = formattedFileReader3.readValue("Id");
                if (!PlayerSkills.TryParseIndices(text, out var specialityIndex, out var skillIndex))
                {
                    UnturnedLog.warn("Level {0} unable to parse skill index {1} ({2})", this, m, text);
                    continue;
                }
                SkillRule skillRule = new SkillRule();
                skillRule.defaultLevel = formattedFileReader3.readValue<int>("Default_Level");
                if (formattedFileReader3.containsKey("Max_Unlockable_Level"))
                {
                    skillRule.maxUnlockableLevel = formattedFileReader3.readValue<int>("Max_Unlockable_Level");
                }
                else
                {
                    skillRule.maxUnlockableLevel = -1;
                }
                if (formattedFileReader3.containsKey("Cost_Multiplier"))
                {
                    skillRule.costMultiplier = formattedFileReader3.readValue<float>("Cost_Multiplier");
                }
                else
                {
                    skillRule.costMultiplier = 1f;
                }
                skillRules[specialityIndex][skillIndex] = skillRule;
            }
        }
        minStealthRadius = reader.readValue<float>("Min_Stealth_Radius");
        fallDamageSpeedThreshold = reader.readValue<float>("Fall_Damage_Speed_Threshold");
        if (reader.containsKey("Enable_Admin_Faster_Salvage_Duration"))
        {
            enableAdminFasterSalvageDuration = reader.readValue<bool>("Enable_Admin_Faster_Salvage_Duration");
        }
        if (reader.containsKey("Has_Clouds"))
        {
            hasClouds = reader.readValue<bool>("Has_Clouds");
        }
        else
        {
            hasClouds = true;
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Default_Game_Mode", defaultGameMode);
        writer.beginArray("Supported_Game_Modes");
        foreach (TypeReference<GameMode> supportedGameMode in supportedGameModes)
        {
            writer.writeValue(supportedGameMode);
        }
        writer.endArray();
        writer.writeValue("Dropship", dropshipPrefab);
        writer.writeValue("Airdrop", airdropRef);
        writer.writeValue("Min_Stealth_Radius", minStealthRadius);
        writer.writeValue("Fall_Damage_Speed_Threshold", fallDamageSpeedThreshold);
    }

    public LevelAsset()
    {
        supportedGameModes = new InspectableList<TypeReference<GameMode>>();
    }

    public LevelAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        supportedGameModes = new InspectableList<TypeReference<GameMode>>();
    }

    public LevelAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        supportedGameModes = new InspectableList<TypeReference<GameMode>>();
    }
}
