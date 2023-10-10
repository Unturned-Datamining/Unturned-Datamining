using UnityEngine;

namespace SDG.Unturned;

public class EditorEnvironmentNavigationUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekSlider widthSlider;

    private static ISleekSlider heightSlider;

    private static ISleekBox navBox;

    private static ISleekField difficultyGUIDField;

    private static ISleekUInt8Field maxZombiesField;

    private static ISleekInt32Field maxBossZombiesField;

    private static ISleekToggle spawnZombiesToggle;

    private static ISleekToggle hyperAgroToggle;

    private static SleekButtonIcon bakeNavigationButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorNavigation.isPathfinding = true;
            EditorUI.message(EEditorMessage.NAVIGATION);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorNavigation.isPathfinding = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    public static void updateSelection(Flag flag)
    {
        if (flag != null)
        {
            widthSlider.Value = flag.width;
            heightSlider.Value = flag.height;
            navBox.Text = flag.graph.graphIndex.ToString();
            difficultyGUIDField.Text = flag.data.difficultyGUID;
            maxZombiesField.Value = flag.data.maxZombies;
            maxBossZombiesField.Value = flag.data.maxBossZombies;
            spawnZombiesToggle.Value = flag.data.spawnZombies;
            hyperAgroToggle.Value = flag.data.hyperAgro;
        }
        widthSlider.IsVisible = flag != null;
        heightSlider.IsVisible = flag != null;
        navBox.IsVisible = flag != null;
        difficultyGUIDField.IsVisible = flag != null;
        maxZombiesField.IsVisible = flag != null;
        maxBossZombiesField.IsVisible = flag != null;
        spawnZombiesToggle.IsVisible = flag != null;
        hyperAgroToggle.IsVisible = flag != null;
        bakeNavigationButton.IsVisible = flag != null;
    }

    private static void onDraggedWidthSlider(ISleekSlider slider, float state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.width = state;
            EditorNavigation.flag.buildMesh();
        }
    }

    private static void onDraggedHeightSlider(ISleekSlider slider, float state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.height = state;
            EditorNavigation.flag.buildMesh();
        }
    }

    private static void onDifficultyGUIDFieldTyped(ISleekField field, string state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.data.difficultyGUID = state;
        }
    }

    private static void onMaxZombiesFieldTyped(ISleekUInt8Field field, byte state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.data.maxZombies = state;
        }
    }

    private static void onMaxBossZombiesFieldTyped(ISleekInt32Field field, int state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.data.maxBossZombies = state;
        }
    }

    private static void onToggledSpawnZombiesToggle(ISleekToggle toggle, bool state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.data.spawnZombies = state;
        }
    }

    private static void onToggledHyperAgroToggle(ISleekToggle toggle, bool state)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.data.hyperAgro = state;
        }
    }

    private static void onClickedBakeNavigationButton(ISleekElement button)
    {
        if (EditorNavigation.flag != null)
        {
            EditorNavigation.flag.bakeNavigation();
        }
    }

    public EditorEnvironmentNavigationUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironmentNavigation.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorEnvironmentNavigation/EditorEnvironmentNavigation.unity3d");
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
        widthSlider = Glazier.Get().CreateSlider();
        widthSlider.PositionOffset_X = -200f;
        widthSlider.PositionOffset_Y = 80f;
        widthSlider.PositionScale_X = 1f;
        widthSlider.SizeOffset_X = 200f;
        widthSlider.SizeOffset_Y = 20f;
        widthSlider.Orientation = ESleekOrientation.HORIZONTAL;
        widthSlider.AddLabel(local.format("Width_Label"), ESleekSide.LEFT);
        widthSlider.OnValueChanged += onDraggedWidthSlider;
        container.AddChild(widthSlider);
        widthSlider.IsVisible = false;
        heightSlider = Glazier.Get().CreateSlider();
        heightSlider.PositionOffset_X = -200f;
        heightSlider.PositionOffset_Y = 110f;
        heightSlider.PositionScale_X = 1f;
        heightSlider.SizeOffset_X = 200f;
        heightSlider.SizeOffset_Y = 20f;
        heightSlider.Orientation = ESleekOrientation.HORIZONTAL;
        heightSlider.AddLabel(local.format("Height_Label"), ESleekSide.LEFT);
        heightSlider.OnValueChanged += onDraggedHeightSlider;
        container.AddChild(heightSlider);
        heightSlider.IsVisible = false;
        navBox = Glazier.Get().CreateBox();
        navBox.PositionOffset_X = -200f;
        navBox.PositionOffset_Y = 140f;
        navBox.PositionScale_X = 1f;
        navBox.SizeOffset_X = 200f;
        navBox.SizeOffset_Y = 30f;
        navBox.AddLabel(local.format("Nav_Label"), ESleekSide.LEFT);
        container.AddChild(navBox);
        navBox.IsVisible = false;
        difficultyGUIDField = Glazier.Get().CreateStringField();
        difficultyGUIDField.PositionOffset_X = -200f;
        difficultyGUIDField.PositionOffset_Y = 180f;
        difficultyGUIDField.PositionScale_X = 1f;
        difficultyGUIDField.SizeOffset_X = 200f;
        difficultyGUIDField.SizeOffset_Y = 30f;
        difficultyGUIDField.MaxLength = 32;
        difficultyGUIDField.OnTextChanged += onDifficultyGUIDFieldTyped;
        difficultyGUIDField.AddLabel(local.format("Difficulty_GUID_Field_Label"), ESleekSide.LEFT);
        container.AddChild(difficultyGUIDField);
        difficultyGUIDField.IsVisible = false;
        maxZombiesField = Glazier.Get().CreateUInt8Field();
        maxZombiesField.PositionOffset_X = -200f;
        maxZombiesField.PositionOffset_Y = 220f;
        maxZombiesField.PositionScale_X = 1f;
        maxZombiesField.SizeOffset_X = 200f;
        maxZombiesField.SizeOffset_Y = 30f;
        maxZombiesField.OnValueChanged += onMaxZombiesFieldTyped;
        maxZombiesField.AddLabel(local.format("Max_Zombies_Field_Label"), ESleekSide.LEFT);
        container.AddChild(maxZombiesField);
        maxZombiesField.IsVisible = false;
        maxBossZombiesField = Glazier.Get().CreateInt32Field();
        maxBossZombiesField.PositionOffset_X = -200f;
        maxBossZombiesField.PositionOffset_Y = 260f;
        maxBossZombiesField.PositionScale_X = 1f;
        maxBossZombiesField.SizeOffset_X = 200f;
        maxBossZombiesField.SizeOffset_Y = 30f;
        maxBossZombiesField.OnValueChanged += onMaxBossZombiesFieldTyped;
        maxBossZombiesField.AddLabel(local.format("Max_Boss_Zombies_Field_Label"), ESleekSide.LEFT);
        container.AddChild(maxBossZombiesField);
        maxBossZombiesField.IsVisible = false;
        spawnZombiesToggle = Glazier.Get().CreateToggle();
        spawnZombiesToggle.PositionOffset_X = -200f;
        spawnZombiesToggle.PositionOffset_Y = 300f;
        spawnZombiesToggle.PositionScale_X = 1f;
        spawnZombiesToggle.SizeOffset_X = 40f;
        spawnZombiesToggle.SizeOffset_Y = 40f;
        spawnZombiesToggle.OnValueChanged += onToggledSpawnZombiesToggle;
        spawnZombiesToggle.AddLabel(local.format("Spawn_Zombies_Toggle_Label"), ESleekSide.RIGHT);
        container.AddChild(spawnZombiesToggle);
        spawnZombiesToggle.IsVisible = false;
        hyperAgroToggle = Glazier.Get().CreateToggle();
        hyperAgroToggle.PositionOffset_X = -200f;
        hyperAgroToggle.PositionOffset_Y = 350f;
        hyperAgroToggle.PositionScale_X = 1f;
        hyperAgroToggle.SizeOffset_X = 40f;
        hyperAgroToggle.SizeOffset_Y = 40f;
        hyperAgroToggle.OnValueChanged += onToggledHyperAgroToggle;
        hyperAgroToggle.AddLabel(local.format("Hyper_Agro_Label"), ESleekSide.RIGHT);
        container.AddChild(hyperAgroToggle);
        hyperAgroToggle.IsVisible = false;
        bakeNavigationButton = new SleekButtonIcon(bundle.load<Texture2D>("Navigation"));
        bakeNavigationButton.PositionOffset_X = -200f;
        bakeNavigationButton.PositionOffset_Y = -30f;
        bakeNavigationButton.PositionScale_X = 1f;
        bakeNavigationButton.PositionScale_Y = 1f;
        bakeNavigationButton.SizeOffset_X = 200f;
        bakeNavigationButton.SizeOffset_Y = 30f;
        bakeNavigationButton.text = local.format("Bake_Navigation");
        bakeNavigationButton.tooltip = local.format("Bake_Navigation_Tooltip");
        bakeNavigationButton.onClickedButton += onClickedBakeNavigationButton;
        container.AddChild(bakeNavigationButton);
        bakeNavigationButton.IsVisible = false;
        bundle.unload();
    }
}
