using UnityEngine;

namespace SDG.Unturned;

public class EditorDashboardUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    private static SleekButtonIcon terrainButton;

    private static SleekButtonIcon environmentButton;

    private static SleekButtonIcon spawnsButton;

    private static SleekButtonIcon levelButton;

    internal EditorTerrainUI terrainMenu;

    private EditorEnvironmentUI environmentUI;

    private EditorLevelUI levelUI;

    private void onClickedTerrainButton(ISleekElement button)
    {
        terrainMenu.open();
        EditorEnvironmentUI.close();
        EditorSpawnsUI.close();
        EditorLevelUI.close();
    }

    private void onClickedEnvironmentButton(ISleekElement button)
    {
        terrainMenu.close();
        EditorEnvironmentUI.open();
        EditorSpawnsUI.close();
        EditorLevelUI.close();
    }

    private void onClickedSpawnsButton(ISleekElement button)
    {
        terrainMenu.close();
        EditorEnvironmentUI.close();
        EditorSpawnsUI.open();
        EditorLevelUI.close();
    }

    private void onClickedLevelButton(ISleekElement button)
    {
        terrainMenu.close();
        EditorEnvironmentUI.close();
        EditorSpawnsUI.close();
        EditorLevelUI.open();
    }

    public void OnDestroy()
    {
        environmentUI.OnDestroy();
        levelUI.OnDestroy();
    }

    public EditorDashboardUI()
    {
        localization = Localization.read("/Editor/EditorDashboard.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorDashboard/EditorDashboard.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        terrainButton = new SleekButtonIcon(bundle.load<Texture2D>("Terrain"));
        terrainButton.sizeOffset_X = -5;
        terrainButton.sizeOffset_Y = 30;
        terrainButton.sizeScale_X = 0.25f;
        terrainButton.text = localization.format("TerrainButtonText");
        terrainButton.tooltip = localization.format("TerrainButtonTooltip");
        terrainButton.onClickedButton += onClickedTerrainButton;
        container.AddChild(terrainButton);
        environmentButton = new SleekButtonIcon(bundle.load<Texture2D>("Environment"));
        environmentButton.positionOffset_X = 5;
        environmentButton.positionScale_X = 0.25f;
        environmentButton.sizeOffset_X = -10;
        environmentButton.sizeOffset_Y = 30;
        environmentButton.sizeScale_X = 0.25f;
        environmentButton.text = localization.format("EnvironmentButtonText");
        environmentButton.tooltip = localization.format("EnvironmentButtonTooltip");
        environmentButton.onClickedButton += onClickedEnvironmentButton;
        container.AddChild(environmentButton);
        spawnsButton = new SleekButtonIcon(bundle.load<Texture2D>("Spawns"));
        spawnsButton.positionOffset_X = 5;
        spawnsButton.positionScale_X = 0.5f;
        spawnsButton.sizeOffset_X = -10;
        spawnsButton.sizeOffset_Y = 30;
        spawnsButton.sizeScale_X = 0.25f;
        spawnsButton.text = localization.format("SpawnsButtonText");
        spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
        spawnsButton.onClickedButton += onClickedSpawnsButton;
        container.AddChild(spawnsButton);
        levelButton = new SleekButtonIcon(bundle.load<Texture2D>("Level"));
        levelButton.positionOffset_X = 5;
        levelButton.positionScale_X = 0.75f;
        levelButton.sizeOffset_X = -5;
        levelButton.sizeOffset_Y = 30;
        levelButton.sizeScale_X = 0.25f;
        levelButton.text = localization.format("LevelButtonText");
        levelButton.tooltip = localization.format("LevelButtonTooltip");
        levelButton.onClickedButton += onClickedLevelButton;
        container.AddChild(levelButton);
        bundle.unload();
        new EditorPauseUI();
        terrainMenu = new EditorTerrainUI();
        environmentUI = new EditorEnvironmentUI();
        new EditorSpawnsUI();
        levelUI = new EditorLevelUI();
    }
}
