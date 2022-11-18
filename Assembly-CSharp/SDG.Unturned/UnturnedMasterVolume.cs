using UnityEngine;

namespace SDG.Unturned;

public static class UnturnedMasterVolume
{
    private static bool internalMutedByDedicatedServer;

    private static bool internalMutedByLoadingScreen;

    private static bool mutedByCamera;

    private static float internalPreferredVolume;

    public static bool mutedByDedicatedServer
    {
        get
        {
            return internalMutedByDedicatedServer;
        }
        set
        {
            internalMutedByDedicatedServer = value;
            synchronizeAudioListener();
        }
    }

    public static bool mutedByLoadingScreen
    {
        get
        {
            return internalMutedByLoadingScreen;
        }
        set
        {
            if (value != internalMutedByLoadingScreen)
            {
                internalMutedByLoadingScreen = value;
                synchronizeAudioListener();
            }
        }
    }

    public static float preferredVolume
    {
        get
        {
            return internalPreferredVolume;
        }
        set
        {
            internalPreferredVolume = value;
            synchronizeAudioListener();
        }
    }

    private static void handleMainCameraAvailabilityChanged()
    {
        mutedByCamera = !MainCamera.isAvailable;
        synchronizeAudioListener();
    }

    private static void synchronizeAudioListener()
    {
        if (internalMutedByDedicatedServer || internalMutedByLoadingScreen || mutedByCamera)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = internalPreferredVolume;
        }
    }

    static UnturnedMasterVolume()
    {
        internalMutedByDedicatedServer = true;
        internalMutedByLoadingScreen = true;
        mutedByCamera = true;
        internalPreferredVolume = 0f;
        synchronizeAudioListener();
        MainCamera.availabilityChanged += handleMainCameraAvailabilityChanged;
    }
}
