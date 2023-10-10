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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        lightingButton = new SleekButtonIcon(bundle.load<Texture2D>("Lighting"));
        lightingButton.PositionOffset_Y = 40f;
        lightingButton.SizeOffset_X = -5f;
        lightingButton.SizeOffset_Y = 30f;
        lightingButton.SizeScale_X = 0.25f;
        lightingButton.text = local.format("LightingButtonText");
        lightingButton.tooltip = local.format("LightingButtonTooltip");
        lightingButton.onClickedButton += onClickedLightingButton;
        container.AddChild(lightingButton);
        roadsButton = new SleekButtonIcon(bundle.load<Texture2D>("Roads"));
        roadsButton.PositionOffset_X = 5f;
        roadsButton.PositionOffset_Y = 40f;
        roadsButton.PositionScale_X = 0.25f;
        roadsButton.SizeOffset_X = -10f;
        roadsButton.SizeOffset_Y = 30f;
        roadsButton.SizeScale_X = 0.25f;
        roadsButton.text = local.format("RoadsButtonText");
        roadsButton.tooltip = local.format("RoadsButtonTooltip");
        roadsButton.onClickedButton += onClickedRoadsButton;
        container.AddChild(roadsButton);
        navigationButton = new SleekButtonIcon(bundle.load<Texture2D>("Navigation"));
        navigationButton.PositionOffset_X = 5f;
        navigationButton.PositionOffset_Y = 40f;
        navigationButton.PositionScale_X = 0.5f;
        navigationButton.SizeOffset_X = -10f;
        navigationButton.SizeOffset_Y = 30f;
        navigationButton.SizeScale_X = 0.25f;
        navigationButton.text = local.format("NavigationButtonText");
        navigationButton.tooltip = local.format("NavigationButtonTooltip");
        navigationButton.onClickedButton += onClickedNavigationButton;
        container.AddChild(navigationButton);
        nodesButton = new SleekButtonIcon(bundle.load<Texture2D>("Nodes"));
        nodesButton.PositionOffset_X = 5f;
        nodesButton.PositionOffset_Y = 40f;
        nodesButton.PositionScale_X = 0.75f;
        nodesButton.SizeOffset_X = -5f;
        nodesButton.SizeOffset_Y = 30f;
        nodesButton.SizeScale_X = 0.25f;
        nodesButton.text = local.format("NodesButtonText");
        nodesButton.tooltip = local.format("NodesButtonTooltip");
        nodesButton.onClickedButton += onClickedNodesButton;
        container.AddChild(nodesButton);
        bundle.unload();
        new EditorEnvironmentLightingUI();
        new EditorEnvironmentRoadsUI();
        new EditorEnvironmentNavigationUI();
        nodesUI = new EditorEnvironmentNodesUI();
        nodesUI.PositionOffset_X = 10f;
        nodesUI.PositionOffset_Y = 90f;
        nodesUI.PositionScale_X = 1f;
        nodesUI.SizeOffset_X = -20f;
        nodesUI.SizeOffset_Y = -100f;
        nodesUI.SizeScale_X = 1f;
        nodesUI.SizeScale_Y = 1f;
        EditorUI.window.AddChild(nodesUI);
    }
}
