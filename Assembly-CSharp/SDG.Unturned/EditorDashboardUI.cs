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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        terrainButton = new SleekButtonIcon(bundle.load<Texture2D>("Terrain"));
        terrainButton.SizeOffset_X = -5f;
        terrainButton.SizeOffset_Y = 30f;
        terrainButton.SizeScale_X = 0.25f;
        terrainButton.text = localization.format("TerrainButtonText");
        terrainButton.tooltip = localization.format("TerrainButtonTooltip");
        terrainButton.onClickedButton += onClickedTerrainButton;
        container.AddChild(terrainButton);
        environmentButton = new SleekButtonIcon(bundle.load<Texture2D>("Environment"));
        environmentButton.PositionOffset_X = 5f;
        environmentButton.PositionScale_X = 0.25f;
        environmentButton.SizeOffset_X = -10f;
        environmentButton.SizeOffset_Y = 30f;
        environmentButton.SizeScale_X = 0.25f;
        environmentButton.text = localization.format("EnvironmentButtonText");
        environmentButton.tooltip = localization.format("EnvironmentButtonTooltip");
        environmentButton.onClickedButton += onClickedEnvironmentButton;
        container.AddChild(environmentButton);
        spawnsButton = new SleekButtonIcon(bundle.load<Texture2D>("Spawns"));
        spawnsButton.PositionOffset_X = 5f;
        spawnsButton.PositionScale_X = 0.5f;
        spawnsButton.SizeOffset_X = -10f;
        spawnsButton.SizeOffset_Y = 30f;
        spawnsButton.SizeScale_X = 0.25f;
        spawnsButton.text = localization.format("SpawnsButtonText");
        spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
        spawnsButton.onClickedButton += onClickedSpawnsButton;
        container.AddChild(spawnsButton);
        levelButton = new SleekButtonIcon(bundle.load<Texture2D>("Level"));
        levelButton.PositionOffset_X = 5f;
        levelButton.PositionScale_X = 0.75f;
        levelButton.SizeOffset_X = -5f;
        levelButton.SizeOffset_Y = 30f;
        levelButton.SizeScale_X = 0.25f;
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
