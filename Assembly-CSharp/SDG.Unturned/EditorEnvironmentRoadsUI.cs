using UnityEngine;

namespace SDG.Unturned;

public class EditorEnvironmentRoadsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekScrollView roadScrollBox;

    private static ISleekBox selectedBox;

    private static ISleekFloat32Field widthField;

    private static ISleekFloat32Field heightField;

    private static ISleekFloat32Field depthField;

    private static ISleekFloat32Field offset2Field;

    private static ISleekToggle concreteToggle;

    private static SleekButtonIcon bakeRoadsButton;

    private static ISleekFloat32Field offsetField;

    private static ISleekToggle loopToggle;

    private static ISleekToggle ignoreTerrainToggle;

    private static SleekButtonState modeButton;

    private static ISleekBox roadIndexBox;

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
            offsetField.state = joint.offset;
            loopToggle.state = road.isLoop;
            ignoreTerrainToggle.state = joint.ignoreTerrain;
            modeButton.state = (int)joint.mode;
            roadIndexBox.text = LevelRoads.getRoadIndex(road).ToString();
        }
        offsetField.isVisible = road != null;
        loopToggle.isVisible = road != null;
        ignoreTerrainToggle.isVisible = road != null;
        modeButton.isVisible = road != null;
        roadIndexBox.isVisible = road != null;
    }

    private static void updateSelection()
    {
        if (EditorRoads.selected < LevelRoads.materials.Length)
        {
            RoadMaterial roadMaterial = LevelRoads.materials[EditorRoads.selected];
            selectedBox.text = roadMaterial.material.mainTexture.name;
            widthField.state = roadMaterial.width;
            heightField.state = roadMaterial.height;
            depthField.state = roadMaterial.depth;
            offset2Field.state = roadMaterial.offset;
            concreteToggle.state = roadMaterial.isConcrete;
        }
    }

    private static void onClickedRoadButton(ISleekElement button)
    {
        EditorRoads.selected = (byte)(button.parent.positionOffset_Y / 70);
        if (EditorRoads.road != null)
        {
            EditorRoads.road.material = EditorRoads.selected;
        }
        updateSelection();
    }

    private static void onTypedWidthField(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].width = state;
    }

    private static void onTypedHeightField(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].height = state;
    }

    private static void onTypedDepthField(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].depth = state;
    }

    private static void onTypedOffset2Field(ISleekFloat32Field field, float state)
    {
        LevelRoads.materials[EditorRoads.selected].offset = state;
    }

    private static void onToggledConcreteToggle(ISleekToggle toggle, bool state)
    {
        LevelRoads.materials[EditorRoads.selected].isConcrete = state;
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

    public EditorEnvironmentRoadsUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironmentRoads.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorEnvironmentRoads/EditorEnvironmentRoads.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        roadScrollBox = Glazier.Get().CreateScrollView();
        roadScrollBox.positionOffset_X = -400;
        roadScrollBox.positionOffset_Y = 120;
        roadScrollBox.positionScale_X = 1f;
        roadScrollBox.sizeOffset_X = 400;
        roadScrollBox.sizeOffset_Y = -160;
        roadScrollBox.sizeScale_Y = 1f;
        roadScrollBox.scaleContentToWidth = true;
        roadScrollBox.contentSizeOffset = new Vector2(0f, LevelRoads.materials.Length * 70 + 200);
        container.AddChild(roadScrollBox);
        for (int i = 0; i < LevelRoads.materials.Length; i++)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = 200;
            sleekImage.positionOffset_Y = i * 70;
            sleekImage.sizeOffset_X = 64;
            sleekImage.sizeOffset_Y = 64;
            sleekImage.texture = LevelRoads.materials[i].material.mainTexture;
            roadScrollBox.AddChild(sleekImage);
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = 70;
            sleekButton.sizeOffset_X = 100;
            sleekButton.sizeOffset_Y = 64;
            sleekButton.text = LevelRoads.materials[i].material.mainTexture.name;
            sleekButton.onClickedButton += onClickedRoadButton;
            sleekImage.AddChild(sleekButton);
        }
        widthField = Glazier.Get().CreateFloat32Field();
        widthField.positionOffset_X = 200;
        widthField.positionOffset_Y = LevelRoads.materials.Length * 70;
        widthField.sizeOffset_X = 170;
        widthField.sizeOffset_Y = 30;
        widthField.addLabel(local.format("WidthFieldLabelText"), ESleekSide.LEFT);
        widthField.onTypedSingle += onTypedWidthField;
        roadScrollBox.AddChild(widthField);
        heightField = Glazier.Get().CreateFloat32Field();
        heightField.positionOffset_X = 200;
        heightField.positionOffset_Y = LevelRoads.materials.Length * 70 + 40;
        heightField.sizeOffset_X = 170;
        heightField.sizeOffset_Y = 30;
        heightField.addLabel(local.format("HeightFieldLabelText"), ESleekSide.LEFT);
        heightField.onTypedSingle += onTypedHeightField;
        roadScrollBox.AddChild(heightField);
        depthField = Glazier.Get().CreateFloat32Field();
        depthField.positionOffset_X = 200;
        depthField.positionOffset_Y = LevelRoads.materials.Length * 70 + 80;
        depthField.sizeOffset_X = 170;
        depthField.sizeOffset_Y = 30;
        depthField.addLabel(local.format("DepthFieldLabelText"), ESleekSide.LEFT);
        depthField.onTypedSingle += onTypedDepthField;
        roadScrollBox.AddChild(depthField);
        offset2Field = Glazier.Get().CreateFloat32Field();
        offset2Field.positionOffset_X = 200;
        offset2Field.positionOffset_Y = LevelRoads.materials.Length * 70 + 120;
        offset2Field.sizeOffset_X = 170;
        offset2Field.sizeOffset_Y = 30;
        offset2Field.addLabel(local.format("OffsetFieldLabelText"), ESleekSide.LEFT);
        offset2Field.onTypedSingle += onTypedOffset2Field;
        roadScrollBox.AddChild(offset2Field);
        concreteToggle = Glazier.Get().CreateToggle();
        concreteToggle.positionOffset_X = 200;
        concreteToggle.positionOffset_Y = LevelRoads.materials.Length * 70 + 160;
        concreteToggle.sizeOffset_X = 40;
        concreteToggle.sizeOffset_Y = 40;
        concreteToggle.addLabel(local.format("ConcreteToggleLabelText"), ESleekSide.RIGHT);
        concreteToggle.onToggled += onToggledConcreteToggle;
        roadScrollBox.AddChild(concreteToggle);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.positionOffset_X = -200;
        selectedBox.positionOffset_Y = 80;
        selectedBox.positionScale_X = 1f;
        selectedBox.sizeOffset_X = 200;
        selectedBox.sizeOffset_Y = 30;
        selectedBox.addLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        updateSelection();
        bakeRoadsButton = new SleekButtonIcon(bundle.load<Texture2D>("Roads"));
        bakeRoadsButton.positionOffset_X = -200;
        bakeRoadsButton.positionOffset_Y = -30;
        bakeRoadsButton.positionScale_X = 1f;
        bakeRoadsButton.positionScale_Y = 1f;
        bakeRoadsButton.sizeOffset_X = 200;
        bakeRoadsButton.sizeOffset_Y = 30;
        bakeRoadsButton.text = local.format("BakeRoadsButtonText");
        bakeRoadsButton.tooltip = local.format("BakeRoadsButtonTooltip");
        bakeRoadsButton.onClickedButton += onClickedBakeRoadsButton;
        container.AddChild(bakeRoadsButton);
        offsetField = Glazier.Get().CreateFloat32Field();
        offsetField.positionOffset_Y = -210;
        offsetField.positionScale_Y = 1f;
        offsetField.sizeOffset_X = 200;
        offsetField.sizeOffset_Y = 30;
        offsetField.addLabel(local.format("OffsetFieldLabelText"), ESleekSide.RIGHT);
        offsetField.onTypedSingle += onTypedOffsetField;
        container.AddChild(offsetField);
        offsetField.isVisible = false;
        loopToggle = Glazier.Get().CreateToggle();
        loopToggle.positionOffset_Y = -170;
        loopToggle.positionScale_Y = 1f;
        loopToggle.sizeOffset_X = 40;
        loopToggle.sizeOffset_Y = 40;
        loopToggle.addLabel(local.format("LoopToggleLabelText"), ESleekSide.RIGHT);
        loopToggle.onToggled += onToggledLoopToggle;
        container.AddChild(loopToggle);
        loopToggle.isVisible = false;
        ignoreTerrainToggle = Glazier.Get().CreateToggle();
        ignoreTerrainToggle.positionOffset_Y = -120;
        ignoreTerrainToggle.positionScale_Y = 1f;
        ignoreTerrainToggle.sizeOffset_X = 40;
        ignoreTerrainToggle.sizeOffset_Y = 40;
        ignoreTerrainToggle.addLabel(local.format("IgnoreTerrainToggleLabelText"), ESleekSide.RIGHT);
        ignoreTerrainToggle.onToggled += onToggledIgnoreTerrainToggle;
        container.AddChild(ignoreTerrainToggle);
        ignoreTerrainToggle.isVisible = false;
        modeButton = new SleekButtonState(new GUIContent(local.format("Mirror")), new GUIContent(local.format("Aligned")), new GUIContent(local.format("Free")));
        modeButton.positionOffset_Y = -70;
        modeButton.positionScale_Y = 1f;
        modeButton.sizeOffset_X = 200;
        modeButton.sizeOffset_Y = 30;
        modeButton.tooltip = local.format("ModeButtonTooltipText");
        modeButton.onSwappedState = onSwappedStateMode;
        container.AddChild(modeButton);
        modeButton.isVisible = false;
        roadIndexBox = Glazier.Get().CreateBox();
        roadIndexBox.positionOffset_Y = -30;
        roadIndexBox.positionScale_Y = 1f;
        roadIndexBox.sizeOffset_X = 200;
        roadIndexBox.sizeOffset_Y = 30;
        roadIndexBox.addLabel(local.format("RoadIndexLabelText"), ESleekSide.RIGHT);
        container.AddChild(roadIndexBox);
        roadIndexBox.isVisible = false;
        bundle.unload();
    }
}
