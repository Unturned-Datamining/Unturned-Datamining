using System;
using System.Collections.Generic;
using HighlightingSystem;
using SDG.Framework.Foliage;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace SDG.Unturned;

public class GraphicsSettings
{
    private static bool _uncapLandmarks = false;

    public const float EFFECT_ULTRA = 64f;

    public const float EFFECT_HIGH = 48f;

    public const float EFFECT_MEDIUM = 32f;

    public const float EFFECT_LOW = 16f;

    /// <summary>
    /// Planar reflection component updates its culling distance and culling mask when this is incremented.
    /// </summary>
    public static int planarReflectionUpdateIndex;

    private static GraphicsSettingsData graphicsSettingsData = new GraphicsSettingsData();

    /// <summary>
    /// Overrides in-game UI scale setting.
    /// </summary>
    private static CommandLineFloat uiScale = new CommandLineFloat("-ui_scale");

    private static bool didCacheUIScaleOverride = false;

    private static float? cachedUIScaleOverride = null;

    private static CommandLineInt clTargetFrameRate = new CommandLineInt("-FrameRateLimit");

    /// <summary>
    /// Added for players who want to see if they can get better performance with a ridiculously low max draw distance.
    /// </summary>
    private static CommandLineFloat clFarClipDistance = new CommandLineFloat("-FarClipDistance");

    private static bool changeResolution;

    private static CommandLineInt valveWidth = new CommandLineInt("-w");

    private static CommandLineInt valveHeight = new CommandLineInt("-h");

    private static CommandLineInt clWidth = new CommandLineInt("-width");

    private static CommandLineInt clHeight = new CommandLineInt("-height");

    private static CommandLineInt clFullscreenMode = new CommandLineInt("-fullscreenmode");

    private static CommandLineInt clRefreshRate = new CommandLineInt("-refreshrate");

    private static bool hasAppliedTargetFrameRate = false;

    private static int lastAppliedTargetFrameRate = -1;

    private static bool hasBoundApplicationFocusChangedEvent;

    public static bool uncapLandmarks
    {
        get
        {
            return _uncapLandmarks;
        }
        set
        {
            _uncapLandmarks = value;
            apply("changed uncapLandmarks");
            UnturnedLog.info("Set uncap_landmarks to: " + uncapLandmarks);
        }
    }

    public static float effect
    {
        get
        {
            if (effectQuality == EGraphicQuality.ULTRA)
            {
                return UnityEngine.Random.Range(48f, 80f);
            }
            if (effectQuality == EGraphicQuality.HIGH)
            {
                return UnityEngine.Random.Range(40f, 56f);
            }
            if (effectQuality == EGraphicQuality.MEDIUM)
            {
                return UnityEngine.Random.Range(28f, 36f);
            }
            if (effectQuality == EGraphicQuality.LOW)
            {
                return UnityEngine.Random.Range(14f, 18f);
            }
            return 0f;
        }
    }

    public static GraphicsSettingsResolution resolution
    {
        get
        {
            return graphicsSettingsData.Resolution;
        }
        set
        {
            graphicsSettingsData.Resolution = value;
            changeResolution = true;
        }
    }

    public static FullScreenMode fullscreenMode
    {
        get
        {
            return graphicsSettingsData.FullscreenMode;
        }
        set
        {
            graphicsSettingsData.FullscreenMode = value;
            changeResolution = true;
        }
    }

    public static bool buffer
    {
        get
        {
            return graphicsSettingsData.IsVSyncEnabled;
        }
        set
        {
            graphicsSettingsData.IsVSyncEnabled = value;
        }
    }

    public static float userInterfaceScale
    {
        get
        {
            if (!didCacheUIScaleOverride)
            {
                didCacheUIScaleOverride = true;
                if (uiScale.hasValue && uiScale.value.IsFinite())
                {
                    cachedUIScaleOverride = uiScale.value;
                }
                else if (Provider.preferenceData != null && Provider.preferenceData.Graphics != null)
                {
                    float override_UI_Scale = Provider.preferenceData.Graphics.Override_UI_Scale;
                    if (override_UI_Scale.IsFinite() && override_UI_Scale > 0f)
                    {
                        cachedUIScaleOverride = override_UI_Scale;
                    }
                }
            }
            if (cachedUIScaleOverride.HasValue)
            {
                return cachedUIScaleOverride.Value;
            }
            return graphicsSettingsData.UserInterfaceScale;
        }
        set
        {
            graphicsSettingsData.UserInterfaceScale = value;
        }
    }

