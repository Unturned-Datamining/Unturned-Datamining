using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

namespace SDG.Unturned;

public class LevelLighting
{
    private class CustomWeatherInstance
    {
        public WeatherAssetBase asset;

        public NetId netId;

        /// <summary>
        /// [0, 1] used to avoid invoking BlendAlphaChanged every frame.
        /// Compared against globalBlendAlpha not taking into account local volume.
        /// </summary>
        public float eventBlendAlpha;

        public GameObject gameObject;

        public WeatherComponentBase component;

        public void initialize()
        {
            gameObject = new GameObject(asset.name);
            gameObject.transform.parent = lighting;
            gameObject.transform.localPosition = Vector3.zero;
            component = gameObject.AddComponent(asset.componentType) as WeatherComponentBase;
            component.asset = asset;
            component.netId = netId;
            if (!netId.IsNull())
            {
                NetIdRegistry.Assign(netId, component);
            }
            component.InitializeWeather();
            if (asset.hasLightning && !netId.IsNull())
            {
                LightningWeatherComponent lightningWeatherComponent = gameObject.AddComponent<LightningWeatherComponent>();
                lightningWeatherComponent.weatherComponent = component;
                lightningWeatherComponent.netId = netId + 1u;
                NetIdRegistry.Assign(lightningWeatherComponent.netId, lightningWeatherComponent);
            }
        }

