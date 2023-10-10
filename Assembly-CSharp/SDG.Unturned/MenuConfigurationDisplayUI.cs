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
        int num = Mathf.FloorToInt((button.PositionOffset_Y - 230f) / 40f);
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
        targetFrameRateField.IsVisible = state;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
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
        resolutionsBox.PositionOffset_X = -200f;
        resolutionsBox.PositionOffset_Y = 100f;
        resolutionsBox.PositionScale_X = 0.5f;
        resolutionsBox.SizeOffset_X = 430f;
        resolutionsBox.SizeOffset_Y = -200f;
        resolutionsBox.SizeScale_Y = 1f;
        resolutionsBox.ScaleContentToWidth = true;
        resolutionsBox.ContentSizeOffset = new Vector2(0f, 230 + recommendedResolutions.Length * 40 - 10);
        container.AddChild(resolutionsBox);
        buttons = new ISleekButton[recommendedResolutions.Length];
        for (byte b = 0; b < buttons.Length; b = (byte)(b + 1))
        {
            Resolution resolution = recommendedResolutions[b];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_Y = 230 + b * 40;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.SizeScale_X = 1f;
            sleekButton.OnClicked += onClickedResolutionButton;
            sleekButton.Text = resolution.width + " x " + resolution.height + " [" + resolution.refreshRate + "Hz]";
            resolutionsBox.AddChild(sleekButton);
            buttons[b] = sleekButton;
        }
        fullscreenMode = new SleekButtonState(new GUIContent(localization.format("Fullscreen_Mode_Exclusive")), new GUIContent(localization.format("Fullscreen_Mode_Borderless")), new GUIContent(localization.format("Fullscreen_Mode_Windowed")));
        fullscreenMode.SizeOffset_X = 200f;
        fullscreenMode.SizeOffset_Y = 30f;
        fullscreenMode.AddLabel(localization.format("Fullscreen_Mode_Label"), ESleekSide.RIGHT);
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
        bufferToggle.PositionOffset_Y = 50f;
        bufferToggle.SizeOffset_X = 40f;
        bufferToggle.SizeOffset_Y = 40f;
        bufferToggle.AddLabel(localization.format("Buffer_Toggle_Label"), ESleekSide.RIGHT);
        bufferToggle.Value = GraphicsSettings.buffer;
        bufferToggle.OnValueChanged += onToggledBufferToggle;
        resolutionsBox.AddChild(bufferToggle);
        userInterfaceScaleField = Glazier.Get().CreateFloat32Field();
        userInterfaceScaleField.PositionOffset_Y = 100f;
        userInterfaceScaleField.SizeOffset_X = 200f;
        userInterfaceScaleField.SizeOffset_Y = 30f;
        userInterfaceScaleField.AddLabel(localization.format("User_Interface_Scale_Field_Label"), ESleekSide.RIGHT);
        userInterfaceScaleField.Value = GraphicsSettings.userInterfaceScale;
        userInterfaceScaleField.OnValueSubmitted += onTypedUserInterfaceScale;
        resolutionsBox.AddChild(userInterfaceScaleField);
        targetFrameRateToggle = Glazier.Get().CreateToggle();
        targetFrameRateToggle.PositionOffset_Y = 140f;
        targetFrameRateToggle.SizeOffset_X = 40f;
        targetFrameRateToggle.SizeOffset_Y = 40f;
        targetFrameRateToggle.AddLabel(localization.format("UseTargetFrameRate_Toggle_Label"), ESleekSide.RIGHT);
        targetFrameRateToggle.Value = GraphicsSettings.UseTargetFrameRate;
        targetFrameRateToggle.OnValueChanged += OnToggledTargetFrameRate;
        resolutionsBox.AddChild(targetFrameRateToggle);
        targetFrameRateField = Glazier.Get().CreateUInt32Field();
        targetFrameRateField.PositionOffset_Y = 180f;
        targetFrameRateField.SizeOffset_X = 200f;
        targetFrameRateField.SizeOffset_Y = 30f;
        targetFrameRateField.AddLabel(localization.format("TargetFrameRate_Field_Label"), ESleekSide.RIGHT);
        targetFrameRateField.Value = (uint)GraphicsSettings.TargetFrameRate;
        targetFrameRateField.OnValueChanged += OnTypedTargetFrameRate;
        targetFrameRateField.IsVisible = GraphicsSettings.UseTargetFrameRate;
        resolutionsBox.AddChild(targetFrameRateField);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
