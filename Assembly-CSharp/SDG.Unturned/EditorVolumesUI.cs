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
                focusedItemMenu.positionOffset_Y = snapTransformField.positionOffset_Y - 10 - focusedItemMenu.sizeOffset_Y;
                focusedItemMenu.positionScale_Y = 1f;
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
        int num = 0;
        surfaceMaskField = Glazier.Get().CreateUInt32Field();
        surfaceMaskField.positionScale_Y = 1f;
        surfaceMaskField.sizeOffset_X = 200;
        surfaceMaskField.sizeOffset_Y = 30;
        num -= surfaceMaskField.sizeOffset_Y;
        surfaceMaskField.positionOffset_Y = num;
        num -= 10;
        surfaceMaskField.addLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        surfaceMaskField.onTypedUInt32 += OnSurfaceMaskTyped;
        AddChild(surfaceMaskField);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.positionScale_Y = 1f;
        coordinateButton.sizeOffset_X = 200;
        coordinateButton.sizeOffset_Y = 30;
        num -= coordinateButton.sizeOffset_Y;
        coordinateButton.positionOffset_Y = num;
        num -= 10;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = OnSwappedStateCoordinate;
        AddChild(coordinateButton);
        scaleButton = new SleekButtonIcon(bundle.load<Texture2D>("Scale"));
        scaleButton.positionScale_Y = 1f;
        scaleButton.sizeOffset_X = 200;
        scaleButton.sizeOffset_Y = 30;
        num -= scaleButton.sizeOffset_Y;
        scaleButton.positionOffset_Y = num;
        num -= 10;
        scaleButton.text = local.format("ScaleButtonText", ControlsSettings.tool_3);
        scaleButton.tooltip = local.format("ScaleButtonTooltip");
        scaleButton.onClickedButton += OnScaleClicked;
        AddChild(scaleButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.positionScale_Y = 1f;
        rotateButton.sizeOffset_X = 200;
        rotateButton.sizeOffset_Y = 30;
        num -= rotateButton.sizeOffset_Y;
        rotateButton.positionOffset_Y = num;
        num -= 10;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += OnRotateClicked;
        AddChild(rotateButton);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.positionScale_Y = 1f;
        transformButton.sizeOffset_X = 200;
        transformButton.sizeOffset_Y = 30;
        num -= transformButton.sizeOffset_Y;
        transformButton.positionOffset_Y = num;
        num -= 10;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += OnTransformClicked;
        AddChild(transformButton);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.positionScale_Y = 1f;
        snapRotationField.sizeOffset_X = 200;
        snapRotationField.sizeOffset_Y = 30;
        num -= snapRotationField.sizeOffset_Y;
        snapRotationField.positionOffset_Y = num;
        num -= 10;
        snapRotationField.addLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.onTypedSingle += OnTypedSnapRotationField;
        AddChild(snapRotationField);
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.positionScale_Y = 1f;
        snapTransformField.sizeOffset_X = 200;
        snapTransformField.sizeOffset_Y = 30;
        num -= snapTransformField.sizeOffset_Y;
        snapTransformField.positionOffset_Y = num;
        snapTransformField.addLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.onTypedSingle += OnTypedSnapTransformField;
        AddChild(snapTransformField);
        bundle.unload();
        enableUnderwaterEffectsToggle = Glazier.Get().CreateToggle();
        enableUnderwaterEffectsToggle.positionOffset_X = 400;
        enableUnderwaterEffectsToggle.positionOffset_Y = -40;
        enableUnderwaterEffectsToggle.positionScale_Y = 1f;
        enableUnderwaterEffectsToggle.sizeOffset_X = 40;
        enableUnderwaterEffectsToggle.sizeOffset_Y = 40;
        enableUnderwaterEffectsToggle.addLabel(localization.format("WantsUnderwaterEffects"), ESleekSide.RIGHT);
        enableUnderwaterEffectsToggle.state = LevelLighting.EditorWantsUnderwaterEffects;
        enableUnderwaterEffectsToggle.isVisible = false;
        enableUnderwaterEffectsToggle.onToggled += OnUnderwaterEffectsToggled;
        AddChild(enableUnderwaterEffectsToggle);
        enableWaterSurfaceToggle = Glazier.Get().CreateToggle();
        enableWaterSurfaceToggle.positionOffset_X = 400;
        enableWaterSurfaceToggle.positionOffset_Y = -90;
        enableWaterSurfaceToggle.positionScale_Y = 1f;
        enableWaterSurfaceToggle.sizeOffset_X = 40;
        enableWaterSurfaceToggle.sizeOffset_Y = 40;
        enableWaterSurfaceToggle.addLabel(localization.format("WantsWaterSurface"), ESleekSide.RIGHT);
        enableWaterSurfaceToggle.state = LevelLighting.EditorWantsWaterSurface;
        enableWaterSurfaceToggle.isVisible = false;
        enableWaterSurfaceToggle.onToggled += OnWaterSurfaceToggled;
        AddChild(enableWaterSurfaceToggle);
        refreshCullingVolumesButton = Glazier.Get().CreateButton();
        refreshCullingVolumesButton.positionOffset_X = 400;
        refreshCullingVolumesButton.positionOffset_Y = -30;
        refreshCullingVolumesButton.positionScale_Y = 1f;
        refreshCullingVolumesButton.sizeOffset_X = 200;
        refreshCullingVolumesButton.sizeOffset_Y = 30;
        refreshCullingVolumesButton.text = localization.format("RefreshCullingVolumes");
        refreshCullingVolumesButton.tooltipText = localization.format("RefreshCullingVolumes_Tooltip");
        refreshCullingVolumesButton.isVisible = false;
        refreshCullingVolumesButton.onClickedButton += OnRefreshCullingVolumesClicked;
        AddChild(refreshCullingVolumesButton);
        previewCullingToggle = Glazier.Get().CreateToggle();
        previewCullingToggle.positionOffset_X = 400;
        previewCullingToggle.positionOffset_Y = -80;
        previewCullingToggle.positionScale_Y = 1f;
        previewCullingToggle.sizeOffset_X = 40;
        previewCullingToggle.sizeOffset_Y = 40;
        previewCullingToggle.addLabel(localization.format("PreviewCulling"), ESleekSide.RIGHT);
        previewCullingToggle.state = EditorWantsToPreviewCulling;
        previewCullingToggle.isVisible = false;
        previewCullingToggle.onToggled += OnPreviewCullingToggled;
        AddChild(previewCullingToggle);
        int num2 = 0;
        selectedTypeBox = Glazier.Get().CreateBox();
        selectedTypeBox.positionScale_X = 1f;
        selectedTypeBox.positionOffset_Y = num2;
        selectedTypeBox.sizeOffset_X = 300;
        selectedTypeBox.positionOffset_X = -selectedTypeBox.sizeOffset_X;
        selectedTypeBox.sizeOffset_Y = 30;
        selectedTypeBox.addLabel(localization.format("SelectedType_Label"), ESleekSide.LEFT);
        AddChild(selectedTypeBox);
        num2 += selectedTypeBox.sizeOffset_Y + 10;
        activeVisibilityButton = new SleekButtonState(new GUIContent(localization.format("Visibility_Hidden")), new GUIContent(localization.format("Visibility_Wireframe")), new GUIContent(localization.format("Visibility_Solid")));
        activeVisibilityButton.positionScale_X = 1f;
        activeVisibilityButton.positionOffset_Y = num2;
        activeVisibilityButton.sizeOffset_X = 300;
        activeVisibilityButton.positionOffset_X = -activeVisibilityButton.sizeOffset_X;
        activeVisibilityButton.sizeOffset_Y = 30;
        activeVisibilityButton.addLabel(localization.format("ActiveVisibility_Label"), ESleekSide.LEFT);
        SleekButtonState sleekButtonState = activeVisibilityButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedActiveVisibility));
        activeVisibilityButton.isVisible = false;
        AddChild(activeVisibilityButton);
        num2 += selectedTypeBox.sizeOffset_Y + 10;
        typeScrollView = Glazier.Get().CreateScrollView();
        typeScrollView.positionScale_X = 1f;
        typeScrollView.sizeOffset_X = 300;
        typeScrollView.positionOffset_X = -typeScrollView.sizeOffset_X;
        typeScrollView.positionOffset_Y = num2;
        typeScrollView.sizeOffset_Y = -num2;
        typeScrollView.sizeScale_Y = 1f;
        typeScrollView.scaleContentToWidth = true;
        AddChild(typeScrollView);
        int num3 = 0;
        int num4 = 0;
        volumeButtons = new VolumeTypeButton[list.Count];
        foreach (VolumeManagerBase item in list)
        {
            VolumeTypeButton volumeTypeButton = new VolumeTypeButton(this, item)
            {
                positionOffset_Y = num4,
                sizeScale_X = 1f,
                sizeOffset_Y = 30
            };
            typeScrollView.AddChild(volumeTypeButton);
            volumeButtons[num3] = volumeTypeButton;
            num4 += volumeTypeButton.sizeOffset_Y;
            num3++;
        }
        typeScrollView.contentSizeOffset = new Vector2(0f, num4);
    }

    internal void SetSelectedType(VolumeManagerBase type)
    {
        selectedTypeBox.text = type.FriendlyName;
        tool.activeVolumeManager = type;
        activeVisibilityButton.state = (int)type.Visibility;
        activeVisibilityButton.isVisible = true;
        enableUnderwaterEffectsToggle.isVisible = type is WaterVolumeManager;
        enableWaterSurfaceToggle.isVisible = enableUnderwaterEffectsToggle.isVisible;
        refreshCullingVolumesButton.isVisible = type is CullingVolumeManager;
        previewCullingToggle.isVisible = refreshCullingVolumesButton.isVisible;
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
        surfaceMaskField.state = (uint)DevkitSelectionToolOptions.instance.selectionMask;
        coordinateButton.state = (DevkitSelectionToolOptions.instance.localSpace ? 1 : 0);
        snapRotationField.state = DevkitSelectionToolOptions.instance.snapRotation;
        snapTransformField.state = DevkitSelectionToolOptions.instance.snapPosition;
    }
}
