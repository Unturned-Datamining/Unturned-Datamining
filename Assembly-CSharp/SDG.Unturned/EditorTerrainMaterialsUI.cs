using System;
using System.Collections.Generic;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using SDG.Framework.Landscapes;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class EditorTerrainMaterialsUI : SleekFullscreenBox
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
        TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.SPLATMAP;
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
        modeButton.state = (int)TerrainEditor.splatmapMode;
        brushRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
        brushFalloffField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
        brushStrengthField.Value = EditorInteract.instance.terrainTool.splatmapBrushStrength;
        weightTargetField.Value = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
        LandscapeMaterialAsset landscapeMaterialAsset = TerrainEditor.splatmapMaterialTarget.Find();
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
        if (TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT)
        {
            hintLabel.Text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
            hintLabel.IsVisible = true;
        }
        else if (TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT)
        {
            hintLabel.Text = localization.format("Hint_Cut", "Shift");
            hintLabel.IsVisible = true;
        }
        else
        {
            hintLabel.IsVisible = false;
        }
        UpdateLowerLeftOffset();
    }

    public EditorTerrainMaterialsUI()
    {
        localization = Localization.read("/Editor/EditorTerrainMaterials.dat");
        DevkitLandscapeToolSplatmapOptions.load();
        searchAssets = new List<LandscapeMaterialAsset>();
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.PositionScale_Y = 1f;
        hintLabel.PositionOffset_Y = -30f;
        hintLabel.SizeScale_X = 1f;
        hintLabel.SizeOffset_Y = 30f;
        hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        hintLabel.IsVisible = false;
        AddChild(hintLabel);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")), new GUIContent(localization.format("Mode_Auto", "W")), new GUIContent(localization.format("Mode_Smooth", "E")), new GUIContent(localization.format("Mode_Cut", "R")));
        modeButton.PositionScale_Y = 1f;
        modeButton.SizeOffset_X = 200f;
        modeButton.SizeOffset_Y = 30f;
        modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)TerrainEditor.splatmapMode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.PositionScale_Y = 1f;
        brushRadiusField.SizeOffset_X = 200f;
        brushRadiusField.SizeOffset_Y = 30f;
        brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
        brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.PositionScale_Y = 1f;
        brushFalloffField.SizeOffset_X = 200f;
        brushFalloffField.SizeOffset_Y = 30f;
        brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
        brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.PositionScale_Y = 1f;
        brushStrengthField.SizeOffset_X = 200f;
        brushStrengthField.SizeOffset_Y = 30f;
        brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushStrength;
        brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
        smoothMethodButton.PositionScale_Y = 1f;
        smoothMethodButton.SizeOffset_X = 200f;
        smoothMethodButton.SizeOffset_Y = 30f;
        smoothMethodButton.AddLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
        smoothMethodButton.state = (int)DevkitLandscapeToolSplatmapOptions.instance.smoothMethod;
        SleekButtonState sleekButtonState2 = smoothMethodButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSmoothMethod));
        AddChild(smoothMethodButton);
        autoRayMaskField = Glazier.Get().CreateUInt32Field();
        autoRayMaskField.PositionScale_Y = 1f;
        autoRayMaskField.SizeOffset_X = 200f;
        autoRayMaskField.SizeOffset_Y = 30f;
        autoRayMaskField.AddLabel("Ray Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        autoRayMaskField.Value = (uint)DevkitLandscapeToolSplatmapOptions.instance.autoRayMask;
        autoRayMaskField.OnValueChanged += OnAutoRayMaskTyped;
        AddChild(autoRayMaskField);
        autoRayLengthField = Glazier.Get().CreateFloat32Field();
        autoRayLengthField.PositionScale_Y = 1f;
        autoRayLengthField.SizeOffset_X = 200f;
        autoRayLengthField.SizeOffset_Y = 30f;
        autoRayLengthField.AddLabel(localization.format("AutoRayLength"), ESleekSide.RIGHT);
        autoRayLengthField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoRayLength;
        autoRayLengthField.OnValueChanged += OnAutoRayLengthTyped;
        AddChild(autoRayLengthField);
        autoRayRadiusField = Glazier.Get().CreateFloat32Field();
        autoRayRadiusField.PositionScale_Y = 1f;
        autoRayRadiusField.SizeOffset_X = 200f;
        autoRayRadiusField.SizeOffset_Y = 30f;
        autoRayRadiusField.AddLabel(localization.format("AutoRayRadius"), ESleekSide.RIGHT);
        autoRayRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius;
        autoRayRadiusField.OnValueChanged += OnAutoRayRadiusTyped;
        AddChild(autoRayRadiusField);
        useAutoFoundationToggle = Glazier.Get().CreateToggle();
        useAutoFoundationToggle.PositionScale_Y = 1f;
        useAutoFoundationToggle.SizeOffset_X = 40f;
        useAutoFoundationToggle.SizeOffset_Y = 40f;
        useAutoFoundationToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
        useAutoFoundationToggle.OnValueChanged += OnClickedUseAutoFoundation;
        useAutoFoundationToggle.AddLabel(localization.format("UseAutoFoundation"), ESleekSide.RIGHT);
        AddChild(useAutoFoundationToggle);
        autoMaxAngleBeginField = Glazier.Get().CreateFloat32Field();
        autoMaxAngleBeginField.PositionScale_Y = 1f;
        autoMaxAngleBeginField.SizeOffset_X = 100f;
        autoMaxAngleBeginField.SizeOffset_Y = 30f;
        autoMaxAngleBeginField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin;
        autoMaxAngleBeginField.OnValueChanged += OnAutoMaxAngleBeginTyped;
        AddChild(autoMaxAngleBeginField);
        autoMaxAngleEndField = Glazier.Get().CreateFloat32Field();
        autoMaxAngleEndField.PositionOffset_X = 100f;
        autoMaxAngleEndField.PositionScale_Y = 1f;
        autoMaxAngleEndField.SizeOffset_X = 100f;
        autoMaxAngleEndField.SizeOffset_Y = 30f;
        autoMaxAngleEndField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd;
        autoMaxAngleEndField.OnValueChanged += OnAutoMaxAngleEndTyped;
        autoMaxAngleEndField.AddLabel(localization.format("MaxAngleRange"), ESleekSide.RIGHT);
        AddChild(autoMaxAngleEndField);
        autoMinAngleBeginField = Glazier.Get().CreateFloat32Field();
        autoMinAngleBeginField.PositionScale_Y = 1f;
        autoMinAngleBeginField.SizeOffset_X = 100f;
        autoMinAngleBeginField.SizeOffset_Y = 30f;
        autoMinAngleBeginField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin;
        autoMinAngleBeginField.OnValueChanged += OnAutoMinAngleBeginTyped;
        AddChild(autoMinAngleBeginField);
        autoMinAngleEndField = Glazier.Get().CreateFloat32Field();
        autoMinAngleEndField.PositionOffset_X = 100f;
        autoMinAngleEndField.PositionScale_Y = 1f;
        autoMinAngleEndField.SizeOffset_X = 100f;
        autoMinAngleEndField.SizeOffset_Y = 30f;
        autoMinAngleEndField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd;
        autoMinAngleEndField.OnValueChanged += OnAutoMinAngleEndTyped;
        autoMinAngleEndField.AddLabel(localization.format("MinAngleRange"), ESleekSide.RIGHT);
        AddChild(autoMinAngleEndField);
        useAutoSlopeToggle = Glazier.Get().CreateToggle();
        useAutoSlopeToggle.PositionScale_Y = 1f;
        useAutoSlopeToggle.SizeOffset_X = 40f;
        useAutoSlopeToggle.SizeOffset_Y = 40f;
        useAutoSlopeToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
        useAutoSlopeToggle.OnValueChanged += OnClickedUseAutoSlope;
        useAutoSlopeToggle.AddLabel(localization.format("UseAutoSlope"), ESleekSide.RIGHT);
        AddChild(useAutoSlopeToggle);
        useWeightTargetToggle = Glazier.Get().CreateToggle();
        useWeightTargetToggle.PositionScale_Y = 1f;
        useWeightTargetToggle.SizeOffset_X = 40f;
        useWeightTargetToggle.SizeOffset_Y = 40f;
        useWeightTargetToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget;
        useWeightTargetToggle.OnValueChanged += OnClickedUseWeightTarget;
        AddChild(useWeightTargetToggle);
        weightTargetField = Glazier.Get().CreateFloat32Field();
        weightTargetField.PositionOffset_X = 40f;
        weightTargetField.PositionScale_Y = 1f;
        weightTargetField.SizeOffset_X = 160f;
        weightTargetField.SizeOffset_Y = 30f;
        weightTargetField.Value = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
        weightTargetField.AddLabel(localization.format("WeightTarget", "G"), ESleekSide.RIGHT);
        weightTargetField.OnValueChanged += OnWeightTargetTyped;
        AddChild(weightTargetField);
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.PositionScale_Y = 1f;
        maxPreviewSamplesField.SizeOffset_X = 200f;
        maxPreviewSamplesField.SizeOffset_Y = 30f;
        maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.Value = DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
        AddChild(maxPreviewSamplesField);
        previewMethodButton = new SleekButtonState(new GUIContent(localization.format("PreviewMethod_BrushAlpha")), new GUIContent(localization.format("PreviewMethod_Weight")));
        previewMethodButton.PositionScale_Y = 1f;
        previewMethodButton.SizeOffset_X = 200f;
        previewMethodButton.SizeOffset_Y = 30f;
        previewMethodButton.AddLabel(localization.format("PreviewMethod_Label"), ESleekSide.RIGHT);
        previewMethodButton.state = (int)DevkitLandscapeToolSplatmapOptions.instance.previewMethod;
        SleekButtonState sleekButtonState3 = previewMethodButton;
        sleekButtonState3.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState3.onSwappedState, new SwappedState(OnSwappedPreviewMethod));
        AddChild(previewMethodButton);
        highlightHolesToggle = Glazier.Get().CreateToggle();
        highlightHolesToggle.PositionScale_Y = 1f;
        highlightHolesToggle.SizeOffset_X = 40f;
        highlightHolesToggle.SizeOffset_Y = 40f;
        highlightHolesToggle.OnValueChanged += OnClickedHighlightHoles;
        highlightHolesToggle.IsVisible = false;
        highlightHolesToggle.AddLabel(localization.format("HighlightHoles_Label"), ESleekSide.RIGHT);
        AddChild(highlightHolesToggle);
        UpdateLowerLeftOffset();
        float num = 0f;
        selectedAssetBox = new SleekBoxIcon(null, 64);
        selectedAssetBox.PositionScale_X = 1f;
        selectedAssetBox.SizeOffset_X = 300f;
        selectedAssetBox.PositionOffset_X = 0f - selectedAssetBox.SizeOffset_X;
        selectedAssetBox.SizeOffset_Y = 74f;
        selectedAssetBox.AddLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
        AddChild(selectedAssetBox);
        num += selectedAssetBox.SizeOffset_Y + 10f;
        onlyUsedMaterialsToggle = Glazier.Get().CreateToggle();
        onlyUsedMaterialsToggle.PositionScale_X = 1f;
        onlyUsedMaterialsToggle.SizeOffset_X = 40f;
        onlyUsedMaterialsToggle.PositionOffset_X = -300f;
        onlyUsedMaterialsToggle.SizeOffset_Y = 40f;
        onlyUsedMaterialsToggle.PositionOffset_Y = num;
        onlyUsedMaterialsToggle.AddLabel(localization.format("OnlyUsedMaterials"), ESleekSide.RIGHT);
        onlyUsedMaterialsToggle.Value = true;
        onlyUsedMaterialsToggle.OnValueChanged += OnClickedOnlyUsedMaterials;
        AddChild(onlyUsedMaterialsToggle);
        num += onlyUsedMaterialsToggle.SizeOffset_Y + 10f;
        searchField = Glazier.Get().CreateStringField();
        searchField.PositionOffset_X = -300f;
        searchField.PositionOffset_Y = num;
        searchField.PositionScale_X = 1f;
        searchField.SizeOffset_X = 300f;
        searchField.SizeOffset_Y = 30f;
        searchField.PlaceholderText = localization.format("SearchHint");
        searchField.OnTextSubmitted += OnNameFilterEntered;
        AddChild(searchField);
        num += searchField.SizeOffset_Y + 10f;
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.PositionScale_X = 1f;
        assetScrollView.SizeOffset_X = 300f;
        assetScrollView.PositionOffset_X = 0f - assetScrollView.SizeOffset_X;
        assetScrollView.PositionOffset_Y = num;
        assetScrollView.SizeOffset_Y = 0f - num;
        assetScrollView.SizeScale_Y = 1f;
        assetScrollView.ScaleContentToWidth = true;
        AddChild(assetScrollView);
    }

    private void OnSwappedMode(SleekButtonState element, int index)
    {
        TerrainEditor.splatmapMode = (TerrainEditor.EDevkitLandscapeToolSplatmapMode)index;
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
        TerrainEditor.splatmapMaterialTarget = new AssetReference<LandscapeMaterialAsset>(searchAssets[index].GUID);
    }

    private void RefreshAssets()
    {
        searchAssets.Clear();
        assetScrollView.RemoveAllChildren();
        float num = 0f;
        if (onlyUsedMaterialsToggle.Value)
        {
            Landscape.GetUniqueMaterials(searchAssets);
        }
        else
        {
            Assets.find(searchAssets);
        }
        string searchText = searchField.Text;
        if (!string.IsNullOrEmpty(searchText))
        {
            searchAssets.RemoveSwap((LandscapeMaterialAsset asset) => asset.FriendlyName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) == -1);
        }
        searchAssets.Sort((LandscapeMaterialAsset lhs, LandscapeMaterialAsset rhs) => lhs.name.CompareTo(rhs.name));
        foreach (LandscapeMaterialAsset searchAsset in searchAssets)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(Assets.load(searchAsset.texture), 64);
            sleekButtonIcon.PositionOffset_Y = num;
            sleekButtonIcon.SizeScale_X = 1f;
            sleekButtonIcon.SizeOffset_Y = 74f;
            sleekButtonIcon.text = searchAsset.FriendlyName;
            sleekButtonIcon.onClickedButton += OnAssetClicked;
            assetScrollView.AddChild(sleekButtonIcon);
            num += sleekButtonIcon.SizeOffset_Y;
        }
        assetScrollView.ContentSizeOffset = new Vector2(0f, num);
    }

    private void UpdateLowerLeftOffset()
    {
        float num = 0f;
        num -= modeButton.SizeOffset_Y;
        modeButton.PositionOffset_Y = num;
        num -= 10f;
        num -= previewMethodButton.SizeOffset_Y;
        previewMethodButton.PositionOffset_Y = num;
        num -= 10f;
        num -= maxPreviewSamplesField.SizeOffset_Y;
        maxPreviewSamplesField.PositionOffset_Y = num;
        num -= 10f;
        smoothMethodButton.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.SMOOTH;
        if (smoothMethodButton.IsVisible)
        {
            num -= smoothMethodButton.SizeOffset_Y;
            smoothMethodButton.PositionOffset_Y = num;
            num -= 10f;
        }
        autoRayRadiusField.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT && DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
        autoRayLengthField.IsVisible = autoRayRadiusField.IsVisible;
        autoRayMaskField.IsVisible = autoRayRadiusField.IsVisible;
        if (autoRayRadiusField.IsVisible)
        {
            num -= autoRayMaskField.SizeOffset_Y;
            autoRayMaskField.PositionOffset_Y = num;
            num -= autoRayLengthField.SizeOffset_Y;
            autoRayLengthField.PositionOffset_Y = num;
            num -= autoRayRadiusField.SizeOffset_Y;
            autoRayRadiusField.PositionOffset_Y = num;
        }
        useAutoFoundationToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
        if (useAutoFoundationToggle.IsVisible)
        {
            num -= useAutoFoundationToggle.SizeOffset_Y;
            useAutoFoundationToggle.PositionOffset_Y = num;
            num -= 10f;
        }
        autoMinAngleBeginField.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT && DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
        autoMinAngleEndField.IsVisible = autoMinAngleBeginField.IsVisible;
        autoMaxAngleBeginField.IsVisible = autoMinAngleBeginField.IsVisible;
        autoMaxAngleEndField.IsVisible = autoMinAngleBeginField.IsVisible;
        if (autoMinAngleBeginField.IsVisible)
        {
            num -= autoMaxAngleBeginField.SizeOffset_Y;
            autoMaxAngleBeginField.PositionOffset_Y = num;
            autoMaxAngleEndField.PositionOffset_Y = num;
            num -= autoMinAngleBeginField.SizeOffset_Y;
            autoMinAngleBeginField.PositionOffset_Y = num;
            autoMinAngleEndField.PositionOffset_Y = num;
        }
        useAutoSlopeToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
        if (useAutoSlopeToggle.IsVisible)
        {
            num -= useAutoSlopeToggle.SizeOffset_Y;
            useAutoSlopeToggle.PositionOffset_Y = num;
            num -= 10f;
        }
        useWeightTargetToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
        weightTargetField.IsVisible = useWeightTargetToggle.IsVisible;
        if (useWeightTargetToggle.IsVisible)
        {
            num -= useWeightTargetToggle.SizeOffset_Y;
            useWeightTargetToggle.PositionOffset_Y = num;
            weightTargetField.PositionOffset_Y = num + 5f;
            num -= 10f;
        }
        brushStrengthField.IsVisible = TerrainEditor.splatmapMode != TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT;
        brushFalloffField.IsVisible = brushStrengthField.IsVisible;
        if (brushStrengthField.IsVisible)
        {
            num -= brushStrengthField.SizeOffset_Y;
            brushStrengthField.PositionOffset_Y = num;
            num -= 10f;
            num -= brushFalloffField.SizeOffset_Y;
            brushFalloffField.PositionOffset_Y = num;
            num -= 10f;
        }
        num -= brushRadiusField.SizeOffset_Y;
        brushRadiusField.PositionOffset_Y = num;
        num -= 10f;
        highlightHolesToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT;
        if (highlightHolesToggle.IsVisible)
        {
            num -= highlightHolesToggle.SizeOffset_Y;
            highlightHolesToggle.PositionOffset_Y = num;
        }
    }
}
