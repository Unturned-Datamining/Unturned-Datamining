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

    private static ISleekSlider volumeSlider;

    private static ISleekSlider voiceSlider;

    private static ISleekSlider loadingScreenMusicVolumeSlider;

    private static ISleekToggle debugToggle;

    private static ISleekToggle musicToggle;

    private static ISleekToggle timerToggle;

    private static ISleekToggle goreToggle;

    private static ISleekToggle filterToggle;

    private static ISleekToggle chatTextToggle;

    private static ISleekToggle chatVoiceInToggle;

    private static ISleekToggle chatVoiceOutToggle;

    private static ISleekToggle hintsToggle;

    private static ISleekToggle ambienceToggle;

    private static ISleekToggle streamerToggle;

    private static ISleekToggle featuredWorkshopToggle;

    private static ISleekToggle matchmakingShowAllMapsToggle;

    private static ISleekToggle showHotbarToggle;

    private static ISleekToggle pauseWhenUnfocusedToggle;

    private static ISleekInt32Field minMatchmakingPlayersField;

    private static ISleekInt32Field maxMatchmakingPingField;

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
        fovLabel.text = FormatFieldOfViewTooltip();
    }

    private static void onDraggedVolumeSlider(ISleekSlider slider, float state)
    {
        OptionsSettings.volume = state;
        OptionsSettings.apply();
        volumeSlider.updateLabel(localization.format("Volume_Slider_Label", (int)(OptionsSettings.volume * 100f)));
    }

    private static void onDraggedVoiceSlider(ISleekSlider slider, float state)
    {
        OptionsSettings.voiceVolume = state;
        voiceSlider.updateLabel(localization.format("Voice_Slider_Label", (int)(OptionsSettings.voiceVolume * 100f)));
    }

    private static void onDraggedLoadingScreenMusicVolumeSlider(ISleekSlider slider, float state)
    {
        OptionsSettings.loadingScreenMusicVolume = state;
        loadingScreenMusicVolumeSlider.updateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", (int)(OptionsSettings.loadingScreenMusicVolume * 100f)));
    }

    private static void onToggledDebugToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.debug = state;
    }

    private static void onToggledMusicToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.music = state;
        OptionsSettings.apply();
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
    }

    private static void onToggledHintsToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.hints = state;
    }

    private static void onToggledAmbienceToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.ambience = state;
        OptionsSettings.apply();
    }

    private static void onToggledStreamerToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.streamer = state;
    }

    private static void onToggledFeaturedWorkshopToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.featuredWorkshop = state;
    }

    private static void onToggledMatchmakingShowAllMapsToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.matchmakingShowAllMaps = state;
    }

    private static void onToggledShowHotbarToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.showHotbar = state;
    }

    private static void onToggledPauseWhenUnfocusedToggle(ISleekToggle toggle, bool state)
    {
        OptionsSettings.pauseWhenUnfocused = state;
    }

    private static void onTypedMinMatchmakingPlayersField(ISleekInt32Field field, int state)
    {
        OptionsSettings.minMatchmakingPlayers = state;
    }

    private static void onTypedMaxMatchmakingPingField(ISleekInt32Field field, int state)
    {
        OptionsSettings.maxMatchmakingPing = state;
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
        OptionsSettings.hitmarker = index == 1;
    }

    private static void onSwappedHitmarkerStyleState(SleekButtonState button, int index)
    {
        OptionsSettings.hitmarkerStyle = (EHitmarkerStyle)index;
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
        fovSlider.state = OptionsSettings.fov;
        fovLabel.text = FormatFieldOfViewTooltip();
        volumeSlider.state = OptionsSettings.volume;
        volumeSlider.updateLabel(localization.format("Volume_Slider_Label", (int)(OptionsSettings.volume * 100f)));
        voiceSlider.state = OptionsSettings.voiceVolume;
        voiceSlider.updateLabel(localization.format("Voice_Slider_Label", (int)(OptionsSettings.voiceVolume * 100f)));
        loadingScreenMusicVolumeSlider.state = OptionsSettings.loadingScreenMusicVolume;
        loadingScreenMusicVolumeSlider.updateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", (int)(OptionsSettings.loadingScreenMusicVolume * 100f)));
        debugToggle.state = OptionsSettings.debug;
        musicToggle.state = OptionsSettings.music;
        timerToggle.state = OptionsSettings.timer;
        goreToggle.state = OptionsSettings.gore;
        filterToggle.state = OptionsSettings.filter;
        chatTextToggle.state = OptionsSettings.chatText;
        chatVoiceInToggle.state = OptionsSettings.chatVoiceIn;
        chatVoiceOutToggle.state = OptionsSettings.chatVoiceOut;
        hintsToggle.state = OptionsSettings.hints;
        ambienceToggle.state = OptionsSettings.ambience;
        streamerToggle.state = OptionsSettings.streamer;
        featuredWorkshopToggle.state = OptionsSettings.featuredWorkshop;
        matchmakingShowAllMapsToggle.state = OptionsSettings.matchmakingShowAllMaps;
        showHotbarToggle.state = OptionsSettings.showHotbar;
        pauseWhenUnfocusedToggle.state = OptionsSettings.pauseWhenUnfocused;
        minMatchmakingPlayersField.state = OptionsSettings.minMatchmakingPlayers;
        maxMatchmakingPingField.state = OptionsSettings.maxMatchmakingPing;
        screenshotSizeMultiplierField.state = OptionsSettings.screenshotSizeMultiplier;
        screenshotSupersamplingToggle.state = OptionsSettings.enableScreenshotSupersampling;
        screenshotsWhileLoadingToggle.state = OptionsSettings.enableScreenshotsOnLoadingScreen;
        staticCrosshairToggle.state = OptionsSettings.useStaticCrosshair;
        staticCrosshairSizeSlider.state = OptionsSettings.staticCrosshairSize;
        crosshairShapeButton.state = (int)OptionsSettings.crosshairShape;
        metricButton.state = (OptionsSettings.metric ? 1 : 0);
        talkButton.state = (OptionsSettings.talk ? 1 : 0);
        uiButton.state = (OptionsSettings.proUI ? 1 : 0);
        hitmarkerButton.state = (OptionsSettings.hitmarker ? 1 : 0);
        hitmarkerStyleButton.state = (int)OptionsSettings.hitmarkerStyle;
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
        optionsBox = Glazier.Get().CreateScrollView();
        optionsBox.positionOffset_X = -250;
        optionsBox.positionOffset_Y = 100;
        optionsBox.positionScale_X = 0.5f;
        optionsBox.sizeOffset_X = 530;
        optionsBox.sizeOffset_Y = -200;
        optionsBox.sizeScale_Y = 1f;
        optionsBox.scaleContentToWidth = true;
        container.AddChild(optionsBox);
        int num = 0;
        debugToggle = Glazier.Get().CreateToggle();
        debugToggle.positionOffset_Y = num;
        debugToggle.sizeOffset_X = 40;
        debugToggle.sizeOffset_Y = 40;
        debugToggle.addLabel(localization.format("Debug_Toggle_Label"), ESleekSide.RIGHT);
        debugToggle.onToggled += onToggledDebugToggle;
        optionsBox.AddChild(debugToggle);
        num += 50;
        musicToggle = Glazier.Get().CreateToggle();
        musicToggle.positionOffset_Y = num;
        musicToggle.sizeOffset_X = 40;
        musicToggle.sizeOffset_Y = 40;
        musicToggle.addLabel(localization.format("Music_Toggle_Label"), ESleekSide.RIGHT);
        musicToggle.onToggled += onToggledMusicToggle;
        optionsBox.AddChild(musicToggle);
        num += 50;
        timerToggle = Glazier.Get().CreateToggle();
        timerToggle.positionOffset_Y = num;
        timerToggle.sizeOffset_X = 40;
        timerToggle.sizeOffset_Y = 40;
        timerToggle.addLabel(localization.format("Timer_Toggle_Label"), ESleekSide.RIGHT);
        timerToggle.onToggled += onToggledTimerToggle;
        optionsBox.AddChild(timerToggle);
        num += 50;
        goreToggle = Glazier.Get().CreateToggle();
        goreToggle.positionOffset_Y = num;
        goreToggle.sizeOffset_X = 40;
        goreToggle.sizeOffset_Y = 40;
        goreToggle.addLabel(localization.format("Gore_Toggle_Label"), ESleekSide.RIGHT);
        goreToggle.onToggled += onToggledGoreToggle;
        optionsBox.AddChild(goreToggle);
        num += 50;
        filterToggle = Glazier.Get().CreateToggle();
        filterToggle.positionOffset_Y = num;
        filterToggle.sizeOffset_X = 40;
        filterToggle.sizeOffset_Y = 40;
        filterToggle.addLabel(localization.format("Filter_Toggle_Label"), ESleekSide.RIGHT);
        filterToggle.tooltipText = localization.format("Filter_Toggle_Tooltip");
        filterToggle.onToggled += onToggledFilterToggle;
        optionsBox.AddChild(filterToggle);
        num += 50;
        chatTextToggle = Glazier.Get().CreateToggle();
        chatTextToggle.positionOffset_Y = num;
        chatTextToggle.sizeOffset_X = 40;
        chatTextToggle.sizeOffset_Y = 40;
        chatTextToggle.addLabel(localization.format("Chat_Text_Toggle_Label"), ESleekSide.RIGHT);
        chatTextToggle.onToggled += onToggledChatTextToggle;
        optionsBox.AddChild(chatTextToggle);
        num += 50;
        chatVoiceInToggle = Glazier.Get().CreateToggle();
        chatVoiceInToggle.positionOffset_Y = num;
        chatVoiceInToggle.sizeOffset_X = 40;
        chatVoiceInToggle.sizeOffset_Y = 40;
        chatVoiceInToggle.addLabel(localization.format("Chat_Voice_In_Toggle_Label"), ESleekSide.RIGHT);
        chatVoiceInToggle.onToggled += onToggledChatVoiceInToggle;
        optionsBox.AddChild(chatVoiceInToggle);
        num += 50;
        chatVoiceOutToggle = Glazier.Get().CreateToggle();
        chatVoiceOutToggle.positionOffset_Y = num;
        chatVoiceOutToggle.sizeOffset_X = 40;
        chatVoiceOutToggle.sizeOffset_Y = 40;
        chatVoiceOutToggle.addLabel(localization.format("Chat_Voice_Out_Toggle_Label"), ESleekSide.RIGHT);
        chatVoiceOutToggle.onToggled += onToggledChatVoiceOutToggle;
        optionsBox.AddChild(chatVoiceOutToggle);
        num += 50;
        hintsToggle = Glazier.Get().CreateToggle();
        hintsToggle.positionOffset_Y = num;
        hintsToggle.sizeOffset_X = 40;
        hintsToggle.sizeOffset_Y = 40;
        hintsToggle.addLabel(localization.format("Hints_Toggle_Label"), ESleekSide.RIGHT);
        hintsToggle.onToggled += onToggledHintsToggle;
        optionsBox.AddChild(hintsToggle);
        num += 50;
        ambienceToggle = Glazier.Get().CreateToggle();
        ambienceToggle.positionOffset_Y = num;
        ambienceToggle.sizeOffset_X = 40;
        ambienceToggle.sizeOffset_Y = 40;
        ambienceToggle.addLabel(localization.format("Ambience_Toggle_Label"), ESleekSide.RIGHT);
        ambienceToggle.onToggled += onToggledAmbienceToggle;
        optionsBox.AddChild(ambienceToggle);
        num += 50;
        streamerToggle = Glazier.Get().CreateToggle();
        streamerToggle.positionOffset_Y = num;
        streamerToggle.sizeOffset_X = 40;
        streamerToggle.sizeOffset_Y = 40;
        streamerToggle.addLabel(localization.format("Streamer_Toggle_Label"), ESleekSide.RIGHT);
        streamerToggle.onToggled += onToggledStreamerToggle;
        optionsBox.AddChild(streamerToggle);
        num += 50;
        featuredWorkshopToggle = Glazier.Get().CreateToggle();
        featuredWorkshopToggle.positionOffset_Y = num;
        featuredWorkshopToggle.sizeOffset_X = 40;
        featuredWorkshopToggle.sizeOffset_Y = 40;
        featuredWorkshopToggle.addLabel(localization.format("Featured_Workshop_Toggle_Label"), ESleekSide.RIGHT);
        featuredWorkshopToggle.onToggled += onToggledFeaturedWorkshopToggle;
        optionsBox.AddChild(featuredWorkshopToggle);
        num += 50;
        showHotbarToggle = Glazier.Get().CreateToggle();
        showHotbarToggle.positionOffset_Y = num;
        showHotbarToggle.sizeOffset_X = 40;
        showHotbarToggle.sizeOffset_Y = 40;
        showHotbarToggle.addLabel(localization.format("Show_Hotbar_Toggle_Label"), ESleekSide.RIGHT);
        showHotbarToggle.onToggled += onToggledShowHotbarToggle;
        optionsBox.AddChild(showHotbarToggle);
        num += 50;
        pauseWhenUnfocusedToggle = Glazier.Get().CreateToggle();
        pauseWhenUnfocusedToggle.positionOffset_Y = num;
        pauseWhenUnfocusedToggle.sizeOffset_X = 40;
        pauseWhenUnfocusedToggle.sizeOffset_Y = 40;
        pauseWhenUnfocusedToggle.addLabel(localization.format("Pause_When_Unfocused_Label"), ESleekSide.RIGHT);
        pauseWhenUnfocusedToggle.onToggled += onToggledPauseWhenUnfocusedToggle;
        optionsBox.AddChild(pauseWhenUnfocusedToggle);
        num += 50;
        fovSlider = Glazier.Get().CreateSlider();
        fovSlider.positionOffset_Y = num;
        fovSlider.sizeOffset_X = 200;
        fovSlider.sizeOffset_Y = 20;
        fovSlider.orientation = ESleekOrientation.HORIZONTAL;
        fovSlider.onDragged += onDraggedFOVSlider;
        optionsBox.AddChild(fovSlider);
        fovLabel = Glazier.Get().CreateLabel();
        fovLabel.positionOffset_X = 5;
        fovLabel.positionOffset_Y = -30;
        fovLabel.positionScale_X = 1f;
        fovLabel.positionScale_Y = 0.5f;
        fovLabel.sizeOffset_X = 300;
        fovLabel.sizeOffset_Y = 60;
        fovLabel.fontAlignment = TextAnchor.MiddleLeft;
        fovLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        fovLabel.text = FormatFieldOfViewTooltip();
        fovSlider.AddChild(fovLabel);
        num += 30;
        volumeSlider = Glazier.Get().CreateSlider();
        volumeSlider.positionOffset_Y = num;
        volumeSlider.sizeOffset_X = 200;
        volumeSlider.sizeOffset_Y = 20;
        volumeSlider.orientation = ESleekOrientation.HORIZONTAL;
        volumeSlider.addLabel(localization.format("Volume_Slider_Label", (int)(OptionsSettings.volume * 100f)), ESleekSide.RIGHT);
        volumeSlider.onDragged += onDraggedVolumeSlider;
        optionsBox.AddChild(volumeSlider);
        num += 30;
        voiceSlider = Glazier.Get().CreateSlider();
        voiceSlider.positionOffset_Y = num;
        voiceSlider.sizeOffset_X = 200;
        voiceSlider.sizeOffset_Y = 20;
        voiceSlider.orientation = ESleekOrientation.HORIZONTAL;
        voiceSlider.addLabel(localization.format("Voice_Slider_Label", (int)(OptionsSettings.voiceVolume * 100f)), ESleekSide.RIGHT);
        voiceSlider.onDragged += onDraggedVoiceSlider;
        optionsBox.AddChild(voiceSlider);
        num += 30;
        loadingScreenMusicVolumeSlider = Glazier.Get().CreateSlider();
        loadingScreenMusicVolumeSlider.positionOffset_Y = num;
        loadingScreenMusicVolumeSlider.sizeOffset_X = 200;
        loadingScreenMusicVolumeSlider.sizeOffset_Y = 20;
        loadingScreenMusicVolumeSlider.orientation = ESleekOrientation.HORIZONTAL;
        loadingScreenMusicVolumeSlider.addLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", (int)(OptionsSettings.loadingScreenMusicVolume * 100f)), ESleekSide.RIGHT);
        loadingScreenMusicVolumeSlider.onDragged += onDraggedLoadingScreenMusicVolumeSlider;
        optionsBox.AddChild(loadingScreenMusicVolumeSlider);
        num += 30;
        matchmakingShowAllMapsToggle = Glazier.Get().CreateToggle();
        matchmakingShowAllMapsToggle.positionOffset_Y = num;
        matchmakingShowAllMapsToggle.sizeOffset_X = 40;
        matchmakingShowAllMapsToggle.sizeOffset_Y = 40;
        matchmakingShowAllMapsToggle.addLabel(localization.format("Matchmaking_Show_All_Maps_Toggle_Label"), ESleekSide.RIGHT);
        matchmakingShowAllMapsToggle.onToggled += onToggledMatchmakingShowAllMapsToggle;
        optionsBox.AddChild(matchmakingShowAllMapsToggle);
        num += 50;
        minMatchmakingPlayersField = Glazier.Get().CreateInt32Field();
        minMatchmakingPlayersField.positionOffset_Y = num;
        minMatchmakingPlayersField.sizeOffset_X = 200;
        minMatchmakingPlayersField.sizeOffset_Y = 30;
        minMatchmakingPlayersField.addLabel(localization.format("Min_Matchmaking_Players_Field_Label"), ESleekSide.RIGHT);
        minMatchmakingPlayersField.onTypedInt += onTypedMinMatchmakingPlayersField;
        optionsBox.AddChild(minMatchmakingPlayersField);
        num += 40;
        maxMatchmakingPingField = Glazier.Get().CreateInt32Field();
        maxMatchmakingPingField.positionOffset_Y = num;
        maxMatchmakingPingField.sizeOffset_X = 200;
        maxMatchmakingPingField.sizeOffset_Y = 30;
        maxMatchmakingPingField.addLabel(localization.format("Max_Matchmaking_Ping_Field_Label"), ESleekSide.RIGHT);
        maxMatchmakingPingField.onTypedInt += onTypedMaxMatchmakingPingField;
        optionsBox.AddChild(maxMatchmakingPingField);
        num += 40;
        screenshotSizeMultiplierField = Glazier.Get().CreateInt32Field();
        screenshotSizeMultiplierField.positionOffset_Y = num;
        screenshotSizeMultiplierField.sizeOffset_X = 200;
        screenshotSizeMultiplierField.sizeOffset_Y = 30;
        screenshotSizeMultiplierField.addLabel(localization.format("ScreenshotSizeMultiplier_Label"), ESleekSide.RIGHT);
        screenshotSizeMultiplierField.tooltipText = localization.format("ScreenshotSizeMultiplier_Tooltip");
        screenshotSizeMultiplierField.onTypedInt += OnScreenshotSizeMultiplierChanged;
        optionsBox.AddChild(screenshotSizeMultiplierField);
        num += 40;
        screenshotSupersamplingToggle = Glazier.Get().CreateToggle();
        screenshotSupersamplingToggle.positionOffset_Y = num;
        screenshotSupersamplingToggle.sizeOffset_X = 40;
        screenshotSupersamplingToggle.sizeOffset_Y = 40;
        screenshotSupersamplingToggle.addLabel(localization.format("ScreenshotSupersampling_Label"), ESleekSide.RIGHT);
        screenshotSupersamplingToggle.tooltipText = localization.format("ScreenshotSupersampling_Tooltip");
        screenshotSupersamplingToggle.onToggled += OnScreenshotSupersamplingChanged;
        optionsBox.AddChild(screenshotSupersamplingToggle);
        num += 50;
        screenshotsWhileLoadingToggle = Glazier.Get().CreateToggle();
        screenshotsWhileLoadingToggle.positionOffset_Y = num;
        screenshotsWhileLoadingToggle.sizeOffset_X = 40;
        screenshotsWhileLoadingToggle.sizeOffset_Y = 40;
        screenshotsWhileLoadingToggle.addLabel(localization.format("ScreenshotsWhileLoading_Label"), ESleekSide.RIGHT);
        screenshotsWhileLoadingToggle.tooltipText = localization.format("ScreenshotsWhileLoading_Tooltip");
        screenshotsWhileLoadingToggle.onToggled += OnScreenshotsWhileLoadingChanged;
        optionsBox.AddChild(screenshotsWhileLoadingToggle);
        num += 50;
        staticCrosshairToggle = Glazier.Get().CreateToggle();
        staticCrosshairToggle.positionOffset_Y = num;
        staticCrosshairToggle.sizeOffset_X = 40;
        staticCrosshairToggle.sizeOffset_Y = 40;
        staticCrosshairToggle.addLabel(localization.format("UseStaticCrosshair_Label"), ESleekSide.RIGHT);
        staticCrosshairToggle.tooltipText = localization.format("UseStaticCrosshair_Tooltip");
        staticCrosshairToggle.onToggled += OnUseStaticCrosshairChanged;
        optionsBox.AddChild(staticCrosshairToggle);
        num += 50;
        staticCrosshairSizeSlider = Glazier.Get().CreateSlider();
        staticCrosshairSizeSlider.positionOffset_Y = num;
        staticCrosshairSizeSlider.sizeOffset_X = 200;
        staticCrosshairSizeSlider.sizeOffset_Y = 20;
        staticCrosshairSizeSlider.orientation = ESleekOrientation.HORIZONTAL;
        staticCrosshairSizeSlider.addLabel(localization.format("StaticCrosshairSize_Label"), ESleekSide.RIGHT);
        staticCrosshairSizeSlider.onDragged += OnStaticCrosshairSizeChanged;
        optionsBox.AddChild(staticCrosshairSizeSlider);
        num += 30;
        crosshairShapeButton = new SleekButtonState(new GUIContent(localization.format("CrosshairShape_Line")), new GUIContent(localization.format("CrosshairShape_Classic")));
        crosshairShapeButton.positionOffset_Y = num;
        crosshairShapeButton.sizeOffset_X = 200;
        crosshairShapeButton.sizeOffset_Y = 30;
        crosshairShapeButton.state = (int)OptionsSettings.crosshairShape;
        crosshairShapeButton.addLabel(localization.format("CrosshairShape_Label"), ESleekSide.RIGHT);
        crosshairShapeButton.onSwappedState = OnCrosshairShapeChanged;
        optionsBox.AddChild(crosshairShapeButton);
        num += 40;
        talkButton = new SleekButtonState(new GUIContent(localization.format("Talk_Off")), new GUIContent(localization.format("Talk_On")));
        talkButton.positionOffset_Y = num;
        talkButton.sizeOffset_X = 200;
        talkButton.sizeOffset_Y = 30;
        talkButton.state = (OptionsSettings.talk ? 1 : 0);
        talkButton.tooltip = localization.format("Talk_Tooltip");
        talkButton.onSwappedState = onSwappedTalkState;
        optionsBox.AddChild(talkButton);
        num += 40;
        metricButton = new SleekButtonState(new GUIContent(localization.format("Metric_Off")), new GUIContent(localization.format("Metric_On")));
        metricButton.positionOffset_Y = num;
        metricButton.sizeOffset_X = 200;
        metricButton.sizeOffset_Y = 30;
        metricButton.state = (OptionsSettings.metric ? 1 : 0);
        metricButton.tooltip = localization.format("Metric_Tooltip");
        metricButton.onSwappedState = onSwappedMetricState;
        optionsBox.AddChild(metricButton);
        num += 40;
        uiButton = new SleekButtonState(new GUIContent(localization.format("UI_Free")), new GUIContent(localization.format("UI_Pro")));
        uiButton.positionOffset_Y = num;
        uiButton.sizeOffset_X = 200;
        uiButton.sizeOffset_Y = 30;
        uiButton.tooltip = localization.format("UI_Tooltip");
        uiButton.onSwappedState = onSwappedUIState;
        optionsBox.AddChild(uiButton);
        num += 40;
        hitmarkerButton = new SleekButtonState(new GUIContent(localization.format("Hitmarker_Static")), new GUIContent(localization.format("Hitmarker_Dynamic")));
        hitmarkerButton.positionOffset_Y = num;
        hitmarkerButton.sizeOffset_X = 200;
        hitmarkerButton.sizeOffset_Y = 30;
        hitmarkerButton.tooltip = localization.format("Hitmarker_Tooltip");
        hitmarkerButton.onSwappedState = onSwappedHitmarkerState;
        optionsBox.AddChild(hitmarkerButton);
        num += 40;
        hitmarkerStyleButton = new SleekButtonState(new GUIContent(localization.format("HitmarkerStyle_Animated")), new GUIContent(localization.format("HitmarkerStyle_Classic")));
        hitmarkerStyleButton.positionOffset_Y = num;
        hitmarkerStyleButton.sizeOffset_X = 200;
        hitmarkerStyleButton.sizeOffset_Y = 30;
        hitmarkerStyleButton.addLabel(localization.format("HitmarkerStyle_Label"), ESleekSide.RIGHT);
        hitmarkerStyleButton.onSwappedState = onSwappedHitmarkerStyleState;
        optionsBox.AddChild(hitmarkerStyleButton);
        num += 40;
        crosshairBox = Glazier.Get().CreateBox();
        crosshairBox.positionOffset_Y = num;
        crosshairBox.sizeOffset_X = 240;
        crosshairBox.sizeOffset_Y = 30;
        crosshairBox.text = localization.format("Crosshair_Box");
        optionsBox.AddChild(crosshairBox);
        num += 40;
        crosshairColorPicker = new SleekColorPicker();
        crosshairColorPicker.positionOffset_Y = num;
        crosshairColorPicker.onColorPicked = onCrosshairColorPicked;
        crosshairColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(crosshairColorPicker);
        num += 160;
        hitmarkerBox = Glazier.Get().CreateBox();
        hitmarkerBox.positionOffset_Y = num;
        hitmarkerBox.sizeOffset_X = 240;
        hitmarkerBox.sizeOffset_Y = 30;
        hitmarkerBox.text = localization.format("Hitmarker_Box");
        optionsBox.AddChild(hitmarkerBox);
        num += 40;
        hitmarkerColorPicker = new SleekColorPicker();
        hitmarkerColorPicker.positionOffset_Y = num;
        hitmarkerColorPicker.onColorPicked = onHitmarkerColorPicked;
        hitmarkerColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(hitmarkerColorPicker);
        num += 160;
        criticalHitmarkerBox = Glazier.Get().CreateBox();
        criticalHitmarkerBox.positionOffset_Y = num;
        criticalHitmarkerBox.sizeOffset_X = 240;
        criticalHitmarkerBox.sizeOffset_Y = 30;
        criticalHitmarkerBox.text = localization.format("Critical_Hitmarker_Box");
        optionsBox.AddChild(criticalHitmarkerBox);
        num += 40;
        criticalHitmarkerColorPicker = new SleekColorPicker();
        criticalHitmarkerColorPicker.positionOffset_Y = num;
        criticalHitmarkerColorPicker.onColorPicked = onCriticalHitmarkerColorPicked;
        criticalHitmarkerColorPicker.SetAllowAlpha(allowAlpha: true);
        optionsBox.AddChild(criticalHitmarkerColorPicker);
        num += 160;
        cursorBox = Glazier.Get().CreateBox();
        cursorBox.positionOffset_Y = num;
        cursorBox.sizeOffset_X = 240;
        cursorBox.sizeOffset_Y = 30;
        cursorBox.text = localization.format("Cursor_Box");
        optionsBox.AddChild(cursorBox);
        num += 40;
        cursorColorPicker = new SleekColorPicker();
        cursorColorPicker.positionOffset_Y = num;
        cursorColorPicker.onColorPicked = onCursorColorPicked;
        optionsBox.AddChild(cursorColorPicker);
        num += 130;
        backgroundBox = Glazier.Get().CreateBox();
        backgroundBox.positionOffset_Y = num;
        backgroundBox.sizeOffset_X = 240;
        backgroundBox.sizeOffset_Y = 30;
        backgroundBox.text = localization.format("Background_Box");
        optionsBox.AddChild(backgroundBox);
        num += 40;
        backgroundColorPicker = new SleekColorPicker();
        backgroundColorPicker.positionOffset_Y = num;
        if (Provider.isPro)
        {
            backgroundColorPicker.onColorPicked = onBackgroundColorPicked;
        }
        else
        {
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = -40;
            sleekImage.positionOffset_Y = -40;
            sleekImage.positionScale_X = 0.5f;
            sleekImage.positionScale_Y = 0.5f;
            sleekImage.sizeOffset_X = 80;
            sleekImage.sizeOffset_Y = 80;
            sleekImage.texture = bundle.load<Texture2D>("Lock_Large");
            backgroundColorPicker.AddChild(sleekImage);
            bundle.unload();
        }
        optionsBox.AddChild(backgroundColorPicker);
        num += 130;
        foregroundBox = Glazier.Get().CreateBox();
        foregroundBox.positionOffset_Y = num;
        foregroundBox.sizeOffset_X = 240;
        foregroundBox.sizeOffset_Y = 30;
        foregroundBox.text = localization.format("Foreground_Box");
        optionsBox.AddChild(foregroundBox);
        num += 40;
        foregroundColorPicker = new SleekColorPicker();
        foregroundColorPicker.positionOffset_Y = num;
        if (Provider.isPro)
        {
            foregroundColorPicker.onColorPicked = onForegroundColorPicked;
        }
        else
        {
            Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.positionOffset_X = -40;
            sleekImage2.positionOffset_Y = -40;
            sleekImage2.positionScale_X = 0.5f;
            sleekImage2.positionScale_Y = 0.5f;
            sleekImage2.sizeOffset_X = 80;
            sleekImage2.sizeOffset_Y = 80;
            sleekImage2.texture = bundle2.load<Texture2D>("Lock_Large");
            foregroundColorPicker.AddChild(sleekImage2);
            bundle2.unload();
        }
        optionsBox.AddChild(foregroundColorPicker);
        num += 130;
        fontBox = Glazier.Get().CreateBox();
        fontBox.positionOffset_Y = num;
        fontBox.sizeOffset_X = 240;
        fontBox.sizeOffset_Y = 30;
        fontBox.text = localization.format("Font_Box");
        optionsBox.AddChild(fontBox);
        num += 40;
        fontColorPicker = new SleekColorPicker();
        fontColorPicker.positionOffset_Y = num;
        if (Provider.isPro)
        {
            fontColorPicker.onColorPicked = onFontColorPicked;
        }
        else
        {
            Bundle bundle3 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage3 = Glazier.Get().CreateImage();
            sleekImage3.positionOffset_X = -40;
            sleekImage3.positionOffset_Y = -40;
            sleekImage3.positionScale_X = 0.5f;
            sleekImage3.positionScale_Y = 0.5f;
            sleekImage3.sizeOffset_X = 80;
            sleekImage3.sizeOffset_Y = 80;
            sleekImage3.texture = bundle3.load<Texture2D>("Lock_Large");
            fontColorPicker.AddChild(sleekImage3);
            bundle3.unload();
        }
        optionsBox.AddChild(fontColorPicker);
        num += 130;
        shadowBox = Glazier.Get().CreateBox();
        shadowBox.positionOffset_Y = num;
        shadowBox.sizeOffset_X = 240;
        shadowBox.sizeOffset_Y = 30;
        shadowBox.text = localization.format("Shadow_Box");
        optionsBox.AddChild(shadowBox);
        num += shadowBox.sizeOffset_Y + 10;
        shadowColorPicker = new SleekColorPicker();
        shadowColorPicker.positionOffset_Y = num;
        if (Provider.isPro)
        {
            shadowColorPicker.onColorPicked = onShadowColorPicked;
        }
        else
        {
            Bundle bundle4 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage4 = Glazier.Get().CreateImage();
            sleekImage4.positionOffset_X = -40;
            sleekImage4.positionOffset_Y = -40;
            sleekImage4.positionScale_X = 0.5f;
            sleekImage4.positionScale_Y = 0.5f;
            sleekImage4.sizeOffset_X = 80;
            sleekImage4.sizeOffset_Y = 80;
            sleekImage4.texture = bundle4.load<Texture2D>("Lock_Large");
            shadowColorPicker.AddChild(sleekImage4);
            bundle4.unload();
        }
        optionsBox.AddChild(shadowColorPicker);
        num += shadowColorPicker.sizeOffset_Y + 10;
        badColorBox = Glazier.Get().CreateBox();
        badColorBox.positionOffset_Y = num;
        badColorBox.sizeOffset_X = 240;
        badColorBox.sizeOffset_Y = 30;
        badColorBox.text = localization.format("Bad_Color_Box");
        optionsBox.AddChild(badColorBox);
        num += badColorBox.sizeOffset_Y + 10;
        badColorPicker = new SleekColorPicker();
        badColorPicker.positionOffset_Y = num;
        if (Provider.isPro)
        {
            badColorPicker.onColorPicked = onBadColorPicked;
        }
        else
        {
            Bundle bundle5 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage5 = Glazier.Get().CreateImage();
            sleekImage5.positionOffset_X = -40;
            sleekImage5.positionOffset_Y = -40;
            sleekImage5.positionScale_X = 0.5f;
            sleekImage5.positionScale_Y = 0.5f;
            sleekImage5.sizeOffset_X = 80;
            sleekImage5.sizeOffset_Y = 80;
            sleekImage5.texture = bundle5.load<Texture2D>("Lock_Large");
            badColorPicker.AddChild(sleekImage5);
            bundle5.unload();
        }
        optionsBox.AddChild(badColorPicker);
        num += badColorPicker.sizeOffset_Y;
        optionsBox.contentSizeOffset = new Vector2(0f, num);
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
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.positionOffset_X = -200;
        defaultButton.positionOffset_Y = -50;
        defaultButton.positionScale_X = 1f;
        defaultButton.positionScale_Y = 1f;
        defaultButton.sizeOffset_X = 200;
        defaultButton.sizeOffset_Y = 50;
        defaultButton.text = MenuPlayConfigUI.localization.format("Default");
        defaultButton.tooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
        defaultButton.onClickedButton += onClickedDefaultButton;
        defaultButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        updateAll();
    }
}
