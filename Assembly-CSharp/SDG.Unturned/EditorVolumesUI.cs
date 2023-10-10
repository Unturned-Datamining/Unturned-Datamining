using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

internal class EditorVolumesUI : SleekFullscreenBox
{
    internal Local localization;

    private VolumesEditor tool;

    private GameObject focusedGameObject;

    private ISleekElement focusedItemMenu;

    private ISleekFloat32Field snapTransformField;

    private ISleekFloat32Field snapRotationField;

    private SleekButtonIcon transformButton;

    private SleekButtonIcon rotateButton;

    private SleekButtonIcon scaleButton;

    private SleekButtonState coordinateButton;

    private ISleekUInt32Field surfaceMaskField;

    private ISleekBox selectedTypeBox;

    private SleekButtonState activeVisibilityButton;

    private ISleekScrollView typeScrollView;

    private VolumeTypeButton[] volumeButtons;

    private ISleekToggle enableUnderwaterEffectsToggle;

    private ISleekToggle enableWaterSurfaceToggle;

    private ISleekButton refreshCullingVolumesButton;

    private ISleekToggle previewCullingToggle;

    internal static bool EditorWantsToPreviewCulling;

    public void Open()
    {
        SyncSettings();
        AnimateIntoView();
        EditorInteract.instance.SetActiveTool(tool);
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        DevkitSelectionToolOptions.save();
        EditorInteract.instance.SetActiveTool(null);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        GameObject mostRecentGameObject = DevkitSelectionManager.mostRecentGameObject;
        if (focusedGameObject == mostRecentGameObject)
        {
            return;
        }
        if (focusedItemMenu != null)
        {
            RemoveChild(focusedItemMenu);
            focusedItemMenu = null;
        }
        focusedGameObject = mostRecentGameObject;
        VolumeBase volumeBase = focusedGameObject?.GetComponent<VolumeBase>();
        if (volumeBase != null)
        {
            focusedItemMenu = volumeBase.CreateMenu();
            if (focusedItemMenu != null)
            {
                focusedItemMenu.PositionOffset_Y = snapTransformField.PositionOffset_Y - 10f - focusedItemMenu.SizeOffset_Y;
                focusedItemMenu.PositionScale_Y = 1f;
                AddChild(focusedItemMenu);
            }
        }
    }

