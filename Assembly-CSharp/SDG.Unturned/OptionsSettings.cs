using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDG.Unturned;

public class OptionsSettings
{
    private const byte SAVEDATA_VERSION_ADDED_LOADING_SCREEN_MUSIC = 37;

    private const byte SAVEDATA_VERSION_ADDED_SCREENSHOT_SIZE = 38;

    private const byte SAVEDATA_VERSION_ADDED_SCREENSHOT_SUPERSAMPLING = 39;

    private const byte SAVEDATA_VERSION_ADDED_LOADING_SCREEN_SCREENSHOTS = 40;

    private const byte SAVEDATA_VERSION_ADDED_STATIC_CROSSHAIR = 41;

    private const byte SAVEDATA_VERSION_ADDED_CROSSHAIR_SHAPE = 42;

    private const byte SAVEDATA_VERSION_ADDED_CROSSHAIR_AND_HITMARKER_ALPHA = 43;

    private const byte SAVEDATA_VERSION_ADDED_HITMARKER_STYLE = 44;

    /// <summary>
    /// Unfortunately the version which added hitmarker style saved but didn't actually load (sigh).
    /// </summary>
    private const byte SAVEDATA_VERSION_ADDED_HITMARKER_STYLE_FIX = 45;

    private const byte SAVEDATA_VERSION_REMOVED_MATCHMAKING = 46;

    private const byte SAVEDATA_VERSION_ADDED_VOICE_ALWAYS_RECORDING = 47;

    /// <summary>
    /// Nelson 2023-12-28: this option was causing players to crash in the 3.23.14.0 update. Hopefully
    /// it's resolved for the patch, but to be safe it will default to false.
    /// </summary>
    private const byte SAVEDATA_VERSION_RESET_VOICE_ALWAYS_RECORDING = 48;

    private const byte SAVEDATA_VERSION_ADDED_NAMETAG_FADEOUT_OPT = 49;

    private const byte SAVEDATA_VERSION_ADDED_GAME_VOLUME = 50;

    private const byte SAVEDATA_VERSION_ADDED_UNFOCUSED_VOLUME = 51;

    private const byte SAVEDATA_VERSION_ADDED_VEHICLE_THIRD_PERSON_CAMERA_MODE = 52;

    private const byte SAVEDATA_VERSION_REPLACTED_MUSIC_TOGGLE_WITH_VOLUMES = 53;

    private const byte SAVEDATA_VERSION_ADDED_MUSIC_MASTER_VOLUME = 54;

    private const byte SAVEDATA_VERSION_ADDED_ATMOSPHERE_VOLUME = 55;

    private const byte SAVEDATA_VERSION_SEPARATED_AIRCRAFT_THIRD_PERSON_CAMERA_MODE = 56;

    private const byte SAVEDATA_VERSION_ADDED_ONLINE_SAFETY_MENU = 57;

    private const byte SAVEDATA_VERSION_NEWEST = 57;

    public static readonly byte SAVEDATA_VERSION = 57;

    public static readonly byte MIN_FOV = 60;

    public static readonly byte MAX_FOV = 40;

    private static float _fov;

    private static float _cachedVerticalFOV;

    public const float DEFAULT_MASTER_VOLUME = 1f;

    public static float volume;

    public const float DEFAULT_UNFOCUSED_VOLUME = 0.5f;

    public const float DEFAULT_MUSIC_MASTER_VOLUME = 0.7f;

    private static float _musicMasterVolume;

    private const float DEFAULT_GAME_VOLUME = 0.7f;

    private static float _gameVolume;

    private const float DEFAULT_VOICE_VOLUME = 0.7f;

    private static float _voiceVolume;

    private const float DEFAULT_LOADING_SCREEN_MUSIC_VOLUME = 0.5f;

    public static float loadingScreenMusicVolume;

    public const float DEFAULT_DEATH_MUSIC_VOLUME = 0.7f;

    public static float deathMusicVolume;

    public const float DEFAULT_MAIN_MENU_MUSIC_VOLUME = 0.7f;

    private static float _mainMenuMusicVolume;

    public const float DEFAULT_ATMOSPHERE_VOLUME = 0.7f;

    private static float _atmosphereVolume;

    public const float DEFAULT_AMBIENT_MUSIC_VOLUME = 0.7f;

    public static float ambientMusicVolume;

    public static bool debug;

    public static bool splashscreen;

    public static bool timer;

