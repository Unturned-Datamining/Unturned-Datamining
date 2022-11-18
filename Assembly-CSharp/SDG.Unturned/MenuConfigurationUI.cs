using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon optionsButton;

    private static SleekButtonIcon displayButton;

    private static SleekButtonIcon graphicsButton;

    private static SleekButtonIcon controlsButton;

    private static SleekButtonIcon backButton;

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
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedOptionsButton(ISleekElement button)
    {
        MenuConfigurationOptionsUI.open();
        close();
    }

    private static void onClickedDisplayButton(ISleekElement button)
    {
        MenuConfigurationDisplayUI.open();
        close();
    }

    private static void onClickedGraphicsButton(ISleekElement button)
    {
        MenuConfigurationGraphicsUI.open();
        close();
    }

    private static void onClickedControlsButton(ISleekElement button)
    {
        MenuConfigurationControlsUI.open();
        close();
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuDashboardUI.open();
        MenuTitleUI.open();
        close();
    }

    public MenuConfigurationUI()
    {
        Local local = Localization.read("/Menu/Configuration/MenuConfiguration.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Configuration/MenuConfiguration/MenuConfiguration.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = -1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        optionsButton = new SleekButtonIcon(bundle.load<Texture2D>("Options"));
        optionsButton.positionOffset_X = -100;
        optionsButton.positionOffset_Y = -145;
        optionsButton.positionScale_X = 0.5f;
        optionsButton.positionScale_Y = 0.5f;
        optionsButton.sizeOffset_X = 200;
        optionsButton.sizeOffset_Y = 50;
        optionsButton.text = local.format("Options_Button_Text");
        optionsButton.tooltip = local.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.fontSize = ESleekFontSize.Medium;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(bundle.load<Texture2D>("Display"));
        displayButton.positionOffset_X = -100;
        displayButton.positionOffset_Y = -85;
        displayButton.positionScale_X = 0.5f;
        displayButton.positionScale_Y = 0.5f;
        displayButton.sizeOffset_X = 200;
        displayButton.sizeOffset_Y = 50;
        displayButton.text = local.format("Display_Button_Text");
        displayButton.tooltip = local.format("Display_Button_Tooltip");
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        displayButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(bundle.load<Texture2D>("Graphics"));
        graphicsButton.positionOffset_X = -100;
        graphicsButton.positionOffset_Y = -25;
        graphicsButton.positionScale_X = 0.5f;
        graphicsButton.positionScale_Y = 0.5f;
        graphicsButton.sizeOffset_X = 200;
        graphicsButton.sizeOffset_Y = 50;
        graphicsButton.text = local.format("Graphics_Button_Text");
        graphicsButton.tooltip = local.format("Graphics_Button_Tooltip");
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(bundle.load<Texture2D>("Controls"));
        controlsButton.positionOffset_X = -100;
        controlsButton.positionOffset_Y = 35;
        controlsButton.positionScale_X = 0.5f;
        controlsButton.positionScale_Y = 0.5f;
        controlsButton.sizeOffset_X = 200;
        controlsButton.sizeOffset_Y = 50;
        controlsButton.text = local.format("Controls_Button_Text");
        controlsButton.tooltip = local.format("Controls_Button_Tooltip");
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(controlsButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_X = -100;
        backButton.positionOffset_Y = 95;
        backButton.positionScale_X = 0.5f;
        backButton.positionScale_Y = 0.5f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        bundle.unload();
        new MenuConfigurationOptionsUI();
        new MenuConfigurationDisplayUI();
        new MenuConfigurationGraphicsUI();
        new MenuConfigurationControlsUI();
    }
}
