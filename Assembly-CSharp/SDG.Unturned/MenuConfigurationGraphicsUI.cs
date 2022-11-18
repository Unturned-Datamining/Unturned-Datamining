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

    private static SleekButtonState boneButton;

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
        farClipDistanceSlider.updateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(state * 150f)));
    }

    private static void onDraggedDistanceSlider(ISleekSlider slider, float state)
    {
        GraphicsSettings.normalizedDrawDistance = state;
        GraphicsSettings.apply("changed draw distance");
        distanceSlider.updateLabel(localization.format("Distance_Slider_Label", 25 + (int)(state * 75f)));
    }

    private static void onDraggedLandmarkSlider(ISleekSlider slider, float state)
    {
        GraphicsSettings.normalizedLandmarkDrawDistance = state;
        GraphicsSettings.apply("changed landmark draw distance");
        landmarkSlider.updateLabel(localization.format("Landmark_Slider_Label", (int)(state * 100f)));
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

    private static void onSwappedBoneState(SleekButtonState button, int index)
    {
        GraphicsSettings.boneQuality = (EGraphicQuality)(index + 1);
        GraphicsSettings.apply("changed bone quality");
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
        ambientOcclusionToggle.state = GraphicsSettings.isAmbientOcclusionEnabled;
        bloomToggle.state = GraphicsSettings.bloom;
        chromaticAberrationToggle.state = GraphicsSettings.chromaticAberration;
        filmGrainToggle.state = GraphicsSettings.filmGrain;
        grassDisplacementToggle.state = GraphicsSettings.grassDisplacement;
        foliageFocusToggle.state = GraphicsSettings.foliageFocus;
        landmarkButton.state = (int)GraphicsSettings.landmarkQuality;
        ragdollsToggle.state = GraphicsSettings.ragdolls;
        debrisToggle.state = GraphicsSettings.debris;
        blastToggle.state = GraphicsSettings.blast;
        puddleToggle.state = GraphicsSettings.puddle;
        glitterToggle.state = GraphicsSettings.glitter;
        triplanarToggle.state = GraphicsSettings.triplanar;
        skyboxReflectionToggle.state = GraphicsSettings.skyboxReflection;
        itemIconAntiAliasingToggle.state = GraphicsSettings.IsItemIconAntiAliasingEnabled;
        farClipDistanceSlider.state = GraphicsSettings.NormalizedFarClipDistance;
        farClipDistanceSlider.updateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150f)));
        distanceSlider.state = GraphicsSettings.normalizedDrawDistance;
        distanceSlider.updateLabel(localization.format("Distance_Slider_Label", 25 + (int)(GraphicsSettings.normalizedDrawDistance * 75f)));
        landmarkSlider.state = GraphicsSettings.normalizedLandmarkDrawDistance;
        landmarkSlider.updateLabel(localization.format("Landmark_Slider_Label", (int)(GraphicsSettings.normalizedLandmarkDrawDistance * 100f)));
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
        boneButton.state = (int)(GraphicsSettings.boneQuality - 1);
        terrainButton.state = (int)(GraphicsSettings.terrainQuality - 1);
        windButton.state = (int)GraphicsSettings.windQuality;
        treeModeButton.state = (int)GraphicsSettings.treeMode;
        renderButton.state = (int)GraphicsSettings.renderMode;
        updatePerfWarnings();
    }

    private static void updatePerfWarnings()
    {
        landmarkSlider.isInteractable = GraphicsSettings.landmarkQuality != EGraphicQuality.OFF;
        foliagePerf.isVisible = !SystemInfo.supportsInstancing;
        grassDisplacementToggle.isInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;
        foliageFocusToggle.isInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;
        waterPerf.isVisible = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
        planarReflectionButton.isInteractable = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
        scopePerf.isVisible = GraphicsSettings.scopeQuality != EGraphicQuality.OFF;
        treePerf.isVisible = GraphicsSettings.treeMode != ETreeGraphicMode.LEGACY;
        reflectionButton.isInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;
        blastToggle.isInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;
    }

    public MenuConfigurationGraphicsUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationGraphics.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Configuration/MenuConfigurationGraphics/MenuConfigurationGraphics.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
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
        graphicsBox.positionOffset_X = -425;
        graphicsBox.positionOffset_Y = 100;
        graphicsBox.positionScale_X = 0.5f;
        graphicsBox.sizeOffset_X = 680;
        graphicsBox.sizeOffset_Y = -200;
        graphicsBox.sizeScale_Y = 1f;
        graphicsBox.scaleContentToWidth = true;
        container.AddChild(graphicsBox);
        int num = 0;
        farClipDistanceSlider = Glazier.Get().CreateSlider();
        farClipDistanceSlider.positionOffset_X = 205;
        farClipDistanceSlider.positionOffset_Y = num;
        farClipDistanceSlider.sizeOffset_X = 200;
        farClipDistanceSlider.sizeOffset_Y = 20;
        farClipDistanceSlider.orientation = ESleekOrientation.HORIZONTAL;
        farClipDistanceSlider.addLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150f)), ESleekSide.RIGHT);
        farClipDistanceSlider.onDragged += OnDraggedFarClipDistanceSlider;
        graphicsBox.AddChild(farClipDistanceSlider);
        num += 30;
        farClipDistanceSlider.sideLabel.sizeOffset_X += 100;
        distanceSlider = Glazier.Get().CreateSlider();
        distanceSlider.positionOffset_X = 205;
        distanceSlider.positionOffset_Y = num;
        distanceSlider.sizeOffset_X = 200;
        distanceSlider.sizeOffset_Y = 20;
        distanceSlider.orientation = ESleekOrientation.HORIZONTAL;
        distanceSlider.addLabel(localization.format("Distance_Slider_Label", 25 + (int)(GraphicsSettings.normalizedDrawDistance * 75f)), ESleekSide.RIGHT);
        distanceSlider.onDragged += onDraggedDistanceSlider;
        graphicsBox.AddChild(distanceSlider);
        num += 30;
        distanceSlider.sideLabel.sizeOffset_X += 100;
        landmarkButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        landmarkButton.positionOffset_X = 205;
        landmarkButton.positionOffset_Y = num;
        landmarkButton.sizeOffset_X = 200;
        landmarkButton.sizeOffset_Y = 30;
        landmarkButton.addLabel(localization.format("Landmark_Button_Label"), ESleekSide.RIGHT);
        landmarkButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Landmark_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Landmark_Low", localization.format("Low")) + "\n" + localization.format("Landmark_Medium", localization.format("Medium")) + "\n" + localization.format("Landmark_High", localization.format("High")) + "\n" + localization.format("Landmark_Ultra", localization.format("Ultra")), color2);
        landmarkButton.onSwappedState = onSwappedLandmarkState;
        graphicsBox.AddChild(landmarkButton);
        num += 40;
        landmarkSlider = Glazier.Get().CreateSlider();
        landmarkSlider.positionOffset_X = 205;
        landmarkSlider.positionOffset_Y = num;
        landmarkSlider.sizeOffset_X = 200;
        landmarkSlider.sizeOffset_Y = 20;
        landmarkSlider.orientation = ESleekOrientation.HORIZONTAL;
        landmarkSlider.addLabel(localization.format("Landmark_Slider_Label", 25 + (int)(GraphicsSettings.normalizedLandmarkDrawDistance * 75f)), ESleekSide.RIGHT);
        landmarkSlider.onDragged += onDraggedLandmarkSlider;
        graphicsBox.AddChild(landmarkSlider);
        num += 30;
        landmarkSlider.sideLabel.sizeOffset_X += 100;
        ragdollsToggle = Glazier.Get().CreateToggle();
        ragdollsToggle.positionOffset_X = 205;
        ragdollsToggle.positionOffset_Y = num;
        ragdollsToggle.sizeOffset_X = 40;
        ragdollsToggle.sizeOffset_Y = 40;
        ragdollsToggle.addLabel(localization.format("Ragdolls_Toggle_Label"), ESleekSide.RIGHT);
        ragdollsToggle.tooltipText = localization.format("Ragdolls_Tooltip");
        ragdollsToggle.onToggled += onToggledRagdollsToggle;
        graphicsBox.AddChild(ragdollsToggle);
        num += 50;
        debrisToggle = Glazier.Get().CreateToggle();
        debrisToggle.positionOffset_X = 205;
        debrisToggle.positionOffset_Y = num;
        debrisToggle.sizeOffset_X = 40;
        debrisToggle.sizeOffset_Y = 40;
        debrisToggle.addLabel(localization.format("Debris_Toggle_Label"), ESleekSide.RIGHT);
        debrisToggle.tooltipText = localization.format("Debris_Tooltip");
        debrisToggle.onToggled += onToggledDebrisToggle;
        graphicsBox.AddChild(debrisToggle);
        num += 50;
        ambientOcclusionToggle = Glazier.Get().CreateToggle();
        ambientOcclusionToggle.positionOffset_X = 205;
        ambientOcclusionToggle.positionOffset_Y = num;
        ambientOcclusionToggle.sizeOffset_X = 40;
        ambientOcclusionToggle.sizeOffset_Y = 40;
        ambientOcclusionToggle.addLabel(localization.format("Ambient_Occlusion_Label"), ESleekSide.RIGHT);
        ambientOcclusionToggle.tooltipText = localization.format("Ambient_Occlusion_Tooltip");
        ambientOcclusionToggle.onToggled += onToggledAmbientOcclusion;
        graphicsBox.AddChild(ambientOcclusionToggle);
        num += 50;
        bloomToggle = Glazier.Get().CreateToggle();
        bloomToggle.positionOffset_X = 205;
        bloomToggle.positionOffset_Y = num;
        bloomToggle.sizeOffset_X = 40;
        bloomToggle.sizeOffset_Y = 40;
        bloomToggle.addLabel(localization.format("Bloom_Toggle_Label"), ESleekSide.RIGHT);
        bloomToggle.tooltipText = localization.format("Bloom_Tooltip");
        bloomToggle.onToggled += onToggledBloomToggle;
        graphicsBox.AddChild(bloomToggle);
        num += 50;
        filmGrainToggle = Glazier.Get().CreateToggle();
        filmGrainToggle.positionOffset_X = 205;
        filmGrainToggle.positionOffset_Y = num;
        filmGrainToggle.sizeOffset_X = 40;
        filmGrainToggle.sizeOffset_Y = 40;
        filmGrainToggle.addLabel(localization.format("Film_Grain_Toggle_Label"), ESleekSide.RIGHT);
        filmGrainToggle.tooltipText = localization.format("Film_Grain_Tooltip");
        filmGrainToggle.onToggled += onToggledFilmGrainToggle;
        graphicsBox.AddChild(filmGrainToggle);
        num += 50;
        blendToggle = Glazier.Get().CreateToggle();
        blendToggle.positionOffset_X = 205;
        blendToggle.positionOffset_Y = num;
        blendToggle.sizeOffset_X = 40;
        blendToggle.sizeOffset_Y = 40;
        blendToggle.addLabel(localization.format("Blend_Toggle_Label"), ESleekSide.RIGHT);
        blendToggle.tooltipText = localization.format("Blend_Tooltip");
        blendToggle.state = GraphicsSettings.blend;
        blendToggle.onToggled += onToggledBlendToggle;
        graphicsBox.AddChild(blendToggle);
        num += 50;
        grassDisplacementToggle = Glazier.Get().CreateToggle();
        grassDisplacementToggle.positionOffset_X = 205;
        grassDisplacementToggle.positionOffset_Y = num;
        grassDisplacementToggle.sizeOffset_X = 40;
        grassDisplacementToggle.sizeOffset_Y = 40;
        grassDisplacementToggle.addLabel(localization.format("Grass_Displacement_Toggle_Label"), ESleekSide.RIGHT);
        grassDisplacementToggle.tooltipText = localization.format("Grass_Displacement_Tooltip");
        grassDisplacementToggle.onToggled += onToggledGrassDisplacementToggle;
        graphicsBox.AddChild(grassDisplacementToggle);
        num += 50;
        foliageFocusToggle = Glazier.Get().CreateToggle();
        foliageFocusToggle.positionOffset_X = 205;
        foliageFocusToggle.positionOffset_Y = num;
        foliageFocusToggle.sizeOffset_X = 40;
        foliageFocusToggle.sizeOffset_Y = 40;
        foliageFocusToggle.addLabel(localization.format("Foliage_Focus_Toggle_Label"), ESleekSide.RIGHT);
        foliageFocusToggle.onToggled += onToggledFoliageFocusToggle;
        foliageFocusToggle.tooltipText = localization.format("Foliage_Focus_Tooltip");
        graphicsBox.AddChild(foliageFocusToggle);
        num += 50;
        blastToggle = Glazier.Get().CreateToggle();
        blastToggle.positionOffset_X = 205;
        blastToggle.positionOffset_Y = num;
        blastToggle.sizeOffset_X = 40;
        blastToggle.sizeOffset_Y = 40;
        blastToggle.addLabel(localization.format("Blast_Toggle_Label"), ESleekSide.RIGHT);
        blastToggle.tooltipText = localization.format("Blast_Toggle_Tooltip");
        blastToggle.onToggled += onToggledBlastToggle;
        graphicsBox.AddChild(blastToggle);
        num += 50;
        puddleToggle = Glazier.Get().CreateToggle();
        puddleToggle.positionOffset_X = 205;
        puddleToggle.positionOffset_Y = num;
        puddleToggle.sizeOffset_X = 40;
        puddleToggle.sizeOffset_Y = 40;
        puddleToggle.addLabel(localization.format("Puddle_Toggle_Label"), ESleekSide.RIGHT);
        puddleToggle.tooltipText = localization.format("Puddle_Tooltip");
        puddleToggle.onToggled += onToggledPuddleToggle;
        graphicsBox.AddChild(puddleToggle);
        num += 50;
        glitterToggle = Glazier.Get().CreateToggle();
        glitterToggle.positionOffset_X = 205;
        glitterToggle.positionOffset_Y = num;
        glitterToggle.sizeOffset_X = 40;
        glitterToggle.sizeOffset_Y = 40;
        glitterToggle.addLabel(localization.format("Glitter_Toggle_Label"), ESleekSide.RIGHT);
        glitterToggle.tooltipText = localization.format("Glitter_Tooltip");
        glitterToggle.onToggled += onToggledGlitterToggle;
        graphicsBox.AddChild(glitterToggle);
        num += 50;
        triplanarToggle = Glazier.Get().CreateToggle();
        triplanarToggle.positionOffset_X = 205;
        triplanarToggle.positionOffset_Y = num;
        triplanarToggle.sizeOffset_X = 40;
        triplanarToggle.sizeOffset_Y = 40;
        triplanarToggle.addLabel(localization.format("Triplanar_Toggle_Label"), ESleekSide.RIGHT);
        triplanarToggle.tooltipText = localization.format("Triplanar_Tooltip");
        triplanarToggle.onToggled += onToggledTriplanarToggle;
        graphicsBox.AddChild(triplanarToggle);
        num += 50;
        skyboxReflectionToggle = Glazier.Get().CreateToggle();
        skyboxReflectionToggle.positionOffset_X = 205;
        skyboxReflectionToggle.positionOffset_Y = num;
        skyboxReflectionToggle.sizeOffset_X = 40;
        skyboxReflectionToggle.sizeOffset_Y = 40;
        skyboxReflectionToggle.addLabel(localization.format("Skybox_Reflection_Label"), ESleekSide.RIGHT);
        skyboxReflectionToggle.tooltipText = localization.format("Skybox_Reflection_Tooltip");
        skyboxReflectionToggle.onToggled += onToggledSkyboxReflectionToggle;
        graphicsBox.AddChild(skyboxReflectionToggle);
        num += 50;
        itemIconAntiAliasingToggle = Glazier.Get().CreateToggle();
        itemIconAntiAliasingToggle.positionOffset_X = 205;
        itemIconAntiAliasingToggle.positionOffset_Y = num;
        itemIconAntiAliasingToggle.sizeOffset_X = 40;
        itemIconAntiAliasingToggle.sizeOffset_Y = 40;
        itemIconAntiAliasingToggle.addLabel(localization.format("Item_Icon_Anti_Aliasing_Label"), ESleekSide.RIGHT);
        itemIconAntiAliasingToggle.tooltipText = localization.format("Item_Icon_Anti_Aliasing_Tooltip");
        itemIconAntiAliasingToggle.onToggled += onToggledItemIconAntiAliasingToggle;
        graphicsBox.AddChild(itemIconAntiAliasingToggle);
        num += 50;
        chromaticAberrationToggle = Glazier.Get().CreateToggle();
        chromaticAberrationToggle.positionOffset_X = 205;
        chromaticAberrationToggle.positionOffset_Y = num;
        chromaticAberrationToggle.sizeOffset_X = 40;
        chromaticAberrationToggle.sizeOffset_Y = 40;
        chromaticAberrationToggle.addLabel(localization.format("Chromatic_Aberration_Toggle_Label"), ESleekSide.RIGHT);
        chromaticAberrationToggle.tooltipText = localization.format("Chromatic_Aberration_Tooltip");
        chromaticAberrationToggle.onToggled += onToggledChromaticAberrationToggle;
        graphicsBox.AddChild(chromaticAberrationToggle);
        num += 50;
        antiAliasingButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("FXAA")), new GUIContent(localization.format("TAA")));
        antiAliasingButton.positionOffset_X = 205;
        antiAliasingButton.positionOffset_Y = num;
        antiAliasingButton.sizeOffset_X = 200;
        antiAliasingButton.sizeOffset_Y = 30;
        antiAliasingButton.addLabel(localization.format("Anti_Aliasing_Button_Label"), ESleekSide.RIGHT);
        antiAliasingButton.tooltip = localization.format("Anti_Aliasing_Button_Tooltip");
        antiAliasingButton.onSwappedState = onSwappedAntiAliasingState;
        graphicsBox.AddChild(antiAliasingButton);
        num += 40;
        anisotropicFilteringButton = new SleekButtonState(new GUIContent(localization.format("AF_Disabled")), new GUIContent(localization.format("AF_Per_Texture")), new GUIContent(localization.format("AF_Forced_On")));
        anisotropicFilteringButton.positionOffset_X = 205;
        anisotropicFilteringButton.positionOffset_Y = num;
        anisotropicFilteringButton.sizeOffset_X = 200;
        anisotropicFilteringButton.sizeOffset_Y = 30;
        anisotropicFilteringButton.addLabel(localization.format("Anisotropic_Filtering_Button_Label"), ESleekSide.RIGHT);
        anisotropicFilteringButton.tooltip = localization.format("Anisotropic_Filtering_Button_Tooltip");
        anisotropicFilteringButton.onSwappedState = onSwappedAnisotropicFilteringState;
        graphicsBox.AddChild(anisotropicFilteringButton);
        num += 40;
        effectButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        effectButton.positionOffset_X = 205;
        effectButton.positionOffset_Y = num;
        effectButton.sizeOffset_X = 200;
        effectButton.sizeOffset_Y = 30;
        effectButton.addLabel(localization.format("Effect_Button_Label"), ESleekSide.RIGHT);
        effectButton.tooltip = localization.format("Effect_Button_Tooltip");
        effectButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Effect_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Effect_Tier", localization.format("Low"), 16f) + "\n" + localization.format("Effect_Tier", localization.format("Medium"), 32f) + "\n" + localization.format("Effect_Tier", localization.format("High"), 48f) + "\n" + localization.format("Effect_Tier", localization.format("Ultra"), 64f), color2);
        effectButton.onSwappedState = onSwappedEffectState;
        graphicsBox.AddChild(effectButton);
        num += 40;
        foliageButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        foliageButton.positionOffset_X = 205;
        foliageButton.positionOffset_Y = num;
        foliageButton.sizeOffset_X = 200;
        foliageButton.sizeOffset_Y = 30;
        foliageButton.addLabel(localization.format("Foliage_Button_Label"), ESleekSide.RIGHT);
        foliageButton.tooltip = localization.format("Foliage_Button_Tooltip");
        foliageButton.onSwappedState = onSwappedFoliageState;
        graphicsBox.AddChild(foliageButton);
        foliagePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        foliagePerf.positionOffset_X = 175;
        foliagePerf.positionOffset_Y = num;
        foliagePerf.sizeOffset_X = 30;
        foliagePerf.sizeOffset_Y = 30;
        foliagePerf.iconColor = ESleekTint.FOREGROUND;
        foliagePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Foliage_Instancing_Not_Supported"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(foliagePerf);
        num += 40;
        sunShaftsButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        sunShaftsButton.positionOffset_X = 205;
        sunShaftsButton.positionOffset_Y = num;
        sunShaftsButton.sizeOffset_X = 200;
        sunShaftsButton.sizeOffset_Y = 30;
        sunShaftsButton.addLabel(localization.format("Sun_Shafts_Button_Label"), ESleekSide.RIGHT);
        sunShaftsButton.tooltip = localization.format("Sun_Shafts_Button_Tooltip");
        sunShaftsButton.onSwappedState = onSwappedSunShaftsState;
        graphicsBox.AddChild(sunShaftsButton);
        num += 40;
        lightingButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        lightingButton.positionOffset_X = 205;
        lightingButton.positionOffset_Y = num;
        lightingButton.sizeOffset_X = 200;
        lightingButton.sizeOffset_Y = 30;
        lightingButton.addLabel(localization.format("Lighting_Button_Label"), ESleekSide.RIGHT);
        lightingButton.tooltip = localization.format("Lighting_Button_Tooltip");
        lightingButton.onSwappedState = onSwappedLightingState;
        graphicsBox.AddChild(lightingButton);
        num += 40;
        reflectionButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        reflectionButton.positionOffset_X = 205;
        reflectionButton.positionOffset_Y = num;
        reflectionButton.sizeOffset_X = 200;
        reflectionButton.sizeOffset_Y = 30;
        reflectionButton.addLabel(localization.format("Reflection_Button_Label"), ESleekSide.RIGHT);
        reflectionButton.tooltip = localization.format("Reflection_Button_Tooltip");
        reflectionButton.onSwappedState = onSwappedReflectionState;
        graphicsBox.AddChild(reflectionButton);
        num += 40;
        waterButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        waterButton.positionOffset_X = 205;
        waterButton.positionOffset_Y = num;
        waterButton.sizeOffset_X = 200;
        waterButton.sizeOffset_Y = 30;
        waterButton.addLabel(localization.format("Water_Button_Label"), ESleekSide.RIGHT);
        waterButton.tooltip = localization.format("Water_Button_Tooltip");
        waterButton.onSwappedState = onSwappedWaterState;
        graphicsBox.AddChild(waterButton);
        waterPerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        waterPerf.positionOffset_X = 175;
        waterPerf.positionOffset_Y = num;
        waterPerf.sizeOffset_X = 30;
        waterPerf.sizeOffset_Y = 30;
        waterPerf.iconColor = ESleekTint.FOREGROUND;
        waterPerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Water_Reflections"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(waterPerf);
        num += 40;
        planarReflectionButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        planarReflectionButton.positionOffset_X = 205;
        planarReflectionButton.positionOffset_Y = num;
        planarReflectionButton.sizeOffset_X = 200;
        planarReflectionButton.sizeOffset_Y = 30;
        planarReflectionButton.addLabel(localization.format("Planar_Reflection_Button_Label"), ESleekSide.RIGHT);
        planarReflectionButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Planar_Reflection_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Planar_Reflection_Low", localization.format("Low")) + "\n" + localization.format("Planar_Reflection_Medium", localization.format("Medium")) + "\n" + localization.format("Planar_Reflection_High", localization.format("High")) + "\n" + localization.format("Planar_Reflection_Ultra", localization.format("Ultra")), color2);
        planarReflectionButton.onSwappedState = onSwappedPlanarReflectionState;
        graphicsBox.AddChild(planarReflectionButton);
        num += 40;
        scopeButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        scopeButton.positionOffset_X = 205;
        scopeButton.positionOffset_Y = num;
        scopeButton.sizeOffset_X = 200;
        scopeButton.sizeOffset_Y = 30;
        scopeButton.addLabel(localization.format("Scope_Button_Label"), ESleekSide.RIGHT);
        scopeButton.tooltip = localization.format("Scope_Button_Tooltip");
        scopeButton.onSwappedState = onSwappedScopeState;
        graphicsBox.AddChild(scopeButton);
        scopePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        scopePerf.positionOffset_X = 175;
        scopePerf.positionOffset_Y = num;
        scopePerf.sizeOffset_X = 30;
        scopePerf.sizeOffset_Y = 30;
        scopePerf.iconColor = ESleekTint.FOREGROUND;
        scopePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Dual_Render_Scope"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(scopePerf);
        num += 40;
        outlineButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        outlineButton.positionOffset_X = 205;
        outlineButton.positionOffset_Y = num;
        outlineButton.sizeOffset_X = 200;
        outlineButton.sizeOffset_Y = 30;
        outlineButton.addLabel(localization.format("Outline_Button_Label"), ESleekSide.RIGHT);
        outlineButton.tooltip = localization.format("Outline_Button_Tooltip");
        outlineButton.onSwappedState = onSwappedOutlineState;
        graphicsBox.AddChild(outlineButton);
        num += 40;
        boneButton = new SleekButtonState(new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        boneButton.positionOffset_X = 205;
        boneButton.positionOffset_Y = num;
        boneButton.sizeOffset_X = 200;
        boneButton.sizeOffset_Y = 30;
        boneButton.addLabel(localization.format("Bone_Button_Label"), ESleekSide.RIGHT);
        boneButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Bone_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Bone_Influences_Medium", localization.format("Medium")) + "\n" + localization.format("Bone_Influences_High", localization.format("High")) + "\n" + localization.format("Bone_Influences_Ultra", localization.format("Ultra")), color2);
        boneButton.onSwappedState = onSwappedBoneState;
        graphicsBox.AddChild(boneButton);
        num += 40;
        terrainButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        terrainButton.positionOffset_X = 205;
        terrainButton.positionOffset_Y = num;
        terrainButton.sizeOffset_X = 200;
        terrainButton.sizeOffset_Y = 30;
        terrainButton.addLabel(localization.format("Terrain_Button_Label"), ESleekSide.RIGHT);
        terrainButton.tooltip = localization.format("Terrain_Button_Tooltip");
        terrainButton.onSwappedState = onSwappedTerrainState;
        graphicsBox.AddChild(terrainButton);
        num += 40;
        windButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
        windButton.positionOffset_X = 205;
        windButton.positionOffset_Y = num;
        windButton.sizeOffset_X = 200;
        windButton.sizeOffset_Y = 30;
        windButton.addLabel(localization.format("Wind_Button_Label"), ESleekSide.RIGHT);
        windButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Wind_Button_Tooltip"), color) + RichTextUtil.wrapWithColor("\n" + localization.format("Wind_Low", localization.format("Low")) + "\n" + localization.format("Wind_Medium", localization.format("Medium")), color2);
        windButton.onSwappedState = onSwappedWindState;
        graphicsBox.AddChild(windButton);
        num += 40;
        treeModeButton = new SleekButtonState(new GUIContent(localization.format("TM_Legacy")), new GUIContent(localization.format("TM_SpeedTree_Fade_None")), new GUIContent(localization.format("TM_SpeedTree_Fade_SpeedTree")));
        treeModeButton.positionOffset_X = 205;
        treeModeButton.positionOffset_Y = num;
        treeModeButton.sizeOffset_X = 200;
        treeModeButton.sizeOffset_Y = 30;
        treeModeButton.addLabel(localization.format("Tree_Mode_Button_Label"), ESleekSide.RIGHT);
        treeModeButton.tooltip = localization.format("Tree_Mode_Button_Tooltip");
        treeModeButton.onSwappedState = onSwappedTreeModeState;
        graphicsBox.AddChild(treeModeButton);
        treePerf = new SleekBoxIcon(bundle.load<Texture2D>("Perf"));
        treePerf.positionOffset_X = 175;
        treePerf.positionOffset_Y = num;
        treePerf.sizeOffset_X = 30;
        treePerf.sizeOffset_Y = 30;
        treePerf.iconColor = ESleekTint.FOREGROUND;
        treePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_SpeedTrees"), new Color(1f, 0.5f, 0f));
        graphicsBox.AddChild(treePerf);
        num += 40;
        renderButton = new SleekButtonState(new GUIContent(localization.format("Deferred")), new GUIContent(localization.format("Forward")));
        renderButton.positionOffset_X = 205;
        renderButton.positionOffset_Y = num;
        renderButton.sizeOffset_X = 200;
        renderButton.sizeOffset_Y = 30;
        renderButton.addLabel(localization.format("Render_Mode_Button_Label"), ESleekSide.RIGHT);
        renderButton.tooltip = localization.format("Render_Mode_Button_Tooltip");
        renderButton.onSwappedState = onSwappedRenderState;
        graphicsBox.AddChild(renderButton);
        num += 40;
        graphicsBox.contentSizeOffset = new Vector2(0f, num - 10);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.positionOffset_X = -200;
        defaultButton.positionOffset_Y = -50;
        defaultButton.positionScale_X = 1f;
        defaultButton.positionScale_Y = 1f;
        defaultButton.sizeOffset_X = 200;
        defaultButton.sizeOffset_Y = 50;
        defaultButton.text = MenuPlayConfigUI.localization.format("Default");
        defaultButton.tooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
        defaultButton.onClickedButton += onClickedDefaultButton;
        defaultButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        updateAll();
    }
}
