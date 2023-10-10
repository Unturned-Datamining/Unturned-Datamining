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
        dragBox.PositionScale_X = vector.x;
        dragBox.PositionScale_Y = vector.y;
        dragBox.SizeScale_X = vector2.x - vector.x;
        dragBox.SizeScale_Y = vector2.y - vector.y;
        dragBox.IsVisible = true;
    }

    private static void onDragStopped()
    {
        dragBox.IsVisible = false;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.window.AddChild(container);
        active = false;
        Player.player.workzone.onDragStarted = onDragStarted;
        Player.player.workzone.onDragStopped = onDragStopped;
        dragBox = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        dragBox.TintColor = new Color(1f, 1f, 0f, 0.2f);
        PlayerUI.window.AddChild(dragBox);
        dragBox.IsVisible = false;
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.PositionOffset_Y = -190f;
        snapTransformField.PositionScale_Y = 1f;
        snapTransformField.SizeOffset_X = 200f;
        snapTransformField.SizeOffset_Y = 30f;
        snapTransformField.Value = Player.player.workzone.snapTransform;
        snapTransformField.AddLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.OnValueChanged += onTypedSnapTransformField;
        container.AddChild(snapTransformField);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.PositionOffset_Y = -150f;
        snapRotationField.PositionScale_Y = 1f;
        snapRotationField.SizeOffset_X = 200f;
        snapRotationField.SizeOffset_Y = 30f;
        snapRotationField.Value = Player.player.workzone.snapRotation;
        snapRotationField.AddLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.OnValueChanged += onTypedSnapRotationField;
        container.AddChild(snapRotationField);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.PositionOffset_Y = -110f;
        transformButton.PositionScale_Y = 1f;
        transformButton.SizeOffset_X = 200f;
        transformButton.SizeOffset_Y = 30f;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += onClickedTransformButton;
        container.AddChild(transformButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.PositionOffset_Y = -70f;
        rotateButton.PositionScale_Y = 1f;
        rotateButton.SizeOffset_X = 200f;
        rotateButton.SizeOffset_Y = 30f;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += onClickedRotateButton;
        container.AddChild(rotateButton);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.PositionOffset_Y = -30f;
        coordinateButton.PositionScale_Y = 1f;
        coordinateButton.SizeOffset_X = 200f;
        coordinateButton.SizeOffset_Y = 30f;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = onSwappedStateCoordinate;
        container.AddChild(coordinateButton);
        bundle.unload();
    }
}