    public static bool UseTargetFrameRate
    {
        get
        {
            return graphicsSettingsData.UseTargetFrameRate;
        }
        set
        {
            graphicsSettingsData.UseTargetFrameRate = value;
        }
    }

    public static int TargetFrameRate
    {
        get
        {
            return graphicsSettingsData.TargetFrameRate;
        }
        set
        {
            graphicsSettingsData.TargetFrameRate = value;
        }
    }

    public static bool UseUnfocusedTargetFrameRate
    {
        get
        {
            return graphicsSettingsData.UseUnfocusedTargetFrameRate;
        }
        set
        {
            graphicsSettingsData.UseUnfocusedTargetFrameRate = value;
        }
    }

    public static int UnfocusedTargetFrameRate
    {
        get
        {
            return graphicsSettingsData.UnfocusedTargetFrameRate;
        }
        set
        {
            graphicsSettingsData.UnfocusedTargetFrameRate = value;
        }
    }

    public static EAntiAliasingType antiAliasingType
    {
        get
        {
            return graphicsSettingsData.AntiAliasingType5;
        }
        set
        {
            graphicsSettingsData.AntiAliasingType5 = value;
        }
    }

    public static EAnisotropicFilteringMode anisotropicFilteringMode
    {
        get
        {
            return graphicsSettingsData.AnisotropicFilteringMode;
        }
        set
        {
            graphicsSettingsData.AnisotropicFilteringMode = value;
        }
    }

    public static bool isAmbientOcclusionEnabled
    {
        get
        {
            return graphicsSettingsData.IsAmbientOcclusionEnabled;
        }
        set
        {
            graphicsSettingsData.IsAmbientOcclusionEnabled = value;
        }
    }

    public static bool bloom
    {
        get
        {
            return graphicsSettingsData.IsBloomEnabled;
        }
        set
        {
            graphicsSettingsData.IsBloomEnabled = value;
        }
    }

    public static bool chromaticAberration
    {
        get
        {
            return graphicsSettingsData.IsChromaticAberrationEnabled;
        }
        set
        {
            graphicsSettingsData.IsChromaticAberrationEnabled = value;
        }
    }

    public static bool filmGrain
    {
        get
        {
            return graphicsSettingsData.IsFilmGrainEnabled;
        }
        set
        {
            graphicsSettingsData.IsFilmGrainEnabled = value;
        }
    }

    public static bool blend
    {
        get
        {
            return graphicsSettingsData.IsNiceBlendEnabled;
        }
        set
        {
            graphicsSettingsData.IsNiceBlendEnabled = value;
        }
    }

    /// <summary>
    /// Distance to use terrain shaders before fallback to a baked texture.
    /// </summary>
    public static float terrainBasemapDistance => blend ? 512 : 256;

    /// <summary>
    /// Higher error reduces vertex density as distance increases.
    /// </summary>
    public static float terrainHeightmapPixelError
    {
        get
        {
            switch (terrainQuality)
            {
            case EGraphicQuality.LOW:
                return 64f;
            case EGraphicQuality.MEDIUM:
                return 32f;
            case EGraphicQuality.HIGH:
                return 16f;
            case EGraphicQuality.ULTRA:
                return 8f;
            default:
                UnturnedLog.warn("Unknown terrain quality {0} in terrainHeightmapPixelError", terrainQuality);
                return 1f;
            }
        }
    }

    public static bool grassDisplacement
    {
        get
        {
            return graphicsSettingsData.IsGrassDisplacementEnabled;
        }
        set
        {
            graphicsSettingsData.IsGrassDisplacementEnabled = value;
        }
    }

    public static bool foliageFocus
    {
        get
        {
            return graphicsSettingsData.IsFoliageFocusEnabled;
        }
        set
        {
            graphicsSettingsData.IsFoliageFocusEnabled = value;
        }
    }

    public static EGraphicQuality landmarkQuality
    {
        get
        {
            return graphicsSettingsData.LandmarkQuality;
        }
        set
        {
            graphicsSettingsData.LandmarkQuality = value;
        }
    }

    public static bool ragdolls
    {
        get
        {
            return graphicsSettingsData.IsRagdollsEnabled;
        }
        set
        {
            graphicsSettingsData.IsRagdollsEnabled = value;
        }
    }

