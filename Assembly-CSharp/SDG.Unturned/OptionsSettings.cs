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

    private const byte SAVEDATA_VERSION_NEWEST = 42;

    public static readonly byte SAVEDATA_VERSION = 42;

    public static readonly byte MIN_FOV = 60;

    public static readonly byte MAX_FOV = 40;

    private static float _fov;

    private static float _cachedVerticalFOV;

    public static float volume;

    public static float voiceVolume;

    public static float loadingScreenMusicVolume;

    public static bool debug;

    public static bool music;

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

    public static bool hitmarker;

    public static bool streamer;

    public static bool featuredWorkshop;

    public static bool matchmakingShowAllMaps;

    public static bool showHotbar;

    public static bool pauseWhenUnfocused;

    public static int minMatchmakingPlayers;

    public static int maxMatchmakingPing;

    public static int screenshotSizeMultiplier;

    public static bool enableScreenshotSupersampling;

    public static bool enableScreenshotsOnLoadingScreen;

    public static bool useStaticCrosshair;

    public static float staticCrosshairSize;

    public static ECrosshairShape crosshairShape;

    public static Color crosshairColor;

    public static Color hitmarkerColor;

    public static Color criticalHitmarkerColor;

    public static Color cursorColor = Color.white;

    private static Color _shadowColor = Color.black;

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
            return _shadowColor;
        }
        set
        {
            _shadowColor = value;
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

    public static event System.Action OnUnitSystemChanged;

    public static event System.Action OnCustomColorsChanged;

    public static event System.Action OnThemeChanged;

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
        if (LevelLighting.dayAudio != null)
        {
            if (!LevelLighting.dayAudio.enabled && ambience)
            {
                LevelLighting.dayAudio.enabled = true;
                LevelLighting.dayAudio.Play();
            }
            else
            {
                LevelLighting.dayAudio.enabled = ambience;
            }
        }
        if (LevelLighting.nightAudio != null)
        {
            if (!LevelLighting.nightAudio.enabled && ambience)
            {
                LevelLighting.nightAudio.enabled = true;
                LevelLighting.nightAudio.Play();
            }
            else
            {
                LevelLighting.nightAudio.enabled = ambience;
            }
        }
    }

    public static void restoreDefaults()
    {
        music = true;
        splashscreen = true;
        timer = false;
        fov = 0.75f;
        volume = 1f;
        voiceVolume = 1f;
        loadingScreenMusicVolume = 0.5f;
        debug = false;
        gore = true;
        filter = true;
        chatText = true;
        chatVoiceIn = true;
        chatVoiceOut = true;
        metric = true;
        talk = false;
        hints = true;
        ambience = true;
        proUI = true;
        hitmarker = false;
        streamer = false;
        featuredWorkshop = true;
        matchmakingShowAllMaps = false;
        showHotbar = true;
        pauseWhenUnfocused = true;
        minMatchmakingPlayers = 12;
        maxMatchmakingPing = 300;
        screenshotSizeMultiplier = 1;
        enableScreenshotSupersampling = true;
        enableScreenshotsOnLoadingScreen = true;
        useStaticCrosshair = false;
        staticCrosshairSize = 0.1f;
        crosshairShape = ECrosshairShape.Line;
        crosshairColor = Color.white;
        hitmarkerColor = Color.white;
        criticalHitmarkerColor = Color.red;
        cursorColor = Color.white;
        backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        foregroundColor = new Color(0.9f, 0.9f, 0.9f);
        fontColor = new Color(0.9f, 0.9f, 0.9f);
        shadowColor = Color.black;
        badColor = Palette.COLOR_R;
    }

    public static void load()
    {
        restoreDefaults();
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
        music = block.readBoolean();
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
            voiceVolume = 1f;
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
        if (b < 32)
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
            chatVoiceIn = true;
        }
        chatVoiceOut = block.readBoolean();
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
        if (b > 13)
        {
            ambience = block.readBoolean();
        }
        else
        {
            ambience = true;
        }
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
            hitmarker = block.readBoolean();
        }
        else
        {
            hitmarker = false;
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
        if (b > 28)
        {
            matchmakingShowAllMaps = block.readBoolean();
        }
        else
        {
            matchmakingShowAllMaps = false;
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
        if (b > 27)
        {
            minMatchmakingPlayers = block.readInt32();
        }
        else
        {
            minMatchmakingPlayers = 12;
        }
        if (b > 26)
        {
            maxMatchmakingPing = block.readInt32();
        }
        else
        {
            maxMatchmakingPing = 300;
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
        block.writeByte(42);
        block.writeBoolean(music);
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
        block.writeBoolean(ambience);
        block.writeBoolean(proUI);
        block.writeBoolean(hitmarker);
        block.writeBoolean(streamer);
        block.writeBoolean(featuredWorkshop);
        block.writeBoolean(matchmakingShowAllMaps);
        block.writeBoolean(showHotbar);
        block.writeBoolean(pauseWhenUnfocused);
        block.writeInt32(minMatchmakingPlayers);
        block.writeInt32(maxMatchmakingPing);
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
        ReadWrite.writeBlock("/Options.dat", useCloud: true, block);
    }
}
