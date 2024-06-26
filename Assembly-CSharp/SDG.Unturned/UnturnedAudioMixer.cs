using UnityEngine;
using UnityEngine.Audio;

namespace SDG.Unturned;

internal static class UnturnedAudioMixer
{
    private static bool hasBeenInitialized;

    private static AudioMixer mainMix;

    private static AudioMixerGroup defaultGroup;

    public static AudioMixerGroup GetDefaultGroup()
    {
        if (!hasBeenInitialized)
        {
            Initialize();
        }
        return defaultGroup;
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
    }
}
