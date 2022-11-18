using System;
using UnityEngine;

namespace SDG.Unturned;

public class Dedicator : MonoBehaviour
{
    public static ESteamServerVisibility serverVisibility;

    public static string serverID;

    public const bool IsDedicatedServer = true;

    public static CommandLineFlag offlineOnly = new CommandLineFlag(defaultValue: false, "-OfflineOnly");

    private static bool _hasBattlEye;

    private static bool _isVR;

    public static CommandWindow commandWindow { get; protected set; }

    [Obsolete("Server plugins do not need to check this because they run on the dedicated-server-only builds.")]
    public static bool isDedicated => true;

    public static bool isStandaloneDedicatedServer => true;

    public static bool hasBattlEye => _hasBattlEye;

    public static bool isVR => _isVR;

    private void Update()
    {
        if (commandWindow != null)
        {
            commandWindow.update();
        }
    }

    public void awake()
    {
        bool num = CommandLine.tryGetServer(out serverVisibility, out serverID);
        _hasBattlEye = Environment.CommandLine.IndexOf("-BattlEye", StringComparison.OrdinalIgnoreCase) != -1;
        _isVR = false;
        bool flag = !num;
        if (flag)
        {
            serverVisibility = ESteamServerVisibility.LAN;
            serverID = "Default";
        }
        UnturnedMasterVolume.mutedByDedicatedServer = true;
        commandWindow = new CommandWindow();
        Application.targetFrameRate = 50;
        if (flag)
        {
            CommandWindow.Log("Running standalone dedicated server, but launch arguments were not specified on the command-line.");
            CommandWindow.LogFormat("Defaulting to {0} {1}. Valid command-line dedicated server launch arguments are:", serverID, serverVisibility);
            CommandWindow.Log("+InternetServer/{ID}");
            CommandWindow.Log("+LANServer/{ID}");
        }
    }

    private void OnApplicationQuit()
    {
        if (commandWindow != null)
        {
            commandWindow.shutdown();
        }
    }
}
