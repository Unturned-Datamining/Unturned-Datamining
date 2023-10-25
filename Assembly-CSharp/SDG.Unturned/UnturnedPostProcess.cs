using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

/// <summary>
/// Manages global post-process volumes.
/// </summary>
public class UnturnedPostProcess : MonoBehaviour
{
    private class PostProcessProfileWrapper
    {
        public PostProcessProfile profile;

        public AmbientOcclusion ambientOcclusion;

        public Bloom bloom;

        public ChromaticAberration chromaticAberration;

        public ColorGrading colorGrading;

        public Grain filmGrain;

        public ScreenSpaceReflections screenSpaceReflections;

        public Vignette vignette;

        public PostProcessProfileWrapper(PostProcessProfile profile, bool viewmodel)
        {
            this.profile = profile;
            ambientOcclusion = profile.AddSettings<AmbientOcclusion>();
            ambientOcclusion.active = false;
            ambientOcclusion.intensity.Override(0.25f);
            bloom = profile.AddSettings<Bloom>();
            bloom.active = false;
            bloom.intensity.Override(1f);
            bloom.softKnee.Override(0f);
            colorGrading = profile.AddSettings<ColorGrading>();
            colorGrading.active = false;
            chromaticAberration = profile.AddSettings<ChromaticAberration>();
            chromaticAberration.active = false;
            filmGrain = profile.AddSettings<Grain>();
            filmGrain.active = false;
            filmGrain.intensity.Override(0.25f);
            screenSpaceReflections = profile.AddSettings<ScreenSpaceReflections>();
            screenSpaceReflections.active = false;
            vignette = profile.AddSettings<Vignette>();
            vignette.active = false;
            vignette.rounded.Override(x: true);
            if (!viewmodel)
            {
                profile.AddSettings<SkyFog>();
            }
        }
    }

    public const int BASE_LAYER = 8;

    public const int VIEWMODEL_LAYER = 11;

    private bool _disableAntiAliasingForScreenshot;

    public Texture dirtTexture;

    private PostProcessProfileWrapper baseProfile;

    private PostProcessProfileWrapper viewmodelProfile;

    private PostProcessLayer basePostProcess;

    private PostProcessLayer viewmodelPostProcess;

    public bool DisableAntiAliasingForScreenshot
    {
        get
        {
            return _disableAntiAliasingForScreenshot;
        }
        set
        {
            if (_disableAntiAliasingForScreenshot != value)
            {
                _disableAntiAliasingForScreenshot = value;
                if (basePostProcess != null)
                {
                    applyAntiAliasing(basePostProcess);
                }
            }
        }
    }

    public static UnturnedPostProcess instance { get; private set; }

    private bool hasActiveOverlay
    {
        get
        {
            if (viewmodelPostProcess != null)
            {
                return viewmodelPostProcess.gameObject.activeInHierarchy;
            }
            return false;
        }
    }

    public void setBaseCamera(Camera baseCamera)
    {
        basePostProcess = baseCamera.GetComponent<PostProcessLayer>();
        basePostProcess.fog.enabled = true;
        basePostProcess.fog.excludeSkybox = true;
    }

    public void setOverlayCamera(Camera overlayCamera)
    {
        viewmodelPostProcess = overlayCamera.GetComponent<PostProcessLayer>();
        viewmodelPostProcess.fog.enabled = false;
        viewmodelPostProcess.fog.excludeSkybox = true;
    }

    public void setScopeCamera(Camera scopeCamera)
    {
        PostProcessLayer component = scopeCamera.GetComponent<PostProcessLayer>();
        component.fog.enabled = true;
        component.fog.excludeSkybox = true;
    }

    public void setIsHallucinating(bool isHallucinating)
    {
        baseProfile.colorGrading.active = isHallucinating;
        baseProfile.colorGrading.hueShift.Override(Random.Range(-180f, 180f));
        viewmodelProfile.colorGrading.active = isHallucinating;
        viewmodelProfile.colorGrading.hueShift.Override(Random.Range(-180f, 180f));
        baseProfile.vignette.active = isHallucinating;
    }

    private void tickHallucinationColorGrading(PostProcessProfileWrapper profile, float deltaTime)
    {
        float num = 2.5f;
        float value = profile.colorGrading.hueShift.value;
        value += deltaTime * num;
        if (value > 180f)
        {
            value -= 360f;
        }
        profile.colorGrading.hueShift.Override(value);
    }

    public void tickIsHallucinating(float deltaTime, float hallucinationTimer)
    {
        tickHallucinationColorGrading(baseProfile, deltaTime);
        tickHallucinationColorGrading(viewmodelProfile, deltaTime);
        float num = 0.333f;
        float num2 = 4f;
        baseProfile.vignette.intensity.Override(Mathf.Abs(Mathf.Sin(hallucinationTimer / num2)) * num);
    }

