using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned;

internal class EditorTerrainTilesUI : SleekFullscreenBox
{
    internal static Local localization;

    private ISleekLabel hintLabel;

    private TerrainTileLayer[] layers;

    private SleekButtonIconConfirm resetHeightmapButton;

    private SleekButtonIconConfirm resetSplatmapButton;

    private SleekButtonIconConfirm applyToAllTilesButton;

    private ISleekButton repairEdgesButton;

    internal int selectedLayerIndex;

    private List<LandscapeMaterialAsset> searchAssets;

    private SleekBoxIcon selectedLayerBox;

    private ISleekField layerGuidField;

    private ISleekButton resetAssetButton;

    private ISleekScrollView assetScrollView;

    public void Open()
    {
        AnimateIntoView();
        TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.TILE;
        EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        TerrainEditor.selectedTile = null;
        EditorInteract.instance.SetActiveTool(null);
    }

    public override void OnDestroy()
    {
        TerrainEditor.selectedTileChanged -= OnSelectedTileChanged;
    }

    public EditorTerrainTilesUI()
    {
        localization = Localization.read("/Editor/EditorTerrainTiles.dat");
        TerrainEditor.selectedTileChanged += OnSelectedTileChanged;
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.PositionScale_Y = 1f;
        hintLabel.PositionOffset_Y = -30f;
        hintLabel.SizeScale_X = 1f;
        hintLabel.SizeOffset_Y = 30f;
        hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        hintLabel.Text = localization.format("Hint_Remove", "Delete");
        AddChild(hintLabel);
        float num = 0f;
        repairEdgesButton = Glazier.Get().CreateButton();
        repairEdgesButton.PositionScale_X = 1f;
        repairEdgesButton.PositionScale_Y = 1f;
        repairEdgesButton.PositionOffset_X = -560f;
        repairEdgesButton.SizeOffset_X = 250f;
        repairEdgesButton.SizeOffset_Y = 30f;
        num -= repairEdgesButton.SizeOffset_Y;
        repairEdgesButton.PositionOffset_Y = num;
        num -= 10f;
        repairEdgesButton.Text = localization.format("RepairEdges_Label");
        repairEdgesButton.OnClicked += OnClickedRepairEdges;
        repairEdgesButton.TooltipText = localization.format("RepairEdges_Tooltip");
        AddChild(repairEdgesButton);
        applyToAllTilesButton = new SleekButtonIconConfirm(null, localization.format("ApplyToAllTiles_Confirm_Label"), localization.format("ApplyToAllTiles_Confirm_Tooltip"), localization.format("ApplyToAllTiles_Deny_Label"), localization.format("ApplyToAllTiles_Deny_Tooltip"));
        applyToAllTilesButton.PositionScale_X = 1f;
        applyToAllTilesButton.PositionScale_Y = 1f;
        applyToAllTilesButton.PositionOffset_X = -560f;
        applyToAllTilesButton.SizeOffset_X = 250f;
        applyToAllTilesButton.SizeOffset_Y = 30f;
        num -= applyToAllTilesButton.SizeOffset_Y;
        applyToAllTilesButton.PositionOffset_Y = num;
        num -= 10f;
        applyToAllTilesButton.text = localization.format("ApplyToAllTiles_Label");
        applyToAllTilesButton.tooltip = localization.format("ApplyToAllTiles_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = applyToAllTilesButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(OnApplyToAllTiles));
        AddChild(applyToAllTilesButton);
        resetSplatmapButton = new SleekButtonIconConfirm(null, localization.format("ResetSplatmap_Confirm_Label"), localization.format("ResetSplatmap_Confirm_Tooltip"), localization.format("ResetSplatmap_Deny_Label"), localization.format("ResetSplatmap_Deny_Tooltip"));
        resetSplatmapButton.PositionScale_X = 1f;
        resetSplatmapButton.PositionScale_Y = 1f;
        resetSplatmapButton.PositionOffset_X = -560f;
        resetSplatmapButton.SizeOffset_X = 250f;
        resetSplatmapButton.SizeOffset_Y = 30f;
        num -= resetSplatmapButton.SizeOffset_Y;
        resetSplatmapButton.PositionOffset_Y = num;
        num -= 10f;
        resetSplatmapButton.text = localization.format("ResetSplatmap_Label");
        resetSplatmapButton.tooltip = localization.format("ResetSplatmap_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm2 = resetSplatmapButton;
        sleekButtonIconConfirm2.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm2.onConfirmed, new Confirm(OnResetSplatmap));
        AddChild(resetSplatmapButton);
        resetHeightmapButton = new SleekButtonIconConfirm(null, localization.format("ResetHeightmap_Confirm_Label"), localization.format("ResetHeightmap_Confirm_Tooltip"), localization.format("ResetHeightmap_Deny_Label"), localization.format("ResetHeightmap_Deny_Tooltip"));
        resetHeightmapButton.PositionScale_X = 1f;
        resetHeightmapButton.PositionScale_Y = 1f;
        resetHeightmapButton.PositionOffset_X = -560f;
        resetHeightmapButton.SizeOffset_X = 250f;
        resetHeightmapButton.SizeOffset_Y = 30f;
        num -= resetHeightmapButton.SizeOffset_Y;
        resetHeightmapButton.PositionOffset_Y = num;
        num -= 10f;
        resetHeightmapButton.text = localization.format("ResetHeightmap_Label");
        resetHeightmapButton.tooltip = localization.format("ResetHeightmap_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm3 = resetHeightmapButton;
        sleekButtonIconConfirm3.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm3.onConfirmed, new Confirm(OnResetHeightmap));
        AddChild(resetHeightmapButton);
        layers = new TerrainTileLayer[Landscape.SPLATMAP_LAYERS];
        for (int num2 = layers.Length - 1; num2 >= 0; num2--)
        {
            TerrainTileLayer terrainTileLayer = new TerrainTileLayer(this, num2);
            layers[num2] = terrainTileLayer;
            terrainTileLayer.PositionScale_X = 1f;
            terrainTileLayer.PositionScale_Y = 1f;
            terrainTileLayer.PositionOffset_X = -560f;
            terrainTileLayer.SizeOffset_X = 250f;
            terrainTileLayer.SizeOffset_Y = 30f;
            num = (terrainTileLayer.PositionOffset_Y = num - terrainTileLayer.SizeOffset_Y);
            AddChild(terrainTileLayer);
        }
        int num4 = 300;
        float num5 = 0f;
        selectedLayerBox = new SleekBoxIcon(null, 64);
        selectedLayerBox.SizeOffset_X = num4;
        selectedLayerBox.PositionOffset_X = 0f - selectedLayerBox.SizeOffset_X;
        selectedLayerBox.SizeOffset_Y = 74f;
        selectedLayerBox.PositionScale_X = 1f;
        selectedLayerBox.AddLabel(localization.format("SelectedLayer"), ESleekSide.LEFT);
        AddChild(selectedLayerBox);
        num5 += selectedLayerBox.SizeOffset_Y + 10f;
        layerGuidField = Glazier.Get().CreateStringField();
        layerGuidField.SizeOffset_X = num4;
        layerGuidField.PositionOffset_X = 0f - layerGuidField.SizeOffset_X;
        layerGuidField.PositionOffset_Y = num5;
        layerGuidField.SizeOffset_Y = 30f;
        layerGuidField.PositionScale_X = 1f;
        layerGuidField.MaxLength = 32;
        layerGuidField.AddLabel(localization.format("LayerGuid"), ESleekSide.LEFT);
        layerGuidField.OnTextSubmitted += OnLayerGuidEntered;
        AddChild(layerGuidField);
        num5 += layerGuidField.SizeOffset_Y + 10f;
        resetAssetButton = Glazier.Get().CreateButton();
        resetAssetButton.SizeOffset_X = num4;
        resetAssetButton.PositionOffset_X = 0f - resetAssetButton.SizeOffset_X;
        resetAssetButton.PositionOffset_Y = num5;
        resetAssetButton.SizeOffset_Y = 30f;
        resetAssetButton.PositionScale_X = 1f;
        resetAssetButton.Text = localization.format("ResetAsset");
        resetAssetButton.OnClicked += OnClickedResetAsset;
        AddChild(resetAssetButton);
        num5 += resetAssetButton.SizeOffset_Y + 10f;
        searchAssets = new List<LandscapeMaterialAsset>();
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.PositionOffset_Y = num5;
        assetScrollView.PositionScale_X = 1f;
        assetScrollView.SizeOffset_X = num4;
        assetScrollView.SizeOffset_Y = 0f - num5;
        assetScrollView.PositionOffset_X = 0f - assetScrollView.SizeOffset_X;
        assetScrollView.SizeScale_Y = 1f;
        assetScrollView.ScaleContentToWidth = true;
        AddChild(assetScrollView);
        RefreshAssets();
    }

    private void OnSelectedTileChanged(LandscapeTile oldSelectedTile, LandscapeTile newSelectedTile)
    {
        TerrainTileLayer[] array = layers;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].UpdateSelectedTile();
        }
        SetSelectedLayerIndex(-1);
    }

    public void SetSelectedLayerIndex(int layerIndex)
    {
        selectedLayerIndex = layerIndex;
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null && layerIndex >= 0)
        {
            AssetReference<LandscapeMaterialAsset> assetReference = selectedTile.materials[layerIndex];
            LandscapeMaterialAsset landscapeMaterialAsset = assetReference.Find();
            if (landscapeMaterialAsset != null)
            {
                selectedLayerBox.icon = Assets.load(landscapeMaterialAsset.texture);
                selectedLayerBox.text = landscapeMaterialAsset.FriendlyName;
            }
            else if (assetReference.isNull)
            {
                selectedLayerBox.icon = null;
                selectedLayerBox.text = localization.format("LayerNull");
            }
            else
            {
                selectedLayerBox.icon = null;
                selectedLayerBox.text = localization.format("LayerMissing");
            }
            layerGuidField.Text = assetReference.ToString();
        }
        else
        {
            selectedLayerBox.icon = null;
            selectedLayerBox.text = string.Empty;
            layerGuidField.Text = string.Empty;
        }
    }

    private void OnLayerGuidEntered(ISleekField field)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null && selectedLayerIndex >= 0)
        {
            AssetReference<LandscapeMaterialAsset>.TryParse(field.Text, out var result);
            selectedTile.materials[selectedLayerIndex] = result;
            selectedTile.updatePrototypes();
            layers[selectedLayerIndex].UpdateSelectedTile();
            SetSelectedLayerIndex(selectedLayerIndex);
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnClickedResetAsset(ISleekElement button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null && selectedLayerIndex >= 0)
        {
            selectedTile.materials[selectedLayerIndex] = AssetReference<LandscapeMaterialAsset>.invalid;
            selectedTile.updatePrototypes();
            layers[selectedLayerIndex].UpdateSelectedTile();
            SetSelectedLayerIndex(selectedLayerIndex);
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnAssetClicked(ISleekElement button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null && selectedLayerIndex >= 0)
        {
            int index = assetScrollView.FindIndexOfChild(button);
            selectedTile.materials[selectedLayerIndex] = searchAssets[index].getReferenceTo<LandscapeMaterialAsset>();
            selectedTile.updatePrototypes();
            layers[selectedLayerIndex].UpdateSelectedTile();
            SetSelectedLayerIndex(selectedLayerIndex);
            LevelHierarchy.MarkDirty();
        }
    }

    private void RefreshAssets()
    {
        searchAssets.Clear();
        assetScrollView.RemoveAllChildren();
        float num = 0f;
        Assets.find(searchAssets);
        searchAssets.Sort((LandscapeMaterialAsset lhs, LandscapeMaterialAsset rhs) => lhs.name.CompareTo(rhs.name));
        foreach (LandscapeMaterialAsset searchAsset in searchAssets)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(Assets.load(searchAsset.texture), 64);
            sleekButtonIcon.PositionOffset_Y = num;
            sleekButtonIcon.SizeScale_X = 1f;
            sleekButtonIcon.SizeOffset_Y = 74f;
            sleekButtonIcon.text = searchAsset.FriendlyName;
            sleekButtonIcon.onClickedButton += OnAssetClicked;
            assetScrollView.AddChild(sleekButtonIcon);
            num += sleekButtonIcon.SizeOffset_Y;
        }
        assetScrollView.ContentSizeOffset = new Vector2(0f, num);
    }

    private void OnResetHeightmap(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null)
        {
            selectedTile.resetHeightmap();
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnResetSplatmap(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null)
        {
            selectedTile.resetSplatmap();
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnApplyToAllTiles(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null)
        {
            Landscape.CopyLayersToAllTiles(selectedTile);
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnClickedRepairEdges(ISleekElement button)
    {
        LandscapeTile selectedTile = TerrainEditor.selectedTile;
        if (selectedTile != null)
        {
            Landscape.reconcileNeighbors(selectedTile);
            LevelHierarchy.MarkDirty();
        }
    }
}
