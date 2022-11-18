using System;
using SDG.Provider.Services;
using SDG.Provider.Services.Achievements;
using SDG.Unturned;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Achievements;

public class SteamworksAchievementsService : Service, IAchievementsService, IService
{
    public bool getAchievement(string name, out bool has)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        bool achievement = SteamUserStats.GetAchievement(name, out has);
        if (!achievement)
        {
            UnturnedLog.error("Failed to get Steam achievement \"" + name + "\" status");
        }
        return achievement;
    }

    public bool setAchievement(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        bool num = SteamUserStats.SetAchievement(name);
        if (num)
        {
            UnturnedLog.info("Unlocked Steam achievement \"" + name + "\"");
        }
        else
        {
            UnturnedLog.error("Failed to unlock Steam achievement \"" + name + "\"");
        }
        SteamUserStats.StoreStats();
        return num;
    }
}