    public static bool gore;

    public static bool filter;

    public static bool chatText;

    public static bool chatVoiceIn;

    public static bool chatVoiceOut;

    private static bool _metric;

    public static bool talk;

    public static bool hints;

    public static bool ambience;

    private static bool _voiceAlwaysRecording;

    /// <summary>
    /// If true, group member name labels fade out when near the center of the screen.
    /// Defaults to true.
    /// </summary>
    public static bool shouldNametagFadeOut;

    [Obsolete("Renamed to ShouldHitmarkersFollowWorldPosition")]
    public static bool hitmarker;

    public static bool streamer;

    public static bool featuredWorkshop;

    public static bool showHotbar;

    public static bool pauseWhenUnfocused;

    public static int screenshotSizeMultiplier;

    public static bool enableScreenshotSupersampling;

    public static bool enableScreenshotsOnLoadingScreen;

    public static bool useStaticCrosshair;

    public static float staticCrosshairSize;

    public static ECrosshairShape crosshairShape;

    /// <summary>
    /// Controls whether hitmarkers are animated outward (newer) or just a static image ("classic"). 
    /// </summary>
    public static EHitmarkerStyle hitmarkerStyle;

    /// <summary>
    /// Determines how camera follows vehicle in third-person view.
    /// </summary>
    public static EVehicleThirdPersonCameraMode vehicleThirdPersonCameraMode;

    /// <summary>
    /// Determines how camera follows aircraft vehicle in third-person view.
    /// </summary>
    public static EVehicleThirdPersonCameraMode vehicleAircraftThirdPersonCameraMode;

    public static Color crosshairColor;

    public static Color hitmarkerColor;

    public static Color criticalHitmarkerColor;

    /// <summary>
    /// Number of times the player has clicked "Proceed" in the online safety menu.
    /// </summary>
    public static int onlineSafetyMenuProceedCount;

    /// <summary>
    /// If true, "don't show again" is checked in the online safety menu.
    /// </summary>
    public static bool wantsToHideOnlineSafetyMenu;

    /// <summary>
    /// Prevents menu from being shown twice without a restart.
    /// </summary>
    internal static bool didShowOnlineSafetyMenuThisSession;

    public static float fov
    {
        get
        {
            return _fov;
        }
        set
        {
            _fov = value;
            CacheVerticalFOV();
        }
    }

    public static float DesiredVerticalFieldOfView => _cachedVerticalFOV;

    public static float UnfocusedVolume
    {
        get
        {
            return UnturnedMasterVolume.UnfocusedVolume;
        }
        set
        {
            UnturnedMasterVolume.UnfocusedVolume = value;
        }
    }

    public static float MusicMasterVolume
    {
        get
        {
            return _musicMasterVolume;
        }
        set
        {
            _musicMasterVolume = value;
            UnturnedAudioMixer.SetMusicMasterVolume(value);
        }
    }

    public static float gameVolume
    {
        get
        {
            return _gameVolume;
        }
        set
        {
            _gameVolume = value;
            UnturnedAudioMixer.SetDefaultVolume(value);
        }
    }

    public static float voiceVolume
    {
        get
        {
            return _voiceVolume;
        }
        set
        {
            _voiceVolume = value;
            UnturnedAudioMixer.SetVoiceVolume(value);
        }
    }

    public static float MainMenuMusicVolume
    {
        get
        {
            return _mainMenuMusicVolume;
        }
        set
        {
            _mainMenuMusicVolume = value;
            UnturnedAudioMixer.SetMainMenuMusicVolume(value);
        }
    }

    public static float AtmosphereVolume
    {
        get
        {
            return _atmosphereVolume;
        }
        set
        {
            _atmosphereVolume = value;
            UnturnedAudioMixer.SetAtmosphereVolume(value);
        }
    }

    public static bool metric
    {
        get
        {
            return _metric;
        }
        set
        {
            if (_metric != value)
            {
                _metric = value;
                OptionsSettings.OnUnitSystemChanged?.Invoke();
            }
        }
    }

    public static bool proUI
    {
        get
        {
            return SleekCustomization.darkTheme;
        }
        set
        {
            SleekCustomization.darkTheme = value;
            OptionsSettings.OnThemeChanged?.Invoke();
        }
    }

    public static bool ShouldHitmarkersFollowWorldPosition
    {
        get
        {
            return hitmarker;
        }
        set
        {
            hitmarker = value;
        }
    }

