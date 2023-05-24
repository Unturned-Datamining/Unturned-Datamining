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

    public static void Refresh()
    {
        if (SteamUser.BLoggedOn() && (bool)Provider.allowWebRequests)
        {
            LiveConfigManager.Get().Refresh();
        }
    }

    public static LiveConfigData Get()
    {
        return LiveConfigManager.Get().config;
    }
}
