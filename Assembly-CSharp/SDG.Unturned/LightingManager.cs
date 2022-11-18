using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class LightingManager : SteamCaller
{
    private enum EScheduledWeatherStage
    {
        None,
        Forecast,
        Active,
        PerpetuallyActive
    }

    public static readonly byte SAVEDATA_VERSION = 6;

    public static DayNightUpdated onDayNightUpdated;

    public static DayNightUpdated onDayNightUpdated_ModHook;

    public static TimeOfDayChanged onTimeOfDayChanged;

    public static MoonUpdated onMoonUpdated;

    public static MoonUpdated onMoonUpdated_ModHook;

    public static RainUpdated onRainUpdated;

    public static RainUpdated onRainUpdated_ModHook;

    public static SnowUpdated onSnowUpdated;

    public static SnowUpdated onSnowUpdated_ModHook;

    private static LightingManager manager;

    private static uint _cycle;

    private static uint _time;

    private static uint _offset;

    [Obsolete]
    public static uint rainFrequency;

    [Obsolete]
    public static uint rainDuration;

    [Obsolete]
    public static uint snowFrequency;

    [Obsolete]
    public static uint snowDuration;

    private static LevelAsset.SchedulableWeather[] schedulableWeathers;

    private static EScheduledWeatherStage scheduledWeatherStage;

    private static float scheduledWeatherForecastTimer;

    private static float scheduledWeatherActiveTimer;

    private static AssetReference<WeatherAssetBase> scheduledWeatherRef;

    private static bool shouldTickScheduledWeather;

    private static float loadedWeatherBlendAlpha;

    private static bool isCycled;

    private static bool _isFullMoon;

    private static float lastWind;

    private static float windDelay;

    private static readonly ClientStaticMethod<uint, uint, uint, byte, byte, Guid, float, NetId> SendInitialLightingState = ClientStaticMethod<uint, uint, uint, byte, byte, Guid, float, NetId>.Get(ReceiveInitialLightingState);

    private static readonly ClientStaticMethod<uint> SendLightingCycle = ClientStaticMethod<uint>.Get(ReceiveLightingCycle);

    private static readonly ClientStaticMethod<uint> SendLightingOffset = ClientStaticMethod<uint>.Get(ReceiveLightingOffset);

    private static readonly ClientStaticMethod<byte> SendLightingWind = ClientStaticMethod<byte>.Get(ReceiveLightingWind);

    private static readonly ClientStaticMethod<Guid, float, NetId> SendLightingActiveWeather = ClientStaticMethod<Guid, float, NetId>.Get(ReceiveLightingActiveWeather);

    public static float day => (float)time / (float)cycle;

    public static uint cycle
    {
        get
        {
            return _cycle;
        }
        set
        {
            _offset = Provider.time - (uint)(day * (float)value);
            _cycle = ((value != 0) ? value : 3600u);
            if (Provider.isServer)
            {
                manager.updateLighting();
                SendLightingCycle.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), cycle);
            }
        }
    }

    public static uint time
    {
        get
        {
            return _time;
        }
        set
        {
            value %= cycle;
            _offset = Provider.time - value;
            _time = value;
            manager.updateLighting();
            SendLightingOffset.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), offset);
        }
    }

    public static uint offset => _offset;

    [Obsolete]
    public static bool hasRain => false;

    [Obsolete]
    public static bool hasSnow => false;

    public static ELevelWeatherOverride levelWeatherOverride
    {
        get
        {
            if (Level.info != null && Level.info.configData != null)
            {
                return Level.info.configData.Weather_Override;
            }
            return ELevelWeatherOverride.NONE;
        }
    }

    public static bool isFullMoon
    {
        get
        {
            return _isFullMoon;
        }
        set
        {
            if (value != isFullMoon)
            {
                _isFullMoon = value;
                broadcastMoonUpdated(isFullMoon);
            }
        }
    }

    public static bool isDaytime => day < LevelLighting.bias;

    public static bool isNighttime => !isDaytime;

    private static void broadcastDayNightUpdated(bool isDaytime)
    {
        if (onDayNightUpdated != null)
        {
            onDayNightUpdated(isDaytime);
        }
        if (onDayNightUpdated_ModHook != null)
        {
            onDayNightUpdated_ModHook(isDaytime);
        }
    }

    private static void broadcastMoonUpdated(bool isFullMoon)
    {
        if (onMoonUpdated != null)
        {
            onMoonUpdated(isFullMoon);
        }
        if (onMoonUpdated_ModHook != null)
        {
            onMoonUpdated_ModHook(isFullMoon);
        }
    }

    internal static void broadcastRainUpdated(ELightingRain rain)
    {
        if (onRainUpdated != null)
        {
            onRainUpdated(rain);
        }
        if (onRainUpdated_ModHook != null)
        {
            onRainUpdated_ModHook(rain);
        }
    }

    internal static void broadcastSnowUpdated(ELightingSnow snow)
    {
        if (onSnowUpdated != null)
        {
            onSnowUpdated(snow);
        }
        if (onSnowUpdated_ModHook != null)
        {
            onSnowUpdated_ModHook(snow);
        }
    }

    [Obsolete("Replaced by LevelLighting.GetActiveWeatherAsset")]
    public static WeatherAssetBase getCustomWeather()
    {
        return LevelLighting.GetActiveWeatherAsset();
    }

    private static void SetAndReplicateActiveWeatherAsset(WeatherAssetBase asset, float blendAlpha)
    {
        NetId netId = ((asset != null) ? NetIdRegistry.ClaimBlock(2u) : default(NetId));
        LevelLighting.SetActiveWeatherAsset(asset, blendAlpha, netId);
        Guid arg = asset?.GUID ?? Guid.Empty;
        SendLightingActiveWeather.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), arg, blendAlpha, netId);
    }

    public static bool IsWeatherActive(WeatherAssetBase weatherAsset)
    {
        if (weatherAsset == null)
        {
            throw new ArgumentNullException("weatherAsset");
        }
        return LevelLighting.IsWeatherActive(weatherAsset);
    }

    private static void SetPerpetualWeather(WeatherAssetBase asset, float blendAlpha)
    {
        if (asset == null)
        {
            throw new ArgumentNullException("asset");
        }
        scheduledWeatherStage = EScheduledWeatherStage.PerpetuallyActive;
        scheduledWeatherForecastTimer = 0f;
        scheduledWeatherActiveTimer = 0f;
        scheduledWeatherRef = asset.getReferenceTo<WeatherAssetBase>();
        SetAndReplicateActiveWeatherAsset(asset, blendAlpha);
        shouldTickScheduledWeather = false;
    }

    public static void SetScheduledWeather(WeatherAssetBase weatherAsset, float forecastDuration, float activeDuration)
    {
        if (weatherAsset == null)
        {
            throw new ArgumentNullException("weatherAsset");
        }
        scheduledWeatherStage = EScheduledWeatherStage.Forecast;
        scheduledWeatherForecastTimer = forecastDuration;
        scheduledWeatherActiveTimer = activeDuration;
        scheduledWeatherRef = weatherAsset.getReferenceTo<WeatherAssetBase>();
        shouldTickScheduledWeather = true;
    }

    public static void ActivatePerpetualWeather(WeatherAssetBase asset)
    {
        SetPerpetualWeather(asset, 0f);
    }

    public static bool ForecastWeatherImmediately(WeatherAssetBase weatherAsset)
    {
        if (schedulableWeathers != null)
        {
            LevelAsset.SchedulableWeather[] array = schedulableWeathers;
            for (int i = 0; i < array.Length; i++)
            {
                LevelAsset.SchedulableWeather schedulableWeather = array[i];
                AssetReference<WeatherAssetBase> assetRef = schedulableWeather.assetRef;
                if (assetRef.isReferenceTo(weatherAsset))
                {
                    float activeDuration = UnityEngine.Random.Range(schedulableWeather.minDuration, schedulableWeather.maxDuration) * Provider.modeConfigData.Events.Weather_Duration_Multiplier * (float)cycle;
                    SetScheduledWeather(weatherAsset, 0f, activeDuration);
                    return true;
                }
            }
        }
        return false;
    }

    public static void ResetScheduledWeather()
    {
        if (LevelLighting.GetActiveWeatherAsset() != null)
        {
            SetAndReplicateActiveWeatherAsset(null, 0f);
        }
        scheduledWeatherStage = EScheduledWeatherStage.None;
    }

    public static void DisableWeather()
    {
        if (LevelLighting.GetActiveWeatherAsset() != null)
        {
            SetAndReplicateActiveWeatherAsset(null, 0f);
        }
        scheduledWeatherStage = EScheduledWeatherStage.None;
        shouldTickScheduledWeather = false;
    }

    [Obsolete]
    public void tellLighting(CSteamID steamID, uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, byte rain, byte snow, Guid activeWeatherGuid)
    {
        tellLighting(steamID, serverTime, newCycle, newOffset, moon, wind, activeWeatherGuid, 0f);
    }

    [Obsolete]
    public void tellLighting(CSteamID steamID, uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, Guid activeWeatherGuid, float activeWeatherBlendAlpha)
    {
        ReceiveInitialLightingState(serverTime, newCycle, newOffset, moon, wind, activeWeatherGuid, activeWeatherBlendAlpha, default(NetId));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLighting")]
    public static void ReceiveInitialLightingState(uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, Guid activeWeatherGuid, float activeWeatherBlendAlpha, NetId activeWeatherNetId)
    {
        Provider.time = serverTime;
        _cycle = newCycle;
        _offset = newOffset;
        WeatherAssetBase asset = new AssetReference<WeatherAssetBase>(activeWeatherGuid).Find();
        LevelLighting.SetActiveWeatherAsset(asset, activeWeatherBlendAlpha, activeWeatherNetId);
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(activeWeatherGuid, asset, "ReceiveInitialLightingState");
        }
        manager.updateLighting();
        LevelLighting.moon = moon;
        isCycled = day > LevelLighting.bias;
        isFullMoon = isCycled && LevelLighting.moon == 2;
        broadcastDayNightUpdated(isDaytime);
        if (onTimeOfDayChanged != null)
        {
            onTimeOfDayChanged();
        }
        LevelLighting.wind = (float)(int)wind * 2f;
        Level.isLoadingLighting = false;
    }

    [Obsolete]
    public void askLighting(CSteamID steamID)
    {
    }

    internal static void SendInitialGlobalState(SteamPlayer client)
    {
        if (Level.info.type == ELevelType.SURVIVAL)
        {
            LevelLighting.GetActiveWeatherNetState(out var asset, out var blendAlpha, out var netId);
            Guid arg = asset?.GUID ?? Guid.Empty;
            SendInitialLightingState.Invoke(ENetReliability.Reliable, client.transportConnection, Provider.time, cycle, offset, LevelLighting.moon, MeasurementTool.angleToByte(LevelLighting.wind), arg, blendAlpha, netId);
        }
    }

    [Obsolete]
    public void tellLightingCycle(CSteamID steamID, uint newScale)
    {
        ReceiveLightingCycle(newScale);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLightingCycle")]
    public static void ReceiveLightingCycle(uint newScale)
    {
        _offset = Provider.time - (uint)(day * (float)newScale);
        _cycle = newScale;
        manager.updateLighting();
    }

    [Obsolete]
    public void tellLightingOffset(CSteamID steamID, uint newOffset)
    {
        ReceiveLightingOffset(newOffset);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLightingOffset")]
    public static void ReceiveLightingOffset(uint newOffset)
    {
        _offset = newOffset;
        manager.updateLighting();
    }

    [Obsolete]
    public void tellLightingWind(CSteamID steamID, byte newWind)
    {
        ReceiveLightingWind(newWind);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLightingWind")]
    public static void ReceiveLightingWind(byte newWind)
    {
        LevelLighting.wind = MeasurementTool.byteToAngle(newWind);
    }

    [Obsolete]
    public void tellLightingRain(CSteamID steamID, byte newRain)
    {
    }

    [Obsolete]
    public void tellLightingSnow(CSteamID steamID, byte newSnow)
    {
    }

    [Obsolete]
    public void tellLightingActiveWeather(CSteamID steamID, Guid assetGuid, float blendAlpha)
    {
        ReceiveLightingActiveWeather(assetGuid, blendAlpha, default(NetId));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellLightingActiveWeather")]
    public static void ReceiveLightingActiveWeather(Guid assetGuid, float blendAlpha, NetId netId)
    {
        WeatherAssetBase asset = new AssetReference<WeatherAssetBase>(assetGuid).Find();
        LevelLighting.SetActiveWeatherAsset(asset, blendAlpha, netId);
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(assetGuid, asset, "ReceiveLightingActiveWeather");
        }
    }

    private void updateLighting()
    {
        _time = (Provider.time - offset) % cycle;
        if (Provider.isServer && Time.time - lastWind > windDelay)
        {
            windDelay = UnityEngine.Random.Range(45, 75);
            lastWind = Time.time;
            LevelLighting.wind = UnityEngine.Random.Range(0, 360);
            SendLightingWind.Invoke(ENetReliability.Reliable, Provider.EnumerateClients_Remote(), MeasurementTool.angleToByte(LevelLighting.wind));
        }
        if (day > LevelLighting.bias)
        {
            if (!isCycled)
            {
                isCycled = true;
                if (LevelLighting.moon < LevelLighting.MOON_CYCLES - 1)
                {
                    LevelLighting.moon++;
                    isFullMoon = LevelLighting.moon == 2;
                }
                else
                {
                    LevelLighting.moon = 0;
                    isFullMoon = false;
                }
                broadcastDayNightUpdated(isDaytime: false);
            }
        }
        else if (isCycled)
        {
            isCycled = false;
            isFullMoon = false;
            broadcastDayNightUpdated(isDaytime: true);
        }
        if (onTimeOfDayChanged != null)
        {
            onTimeOfDayChanged();
        }
    }

    private void TickScheduledWeather()
    {
        if (scheduledWeatherStage == EScheduledWeatherStage.None)
        {
            if (schedulableWeathers != null && schedulableWeathers.Length != 0)
            {
                int num = UnityEngine.Random.Range(0, schedulableWeathers.Length);
                LevelAsset.SchedulableWeather schedulableWeather = schedulableWeathers[num];
                WeatherAssetBase weatherAssetBase = schedulableWeather.assetRef.Find();
                if (weatherAssetBase != null)
                {
                    float forecastDuration = UnityEngine.Random.Range(schedulableWeather.minFrequency, schedulableWeather.maxFrequency) * Provider.modeConfigData.Events.Weather_Frequency_Multiplier * (float)cycle;
                    float activeDuration = UnityEngine.Random.Range(schedulableWeather.minDuration, schedulableWeather.maxDuration) * Provider.modeConfigData.Events.Weather_Duration_Multiplier * (float)cycle;
                    SetScheduledWeather(weatherAssetBase, forecastDuration, activeDuration);
                    UnturnedLog.info("Weather {0} forecast in {1} seconds", weatherAssetBase.name, scheduledWeatherForecastTimer);
                }
                else
                {
                    UnturnedLog.warn("Missing level weather asset {0}", schedulableWeather.assetRef);
                    shouldTickScheduledWeather = false;
                }
            }
            else
            {
                UnturnedLog.warn("Disabling scheduled weather because none are available");
                shouldTickScheduledWeather = false;
            }
        }
        else if (scheduledWeatherStage == EScheduledWeatherStage.Forecast)
        {
            scheduledWeatherForecastTimer -= Time.deltaTime;
            if (scheduledWeatherForecastTimer <= 0f)
            {
                WeatherAssetBase weatherAssetBase2 = scheduledWeatherRef.Find();
                if (weatherAssetBase2 != null)
                {
                    scheduledWeatherStage = EScheduledWeatherStage.Active;
                    SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), 0f);
                    UnturnedLog.info("Weather {0} starting for {1} seconds", weatherAssetBase2.name, scheduledWeatherActiveTimer);
                }
                else
                {
                    scheduledWeatherStage = EScheduledWeatherStage.None;
                    UnturnedLog.warn("Missing forecast weather asset {0}", scheduledWeatherRef);
                }
            }
        }
        else
        {
            if (scheduledWeatherStage != EScheduledWeatherStage.Active)
            {
                return;
            }
            scheduledWeatherActiveTimer -= Time.deltaTime;
            if (scheduledWeatherActiveTimer <= 0f)
            {
                WeatherAssetBase weatherAssetBase3 = scheduledWeatherRef.Find();
                if (weatherAssetBase3 != null)
                {
                    scheduledWeatherStage = EScheduledWeatherStage.None;
                    SetAndReplicateActiveWeatherAsset(null, 0f);
                    UnturnedLog.info("Weather {0} ending", weatherAssetBase3.name);
                }
                else
                {
                    scheduledWeatherStage = EScheduledWeatherStage.None;
                    UnturnedLog.warn("Missing active weather asset {0}", scheduledWeatherRef);
                }
            }
        }
    }

    private void InitSchedulableWeathers()
    {
        if (Provider.modeConfigData.Events.Weather_Duration_Multiplier < 0.001f)
        {
            UnturnedLog.info("Disabling scheduled weather because duration multiplier is zero");
            schedulableWeathers = null;
            return;
        }
        LevelAsset asset = Level.getAsset();
        if (asset != null && asset.schedulableWeathers != null)
        {
            schedulableWeathers = asset.schedulableWeathers;
            return;
        }
        if (!LevelLighting.canRain && !LevelLighting.canSnow)
        {
            schedulableWeathers = null;
            return;
        }
        List<LevelAsset.SchedulableWeather> list = new List<LevelAsset.SchedulableWeather>(2);
        if (LevelLighting.canRain)
        {
            float num = Provider.modeConfigData.Events.Rain_Duration_Min * LevelLighting.rainDur;
            float b = Provider.modeConfigData.Events.Rain_Duration_Max * LevelLighting.rainDur;
            if (Mathf.Max(num, b) > 0.001f)
            {
                LevelAsset.SchedulableWeather item = default(LevelAsset.SchedulableWeather);
                item.assetRef = WeatherAssetBase.DEFAULT_RAIN;
                item.minFrequency = Mathf.Max(0f, Provider.modeConfigData.Events.Rain_Frequency_Min * LevelLighting.rainFreq);
                item.maxFrequency = Mathf.Max(0f, Provider.modeConfigData.Events.Rain_Frequency_Max * LevelLighting.rainFreq);
                item.minDuration = Mathf.Max(0f, num);
                item.maxDuration = Mathf.Max(0f, b);
                list.Add(item);
            }
            else
            {
                UnturnedLog.info("Disabling legacy rain because max duration is zero");
            }
        }
        if (LevelLighting.canSnow)
        {
            float num2 = Provider.modeConfigData.Events.Snow_Duration_Min * LevelLighting.snowDur;
            float b2 = Provider.modeConfigData.Events.Snow_Duration_Max * LevelLighting.snowDur;
            if (Mathf.Max(num2, b2) > 0.001f)
            {
                LevelAsset.SchedulableWeather item2 = default(LevelAsset.SchedulableWeather);
                item2.assetRef = WeatherAssetBase.DEFAULT_SNOW;
                item2.minFrequency = Mathf.Max(0f, Provider.modeConfigData.Events.Snow_Frequency_Min * LevelLighting.snowFreq);
                item2.maxFrequency = Mathf.Max(0f, Provider.modeConfigData.Events.Snow_Frequency_Max * LevelLighting.snowFreq);
                item2.minDuration = Mathf.Max(0f, num2);
                item2.maxDuration = Mathf.Max(0f, b2);
                list.Add(item2);
            }
            else
            {
                UnturnedLog.info("Disabling legacy snow because max duration is zero");
            }
        }
        schedulableWeathers = list.ToArray();
    }

    private bool InitPerpetualWeather()
    {
        LevelAsset asset = Level.getAsset();
        AssetReference<WeatherAssetBase> assetReference;
        if (asset != null && asset.perpetualWeatherRef.isValid)
        {
            assetReference = asset.perpetualWeatherRef;
        }
        else
        {
            switch (levelWeatherOverride)
            {
            case ELevelWeatherOverride.RAIN:
                assetReference = WeatherAssetBase.DEFAULT_RAIN;
                break;
            case ELevelWeatherOverride.SNOW:
                assetReference = WeatherAssetBase.DEFAULT_SNOW;
                break;
            default:
                return false;
            }
        }
        WeatherAssetBase weatherAssetBase = assetReference.Find();
        if (weatherAssetBase != null)
        {
            UnturnedLog.info("Level perpetual weather override {0}", weatherAssetBase.name);
            SetPerpetualWeather(weatherAssetBase, 1f);
            return true;
        }
        UnturnedLog.warn("Missing level perpetual weather asset {0}", assetReference);
        return false;
    }

    private void InitLoadedWeather()
    {
        if (scheduledWeatherStage == EScheduledWeatherStage.Forecast)
        {
            WeatherAssetBase weatherAssetBase = scheduledWeatherRef.Find();
            if (weatherAssetBase != null)
            {
                UnturnedLog.info("Loaded weather {0} forecast in {1} seconds", weatherAssetBase.name, scheduledWeatherForecastTimer);
            }
            else
            {
                scheduledWeatherStage = EScheduledWeatherStage.None;
                UnturnedLog.warn("Missing loaded forecast weather asset {0}", scheduledWeatherRef);
            }
        }
        else if (scheduledWeatherStage == EScheduledWeatherStage.Active)
        {
            WeatherAssetBase weatherAssetBase2 = scheduledWeatherRef.Find();
            if (weatherAssetBase2 != null)
            {
                SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), loadedWeatherBlendAlpha);
                UnturnedLog.info("Loaded weather {0} with global alpha {1} ending in {2} seconds", weatherAssetBase2.name, loadedWeatherBlendAlpha, scheduledWeatherActiveTimer);
            }
            else
            {
                scheduledWeatherStage = EScheduledWeatherStage.None;
                UnturnedLog.warn("Missing loaded active weather asset {0}", scheduledWeatherRef);
            }
        }
        else if (scheduledWeatherStage == EScheduledWeatherStage.PerpetuallyActive)
        {
            WeatherAssetBase weatherAssetBase3 = scheduledWeatherRef.Find();
            if (weatherAssetBase3 != null)
            {
                SetAndReplicateActiveWeatherAsset(scheduledWeatherRef.Find(), loadedWeatherBlendAlpha);
                UnturnedLog.info("Loaded perpetual weather {0} with global alpha {1}", weatherAssetBase3.name, loadedWeatherBlendAlpha);
                shouldTickScheduledWeather = false;
            }
            else
            {
                scheduledWeatherStage = EScheduledWeatherStage.None;
                UnturnedLog.warn("Missing loaded perpetual weather asset {0}", scheduledWeatherRef);
            }
        }
    }

    private void onPrePreLevelLoaded(int level)
    {
        onDayNightUpdated = null;
        onTimeOfDayChanged = null;
        onMoonUpdated = null;
        onRainUpdated = null;
        onSnowUpdated = null;
    }

    private void onLevelLoaded(int level)
    {
        if (level <= Level.BUILD_INDEX_SETUP)
        {
            return;
        }
        scheduledWeatherStage = EScheduledWeatherStage.None;
        scheduledWeatherForecastTimer = -1f;
        scheduledWeatherActiveTimer = -1f;
        scheduledWeatherRef = AssetReference<WeatherAssetBase>.invalid;
        InitSchedulableWeathers();
        shouldTickScheduledWeather = schedulableWeathers != null && schedulableWeathers.Length != 0;
        LevelLighting.rainyness = ELightingRain.NONE;
        LevelLighting.snowyness = ELightingSnow.NONE;
        if (Level.info != null && Level.info.type != 0)
        {
            _cycle = 3600u;
            _offset = 0u;
            if (Level.info.type == ELevelType.HORDE)
            {
                _time = (uint)((LevelLighting.bias + (1f - LevelLighting.bias) / 2f) * (float)cycle);
                _isFullMoon = true;
            }
            else if (Level.info.type == ELevelType.ARENA)
            {
                _time = (uint)(LevelLighting.transition * (float)cycle);
                _isFullMoon = false;
            }
            windDelay = UnityEngine.Random.Range(45, 75);
            LevelLighting.wind = UnityEngine.Random.Range(0, 360);
            InitPerpetualWeather();
            Level.isLoadingLighting = false;
            return;
        }
        _cycle = 3600u;
        _time = 0u;
        _offset = 0u;
        _isFullMoon = false;
        isCycled = false;
        broadcastDayNightUpdated(isDaytime: true);
        if (onTimeOfDayChanged != null)
        {
            onTimeOfDayChanged();
        }
        windDelay = UnityEngine.Random.Range(45, 75);
        LevelLighting.wind = UnityEngine.Random.Range(0, 360);
        if (Provider.isServer)
        {
            load();
            if (!InitPerpetualWeather())
            {
                InitLoadedWeather();
            }
            updateLighting();
            Level.isLoadingLighting = false;
        }
    }

    private void Update()
    {
        if (Level.isLoaded && Level.info != null)
        {
            if (Level.isEditor)
            {
                LevelLighting.updateLighting();
            }
            else if (Level.info.type == ELevelType.SURVIVAL)
            {
                updateLighting();
            }
            LevelLighting.tickCustomWeatherBlending(uint.MaxValue);
            if (Provider.isServer && shouldTickScheduledWeather)
            {
                TickScheduledWeather();
            }
        }
    }

    private void Start()
    {
        manager = this;
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onPrePreLevelLoaded));
        Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
    }

    public static void load()
    {
        bool flag = true;
        if (LevelSavedata.fileExists("/Lighting.dat"))
        {
            River river = LevelSavedata.openRiver("/Lighting.dat", isReading: true);
            byte b = river.readByte();
            if (b > 0)
            {
                _cycle = river.readUInt32();
                if (_cycle < 1)
                {
                    _cycle = 3600u;
                }
                _time = river.readUInt32();
                if (b > 1 && b < 5)
                {
                    river.readUInt32();
                    river.readUInt32();
                    river.readBoolean();
                    river.readByte();
                }
                if (b > 2 && b < 5)
                {
                    river.readUInt32();
                    river.readUInt32();
                    river.readBoolean();
                    river.readByte();
                }
                if (b > 3)
                {
                    scheduledWeatherStage = (EScheduledWeatherStage)river.readByte();
                    scheduledWeatherForecastTimer = river.readSingle();
                    scheduledWeatherActiveTimer = river.readSingle();
                    scheduledWeatherRef = new AssetReference<WeatherAssetBase>(river.readGUID());
                    if (b > 5)
                    {
                        loadedWeatherBlendAlpha = river.readSingle();
                    }
                    else
                    {
                        loadedWeatherBlendAlpha = 0f;
                    }
                }
                _offset = Provider.time - time;
                flag = false;
            }
            river.closeRiver();
        }
        if (flag)
        {
            _time = (uint)((float)cycle * LevelLighting.transition);
            _offset = Provider.time - time;
        }
    }

    public static void save()
    {
        River river = LevelSavedata.openRiver("/Lighting.dat", isReading: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeUInt32(cycle);
        river.writeUInt32(time);
        river.writeByte((byte)scheduledWeatherStage);
        river.writeSingle(scheduledWeatherForecastTimer);
        river.writeSingle(scheduledWeatherActiveTimer);
        river.writeGUID(scheduledWeatherRef.GUID);
        river.writeSingle(LevelLighting.GetActiveWeatherGlobalBlendAlpha());
        river.closeRiver();
    }
}
