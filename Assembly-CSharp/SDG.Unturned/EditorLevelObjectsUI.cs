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
                materialPaletteOverrideField.isVisible = true;
                materialIndexOverrideField.isVisible = true;
                materialPaletteOverrideField.text = focusedLevelObject.customMaterialOverride.ToString();
                materialIndexOverrideField.state = focusedLevelObject.materialIndexOverride;
            }
            else
            {
                materialPaletteOverrideField.isVisible = false;
                materialIndexOverrideField.isVisible = false;
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
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
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
        sleekButton.text = text;
        sleekButton.onClickedButton += onClickedAssetButton;
        return sleekButton;
    }

    private static void onClickedAssetButton(ISleekElement button)
    {
        int index = button.positionOffset_Y / 40;
        EditorObjects.selectedObjectAsset = assets[index] as ObjectAsset;
        EditorObjects.selectedItemAsset = assets[index] as ItemAsset;
        if (EditorObjects.selectedObjectAsset != null)
        {
            selectedBox.text = EditorObjects.selectedObjectAsset.objectName;
        }
        else if (EditorObjects.selectedItemAsset != null)
        {
            selectedBox.text = EditorObjects.selectedItemAsset.itemName;
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
        dragBox.positionScale_X = vector.x;
        dragBox.positionScale_Y = vector.y;
        dragBox.sizeScale_X = vector2.x - vector.x;
        dragBox.sizeScale_Y = vector2.y - vector.y;
        dragBox.isVisible = true;
    }

    private static void onDragStopped()
    {
        dragBox.isVisible = false;
    }

    private static void onEnteredSearchField(ISleekField field)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
    }

    private static void onToggledLargeToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, state, mediumToggle.state, smallToggle.state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
    }

    private static void onToggledMediumToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, largeToggle.state, state, smallToggle.state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
    }

    private static void onToggledSmallToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, state, barricadesToggle.state, structuresToggle.state, npcsToggle.state);
    }

    private static void onToggledBarricadesToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, state, structuresToggle.state, npcsToggle.state);
    }

    private static void onToggledStructuresToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, barricadesToggle.state, state, npcsToggle.state);
    }

    private static void onToggledNPCsToggle(ISleekToggle toggle, bool state)
    {
        updateSelection(searchField.text, largeToggle.state, mediumToggle.state, smallToggle.state, barricadesToggle.state, structuresToggle.state, state);
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
        selectedBox.positionOffset_X = -230;
        selectedBox.positionScale_X = 1f;
        selectedBox.sizeOffset_X = 230;
        selectedBox.sizeOffset_Y = 30;
        selectedBox.addLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        AddChild(selectedBox);
        searchField = Glazier.Get().CreateStringField();
        searchField.positionOffset_X = -230;
        searchField.positionOffset_Y = 40;
        searchField.positionScale_X = 1f;
        searchField.sizeOffset_X = 160;
        searchField.sizeOffset_Y = 30;
        searchField.hint = local.format("Search_Field_Hint");
        searchField.onEntered += onEnteredSearchField;
        AddChild(searchField);
        searchButton = Glazier.Get().CreateButton();
        searchButton.positionOffset_X = -60;
        searchButton.positionOffset_Y = 40;
        searchButton.positionScale_X = 1f;
        searchButton.sizeOffset_X = 60;
        searchButton.sizeOffset_Y = 30;
        searchButton.text = local.format("Search");
        searchButton.tooltipText = local.format("Search_Tooltip");
        searchButton.onClickedButton += onClickedSearchButton;
        AddChild(searchButton);
        largeToggle = Glazier.Get().CreateToggle();
        largeToggle.positionOffset_X = -230;
        largeToggle.positionOffset_Y = 80;
        largeToggle.positionScale_X = 1f;
        largeToggle.sizeOffset_X = 40;
        largeToggle.sizeOffset_Y = 40;
        largeToggle.addLabel(local.format("LargeLabel"), ESleekSide.RIGHT);
        largeToggle.state = true;
        largeToggle.onToggled += onToggledLargeToggle;
        AddChild(largeToggle);
        mediumToggle = Glazier.Get().CreateToggle();
        mediumToggle.positionOffset_X = -230;
        mediumToggle.positionOffset_Y = 130;
        mediumToggle.positionScale_X = 1f;
        mediumToggle.sizeOffset_X = 40;
        mediumToggle.sizeOffset_Y = 40;
        mediumToggle.addLabel(local.format("MediumLabel"), ESleekSide.RIGHT);
        mediumToggle.state = true;
        mediumToggle.onToggled += onToggledMediumToggle;
        AddChild(mediumToggle);
        smallToggle = Glazier.Get().CreateToggle();
        smallToggle.positionOffset_X = -230;
        smallToggle.positionOffset_Y = 180;
        smallToggle.positionScale_X = 1f;
        smallToggle.sizeOffset_X = 40;
        smallToggle.sizeOffset_Y = 40;
        smallToggle.addLabel(local.format("SmallLabel"), ESleekSide.RIGHT);
        smallToggle.state = true;
        smallToggle.onToggled += onToggledSmallToggle;
        AddChild(smallToggle);
        barricadesToggle = Glazier.Get().CreateToggle();
        barricadesToggle.positionOffset_X = -130;
        barricadesToggle.positionOffset_Y = 80;
        barricadesToggle.positionScale_X = 1f;
        barricadesToggle.sizeOffset_X = 40;
        barricadesToggle.sizeOffset_Y = 40;
        barricadesToggle.addLabel(local.format("BarricadesLabel"), ESleekSide.RIGHT);
        barricadesToggle.state = false;
        barricadesToggle.onToggled += onToggledBarricadesToggle;
        AddChild(barricadesToggle);
        structuresToggle = Glazier.Get().CreateToggle();
        structuresToggle.positionOffset_X = -130;
        structuresToggle.positionOffset_Y = 130;
        structuresToggle.positionScale_X = 1f;
        structuresToggle.sizeOffset_X = 40;
        structuresToggle.sizeOffset_Y = 40;
        structuresToggle.addLabel(local.format("StructuresLabel"), ESleekSide.RIGHT);
        structuresToggle.state = false;
        structuresToggle.onToggled += onToggledStructuresToggle;
        AddChild(structuresToggle);
        npcsToggle = Glazier.Get().CreateToggle();
        npcsToggle.positionOffset_X = -130;
        npcsToggle.positionOffset_Y = 180;
        npcsToggle.positionScale_X = 1f;
        npcsToggle.sizeOffset_X = 40;
        npcsToggle.sizeOffset_Y = 40;
        npcsToggle.addLabel(local.format("NPCsLabel"), ESleekSide.RIGHT);
        npcsToggle.state = false;
        npcsToggle.onToggled += onToggledNPCsToggle;
        AddChild(npcsToggle);
        assetsScrollBox = new SleekList<Asset>();
        assetsScrollBox.positionOffset_X = -230;
        assetsScrollBox.positionOffset_Y = 230;
        assetsScrollBox.positionScale_X = 1f;
        assetsScrollBox.sizeOffset_X = 230;
        assetsScrollBox.sizeOffset_Y = -230;
        assetsScrollBox.sizeScale_Y = 1f;
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
        dragBox.color = new Color(1f, 1f, 0f, 0.2f);
        EditorUI.window.AddChild(dragBox);
        dragBox.isVisible = false;
        materialPaletteOverrideField = Glazier.Get().CreateStringField();
        materialPaletteOverrideField.positionOffset_Y = -310;
        materialPaletteOverrideField.positionScale_Y = 1f;
        materialPaletteOverrideField.sizeOffset_X = 200;
        materialPaletteOverrideField.sizeOffset_Y = 30;
        materialPaletteOverrideField.addLabel(local.format("MaterialPaletteOverride_Label"), ESleekSide.RIGHT);
        materialPaletteOverrideField.tooltipText = local.format("MaterialPaletteOverride_Tooltip");
        materialPaletteOverrideField.onTyped += OnTypedMaterialPaletteOverride;
        AddChild(materialPaletteOverrideField);
        materialIndexOverrideField = Glazier.Get().CreateInt32Field();
        materialIndexOverrideField.positionOffset_Y = -270;
        materialIndexOverrideField.positionScale_Y = 1f;
        materialIndexOverrideField.sizeOffset_X = 200;
        materialIndexOverrideField.sizeOffset_Y = 30;
        materialIndexOverrideField.addLabel(local.format("MaterialIndexOverride_Label"), ESleekSide.RIGHT);
        materialIndexOverrideField.tooltipText = local.format("MaterialIndexOverride_Tooltip");
        materialIndexOverrideField.onTypedInt += OnTypedMaterialIndexOverride;
        AddChild(materialIndexOverrideField);
        snapTransformField = Glazier.Get().CreateFloat32Field();
        snapTransformField.positionOffset_Y = -230;
        snapTransformField.positionScale_Y = 1f;
        snapTransformField.sizeOffset_X = 200;
        snapTransformField.sizeOffset_Y = 30;
        snapTransformField.state = EditorObjects.snapTransform;
        snapTransformField.addLabel(local.format("SnapTransformLabelText"), ESleekSide.RIGHT);
        snapTransformField.onTypedSingle += onTypedSnapTransformField;
        AddChild(snapTransformField);
        snapRotationField = Glazier.Get().CreateFloat32Field();
        snapRotationField.positionOffset_Y = -190;
        snapRotationField.positionScale_Y = 1f;
        snapRotationField.sizeOffset_X = 200;
        snapRotationField.sizeOffset_Y = 30;
        snapRotationField.state = EditorObjects.snapRotation;
        snapRotationField.addLabel(local.format("SnapRotationLabelText"), ESleekSide.RIGHT);
        snapRotationField.onTypedSingle += onTypedSnapRotationField;
        AddChild(snapRotationField);
        transformButton = new SleekButtonIcon(bundle.load<Texture2D>("Transform"));
        transformButton.positionOffset_Y = -150;
        transformButton.positionScale_Y = 1f;
        transformButton.sizeOffset_X = 200;
        transformButton.sizeOffset_Y = 30;
        transformButton.text = local.format("TransformButtonText", ControlsSettings.tool_0);
        transformButton.tooltip = local.format("TransformButtonTooltip");
        transformButton.onClickedButton += onClickedTransformButton;
        AddChild(transformButton);
        rotateButton = new SleekButtonIcon(bundle.load<Texture2D>("Rotate"));
        rotateButton.positionOffset_Y = -110;
        rotateButton.positionScale_Y = 1f;
        rotateButton.sizeOffset_X = 200;
        rotateButton.sizeOffset_Y = 30;
        rotateButton.text = local.format("RotateButtonText", ControlsSettings.tool_1);
        rotateButton.tooltip = local.format("RotateButtonTooltip");
        rotateButton.onClickedButton += onClickedRotateButton;
        AddChild(rotateButton);
        scaleButton = new SleekButtonIcon(bundle.load<Texture2D>("Scale"));
        scaleButton.positionOffset_Y = -70;
        scaleButton.positionScale_Y = 1f;
        scaleButton.sizeOffset_X = 200;
        scaleButton.sizeOffset_Y = 30;
        scaleButton.text = local.format("ScaleButtonText", ControlsSettings.tool_3);
        scaleButton.tooltip = local.format("ScaleButtonTooltip");
        scaleButton.onClickedButton += onClickedScaleButton;
        AddChild(scaleButton);
        coordinateButton = new SleekButtonState(new GUIContent(local.format("CoordinateButtonTextGlobal"), bundle.load<Texture>("Global")), new GUIContent(local.format("CoordinateButtonTextLocal"), bundle.load<Texture>("Local")));
        coordinateButton.positionOffset_Y = -30;
        coordinateButton.positionScale_Y = 1f;
        coordinateButton.sizeOffset_X = 200;
        coordinateButton.sizeOffset_Y = 30;
        coordinateButton.tooltip = local.format("CoordinateButtonTooltip");
        coordinateButton.onSwappedState = onSwappedStateCoordinate;
        AddChild(coordinateButton);
        bundle.unload();
        onAssetsRefreshed();
        Assets.onAssetsRefreshed = (AssetsRefreshed)Delegate.Combine(Assets.onAssetsRefreshed, new AssetsRefreshed(onAssetsRefreshed));
    }
}
