using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using UnityEngine;

namespace SDG.Unturned;

internal class EditorEnvironmentNodesUI : SleekFullscreenBox
{
    internal Local localization;

    private NodesEditorV2 tool;

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
                focusedItemMenu.positionOffset_Y = snapTransformField.positionOffset_Y - 10 - focusedItemMenu.sizeOffset_Y;
                focusedItemMenu.positionScale_Y = 1f;
                AddChild(focusedItemMenu);
            }
        }
    }

    public EditorEnvironmentNodesUI()
    {
        DevkitSelectionToolOptions.load();
        tool = new NodesEditorV2();
        localization = Localization.read("/Editor/EditorLevelNodes.dat");
        Local local = Localization.read("/Editor/EditorLevelObjects.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelObjects/EditorLevelObjects.unity3d");
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
        int num2 = 0;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.positionScale_X = 1f;
        sleekElement.sizeOffset_X = 200;
        sleekElement.positionOffset_X = -sleekElement.sizeOffset_X;
        sleekElement.sizeScale_Y = 1f;
        AddChild(sleekElement);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_Y = num2;
        sleekButton.sizeScale_X = 1f;
        sleekButton.sizeOffset_Y = 30;
        sleekButton.text = "Airdrop Marker";
        sleekButton.onClickedButton += delegate
        {
            tool.activeNodeSystem = AirdropDevkitNodeSystem.Get();
        };
        sleekElement.AddChild(sleekButton);
        num2 += sleekButton.sizeOffset_Y;
        ISleekButton sleekButton2 = Glazier.Get().CreateButton();
        sleekButton2.positionOffset_Y = num2;
        sleekButton2.sizeScale_X = 1f;
        sleekButton2.sizeOffset_Y = 30;
        sleekButton2.text = "Named Location";
        sleekButton2.onClickedButton += delegate
        {
            tool.activeNodeSystem = LocationDevkitNodeSystem.Get();
        };
        sleekElement.AddChild(sleekButton2);
        num2 += sleekButton2.sizeOffset_Y;
        ISleekButton sleekButton3 = Glazier.Get().CreateButton();
        sleekButton3.positionOffset_Y = num2;
        sleekButton3.sizeScale_X = 1f;
        sleekButton3.sizeOffset_Y = 30;
        sleekButton3.text = "Spawnpoint";
        sleekButton3.onClickedButton += delegate
        {
            tool.activeNodeSystem = SpawnpointSystemV2.Get();
        };
        sleekElement.AddChild(sleekButton3);
        _ = num2 + sleekButton3.sizeOffset_Y;
    }

    private void SyncSettings()
    {
        surfaceMaskField.state = (uint)DevkitSelectionToolOptions.instance.selectionMask;
        coordinateButton.state = (DevkitSelectionToolOptions.instance.localSpace ? 1 : 0);
        snapRotationField.state = DevkitSelectionToolOptions.instance.snapRotation;
        snapTransformField.state = DevkitSelectionToolOptions.instance.snapPosition;
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
        tool.mode = SelectionToolV2.ESelectionMode.POSITION;
    }

    private void OnRotateClicked(ISleekElement button)
    {
        tool.mode = SelectionToolV2.ESelectionMode.ROTATION;
    }

    private void OnScaleClicked(ISleekElement button)
    {
        tool.mode = SelectionToolV2.ESelectionMode.SCALE;
    }

    private void OnSwappedStateCoordinate(SleekButtonState button, int index)
    {
        DevkitSelectionToolOptions.instance.localSpace = index > 0;
    }
}
