using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationAudioUI : SleekFullscreenBox
{
    public Local localization;

    public bool active;

    private SleekButtonIcon backButton;

    private ISleekButton defaultButton;

    private ISleekScrollView audioBox;

    private ISleekSlider masterVolumeSlider;

    private ISleekSlider unfocusedVolumeSlider;

    private ISleekSlider loadingScreenMusicVolumeSlider;

    private ISleekSlider gameVolumeSlider;

    private ISleekSlider voiceVolumeSlider;

    private ISleekToggle musicToggle;

    private ISleekToggle ambienceToggle;

    public void open()
    {
        if (!active)
        {
            active = true;
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            OptionsSettings.save();
            AnimateOutOfView(0f, 1f);
        }
    }

    private void OnVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.volume = state;
        OptionsSettings.apply();
        masterVolumeSlider.UpdateLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")));
    }

    private void OnVoiceVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.voiceVolume = state;
        voiceVolumeSlider.UpdateLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")));
    }

    private void OnGameVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.gameVolume = state;
        gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
    }

    private static void OnMusicToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.music = state;
        OptionsSettings.apply();
    }

    private static void OnAmbienceToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.ambience = state;
        OptionsSettings.apply();
    }

    private void OnUnfocusedVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.UnfocusedVolume = state;
        unfocusedVolumeSlider.UpdateLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")));
    }

    private void OnLoadingScreenMusicVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.loadingScreenMusicVolume = state;
        loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
    }

    private void onClickedBackButton(ISleekElement button)
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

    private void onClickedDefaultButton(ISleekElement button)
    {
        OptionsSettings.RestoreAudioDefaults();
        updateAll();
    }

    private void updateAll()
    {
        masterVolumeSlider.Value = OptionsSettings.volume;
        masterVolumeSlider.UpdateLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")));
        unfocusedVolumeSlider.Value = OptionsSettings.UnfocusedVolume;
        unfocusedVolumeSlider.UpdateLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")));
        loadingScreenMusicVolumeSlider.Value = OptionsSettings.loadingScreenMusicVolume;
        loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
        voiceVolumeSlider.Value = OptionsSettings.voiceVolume;
        voiceVolumeSlider.UpdateLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")));
        gameVolumeSlider.Value = OptionsSettings.gameVolume;
        gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
        musicToggle.Value = OptionsSettings.music;
        ambienceToggle.Value = OptionsSettings.ambience;
    }

    public MenuConfigurationAudioUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationAudio.dat");
        new Color32(240, 240, 240, byte.MaxValue);
        new Color32(180, 180, 180, byte.MaxValue);
        active = false;
        audioBox = Glazier.Get().CreateScrollView();
        audioBox.PositionOffset_X = -200f;
        audioBox.PositionOffset_Y = 100f;
        audioBox.PositionScale_X = 0.5f;
        audioBox.SizeOffset_X = 430f;
        audioBox.SizeOffset_Y = -200f;
        audioBox.SizeScale_Y = 1f;
        audioBox.ScaleContentToWidth = true;
        AddChild(audioBox);
        int num = 0;
        musicToggle = Glazier.Get().CreateToggle();
        musicToggle.PositionOffset_Y = num;
        musicToggle.SizeOffset_X = 40f;
        musicToggle.SizeOffset_Y = 40f;
        musicToggle.AddLabel(localization.format("Music_Toggle_Label"), ESleekSide.RIGHT);
        musicToggle.TooltipText = localization.format("Music_Toggle_Tooltip");
        musicToggle.OnValueChanged += OnMusicToggled;
        audioBox.AddChild(musicToggle);
        num += 50;
        ambienceToggle = Glazier.Get().CreateToggle();
        ambienceToggle.PositionOffset_Y = num;
        ambienceToggle.SizeOffset_X = 40f;
        ambienceToggle.SizeOffset_Y = 40f;
        ambienceToggle.AddLabel(localization.format("Ambience_Toggle_Label"), ESleekSide.RIGHT);
        ambienceToggle.TooltipText = localization.format("Ambience_Toggle_Tooltip");
        ambienceToggle.OnValueChanged += OnAmbienceToggled;
        audioBox.AddChild(ambienceToggle);
        num += 50;
        masterVolumeSlider = Glazier.Get().CreateSlider();
        masterVolumeSlider.PositionOffset_Y = num;
        masterVolumeSlider.SizeOffset_X = 200f;
        masterVolumeSlider.SizeOffset_Y = 20f;
        masterVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        masterVolumeSlider.AddLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")), ESleekSide.RIGHT);
        masterVolumeSlider.OnValueChanged += OnVolumeSliderDragged;
        audioBox.AddChild(masterVolumeSlider);
        num += 30;
        gameVolumeSlider = Glazier.Get().CreateSlider();
        gameVolumeSlider.PositionOffset_Y = num;
        gameVolumeSlider.SizeOffset_X = 200f;
        gameVolumeSlider.SizeOffset_Y = 20f;
        gameVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        gameVolumeSlider.AddLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")), ESleekSide.RIGHT);
        gameVolumeSlider.OnValueChanged += OnGameVolumeSliderDragged;
        audioBox.AddChild(gameVolumeSlider);
        num += 30;
        unfocusedVolumeSlider = Glazier.Get().CreateSlider();
        unfocusedVolumeSlider.PositionOffset_Y = num;
        unfocusedVolumeSlider.SizeOffset_X = 200f;
        unfocusedVolumeSlider.SizeOffset_Y = 20f;
        unfocusedVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        unfocusedVolumeSlider.AddLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")), ESleekSide.RIGHT);
        unfocusedVolumeSlider.OnValueChanged += OnUnfocusedVolumeSliderDragged;
        audioBox.AddChild(unfocusedVolumeSlider);
        num += 30;
        loadingScreenMusicVolumeSlider = Glazier.Get().CreateSlider();
        loadingScreenMusicVolumeSlider.PositionOffset_Y = num;
        loadingScreenMusicVolumeSlider.SizeOffset_X = 200f;
        loadingScreenMusicVolumeSlider.SizeOffset_Y = 20f;
        loadingScreenMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        loadingScreenMusicVolumeSlider.AddLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")), ESleekSide.RIGHT);
        loadingScreenMusicVolumeSlider.OnValueChanged += OnLoadingScreenMusicVolumeSliderDragged;
        audioBox.AddChild(loadingScreenMusicVolumeSlider);
        num += 30;
        voiceVolumeSlider = Glazier.Get().CreateSlider();
        voiceVolumeSlider.PositionOffset_Y = num;
        voiceVolumeSlider.SizeOffset_X = 200f;
        voiceVolumeSlider.SizeOffset_Y = 20f;
        voiceVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        voiceVolumeSlider.AddLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")), ESleekSide.RIGHT);
        voiceVolumeSlider.OnValueChanged += OnVoiceVolumeSliderDragged;
        audioBox.AddChild(voiceVolumeSlider);
        num += 30;
        audioBox.ContentSizeOffset = new Vector2(0f, num - 10);
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
        AddChild(backButton);
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
        AddChild(defaultButton);
        updateAll();
    }
}
