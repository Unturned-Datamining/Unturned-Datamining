using System;

namespace SDG.Unturned;

public static class GameUpdateMonitor
{
    public delegate void GameUpdateDetectedHandler(string newVersion, ref bool shouldShutdown);

    /// <summary>
    /// Event for plugins to be notified when a server update is detected.
    ///
    /// Pandahut requested this because they run the game as a Windows service and need to shutdown
    /// through their central management system rather than per-process.
    /// </summary>
    public static event GameUpdateDetectedHandler OnGameUpdateDetected;

    internal static void NotifyGameUpdateDetected(string newVersion, ref bool shouldShutdown)
    {
        try
        {
            GameUpdateMonitor.OnGameUpdateDetected?.Invoke(newVersion, ref shouldShutdown);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught plugin exception during OnGameUpdateDetected:");
        }
    }
}
