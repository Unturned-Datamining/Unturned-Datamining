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
            offsetField.Value = joint.offset;
            loopToggle.Value = road.isLoop;
            ignoreTerrainToggle.Value = joint.ignoreTerrain;
            modeButton.state = (int)joint.mode;
            roadIndexBox.Text = LevelRoads.getRoadIndex(road).ToString();
        }
        offsetField.IsVisible = road != null;
        loopToggle.IsVisible = road != null;
        ignoreTerrainToggle.IsVisible = road != null;
        modeButton.IsVisible = road != null;
        roadIndexBox.IsVisible = road != null;
    }

    private static void updateSelection()
    {
        if (EditorRoads.selected < LevelRoads.materials.Length)
        {
            RoadMaterial roadMaterial = LevelRoads.materials[EditorRoads.selected];
            selectedBox.Text = roadMaterial.material.mainTexture.name;
            widthField.Value = roadMaterial.width;
            heightField.Value = roadMaterial.height;
            depthField.Value = roadMaterial.depth;
            offset2Field.Value = roadMaterial.offset;
            concreteToggle.Value = roadMaterial.isConcrete;
        }
    }

    private static void onClickedRoadButton(ISleekElement button)
    {
        EditorRoads.selected = (byte)(button.Parent.PositionOffset_Y / 70f);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        roadScrollBox = Glazier.Get().CreateScrollView();
        roadScrollBox.PositionOffset_X = -400f;
        roadScrollBox.PositionOffset_Y = 120f;
        roadScrollBox.PositionScale_X = 1f;
        roadScrollBox.SizeOffset_X = 400f;
        roadScrollBox.SizeOffset_Y = -160f;
        roadScrollBox.SizeScale_Y = 1f;
        roadScrollBox.ScaleContentToWidth = true;
        roadScrollBox.ContentSizeOffset = new Vector2(0f, LevelRoads.materials.Length * 70 + 200);
        container.AddChild(roadScrollBox);
        for (int i = 0; i < LevelRoads.materials.Length; i++)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = 200f;
            sleekImage.PositionOffset_Y = i * 70;
            sleekImage.SizeOffset_X = 64f;
            sleekImage.SizeOffset_Y = 64f;
            sleekImage.Texture = LevelRoads.materials[i].material.mainTexture;
            roadScrollBox.AddChild(sleekImage);
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 70f;
            sleekButton.SizeOffset_X = 100f;
            sleekButton.SizeOffset_Y = 64f;
            sleekButton.Text = LevelRoads.materials[i].material.mainTexture.name;
            sleekButton.OnClicked += onClickedRoadButton;
            sleekImage.AddChild(sleekButton);
        }
        widthField = Glazier.Get().CreateFloat32Field();
        widthField.PositionOffset_X = 200f;
        widthField.PositionOffset_Y = LevelRoads.materials.Length * 70;
        widthField.SizeOffset_X = 170f;
        widthField.SizeOffset_Y = 30f;
        widthField.AddLabel(local.format("WidthFieldLabelText"), ESleekSide.LEFT);
        widthField.OnValueChanged += onTypedWidthField;
        roadScrollBox.AddChild(widthField);
        heightField = Glazier.Get().CreateFloat32Field();
        heightField.PositionOffset_X = 200f;
        heightField.PositionOffset_Y = LevelRoads.materials.Length * 70 + 40;
        heightField.SizeOffset_X = 170f;
        heightField.SizeOffset_Y = 30f;
        heightField.AddLabel(local.format("HeightFieldLabelText"), ESleekSide.LEFT);
        heightField.OnValueChanged += onTypedHeightField;
        roadScrollBox.AddChild(heightField);
        depthField = Glazier.Get().CreateFloat32Field();
        depthField.PositionOffset_X = 200f;
        depthField.PositionOffset_Y = LevelRoads.materials.Length * 70 + 80;
        depthField.SizeOffset_X = 170f;
        depthField.SizeOffset_Y = 30f;
        depthField.AddLabel(local.format("DepthFieldLabelText"), ESleekSide.LEFT);
        depthField.OnValueChanged += onTypedDepthField;
        roadScrollBox.AddChild(depthField);
        offset2Field = Glazier.Get().CreateFloat32Field();
        offset2Field.PositionOffset_X = 200f;
        offset2Field.PositionOffset_Y = LevelRoads.materials.Length * 70 + 120;
        offset2Field.SizeOffset_X = 170f;
        offset2Field.SizeOffset_Y = 30f;
        offset2Field.AddLabel(local.format("OffsetFieldLabelText"), ESleekSide.LEFT);
        offset2Field.OnValueChanged += onTypedOffset2Field;
        roadScrollBox.AddChild(offset2Field);
        concreteToggle = Glazier.Get().CreateToggle();
        concreteToggle.PositionOffset_X = 200f;
        concreteToggle.PositionOffset_Y = LevelRoads.materials.Length * 70 + 160;
        concreteToggle.SizeOffset_X = 40f;
        concreteToggle.SizeOffset_Y = 40f;
        concreteToggle.AddLabel(local.format("ConcreteToggleLabelText"), ESleekSide.RIGHT);
        concreteToggle.OnValueChanged += onToggledConcreteToggle;
        roadScrollBox.AddChild(concreteToggle);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -200f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 200f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        updateSelection();
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
        offsetField.PositionOffset_Y = -210f;
        offsetField.PositionScale_Y = 1f;
        offsetField.SizeOffset_X = 200f;
        offsetField.SizeOffset_Y = 30f;
        offsetField.AddLabel(local.format("OffsetFieldLabelText"), ESleekSide.RIGHT);
        offsetField.OnValueChanged += onTypedOffsetField;
        container.AddChild(offsetField);
        offsetField.IsVisible = false;
        loopToggle = Glazier.Get().CreateToggle();
        loopToggle.PositionOffset_Y = -170f;
        loopToggle.PositionScale_Y = 1f;
        loopToggle.SizeOffset_X = 40f;
        loopToggle.SizeOffset_Y = 40f;
        loopToggle.AddLabel(local.format("LoopToggleLabelText"), ESleekSide.RIGHT);
        loopToggle.OnValueChanged += onToggledLoopToggle;
        container.AddChild(loopToggle);
        loopToggle.IsVisible = false;
        ignoreTerrainToggle = Glazier.Get().CreateToggle();
        ignoreTerrainToggle.PositionOffset_Y = -120f;
        ignoreTerrainToggle.PositionScale_Y = 1f;
        ignoreTerrainToggle.SizeOffset_X = 40f;
        ignoreTerrainToggle.SizeOffset_Y = 40f;
        ignoreTerrainToggle.AddLabel(local.format("IgnoreTerrainToggleLabelText"), ESleekSide.RIGHT);
        ignoreTerrainToggle.OnValueChanged += onToggledIgnoreTerrainToggle;
        container.AddChild(ignoreTerrainToggle);
        ignoreTerrainToggle.IsVisible = false;
        modeButton = new SleekButtonState(new GUIContent(local.format("Mirror")), new GUIContent(local.format("Aligned")), new GUIContent(local.format("Free")));
        modeButton.PositionOffset_Y = -70f;
        modeButton.PositionScale_Y = 1f;
        modeButton.SizeOffset_X = 200f;
        modeButton.SizeOffset_Y = 30f;
        modeButton.tooltip = local.format("ModeButtonTooltipText");
        modeButton.onSwappedState = onSwappedStateMode;
        container.AddChild(modeButton);
        modeButton.IsVisible = false;
        roadIndexBox = Glazier.Get().CreateBox();
        roadIndexBox.PositionOffset_Y = -30f;
        roadIndexBox.PositionScale_Y = 1f;
        roadIndexBox.SizeOffset_X = 200f;
        roadIndexBox.SizeOffset_Y = 30f;
        roadIndexBox.AddLabel(local.format("RoadIndexLabelText"), ESleekSide.RIGHT);
        container.AddChild(roadIndexBox);
        roadIndexBox.IsVisible = false;
        bundle.unload();
    }
}
