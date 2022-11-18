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
            widthSlider.state = flag.width;
            heightSlider.state = flag.height;
            navBox.text = flag.graph.graphIndex.ToString();
            difficultyGUIDField.text = flag.data.difficultyGUID;
            maxZombiesField.state = flag.data.maxZombies;
            maxBossZombiesField.state = flag.data.maxBossZombies;
            spawnZombiesToggle.state = flag.data.spawnZombies;
            hyperAgroToggle.state = flag.data.hyperAgro;
        }
        widthSlider.isVisible = flag != null;
        heightSlider.isVisible = flag != null;
        navBox.isVisible = flag != null;
        difficultyGUIDField.isVisible = flag != null;
        maxZombiesField.isVisible = flag != null;
        maxBossZombiesField.isVisible = flag != null;
        spawnZombiesToggle.isVisible = flag != null;
        hyperAgroToggle.isVisible = flag != null;
        bakeNavigationButton.isVisible = flag != null;
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        widthSlider = Glazier.Get().CreateSlider();
        widthSlider.positionOffset_X = -200;
        widthSlider.positionOffset_Y = 80;
        widthSlider.positionScale_X = 1f;
        widthSlider.sizeOffset_X = 200;
        widthSlider.sizeOffset_Y = 20;
        widthSlider.orientation = ESleekOrientation.HORIZONTAL;
        widthSlider.addLabel(local.format("Width_Label"), ESleekSide.LEFT);
        widthSlider.onDragged += onDraggedWidthSlider;
        container.AddChild(widthSlider);
        widthSlider.isVisible = false;
        heightSlider = Glazier.Get().CreateSlider();
        heightSlider.positionOffset_X = -200;
        heightSlider.positionOffset_Y = 110;
        heightSlider.positionScale_X = 1f;
        heightSlider.sizeOffset_X = 200;
        heightSlider.sizeOffset_Y = 20;
        heightSlider.orientation = ESleekOrientation.HORIZONTAL;
        heightSlider.addLabel(local.format("Height_Label"), ESleekSide.LEFT);
        heightSlider.onDragged += onDraggedHeightSlider;
        container.AddChild(heightSlider);
        heightSlider.isVisible = false;
        navBox = Glazier.Get().CreateBox();
        navBox.positionOffset_X = -200;
        navBox.positionOffset_Y = 140;
        navBox.positionScale_X = 1f;
        navBox.sizeOffset_X = 200;
        navBox.sizeOffset_Y = 30;
        navBox.addLabel(local.format("Nav_Label"), ESleekSide.LEFT);
        container.AddChild(navBox);
        navBox.isVisible = false;
        difficultyGUIDField = Glazier.Get().CreateStringField();
        difficultyGUIDField.positionOffset_X = -200;
        difficultyGUIDField.positionOffset_Y = 180;
        difficultyGUIDField.positionScale_X = 1f;
        difficultyGUIDField.sizeOffset_X = 200;
        difficultyGUIDField.sizeOffset_Y = 30;
        difficultyGUIDField.maxLength = 32;
        difficultyGUIDField.onTyped += onDifficultyGUIDFieldTyped;
        difficultyGUIDField.addLabel(local.format("Difficulty_GUID_Field_Label"), ESleekSide.LEFT);
        container.AddChild(difficultyGUIDField);
        difficultyGUIDField.isVisible = false;
        maxZombiesField = Glazier.Get().CreateUInt8Field();
        maxZombiesField.positionOffset_X = -200;
        maxZombiesField.positionOffset_Y = 220;
        maxZombiesField.positionScale_X = 1f;
        maxZombiesField.sizeOffset_X = 200;
        maxZombiesField.sizeOffset_Y = 30;
        maxZombiesField.onTypedByte += onMaxZombiesFieldTyped;
        maxZombiesField.addLabel(local.format("Max_Zombies_Field_Label"), ESleekSide.LEFT);
        container.AddChild(maxZombiesField);
        maxZombiesField.isVisible = false;
        maxBossZombiesField = Glazier.Get().CreateInt32Field();
        maxBossZombiesField.positionOffset_X = -200;
        maxBossZombiesField.positionOffset_Y = 260;
        maxBossZombiesField.positionScale_X = 1f;
        maxBossZombiesField.sizeOffset_X = 200;
        maxBossZombiesField.sizeOffset_Y = 30;
        maxBossZombiesField.onTypedInt += onMaxBossZombiesFieldTyped;
        maxBossZombiesField.addLabel(local.format("Max_Boss_Zombies_Field_Label"), ESleekSide.LEFT);
        container.AddChild(maxBossZombiesField);
        maxBossZombiesField.isVisible = false;
        spawnZombiesToggle = Glazier.Get().CreateToggle();
        spawnZombiesToggle.positionOffset_X = -200;
        spawnZombiesToggle.positionOffset_Y = 300;
        spawnZombiesToggle.positionScale_X = 1f;
        spawnZombiesToggle.sizeOffset_X = 40;
        spawnZombiesToggle.sizeOffset_Y = 40;
        spawnZombiesToggle.onToggled += onToggledSpawnZombiesToggle;
        spawnZombiesToggle.addLabel(local.format("Spawn_Zombies_Toggle_Label"), ESleekSide.RIGHT);
        container.AddChild(spawnZombiesToggle);
        spawnZombiesToggle.isVisible = false;
        hyperAgroToggle = Glazier.Get().CreateToggle();
        hyperAgroToggle.positionOffset_X = -200;
        hyperAgroToggle.positionOffset_Y = 350;
        hyperAgroToggle.positionScale_X = 1f;
        hyperAgroToggle.sizeOffset_X = 40;
        hyperAgroToggle.sizeOffset_Y = 40;
        hyperAgroToggle.onToggled += onToggledHyperAgroToggle;
        hyperAgroToggle.addLabel(local.format("Hyper_Agro_Label"), ESleekSide.RIGHT);
        container.AddChild(hyperAgroToggle);
        hyperAgroToggle.isVisible = false;
        bakeNavigationButton = new SleekButtonIcon(bundle.load<Texture2D>("Navigation"));
        bakeNavigationButton.positionOffset_X = -200;
        bakeNavigationButton.positionOffset_Y = -30;
        bakeNavigationButton.positionScale_X = 1f;
        bakeNavigationButton.positionScale_Y = 1f;
        bakeNavigationButton.sizeOffset_X = 200;
        bakeNavigationButton.sizeOffset_Y = 30;
        bakeNavigationButton.text = local.format("Bake_Navigation");
        bakeNavigationButton.tooltip = local.format("Bake_Navigation_Tooltip");
        bakeNavigationButton.onClickedButton += onClickedBakeNavigationButton;
        container.AddChild(bakeNavigationButton);
        bakeNavigationButton.isVisible = false;
        bundle.unload();
    }
}