    public static bool debris
    {
        get
        {
            return graphicsSettingsData.IsDebrisEnabled;
        }
        set
        {
            graphicsSettingsData.IsDebrisEnabled = value;
        }
    }

    public static bool blast
    {
        get
        {
            return graphicsSettingsData.IsBlastEnabled;
        }
        set
        {
            graphicsSettingsData.IsBlastEnabled = value;
        }
    }

    public static bool puddle
    {
        get
        {
            return graphicsSettingsData.IsPuddleEnabled;
        }
        set
        {
            graphicsSettingsData.IsPuddleEnabled = value;
        }
    }

    public static bool glitter
    {
        get
        {
            return graphicsSettingsData.IsGlitterEnabled;
        }
        set
        {
            graphicsSettingsData.IsGlitterEnabled = value;
        }
    }

    public static bool triplanar
    {
        get
        {
            return graphicsSettingsData.IsTriplanarMappingEnabled;
        }
        set
        {
            graphicsSettingsData.IsTriplanarMappingEnabled = value;
        }
    }

    public static bool skyboxReflection
    {
        get
        {
            return graphicsSettingsData.IsSkyboxReflectionEnabled;
        }
        set
        {
            graphicsSettingsData.IsSkyboxReflectionEnabled = value;
        }
    }

    public static bool IsItemIconAntiAliasingEnabled
    {
        get
        {
            return graphicsSettingsData.IsItemIconAntiAliasingEnabled;
        }
        set
        {
            graphicsSettingsData.IsItemIconAntiAliasingEnabled = value;
        }
    }

    public static bool IsWindEnabled
    {
        get
        {
            return graphicsSettingsData.IsWindEnabled;
        }
        set
        {
            graphicsSettingsData.IsWindEnabled = value;
        }
    }

    /// <summary>
    /// Multiplier for far clip plane distance.
    /// Clamped within [0, 1] range to prevent editing config files for an advantage.
    /// </summary>
    public static float NormalizedFarClipDistance
    {
        get
        {
            return Mathf.Clamp01(Level.isEditor ? graphicsSettingsData.EditorFarClipDistance : graphicsSettingsData.FarClipDistance);
        }
        set
        {
            if (Level.isEditor)
            {
                graphicsSettingsData.EditorFarClipDistance = Mathf.Clamp01(value);
            }
            else
            {
                graphicsSettingsData.FarClipDistance = Mathf.Clamp01(value);
            }
        }
    }

    /// <summary>
    /// Multiplier for draw distance.
    /// Clamped within [0, 1] range to prevent editing config files for an advantage.
    /// </summary>
    public static float normalizedDrawDistance
    {
        get
        {
            return Mathf.Clamp01(graphicsSettingsData.DrawDistance);
        }
        set
        {
            graphicsSettingsData.DrawDistance = Mathf.Clamp01(value);
        }
    }

    /// <summary>
    /// Multiplier for draw distance of optional super-low LOD models.
    /// Clamped within [0, 1] range to prevent editing config files for an advantage.
    /// </summary>
    public static float normalizedLandmarkDrawDistance
    {
        get
        {
            return Mathf.Clamp01(graphicsSettingsData.LandmarkDistance);
        }
        set
        {
            graphicsSettingsData.LandmarkDistance = Mathf.Clamp01(value);
        }
    }

    public static EGraphicQuality effectQuality
    {
        get
        {
            return graphicsSettingsData.EffectQuality;
        }
        set
        {
            graphicsSettingsData.EffectQuality = value;
        }
    }

    public static EGraphicQuality foliageQuality
    {
        get
        {
            if (graphicsSettingsData.FoliageQuality2 == EGraphicQuality.OFF)
            {
                return EGraphicQuality.LOW;
            }
            return graphicsSettingsData.FoliageQuality2;
        }
        set
        {
            graphicsSettingsData.FoliageQuality2 = value;
        }
    }

    public static EGraphicQuality sunShaftsQuality
    {
        get
        {
            return graphicsSettingsData.SunShaftsQuality;
        }
        set
        {
            graphicsSettingsData.SunShaftsQuality = value;
        }
    }

    public static EGraphicQuality lightingQuality
    {
        get
        {
            if (Level.isVR && graphicsSettingsData.LightingQuality == EGraphicQuality.ULTRA)
            {
                return EGraphicQuality.HIGH;
            }
            return graphicsSettingsData.LightingQuality;
        }
        set
        {
            graphicsSettingsData.LightingQuality = value;
        }
    }

