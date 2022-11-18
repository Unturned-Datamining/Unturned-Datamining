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
        TerrainEditorV2.toolMode = TerrainEditorV2.EDevkitLandscapeToolMode.TILE;
        EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        TerrainEditorV2.selectedTile = null;
        EditorInteract.instance.SetActiveTool(null);
    }

    public override void OnDestroy()
    {
        TerrainEditorV2.selectedTileChanged -= OnSelectedTileChanged;
    }

    public EditorTerrainTilesUI()
    {
        localization = Localization.read("/Editor/EditorTerrainTiles.dat");
        TerrainEditorV2.selectedTileChanged += OnSelectedTileChanged;
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.positionScale_Y = 1f;
        hintLabel.positionOffset_Y = -30;
        hintLabel.sizeScale_X = 1f;
        hintLabel.sizeOffset_Y = 30;
        hintLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        hintLabel.text = localization.format("Hint_Remove", "Delete");
        AddChild(hintLabel);
        int num = 0;
        repairEdgesButton = Glazier.Get().CreateButton();
        repairEdgesButton.positionScale_X = 1f;
        repairEdgesButton.positionScale_Y = 1f;
        repairEdgesButton.positionOffset_X = -560;
        repairEdgesButton.sizeOffset_X = 250;
        repairEdgesButton.sizeOffset_Y = 30;
        num -= repairEdgesButton.sizeOffset_Y;
        repairEdgesButton.positionOffset_Y = num;
        num -= 10;
        repairEdgesButton.text = localization.format("RepairEdges_Label");
        repairEdgesButton.onClickedButton += OnClickedRepairEdges;
        repairEdgesButton.tooltipText = localization.format("RepairEdges_Tooltip");
        AddChild(repairEdgesButton);
        applyToAllTilesButton = new SleekButtonIconConfirm(null, localization.format("ApplyToAllTiles_Confirm_Label"), localization.format("ApplyToAllTiles_Confirm_Tooltip"), localization.format("ApplyToAllTiles_Deny_Label"), localization.format("ApplyToAllTiles_Deny_Tooltip"));
        applyToAllTilesButton.positionScale_X = 1f;
        applyToAllTilesButton.positionScale_Y = 1f;
        applyToAllTilesButton.positionOffset_X = -560;
        applyToAllTilesButton.sizeOffset_X = 250;
        applyToAllTilesButton.sizeOffset_Y = 30;
        num -= applyToAllTilesButton.sizeOffset_Y;
        applyToAllTilesButton.positionOffset_Y = num;
        num -= 10;
        applyToAllTilesButton.text = localization.format("ApplyToAllTiles_Label");
        applyToAllTilesButton.tooltip = localization.format("ApplyToAllTiles_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = applyToAllTilesButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(OnApplyToAllTiles));
        AddChild(applyToAllTilesButton);
        resetSplatmapButton = new SleekButtonIconConfirm(null, localization.format("ResetSplatmap_Confirm_Label"), localization.format("ResetSplatmap_Confirm_Tooltip"), localization.format("ResetSplatmap_Deny_Label"), localization.format("ResetSplatmap_Deny_Tooltip"));
        resetSplatmapButton.positionScale_X = 1f;
        resetSplatmapButton.positionScale_Y = 1f;
        resetSplatmapButton.positionOffset_X = -560;
        resetSplatmapButton.sizeOffset_X = 250;
        resetSplatmapButton.sizeOffset_Y = 30;
        num -= resetSplatmapButton.sizeOffset_Y;
        resetSplatmapButton.positionOffset_Y = num;
        num -= 10;
        resetSplatmapButton.text = localization.format("ResetSplatmap_Label");
        resetSplatmapButton.tooltip = localization.format("ResetSplatmap_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm2 = resetSplatmapButton;
        sleekButtonIconConfirm2.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm2.onConfirmed, new Confirm(OnResetSplatmap));
        AddChild(resetSplatmapButton);
        resetHeightmapButton = new SleekButtonIconConfirm(null, localization.format("ResetHeightmap_Confirm_Label"), localization.format("ResetHeightmap_Confirm_Tooltip"), localization.format("ResetHeightmap_Deny_Label"), localization.format("ResetHeightmap_Deny_Tooltip"));
        resetHeightmapButton.positionScale_X = 1f;
        resetHeightmapButton.positionScale_Y = 1f;
        resetHeightmapButton.positionOffset_X = -560;
        resetHeightmapButton.sizeOffset_X = 250;
        resetHeightmapButton.sizeOffset_Y = 30;
        num -= resetHeightmapButton.sizeOffset_Y;
        resetHeightmapButton.positionOffset_Y = num;
        num -= 10;
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
            terrainTileLayer.positionScale_X = 1f;
            terrainTileLayer.positionScale_Y = 1f;
            terrainTileLayer.positionOffset_X = -560;
            terrainTileLayer.sizeOffset_X = 250;
            terrainTileLayer.sizeOffset_Y = 30;
            num = (terrainTileLayer.positionOffset_Y = num - terrainTileLayer.sizeOffset_Y);
            AddChild(terrainTileLayer);
        }
        int num4 = 300;
        int num5 = 0;
        selectedLayerBox = new SleekBoxIcon(null, 64);
        selectedLayerBox.sizeOffset_X = num4;
        selectedLayerBox.positionOffset_X = -selectedLayerBox.sizeOffset_X;
        selectedLayerBox.sizeOffset_Y = 74;
        selectedLayerBox.positionScale_X = 1f;
        selectedLayerBox.addLabel(localization.format("SelectedLayer"), ESleekSide.LEFT);
        AddChild(selectedLayerBox);
        num5 += selectedLayerBox.sizeOffset_Y + 10;
        layerGuidField = Glazier.Get().CreateStringField();
        layerGuidField.sizeOffset_X = num4;
        layerGuidField.positionOffset_X = -layerGuidField.sizeOffset_X;
        layerGuidField.positionOffset_Y = num5;
        layerGuidField.sizeOffset_Y = 30;
        layerGuidField.positionScale_X = 1f;
        layerGuidField.maxLength = 32;
        layerGuidField.addLabel(localization.format("LayerGuid"), ESleekSide.LEFT);
        layerGuidField.onEntered += OnLayerGuidEntered;
        AddChild(layerGuidField);
        num5 += layerGuidField.sizeOffset_Y + 10;
        resetAssetButton = Glazier.Get().CreateButton();
        resetAssetButton.sizeOffset_X = num4;
        resetAssetButton.positionOffset_X = -resetAssetButton.sizeOffset_X;
        resetAssetButton.positionOffset_Y = num5;
        resetAssetButton.sizeOffset_Y = 30;
        resetAssetButton.positionScale_X = 1f;
        resetAssetButton.text = localization.format("ResetAsset");
        resetAssetButton.onClickedButton += OnClickedResetAsset;
        AddChild(resetAssetButton);
        num5 += resetAssetButton.sizeOffset_Y + 10;
        searchAssets = new List<LandscapeMaterialAsset>();
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.positionOffset_Y = num5;
        assetScrollView.positionScale_X = 1f;
        assetScrollView.sizeOffset_X = num4;
        assetScrollView.sizeOffset_Y = -num5;
        assetScrollView.positionOffset_X = -assetScrollView.sizeOffset_X;
        assetScrollView.sizeScale_Y = 1f;
        assetScrollView.scaleContentToWidth = true;
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
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
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
            layerGuidField.text = assetReference.ToString();
        }
        else
        {
            selectedLayerBox.icon = null;
            selectedLayerBox.text = string.Empty;
            layerGuidField.text = string.Empty;
        }
    }

    private void OnLayerGuidEntered(ISleekField field)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null && selectedLayerIndex >= 0)
        {
            AssetReference<LandscapeMaterialAsset>.TryParse(field.text, out var result);
            selectedTile.materials[selectedLayerIndex] = result;
            selectedTile.updatePrototypes();
            layers[selectedLayerIndex].UpdateSelectedTile();
            SetSelectedLayerIndex(selectedLayerIndex);
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnClickedResetAsset(ISleekElement button)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
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
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
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
        int num = 0;
        Assets.find(searchAssets);
        searchAssets.Sort((LandscapeMaterialAsset lhs, LandscapeMaterialAsset rhs) => lhs.name.CompareTo(rhs.name));
        foreach (LandscapeMaterialAsset searchAsset in searchAssets)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(Assets.load(searchAsset.texture), 64);
            sleekButtonIcon.positionOffset_Y = num;
            sleekButtonIcon.sizeScale_X = 1f;
            sleekButtonIcon.sizeOffset_Y = 74;
            sleekButtonIcon.text = searchAsset.FriendlyName;
            sleekButtonIcon.onClickedButton += OnAssetClicked;
            assetScrollView.AddChild(sleekButtonIcon);
            num += sleekButtonIcon.sizeOffset_Y;
        }
        assetScrollView.contentSizeOffset = new Vector2(0f, num);
    }

    private void OnResetHeightmap(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null)
        {
            selectedTile.resetHeightmap();
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnResetSplatmap(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null)
        {
            selectedTile.resetSplatmap();
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnApplyToAllTiles(SleekButtonIconConfirm button)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null)
        {
            Landscape.CopyLayersToAllTiles(selectedTile);
            LevelHierarchy.MarkDirty();
        }
    }

    private void OnClickedRepairEdges(ISleekElement button)
    {
        LandscapeTile selectedTile = TerrainEditorV2.selectedTile;
        if (selectedTile != null)
        {
            Landscape.reconcileNeighbors(selectedTile);
            LevelHierarchy.MarkDirty();
        }
    }
}
