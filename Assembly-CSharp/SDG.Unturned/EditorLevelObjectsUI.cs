using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class EditorLevelObjectsUI : SleekFullscreenBox
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static List<ObjectAsset> tempObjectAssets = new List<ObjectAsset>();

    private static List<Asset> assets;

    private static AssetNameAscendingComparator comparator = new AssetNameAscendingComparator();

    private static SleekList<Asset> assetsScrollBox;

    private static ISleekBox selectedBox;

    private static ISleekField searchField;

    private static ISleekButton searchButton;

    private static ISleekToggle largeToggle;

    private static ISleekToggle mediumToggle;

    private static ISleekToggle smallToggle;

    private static ISleekToggle barricadesToggle;

    private static ISleekToggle structuresToggle;

    private static ISleekToggle npcsToggle;

    private static ISleekImage dragBox;

    private static ISleekField materialPaletteOverrideField;

    private static ISleekInt32Field materialIndexOverrideField;

    private static ISleekFloat32Field snapTransformField;

    private static ISleekFloat32Field snapRotationField;

    private static SleekButtonIcon transformButton;

    private static SleekButtonIcon rotateButton;

    private static SleekButtonIcon scaleButton;

    public static SleekButtonState coordinateButton;

    private GameObject focusedGameObject;

    private static LevelObject focusedLevelObject;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorObjects.isBuilding = true;
            EditorUI.message(EEditorMessage.OBJECTS);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorObjects.isBuilding = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        GameObject mostRecentSelectedGameObject = EditorObjects.GetMostRecentSelectedGameObject();
        if (!(focusedGameObject == mostRecentSelectedGameObject))
        {
            focusedGameObject = mostRecentSelectedGameObject;
            focusedLevelObject = LevelObjects.FindLevelObject(focusedGameObject);
            if (focusedLevelObject != null)
            {
                materialPaletteOverrideField.IsVisible = true;
                materialIndexOverrideField.IsVisible = true;
                materialPaletteOverrideField.Text = focusedLevelObject.customMaterialOverride.ToString();
                materialIndexOverrideField.Value = focusedLevelObject.materialIndexOverride;
            }
            else
            {
                materialPaletteOverrideField.IsVisible = false;
                materialIndexOverrideField.IsVisible = false;
            }
        }
    }

    private static void updateSelection(string search, bool large, bool medium, bool small, bool barricades, bool structures, bool npcs)
    {
        if (assets == null)
        {
            return;
        }
        assets.Clear();
        EditorObjectSearchFilter editorObjectSearchFilter = EditorObjectSearchFilter.parse(search);
        if (large || medium || small || npcs)
        {
            tempObjectAssets.Clear();
            Assets.find(tempObjectAssets);
            foreach (ObjectAsset tempObjectAsset in tempObjectAssets)
            {
                if ((large || tempObjectAsset.type != 0) && (medium || tempObjectAsset.type != EObjectType.MEDIUM) && (small || (tempObjectAsset.type != EObjectType.SMALL && tempObjectAsset.type != EObjectType.DECAL)) && (npcs || tempObjectAsset.type != EObjectType.NPC) && (editorObjectSearchFilter == null || !editorObjectSearchFilter.ignores(tempObjectAsset)))
                {
                    assets.Add(tempObjectAsset);
                }
            }
        }
        if (barricades || structures)
        {
            List<ItemAsset> list = new List<ItemAsset>();
            Assets.find(list);
            foreach (ItemAsset item in list)
            {
                if (item is ItemBarricadeAsset)
                {
                    if (!barricades)
                    {
                        continue;
                    }
                }
                else if (!(item is ItemStructureAsset) || !structures)
                {
                    continue;
                }
                if (editorObjectSearchFilter == null || !editorObjectSearchFilter.ignores(item))
                {
                    assets.Add(item);
                }
            }
        }
        assets.Sort(comparator);
        assetsScrollBox.NotifyDataChanged();
    }

    private static void onAssetsRefreshed()
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static ISleekElement onCreateAssetButton(Asset item)
    {
        string text = string.Empty;
        ObjectAsset objectAsset = item as ObjectAsset;
        ItemAsset itemAsset = item as ItemAsset;
        if (objectAsset != null)
        {
            text = objectAsset.objectName;
        }
        else if (itemAsset != null)
        {
            text = itemAsset.itemName;
        }
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.Text = text;
        sleekButton.OnClicked += onClickedAssetButton;
        return sleekButton;
    }

    private static void onClickedAssetButton(ISleekElement button)
    {
        int index = Mathf.FloorToInt(button.PositionOffset_Y / 40f);
        EditorObjects.selectedObjectAsset = assets[index] as ObjectAsset;
        EditorObjects.selectedItemAsset = assets[index] as ItemAsset;
        if (EditorObjects.selectedObjectAsset != null)
        {
            selectedBox.Text = EditorObjects.selectedObjectAsset.objectName;
        }
        else if (EditorObjects.selectedItemAsset != null)
        {
            selectedBox.Text = EditorObjects.selectedItemAsset.itemName;
        }
    }

    private static void onDragStarted(Vector2 minViewportPoint, Vector2 maxViewportPoint)
    {
        Vector2 vector = EditorUI.window.ViewportToNormalizedPosition(minViewportPoint);
        Vector2 vector2 = EditorUI.window.ViewportToNormalizedPosition(maxViewportPoint);
        if (vector2.y < vector.y)
        {
            float y = vector2.y;
            vector2.y = vector.y;
            vector.y = y;
        }
        dragBox.PositionScale_X = vector.x;
        dragBox.PositionScale_Y = vector.y;
        dragBox.SizeScale_X = vector2.x - vector.x;
        dragBox.SizeScale_Y = vector2.y - vector.y;
        dragBox.IsVisible = true;
    }

    private static void onDragStopped()
    {
        dragBox.IsVisible = false;
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onToggledLargeToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, state, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onToggledMediumToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, largeToggle.Value, state, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onToggledSmallToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, state, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onToggledBarricadesToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, state, structuresToggle.Value, npcsToggle.Value);
    }

    private static void onToggledStructuresToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, state, npcsToggle.Value);
    }

    private static void onToggledNPCsToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, state);
    }

    private static void OnTypedMaterialPaletteOverride(ISleekField field, string value)
    {
        AssetReference<MaterialPaletteAsset> customMaterialOverride = new AssetReference<MaterialPaletteAsset>(value);
        foreach (GameObject item in EditorObjects.EnumerateSelectedGameObjects())
        {
            LevelObject levelObject = LevelObjects.FindLevelObject(item);
            if (levelObject != null)
            {
                levelObject.customMaterialOverride = customMaterialOverride;
                levelObject.ReapplyMaterialOverrides();
            }
        }
    }

    private static void OnTypedMaterialIndexOverride(ISleekInt32Field field, int value)
    {
        foreach (GameObject item in EditorObjects.EnumerateSelectedGameObjects())
        {
            LevelObject levelObject = LevelObjects.FindLevelObject(item);
            if (levelObject != null)
            {
                levelObject.materialIndexOverride = value;
                levelObject.ReapplyMaterialOverrides();
            }
        }
    }

    private static void onTypedSnapTransformField(ISleekFloat32Field field, float value)
    {
        EditorObjects.snapTransform = value;
    }

    private static void onTypedSnapRotationField(ISleekFloat32Field field, float value)
    {
        EditorObjects.snapRotation = value;
    }

    private static void onClickedTransformButton(ISleekElement button)
    {
        EditorObjects.dragMode = EDragMode.TRANSFORM;
    }

    private static void onClickedRotateButton(ISleekElement button)
    {
        EditorObjects.dragMode = EDragMode.ROTATE;
    }

    private static void onClickedScaleButton(ISleekElement button)
    {
        EditorObjects.dragMode = EDragMode.SCALE;
    }

    private static void onSwappedStateCoordinate(SleekButtonState button, int index)
    {
        EditorObjects.dragCoordinate = (EDragCoordinate)index;
    }

    public override void OnDestroy()
    {
        Assets.onAssetsRefreshed = (AssetsRefreshed)Delegate.Remove(Assets.onAssetsRefreshed, new AssetsRefreshed(onAssetsRefreshed));
    }

    public EditorLevelObjectsUI()
    {
        Local local = Localization.read("/Editor/EditorLevelObjects.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorLevelObjects/EditorLevelObjects.unity3d");
        container = this;
        active = false;
        assets = new List<Asset>();
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -230f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 230f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        AddChild(selectedBox);
        searchField = Glazier.Get().CreateStringField();
        searchField.PositionOffset_X = -230f;
        searchField.PositionOffset_Y = 40f;
        searchField.PositionScale_X = 1f;
        searchField.SizeOffset_X = 160f;
        searchField.SizeOffset_Y = 30f;
        searchField.PlaceholderText = local.format("Search_Field_Hint");
        searchField.OnTextSubmitted += onEnteredSearchField;
        AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.PositionOffset_X = -60f;
        searchButton.PositionOffset_Y = 40f;
        searchButton.PositionScale_X = 1f;
        searchButton.SizeOffset_X = 60f;
        searchButton.SizeOffset_Y = 30f;
        searchButton.Text = local.format("Search");
        searchButton.TooltipText = local.format("Search_Tooltip");
        searchButton.OnClicked += onClickedSearchButton;
        AddChild(searchButton);
        largeToggle = Glazier.Get().CreateToggle();
        largeToggle.PositionOffset_X = -230f;
        largeToggle.PositionOffset_Y = 80f;
        largeToggle.PositionScale_X = 1f;
        largeToggle.SizeOffset_X = 40f;
        largeToggle.SizeOffset_Y = 40f;
        largeToggle.AddLabel(local.format("LargeLabel"), ESleekSide.RIGHT);
        largeToggle.Value = true;
        largeToggle.OnValueChanged += onToggledLargeToggle;
        AddChild(largeToggle);
        mediumToggle = Glazier.Get().CreateToggle();
        mediumToggle.PositionOffset_X = -230f;
        mediumToggle.PositionOffset_Y = 130f;
        mediumToggle.PositionScale_X = 1f;
        mediumToggle.SizeOffset_X = 40f;
        mediumToggle.SizeOffset_Y = 40f;
        mediumToggle.AddLabel(local.format("MediumLabel"), ESleekSide.RIGHT);
        mediumToggle.Value = true;
        mediumToggle.OnValueChanged += onToggledMediumToggle;
        AddChild(mediumToggle);
        smallToggle = Glazier.Get().CreateToggle();
        smallToggle.PositionOffset_X = -230f;
        smallToggle.PositionOffset_Y = 180f;
        smallToggle.PositionScale_X = 1f;
        smallToggle.SizeOffset_X = 40f;
        smallToggle.SizeOffset_Y = 40f;
        smallToggle.AddLabel(local.format("SmallLabel"), ESleekSide.RIGHT);
        smallToggle.Value = true;
        smallToggle.OnValueChanged += onToggledSmallToggle;
        AddChild(smallToggle);
        barricadesToggle = Glazier.Get().CreateToggle();
        barricadesToggle.PositionOffset_X = -130f;
        barricadesToggle.PositionOffset_Y = 80f;
        barricadesToggle.PositionScale_X = 1f;
        barricadesToggle.SizeOffset_X = 40f;
        barricadesToggle.SizeOffset_Y = 40f;
        barricadesToggle.AddLabel(local.format("BarricadesLabel"), ESleekSide.RIGHT);
        barricadesToggle.Value = false;
        barricadesToggle.OnValueChanged += onToggledBarricadesToggle;
        AddChild(barricadesToggle);
        structuresToggle = Glazier.Get().CreateToggle();
        structuresToggle.PositionOffset_X = -130f;
        structuresToggle.PositionOffset_Y = 130f;
        structuresToggle.PositionScale_X = 1f;
        structuresToggle.SizeOffset_X = 40f;
        structuresToggle.SizeOffset_Y = 40f;
        structuresToggle.AddLabel(local.format("StructuresLabel"), ESleekSide.RIGHT);
        structuresToggle.Value = false;
        structuresToggle.OnValueChanged += onToggledStructuresToggle;
        AddChild(structuresToggle);
        npcsToggle = Glazier.Get().CreateToggle();
        npcsToggle.PositionOffset_X = -130f;
        npcsToggle.PositionOffset_Y = 180f;
        npcsToggle.PositionScale_X = 1f;
        npcsToggle.SizeOffset_X = 40f;
        npcsToggle.SizeOffset_Y = 40f;
        npcsToggle.AddLabel(local.format("NPCsLabel"), ESleekSide.RIGHT);
        npcsToggle.Value = false;
        npcsToggle.OnValueChanged += onToggledNPCsToggle;
        AddChild(npcsToggle);
        assetsScrollBox = new SleekList<Asset>();
        assetsScrollBox.PositionOffset_X = -230f;
        assetsScrollBox.PositionOffset_Y = 230f;
        assetsScrollBox.PositionScale_X = 1f;
        assetsScrollBox.SizeOffset_X = 230f;
        assetsScrollBox.SizeOffset_Y = -230f;
        assetsScrollBox.SizeScale_Y = 1f;
        assetsScrollBox.itemHeight = 30;
        assetsScrollBox.itemPadding = 10;
        assetsScrollBox.onCreateElement = onCreateAssetButton;
        assetsScrollBox.SetData(assets);
        AddChild(assetsScrollBox);
        EditorObjects.selectedObjectAsset = null;
        EditorObjects.selectedItemAsset = null;
        EditorObjects.onDragStarted = onDragStarted;
        EditorObjects.onDragStopped = onDragStopped;
        dragBox = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        dragBox.TintColor = new Color(1f, 1f, 0f, 0.2f);
        EditorUI.window.AddChild(dragBox);
        dragBox.IsVisible = false;
        materialPaletteOverrideField = Glazier.Get().CreateStringField();
        materialPaletteOverrideField.PositionOffset_Y = -310f;
        materialPaletteOverrideField.PositionScale_Y = 1f;
        materialPaletteOverrideField.SizeOffset_X = 200f;
        materialPaletteOverrideField.SizeOffset_Y = 30f;
        materialPaletteOverrideField.AddLabel(local.format("MaterialPaletteOverride_Label"), ESleekSide.RIGHT);
        materialPaletteOverrideField.TooltipText = local.format("MaterialPaletteOverride_Tooltip");
        materialPaletteOverrideField.OnTextChanged += OnTypedMaterialPaletteOverride;
        AddChild(materialPaletteOverrideField);
        materialIndexOverrideField = Glazier.Get().CreateInt32Field();
        materialIndexOverrideField.PositionOffset_Y = -270f;
        materialIndexOverrideField.PositionScale_Y = 1f;
        materialIndexOverrideField.SizeOffset_X = 200f;
        materialIndexOverrideField.SizeOffset_Y = 30f;
        materialIndexOverrideField.AddLabel(local.format("MaterialIndexOverride_Label"), ESleekSide.RIGHT);
        materialIndexOverrideField.TooltipText = local.format("MaterialIndexOverride_Tooltip");
        materialIndexOverrideField.OnValueChanged += OnTypedMaterialIndexOverride;
        AddChild(materialIndexOverrideField);
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.PositionOffset_Y = -230f;
        snapTransformField.PositionScale_Y = 1f;
        snapTransformField.SizeOffset_X = 200f;
        snapTransformField.SizeOffset_Y = 30f;
        snapTransformField.Value = EditorObjects.snapTransform;
        snapTransformField.AddLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.OnValueChanged += onTypedSnapTransformField;
        AddChild(snapTransformField);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.PositionOffset_Y = -190f;
        snapRotationField.PositionScale_Y = 1f;
        snapRotationField.SizeOffset_X = 200f;
        snapRotationField.SizeOffset_Y = 30f;
        snapRotationField.Value = EditorObjects.snapRotation;
        snapRotationField.AddLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.OnValueChanged += onTypedSnapRotationField;
        AddChild(snapRotationField);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.PositionOffset_Y = -150f;
        transformButton.PositionScale_Y = 1f;
        transformButton.SizeOffset_X = 200f;
        transformButton.SizeOffset_Y = 30f;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += onClickedTransformButton;
        AddChild(transformButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.PositionOffset_Y = -110f;
        rotateButton.PositionScale_Y = 1f;
        rotateButton.SizeOffset_X = 200f;
        rotateButton.SizeOffset_Y = 30f;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += onClickedRotateButton;
        AddChild(rotateButton);
        scaleButton = new SleekButtonIcon(bundle.load<Texture2D>("Scale"));
        scaleButton.PositionOffset_Y = -70f;
        scaleButton.PositionScale_Y = 1f;
        scaleButton.SizeOffset_X = 200f;
        scaleButton.SizeOffset_Y = 30f;
        scaleButton.text = local.format("ScaleButtonText", ControlsSettings.tool_3);
        scaleButton.tooltip = local.format("ScaleButtonTooltip");
        scaleButton.onClickedButton += onClickedScaleButton;
        AddChild(scaleButton);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.PositionOffset_Y = -30f;
        coordinateButton.PositionScale_Y = 1f;
        coordinateButton.SizeOffset_X = 200f;
        coordinateButton.SizeOffset_Y = 30f;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = onSwappedStateCoordinate;
        AddChild(coordinateButton);
        bundle.unload();
        onAssetsRefreshed();
        Assets.onAssetsRefreshed = (AssetsRefreshed)Delegate.Combine(Assets.onAssetsRefreshed, new AssetsRefreshed(onAssetsRefreshed));
    }
}
