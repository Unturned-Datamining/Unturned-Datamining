using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class EditorEnvironmentRoadsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    /// <summary>
    /// Switches between "legacy" (per-level road textures bundle) and "assets" (using RoadAsset).
    /// </summary>
    private static SleekButtonState listModeButton;

    private static List<RoadAsset> searchAssets;

    private static ISleekElement legacyRoadMaterialsContainer;

    private static ISleekScrollView legacyScrollBox;

    private static ISleekBox legacySelectedBox;

    private static ISleekFloat32Field legacyWidthField;

    private static ISleekFloat32Field legacyHeightField;

    private static ISleekFloat32Field legacyDepthField;

    private static ISleekFloat32Field legacyOffset2Field;

    private static ISleekToggle legacyConcreteToggle;

    private static ISleekElement assetContainer;

    private static ISleekScrollView assetScrollView;

    private static SleekBoxIcon selectedAssetBox;

    private static ISleekToggle onlyUsedAssetsToggle;

    private static ISleekField searchField;

    private static SleekButtonIcon bakeRoadsButton;

    private static ISleekFloat32Field offsetField;

    private static ISleekToggle loopToggle;

    private static ISleekToggle ignoreTerrainToggle;

    private static SleekButtonState modeButton;

    private static ISleekBox roadIndexBox;

    private static SleekAssetField roadAssetField;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorRoads.isPaving = true;
            EditorUI.message(EEditorMessage.ROADS);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorRoads.isPaving = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    public static void updateSelection(Road road, RoadJoint joint)
    {
        if (road != null && joint != null)
        {
            offsetField.Value = joint.offset;
            loopToggle.Value = road.isLoop;
            ignoreTerrainToggle.Value = joint.ignoreTerrain;
            modeButton.state = (int)joint.mode;
            roadIndexBox.Text = LevelRoads.getRoadIndex(road).ToString();
            roadAssetField.Value = road.RoadAssetRef;
            EditorRoads.selected = road.material;
            EditorRoads.selectedAssetRef = road.RoadAssetRef;
            RefreshAssetSelection();
            RefreshLegacySelection();
            int num = (EditorRoads.selectedAssetRef.IsAssigned ? 1 : 0);
            if (num != listModeButton.state)
            {
                listModeButton.state = num;
                RefreshListMode();
            }
        }
        offsetField.IsVisible = road != null;
        loopToggle.IsVisible = road != null;
        ignoreTerrainToggle.IsVisible = road != null;
        modeButton.IsVisible = road != null;
        roadIndexBox.IsVisible = road != null;
        roadAssetField.IsVisible = road != null;
    }

    private static void RefreshLegacySelection()
    {
        if (EditorRoads.selected < LevelRoads.materials.Length)
        {
            RoadMaterial roadMaterial = LevelRoads.materials[EditorRoads.selected];
            legacySelectedBox.Text = roadMaterial.material.mainTexture.name;
            legacyWidthField.Value = roadMaterial.width;
            legacyHeightField.Value = roadMaterial.height;
            legacyDepthField.Value = roadMaterial.depth;
            legacyOffset2Field.Value = roadMaterial.offset;
            legacyConcreteToggle.Value = roadMaterial.isConcrete;
        }
    }

    private static void RefreshAssetSelection()
    {
        RoadAsset roadAsset = EditorRoads.selectedAssetRef.Get<RoadAsset>();
        if (roadAsset != null)
        {
            selectedAssetBox.icon = roadAsset.RoadTexture;
            selectedAssetBox.text = roadAsset.FriendlyName;
        }
        else
        {
            selectedAssetBox.icon = null;
            selectedAssetBox.text = string.Empty;
        }
    }

    private static void OnLegacyRoadMaterialClicked(ISleekElement button)
    {
        EditorRoads.selected = (byte)(button.Parent.PositionOffset_Y / 70f);
        RefreshLegacySelection();
        if (EditorRoads.road != null)
        {
            EditorRoads.road.material = EditorRoads.selected;
            EditorRoads.road.RoadAssetRef = null;
            roadAssetField.Value = null;
        }
    }

    private static void OnLegacyWidthChanged(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].width = state;
    }

    private static void OnLegacyHeightChanged(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].height = state;
    }

    private static void OnLegacyDepthChanged(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].depth = state;
    }

    private static void OnLegacyMaterialOffsetChanged(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].offset = state;
    }

    private static void OnLegacyConcreteToggled(ISleekToggle toggle, bool state)
    {
        LevelRoads.materials[EditorRoads.selected].isConcrete = state;
    }

    private static void OnAssetClicked(ISleekElement button)
    {
        int num = assetScrollView.FindIndexOfChild(button);
        EditorRoads.selectedAssetRef = ((num >= 0 && num < searchAssets.Count) ? searchAssets[num] : null);
        RefreshAssetSelection();
        if (EditorRoads.road != null)
        {
            EditorRoads.road.RoadAssetRef = EditorRoads.selectedAssetRef;
            roadAssetField.Value = EditorRoads.selectedAssetRef;
        }
    }

    private static void OnOnlyUsedAssetsToggled(ISleekToggle toggle, bool state)
    {
        RefreshAssets();
    }

    private static void OnNameFilterSubmitted(ISleekField field)
    {
        RefreshAssets();
    }

    private static void OnListModeChanged(SleekButtonState button, int state)
    {
        RefreshListMode();
    }

    private static void onClickedBakeRoadsButton(ISleekElement button)
    {
        LevelRoads.bakeRoads();
    }

    private static void onTypedOffsetField(ISleekFloat32Field field, float state)
    {
        EditorRoads.joint.offset = state;
        EditorRoads.road.updatePoints();
    }

    private static void OnRoadAssetChanged(SleekAssetField field)
    {
        if (EditorRoads.road != null)
        {
            EditorRoads.road.RoadAssetRef = field.Value;
        }
    }

    private static void onToggledLoopToggle(ISleekToggle toggle, bool state)
    {
        EditorRoads.road.isLoop = state;
    }

    private static void onToggledIgnoreTerrainToggle(ISleekToggle toggle, bool state)
    {
        EditorRoads.joint.ignoreTerrain = state;
        EditorRoads.road.updatePoints();
    }

    private static void onSwappedStateMode(SleekButtonState button, int index)
    {
        EditorRoads.joint.mode = (ERoadMode)index;
    }

    private static void RefreshListMode()
    {
        bool flag = listModeButton.state == 1;
        legacyRoadMaterialsContainer.IsVisible = !flag;
        assetContainer.IsVisible = flag;
        listModeButton.SizeOffset_X = (flag ? 300 : 200);
        listModeButton.PositionOffset_X = 0f - listModeButton.SizeOffset_X;
        bakeRoadsButton.SizeOffset_X = listModeButton.SizeOffset_X;
        bakeRoadsButton.PositionOffset_X = listModeButton.PositionOffset_X;
        if (flag)
        {
            RefreshAssets();
        }
    }

    private static void RefreshAssets()
    {
        searchAssets.Clear();
        if (onlyUsedAssetsToggle.Value)
        {
            LevelRoads.GatherUniqueAssets(searchAssets);
        }
        else
        {
            Assets.find(searchAssets);
        }
        string searchText = searchField.Text;
        if (!string.IsNullOrEmpty(searchText))
        {
            searchAssets.RemoveSwap((RoadAsset asset) => asset.FriendlyName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) == -1);
        }
        searchAssets.Sort((RoadAsset lhs, RoadAsset rhs) => lhs.FriendlyName.CompareTo(rhs.FriendlyName));
        assetScrollView.RemoveAllChildren();
        float num = 0f;
        foreach (RoadAsset searchAsset in searchAssets)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(searchAsset.RoadTexture, 64);
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

    public EditorEnvironmentRoadsUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironmentRoads.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorEnvironmentRoads/EditorEnvironmentRoads.unity3d");
        searchAssets = new List<RoadAsset>();
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
        listModeButton = new SleekButtonState(new GUIContent(local.format("ListMode_Legacy_Label"), local.format("ListMode_Legacy_Tooltip")), new GUIContent(local.format("ListMode_RoadAssets_Label"), local.format("ListMode_RoadAssets_Tooltip")));
        listModeButton.PositionOffset_X = -200f;
        listModeButton.PositionOffset_Y = 80f;
        listModeButton.PositionScale_X = 1f;
        listModeButton.SizeOffset_X = 200f;
        listModeButton.SizeOffset_Y = 30f;
        listModeButton.UseContentTooltip = true;
        listModeButton.AddLabel(local.format("ListMode_Label"), ESleekSide.LEFT);
        SleekButtonState sleekButtonState = listModeButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnListModeChanged));
        container.AddChild(listModeButton);
        legacyRoadMaterialsContainer = Glazier.Get().CreateFrame();
        legacyRoadMaterialsContainer.PositionOffset_X = -200f;
        legacyRoadMaterialsContainer.PositionOffset_Y = 120f;
        legacyRoadMaterialsContainer.PositionScale_X = 1f;
        legacyRoadMaterialsContainer.SizeOffset_X = 200f;
        legacyRoadMaterialsContainer.SizeOffset_Y = -160f;
        legacyRoadMaterialsContainer.SizeScale_Y = 1f;
        container.AddChild(legacyRoadMaterialsContainer);
        legacySelectedBox = Glazier.Get().CreateBox();
        legacySelectedBox.SizeScale_X = 1f;
        legacySelectedBox.SizeOffset_Y = 30f;
        legacySelectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        legacyRoadMaterialsContainer.AddChild(legacySelectedBox);
        legacyScrollBox = Glazier.Get().CreateScrollView();
        legacyScrollBox.PositionOffset_X = -200f;
        legacyScrollBox.PositionOffset_Y = 40f;
        legacyScrollBox.SizeScale_Y = 1f;
        legacyScrollBox.SizeOffset_X = 400f;
        legacyScrollBox.SizeOffset_Y = -40f;
        legacyScrollBox.ScaleContentToWidth = true;
        legacyScrollBox.ContentSizeOffset = new Vector2(0f, LevelRoads.materials.Length * 70 + 200);
        legacyRoadMaterialsContainer.AddChild(legacyScrollBox);
        for (int i = 0; i < LevelRoads.materials.Length; i++)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = 200f;
            sleekImage.PositionOffset_Y = i * 70;
            sleekImage.SizeOffset_X = 64f;
            sleekImage.SizeOffset_Y = 64f;
            sleekImage.Texture = LevelRoads.materials[i].material.mainTexture;
            legacyScrollBox.AddChild(sleekImage);
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 70f;
            sleekButton.SizeOffset_X = 100f;
            sleekButton.SizeOffset_Y = 64f;
            sleekButton.Text = LevelRoads.materials[i].material.mainTexture.name;
            sleekButton.OnClicked += OnLegacyRoadMaterialClicked;
            sleekImage.AddChild(sleekButton);
        }
        legacyWidthField = Glazier.Get().CreateFloat32Field();
        legacyWidthField.PositionOffset_X = 200f;
        legacyWidthField.PositionOffset_Y = LevelRoads.materials.Length * 70;
        legacyWidthField.SizeOffset_X = 170f;
        legacyWidthField.SizeOffset_Y = 30f;
        legacyWidthField.AddLabel(local.format("WidthFieldLabelText"), ESleekSide.LEFT);
        legacyWidthField.OnValueChanged += OnLegacyWidthChanged;
        legacyScrollBox.AddChild(legacyWidthField);
        legacyHeightField = Glazier.Get().CreateFloat32Field();
        legacyHeightField.PositionOffset_X = 200f;
        legacyHeightField.PositionOffset_Y = LevelRoads.materials.Length * 70 + 40;
        legacyHeightField.SizeOffset_X = 170f;
        legacyHeightField.SizeOffset_Y = 30f;
        legacyHeightField.AddLabel(local.format("HeightFieldLabelText"), ESleekSide.LEFT);
        legacyHeightField.OnValueChanged += OnLegacyHeightChanged;
        legacyScrollBox.AddChild(legacyHeightField);
        legacyDepthField = Glazier.Get().CreateFloat32Field();
        legacyDepthField.PositionOffset_X = 200f;
        legacyDepthField.PositionOffset_Y = LevelRoads.materials.Length * 70 + 80;
        legacyDepthField.SizeOffset_X = 170f;
        legacyDepthField.SizeOffset_Y = 30f;
        legacyDepthField.AddLabel(local.format("DepthFieldLabelText"), ESleekSide.LEFT);
        legacyDepthField.OnValueChanged += OnLegacyDepthChanged;
        legacyScrollBox.AddChild(legacyDepthField);
        legacyOffset2Field = Glazier.Get().CreateFloat32Field();
        legacyOffset2Field.PositionOffset_X = 200f;
        legacyOffset2Field.PositionOffset_Y = LevelRoads.materials.Length * 70 + 120;
        legacyOffset2Field.SizeOffset_X = 170f;
        legacyOffset2Field.SizeOffset_Y = 30f;
        legacyOffset2Field.AddLabel(local.format("OffsetFieldLabelText"), ESleekSide.LEFT);
        legacyOffset2Field.OnValueChanged += OnLegacyMaterialOffsetChanged;
        legacyScrollBox.AddChild(legacyOffset2Field);
        legacyConcreteToggle = Glazier.Get().CreateToggle();
        legacyConcreteToggle.PositionOffset_X = 200f;
        legacyConcreteToggle.PositionOffset_Y = LevelRoads.materials.Length * 70 + 160;
        legacyConcreteToggle.SizeOffset_X = 40f;
        legacyConcreteToggle.SizeOffset_Y = 40f;
        legacyConcreteToggle.AddLabel(local.format("ConcreteToggleLabelText"), ESleekSide.RIGHT);
        legacyConcreteToggle.OnValueChanged += OnLegacyConcreteToggled;
        legacyScrollBox.AddChild(legacyConcreteToggle);
        assetContainer = Glazier.Get().CreateFrame();
        assetContainer.PositionOffset_X = -300f;
        assetContainer.PositionOffset_Y = 120f;
        assetContainer.PositionScale_X = 1f;
        assetContainer.SizeOffset_X = 300f;
        assetContainer.SizeOffset_Y = -160f;
        assetContainer.SizeScale_Y = 1f;
        assetContainer.IsVisible = false;
        container.AddChild(assetContainer);
        selectedAssetBox = new SleekBoxIcon(null, 64);
        selectedAssetBox.SizeScale_X = 1f;
        selectedAssetBox.SizeOffset_Y = 74f;
        selectedAssetBox.AddLabel(local.format("SelectedAsset_Label"), ESleekSide.LEFT);
        assetContainer.AddChild(selectedAssetBox);
        onlyUsedAssetsToggle = Glazier.Get().CreateToggle();
        onlyUsedAssetsToggle.SizeOffset_X = 40f;
        onlyUsedAssetsToggle.SizeOffset_Y = 40f;
        onlyUsedAssetsToggle.PositionOffset_Y = 84f;
        onlyUsedAssetsToggle.AddLabel(local.format("OnlyUsedAssets_Label"), ESleekSide.RIGHT);
        onlyUsedAssetsToggle.OnValueChanged += OnOnlyUsedAssetsToggled;
        assetContainer.AddChild(onlyUsedAssetsToggle);
        searchField = Glazier.Get().CreateStringField();
        searchField.PositionOffset_Y = 124f;
        searchField.SizeOffset_Y = 30f;
        searchField.SizeScale_X = 1f;
        searchField.PlaceholderText = local.format("SearchHint");
        searchField.OnTextSubmitted += OnNameFilterSubmitted;
        assetContainer.AddChild(searchField);
        assetScrollView = Glazier.Get().CreateScrollView();
        assetScrollView.PositionOffset_Y = 154f;
        assetScrollView.SizeScale_X = 1f;
        assetScrollView.SizeScale_Y = 1f;
        assetScrollView.SizeOffset_Y = -154f;
        assetScrollView.ScaleContentToWidth = true;
        assetContainer.AddChild(assetScrollView);
        bakeRoadsButton = new SleekButtonIcon(bundle.load<Texture2D>("Roads"));
        bakeRoadsButton.PositionOffset_X = -200f;
        bakeRoadsButton.PositionOffset_Y = -30f;
        bakeRoadsButton.PositionScale_X = 1f;
        bakeRoadsButton.PositionScale_Y = 1f;
        bakeRoadsButton.SizeOffset_X = 200f;
        bakeRoadsButton.SizeOffset_Y = 30f;
        bakeRoadsButton.text = local.format("BakeRoadsButtonText");
        bakeRoadsButton.tooltip = local.format("BakeRoadsButtonTooltip");
        bakeRoadsButton.onClickedButton += onClickedBakeRoadsButton;
        container.AddChild(bakeRoadsButton);
        offsetField = Glazier.Get().CreateFloat32Field();
        offsetField.PositionOffset_Y = -280f;
        offsetField.PositionScale_Y = 1f;
        offsetField.SizeOffset_X = 200f;
        offsetField.SizeOffset_Y = 30f;
        offsetField.AddLabel(local.format("OffsetFieldLabelText"), ESleekSide.RIGHT);
        offsetField.OnValueChanged += onTypedOffsetField;
        container.AddChild(offsetField);
        offsetField.IsVisible = false;
        loopToggle = Glazier.Get().CreateToggle();
        loopToggle.PositionOffset_Y = -240f;
        loopToggle.PositionScale_Y = 1f;
        loopToggle.SizeOffset_X = 40f;
        loopToggle.SizeOffset_Y = 40f;
        loopToggle.AddLabel(local.format("LoopToggleLabelText"), ESleekSide.RIGHT);
        loopToggle.OnValueChanged += onToggledLoopToggle;
        container.AddChild(loopToggle);
        loopToggle.IsVisible = false;
        ignoreTerrainToggle = Glazier.Get().CreateToggle();
        ignoreTerrainToggle.PositionOffset_Y = -190f;
        ignoreTerrainToggle.PositionScale_Y = 1f;
        ignoreTerrainToggle.SizeOffset_X = 40f;
        ignoreTerrainToggle.SizeOffset_Y = 40f;
        ignoreTerrainToggle.AddLabel(local.format("IgnoreTerrainToggleLabelText"), ESleekSide.RIGHT);
        ignoreTerrainToggle.OnValueChanged += onToggledIgnoreTerrainToggle;
        container.AddChild(ignoreTerrainToggle);
        ignoreTerrainToggle.IsVisible = false;
        modeButton = new SleekButtonState(new GUIContent(local.format("Mirror")), new GUIContent(local.format("Aligned")), new GUIContent(local.format("Free")));
        modeButton.PositionOffset_Y = -140f;
        modeButton.PositionScale_Y = 1f;
        modeButton.SizeOffset_X = 200f;
        modeButton.SizeOffset_Y = 30f;
        modeButton.tooltip = local.format("ModeButtonTooltipText");
        modeButton.onSwappedState = onSwappedStateMode;
        container.AddChild(modeButton);
        modeButton.IsVisible = false;
        roadIndexBox = Glazier.Get().CreateBox();
        roadIndexBox.PositionOffset_Y = -100f;
        roadIndexBox.PositionScale_Y = 1f;
        roadIndexBox.SizeOffset_X = 200f;
        roadIndexBox.SizeOffset_Y = 30f;
        roadIndexBox.AddLabel(local.format("RoadIndexLabelText"), ESleekSide.RIGHT);
        container.AddChild(roadIndexBox);
        roadIndexBox.IsVisible = false;
        roadAssetField = new SleekAssetField(typeof(RoadAsset));
        roadAssetField.PositionOffset_Y = -60f;
        roadAssetField.PositionScale_Y = 1f;
        roadAssetField.SizeOffset_X = 200f;
        roadAssetField.SizeOffset_Y = 60f;
        roadAssetField.AddLabel(local.format("RoadAsset_Label"), ESleekSide.RIGHT);
        roadAssetField.TooltipText = local.format("RoadAsset_Tooltip");
        roadAssetField.OnValueChanged += OnRoadAssetChanged;
        roadAssetField.IsVisible = false;
        container.AddChild(roadAssetField);
        RefreshListMode();
        RefreshLegacySelection();
        bundle.unload();
    }
}