    /// <summary>
    /// If false, call Start and Stop recording before and after push-to-talk key is pressed. This was the
    /// original default behavior, but causes a hitch for some players. As a workaround we can always keep
    /// the microphone rolling and only send data when the push-to-talk key is held. (public issue #4248)
    /// </summary>
    public static bool VoiceAlwaysRecording
    {
        get
        {
            return _voiceAlwaysRecording;
        }
        set
        {
            if (_voiceAlwaysRecording != value)
            {
                _voiceAlwaysRecording = value;
                OptionsSettings.OnVoiceAlwaysRecordingChanged?.Invoke();
            }
        }
    }

    public static Color cursorColor
    {
        get
        {
            return SleekCustomization.cursorColor;
        }
        set
        {
            SleekCustomization.cursorColor = value;
        }
    }

    public static Color backgroundColor
    {
        get
        {
            return SleekCustomization.backgroundColor;
        }
        set
        {
            SleekCustomization.backgroundColor = value;
            OptionsSettings.OnCustomColorsChanged?.Invoke();
        }
    }

    public static Color foregroundColor
    {
        get
        {
            return SleekCustomization.foregroundColor;
        }
        set
        {
            SleekCustomization.foregroundColor = value;
            OptionsSettings.OnCustomColorsChanged?.Invoke();
        }
    }

    public static Color fontColor
    {
        get
        {
            return SleekCustomization.fontColor;
        }
        set
        {
            SleekCustomization.fontColor = value;
            OptionsSettings.OnCustomColorsChanged?.Invoke();
        }
    }

    public static Color shadowColor
    {
        get
        {
            return SleekCustomization.shadowColor;
        }
        set
        {
            SleekCustomization.shadowColor = value;
            OptionsSettings.OnCustomColorsChanged?.Invoke();
        }
    }

    public static Color badColor
    {
        get
        {
            return SleekCustomization.badColor;
        }
        set
        {
            SleekCustomization.badColor = value;
            OptionsSettings.OnCustomColorsChanged?.Invoke();
        }
    }

    internal static bool ShouldShowOnlineSafetyMenu
    {
        get
        {
            if (!wantsToHideOnlineSafetyMenu || onlineSafetyMenuProceedCount < 1)
            {
                return !didShowOnlineSafetyMenuThisSession;
            }
            return false;
        }
    }

    public static event System.Action OnUnitSystemChanged;

    public static event System.Action OnVoiceAlwaysRecordingChanged;

    /// <summary>
    /// Invoked when custom UI colors are set.
    /// </summary>
    public static event System.Action OnCustomColorsChanged;

    /// <summary>
    /// Invoked when dark/light theme is set.
    /// </summary>
    public static event System.Action OnThemeChanged;

    /// <summary>
    /// Prior to 3.22.8.0 all scopes/optics had a base fov of 90 degrees.
    /// </summary>
    public static float GetZoomBaseFieldOfView()
    {
        if (ControlsSettings.sensitivityScalingMode != ESensitivityScalingMode.Legacy)
        {
            return DesiredVerticalFieldOfView;
        }
        return 90f;
    }

    private static void CacheVerticalFOV()
    {
        if (Provider.preferenceData != null && Provider.preferenceData.Graphics != null && Provider.preferenceData.Graphics.Override_Vertical_Field_Of_View > 0.5f)
        {
            _cachedVerticalFOV = Mathf.Clamp(Provider.preferenceData.Graphics.Override_Vertical_Field_Of_View, 1f, 100f);
        }
        else
        {
            _cachedVerticalFOV = (float)(int)MIN_FOV + (float)(int)MAX_FOV * _fov;
        }
    }

    public static void apply()
    {
        if (!Level.isLoaded && MainCamera.instance != null && !Level.isVR && !Dedicator.isVR)
        {
            MainCamera.instance.fieldOfView = DesiredVerticalFieldOfView;
        }
        if (SceneManager.GetActiveScene().buildIndex <= Level.BUILD_INDEX_MENU)
        {
            MenuConfigurationOptions.apply();
        }
        UnturnedMasterVolume.preferredVolume = volume;
    }

    public static void RestoreAudioDefaults()
    {
        volume = 1f;
        UnfocusedVolume = 0.5f;
        gameVolume = 0.7f;
        MusicMasterVolume = 0.7f;
        loadingScreenMusicVolume = 0.5f;
        deathMusicVolume = 0.7f;
        MainMenuMusicVolume = 0.7f;
        ambientMusicVolume = 0.7f;
        voiceVolume = 0.7f;
        AtmosphereVolume = 0.7f;
    }

