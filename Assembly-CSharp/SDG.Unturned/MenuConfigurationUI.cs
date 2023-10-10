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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        optionsButton = new SleekButtonIcon(bundle.load<Texture2D>("Options"));
        optionsButton.PositionOffset_X = -100f;
        optionsButton.PositionOffset_Y = -145f;
        optionsButton.PositionScale_X = 0.5f;
        optionsButton.PositionScale_Y = 0.5f;
        optionsButton.SizeOffset_X = 200f;
        optionsButton.SizeOffset_Y = 50f;
        optionsButton.text = local.format("Options_Button_Text");
        optionsButton.tooltip = local.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.fontSize = ESleekFontSize.Medium;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(bundle.load<Texture2D>("Display"));
        displayButton.PositionOffset_X = -100f;
        displayButton.PositionOffset_Y = -85f;
        displayButton.PositionScale_X = 0.5f;
        displayButton.PositionScale_Y = 0.5f;
        displayButton.SizeOffset_X = 200f;
        displayButton.SizeOffset_Y = 50f;
        displayButton.text = local.format("Display_Button_Text");
        displayButton.tooltip = local.format("Display_Button_Tooltip");
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        displayButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(bundle.load<Texture2D>("Graphics"));
        graphicsButton.PositionOffset_X = -100f;
        graphicsButton.PositionOffset_Y = -25f;
        graphicsButton.PositionScale_X = 0.5f;
        graphicsButton.PositionScale_Y = 0.5f;
        graphicsButton.SizeOffset_X = 200f;
        graphicsButton.SizeOffset_Y = 50f;
        graphicsButton.text = local.format("Graphics_Button_Text");
        graphicsButton.tooltip = local.format("Graphics_Button_Tooltip");
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(bundle.load<Texture2D>("Controls"));
        controlsButton.PositionOffset_X = -100f;
        controlsButton.PositionOffset_Y = 35f;
        controlsButton.PositionScale_X = 0.5f;
        controlsButton.PositionScale_Y = 0.5f;
        controlsButton.SizeOffset_X = 200f;
        controlsButton.SizeOffset_Y = 50f;
        controlsButton.text = local.format("Controls_Button_Text");
        controlsButton.tooltip = local.format("Controls_Button_Tooltip");
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(controlsButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_X = -100f;
        backButton.PositionOffset_Y = 95f;
        backButton.PositionScale_X = 0.5f;
        backButton.PositionScale_Y = 0.5f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
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