    public static EGraphicQuality reflectionQuality
    {
        get
        {
            return graphicsSettingsData.ScreenSpaceReflectionQuality;
        }
        set
        {
            graphicsSettingsData.ScreenSpaceReflectionQuality = value;
        }
    }

    public static EGraphicQuality planarReflectionQuality
    {
        get
        {
            return graphicsSettingsData.PlanarReflectionQuality;
        }
        set
        {
            graphicsSettingsData.PlanarReflectionQuality = value;
        }
    }

    public static EGraphicQuality waterQuality
    {
        get
        {
            if (Level.isVR && graphicsSettingsData.WaterQuality == EGraphicQuality.ULTRA)
            {
                return EGraphicQuality.HIGH;
            }
            return graphicsSettingsData.WaterQuality;
        }
        set
        {
            graphicsSettingsData.WaterQuality = value;
        }
    }

    public static EGraphicQuality scopeQuality
    {
        get
        {
            return graphicsSettingsData.ScopeQuality2;
        }
        set
        {
            graphicsSettingsData.ScopeQuality2 = value;
        }
    }

    public static EGraphicQuality outlineQuality
    {
        get
        {
            return graphicsSettingsData.OutlineQuality;
        }
        set
        {
            graphicsSettingsData.OutlineQuality = value;
        }
    }

    public static EGraphicQuality terrainQuality
    {
        get
        {
            return graphicsSettingsData.TerrainQuality;
        }
        set
        {
            graphicsSettingsData.TerrainQuality = value;
        }
    }

    public static ERenderMode renderMode
    {
        get
        {
            ERenderMode renderMode = graphicsSettingsData.RenderMode2;
            if ((uint)renderMode <= 1u)
            {
                return graphicsSettingsData.RenderMode2;
            }
            return ERenderMode.FORWARD;
        }
        set
        {
            graphicsSettingsData.RenderMode2 = value;
        }
    }

    public static event GraphicsSettingsApplied graphicsSettingsApplied;

    public static void applyResolution()
    {
        if (Application.isEditor)
        {
            return;
        }
        string commandLine = Environment.CommandLine;
        if (false | (commandLine.IndexOf("-screen-width", StringComparison.InvariantCultureIgnoreCase) >= 0) | (commandLine.IndexOf("-screen-height", StringComparison.InvariantCultureIgnoreCase) >= 0) | (commandLine.IndexOf("-screen-fullscreen", StringComparison.InvariantCultureIgnoreCase) >= 0) | (commandLine.IndexOf("-window-mode", StringComparison.InvariantCultureIgnoreCase) >= 0))
        {
            UnturnedLog.info("Ignoring game resolution settings because Unity built-in command-line options were set");
            return;
        }
        int num = resolution.Width;
        int num2 = resolution.Height;
        int num3 = 0;
        if (clWidth.hasValue)
        {
            num = clWidth.value;
        }
        else if (valveWidth.hasValue)
        {
            num = valveWidth.value;
        }
        else if (Provider.preferenceData.Graphics.Override_Resolution_Width > 0)
        {
            num = Provider.preferenceData.Graphics.Override_Resolution_Width;
        }
        if (clHeight.hasValue)
        {
            num2 = clHeight.value;
        }
        else if (valveHeight.hasValue)
        {
            num2 = valveHeight.value;
        }
        else if (Provider.preferenceData.Graphics.Override_Resolution_Height > 0)
        {
            num2 = Provider.preferenceData.Graphics.Override_Resolution_Height;
        }
        if (clRefreshRate.hasValue && clRefreshRate.value > 0)
        {
            num3 = clRefreshRate.value;
        }
        else if (Provider.preferenceData.Graphics.Override_Refresh_Rate > 0)
        {
            num3 = Provider.preferenceData.Graphics.Override_Refresh_Rate;
        }
        if (clWidth.hasValue != clHeight.hasValue)
        {
            UnturnedLog.warn("Mismatch of {0} and {1}", clWidth.key, clHeight.key);
        }
        if (valveWidth.hasValue != valveHeight.hasValue)
        {
            UnturnedLog.warn("Mismatch of {0} and {1}", valveWidth.key, valveHeight.key);
        }
        FullScreenMode fullScreenMode = fullscreenMode;
        if (clFullscreenMode.hasValue)
        {
            if (Enum.IsDefined(typeof(FullScreenMode), clFullscreenMode.value))
            {
                fullScreenMode = (FullScreenMode)clFullscreenMode.value;
            }
            else
            {
                UnturnedLog.warn($"Invalid fullscreen mode on command-line: {clFullscreenMode.value}");
            }
        }
        else if (Provider.preferenceData.Graphics.Override_Fullscreen_Mode >= 0)
        {
            if (Enum.IsDefined(typeof(FullScreenMode), Provider.preferenceData.Graphics.Override_Fullscreen_Mode))
            {
                fullScreenMode = (FullScreenMode)Provider.preferenceData.Graphics.Override_Fullscreen_Mode;
            }
            else
            {
                UnturnedLog.warn($"Invalid fullscreen mode in config: {Provider.preferenceData.Graphics.Override_Fullscreen_Mode}");
            }
        }
        if (fullscreenMode == FullScreenMode.ExclusiveFullScreen && num3 > 0)
        {
            UnturnedLog.info($"Requesting resolution change: {fullScreenMode} {num} x {num2} @ {num3} hz");
            Screen.SetResolution(num, num2, fullScreenMode, num3);
        }
        else
        {
            UnturnedLog.info($"Requesting resolution change: {fullScreenMode} {num} x {num2}");
            Screen.SetResolution(num, num2, fullScreenMode);
        }
    }

