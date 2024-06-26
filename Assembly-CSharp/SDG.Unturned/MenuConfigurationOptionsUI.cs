using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationOptionsUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton defaultButton;

    private static ISleekScrollView optionsBox;

    private static ISleekSlider fovSlider;

    private static ISleekLabel fovLabel;

    private static ISleekToggle debugToggle;

    private static ISleekToggle timerToggle;

    private static ISleekToggle goreToggle;

    private static ISleekToggle filterToggle;

    private static ISleekToggle chatTextToggle;

    private static ISleekToggle chatVoiceInToggle;

    private static ISleekToggle chatVoiceOutToggle;

    private static ISleekToggle chatVoiceAlwaysRecordingToggle;

    private static ISleekToggle hintsToggle;

    private static ISleekToggle streamerToggle;

    private static ISleekToggle featuredWorkshopToggle;

    private static ISleekToggle showHotbarToggle;

    private static ISleekToggle pauseWhenUnfocusedToggle;

    private static ISleekToggle nametagFadeOutToggle;

    private static ISleekInt32Field screenshotSizeMultiplierField;

    private static ISleekToggle screenshotSupersamplingToggle;

    private static ISleekToggle screenshotsWhileLoadingToggle;

    private static ISleekToggle staticCrosshairToggle;

    private static ISleekSlider staticCrosshairSizeSlider;

    private static SleekButtonState crosshairShapeButton;

    private static SleekButtonState metricButton;

    private static SleekButtonState talkButton;

    private static SleekButtonState uiButton;

    private static SleekButtonState hitmarkerButton;

    private static SleekButtonState hitmarkerStyleButton;

    private static SleekButtonState vehicleThirdPersonCameraModeButton;

    private static ISleekBox crosshairBox;

    private static SleekColorPicker crosshairColorPicker;

    private static ISleekBox hitmarkerBox;

    private static SleekColorPicker hitmarkerColorPicker;

    private static ISleekBox criticalHitmarkerBox;

    private static SleekColorPicker criticalHitmarkerColorPicker;

    private static ISleekBox cursorBox;

    private static SleekColorPicker cursorColorPicker;

    private static ISleekBox backgroundBox;

    private static SleekColorPicker backgroundColorPicker;

    private static ISleekBox foregroundBox;

    private static SleekColorPicker foregroundColorPicker;

    private static ISleekBox fontBox;

    private static SleekColorPicker fontColorPicker;

    private static ISleekBox shadowBox;

    private static SleekColorPicker shadowColorPicker;

    private static ISleekBox badColorBox;

    private static SleekColorPicker badColorPicker;

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
            MenuSettings.SaveOptionsIfLoaded();
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static string FormatFieldOfViewTooltip()
    {
        float desiredVerticalFieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
        if (localization.has("FOV_Slider_LabelV2_Value"))
        {
            float f = Camera.VerticalToHorizontalFieldOfView(desiredVerticalFieldOfView, ScreenEx.GetCurrentAspectRatio());
            return localization.format("FOV_Slider_LabelV2_Name") + "\n" + localization.format("FOV_Slider_LabelV2_Value", Mathf.RoundToInt(f), Mathf.RoundToInt(desiredVerticalFieldOfView));
        }
        return localization.format("FOV_Slider_Label", Mathf.RoundToInt(desiredVerticalFieldOfView));
    }

    private static void onDraggedFOVSlider(ISleekSlider slider, float state)
    {
        OptionsSettings.fov = state;
        OptionsSettings.apply();
        fovLabel.Text = FormatFieldOfViewTooltip();
    }

    private static void onToggledDebugToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.debug = state;
    }

    private static void onToggledTimerToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.timer = state;
        OptionsSettings.apply();
    }

    private static void onToggledGoreToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.gore = state;
    }

    private static void onToggledFilterToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.filter = state;
    }

    private static void onToggledChatTextToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.chatText = state;
    }

    private static void onToggledChatVoiceInToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.chatVoiceIn = state;
    }

    private static void onToggledChatVoiceOutToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.chatVoiceOut = state;
        chatVoiceAlwaysRecordingToggle.IsInteractable = state;
    }

    private static void onToggledChatVoiceAlwaysRecordingToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.VoiceAlwaysRecording = state;
    }

    private static void onToggledHintsToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.hints = state;
    }

    private static void onToggledStreamerToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.streamer = state;
    }

    private static void onToggledFeaturedWorkshopToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.featuredWorkshop = state;
    }

    private static void onToggledShowHotbarToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.showHotbar = state;
    }

    private static void onToggledPauseWhenUnfocusedToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.pauseWhenUnfocused = state;
    }

    private static void onToggledNametagFadeOutToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.shouldNametagFadeOut = state;
    }

    private static void OnScreenshotSizeMultiplierChanged(ISleekInt32Field field, int value)
    {
        OptionsSettings.screenshotSizeMultiplier = value;
    }

    private static void OnScreenshotSupersamplingChanged(ISleekToggle toggle, bool state)
    {
        OptionsSettings.enableScreenshotSupersampling = state;
    }

    private static void OnScreenshotsWhileLoadingChanged(ISleekToggle toggle, bool state)
    {
        OptionsSettings.enableScreenshotsOnLoadingScreen = state;
    }

    private static void OnUseStaticCrosshairChanged(ISleekToggle toggle, bool state)
    {
        OptionsSettings.useStaticCrosshair = state;
    }

    private static void OnStaticCrosshairSizeChanged(ISleekSlider slider, float state)
    {
        OptionsSettings.staticCrosshairSize = state;
    }

    private static void OnCrosshairShapeChanged(SleekButtonState button, int index)
    {
        OptionsSettings.crosshairShape = (ECrosshairShape)index;
        if (PlayerLifeUI.crosshair != null)
        {
            PlayerLifeUI.crosshair.SynchronizeImages();
        }
    }

    private static void onSwappedMetricState(SleekButtonState button, int index)
    {
        OptionsSettings.metric = index == 1;
    }

    private static void onSwappedTalkState(SleekButtonState button, int index)
    {
        OptionsSettings.talk = index == 1;
    }

    private static void onSwappedUIState(SleekButtonState button, int index)
    {
        OptionsSettings.proUI = index == 1;
    }

    private static void onSwappedHitmarkerState(SleekButtonState button, int index)
    {
        OptionsSettings.ShouldHitmarkersFollowWorldPosition = index == 1;
    }

    private static void onSwappedHitmarkerStyleState(SleekButtonState button, int index)
    {
        OptionsSettings.hitmarkerStyle = (EHitmarkerStyle)index;
    }

    private static void onSwappedVehicleThirdPersonCameraModeState(SleekButtonState button, int index)
    {
        OptionsSettings.vehicleThirdPersonCameraMode = (EVehicleThirdPersonCameraMode)index;
    }

    private static void onCrosshairColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.crosshairColor = color;
        if (PlayerLifeUI.crosshair != null)
        {
            PlayerLifeUI.crosshair.SynchronizeCustomColors();
        }
    }

    private static void onHitmarkerColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.hitmarkerColor = color;
    }

    private static void onCriticalHitmarkerColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.criticalHitmarkerColor = color;
    }

    private static void onCursorColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.cursorColor = color;
    }

    private static void onBackgroundColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.backgroundColor = color;
    }

    private static void onForegroundColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.foregroundColor = color;
    }

    private static void onFontColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.fontColor = color;
    }

    private static void onShadowColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.shadowColor = color;
    }

    private static void onBadColorPicked(SleekColorPicker picker, Color color)
    {
        OptionsSettings.badColor = color;
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

    private static void onClickedDefaultButton(ISleekElement button)
    {
        OptionsSettings.restoreDefaults();
        updateAll();
    }

    private static void updateAll()
    {
        fovSlider.Value = OptionsSettings.fov;
        fovLabel.Text = FormatFieldOfViewTooltip();
        debugToggle.Value = OptionsSettings.debug;
        timerToggle.Value = OptionsSettings.timer;
        goreToggle.Value = OptionsSettings.gore;
        filterToggle.Value = OptionsSettings.filter;
        chatTextToggle.Value = OptionsSettings.chatText;
        chatVoiceInToggle.Value = OptionsSettings.chatVoiceIn;
        chatVoiceOutToggle.Value = OptionsSettings.chatVoiceOut;
        chatVoiceAlwaysRecordingToggle.Value = OptionsSettings.VoiceAlwaysRecording;
        chatVoiceAlwaysRecordingToggle.IsInteractable = OptionsSettings.chatVoiceOut;
        hintsToggle.Value = OptionsSettings.hints;
        streamerToggle.Value = OptionsSettings.streamer;
        featuredWorkshopToggle.Value = OptionsSettings.featuredWorkshop;
        showHotbarToggle.Value = OptionsSettings.showHotbar;
        pauseWhenUnfocusedToggle.Value = OptionsSettings.pauseWhenUnfocused;
        nametagFadeOutToggle.Value = OptionsSettings.shouldNametagFadeOut;
        screenshotSizeMultiplierField.Value = OptionsSettings.screenshotSizeMultiplier;
        screenshotSupersamplingToggle.Value = OptionsSettings.enableScreenshotSupersampling;
        screenshotsWhileLoadingToggle.Value = OptionsSettings.enableScreenshotsOnLoadingScreen;
        staticCrosshairToggle.Value = OptionsSettings.useStaticCrosshair;
        staticCrosshairSizeSlider.Value = OptionsSettings.staticCrosshairSize;
        crosshairShapeButton.state = (int)OptionsSettings.crosshairShape;
        metricButton.state = (OptionsSettings.metric ? 1 : 0);
        talkButton.state = (OptionsSettings.talk ? 1 : 0);
        uiButton.state = (OptionsSettings.proUI ? 1 : 0);
        hitmarkerButton.state = (OptionsSettings.ShouldHitmarkersFollowWorldPosition ? 1 : 0);
        hitmarkerStyleButton.state = (int)OptionsSettings.hitmarkerStyle;
        vehicleThirdPersonCameraModeButton.state = (int)OptionsSettings.vehicleThirdPersonCameraMode;
        crosshairColorPicker.state = OptionsSettings.crosshairColor;
        hitmarkerColorPicker.state = OptionsSettings.hitmarkerColor;
        criticalHitmarkerColorPicker.state = OptionsSettings.criticalHitmarkerColor;
        cursorColorPicker.state = OptionsSettings.cursorColor;
        backgroundColorPicker.state = OptionsSettings.backgroundColor;
        foregroundColorPicker.state = OptionsSettings.foregroundColor;
        fontColorPicker.state = OptionsSettings.fontColor;
        shadowColorPicker.state = OptionsSettings.shadowColor;
        badColorPicker.state = OptionsSettings.badColor;
    }

    public MenuConfigurationOptionsUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationOptions.dat");
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
        optionsBox = Glazier.Get().CreateScrollView();
        optionsBox.PositionOffset_X = -250f;
        optionsBox.PositionOffset_Y = 100f;
        optionsBox.PositionScale_X = 0.5f;
        optionsBox.SizeOffset_X = 530f;
        optionsBox.SizeOffset_Y = -200f;
        optionsBox.SizeScale_Y = 1f;
        optionsBox.ScaleContentToWidth = true;
        container.AddChild(optionsBox);
        float num = 0f;
        debugToggle = Glazier.Get().CreateToggle();
        debugToggle.PositionOffset_Y = num;
        debugToggle.SizeOffset_X = 40f;
        debugToggle.SizeOffset_Y = 40f;
        debugToggle.AddLabel(localization.format("Debug_Toggle_Label"), ESleekSide.RIGHT);
        debugToggle.OnValueChanged += onToggledDebugToggle;
        optionsBox.AddChild(debugToggle);
        num += 50f;
        timerToggle = Glazier.Get().CreateToggle();
        timerToggle.PositionOffset_Y = num;
        timerToggle.SizeOffset_X = 40f;
        timerToggle.SizeOffset_Y = 40f;
        timerToggle.AddLabel(localization.format("Timer_Toggle_Label"), ESleekSide.RIGHT);
        timerToggle.OnValueChanged += onToggledTimerToggle;
        optionsBox.AddChild(timerToggle);
        num += 50f;
        goreToggle = Glazier.Get().CreateToggle();
        goreToggle.PositionOffset_Y = num;
        goreToggle.SizeOffset_X = 40f;
        goreToggle.SizeOffset_Y = 40f;
        goreToggle.AddLabel(localization.format("Gore_Toggle_Label"), ESleekSide.RIGHT);
        goreToggle.OnValueChanged += onToggledGoreToggle;
        optionsBox.AddChild(goreToggle);
        num += 50f;
        filterToggle = Glazier.Get().CreateToggle();
        filterToggle.PositionOffset_Y = num;
        filterToggle.SizeOffset_X = 40f;
        filterToggle.SizeOffset_Y = 40f;
        filterToggle.AddLabel(localization.format("Filter_Toggle_Label"), ESleekSide.RIGHT);
        filterToggle.TooltipText = localization.format("Filter_Toggle_Tooltip");
        filterToggle.OnValueChanged += onToggledFilterToggle;
        optionsBox.AddChild(filterToggle);
        num += 50f;
        chatTextToggle = Glazier.Get().CreateToggle();
        chatTextToggle.PositionOffset_Y = num;
        chatTextToggle.SizeOffset_X = 40f;
        chatTextToggle.SizeOffset_Y = 40f;
        chatTextToggle.AddLabel(localization.format("Chat_Text_Toggle_Label"), ESleekSide.RIGHT);
        chatTextToggle.OnValueChanged += onToggledChatTextToggle;
        optionsBox.AddChild(chatTextToggle);
        num += 50f;
        chatVoiceInToggle = Glazier.Get().CreateToggle();
        chatVoiceInToggle.PositionOffset_Y = num;
        chatVoiceInToggle.SizeOffset_X = 40f;
        chatVoiceInToggle.SizeOffset_Y = 40f;
        chatVoiceInToggle.AddLabel(localization.format("Chat_Voice_In_Toggle_Label"), ESleekSide.RIGHT);
        chatVoiceInToggle.OnValueChanged += onToggledChatVoiceInToggle;
        optionsBox.AddChild(chatVoiceInToggle);
        num += 50f;
        chatVoiceOutToggle = Glazier.Get().CreateToggle();
        chatVoiceOutToggle.PositionOffset_Y = num;
        chatVoiceOutToggle.SizeOffset_X = 40f;
        chatVoiceOutToggle.SizeOffset_Y = 40f;
        chatVoiceOutToggle.AddLabel(localization.format("Chat_Voice_Out_Toggle_Label"), ESleekSide.RIGHT);
        chatVoiceOutToggle.OnValueChanged += onToggledChatVoiceOutToggle;
        optionsBox.AddChild(chatVoiceOutToggle);
        num += 50f;
        chatVoiceAlwaysRecordingToggle = Glazier.Get().CreateToggle();
        chatVoiceAlwaysRecordingToggle.PositionOffset_Y = num;
        chatVoiceAlwaysRecordingToggle.SizeOffset_X = 40f;
        chatVoiceAlwaysRecordingToggle.SizeOffset_Y = 40f;
        chatVoiceAlwaysRecordingToggle.AddLabel(localization.format("VoiceAlwaysRecording_Label"), ESleekSide.RIGHT);
        chatVoiceAlwaysRecordingToggle.TooltipText = localization.format("VoiceAlwaysRecording_Tooltip");
        chatVoiceAlwaysRecordingToggle.OnValueChanged += onToggledChatVoiceAlwaysRecordingToggle;
        optionsBox.AddChild(chatVoiceAlwaysRecordingToggle);
        num += 50f;
        hintsToggle = Glazier.Get().CreateToggle();
        hintsToggle.PositionOffset_Y = num;
        hintsToggle.SizeOffset_X = 40f;
        hintsToggle.SizeOffset_Y = 40f;
        hintsToggle.AddLabel(localization.format("Hints_Toggle_Label"), ESleekSide.RIGHT);
        hintsToggle.OnValueChanged += onToggledHintsToggle;
        optionsBox.AddChild(hintsToggle);
        num += 50f;
        streamerToggle = Glazier.Get().CreateToggle();
        streamerToggle.PositionOffset_Y = num;
        streamerToggle.SizeOffset_X = 40f;
        streamerToggle.SizeOffset_Y = 40f;
        streamerToggle.AddLabel(localization.format("Streamer_Toggle_Label"), ESleekSide.RIGHT);
        streamerToggle.OnValueChanged += onToggledStreamerToggle;
        optionsBox.AddChild(streamerToggle);
        num += 50f;
        featuredWorkshopToggle = Glazier.Get().CreateToggle();
        featuredWorkshopToggle.PositionOffset_Y = num;
        featuredWorkshopToggle.SizeOffset_X = 40f;
        featuredWorkshopToggle.SizeOffset_Y = 40f;
        featuredWorkshopToggle.AddLabel(localization.format("Featured_Workshop_Toggle_Label"), ESleekSide.RIGHT);
        featuredWorkshopToggle.OnValueChanged += onToggledFeaturedWorkshopToggle;
        optionsBox.AddChild(featuredWorkshopToggle);
        num += 50f;
        showHotbarToggle = Glazier.Get().CreateToggle();
        showHotbarToggle.PositionOffset_Y = num;
        showHotbarToggle.SizeOffset_X = 40f;
        showHotbarToggle.SizeOffset_Y = 40f;
        showHotbarToggle.AddLabel(localization.format("Show_Hotbar_Toggle_Label"), ESleekSide.RIGHT);
        showHotbarToggle.OnValueChanged += onToggledShowHotbarToggle;
        optionsBox.AddChild(showHotbarToggle);
        num += 50f;
        pauseWhenUnfocusedToggle = Glazier.Get().CreateToggle();
        pauseWhenUnfocusedToggle.PositionOffset_Y = num;
        pauseWhenUnfocusedToggle.SizeOffset_X = 40f;
        pauseWhenUnfocusedToggle.SizeOffset_Y = 40f;
        pauseWhenUnfocusedToggle.AddLabel(localization.format("Pause_When_Unfocused_Label"), ESleekSide.RIGHT);
        pauseWhenUnfocusedToggle.OnValueChanged += onToggledPauseWhenUnfocusedToggle;
        optionsBox.AddChild(pauseWhenUnfocusedToggle);
        num += 50f;
        nametagFadeOutToggle = Glazier.Get().CreateToggle();
        nametagFadeOutToggle.PositionOffset_Y = num;
        nametagFadeOutToggle.SizeOffset_X = 40f;
        nametagFadeOutToggle.SizeOffset_Y = 40f;
        nametagFadeOutToggle.AddLabel(localization.format("Nametag_Fade_Out_Label"), ESleekSide.RIGHT);
        nametagFadeOutToggle.TooltipText = localization.format("Nametag_Fade_Out_Tooltip");
        nametagFadeOutToggle.OnValueChanged += onToggledNametagFadeOutToggle;
        optionsBox.AddChild(nametagFadeOutToggle);
        num += 50f;
        fovSlider = Glazier.Get().CreateSlider();
        fovSlider.PositionOffset_Y = num;
        fovSlider.SizeOffset_X = 200f;
        fovSlider.SizeOffset_Y = 20f;
        fovSlider.Orientation = ESleekOrientation.HORIZONTAL;
        fovSlider.OnValueChanged += onDraggedFOVSlider;
        optionsBox.AddChild(fovSlider);
        fovLabel = Glazier.Get().CreateLabel();
        fovLabel.PositionOffset_X = 5f;
        fovLabel.PositionOffset_Y = -30f;
        fovLabel.PositionScale_X = 1f;
        fovLabel.PositionScale_Y = 0.5f;
        fovLabel.SizeOffset_X = 300f;
        fovLabel.SizeOffset_Y = 60f;
        fovLabel.TextAlignment = TextAnchor.MiddleLeft;
        fovLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        fovLabel.Text = FormatFieldOfViewTooltip();
        fovSlider.AddChild(fovLabel);
        num += 30f;
        screenshotSizeMultiplierField = Glazier.Get().CreateInt32Field();
        screenshotSizeMultiplierField.PositionOffset_Y = num;
        screenshotSizeMultiplierField.SizeOffset_X = 200f;
        screenshotSizeMultiplierField.SizeOffset_Y = 30f;
        screenshotSizeMultiplierField.AddLabel(localization.format("ScreenshotSizeMultiplier_Label"), ESleekSide.RIGHT);
        screenshotSizeMultiplierField.TooltipText = localization.format("ScreenshotSizeMultiplier_Tooltip");
        screenshotSizeMultiplierField.OnValueChanged += OnScreenshotSizeMultiplierChanged;
        optionsBox.AddChild(screenshotSizeMultiplierField);
        num += 40f;
        screenshotSupersamplingToggle = Glazier.Get().CreateToggle();
        screenshotSupersamplingToggle.PositionOffset_Y = num;
        screenshotSupersamplingToggle.SizeOffset_X = 40f;
        screenshotSupersamplingToggle.SizeOffset_Y = 40f;
        screenshotSupersamplingToggle.AddLabel(localization.format("ScreenshotSupersampling_Label"), ESleekSide.RIGHT);
        screenshotSupersamplingToggle.TooltipText = localization.format("ScreenshotSupersampling_Tooltip");
        screenshotSupersamplingToggle.OnValueChanged += OnScreenshotSupersamplingChanged;
        optionsBox.AddChild(screenshotSupersamplingToggle);
        num += 50f;
        screenshotsWhileLoadingToggle = Glazier.Get().CreateToggle();
        screenshotsWhileLoadingToggle.PositionOffset_Y = num;
        screenshotsWhileLoadingToggle.SizeOffset_X = 40f;
        screenshotsWhileLoadingToggle.SizeOffset_Y = 40f;
        screenshotsWhileLoadingToggle.AddLabel(localization.format("ScreenshotsWhileLoading_Label"), ESleekSide.RIGHT);
        screenshotsWhileLoadingToggle.TooltipText = localization.format("ScreenshotsWhileLoading_Tooltip");
        screenshotsWhileLoadingToggle.OnValueChanged += OnScreenshotsWhileLoadingChanged;
        optionsBox.AddChild(screenshotsWhileLoadingToggle);
        num += 50f;
        staticCrosshairToggle = Glazier.Get().CreateToggle();
        staticCrosshairToggle.PositionOffset_Y = num;
        staticCrosshairToggle.SizeOffset_X = 40f;
        staticCrosshairToggle.SizeOffset_Y = 40f;
        staticCrosshairToggle.AddLabel(localization.format("UseStaticCrosshair_Label"), ESleekSide.RIGHT);
        staticCrosshairToggle.TooltipText = localization.format("UseStaticCrosshair_Tooltip");
        staticCrosshairToggle.OnValueChanged += OnUseStaticCrosshairChanged;
        optionsBox.AddChild(staticCrosshairToggle);
        num += 50f;
        staticCrosshairSizeSlider = Glazier.Get().CreateSlider();
        staticCrosshairSizeSlider.PositionOffset_Y = num;
        staticCrosshairSizeSlider.SizeOffset_X = 200f;
        staticCrosshairSizeSlider.SizeOffset_Y = 20f;
        staticCrosshairSizeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        staticCrosshairSizeSlider.AddLabel(localization.format("StaticCrosshairSize_Label"), ESleekSide.RIGHT);
        staticCrosshairSizeSlider.OnValueChanged += OnStaticCrosshairSizeChanged;
        optionsBox.AddChild(staticCrosshairSizeSlider);
        num += 30f;
        crosshairShapeButton = new SleekButtonState(new GUIContent(localization.format("CrosshairShape_Line")), new GUIContent(localization.format("CrosshairShape_Classic")));
        crosshairShapeButton.PositionOffset_Y = num;
        crosshairShapeButton.SizeOffset_X = 200f;
        crosshairShapeButton.SizeOffset_Y = 30f;
        crosshairShapeButton.state = (int)OptionsSettings.crosshairShape;
        crosshairShapeButton.AddLabel(localization.format("CrosshairShape_Label"), ESleekSide.RIGHT);
        crosshairShapeButton.onSwappedState = OnCrosshairShapeChanged;
        optionsBox.AddChild(crosshairShapeButton);
        num += 40f;
        talkButton = new SleekButtonState(new GUIContent(localization.format("Talk_Off")), new GUIContent(localization.format("Talk_On")));
        talkButton.PositionOffset_Y = num;
        talkButton.SizeOffset_X = 200f;
        talkButton.SizeOffset_Y = 30f;
        talkButton.state = (OptionsSettings.talk ? 1 : 0);
        talkButton.tooltip = localization.format("Talk_Tooltip");
        talkButton.AddLabel(localization.format("Talk_Label"), ESleekSide.RIGHT);
        talkButton.onSwappedState = onSwappedTalkState;
        optionsBox.AddChild(talkButton);
        num += 40f;
        metricButton = new SleekButtonState(new GUIContent(localization.format("Metric_Off")), new GUIContent(localization.format("Metric_On")));
        metricButton.PositionOffset_Y = num;
        metricButton.SizeOffset_X = 200f;
        metricButton.SizeOffset_Y = 30f;
        metricButton.state = (OptionsSettings.metric ? 1 : 0);
        metricButton.tooltip = localization.format("Metric_Tooltip");
        metricButton.AddLabel(localization.format("Metric_Label"), ESleekSide.RIGHT);
        metricButton.onSwappedState = onSwappedMetricState;
        optionsBox.AddChild(metricButton);
        num += 40f;
        uiButton = new SleekButtonState(new GUIContent(localization.format("UI_Free")), new GUIContent(localization.format("UI_Pro")));
        uiButton.PositionOffset_Y = num;
        uiButton.SizeOffset_X = 200f;
        uiButton.SizeOffset_Y = 30f;
        uiButton.tooltip = localization.format("UI_Tooltip");
        uiButton.AddLabel(localization.format("UI_Label"), ESleekSide.RIGHT);
        uiButton.onSwappedState = onSwappedUIState;
        optionsBox.AddChild(uiButton);
        num += 40f;
        hitmarkerButton = new SleekButtonState(new GUIContent(localization.format("Hitmarker_Static")), new GUIContent(localization.format("Hitmarker_Dynamic")));
        hitmarkerButton.PositionOffset_Y = num;
        hitmarkerButton.SizeOffset_X = 200f;
        hitmarkerButton.SizeOffset_Y = 30f;
        hitmarkerButton.tooltip = localization.format("Hitmarker_Tooltip");
        hitmarkerButton.AddLabel(localization.format("Hitmarker_Label"), ESleekSide.RIGHT);
        hitmarkerButton.onSwappedState = onSwappedHitmarkerState;
        optionsBox.AddChild(hitmarkerButton);
        num += 40f;
        hitmarkerStyleButton = new SleekButtonState(new GUIContent(localization.format("HitmarkerStyle_Animated")), new GUIContent(localization.format("HitmarkerStyle_Classic")));
        hitmarkerStyleButton.PositionOffset_Y = num;
        hitmarkerStyleButton.SizeOffset_X = 200f;
        hitmarkerStyleButton.SizeOffset_Y = 30f;
        hitmarkerStyleButton.AddLabel(localization.format("HitmarkerStyle_Label"), ESleekSide.RIGHT);
        hitmarkerStyleButton.onSwappedState = onSwappedHitmarkerStyleState;
        optionsBox.AddChild(hitmarkerStyleButton);
        num += 40f;
        vehicleThirdPersonCameraModeButton = new SleekButtonState(new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationDetached")), new GUIContent(localization.format("VehicleThirdPersonCameraMode_RotationAttached")));
        vehicleThirdPersonCameraModeButton.PositionOffset_Y = num;
        vehicleThirdPersonCameraModeButton.SizeOffset_X = 200f;
        vehicleThirdPersonCameraModeButton.SizeOffset_Y = 30f;
        vehicleThirdPersonCameraModeButton.AddLabel(localization.format("VehicleThirdPersonCameraMode_Label"), ESleekSide.RIGHT);
        vehicleThirdPersonCameraModeButton.onSwappedState = onSwappedVehicleThirdPersonCameraModeState;
        optionsBox.AddChild(vehicleThirdPersonCameraModeButton);
        num += 40f;
        crosshairBox = Glazier.Get().CreateBox();
        crosshairBox.PositionOffset_Y = num;
        crosshairBox.SizeOffset_X = 240f;
        crosshairBox.SizeOffset_Y = 30f;
        crosshairBox.Text = localization.format("Crosshair_Box");
        optionsBox.AddChild(crosshairBox);
        num += 40f;
        crosshairColorPicker = new SleekColorPicker();
        crosshairColorPicker.PositionOffset_Y = num;
        crosshairColorPicker.onColorPicked = onCrosshairColorPicked;
        crosshairColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(crosshairColorPicker);
        num += 160f;
        hitmarkerBox = Glazier.Get().CreateBox();
        hitmarkerBox.PositionOffset_Y = num;
        hitmarkerBox.SizeOffset_X = 240f;
        hitmarkerBox.SizeOffset_Y = 30f;
        hitmarkerBox.Text = localization.format("Hitmarker_Box");
        optionsBox.AddChild(hitmarkerBox);
        num += 40f;
        hitmarkerColorPicker = new SleekColorPicker();
        hitmarkerColorPicker.PositionOffset_Y = num;
        hitmarkerColorPicker.onColorPicked = onHitmarkerColorPicked;
        hitmarkerColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(hitmarkerColorPicker);
        num += 160f;
        criticalHitmarkerBox = Glazier.Get().CreateBox();
        criticalHitmarkerBox.PositionOffset_Y = num;
        criticalHitmarkerBox.SizeOffset_X = 240f;
        criticalHitmarkerBox.SizeOffset_Y = 30f;
        criticalHitmarkerBox.Text = localization.format("Critical_Hitmarker_Box");
        optionsBox.AddChild(criticalHitmarkerBox);
        num += 40f;
        criticalHitmarkerColorPicker = new SleekColorPicker();
        criticalHitmarkerColorPicker.PositionOffset_Y = num;
        criticalHitmarkerColorPicker.onColorPicked = onCriticalHitmarkerColorPicked;
        criticalHitmarkerColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(criticalHitmarkerColorPicker);
        num += 160f;
        cursorBox = Glazier.Get().CreateBox();
        cursorBox.PositionOffset_Y = num;
        cursorBox.SizeOffset_X = 240f;
        cursorBox.SizeOffset_Y = 30f;
        cursorBox.Text = localization.format("Cursor_Box");
        optionsBox.AddChild(cursorBox);
        num += 40f;
        cursorColorPicker = new SleekColorPicker();
        cursorColorPicker.PositionOffset_Y = num;
        cursorColorPicker.onColorPicked = onCursorColorPicked;
        optionsBox.AddChild(cursorColorPicker);
        num += 130f;
        backgroundBox = Glazier.Get().CreateBox();
        backgroundBox.PositionOffset_Y = num;
        backgroundBox.SizeOffset_X = 240f;
        backgroundBox.SizeOffset_Y = 30f;
        backgroundBox.Text = localization.format("Background_Box");
        optionsBox.AddChild(backgroundBox);
        num += 40f;
        backgroundColorPicker = new SleekColorPicker();
        backgroundColorPicker.PositionOffset_Y = num;
        if (Provider.isPro)
        {
            backgroundColorPicker.onColorPicked = onBackgroundColorPicked;
        }
        else
        {
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = -40f;
            sleekImage.PositionOffset_Y = -40f;
            sleekImage.PositionScale_X = 0.5f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.SizeOffset_X = 80f;
            sleekImage.SizeOffset_Y = 80f;
            sleekImage.Texture = bundle.load<Texture2D>("Lock_Large");
            backgroundColorPicker.AddChild(sleekImage);
            bundle.unload();
        }
        optionsBox.AddChild(backgroundColorPicker);
        num += 130f;
        foregroundBox = Glazier.Get().CreateBox();
        foregroundBox.PositionOffset_Y = num;
        foregroundBox.SizeOffset_X = 240f;
        foregroundBox.SizeOffset_Y = 30f;
        foregroundBox.Text = localization.format("Foreground_Box");
        optionsBox.AddChild(foregroundBox);
        num += 40f;
        foregroundColorPicker = new SleekColorPicker();
        foregroundColorPicker.PositionOffset_Y = num;
        if (Provider.isPro)
        {
            foregroundColorPicker.onColorPicked = onForegroundColorPicked;
        }
        else
        {
            Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.PositionOffset_X = -40f;
            sleekImage2.PositionOffset_Y = -40f;
            sleekImage2.PositionScale_X = 0.5f;
            sleekImage2.PositionScale_Y = 0.5f;
            sleekImage2.SizeOffset_X = 80f;
            sleekImage2.SizeOffset_Y = 80f;
            sleekImage2.Texture = bundle2.load<Texture2D>("Lock_Large");
            foregroundColorPicker.AddChild(sleekImage2);
            bundle2.unload();
        }
        optionsBox.AddChild(foregroundColorPicker);
        num += 130f;
        fontBox = Glazier.Get().CreateBox();
        fontBox.PositionOffset_Y = num;
        fontBox.SizeOffset_X = 240f;
        fontBox.SizeOffset_Y = 30f;
        fontBox.Text = localization.format("Font_Box");
        optionsBox.AddChild(fontBox);
        num += 40f;
        fontColorPicker = new SleekColorPicker();
        fontColorPicker.PositionOffset_Y = num;
        if (Provider.isPro)
        {
            fontColorPicker.onColorPicked = onFontColorPicked;
        }
        else
        {
            Bundle bundle3 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage3 = Glazier.Get().CreateImage();
            sleekImage3.PositionOffset_X = -40f;
            sleekImage3.PositionOffset_Y = -40f;
            sleekImage3.PositionScale_X = 0.5f;
            sleekImage3.PositionScale_Y = 0.5f;
            sleekImage3.SizeOffset_X = 80f;
            sleekImage3.SizeOffset_Y = 80f;
            sleekImage3.Texture = bundle3.load<Texture2D>("Lock_Large");
            fontColorPicker.AddChild(sleekImage3);
            bundle3.unload();
        }
        optionsBox.AddChild(fontColorPicker);
        num += 130f;
        shadowBox = Glazier.Get().CreateBox();
        shadowBox.PositionOffset_Y = num;
        shadowBox.SizeOffset_X = 240f;
        shadowBox.SizeOffset_Y = 30f;
        shadowBox.Text = localization.format("Shadow_Box");
        optionsBox.AddChild(shadowBox);
        num += shadowBox.SizeOffset_Y + 10f;
        shadowColorPicker = new SleekColorPicker();
        shadowColorPicker.PositionOffset_Y = num;
        if (Provider.isPro)
        {
            shadowColorPicker.onColorPicked = onShadowColorPicked;
        }
        else
        {
            Bundle bundle4 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage4 = Glazier.Get().CreateImage();
            sleekImage4.PositionOffset_X = -40f;
            sleekImage4.PositionOffset_Y = -40f;
            sleekImage4.PositionScale_X = 0.5f;
            sleekImage4.PositionScale_Y = 0.5f;
            sleekImage4.SizeOffset_X = 80f;
            sleekImage4.SizeOffset_Y = 80f;
            sleekImage4.Texture = bundle4.load<Texture2D>("Lock_Large");
            shadowColorPicker.AddChild(sleekImage4);
            bundle4.unload();
        }
        optionsBox.AddChild(shadowColorPicker);
        num += shadowColorPicker.SizeOffset_Y + 10f;
        badColorBox = Glazier.Get().CreateBox();
        badColorBox.PositionOffset_Y = num;
        badColorBox.SizeOffset_X = 240f;
        badColorBox.SizeOffset_Y = 30f;
        badColorBox.Text = localization.format("Bad_Color_Box");
        optionsBox.AddChild(badColorBox);
        num += badColorBox.SizeOffset_Y + 10f;
        badColorPicker = new SleekColorPicker();
        badColorPicker.PositionOffset_Y = num;
        if (Provider.isPro)
        {
            badColorPicker.onColorPicked = onBadColorPicked;
        }
        else
        {
            Bundle bundle5 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage5 = Glazier.Get().CreateImage();
            sleekImage5.PositionOffset_X = -40f;
            sleekImage5.PositionOffset_Y = -40f;
            sleekImage5.PositionScale_X = 0.5f;
            sleekImage5.PositionScale_Y = 0.5f;
            sleekImage5.SizeOffset_X = 80f;
            sleekImage5.SizeOffset_Y = 80f;
            sleekImage5.Texture = bundle5.load<Texture2D>("Lock_Large");
            badColorPicker.AddChild(sleekImage5);
            bundle5.unload();
        }
        optionsBox.AddChild(badColorPicker);
        num += badColorPicker.SizeOffset_Y;
        optionsBox.ContentSizeOffset = new Vector2(0f, num);
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
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.PositionOffset_X = -200f;
        defaultButton.PositionOffset_Y = -50f;
        defaultButton.PositionScale_X = 1f;
        defaultButton.PositionScale_Y = 1f;
        defaultButton.SizeOffset_X = 200f;
        defaultButton.SizeOffset_Y = 50f;
        defaultButton.Text = MenuPlayConfigUI.localization.format("Default");
        defaultButton.TooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
        defaultButton.OnClicked += onClickedDefaultButton;
        defaultButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        updateAll();
    }
}
