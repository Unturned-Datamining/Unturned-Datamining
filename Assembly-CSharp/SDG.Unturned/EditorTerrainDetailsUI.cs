using System;
using System.Collections.Generic;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class EditorTerrainDetailsUI : SleekFullscreenBox
{
    private Local localization;

    private FoliageEditor tool;

    private List<FoliageInfoAsset> searchInfoAssets;

    private List<FoliageInfoCollectionAsset> searchCollectionAssets;

    private ISleekBox selectedAssetBox;

    private SleekButtonState searchTypeButton;

    private ISleekField searchField;

    private ISleekScrollView assetScrollView;

    private SleekButtonState modeButton;

    private ISleekFloat32Field brushRadiusField;

    private ISleekFloat32Field brushFalloffField;

    private ISleekFloat32Field brushStrengthField;

    private ISleekFloat32Field densityTargetField;

    private ISleekUInt32Field surfaceMaskField;

    private ISleekUInt32Field maxPreviewSamplesField;

    private ISleekToggle bakeInstancedMeshesToggle;

    private ISleekToggle bakeResourcesToggle;

    private ISleekToggle bakeObjectsToggle;

    private ISleekToggle bakeClearToggle;

    private ISleekToggle bakeApplyScaleToggle;

    private ISleekButton bakeGlobalButton;

    private ISleekButton bakeLocalButton;

    private ISleekButton bakeCancelButton;

    private ISleekLabel bakeProgressLabel;

    private ISleekLabel hintLabel;

    public void Open()
    {
        AnimateIntoView();
        EditorInteract.instance.SetActiveTool(tool);
    }

    public void Close()
    {
        AnimateOutOfView(1f, 0f);
        DevkitFoliageToolOptions.save();
        EditorInteract.instance.SetActiveTool(null);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        modeButton.state = (int)tool.mode;
        brushRadiusField.Value = DevkitFoliageToolOptions.instance.brushRadius;
        brushFalloffField.Value = DevkitFoliageToolOptions.instance.brushFalloff;
        brushStrengthField.Value = DevkitFoliageToolOptions.instance.brushStrength;
        densityTargetField.Value = DevkitFoliageToolOptions.instance.densityTarget;
        int bakeQueueProgress = FoliageSystem.bakeQueueProgress;
        int bakeQueueTotal = FoliageSystem.bakeQueueTotal;
        if (bakeQueueProgress == bakeQueueTotal || bakeQueueTotal < 1)
        {
            bakeProgressLabel.IsVisible = false;
        }
        else
        {
            float num = (float)bakeQueueProgress / (float)bakeQueueTotal;
            bakeProgressLabel.IsVisible = true;
            bakeProgressLabel.Text = bakeQueueProgress + "/" + bakeQueueTotal + " [" + num.ToString("P") + "]";
        }
        if (tool.mode == FoliageEditor.EFoliageMode.PAINT)
        {
            hintLabel.Text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
            hintLabel.IsVisible = true;
        }
        else
        {
            hintLabel.IsVisible = false;
        }
        UpdateOffsets();
    }

    public EditorTerrainDetailsUI()
    {
        localization = Localization.read("/Editor/EditorTerrainDetails.dat");
        DevkitFoliageToolOptions.load();
        tool = new FoliageEditor();
        searchInfoAssets = new List<FoliageInfoAsset>();
        searchCollectionAssets = new List<FoliageInfoCollectionAsset>();
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.PositionScale_Y = 1f;
        maxPreviewSamplesField.SizeOffset_X = 200f;
        maxPreviewSamplesField.SizeOffset_Y = 30f;
        maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.Value = DevkitFoliageToolOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
        AddChild(maxPreviewSamplesField);
        surfaceMaskField = Glazier.Get().CreateUInt32Field();
        surfaceMaskField.PositionScale_Y = 1f;
        surfaceMaskField.SizeOffset_X = 200f;
        surfaceMaskField.SizeOffset_Y = 30f;
        surfaceMaskField.AddLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        surfaceMaskField.Value = (uint)DevkitFoliageToolOptions.instance.surfaceMask;
        surfaceMaskField.OnValueChanged += OnSurfaceMaskTyped;
        AddChild(surfaceMaskField);
        densityTargetField = Glazier.Get().CreateFloat32Field();
        densityTargetField.PositionScale_Y = 1f;
        densityTargetField.SizeOffset_X = 200f;
        densityTargetField.SizeOffset_Y = 30f;
        densityTargetField.AddLabel(localization.format("DensityTarget"), ESleekSide.RIGHT);
        densityTargetField.Value = DevkitFoliageToolOptions.instance.densityTarget;
        densityTargetField.OnValueChanged += OnDensityTargetTyped;
        AddChild(densityTargetField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.PositionScale_Y = 1f;
        brushStrengthField.SizeOffset_X = 200f;
        brushStrengthField.SizeOffset_Y = 30f;
        brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.Value = DevkitFoliageToolOptions.instance.brushStrength;
        brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.PositionScale_Y = 1f;
        brushFalloffField.SizeOffset_X = 200f;
        brushFalloffField.SizeOffset_Y = 30f;
        brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.Value = DevkitFoliageToolOptions.instance.brushFalloff;
        brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.PositionScale_Y = 1f;
        brushRadiusField.SizeOffset_X = 200f;
        brushRadiusField.SizeOffset_Y = 30f;
        brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.Value = DevkitFoliageToolOptions.instance.brushRadius;
        brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")), new GUIContent(localization.format("Mode_Exact", "W")), new GUIContent(localization.format("Mode_Bake", "E")));
        modeButton.PositionScale_Y = 1f;
        modeButton.SizeOffset_X = 200f;
        modeButton.SizeOffset_Y = 30f;
        modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)tool.mode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        float num = 0f;
        bakeCancelButton = Glazier.Get().CreateButton();
        bakeCancelButton.PositionScale_X = 1f;
        bakeCancelButton.PositionScale_Y = 1f;
        bakeCancelButton.SizeOffset_X = 200f;
        bakeCancelButton.PositionOffset_X = 0f - bakeCancelButton.SizeOffset_X;
        bakeCancelButton.SizeOffset_Y = 30f;
        num -= bakeCancelButton.SizeOffset_Y;
        bakeCancelButton.PositionOffset_Y = num;
        num -= 10f;
        bakeCancelButton.Text = localization.format("Bake_Cancel");
        bakeCancelButton.OnClicked += OnBakeCancelButtonClicked;
        AddChild(bakeCancelButton);
        bakeLocalButton = Glazier.Get().CreateButton();
        bakeLocalButton.PositionScale_X = 1f;
        bakeLocalButton.PositionScale_Y = 1f;
        bakeLocalButton.SizeOffset_X = 200f;
        bakeLocalButton.PositionOffset_X = 0f - bakeLocalButton.SizeOffset_X;
        bakeLocalButton.SizeOffset_Y = 30f;
        num -= bakeLocalButton.SizeOffset_Y;
        bakeLocalButton.PositionOffset_Y = num;
        num -= 10f;
        bakeLocalButton.Text = localization.format("Bake_Local");
        bakeLocalButton.OnClicked += OnBakeLocalButtonClicked;
        AddChild(bakeLocalButton);
        bakeGlobalButton = Glazier.Get().CreateButton();
        bakeGlobalButton.PositionScale_X = 1f;
        bakeGlobalButton.PositionScale_Y = 1f;
        bakeGlobalButton.SizeOffset_X = 200f;
        bakeGlobalButton.PositionOffset_X = 0f - bakeGlobalButton.SizeOffset_X;
        bakeGlobalButton.SizeOffset_Y = 30f;
        num -= bakeGlobalButton.SizeOffset_Y;
        bakeGlobalButton.PositionOffset_Y = num;
        num -= 10f;
        bakeGlobalButton.Text = localization.format("Bake_Global");
        bakeGlobalButton.OnClicked += OnBakeGlobalButtonClicked;
        AddChild(bakeGlobalButton);
        bakeClearToggle = Glazier.Get().CreateToggle();
        bakeClearToggle.PositionScale_X = 1f;
        bakeClearToggle.PositionScale_Y = 1f;
        bakeClearToggle.SizeOffset_X = 40f;
        bakeClearToggle.PositionOffset_X = -200f;
        bakeClearToggle.SizeOffset_Y = 40f;
        num -= bakeClearToggle.SizeOffset_Y;
        bakeClearToggle.PositionOffset_Y = num;
        num -= 10f;
        bakeClearToggle.AddLabel(localization.format("Bake_Clear"), ESleekSide.RIGHT);
        bakeClearToggle.Value = DevkitFoliageToolOptions.instance.bakeClear;
        bakeClearToggle.OnValueChanged += OnBakeClearClicked;
        AddChild(bakeClearToggle);
        bakeApplyScaleToggle = Glazier.Get().CreateToggle();
        bakeApplyScaleToggle.PositionScale_X = 1f;
        bakeApplyScaleToggle.PositionScale_Y = 1f;
        bakeApplyScaleToggle.SizeOffset_X = 40f;
        bakeApplyScaleToggle.PositionOffset_X = -200f;
        bakeApplyScaleToggle.SizeOffset_Y = 40f;
        num -= bakeApplyScaleToggle.SizeOffset_Y;
        bakeApplyScaleToggle.PositionOffset_Y = num;
        num -= 10f;
        bakeApplyScaleToggle.AddLabel(localization.format("Bake_ApplyScale"), ESleekSide.RIGHT);
        bakeApplyScaleToggle.Value = DevkitFoliageToolOptions.instance.bakeApplyScale;
        bakeApplyScaleToggle.OnValueChanged += OnBakeApplyScaleClicked;
        AddChild(bakeApplyScaleToggle);
        bakeObjectsToggle = Glazier.Get().CreateToggle();
        bakeObjectsToggle.PositionScale_X = 1f;
        bakeObjectsToggle.PositionScale_Y = 1f;
        bakeObjectsToggle.SizeOffset_X = 40f;
        bakeObjectsToggle.PositionOffset_X = -200f;
        bakeObjectsToggle.SizeOffset_Y = 40f;
        num -= bakeObjectsToggle.SizeOffset_Y;
        bakeObjectsToggle.PositionOffset_Y = num;
        num -= 10f;
        bakeObjectsToggle.AddLabel(localization.format("Bake_Objects"), ESleekSide.RIGHT);
        bakeObjectsToggle.Value = DevkitFoliageToolOptions.instance.bakeObjects;
        bakeObjectsToggle.OnValueChanged += OnBakeObjectsClicked;
        AddChild(bakeObjectsToggle);
        bakeResourcesToggle = Glazier.Get().CreateToggle();
        bakeResourcesToggle.PositionScale_X = 1f;
        bakeResourcesToggle.PositionScale_Y = 1f;
        bakeResourcesToggle.SizeOffset_X = 40f;
        bakeResourcesToggle.PositionOffset_X = -200f;
        bakeResourcesToggle.SizeOffset_Y = 40f;
        num -= bakeResourcesToggle.SizeOffset_Y;
        bakeResourcesToggle.PositionOffset_Y = num;
        num -= 10f;
        bakeResourcesToggle.AddLabel(localization.format("Bake_Resources"), ESleekSide.RIGHT);
        bakeResourcesToggle.Value = DevkitFoliageToolOptions.instance.bakeResources;
        bakeResourcesToggle.OnValueChanged += OnBakeResourcesClicked;
        AddChild(bakeResourcesToggle);
        bakeInstancedMeshesToggle = Glazier.Get().CreateToggle();
        bakeInstancedMeshesToggle.PositionScale_X = 1f;
        bakeInstancedMeshesToggle.PositionScale_Y = 1f;
        bakeInstancedMeshesToggle.SizeOffset_X = 40f;
        bakeInstancedMeshesToggle.PositionOffset_X = -200f;
        bakeInstancedMeshesToggle.SizeOffset_Y = 40f;
        num -= bakeInstancedMeshesToggle.SizeOffset_Y;
        bakeInstancedMeshesToggle.PositionOffset_Y = num;
        bakeInstancedMeshesToggle.AddLabel(localization.format("Bake_InstancedMeshes"), ESleekSide.RIGHT);
        bakeInstancedMeshesToggle.Value = DevkitFoliageToolOptions.instance.bakeInstancedMeshes;
        bakeInstancedMeshesToggle.OnValueChanged += OnBakeInstancedMeshesClicked;
        AddChild(bakeInstancedMeshesToggle);
        bakeProgressLabel = Glazier.Get().CreateLabel();
        bakeProgressLabel.PositionOffset_X = -100f;
        bakeProgressLabel.PositionScale_X = 0.5f;
        bakeProgressLabel.PositionScale_Y = 0.9f;
        bakeProgressLabel.SizeOffset_X = 200f;
        bakeProgressLabel.SizeOffset_Y = 30f;
        bakeProgressLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        bakeProgressLabel.IsVisible = false;
        AddChild(bakeProgressLabel);
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.PositionScale_Y = 1f;
        hintLabel.PositionOffset_Y = -30f;
        hintLabel.SizeScale_X = 1f;
        hintLabel.SizeOffset_Y = 30f;
        hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        hintLabel.IsVisible = false;
        AddChild(hintLabel);
        selectedAssetBox = Glazier.Get().CreateBox();
        selectedAssetBox.PositionScale_X = 1f;
        selectedAssetBox.SizeOffset_X = 200f;
        selectedAssetBox.PositionOffset_X = 0f - selectedAssetBox.SizeOffset_X;
        selectedAssetBox.SizeOffset_Y = 30f;
        selectedAssetBox.AddLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
        AddChild(selectedAssetBox);
        searchTypeButton = new SleekButtonState(new GUIContent(localization.format("SearchType_Assets")), new GUIContent(localization.format("SearchType_Collections")));
        searchTypeButton.PositionScale_X = 1f;
        searchTypeButton.SizeOffset_X = 200f;
        searchTypeButton.PositionOffset_X = 0f - searchTypeButton.SizeOffset_X;
        searchTypeButton.PositionOffset_Y = 40f;
        searchTypeButton.SizeOffset_Y = 30f;
        SleekButtonState sleekButtonState2 = searchTypeButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSearchType));
        searchTypeButton.AddLabel(localization.format("SearchType_Label"), ESleekSide.LEFT);
        AddChild(searchTypeButton);
        searchField = Glazier.Get().CreateStringField();
        searchField.PositionOffset_X = -200f;
        searchField.PositionOffset_Y = 80f;
        searchField.PositionScale_X = 1f;
        searchField.SizeOffset_X = 200f;
        searchField.SizeOffset_Y = 30f;
        searchField.PlaceholderText = localization.format("SearchHint");
        searchField.OnTextSubmitted += OnNameFilterEntered;
        AddChild(searchField);
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.PositionScale_X = 1f;
        assetScrollView.SizeOffset_X = 200f;
        assetScrollView.PositionOffset_X = 0f - assetScrollView.SizeOffset_X;
        assetScrollView.PositionOffset_Y = 120f;
        assetScrollView.SizeOffset_Y = -120f;
        assetScrollView.SizeScale_Y = 1f;
        assetScrollView.ScaleContentToWidth = true;
        AddChild(assetScrollView);
        RefreshAssets();
    }

    private void UpdateOffsets()
    {
        selectedAssetBox.IsVisible = tool.mode != FoliageEditor.EFoliageMode.BAKE;
        searchTypeButton.IsVisible = selectedAssetBox.IsVisible;
        searchField.IsVisible = selectedAssetBox.IsVisible;
        assetScrollView.IsVisible = selectedAssetBox.IsVisible;
        bakeInstancedMeshesToggle.IsVisible = tool.mode == FoliageEditor.EFoliageMode.BAKE;
        bakeResourcesToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeObjectsToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeApplyScaleToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeClearToggle.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeGlobalButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeCancelButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        bakeLocalButton.IsVisible = bakeInstancedMeshesToggle.IsVisible;
        float num = 0f;
        num -= modeButton.SizeOffset_Y;
        modeButton.PositionOffset_Y = num;
        num -= 10f;
        maxPreviewSamplesField.IsVisible = tool.mode == FoliageEditor.EFoliageMode.PAINT;
        if (maxPreviewSamplesField.IsVisible)
        {
            num -= maxPreviewSamplesField.SizeOffset_Y;
            maxPreviewSamplesField.PositionOffset_Y = num;
            num -= 10f;
        }
        surfaceMaskField.IsVisible = tool.mode != FoliageEditor.EFoliageMode.BAKE;
        if (surfaceMaskField.IsVisible)
        {
            num -= surfaceMaskField.SizeOffset_Y;
            surfaceMaskField.PositionOffset_Y = num;
            num -= 10f;
        }
        densityTargetField.IsVisible = tool.mode == FoliageEditor.EFoliageMode.PAINT;
        brushStrengthField.IsVisible = densityTargetField.IsVisible;
        brushFalloffField.IsVisible = densityTargetField.IsVisible;
        brushRadiusField.IsVisible = densityTargetField.IsVisible;
        if (densityTargetField.IsVisible)
        {
            num -= densityTargetField.SizeOffset_Y;
            densityTargetField.PositionOffset_Y = num;
            num -= 10f;
            num -= brushStrengthField.SizeOffset_Y;
            brushStrengthField.PositionOffset_Y = num;
            num -= 10f;
            num -= brushFalloffField.SizeOffset_Y;
            brushFalloffField.PositionOffset_Y = num;
            num -= 10f;
            num -= brushRadiusField.SizeOffset_Y;
            brushRadiusField.PositionOffset_Y = num;
        }
    }

    private void OnSwappedMode(SleekButtonState element, int index)
    {
        tool.mode = (FoliageEditor.EFoliageMode)index;
    }

    private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
    {
        DevkitFoliageToolOptions.instance.maxPreviewSamples = state;
    }

    private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
    {
        DevkitFoliageToolOptions.instance.surfaceMask = (ERayMask)state;
    }

    private void OnDensityTargetTyped(ISleekFloat32Field field, float state)
    {
        DevkitFoliageToolOptions.instance.densityTarget = state;
    }

    private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
    {
        DevkitFoliageToolOptions.instance.brushStrength = state;
    }

    private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
    {
        DevkitFoliageToolOptions.instance.brushFalloff = state;
    }

    private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
    {
        DevkitFoliageToolOptions.instance.brushRadius = state;
    }

    private void OnBakeInstancedMeshesClicked(ISleekToggle element, bool state)
    {
        DevkitFoliageToolOptions.instance.bakeInstancedMeshes = state;
    }

    private void OnBakeResourcesClicked(ISleekToggle element, bool state)
    {
        DevkitFoliageToolOptions.instance.bakeResources = state;
    }

    private void OnBakeObjectsClicked(ISleekToggle element, bool state)
    {
        DevkitFoliageToolOptions.instance.bakeObjects = state;
    }

    private void OnBakeClearClicked(ISleekToggle element, bool state)
    {
        DevkitFoliageToolOptions.instance.bakeClear = state;
    }

    private void OnBakeApplyScaleClicked(ISleekToggle element, bool state)
    {
        DevkitFoliageToolOptions.instance.bakeApplyScale = state;
    }

    private void RefreshAssets()
    {
        searchInfoAssets.Clear();
        searchCollectionAssets.Clear();
        assetScrollView.RemoveAllChildren();
        float num = 0f;
        if (searchTypeButton.state == 0)
        {
            Assets.find(searchInfoAssets);
            string searchText2 = searchField.Text;
            if (!string.IsNullOrEmpty(searchText2))
            {
                searchInfoAssets.RemoveSwap((FoliageInfoAsset asset) => asset.name.IndexOf(searchText2, StringComparison.CurrentCultureIgnoreCase) == -1);
            }
            searchInfoAssets.Sort((FoliageInfoAsset lhs, FoliageInfoAsset rhs) => lhs.name.CompareTo(rhs.name));
            foreach (FoliageInfoAsset searchInfoAsset in searchInfoAssets)
            {
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_Y = num;
                sleekButton.SizeScale_X = 1f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.Text = searchInfoAsset.name;
                sleekButton.OnClicked += OnInfoAssetClicked;
                assetScrollView.AddChild(sleekButton);
                num += sleekButton.SizeOffset_Y;
            }
        }
        else if (searchTypeButton.state == 1)
        {
            Assets.find(searchCollectionAssets);
            string searchText = searchField.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchCollectionAssets.RemoveSwap((FoliageInfoCollectionAsset asset) => asset.name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) == -1);
            }
            searchCollectionAssets.Sort((FoliageInfoCollectionAsset lhs, FoliageInfoCollectionAsset rhs) => lhs.name.CompareTo(rhs.name));
            foreach (FoliageInfoCollectionAsset searchCollectionAsset in searchCollectionAssets)
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.PositionOffset_Y = num;
                sleekButton2.SizeScale_X = 1f;
                sleekButton2.SizeOffset_Y = 30f;
                sleekButton2.Text = searchCollectionAsset.name;
                sleekButton2.OnClicked += OnCollectionAssetClicked;
                assetScrollView.AddChild(sleekButton2);
                num += sleekButton2.SizeOffset_Y;
            }
        }
        assetScrollView.ContentSizeOffset = new Vector2(0f, num);
    }

    private void OnSwappedSearchType(SleekButtonState element, int index)
    {
        RefreshAssets();
    }

    private void OnNameFilterEntered(ISleekField field)
    {
        RefreshAssets();
    }

    private void OnInfoAssetClicked(ISleekElement button)
    {
        int index = assetScrollView.FindIndexOfChild(button);
        tool.selectedInstanceAsset = searchInfoAssets[index];
        tool.selectedCollectionAsset = null;
        selectedAssetBox.Text = tool.selectedInstanceAsset?.name;
    }

    private void OnCollectionAssetClicked(ISleekElement button)
    {
        int index = assetScrollView.FindIndexOfChild(button);
        tool.selectedInstanceAsset = null;
        tool.selectedCollectionAsset = searchCollectionAssets[index];
        selectedAssetBox.Text = tool.selectedCollectionAsset?.name;
    }

    private FoliageBakeSettings getBakeSettings()
    {
        FoliageBakeSettings result = default(FoliageBakeSettings);
        result.bakeInstancesMeshes = DevkitFoliageToolOptions.instance.bakeInstancedMeshes;
        result.bakeResources = DevkitFoliageToolOptions.instance.bakeResources;
        result.bakeObjects = DevkitFoliageToolOptions.instance.bakeObjects;
        result.bakeClear = DevkitFoliageToolOptions.instance.bakeClear;
        result.bakeApplyScale = DevkitFoliageToolOptions.instance.bakeApplyScale;
        return result;
    }

    private void OnBakeGlobalButtonClicked(ISleekElement button)
    {
        FoliageSystem.bakeGlobal(getBakeSettings());
    }

    private void OnBakeLocalButtonClicked(ISleekElement button)
    {
        FoliageSystem.bakeLocal(getBakeSettings());
    }

    private void OnBakeCancelButtonClicked(ISleekElement button)
    {
        FoliageSystem.bakeCancel();
    }
}
