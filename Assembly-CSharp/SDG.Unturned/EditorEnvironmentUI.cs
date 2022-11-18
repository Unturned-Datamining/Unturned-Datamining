using UnityEngine;

namespace SDG.Unturned;

public class EditorEnvironmentUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon lightingButton;

    private static SleekButtonIcon roadsButton;

    private static SleekButtonIcon navigationButton;

    private static SleekButtonIcon nodesButton;

    private static EditorEnvironmentNodesUI nodesUI;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorEnvironmentLightingUI.close();
            EditorEnvironmentRoadsUI.close();
            EditorEnvironmentNavigationUI.close();
            nodesUI.Close();
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onClickedLightingButton(ISleekElement button)
    {
        EditorEnvironmentRoadsUI.close();
        EditorEnvironmentNavigationUI.close();
        nodesUI.Close();
        EditorEnvironmentLightingUI.open();
    }

    private static void onClickedRoadsButton(ISleekElement button)
    {
        EditorEnvironmentLightingUI.close();
        EditorEnvironmentNavigationUI.close();
        nodesUI.Close();
        EditorEnvironmentRoadsUI.open();
    }

    private static void onClickedNavigationButton(ISleekElement button)
    {
        EditorEnvironmentLightingUI.close();
        EditorEnvironmentRoadsUI.close();
        nodesUI.Close();
        EditorEnvironmentNavigationUI.open();
    }

    private static void onClickedNodesButton(ISleekElement button)
    {
        EditorEnvironmentLightingUI.close();
        EditorEnvironmentRoadsUI.close();
        EditorEnvironmentNavigationUI.close();
        nodesUI.Open();
    }

    public void OnDestroy()
    {
        nodesUI.OnDestroy();
    }

    public EditorEnvironmentUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironment.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorEnvironment/EditorEnvironment.unity3d");
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
        lightingButton = new SleekButtonIcon(bundle.load<Texture2D>("Lighting"));
        lightingButton.positionOffset_Y = 40;
        lightingButton.sizeOffset_X = -5;
        lightingButton.sizeOffset_Y = 30;
        lightingButton.sizeScale_X = 0.25f;
        lightingButton.text = local.format("LightingButtonText");
        lightingButton.tooltip = local.format("LightingButtonTooltip");
        lightingButton.onClickedButton += onClickedLightingButton;
        container.AddChild(lightingButton);
        roadsButton = new SleekButtonIcon(bundle.load<Texture2D>("Roads"));
        roadsButton.positionOffset_X = 5;
        roadsButton.positionOffset_Y = 40;
        roadsButton.positionScale_X = 0.25f;
        roadsButton.sizeOffset_X = -10;
        roadsButton.sizeOffset_Y = 30;
        roadsButton.sizeScale_X = 0.25f;
        roadsButton.text = local.format("RoadsButtonText");
        roadsButton.tooltip = local.format("RoadsButtonTooltip");
        roadsButton.onClickedButton += onClickedRoadsButton;
        container.AddChild(roadsButton);
        navigationButton = new SleekButtonIcon(bundle.load<Texture2D>("Navigation"));
        navigationButton.positionOffset_X = 5;
        navigationButton.positionOffset_Y = 40;
        navigationButton.positionScale_X = 0.5f;
        navigationButton.sizeOffset_X = -10;
        navigationButton.sizeOffset_Y = 30;
        navigationButton.sizeScale_X = 0.25f;
        navigationButton.text = local.format("NavigationButtonText");
        navigationButton.tooltip = local.format("NavigationButtonTooltip");
        navigationButton.onClickedButton += onClickedNavigationButton;
        container.AddChild(navigationButton);
        nodesButton = new SleekButtonIcon(bundle.load<Texture2D>("Nodes"));
        nodesButton.positionOffset_X = 5;
        nodesButton.positionOffset_Y = 40;
        nodesButton.positionScale_X = 0.75f;
        nodesButton.sizeOffset_X = -5;
        nodesButton.sizeOffset_Y = 30;
        nodesButton.sizeScale_X = 0.25f;
        nodesButton.text = local.format("NodesButtonText");
        nodesButton.tooltip = local.format("NodesButtonTooltip");
        nodesButton.onClickedButton += onClickedNodesButton;
        container.AddChild(nodesButton);
        bundle.unload();
        new EditorEnvironmentLightingUI();
        new EditorEnvironmentRoadsUI();
        new EditorEnvironmentNavigationUI();
        nodesUI = new EditorEnvironmentNodesUI();
        nodesUI.positionOffset_X = 10;
        nodesUI.positionOffset_Y = 90;
        nodesUI.positionScale_X = 1f;
        nodesUI.sizeOffset_X = -20;
        nodesUI.sizeOffset_Y = -100;
        nodesUI.sizeScale_X = 1f;
        nodesUI.sizeScale_Y = 1f;
        EditorUI.window.AddChild(nodesUI);
    }
}