    /// <summary>
    /// Callback when in-game graphic settings change.
    /// </summary>
    public void applyUserSettings()
    {
        if (basePostProcess != null)
        {
            applyAntiAliasing(basePostProcess);
        }
        syncAmbientOcclusion();
        syncBloom();
        syncChromaticAberration();
        syncFilmGrain();
        syncScreenSpaceReflections();
    }

    /// <summary>
    /// Callback when player changes perspective.
    /// </summary>
    public void notifyPerspectiveChanged()
    {
        syncBloom();
        syncChromaticAberration();
        syncFilmGrain();
    }

    private void syncAmbientOcclusion()
    {
        baseProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
        viewmodelProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
    }

    private void syncBloom()
    {
        if (hasActiveOverlay)
        {
            baseProfile.bloom.active = false;
            viewmodelProfile.bloom.active = GraphicsSettings.bloom;
        }
        else
        {
            baseProfile.bloom.active = GraphicsSettings.bloom;
            viewmodelProfile.bloom.active = false;
        }
    }

    private void syncChromaticAberration()
    {
        if (hasActiveOverlay)
        {
            baseProfile.chromaticAberration.active = false;
            viewmodelProfile.chromaticAberration.active = GraphicsSettings.chromaticAberration;
        }
        else
        {
            baseProfile.chromaticAberration.active = GraphicsSettings.chromaticAberration;
            viewmodelProfile.chromaticAberration.active = false;
        }
    }

    private void syncFilmGrain()
    {
        if (hasActiveOverlay)
        {
            baseProfile.filmGrain.active = false;
            viewmodelProfile.filmGrain.active = GraphicsSettings.filmGrain;
        }
        else
        {
            baseProfile.filmGrain.active = GraphicsSettings.filmGrain;
            viewmodelProfile.filmGrain.active = false;
        }
    }

    private void syncScreenSpaceReflections()
    {
        bool flag = GraphicsSettings.reflectionQuality != 0 && GraphicsSettings.renderMode == ERenderMode.DEFERRED;
        baseProfile.screenSpaceReflections.active = flag;
        if (flag)
        {
            ScreenSpaceReflectionPreset x = GraphicsSettings.reflectionQuality switch
            {
                EGraphicQuality.LOW => ScreenSpaceReflectionPreset.Low, 
                EGraphicQuality.MEDIUM => ScreenSpaceReflectionPreset.Medium, 
                EGraphicQuality.HIGH => ScreenSpaceReflectionPreset.High, 
                EGraphicQuality.ULTRA => ScreenSpaceReflectionPreset.Ultra, 
                _ => ScreenSpaceReflectionPreset.Low, 
            };
            baseProfile.screenSpaceReflections.preset.Override(x);
        }
    }

    private void applyAntiAliasing(PostProcessLayer layer)
    {
        if (_disableAntiAliasingForScreenshot)
        {
            layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
            return;
        }
        switch (GraphicsSettings.antiAliasingType)
        {
        case EAntiAliasingType.OFF:
            layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
            break;
        case EAntiAliasingType.FXAA:
            layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            break;
        case EAntiAliasingType.TAA:
            layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
            break;
        }
    }

    private PostProcessProfileWrapper createGlobalProfile(string name, int layer)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = base.transform;
        obj.layer = layer;
        PostProcessVolume postProcessVolume = obj.AddComponent<PostProcessVolume>();
        postProcessVolume.isGlobal = true;
        postProcessVolume.priority = 1f;
        return new PostProcessProfileWrapper(postProcessVolume.profile, layer == 11);
    }

    public void initialize()
    {
        if (Dedicator.IsDedicatedServer)
        {
            Object.Destroy(base.gameObject);
            return;
        }
        instance = this;
        Object.DontDestroyOnLoad(this);
        baseProfile = createGlobalProfile("Base", 8);
        viewmodelProfile = createGlobalProfile("Viewmodel", 11);
        viewmodelProfile.ambientOcclusion.intensity.Override(1f);
        if (Provider.preferenceData.Graphics.Use_Lens_Dirt)
        {
            baseProfile.bloom.dirtTexture.Override(dirtTexture);
            baseProfile.bloom.dirtIntensity.Override(1f);
            viewmodelProfile.bloom.dirtTexture.Override(dirtTexture);
            viewmodelProfile.bloom.dirtIntensity.Override(1f);
        }
        baseProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
        viewmodelProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
    }
}