    public EditorVolumesUI()
    {
        DevkitSelectionToolOptions.load();
        tool = new VolumesEditor();
        localization = Localization.read("/Editor/EditorLevelVolumes.dat");
        Local local = Localization.read("/Editor/EditorLevelObjects.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelObjects/EditorLevelObjects.unity3d");
        List<VolumeManagerBase> list = new List<VolumeManagerBase>(VolumeManagerBase.allManagers);
        list.Sort((VolumeManagerBase lhs, VolumeManagerBase rhs) => lhs.FriendlyName.CompareTo(rhs.FriendlyName));
        float num = 0f;
        surfaceMaskField = Glazier.Get().CreateUInt32Field();
        surfaceMaskField.PositionScale_Y = 1f;
        surfaceMaskField.SizeOffset_X = 200f;
        surfaceMaskField.SizeOffset_Y = 30f;
        num -= surfaceMaskField.SizeOffset_Y;
        surfaceMaskField.PositionOffset_Y = num;
        num -= 10f;
        surfaceMaskField.AddLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        surfaceMaskField.OnValueChanged += OnSurfaceMaskTyped;
        AddChild(surfaceMaskField);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.PositionScale_Y = 1f;
        coordinateButton.SizeOffset_X = 200f;
        coordinateButton.SizeOffset_Y = 30f;
        num -= coordinateButton.SizeOffset_Y;
        coordinateButton.PositionOffset_Y = num;
        num -= 10f;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = OnSwappedStateCoordinate;
        AddChild(coordinateButton);
        scaleButton = new SleekButtonIcon(bundle.load<Texture2D>("Scale"));
        scaleButton.PositionScale_Y = 1f;
        scaleButton.SizeOffset_X = 200f;
        scaleButton.SizeOffset_Y = 30f;
        num -= scaleButton.SizeOffset_Y;
        scaleButton.PositionOffset_Y = num;
        num -= 10f;
        scaleButton.text = local.format("ScaleButtonText", ControlsSettings.tool_3);
        scaleButton.tooltip = local.format("ScaleButtonTooltip");
        scaleButton.onClickedButton += OnScaleClicked;
        AddChild(scaleButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.PositionScale_Y = 1f;
        rotateButton.SizeOffset_X = 200f;
        rotateButton.SizeOffset_Y = 30f;
        num -= rotateButton.SizeOffset_Y;
        rotateButton.PositionOffset_Y = num;
        num -= 10f;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += OnRotateClicked;
        AddChild(rotateButton);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.PositionScale_Y = 1f;
        transformButton.SizeOffset_X = 200f;
        transformButton.SizeOffset_Y = 30f;
        num -= transformButton.SizeOffset_Y;
        transformButton.PositionOffset_Y = num;
        num -= 10f;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += OnTransformClicked;
        AddChild(transformButton);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.PositionScale_Y = 1f;
        snapRotationField.SizeOffset_X = 200f;
        snapRotationField.SizeOffset_Y = 30f;
        num -= snapRotationField.SizeOffset_Y;
        snapRotationField.PositionOffset_Y = num;
        num -= 10f;
        snapRotationField.AddLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.OnValueChanged += OnTypedSnapRotationField;
        AddChild(snapRotationField);
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.PositionScale_Y = 1f;
        snapTransformField.SizeOffset_X = 200f;
        snapTransformField.SizeOffset_Y = 30f;
        num -= snapTransformField.SizeOffset_Y;
        snapTransformField.PositionOffset_Y = num;
        snapTransformField.AddLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.OnValueChanged += OnTypedSnapTransformField;
        AddChild(snapTransformField);
        bundle.unload();
        enableUnderwaterEffectsToggle = Glazier.Get().CreateToggle();
        enableUnderwaterEffectsToggle.PositionOffset_X = 400f;
        enableUnderwaterEffectsToggle.PositionOffset_Y = -40f;
        enableUnderwaterEffectsToggle.PositionScale_Y = 1f;
        enableUnderwaterEffectsToggle.SizeOffset_X = 40f;
        enableUnderwaterEffectsToggle.SizeOffset_Y = 40f;
        enableUnderwaterEffectsToggle.AddLabel(localization.format("WantsUnderwaterEffects"), ESleekSide.RIGHT);
        enableUnderwaterEffectsToggle.Value = LevelLighting.EditorWantsUnderwaterEffects;
        enableUnderwaterEffectsToggle.IsVisible = false;
        enableUnderwaterEffectsToggle.OnValueChanged += OnUnderwaterEffectsToggled;
        AddChild(enableUnderwaterEffectsToggle);
        enableWaterSurfaceToggle = Glazier.Get().CreateToggle();
        enableWaterSurfaceToggle.PositionOffset_X = 400f;
        enableWaterSurfaceToggle.PositionOffset_Y = -90f;
        enableWaterSurfaceToggle.PositionScale_Y = 1f;
        enableWaterSurfaceToggle.SizeOffset_X = 40f;
        enableWaterSurfaceToggle.SizeOffset_Y = 40f;
        enableWaterSurfaceToggle.AddLabel(localization.format("WantsWaterSurface"), ESleekSide.RIGHT);
        enableWaterSurfaceToggle.Value = LevelLighting.EditorWantsWaterSurface;
        enableWaterSurfaceToggle.IsVisible = false;
        enableWaterSurfaceToggle.OnValueChanged += OnWaterSurfaceToggled;
        AddChild(enableWaterSurfaceToggle);
        refreshCullingVolumesButton = Glazier.Get().CreateButton();
        refreshCullingVolumesButton.PositionOffset_X = 400f;
        refreshCullingVolumesButton.PositionOffset_Y = -30f;
        refreshCullingVolumesButton.PositionScale_Y = 1f;
        refreshCullingVolumesButton.SizeOffset_X = 200f;
        refreshCullingVolumesButton.SizeOffset_Y = 30f;
        refreshCullingVolumesButton.Text = localization.format("RefreshCullingVolumes");
        refreshCullingVolumesButton.TooltipText = localization.format("RefreshCullingVolumes_Tooltip");
        refreshCullingVolumesButton.IsVisible = false;
        refreshCullingVolumesButton.OnClicked += OnRefreshCullingVolumesClicked;
        AddChild(refreshCullingVolumesButton);
        previewCullingToggle = Glazier.Get().CreateToggle();
        previewCullingToggle.PositionOffset_X = 400f;
        previewCullingToggle.PositionOffset_Y = -80f;
        previewCullingToggle.PositionScale_Y = 1f;
        previewCullingToggle.SizeOffset_X = 40f;
        previewCullingToggle.SizeOffset_Y = 40f;
        previewCullingToggle.AddLabel(localization.format("PreviewCulling"), ESleekSide.RIGHT);
        previewCullingToggle.Value = EditorWantsToPreviewCulling;
        previewCullingToggle.IsVisible = false;
        previewCullingToggle.OnValueChanged += OnPreviewCullingToggled;
        AddChild(previewCullingToggle);
        float num2 = 0f;
        selectedTypeBox = Glazier.Get().CreateBox();
        selectedTypeBox.PositionScale_X = 1f;
        selectedTypeBox.PositionOffset_Y = num2;
        selectedTypeBox.SizeOffset_X = 300f;
        selectedTypeBox.PositionOffset_X = 0f - selectedTypeBox.SizeOffset_X;
        selectedTypeBox.SizeOffset_Y = 30f;
        selectedTypeBox.AddLabel(localization.format("SelectedType_Label"), ESleekSide.LEFT);
        AddChild(selectedTypeBox);
        num2 += selectedTypeBox.SizeOffset_Y + 10f;
        activeVisibilityButton = new SleekButtonState(new GUIContent(localization.format("Visibility_Hidden")), new GUIContent(localization.format("Visibility_Wireframe")), new GUIContent(localization.format("Visibility_Solid")));
        activeVisibilityButton.PositionScale_X = 1f;
        activeVisibilityButton.PositionOffset_Y = num2;
        activeVisibilityButton.SizeOffset_X = 300f;
        activeVisibilityButton.PositionOffset_X = 0f - activeVisibilityButton.SizeOffset_X;
        activeVisibilityButton.SizeOffset_Y = 30f;
        activeVisibilityButton.AddLabel(localization.format("ActiveVisibility_Label"), ESleekSide.LEFT);
        SleekButtonState sleekButtonState = activeVisibilityButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedActiveVisibility));
        activeVisibilityButton.IsVisible = false;
        AddChild(activeVisibilityButton);
        num2 += selectedTypeBox.SizeOffset_Y + 10f;
        typeScrollView = Glazier.Get().CreateScrollView();
        typeScrollView.PositionScale_X = 1f;
        typeScrollView.SizeOffset_X = 300f;
        typeScrollView.PositionOffset_X = 0f - typeScrollView.SizeOffset_X;
        typeScrollView.PositionOffset_Y = num2;
        typeScrollView.SizeOffset_Y = 0f - num2;
        typeScrollView.SizeScale_Y = 1f;
        typeScrollView.ScaleContentToWidth = true;
        AddChild(typeScrollView);
        int num3 = 0;
        float num4 = 0f;
        volumeButtons = new VolumeTypeButton[list.Count];
        foreach (VolumeManagerBase item in list)
        {
            VolumeTypeButton volumeTypeButton = new VolumeTypeButton(this, item)
            {
                PositionOffset_Y = num4,
                SizeScale_X = 1f,
                SizeOffset_Y = 30f
            };
            typeScrollView.AddChild(volumeTypeButton);
            volumeButtons[num3] = volumeTypeButton;
            num4 += volumeTypeButton.SizeOffset_Y;
            num3++;
        }
        typeScrollView.ContentSizeOffset = new Vector2(0f, num4);
    }

    internal void SetSelectedType(VolumeManagerBase type)
    {
        selectedTypeBox.Text = type.FriendlyName;
        tool.activeVolumeManager = type;
        activeVisibilityButton.state = (int)type.Visibility;
        activeVisibilityButton.IsVisible = true;
        enableUnderwaterEffectsToggle.IsVisible = type is WaterVolumeManager;
        enableWaterSurfaceToggle.IsVisible = enableUnderwaterEffectsToggle.IsVisible;
        refreshCullingVolumesButton.IsVisible = type is CullingVolumeManager;
        previewCullingToggle.IsVisible = refreshCullingVolumesButton.IsVisible;
    }

    internal void RefreshSelectedVisibility()
    {
        if (tool.activeVolumeManager != null)
        {
            activeVisibilityButton.state = (int)tool.activeVolumeManager.Visibility;
        }
    }

    private void OnTypedSnapTransformField(ISleekFloat32Field field, float value)
    {
        DevkitSelectionToolOptions.instance.snapPosition = value;
    }

    private void OnTypedSnapRotationField(ISleekFloat32Field field, float value)
    {
        DevkitSelectionToolOptions.instance.snapRotation = value;
    }

    private void OnTransformClicked(ISleekElement button)
    {
        tool.mode = SelectionTool.ESelectionMode.POSITION;
    }

    private void OnRotateClicked(ISleekElement button)
    {
        tool.mode = SelectionTool.ESelectionMode.ROTATION;
    }

    private void OnScaleClicked(ISleekElement button)
    {
        tool.mode = SelectionTool.ESelectionMode.SCALE;
    }

    private void OnSwappedStateCoordinate(SleekButtonState button, int index)
    {
        DevkitSelectionToolOptions.instance.localSpace = index > 0;
    }

    private void OnSwappedActiveVisibility(SleekButtonState button, int state)
    {
        if (tool.activeVolumeManager != null)
        {
            tool.activeVolumeManager.Visibility = (ELevelVolumeVisibility)state;
        }
        VolumeTypeButton[] array = volumeButtons;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].RefreshVisibility();
        }
    }

    private void OnUnderwaterEffectsToggled(ISleekToggle toggle, bool state)
    {
        LevelLighting.EditorWantsUnderwaterEffects = state;
    }

    private void OnWaterSurfaceToggled(ISleekToggle toggle, bool state)
    {
        LevelLighting.EditorWantsWaterSurface = state;
    }

    private void OnRefreshCullingVolumesClicked(ISleekElement button)
    {
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().ClearOverlappingObjects();
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().RefreshOverlappingObjects();
    }

    private void OnPreviewCullingToggled(ISleekToggle toggle, bool state)
    {
        EditorWantsToPreviewCulling = state;
    }

    private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
    {
        DevkitSelectionToolOptions.instance.selectionMask = (ERayMask)state;
    }

    private void SyncSettings()
    {
        surfaceMaskField.Value = (uint)DevkitSelectionToolOptions.instance.selectionMask;
        coordinateButton.state = (DevkitSelectionToolOptions.instance.localSpace ? 1 : 0);
        snapRotationField.Value = DevkitSelectionToolOptions.instance.snapRotation;
        snapTransformField.Value = DevkitSelectionToolOptions.instance.snapPosition;
    }
}
