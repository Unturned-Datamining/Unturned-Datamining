using SDG.Framework.Landscapes;

namespace SDG.Unturned;

internal class TerrainTileLayer : SleekWrapper
{
    private EditorTerrainTilesUI owner;

    private int layerIndex;

    private ISleekBox layerBox;

    private ISleekButton nameButton;

    public void UpdateSelectedTile()
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null)
        {
            AssetReference<LandscapeMaterialAsset> assetReference = selectedTile.materials[layerIndex];
            LandscapeMaterialAsset landscapeMaterialAsset = assetReference.Find();
            if (landscapeMaterialAsset != null)
            {
                nameButton.text = landscapeMaterialAsset.FriendlyName;
            }
            else if (assetReference.isNull)
            {
                nameButton.text = EditorTerrainTilesUI.localization.format("LayerNull");
            }
            else
            {
                nameButton.text = assetReference.GUID.ToString("N");
            }
        }
        else
        {
            nameButton.text = string.Empty;
        }
    }

    public TerrainTileLayer(EditorTerrainTilesUI owner, int layerIndex)
    {
        this.owner = owner;
        this.layerIndex = layerIndex;
        layerBox = Glazier.Get().CreateBox();
        layerBox.sizeOffset_X = 30;
        layerBox.sizeScale_Y = 1f;
        layerBox.text = layerIndex.ToString();
        AddChild(layerBox);
        nameButton = Glazier.Get().CreateButton();
        nameButton.positionOffset_X = 30;
        nameButton.sizeScale_X = 1f;
        nameButton.sizeScale_Y = 1f;
        nameButton.sizeOffset_X = -30;
        nameButton.onClickedButton += OnClicked;
        AddChild(nameButton);
        UpdateSelectedTile();
    }

    private void OnClicked(ISleekElement element)
    {
        owner.SetSelectedLayerIndex(layerIndex);
    }
}
