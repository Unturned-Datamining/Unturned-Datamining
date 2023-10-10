using UnityEngine;

namespace SDG.Unturned;

public class EditorLevelUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon objectsButton;

    private static SleekButtonIcon visibilityButton;

    private static SleekButtonIcon playersButton;

    private static SleekButtonIcon volumesButton;

    private EditorLevelObjectsUI objectsUI;

    private static EditorVolumesUI volumesUI;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorLevelObjectsUI.close();
            EditorLevelVisibilityUI.close();
            EditorLevelPlayersUI.close();
            volumesUI.Close();
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private void onClickedObjectsButton(ISleekElement button)
    {
        EditorLevelObjectsUI.open();
        EditorLevelVisibilityUI.close();
        EditorLevelPlayersUI.close();
        volumesUI.Close();
    }

    private void onClickedVisibilityButton(ISleekElement button)
    {
        EditorLevelObjectsUI.close();
        EditorLevelVisibilityUI.open();
        EditorLevelPlayersUI.close();
        volumesUI.Close();
    }

    private void onClickedPlayersButton(ISleekElement button)
    {
        EditorLevelObjectsUI.close();
        EditorLevelVisibilityUI.close();
        EditorLevelPlayersUI.open();
        volumesUI.Close();
    }

    private void OnClickedVolumesButton(ISleekElement button)
    {
        EditorLevelObjectsUI.close();
        EditorLevelVisibilityUI.close();
        EditorLevelPlayersUI.close();
        volumesUI.Open();
    }

    public void OnDestroy()
    {
        objectsUI.OnDestroy();
        volumesUI.OnDestroy();
    }

    public EditorLevelUI()
    {
        Local local = Localization.read("/Editor/EditorLevel.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevel/EditorLevel.unity3d");
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
        objectsButton = new SleekButtonIcon(bundle.load<Texture2D>("Objects"));
        objectsButton.PositionOffset_Y = 40f;
        objectsButton.SizeOffset_X = -5f;
        objectsButton.SizeOffset_Y = 30f;
        objectsButton.SizeScale_X = 0.25f;
        objectsButton.text = local.format("ObjectsButtonText");
        objectsButton.tooltip = local.format("ObjectsButtonTooltip");
        objectsButton.onClickedButton += onClickedObjectsButton;
        container.AddChild(objectsButton);
        visibilityButton = new SleekButtonIcon(bundle.load<Texture2D>("Visibility"));
        visibilityButton.PositionOffset_X = 5f;
        visibilityButton.PositionOffset_Y = 40f;
        visibilityButton.PositionScale_X = 0.25f;
        visibilityButton.SizeOffset_X = -10f;
        visibilityButton.SizeOffset_Y = 30f;
        visibilityButton.SizeScale_X = 0.25f;
        visibilityButton.text = local.format("VisibilityButtonText");
        visibilityButton.tooltip = local.format("VisibilityButtonTooltip");
        visibilityButton.onClickedButton += onClickedVisibilityButton;
        container.AddChild(visibilityButton);
        playersButton = new SleekButtonIcon(bundle.load<Texture2D>("Players"));
        playersButton.PositionOffset_Y = 40f;
        playersButton.PositionOffset_X = 5f;
        playersButton.PositionScale_X = 0.5f;
        playersButton.SizeOffset_X = -10f;
        playersButton.SizeOffset_Y = 30f;
        playersButton.SizeScale_X = 0.25f;
        playersButton.text = local.format("PlayersButtonText");
        playersButton.tooltip = local.format("PlayersButtonTooltip");
        playersButton.onClickedButton += onClickedPlayersButton;
        container.AddChild(playersButton);
        volumesButton = new SleekButtonIcon(null);
        volumesButton.PositionOffset_Y = 40f;
        volumesButton.PositionOffset_X = 5f;
        volumesButton.PositionScale_X = 0.75f;
        volumesButton.SizeOffset_X = -5f;
        volumesButton.SizeOffset_Y = 30f;
        volumesButton.SizeScale_X = 0.25f;
        volumesButton.text = local.format("VolumesButtonText");
        volumesButton.tooltip = local.format("VolumesButtonTooltip");
        volumesButton.onClickedButton += OnClickedVolumesButton;
        container.AddChild(volumesButton);
        bundle.unload();
        objectsUI = new EditorLevelObjectsUI();
        objectsUI.PositionOffset_X = 10f;
        objectsUI.PositionOffset_Y = 90f;
        objectsUI.PositionScale_X = 1f;
        objectsUI.SizeOffset_X = -20f;
        objectsUI.SizeOffset_Y = -100f;
        objectsUI.SizeScale_X = 1f;
        objectsUI.SizeScale_Y = 1f;
        EditorUI.window.AddChild(objectsUI);
        new EditorLevelVisibilityUI();
        new EditorLevelPlayersUI();
        volumesUI = new EditorVolumesUI();
        volumesUI.PositionOffset_X = 10f;
        volumesUI.PositionOffset_Y = 90f;
        volumesUI.PositionScale_X = 1f;
        volumesUI.SizeOffset_X = -20f;
        volumesUI.SizeOffset_Y = -100f;
        volumesUI.SizeScale_X = 1f;
        volumesUI.SizeScale_Y = 1f;
        EditorUI.window.AddChild(volumesUI);
    }
}
