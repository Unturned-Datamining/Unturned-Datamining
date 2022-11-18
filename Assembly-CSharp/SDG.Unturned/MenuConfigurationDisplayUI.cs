using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationDisplayUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekScrollView resolutionsBox;

    private static ISleekButton[] buttons;

    private static SleekButtonState fullscreenMode;

    private static ISleekToggle bufferToggle;

    private static ISleekFloat32Field userInterfaceScaleField;

    private static ISleekToggle targetFrameRateToggle;

    private static ISleekUInt32Field targetFrameRateField;

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
            MenuSettings.SaveGraphicsIfLoaded();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onClickedResolutionButton(ISleekElement button)
    {
        int num = (button.positionOffset_Y - 230) / 40;
        GraphicsSettings.resolution = new GraphicsSettingsResolution(ScreenEx.GetRecommendedResolutions()[num]);
        GraphicsSettings.apply("changed resolution");
    }

    private static void onSwappedFullscreenState(SleekButtonState button, int index)
    {
        GraphicsSettings.fullscreenMode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen, 
            2 => FullScreenMode.Windowed, 
            _ => FullScreenMode.FullScreenWindow, 
        };
        GraphicsSettings.apply("changed fullscreen mode");
    }

    private static void onToggledBufferToggle(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.buffer = state;
        GraphicsSettings.apply("changed vsync");
    }

    private static void onTypedUserInterfaceScale(ISleekFloat32Field field, float state)
    {
        GraphicsSettings.userInterfaceScale = Mathf.Clamp(state, 0.5f, 2f);
        GraphicsSettings.apply("changed UI scale");
    }

    private static void OnToggledTargetFrameRate(ISleekToggle toggle, bool state)
    {
        GraphicsSettings.UseTargetFrameRate = state;
        GraphicsSettings.apply("changed use target frame rate");
        targetFrameRateField.isVisible = state;
    }

    private static void OnTypedTargetFrameRate(ISleekUInt32Field field, uint state)
    {
        GraphicsSettings.TargetFrameRate = (int)state;
        GraphicsSettings.apply("changed target frame rate");
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        if (Player.player != null)
        {
            PlayerPauseUI.open();
        }
        else if (Level.isEditor)
        {
            EditorPauseUI.open();
        }
        else
        {
            MenuConfigurationUI.open();
        }
        close();
    }

    public MenuConfigurationDisplayUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationDisplay.dat");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        if (Provider.isConnected)
        {
            PlayerUI.container.AddChild(container);
        }
        else if (Level.isEditor)
        {
            EditorUI.window.AddChild(container);
        }
        else
        {
            MenuUI.container.AddChild(container);
        }
        active = false;
        Resolution[] recommendedResolutions = ScreenEx.GetRecommendedResolutions();
        resolutionsBox = Glazier.Get().CreateScrollView();
        resolutionsBox.positionOffset_X = -200;
        resolutionsBox.positionOffset_Y = 100;
        resolutionsBox.positionScale_X = 0.5f;
        resolutionsBox.sizeOffset_X = 430;
        resolutionsBox.sizeOffset_Y = -200;
        resolutionsBox.sizeScale_Y = 1f;
        resolutionsBox.scaleContentToWidth = true;
        resolutionsBox.contentSizeOffset = new Vector2(0f, 230 + recommendedResolutions.Length * 40 - 10);
        container.AddChild(resolutionsBox);
        buttons = new ISleekButton[recommendedResolutions.Length];
        for (byte b = 0; b < buttons.Length; b = (byte)(b + 1))
        {
            Resolution resolution = recommendedResolutions[b];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_Y = 230 + b * 40;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.sizeScale_X = 1f;
            sleekButton.onClickedButton += onClickedResolutionButton;
            sleekButton.text = resolution.width + " x " + resolution.height + " [" + resolution.refreshRate + "Hz]";
            resolutionsBox.AddChild(sleekButton);
            buttons[b] = sleekButton;
        }
        fullscreenMode = new SleekButtonState(new GUIContent(localization.format("Fullscreen_Mode_Exclusive")), new GUIContent(localization.format("Fullscreen_Mode_Borderless")), new GUIContent(localization.format("Fullscreen_Mode_Windowed")));
        fullscreenMode.sizeOffset_X = 200;
        fullscreenMode.sizeOffset_Y = 30;
        fullscreenMode.addLabel(localization.format("Fullscreen_Mode_Label"), ESleekSide.RIGHT);
        fullscreenMode.tooltip = localization.format("Fullscreen_Mode_Tooltip");
        switch (GraphicsSettings.fullscreenMode)
        {
        case FullScreenMode.ExclusiveFullScreen:
            fullscreenMode.state = 0;
            break;
        default:
            fullscreenMode.state = 1;
            break;
        case FullScreenMode.Windowed:
            fullscreenMode.state = 2;
            break;
        }
        fullscreenMode.onSwappedState = onSwappedFullscreenState;
        resolutionsBox.AddChild(fullscreenMode);
        bufferToggle = Glazier.Get().CreateToggle();
        bufferToggle.positionOffset_Y = 50;
        bufferToggle.sizeOffset_X = 40;
        bufferToggle.sizeOffset_Y = 40;
        bufferToggle.addLabel(localization.format("Buffer_Toggle_Label"), ESleekSide.RIGHT);
        bufferToggle.state = GraphicsSettings.buffer;
        bufferToggle.onToggled += onToggledBufferToggle;
        resolutionsBox.AddChild(bufferToggle);
        userInterfaceScaleField = Glazier.Get().CreateFloat32Field();
        userInterfaceScaleField.positionOffset_Y = 100;
        userInterfaceScaleField.sizeOffset_X = 200;
        userInterfaceScaleField.sizeOffset_Y = 30;
        userInterfaceScaleField.addLabel(localization.format("User_Interface_Scale_Field_Label"), ESleekSide.RIGHT);
        userInterfaceScaleField.state = GraphicsSettings.userInterfaceScale;
        userInterfaceScaleField.onEnteredSingle += onTypedUserInterfaceScale;
        resolutionsBox.AddChild(userInterfaceScaleField);
        targetFrameRateToggle = Glazier.Get().CreateToggle();
        targetFrameRateToggle.positionOffset_Y = 140;
        targetFrameRateToggle.sizeOffset_X = 40;
        targetFrameRateToggle.sizeOffset_Y = 40;
        targetFrameRateToggle.addLabel(localization.format("UseTargetFrameRate_Toggle_Label"), ESleekSide.RIGHT);
        targetFrameRateToggle.state = GraphicsSettings.UseTargetFrameRate;
        targetFrameRateToggle.onToggled += OnToggledTargetFrameRate;
        resolutionsBox.AddChild(targetFrameRateToggle);
        targetFrameRateField = Glazier.Get().CreateUInt32Field();
        targetFrameRateField.positionOffset_Y = 180;
        targetFrameRateField.sizeOffset_X = 200;
        targetFrameRateField.sizeOffset_Y = 30;
        targetFrameRateField.addLabel(localization.format("TargetFrameRate_Field_Label"), ESleekSide.RIGHT);
        targetFrameRateField.state = (uint)GraphicsSettings.TargetFrameRate;
        targetFrameRateField.onTypedUInt32 += OnTypedTargetFrameRate;
        targetFrameRateField.isVisible = GraphicsSettings.UseTargetFrameRate;
        resolutionsBox.AddChild(targetFrameRateField);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