    internal static void ApplyVSyncAndTargetFrameRate()
    {
        QualitySettings.vSyncCount = (buffer ? 1 : 0);
        int num;
        if (clTargetFrameRate.hasValue)
        {
            num = ((clTargetFrameRate.value > 0) ? Mathf.Max(clTargetFrameRate.value, 15) : (-1));
        }
        else if (UseTargetFrameRate)
        {
            int a = ((!UseUnfocusedTargetFrameRate || Application.isFocused) ? TargetFrameRate : Mathf.Min(TargetFrameRate, UnfocusedTargetFrameRate));
            num = Mathf.Max(a, 15);
        }
        else
        {
            num = -1;
        }
        if (num != lastAppliedTargetFrameRate || !hasAppliedTargetFrameRate)
        {
            hasAppliedTargetFrameRate = true;
            lastAppliedTargetFrameRate = num;
            Application.targetFrameRate = num;
            UnturnedLog.info($"Set target frame rate to {num} fps");
        }
        if (!hasBoundApplicationFocusChangedEvent)
        {
            hasBoundApplicationFocusChangedEvent = true;
            Application.focusChanged += OnApplicationFocusChanged;
        }
    }

    private static void OnApplicationFocusChanged(bool hasFocus)
    {
        ApplyVSyncAndTargetFrameRate();
    }

