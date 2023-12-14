using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_WorkshopDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.isWorkshop == rhs.isWorkshop)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.isWorkshop)
        {
            return -1;
        }
        return 1;
    }
}
