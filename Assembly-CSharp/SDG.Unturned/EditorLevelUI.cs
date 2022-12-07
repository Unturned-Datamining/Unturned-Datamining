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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        objectsButton = new SleekButtonIcon(bundle.load<Texture2D>("Objects"));
        objectsButton.positionOffset_Y = 40;
        objectsButton.sizeOffset_X = -5;
        objectsButton.sizeOffset_Y = 30;
        objectsButton.sizeScale_X = 0.25f;
        objectsButton.text = local.format("ObjectsButtonText");
        objectsButton.tooltip = local.format("ObjectsButtonTooltip");
        objectsButton.onClickedButton += onClickedObjectsButton;
        container.AddChild(objectsButton);
        visibilityButton = new SleekButtonIcon(bundle.load<Texture2D>("Visibility"));
        visibilityButton.positionOffset_X = 5;
        visibilityButton.positionOffset_Y = 40;
        visibilityButton.positionScale_X = 0.25f;
        visibilityButton.sizeOffset_X = -10;
        visibilityButton.sizeOffset_Y = 30;
        visibilityButton.sizeScale_X = 0.25f;
        visibilityButton.text = local.format("VisibilityButtonText");
        visibilityButton.tooltip = local.format("VisibilityButtonTooltip");
        visibilityButton.onClickedButton += onClickedVisibilityButton;
        container.AddChild(visibilityButton);
        playersButton = new SleekButtonIcon(bundle.load<Texture2D>("Players"));
        playersButton.positionOffset_Y = 40;
        playersButton.positionOffset_X = 5;
        playersButton.positionScale_X = 0.5f;
        playersButton.sizeOffset_X = -10;
        playersButton.sizeOffset_Y = 30;
        playersButton.sizeScale_X = 0.25f;
        playersButton.text = local.format("PlayersButtonText");
        playersButton.tooltip = local.format("PlayersButtonTooltip");
        playersButton.onClickedButton += onClickedPlayersButton;
        container.AddChild(playersButton);
        volumesButton = new SleekButtonIcon(null);
        volumesButton.positionOffset_Y = 40;
        volumesButton.positionOffset_X = 5;
        volumesButton.positionScale_X = 0.75f;
        volumesButton.sizeOffset_X = -5;
        volumesButton.sizeOffset_Y = 30;
        volumesButton.sizeScale_X = 0.25f;
        volumesButton.text = local.format("VolumesButtonText");
        volumesButton.tooltip = local.format("VolumesButtonTooltip");
        volumesButton.onClickedButton += OnClickedVolumesButton;
        container.AddChild(volumesButton);
        bundle.unload();
        objectsUI = new EditorLevelObjectsUI();
        objectsUI.positionOffset_X = 10;
        objectsUI.positionOffset_Y = 90;
        objectsUI.positionScale_X = 1f;
        objectsUI.sizeOffset_X = -20;
        objectsUI.sizeOffset_Y = -100;
        objectsUI.sizeScale_X = 1f;
        objectsUI.sizeScale_Y = 1f;
        EditorUI.window.AddChild(objectsUI);
        new EditorLevelVisibilityUI();
        new EditorLevelPlayersUI();
        volumesUI = new EditorVolumesUI();
        volumesUI.positionOffset_X = 10;
        volumesUI.positionOffset_Y = 90;
        volumesUI.positionScale_X = 1f;
        volumesUI.sizeOffset_X = -20;
        volumesUI.sizeOffset_Y = -100;
        volumesUI.sizeScale_X = 1f;
        volumesUI.sizeScale_Y = 1f;
        EditorUI.window.AddChild(volumesUI);
    }
}
