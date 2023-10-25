using System;
using UnityEngine;

namespace SDG.Unturned;

public class Dedicator : MonoBehaviour
{
    public static ESteamServerVisibility serverVisibility;

    public static string serverID;

    private static bool _isDedicated;

    /// <summary>
    /// Should dedicated server disable requests to internet?
    /// While in LAN mode skips the Steam backend connection and workshop item queries.
    /// Needs a non-Steam networking implementation before it will be truly offline only.
    /// </summary>
    public static CommandLineFlag offlineOnly = new CommandLineFlag(defaultValue: false, "-OfflineOnly");

    private static bool _hasBattlEye;

    private static bool _isVR;

    public static CommandWindow commandWindow { get; protected set; }

    /// <summary>
    /// Is the application running as a headless server?
    /// Replacement for isDedicated property. The property could not be changed to const in dedicated-server-only
    /// builds without potentially breaking plugins. Only development builds can be run as both client or server.
    /// </summary>
    public static bool IsDedicatedServer => _isDedicated;

    [Obsolete("Server plugins do not need to check this because they run on the dedicated-server-only builds.")]
    public static bool isDedicated => _isDedicated;

    /// <summary>
    /// Are we currently running the standalone dedicated server app?
    /// </summary>
    public static bool isStandaloneDedicatedServer => false;

    public static bool hasBattlEye => _hasBattlEye;

    public static bool isVR => _isVR;

    private void Update()
    {
        if (IsDedicatedServer && commandWindow != null)
        {
            commandWindow.update();
        }
    }

    public void awake()
    {
        _isDedicated = CommandLine.tryGetServer(out serverVisibility, out serverID);
        _hasBattlEye = Environment.CommandLine.IndexOf("-BattlEye", StringComparison.OrdinalIgnoreCase) != -1;
        _isVR = false;
        UnturnedMasterVolume.mutedByDedicatedServer = IsDedicatedServer;
        if (IsDedicatedServer)
        {
            commandWindow = new CommandWindow();
            int num2 = (Application.targetFrameRate = 50);
            UnturnedLog.info($"Dedicated server set target update rate to {num2}");
        }
    }

    private void OnApplicationQuit()
    {
        if (IsDedicatedServer && commandWindow != null)
        {
            commandWindow.shutdown();
        }
    }
}
