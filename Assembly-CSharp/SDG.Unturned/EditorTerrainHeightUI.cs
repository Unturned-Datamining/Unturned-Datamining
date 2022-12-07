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
        brushRadiusField.state = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
        brushFalloffField.state = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
        brushStrengthField.state = EditorInteract.instance.terrainTool.heightmapBrushStrength;
        flattenTargetField.state = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
        if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.ADJUST)
        {
            hintLabel.text = localization.format("Hint_Adjust", "Shift");
            hintLabel.isVisible = true;
        }
        else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN)
        {
            hintLabel.text = localization.format("Hint_Flatten", "Alt");
            hintLabel.isVisible = true;
        }
        else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.RAMP)
        {
            hintLabel.text = localization.format("Hint_Ramp", "R");
            hintLabel.isVisible = true;
        }
        else
        {
            hintLabel.isVisible = false;
        }
        UpdateLowerLeftOffset();
    }

    public EditorTerrainHeightUI()
    {
        localization = Localization.read("/Editor/EditorTerrainHeight.dat");
        DevkitLandscapeToolHeightmapOptions.load();
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.positionScale_Y = 1f;
        hintLabel.positionOffset_Y = -30;
        hintLabel.sizeScale_X = 1f;
        hintLabel.sizeOffset_Y = 30;
        hintLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        hintLabel.isVisible = false;
        AddChild(hintLabel);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Adjust", "Q")), new GUIContent(localization.format("Mode_Flatten", "W")), new GUIContent(localization.format("Mode_Smooth", "E")), new GUIContent(localization.format("Mode_Ramp", "R")));
        modeButton.positionScale_Y = 1f;
        modeButton.sizeOffset_X = 200;
        modeButton.sizeOffset_Y = 30;
        modeButton.addLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)TerrainEditor.heightmapMode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.positionScale_Y = 1f;
        brushRadiusField.sizeOffset_X = 200;
        brushRadiusField.sizeOffset_Y = 30;
        brushRadiusField.addLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.state = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
        brushRadiusField.onTypedSingle += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.positionScale_Y = 1f;
        brushFalloffField.sizeOffset_X = 200;
        brushFalloffField.sizeOffset_Y = 30;
        brushFalloffField.addLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.state = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
        brushFalloffField.onTypedSingle += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.positionScale_Y = 1f;
        brushStrengthField.sizeOffset_X = 200;
        brushStrengthField.sizeOffset_Y = 30;
        brushStrengthField.addLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.state = DevkitLandscapeToolHeightmapOptions.instance.brushStrength;
        brushStrengthField.onTypedSingle += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
        smoothMethodButton.positionScale_Y = 1f;
        smoothMethodButton.sizeOffset_X = 200;
        smoothMethodButton.sizeOffset_Y = 30;
        smoothMethodButton.addLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
        smoothMethodButton.state = (int)DevkitLandscapeToolHeightmapOptions.instance.smoothMethod;
        SleekButtonState sleekButtonState2 = smoothMethodButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSmoothMethod));
        AddChild(smoothMethodButton);
        flattenTargetField = Glazier.Get().CreateFloat32Field();
        flattenTargetField.positionScale_Y = 1f;
        flattenTargetField.sizeOffset_X = 200;
        flattenTargetField.sizeOffset_Y = 30;
        flattenTargetField.addLabel(localization.format("FlattenTarget", "Alt"), ESleekSide.RIGHT);
        flattenTargetField.state = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
        flattenTargetField.onTypedSingle += OnFlattenTargetTyped;
        AddChild(flattenTargetField);
        flattenMethodButton = new SleekButtonState(new GUIContent(localization.format("FlattenMethod_Regular")), new GUIContent(localization.format("FlattenMethod_Min")), new GUIContent(localization.format("FlattenMethod_Max")));
        flattenMethodButton.positionScale_Y = 1f;
        flattenMethodButton.sizeOffset_X = 200;
        flattenMethodButton.sizeOffset_Y = 30;
        flattenMethodButton.addLabel(localization.format("FlattenMethod_Label"), ESleekSide.RIGHT);
        flattenMethodButton.state = (int)DevkitLandscapeToolHeightmapOptions.instance.flattenMethod;
        SleekButtonState sleekButtonState3 = flattenMethodButton;
        sleekButtonState3.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState3.onSwappedState, new SwappedState(OnSwappedFlattenMethod));
        AddChild(flattenMethodButton);
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.positionScale_Y = 1f;
        maxPreviewSamplesField.sizeOffset_X = 200;
        maxPreviewSamplesField.sizeOffset_Y = 30;
        maxPreviewSamplesField.addLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.state = DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.onTypedUInt32 += OnMaxPreviewSamplesTyped;
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
        int num = 0;
        num -= modeButton.sizeOffset_Y;
        modeButton.positionOffset_Y = num;
        num -= 10;
        num -= maxPreviewSamplesField.sizeOffset_Y;
        maxPreviewSamplesField.positionOffset_Y = num;
        num -= 10;
        smoothMethodButton.isVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.SMOOTH;
        if (smoothMethodButton.isVisible)
        {
            num -= smoothMethodButton.sizeOffset_Y;
            smoothMethodButton.positionOffset_Y = num;
            num -= 10;
        }
        flattenMethodButton.isVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN;
        flattenTargetField.isVisible = flattenMethodButton.isVisible;
        if (flattenMethodButton.isVisible)
        {
            num -= flattenMethodButton.sizeOffset_Y;
            flattenMethodButton.positionOffset_Y = num;
            num -= 10;
            num -= flattenTargetField.sizeOffset_Y;
            flattenTargetField.positionOffset_Y = num;
            num -= 10;
        }
        num -= brushStrengthField.sizeOffset_Y;
        brushStrengthField.positionOffset_Y = num;
        num -= 10;
        num -= brushFalloffField.sizeOffset_Y;
        brushFalloffField.positionOffset_Y = num;
        num -= 10;
        num -= brushRadiusField.sizeOffset_Y;
        brushRadiusField.positionOffset_Y = num;
    }
}
