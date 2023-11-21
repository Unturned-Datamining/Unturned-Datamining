using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_WorkshopDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
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
