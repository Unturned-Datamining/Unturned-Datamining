using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_GoldDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.isPro == rhs.isPro)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.isPro)
        {
            return 1;
        }
        return -1;
    }
}
