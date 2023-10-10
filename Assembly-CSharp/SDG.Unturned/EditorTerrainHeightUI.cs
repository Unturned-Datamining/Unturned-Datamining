using System;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using UnityEngine;

namespace SDG.Unturned;

internal class EditorTerrainHeightUI : SleekFullscreenBox
{
    private Local localization;

    private ISleekLabel hintLabel;

    private SleekButtonState modeButton;

    private ISleekFloat32Field brushRadiusField;

    private ISleekFloat32Field brushFalloffField;

    private ISleekFloat32Field brushStrengthField;

    private ISleekFloat32Field flattenTargetField;

    private ISleekUInt32Field maxPreviewSamplesField;

    private SleekButtonState smoothMethodButton;

    private SleekButtonState flattenMethodButton;

    public void Open()
    {
        AnimateIntoView();
        TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.HEIGHTMAP;
        EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);
        if (FoliageSystem.instance != null)
        {
            FoliageSystem.instance.hiddenByHeightEditor = true;
        }
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        DevkitLandscapeToolHeightmapOptions.save();
        EditorInteract.instance.SetActiveTool(null);
        if (FoliageSystem.instance != null)
        {
            FoliageSystem.instance.hiddenByHeightEditor = false;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        modeButton.state = (int)TerrainEditor.heightmapMode;
        brushRadiusField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
        brushFalloffField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
        brushStrengthField.Value = EditorInteract.instance.terrainTool.heightmapBrushStrength;
        flattenTargetField.Value = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
        if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.ADJUST)
        {
            hintLabel.Text = localization.format("Hint_Adjust", "Shift");
            hintLabel.IsVisible = true;
        }
        else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN)
        {
            hintLabel.Text = localization.format("Hint_Flatten", "Alt");
            hintLabel.IsVisible = true;
        }
        else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.RAMP)
        {
            hintLabel.Text = localization.format("Hint_Ramp", "R");
            hintLabel.IsVisible = true;
        }
        else
        {
            hintLabel.IsVisible = false;
        }
        UpdateLowerLeftOffset();
    }

    public EditorTerrainHeightUI()
    {
        localization = Localization.read("/Editor/EditorTerrainHeight.dat");
        DevkitLandscapeToolHeightmapOptions.load();
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.PositionScale_Y = 1f;
        hintLabel.PositionOffset_Y = -30f;
        hintLabel.SizeScale_X = 1f;
        hintLabel.SizeOffset_Y = 30f;
        hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        hintLabel.IsVisible = false;
        AddChild(hintLabel);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Adjust", "Q")), new GUIContent(localization.format("Mode_Flatten", "W")), new GUIContent(localization.format("Mode_Smooth", "E")), new GUIContent(localization.format("Mode_Ramp", "R")));
        modeButton.PositionScale_Y = 1f;
        modeButton.SizeOffset_X = 200f;
        modeButton.SizeOffset_Y = 30f;
        modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)TerrainEditor.heightmapMode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.PositionScale_Y = 1f;
        brushRadiusField.SizeOffset_X = 200f;
        brushRadiusField.SizeOffset_Y = 30f;
        brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
        brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.PositionScale_Y = 1f;
        brushFalloffField.SizeOffset_X = 200f;
        brushFalloffField.SizeOffset_Y = 30f;
        brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
        brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.PositionScale_Y = 1f;
        brushStrengthField.SizeOffset_X = 200f;
        brushStrengthField.SizeOffset_Y = 30f;
        brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushStrength;
        brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
        smoothMethodButton.PositionScale_Y = 1f;
        smoothMethodButton.SizeOffset_X = 200f;
        smoothMethodButton.SizeOffset_Y = 30f;
        smoothMethodButton.AddLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
        smoothMethodButton.state = (int)DevkitLandscapeToolHeightmapOptions.instance.smoothMethod;
        SleekButtonState sleekButtonState2 = smoothMethodButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSmoothMethod));
        AddChild(smoothMethodButton);
        flattenTargetField = Glazier.Get().CreateFloat32Field();
        flattenTargetField.PositionScale_Y = 1f;
        flattenTargetField.SizeOffset_X = 200f;
        flattenTargetField.SizeOffset_Y = 30f;
        flattenTargetField.AddLabel(localization.format("FlattenTarget", "Alt"), ESleekSide.RIGHT);
        flattenTargetField.Value = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
        flattenTargetField.OnValueChanged += OnFlattenTargetTyped;
        AddChild(flattenTargetField);
        flattenMethodButton = new SleekButtonState(new GUIContent(localization.format("FlattenMethod_Regular")), new GUIContent(localization.format("FlattenMethod_Min")), new GUIContent(localization.format("FlattenMethod_Max")));
        flattenMethodButton.PositionScale_Y = 1f;
        flattenMethodButton.SizeOffset_X = 200f;
        flattenMethodButton.SizeOffset_Y = 30f;
        flattenMethodButton.AddLabel(localization.format("FlattenMethod_Label"), ESleekSide.RIGHT);
        flattenMethodButton.state = (int)DevkitLandscapeToolHeightmapOptions.instance.flattenMethod;
        SleekButtonState sleekButtonState3 = flattenMethodButton;
        sleekButtonState3.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState3.onSwappedState, new SwappedState(OnSwappedFlattenMethod));
        AddChild(flattenMethodButton);
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.PositionScale_Y = 1f;
        maxPreviewSamplesField.SizeOffset_X = 200f;
        maxPreviewSamplesField.SizeOffset_Y = 30f;
        maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.Value = DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
        AddChild(maxPreviewSamplesField);
        UpdateLowerLeftOffset();
    }

    private void OnSwappedMode(SleekButtonState element, int index)
    {
        TerrainEditor.heightmapMode = (TerrainEditor.EDevkitLandscapeToolHeightmapMode)index;
    }

    private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
    {
        EditorInteract.instance.terrainTool.heightmapBrushStrength = state;
    }

    private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolHeightmapOptions.instance.brushFalloff = state;
    }

    private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolHeightmapOptions.instance.brushRadius = state;
    }

    private void OnFlattenTargetTyped(ISleekFloat32Field field, float state)
    {
        DevkitLandscapeToolHeightmapOptions.instance.flattenTarget = state;
    }

    private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
    {
        DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples = state;
    }

    private void OnSwappedSmoothMethod(SleekButtonState element, int index)
    {
        DevkitLandscapeToolHeightmapOptions.instance.smoothMethod = (EDevkitLandscapeToolHeightmapSmoothMethod)index;
    }

    private void OnSwappedFlattenMethod(SleekButtonState element, int index)
    {
        DevkitLandscapeToolHeightmapOptions.instance.flattenMethod = (EDevkitLandscapeToolHeightmapFlattenMethod)index;
    }

    private void UpdateLowerLeftOffset()
    {
        float num = 0f;
        num -= modeButton.SizeOffset_Y;
        modeButton.PositionOffset_Y = num;
        num -= 10f;
        num -= maxPreviewSamplesField.SizeOffset_Y;
        maxPreviewSamplesField.PositionOffset_Y = num;
        num -= 10f;
        smoothMethodButton.IsVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.SMOOTH;
        if (smoothMethodButton.IsVisible)
        {
            num -= smoothMethodButton.SizeOffset_Y;
            smoothMethodButton.PositionOffset_Y = num;
            num -= 10f;
        }
        flattenMethodButton.IsVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN;
        flattenTargetField.IsVisible = flattenMethodButton.IsVisible;
        if (flattenMethodButton.IsVisible)
        {
            num -= flattenMethodButton.SizeOffset_Y;
            flattenMethodButton.PositionOffset_Y = num;
            num -= 10f;
            num -= flattenTargetField.SizeOffset_Y;
            flattenTargetField.PositionOffset_Y = num;
            num -= 10f;
        }
        num -= brushStrengthField.SizeOffset_Y;
        brushStrengthField.PositionOffset_Y = num;
        num -= 10f;
        num -= brushFalloffField.SizeOffset_Y;
        brushFalloffField.PositionOffset_Y = num;
        num -= 10f;
        num -= brushRadiusField.SizeOffset_Y;
        brushRadiusField.PositionOffset_Y = num;
    }
}
