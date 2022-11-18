using System;

namespace SDG.Unturned;

public static class GameUpdateMonitor
{
    public delegate void GameUpdateDetectedHandler(string newVersion, ref bool shouldShutdown);

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