    public static void apply(string reason)
    {
        UnturnedLog.info("Applying graphics settings ({0})", reason);
        if (changeResolution)
        {
            changeResolution = false;
            applyResolution();
        }
        if (LevelLighting.sun != null)
        {
            if (lightingQuality == EGraphicQuality.ULTRA || lightingQuality == EGraphicQuality.HIGH)
            {
                LevelLighting.sun.GetComponent<Light>().shadowNormalBias = 0f;
            }
            else
            {
                LevelLighting.sun.GetComponent<Light>().shadowNormalBias = 0.5f;
            }
        }
        QualitySettings.SetQualityLevel((byte)lightingQuality + 1, applyExpensiveChanges: true);
        ApplyVSyncAndTargetFrameRate();
        switch (anisotropicFilteringMode)
        {
        case EAnisotropicFilteringMode.DISABLED:
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            break;
        case EAnisotropicFilteringMode.PER_TEXTURE:
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            break;
        case EAnisotropicFilteringMode.FORCED_ON:
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            break;
        }
        float num = (0.3f + NormalizedFarClipDistance * 0.7f) * 2048f;
        if (clFarClipDistance.hasValue)
        {
            num = Mathf.Clamp(clFarClipDistance.value, 16f, 2048f);
        }
        float num2 = num + 725f;
        float a = 256f + normalizedDrawDistance * 256f;
        a = Mathf.Min(a, num);
        float[] array = new float[32]
        {
            0f,
            0f,
            0f,
            0f,
            num2,
            0f,
            0f,
            0f,
            Level.isEditor ? a : 0f,
            0f,
            a,
            0f,
            a,
            a * 0.125f,
            a,
            a,
            a * 0.5f,
            a * 0.125f,
            num,
            a,
            num2,
            0f,
            Level.isEditor ? a : 0f,
            a,
            0f,
            0f,
            a,
            a,
            a,
            0f,
            a,
            num
        };
        float num3 = Mathf.Max(0f, num - a) * normalizedLandmarkDrawDistance;
        if (landmarkQuality >= EGraphicQuality.LOW)
        {
            if (uncapLandmarks)
            {
                array[15] = num;
            }
            else
            {
                array[15] += num3;
            }
        }
        if (landmarkQuality >= EGraphicQuality.MEDIUM)
        {
            if (uncapLandmarks)
            {
                array[14] = num;
            }
            else
            {
                array[14] += num3;
            }
        }
        if (landmarkQuality >= EGraphicQuality.ULTRA)
        {
            if (uncapLandmarks)
            {
                array[19] = num;
            }
            else
            {
                array[19] += num3;
            }
        }
        if (Level.isEditor)
        {
            num *= 2f;
            for (int i = 0; i < 32; i++)
            {
                array[i] *= 2f;
            }
        }
        if (!LevelObjects.shouldInstantlyLoad && !LevelGround.shouldInstantlyLoad)
        {
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    if (LevelObjects.regions != null && !LevelObjects.regions[b, b2])
                    {
                        List<LevelObject> list = LevelObjects.objects[b, b2];
                        for (int j = 0; j < list.Count; j++)
                        {
                            list[j]?.UpdateSkyboxActive();
                        }
                    }
                    if (LevelGround.regions != null && !LevelGround.regions[b, b2])
                    {
                        List<ResourceSpawnpoint> list2 = LevelGround.trees[b, b2];
                        for (int k = 0; k < list2.Count; k++)
                        {
                            ResourceSpawnpoint resourceSpawnpoint = list2[k];
                            if (resourceSpawnpoint != null)
                            {
                                if (landmarkQuality >= EGraphicQuality.MEDIUM)
                                {
                                    resourceSpawnpoint.enableSkybox();
                                }
                                else
                                {
                                    resourceSpawnpoint.disableSkybox();
                                }
                            }
                        }
                    }
                }
            }
        }
        QualitySettings.lodBias = 2f + normalizedDrawDistance * 3f + Mathf.Clamp(Provider.preferenceData.Graphics.LOD_Bias, 0f, 5f);
        LODGroupManager.Get().SynchronizeLODBias();
        QualitySettings.skinWeights = SkinWeights.FourBones;
        if (MainCamera.instance != null)
        {
            MainCamera.instance.renderingPath = ((renderMode != 0) ? RenderingPath.Forward : RenderingPath.DeferredShading);
            MainCamera.instance.allowHDR = true;
            MainCamera.instance.allowMSAA = false;
            SunShaftsCs component = MainCamera.instance.GetComponent<SunShaftsCs>();
            if (component != null)
            {
                if (sunShaftsQuality == EGraphicQuality.LOW)
                {
                    component.resolution = ESunShaftsCsResolution.Low;
                }
                else if (sunShaftsQuality == EGraphicQuality.MEDIUM)
                {
                    component.resolution = ESunShaftsCsResolution.Normal;
                }
                else if (sunShaftsQuality == EGraphicQuality.HIGH)
                {
                    component.resolution = ESunShaftsCsResolution.High;
                }
                component.enabled = sunShaftsQuality != EGraphicQuality.OFF;
            }
            HighlightingRenderer component2 = MainCamera.instance.GetComponent<HighlightingRenderer>();
            if (component2 != null)
            {
                if (outlineQuality == EGraphicQuality.LOW)
                {
                    component2.downsampleFactor = 4;
                    component2.iterations = 1;
                    component2.blurMinSpread = 0.75f;
                    component2.blurSpread = 0f;
                    component2.blurIntensity = 0.25f;
                }
                else if (outlineQuality == EGraphicQuality.MEDIUM)
                {
                    component2.downsampleFactor = 4;
                    component2.iterations = 2;
                    component2.blurMinSpread = 0.5f;
                    component2.blurSpread = 0.25f;
                    component2.blurIntensity = 0.25f;
                }
                else if (outlineQuality == EGraphicQuality.HIGH)
                {
                    component2.downsampleFactor = 2;
                    component2.iterations = 2;
                    component2.blurMinSpread = 1f;
                    component2.blurSpread = 0.5f;
                    component2.blurIntensity = 0.25f;
                }
                else if (outlineQuality == EGraphicQuality.ULTRA)
                {
                    component2.downsampleFactor = 1;
                    component2.iterations = 3;
                    component2.blurMinSpread = 0.5f;
                    component2.blurSpread = 0.5f;
                    component2.blurIntensity = 0.25f;
                }
            }
            MainCamera.instance.farClipPlane = num;
            MainCamera.instance.layerCullDistances = array;
            MainCamera.instance.layerCullSpherical = true;
            if (Player.player != null)
            {
                Player.player.look.scopeCamera.farClipPlane = num;
                Player.player.look.scopeCamera.layerCullDistances = array;
                Player.player.look.scopeCamera.layerCullSpherical = true;
                Player.player.look.scopeCamera.depthTextureMode = DepthTextureMode.Depth;
                Player.player.look.updateScope(scopeQuality);
                Player.player.look.scopeCamera.renderingPath = ((renderMode != 0) ? RenderingPath.Forward : RenderingPath.DeferredShading);
                Player.player.look.scopeCamera.allowHDR = true;
                Player.player.look.scopeCamera.allowMSAA = false;
                Player.player.animator.viewmodelCamera.renderingPath = ((renderMode != 0) ? RenderingPath.Forward : RenderingPath.DeferredShading);
                Player.player.animator.viewmodelCamera.allowHDR = true;
                Player.player.animator.viewmodelCamera.allowMSAA = false;
            }
        }
        switch (foliageQuality)
        {
        case EGraphicQuality.OFF:
            FoliageSettings.enabled = false;
            FoliageSettings.drawDistance = 0;
            FoliageSettings.instanceDensity = 0f;
            FoliageSettings.drawFocusDistance = 0;
            FoliageSettings.focusDistance = 0f;
            break;
        case EGraphicQuality.LOW:
            FoliageSettings.enabled = true;
            FoliageSettings.drawDistance = 2;
            FoliageSettings.instanceDensity = 0.25f;
            FoliageSettings.drawFocusDistance = 1;
            FoliageSettings.focusDistance = num;
            break;
        case EGraphicQuality.MEDIUM:
            FoliageSettings.enabled = true;
            FoliageSettings.drawDistance = 3;
            FoliageSettings.instanceDensity = 0.5f;
            FoliageSettings.drawFocusDistance = 2;
            FoliageSettings.focusDistance = num;
            break;
        case EGraphicQuality.HIGH:
            FoliageSettings.enabled = true;
            FoliageSettings.drawDistance = 4;
            FoliageSettings.instanceDensity = 0.75f;
            FoliageSettings.drawFocusDistance = 3;
            FoliageSettings.focusDistance = num;
            break;
        case EGraphicQuality.ULTRA:
            FoliageSettings.enabled = true;
            FoliageSettings.drawDistance = 5;
            FoliageSettings.instanceDensity = 1f;
            FoliageSettings.drawFocusDistance = 4;
            FoliageSettings.focusDistance = num;
            break;
        default:
            FoliageSettings.enabled = true;
            FoliageSettings.drawDistance = 2;
            FoliageSettings.instanceDensity = 0.25f;
            FoliageSettings.drawFocusDistance = 1;
            FoliageSettings.focusDistance = num;
            UnturnedLog.error("Unknown foliage quality: " + foliageQuality);
            break;
        }
        FoliageSettings.drawFocus = foliageFocus;
        if (waterQuality == EGraphicQuality.LOW || waterQuality == EGraphicQuality.MEDIUM)
        {
            Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
            Shader.DisableKeyword("WATER_EDGEBLEND_ON");
        }
        else if (waterQuality == EGraphicQuality.HIGH || waterQuality == EGraphicQuality.ULTRA)
        {
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                Shader.EnableKeyword("WATER_EDGEBLEND_ON");
                Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
            }
            else
            {
                Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
                Shader.DisableKeyword("WATER_EDGEBLEND_ON");
            }
        }
        LevelLighting.isSkyboxReflectionEnabled = skyboxReflection;
        if (IsWindEnabled)
        {
            Shader.EnableKeyword("NICE_FOLIAGE_ON");
            Shader.EnableKeyword("GRASS_WIND_ON");
        }
        else
        {
            Shader.DisableKeyword("NICE_FOLIAGE_ON");
            Shader.DisableKeyword("GRASS_WIND_ON");
        }
        if (grassDisplacement)
        {
            Shader.EnableKeyword("GRASS_DISPLACEMENT_ON");
        }
        else
        {
            Shader.DisableKeyword("GRASS_DISPLACEMENT_ON");
        }
        if (Level.info != null && Level.info.configData != null && Level.info.configData.Terrain_Snow_Sparkle && glitter)
        {
            Shader.EnableKeyword("IS_SNOWING");
        }
        else
        {
            Shader.DisableKeyword("IS_SNOWING");
        }
        if (triplanar)
        {
            Shader.EnableKeyword("TRIPLANAR_MAPPING_ON");
        }
        else
        {
            Shader.DisableKeyword("TRIPLANAR_MAPPING_ON");
        }
        planarReflectionUpdateIndex++;
        UnturnedPostProcess.instance.applyUserSettings();
        GraphicsSettings.graphicsSettingsApplied?.Invoke();
        UnturnedLog.info("Applied graphics settings");
    }

    public static void restoreDefaults()
    {
        FullScreenMode fullScreenMode = FullScreenMode.Windowed;
        bool isVSyncEnabled = false;
        GraphicsSettingsResolution graphicsSettingsResolution = new GraphicsSettingsResolution();
        if (graphicsSettingsData != null)
        {
            fullScreenMode = graphicsSettingsData.FullscreenMode;
            isVSyncEnabled = graphicsSettingsData.IsVSyncEnabled;
            graphicsSettingsResolution = graphicsSettingsData.Resolution;
        }
        graphicsSettingsData = new GraphicsSettingsData();
        graphicsSettingsData.FullscreenMode = fullScreenMode;
        graphicsSettingsData.IsVSyncEnabled = isVSyncEnabled;
        graphicsSettingsData.Resolution = graphicsSettingsResolution;
        fixDefaultResolution();
        apply("restoring defaults");
    }

    /// <summary>
    /// Called after loading graphics settings from disk so that their values can be adjusted.
    /// </summary>
    private static void validateSettings()
    {
        if (graphicsSettingsData.UserInterfaceScale.IsFinite())
        {
            float num = graphicsSettingsData.UserInterfaceScale;
            float num2 = Mathf.Clamp(num, 0.5f, 2f);
            if (num != num2)
            {
                UnturnedLog.info($"Clamped UI scale from {num} to {num2}");
            }
            graphicsSettingsData.UserInterfaceScale = num2;
        }
        else
        {
            UnturnedLog.info($"Reset UI scale (was {graphicsSettingsData.UserInterfaceScale})");
            graphicsSettingsData.UserInterfaceScale = 1f;
        }
        fixDefaultResolution();
    }

    /// <summary>
    /// If default resolution is zero, try falling back to a higher one.
    /// Used when restoring defaults and validating loaded settings.
    /// </summary>
    private static void fixDefaultResolution()
    {
        GraphicsSettingsResolution graphicsSettingsResolution = graphicsSettingsData.Resolution;
        if (graphicsSettingsResolution == null || graphicsSettingsResolution.Width < 1 || graphicsSettingsResolution.Height < 1)
        {
            graphicsSettingsData.Resolution = new GraphicsSettingsResolution(ScreenEx.GetHighestRecommendedResolution());
            UnturnedLog.info($"Restored default resolution to {graphicsSettingsData.Resolution.Width}x{graphicsSettingsData.Resolution.Height}");
        }
    }

    public static void load()
    {
        if (ReadWrite.fileExists("/Settings/Graphics.json", useCloud: true))
        {
            try
            {
                graphicsSettingsData = ReadWrite.deserializeJSON<GraphicsSettingsData>("/Settings/Graphics.json", useCloud: true);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Unable to parse Graphics.json! consider validating with a JSON linter");
                graphicsSettingsData = null;
            }
            if (graphicsSettingsData == null)
            {
                restoreDefaults();
            }
            else
            {
                validateSettings();
            }
        }
        else
        {
            restoreDefaults();
        }
        if (graphicsSettingsData.EffectQuality == EGraphicQuality.OFF)
        {
            graphicsSettingsData.EffectQuality = EGraphicQuality.MEDIUM;
        }
        if (!Application.isEditor)
        {
            Resolution highestRecommendedResolution = ScreenEx.GetHighestRecommendedResolution();
            if (resolution.Width > highestRecommendedResolution.width || resolution.Height > highestRecommendedResolution.height)
            {
                resolution = new GraphicsSettingsResolution(highestRecommendedResolution);
            }
        }
    }

    public static void save()
    {
        ReadWrite.serializeJSON("/Settings/Graphics.json", useCloud: true, graphicsSettingsData);
    }
}
