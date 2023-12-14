using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_CombatDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.isPvP == rhs.isPvP)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.isPvP)
        {
            return -1;
        }
        return 1;
    }
}
