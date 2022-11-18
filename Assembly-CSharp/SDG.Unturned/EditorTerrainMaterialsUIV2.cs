using System;
using System.Collections.Generic;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using SDG.Framework.Landscapes;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class EditorTerrainMaterialsUIV2 : SleekFullscreenBox
{
    private Local localization;

    private LandscapeMaterialAsset selectedMaterialAsset;

    private List<LandscapeMaterialAsset> searchAssets;

    private ISleekLabel hintLabel;

    private SleekButtonState modeButton;

    private ISleekFloat32Field brushRadiusField;

    private ISleekFloat32Field brushFalloffField;

    private ISleekFloat32Field brushStrengthField;

    private ISleekToggle useWeightTargetToggle;

    private ISleekFloat32Field weightTargetField;

    private ISleekUInt32Field maxPreviewSamplesField;

    private SleekButtonState smoothMethodButton;

    private SleekButtonState previewMethodButton;

    private ISleekToggle highlightHolesToggle;

    private ISleekToggle useAutoSlopeToggle;

    private ISleekFloat32Field autoMinAngleBeginField;

    private ISleekFloat32Field autoMinAngleEndField;

    private ISleekFloat32Field autoMaxAngleBeginField;

    private ISleekFloat32Field autoMaxAngleEndField;

    private ISleekToggle useAutoFoundationToggle;

    private ISleekFloat32Field autoRayRadiusField;

    private ISleekFloat32Field autoRayLengthField;

    private ISleekUInt32Field autoRayMaskField;

    private SleekBoxIcon selectedAssetBox;

    private ISleekToggle onlyUsedMaterialsToggle;

    private ISleekField searchField;

    private ISleekScrollView assetScrollView;

    public void Open()
    {
        AnimateIntoView();
        TerrainEditorV2.toolMode = TerrainEditorV2.EDevkitLandscapeToolMode.SPLATMAP;
        EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);
        if (FoliageSystem.instance != null)
        {
            FoliageSystem.instance.hiddenByMaterialEditor = true;
        }
        RefreshAssets();
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        DevkitLandscapeToolSplatmapOptions.save();
        EditorInteract.instance.SetActiveTool(null);
        if (FoliageSystem.instance != null)
        {
            FoliageSystem.instance.hiddenByMaterialEditor = false;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        modeButton.state = (int)TerrainEditorV2.splatmapMode;
        brushRadiusField.state = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
        brushFalloffField.state = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
        brushStrengthField.state = EditorInteract.instance.terrainTool.splatmapBrushStrength;
        weightTargetField.state = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
        LandscapeMaterialAsset landscapeMaterialAsset = TerrainEditorV2.splatmapMaterialTarget.Find();
        if (selectedMaterialAsset != landscapeMaterialAsset)
        {
            selectedMaterialAsset = landscapeMaterialAsset;
            if (selectedMaterialAsset != null)
            {
                selectedAssetBox.icon = Assets.load(selectedMaterialAsset.texture);
                selectedAssetBox.text = selectedMaterialAsset.FriendlyName;
            }
            else
            {
                selectedAssetBox.icon = null;
                selectedAssetBox.text = string.Empty;
            }
        }
        if (TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT)
        {
            hintLabel.text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
            hintLabel.isVisible = true;
        }
        else if (TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.CUT)
        {
            hintLabel.text = localization.format("Hint_Cut", "Shift");
            hintLabel.isVisible = true;
        }
        else
        {
            hintLabel.isVisible = false;
        }
        UpdateLowerLeftOffset();
    }

    public EditorTerrainMaterialsUIV2()
    {
        localization = Localization.read("/Editor/EditorTerrainMaterials.dat");
        DevkitLandscapeToolSplatmapOptions.load();
        searchAssets = new List<LandscapeMaterialAsset>();
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.positionScale_Y = 1f;
        hintLabel.positionOffset_Y = -30;
        hintLabel.sizeScale_X = 1f;
        hintLabel.sizeOffset_Y = 30;
        hintLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        hintLabel.isVisible = false;
        AddChild(hintLabel);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")), new GUIContent(localization.format("Mode_Auto", "W")), new GUIContent(localization.format("Mode_Smooth", "E")), new GUIContent(localization.format("Mode_Cut", "R")));
        modeButton.positionScale_Y = 1f;
        modeButton.sizeOffset_X = 200;
        modeButton.sizeOffset_Y = 30;
        modeButton.addLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)TerrainEditorV2.splatmapMode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.positionScale_Y = 1f;
        brushRadiusField.sizeOffset_X = 200;
        brushRadiusField.sizeOffset_Y = 30;
        brushRadiusField.addLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.state = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
        brushRadiusField.onTypedSingle += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.positionScale_Y = 1f;
        brushFalloffField.sizeOffset_X = 200;
        brushFalloffField.sizeOffset_Y = 30;
        brushFalloffField.addLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.state = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
        brushFalloffField.onTypedSingle += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.positionScale_Y = 1f;
        brushStrengthField.sizeOffset_X = 200;
        brushStrengthField.sizeOffset_Y = 30;
        brushStrengthField.addLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.state = DevkitLandscapeToolSplatmapOptions.instance.brushStrength;
        brushStrengthField.onTypedSingle += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
        smoothMethodButton.positionScale_Y = 1f;
        smoothMethodButton.sizeOffset_X = 200;
        smoothMethodButton.sizeOffset_Y = 30;
        smoothMethodButton.addLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
        smoothMethodButton.state = (int)DevkitLandscapeToolSplatmapOptions.instance.smoothMethod;
        SleekButtonState sleekButtonState2 = smoothMethodButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSmoothMethod));
        AddChild(smoothMethodButton);
        autoRayMaskField = Glazier.Get().CreateUInt32Field();
        autoRayMaskField.positionScale_Y = 1f;
        autoRayMaskField.sizeOffset_X = 200;
        autoRayMaskField.sizeOffset_Y = 30;
        autoRayMaskField.addLabel("Ray Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        autoRayMaskField.state = (uint)DevkitLandscapeToolSplatmapOptions.instance.autoRayMask;
        autoRayMaskField.onTypedUInt32 += OnAutoRayMaskTyped;
        AddChild(autoRayMaskField);
        autoRayLengthField = Glazier.Get().CreateFloat32Field();
        autoRayLengthField.positionScale_Y = 1f;
        autoRayLengthField.sizeOffset_X = 200;
        autoRayLengthField.sizeOffset_Y = 30;
        autoRayLengthField.addLabel(localization.format("AutoRayLength"), ESleekSide.RIGHT);
        autoRayLengthField.state = DevkitLandscapeToolSplatmapOptions.instance.autoRayLength;
        autoRayLengthField.onTypedSingle += OnAutoRayLengthTyped;
        AddChild(autoRayLengthField);
        autoRayRadiusField = Glazier.Get().CreateFloat32Field();
        autoRayRadiusField.positionScale_Y = 1f;
        autoRayRadiusField.sizeOffset_X = 200;
        autoRayRadiusField.sizeOffset_Y = 30;
        autoRayRadiusField.addLabel(localization.format("AutoRayRadius"), ESleekSide.RIGHT);
        autoRayRadiusField.state = DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius;
        autoRayRadiusField.onTypedSingle += OnAutoRayRadiusTyped;
        AddChild(autoRayRadiusField);
        useAutoFoundationToggle = Glazier.Get().CreateToggle();
        useAutoFoundationToggle.positionScale_Y = 1f;
        useAutoFoundationToggle.sizeOffset_X = 40;
        useAutoFoundationToggle.sizeOffset_Y = 40;
        useAutoFoundationToggle.state = DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
        useAutoFoundationToggle.onToggled += OnClickedUseAutoFoundation;
        useAutoFoundationToggle.addLabel(localization.format("UseAutoFoundation"), ESleekSide.RIGHT);
        AddChild(useAutoFoundationToggle);
        autoMaxAngleBeginField = Glazier.Get().CreateFloat32Field();
        autoMaxAngleBeginField.positionScale_Y = 1f;
        autoMaxAngleBeginField.sizeOffset_X = 100;
        autoMaxAngleBeginField.sizeOffset_Y = 30;
        autoMaxAngleBeginField.state = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin;
        autoMaxAngleBeginField.onTypedSingle += OnAutoMaxAngleBeginTyped;
        AddChild(autoMaxAngleBeginField);
        autoMaxAngleEndField = Glazier.Get().CreateFloat32Field();
        autoMaxAngleEndField.positionOffset_X = 100;
        autoMaxAngleEndField.positionScale_Y = 1f;
        autoMaxAngleEndField.sizeOffset_X = 100;
        autoMaxAngleEndField.sizeOffset_Y = 30;
        autoMaxAngleEndField.state = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd;
        autoMaxAngleEndField.onTypedSingle += OnAutoMaxAngleEndTyped;
        autoMaxAngleEndField.addLabel(localization.format("MaxAngleRange"), ESleekSide.RIGHT);
        AddChild(autoMaxAngleEndField);
        autoMinAngleBeginField = Glazier.Get().CreateFloat32Field();
        autoMinAngleBeginField.positionScale_Y = 1f;
        autoMinAngleBeginField.sizeOffset_X = 100;
        autoMinAngleBeginField.sizeOffset_Y = 30;
        autoMinAngleBeginField.state = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin;
        autoMinAngleBeginField.onTypedSingle += OnAutoMinAngleBeginTyped;
        AddChild(autoMinAngleBeginField);
        autoMinAngleEndField = Glazier.Get().CreateFloat32Field();
        autoMinAngleEndField.positionOffset_X = 100;
        autoMinAngleEndField.positionScale_Y = 1f;
        autoMinAngleEndField.sizeOffset_X = 100;
        autoMinAngleEndField.sizeOffset_Y = 30;
        autoMinAngleEndField.state = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd;
        autoMinAngleEndField.onTypedSingle += OnAutoMinAngleEndTyped;
        autoMinAngleEndField.addLabel(localization.format("MinAngleRange"), ESleekSide.RIGHT);
        AddChild(autoMinAngleEndField);
        useAutoSlopeToggle = Glazier.Get().CreateToggle();
        useAutoSlopeToggle.positionScale_Y = 1f;
        useAutoSlopeToggle.sizeOffset_X = 40;
        useAutoSlopeToggle.sizeOffset_Y = 40;
        useAutoSlopeToggle.state = DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
        useAutoSlopeToggle.onToggled += OnClickedUseAutoSlope;
        useAutoSlopeToggle.addLabel(localization.format("UseAutoSlope"), ESleekSide.RIGHT);
        AddChild(useAutoSlopeToggle);
        useWeightTargetToggle = Glazier.Get().CreateToggle();
        useWeightTargetToggle.positionScale_Y = 1f;
        useWeightTargetToggle.sizeOffset_X = 40;
        useWeightTargetToggle.sizeOffset_Y = 40;
        useWeightTargetToggle.state = DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget;
        useWeightTargetToggle.onToggled += OnClickedUseWeightTarget;
        AddChild(useWeightTargetToggle);
        weightTargetField = Glazier.Get().CreateFloat32Field();
        weightTargetField.positionOffset_X = 40;
        weightTargetField.positionScale_Y = 1f;
        weightTargetField.sizeOffset_X = 160;
        weightTargetField.sizeOffset_Y = 30;
        weightTargetField.state = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
        weightTargetField.addLabel(localization.format("WeightTarget", "G"), ESleekSide.RIGHT);
        weightTargetField.onTypedSingle += OnWeightTargetTyped;
        AddChild(weightTargetField);
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.positionScale_Y = 1f;
        maxPreviewSamplesField.sizeOffset_X = 200;
        maxPreviewSamplesField.sizeOffset_Y = 30;
        maxPreviewSamplesField.addLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.state = DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.onTypedUInt32 += OnMaxPreviewSamplesTyped;
        AddChild(maxPreviewSamplesField);
        previewMethodButton = new SleekButtonState(new GUIContent(localization.format("PreviewMethod_BrushAlpha")), new GUIContent(localization.format("PreviewMethod_Weight")));
        previewMethodButton.positionScale_Y = 1f;
        previewMethodButton.sizeOffset_X = 200;
        previewMethodButton.sizeOffset_Y = 30;
        previewMethodButton.addLabel(localization.format("PreviewMethod_Label"), ESleekSide.RIGHT);
        previewMethodButton.state = (int)DevkitLandscapeToolSplatmapOptions.instance.previewMethod;
        SleekButtonState sleekButtonState3 = previewMethodButton;
        sleekButtonState3.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState3.onSwappedState, new SwappedState(OnSwappedPreviewMethod));
        AddChild(previewMethodButton);
        highlightHolesToggle = Glazier.Get().CreateToggle();
        highlightHolesToggle.positionScale_Y = 1f;
        highlightHolesToggle.sizeOffset_X = 40;
        highlightHolesToggle.sizeOffset_Y = 40;
        highlightHolesToggle.onToggled += OnClickedHighlightHoles;
        highlightHolesToggle.isVisible = false;
        highlightHolesToggle.addLabel(localization.format("HighlightHoles_Label"), ESleekSide.RIGHT);
        AddChild(highlightHolesToggle);
        UpdateLowerLeftOffset();
        int num = 0;
        selectedAssetBox = new SleekBoxIcon(null, 64);
        selectedAssetBox.positionScale_X = 1f;
        selectedAssetBox.sizeOffset_X = 300;
        selectedAssetBox.positionOffset_X = -selectedAssetBox.sizeOffset_X;
        selectedAssetBox.sizeOffset_Y = 74;
        selectedAssetBox.addLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
        AddChild(selectedAssetBox);
        num += selectedAssetBox.sizeOffset_Y + 10;
        onlyUsedMaterialsToggle = Glazier.Get().CreateToggle();
        onlyUsedMaterialsToggle.positionScale_X = 1f;
        onlyUsedMaterialsToggle.sizeOffset_X = 40;
        onlyUsedMaterialsToggle.positionOffset_X = -300;
        onlyUsedMaterialsToggle.sizeOffset_Y = 40;
        onlyUsedMaterialsToggle.positionOffset_Y = num;
        onlyUsedMaterialsToggle.addLabel(localization.format("OnlyUsedMaterials"), ESleekSide.RIGHT);
        onlyUsedMaterialsToggle.state = true;
        onlyUsedMaterialsToggle.onToggled += OnClickedOnlyUsedMaterials;
        AddChild(onlyUsedMaterialsToggle);
        num += onlyUsedMaterialsToggle.sizeOffset_Y + 10;
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_X = -300;
        searchField.positionOffset_Y = num;
        searchField.positionScale_X = 1f;
        searchField.sizeOffset_X = 300;
        searchField.sizeOffset_Y = 30;
        searchField.hint = localization.format("SearchHint");
        searchField.onEntered += OnNameFilterEntered;
        AddChild(searchField);
        num += searchField.sizeOffset_Y + 10;
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.positionScale_X = 1f;
        assetScrollView.sizeOffset_X = 300;
        assetScrollView.positionOffset_X = -assetScrollView.sizeOffset_X;
        assetScrollView.positionOffset_Y = num;
        assetScrollView.sizeOffset_Y = -num;
        assetScrollView.sizeScale_Y = 1f;
        assetScrollView.scaleContentToWidth = true;
        AddChild(assetScrollView);
    }

    private void OnSwappedMode(SleekButtonState element, int index)
    {
        TerrainEditorV2.splatmapMode = (TerrainEditorV2.EDevkitLandscapeToolSplatmapMode)index;
    }

    private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
    {
        EditorInteract.instance.terrainTool.splatmapBrushStrength = state;
    }

    private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.brushFalloff = state;
    }

    private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.brushRadius = state;
    }

    private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples = state;
    }

    private void OnClickedUseWeightTarget(ISleekToggle toggle, bool state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget = state;
    }

    private void OnWeightTargetTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.weightTarget = state;
    }

    private void OnSwappedSmoothMethod(SleekButtonState element, int index)
    {
        DevkitLandscapeToolSplatmapOptions.instance.smoothMethod = (EDevkitLandscapeToolSplatmapSmoothMethod)index;
    }

    private void OnSwappedPreviewMethod(SleekButtonState element, int index)
    {
        DevkitLandscapeToolSplatmapOptions.instance.previewMethod = (EDevkitLandscapeToolSplatmapPreviewMethod)index;
    }

    private void OnClickedUseAutoSlope(ISleekToggle toggle, bool state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope = state;
    }

    private void OnAutoMinAngleBeginTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin = state;
    }

    private void OnAutoMinAngleEndTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd = state;
    }

    private void OnAutoMaxAngleBeginTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin = state;
    }

    private void OnAutoMaxAngleEndTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd = state;
    }

    private void OnClickedUseAutoFoundation(ISleekToggle toggle, bool state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation = state;
    }

    private void OnAutoRayRadiusTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius = state;
    }

    private void OnAutoRayLengthTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoRayLength = state;
    }

    private void OnAutoRayMaskTyped(ISleekUInt32Field field, uint state)
    {
        DevkitLandscapeToolSplatmapOptions.instance.autoRayMask = (ERayMask)state;
    }

    private void OnClickedHighlightHoles(ISleekToggle toggle, bool state)
    {
        Landscape.HighlightHoles = state;
    }

    private void OnClickedOnlyUsedMaterials(ISleekToggle toggle, bool state)
    {
        RefreshAssets();
    }

    private void OnNameFilterEntered(ISleekField field)
    {
        RefreshAssets();
    }

    private void OnAssetClicked(ISleekElement button)
    {
        int index = assetScrollView.FindIndexOfChild(button);
        TerrainEditorV2.splatmapMaterialTarget = new AssetReference<LandscapeMaterialAsset>(searchAssets[index].GUID);
    }

    private void RefreshAssets()
    {
        searchAssets.Clear();
        assetScrollView.RemoveAllChildren();
        int num = 0;
        if (onlyUsedMaterialsToggle.state)
        {
            Landscape.GetUniqueMaterials(searchAssets);
        }
        else
        {
            Assets.find(searchAssets);
        }
        string searchText = searchField.text;
        if (!string.IsNullOrEmpty(searchText))
        {
            searchAssets.RemoveSwap((LandscapeMaterialAsset asset) => asset.FriendlyName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) == -1);
        }
        searchAssets.Sort((LandscapeMaterialAsset lhs, LandscapeMaterialAsset rhs) => lhs.name.CompareTo(rhs.name));
        foreach (LandscapeMaterialAsset searchAsset in searchAssets)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(Assets.load(searchAsset.texture), 64);
            sleekButtonIcon.positionOffset_Y = num;
            sleekButtonIcon.sizeScale_X = 1f;
            sleekButtonIcon.sizeOffset_Y = 74;
            sleekButtonIcon.text = searchAsset.FriendlyName;
            sleekButtonIcon.onClickedButton += OnAssetClicked;
            assetScrollView.AddChild(sleekButtonIcon);
            num += sleekButtonIcon.sizeOffset_Y;
        }
        assetScrollView.contentSizeOffset = new Vector2(0f, num);
    }

    private void UpdateLowerLeftOffset()
    {
        int num = 0;
        num -= modeButton.sizeOffset_Y;
        modeButton.positionOffset_Y = num;
        num -= 10;
        num -= previewMethodButton.sizeOffset_Y;
        previewMethodButton.positionOffset_Y = num;
        num -= 10;
        num -= maxPreviewSamplesField.sizeOffset_Y;
        maxPreviewSamplesField.positionOffset_Y = num;
        num -= 10;
        smoothMethodButton.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.SMOOTH;
        if (smoothMethodButton.isVisible)
        {
            num -= smoothMethodButton.sizeOffset_Y;
            smoothMethodButton.positionOffset_Y = num;
            num -= 10;
        }
        autoRayRadiusField.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT && DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
        autoRayLengthField.isVisible = autoRayRadiusField.isVisible;
        autoRayMaskField.isVisible = autoRayRadiusField.isVisible;
        if (autoRayRadiusField.isVisible)
        {
            num -= autoRayMaskField.sizeOffset_Y;
            autoRayMaskField.positionOffset_Y = num;
            num -= autoRayLengthField.sizeOffset_Y;
            autoRayLengthField.positionOffset_Y = num;
            num -= autoRayRadiusField.sizeOffset_Y;
            autoRayRadiusField.positionOffset_Y = num;
        }
        useAutoFoundationToggle.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT;
        if (useAutoFoundationToggle.isVisible)
        {
            num -= useAutoFoundationToggle.sizeOffset_Y;
            useAutoFoundationToggle.positionOffset_Y = num;
            num -= 10;
        }
        autoMinAngleBeginField.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT && DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
        autoMinAngleEndField.isVisible = autoMinAngleBeginField.isVisible;
        autoMaxAngleBeginField.isVisible = autoMinAngleBeginField.isVisible;
        autoMaxAngleEndField.isVisible = autoMinAngleBeginField.isVisible;
        if (autoMinAngleBeginField.isVisible)
        {
            num -= autoMaxAngleBeginField.sizeOffset_Y;
            autoMaxAngleBeginField.positionOffset_Y = num;
            autoMaxAngleEndField.positionOffset_Y = num;
            num -= autoMinAngleBeginField.sizeOffset_Y;
            autoMinAngleBeginField.positionOffset_Y = num;
            autoMinAngleEndField.positionOffset_Y = num;
        }
        useAutoSlopeToggle.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT;
        if (useAutoSlopeToggle.isVisible)
        {
            num -= useAutoSlopeToggle.sizeOffset_Y;
            useAutoSlopeToggle.positionOffset_Y = num;
            num -= 10;
        }
        useWeightTargetToggle.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.PAINT;
        weightTargetField.isVisible = useWeightTargetToggle.isVisible;
        if (useWeightTargetToggle.isVisible)
        {
            num -= useWeightTargetToggle.sizeOffset_Y;
            useWeightTargetToggle.positionOffset_Y = num;
            weightTargetField.positionOffset_Y = num + 5;
            num -= 10;
        }
        brushStrengthField.isVisible = TerrainEditorV2.splatmapMode != TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.CUT;
        brushFalloffField.isVisible = brushStrengthField.isVisible;
        if (brushStrengthField.isVisible)
        {
            num -= brushStrengthField.sizeOffset_Y;
            brushStrengthField.positionOffset_Y = num;
            num -= 10;
            num -= brushFalloffField.sizeOffset_Y;
            brushFalloffField.positionOffset_Y = num;
            num -= 10;
        }
        num -= brushRadiusField.sizeOffset_Y;
        brushRadiusField.positionOffset_Y = num;
        num -= 10;
        highlightHolesToggle.isVisible = TerrainEditorV2.splatmapMode == TerrainEditorV2.EDevkitLandscapeToolSplatmapMode.CUT;
        if (highlightHolesToggle.isVisible)
        {
            num -= highlightHolesToggle.sizeOffset_Y;
            highlightHolesToggle.positionOffset_Y = num;
        }
    }
}
