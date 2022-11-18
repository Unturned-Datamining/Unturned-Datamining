using System;
using System.Collections.Generic;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Foliage;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal class EditorTerrainDetailsUIV2 : SleekFullscreenBox
{
    private Local localization;

    private FoliageEditorV2 tool;

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
        brushRadiusField.state = DevkitFoliageToolOptions.instance.brushRadius;
        brushFalloffField.state = DevkitFoliageToolOptions.instance.brushFalloff;
        brushStrengthField.state = DevkitFoliageToolOptions.instance.brushStrength;
        densityTargetField.state = DevkitFoliageToolOptions.instance.densityTarget;
        int bakeQueueProgress = FoliageSystem.bakeQueueProgress;
        int bakeQueueTotal = FoliageSystem.bakeQueueTotal;
        if (bakeQueueProgress == bakeQueueTotal || bakeQueueTotal < 1)
        {
            bakeProgressLabel.isVisible = false;
        }
        else
        {
            float num = (float)bakeQueueProgress / (float)bakeQueueTotal;
            bakeProgressLabel.isVisible = true;
            bakeProgressLabel.text = bakeQueueProgress + "/" + bakeQueueTotal + " [" + num.ToString("P") + "]";
        }
        if (tool.mode == FoliageEditorV2.EFoliageMode.PAINT)
        {
            hintLabel.text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
            hintLabel.isVisible = true;
        }
        else
        {
            hintLabel.isVisible = false;
        }
        UpdateOffsets();
    }

    public EditorTerrainDetailsUIV2()
    {
        localization = Localization.read("/Editor/EditorTerrainDetails.dat");
        DevkitFoliageToolOptions.load();
        tool = new FoliageEditorV2();
        searchInfoAssets = new List<FoliageInfoAsset>();
        searchCollectionAssets = new List<FoliageInfoCollectionAsset>();
        maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
        maxPreviewSamplesField.positionScale_Y = 1f;
        maxPreviewSamplesField.sizeOffset_X = 200;
        maxPreviewSamplesField.sizeOffset_Y = 30;
        maxPreviewSamplesField.addLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
        maxPreviewSamplesField.state = DevkitFoliageToolOptions.instance.maxPreviewSamples;
        maxPreviewSamplesField.onTypedUInt32 += OnMaxPreviewSamplesTyped;
        AddChild(maxPreviewSamplesField);
        surfaceMaskField = Glazier.Get().CreateUInt32Field();
        surfaceMaskField.positionScale_Y = 1f;
        surfaceMaskField.sizeOffset_X = 200;
        surfaceMaskField.sizeOffset_Y = 30;
        surfaceMaskField.addLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
        surfaceMaskField.state = (uint)DevkitFoliageToolOptions.instance.surfaceMask;
        surfaceMaskField.onTypedUInt32 += OnSurfaceMaskTyped;
        AddChild(surfaceMaskField);
        densityTargetField = Glazier.Get().CreateFloat32Field();
        densityTargetField.positionScale_Y = 1f;
        densityTargetField.sizeOffset_X = 200;
        densityTargetField.sizeOffset_Y = 30;
        densityTargetField.addLabel(localization.format("DensityTarget"), ESleekSide.RIGHT);
        densityTargetField.state = DevkitFoliageToolOptions.instance.densityTarget;
        densityTargetField.onTypedSingle += OnDensityTargetTyped;
        AddChild(densityTargetField);
        brushStrengthField = Glazier.Get().CreateFloat32Field();
        brushStrengthField.positionScale_Y = 1f;
        brushStrengthField.sizeOffset_X = 200;
        brushStrengthField.sizeOffset_Y = 30;
        brushStrengthField.addLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
        brushStrengthField.state = DevkitFoliageToolOptions.instance.brushStrength;
        brushStrengthField.onTypedSingle += OnBrushStrengthTyped;
        AddChild(brushStrengthField);
        brushFalloffField = Glazier.Get().CreateFloat32Field();
        brushFalloffField.positionScale_Y = 1f;
        brushFalloffField.sizeOffset_X = 200;
        brushFalloffField.sizeOffset_Y = 30;
        brushFalloffField.addLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
        brushFalloffField.state = DevkitFoliageToolOptions.instance.brushFalloff;
        brushFalloffField.onTypedSingle += OnBrushFalloffTyped;
        AddChild(brushFalloffField);
        brushRadiusField = Glazier.Get().CreateFloat32Field();
        brushRadiusField.positionScale_Y = 1f;
        brushRadiusField.sizeOffset_X = 200;
        brushRadiusField.sizeOffset_Y = 30;
        brushRadiusField.addLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
        brushRadiusField.state = DevkitFoliageToolOptions.instance.brushRadius;
        brushRadiusField.onTypedSingle += OnBrushRadiusTyped;
        AddChild(brushRadiusField);
        modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")), new GUIContent(localization.format("Mode_Exact", "W")), new GUIContent(localization.format("Mode_Bake", "E")));
        modeButton.positionScale_Y = 1f;
        modeButton.sizeOffset_X = 200;
        modeButton.sizeOffset_Y = 30;
        modeButton.addLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
        modeButton.state = (int)tool.mode;
        SleekButtonState sleekButtonState = modeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
        AddChild(modeButton);
        int num = 0;
        bakeCancelButton = Glazier.Get().CreateButton();
        bakeCancelButton.positionScale_X = 1f;
        bakeCancelButton.positionScale_Y = 1f;
        bakeCancelButton.sizeOffset_X = 200;
        bakeCancelButton.positionOffset_X = -bakeCancelButton.sizeOffset_X;
        bakeCancelButton.sizeOffset_Y = 30;
        num -= bakeCancelButton.sizeOffset_Y;
        bakeCancelButton.positionOffset_Y = num;
        num -= 10;
        bakeCancelButton.text = localization.format("Bake_Cancel");
        bakeCancelButton.onClickedButton += OnBakeCancelButtonClicked;
        AddChild(bakeCancelButton);
        bakeLocalButton = Glazier.Get().CreateButton();
        bakeLocalButton.positionScale_X = 1f;
        bakeLocalButton.positionScale_Y = 1f;
        bakeLocalButton.sizeOffset_X = 200;
        bakeLocalButton.positionOffset_X = -bakeLocalButton.sizeOffset_X;
        bakeLocalButton.sizeOffset_Y = 30;
        num -= bakeLocalButton.sizeOffset_Y;
        bakeLocalButton.positionOffset_Y = num;
        num -= 10;
        bakeLocalButton.text = localization.format("Bake_Local");
        bakeLocalButton.onClickedButton += OnBakeLocalButtonClicked;
        AddChild(bakeLocalButton);
        bakeGlobalButton = Glazier.Get().CreateButton();
        bakeGlobalButton.positionScale_X = 1f;
        bakeGlobalButton.positionScale_Y = 1f;
        bakeGlobalButton.sizeOffset_X = 200;
        bakeGlobalButton.positionOffset_X = -bakeGlobalButton.sizeOffset_X;
        bakeGlobalButton.sizeOffset_Y = 30;
        num -= bakeGlobalButton.sizeOffset_Y;
        bakeGlobalButton.positionOffset_Y = num;
        num -= 10;
        bakeGlobalButton.text = localization.format("Bake_Global");
        bakeGlobalButton.onClickedButton += OnBakeGlobalButtonClicked;
        AddChild(bakeGlobalButton);
        bakeClearToggle = Glazier.Get().CreateToggle();
        bakeClearToggle.positionScale_X = 1f;
        bakeClearToggle.positionScale_Y = 1f;
        bakeClearToggle.sizeOffset_X = 40;
        bakeClearToggle.positionOffset_X = -200;
        bakeClearToggle.sizeOffset_Y = 40;
        num -= bakeClearToggle.sizeOffset_Y;
        bakeClearToggle.positionOffset_Y = num;
        num -= 10;
        bakeClearToggle.addLabel(localization.format("Bake_Clear"), ESleekSide.RIGHT);
        bakeClearToggle.state = DevkitFoliageToolOptions.instance.bakeClear;
        bakeClearToggle.onToggled += OnBakeClearClicked;
        AddChild(bakeClearToggle);
        bakeApplyScaleToggle = Glazier.Get().CreateToggle();
        bakeApplyScaleToggle.positionScale_X = 1f;
        bakeApplyScaleToggle.positionScale_Y = 1f;
        bakeApplyScaleToggle.sizeOffset_X = 40;
        bakeApplyScaleToggle.positionOffset_X = -200;
        bakeApplyScaleToggle.sizeOffset_Y = 40;
        num -= bakeApplyScaleToggle.sizeOffset_Y;
        bakeApplyScaleToggle.positionOffset_Y = num;
        num -= 10;
        bakeApplyScaleToggle.addLabel(localization.format("Bake_ApplyScale"), ESleekSide.RIGHT);
        bakeApplyScaleToggle.state = DevkitFoliageToolOptions.instance.bakeApplyScale;
        bakeApplyScaleToggle.onToggled += OnBakeApplyScaleClicked;
        AddChild(bakeApplyScaleToggle);
        bakeObjectsToggle = Glazier.Get().CreateToggle();
        bakeObjectsToggle.positionScale_X = 1f;
        bakeObjectsToggle.positionScale_Y = 1f;
        bakeObjectsToggle.sizeOffset_X = 40;
        bakeObjectsToggle.positionOffset_X = -200;
        bakeObjectsToggle.sizeOffset_Y = 40;
        num -= bakeObjectsToggle.sizeOffset_Y;
        bakeObjectsToggle.positionOffset_Y = num;
        num -= 10;
        bakeObjectsToggle.addLabel(localization.format("Bake_Objects"), ESleekSide.RIGHT);
        bakeObjectsToggle.state = DevkitFoliageToolOptions.instance.bakeObjects;
        bakeObjectsToggle.onToggled += OnBakeObjectsClicked;
        AddChild(bakeObjectsToggle);
        bakeResourcesToggle = Glazier.Get().CreateToggle();
        bakeResourcesToggle.positionScale_X = 1f;
        bakeResourcesToggle.positionScale_Y = 1f;
        bakeResourcesToggle.sizeOffset_X = 40;
        bakeResourcesToggle.positionOffset_X = -200;
        bakeResourcesToggle.sizeOffset_Y = 40;
        num -= bakeResourcesToggle.sizeOffset_Y;
        bakeResourcesToggle.positionOffset_Y = num;
        num -= 10;
        bakeResourcesToggle.addLabel(localization.format("Bake_Resources"), ESleekSide.RIGHT);
        bakeResourcesToggle.state = DevkitFoliageToolOptions.instance.bakeResources;
        bakeResourcesToggle.onToggled += OnBakeResourcesClicked;
        AddChild(bakeResourcesToggle);
        bakeInstancedMeshesToggle = Glazier.Get().CreateToggle();
        bakeInstancedMeshesToggle.positionScale_X = 1f;
        bakeInstancedMeshesToggle.positionScale_Y = 1f;
        bakeInstancedMeshesToggle.sizeOffset_X = 40;
        bakeInstancedMeshesToggle.positionOffset_X = -200;
        bakeInstancedMeshesToggle.sizeOffset_Y = 40;
        num -= bakeInstancedMeshesToggle.sizeOffset_Y;
        bakeInstancedMeshesToggle.positionOffset_Y = num;
        bakeInstancedMeshesToggle.addLabel(localization.format("Bake_InstancedMeshes"), ESleekSide.RIGHT);
        bakeInstancedMeshesToggle.state = DevkitFoliageToolOptions.instance.bakeInstancedMeshes;
        bakeInstancedMeshesToggle.onToggled += OnBakeInstancedMeshesClicked;
        AddChild(bakeInstancedMeshesToggle);
        bakeProgressLabel = Glazier.Get().CreateLabel();
        bakeProgressLabel.positionOffset_X = -100;
        bakeProgressLabel.positionScale_X = 0.5f;
        bakeProgressLabel.positionScale_Y = 0.9f;
        bakeProgressLabel.sizeOffset_X = 200;
        bakeProgressLabel.sizeOffset_Y = 30;
        bakeProgressLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        bakeProgressLabel.isVisible = false;
        AddChild(bakeProgressLabel);
        hintLabel = Glazier.Get().CreateLabel();
        hintLabel.positionScale_Y = 1f;
        hintLabel.positionOffset_Y = -30;
        hintLabel.sizeScale_X = 1f;
        hintLabel.sizeOffset_Y = 30;
        hintLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        hintLabel.isVisible = false;
        AddChild(hintLabel);
        selectedAssetBox = Glazier.Get().CreateBox();
        selectedAssetBox.positionScale_X = 1f;
        selectedAssetBox.sizeOffset_X = 200;
        selectedAssetBox.positionOffset_X = -selectedAssetBox.sizeOffset_X;
        selectedAssetBox.sizeOffset_Y = 30;
        selectedAssetBox.addLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
        AddChild(selectedAssetBox);
        searchTypeButton = new SleekButtonState(new GUIContent(localization.format("SearchType_Assets")), new GUIContent(localization.format("SearchType_Collections")));
        searchTypeButton.positionScale_X = 1f;
        searchTypeButton.sizeOffset_X = 200;
        searchTypeButton.positionOffset_X = -searchTypeButton.sizeOffset_X;
        searchTypeButton.positionOffset_Y = 40;
        searchTypeButton.sizeOffset_Y = 30;
        SleekButtonState sleekButtonState2 = searchTypeButton;
        sleekButtonState2.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState2.onSwappedState, new SwappedState(OnSwappedSearchType));
        searchTypeButton.addLabel(localization.format("SearchType_Label"), ESleekSide.LEFT);
        AddChild(searchTypeButton);
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_X = -200;
        searchField.positionOffset_Y = 80;
        searchField.positionScale_X = 1f;
        searchField.sizeOffset_X = 200;
        searchField.sizeOffset_Y = 30;
        searchField.hint = localization.format("SearchHint");
        searchField.onEntered += OnNameFilterEntered;
        AddChild(searchField);
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.positionScale_X = 1f;
        assetScrollView.sizeOffset_X = 200;
        assetScrollView.positionOffset_X = -assetScrollView.sizeOffset_X;
        assetScrollView.positionOffset_Y = 120;
        assetScrollView.sizeOffset_Y = -120;
        assetScrollView.sizeScale_Y = 1f;
        assetScrollView.scaleContentToWidth = true;
        AddChild(assetScrollView);
        RefreshAssets();
    }

    private void UpdateOffsets()
    {
        selectedAssetBox.isVisible = tool.mode != FoliageEditorV2.EFoliageMode.BAKE;
        searchTypeButton.isVisible = selectedAssetBox.isVisible;
        searchField.isVisible = selectedAssetBox.isVisible;
        assetScrollView.isVisible = selectedAssetBox.isVisible;
        bakeInstancedMeshesToggle.isVisible = tool.mode == FoliageEditorV2.EFoliageMode.BAKE;
        bakeResourcesToggle.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeObjectsToggle.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeApplyScaleToggle.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeClearToggle.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeGlobalButton.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeCancelButton.isVisible = bakeInstancedMeshesToggle.isVisible;
        bakeLocalButton.isVisible = bakeInstancedMeshesToggle.isVisible;
        int num = 0;
        num -= modeButton.sizeOffset_Y;
        modeButton.positionOffset_Y = num;
        num -= 10;
        maxPreviewSamplesField.isVisible = tool.mode == FoliageEditorV2.EFoliageMode.PAINT;
        if (maxPreviewSamplesField.isVisible)
        {
            num -= maxPreviewSamplesField.sizeOffset_Y;
            maxPreviewSamplesField.positionOffset_Y = num;
            num -= 10;
        }
        surfaceMaskField.isVisible = tool.mode != FoliageEditorV2.EFoliageMode.BAKE;
        if (surfaceMaskField.isVisible)
        {
            num -= surfaceMaskField.sizeOffset_Y;
            surfaceMaskField.positionOffset_Y = num;
            num -= 10;
        }
        densityTargetField.isVisible = tool.mode == FoliageEditorV2.EFoliageMode.PAINT;
        brushStrengthField.isVisible = densityTargetField.isVisible;
        brushFalloffField.isVisible = densityTargetField.isVisible;
        brushRadiusField.isVisible = densityTargetField.isVisible;
        if (densityTargetField.isVisible)
        {
            num -= densityTargetField.sizeOffset_Y;
            densityTargetField.positionOffset_Y = num;
            num -= 10;
            num -= brushStrengthField.sizeOffset_Y;
            brushStrengthField.positionOffset_Y = num;
            num -= 10;
            num -= brushFalloffField.sizeOffset_Y;
            brushFalloffField.positionOffset_Y = num;
            num -= 10;
            num -= brushRadiusField.sizeOffset_Y;
            brushRadiusField.positionOffset_Y = num;
        }
    }

    private void OnSwappedMode(SleekButtonState element, int index)
    {
        tool.mode = (FoliageEditorV2.EFoliageMode)index;
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
        int num = 0;
        if (searchTypeButton.state == 0)
        {
            Assets.find(searchInfoAssets);
            string searchText2 = searchField.text;
            if (!string.IsNullOrEmpty(searchText2))
            {
                searchInfoAssets.RemoveSwap((FoliageInfoAsset asset) => asset.name.IndexOf(searchText2, StringComparison.CurrentCultureIgnoreCase) == -1);
            }
            searchInfoAssets.Sort((FoliageInfoAsset lhs, FoliageInfoAsset rhs) => lhs.name.CompareTo(rhs.name));
            foreach (FoliageInfoAsset searchInfoAsset in searchInfoAssets)
            {
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_Y = num;
                sleekButton.sizeScale_X = 1f;
                sleekButton.sizeOffset_Y = 30;
                sleekButton.text = searchInfoAsset.name;
                sleekButton.onClickedButton += OnInfoAssetClicked;
                assetScrollView.AddChild(sleekButton);
                num += sleekButton.sizeOffset_Y;
            }
        }
        else if (searchTypeButton.state == 1)
        {
            Assets.find(searchCollectionAssets);
            string searchText = searchField.text;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchCollectionAssets.RemoveSwap((FoliageInfoCollectionAsset asset) => asset.name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) == -1);
            }
            searchCollectionAssets.Sort((FoliageInfoCollectionAsset lhs, FoliageInfoCollectionAsset rhs) => lhs.name.CompareTo(rhs.name));
            foreach (FoliageInfoCollectionAsset searchCollectionAsset in searchCollectionAssets)
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.positionOffset_Y = num;
                sleekButton2.sizeScale_X = 1f;
                sleekButton2.sizeOffset_Y = 30;
                sleekButton2.text = searchCollectionAsset.name;
                sleekButton2.onClickedButton += OnCollectionAssetClicked;
                assetScrollView.AddChild(sleekButton2);
                num += sleekButton2.sizeOffset_Y;
            }
        }
        assetScrollView.contentSizeOffset = new Vector2(0f, num);
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
        selectedAssetBox.text = tool.selectedInstanceAsset?.name;
    }

    private void OnCollectionAssetClicked(ISleekElement button)
    {
        int index = assetScrollView.FindIndexOfChild(button);
        tool.selectedInstanceAsset = null;
        tool.selectedCollectionAsset = searchCollectionAssets[index];
        selectedAssetBox.text = tool.selectedCollectionAsset?.name;
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
