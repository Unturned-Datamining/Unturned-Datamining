using UnityEngine;

namespace SDG.Unturned;

public class GraphicsSettingsData
{
    public FullScreenMode FullscreenMode;

    public float UserInterfaceScale;

    public bool UseTargetFrameRate;

    public int TargetFrameRate;

    public bool IsAmbientOcclusionEnabled;

    public bool IsGrassDisplacementEnabled;

    public bool IsFoliageFocusEnabled;

    public bool IsRagdollsEnabled;

    public bool IsDebrisEnabled;

    public bool IsBlastEnabled;

    public bool IsPuddleEnabled;

    public bool IsGlitterEnabled;

    public bool IsTriplanarMappingEnabled;

    public bool IsSkyboxReflectionEnabled;

    public bool IsItemIconAntiAliasingEnabled;

    /// <summary>
    /// Far clip plane multiplier in-game.
    /// </summary>
    public float FarClipDistance;

    /// <summary>
    /// Far clip plane multiplier in level editor.
    /// </summary>
    public float EditorFarClipDistance;

    public ERenderMode RenderMode2;

    public EGraphicQuality LandmarkQuality;

    public GraphicsSettingsResolution Resolution { get; set; }

    public bool IsVSyncEnabled { get; set; }

    public bool IsBloomEnabled { get; set; }

    public bool IsChromaticAberrationEnabled { get; set; }

    public bool IsFilmGrainEnabled { get; set; }

    public bool IsNiceBlendEnabled { get; set; }

    public float DrawDistance { get; set; }

    public float LandmarkDistance { get; set; }

    public EAntiAliasingType AntiAliasingType5 { get; set; }

    public EAnisotropicFilteringMode AnisotropicFilteringMode { get; set; }

    public EGraphicQuality EffectQuality { get; set; }

    public EGraphicQuality FoliageQuality2 { get; set; }

    public EGraphicQuality SunShaftsQuality { get; set; }

    public EGraphicQuality LightingQuality { get; set; }

    public EGraphicQuality ScreenSpaceReflectionQuality { get; set; }

    public EGraphicQuality PlanarReflectionQuality { get; set; }

    public EGraphicQuality WaterQuality { get; set; }

    public EGraphicQuality ScopeQuality2 { get; set; }

    public EGraphicQuality OutlineQuality { get; set; }

    public EGraphicQuality TerrainQuality { get; set; }

    public EGraphicQuality WindQuality { get; set; }

    public ETreeGraphicMode TreeMode { get; set; }

    public GraphicsSettingsData()
    {
        Resolution = new GraphicsSettingsResolution();
        FullscreenMode = FullScreenMode.FullScreenWindow;
        IsVSyncEnabled = false;
        UserInterfaceScale = 1f;
        UseTargetFrameRate = false;
        TargetFrameRate = 1000;
        IsAmbientOcclusionEnabled = false;
        IsBloomEnabled = false;
        IsChromaticAberrationEnabled = false;
        IsFilmGrainEnabled = false;
        IsNiceBlendEnabled = true;
        IsGrassDisplacementEnabled = false;
        IsFoliageFocusEnabled = false;
        IsRagdollsEnabled = true;
        IsDebrisEnabled = true;
        IsBlastEnabled = true;
        IsPuddleEnabled = true;
        IsGlitterEnabled = true;
        IsTriplanarMappingEnabled = true;
        IsSkyboxReflectionEnabled = false;
        IsItemIconAntiAliasingEnabled = false;
        FarClipDistance = 0.333333f;
        EditorFarClipDistance = 1f;
        DrawDistance = 1f;
        LandmarkDistance = 0f;
        AntiAliasingType5 = EAntiAliasingType.OFF;
        AnisotropicFilteringMode = EAnisotropicFilteringMode.FORCED_ON;
        EffectQuality = EGraphicQuality.MEDIUM;
        FoliageQuality2 = EGraphicQuality.LOW;
        SunShaftsQuality = EGraphicQuality.OFF;
        LightingQuality = EGraphicQuality.LOW;
        ScreenSpaceReflectionQuality = EGraphicQuality.OFF;
        PlanarReflectionQuality = EGraphicQuality.MEDIUM;
        WaterQuality = EGraphicQuality.LOW;
        ScopeQuality2 = EGraphicQuality.OFF;
        OutlineQuality = EGraphicQuality.LOW;
        TerrainQuality = EGraphicQuality.MEDIUM;
        WindQuality = EGraphicQuality.OFF;
        TreeMode = ETreeGraphicMode.LEGACY;
        RenderMode2 = ERenderMode.FORWARD;
        LandmarkQuality = EGraphicQuality.OFF;
    }
}
