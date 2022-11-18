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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        altToggle = Glazier.Get().CreateToggle();
        altToggle.positionOffset_Y = -180;
        altToggle.positionScale_Y = 1f;
        altToggle.sizeOffset_X = 40;
        altToggle.sizeOffset_Y = 40;
        altToggle.state = EditorSpawns.selectedAlt;
        altToggle.addLabel(local.format("AltLabelText"), ESleekSide.RIGHT);
        altToggle.onToggled += onToggledAltToggle;
        container.AddChild(altToggle);
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.positionOffset_Y = -130;
        radiusSlider.positionScale_Y = 1f;
        radiusSlider.sizeOffset_X = 200;
        radiusSlider.sizeOffset_Y = 20;
        radiusSlider.state = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.addLabel(local.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.onDragged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        rotationSlider = Glazier.Get().CreateSlider();
        rotationSlider.positionOffset_Y = -100;
        rotationSlider.positionScale_Y = 1f;
        rotationSlider.sizeOffset_X = 200;
        rotationSlider.sizeOffset_Y = 20;
        rotationSlider.state = EditorSpawns.rotation / 360f;
        rotationSlider.orientation = ESleekOrientation.HORIZONTAL;
        rotationSlider.addLabel(local.format("RotationSliderLabelText"), ESleekSide.RIGHT);
        rotationSlider.onDragged += onDraggedRotationSlider;
        container.AddChild(rotationSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.positionOffset_Y = -70;
        addButton.positionScale_Y = 1f;
        addButton.sizeOffset_X = 200;
        addButton.sizeOffset_Y = 30;
        addButton.text = local.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = local.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.positionOffset_Y = -30;
        removeButton.positionScale_Y = 1f;
        removeButton.sizeOffset_X = 200;
        removeButton.sizeOffset_Y = 30;
        removeButton.text = local.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = local.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
