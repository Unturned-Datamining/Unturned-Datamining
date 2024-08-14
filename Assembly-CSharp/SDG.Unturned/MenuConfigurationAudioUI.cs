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

    private ISleekSlider musicMasterVolumeSlider;

    private ISleekSlider loadingScreenMusicVolumeSlider;

    private ISleekSlider deathMusicVolumeSlider;

    private ISleekSlider mainMenuMusicVolumeSlider;

    private ISleekSlider ambientMusicVolumeSlider;

    private ISleekSlider gameVolumeSlider;

    private ISleekSlider voiceVolumeSlider;

    private ISleekSlider atmosphereVolumeSlider;

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

    private void OnAtmosphereVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.AtmosphereVolume = state;
        atmosphereVolumeSlider.UpdateLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")));
    }

    private void OnGameVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.gameVolume = state;
        gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
    }

    private void OnUnfocusedVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.UnfocusedVolume = state;
        unfocusedVolumeSlider.UpdateLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")));
    }

    private void OnMusicMasterVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.MusicMasterVolume = state;
        musicMasterVolumeSlider.UpdateLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")));
    }

    private void OnLoadingScreenMusicVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.loadingScreenMusicVolume = state;
        loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
    }

    private void OnDeathMusicVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.deathMusicVolume = state;
        deathMusicVolumeSlider.UpdateLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")));
    }

    private void OnMainMenuMusicVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.MainMenuMusicVolume = state;
        mainMenuMusicVolumeSlider.UpdateLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")));
    }

    private void OnAmbientMusicVolumeSliderDragged(ISleekSlider slider, float state)
    {
        OptionsSettings.ambientMusicVolume = state;
        ambientMusicVolumeSlider.UpdateLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")));
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
        musicMasterVolumeSlider.Value = OptionsSettings.MusicMasterVolume;
        musicMasterVolumeSlider.UpdateLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")));
        loadingScreenMusicVolumeSlider.Value = OptionsSettings.loadingScreenMusicVolume;
        loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
        deathMusicVolumeSlider.Value = OptionsSettings.deathMusicVolume;
        deathMusicVolumeSlider.UpdateLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")));
        mainMenuMusicVolumeSlider.Value = OptionsSettings.MainMenuMusicVolume;
        mainMenuMusicVolumeSlider.UpdateLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")));
        ambientMusicVolumeSlider.Value = OptionsSettings.ambientMusicVolume;
        ambientMusicVolumeSlider.UpdateLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")));
        voiceVolumeSlider.Value = OptionsSettings.voiceVolume;
        voiceVolumeSlider.UpdateLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")));
        gameVolumeSlider.Value = OptionsSettings.gameVolume;
        gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
        atmosphereVolumeSlider.Value = OptionsSettings.AtmosphereVolume;
        atmosphereVolumeSlider.UpdateLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")));
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
        unfocusedVolumeSlider.AddLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")), ESleekSide.RIGHT);
        unfocusedVolumeSlider.OnValueChanged += OnUnfocusedVolumeSliderDragged;
        audioBox.AddChild(unfocusedVolumeSlider);
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
        atmosphereVolumeSlider = Glazier.Get().CreateSlider();
        atmosphereVolumeSlider.PositionOffset_Y = num;
        atmosphereVolumeSlider.SizeOffset_X = 200f;
        atmosphereVolumeSlider.SizeOffset_Y = 20f;
        atmosphereVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        atmosphereVolumeSlider.AddLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")), ESleekSide.RIGHT);
        atmosphereVolumeSlider.OnValueChanged += OnAtmosphereVolumeSliderDragged;
        audioBox.AddChild(atmosphereVolumeSlider);
        num += 30;
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.PositionOffset_Y = num;
        sleekBox.SizeOffset_X = 400f;
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.Text = localization.format("Music_Header");
        audioBox.AddChild(sleekBox);
        num += 40;
        musicMasterVolumeSlider = Glazier.Get().CreateSlider();
        musicMasterVolumeSlider.PositionOffset_Y = num;
        musicMasterVolumeSlider.SizeOffset_X = 200f;
        musicMasterVolumeSlider.SizeOffset_Y = 20f;
        musicMasterVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        musicMasterVolumeSlider.AddLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")), ESleekSide.RIGHT);
        musicMasterVolumeSlider.OnValueChanged += OnMusicMasterVolumeSliderDragged;
        audioBox.AddChild(musicMasterVolumeSlider);
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
        deathMusicVolumeSlider = Glazier.Get().CreateSlider();
        deathMusicVolumeSlider.PositionOffset_Y = num;
        deathMusicVolumeSlider.SizeOffset_X = 200f;
        deathMusicVolumeSlider.SizeOffset_Y = 20f;
        deathMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        deathMusicVolumeSlider.AddLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")), ESleekSide.RIGHT);
        deathMusicVolumeSlider.OnValueChanged += OnDeathMusicVolumeSliderDragged;
        audioBox.AddChild(deathMusicVolumeSlider);
        num += 30;
        mainMenuMusicVolumeSlider = Glazier.Get().CreateSlider();
        mainMenuMusicVolumeSlider.PositionOffset_Y = num;
        mainMenuMusicVolumeSlider.SizeOffset_X = 200f;
        mainMenuMusicVolumeSlider.SizeOffset_Y = 20f;
        mainMenuMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        mainMenuMusicVolumeSlider.AddLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")), ESleekSide.RIGHT);
        mainMenuMusicVolumeSlider.OnValueChanged += OnMainMenuMusicVolumeSliderDragged;
        audioBox.AddChild(mainMenuMusicVolumeSlider);
        num += 30;
        ambientMusicVolumeSlider = Glazier.Get().CreateSlider();
        ambientMusicVolumeSlider.PositionOffset_Y = num;
        ambientMusicVolumeSlider.SizeOffset_X = 200f;
        ambientMusicVolumeSlider.SizeOffset_Y = 20f;
        ambientMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        ambientMusicVolumeSlider.AddLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")), ESleekSide.RIGHT);
        ambientMusicVolumeSlider.OnValueChanged += OnAmbientMusicVolumeSliderDragged;
        audioBox.AddChild(ambientMusicVolumeSlider);
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
