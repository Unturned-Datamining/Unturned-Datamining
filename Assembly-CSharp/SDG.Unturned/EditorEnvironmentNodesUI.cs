using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using UnityEngine;

namespace SDG.Unturned;

internal class EditorEnvironmentNodesUI : SleekFullscreenBox
{
    internal Local localization;

    private NodesEditor tool;

    private GameObject focusedGameObject;

    private ISleekElement focusedItemMenu;

    private ISleekFloat32Field snapTransformField;

    private ISleekFloat32Field snapRotationField;

    private SleekButtonIcon transformButton;

    private SleekButtonIcon rotateButton;

    private SleekButtonIcon scaleButton;

    private SleekButtonState coordinateButton;

    private ISleekUInt32Field surfaceMaskField;

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
        TempNodeBase tempNodeBase = focusedGameObject?.GetComponent<TempNodeBase>();
        if (tempNodeBase != null)
        {
            focusedItemMenu = tempNodeBase.CreateMenu();
            if (focusedItemMenu != null)
            {
                focusedItemMenu.PositionOffset_Y = snapTransformField.PositionOffset_Y - 10f - focusedItemMenu.SizeOffset_Y;
                focusedItemMenu.PositionScale_Y = 1f;
                AddChild(focusedItemMenu);
            }
        }
    }

    public EditorEnvironmentNodesUI()
    {
        DevkitSelectionToolOptions.load();
        tool = new NodesEditor();
        localization = Localization.read("/Editor/EditorLevelNodes.dat");
        Local local = Localization.read("/Editor/EditorLevelObjects.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelObjects/EditorLevelObjects.unity3d");
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
        float num2 = 0f;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionScale_X = 1f;
        sleekElement.SizeOffset_X = 200f;
        sleekElement.PositionOffset_X = 0f - sleekElement.SizeOffset_X;
        sleekElement.SizeScale_Y = 1f;
        AddChild(sleekElement);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.PositionOffset_Y = num2;
        sleekButton.SizeScale_X = 1f;
        sleekButton.SizeOffset_Y = 30f;
        sleekButton.Text = "Airdrop Marker";
        sleekButton.OnClicked += delegate
        {
            tool.activeNodeSystem = AirdropDevkitNodeSystem.Get();
        };
        sleekElement.AddChild(sleekButton);
        num2 += sleekButton.SizeOffset_Y;
        ISleekButton sleekButton2 = Glazier.Get().CreateButton();
        sleekButton2.PositionOffset_Y = num2;
        sleekButton2.SizeScale_X = 1f;
        sleekButton2.SizeOffset_Y = 30f;
        sleekButton2.Text = "Named Location";
        sleekButton2.OnClicked += delegate
        {
            tool.activeNodeSystem = LocationDevkitNodeSystem.Get();
        };
        sleekElement.AddChild(sleekButton2);
        num2 += sleekButton2.SizeOffset_Y;
        ISleekButton sleekButton3 = Glazier.Get().CreateButton();
        sleekButton3.PositionOffset_Y = num2;
        sleekButton3.SizeScale_X = 1f;
        sleekButton3.SizeOffset_Y = 30f;
        sleekButton3.Text = "Spawnpoint";
        sleekButton3.OnClicked += delegate
        {
            tool.activeNodeSystem = SpawnpointSystemV2.Get();
        };
        sleekElement.AddChild(sleekButton3);
        _ = num2 + sleekButton3.SizeOffset_Y;
    }

    /// <summary>
    /// Other menus can modify DevkitSelectionToolOptions so we need to sync our menu when opened.
    /// </summary>
    private void SyncSettings()
    {
        surfaceMaskField.Value = (uint)DevkitSelectionToolOptions.instance.selectionMask;
        coordinateButton.state = (DevkitSelectionToolOptions.instance.localSpace ? 1 : 0);
        snapRotationField.Value = DevkitSelectionToolOptions.instance.snapRotation;
        snapTransformField.Value = DevkitSelectionToolOptions.instance.snapPosition;
    }

    private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
    {
        DevkitSelectionToolOptions.instance.selectionMask = (ERayMask)state;
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
}
