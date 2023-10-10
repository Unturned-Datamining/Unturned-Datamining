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
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null)
        {
            AssetReference<LandscapeMaterialAsset> assetReference = selectedTile.materials[layerIndex];
            LandscapeMaterialAsset landscapeMaterialAsset = assetReference.Find();
            if (landscapeMaterialAsset != null)
            {
                nameButton.Text = landscapeMaterialAsset.FriendlyName;
            }
            else if (assetReference.isNull)
            {
                nameButton.Text = EditorTerrainTilesUI.localization.format("LayerNull");
            }
            else
            {
                nameButton.Text = assetReference.GUID.ToString("N");
            }
        }
        else
        {
            nameButton.Text = string.Empty;
        }
    }

    public TerrainTileLayer(EditorTerrainTilesUI owner, int layerIndex)
    {
        this.owner = owner;
        this.layerIndex = layerIndex;
        layerBox = Glazier.Get().CreateBox();
        layerBox.SizeOffset_X = 30f;
        layerBox.SizeScale_Y = 1f;
        layerBox.Text = layerIndex.ToString();
        AddChild(layerBox);
        nameButton = Glazier.Get().CreateButton();
        nameButton.PositionOffset_X = 30f;
        nameButton.SizeScale_X = 1f;
        nameButton.SizeScale_Y = 1f;
        nameButton.SizeOffset_X = -30f;
        nameButton.OnClicked += OnClicked;
        AddChild(nameButton);
        UpdateSelectedTile();
    }

    private void OnClicked(ISleekElement element)
    {
        owner.SetSelectedLayerIndex(layerIndex);
    }
}
