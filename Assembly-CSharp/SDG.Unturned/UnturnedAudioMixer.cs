using UnityEngine;
using UnityEngine.Audio;

namespace SDG.Unturned;

internal static class UnturnedAudioMixer
{
    private static bool hasBeenInitialized;

    private static AudioMixer mainMix;

    private static AudioMixerGroup defaultGroup;

    private static AudioMixerGroup musicGroup;

    private static AudioMixerGroup atmosphereGroup;

    public static AudioMixerGroup GetDefaultGroup()
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        return defaultGroup;
    }

    public static AudioMixerGroup GetMusicGroup()
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        return musicGroup;
    }

    public static AudioMixerGroup GetAtmosphereGroup()
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        return atmosphereGroup;
    }

    public static void SetDefaultVolume(float linearVolume)
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        mainMix.SetFloat("DefaultVolume", LinearToDb(linearVolume));
    }

    public static void SetVoiceVolume(float linearVolume)
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        mainMix.SetFloat("VoiceVolume", LinearToDb(linearVolume));
    }

    public static void SetMusicMasterVolume(float linearVolume)
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        mainMix.SetFloat("MusicVolume", LinearToDb(linearVolume));
    }

    public static void SetMainMenuMusicVolume(float linearVolume)
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        mainMix.SetFloat("MainMenuMusicVolume", LinearToDb(linearVolume));
    }

    public static void SetAtmosphereVolume(float linearVolume)
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        mainMix.SetFloat("AtmosphereVolume", LinearToDb(linearVolume));
    }

    private static float LinearToDb(float linearVolume)
    {
        if (linearVolume < 0.0001f)
        {
            return -80f;
        }
        return Mathf.Log10(linearVolume) * 20f;
    }

    private static void Initialize()
    {
        hasBeenInitialized = true;
        mainMix = Resources.Load<AudioMixer>("Sounds/MainMix");
        defaultGroup = mainMix.FindMatchingGroups("Default")[0];
        musicGroup = mainMix.FindMatchingGroups("Music")[0];
        atmosphereGroup = mainMix.FindMatchingGroups("Atmosphere")[0];
    }
}
