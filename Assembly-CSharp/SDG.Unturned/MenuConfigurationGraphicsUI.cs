using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationGraphicsUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton defaultButton;

    private static ISleekScrollView graphicsBox;

    private static ISleekToggle ambientOcclusionToggle;

    private static ISleekToggle bloomToggle;

    private static ISleekToggle chromaticAberrationToggle;

    private static ISleekToggle filmGrainToggle;

    private static ISleekToggle blendToggle;

    private static ISleekToggle grassDisplacementToggle;

    private static ISleekToggle foliageFocusToggle;

    private static ISleekToggle ragdollsToggle;

    private static ISleekToggle debrisToggle;

    private static ISleekToggle blastToggle;

    private static ISleekToggle puddleToggle;

    private static ISleekToggle glitterToggle;

    private static ISleekToggle triplanarToggle;

    private static ISleekToggle skyboxReflectionToggle;

    private static ISleekToggle itemIconAntiAliasingToggle;

    private static ISleekSlider farClipDistanceSlider;

    private static ISleekSlider distanceSlider;

    private static ISleekSlider landmarkSlider;

    private static SleekButtonState landmarkButton;

    public static SleekButtonState antiAliasingButton;

    public static SleekButtonState anisotropicFilteringButton;

    private static SleekButtonState effectButton;

    private static SleekBoxIcon foliagePerf;

    private static SleekButtonState foliageButton;

    private static SleekButtonState sunShaftsButton;

    private static SleekButtonState lightingButton;

    private static SleekButtonState ambientOcclusionButton;

    private static SleekButtonState reflectionButton;

    private static SleekButtonState planarReflectionButton;

    private static SleekButtonState waterButton;

    private static SleekBoxIcon waterPerf;

    private static SleekButtonState scopeButton;

    private static SleekBoxIcon scopePerf;

    private static SleekButtonState outlineButton;

    private static SleekButtonState terrainButton;

    private static SleekButtonState windButton;

    private static SleekButtonState treeModeButton;

    private static SleekBoxIcon treePerf;

    private static SleekButtonState renderButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            MenuSettings.SaveGraphicsIfLoaded();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onToggledAmbientOcclusion(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.isAmbientOcclusionEnabled = state;
        GraphicsSettings.apply("changed ambient occlusion");
    }

    private static void onToggledBloomToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.bloom = state;
        GraphicsSettings.apply("changed bloom");
    }

    private static void onToggledChromaticAberrationToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.chromaticAberration = state;
        GraphicsSettings.apply("changed chromatic aberration");
    }

    private static void onToggledFilmGrainToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.filmGrain = state;
        GraphicsSettings.apply("changed film grain");
    }

    private static void onToggledBlendToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.blend = state;
        GraphicsSettings.apply("changed blend");
    }

    private static void onToggledGrassDisplacementToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.grassDisplacement = state;
        GraphicsSettings.apply("changed grass displacement");
    }

    private static void onToggledFoliageFocusToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.foliageFocus = state;
        GraphicsSettings.apply("changed foliage focus");
    }

    private static void onSwappedLandmarkState(SleekButtonState button, int index)
    {
        GraphicsSettings.landmarkQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed landmark quality");
        updatePerfWarnings();
    }

    private static void onToggledRagdollsToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.ragdolls = state;
        GraphicsSettings.apply("changed ragdolls");
    }

    private static void onToggledDebrisToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.debris = state;
        GraphicsSettings.apply("changed debris");
    }

    private static void onToggledBlastToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.blast = state;
        GraphicsSettings.apply("changed blastmarks");
    }

    private static void onToggledPuddleToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.puddle = state;
        GraphicsSettings.apply("changed puddles");
    }

    private static void onToggledGlitterToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.glitter = state;
        GraphicsSettings.apply("changed glitter");
    }

    private static void onToggledTriplanarToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.triplanar = state;
        GraphicsSettings.apply("changed triplanar");
    }

    private static void onToggledSkyboxReflectionToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.skyboxReflection = state;
        GraphicsSettings.apply("changed skybox reflection");
    }

    private static void onToggledItemIconAntiAliasingToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.IsItemIconAntiAliasingEnabled = state;
        GraphicsSettings.apply("changed item icon anti-aliasing");
    }

    private static void OnDraggedFarClipDistanceSlider(ISleekSlider slider, float state)
    {
        GraphicsSettings.NormalizedFarClipDistance = state;
        GraphicsSettings.apply("changed far clip distance");
        farClipDistanceSlider.UpdateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(state * 150f)));
    }

    private static void onDraggedDistanceSlider(ISleekSlider slider, float state)
    {
        GraphicsSettings.normalizedDrawDistance = state;
        GraphicsSettings.apply("changed draw distance");
        distanceSlider.UpdateLabel(localization.format("Distance_Slider_Label", 25 + (int)(state * 75f)));
    }

    private static void onDraggedLandmarkSlider(ISleekSlider slider, float state)
    {
        GraphicsSettings.normalizedLandmarkDrawDistance = state;
        GraphicsSettings.apply("changed landmark draw distance");
        landmarkSlider.UpdateLabel(localization.format("Landmark_Slider_Label", (int)(state * 100f)));
    }

    private static void onSwappedAntiAliasingState(SleekButtonState button, int index)
    {
        GraphicsSettings.antiAliasingType = (EAntiAliasingType)index;
        GraphicsSettings.apply("changed anti-aliasing type");
    }

    private static void onSwappedAnisotropicFilteringState(SleekButtonState button, int index)
    {
        GraphicsSettings.anisotropicFilteringMode = (EAnisotropicFilteringMode)index;
        GraphicsSettings.apply("changed anisotropic filtering mode");
    }

    private static void onSwappedEffectState(SleekButtonState button, int index)
    {
        GraphicsSettings.effectQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed effect quality");
    }

    private static void onSwappedFoliageState(SleekButtonState button, int index)
    {
        GraphicsSettings.foliageQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed foliage quality");
        updatePerfWarnings();
    }

    private static void onSwappedSunShaftsState(SleekButtonState button, int index)
    {
        GraphicsSettings.sunShaftsQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed sun shafts quality");
    }

    private static void onSwappedLightingState(SleekButtonState button, int index)
    {
        GraphicsSettings.lightingQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed lighting quality");
    }

    private static void onSwappedReflectionState(SleekButtonState button, int index)
    {
        GraphicsSettings.reflectionQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed reflection quality");
    }

    private static void onSwappedPlanarReflectionState(SleekButtonState button, int index)
    {
        GraphicsSettings.planarReflectionQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed planar reflection quality");
        updatePerfWarnings();
    }

    private static void onSwappedWaterState(SleekButtonState button, int index)
    {
        GraphicsSettings.waterQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed water quality");
        updatePerfWarnings();
    }

    private static void onSwappedScopeState(SleekButtonState button, int index)
    {
        GraphicsSettings.scopeQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed scope quality");
        updatePerfWarnings();
    }

    private static void onSwappedOutlineState(SleekButtonState button, int index)
    {
        GraphicsSettings.outlineQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed outline quality");
    }

    private static void onSwappedTerrainState(SleekButtonState button, int index)
    {
        GraphicsSettings.terrainQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed terrain quality");
    }

    private static void onSwappedWindState(SleekButtonState button, int index)
    {
        GraphicsSettings.windQuality = (EGraphicQuality)index;
        GraphicsSettings.apply("changed wind quality");
    }

    private static void onSwappedTreeModeState(SleekButtonState button, int index)
    {
        GraphicsSettings.treeMode = (ETreeGraphicMode)index;
        updatePerfWarnings();
    }

    private static void onSwappedRenderState(SleekButtonState button, int index)
    {
        GraphicsSettings.renderMode = (ERenderMode)index;
        GraphicsSettings.apply("changed render mode");
        updatePerfWarnings();
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        if (Player.player != null)
        {
            PlayerPauseUI.open();
        }
        else if (Level.isEditor)
        {
            EditorPauseUI.open();
        }
        else
        {
            MenuConfigurationUI.open();
        }
        close();
    }

    private static void onClickedDefaultButton(ISleekElement button)
    {
        GraphicsSettings.restoreDefaults();
        updateAll();
    }

    private static void updateAll()
    {
        ambientOcclusionToggle.Value = GraphicsSettings.isAmbientOcclusionEnabled;
        bloomToggle.Value = GraphicsSettings.bloom;
        chromaticAberrationToggle.Value = GraphicsSettings.chromaticAberration;
        filmGrainToggle.Value = GraphicsSettings.filmGrain;
        grassDisplacementToggle.Value = GraphicsSettings.grassDisplacement;
        foliageFocusToggle.Value = GraphicsSettings.foliageFocus;
        landmarkButton.state = (int)GraphicsSettings.landmarkQuality;
        ragdollsToggle.Value = GraphicsSettings.ragdolls;
        debrisToggle.Value = GraphicsSettings.debris;
        blastToggle.Value = GraphicsSettings.blast;
        puddleToggle.Value = GraphicsSettings.puddle;
        glitterToggle.Value = GraphicsSettings.glitter;
        triplanarToggle.Value = GraphicsSettings.triplanar;
        skyboxReflectionToggle.Value = GraphicsSettings.skyboxReflection;
        itemIconAntiAliasingToggle.Value = GraphicsSettings.IsItemIconAntiAliasingEnabled;
        farClipDistanceSlider.Value = GraphicsSettings.NormalizedFarClipDistance;
        farClipDistanceSlider.UpdateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150f)));
        distanceSlider.Value = GraphicsSettings.normalizedDrawDistance;
        distanceSlider.UpdateLabel(localization.format("Distance_Slider_Label", 25 + (int)(GraphicsSettings.normalizedDrawDistance * 75f)));
        landmarkSlider.Value = GraphicsSettings.normalizedLandmarkDrawDistance;
        landmarkSlider.UpdateLabel(localization.format("Landmark_Slider_Label", (int)(GraphicsSettings.normalizedLandmarkDrawDistance * 100f)));
        antiAliasingButton.state = (int)GraphicsSettings.antiAliasingType;
        anisotropicFilteringButton.state = (int)GraphicsSettings.anisotropicFilteringMode;
        effectButton.state = (int)(GraphicsSettings.effectQuality - 1);
        foliageButton.state = (int)(GraphicsSettings.foliageQuality - 1);
        sunShaftsButton.state = (int)GraphicsSettings.sunShaftsQuality;
        lightingButton.state = (int)GraphicsSettings.lightingQuality;
        reflectionButton.state = (int)GraphicsSettings.reflectionQuality;
        planarReflectionButton.state = (int)(GraphicsSettings.planarReflectionQuality - 1);
        waterButton.state = (int)(GraphicsSettings.waterQuality - 1);
        scopeButton.state = (int)GraphicsSettings.scopeQuality;
        outlineButton.state = (int)(GraphicsSettings.outlineQuality - 1);
        terrainButton.state = (int)(GraphicsSettings.terrainQuality - 1);
        windButton.state = (int)GraphicsSettings.windQuality;
        treeModeButton.state = (int)GraphicsSettings.treeMode;
        renderButton.state = (int)GraphicsSettings.renderMode;
        updatePerfWarnings();
    }

    private static void updatePerfWarnings()
    {
        landmarkSlider.IsInteractable = GraphicsSettings.landmarkQuality != EGraphicQuality.OFF;
        foliagePerf.IsVisible = !SystemInfo.supportsInstancing;
        grassDisplacementToggle.IsInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;
        foliageFocusToggle.IsInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;
        waterPerf.IsVisible = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
        planarReflectionButton.isInteractable = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
        scopePerf.IsVisible = GraphicsSettings.scopeQuality != EGraphicQuality.OFF;
        treePerf.IsVisible = GraphicsSettings.treeMode != ETreeGraphicMode.LEGACY;
        reflectionButton.isInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;
        blastToggle.IsInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;
    }

    public MenuConfigurationGraphicsUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationGraphics.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Configuration/MenuConfigurationGraphics/MenuConfigurationGraphics.unity3d");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        if (Provider.isConnected)
        {
            PlayerUI.container.AddChild(container);
        }
        else if (Level.isEditor)
        {
            EditorUI.window.AddChild(container);
        }
        else
        {
            MenuUI.container.AddChild(container);
        }
        Color32 color = new Color32(240, 240, 240, byte.MaxValue);
        Color32 color2 = new Color32(180, 180, 180, byte.MaxValue);
        active = false;
        graphicsBox = Glazier.Get().CreateScrollView();
        graphicsBox.PositionOffset_X = -425f;
        graphicsBox.PositionOffset_Y = 100f;
        graphicsBox.PositionScale_X = 0.5f;
        graphicsBox.SizeOffset_X = 680f;
        graphicsBox.SizeOffset_Y = -200f;
        graphicsBox.SizeScale_Y = 1f;
        graphicsBox.ScaleContentToWidth = true;
        container.AddChild(graphicsBox);
        int num = 0;
        farClipDistanceSlider = Glazier.Get().CreateSlider();
        farClipDistanceSlider.PositionOffset_X = 205f;
        farClipDistanceSlider.PositionOffset_Y = num;
        farClipDistanceSlider.SizeOffset_X = 200f;
        farClipDistanceSlider.SizeOffset_Y = 20f;
        farClipDistanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
        farClipDistanceSlider.AddLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150f)), ESleekSide.RIGHT);
        farClipDistanceSlider.OnValueChanged += OnDraggedFarClipDistanceSlider;
        graphicsBox.AddChild(farClipDistanceSlider);
        num += 30;
        farClipDistanceSlider.SideLabel.SizeOffset_X += 100f;
        distanceSlider = Glazier.Get().CreateSlider();
        distanceSlider.PositionOffset_X = 205f;
        distanceSlider.PositionOffset_Y = num;
        distanceSlider.SizeOffset_X = 200f;
        distanceSlider.SizeOffset_Y = 20f;
        distanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
        distanceSlider.AddLabel(localization.format("Distance_Slider_Label", 25 + (int)(GraphicsSettings.normalizedDrawDistance * 75f)), ESleekSide.RIGHT);
        distanceSlider.OnValueChanged += onDraggedDistanceSlider;
        graphicsBox.AddChild(distanceSlider);
        num += 30;
        distanceSlider.SideLabel.SizeOffset_X += 100f;
        landmarkButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        landmarkButton.PositionOffset_X = 205f;
        landmarkButton.PositionOffset_Y = num;
        landmarkButton.SizeOffset_X = 200f;
        landmarkButton.SizeOffset_Y = 30f;
        landmarkButton.AddLabel(localization.format("Landmark_Button_Label"), ESleekSide.RIGHT);
        landmarkButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Landmark_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Landmark_Low", localization.format("Low")) + "\n" + localization.format("Landmark_Medium", localization.format("Medium")) + "\n" + localization.format("Landmark_High", localization.format("High")) + "\n" + localization.format("Landmark_Ultra", localization.format("Ultra")), color2);
        landmarkButton.onSwappedState = onSwappedLandmarkState;
        graphicsBox.AddChild(landmarkButton);
        num += 40;
        landmarkSlider = Glazier.Get().CreateSlider();
        landmarkSlider.PositionOffset_X = 205f;
        landmarkSlider.PositionOffset_Y = num;
        landmarkSlider.SizeOffset_X = 200f;
        landmarkSlider.SizeOffset_Y = 20f;
        landmarkSlider.Orientation = ESleekOrientation.HORIZONTAL;
        landmarkSlider.AddLabel(localization.format("Landmark_Slider_Label", 25 + (int)(GraphicsSettings.normalizedLandmarkDrawDistance * 75f)), ESleekSide.RIGHT);
        landmarkSlider.OnValueChanged += onDraggedLandmarkSlider;
        graphicsBox.AddChild(landmarkSlider);
        num += 30;
        landmarkSlider.SideLabel.SizeOffset_X += 100f;
        ragdollsToggle = Glazier.Get().CreateToggle();
        ragdollsToggle.PositionOffset_X = 205f;
        ragdollsToggle.PositionOffset_Y = num;
        ragdollsToggle.SizeOffset_X = 40f;
        ragdollsToggle.SizeOffset_Y = 40f;
        ragdollsToggle.AddLabel(localization.format("Ragdolls_Toggle_Label"), ESleekSide.RIGHT);
        ragdollsToggle.TooltipText = localization.format("Ragdolls_Tooltip");
        ragdollsToggle.OnValueChanged += onToggledRagdollsToggle;
        graphicsBox.AddChild(ragdollsToggle);
        num += 50;
        debrisToggle = Glazier.Get().CreateToggle();
        debrisToggle.PositionOffset_X = 205f;
        debrisToggle.PositionOffset_Y = num;
        debrisToggle.SizeOffset_X = 40f;
        debrisToggle.SizeOffset_Y = 40f;
        debrisToggle.AddLabel(localization.format("Debris_Toggle_Label"), ESleekSide.RIGHT);
        debrisToggle.TooltipText = localization.format("Debris_Tooltip");
        debrisToggle.OnValueChanged += onToggledDebrisToggle;
        graphicsBox.AddChild(debrisToggle);
        num += 50;
        ambientOcclusionToggle = Glazier.Get().CreateToggle();
        ambientOcclusionToggle.PositionOffset_X = 205f;
        ambientOcclusionToggle.PositionOffset_Y = num;
        ambientOcclusionToggle.SizeOffset_X = 40f;
        ambientOcclusionToggle.SizeOffset_Y = 40f;
        ambientOcclusionToggle.AddLabel(localization.format("Ambient_Occlusion_Label"), ESleekSide.RIGHT);
        ambientOcclusionToggle.TooltipText = localization.format("Ambient_Occlusion_Tooltip");
        ambientOcclusionToggle.OnValueChanged += onToggledAmbientOcclusion;
        graphicsBox.AddChild(ambientOcclusionToggle);
        num += 50;
        bloomToggle = Glazier.Get().CreateToggle();
        bloomToggle.PositionOffset_X = 205f;
        bloomToggle.PositionOffset_Y = num;
        bloomToggle.SizeOffset_X = 40f;
        bloomToggle.SizeOffset_Y = 40f;
        bloomToggle.AddLabel(localization.format("Bloom_Toggle_Label"), ESleekSide.RIGHT);
        bloomToggle.TooltipText = localization.format("Bloom_Tooltip");
        bloomToggle.OnValueChanged += onToggledBloomToggle;
        graphicsBox.AddChild(bloomToggle);
        num += 50;
        filmGrainToggle = Glazier.Get().CreateToggle();
        filmGrainToggle.PositionOffset_X = 205f;
        filmGrainToggle.PositionOffset_Y = num;
        filmGrainToggle.SizeOffset_X = 40f;
        filmGrainToggle.SizeOffset_Y = 40f;
        filmGrainToggle.AddLabel(localization.format("Film_Grain_Toggle_Label"), ESleekSide.RIGHT);
        filmGrainToggle.TooltipText = localization.format("Film_Grain_Tooltip");
        filmGrainToggle.OnValueChanged += onToggledFilmGrainToggle;
        graphicsBox.AddChild(filmGrainToggle);
        num += 50;
        blendToggle = Glazier.Get().CreateToggle();
        blendToggle.PositionOffset_X = 205f;
        blendToggle.PositionOffset_Y = num;
        blendToggle.SizeOffset_X = 40f;
        blendToggle.SizeOffset_Y = 40f;
        blendToggle.AddLabel(localization.format("Blend_Toggle_Label"), ESleekSide.RIGHT);
        blendToggle.TooltipText = localization.format("Blend_Tooltip");
        blendToggle.Value = GraphicsSettings.blend;
        blendToggle.OnValueChanged += onToggledBlendToggle;
        graphicsBox.AddChild(blendToggle);
        num += 50;
        grassDisplacementToggle = Glazier.Get().CreateToggle();
        grassDisplacementToggle.PositionOffset_X = 205f;
        grassDisplacementToggle.PositionOffset_Y = num;
        grassDisplacementToggle.SizeOffset_X = 40f;
        grassDisplacementToggle.SizeOffset_Y = 40f;
        grassDisplacementToggle.AddLabel(localization.format("Grass_Displacement_Toggle_Label"), ESleekSide.RIGHT);
        grassDisplacementToggle.TooltipText = localization.format("Grass_Displacement_Tooltip");
        grassDisplacementToggle.OnValueChanged += onToggledGrassDisplacementToggle;
        graphicsBox.AddChild(grassDisplacementToggle);
        num += 50;
        foliageFocusToggle = Glazier.Get().CreateToggle();
        foliageFocusToggle.PositionOffset_X = 205f;
        foliageFocusToggle.PositionOffset_Y = num;
        foliageFocusToggle.SizeOffset_X = 40f;
        foliageFocusToggle.SizeOffset_Y = 40f;
        foliageFocusToggle.AddLabel(localization.format("Foliage_Focus_Toggle_Label"), ESleekSide.RIGHT);
        foliageFocusToggle.OnValueChanged += onToggledFoliageFocusToggle;
        foliageFocusToggle.TooltipText = localization.format("Foliage_Focus_Tooltip");
        graphicsBox.AddChild(foliageFocusToggle);
        num += 50;
        blastToggle = Glazier.Get().CreateToggle();
        blastToggle.PositionOffset_X = 205f;
        blastToggle.PositionOffset_Y = num;
        blastToggle.SizeOffset_X = 40f;
        blastToggle.SizeOffset_Y = 40f;
        blastToggle.AddLabel(localization.format("Blast_Toggle_Label"), ESleekSide.RIGHT);
        blastToggle.TooltipText = localization.format("Blast_Toggle_Tooltip");
        blastToggle.OnValueChanged += onToggledBlastToggle;
        graphicsBox.AddChild(blastToggle);
        num += 50;
        puddleToggle = Glazier.Get().CreateToggle();
        puddleToggle.PositionOffset_X = 205f;
        puddleToggle.PositionOffset_Y = num;
        puddleToggle.SizeOffset_X = 40f;
        puddleToggle.SizeOffset_Y = 40f;
        puddleToggle.AddLabel(localization.format("Puddle_Toggle_Label"), ESleekSide.RIGHT);
        puddleToggle.TooltipText = localization.format("Puddle_Tooltip");
        puddleToggle.OnValueChanged += onToggledPuddleToggle;
        graphicsBox.AddChild(puddleToggle);
        num += 50;
        glitterToggle = Glazier.Get().CreateToggle();
        glitterToggle.PositionOffset_X = 205f;
        glitterToggle.PositionOffset_Y = num;
        glitterToggle.SizeOffset_X = 40f;
        glitterToggle.SizeOffset_Y = 40f;
        glitterToggle.AddLabel(localization.format("Glitter_Toggle_Label"), ESleekSide.RIGHT);
        glitterToggle.TooltipText = localization.format("Glitter_Tooltip");
        glitterToggle.OnValueChanged += onToggledGlitterToggle;
        graphicsBox.AddChild(glitterToggle);
        num += 50;
        triplanarToggle = Glazier.Get().CreateToggle();
        triplanarToggle.PositionOffset_X = 205f;
        triplanarToggle.PositionOffset_Y = num;
        triplanarToggle.SizeOffset_X = 40f;
        triplanarToggle.SizeOffset_Y = 40f;
        triplanarToggle.AddLabel(localization.format("Triplanar_Toggle_Label"), ESleekSide.RIGHT);
        triplanarToggle.TooltipText = localization.format("Triplanar_Tooltip");
        triplanarToggle.OnValueChanged += onToggledTriplanarToggle;
        graphicsBox.AddChild(triplanarToggle);
        num += 50;
        skyboxReflectionToggle = Glazier.Get().CreateToggle();
        skyboxReflectionToggle.PositionOffset_X = 205f;
        skyboxReflectionToggle.PositionOffset_Y = num;
        skyboxReflectionToggle.SizeOffset_X = 40f;
        skyboxReflectionToggle.SizeOffset_Y = 40f;
        skyboxReflectionToggle.AddLabel(localization.format("Skybox_Reflection_Label"), ESleekSide.RIGHT);
        skyboxReflectionToggle.TooltipText = localization.format("Skybox_Reflection_Tooltip");
        skyboxReflectionToggle.OnValueChanged += onToggledSkyboxReflectionToggle;
        graphicsBox.AddChild(skyboxReflectionToggle);
        num += 50;
        itemIconAntiAliasingToggle = Glazier.Get().CreateToggle();
        itemIconAntiAliasingToggle.PositionOffset_X = 205f;
        itemIconAntiAliasingToggle.PositionOffset_Y = num;
        itemIconAntiAliasingToggle.SizeOffset_X = 40f;
        itemIconAntiAliasingToggle.SizeOffset_Y = 40f;
        itemIconAntiAliasingToggle.AddLabel(localization.format("Item_Icon_Anti_Aliasing_Label"), ESleekSide.RIGHT);
        itemIconAntiAliasingToggle.TooltipText = localization.format("Item_Icon_Anti_Aliasing_Tooltip");
        itemIconAntiAliasingToggle.OnValueChanged += onToggledItemIconAntiAliasingToggle;
        graphicsBox.AddChild(itemIconAntiAliasingToggle);
        num += 50;
        chromaticAberrationToggle = Glazier.Get().CreateToggle();
        chromaticAberrationToggle.PositionOffset_X = 205f;
        chromaticAberrationToggle.PositionOffset_Y = num;
        chromaticAberrationToggle.SizeOffset_X = 40f;
        chromaticAberrationToggle.SizeOffset_Y = 40f;
        chromaticAberrationToggle.AddLabel(localization.format("Chromatic_Aberration_Toggle_Label"), ESleekSide.RIGHT);
        chromaticAberrationToggle.TooltipText = localization.format("Chromatic_Aberration_Tooltip");
        chromaticAberrationToggle.OnValueChanged += onToggledChromaticAberrationToggle;
        graphicsBox.AddChild(chromaticAberrationToggle);
        num += 50;
        antiAliasingButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("FXAA")), new GUIContent(localization.format("TAA")));
        antiAliasingButton.PositionOffset_X = 205f;
        antiAliasingButton.PositionOffset_Y = num;
        antiAliasingButton.SizeOffset_X = 200f;
        antiAliasingButton.SizeOffset_Y = 30f;
        antiAliasingButton.AddLabel(localization.format("Anti_Aliasing_Button_Label"), ESleekSide.RIGHT);
        antiAliasingButton.tooltip = localization.format("Anti_Aliasing_Button_Tooltip");
        antiAliasingButton.onSwappedState = onSwappedAntiAliasingState;
        graphicsBox.AddChild(antiAliasingButton);
        num += 40;
        anisotropicFilteringButton = new SleekButtonState(new GUIContent(localization.format("AF_Disabled")), new GUIContent(localization.format("AF_Per_Texture")), new GUIContent(localization.format("AF_Forced_On")));
        anisotropicFilteringButton.PositionOffset_X = 205f;
        anisotropicFilteringButton.PositionOffset_Y = num;
        anisotropicFilteringButton.SizeOffset_X = 200f;
        anisotropicFilteringButton.SizeOffset_Y = 30f;
        anisotropicFilteringButton.AddLabel(localization.format("Anisotropic_Filtering_Button_Label"), ESleekSide.RIGHT);
        anisotropicFilteringButton.tooltip = localization.format("Anisotropic_Filtering_Button_Tooltip");
        anisotropicFilteringButton.onSwappedState = onSwappedAnisotropicFilteringState;
        graphicsBox.AddChild(anisotropicFilteringButton);
        num += 40;
        effectButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        effectButton.PositionOffset_X = 205f;
        effectButton.PositionOffset_Y = num;
        effectButton.SizeOffset_X = 200f;
        effectButton.SizeOffset_Y = 30f;
        effectButton.AddLabel(localization.format("Effect_Button_Label"), ESleekSide.RIGHT);
        effectButton.tooltip = localization.format("Effect_Button_Tooltip");
        effectButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Effect_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Effect_Tier", localization.format("Low"), 16f) + "\n" + localization.format("Effect_Tier", localization.format("Medium"), 32f) + "\n" + localization.format("Effect_Tier", localization.format("High"), 48f) + "\n" + localization.format("Effect_Tier", localization.format("Ultra"), 64f), color2);
        effectButton.onSwappedState = onSwappedEffectState;
        graphicsBox.AddChild(effectButton);
        num += 40;
        foliageButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        foliageButton.PositionOffset_X = 205f;
        foliageButton.PositionOffset_Y = num;
        foliageButton.SizeOffset_X = 200f;
        foliageButton.SizeOffset_Y = 30f;
        foliageButton.AddLabel(localization.format("Foliage_Button_Label"), ESleekSide.RIGHT);
        foliageButton.tooltip = localization.format("Foliage_Button_Tooltip");
        foliageButton.onSwappedState = onSwappedFoliageState;
        graphicsBox.AddChild(foliageButton);
        foliagePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        foliagePerf.PositionOffset_X = 175f;
        foliagePerf.PositionOffset_Y = num;
        foliagePerf.SizeOffset_X = 30f;
        foliagePerf.SizeOffset_Y = 30f;
        foliagePerf.iconColor = ESleekTint.FOREGROUND;
        foliagePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Foliage_Instancing_Not_Supported"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(foliagePerf);
        num += 40;
        sunShaftsButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        sunShaftsButton.PositionOffset_X = 205f;
        sunShaftsButton.PositionOffset_Y = num;
        sunShaftsButton.SizeOffset_X = 200f;
        sunShaftsButton.SizeOffset_Y = 30f;
        sunShaftsButton.AddLabel(localization.format("Sun_Shafts_Button_Label"), ESleekSide.RIGHT);
        sunShaftsButton.tooltip = localization.format("Sun_Shafts_Button_Tooltip");
        sunShaftsButton.onSwappedState = onSwappedSunShaftsState;
        graphicsBox.AddChild(sunShaftsButton);
        num += 40;
        lightingButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        lightingButton.PositionOffset_X = 205f;
        lightingButton.PositionOffset_Y = num;
        lightingButton.SizeOffset_X = 200f;
        lightingButton.SizeOffset_Y = 30f;
        lightingButton.AddLabel(localization.format("Lighting_Button_Label"), ESleekSide.RIGHT);
        lightingButton.tooltip = localization.format("Lighting_Button_Tooltip");
        lightingButton.onSwappedState = onSwappedLightingState;
        graphicsBox.AddChild(lightingButton);
        num += 40;
        reflectionButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        reflectionButton.PositionOffset_X = 205f;
        reflectionButton.PositionOffset_Y = num;
        reflectionButton.SizeOffset_X = 200f;
        reflectionButton.SizeOffset_Y = 30f;
        reflectionButton.AddLabel(localization.format("Reflection_Button_Label"), ESleekSide.RIGHT);
        reflectionButton.tooltip = localization.format("Reflection_Button_Tooltip");
        reflectionButton.onSwappedState = onSwappedReflectionState;
        graphicsBox.AddChild(reflectionButton);
        num += 40;
        waterButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        waterButton.PositionOffset_X = 205f;
        waterButton.PositionOffset_Y = num;
        waterButton.SizeOffset_X = 200f;
        waterButton.SizeOffset_Y = 30f;
        waterButton.AddLabel(localization.format("Water_Button_Label"), ESleekSide.RIGHT);
        waterButton.tooltip = localization.format("Water_Button_Tooltip");
        waterButton.onSwappedState = onSwappedWaterState;
        graphicsBox.AddChild(waterButton);
        waterPerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        waterPerf.PositionOffset_X = 175f;
        waterPerf.PositionOffset_Y = num;
        waterPerf.SizeOffset_X = 30f;
        waterPerf.SizeOffset_Y = 30f;
        waterPerf.iconColor = ESleekTint.FOREGROUND;
        waterPerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Water_Reflections"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(waterPerf);
        num += 40;
        planarReflectionButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        planarReflectionButton.PositionOffset_X = 205f;
        planarReflectionButton.PositionOffset_Y = num;
        planarReflectionButton.SizeOffset_X = 200f;
        planarReflectionButton.SizeOffset_Y = 30f;
        planarReflectionButton.AddLabel(localization.format("Planar_Reflection_Button_Label"), ESleekSide.RIGHT);
        planarReflectionButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Planar_Reflection_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Planar_Reflection_Low", localization.format("Low")) + "\n" + localization.format("Planar_Reflection_Medium", localization.format("Medium")) + "\n" + localization.format("Planar_Reflection_High", localization.format("High")) + "\n" + localization.format("Planar_Reflection_Ultra", localization.format("Ultra")), color2);
        planarReflectionButton.onSwappedState = onSwappedPlanarReflectionState;
        graphicsBox.AddChild(planarReflectionButton);
        num += 40;
        scopeButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        scopeButton.PositionOffset_X = 205f;
        scopeButton.PositionOffset_Y = num;
        scopeButton.SizeOffset_X = 200f;
        scopeButton.SizeOffset_Y = 30f;
        scopeButton.AddLabel(localization.format("Scope_Button_Label"), ESleekSide.RIGHT);
        scopeButton.tooltip = localization.format("Scope_Button_Tooltip");
        scopeButton.onSwappedState = onSwappedScopeState;
        graphicsBox.AddChild(scopeButton);
        scopePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        scopePerf.PositionOffset_X = 175f;
        scopePerf.PositionOffset_Y = num;
        scopePerf.SizeOffset_X = 30f;
        scopePerf.SizeOffset_Y = 30f;
        scopePerf.iconColor = ESleekTint.FOREGROUND;
        scopePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Dual_Render_Scope"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(scopePerf);
        num += 40;
        outlineButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        outlineButton.PositionOffset_X = 205f;
        outlineButton.PositionOffset_Y = num;
        outlineButton.SizeOffset_X = 200f;
        outlineButton.SizeOffset_Y = 30f;
        outlineButton.AddLabel(localization.format("Outline_Button_Label"), ESleekSide.RIGHT);
        outlineButton.tooltip = localization.format("Outline_Button_Tooltip");
        outlineButton.onSwappedState = onSwappedOutlineState;
        graphicsBox.AddChild(outlineButton);
        num += 40;
        terrainButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        terrainButton.PositionOffset_X = 205f;
        terrainButton.PositionOffset_Y = num;
        terrainButton.SizeOffset_X = 200f;
        terrainButton.SizeOffset_Y = 30f;
        terrainButton.AddLabel(localization.format("Terrain_Button_Label"), ESleekSide.RIGHT);
        terrainButton.tooltip = localization.format("Terrain_Button_Tooltip");
        terrainButton.onSwappedState = onSwappedTerrainState;
        graphicsBox.AddChild(terrainButton);
        num += 40;
        windButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        windButton.PositionOffset_X = 205f;
        windButton.PositionOffset_Y = num;
        windButton.SizeOffset_X = 200f;
        windButton.SizeOffset_Y = 30f;
        windButton.AddLabel(localization.format("Wind_Button_Label"), ESleekSide.RIGHT);
        windButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Wind_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Wind_Low", localization.format("Low")) + "\n" + localization.format("Wind_Medium", localization.format("Medium")), color2);
        windButton.onSwappedState = onSwappedWindState;
        graphicsBox.AddChild(windButton);
        num += 40;
        treeModeButton = new SleekButtonState(new GUIContent(localization.format("TM_Legacy")), new GUIContent(localization.format("TM_SpeedTree_Fade_None")), new GUIContent(localization.format("TM_SpeedTree_Fade_SpeedTree")));
        treeModeButton.PositionOffset_X = 205f;
        treeModeButton.PositionOffset_Y = num;
        treeModeButton.SizeOffset_X = 200f;
        treeModeButton.SizeOffset_Y = 30f;
        treeModeButton.AddLabel(localization.format("Tree_Mode_Button_Label"), ESleekSide.RIGHT);
        treeModeButton.tooltip = localization.format("Tree_Mode_Button_Tooltip");
        treeModeButton.onSwappedState = onSwappedTreeModeState;
        graphicsBox.AddChild(treeModeButton);
        treePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        treePerf.PositionOffset_X = 175f;
        treePerf.PositionOffset_Y = num;
        treePerf.SizeOffset_X = 30f;
        treePerf.SizeOffset_Y = 30f;
        treePerf.iconColor = ESleekTint.FOREGROUND;
        treePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_SpeedTrees"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(treePerf);
        num += 40;
        renderButton = new SleekButtonState(new GUIContent(localization.format("Deferred")), new GUIContent(localization.format("Forward")));
        renderButton.PositionOffset_X = 205f;
        renderButton.PositionOffset_Y = num;
        renderButton.SizeOffset_X = 200f;
        renderButton.SizeOffset_Y = 30f;
        renderButton.AddLabel(localization.format("Render_Mode_Button_Label"), ESleekSide.RIGHT);
        renderButton.tooltip = localization.format("Render_Mode_Button_Tooltip");
        renderButton.onSwappedState = onSwappedRenderState;
        graphicsBox.AddChild(renderButton);
        num += 40;
        graphicsBox.ContentSizeOffset = new Vector2(0f, num - 10);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.PositionOffset_X = -200f;
        defaultButton.PositionOffset_Y = -50f;
        defaultButton.PositionScale_X = 1f;
        defaultButton.PositionScale_Y = 1f;
        defaultButton.SizeOffset_X = 200f;
        defaultButton.SizeOffset_Y = 50f;
        defaultButton.Text = MenuPlayConfigUI.localization.format("Default");
        defaultButton.TooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
        defaultButton.OnClicked += onClickedDefaultButton;
        defaultButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        updateAll();
    }
}
