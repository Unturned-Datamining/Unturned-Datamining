using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

/// <summary>
/// Manages global post-process volumes.
/// </summary>
public class UnturnedPostProcess : MonoBehaviour
{
    private enum EPostProcessLayer
    {
        Base,
        Viewmodel,
        Scope
    }

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

        public DepthOfField dof;

        public SrScope singleRenderScope;

        public PostProcessProfileWrapper(PostProcessProfile profile, EPostProcessLayer layer)
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
            if (layer == EPostProcessLayer.Base)
            {
                dof = profile.AddSettings<DepthOfField>();
                dof.active = false;
                dof.focusDistance.Override(1f);
            }
            if (layer != EPostProcessLayer.Viewmodel)
            {
                profile.AddSettings<SkyFog>();
            }
            if (layer == EPostProcessLayer.Base)
            {
                singleRenderScope = profile.AddSettings<SrScope>();
                singleRenderScope.active = false;
            }
        }
    }

    public const int BASE_LAYER = 8;

    public const int VIEWMODEL_LAYER = 11;

    public const int SCOPE_LAYER = 31;

    private bool _disableAntiAliasingForScreenshot;

    public Texture dirtTexture;

    private PostProcessProfileWrapper baseProfile;

    private PostProcessProfileWrapper viewmodelProfile;

    private PostProcessProfileWrapper scopeProfile;

    private PostProcessLayer basePostProcessLayer;

    private PostProcessLayer viewmodelPostProcessLayer;

    private PostProcessLayer scopePostProcessLayer;

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
                if (basePostProcessLayer != null)
                {
                    applyAntiAliasing(basePostProcessLayer);
                }
                if (scopePostProcessLayer != null)
                {
                    applyAntiAliasing(scopePostProcessLayer);
                }
            }
        }
    }

    public static UnturnedPostProcess instance { get; private set; }

    private bool hasActiveOverlay
    {
        get
        {
            if (viewmodelPostProcessLayer != null)
            {
                return viewmodelPostProcessLayer.gameObject.activeInHierarchy;
            }
            return false;
        }
    }

    public void setBaseCamera(Camera baseCamera)
    {
        basePostProcessLayer = baseCamera.GetComponent<PostProcessLayer>();
        basePostProcessLayer.fog.enabled = true;
        basePostProcessLayer.fog.excludeSkybox = true;
    }

    public void setOverlayCamera(Camera overlayCamera)
    {
        viewmodelPostProcessLayer = overlayCamera.GetComponent<PostProcessLayer>();
        viewmodelPostProcessLayer.fog.enabled = false;
        viewmodelPostProcessLayer.fog.excludeSkybox = true;
    }

    public void setScopeCamera(Camera scopeCamera)
    {
        scopePostProcessLayer = scopeCamera.GetComponent<PostProcessLayer>();
        scopePostProcessLayer.fog.enabled = true;
        scopePostProcessLayer.fog.excludeSkybox = true;
    }

    public void SetSingleRenderScopeIsActive(bool isActive)
    {
        baseProfile.singleRenderScope.active = isActive;
    }

    public void SetSingleRenderScopeZoomFactor(float zoomFactor)
    {
        if (zoomFactor > 1.0001f)
        {
            baseProfile.singleRenderScope.standardDeviation.Override(Mathf.Min(zoomFactor, 32f));
        }
        else
        {
            baseProfile.singleRenderScope.standardDeviation.Override(-1f);
        }
    }

    public void SetSingleRenderScopeTarget(RenderTexture target)
    {
        baseProfile.singleRenderScope.renderTarget.Override(target);
    }

    public void setIsHallucinating(bool isHallucinating)
    {
        baseProfile.colorGrading.active = isHallucinating;
        baseProfile.colorGrading.hueShift.Override(Random.Range(-180f, 180f));
        viewmodelProfile.colorGrading.active = isHallucinating;
        viewmodelProfile.colorGrading.hueShift.Override(Random.Range(-180f, 180f));
        scopeProfile.colorGrading.active = isHallucinating;
        scopeProfile.colorGrading.hueShift.Override(Random.Range(-180f, 180f));
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
        tickHallucinationColorGrading(scopeProfile, deltaTime);
        float num = 0.333f;
        float num2 = 4f;
        baseProfile.vignette.intensity.Override(Mathf.Abs(Mathf.Sin(hallucinationTimer / num2)) * num);
    }

    public void SetIsMainBlurEnabled(bool enabled)
    {
        baseProfile.dof.active = enabled;
    }

    /// <summary>
    /// Callback when in-game graphic settings change.
    /// </summary>
    public void applyUserSettings()
    {
        if (basePostProcessLayer != null)
        {
            applyAntiAliasing(basePostProcessLayer);
        }
        if (scopePostProcessLayer != null)
        {
            applyAntiAliasing(scopePostProcessLayer);
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
        scopeProfile.ambientOcclusion.active = GraphicsSettings.isAmbientOcclusionEnabled;
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
        scopeProfile.bloom.active = false;
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
        scopeProfile.chromaticAberration.active = false;
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
        scopeProfile.filmGrain.active = false;
    }

    private void syncScreenSpaceReflections()
    {
        bool flag = GraphicsSettings.reflectionQuality != 0 && GraphicsSettings.renderMode == ERenderMode.DEFERRED;
        baseProfile.screenSpaceReflections.active = flag;
        scopeProfile.screenSpaceReflections.active = false;
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

    private PostProcessProfileWrapper createGlobalProfile(string name, int physicsLayer, EPostProcessLayer layer)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = base.transform;
        obj.layer = physicsLayer;
        PostProcessVolume postProcessVolume = obj.AddComponent<PostProcessVolume>();
        postProcessVolume.isGlobal = true;
        postProcessVolume.priority = 1f;
        return new PostProcessProfileWrapper(postProcessVolume.profile, layer);
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
        baseProfile = createGlobalProfile("Base", 8, EPostProcessLayer.Base);
        viewmodelProfile = createGlobalProfile("Viewmodel", 11, EPostProcessLayer.Viewmodel);
        scopeProfile = createGlobalProfile("Scope", 31, EPostProcessLayer.Scope);
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
        scopeProfile.chromaticAberration.intensity.Override(Provider.preferenceData.Graphics.Chromatic_Aberration_Intensity);
    }
}