        public void teardown()
        {
            if (component != null)
            {
                component.PreDestroyWeather();
                component = null;
            }
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
                gameObject = null;
            }
            if (!netId.IsNull())
            {
                NetIdRegistry.Release(netId);
                netId.Clear();
            }
        }
    }

    public delegate void IsSeaChangedHandler(bool isSea);

    private static bool _editorWantsUnderwaterEffects;

    private static bool _editorWantsWaterSurface;

    public static readonly byte SAVEDATA_VERSION;

    public static readonly byte MOON_CYCLES;

    [Obsolete("Never used?")]
    public static readonly float CLOUDS;

    public static readonly float AUDIO_MIN;

    public static readonly float AUDIO_MAX;

    private static readonly Color FOAM_DAWN;

    private static readonly Color FOAM_MIDDAY;

    private static readonly Color FOAM_DUSK;

    private static readonly Color FOAM_MIDNIGHT;

    private static readonly float SPECULAR_DAWN;

    private static readonly float SPECULAR_MIDDAY;

    private static readonly float SPECULAR_DUSK;

    private static readonly float SPECULAR_MIDNIGHT;

    private static readonly float PITCH_DARK_WATER_BLEND;

    private static readonly float REFLECTION_DAWN;

    private static readonly float REFLECTION_MIDDAY;

    private static readonly float REFLECTION_DUSK;

    private static readonly float REFLECTION_MIDNIGHT;

    internal static readonly Color NIGHTVISION_MILITARY;

    internal static readonly Color NIGHTVISION_CIVILIAN;

    private static float _azimuth;

    private static float _transition;

    private static float _bias;

    private static float _fade;

    private static float _time;

    private static float _wind;

    /// <summary>
    /// Kept for backwards compatibility with mod hooks, plugins, and events.
    /// </summary>
    public static ELightingRain rainyness;

    /// <summary>
    /// Kept for backwards compatibility with mod hooks, plugins, and events.
    /// </summary>
    public static ELightingSnow snowyness;

    private static List<CustomWeatherInstance> customWeatherInstances;

    private static CustomWeatherInstance activeCustomWeather;

    private static LightingInfo[] _times;

    private static float _seaLevel;

    private static float _snowLevel;

    public static float rainFreq;

    public static float rainDur;

    public static float snowFreq;

    public static float snowDur;

    public static bool canRain;

    public static bool canSnow;

    private static ELightingVision _vision;

    public static Color nightvisionColor;

    public static float nightvisionFogIntensity;

    protected static bool _isSea;

    private static Material skybox;

    private static Transform lighting;

    private static Rain puddles;

    private static float auroraBorealisCurrentIntensity;

    private static float auroraBorealisTargetIntensity;

    private static Color skyboxGround;

    private static Color cloudColor;

    private static Color cloudRimColor;

    private static bool skyboxNeedsReflectionUpdate;

    private static float lastSkyboxReflectionUpdate;

    private static Color particleLightingColor;

    private static Color raysColor;

    private static float raysIntensity;

    /// <summary>
    /// Level designed target fog color.
    /// </summary>
    private static Color levelFogColor;

    /// <summary>
    /// Level designed target fog intensity.
    /// </summary>
    private static float levelFogIntensity;

    /// <summary>
    /// Level designed target atmospheric fog intensity.
    /// </summary>
    private static float levelAtmosphericFog;

    public static Transform sun;

    private static Light sunLight;

    private static Transform sunFlare;

    private static AudioSource _effectAudio;

    private static AudioSource _dayAudio;

    private static AudioSource _nightAudio;

    private static AudioSource _waterAudio;

    private static AudioSource _windAudio;

    private static AudioSource _belowAudio;

    private static float currentAudioVolume;

    private static float targetAudioVolume;

    private static float nextAudioVolumeChangeTime;

    private static float dayVolume;

    private static float nightVolume;

    private static Camera reflectionCamera;

    private static Cubemap reflectionMap;

    private static Cubemap reflectionMapVision;

    private static int reflectionIndex;

    private static int reflectionIndexVision;

    private static bool isReflectionBuilding;

    private static bool isReflectionBuildingVision;

    private static bool _isSkyboxReflectionEnabled;

    private static Transform _bubbles;

    private static WindZone _windZone;

    private static WaterVolume legacyWater;

    private static Transform legacyWaterTransform;

    private static Transform[] moons;

    private static byte _moon;

    private static bool init;

    private static Vector3 localPoint;

    private static float localWindOverride;

    private static IAmbianceNode localEffectNode;

    private static EffectAsset currentEffectAsset;

    private static bool localPlayingEffect;

    private static bool localBlendingLight;

    private static float localLightingBlend;

    private static bool localBlendingFog;

    private static float localFogBlend;

    private static Color localFogColor;

    private static float localFogIntensity;

    private static float localAtmosphericFog;

    private static int tickedWeatherBlendingFrame;

    private static float cachedAtmosphericFog;

    private static Color cachedAlphaParticleLightingColor;

    internal static Color cachedSkyColor;

    internal static Color cachedEquatorColor;

    internal static Color cachedGroundColor;

    private static bool skyboxNeedsColorUpdate;

    public static bool enableUnderwaterEffects
    {
        get
        {
            if (Level.isEditor)
            {
                return _editorWantsUnderwaterEffects;
            }
            return true;
        }
    }

    public static bool EditorWantsUnderwaterEffects
    {
        get
        {
            return _editorWantsUnderwaterEffects;
        }
        set
        {
            _editorWantsUnderwaterEffects = value;
            ConvenientSavedata.get().write("EditorWantsUnderwaterEffects", value);
        }
    }

    public static bool EditorWantsWaterSurface
    {
        get
        {
            return _editorWantsWaterSurface;
        }
        set
        {
            if (_editorWantsWaterSurface != value)
            {
                _editorWantsWaterSurface = value;
                ConvenientSavedata.get().write("EditorWantsWaterSurfaceVisible", value);
                if (Level.isEditor)
                {
                    VolumeManager<WaterVolume, WaterVolumeManager>.Get().ForceUpdateEditorVisibility();
                }
            }
        }
    }

    public static float azimuth
    {
        get
        {
            return _azimuth;
        }
        set
        {
            _azimuth = value;
            updateLighting();
        }
    }

    public static float transition => _transition;

    public static float bias
    {
        get
        {
            return _bias;
        }
        set
        {
            _bias = value;
            if (bias < 1f - bias)
            {
                _transition = bias / 2f * fade;
            }
            else
            {
                _transition = (1f - bias) / 2f * fade;
            }
            updateLighting();
        }
    }

    public static float fade
    {
        get
        {
            return _fade;
        }
        set
        {
            _fade = value;
            if (bias < 1f - bias)
            {
                _transition = bias / 2f * fade;
            }
            else
            {
                _transition = (1f - bias) / 2f * fade;
            }
            updateLighting();
        }
    }

    public static float time
    {
        get
        {
            return _time;
        }
        set
        {
            float num = Mathf.Min(Mathf.Abs(value - _time), value + 1f - _time);
            skyboxNeedsReflectionUpdate = skyboxNeedsReflectionUpdate || num > 0.05f;
            _time = value;
            updateLighting();
        }
    }

    public static float wind
    {
        get
        {
            return _wind;
        }
        set
        {
            _wind = value;
        }
    }

    [Obsolete]
    public static float christmasyness { get; private set; }

    [Obsolete]
    public static float blizzardyness { get; private set; }

    [Obsolete]
    public static float mistyness { get; private set; }

    [Obsolete]
    public static float drizzlyness { get; private set; }

    /// <summary>
    /// Hash of lighting config.
    /// Prevents using the level editor to make night time look like day.
    /// </summary>
    public static byte[] hash { get; private set; }

    public static LightingInfo[] times => _times;

    public static float seaLevel
    {
        get
        {
            return _seaLevel;
        }
        set
        {
            _seaLevel = value;
            UpdateBubblesActive();
            UpdateLegacyWaterTransform();
        }
    }

    public static float snowLevel
    {
        get
        {
            return _snowLevel;
        }
        set
        {
            _snowLevel = value;
        }
    }

    public static ELightingVision vision
    {
        get
        {
            return _vision;
        }
        set
        {
            if (value != _vision)
            {
                _vision = value;
                skyboxNeedsReflectionUpdate = true;
            }
        }
    }

    public static bool isSea
    {
        get
        {
            return _isSea;
        }
        protected set
        {
            if (isSea != value)
            {
                _isSea = value;
                LevelLighting.isSeaChanged?.Invoke(isSea);
                skyboxNeedsReflectionUpdate = true;
            }
        }
    }

    public static Color skyboxSky { get; private set; }

    public static Color skyboxEquator { get; private set; }

    public static AudioSource effectAudio => _effectAudio;

    public static AudioSource dayAudio => _dayAudio;

    public static AudioSource nightAudio => _nightAudio;

    public static AudioSource waterAudio => _waterAudio;

    public static AudioSource windAudio => _windAudio;

    public static AudioSource belowAudio => _belowAudio;

    public static bool isSkyboxReflectionEnabled
    {
        get
        {
            return _isSkyboxReflectionEnabled;
        }
        set
        {
            _isSkyboxReflectionEnabled = value;
            updateSkyboxReflections();
        }
    }

    public static Transform bubbles => _bubbles;

    public static WindZone windZone => _windZone;

    public static byte moon
    {
        get
        {
            return _moon;
        }
        set
        {
            _moon = value;
        }
    }

    public static event IsSeaChangedHandler isSeaChanged;

    private static CustomWeatherInstance FindWeatherInstanceByAsset(WeatherAssetBase asset)
    {
        foreach (CustomWeatherInstance customWeatherInstance in customWeatherInstances)
        {
            if (customWeatherInstance.asset == asset)
            {
                return customWeatherInstance;
            }
        }
        return null;
    }

    [Obsolete("Renamed to GetActiveWeatherAsset")]
    public static WeatherAssetBase getCustomWeather()
    {
        return GetActiveWeatherAsset();
    }

    public static WeatherAssetBase GetActiveWeatherAsset()
    {
        if (activeCustomWeather == null)
        {
            return null;
        }
        return activeCustomWeather.asset;
    }

    public static float GetActiveWeatherGlobalBlendAlpha()
    {
        if (activeCustomWeather == null)
        {
            return 0f;
        }
        return activeCustomWeather.component.globalBlendAlpha;
    }

    public static bool GetActiveWeatherNetState(out WeatherAssetBase asset, out float blendAlpha, out NetId netId)
    {
        if (activeCustomWeather != null)
        {
            asset = activeCustomWeather.asset;
            blendAlpha = activeCustomWeather.component.globalBlendAlpha;
            netId = activeCustomWeather.component.GetNetId();
            return true;
        }
        asset = null;
        blendAlpha = 0f;
        netId = default(NetId);
        return false;
    }

    public static bool IsWeatherActive(WeatherAssetBase asset)
    {
        if (activeCustomWeather != null)
        {
            return activeCustomWeather.asset == asset;
        }
        return false;
    }

    public static bool IsWeatherTransitioningIn(WeatherAssetBase asset)
    {
        if (activeCustomWeather != null && !activeCustomWeather.component.isFullyTransitionedIn)
        {
            return activeCustomWeather.asset == asset;
        }
        return false;
    }

    public static bool IsWeatherFullyTransitionedIn(WeatherAssetBase asset)
    {
        if (activeCustomWeather != null && activeCustomWeather.component.isFullyTransitionedIn)
        {
            return activeCustomWeather.asset == asset;
        }
        return false;
    }

    public static bool IsWeatherTransitioningOut(WeatherAssetBase asset)
    {
        CustomWeatherInstance customWeatherInstance = FindWeatherInstanceByAsset(asset);
        if (customWeatherInstance != null)
        {
            return !customWeatherInstance.component.isWeatherActive;
        }
        return false;
    }

    public static bool IsWeatherFullyTransitionedOut(WeatherAssetBase asset)
    {
        return FindWeatherInstanceByAsset(asset) == null;
    }

    public static bool IsWeatherTransitioning(WeatherAssetBase asset)
    {
        CustomWeatherInstance customWeatherInstance = FindWeatherInstanceByAsset(asset);
        if (customWeatherInstance != null)
        {
            return !customWeatherInstance.component.isFullyTransitionedIn;
        }
        return false;
    }

    public static float GetWeatherGlobalBlendAlpha(WeatherAssetBase asset)
    {
        return FindWeatherInstanceByAsset(asset)?.component.globalBlendAlpha ?? 0f;
    }

    internal static bool GetWeatherStateForListeners(Guid assetGuid, out bool isActive, out bool isFullyTransitionedIn)
    {
        foreach (CustomWeatherInstance customWeatherInstance in customWeatherInstances)
        {
            if (customWeatherInstance.asset.GUID == assetGuid)
            {
                isActive = customWeatherInstance.component.isWeatherActive;
                isFullyTransitionedIn = customWeatherInstance.component.isFullyTransitionedIn;
                return true;
            }
        }
        isActive = false;
        isFullyTransitionedIn = false;
        return false;
    }

    [Obsolete]
    public static void setCustomWeather(WeatherAssetBase asset)
    {
    }

    internal static void SetActiveWeatherAsset(WeatherAssetBase asset, float blendAlpha, NetId netId)
    {
        if (activeCustomWeather != null)
        {
            if (activeCustomWeather.asset == asset)
            {
                return;
            }
            activeCustomWeather.component.OnBeginTransitionOut();
            WeatherEventListenerManager.InvokeBeginTransitionOut(activeCustomWeather.asset.GUID);
            WeatherEventListenerManager.InvokeStatusChange(activeCustomWeather.asset, EWeatherStatusChange.BeginTransitionOut);
            activeCustomWeather.component.isWeatherActive = false;
            activeCustomWeather = null;
        }
        if (asset == null)
        {
            return;
        }
        foreach (CustomWeatherInstance customWeatherInstance in customWeatherInstances)
        {
            if (customWeatherInstance.asset.GUID == asset.GUID)
            {
                activeCustomWeather = customWeatherInstance;
                break;
            }
        }
        if (activeCustomWeather == null)
        {
            activeCustomWeather = new CustomWeatherInstance();
            activeCustomWeather.asset = asset;
            activeCustomWeather.netId = netId;
            activeCustomWeather.initialize();
            customWeatherInstances.Add(activeCustomWeather);
        }
        activeCustomWeather.component.isWeatherActive = true;
        WeatherEventListenerManager.InvokeBeginTransitionIn(activeCustomWeather.asset.GUID);
        WeatherEventListenerManager.InvokeStatusChange(asset, EWeatherStatusChange.BeginTransitionIn);
        activeCustomWeather.component.globalBlendAlpha = blendAlpha;
        activeCustomWeather.component.OnBeginTransitionIn();
    }

    public static void setEnabled(bool isEnabled)
    {
        if (sun != null)
        {
            sunLight.enabled = isEnabled;
        }
    }

    public static bool isPositionSnowy(Vector3 position)
    {
        if (Level.info != null && Level.info.configData.Use_Legacy_Snow_Height)
        {
            if (snowLevel > 0.01f)
            {
                return position.y > snowLevel * Level.TERRAIN;
            }
            return false;
        }
        return false;
    }

    [Obsolete("Replaced by WaterUtility")]
    public static bool isPositionUnderwater(Vector3 position)
    {
        if (Level.info != null && Level.info.configData.Use_Legacy_Water)
        {
            if (seaLevel < 0.99f)
            {
                return position.y < seaLevel * Level.TERRAIN;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// If global ocean plane is enabled then return the worldspace height,
    /// otherwise return the optional default value. Default for volume based
    /// water is -1024, but atmosphere measure uses a default of zero.
    /// </summary>
    public static float getWaterSurfaceElevation(float defaultValue = -1024f)
    {
        if (Level.info != null && Level.info.configData.Use_Legacy_Water && seaLevel < 0.99f)
        {
            return seaLevel * Level.TERRAIN;
        }
        return defaultValue;
    }

    public static void setSeaVector(string name, Vector4 vector)
    {
        foreach (WaterVolume item in VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes())
        {
            if (!(item.sharedMaterial == null))
            {
                item.sharedMaterial.SetVector(name, vector);
            }
        }
    }

    public static Color getSeaColor(string name)
    {
        return GetFirstOrDefaultWaterVolume()?.sharedMaterial?.GetColor(name) ?? ((Color)Vector4.zero);
    }

    public static void setSeaColor(string name, Color color)
    {
        foreach (WaterVolume item in VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes())
        {
            if (!(item.sharedMaterial == null))
            {
                item.sharedMaterial.SetColor(name, color);
            }
        }
    }

    public static float getSeaFloat(string name)
    {
        return (GetFirstOrDefaultWaterVolume()?.sharedMaterial?.GetFloat(name)).GetValueOrDefault();
    }

    public static void setSeaFloat(string name, float value)
    {
        foreach (WaterVolume item in VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes())
        {
            if (!(item.sharedMaterial == null))
            {
                item.sharedMaterial.SetFloat(name, value);
            }
        }
    }

    private static WaterVolume GetFirstOrDefaultWaterVolume()
    {
        IReadOnlyList<WaterVolume> readOnlyList = VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes();
        if (readOnlyList.Count <= 0)
        {
            return null;
        }
        return readOnlyList[0];
    }

    public static void updateLighting()
    {
        if (sun == null)
        {
            return;
        }
        float num = 0f;
        setSeaVector("_WorldLightDir", sun.forward);
        int num2;
        int num3;
        float value;
        if (time < bias)
        {
            sun.rotation = Quaternion.Euler(time / bias * 180f, azimuth, 0f);
            if (time < transition)
            {
                dayVolume = Mathf.Lerp(0.5f, 1f, time / transition);
                nightVolume = Mathf.Lerp(0.5f, 0f, time / transition);
                num2 = 0;
                num3 = 1;
                num = time / transition;
                setSeaColor("_Foam", Color.Lerp(FOAM_DAWN, FOAM_MIDDAY, time / transition));
                setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_DAWN, SPECULAR_MIDDAY, time / transition));
                RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_DAWN, REFLECTION_MIDDAY, time / transition);
            }
            else if (time < bias - transition)
            {
                dayVolume = 1f;
                nightVolume = 0f;
                num2 = -1;
                num3 = 1;
                setSeaColor("_Foam", FOAM_MIDDAY);
                setSeaFloat("_Shininess", SPECULAR_MIDDAY);
                RenderSettings.reflectionIntensity = REFLECTION_MIDDAY;
            }
            else
            {
                dayVolume = Mathf.Lerp(1f, 0.5f, (time - bias + transition) / transition);
                nightVolume = Mathf.Lerp(0f, 0.5f, (time - bias + transition) / transition);
                num2 = 1;
                num3 = 2;
                num = (time - bias + transition) / transition;
                setSeaColor("_Foam", Color.Lerp(FOAM_MIDDAY, FOAM_DUSK, (time - bias + transition) / transition));
                setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_MIDDAY, SPECULAR_DUSK, (time - bias + transition) / transition));
                RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_MIDDAY, REFLECTION_DUSK, (time - bias + transition) / transition);
            }
            value = 1f;
            auroraBorealisTargetIntensity = 0f;
        }
        else
        {
            sun.rotation = Quaternion.Euler(180f + (time - bias) / (1f - bias) * 180f, azimuth, 0f);
            if (time < bias + transition)
            {
                dayVolume = Mathf.Lerp(0.5f, 0f, (time - bias) / transition);
                nightVolume = Mathf.Lerp(0.5f, 1f, (time - bias) / transition);
                num2 = 2;
                num3 = 3;
                num = (time - bias) / transition;
                setSeaColor("_Foam", Color.Lerp(FOAM_DUSK, FOAM_MIDNIGHT, (time - bias) / transition));
                setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_DUSK, SPECULAR_MIDNIGHT, (time - bias) / transition));
                RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_DUSK, REFLECTION_MIDNIGHT, (time - bias) / transition);
                value = Mathf.Lerp(1f, 0.05f, num);
                auroraBorealisTargetIntensity = 0f;
            }
            else if (time < 1f - transition)
            {
                dayVolume = 0f;
                nightVolume = 1f;
                num2 = -1;
                num3 = 3;
                setSeaColor("_Foam", FOAM_MIDNIGHT);
                setSeaFloat("_Shininess", SPECULAR_MIDNIGHT);
                RenderSettings.reflectionIntensity = REFLECTION_MIDNIGHT;
                value = 0.05f;
                auroraBorealisTargetIntensity = 1f;
            }
            else
            {
                dayVolume = Mathf.Lerp(0f, 0.5f, (time - 1f + transition) / transition);
                nightVolume = Mathf.Lerp(1f, 0.5f, (time - 1f + transition) / transition);
                num2 = 3;
                num3 = 0;
                num = (time - 1f + transition) / transition;
                setSeaColor("_Foam", Color.Lerp(FOAM_MIDNIGHT, FOAM_DAWN, (time - 1f + transition) / transition));
                setSeaFloat("_Shininess", Mathf.Lerp(SPECULAR_MIDNIGHT, SPECULAR_DAWN, (time - 1f + transition) / transition));
                RenderSettings.reflectionIntensity = Mathf.Lerp(REFLECTION_MIDNIGHT, REFLECTION_DAWN, (time - 1f + transition) / transition);
                value = Mathf.Lerp(0.05f, 1f, num);
                auroraBorealisTargetIntensity = 0f;
            }
        }
        LightingInfo lightingInfo = ((num2 < 0) ? null : times[num2]);
        LightingInfo lightingInfo2 = times[num3];
        float num4;
        float value2;
        if (lightingInfo == null)
        {
            sunLight.color = lightingInfo2.colors[0];
            sunLight.intensity = lightingInfo2.singles[0];
            num4 = lightingInfo2.singles[3];
            setSeaColor("_BaseColor", lightingInfo2.colors[1]);
            setSeaColor("_ReflectionColor", lightingInfo2.colors[1]);
            RenderSettings.ambientSkyColor = lightingInfo2.colors[6];
            RenderSettings.ambientEquatorColor = lightingInfo2.colors[7];
            RenderSettings.ambientGroundColor = lightingInfo2.colors[8];
            skyboxSky = lightingInfo2.colors[3];
            skyboxEquator = lightingInfo2.colors[4];
            skyboxGround = lightingInfo2.colors[5];
            cloudRimColor = lightingInfo2.colors[9];
            particleLightingColor = lightingInfo2.colors[11];
            raysColor = lightingInfo2.colors[10];
            raysIntensity = lightingInfo2.singles[4] * 4f;
            levelFogColor = lightingInfo2.colors[2];
            levelFogIntensity = lightingInfo2.singles[1];
            value2 = lightingInfo2.singles[2];
        }
        else
        {
            sunLight.color = Color.Lerp(lightingInfo.colors[0], lightingInfo2.colors[0], num);
            sunLight.intensity = Mathf.Lerp(lightingInfo.singles[0], lightingInfo2.singles[0], num);
            num4 = Mathf.Lerp(lightingInfo.singles[3], lightingInfo2.singles[3], num);
            setSeaColor("_BaseColor", Color.Lerp(lightingInfo.colors[1], lightingInfo2.colors[1], num));
            setSeaColor("_ReflectionColor", Color.Lerp(lightingInfo.colors[1], lightingInfo2.colors[1], num));
            RenderSettings.ambientSkyColor = Color.Lerp(lightingInfo.colors[6], lightingInfo2.colors[6], num);
            RenderSettings.ambientEquatorColor = Color.Lerp(lightingInfo.colors[7], lightingInfo2.colors[7], num);
            RenderSettings.ambientGroundColor = Color.Lerp(lightingInfo.colors[8], lightingInfo2.colors[8], num);
            skyboxSky = Color.Lerp(lightingInfo.colors[3], lightingInfo2.colors[3], num);
            skyboxEquator = Color.Lerp(lightingInfo.colors[4], lightingInfo2.colors[4], num);
            skyboxGround = Color.Lerp(lightingInfo.colors[5], lightingInfo2.colors[5], num);
            cloudRimColor = Color.Lerp(lightingInfo.colors[9], lightingInfo2.colors[9], num);
            particleLightingColor = Color.Lerp(lightingInfo.colors[11], lightingInfo2.colors[11], num);
            raysColor = Color.Lerp(lightingInfo.colors[10], lightingInfo2.colors[10], num);
            raysIntensity = Mathf.Lerp(lightingInfo.singles[4], lightingInfo2.singles[4], num) * 4f;
            levelFogColor = Color.Lerp(lightingInfo.colors[2], lightingInfo2.colors[2], num);
            levelFogIntensity = Mathf.Lerp(lightingInfo.singles[1], lightingInfo2.singles[1], num);
            value2 = Mathf.Lerp(lightingInfo.singles[2], lightingInfo2.singles[2], num);
        }
        cloudColor = cloudRimColor;
        levelAtmosphericFog = 0f;
        float num5 = 1f;
        float num6 = 1f;
        foreach (CustomWeatherInstance customWeatherInstance in customWeatherInstances)
        {
            customWeatherInstance.component.UpdateLightingTime(num2, num3, num);
            if (customWeatherInstance.component.overrideFog)
            {
                float t = Mathf.Pow(customWeatherInstance.component.EffectBlendAlpha, customWeatherInstance.component.fogBlendExponent);
                levelFogColor = Color.Lerp(levelFogColor, customWeatherInstance.component.fogColor, t);
                levelFogIntensity = Mathf.Lerp(levelFogIntensity, customWeatherInstance.component.fogDensity, t);
                if (customWeatherInstance.component.overrideAtmosphericFog)
                {
                    levelAtmosphericFog = Mathf.Lerp(levelAtmosphericFog, 1f, t);
                }
            }
            if (customWeatherInstance.component.overrideCloudColors)
            {
                float t2 = Mathf.Pow(customWeatherInstance.component.EffectBlendAlpha, customWeatherInstance.component.cloudBlendExponent);
                cloudColor = Color.Lerp(cloudColor, customWeatherInstance.component.cloudColor, t2);
                cloudRimColor = Color.Lerp(cloudRimColor, customWeatherInstance.component.cloudRimColor, t2);
            }
            num5 = Mathf.Lerp(num5, customWeatherInstance.component.shadowStrengthMultiplier, customWeatherInstance.component.EffectBlendAlpha);
            num6 = Mathf.Lerp(num6, customWeatherInstance.component.brightnessMultiplier, customWeatherInstance.component.EffectBlendAlpha);
        }
        if (localBlendingFog)
        {
            levelFogColor = Color.Lerp(levelFogColor, localFogColor, localFogBlend);
            levelFogIntensity = Mathf.Lerp(levelFogIntensity, localFogIntensity, localFogBlend);
            levelAtmosphericFog = Mathf.Lerp(levelAtmosphericFog, localAtmosphericFog, localFogBlend);
        }
        sunLight.shadowStrength = num4 * num5;
        if (num6 != 1f)
        {
            setSeaColor("_Foam", getSeaColor("_Foam") * num6);
            setSeaFloat("_Shininess", getSeaFloat("_Shininess") * num6);
            setSeaColor("_BaseColor", getSeaColor("_BaseColor") * num6);
            setSeaColor("_ReflectionColor", getSeaColor("_ReflectionColor") * num6);
            sunLight.intensity *= num6;
            RenderSettings.ambientSkyColor *= num6;
            RenderSettings.ambientEquatorColor *= num6;
            RenderSettings.ambientGroundColor *= num6;
            skyboxSky *= num6;
            skyboxEquator *= num6;
            skyboxGround *= num6;
            particleLightingColor *= num6;
        }
        if (localBlendingLight)
        {
            setSeaColor("_Foam", Color.Lerp(getSeaColor("_Foam"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));
            setSeaFloat("_Shininess", Mathf.Lerp(getSeaFloat("_Shininess"), 0f, localLightingBlend * PITCH_DARK_WATER_BLEND));
            setSeaColor("_BaseColor", Color.Lerp(getSeaColor("_BaseColor"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));
            setSeaColor("_ReflectionColor", Color.Lerp(getSeaColor("_ReflectionColor"), Color.black, localLightingBlend * PITCH_DARK_WATER_BLEND));
            sunLight.color = Color.Lerp(sunLight.color, Color.black, localLightingBlend);
            sunLight.intensity = Mathf.Lerp(sunLight.intensity, 0f, localLightingBlend);
            sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength, 0f, localLightingBlend);
            RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend);
            RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, Color.black, localLightingBlend);
            RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, Color.black, localLightingBlend);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            skyboxSky = Color.Lerp(skyboxSky, Color.black, localLightingBlend);
            skyboxEquator = Color.Lerp(skyboxEquator, Color.black, localLightingBlend);
            skyboxGround = Color.Lerp(skyboxGround, Color.black, localLightingBlend);
            cloudRimColor = Color.Lerp(cloudRimColor, Color.black, localLightingBlend);
            particleLightingColor = Color.Lerp(particleLightingColor, Color.black, localLightingBlend);
        }
        setSeaColor("_SpecularColor", sunLight.color);
        if (vision == ELightingVision.MILITARY || vision == ELightingVision.CIVILIAN)
        {
            setSeaColor("_BaseColor", nightvisionColor);
            setSeaColor("_ReflectionColor", nightvisionColor);
            RenderSettings.ambientSkyColor = nightvisionColor;
            RenderSettings.ambientEquatorColor = nightvisionColor;
            RenderSettings.ambientGroundColor = nightvisionColor;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            skyboxSky = nightvisionColor;
            skyboxEquator = nightvisionColor;
            skyboxGround = nightvisionColor;
            cloudRimColor = nightvisionColor;
            levelFogColor = nightvisionColor;
            levelFogIntensity = Mathf.Max(levelFogIntensity, nightvisionFogIntensity);
            if (localBlendingLight)
            {
                RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2f);
                RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2f);
                RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientSkyColor, Color.black, localLightingBlend / 2f);
                skyboxSky = Color.Lerp(skyboxSky, Color.black, localLightingBlend / 2f);
                skyboxEquator = Color.Lerp(skyboxEquator, Color.black, localLightingBlend / 2f);
                skyboxGround = Color.Lerp(skyboxGround, Color.black, localLightingBlend / 2f);
                cloudRimColor = Color.Lerp(cloudRimColor, Color.black, localLightingBlend / 2f);
            }
        }
        if (MainCamera.instance != null)
        {
            SunShaftsCs component = MainCamera.instance.GetComponent<SunShaftsCs>();
            if (component != null)
            {
                component.sunTransform = sunFlare;
                component.sunColor = raysColor;
            }
        }
        skybox.SetVector("_SkyHackAmbientEquator", RenderSettings.ambientEquatorColor.linear);
        skybox.SetVector("_SkyHackAmbientGround", RenderSettings.ambientGroundColor.linear);
        skybox.SetVector("_SunDirection", sun.forward);
        skybox.SetColor("_SunColor", Color.Lerp(sunLight.color, Color.white, 0.5f));
        skybox.SetFloat("_StarsCutoff", value);
        skybox.SetVector("_MoonDirection", -sun.forward);
        skybox.SetVector("_MoonLightDirection", moons[_moon].forward);
        skybox.SetColor("_CloudColor", cloudColor);
        skybox.SetColor("_CloudRimColor", cloudRimColor);
        skybox.SetFloat("_CloudIntensity", value2);
    }

    private static void updateHolidayWeatherRestrictions()
    {
        if (Level.shouldUseHolidayRedirects)
        {
            canRain = false;
            canSnow = false;
        }
    }

    public static void load(ushort size)
    {
        vision = ELightingVision.NONE;
        isSea = false;
        localEffectNode = null;
        currentEffectAsset = null;
        localPlayingEffect = false;
        localBlendingLight = false;
        localLightingBlend = 1f;
        localBlendingFog = false;
        localFogBlend = 1f;
        auroraBorealisCurrentIntensity = 0f;
        auroraBorealisTargetIntensity = 0f;
        currentAudioVolume = 0f;
        targetAudioVolume = 0f;
        nextAudioVolumeChangeTime = -1f;
        customWeatherInstances.Clear();
        activeCustomWeather = null;
        legacyWater = null;
        legacyWaterTransform = null;
        if (!Dedicator.IsDedicatedServer)
        {
            skybox = (Material)UnityEngine.Object.Instantiate(Resources.Load("Level/Skybox"));
            RenderSettings.skybox = skybox;
            if (Level.info.configData.Is_Aurora_Borealis_Visible)
            {
                skybox.EnableKeyword("WITH_AURORA_BOREALIS");
            }
            LevelAsset asset = Level.getAsset();
            if (asset != null && !asset.hasClouds)
            {
                skybox.DisableKeyword("WITH_CLOUDS");
            }
            lighting = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Level/Lighting"))).transform;
            lighting.name = "Lighting";
            lighting.position = Vector3.zero;
            lighting.rotation = Quaternion.identity;
            lighting.parent = Level.level;
            sun = lighting.Find("Sun");
            sunLight = sun.GetComponent<Light>();
            sunFlare = sun.Find("Flare_Sun");
            _bubbles = lighting.Find("Bubbles");
            UpdateBubblesActive();
            _windZone = lighting.Find("WindZone").GetComponent<WindZone>();
            reflectionCamera = lighting.Find("Reflection").GetComponent<Camera>();
            reflectionMap = new Cubemap(16, TextureFormat.ARGB32, mipChain: false);
            reflectionMap.name = "Skybox_Reflection";
            reflectionMapVision = new Cubemap(16, TextureFormat.ARGB32, mipChain: false);
            reflectionMapVision.name = "Skybox_Reflection_Vision";
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            reflectionIndex = 0;
            reflectionIndexVision = 0;
            isReflectionBuilding = false;
            isReflectionBuildingVision = false;
            puddles = lighting.GetComponent<Rain>();
            moons = new Transform[MOON_CYCLES];
            for (int i = 0; i < moons.Length; i++)
            {
                moons[i] = sun.Find("MoonLightDirection_" + i);
            }
            _effectAudio = lighting.Find("Effect").GetComponent<AudioSource>();
            _dayAudio = lighting.Find("Day").GetComponent<AudioSource>();
            _nightAudio = lighting.Find("Night").GetComponent<AudioSource>();
            _waterAudio = lighting.Find("Water").GetComponent<AudioSource>();
            _windAudio = lighting.Find("Wind").GetComponent<AudioSource>();
            _belowAudio = lighting.Find("Below").GetComponent<AudioSource>();
            if (ReadWrite.fileExists(Level.info.path + "/Environment/Ambience.unity3d", useCloud: false, usePath: false))
            {
                try
                {
                    Bundle bundle = Bundles.getBundle(Level.info.path + "/Environment/Ambience.unity3d", prependRoot: false);
                    dayAudio.clip = bundle.load<AudioClip>("Day");
                    nightAudio.clip = bundle.load<AudioClip>("Night");
                    waterAudio.clip = bundle.load<AudioClip>("Water");
                    windAudio.clip = bundle.load<AudioClip>("Wind");
                    belowAudio.clip = bundle.load<AudioClip>("Below");
                    bundle.unload();
                }
                catch (Exception e)
                {
                    UnturnedLog.exception(e, "Exception loading ambient audio:");
                }
            }
        }
        if (ReadWrite.fileExists(Level.info.path + "/Environment/Lighting.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Environment/Lighting.dat", useCloud: false, usePath: false, 0);
            byte b = block.readByte();
            _azimuth = block.readSingle();
            _bias = block.readSingle();
            _fade = block.readSingle();
            _time = block.readSingle();
            moon = block.readByte();
            if (b >= 5)
            {
                _seaLevel = block.readSingle();
                _snowLevel = block.readSingle();
                if (b > 6)
                {
                    canRain = block.readBoolean();
                }
                else
                {
                    canRain = false;
                }
                if (b > 10)
                {
                    canSnow = block.readBoolean();
                }
                else
                {
                    canSnow = false;
                }
                if (b < 8)
                {
                    rainFreq = 1f;
                    rainDur = 1f;
                }
                else
                {
                    rainFreq = block.readSingle();
                    rainDur = block.readSingle();
                }
                if (b < 11)
                {
                    snowFreq = 1f;
                    snowDur = 1f;
                }
                else
                {
                    snowFreq = block.readSingle();
                    snowDur = block.readSingle();
                }
                _times = new LightingInfo[4];
                for (int j = 0; j < times.Length; j++)
                {
                    Color[] array = new Color[12];
                    float[] array2 = new float[5];
                    if (b > 9)
                    {
                        for (int k = 0; k < array.Length; k++)
                        {
                            array[k] = block.readColor();
                        }
                        for (int l = 0; l < array2.Length; l++)
                        {
                            array2[l] = block.readSingle();
                        }
                    }
                    else if (b > 8)
                    {
                        for (int m = 0; m < array.Length - 1; m++)
                        {
                            array[m] = block.readColor();
                        }
                        array[11] = array[3];
                        for (int n = 0; n < array2.Length; n++)
                        {
                            array2[n] = block.readSingle();
                        }
                    }
                    else
                    {
                        if (b >= 6)
                        {
                            for (int num = 0; num < array.Length - 2; num++)
                            {
                                array[num] = block.readColor();
                            }
                        }
                        else
                        {
                            for (int num2 = 0; num2 < array.Length - 3; num2++)
                            {
                                array[num2] = block.readColor();
                            }
                            array[9] = array[2];
                        }
                        array[10] = array[0];
                        array[11] = array[3];
                        for (int num3 = 0; num3 < array2.Length - 1; num3++)
                        {
                            array2[num3] = block.readSingle();
                        }
                        array2[4] = 0.25f;
                    }
                    if (b < 12)
                    {
                        array2[1] = Mathf.Min(array2[1], 0.33f);
                    }
                    LightingInfo lightingInfo = new LightingInfo(array, array2);
                    times[j] = lightingInfo;
                }
            }
            else
            {
                _times = new LightingInfo[4];
                for (int num4 = 0; num4 < times.Length; num4++)
                {
                    Color[] newColors = new Color[12];
                    float[] newSingles = new float[5];
                    LightingInfo lightingInfo2 = new LightingInfo(newColors, newSingles);
                    times[num4] = lightingInfo2;
                }
                times[0].colors[3] = block.readColor();
                times[1].colors[3] = block.readColor();
                times[2].colors[3] = block.readColor();
                times[3].colors[3] = block.readColor();
                times[0].colors[4] = times[0].colors[3];
                times[1].colors[4] = times[1].colors[3];
                times[2].colors[4] = times[2].colors[3];
                times[3].colors[4] = times[3].colors[3];
                times[0].colors[5] = times[0].colors[3];
                times[1].colors[5] = times[1].colors[3];
                times[2].colors[5] = times[2].colors[3];
                times[3].colors[5] = times[3].colors[3];
                times[0].colors[6] = block.readColor();
                times[1].colors[6] = block.readColor();
                times[2].colors[6] = block.readColor();
                times[3].colors[6] = block.readColor();
                times[0].colors[7] = times[0].colors[6];
                times[1].colors[7] = times[1].colors[6];
                times[2].colors[7] = times[2].colors[6];
                times[3].colors[7] = times[3].colors[6];
                times[0].colors[8] = times[0].colors[6];
                times[1].colors[8] = times[1].colors[6];
                times[2].colors[8] = times[2].colors[6];
                times[3].colors[8] = times[3].colors[6];
                times[0].colors[2] = block.readColor();
                times[1].colors[2] = block.readColor();
                times[2].colors[2] = block.readColor();
                times[3].colors[2] = block.readColor();
                times[0].colors[0] = block.readColor();
                times[1].colors[0] = block.readColor();
                times[2].colors[0] = block.readColor();
                times[3].colors[0] = block.readColor();
                times[0].singles[0] = block.readSingle();
                times[1].singles[0] = block.readSingle();
                times[2].singles[0] = block.readSingle();
                times[3].singles[0] = block.readSingle();
                times[0].singles[1] = block.readSingle();
                times[1].singles[1] = block.readSingle();
                times[2].singles[1] = block.readSingle();
                times[3].singles[1] = block.readSingle();
                times[0].singles[2] = block.readSingle();
                times[1].singles[2] = block.readSingle();
                times[2].singles[2] = block.readSingle();
                times[3].singles[2] = block.readSingle();
                times[0].singles[3] = block.readSingle();
                times[1].singles[3] = block.readSingle();
                times[2].singles[3] = block.readSingle();
                times[3].singles[3] = block.readSingle();
                if (b > 2)
                {
                    _seaLevel = block.readSingle();
                }
                else
                {
                    _seaLevel = block.readSingle() / 2f;
                }
                if (b > 1)
                {
                    _snowLevel = block.readSingle();
                }
                else
                {
                    _snowLevel = 0f;
                }
                canRain = false;
                canSnow = false;
                times[0].colors[1] = block.readColor();
                times[1].colors[1] = block.readColor();
                times[2].colors[1] = block.readColor();
                times[3].colors[1] = block.readColor();
            }
            hash = block.getHash();
        }
        else
        {
            _azimuth = 0.2f;
            _bias = 0.5f;
            _fade = 1f;
            _time = bias / 2f;
            moon = 0;
            _seaLevel = 1f;
            _snowLevel = 0f;
            canRain = true;
            canSnow = false;
            rainFreq = 1f;
            rainDur = 1f;
            snowFreq = 1f;
            snowDur = 1f;
            _times = new LightingInfo[4];
            for (int num5 = 0; num5 < times.Length; num5++)
            {
                Color[] newColors2 = new Color[12];
                float[] newSingles2 = new float[5];
                LightingInfo lightingInfo3 = new LightingInfo(newColors2, newSingles2);
                times[num5] = lightingInfo3;
            }
            hash = new byte[20];
        }
        if (bias < 1f - bias)
        {
            _transition = bias / 2f * fade;
        }
        else
        {
            _transition = (1f - bias) / 2f * fade;
        }
        times[0].colors[1].a = 0.25f;
        times[1].colors[1].a = 0.5f;
        times[2].colors[1].a = 0.75f;
        times[3].colors[1].a = 0.9f;
        if (Level.info.configData.Use_Legacy_Water)
        {
            GameObject gameObject = new GameObject();
            legacyWaterTransform = gameObject.transform;
            legacyWaterTransform.parent = Level.level;
            legacyWater = gameObject.AddComponent<WaterVolume>();
            legacyWater.isManagedByLighting = true;
            legacyWater.isSeaLevel = true;
            legacyWater.isSurfaceVisible = true;
            legacyWater.isReflectionVisible = true;
            UpdateLegacyWaterTransform();
        }
        init = false;
        updateHolidayWeatherRestrictions();
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeSingle(azimuth);
        block.writeSingle(bias);
        block.writeSingle(fade);
        block.writeSingle(time);
        block.writeByte(moon);
        block.writeSingle(seaLevel);
        block.writeSingle(snowLevel);
        block.writeBoolean(canRain);
        block.writeBoolean(canSnow);
        block.writeSingle(rainFreq);
        block.writeSingle(rainDur);
        block.writeSingle(snowFreq);
        block.writeSingle(snowDur);
        for (int i = 0; i < times.Length; i++)
        {
            LightingInfo lightingInfo = times[i];
            for (int j = 0; j < lightingInfo.colors.Length; j++)
            {
                block.writeColor(lightingInfo.colors[j]);
            }
            for (int k = 0; k < lightingInfo.singles.Length; k++)
            {
                block.writeSingle(lightingInfo.singles[k]);
            }
        }
        ReadWrite.writeBlock(Level.info.path + "/Environment/Lighting.dat", useCloud: false, usePath: false, block);
    }

    private static void UpdateLegacyWaterTransform()
    {
        if (!(legacyWater == null))
        {
            if (seaLevel < 0.99f)
            {
                float num = (float)(int)Level.size * 2f;
                float num2 = seaLevel * Level.TERRAIN;
                legacyWaterTransform.position = new Vector3(0f, -512f + num2 * 0.5f, 0f);
                legacyWaterTransform.localScale = new Vector3(num, 1024f + num2, num);
                legacyWater.gameObject.SetActive(value: true);
            }
            else
            {
                legacyWater.gameObject.SetActive(value: false);
            }
        }
    }

    private static void UpdateBubblesActive()
    {
        bool flag = !Level.info.configData.Use_Legacy_Water || seaLevel < 0.99f;
        if (!Level.info.configData.Use_Vanilla_Bubbles)
        {
            flag = false;
        }
        bubbles.gameObject.SetActive(flag);
        if (flag)
        {
            bubbles.GetComponent<ParticleSystem>().Play();
        }
    }

    /// <summary>
    /// Ticked on dedicated server as well as client so that server can listen for weather events.
    /// </summary>
    /// <param name="localVolumeMask">On dedicated server this is always 0xFFFFFFFF.</param>
    public static void tickCustomWeatherBlending(uint localVolumeMask)
    {
        int frameCount = Time.frameCount;
        if (frameCount == tickedWeatherBlendingFrame)
        {
            return;
        }
        tickedWeatherBlendingFrame = frameCount;
        float deltaTime = Time.deltaTime;
        for (int num = customWeatherInstances.Count - 1; num >= 0; num--)
        {
            CustomWeatherInstance customWeatherInstance = customWeatherInstances[num];
            bool flag = (customWeatherInstance.asset.volumeMask & localVolumeMask) != 0;
            if (!customWeatherInstance.component.hasTickedBlending)
            {
                customWeatherInstance.component.hasTickedBlending = true;
                customWeatherInstance.component.localVolumeBlendAlpha = (flag ? customWeatherInstance.component.globalBlendAlpha : 0f);
            }
            if (flag && customWeatherInstance.component.isWeatherActive)
            {
                customWeatherInstance.component.localVolumeBlendAlpha = Mathf.Min(1f, customWeatherInstance.component.localVolumeBlendAlpha + deltaTime / Mathf.Max(0.1f, customWeatherInstance.asset.fadeInDuration));
            }
            else
            {
                customWeatherInstance.component.localVolumeBlendAlpha = Mathf.Max(0f, customWeatherInstance.component.localVolumeBlendAlpha - deltaTime / Mathf.Max(0.1f, customWeatherInstance.asset.fadeOutDuration));
            }
            if (customWeatherInstance.component.isWeatherActive)
            {
                customWeatherInstance.component.globalBlendAlpha += deltaTime / Mathf.Max(0.1f, customWeatherInstance.asset.fadeInDuration);
                if (customWeatherInstance.component.globalBlendAlpha >= 1f)
                {
                    customWeatherInstance.component.globalBlendAlpha = 1f;
                    if (!customWeatherInstance.component.isFullyTransitionedIn)
                    {
                        customWeatherInstance.component.isFullyTransitionedIn = true;
                        WeatherEventListenerManager.InvokeEndTransitionIn(customWeatherInstance.asset.GUID);
                        WeatherEventListenerManager.InvokeStatusChange(customWeatherInstance.component.asset, EWeatherStatusChange.EndTransitionIn);
                        customWeatherInstance.component.OnEndTransitionIn();
                        customWeatherInstance.eventBlendAlpha = 1f;
                        WeatherEventListenerManager.InvokeBlendAlphaChanged(customWeatherInstance.component.asset, 1f);
                    }
                }
                else if (customWeatherInstance.component.globalBlendAlpha - customWeatherInstance.eventBlendAlpha >= 0.01f)
                {
                    customWeatherInstance.eventBlendAlpha = customWeatherInstance.component.globalBlendAlpha;
                    WeatherEventListenerManager.InvokeBlendAlphaChanged(customWeatherInstance.asset, customWeatherInstance.component.globalBlendAlpha);
                }
            }
            else
            {
                customWeatherInstance.component.isFullyTransitionedIn = false;
                customWeatherInstance.component.globalBlendAlpha -= deltaTime / Mathf.Max(0.1f, customWeatherInstance.asset.fadeOutDuration);
                if (customWeatherInstance.component.globalBlendAlpha <= 0f)
                {
                    customWeatherInstance.component.globalBlendAlpha = 0f;
                    WeatherEventListenerManager.InvokeEndTransitionOut(customWeatherInstance.component.asset.GUID);
                    WeatherEventListenerManager.InvokeStatusChange(customWeatherInstance.component.asset, EWeatherStatusChange.EndTransitionOut);
                    customWeatherInstance.component.OnEndTransitionOut();
                    customWeatherInstance.eventBlendAlpha = 0f;
                    WeatherEventListenerManager.InvokeBlendAlphaChanged(customWeatherInstance.component.asset, 0f);
                    customWeatherInstance.teardown();
                    customWeatherInstances.RemoveAtFast(num);
                }
                else if (customWeatherInstance.eventBlendAlpha - customWeatherInstance.component.globalBlendAlpha >= 0.01f)
                {
                    customWeatherInstance.eventBlendAlpha = customWeatherInstance.component.globalBlendAlpha;
                    WeatherEventListenerManager.InvokeBlendAlphaChanged(customWeatherInstance.asset, customWeatherInstance.component.globalBlendAlpha);
                }
            }
        }
    }

    public static void updateLocal()
    {
        updateLocal(localPoint, localWindOverride, localEffectNode);
    }

    public static void updateLocal(Vector3 point, float windOverride, IAmbianceNode effectNode)
    {
        localPoint = point;
        localWindOverride = windOverride;
        if (effectNode != localEffectNode)
        {
            if (effectNode != null)
            {
                EffectAsset effectAsset = effectNode.GetEffectAsset();
                if (localEffectNode == null || effectAsset != currentEffectAsset)
                {
                    currentEffectAsset = effectAsset;
                    if (effectAsset != null && effectAsset.effect != null)
                    {
                        AudioSource component = effectAsset.effect.GetComponent<AudioSource>();
                        if (component != null)
                        {
                            if (!effectAsset.isMusic || OptionsSettings.ambientMusicVolume > 0f)
                            {
                                if (effectAsset.isMusic)
                                {
                                    effectAudio.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
                                }
                                else
                                {
                                    effectAudio.outputAudioMixerGroup = UnturnedAudioMixer.GetAtmosphereGroup();
                                }
                                effectAudio.clip = component.clip;
                                effectAudio.Play();
                                localPlayingEffect = true;
                            }
                            else
                            {
                                localPlayingEffect = false;
                            }
                        }
                        else
                        {
                            localPlayingEffect = false;
                        }
                    }
                    else
                    {
                        localPlayingEffect = false;
                    }
                }
            }
            else
            {
                localPlayingEffect = false;
            }
        }
        localEffectNode = effectNode;
        if (localEffectNode != null && localEffectNode.noLighting && !Level.isEditor)
        {
            localLightingBlend = Mathf.Lerp(localLightingBlend, 1f, 0.25f * Time.deltaTime);
            localBlendingLight = true;
        }
        else
        {
            localLightingBlend = Mathf.Lerp(localLightingBlend, 0f, 0.25f * Time.deltaTime);
            if (localLightingBlend < 0.01f)
            {
                localLightingBlend = 0f;
                localBlendingLight = false;
            }
        }
        AmbianceVolume ambianceVolume = localEffectNode as AmbianceVolume;
        if (ambianceVolume != null && ambianceVolume.overrideFog)
        {
            localFogBlend = Mathf.Lerp(localFogBlend, 1f, 0.05f * Time.deltaTime);
            localBlendingFog = true;
            localFogColor = ambianceVolume.fogColor;
            localFogIntensity = ambianceVolume.fogIntensity;
            localAtmosphericFog = (ambianceVolume.overrideAtmosphericFog ? ambianceVolume.fogIntensity : 0f);
        }
        else
        {
            localFogBlend = Mathf.Lerp(localFogBlend, 0f, 0.125f * Time.deltaTime);
            if (localFogBlend < 0.01f)
            {
                localFogBlend = 0f;
                localBlendingFog = false;
            }
        }
        uint num = ((!(ambianceVolume != null)) ? ((uint)(((int?)Level.getAsset()?.globalWeatherMask) ?? (-1))) : ambianceVolume.weatherMask);
        if (Level.info != null && Level.info.configData != null)
        {
            if (!Level.info.configData.Use_Rain_Volumes)
            {
                num |= 1u;
            }
            if (!Level.info.configData.Use_Snow_Volumes)
            {
                num |= 2u;
            }
            if (Level.info.configData.Use_Legacy_Snow_Height)
            {
                num = ((!isPositionSnowy(point)) ? (num & 0xFFFFFFFDu) : (num | 2u));
            }
        }
        tickCustomWeatherBlending(num);
        if (!init)
        {
            init = true;
            resetCachedValues();
            updateLighting();
            bubbles.GetComponent<ParticleSystem>().Play();
            if (dayAudio.clip != null)
            {
                dayAudio.Play();
            }
            if (nightAudio.clip != null)
            {
                nightAudio.Play();
            }
            if (waterAudio.clip != null)
            {
                waterAudio.Play();
            }
            if (windAudio.clip != null)
            {
                windAudio.Play();
            }
            if (belowAudio.clip != null)
            {
                belowAudio.Play();
            }
        }
        lighting.position = point;
        setSkyColor(skyboxSky);
        setEquatorColor(skyboxEquator);
        setGroundColor(skyboxGround);
        float num2 = WaterUtility.getWaterSurfaceElevation(point);
        if (!enableUnderwaterEffects)
        {
            num2 = -1024f;
        }
        if (enableUnderwaterEffects && WaterUtility.isPointUnderwater(point))
        {
            waterAudio.volume = 0f;
            belowAudio.volume = 1f;
            isSea = true;
        }
        else
        {
            if (point.y < num2 + 8f && (localEffectNode == null || !localEffectNode.noWater))
            {
                waterAudio.volume = Mathf.Lerp(0f, 0.25f, 1f - (point.y - num2) / 8f);
                belowAudio.volume = 0f;
            }
            else
            {
                waterAudio.volume = 0f;
                belowAudio.volume = 0f;
            }
            isSea = false;
        }
        if (isSea)
        {
            RenderSettings.fogColor = getSeaColor("_BaseColor");
            RenderSettings.fogDensity = 0.075f;
            setAtmosphericFog(1f);
        }
        else
        {
            RenderSettings.fogColor = levelFogColor;
            RenderSettings.fogDensity = Mathf.Pow(levelFogIntensity, 3f) * 0.025f;
            setAtmosphericFog(levelAtmosphericFog);
        }
        auroraBorealisCurrentIntensity = Mathf.Clamp01(Mathf.Lerp(auroraBorealisCurrentIntensity, auroraBorealisTargetIntensity, 0.1f * Time.deltaTime));
        skybox.SetFloat("_AuroraBorealisIntensity", auroraBorealisCurrentIntensity);
        setAlphaParticleLightingColor(particleLightingColor);
        if (isSea)
        {
            Color fogColor = RenderSettings.fogColor;
            setSkyColor(fogColor);
            setEquatorColor(fogColor);
            setGroundColor(fogColor);
        }
        if (puddles != null)
        {
            float num3 = 0f;
            float num4 = 0f;
            foreach (CustomWeatherInstance customWeatherInstance in customWeatherInstances)
            {
                num3 = Mathf.Max(num3, customWeatherInstance.component.puddleWaterLevel * customWeatherInstance.component.EffectBlendAlpha);
                num4 = Mathf.Max(num4, customWeatherInstance.component.puddleIntensity * customWeatherInstance.component.EffectBlendAlpha);
            }
            if (num3 > puddles.Water_Level)
            {
                puddles.Water_Level = Mathf.Lerp(puddles.Water_Level, num3, 0.2f * Time.deltaTime);
            }
            else
            {
                puddles.Water_Level = Mathf.Lerp(puddles.Water_Level, num3, 0.025f * Time.deltaTime);
            }
            puddles.Intensity = num4;
        }
        if (Time.time > nextAudioVolumeChangeTime)
        {
            nextAudioVolumeChangeTime = Time.time + (float)UnityEngine.Random.Range(15, 60);
            targetAudioVolume = UnityEngine.Random.Range(AUDIO_MIN, AUDIO_MAX);
        }
        currentAudioVolume = Mathf.Lerp(currentAudioVolume, targetAudioVolume, 0.1f * Time.deltaTime);
        float b = ((!localPlayingEffect) ? 0f : ((currentEffectAsset == null || !currentEffectAsset.isMusic) ? 1f : OptionsSettings.ambientMusicVolume));
        effectAudio.volume = Mathf.Lerp(effectAudio.volume, b, Level.isEditor ? 1f : (0.5f * Time.deltaTime));
        float num5 = 1f - effectAudio.volume;
        float num6 = 0f;
        float num7 = 0.15f;
        foreach (CustomWeatherInstance customWeatherInstance2 in customWeatherInstances)
        {
            if (customWeatherInstance2.component.ambientAudioSource != null)
            {
                float b2 = num5 * customWeatherInstance2.component.EffectBlendAlpha;
                customWeatherInstance2.component.ambientAudioSource.volume = Mathf.Lerp(customWeatherInstance2.component.ambientAudioSource.volume, b2, 0.5f * Time.deltaTime);
                num6 = Mathf.Max(num6, customWeatherInstance2.component.ambientAudioSource.volume);
            }
            float b3 = customWeatherInstance2.component.windMain * customWeatherInstance2.component.EffectBlendAlpha;
            num7 = Mathf.Max(num7, b3);
            customWeatherInstance2.component.UpdateWeather();
        }
        float num8 = 1f - num6;
        windAudio.volume = windOverride;
        dayAudio.volume = Mathf.Lerp(dayAudio.volume, dayVolume * currentAudioVolume * (1f - waterAudio.volume * 4f) * (1f - belowAudio.volume) * (1f - windAudio.volume) * (1f - effectAudio.volume) * num8, 0.5f * Time.deltaTime);
        nightAudio.volume = Mathf.Lerp(nightAudio.volume, nightVolume * currentAudioVolume * (1f - waterAudio.volume * 4f) * (1f - belowAudio.volume) * (1f - windAudio.volume) * (1f - effectAudio.volume) * num8, 0.5f * Time.deltaTime);
        windZone.transform.rotation = Quaternion.Slerp(windZone.transform.rotation, Quaternion.Euler(0f, wind, 0f), 0.5f * Time.deltaTime);
        windZone.windMain = Mathf.Lerp(windZone.windMain, num7, 0.5f * Time.deltaTime);
        point.y = Mathf.Min(point.y - 16f, num2 - 32f);
        bubbles.position = point;
        if (skyboxNeedsColorUpdate)
        {
            updateSkyboxColors();
        }
        if (skyboxNeedsReflectionUpdate)
        {
            lastSkyboxReflectionUpdate = Time.time;
            skyboxNeedsReflectionUpdate = false;
            if (vision != ELightingVision.CIVILIAN && vision != ELightingVision.MILITARY && !localBlendingLight)
            {
                if (Provider.preferenceData != null && Provider.preferenceData.Graphics.Use_Skybox_Ambience)
                {
                    RenderSettings.ambientMode = AmbientMode.Skybox;
                    DynamicGI.UpdateEnvironment();
                }
                else
                {
                    RenderSettings.ambientMode = AmbientMode.Trilight;
                }
            }
            isReflectionBuilding = true;
            isReflectionBuildingVision = true;
        }
        else if (Time.time - lastSkyboxReflectionUpdate > 3f)
        {
            skyboxNeedsReflectionUpdate = true;
        }
        updateSkyboxReflections();
    }

    private static void renderSkyboxReflection(Cubemap target, ref int index, ref bool isBuilding)
    {
        if (isBuilding && !(target == null) && !(reflectionCamera == null))
        {
            int faceMask = 1 << index;
            index++;
            if (index > 5)
            {
                index = 0;
                isBuilding = false;
            }
            reflectionCamera.RenderToCubemap(target, faceMask);
        }
    }

    public static void updateSkyboxReflections()
    {
        if (isSkyboxReflectionEnabled)
        {
            if (vision == ELightingVision.NONE)
            {
                renderSkyboxReflection(reflectionMap, ref reflectionIndex, ref isReflectionBuilding);
                RenderSettings.customReflection = reflectionMap;
            }
            else
            {
                renderSkyboxReflection(reflectionMapVision, ref reflectionIndexVision, ref isReflectionBuildingVision);
                RenderSettings.customReflection = reflectionMapVision;
            }
        }
        else
        {
            RenderSettings.customReflection = null;
        }
    }

    private static void setAtmosphericFog(float newFog)
    {
        if (!MathfEx.IsNearlyEqual(cachedAtmosphericFog, newFog, 0.001f))
        {
            cachedAtmosphericFog = newFog;
            Shader.SetGlobalFloat("_AtmosphericFog", newFog);
        }
        if (MainCamera.instance != null)
        {
            SunShaftsCs component = MainCamera.instance.GetComponent<SunShaftsCs>();
            if (component != null)
            {
                component.sunShaftIntensity = 1f - newFog;
            }
        }
    }

    private static void setAlphaParticleLightingColor(Color newColor)
    {
        if (!MathfEx.IsNearlyEqual(newColor, cachedAlphaParticleLightingColor))
        {
            cachedAlphaParticleLightingColor = newColor;
            Shader.SetGlobalColor("_AlphaParticleLightingColor", newColor);
        }
    }

    private static void setSkyColor(Color skyColor)
    {
        if (!MathfEx.IsNearlyEqual(skyColor, cachedSkyColor))
        {
            cachedSkyColor = skyColor;
            skyboxNeedsColorUpdate = true;
        }
    }

    private static void setEquatorColor(Color equatorColor)
    {
        if (!MathfEx.IsNearlyEqual(equatorColor, cachedEquatorColor))
        {
            cachedEquatorColor = equatorColor;
            skyboxNeedsColorUpdate = true;
        }
    }

    private static void setGroundColor(Color groundColor)
    {
        if (!MathfEx.IsNearlyEqual(groundColor, cachedGroundColor))
        {
            cachedGroundColor = groundColor;
            skyboxNeedsColorUpdate = true;
        }
    }

    private static void updateSkyboxColors()
    {
        skyboxNeedsColorUpdate = false;
        skybox.SetColor("_SkyColor", cachedSkyColor);
        skybox.SetColor("_EquatorColor", cachedEquatorColor);
        skybox.SetColor("_GroundColor", cachedGroundColor);
        setSeaColor("_SkyColor", cachedSkyColor);
        setSeaColor("_EquatorColor", cachedEquatorColor);
        setSeaColor("_GroundColor", cachedGroundColor);
    }

    private static void resetCachedValues()
    {
        cachedAtmosphericFog = -1f;
        cachedAlphaParticleLightingColor = new Color(-1f, -1f, -1f);
        cachedSkyColor = new Color(-1f, -1f, -1f);
        cachedEquatorColor = new Color(-1f, -1f, -1f);
        cachedGroundColor = new Color(-1f, -1f, -1f);
    }

    /// <summary>
    /// Reset any global shader properties that may affect the main menu.
    /// </summary>
    public static void resetForMainMenu()
    {
        setAtmosphericFog(0f);
        setAlphaParticleLightingColor(Color.white);
    }

    static LevelLighting()
    {
        _editorWantsUnderwaterEffects = true;
        _editorWantsWaterSurface = true;
        SAVEDATA_VERSION = 12;
        MOON_CYCLES = 5;
        CLOUDS = 2f;
        AUDIO_MIN = 0.075f;
        AUDIO_MAX = 0.15f;
        FOAM_DAWN = new Color(0.125f, 0f, 0f, 0f);
        FOAM_MIDDAY = new Color(0.25f, 0f, 0f, 0f);
        FOAM_DUSK = new Color(0.05f, 0f, 0f, 0f);
        FOAM_MIDNIGHT = new Color(0.01f, 0f, 0f, 0f);
        SPECULAR_DAWN = 5f;
        SPECULAR_MIDDAY = 50f;
        SPECULAR_DUSK = 5f;
        SPECULAR_MIDNIGHT = 50f;
        PITCH_DARK_WATER_BLEND = 0.9f;
        REFLECTION_DAWN = 0.75f;
        REFLECTION_MIDDAY = 0.75f;
        REFLECTION_DUSK = 0.5f;
        REFLECTION_MIDNIGHT = 0.5f;
        NIGHTVISION_MILITARY = new Color32(20, 120, 80, 0);
        NIGHTVISION_CIVILIAN = new Color(0.4f, 0.4f, 0.4f, 0f);
        customWeatherInstances = new List<CustomWeatherInstance>();
        activeCustomWeather = null;
        skyboxNeedsColorUpdate = false;
        if (ConvenientSavedata.get().read("EditorWantsUnderwaterEffects", out bool value))
        {
            _editorWantsUnderwaterEffects = value;
        }
        if (ConvenientSavedata.get().read("EditorWantsWaterSurfaceVisible", out bool value2))
        {
            _editorWantsWaterSurface = value2;
        }
    }
}
