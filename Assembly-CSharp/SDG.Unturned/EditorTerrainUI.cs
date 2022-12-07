using UnityEngine;

namespace SDG.Unturned;

public class EditorTerrainUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private EditorTerrainHeightUI heightV2;

    private EditorTerrainMaterialsUI materialsV2;

    private EditorTerrainDetailsUI detailsV2;

    private EditorTerrainTilesUI tiles;

    public void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            heightV2.Close();
            materialsV2.Close();
            detailsV2.Close();
            tiles.Close();
            container.AnimateOutOfView(1f, 0f);
        }
    }

    public void GoToHeightsTab()
    {
        detailsV2.Close();
        materialsV2.Close();
        tiles.Close();
        heightV2.Open();
    }

    public void GoToMaterialsTab()
    {
        heightV2.Close();
        detailsV2.Close();
        tiles.Close();
        materialsV2.Open();
    }

    public void GoToFoliageTab()
    {
        heightV2.Close();
        materialsV2.Close();
        tiles.Close();
        detailsV2.Open();
    }

    public void GoToTilesTab()
    {
        heightV2.Close();
        materialsV2.Close();
        detailsV2.Close();
        tiles.Open();
    }

    private void onClickedHeightButton(ISleekElement button)
    {
        GoToHeightsTab();
    }

    private void onClickedMaterialsButton(ISleekElement button)
    {
        GoToMaterialsTab();
    }

    private void onClickedDetailsButton(ISleekElement button)
    {
        GoToFoliageTab();
    }

    private void OnClickedTilesButton(ISleekElement button)
    {
        GoToTilesTab();
    }

    public EditorTerrainUI()
    {
        Local local = Localization.read("/Editor/EditorTerrain.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorTerrain/EditorTerrain.unity3d");
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
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(bundle.load<Texture2D>("Height"));
        sleekButtonIcon.positionOffset_Y = 40;
        sleekButtonIcon.sizeOffset_X = -5;
        sleekButtonIcon.sizeOffset_Y = 30;
        sleekButtonIcon.sizeScale_X = 0.25f;
        sleekButtonIcon.text = local.format("HeightButtonText") + " [1]";
        sleekButtonIcon.tooltip = local.format("HeightButtonTooltip");
        sleekButtonIcon.onClickedButton += onClickedHeightButton;
        container.AddChild(sleekButtonIcon);
        SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(bundle.load<Texture2D>("Materials"));
        sleekButtonIcon2.positionOffset_X = 5;
        sleekButtonIcon2.positionOffset_Y = 40;
        sleekButtonIcon2.positionScale_X = 0.25f;
        sleekButtonIcon2.sizeOffset_X = -10;
        sleekButtonIcon2.sizeOffset_Y = 30;
        sleekButtonIcon2.sizeScale_X = 0.25f;
        sleekButtonIcon2.text = local.format("MaterialsButtonText") + " [2]";
        sleekButtonIcon2.tooltip = local.format("MaterialsButtonTooltip");
        sleekButtonIcon2.onClickedButton += onClickedMaterialsButton;
        container.AddChild(sleekButtonIcon2);
        SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(bundle.load<Texture2D>("Details"));
        sleekButtonIcon3.positionOffset_X = 5;
        sleekButtonIcon3.positionOffset_Y = 40;
        sleekButtonIcon3.positionScale_X = 0.5f;
        sleekButtonIcon3.sizeOffset_X = -10;
        sleekButtonIcon3.sizeOffset_Y = 30;
        sleekButtonIcon3.sizeScale_X = 0.25f;
        sleekButtonIcon3.text = local.format("DetailsButtonText") + " [3]";
        sleekButtonIcon3.tooltip = local.format("DetailsButtonTooltip");
        sleekButtonIcon3.onClickedButton += onClickedDetailsButton;
        container.AddChild(sleekButtonIcon3);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_X = 5;
        sleekButton.positionOffset_Y = 40;
        sleekButton.positionScale_X = 0.75f;
        sleekButton.sizeOffset_X = -5;
        sleekButton.sizeOffset_Y = 30;
        sleekButton.sizeScale_X = 0.25f;
        sleekButton.text = local.format("TilesButton_Label") + " [4]";
        sleekButton.tooltipText = local.format("TilesButton_Tooltip");
        sleekButton.onClickedButton += OnClickedTilesButton;
        container.AddChild(sleekButton);
        heightV2 = new EditorTerrainHeightUI();
        heightV2.positionOffset_X = 10;
        heightV2.positionOffset_Y = 90;
        heightV2.positionScale_X = 1f;
        heightV2.sizeOffset_X = -20;
        heightV2.sizeOffset_Y = -100;
        heightV2.sizeScale_X = 1f;
        heightV2.sizeScale_Y = 1f;
        EditorUI.window.AddChild(heightV2);
        materialsV2 = new EditorTerrainMaterialsUI();
        materialsV2.positionOffset_X = 10;
        materialsV2.positionOffset_Y = 90;
        materialsV2.positionScale_X = 1f;
        materialsV2.sizeOffset_X = -20;
        materialsV2.sizeOffset_Y = -100;
        materialsV2.sizeScale_X = 1f;
        materialsV2.sizeScale_Y = 1f;
        EditorUI.window.AddChild(materialsV2);
        detailsV2 = new EditorTerrainDetailsUI();
        detailsV2.positionOffset_X = 10;
        detailsV2.positionOffset_Y = 90;
        detailsV2.positionScale_X = 1f;
        detailsV2.sizeOffset_X = -20;
        detailsV2.sizeOffset_Y = -100;
        detailsV2.sizeScale_X = 1f;
        detailsV2.sizeScale_Y = 1f;
        EditorUI.window.AddChild(detailsV2);
        tiles = new EditorTerrainTilesUI();
        tiles.positionOffset_X = 10;
        tiles.positionOffset_Y = 90;
        tiles.positionScale_X = 1f;
        tiles.sizeOffset_X = -20;
        tiles.sizeOffset_Y = -100;
        tiles.sizeScale_X = 1f;
        tiles.sizeScale_Y = 1f;
        EditorUI.window.AddChild(tiles);
        bundle.unload();
    }
}
