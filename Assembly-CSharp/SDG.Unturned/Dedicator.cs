using System;
using UnityEngine;

namespace SDG.Unturned;

public class Dedicator : MonoBehaviour
{
    public static ESteamServerVisibility serverVisibility;

    public static string serverID;

    private static bool _isDedicated;

    public static CommandLineFlag offlineOnly = new CommandLineFlag(defaultValue: false, "-OfflineOnly");

    private static bool _hasBattlEye;

    private static bool _isVR;

    public static CommandWindow commandWindow { get; protected set; }

    public static bool IsDedicatedServer => _isDedicated;

    [Obsolete("Server plugins do not need to check this because they run on the dedicated-server-only builds.")]
    public static bool isDedicated => _isDedicated;

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
