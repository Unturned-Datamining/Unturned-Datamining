using System;
using Steamworks;

namespace SDG.Unturned;

public static class LiveConfig
{
    public static bool WasPopulated => LiveConfigManager.Get().wasPopulated;

    public static event System.Action OnRefreshed
    {
        add
        {
            LiveConfigManager.Get().OnConfigRefreshed += value;
        }
        remove
        {
            LiveConfigManager.Get().OnConfigRefreshed -= value;
        }
    }

    /// <summary>
    /// Called during startup and when returning to the main menu.
    /// </summary>
    public static void Refresh()
    {
        if (SteamUser.BLoggedOn() && (bool)Provider.allowWebRequests)
        {
            LiveConfigManager.Get().Refresh();
        }
    }

    /// <summary>
    /// Result is never null, but may be empty or out-of-date.
    /// </summary>
    public static LiveConfigData Get()
    {
        return LiveConfigManager.Get().config;
    }
}