    public static void restoreDefaults()
    {
        splashscreen = true;
        timer = false;
        fov = 0.75f;
        debug = false;
        gore = true;
        filter = true;
        chatText = true;
        chatVoiceIn = false;
        chatVoiceOut = false;
        metric = true;
        talk = false;
        hints = true;
        proUI = true;
        ShouldHitmarkersFollowWorldPosition = false;
        streamer = false;
        featuredWorkshop = true;
        showHotbar = true;
        pauseWhenUnfocused = true;
        screenshotSizeMultiplier = 1;
        enableScreenshotSupersampling = true;
        enableScreenshotsOnLoadingScreen = true;
        useStaticCrosshair = false;
        staticCrosshairSize = 0.1f;
        crosshairShape = ECrosshairShape.Line;
        hitmarkerStyle = EHitmarkerStyle.Animated;
        vehicleThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationDetached;
        vehicleAircraftThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationAttached;
        crosshairColor = new Color(1f, 1f, 1f, 0.5f);
        hitmarkerColor = new Color(1f, 1f, 1f, 0.5f);
        criticalHitmarkerColor = new Color(1f, 0f, 0f, 0.5f);
        cursorColor = Color.white;
        backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        foregroundColor = new Color(0.9f, 0.9f, 0.9f);
        fontColor = new Color(0.9f, 0.9f, 0.9f);
        shadowColor = Color.black;
        badColor = Palette.COLOR_R;
        _voiceAlwaysRecording = false;
        shouldNametagFadeOut = true;
    }

