using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Helper wrapping Unturned's usage of AudioListener.volume, which is the master volume level.
/// This makes it easier to track what controls the master volume and avoid bugs.
/// </summary>
public static class UnturnedMasterVolume
{
    private static bool internalMutedByDedicatedServer;

    private static bool internalMutedByLoadingScreen;

    private static bool mutedByCamera;

    private static float internalPreferredVolume;

    /// <summary>
    /// Is audio muted because this is a dedicated server?
    ///
    /// While dedicated server should not even be processing audio code,
    /// older versions of Unity in particular have issues with headless audio.
    /// </summary>
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

    /// <summary>
    /// Is audio muted because loading screen is visible?
    /// </summary>
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

    /// <summary>
    /// Player's volume multiplier from the options menu.
    /// </summary>
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

    /// <summary>
    /// Mute or un-mute audio depending whether camera is valid.
    /// </summary>
    private static void handleMainCameraAvailabilityChanged()
    {
        mutedByCamera = !MainCamera.isAvailable;
        synchronizeAudioListener();
    }

    /// <summary>
    /// Synchronize AudioListener.volume with Unturned's parameters.
    /// </summary>
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
