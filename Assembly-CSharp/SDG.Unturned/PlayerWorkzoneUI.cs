using UnityEngine;

namespace SDG.Unturned;

public class PlayerWorkzoneUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekImage dragBox;

    private static ISleekFloat32Field snapTransformField;

    private static ISleekFloat32Field snapRotationField;

    private static SleekButtonIcon transformButton;

    private static SleekButtonIcon rotateButton;

    public static SleekButtonState coordinateButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            Player.player.workzone.isBuilding = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            Player.player.workzone.isBuilding = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onDragStarted(Vector2 minViewportPoint, Vector2 maxViewportPoint)
    {
        Vector2 vector = PlayerUI.window.ViewportToNormalizedPosition(minViewportPoint);
        Vector2 vector2 = PlayerUI.window.ViewportToNormalizedPosition(maxViewportPoint);
        if (vector2.y < vector.y)
        {
            float y = vector2.y;
            vector2.y = vector.y;
            vector.y = y;
        }
        dragBox.positionScale_X = vector.x;
        dragBox.positionScale_Y = vector.y;
        dragBox.sizeScale_X = vector2.x - vector.x;
        dragBox.sizeScale_Y = vector2.y - vector.y;
        dragBox.isVisible = true;
    }

    private static void onDragStopped()
    {
        dragBox.isVisible = false;
    }

    private static void onTypedSnapTransformField(ISleekFloat32Field field, float value)
    {
        Player.player.workzone.snapTransform = value;
    }

    private static void onTypedSnapRotationField(ISleekFloat32Field field, float value)
    {
        Player.player.workzone.snapRotation = value;
    }

    private static void onClickedTransformButton(ISleekElement button)
    {
        Player.player.workzone.dragMode = EDragMode.TRANSFORM;
    }

    private static void onClickedRotateButton(ISleekElement button)
    {
        Player.player.workzone.dragMode = EDragMode.ROTATE;
    }

    private static void onSwappedStateCoordinate(SleekButtonState button, int index)
    {
        Player.player.workzone.dragCoordinate = (EDragCoordinate)index;
    }

    public PlayerWorkzoneUI()
    {
        Local local = Localization.read("/Editor/EditorLevelObjects.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelObjects/EditorLevelObjects.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.window.AddChild(container);
        active = false;
        Player.player.workzone.onDragStarted = onDragStarted;
        Player.player.workzone.onDragStopped = onDragStopped;
        dragBox = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        dragBox.color = new Color(1f, 1f, 0f, 0.2f);
        PlayerUI.window.AddChild(dragBox);
        dragBox.isVisible = false;
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.positionOffset_Y = -190;
        snapTransformField.positionScale_Y = 1f;
        snapTransformField.sizeOffset_X = 200;
        snapTransformField.sizeOffset_Y = 30;
        snapTransformField.state = Player.player.workzone.snapTransform;
        snapTransformField.addLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.onTypedSingle += onTypedSnapTransformField;
        container.AddChild(snapTransformField);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.positionOffset_Y = -150;
        snapRotationField.positionScale_Y = 1f;
        snapRotationField.sizeOffset_X = 200;
        snapRotationField.sizeOffset_Y = 30;
        snapRotationField.state = Player.player.workzone.snapRotation;
        snapRotationField.addLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.onTypedSingle += onTypedSnapRotationField;
        container.AddChild(snapRotationField);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.positionOffset_Y = -110;
        transformButton.positionScale_Y = 1f;
        transformButton.sizeOffset_X = 200;
        transformButton.sizeOffset_Y = 30;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += onClickedTransformButton;
        container.AddChild(transformButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.positionOffset_Y = -70;
        rotateButton.positionScale_Y = 1f;
        rotateButton.sizeOffset_X = 200;
        rotateButton.sizeOffset_Y = 30;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += onClickedRotateButton;
        container.AddChild(rotateButton);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.positionOffset_Y = -30;
        coordinateButton.positionScale_Y = 1f;
        coordinateButton.sizeOffset_X = 200;
        coordinateButton.sizeOffset_Y = 30;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = onSwappedStateCoordinate;
        container.AddChild(coordinateButton);
        bundle.unload();
    }
}
