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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(bundle.load<Texture2D>("Height"));
        sleekButtonIcon.PositionOffset_Y = 40f;
        sleekButtonIcon.SizeOffset_X = -5f;
        sleekButtonIcon.SizeOffset_Y = 30f;
        sleekButtonIcon.SizeScale_X = 0.25f;
        sleekButtonIcon.text = local.format("HeightButtonText") + " [1]";
        sleekButtonIcon.tooltip = local.format("HeightButtonTooltip");
        sleekButtonIcon.onClickedButton += onClickedHeightButton;
        container.AddChild(sleekButtonIcon);
        SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(bundle.load<Texture2D>("Materials"));
        sleekButtonIcon2.PositionOffset_X = 5f;
        sleekButtonIcon2.PositionOffset_Y = 40f;
        sleekButtonIcon2.PositionScale_X = 0.25f;
        sleekButtonIcon2.SizeOffset_X = -10f;
        sleekButtonIcon2.SizeOffset_Y = 30f;
        sleekButtonIcon2.SizeScale_X = 0.25f;
        sleekButtonIcon2.text = local.format("MaterialsButtonText") + " [2]";
        sleekButtonIcon2.tooltip = local.format("MaterialsButtonTooltip");
        sleekButtonIcon2.onClickedButton += onClickedMaterialsButton;
        container.AddChild(sleekButtonIcon2);
        SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(bundle.load<Texture2D>("Details"));
        sleekButtonIcon3.PositionOffset_X = 5f;
        sleekButtonIcon3.PositionOffset_Y = 40f;
        sleekButtonIcon3.PositionScale_X = 0.5f;
        sleekButtonIcon3.SizeOffset_X = -10f;
        sleekButtonIcon3.SizeOffset_Y = 30f;
        sleekButtonIcon3.SizeScale_X = 0.25f;
        sleekButtonIcon3.text = local.format("DetailsButtonText") + " [3]";
        sleekButtonIcon3.tooltip = local.format("DetailsButtonTooltip");
        sleekButtonIcon3.onClickedButton += onClickedDetailsButton;
        container.AddChild(sleekButtonIcon3);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.PositionOffset_X = 5f;
        sleekButton.PositionOffset_Y = 40f;
        sleekButton.PositionScale_X = 0.75f;
        sleekButton.SizeOffset_X = -5f;
        sleekButton.SizeOffset_Y = 30f;
        sleekButton.SizeScale_X = 0.25f;
        sleekButton.Text = local.format("TilesButton_Label") + " [4]";
        sleekButton.TooltipText = local.format("TilesButton_Tooltip");
        sleekButton.OnClicked += OnClickedTilesButton;
        container.AddChild(sleekButton);
        heightV2 = new EditorTerrainHeightUI();
        heightV2.PositionOffset_X = 10f;
        heightV2.PositionOffset_Y = 90f;
        heightV2.PositionScale_X = 1f;
        heightV2.SizeOffset_X = -20f;
        heightV2.SizeOffset_Y = -100f;
        heightV2.SizeScale_X = 1f;
        heightV2.SizeScale_Y = 1f;
        EditorUI.window.AddChild(heightV2);
        materialsV2 = new EditorTerrainMaterialsUI();
        materialsV2.PositionOffset_X = 10f;
        materialsV2.PositionOffset_Y = 90f;
        materialsV2.PositionScale_X = 1f;
        materialsV2.SizeOffset_X = -20f;
        materialsV2.SizeOffset_Y = -100f;
        materialsV2.SizeScale_X = 1f;
        materialsV2.SizeScale_Y = 1f;
        EditorUI.window.AddChild(materialsV2);
        detailsV2 = new EditorTerrainDetailsUI();
        detailsV2.PositionOffset_X = 10f;
        detailsV2.PositionOffset_Y = 90f;
        detailsV2.PositionScale_X = 1f;
        detailsV2.SizeOffset_X = -20f;
        detailsV2.SizeOffset_Y = -100f;
        detailsV2.SizeScale_X = 1f;
        detailsV2.SizeScale_Y = 1f;
        EditorUI.window.AddChild(detailsV2);
        tiles = new EditorTerrainTilesUI();
        tiles.PositionOffset_X = 10f;
        tiles.PositionOffset_Y = 90f;
        tiles.PositionScale_X = 1f;
        tiles.SizeOffset_X = -20f;
        tiles.SizeOffset_Y = -100f;
        tiles.SizeScale_X = 1f;
        tiles.SizeScale_Y = 1f;
        EditorUI.window.AddChild(tiles);
        bundle.unload();
    }
}
