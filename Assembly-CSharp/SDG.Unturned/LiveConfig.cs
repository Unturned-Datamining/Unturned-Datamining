using System;
using Steamworks;
using Unturned.LiveConfig;

namespace SDG.Unturned;

public static class LiveConfig
{
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

    public static global::Unturned.LiveConfig.LiveConfig Get()
    {
        return LiveConfigManager.Get().config;
    }
}
