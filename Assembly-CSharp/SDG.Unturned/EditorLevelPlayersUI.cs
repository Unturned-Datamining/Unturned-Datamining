using UnityEngine;

namespace SDG.Unturned;

public class EditorLevelPlayersUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekToggle altToggle;

    private static ISleekSlider radiusSlider;

    private static ISleekSlider rotationSlider;

    private static SleekButtonIcon addButton;

    private static SleekButtonIcon removeButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorSpawns.isSpawning = true;
            EditorSpawns.spawnMode = ESpawnMode.ADD_PLAYER;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorSpawns.isSpawning = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onToggledAltToggle(ISleekToggle toggle, bool state)
    {
        EditorSpawns.selectedAlt = state;
    }

    private static void onDraggedRadiusSlider(ISleekSlider slider, float state)
    {
        EditorSpawns.radius = (byte)((float)(int)EditorSpawns.MIN_REMOVE_SIZE + state * (float)(int)EditorSpawns.MAX_REMOVE_SIZE);
    }

    private static void onDraggedRotationSlider(ISleekSlider slider, float state)
    {
        EditorSpawns.rotation = state * 360f;
    }

    private static void onClickedAddButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.ADD_PLAYER;
    }

    private static void onClickedRemoveButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.REMOVE_PLAYER;
    }

    public EditorLevelPlayersUI()
    {
        Local local = Localization.read("/Editor/EditorLevelPlayers.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelPlayers/EditorLevelPlayers.unity3d");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        altToggle = Glazier.Get().CreateToggle();
        altToggle.PositionOffset_Y = -180f;
        altToggle.PositionScale_Y = 1f;
        altToggle.SizeOffset_X = 40f;
        altToggle.SizeOffset_Y = 40f;
        altToggle.Value = EditorSpawns.selectedAlt;
        altToggle.AddLabel(local.format("AltLabelText"), ESleekSide.RIGHT);
        altToggle.OnValueChanged += onToggledAltToggle;
        container.AddChild(altToggle);
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.PositionOffset_Y = -130f;
        radiusSlider.PositionScale_Y = 1f;
        radiusSlider.SizeOffset_X = 200f;
        radiusSlider.SizeOffset_Y = 20f;
        radiusSlider.Value = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.AddLabel(local.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.OnValueChanged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        rotationSlider = Glazier.Get().CreateSlider();
        rotationSlider.PositionOffset_Y = -100f;
        rotationSlider.PositionScale_Y = 1f;
        rotationSlider.SizeOffset_X = 200f;
        rotationSlider.SizeOffset_Y = 20f;
        rotationSlider.Value = EditorSpawns.rotation / 360f;
        rotationSlider.Orientation = ESleekOrientation.HORIZONTAL;
        rotationSlider.AddLabel(local.format("RotationSliderLabelText"), ESleekSide.RIGHT);
        rotationSlider.OnValueChanged += onDraggedRotationSlider;
        container.AddChild(rotationSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.PositionOffset_Y = -70f;
        addButton.PositionScale_Y = 1f;
        addButton.SizeOffset_X = 200f;
        addButton.SizeOffset_Y = 30f;
        addButton.text = local.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = local.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.PositionOffset_Y = -30f;
        removeButton.PositionScale_Y = 1f;
        removeButton.SizeOffset_X = 200f;
        removeButton.SizeOffset_Y = 30f;
        removeButton.text = local.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = local.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