    public static void load()
    {
        restoreDefaults();
        RestoreAudioDefaults();
        if (!ReadWrite.fileExists("/Options.dat", useCloud: true))
        {
            return;
        }
        Block block = ReadWrite.readBlock("/Options.dat", useCloud: true, 0);
        if (block == null)
        {
            return;
        }
        byte b = block.readByte();
        if (b <= 2)
        {
            return;
        }
        bool flag = block.readBoolean();
        if (b < 31)
        {
            splashscreen = true;
        }
        else
        {
            splashscreen = block.readBoolean();
        }
        if (b < 20)
        {
            timer = false;
        }
        else
        {
            timer = block.readBoolean();
        }
        if (b < 10)
        {
            block.readBoolean();
        }
        if (b > 7)
        {
            fov = block.readSingle();
        }
        else
        {
            fov = block.readSingle() * 0.5f;
        }
        if (b < 24)
        {
            fov *= 1.5f;
            fov = Mathf.Clamp01(fov);
        }
        if (b > 4)
        {
            volume = block.readSingle();
        }
        else
        {
            volume = 1f;
        }
        if (b > 22)
        {
            voiceVolume = block.readSingle();
            if (b < 36)
            {
                voiceVolume = Mathf.Min(voiceVolume, 1f);
            }
        }
        else
        {
            voiceVolume = 0.7f;
        }
        if (b >= 37)
        {
            loadingScreenMusicVolume = block.readSingle();
        }
        else
        {
            loadingScreenMusicVolume = 0.5f;
        }
        debug = block.readBoolean();
        gore = block.readBoolean();
        filter = block.readBoolean();
        if (b < 57)
        {
            filter = true;
        }
        chatText = block.readBoolean();
        if (b > 8)
        {
            chatVoiceIn = block.readBoolean();
        }
        else
        {
            chatVoiceIn = false;
        }
        if (b < 57)
        {
            chatVoiceIn = false;
        }
        chatVoiceOut = block.readBoolean();
        if (b < 57)
        {
            chatVoiceOut = false;
        }
        metric = block.readBoolean();
        if (b > 24)
        {
            talk = block.readBoolean();
        }
        else
        {
            talk = false;
        }
        if (b > 3)
        {
            hints = block.readBoolean();
        }
        else
        {
            hints = true;
        }
        bool flag2 = b <= 13 || block.readBoolean();
        if (b > 12)
        {
            proUI = block.readBoolean();
        }
        else
        {
            proUI = true;
        }
        if (b > 20)
        {
            ShouldHitmarkersFollowWorldPosition = block.readBoolean();
        }
        else
        {
            ShouldHitmarkersFollowWorldPosition = false;
        }
        if (b > 21)
        {
            streamer = block.readBoolean();
        }
        else
        {
            streamer = false;
        }
        if (b > 25)
        {
            featuredWorkshop = block.readBoolean();
        }
        else
        {
            featuredWorkshop = true;
        }
        if (b > 28 && b < 46)
        {
            block.readBoolean();
        }
        if (b > 29)
        {
            showHotbar = block.readBoolean();
        }
        else
        {
            showHotbar = true;
        }
        if (b > 32)
        {
            pauseWhenUnfocused = block.readBoolean();
        }
        else
        {
            pauseWhenUnfocused = true;
        }
        if (b > 27 && b < 46)
        {
            block.readInt32();
        }
        if (b > 26 && b < 46)
        {
            block.readInt32();
        }
        if (b > 6)
        {
            crosshairColor = block.readColor();
            hitmarkerColor = block.readColor();
            criticalHitmarkerColor = block.readColor();
            cursorColor = block.readColor();
        }
        else
        {
            crosshairColor = Color.white;
            hitmarkerColor = Color.white;
            criticalHitmarkerColor = Color.red;
            cursorColor = Color.white;
        }
        if (b > 18)
        {
            backgroundColor = block.readColor();
            foregroundColor = block.readColor();
            fontColor = block.readColor();
        }
        else
        {
            backgroundColor = new Color(0.9f, 0.9f, 0.9f);
            foregroundColor = new Color(0.9f, 0.9f, 0.9f);
            fontColor = new Color(0.9f, 0.9f, 0.9f);
        }
        if (b > 33)
        {
            shadowColor = block.readColor();
        }
        else
        {
            shadowColor = Color.black;
        }
        if (b > 34)
        {
            badColor = block.readColor();
        }
        else
        {
            badColor = Palette.COLOR_R;
        }
        if (b < 38)
        {
            screenshotSizeMultiplier = 1;
        }
        else
        {
            screenshotSizeMultiplier = block.readInt32();
        }
        if (b < 39)
        {
            enableScreenshotSupersampling = true;
        }
        else
        {
            enableScreenshotSupersampling = block.readBoolean();
        }
        if (b < 40)
        {
            enableScreenshotsOnLoadingScreen = true;
        }
        else
        {
            enableScreenshotsOnLoadingScreen = block.readBoolean();
        }
        if (b < 41)
        {
            useStaticCrosshair = false;
            staticCrosshairSize = 0.25f;
        }
        else
        {
            useStaticCrosshair = block.readBoolean();
            staticCrosshairSize = block.readSingle();
        }
        if (b < 42)
        {
            crosshairShape = ECrosshairShape.Line;
        }
        else
        {
            crosshairShape = (ECrosshairShape)block.readByte();
        }
        if (b < 43)
        {
            crosshairColor.a = 0.5f;
            hitmarkerColor.a = 0.5f;
            criticalHitmarkerColor.a = 0.5f;
        }
        else
        {
            crosshairColor.a = (float)(int)block.readByte() / 255f;
            hitmarkerColor.a = (float)(int)block.readByte() / 255f;
            criticalHitmarkerColor.a = (float)(int)block.readByte() / 255f;
        }
        if (b < 45)
        {
            hitmarkerStyle = EHitmarkerStyle.Animated;
        }
        else
        {
            hitmarkerStyle = (EHitmarkerStyle)block.readByte();
        }
        if (b >= 47)
        {
            _voiceAlwaysRecording = block.readBoolean();
            if (b < 48)
            {
                _voiceAlwaysRecording = false;
            }
        }
        else
        {
            _voiceAlwaysRecording = false;
        }
        if (b >= 49)
        {
            shouldNametagFadeOut = block.readBoolean();
        }
        else
        {
            shouldNametagFadeOut = true;
        }
        if (b >= 50)
        {
            gameVolume = block.readSingle();
        }
        else
        {
            gameVolume = 0.7f;
        }
        if (b >= 51)
        {
            UnfocusedVolume = block.readSingle();
        }
        else
        {
            UnfocusedVolume = 0.5f;
        }
        if (b >= 52)
        {
            vehicleThirdPersonCameraMode = (EVehicleThirdPersonCameraMode)block.readByte();
        }
        else
        {
            vehicleThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationDetached;
        }
        if (b >= 53)
        {
            deathMusicVolume = block.readSingle();
            MainMenuMusicVolume = block.readSingle();
            ambientMusicVolume = block.readSingle();
        }
        else if (flag)
        {
            deathMusicVolume = 0.7f;
            MainMenuMusicVolume = 0.7f;
            ambientMusicVolume = 0.7f;
        }
        else
        {
            deathMusicVolume = 0f;
            MainMenuMusicVolume = 0f;
            ambientMusicVolume = 0f;
        }
        if (b >= 54)
        {
            MusicMasterVolume = block.readSingle();
        }
        else
        {
            MusicMasterVolume = 0.7f;
        }
        if (b >= 55)
        {
            AtmosphereVolume = block.readSingle();
        }
        else
        {
            AtmosphereVolume = (flag2 ? 0.7f : 0f);
        }
        if (b >= 56)
        {
            vehicleAircraftThirdPersonCameraMode = (EVehicleThirdPersonCameraMode)block.readByte();
        }
        else
        {
            vehicleAircraftThirdPersonCameraMode = EVehicleThirdPersonCameraMode.RotationAttached;
        }
        if (b >= 57)
        {
            onlineSafetyMenuProceedCount = block.readInt32();
            wantsToHideOnlineSafetyMenu = block.readBoolean();
        }
        if (!Provider.isPro)
        {
            backgroundColor = new Color(0.9f, 0.9f, 0.9f);
            foregroundColor = new Color(0.9f, 0.9f, 0.9f);
            fontColor = new Color(0.9f, 0.9f, 0.9f);
            shadowColor = Color.black;
            badColor = Palette.COLOR_R;
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(57);
        block.writeBoolean(value: false);
        block.writeBoolean(splashscreen);
        block.writeBoolean(timer);
        block.writeSingle(fov);
        block.writeSingle(volume);
        block.writeSingle(voiceVolume);
        block.writeSingle(loadingScreenMusicVolume);
        block.writeBoolean(debug);
        block.writeBoolean(gore);
        block.writeBoolean(filter);
        block.writeBoolean(chatText);
        block.writeBoolean(chatVoiceIn);
        block.writeBoolean(chatVoiceOut);
        block.writeBoolean(metric);
        block.writeBoolean(talk);
        block.writeBoolean(hints);
        block.writeBoolean(value: false);
        block.writeBoolean(proUI);
        block.writeBoolean(ShouldHitmarkersFollowWorldPosition);
        block.writeBoolean(streamer);
        block.writeBoolean(featuredWorkshop);
        block.writeBoolean(showHotbar);
        block.writeBoolean(pauseWhenUnfocused);
        block.writeColor(crosshairColor);
        block.writeColor(hitmarkerColor);
        block.writeColor(criticalHitmarkerColor);
        block.writeColor(cursorColor);
        block.writeColor(backgroundColor);
        block.writeColor(foregroundColor);
        block.writeColor(fontColor);
        block.writeColor(shadowColor);
        block.writeColor(badColor);
        block.writeInt32(screenshotSizeMultiplier);
        block.writeBoolean(enableScreenshotSupersampling);
        block.writeBoolean(enableScreenshotsOnLoadingScreen);
        block.writeBoolean(useStaticCrosshair);
        block.writeSingle(staticCrosshairSize);
        block.writeByte((byte)crosshairShape);
        block.writeByte(MathfEx.RoundAndClampToByte(crosshairColor.a * 255f));
        block.writeByte(MathfEx.RoundAndClampToByte(hitmarkerColor.a * 255f));
        block.writeByte(MathfEx.RoundAndClampToByte(criticalHitmarkerColor.a * 255f));
        block.writeByte((byte)hitmarkerStyle);
        block.writeBoolean(_voiceAlwaysRecording);
        block.writeBoolean(shouldNametagFadeOut);
        block.writeSingle(gameVolume);
        block.writeSingle(UnfocusedVolume);
        block.writeByte((byte)vehicleThirdPersonCameraMode);
        block.writeSingle(deathMusicVolume);
        block.writeSingle(MainMenuMusicVolume);
        block.writeSingle(ambientMusicVolume);
        block.writeSingle(MusicMasterVolume);
        block.writeSingle(AtmosphereVolume);
        block.writeByte((byte)vehicleAircraftThirdPersonCameraMode);
        block.writeInt32(onlineSafetyMenuProceedCount);
        block.writeBoolean(wantsToHideOnlineSafetyMenu);
        ReadWrite.writeBlock("/Options.dat", useCloud: true, block);
    }
}
